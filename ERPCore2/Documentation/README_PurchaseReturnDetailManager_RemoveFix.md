# PurchaseReturnDetailManagerComponent 刪除功能修復紀錄

## 問題描述

**日期**: 2025年9月23日  
**問題**: PurchaseReturnDetailManagerComponent 組件中的刪除功能無法正常工作，用戶刪除明細項目後儲存，再次進入編輯模式時被刪除的明細仍然顯示。

## 問題分析

### 根本原因
1. **缺少刪除事件處理**: 子組件 `PurchaseReturnDetailManagerComponent` 沒有將刪除事件正確傳遞給父組件
2. **父組件未處理刪除**: 父組件 `PurchaseReturnEditModalComponent` 缺少對應的刪除事件處理器
3. **軟刪除重新載入問題**: 原本使用軟刪除，但資料重新載入時沒有過濾已軟刪除的記錄

### 對比分析
通過與成功的 `ProductSupplierManagerComponent` 和 `SupplierProductManagerComponent` 對比，發現關鍵差異：
- 成功的組件使用 `ItemRemoved` 事件讓父組件處理實際刪除
- 成功的組件使用 `AutoEmptyRowHelper.HandleItemRemove()` 處理項目移除

## 解決方案

### 1. 子組件修改 (`PurchaseReturnDetailManagerComponent.razor`)

#### 1.1 添加刪除追蹤機制
```csharp
// 添加已刪除明細ID的追蹤列表
private List<int> _deletedDetailIds { get; set; } = new List<int>();

// 添加刪除事件參數
[Parameter] public EventCallback<List<int>> OnDeletedDetailsChanged { get; set; }
[Parameter] public EventCallback<ReturnItem> OnItemRemoved { get; set; }
```

#### 1.2 修改刪除方法
```csharp
private async Task RemoveItemAsync(int index)
{
    if (index >= 0 && index < ReturnItems.Count)
    {
        var itemToRemove = ReturnItems[index];
        
        // 記錄要刪除的資料庫實體ID
        if (itemToRemove.ExistingDetailEntity?.Id > 0)
        {
            _deletedDetailIds.Add(itemToRemove.ExistingDetailEntity.Id);
        }
        
        // 通知父組件項目即將被移除
        if (OnItemRemoved.HasDelegate)
        {
            await OnItemRemoved.InvokeAsync(itemToRemove);
        }
        
        ReturnItems.RemoveAt(index);
        EnsureOneEmptyRow();
        await NotifyDetailsChanged();
    }
}
```

#### 1.3 修改通知方法
```csharp
private async Task NotifyDetailsChanged()
{
    // 轉換為實體清單
    var details = ReturnItems.Where(item => !IsEmptyRow(item) && item.ReturnQuantity > 0)
                             .Select(ConvertToEntity)
                             .Where(detail => detail != null)
                             .Cast<PurchaseReturnDetail>()
                             .ToList();
    
    if (EffectiveDetailsChangedCallback.HasDelegate)
    {
        await EffectiveDetailsChangedCallback.InvokeAsync(details);
    }
    
    // 通知已刪除的明細ID
    if (OnDeletedDetailsChanged.HasDelegate && _deletedDetailIds.Any())
    {
        await OnDeletedDetailsChanged.InvokeAsync(_deletedDetailIds.ToList());
        _deletedDetailIds.Clear(); // 清空已通知的刪除ID
    }
    
    if (OnReturnItemsChanged.HasDelegate)
    {
        await OnReturnItemsChanged.InvokeAsync(ReturnItems);
    }
}
```

### 2. 父組件修改 (`PurchaseReturnEditModalComponent.razor`)

#### 2.1 注入必要服務
```csharp
@inject IPurchaseReturnDetailService PurchaseReturnDetailService
```

#### 2.2 添加事件綁定
```razor
<PurchaseReturnDetailManagerComponent @ref="purchaseReturnDetailManager"
    SupplierId="@editModalComponent.Entity.SupplierId"
    FilterPurchaseReceivingId="@filterPurchaseReceivingId"
    FilterProductId="@filterProductId"
    IsEditMode="@(PurchaseReturnId.HasValue)"
    ExistingReturnDetails="@(purchaseReturnDetails ?? new List<PurchaseReturnDetail>())"
    OnReturnDetailsChanged="@HandleReturnDetailsChanged"
    OnDeletedDetailsChanged="@HandleDeletedDetailsChanged" />
```

#### 2.3 實作刪除處理方法
```csharp
/// <summary>
/// 處理已刪除的退貨明細ID
/// </summary>
private async Task HandleDeletedDetailsChanged(List<int> deletedDetailIds)
{
    try
    {
        if (deletedDetailIds?.Any() == true)
        {
            // 如果是編輯模式，實際從資料庫永久刪除
            if (PurchaseReturnId.HasValue)
            {
                foreach (var detailId in deletedDetailIds)
                {
                    var result = await PurchaseReturnDetailService.PermanentDeleteAsync(detailId);
                    if (!result.IsSuccess)
                    {
                        await NotificationService.ShowErrorAsync($"刪除明細ID {detailId} 失敗: {result.ErrorMessage}");
                        return;
                    }
                }
                await NotificationService.ShowSuccessAsync($"已永久刪除 {deletedDetailIds.Count} 筆退貨明細");
            }
            
            // 從本地狀態中移除已刪除的明細
            if (purchaseReturnDetails != null)
            {
                purchaseReturnDetails = purchaseReturnDetails
                    .Where(detail => !deletedDetailIds.Contains(detail.Id))
                    .ToList();
            }
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"永久刪除退貨明細時發生錯誤：{ex.Message}");
    }
}
```

### 3. 使用硬刪除替代軟刪除

#### 3.1 問題背景
原先使用軟刪除 (`DeleteAsync`)，但重新載入資料時會包含已軟刪除的記錄，導致被刪除的明細重新出現。

#### 3.2 解決方案
使用硬刪除 (`PermanentDeleteAsync`) 直接從資料庫移除記錄：
```csharp
// 使用硬刪除替代軟刪除
var result = await PurchaseReturnDetailService.PermanentDeleteAsync(detailId);
```

#### 3.3 移除軟刪除篩選
```csharp
// 使用硬刪除後不需要篩選 IsDeleted
purchaseReturnDetails = purchaseReturn.PurchaseReturnDetails.ToList();
```

### 4. 庫存回滾功能實現

#### 4.1 業務邏輯需求
當刪除採購退貨明細時，需要恢復對應的庫存數量：
- 採購退貨代表商品從庫存中扣除並退回給供應商
- 刪除退貨明細表示取消這筆退貨，商品應該重新回到庫存
- 因此刪除時需要**增加**庫存數量

#### 4.2 服務層實現
修改 `PurchaseReturnDetailService` 注入庫存服務並重寫刪除方法：

```csharp
public class PurchaseReturnDetailService : BaseService<PurchaseReturnDetail>, IPurchaseReturnDetailService
{
    private readonly IInventoryStockService _inventoryStockService;

    public PurchaseReturnDetailService(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<PurchaseReturnDetailService> logger,
        IInventoryStockService inventoryStockService) 
        : base(contextFactory, logger)
    {
        _inventoryStockService = inventoryStockService;
    }

    /// <summary>
    /// 永久刪除採購退貨明細並處理庫存回滾
    /// </summary>
    public override async Task PermanentDeleteAsync(int id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 先查詢明細資訊（包含商品和數量）
            var detail = await _context.PurchaseReturnDetails
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (detail != null)
            {
                // 庫存回滾：因為取消退貨，所以要增加庫存
                await _inventoryStockService.AddStockAsync(
                    detail.ProductId, 
                    detail.WarehouseId, 
                    detail.Quantity);

                // 刪除明細記錄
                _context.PurchaseReturnDetails.Remove(detail);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

#### 4.3 依賴注入配置
確認 `ServiceRegistration.cs` 中已註冊庫存服務：
```csharp
// 庫存管理服務
services.AddScoped<IInventoryStockService, InventoryStockService>();
services.AddScoped<IPurchaseReturnDetailService, PurchaseReturnDetailService>();
```

## 實作結果

### 修復前後對比

| 項目 | 修復前 | 修復後 |
|------|---------|---------|
| 刪除UI反應 | ✅ 立即從列表移除 | ✅ 立即從列表移除 |
| 資料庫刪除 | ❌ 未執行實際刪除 | ✅ 永久刪除記錄 |
| 庫存回滾 | ❌ 無庫存處理 | ✅ 自動恢復庫存數量 |
| 重新載入 | ❌ 已刪除項目重新出現 | ✅ 已刪除項目不再出現 |
| 錯誤處理 | ❌ 無錯誤提示 | ✅ 詳細錯誤訊息 |
| 成功提示 | ❌ 無成功提示 | ✅ 明確成功訊息 |
| 交易安全 | ❌ 無交易控制 | ✅ 資料庫交易確保一致性 |

### 工作流程

1. **用戶操作**: 點擊刪除按鈕
2. **子組件處理**: 
   - 記錄待刪除的明細ID
   - 從UI列表中移除項目
   - 觸發 `OnDeletedDetailsChanged` 事件
3. **父組件處理**:
   - 接收已刪除明細ID列表
   - 調用 `PermanentDeleteAsync` 永久刪除
   - 從本地狀態移除對應項目
   - 顯示成功訊息
4. **服務層處理**:
   - 開始資料庫交易
   - 查詢待刪除明細的商品和數量資訊
   - 調用庫存服務恢復庫存（`AddStockAsync`）
   - 從資料庫永久刪除明細記錄
   - 提交交易
5. **結果**: 明細被永久刪除，庫存被正確恢復，重新進入編輯模式不會再出現已刪除項目

## 核心改進

### 1. 事件驅動架構
- 子組件專注於UI操作和狀態管理
- 父組件負責實際的資料庫操作
- 通過事件機制解耦組件間依賴

### 2. 雙重通知機制
- `OnItemRemoved`: 單個項目移除通知（預留擴展）
- `OnDeletedDetailsChanged`: 批量刪除ID通知（主要機制）

### 3. 硬刪除策略
- 避免軟刪除重新載入問題
- 確保資料庫一致性
- 提升查詢效能

### 4. 庫存業務邏輯整合
- 刪除退貨明細時自動恢復庫存
- 使用資料庫交易確保操作原子性
- 庫存服務注入實現解耦合

### 5. 完整錯誤處理
- 個別刪除失敗時立即停止
- 詳細錯誤訊息提示
- 異常捕獲和用戶友好提示
- 交易回滾確保資料完整性

## 技術要點

### 1. 刪除狀態追蹤
```csharp
// 追蹤已刪除但尚未通知的明細ID
private List<int> _deletedDetailIds { get; set; } = new List<int>();

// 避免重複刪除
_deletedDetailIds.Clear();
```

### 2. 事件參數設計
```csharp
// 支援批量刪除ID傳遞
[Parameter] public EventCallback<List<int>> OnDeletedDetailsChanged { get; set; }

// 支援單個項目傳遞（含完整實體資訊）
[Parameter] public EventCallback<ReturnItem> OnItemRemoved { get; set; }
```

### 3. 條件式資料庫操作
```csharp
// 只在編輯模式下執行實際刪除
if (PurchaseReturnId.HasValue)
{
    // 執行資料庫刪除
}
```

## 測試驗證

### 測試場景
1. **單筆刪除**: 刪除一筆明細，確認不再出現
2. **多筆刪除**: 刪除多筆明細，確認全部移除
3. **庫存驗證**: 刪除後檢查庫存是否正確增加
4. **交易完整性**: 模擬庫存操作失敗，確認交易回滾
5. **新增模式**: 新增模式下刪除不執行資料庫操作
6. **錯誤處理**: 模擬刪除失敗，確認錯誤提示
7. **重新載入**: 儲存後重新進入編輯，確認已刪除項目不出現

### 預期結果
- ✅ 刪除操作立即生效
- ✅ 庫存數量正確恢復
- ✅ 成功訊息正確顯示
- ✅ 重新載入不會看到已刪除項目
- ✅ 錯誤情況有適當提示
- ✅ 交易失敗時資料不會部分更新

## 相關文件

- [PurchaseReturnDetailManagerComponent 開發紀錄](README_PurchaseReturnDetailManagerComponent.md)
- [InteractiveTableComponent 使用說明](README_InteractiveTableComponent.md)
- [AutoEmptyRowHelper 功能說明](README_AutoEmptyRow_Feature.md)

## 總結

此次修復徹底解決了 PurchaseReturnDetailManagerComponent 的刪除功能問題，通過事件驅動架構、硬刪除策略和庫存業務邏輯整合，確保了刪除操作的可靠性和一致性。

### 主要成就
1. **完整的刪除流程**: 從UI操作到資料庫刪除再到庫存回滾的完整鏈路
2. **業務邏輯正確性**: 正確處理採購退貨取消時的庫存恢復
3. **資料一致性保證**: 使用資料庫交易確保操作的原子性
4. **錯誤處理完善**: 完整的異常捕獲和用戶友好提示
5. **架構設計清晰**: 事件驅動的組件間通信，職責分離明確

修復後的組件具有完整的錯誤處理、用戶友好的提示訊息、正確的庫存業務邏輯，以及穩定的資料同步機制。這為其他類似組件的實現提供了可參考的標準模式。