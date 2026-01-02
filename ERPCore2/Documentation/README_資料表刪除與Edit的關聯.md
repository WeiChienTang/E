# 資料表刪除與 Edit Modal 的關聯

## 概述

本文件說明在 ERPCore2 系統中，明細資料表（Table Component）的刪除操作如何與編輯組件（Edit Modal）協作，最終讓資料庫產生異動。

## 核心概念

### 延遲刪除機制

系統採用「延遲刪除」設計模式：
- **Table 組件**：負責 UI 上的資料移除，但**不直接操作資料庫**
- **Edit Modal**：在使用者按下「儲存」時，才真正對資料庫進行異動

這種設計的優點：
1. 使用者可以反悔（關閉 Modal 不儲存即可還原）
2. 減少不必要的資料庫操作
3. 確保資料一致性（主檔和明細一起儲存）

---

## 實作方式

### 方式一：追蹤刪除列表（SupplierProductTable 模式）

適用於：獨立的明細管理，需要明確追蹤被刪除的項目

#### 架構圖

```
┌─────────────────────────────────────────────────────────────────┐
│                    SupplierEditModalComponent                    │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  productSuppliers: List<ProductSupplier>     ← 當前明細列表  │ │
│  │  deletedProductSuppliers: List<ProductSupplier> ← 待刪除列表 │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                              ▲                                   │
│                              │ OnItemDeleted 事件                │
│                              │                                   │
│  ┌─────────────────────────────────────────────────────────────┐ │    
│  │                   SupplierProductTable                       │ │
│  │  - 刪除單筆：觸發 OnItemDeleted                              │ │
│  │  - 清除全部：逐一觸發 OnItemDeleted                          │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

#### 關鍵程式碼

**1. Table 組件 - 刪除單筆項目**

```razor
// SupplierProductTable.razor
private async Task HandleDeleteItem(ProductSupplier item)
{
    Items.Remove(item);                          // 從 UI 列表移除
    await ItemsChanged.InvokeAsync(Items);       // 通知父組件列表已變更
    await OnItemDeleted.InvokeAsync(item);       // 🔑 通知父組件此項目被刪除
    await InvokeAsync(StateHasChanged);
}
```

**2. Table 組件 - 清除全部明細**

```razor
// SupplierProductTable.razor
private async Task ClearAllDetails()
{
    // 🔑 關鍵：在清除前，先收集需要刪除的項目
    var itemsToDelete = Items.Where(item => item.Id > 0 && item.Product != null).ToList();
    
    var cleared = await ItemManagementHelper.ClearAllDetailsAsync(
        Items,
        JSRuntime,
        async () =>
        {
            // 🔑 通知父組件哪些項目需要從資料庫刪除
            foreach (var item in itemsToDelete)
            {
                await OnItemDeleted.InvokeAsync(item);
            }
            
            await ItemsChanged.InvokeAsync(Items);
            tableComponent?.RefreshEmptyRow();
        },
        "確定要清除所有供應商品明細嗎？"
    );
}
```

**3. Edit Modal - 接收刪除通知**

```razor
// SupplierEditModalComponent.razor
private List<ProductSupplier> deletedProductSuppliers = new();  // 待刪除列表

private async Task HandleDeleteProductSupplier(ProductSupplier item)
{
    if (item.Id > 0)  // 只追蹤已存在於資料庫的項目
    {
        deletedProductSuppliers.Add(item);  // 🔑 加入待刪除列表
    }
    productSuppliers.Remove(item);
    await InvokeAsync(StateHasChanged);
}
```

**4. Edit Modal - 儲存時處理刪除**

```razor
// SupplierEditModalComponent.razor
private async Task SaveProductSuppliersAsync(int supplierId)
{
    // 🔑 刪除已標記刪除的項目
    foreach (var deletedItem in deletedProductSuppliers)
    {
        if (deletedItem.Id > 0)
        {
            await ProductSupplierService.DeleteAsync(deletedItem.Id);
        }
    }
    
    // 新增或更新有效綁定
    foreach (var ps in validProductSuppliers)
    {
        if (ps.Id > 0)
            await ProductSupplierService.UpdateAsync(ps);
        else
            await ProductSupplierService.CreateAsync(ps);
    }
    
    deletedProductSuppliers.Clear();  // 清空待刪除列表
}
```

---

### 方式二：比較差異刪除（PurchaseOrderTable 模式）

適用於：明細與主檔緊密關聯，透過比較資料庫現有資料與 UI 列表的差異來決定刪除

#### 架構圖

```
┌─────────────────────────────────────────────────────────────────┐
│                 PurchaseOrderEditModalComponent                  │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │  purchaseOrderDetails: List<PurchaseOrderDetail>             │ │
│  │  （只維護當前列表，不追蹤刪除項目）                           │ │
│  └─────────────────────────────────────────────────────────────┘ │
│                              ▲                                   │
│                              │ OnDetailsChanged 事件             │
│                              │                                   │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                    PurchaseOrderTable                        │ │
│  │  - 任何變更都觸發 OnDetailsChanged                           │ │
│  │  - 不需要額外的刪除事件                                      │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

#### 關鍵程式碼

**1. Table 組件 - 通知變更**

```razor
// PurchaseOrderTable.razor
private async Task NotifyDetailsChanged()
{
    var details = ConvertToDetailEntities();
    await DetailSyncHelper.SyncToParentAsync(details, OnDetailsChanged);
    // 不需要額外追蹤刪除項目
}
```

**2. Edit Modal - 儲存時比較差異**

```razor
// PurchaseOrderEditModalComponent.razor
private async Task SavePurchaseOrderDetails(int purchaseOrderId)
{
    // 🔑 從資料庫取得現有明細
    var existingDetails = await PurchaseOrderService.GetOrderDetailsAsync(purchaseOrderId);
    
    // 處理新增和更新
    foreach (var detail in purchaseOrderDetails.Where(d => d.ProductId > 0))
    {
        if (detail.Id == 0)
            await PurchaseOrderService.AddOrderDetailAsync(detail);
        else
            await PurchaseOrderService.UpdateOrderDetailAsync(detail);
    }
    
    // 🔑 關鍵：找出需要刪除的項目
    // （資料庫有，但當前列表沒有的項目）
    var currentDetailIds = purchaseOrderDetails
        .Where(d => d.Id > 0)
        .Select(d => d.Id)
        .ToList();
    
    var detailsToDelete = existingDetails
        .Where(e => !currentDetailIds.Contains(e.Id))
        .ToList();
    
    // 執行刪除
    foreach (var detailToDelete in detailsToDelete)
    {
        await PurchaseOrderService.DeleteOrderDetailAsync(detailToDelete.Id);
    }
}
```

---

## 兩種方式的比較

| 特性 | 追蹤刪除列表 | 比較差異刪除 |
|------|-------------|-------------|
| **實作複雜度** | 中等 | 較低 |
| **記憶體使用** | 需要額外列表 | 不需要 |
| **資料庫查詢** | 儲存時不需額外查詢 | 儲存時需查詢現有資料 |
| **適用場景** | 明細可獨立管理 | 明細與主檔緊密關聯 |
| **效能考量** | 大量刪除時較佳 | 少量變更時較佳 |

---

## 重要注意事項

### 1. 新增項目的處理

新增但尚未儲存的項目（`Id = 0`）：
- 不需要加入刪除列表
- 因為資料庫中不存在，只需從 UI 移除即可

```csharp
if (item.Id > 0)  // 只處理已存在於資料庫的項目
{
    deletedProductSuppliers.Add(item);
}
```

### 2. 清除明細時的處理

清除全部明細時，必須確保：
1. 收集所有有效的待刪除項目（`Id > 0` 且有選擇商品）
2. 逐一通知父組件
3. 清空 UI 列表

```csharp
var itemsToDelete = Items.Where(item => item.Id > 0 && item.Product != null).ToList();
```

### 3. 關閉 Modal 時的處理

如果使用者取消編輯（關閉 Modal 不儲存）：
- `deletedProductSuppliers` 會在下次開啟時被清空
- 或在 `OnParametersSetAsync` 中重置

```csharp
else if (!IsVisible)
{
    isDataLoaded = false;
    deletedProductSuppliers.Clear();  // 清空待刪除列表
}
```

---

## 流程圖

### 刪除單筆項目

```
使用者點擊刪除按鈕
        │
        ▼
Table.HandleDeleteItem()
        │
        ├── Items.Remove(item)          → UI 移除
        │
        ├── ItemsChanged.InvokeAsync()  → 通知列表變更
        │
        └── OnItemDeleted.InvokeAsync() → 通知項目被刪除
                    │
                    ▼
        Edit.HandleDeleteProductSupplier()
                    │
                    └── deletedProductSuppliers.Add(item) → 加入待刪除列表
                                │
                                ▼
                    使用者按下儲存
                                │
                                ▼
                    Edit.SaveProductSuppliersAsync()
                                │
                                └── DeleteAsync(item.Id) → 資料庫刪除
```

### 清除全部明細

```
使用者點擊清除明細按鈕
        │
        ▼
Table.ClearAllDetails()
        │
        ├── 收集待刪除項目 (Id > 0)
        │
        ├── ItemManagementHelper.ClearAllDetailsAsync()
        │           │
        │           └── 確認對話框 → 使用者確認
        │
        ├── 逐一 OnItemDeleted.InvokeAsync() → 通知每個被刪除的項目
        │
        ├── Items.Clear()               → UI 清空
        │
        └── ItemsChanged.InvokeAsync()  → 通知列表變更
                    │
                    ▼
        Edit 收到多個 OnItemDeleted 事件
                    │
                    └── 每個項目加入 deletedProductSuppliers
                                │
                                ▼
                    使用者按下儲存
                                │
                                ▼
                    foreach → DeleteAsync() → 資料庫批次刪除
```

---

## 相關檔案

- `Components/Shared/BaseModal/Modals/Supplier/SupplierProductTable.razor` - 供應商品資料表
- `Components/Pages/Suppliers/SupplierEditModalComponent.razor` - 廠商編輯 Modal
- `Components/Shared/BaseModal/Modals/Purchase/PurchaseOrderTable.razor` - 採購單商品資料表
- `Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor` - 採購單編輯 Modal
- `Helpers/Common/ItemManagementHelper.cs` - 項目管理輔助方法
