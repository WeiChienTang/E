# RelatedDocumentsModalComponent 重構計畫

## 📋 目錄
- [問題分析](#問題分析)
- [重構目標](#重構目標)
- [重構方案](#重構方案)
- [檔案結構](#檔案結構)
- [實作步驟](#實作步驟)
- [使用範例](#使用範例)
- [遷移指南](#遷移指南)

---

## 🔍 問題分析

### 現有問題

目前的 `RelatedDocumentsModalComponent.razor` 存在以下問題：

#### 1. **大量重複代碼**
```razor
@* 每個單據類型都重複相同的 HTML 結構 *@
<div class="mb-4">
    <h6 class="text-purple mb-3">...</h6>
    <div class="list-group">
        @foreach (var doc in compositionDocs) { ... }
    </div>
</div>

<div class="mb-4">
    <h6 class="text-primary mb-3">...</h6>
    <div class="list-group">
        @foreach (var doc in salesOrderDocs) { ... }
    </div>
</div>
<!-- 重複 5 次... -->
```

#### 2. **硬編碼的配置**
- 顏色：`text-purple`, `text-primary`, `text-info`, `text-warning`, `text-success`
- 圖示：`bi-diagram-3`, `bi-cart-check`, `bi-box-seam`, 等
- 標題：`商品物料清單`, `銷貨訂單`, `入庫記錄`, 等

#### 3. **不一致的欄位顯示邏輯**
- 商品物料清單：只顯示日期和備註
- 銷貨訂單：顯示日期、數量、單價、備註
- 入庫單：顯示日期、數量、單價、備註
- 退貨單：顯示日期、數量、備註
- 沖款單：顯示日期、多種金額欄位、備註

#### 4. **Footer 按鈕邏輯混亂**
```razor
@if (RelatedDocuments?.Any() == true && 
     RelatedDocuments.First().DocumentType == RelatedDocumentType.ProductComposition)
{
    @* 只有商品物料清單才顯示新增按鈕 *@
}
```

#### 5. **擴展困難**
- 新增單據類型需要複製整個區塊（約 50 行程式碼）
- 修改樣式需要改 5 個地方
- 容易遺漏或不一致

---

## 🎯 重構目標

### 主要目標

1. ✅ **消除重複代碼**：減少 80% 以上的重複 HTML
2. ✅ **提高可維護性**：集中管理配置，單一修改點
3. ✅ **增強擴展性**：新增單據類型只需加配置，不需複製編號
4. ✅ **保持靈活性**：每種單據類型可自訂顯示內容
5. ✅ **向下相容**：不影響現有功能和調用方式

### 次要目標

- 提升程式碼可讀性
- 增強類型安全
- 便於單元測試
- 改善開發體驗

---

## 🏗️ 重構方案

### 方案選擇：混合架構（Configuration + Component）

結合**配置驅動**和**組件化**的優勢：

```
配置類 (Config)          定義單據類型的顯示規則
    ↓
子組件 (Section)         處理重複的 HTML 結構
    ↓
範本 (Templates)         自訂每種單據的詳細欄位顯示
    ↓
主組件 (Modal)           組合以上元素，提供統一介面
```

---

## 📁 檔案結構

### 新增檔案

```
Components/Shared/BaseModal/Modals/RelatedDocument/
│
├── RelatedDocumentsModalComponent.razor          (主 Modal - 重構後)
├── README_Related寫法重構.md                     (本文件)
│
├── Config/
│   └── DocumentSectionConfig.cs                  (配置類)
│
├── Components/
│   └── RelatedDocumentSectionComponent.razor     (可重用的區塊組件)
│
└── Templates/
    ├── CompositionDetailsTemplate.razor          (商品物料清單詳細欄位範本)
    ├── SalesOrderDetailsTemplate.razor           (銷貨訂單詳細欄位範本)
    ├── ReceivingDetailsTemplate.razor            (入庫單詳細欄位範本)
    ├── ReturnDetailsTemplate.razor               (退貨單詳細欄位範本)
    └── SetoffDetailsTemplate.razor               (沖款單詳細欄位範本)
```

### 檔案說明

| 檔案 | 責任 | 大小 |
|------|------|------|
| `DocumentSectionConfig.cs` | 定義每種單據的配置（顏色、圖示、標題、行為） | ~100 行 |
| `RelatedDocumentSectionComponent.razor` | 渲染單據區塊的共用 HTML 結構 | ~80 行 |
| `*DetailsTemplate.razor` | 定義每種單據的詳細欄位顯示邏輯 | ~30-50 行/個 |
| `RelatedDocumentsModalComponent.razor` | 主 Modal（重構後簡化到 ~100 行） | ~100 行 |

---

## 🔧 實作步驟

### 步驟 1：建立配置類

**檔案**：`Config/DocumentSectionConfig.cs`

```csharp
namespace ERPCore2.Components.Shared.BaseModal.Modals.RelatedDocument.Config;

/// <summary>
/// 定義單據區塊的顯示配置
/// </summary>
public class DocumentSectionConfig
{
    /// <summary>
    /// 區塊標題（例如：「商品物料清單」）
    /// </summary>
    public string Title { get; init; } = "";
    
    /// <summary>
    /// 標題圖示（Bootstrap Icons 類別名稱，例如：「diagram-3」）
    /// </summary>
    public string Icon { get; init; } = "";
    
    /// <summary>
    /// 標題文字顏色（Bootstrap 顏色類別，例如：「purple」）
    /// </summary>
    public string TextColor { get; init; } = "primary";
    
    /// <summary>
    /// Badge 背景顏色（Bootstrap 顏色類別）
    /// </summary>
    public string BadgeColor { get; init; } = "primary";
    
    /// <summary>
    /// Badge 文字顏色（例如：「text-dark」用於淺色背景）
    /// </summary>
    public string BadgeTextClass { get; init; } = "";
    
    /// <summary>
    /// 是否顯示「新增」按鈕
    /// </summary>
    public bool ShowAddButton { get; init; }
    
    /// <summary>
    /// 「新增」按鈕的文字
    /// </summary>
    public string AddButtonText { get; init; } = "+ 新增";
    
    /// <summary>
    /// 根據單據類型取得對應的配置
    /// </summary>
    public static DocumentSectionConfig GetConfig(RelatedDocumentType type)
    {
        return type switch
        {
            RelatedDocumentType.ProductComposition => new()
            {
                Title = "商品物料清單",
                Icon = "diagram-3",
                TextColor = "purple",
                BadgeColor = "purple",
                ShowAddButton = true,
                AddButtonText = "+ 新增物料清單"
            },
            
            RelatedDocumentType.SalesOrder => new()
            {
                Title = "銷貨訂單",
                Icon = "cart-check",
                TextColor = "primary",
                BadgeColor = "primary",
                ShowAddButton = false
            },
            
            RelatedDocumentType.ReceivingDocument => new()
            {
                Title = "入庫記錄",
                Icon = "box-seam",
                TextColor = "info",
                BadgeColor = "info",
                ShowAddButton = false
            },
            
            RelatedDocumentType.ReturnDocument => new()
            {
                Title = "退貨記錄",
                Icon = "arrow-return-left",
                TextColor = "warning",
                BadgeColor = "warning",
                BadgeTextClass = "text-dark",
                ShowAddButton = false
            },
            
            RelatedDocumentType.SetoffDocument => new()
            {
                Title = "沖款記錄",
                Icon = "cash-coin",
                TextColor = "success",
                BadgeColor = "success",
                ShowAddButton = false
            },
            
            _ => throw new ArgumentException($"未知的單據類型: {type}")
        };
    }
}
```

**優點**：
- 集中管理所有單據類型的顯示配置
- 易於擴展（新增單據類型只需加一個 case）
- 類型安全（使用 enum）

---

### 步驟 2：建立可重用的區塊組件

**檔案**：`Components/RelatedDocumentSectionComponent.razor`

```razor
@* 可重用的單據區塊組件 *@
@using ERPCore2.Components.Shared.BaseModal.Modals.RelatedDocument.Config

@if (Documents.Any())
{
    <div class="mb-4">
        @* 區塊標題 *@
        <h6 class="text-@Config.TextColor mb-3">
            <i class="bi bi-@Config.Icon me-2"></i>
            @Config.Title (@Documents.Count)
        </h6>
        
        @* 單據清單 *@
        <div class="list-group">
            @foreach (var doc in Documents)
            {
                <a href="javascript:void(0)" 
                   class="list-group-item list-group-item-action"
                   @onclick="@(() => OnDocumentClick.InvokeAsync(doc))">
                    <div class="d-flex w-100 justify-content-between align-items-center">
                        <div>
                            @* 單據標題 *@
                            <h6 class="mb-1">
                                <span class="badge bg-@Config.BadgeColor @Config.BadgeTextClass me-2">
                                    <i class="@doc.Icon me-1"></i>
                                    @doc.TypeDisplayName
                                </span>
                                @doc.DocumentNumber
                            </h6>
                            
                            @* 詳細資訊（由父組件通過 RenderFragment 提供） *@
                            @if (DetailsTemplate != null)
                            {
                                @DetailsTemplate(doc)
                            }
                            else
                            {
                                @* 預設顯示：日期和備註 *@
                                <p class="mb-1 text-muted small">
                                    <span class="text-nowrap">
                                        <i class="bi bi-calendar3 me-1"></i>
                                        @doc.DocumentDate.ToString("yyyy-MM-dd")
                                    </span>
                                </p>
                                @if (!string.IsNullOrEmpty(doc.Remarks))
                                {
                                    <p class="mb-0 text-muted small">
                                        <i class="bi bi-chat-left-text me-1"></i>
                                        @doc.Remarks
                                    </p>
                                }
                            }
                        </div>
                        <div>
                            <i class="bi bi-chevron-right"></i>
                        </div>
                    </div>
                </a>
            }
        </div>
    </div>
}

@code {
    /// <summary>
    /// 要顯示的單據清單
    /// </summary>
    [Parameter, EditorRequired]
    public List<RelatedDocument> Documents { get; set; } = new();
    
    /// <summary>
    /// 區塊配置
    /// </summary>
    [Parameter, EditorRequired]
    public DocumentSectionConfig Config { get; set; } = null!;
    
    /// <summary>
    /// 詳細欄位的自訂範本（可選）
    /// </summary>
    [Parameter]
    public RenderFragment<RelatedDocument>? DetailsTemplate { get; set; }
    
    /// <summary>
    /// 當點擊單據時觸發
    /// </summary>
    [Parameter]
    public EventCallback<RelatedDocument> OnDocumentClick { get; set; }
}
```

**優點**：
- 完全消除重複的 HTML 結構
- 支援自訂詳細欄位範本
- 提供預設顯示邏輯

---

### 步驟 3：建立詳細欄位範本

#### 範本 1：商品物料清單

**檔案**：`Templates/CompositionDetailsTemplate.razor`

```razor
@* 商品物料清單詳細欄位範本 *@
<p class="mb-1 text-muted small">
    <span class="text-nowrap">
        <i class="bi bi-calendar3 me-1"></i>
        @Document.DocumentDate.ToString("yyyy-MM-dd")
    </span>
</p>
@if (!string.IsNullOrEmpty(Document.Remarks))
{
    <p class="mb-0 text-muted small">
        <i class="bi bi-info-circle me-1"></i>
        @Document.Remarks
    </p>
}

@code {
    [Parameter, EditorRequired]
    public RelatedDocument Document { get; set; } = null!;
}
```

#### 範本 2：銷貨訂單

**檔案**：`Templates/SalesOrderDetailsTemplate.razor`

```razor
@* 銷貨訂單詳細欄位範本 *@
<p class="mb-1 text-muted small">
    <span class="text-nowrap">
        <i class="bi bi-calendar3 me-1"></i>
        @Document.DocumentDate.ToString("yyyy-MM-dd")
    </span>
    @if (Document.Quantity.HasValue)
    {
        <span class="ms-3 text-nowrap">
            <i class="bi bi-box-seam me-1"></i>
            訂單數量: @Document.Quantity.Value
        </span>
    }
    @if (Document.UnitPrice.HasValue)
    {
        <span class="ms-3 text-nowrap">
            <i class="bi bi-cash me-1"></i>
            單價: @Document.UnitPrice.Value.ToString("N2")
        </span>
    }
</p>
@if (!string.IsNullOrEmpty(Document.Remarks))
{
    <p class="mb-0 text-muted small">
        <i class="bi bi-chat-left-text me-1"></i>
        @Document.Remarks
    </p>
}

@code {
    [Parameter, EditorRequired]
    public RelatedDocument Document { get; set; } = null!;
}
```

#### 範本 3：入庫單

**檔案**：`Templates/ReceivingDetailsTemplate.razor`

```razor
@* 入庫單詳細欄位範本 *@
<p class="mb-1 text-muted small">
    <span class="text-nowrap">
        <i class="bi bi-calendar3 me-1"></i>
        @Document.DocumentDate.ToString("yyyy-MM-dd")
    </span>
    @if (Document.Quantity.HasValue)
    {
        <span class="ms-3 text-nowrap">
            <i class="bi bi-box-seam me-1"></i>
            入庫數量: @Document.Quantity.Value
        </span>
    }
    @if (Document.UnitPrice.HasValue)
    {
        <span class="ms-3 text-nowrap">
            <i class="bi bi-cash me-1"></i>
            單價: @Document.UnitPrice.Value.ToString("N2")
        </span>
    }
</p>
@if (!string.IsNullOrEmpty(Document.Remarks))
{
    <p class="mb-0 text-muted small">
        <i class="bi bi-chat-left-text me-1"></i>
        @Document.Remarks
    </p>
}

@code {
    [Parameter, EditorRequired]
    public RelatedDocument Document { get; set; } = null!;
}
```

#### 範本 4：退貨單

**檔案**：`Templates/ReturnDetailsTemplate.razor`

```razor
@* 退貨單詳細欄位範本 *@
<p class="mb-1 text-muted small">
    <span class="text-nowrap">
        <i class="bi bi-calendar3 me-1"></i>
        @Document.DocumentDate.ToString("yyyy-MM-dd")
    </span>
    @if (Document.Quantity.HasValue)
    {
        <span class="ms-3 text-nowrap">
            <i class="bi bi-box-seam me-1"></i>
            退貨數量: @Document.Quantity.Value
        </span>
    }
</p>
@if (!string.IsNullOrEmpty(Document.Remarks))
{
    <p class="mb-0 text-muted small">
        <i class="bi bi-chat-left-text me-1"></i>
        @Document.Remarks
    </p>
}

@code {
    [Parameter, EditorRequired]
    public RelatedDocument Document { get; set; } = null!;
}
```

#### 範本 5：沖款單

**檔案**：`Templates/SetoffDetailsTemplate.razor`

```razor
@* 沖款單詳細欄位範本 *@
<p class="mb-1 text-muted small">
    <span class="text-nowrap">
        <i class="bi bi-calendar3 me-1"></i>
        @Document.DocumentDate.ToString("yyyy-MM-dd")
    </span>
    @if (Document.Amount.HasValue)
    {
        <span class="ms-3 text-nowrap">
            <i class="bi bi-currency-dollar me-1"></i>
            使用金額: @Document.Amount.Value.ToString("N2")
        </span>
    }
    else
    {
        @if (Document.CurrentAmount.HasValue)
        {
            <span class="ms-3 text-nowrap">
                <i class="bi bi-currency-dollar me-1"></i>
                本次收款: @Document.CurrentAmount.Value.ToString("N2")
            </span>
        }
        @if (Document.TotalAmount.HasValue)
        {
            <span class="ms-3 text-nowrap">
                <i class="bi bi-cash-stack me-1"></i>
                累計收款: @Document.TotalAmount.Value.ToString("N2")
            </span>
        }
    }
</p>
@if (!string.IsNullOrEmpty(Document.Remarks))
{
    <p class="mb-0 text-muted small">
        <i class="bi bi-chat-left-text me-1"></i>
        @Document.Remarks
    </p>
}

@code {
    [Parameter, EditorRequired]
    public RelatedDocument Document { get; set; } = null!;
}
```

**優點**：
- 每種單據類型的顯示邏輯清晰分離
- 易於維護和修改
- 可獨立測試

---

### 步驟 4：重構主 Modal 組件

**檔案**：`RelatedDocumentsModalComponent.razor`（重構後）

```razor
@inject INotificationService NotificationService
@using ERPCore2.Components.Shared.BaseModal.Modals.RelatedDocument.Config
@using ERPCore2.Components.Shared.BaseModal.Modals.RelatedDocument.Components
@using ERPCore2.Components.Shared.BaseModal.Modals.RelatedDocument.Templates

<BaseModalComponent IsVisible="@IsVisible"
                   IsVisibleChanged="@IsVisibleChanged"
                   Title="@($"相關單據 - {ProductName}")"
                   Icon="bi bi-link-45deg"
                   Size="BaseModalComponent.ModalSize.Large"
                   HeaderColor="BaseModalComponent.HeaderVariant.Primary"
                   CloseOnEscape="true"
                   CloseOnBackdropClick="false"
                   IsLoading="@IsLoading"
                   LoadingMessage="正在載入相關單據..."
                   OnClose="@Close">
    
    <BodyContent>
        @if (RelatedDocuments == null || !RelatedDocuments.Any())
        {
            <div class="text-center py-5">
                <i class="bi bi-inbox display-1 text-muted"></i>
                <p class="mt-3 text-muted">暫無相關單據</p>
            </div>
        }
        else
        {
            @* 使用重構後的組件顯示各類單據 *@
            @foreach (var group in DocumentGroups)
            {
                <RelatedDocumentSectionComponent 
                    Documents="@group.Documents"
                    Config="@group.Config"
                    OnDocumentClick="@HandleDocumentClick"
                    DetailsTemplate="@GetDetailsTemplate(group.Type)" />
            }
        }
    </BodyContent>
    
    <FooterContent>
        @* 顯示所有類型的新增按鈕（根據配置） *@
        @foreach (var group in DocumentGroups.Where(g => g.Config.ShowAddButton))
        {
            <GenericButtonComponent
                Text="@group.Config.AddButtonText"
                Variant="ButtonVariant.DarkBlue"
                OnClick="@OnAddNew"
                Size="ButtonSize.Small" />
        }
        
        <GenericButtonComponent
            Text="關閉"
            Variant="ButtonVariant.Gray"
            OnClick="@Close"
            Size="ButtonSize.Small" />
    </FooterContent>
    
</BaseModalComponent>

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public string ProductName { get; set; } = string.Empty;
    [Parameter] public List<RelatedDocument>? RelatedDocuments { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback<RelatedDocument> OnDocumentClick { get; set; }
    [Parameter] public EventCallback OnAddNew { get; set; }

    /// <summary>
    /// 單據分組資訊
    /// </summary>
    private class DocumentGroup
    {
        public RelatedDocumentType Type { get; set; }
        public List<RelatedDocument> Documents { get; set; } = new();
        public DocumentSectionConfig Config { get; set; } = null!;
    }

    /// <summary>
    /// 取得分組後的單據清單
    /// </summary>
    private List<DocumentGroup> DocumentGroups
    {
        get
        {
            if (RelatedDocuments == null || !RelatedDocuments.Any())
                return new List<DocumentGroup>();

            return RelatedDocuments
                .GroupBy(d => d.DocumentType)
                .Select(g => new DocumentGroup
                {
                    Type = g.Key,
                    Documents = g.ToList(),
                    Config = DocumentSectionConfig.GetConfig(g.Key)
                })
                .OrderBy(g => g.Type) // 可根據需要調整排序
                .ToList();
        }
    }

    /// <summary>
    /// 根據單據類型取得對應的詳細欄位範本
    /// </summary>
    private RenderFragment<RelatedDocument>? GetDetailsTemplate(RelatedDocumentType type)
    {
        return type switch
        {
            RelatedDocumentType.ProductComposition => doc => 
                @<CompositionDetailsTemplate Document="@doc" />,
            
            RelatedDocumentType.SalesOrder => doc => 
                @<SalesOrderDetailsTemplate Document="@doc" />,
            
            RelatedDocumentType.ReceivingDocument => doc => 
                @<ReceivingDetailsTemplate Document="@doc" />,
            
            RelatedDocumentType.ReturnDocument => doc => 
                @<ReturnDetailsTemplate Document="@doc" />,
            
            RelatedDocumentType.SetoffDocument => doc => 
                @<SetoffDetailsTemplate Document="@doc" />,
            
            _ => null // 使用預設範本
        };
    }

    private async Task Close()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
    }

    private async Task HandleDocumentClick(RelatedDocument document)
    {
        if (OnDocumentClick.HasDelegate)
        {
            await OnDocumentClick.InvokeAsync(document);
        }
    }
}
```

**優點**：
- 編號從 ~387 行減少到 ~130 行（減少 66%）
- 邏輯清晰易懂
- 易於擴展和維護

---

## 📖 使用範例

### 範例 1：在商品編輯頁面中使用（與現有用法相同）

```razor
<!-- 相關單據查看 Modal（物料清單清單）-->
<RelatedDocumentsModalComponent 
    IsVisible="@showRelatedDocumentsModal"
    IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
    ProductName="@selectedProductName"
    RelatedDocuments="@relatedDocuments"
    IsLoading="@isLoadingRelatedDocuments"
    OnDocumentClick="@HandleRelatedDocumentClick"
    OnAddNew="@HandleAddNewComposition" />
```

**完全向下相容！** 無需修改現有調用編號。

### 範例 2：新增單據類型（例如：採購訂單）

#### 步驟 1：在 `RelatedDocumentType` enum 中新增類型

```csharp
public enum RelatedDocumentType
{
    // ... 現有類型
    PurchaseOrder = 6  // 新增
}
```

#### 步驟 2：在 `DocumentSectionConfig` 中新增配置

```csharp
public static DocumentSectionConfig GetConfig(RelatedDocumentType type)
{
    return type switch
    {
        // ... 現有配置
        
        RelatedDocumentType.PurchaseOrder => new()
        {
            Title = "採購訂單",
            Icon = "cart-plus",
            TextColor = "indigo",
            BadgeColor = "indigo",
            ShowAddButton = false
        },
        
        _ => throw new ArgumentException($"未知的單據類型: {type}")
    };
}
```

#### 步驟 3：建立詳細欄位範本

**檔案**：`Templates/PurchaseOrderDetailsTemplate.razor`

```razor
<p class="mb-1 text-muted small">
    <span class="text-nowrap">
        <i class="bi bi-calendar3 me-1"></i>
        @Document.DocumentDate.ToString("yyyy-MM-dd")
    </span>
    @if (Document.Quantity.HasValue)
    {
        <span class="ms-3 text-nowrap">
            <i class="bi bi-box-seam me-1"></i>
            採購數量: @Document.Quantity.Value
        </span>
    }
</p>

@code {
    [Parameter, EditorRequired]
    public RelatedDocument Document { get; set; } = null!;
}
```

#### 步驟 4：在主 Modal 的 `GetDetailsTemplate` 方法中新增

```csharp
private RenderFragment<RelatedDocument>? GetDetailsTemplate(RelatedDocumentType type)
{
    return type switch
    {
        // ... 現有範本
        
        RelatedDocumentType.PurchaseOrder => doc => 
            @<PurchaseOrderDetailsTemplate Document="@doc" />,
        
        _ => null
    };
}
```

**完成！** 只需 4 個步驟，無需複製大量編號。

---

## 🔄 遷移指南

### 遷移步驟

#### 階段 1：準備工作（不影響現有功能）

1. ✅ 建立 `Config/DocumentSectionConfig.cs`
2. ✅ 建立 `Components/RelatedDocumentSectionComponent.razor`
3. ✅ 建立所有 `Templates/*.razor` 範本

#### 階段 2：重構主組件（完全替換）

4. ✅ 備份原始 `RelatedDocumentsModalComponent.razor`
5. ✅ 使用新版本替換主組件
6. ✅ 測試所有單據類型的顯示

#### 階段 3：驗證與清理

7. ✅ 驗證所有調用處正常運作
8. ✅ 刪除備份檔案

### 回滾計畫

如果遇到問題，可以：

1. 從備份還原原始 `RelatedDocumentsModalComponent.razor`
2. 保留新建立的檔案（供未來使用）

### 測試清單

- [ ] 商品物料清單顯示正確
- [ ] 銷貨訂單顯示正確
- [ ] 入庫單顯示正確
- [ ] 退貨單顯示正確
- [ ] 沖款單顯示正確
- [ ] 點擊單據開啟編輯 Modal
- [ ] 「新增物料清單」按鈕正常運作
- [ ] 空白狀態顯示正確
- [ ] Loading 狀態顯示正確

---

## 📊 重構效益對比

### 程式碼量對比

| 項目 | 重構前 | 重構後 | 改善 |
|------|--------|--------|------|
| 主組件行數 | 387 行 | 130 行 | ↓ 66% |
| 重複代碼 | ~250 行 | 0 行 | ↓ 100% |
| 總檔案數 | 1 個 | 8 個 | - |
| 總編號行數 | 387 行 | ~450 行 | +16% |

**說明**：雖然總編號行數略增，但**可維護性大幅提升**。

### 維護成本對比

| 任務 | 重構前 | 重構後 |
|------|--------|--------|
| 修改某類單據的顯示邏輯 | 需找到並修改對應區塊（~50 行） | 只需修改對應範本（~30 行） |
| 新增單據類型 | 複製 ~50 行程式碼並修改 | 新增配置 + 範本（~40 行） |
| 修改通用樣式 | 需修改 5 個區塊 | 只需修改區塊組件 1 處 |
| 單元測試 | 難以測試（邏輯混在 HTML 中） | 易於測試（邏輯分離） |

---

## 🎓 設計原則

本次重構遵循以下設計原則：

### 1. **DRY（Don't Repeat Yourself）**
- 消除重複的 HTML 結構
- 集中管理配置

### 2. **單一職責原則（SRP）**
- 配置類只負責定義配置
- 區塊組件只負責渲染結構
- 範本只負責顯示詳細欄位

### 3. **開放封閉原則（OCP）**
- 對擴展開放：易於新增單據類型
- 對修改封閉：不需修改核心編號

### 4. **關注點分離（SoC）**
- 結構（Structure）：區塊組件
- 樣式（Style）：配置類
- 內容（Content）：範本

### 5. **組合優於繼承**
- 使用 RenderFragment 組合範本
- 使用配置類組合行為

---

## ✅ 驗收標準

重構完成後應滿足：

### 功能性

- ✅ 所有現有功能正常運作
- ✅ 向下相容，不影響調用方
- ✅ 支援所有單據類型

### 非功能性

- ✅ 編號重複率 < 5%
- ✅ 主組件程式碼量減少 > 50%
- ✅ 新增單據類型耗時 < 30 分鐘
- ✅ 程式碼可讀性提升
- ✅ 易於單元測試

---

## 🚀 未來擴展建議

### 短期（1-2 個月）

1. **支援更多單據類型**
   - 採購訂單
   - 生產工單
   - 庫存調整單

2. **增強互動功能**
   - 單據排序（按日期、金額等）
   - 單據篩選（按狀態、時間範圍等）
   - 批次操作

### 中期（3-6 個月）

3. **效能優化**
   - 虛擬滾動（大量單據時）
   - 延遲載入（Lazy Loading）

4. **UI/UX 改善**
   - 響應式設計優化
   - 深色模式支援
   - 列表/卡片視圖切換

### 長期（6 個月以上）

5. **進階功能**
   - 單據關聯圖視覺化
   - 單據流程追蹤
   - 導出/列印功能

---

## 📚 參考資源

### 相關文件

- `README_A單轉B單.md` - 單據轉換機制
- `README_報價單BOM組成編輯功能.md` - BOM 組成編輯
- `README_銷貨訂單BOM組成編輯功能.md` - 銷貨訂單 BOM

### Blazor 官方文件

- [RenderFragment](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/templated-components)
- [Component Parameters](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/#component-parameters)

---

## 📝 版本歷史

| 版本 | 日期 | 作者 | 說明 |
|------|------|------|------|
| 1.0 | 2025-12-04 | GitHub Copilot | 初始版本 - 重構計畫文件 |

---

## 💡 總結

本次重構將大幅提升 `RelatedDocumentsModalComponent` 的：

✅ **可維護性** - 編號集中、邏輯清晰  
✅ **可擴展性** - 易於新增單據類型  
✅ **可讀性** - 結構分明、職責單一  
✅ **可測試性** - 邏輯分離、易於測試  

**建議優先實作，效益明顯！** 🎯
