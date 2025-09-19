# 銷貨訂單明細庫存回滾機制 - 修改日誌

## 修改日期
2025年9月19日（最後更新：2025年9月19日 - 修復回滾數量累積錯誤問題）

## 問題背景

### 問題1：明細修改時庫存重複扣減
在編輯銷貨訂單明細時，系統沒有先回滾原本的庫存扣減，直接進行新的扣減，導致庫存被重複扣減。

**問題範例：**
```
原始狀態：庫存數量 40，銷貨數量 10 → 庫存變成 30
修改後：銷貨數量改為 15
期望結果：庫存應該是 25 (40-15)
實際結果：庫存變成 15 (30-15)，少扣減了 10
```

### 問題2：倉庫位置回滾困難
- `SalesOrderDetail` 只有 `WarehouseId`，沒有 `WarehouseLocationId`
- FIFO 扣減可能跨越多個倉庫位置的批次
- 回滾時無法精確還原到原來的倉庫位置

### 問題3：編輯模式庫存驗證時機錯誤 ⚠️
**發現時間：** 2025年9月19日

在編輯模式測試時發現，庫存驗證(`ValidateWarehouseInventoryStockAsync`)在庫存回滾之前執行，導致驗證時檢查的是已扣減後的庫存量，而非回滾後的真實可用庫存。

**問題情境：**
```
初始庫存：160個
新增銷貨：154個 → 剩餘庫存 6個 ✅ 成功
編輯修改為：100個
錯誤結果：驗證時檢查庫存只有6個，但需要100個 → 驗證失敗 ❌
正確邏輯：應該先回滾154個(釋放到160個)，再檢查是否足夠100個 → 應該成功 ✅
```

**執行順序問題：**
1. ❌ 錯誤順序：驗證庫存(6個) → 回滾庫存 → 重新扣減
2. ✅ 正確順序：回滾庫存 → 重新扣減(內建驗證) 或 回滾庫存 → 驗證庫存 → 重新扣減

### 問題4：回滾數量累積錯誤 ⚠️
**發現時間：** 2025年9月19日下午

在測試編輯功能時發現，多次編輯同一銷貨訂單會導致回滾數量越來越多，原因是 `GetInventoryTransactionsBySalesOrderAsync` 方法會查找**所有歷史交易記錄**，而不是**淨需要回滾的數量**。

**問題情境：**
```
交易記錄分析：
1-2. RCV進貨：批號001=20，批號002=20
3-4. SO-1初始銷貨：扣減批號001(-20)，批號002(-10)
5-6. 第一次編輯回滾：回滾+20，+10 ✅ 正確
7.   第一次編輯扣減：批號001(-5)  
8-10. 第二次編輯回滾：❌ 錯誤地回滾了id3,4,7全部記錄
      - 批號001 +20 (應該是+5)
      - 批號002 +10 (應該是+0)  
      - 批號001 +5  (重複回滾)
```

**根本原因：**
- 查詢 `SO-1` 開頭的記錄會找到所有歷史銷貨記錄
- 沒有考慮已經回滾過的數量  
- 導致重複回滾相同的庫存扣減

## 解決方案設計

採用 **基於 InventoryTransaction 的追蹤回滾機制**，利用現有的庫存交易記錄來實現精確的回滾操作。

### 核心設計理念
1. **利用現有架構**：不需要大幅修改資料庫結構
2. **精確追蹤**：透過交易記錄可以準確回滾到原始狀態
3. **保持簡潔**：避免在 SalesOrderDetail 增加複雜的位置欄位
4. **可擴展性**：未來其他單據也可使用相同機制

## 修改內容

### 1. InventoryStockService 改進 ✅

#### 1.1 修改 ReduceStockWithFIFOAsync 方法
**檔案：** `Services/Warehouses/InventoryStockService.cs`

```csharp
// 新增 salesOrderDetailId 參數
public async Task<ServiceResult> ReduceStockWithFIFOAsync(int productId, int warehouseId, int quantity,
    InventoryTransactionTypeEnum transactionType, string transactionNumber,
    int? locationId = null, string? remarks = null, int? salesOrderDetailId = null)
```

**變更重點：**
- 新增 `salesOrderDetailId` 可選參數
- 在 `InventoryTransaction` 中記錄 `ReferenceNumber = salesOrderDetailId?.ToString()`
- 同步更新介面 `IInventoryStockService`

#### 1.2 新增庫存交易查詢方法 🔧
**檔案：** `Services/Warehouses/InventoryStockService.cs`  
**重大修正：** 2025年9月19日下午 - 修復回滾數量累積錯誤

```csharp
/// <summary>
/// 根據銷貨訂單ID查找相關的庫存交易記錄（用於回滾）
/// 修正：計算淨扣減量，避免重複回滾
/// </summary>
public async Task<List<InventoryTransaction>> GetInventoryTransactionsBySalesOrderAsync(int salesOrderId)
```

**核心邏輯改進：**
```csharp
// 分離銷貨記錄和回滾記錄
var saleTransactions = allTransactions.Where(t => !t.TransactionNumber.Contains("_REVERT"));
var revertTransactions = allTransactions.Where(t => t.TransactionNumber.Contains("_REVERT"));

// 計算每個InventoryStockId的淨扣減量
var netReductions = new Dictionary<int, int>();

// 加入所有銷貨扣減
foreach (var sale in saleTransactions)
    netReductions[stockId] += Math.Abs(sale.Quantity);

// 減去已回滾的量  
foreach (var revert in revertTransactions)
    netReductions[stockId] -= revert.Quantity;

// 只回滾淨扣減量大於0的記錄
var virtualTransactions = netReductions.Where(x => x.Value > 0);
```

**功能特點：**
- **智能計算**：只回滾真正需要回滾的淨數量
- **避免重複**：已回滾的數量不會再次回滾  
- **精確追蹤**：基於 `InventoryStockId` 進行精確計算
- **虛擬交易**：返回計算後的虛擬交易記錄，簡化後續處理

#### 1.3 新增精確回滾方法 ✅
**檔案：** `Services/Warehouses/InventoryStockService.cs`

```csharp
/// <summary>
/// 基於 InventoryStockId 的精確庫存回滾
/// </summary>
public async Task<ServiceResult> RevertStockByInventoryStockIdAsync(
    int inventoryStockId, int quantity, string transactionNumber, string? remarks = null)
```

**功能特點：**
- 直接根據 `InventoryStockId` 找到原始 `InventoryStock` 記錄
- 優先使用精確回滾，找不到原記錄時才使用 `AddStockAsync`
- 避免創建重複的 `InventoryStock` 記錄

### 2. SalesOrderDetailService 擴展 ✅

#### 2.1 依賴注入改進
**檔案：** `Services/Sales/SalesOrderDetailService.cs`

```csharp
private readonly IInventoryStockService? _inventoryStockService;

public SalesOrderDetailService(
    IDbContextFactory<AppDbContext> contextFactory, 
    ILogger<GenericManagementService<SalesOrderDetail>> logger,
    IInventoryStockService? inventoryStockService = null) : base(contextFactory, logger)
{
    _inventoryStockService = inventoryStockService;
}
```

#### 2.2 核心方法實作

##### UpdateDetailsWithInventoryAsync（主要方法）
```csharp
/// <summary>
/// 更新銷貨明細並處理庫存回滾/重新分配
/// </summary>
public async Task<ServiceResult> UpdateDetailsWithInventoryAsync(
    int salesOrderId, 
    List<SalesOrderDetail> newDetails, 
    List<SalesOrderDetail> originalDetails)
```

**處理流程：**
1. 回滾原有庫存扣減
2. 更新明細資料
3. 重新進行庫存扣減
4. 完整的事務管理

##### RevertInventoryForSalesOrderAsync（回滾方法） 🔧
**重大修正：** 2025年9月19日下午

```csharp
/// <summary>
/// 回滾銷貨訂單的庫存扣減
/// 修正：優先使用精確回滾，避免創建重複記錄
/// </summary>
private async Task<ServiceResult> RevertInventoryForSalesOrderAsync(int salesOrderId)
```

**修正後的回滾邏輯：**
1. 呼叫改進的 `GetInventoryTransactionsBySalesOrderAsync` 獲取淨回滾量
2. 優先使用 `RevertStockByInventoryStockIdAsync` 進行精確回滾
3. 找不到原記錄時才使用 `AddStockAsync` 創建新記錄
4. 創建標記為 `_REVERT` 的交易記錄

**關鍵改進：**
```csharp
foreach (var trans in transactions)
{
    // 優先嘗試精確回滾
    var revertResult = await _inventoryStockService.RevertStockByInventoryStockIdAsync(
        trans.InventoryStockId.Value, revertQuantity, $"SO-{salesOrderId}_REVERT",
        $"精確回滾銷貨訂單 SO-{salesOrderId} 的庫存扣減");
        
    if (!revertResult.IsSuccess)
    {
        // 精確回滾失敗，使用 AddStockAsync 作為後備
        revertResult = await _inventoryStockService.AddStockAsync(...);
    }
}
```

##### UpdateDetailsInternalAsync（內部更新方法）
```csharp
/// <summary>
/// 內部明細更新方法（支持外部交易）
/// </summary>
private async Task<ServiceResult> UpdateDetailsInternalAsync(
    int salesOrderId, 
    List<SalesOrderDetail> details, 
    Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? externalTransaction = null)
```

**功能特點：**
- 支援外部交易管理
- 處理新增、更新、刪除明細
- 自動重新計算小計

### 3. UI 組件更新 ✅

#### 3.1 庫存驗證時機修正 🔧
**檔案：** `Components/Pages/Sales/SalesOrderEditModalComponent.razor`
**修改時間：** 2025年9月19日

**問題修復：**
```csharp
// 修改前：所有模式都進行事前庫存驗證
var stockValidationResult = await SalesOrderService.ValidateWarehouseInventoryStockAsync(salesOrderDetails);
if (!stockValidationResult.IsSuccess) { return false; }

// 修改後：只在新增模式進行事前驗證，編輯模式跳過
var isEditMode = SalesOrderId.HasValue;

// 儲存前檢查倉庫選擇和庫存（僅在新增模式下進行）
// 編輯模式下會先進行庫存回滾，所以不需要事前驗證
if (!isEditMode)
{
    var stockValidationResult = await SalesOrderService.ValidateWarehouseInventoryStockAsync(salesOrderDetails);
    if (!stockValidationResult.IsSuccess)
    {
        await NotificationService.ShowErrorAsync($"驗證失敗，無法儲存：\n{stockValidationResult.ErrorMessage}");
        return false;
    }
}
```

**修正邏輯：**
- **新增模式：** 先驗證庫存 → 儲存主檔 → 儲存明細 → 扣減庫存
- **編輯模式：** 儲存主檔 → 執行帶回滾的更新(內建驗證) → 完成

#### 3.2 FIFO 扣減改進
**檔案：** `Components/Pages/Sales/SalesOrderEditModalComponent.razor`

```csharp
var result = await InventoryStockService.ReduceStockWithFIFOAsync(
    detail.ProductId,
    detail.WarehouseId.Value,
    (int)Math.Ceiling(detail.OrderQuantity),
    InventoryTransactionTypeEnum.Sale,
    $"SO-{salesOrderId}",
    null,
    $"銷貨訂單出貨 - SO-{salesOrderId}",
    detail.Id > 0 ? detail.Id : null // 新增：傳遞明細ID用於追蹤
);
```

#### 3.3 儲存邏輯重構
**檔案：** `Components/Pages/Sales/SalesOrderEditModalComponent.razor`

```csharp
// 區分新增和編輯模式
var isEditMode = SalesOrderId.HasValue;

if (isEditMode)
{
    // 編輯模式：使用帶有庫存回滾功能的更新方法
    var updateResult = await SalesOrderDetailService.UpdateDetailsWithInventoryAsync(
        result.Data.Id, 
        salesOrderDetails, 
        originalDetails);
}
else
{
    // 新增模式：使用原有的儲存邏輯
    await SaveSalesOrderDetails(result.Data.Id);
    await ReduceInventoryWithFIFOAsync(result.Data.Id);
}
```

## 技術實作細節

### 追蹤機制設計

#### TransactionNumber 規則
- **銷貨扣減：** `SO-{salesOrderId}`
- **回滾操作：** `SO-{salesOrderId}_REVERT`

#### ReferenceNumber 用途
- 記錄 `SalesOrderDetailId`，用於精確追蹤到明細層級

#### 庫存交易類型
- **Sale：** 銷貨出庫
- **Return：** 回滾入庫

### 驗證與執行時機

#### 新增模式流程 ✅
1. **事前驗證庫存：** 檢查當前庫存是否充足
2. **儲存主檔：** 建立銷貨訂單
3. **儲存明細：** 建立銷貨明細
4. **扣減庫存：** 使用 FIFO 方式扣減庫存
5. **完成：** 記錄交易並完成

#### 編輯模式流程 ✅（修正後）
1. **~~事前驗證庫存~~：** 跳過，因為會先回滾
2. **儲存主檔：** 更新銷貨訂單
3. **執行回滾更新：** `UpdateDetailsWithInventoryAsync`
   - 3.1 **回滾原庫存：** 根據交易記錄回滾
   - 3.2 **更新明細：** 更新銷貨明細資料  
   - 3.3 **重新扣減庫存：** 根據新明細扣減(內建庫存驗證)
4. **完成：** 交易提交完成

**關鍵修正：**
- 編輯模式不再進行事前庫存驗證
- 庫存驗證改由 `ReduceStockWithFIFOAsync` 內部處理
- 確保回滾-扣減的原子性操作
### 事務管理
```csharp
using var context = await _contextFactory.CreateDbContextAsync();
using var transaction = await context.Database.BeginTransactionAsync();

try
{
    // 1. 回滾庫存
    // 2. 更新明細
    // 3. 重新扣減庫存
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 錯誤處理策略
1. **新增模式驗證：** 事前檢查庫存充足性，失敗即停止
2. **編輯模式回滾：** 失敗時停止整個流程  
3. **明細更新階段：** 失敗時回滾整個事務
4. **庫存扣減階段：** 內建驗證，不足時回滾整個事務

### 庫存驗證策略調整 🔄
**修改日期：** 2025年9月19日

#### 驗證時機差異
| 模式 | 事前驗證 | 回滾階段 | 扣減階段驗證 | 說明 |
|------|----------|----------|-------------|------|
| **新增模式** | ✅ 必要 | ❌ 無 | ✅ 二次檢查 | 確保庫存充足再執行 |
| **編輯模式** | ❌ 跳過 | ✅ 執行 | ✅ 內建驗證 | 回滾後再驗證避免誤判 |

#### 為什麼編輯模式跳過事前驗證？
1. **邏輯問題：** 編輯時庫存已被扣減，事前驗證會檢查到錯誤的庫存數量
2. **回滾機制：** 系統會先釋放原有的庫存扣減，再進行新的扣減
3. **內建保護：** `ReduceStockWithFIFOAsync` 方法內部已有庫存充足性檢查
4. **事務安全：** 整個過程在事務中執行，失敗會自動回滾

## 使用說明

### 編輯模式流程
1. 使用者修改銷貨訂單明細
2. 系統自動查找原本的庫存扣減記錄
3. 回滾原本的庫存變動
4. 更新明細資料
5. 根據新明細重新進行 FIFO 庫存扣減

### 新增模式流程
1. 維持原有邏輯不變
2. 直接進行庫存扣減
3. 記錄明細ID用於未來回滾

## 測試建議

### 測試案例設計

#### 案例1：簡單數量修改
```
初始狀態：庫存 100，銷貨 20
修改為：銷貨 30
預期結果：庫存變成 70
```

#### 案例2：跨批次FIFO測試
```
批次A：數量50，批次日期較早
批次B：數量30，批次日期較晚
銷貨數量：從60改為40
預期：應該從批次A回滾20，最終批次A剩30，批次B剩30
```

#### 案例3：倉庫變更測試
```
原本：倉庫A扣減20
修改：改為倉庫B扣減20
預期：倉庫A回滾+20，倉庫B扣減-20
```

#### 案例4：明細新增/刪除測試
```
原本：產品A數量10，產品B數量20
修改：刪除產品A，新增產品C數量15
預期：產品A回滾+10，產品B不變，產品C扣減-15
```

#### 案例5：庫存驗證時機測試 🆕
**問題重現測試：**
```
初始庫存：160個
步驟1：新增銷貨154個 → 剩餘6個 ✅ 
步驟2：編輯修改為100個
修復前：❌ 驗證失敗(檢查到6個<100個)
修復後：✅ 成功(先回滾到160個，再扣減100個)
```

**邊界值測試：**
```
情境A：編輯數量減少
- 原始：銷貨80個，剩餘20個
- 修改：銷貨50個，預期剩餘50個 ✅

情境B：編輯數量增加（但總量充足）
- 原始：銷貨30個，剩餘70個  
- 修改：銷貨90個，預期剩餘10個 ✅

情境C：編輯數量增加（超出總量）
- 原始：銷貨30個，剩餘70個，總庫存100個
- 修改：銷貨110個，預期失敗 ❌（庫存不足）
```

#### 案例6：回滾數量累積錯誤測試 🆕
**問題重現測試：**
```
初始庫存：批號001=20，批號002=20
步驟1：銷貨30個(批號001全部+批號002部分)
步驟2：編輯為5個，預期回滾25個，只扣減5個  
步驟3：再編輯為35個，預期回滾5個，扣減35個

修復前：❌ 步驟3會回滾歷史上的所有扣減記錄(重複回滾)  
修復後：✅ 步驟3只回滾淨扣減量5個
```

**多次編輯測試：**
```
交易順序測試：
1. 初始銷貨：扣減A(-10), B(-20)
2. 第一次編輯：回滾A(+10), B(+20)，重新扣減A(-5)
3. 第二次編輯：只回滾A(+5)，重新扣減C(-15)
4. 第三次編輯：只回滾C(+15)，重新扣減A(-8), B(-7)

預期每次只回滾實際需要回滾的淨數量 ✅
```

**重複編輯穩定性測試：**
```
壓力測試情境：
- 對同一訂單連續編輯10次
- 每次修改不同的產品和數量組合  
- 驗證庫存數據的一致性和交易記錄的正確性
- 確認沒有庫存洩漏或重複扣減問題 ✅
```

### 驗證點
1. **庫存數量正確性**
2. **庫存交易記錄完整性**
3. **批次FIFO順序正確性**
4. **倉庫位置分配正確性**
5. **異常時事務回滾完整性**
6. **編輯模式庫存驗證時機正確性** 🆕
7. **新增vs編輯模式行為差異驗證** 🆕
8. **多次編輯回滾數量正確性** 🆕
9. **精確回滾vs後備回滾機制驗證** 🆕
10. **淨扣減量計算準確性** 🆕

## 影響範圍

### 已修改檔案（按時間順序）
**第一階段修改（2025年9月19日上午）：**
1. `Services/Warehouses/InventoryStockService.cs`
2. `Services/Warehouses/IInventoryStockService.cs`  
3. `Services/Sales/SalesOrderDetailService.cs`
4. `Services/Sales/ISalesOrderDetailService.cs`
5. `Components/Pages/Sales/SalesOrderEditModalComponent.razor`

**第二階段修正（2025年9月19日下午）：**
6. `Components/Pages/Sales/SalesOrderEditModalComponent.razor` - 庫存驗證時機修正

**第三階段修正（2025年9月19日下午）：**
7. `Services/Warehouses/InventoryStockService.cs` - 回滾數量累積錯誤修正
8. `Services/Warehouses/IInventoryStockService.cs` - 精確回滾方法介面擴展
9. `Services/Sales/SalesOrderDetailService.cs` - 回滾邏輯優化

### 修改類型統計
- **新增方法：** 3個 (`GetInventoryTransactionsBySalesOrderAsync`, `UpdateDetailsWithInventoryAsync`, `RevertStockByInventoryStockIdAsync`)
- **修改方法：** 3個 (`ReduceStockWithFIFOAsync`, `SaveSalesOrderWithDetails`, `RevertInventoryForSalesOrderAsync`) 
- **介面擴展：** 2個 (`IInventoryStockService`, `ISalesOrderDetailService`)
- **邏輯修正：** 2個 (庫存驗證時機調整、回滾數量累積錯誤修正)
- **演算法改進：** 1個 (淨扣減量計算邏輯)

### 相容性
- **向下相容**：新增模式完全不受影響
- **API 擴展**：只增加可選參數，不影響現有調用
- **資料庫**：不需要修改現有資料結構

## 效能考量

### 查詢優化
- `GetInventoryTransactionsBySalesOrderAsync` 使用索引查詢
- 只查詢必要的關聯資料
- 排序優化減少記憶體使用

### 事務優化
- 最小化事務範圍
- 批次處理多筆明細
- 適當的錯誤處理避免長時間鎖定

## 後續改進建議

### 短期改進
1. **增加日誌記錄**：詳細記錄回滾過程
2. **效能監控**：監控大量明細的處理時間
3. **單元測試**：補充完整的測試覆蓋率

### 長期優化
1. **異步處理**：對於大量明細可考慮異步處理
2. **快照機制**：考慮引入庫存快照來加速回滾
3. **審計追蹤**：增強庫存變動的審計功能

## 維護注意事項

### 開發注意點
1. **編輯模式：** 必須使用新的 `UpdateDetailsWithInventoryAsync` 方法
2. **新增模式：** 記得傳遞 `salesOrderDetailId` 參數到 FIFO 扣減方法
3. **庫存驗證：** 編輯模式不需要事前驗證，新增模式需要事前驗證
4. **命名規則：** 庫存交易的 `TransactionNumber` 命名規則不可隨意更改
5. **精確回滾：** 優先使用 `RevertStockByInventoryStockIdAsync`，失敗時才用 `AddStockAsync` 🆕
6. **淨回滾量：** `GetInventoryTransactionsBySalesOrderAsync` 已處理重複回滾問題 🆕

### 監控要點
1. 庫存交易記錄的完整性
2. 回滾操作的成功率
3. 庫存數據的一致性檢查
4. **編輯模式驗證跳過率** 🆕（確認編輯時正確跳過事前驗證）
5. **精確回滾成功率** 🆕（監控基於InventoryStockId的回滾成功率）
6. **淨回滾量計算正確率** 🆕（確認多次編輯時回滾數量正確）

### 錯誤處理
1. 庫存不足時的友善提示
2. 回滾失敗時的復原機制  
3. 異常狀況的日誌記錄
4. **驗證時機錯誤的提示** 🆕（區分新增/編輯模式的驗證失敗原因）
5. **精確回滾失敗的後備處理** 🆕（自動降級到AddStockAsync）
6. **重複回滾檢測** 🆕（避免相同交易被多次回滾）

## 結論

本次修改成功解決了銷貨訂單明細編輯時的庫存重複扣減問題，同時提供了精確的倉庫位置回滾機制。

### 解決的問題總結
1. ✅ **庫存重複扣減**：透過 `InventoryTransaction` 追蹤實現精確回滾
2. ✅ **倉庫位置回滾**：利用交易記錄還原到原始庫存狀態
3. ✅ **庫存驗證時機**：修正編輯模式下的驗證邏輯，避免誤判
4. ✅ **回滾數量累積錯誤**：實現淨扣減量計算，避免重複回滾 🆕
5. ✅ **InventoryStock記錄重複**：優先精確回滾，避免創建重複記錄 🆕

### 技術成果
- **無侵入性設計**：不需要修改資料庫結構或大幅重構現有程式碼
- **精確回滾機制**：基於交易記錄的追蹤，確保庫存狀態的準確性
- **模式化處理**：新增和編輯模式采用不同但最適合的處理邏輯
- **事務安全性**：完整的錯誤處理和事務回滾保護
- **智能算法**：淨扣減量計算避免重複回滾問題 🆕
- **多層保護**：精確回滾+後備機制確保系統穩定性 🆕

### 實際測試驗證
**測試情境1：** 初始庫存160個 → 銷貨154個（剩6個）→ 編輯為100個
- **修復前：** ❌ 驗證失敗「庫存6個不足100個」
- **修復後：** ✅ 成功執行「回滾154個→重新扣減100個→剩餘60個」

**測試情境2：** 多次編輯回滾數量測試 🆕
- **修復前：** ❌ 第二次編輯回滾了歷史上所有扣減記錄（累積錯誤）
- **修復後：** ✅ 每次編輯只回滾實際需要回滾的淨數量（精確計算）

**測試情境3：** InventoryStock記錄完整性 🆕  
- **修復前：** ❌ 可能創建重複的庫存記錄
- **修復後：** ✅ 優先回滾到原記錄，確保數據一致性

這個解決方案為未來其他單據（如採購退貨、銷貨退貨等）的庫存回滾機制提供了良好的參考模板，並建立了一套完整的庫存管理最佳實務。

**關鍵創新點：**
1. **淨扣減量算法**：解決了多次編輯導致的重複回滾問題
2. **精確回滾機制**：基於InventoryStockId的直接回滾，避免記錄重複  
3. **智能驗證時機**：新增/編輯模式差異化處理，提升使用體驗
4. **多層安全保護**：精確回滾+後備機制+事務安全，確保系統穩定

---

**文件版本：** v3.0  
**最後更新：** 2025年9月19日  
**更新內容：** 回滾數量累積錯誤修正、精確回滾機制實現、測試案例完善、技術架構優化