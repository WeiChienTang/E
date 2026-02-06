# 返回編輯功能修復說明

## 問題描述

使用「返回編輯」功能（HandleReturnToLast）時，Table 組件有時無法正確顯示最新的明細資料。

### 問題現象
1. 點擊「返回編輯」後，Table 顯示的是舊資料（上一筆的明細）
2. 有時明細會重複顯示（例如：1 筆變成 3 筆）
3. Console 顯示 `LoadExistingDetailsAsync` 被多次呼叫

### 根本原因

當「返回編輯」觸發 `NavigateToRecordAsync` 時：
1. `GenericEditModalComponent` 載入新的 Entity
2. 父組件（如 `PurchaseOrderEditModalComponent`）的 `OnEntityLoaded` 被觸發
3. 父組件重新載入明細資料，建立**新的 List 物件**
4. Table 組件的 `OnParametersSetAsync` 被觸發多次
5. 原本的 Table 只追蹤 `SelectedSupplierId` 變化，沒有追蹤 `ExistingDetails` 參考變化
6. 導致 Table 沒有重新載入資料，或因多次觸發導致資料重複

## 解決方案：使用 `@key` 強制組件重建

### 原理

Blazor 的 `@key` 指令用於控制組件的重用。當 `@key` 值變化時，Blazor 會：
1. **銷毀**舊的組件實例
2. **創建**全新的組件實例
3. 自動執行 `OnInitializedAsync`，載入正確的資料

### 實施方式

在所有 EditModalComponent 的 Table 組件上加入 `@key`：

```razor
<PurchaseOrderTable @ref="purchaseOrderDetailManager"
                    @key="@editModalComponent.Entity.Id"
                    TMainEntity="PurchaseOrder" 
                    ...其他參數... />
```

## 修改清單

### 1. 移除 Table 中的複雜追蹤邏輯

**檔案：**
- `Components/Pages/Purchase/PurchaseOrderTable.razor`
- `Components/Pages/Sales/QuotationTable.razor`

**移除的程式碼：**
```csharp
// 已移除 - 不再需要手動追蹤
private int _previousExistingDetailsCount = 0;
private List<TDetailEntity>? _previousExistingDetailsRef = null;
private bool _isLoadingDetails = false;  // 載入鎖

// OnParametersSetAsync 中的 ReferenceEquals 檢測也已移除
bool existingDetailsRefChanged = !ReferenceEquals(_previousExistingDetailsRef, ExistingDetails);
```

**簡化後的 OnParametersSetAsync：**
```csharp
protected override async Task OnParametersSetAsync()
{
    base.OnParametersSet();
    
    // 使用 @key 方案後，ExistingDetails 變更會觸發組件重建，不需要手動追蹤
    bool supplierChanged = _previousSelectedSupplierId != SelectedSupplierId;
    
    if (supplierChanged)
    {
        _previousSelectedSupplierId = SelectedSupplierId;
        ProductItems.Clear();
        await LoadExistingDetailsAsync();
        await CheckLastPurchaseRecordAsync();
    }
}
```

### 2. 在 EditModalComponent 中加入 @key

**修改的檔案及位置：**

| 檔案 | Table 組件 | @key 值 |
|------|-----------|---------|
| `PurchaseOrderEditModalComponent.razor` | `<PurchaseOrderTable>` | `@editModalComponent.Entity.Id` |
| `QuotationEditModalComponent.razor` | `<QuotationTable>` | `@editModalComponent.Entity.Id` |
| `SalesOrderEditModalComponent.razor` | `<SalesOrderTable>` | `@editModalComponent.Entity.Id` |
| `SalesDeliveryEditModalComponent.razor` | `<SalesDeliveryTable>` | `@editModalComponent.Entity.Id` |
| `PurchaseReceivingEditModalComponent.razor` | `<PurchaseReceivingTable>` | `@editModalComponent.Entity.Id` |
| `InventoryStockEditModalComponent.razor` | `<InventoryStockTable>` | `@editModalComponent.Entity.Id` |
| `ProductCompositionEditModalComponent.razor` | `<ProductCompositionTable>` | `@editModalComponent.Entity.Id` |
| `SetoffDocumentEditModalComponent.razor` | `<SetoffProductTable>` | `@editModalComponent.Entity.Id` |

## 流程圖

```
返回編輯流程（修復後）
========================

1. 使用者點擊「返回編輯」
   ↓
2. GenericEditModalComponent.HandleReturnToLast()
   ↓
3. NavigateToRecordAsync(targetId)
   ↓
4. 載入新的 Entity（Entity.Id 變更）
   ↓
5. OnEntityLoaded 觸發父組件載入明細資料
   ↓
6. 🔑 @key 偵測到 Entity.Id 變化
   ↓
7. Blazor 銷毀舊 Table，創建新 Table
   ↓
8. 新 Table 執行 OnInitializedAsync
   ↓
9. LoadExistingDetailsAsync 載入正確的明細
   ↓
10. ✅ 顯示正確的資料
```

## 優點

1. **通用性高** - 所有使用 Table 的 EditModal 都自動受益
2. **程式碼簡化** - 移除複雜的參考追蹤和載入鎖邏輯
3. **可靠性提升** - 不會因為 OnParametersSetAsync 多次觸發而導致問題
4. **符合最佳實踐** - `@key` 是 Blazor 官方推薦的組件重用控制方式

## 注意事項

### 效能考量
- 使用 `@key` 會導致組件完全重建，比參數更新略慢
- 對於主明細編輯場景，這個效能開銷是可接受的
- 如果 Table 有大量初始化邏輯（如載入選項資料），可能需要在父組件快取

### 未來修改建議
如果需要進一步優化，可考慮：

1. **方案 A：Table 自己載入資料**
   - Table 接收 `MainEntityId` 而非 `ExistingDetails`
   - Table 內部透過 Service 載入資料
   - 完全解耦，但需要較大重構

2. **方案 B：使用 EventCallback 明確通知**
   - 父組件透過 EventCallback 通知 Table 刷新
   - 需要在每個 EditModal 和 Table 之間建立事件機制

## 相關檔案

- `Components/Shared/PageTemplate/GenericEditModalComponent.razor` - 包含 NavigateToRecordAsync
- `Components/Shared/Base/InteractiveTableComponent.razor` - 基礎表格組件
- `Documentation/README_上下筆Table跟著載入.md` - OnEntityLoaded 機制說明

## 修改日期

- **2026-02-06** - 初始修復，使用 @key 方案解決返回編輯問題
