# InteractiveTableComponent 使用指南

## 📋 概述

`InteractiveTableComponent` 是一個功能強大的互動式表格組件，專門設計來替代各種 SubCollection 組件中重複的表格UI。它支援多種輸入控件類型，保持與 `GenericTableComponent` 一致的視覺風格。

## 🎯 設計目標

- **統一UI風格**：與 `GenericTableComponent` 保持視覺一致性
- **多元控件支援**：支援 Input、Number、Select、Checkbox、Button、Display、Custom 等類型
- **靈活配置**：透過 `InteractiveColumnDefinition` 輕鬆配置欄位
- **響應式設計**：自動適應不同螢幕尺寸
- **即時驗證**：內建驗證機制和錯誤提示

## 🔧 基本使用

### 1. 引入必要的命名空間

```csharp
@using ERPCore2.Components.Shared.SubCollections
@using ERPCore2.Components.Shared.Buttons
```

### 2. 基本範例

```razor
<InteractiveTableComponent TItem="ProductModel" 
                          Items="@productItems"
                          ColumnDefinitions="@GetColumnDefinitions()"
                          IsReadOnly="@IsReadOnly"
                          ShowRowNumbers="true" />
```

### 3. 欄位定義範例

```csharp
@code {
    private List<InteractiveColumnDefinition> GetColumnDefinitions()
    {
        return new List<InteractiveColumnDefinition>
        {
            // 純顯示欄位
            new() 
            { 
                Title = "產品代碼", 
                PropertyName = "Code",
                ColumnType = InteractiveColumnType.Display,
                Width = "15%"
            },
            
            // 文字輸入欄位
            new() 
            { 
                Title = "產品名稱", 
                PropertyName = "Name",
                ColumnType = InteractiveColumnType.Input,
                Width = "25%",
                IsRequired = true,
                Placeholder = "請輸入產品名稱",
                OnInputChanged = OnProductNameChanged
            },
            
            // 數字輸入欄位
            new() 
            { 
                Title = "數量", 
                PropertyName = "Quantity",
                ColumnType = InteractiveColumnType.Number,
                Width = "10%",
                MinValue = 0,
                MaxValue = 9999,
                Step = 1,
                OnInputChanged = OnQuantityChanged
            },
            
            // 下拉選單欄位
            new() 
            { 
                Title = "倉庫", 
                PropertyName = "WarehouseId",
                ColumnType = InteractiveColumnType.Select,
                Width = "15%",
                Placeholder = "請選擇倉庫",
                Options = GetWarehouseOptions(),
                OnSelectionChanged = OnWarehouseChanged
            },
            
            // 勾選框欄位
            new() 
            { 
                Title = "啟用", 
                PropertyName = "IsActive",
                ColumnType = InteractiveColumnType.Checkbox,
                Width = "8%",
                CheckedText = "啟用",
                UncheckedText = "停用",
                OnCheckboxChanged = OnActiveStatusChanged
            },
            
            // 按鈕欄位
            new() 
            { 
                Title = "操作", 
                PropertyName = "",
                ColumnType = InteractiveColumnType.Button,
                Width = "10%",
                ButtonText = "刪除",
                ButtonIcon = "fas fa-trash",
                ButtonVariant = ButtonVariant.Danger,
                ButtonSize = ButtonSize.Small,
                OnButtonClick = OnDeleteItem,
                IsButtonDisabled = item => ((ProductModel)item).IsSystemDefault
            }
        };
    }
}
```

## 📝 完整範例

```razor
@page "/interactive-table-demo"
@using ERPCore2.Components.Shared.SubCollections
@using ERPCore2.Components.Shared.Buttons

<div class="container-fluid">
    <div class="row">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h5 class="card-title">產品管理</h5>
                </div>
                <div class="card-body">
                    <InteractiveTableComponent TItem="ProductModel" 
                                              Items="@productItems"
                                              ColumnDefinitions="@GetColumnDefinitions()"
                                              IsReadOnly="@IsReadOnly"
                                              ShowRowNumbers="true"
                                              ShowTotalRow="true"
                                              TotalRowTemplate="@GetTotalRowTemplate"
                                              OnValidationFailed="@OnValidationFailed"
                                              ValidationErrors="@validationErrors" />
                </div>
                <div class="card-footer">
                    <button class="btn btn-primary" @onclick="AddNewItem">
                        <i class="fas fa-plus me-1"></i>新增產品
                    </button>
                    <button class="btn btn-success ms-2" @onclick="SaveChanges">
                        <i class="fas fa-save me-1"></i>儲存變更
                    </button>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private List<ProductModel> productItems = new();
    private bool IsReadOnly = false;
    private Dictionary<string, string> validationErrors = new();

    protected override void OnInitialized()
    {
        LoadSampleData();
    }

    private void LoadSampleData()
    {
        productItems = new List<ProductModel>
        {
            new() { Id = 1, Code = "P001", Name = "筆記型電腦", Quantity = 10, WarehouseId = 1, IsActive = true },
            new() { Id = 2, Code = "P002", Name = "滑鼠", Quantity = 50, WarehouseId = 2, IsActive = true },
            new() { Id = 3, Code = "P003", Name = "鍵盤", Quantity = 30, WarehouseId = 1, IsActive = false }
        };
    }

    private List<SelectOption> GetWarehouseOptions()
    {
        return new List<SelectOption>
        {
            new() { Value = 1, Text = "主倉庫" },
            new() { Value = 2, Text = "備用倉庫" },
            new() { Value = 3, Text = "展示倉庫" }
        };
    }

    private RenderFragment<InteractiveColumnDefinition> GetTotalRowTemplate => column => __builder =>
    {
        if (column.PropertyName == "Quantity")
        {
            <strong class="text-success">總計: @productItems.Sum(x => x.Quantity)</strong>
        }
    };

    // 事件處理方法
    private async Task OnProductNameChanged((object item, string? value) args)
    {
        var product = (ProductModel)args.item;
        // 處理產品名稱變更邏輯
        Console.WriteLine($"產品 {product.Code} 名稱更改為: {args.value}");
    }

    private async Task OnQuantityChanged((object item, string? value) args)
    {
        var product = (ProductModel)args.item;
        // 處理數量變更邏輯
        if (int.TryParse(args.value, out var quantity))
        {
            Console.WriteLine($"產品 {product.Code} 數量更改為: {quantity}");
        }
    }

    private async Task OnWarehouseChanged((object item, object? value) args)
    {
        var product = (ProductModel)args.item;
        // 處理倉庫變更邏輯
        Console.WriteLine($"產品 {product.Code} 倉庫更改為: {args.value}");
    }

    private async Task OnActiveStatusChanged((object item, bool isChecked) args)
    {
        var product = (ProductModel)args.item;
        // 處理啟用狀態變更邏輯
        Console.WriteLine($"產品 {product.Code} 啟用狀態: {isChecked}");
    }

    private async Task OnDeleteItem(object item)
    {
        var product = (ProductModel)item;
        productItems.Remove(product);
        StateHasChanged();
    }

    private async Task OnValidationFailed((ProductModel item, string propertyName, string? errorMessage) args)
    {
        var key = $"{args.item.GetHashCode()}_{args.propertyName}";
        validationErrors[key] = args.errorMessage ?? "";
        StateHasChanged();
    }

    private void AddNewItem()
    {
        var newProduct = new ProductModel 
        { 
            Id = productItems.Count + 1,
            Code = $"P{(productItems.Count + 1):D3}",
            Name = "",
            Quantity = 0,
            WarehouseId = 0,
            IsActive = true
        };
        productItems.Add(newProduct);
        StateHasChanged();
    }

    private async Task SaveChanges()
    {
        // 儲存邏輯
        Console.WriteLine("儲存所有變更");
    }
}

// 資料模型
public class ProductModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int WarehouseId { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemDefault { get; set; } = false;
}
```

## 🔧 進階功能

### 自訂模板欄位

```csharp
new InteractiveColumnDefinition
{
    Title = "狀態",
    PropertyName = "Status",
    ColumnType = InteractiveColumnType.Custom,
    CustomTemplate = item =>
    {
        var product = (ProductModel)item;
        return @<span class="badge @(product.IsActive ? "bg-success" : "bg-danger")">
                   @(product.IsActive ? "啟用" : "停用")
               </span>;
    }
}
```

### 複雜驗證規則

```csharp
new InteractiveColumnDefinition
{
    Title = "電子郵件",
    PropertyName = "Email",
    ColumnType = InteractiveColumnType.Input,
    ValidationPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
    IsRequired = true
}
```

## 📱 響應式設計

```csharp
new InteractiveColumnDefinition
{
    Title = "備註",
    PropertyName = "Remarks",
    ColumnType = InteractiveColumnType.Input,
    HideOnMobile = true  // 在手機版隱藏此欄位
}
```

## 🎨 樣式客製化

組件使用 `InteractiveTableComponent.razor.css` 進行樣式定義，您可以透過 CSS 變數和類別覆寫來客製化外觀。

## 📋 參數完整列表

### 主要參數
- `Items`: 資料項目集合
- `ColumnDefinitions`: 欄位定義列表
- `IsReadOnly`: 是否為唯讀模式
- `ShowHeader`: 是否顯示表頭
- `ShowRowNumbers`: 是否顯示行號
- `ShowActions`: 是否顯示操作欄
- `ShowTotalRow`: 是否顯示總計行

### 樣式參數
- `IsStriped`: 是否使用條紋樣式
- `IsHoverable`: 是否支援懸停效果
- `IsBordered`: 是否顯示邊框
- `CssClass`: 額外的 CSS 類別

### 事件參數
- `OnRowClick`: 行點擊事件
- `OnValidationFailed`: 驗證失敗事件
- `ValidationErrors`: 驗證錯誤字典

## ✅ 最佳實踐

1. **欄位寬度**：建議使用百分比設定欄位寬度以確保響應式效果
2. **驗證處理**：利用 `OnValidationFailed` 事件處理驗證錯誤並更新 UI
3. **效能考量**：對於大量資料，考慮實作虛擬化或分頁
4. **可訪問性**：為按鈕和控件設定適當的 `Title` 屬性
5. **行動裝置**：合理使用 `HideOnMobile` 屬性優化小螢幕體驗

## 🔄 遷移指南

將現有的 SubCollection 組件遷移到 `InteractiveTableComponent`：

1. 分析現有表格的欄位類型
2. 建立對應的 `InteractiveColumnDefinition` 列表
3. 將事件處理邏輯遷移到新的事件模型
4. 測試並調整樣式和行為
5. 移除舊的表格實作程式碼

這個組件將大幅簡化您的程式碼維護，並提供一致的使用者體驗。
