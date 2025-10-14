# 採購訂單明細刪除保護與相關單據查看功能

## 📋 功能說明

為採購訂單明細實作與其他單據一致的設計：

1. **刪除保護**：已被入庫單使用的採購訂單明細不能刪除
2. **相關單據查看**：不能刪除時，顯示「查看」按鈕，可以查看該明細在哪些入庫單中被使用
3. **ActionButton 隱藏**：採購單審核通過後，ActionButton 應該隱藏而非禁用

---

## 🏗️ 實作內容

### 1. Models/RelatedDocument.cs

#### 新增入庫單類型
```csharp
public enum RelatedDocumentType
{
    ReturnDocument,      // 退貨單
    SetoffDocument,      // 沖款單
    ReceivingDocument    // 入庫單（新增）
}
```

#### 更新顯示屬性
使用 switch expression 來處理三種單據類型：
- 圖示：`bi-box-seam`
- 顏色：`info`
- 顯示名稱：`入庫單`

---

### 2. Helpers/RelatedDocumentsHelper.cs

#### 新增查詢方法
```csharp
/// <summary>
/// 取得與採購訂單明細相關的單據（入庫單）
/// </summary>
public async Task<List<RelatedDocument>> GetRelatedDocumentsForPurchaseOrderDetailAsync(int purchaseOrderDetailId)
```

**查詢邏輯**：
- 查詢資料表：`PurchaseReceivingDetails`
- 查詢條件：`PurchaseOrderDetailId == purchaseOrderDetailId`
- 包含導覽屬性：`PurchaseReceiving`
- 返回資訊：
  - 入庫單編號：`ReceiptNumber`
  - 入庫日期：`ReceiptDate`
  - 入庫數量：`ReceivedQuantity`
  - 備註：顯示入庫數量和單價

---

### 3. PurchaseOrderDetailManagerComponent.razor

#### 新增依賴注入
```razor
@using ERPCore2.Models
@inject RelatedDocumentsHelper RelatedDocumentsHelper
```

#### 新增狀態變數
```csharp
// ===== 相關單據查看 =====
private bool showRelatedDocumentsModal = false;
private string selectedProductName = string.Empty;
private List<RelatedDocument>? relatedDocuments = null;
private bool isLoadingRelatedDocuments = false;
```

#### 新增 Modal 組件
```razor
<RelatedDocumentsModalComponent IsVisible="@showRelatedDocumentsModal"
                               IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
                               ProductName="@selectedProductName"
                               RelatedDocuments="@relatedDocuments"
                               IsLoading="@isLoadingRelatedDocuments"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

#### 修改 InteractiveTableComponent 配置
```razor
<InteractiveTableComponent TItem="ProductItem" 
                          ...
                          ShowBuiltInDeleteButton="false"
                          CustomActionsTemplate="@GetCustomActionsTemplate"
                          ... />
```

#### 實作 CustomActionsTemplate
```csharp
private RenderFragment<ProductItem> GetCustomActionsTemplate => item => __builder =>
{
    var hasUsage = item.HasUsageRecordCache ?? false;
    
    if (!hasUsage)
    {
        // 顯示刪除按鈕
    }
    else
    {
        // 顯示查看按鈕
    }
};
```

#### 新增檢查方法
```csharp
/// <summary>
/// 檢查指定的採購訂單明細項目是否已被入庫單使用
/// </summary>
private async Task<bool> HasUsageRecord(ProductItem item)

/// <summary>
/// 顯示相關單據（入庫單）
/// </summary>
private async Task ShowRelatedDocuments(ProductItem item)

/// <summary>
/// 處理點擊相關單據的事件
/// </summary>
private async Task HandleRelatedDocumentClick(RelatedDocument document)
```

#### 更新 ProductItem 類別
```csharp
public class ProductItem
{
    ...
    // 標記是否已被入庫單使用（避免重複查詢）
    public bool? HasUsageRecordCache { get; set; } = null;
}
```

#### 更新載入方法
將 `LoadExistingDetailsAsync` 改為 async 方法，在載入時檢查每個項目是否被使用：

```csharp
private async Task LoadExistingDetailsAsync()
{
    ...
    foreach (var detail in ExistingDetails)
    {
        var item = new ProductItem { ... };
        
        // 檢查是否已被入庫單使用
        item.HasUsageRecordCache = await HasUsageRecord(item);
        
        ProductItems.Add(item);
    }
    ...
}
```

---

### 4. PurchaseOrderEditModalComponent.razor

#### 修改 ActionButton 邏輯
```csharp
private async Task<List<FieldActionButton>> GetSupplierActionButtonsAsync()
{
    // 如果採購單已核准，隱藏所有按鈕（返回空列表）
    bool isApproved = editModalComponent?.Entity?.IsApproved ?? false;
    if (isApproved)
    {
        return new List<FieldActionButton>();
    }
    
    var buttons = await ActionButtonHelper.GenerateFieldActionButtonsAsync(
        editModalComponent, 
        supplierModalManager, 
        nameof(PurchaseOrder.SupplierId)
    );
    
    return buttons;
}
```

**改進說明**：
- **原本**：審核通過後禁用所有按鈕 (`button.IsDisabled = true`)
- **現在**：審核通過後直接返回空列表，按鈕完全隱藏

---

## 🎯 使用流程

### 案例 1：新建的採購訂單明細
1. 建立採購單並新增明細
2. 明細項目旁顯示「刪除」按鈕（紅色垃圾桶圖示）
3. 可以正常刪除

### 案例 2：已被入庫單使用的明細
1. 建立採購單並新增明細後儲存
2. 建立入庫單，並選擇該採購訂單的明細
3. 重新編輯採購單
4. 該明細項目旁顯示「查看」按鈕（藍色眼睛圖示）
5. 點擊「查看」按鈕，開啟相關單據 Modal
6. Modal 顯示：
   - 商品名稱
   - 入庫單列表（入庫單號、日期、數量等）
7. 點擊入庫單項目，顯示提示訊息

### 案例 3：採購單審核通過
1. 採購單審核通過後
2. 廠商欄位的 ActionButton（新增、編輯、檢視）完全隱藏
3. 明細項目的操作按鈕變為：
   - 未被使用：仍顯示刪除按鈕（但實際上審核後應該也不允許刪除，這部分可能需要額外邏輯）
   - 已被使用：顯示查看按鈕

---

## 🔍 技術細節

### 快取機制
為避免重複查詢資料庫，使用 `HasUsageRecordCache` 屬性：
- 在載入明細時，一次性檢查所有項目的使用狀態
- 將結果快取在 `ProductItem.HasUsageRecordCache` 中
- 在 `CustomActionsTemplate` 中直接使用快取值

### 非同步處理
由於 `CustomActionsTemplate` 是在渲染期間執行，無法直接呼叫 async 方法，因此：
1. 在 `LoadExistingDetailsAsync` 中預先查詢所有項目的使用狀態
2. 將結果快取在 `HasUsageRecordCache` 屬性中
3. 在渲染時直接使用快取的值

### 資料查詢
查詢邏輯位於 `RelatedDocumentsHelper.GetRelatedDocumentsForPurchaseOrderDetailAsync`：
```csharp
var receivingDetails = await context.PurchaseReceivingDetails
    .Include(d => d.PurchaseReceiving)
    .Where(d => d.PurchaseOrderDetailId == purchaseOrderDetailId)
    .ToListAsync();
```

---

## 📊 與其他單據的一致性

| 單據類型 | DetailManager | 限制條件 | 相關單據類型 | 狀態 |
|---------|---------------|---------|-------------|------|
| 採購訂單 | PurchaseOrderDetailManager | 已被入庫單使用 | 入庫單 | ✅ 已完成 |
| 採購進貨 | PurchaseReceivingDetailManager | 已有退貨或沖款 | 退貨單、沖款單 | ✅ 已完成 |
| 採購退貨 | PurchaseReturnDetailManager | 已有沖款 | 沖款單 | ✅ 已完成 |
| 銷貨訂單 | SalesOrderDetailManager | 已有退回或沖款 | 退回單、沖款單 | ⏳ 待實作 |
| 銷貨退回 | SalesReturnDetailManager | 已有沖款 | 沖款單 | ⏳ 待實作 |

---

## 🚀 後續改進建議

### 1. 審核後完全鎖定
目前審核通過後：
- ✅ ActionButton 已隱藏
- ⚠️ 明細的刪除按鈕仍可能顯示（如果未被入庫單使用）

建議加強：
```csharp
private RenderFragment<ProductItem> GetCustomActionsTemplate => item => __builder =>
{
    // 如果主單已審核，所有明細都不能刪除，只能查看
    bool isMainEntityApproved = IsMainEntityApproved;
    var hasUsage = item.HasUsageRecordCache ?? false;
    
    if (isMainEntityApproved || hasUsage)
    {
        // 顯示查看按鈕
    }
    else
    {
        // 顯示刪除按鈕
    }
};
```

### 2. 直接開啟相關單據
目前點擊相關單據只顯示提示訊息，未來可以：
- 實作直接開啟入庫單 Modal
- 處理多層 Modal 的 z-index
- 提供便捷的單據導覽功能

### 3. 更豐富的資訊顯示
在相關單據 Modal 中可以顯示：
- 入庫單狀態（已核准、待核准）
- 倉庫和庫位資訊
- 更詳細的備註

---

## � 已修復的問題

### 問題 1: 操作按鈕不顯示
**原因**: `ShowBuiltInActions="false"` 導致自訂操作按鈕不會顯示
**修正**: 將 `ShowBuiltInActions` 改為 `true`，並設置 `ShowBuiltInDeleteButton="false"`

### 問題 2: Modal 顯示但無內容
**原因**: `RelatedDocumentsModalComponent` 只處理 `ReturnDocument` 和 `SetoffDocument`，沒有處理 `ReceivingDocument`
**修正**: 在 Modal 中添加入庫單區塊的顯示邏輯

```razor
@* 入庫單區塊 *@
@if (receivingDocs.Any())
{
    <div class="mb-4">
        <h6 class="text-info mb-3">
            <i class="bi bi-box-seam me-2"></i>
            入庫記錄 (@receivingDocs.Count)
        </h6>
        <div class="list-group">
            @foreach (var doc in receivingDocs)
            {
                <!-- 顯示入庫單資訊 -->
            }
        </div>
    </div>
}
```

---

## �📅 變更歷史

| 日期 | 版本 | 變更內容 | 作者 |
|------|------|----------|------|
| 2025-01-13 | 1.0 | 初始版本 - 實作採購訂單明細刪除保護與相關單據查看功能 | GitHub Copilot |
| 2025-01-13 | 1.1 | 修復操作按鈕不顯示和 Modal 無內容的問題 | GitHub Copilot |

---

## 🔗 相關文件

- [相關單據查看功能實作指南](./README_RelatedDocumentsView.md)
- [刪除限制設計](./README_刪除限制設計.md)
- [進貨單刪除限制增強](./README_PurchaseReceiving_刪除限制增強.md)
- [退貨明細刪除限制設計](./README_PurchaseReturnDetail_刪除限制設計.md)
