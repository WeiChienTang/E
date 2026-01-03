# 檢視功能實作指南

## 概述

「檢視」功能允許使用者在明細表格中查看與該明細相關的所有下一步單據（例如：從銷貨訂單查看出貨單、退貨單、沖款單等）。

本文件說明如何從零開始為任何明細表格組件實作完整的「檢視」功能。

---

## 架構說明

### 核心組件

1. **`RelatedDocument` Model** - 定義相關單據的資料結構
2. **`RelatedDocumentType` Enum** - 定義支援的單據類型
3. **`RelatedDocumentsHelper`** - 查詢相關單據的服務層
4. **`RelatedDocumentsModalComponent`** - 顯示相關單據的 Modal 組件
5. **`DocumentSectionConfig`** - 定義單據區塊的顯示配置
6. **明細表格組件（如 `SalesOrderTable.razor`）** - 觸發檢視功能
7. **編輯 Modal 組件（如 `SalesOrderEditModalComponent.razor`）** - 處理開啟相關單據

---

## 實作步驟

### 步驟 1：定義單據類型（如果需要新類型）

**檔案位置：** `Models/RelatedDocument.cs`

#### 1.1 新增單據類型到 `RelatedDocumentType` 枚舉

```csharp
public enum RelatedDocumentType
{
    ReturnDocument,        // 退貨單
    SetoffDocument,        // 沖款單
    ReceivingDocument,     // 入庫單
    SalesOrder,            // 銷貨訂單
    ProductComposition,    // 商品物料清單
    DeliveryDocument,      // 銷貨單/出貨單 ✅ 新增
    ProductionSchedule     // 生產排程 ✅ 新增
}
```

#### 1.2 更新 `RelatedDocument` 類別的顯示屬性

**Icon 屬性**（Bootstrap Icons）：
```csharp
public string Icon => DocumentType switch
{
    RelatedDocumentType.ReturnDocument => "bi-arrow-return-left",
    RelatedDocumentType.SetoffDocument => "bi-cash-coin",
    RelatedDocumentType.ReceivingDocument => "bi-box-seam",
    RelatedDocumentType.SalesOrder => "bi-cart-check",
    RelatedDocumentType.ProductComposition => "bi-diagram-3",
    RelatedDocumentType.DeliveryDocument => "bi-truck",           // ✅ 新增
    RelatedDocumentType.ProductionSchedule => "bi-calendar-check", // ✅ 新增
    _ => "bi-file-text"
};
```

**BadgeColor 屬性**：
```csharp
public string BadgeColor => DocumentType switch
{
    RelatedDocumentType.ReturnDocument => "warning",
    RelatedDocumentType.SetoffDocument => "success",
    RelatedDocumentType.ReceivingDocument => "info",
    RelatedDocumentType.SalesOrder => "primary",
    RelatedDocumentType.ProductComposition => "purple",
    RelatedDocumentType.DeliveryDocument => "info",        // ✅ 新增
    RelatedDocumentType.ProductionSchedule => "dark",      // ✅ 新增
    _ => "secondary"
};
```

**TypeDisplayName 屬性**：
```csharp
public string TypeDisplayName => DocumentType switch
{
    RelatedDocumentType.ReturnDocument => "退貨單",
    RelatedDocumentType.SetoffDocument => "沖款單",
    RelatedDocumentType.ReceivingDocument => "入庫單",
    RelatedDocumentType.SalesOrder => "銷貨訂單",
    RelatedDocumentType.ProductComposition => "商品物料清單",
    RelatedDocumentType.DeliveryDocument => "銷貨單",       // ✅ 新增
    RelatedDocumentType.ProductionSchedule => "生產排程",   // ✅ 新增
    _ => "未知單據"
};
```

---

### 步驟 2：更新 DocumentSectionConfig

**檔案位置：** `Components/Shared/BaseModal/Modals/RelatedDocument/Config/DocumentSectionConfig.cs`

在 `GetConfig` 方法中新增對應的配置：

```csharp
public static DocumentSectionConfig GetConfig(RelatedDocumentType type)
{
    return type switch
    {
        // ... 現有配置 ...
        
        RelatedDocumentType.DeliveryDocument => new()
        {
            Title = "出貨記錄",
            Icon = "truck",
            TextColor = "info",
            BadgeColor = "info",
            ShowAddButton = false
        },
        
        RelatedDocumentType.ProductionSchedule => new()
        {
            Title = "生產排程",
            Icon = "calendar-check",
            TextColor = "dark",
            BadgeColor = "dark",
            ShowAddButton = false
        },
        
        _ => throw new ArgumentException($"未知的單據類型: {type}")
    };
}
```

---

### 步驟 3：實作查詢方法（RelatedDocumentsHelper）

**檔案位置：** `Helpers/RelatedDocumentsHelper.cs`

#### 3.1 新增或修改查詢方法

每個明細類型需要一個對應的查詢方法。方法命名規則：
- `GetRelatedDocumentsFor{DetailType}Async`

**範例：銷貨訂單明細的查詢方法**

```csharp
/// <summary>
/// 取得與銷貨訂單明細相關的單據（出貨單、退貨單、沖款單、生產排程）
/// </summary>
public async Task<List<RelatedDocument>> GetRelatedDocumentsForSalesOrderDetailAsync(int salesOrderDetailId)
{
    var documents = new List<RelatedDocument>();
    using var context = await _contextFactory.CreateDbContextAsync();

    // 1. 查詢銷貨單/出貨單
    var deliveryDetails = await context.SalesDeliveryDetails
        .Include(d => d.SalesDelivery)
        .Where(d => d.SalesOrderDetailId == salesOrderDetailId)
        .ToListAsync();

    foreach (var detail in deliveryDetails)
    {
        documents.Add(new RelatedDocument
        {
            DocumentId = detail.SalesDeliveryId,
            DocumentType = RelatedDocumentType.DeliveryDocument,
            DocumentNumber = detail.SalesDelivery.Code ?? string.Empty,
            DocumentDate = detail.SalesDelivery.DeliveryDate,
            Quantity = detail.DeliveryQuantity,
            UnitPrice = detail.UnitPrice,
            Remarks = detail.SalesDelivery.Remarks
        });
    }

    // 2. 查詢生產排程
    var scheduleItems = await context.ProductionScheduleItems
        .Include(i => i.ProductionSchedule)
        .Include(i => i.Product)
        .Where(i => i.SalesOrderDetailId == salesOrderDetailId)
        .ToListAsync();

    foreach (var item in scheduleItems)
    {
        documents.Add(new RelatedDocument
        {
            DocumentId = item.ProductionScheduleId,
            DocumentType = RelatedDocumentType.ProductionSchedule,
            DocumentNumber = item.ProductionSchedule.Code ?? string.Empty,
            DocumentDate = item.ProductionSchedule.ScheduleDate,
            Quantity = item.ScheduledQuantity,
            Remarks = $"{item.Product.Name} - {item.ProductionItemStatus.ToString()}"
        });
    }

    // 3. 查詢退貨單（注意：退貨是從出貨單產生，需透過出貨明細查詢）
    var deliveryDetailIds = deliveryDetails.Select(d => d.Id).ToList();
    if (deliveryDetailIds.Any())
    {
        var returnDetails = await context.SalesReturnDetails
            .Include(d => d.SalesReturn)
            .Where(d => deliveryDetailIds.Contains(d.SalesDeliveryDetailId ?? 0))
            .ToListAsync();

        foreach (var detail in returnDetails)
        {
            documents.Add(new RelatedDocument
            {
                DocumentId = detail.SalesReturnId,
                DocumentType = RelatedDocumentType.ReturnDocument,
                DocumentNumber = detail.SalesReturn.Code ?? string.Empty,
                DocumentDate = detail.SalesReturn.ReturnDate,
                Quantity = detail.ReturnQuantity,
                Remarks = detail.SalesReturn.Remarks
            });
        }
    }

    // 4. 查詢沖款單
    var setoffDetails = await context.SetoffProductDetails
        .Include(d => d.SetoffDocument)
        .Where(d => d.SourceDetailType == SetoffDetailType.SalesOrderDetail 
                 && d.SourceDetailId == salesOrderDetailId)
        .ToListAsync();

    foreach (var detail in setoffDetails)
    {
        if (detail.SetoffDocument == null) continue;
        
        documents.Add(new RelatedDocument
        {
            DocumentId = detail.SetoffDocumentId,
            DocumentType = RelatedDocumentType.SetoffDocument,
            DocumentNumber = detail.SetoffDocument.Code ?? string.Empty,
            DocumentDate = detail.SetoffDocument.SetoffDate,
            Amount = detail.CurrentSetoffAmount,
            CurrentAmount = detail.CurrentSetoffAmount,
            TotalAmount = detail.TotalSetoffAmount,
            Remarks = detail.SetoffDocument.Remarks
        });
    }

    return documents.OrderByDescending(d => d.DocumentDate).ToList();
}
```

#### 3.2 查詢方法的關鍵要點

1. **關聯欄位**：找出下一步單據明細表中關聯來源明細的外鍵
   - 例如：`SalesDeliveryDetail.SalesOrderDetailId` → `SalesOrderDetail.Id`
   - 例如：`ProductionScheduleItem.SalesOrderDetailId` → `SalesOrderDetail.Id`

2. **Include 相關實體**：確保載入主檔資料以取得編號、日期等資訊
   ```csharp
   .Include(d => d.SalesDelivery)
   ```

3. **設定正確的 DocumentType**：使用對應的枚舉值

4. **排序結果**：通常按日期降序排列
   ```csharp
   return documents.OrderByDescending(d => d.DocumentDate).ToList();
   ```

---

### 步驟 4：在明細表格組件中實作「檢視」按鈕

**檔案位置：** 例如 `Components/Shared/BaseModal/Modals/Sales/SalesOrderTable.razor`

#### 4.1 注入必要的服務

```csharp
@inject RelatedDocumentsHelper RelatedDocumentsHelper
@inject INotificationService NotificationService
```

#### 4.2 加入 RelatedDocumentsModalComponent

```razor
<!-- 相關單據查看 Modal -->
<RelatedDocumentsModalComponent IsVisible="@showRelatedDocumentsModal"
                               IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
                               ProductName="@selectedProductName"
                               RelatedDocuments="@relatedDocuments"
                               IsLoading="@isLoadingRelatedDocuments"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

#### 4.3 定義私有變數

```csharp
@code {
    // ===== 相關單據查看 =====
    private bool showRelatedDocumentsModal = false;
    private string selectedProductName = string.Empty;
    private List<RelatedDocument>? relatedDocuments = null;
    private bool isLoadingRelatedDocuments = false;
}
```

#### 4.4 加入事件參數（用於通知父組件開啟相關單據）

```csharp
/// <summary>
/// 相關單據開啟事件 - 通知父組件開啟指定的相關單據
/// </summary>
[Parameter] 
public EventCallback<(RelatedDocumentType type, int id)> OnOpenRelatedDocument { get; set; }
```

#### 4.5 在 GetCustomActionsTemplate 中加入「檢視」按鈕

```razor
private RenderFragment<SalesItem> GetCustomActionsTemplate => item => __builder =>
{
    <div class="d-flex gap-1">
        <!-- 檢視按鈕：顯示相關單據 -->
        @if (item.ExistingDetailEntity != null && 
             item.ExistingDetailEntity is SalesOrderDetail detail && 
             detail.Id > 0)
        {
            <GenericButtonComponent
                Icon="eye"
                OnClick="async () => await ShowRelatedDocuments(item)"
                Title="檢視相關單據（出貨、退貨、沖款、排程等）"
                Variant="ButtonVariant.Info"
                Size="ButtonSize.Small"
                IconOnly="true" />
        }
        
        <!-- 其他按鈕... -->
    </div>
};
```

#### 4.6 實作 ShowRelatedDocuments 方法

```csharp
/// <summary>
/// 顯示相關單據（出貨單、退貨單、沖款單、生產排程）
/// </summary>
private async Task ShowRelatedDocuments(SalesItem item)
{
    // 檢查是否有現有的明細實體 ID
    if (item.ExistingDetailEntity is not SalesOrderDetail detail || detail.Id <= 0)
    {
        await NotificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
        return;
    }

    // 設定商品名稱
    selectedProductName = item.SelectedProduct?.Name ?? "未知商品";

    // 顯示 Modal 並開始載入
    showRelatedDocumentsModal = true;
    isLoadingRelatedDocuments = true;
    relatedDocuments = null;
    StateHasChanged();

    try
    {
        // 查詢相關單據（呼叫 RelatedDocumentsHelper）
        relatedDocuments = await RelatedDocumentsHelper.GetRelatedDocumentsForSalesOrderDetailAsync(detail.Id);
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入相關單據失敗：{ex.Message}");
    }
    finally
    {
        isLoadingRelatedDocuments = false;
        StateHasChanged();
    }
}
```

#### 4.7 實作 HandleRelatedDocumentClick 方法

```csharp
/// <summary>
/// 處理點擊相關單據的事件
/// </summary>
private async Task HandleRelatedDocumentClick(RelatedDocument document)
{
    // 觸發事件，讓父組件（EditModal）處理開啟相關單據
    if (OnOpenRelatedDocument.HasDelegate)
    {
        await OnOpenRelatedDocument.InvokeAsync((document.DocumentType, document.DocumentId));
    }
    else
    {
        // 如果父組件沒有處理，顯示提示訊息
        await NotificationService.ShowInfoAsync(
            $"請在主畫面中開啟 {document.TypeDisplayName}: {document.DocumentNumber}", 
            "提示"
        );
    }
}
```

---

### 步驟 5：在父組件（EditModal）處理相關單據開啟

**檔案位置：** 例如 `Components/Pages/Sales/SalesOrderEditModalComponent.razor`

#### 5.1 在明細表格組件上綁定事件

```razor
<SalesOrderTable @ref="salesOrderTable"
                 Products="@products"
                 SelectedCustomerId="@Entity?.CustomerId"
                 OnOpenRelatedDocument="@HandleOpenRelatedDocument"
                 ... />
```

#### 5.2 準備相關 Modal 組件（如果尚未加入）

```razor
@* 銷貨退回單編輯 Modal *@
<SalesReturnEditModalComponent IsVisible="@showSalesReturnModal"
                              IsVisibleChanged="@((bool visible) => showSalesReturnModal = visible)"
                              SalesReturnId="@selectedSalesReturnId"
                              OnSalesReturnSaved="@HandleSalesReturnSaved"
                              OnCancel="@(() => showSalesReturnModal = false)" />

@* 銷貨出貨單編輯 Modal *@
<SalesDeliveryEditModalComponent IsVisible="@showSalesDeliveryModal"
                                IsVisibleChanged="@(value => showSalesDeliveryModal = value)"
                                SalesDeliveryId="@selectedSalesDeliveryId"
                                OnSalesDeliverySaved="@HandleSalesDeliverySaved"
                                OnCancel="@(() => showSalesDeliveryModal = false)" />

@* 沖款單編輯 Modal *@
<SetoffDocumentEditModalComponent IsVisible="@showSetoffDocumentModal"
                                 IsVisibleChanged="@((bool visible) => showSetoffDocumentModal = visible)"
                                 SetoffDocumentId="@selectedSetoffDocumentId"
                                 OnSetoffDocumentSaved="@HandleSetoffDocumentSaved"
                                 OnCancel="@(() => showSetoffDocumentModal = false)" />
```

#### 5.3 定義私有變數

```csharp
@code {
    // ===== 相關單據 Modal 控制 =====
    private bool showSalesReturnModal = false;
    private int? selectedSalesReturnId = null;
    
    private bool showSalesDeliveryModal = false;
    private int? selectedSalesDeliveryId = null;
    
    private bool showSetoffDocumentModal = false;
    private int? selectedSetoffDocumentId = null;
}
```

#### 5.4 實作 HandleOpenRelatedDocument 方法

```csharp
/// <summary>
/// 處理開啟相關單據的事件
/// </summary>
private async Task HandleOpenRelatedDocument((RelatedDocumentType type, int id) args)
{
    try
    {
        if (args.type == RelatedDocumentType.ReturnDocument)
        {
            // 開啟銷貨退回單
            selectedSalesReturnId = args.id;
            showSalesReturnModal = true;
            StateHasChanged();
        }
        else if (args.type == RelatedDocumentType.SetoffDocument)
        {
            // 開啟沖款單
            selectedSetoffDocumentId = args.id;
            showSetoffDocumentModal = true;
            StateHasChanged();
        }
        else if (args.type == RelatedDocumentType.DeliveryDocument)
        {
            // 開啟銷貨出貨單
            selectedSalesDeliveryId = args.id;
            showSalesDeliveryModal = true;
            StateHasChanged();
        }
        else if (args.type == RelatedDocumentType.ProductionSchedule)
        {
            // 開啟生產排程（如果尚未整合，可以先顯示提示）
            await NotificationService.ShowInfoAsync("請在生產管理模組中查看生產排程", "提示");
        }
        else
        {
            await NotificationService.ShowWarningAsync("不支援的單據類型", "提示");
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"開啟單據失敗：{ex.Message}");
    }
}
```

#### 5.5 實作各單據儲存後的處理方法（可選）

```csharp
/// <summary>
/// 處理銷貨退回單儲存後的事件
/// </summary>
private async Task HandleSalesReturnSaved(SalesReturn savedReturn)
{
    try
    {
        showSalesReturnModal = false;
        await NotificationService.ShowSuccessAsync("銷貨退回單已更新");
        
        // 重新載入明細以更新退貨數量
        if (salesOrderTable != null)
        {
            await salesOrderTable.LoadReturnedQuantitiesAsync();
        }
        
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理銷貨退回單儲存事件失敗：{ex.Message}");
    }
}
```

---

## 檢查清單

實作「檢視」功能時，請確認以下項目都已完成：

### ✅ Model 層
- [ ] 在 `RelatedDocumentType` 枚舉中新增單據類型
- [ ] 更新 `RelatedDocument` 的 `Icon` 屬性
- [ ] 更新 `RelatedDocument` 的 `BadgeColor` 屬性
- [ ] 更新 `RelatedDocument` 的 `TypeDisplayName` 屬性

### ✅ Config 層
- [ ] 在 `DocumentSectionConfig.GetConfig` 中新增單據類型配置

### ✅ Service 層
- [ ] 在 `RelatedDocumentsHelper` 中實作查詢方法
- [ ] 確認查詢方法包含所有相關的下一步單據
- [ ] 確認查詢方法正確使用 Include 載入關聯資料

### ✅ 明細表格組件
- [ ] 注入 `RelatedDocumentsHelper` 和 `INotificationService`
- [ ] 加入 `RelatedDocumentsModalComponent`
- [ ] 定義私有變數（showRelatedDocumentsModal、relatedDocuments 等）
- [ ] 加入 `OnOpenRelatedDocument` 事件參數
- [ ] 在 `GetCustomActionsTemplate` 中加入「檢視」按鈕
- [ ] 實作 `ShowRelatedDocuments` 方法
- [ ] 實作 `HandleRelatedDocumentClick` 方法
- [ ] 檢視按鈕只在明細已儲存時顯示（`detail.Id > 0`）

### ✅ 父組件（EditModal）
- [ ] 在明細表格組件上綁定 `OnOpenRelatedDocument` 事件
- [ ] 準備相關 Modal 組件（退貨、出貨、沖款等）
- [ ] 定義私有變數（showXxxModal、selectedXxxId）
- [ ] 實作 `HandleOpenRelatedDocument` 方法
- [ ] 處理所有支援的單據類型
- [ ] 實作各單據儲存後的處理方法（可選）

---

## 常見問題

### Q1：如何找出下一步單據的關聯欄位？

**A：** 檢查下一步單據的明細實體（Detail Entity），找出關聯來源明細的外鍵。

範例：
- `SalesDeliveryDetail.SalesOrderDetailId` → 關聯到 `SalesOrderDetail.Id`
- `ProductionScheduleItem.SalesOrderDetailId` → 關聯到 `SalesOrderDetail.Id`
- `PurchaseReturnDetail.PurchaseReceivingDetailId` → 關聯到 `PurchaseReceivingDetail.Id`

### Q2：退貨單的查詢邏輯為何較複雜？

**A：** 因為退貨單通常是從出貨單產生，而非直接從訂單產生。所以需要：
1. 先查出所有出貨明細（透過訂單明細 ID）
2. 再透過出貨明細 ID 查詢退貨明細

範例：
```csharp
// 先查出貨明細
var deliveryDetails = await context.SalesDeliveryDetails
    .Where(d => d.SalesOrderDetailId == salesOrderDetailId)
    .ToListAsync();

// 取得出貨明細的 ID 列表
var deliveryDetailIds = deliveryDetails.Select(d => d.Id).ToList();

// 透過出貨明細 ID 查詢退貨明細
if (deliveryDetailIds.Any())
{
    var returnDetails = await context.SalesReturnDetails
        .Where(d => deliveryDetailIds.Contains(d.SalesDeliveryDetailId ?? 0))
        .ToListAsync();
}
```

### Q3：如果某種單據類型暫時無法開啟怎麼辦？

**A：** 在 `HandleOpenRelatedDocument` 方法中顯示提示訊息：

```csharp
else if (args.type == RelatedDocumentType.ProductionSchedule)
{
    await NotificationService.ShowInfoAsync("請在生產管理模組中查看生產排程", "提示");
}
```

### Q4：為什麼要在父組件處理開啟單據，而不是在明細表格組件中直接開啟？

**A：** 因為：
1. **關注點分離**：明細表格組件專注於顯示明細，父組件負責管理所有 Modal
2. **狀態管理**：父組件可以統一管理各種 Modal 的開啟/關閉狀態
3. **靈活性**：不同的父組件可以用不同方式處理相同的事件

### Q5：RelatedDocument 的 Quantity 和 Amount 欄位如何選擇？

**A：** 根據單據性質：
- **Quantity**：用於有數量的單據（退貨數量、出貨數量、排程數量等）
- **Amount**：用於金額相關的單據（沖款金額、收款金額等）
- **UnitPrice**：用於有單價的單據（入庫單價、出貨單價等）

---

## 參考範例

### 完整範例 1：銷貨訂單明細的檢視功能

- **查詢方法**：`RelatedDocumentsHelper.GetRelatedDocumentsForSalesOrderDetailAsync`
- **明細組件**：`SalesOrderTable.razor`
- **父組件**：`SalesOrderEditModalComponent.razor`
- **支援單據**：出貨單、退貨單、沖款單、生產排程

### 完整範例 2：採購入庫明細的檢視功能

- **查詢方法**：`RelatedDocumentsHelper.GetRelatedDocumentsForPurchaseReceivingDetailAsync`
- **明細組件**：`PurchaseReceivingTable.razor`
- **父組件**：`PurchaseReceivingEditModalComponent.razor`（或其他）
- **支援單據**：退貨單、沖款單

---

## 版本記錄

| 日期 | 版本 | 說明 |
|------|------|------|
| 2025-12-10 | 1.0 | 初版建立，記錄完整的檢視功能實作流程 |

---

## 總結

實作「檢視」功能的核心步驟：

1. **定義單據類型** → `RelatedDocumentType` 枚舉
2. **配置顯示樣式** → `DocumentSectionConfig`
3. **實作查詢邏輯** → `RelatedDocumentsHelper`
4. **加入檢視按鈕** → 明細表格組件
5. **處理開啟動作** → 父組件（EditModal）

遵循此文件的步驟，可以快速為任何明細表格實作完整的「檢視」功能。
