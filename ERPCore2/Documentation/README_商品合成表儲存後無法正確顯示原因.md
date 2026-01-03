# 商品物料清單儲存後無法正確顯示問題分析與修復

## 問題現象

**使用者反饋：**
新增一筆銷貨訂單明細時，即使選擇了有組合表(BOM)的商品，儲存後在「庫存檢查視窗」(OrderInventoryCheckModal)中不會顯示該商品的組合明細。必須重新進入編輯模式、再次選取商品並儲存，組合明細才會顯示。

## 問題根本原因

### 1. 資料架構理解

系統中有兩套 BOM 資料：

- **ProductComposition / ProductCompositionDetail**：產品主檔的 BOM 模板（全局設定）
- **SalesOrderCompositionDetail**：訂單專屬的 BOM 副本（可修改）

當新增訂單明細時，系統需要將 ProductComposition 複製到 SalesOrderCompositionDetail，但此步驟未正確執行。

### 2. 臨時索引映射錯誤

**核心問題：** 新增明細時使用負數作為臨時索引，但索引生成與解析邏輯不一致，導致組合明細被儲存到錯誤的 DetailId。

#### 問題編號（修復前）

**SalesOrderTable.razor - GetCompositionDetails()**
```csharp
// ❌ 錯誤：使用遞減的臨時索引
int tempIndex = -1;
foreach (var item in SalesItems)
{
    if (/* 新增明細 */)
    {
        result[tempIndex] = item.CustomCompositionDetails;
        tempIndex--; // -1, -2, -3...
    }
}
```

**問題：** 第一個新增項目使用 -1，第二個用 -2，但沒有對應到 SalesItems 的實際索引位置。

#### 映射轉換問題（修復前）

**SalesOrderEditModalComponent.razor - SaveSalesOrderCompositionDetails()**
```csharp
// ❌ 錯誤：Math.Abs(salesOrderDetailId) - 1 無法正確還原
int salesItemIndex = Math.Abs(salesOrderDetailId) - 1;
// -1 → 0 (正確)
// -2 → 1 (正確)
// 但如果 SalesItems[1] 才是新增的，-1 會指向 SalesItems[0]（可能是編輯項目）
```

### 3. 實際案例重現

**場景：** SalesItems 有 2 項
- SalesItems[0] = A1（編輯模式，DetailId=1）
- SalesItems[1] = A13（新增模式，DetailId=0）

**錯誤流程：**
```
GetCompositionDetails():
  A1 (編輯) → result[1] = [...] ✓ 正確
  A13 (新增) → result[-1] = [...] ❌ 錯誤，應該用 -2

SaveSalesOrderCompositionDetails():
  處理 -1:
    salesItemIndex = Math.Abs(-1) - 1 = 0
    找到 SalesItems[0] = A1 ❌ 錯誤！應該是 A13
    使用 ProductId=2 查詢
    找到 DetailId=1 (A1 的 ID)
    A13 的組合明細被存到 DetailId=1 ❌ 數據錯亂！
```

## 修復方案

### 核心修復：位置對應的臨時索引

**關鍵原則：** 臨時索引必須與 SalesItems 陣列索引一一對應

#### 修復後的編號

**SalesOrderTable.razor - GetCompositionDetails()**
```csharp
// ✅ 正確：使用位置對應的臨時索引
for (int i = 0; i < SalesItems.Count; i++)
{
    var item = SalesItems[i];
    var detailId = item.ExistingDetailEntity?.Id ?? 0;
    
    if (item.CustomCompositionDetails?.Any() == true)
    {
        if (detailId > 0)
        {
            result[detailId] = item.CustomCompositionDetails; // 編輯模式
        }
        else
        {
            // 🔑 關鍵修正：使用 -(i+1)
            int tempIndex = -(i + 1);
            // SalesItems[0] → -1
            // SalesItems[1] → -2
            // SalesItems[2] → -3
            
            foreach (var detail in item.CustomCompositionDetails)
            {
                detail.SalesOrderDetailId = tempIndex;
            }
            result[tempIndex] = item.CustomCompositionDetails;
        }
    }
}
```

**SalesOrderEditModalComponent.razor - SaveSalesOrderCompositionDetails()**
```csharp
// ✅ 正確：反向計算回陣列索引
if (salesOrderDetailId < 0)
{
    // 🔑 反向計算：-(i+1) → i
    int salesItemIndex = Math.Abs(salesOrderDetailId) - 1;
    // -1 → 0 (SalesItems[0])
    // -2 → 1 (SalesItems[1])
    // -3 → 2 (SalesItems[2])
    
    var salesItem = salesOrderDetailManager.SalesItems[salesItemIndex];
    
    // 使用 ProductId 查詢已儲存的明細
    var newDetails = salesOrderDetails
        .Where(d => d.ProductId == salesItem.SelectedProduct?.Id)
        .OrderBy(d => d.Id)
        .ToList();
    
    // 處理同商品多筆明細的情況
    int newItemSequence = /* 計算這是第幾個新增項 */;
    int existingCount = /* 計算既有項數量 */;
    
    actualDetailId = newDetails[newItemSequence + existingCount].Id;
}
```

### 映射對應表

| SalesItems 索引 | 項目狀態 | 使用的 ID/索引 | 說明 |
|----------------|---------|---------------|------|
| 0 | 編輯 (DetailId=1) | `1` | 直接使用實際 DetailId |
| 1 | 新增 (無 DetailId) | `-2` | 使用 -(1+1) = -2 |
| 2 | 編輯 (DetailId=5) | `5` | 直接使用實際 DetailId |
| 3 | 新增 (無 DetailId) | `-4` | 使用 -(3+1) = -4 |

## 修復效果

### 修復前
```
新增 A13 → 
GetCompositionDetails: result[-1] → 
SaveSalesOrderCompositionDetails: 找到 A1 (錯誤) → 
存到 DetailId=1 ❌
```

### 修復後
```
新增 A13 (SalesItems[1]) → 
GetCompositionDetails: result[-2] (因為 i=1) → 
SaveSalesOrderCompositionDetails: 
  Math.Abs(-2) - 1 = 1 → SalesItems[1] (正確) → 
  找到 A13 → 存到正確的 DetailId ✓
```

## 技術細節

### 為什麼使用 -(i+1) 而不是 -i

```csharp
// ❌ 使用 -i
SalesItems[0] → -0 = 0  // 與未儲存的 DetailId=0 混淆！

// ✅ 使用 -(i+1)
SalesItems[0] → -1
SalesItems[1] → -2
SalesItems[2] → -3
// 所有值都是負數，不會與正數的實際 DetailId 或 0 混淆
```

### 處理同商品多筆明細

當 SalesItems 中有多筆相同 ProductId 的新增項目時：

```csharp
// 計算這是第幾個新增的該商品項目
int newItemSequence = 0;
for (int idx = 0; idx <= salesItemIndex; idx++)
{
    var checkItem = salesOrderDetailManager.SalesItems[idx];
    if (checkItem.SelectedProduct?.Id == salesItem.SelectedProduct?.Id && 
        checkItem.ExistingDetailEntity == null) // 只計算新增項
    {
        if (idx == salesItemIndex) break;
        newItemSequence++;
    }
}

// 排除既有項目，取得對應的新增明細
var existingCount = /* 既有項目數量 */;
var matchedDetail = newDetails[newItemSequence + existingCount];
```

## 相關檔案

- [Components/Shared/BaseModal/Modals/Sales/SalesOrderTable.razor](../Components/Shared/BaseModal/Modals/Sales/SalesOrderTable.razor)
  - `GetCompositionDetails()` 方法
  
- [Components/Pages/Sales/SalesOrderEditModalComponent.razor](../Components/Pages/Sales/SalesOrderEditModalComponent.razor)
  - `SaveSalesOrderCompositionDetails()` 方法
  
- [Components/Shared/BaseModal/Modals/Sales/OrderInventoryCheckModal.razor](../Components/Shared/BaseModal/Modals/Sales/OrderInventoryCheckModal.razor)
  - 顯示組合明細的視窗

- [Data/Entities/SalesOrderCompositionDetail.cs](../Data/Entities/SalesOrderCompositionDetail.cs)
  - 訂單組合明細實體

## 學到的教訓

1. **臨時 ID 設計原則**
   - 必須與陣列索引明確對應
   - 避免使用順序遞增/遞減，改用位置對應
   - 使用負數確保不與實際 ID 衝突

2. **索引映射的雙向一致性**
   - 生成邏輯：`-(i+1)`
   - 解析邏輯：`Math.Abs(tempIndex) - 1`
   - 兩者必須互為反函數

3. **除錯技巧**
   - 使用 ConsoleHelper 追蹤臨時索引轉換過程
   - 記錄每個步驟的 ProductId 和 DetailId
   - 驗證陣列索引對應關係

## 測試建議

### 測試案例 1：單一新增項目
1. 開啟現有訂單（已有 1 筆明細 A1）
2. 新增組合商品 A13
3. 儲存
4. 開啟庫存檢查視窗
5. **預期：** A13 顯示組合明細展開按鈕

### 測試案例 2：多筆新增項目
1. 開啟新訂單
2. 新增 A13、B5、A13（重複商品）
3. 儲存
4. **預期：** 三筆明細各自有正確的組合明細

### 測試案例 3：混合編輯與新增
1. 開啟現有訂單（已有 A1, B2）
2. 修改 A1 數量
3. 新增 A13
4. 儲存
5. **預期：** A1 和 A13 的組合明細都正確

## 版本資訊

- **修復日期：** 2025-12-16
- **影響版本：** 所有包含組合商品功能的版本
- **修復分支：** main
- **相關 Commit：** 修復商品物料清單臨時索引映射錯誤
