# 相關單據檢視 Modal 設計文件

## 概述

`Components/Shared/RelatedDocument/` 資料夾提供了一套完整的「相關單據檢視」解決方案，用於在 ERP 系統中顯示與特定項目關聯的單據清單（如入庫記錄、退貨記錄、沖款記錄等）。

此架構採用**組件化設計**，將顯示邏輯、配置與範本分離，便於擴充新的單據類型。

---

## 資料夾結構

```
Components/Shared/RelatedDocument/
├── RelatedDocumentsModalComponent.razor    # 主要 Modal 組件
├── InventoryTransactionRelatedModal.razor  # 庫存異動專用 Modal
├── Components/
│   └── RelatedDocumentSectionComponent.razor  # 單據區塊子組件
├── Config/
│   └── DocumentSectionConfig.cs              # 單據類型配置
└── Templates/                                 # 各類型詳細欄位範本
    ├── CompositionDetailsTemplate.razor
    ├── InventoryTransactionDetailsTemplate.razor
    ├── ReceivingDetailsTemplate.razor
    ├── ReturnDetailsTemplate.razor
    ├── SalesOrderDetailsTemplate.razor
    ├── SetoffDetailsTemplate.razor
    └── SupplierRecommendationDetailsTemplate.razor
```

---

## 核心組件說明

### 1. RelatedDocumentsModalComponent

主要的相關單據檢視 Modal，自動根據單據類型分組顯示。

**參數：**

| 參數名稱 | 類型 | 說明 |
|---------|------|------|
| `IsVisible` | `bool` | 控制 Modal 顯示/隱藏 |
| `IsVisibleChanged` | `EventCallback<bool>` | 雙向綁定回調 |
| `ProductName` | `string` | 顯示於標題的商品名稱 |
| `RelatedDocuments` | `List<RelatedDocumentInfo>?` | 相關單據資料清單 |
| `IsLoading` | `bool` | 載入狀態 |
| `OnDocumentClick` | `EventCallback<RelatedDocumentInfo>` | 點擊單據時觸發 |
| `OnAddNew` | `EventCallback` | 點擊新增按鈕時觸發 |

### 2. RelatedDocumentSectionComponent

單一類型的單據區塊組件，由主 Modal 內部使用。

**參數：**

| 參數名稱 | 類型 | 說明 |
|---------|------|------|
| `Documents` | `List<RelatedDocumentInfo>` | 該類型的單據清單 |
| `Config` | `DocumentSectionConfig` | 區塊顯示配置 |
| `DetailsTemplate` | `RenderFragment<RelatedDocumentInfo>?` | 自訂詳細欄位範本 |
| `OnDocumentClick` | `EventCallback<RelatedDocumentInfo>` | 點擊事件回調 |

### 3. DocumentSectionConfig

定義各單據類型的顯示配置（標題、圖示、顏色等）。

```csharp
public class DocumentSectionConfig
{
    public string Title { get; init; }        // 區塊標題
    public string Icon { get; init; }         // Bootstrap Icons 類別
    public string TextColor { get; init; }    // 文字顏色
    public string BadgeColor { get; init; }   // Badge 背景顏色
    public bool ShowAddButton { get; init; }  // 是否顯示新增按鈕
    public string AddButtonText { get; init; } // 新增按鈕文字
}
```

---

## 支援的單據類型

| 類型 | 說明 | 圖示 | 顏色 |
|------|------|------|------|
| `ProductComposition` | 商品物料清單 (BOM) | diagram-3 | purple |
| `SalesOrder` | 銷貨訂單 | cart-check | primary |
| `ReceivingDocument` | 入庫記錄 | box-seam | info |
| `ReturnDocument` | 退貨記錄 | arrow-return-left | warning |
| `SetoffDocument` | 沖款記錄 | cash-coin | success |
| `DeliveryDocument` | 出貨記錄 | truck | info |
| `ProductionSchedule` | 生產排程 | calendar-check | dark |
| `SupplierRecommendation` | 供應商推薦 | shop | success |
| `InventoryTransaction` | 庫存異動記錄 | arrow-left-right | secondary |

---

## 使用範例

### 基本使用（參考 PurchaseOrderTable.razor）

#### 步驟 1：宣告狀態變數

```csharp
@code {
    // 相關單據查看狀態
    private bool showRelatedDocumentsModal = false;
    private string selectedProductName = string.Empty;
    private List<RelatedDocumentInfo>? relatedDocuments = null;
    private bool isLoadingRelatedDocuments = false;
}
```

#### 步驟 2：放置 Modal 組件

```razor
<!-- 相關單據查看 Modal -->
<RelatedDocumentsModalComponent 
    IsVisible="@showRelatedDocumentsModal"
    IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
    ProductName="@selectedProductName"
    RelatedDocuments="@relatedDocuments"
    IsLoading="@isLoadingRelatedDocuments"
    OnDocumentClick="@HandleRelatedDocumentClick" />
```

#### 步驟 3：顯示相關單據方法

```csharp
/// <summary>
/// 顯示相關單據（入庫單）
/// </summary>
private async Task ShowRelatedDocuments(ProductItem item)
{
    if (item.ExistingDetailEntity is not PurchaseOrderDetail detail || detail.Id <= 0)
    {
        await NotificationService.ShowWarningAsync("此商品尚未儲存，無法查看相關單據");
        return;
    }

    selectedProductName = item.SelectedProduct?.Name ?? "未知商品";
    
    // 顯示 Modal 並設定載入狀態
    showRelatedDocumentsModal = true;
    isLoadingRelatedDocuments = true;
    relatedDocuments = null;
    StateHasChanged();

    try
    {
        // 從服務取得相關單據資料
        relatedDocuments = await SomeService.GetRelatedDocumentsAsync(detail.Id);
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入相關單據失敗: {ex.Message}");
    }
    finally
    {
        isLoadingRelatedDocuments = false;
        StateHasChanged();
    }
}
```

#### 步驟 4：處理點擊事件

```csharp
/// <summary>
/// 處理點擊相關單據的事件
/// </summary>
private async Task HandleRelatedDocumentClick(RelatedDocumentInfo document)
{
    // 觸發事件，讓父組件處理開啟相關單據
    if (OnOpenRelatedDocument.HasDelegate)
    {
        await OnOpenRelatedDocument.InvokeAsync((document.DocumentType, document.DocumentId));
    }
    else
    {
        // 預設行為：顯示通知
        await NotificationService.ShowInfoAsync($"開啟單據: {document.DocumentNumber}");
    }
}
```

### 在表格操作欄位使用

```razor
private RenderFragment<ProductItem> GetCustomActionsTemplate => item => __builder =>
{
    if (DetailLockHelper.CanDeleteItem(item, out _))
    {
        // 可刪除：顯示刪除按鈕
        <GenericButtonComponent Variant="ButtonVariant.Red"
                               Icon="trash"
                               OnClick="@(() => HandleItemDelete(item))" />
    }
    else
    {
        // 已被使用：顯示查看按鈕
        <GenericButtonComponent Variant="ButtonVariant.Blue"
                               Icon="eye"
                               Tooltip="查看相關單據"
                               OnClick="@(() => ShowRelatedDocuments(item))" />
    }
};
```

---

## 擴充新的單據類型

### 步驟 1：新增 Enum 值

在 `Models/Enums/RelatedDocumentType.cs` 新增：

```csharp
/// <summary>
/// 新單據類型
/// </summary>
NewDocumentType,
```

### 步驟 2：新增配置

在 `Config/DocumentSectionConfig.cs` 的 `GetConfig` 方法新增：

```csharp
RelatedDocumentType.NewDocumentType => new()
{
    Title = "新單據類型",
    Icon = "file-text",        // Bootstrap Icons 名稱
    TextColor = "primary",
    BadgeColor = "primary",
    ShowAddButton = false
},
```

### 步驟 3：新增詳細欄位範本（可選）

在 `Templates/` 資料夾建立 `NewDocumentDetailsTemplate.razor`：

```razor
@* 新單據類型詳細欄位範本 *@
@using ERPCore2.Models

<span class="text-muted">
    <i class="bi bi-calendar3 me-1"></i>@Document.DocumentDate.ToString("yyyy-MM-dd")
</span>
@if (Document.Quantity.HasValue)
{
    <span class="text-muted">
        <i class="bi bi-hash me-1"></i>數量: @Document.Quantity.Value
    </span>
}

@code {
    [Parameter, EditorRequired]
    public RelatedDocumentInfo Document { get; set; } = null!;
}
```

### 步驟 4：註冊範本

在 `RelatedDocumentsModalComponent.razor` 的 `GetDetailsTemplate` 方法新增：

```csharp
RelatedDocumentType.NewDocumentType => doc => 
    @<NewDocumentDetailsTemplate Document="@doc" />,
```

### 步驟 5：更新 RelatedDocumentInfo

在 `Models/Documents/RelatedDocument.cs` 的 `Icon` 和 `BadgeColor` 屬性新增對應值。

---

## 資料模型

### RelatedDocumentInfo

```csharp
public class RelatedDocumentInfo
{
    public int DocumentId { get; set; }           // 單據 ID
    public RelatedDocumentType DocumentType { get; set; }  // 單據類型
    public string DocumentNumber { get; set; }    // 單據編號
    public DateTime DocumentDate { get; set; }    // 單據日期
    public decimal? Quantity { get; set; }        // 相關數量
    public decimal? UnitPrice { get; set; }       // 單價
    public decimal? Amount { get; set; }          // 相關金額
    public decimal? CurrentAmount { get; set; }   // 本次金額
    public decimal? TotalAmount { get; set; }     // 累計金額
    public string? Remarks { get; set; }          // 備註
    
    // 計算屬性
    public string Icon { get; }                   // 根據類型自動取得圖示
    public string BadgeColor { get; }             // 根據類型自動取得顏色
    public string TypeDisplayName { get; }        // 類型顯示名稱
}
```

---

## 設計特點

1. **自動分組**：Modal 自動根據 `DocumentType` 將單據分組顯示
2. **可擴充**：透過配置和範本機制，輕鬆新增單據類型
3. **一致性**：統一的 UI 風格與互動體驗
4. **單行設計**：採用精簡的單行列表，資訊密度高
5. **懶載入**：支援非同步載入，避免阻塞 UI

---

## 相關檔案

- `Models/Documents/RelatedDocument.cs` - 資料模型定義
- `Models/Enums/RelatedDocumentType.cs` - 單據類型枚舉
- `Helpers/RelatedDocumentsHelper.cs` - 服務層輔助方法
- [README_共用元件設計總綱.md](README_共用元件設計總綱.md) - 共用元件設計總綱
