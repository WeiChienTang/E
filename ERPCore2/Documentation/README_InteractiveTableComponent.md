# Inte## 🔧 設計目標

- **統一UI風格**：與 `GenericTableComponent` 保持視覺一致性
- **多元控件支援**：支援 Input、Number、Select、Checkbox、Button、Display、Custom、**SearchableSelect** 等類型
- **內建操作功能**：提供標準的刪除按鈕和自訂操作範本
- **靈活配置**：透過 `InteractiveColumnDefinition` 輕鬆配置欄位
- **響應式設計**：自動適應不同螢幕尺寸
- **即時驗證**：內建驗證機制和錯誤提示
- **🆕 自動空行功能**：提供類似 Excel 的智能空行管理，提升使用者體驗
- **🔍 智能搜尋選擇**：統一的 SearchableSelect 功能，解決重複實作 input + dropdown 的問題bleComponent 使用指南

## 📋 概述

`InteractiveTableComponent` 是一個功能強大的互動式表格組件，專門設計來替代各種 SubCollection 組件中重複的表格UI。它支援多種輸入控件類型，保持與 `GenericTableComponent` 一致的視覺風格，並配備了先進的自動空行功能。

## 🎯 設計目標

- **統一UI風格**：與 `GenericTableComponent` 保持視覺一致性
- **多元控件支援**：支援 Input、Number、Select、Checkbox、Button、Display、Custom 等類型
- **內建操作功能**：提供標準的刪除按鈕和自訂操作範本
- **靈活配置**：透過 `InteractiveColumnDefinition` 輕鬆配置欄位
- **響應式設計**：自動適應不同螢幕尺寸
- **即時驗證**：內建驗證機制和錯誤提示
- **🆕 自動空行功能**：提供類似 Excel 的智能空行管理，提升使用者體驗

## 🚀 自動空行功能 (AutoEmptyRow Feature)

### 功能特色
- **智能偵測**：自動判斷何時需要新增空行供使用者輸入
- **類似 Excel 體驗**：使用者無需點擊「新增」按鈕，直接在空行開始輸入
- **業務邏輯驅動**：根據不同組件的核心欄位智能判斷空行狀態
- **效能優化**：避免不必要的重複新增，確保始終只有一行空行

### 已整合自動空行功能的組件
以下組件已成功整合自動空行功能，提供流暢的資料輸入體驗：

#### ✅ ProductSupplierManagerComponent
- **核心判定欄位**：廠商選擇 (SupplierId)
- **業務邏輯**：只要選擇了廠商，即視為有效資料行
- **使用場景**：產品廠商資訊管理
- **整合狀態**：已完成測試，運行穩定

### 如何為組件添加自動空行功能

如果您的組件使用 `InteractiveTableComponent` 並希望添加自動空行功能，請參考以下步驟：

#### 1. 引入 AutoEmptyRowHelper
```csharp
@using ERPCore2.Helpers
```

#### 2. 實作必要方法
```csharp
/// <summary>
/// 定義空行判定邏輯 - 根據業務需求自訂
/// </summary>
private bool IsEmptyRow(TEntity item)
{
    // 以核心欄位為判定基準
    var coreFieldValue = GetCoreField(item);
    return !coreFieldValue.HasValue || coreFieldValue.Value <= 0;
}

/// <summary>
/// 創建新的空項目
/// </summary>
private TEntity CreateEmptyItem()
{
    var newItem = new TEntity();
    SetParentId(newItem, ParentEntityId); // 設定必要的關聯
    return newItem;
}

/// <summary>
/// 確保有空行的便利方法
/// </summary>
private void EnsureOneEmptyRow()
{
    AutoEmptyRowHelper.For<TEntity>.EnsureOneEmptyRow(
        Items, IsEmptyRow, CreateEmptyItem, SetParentId, ParentEntityId);
}
```

#### 3. 整合到組件生命週期
```csharp
protected override void OnParametersSet()
{
    base.OnParametersSet();
    EnsureOneEmptyRow(); // 確保始終有空行
}
```

#### 4. 在輸入事件中觸發檢查
```csharp
private async Task OnCoreFieldChanged((object item, object? value) args)
{
    var entity = (TEntity)args.item;
    var wasEmpty = IsEmptyRow(entity); // 記錄變更前狀態
    
    // 執行業務邏輯
    SetCoreField(entity, value);
    
    // 智能處理空行
    AutoEmptyRowHelper.For<TEntity>.HandleInputChangeAdvanced(
        Items, entity, IsEmptyRow, CreateEmptyItem, wasEmpty, SetParentId, ParentEntityId);
    
    await ItemsChanged.InvokeAsync(Items);
    StateHasChanged();
}
```

### 詳細技術文件
有關自動空行功能的完整技術說明、實作細節和最佳實踐，請參閱：
📖 [自動空行功能詳細說明](README_AutoEmptyRow_Feature.md)

## 🔍 SearchableSelect 功能

### 功能概述
`SearchableSelect` 是專為解決重複實作 input + dropdown 功能而設計的統一解決方案。它提供了智能搜尋、即時過濾和鍵盤操作等功能，讓開發者無需每次都重新撰寫相同的邏輯。

### 核心特色
- **🔍 即時搜尋**：支援輸入文字即時過濾選項
- **⌨️ 鍵盤操作**：上下箭頭選擇、Enter 確認、Esc 取消
- **🎯 智能過濾**：可自訂過濾邏輯，支援模糊搜尋
- **📱 響應式設計**：自動調整下拉選單位置和尺寸
- **🔧 高度可配置**：支援自訂顯示格式、最大顯示項目數等
- **🚀 效能優化**：只顯示前 N 項結果，避免大量資料造成效能問題

### 使用 SearchableSelectHelper 快速設定

#### 1. 商品搜尋選擇欄位
```csharp
@using ERPCore2.Helpers

private List<InteractiveColumnDefinition> GetColumnDefinitions()
{
    var columns = new List<InteractiveColumnDefinition>();
    
    // 使用 SearchableSelectHelper 快速建立商品搜尋欄位
    var productColumn = SearchableSelectHelper.CreateProductSearchableSelect<ProductItem, Product>(
        title: "商品",
        availableProductsProvider: () => AvailableProducts,
        onSearchInputChanged: EventCallback.Factory.Create<(ProductItem, string?)>(this, OnProductSearchInput),
        onProductSelected: EventCallback.Factory.Create<(ProductItem, Product?)>(this, OnProductSelected),
        onInputFocus: EventCallback.Factory.Create<ProductItem>(this, OnProductInputFocus),
        onInputBlur: EventCallback.Factory.Create<ProductItem>(this, OnProductInputBlur),
        onItemMouseEnter: EventCallback.Factory.Create<(ProductItem, int)>(this, OnProductItemMouseEnter),
        isReadOnly: IsReadOnly
    );
    productColumn.Width = "25%";
    columns.Add(productColumn);
    
    return columns;
}

// 事件處理方法
private async Task OnProductSearchInput((ProductItem item, string? searchValue) args)
{
    var wasEmpty = IsEmptyRow(args.item);
    
    // 更新搜尋值和過濾結果
    args.item.ProductSearch = args.searchValue ?? string.Empty;
    args.item.FilteredProducts = FilterProducts(args.searchValue);
    args.item.ShowDropdown = args.item.FilteredProducts.Any();
    
    // 處理自動空行邏輯
    AutoEmptyRowHelper.ForAny<ProductItem>.HandleInputChange(
        ProductItems, args.item, IsEmptyRow, CreateEmptyItem, wasEmpty, !IsEmptyRow(args.item));
    
    await NotifyDetailsChanged();
    StateHasChanged();
}

private async Task OnProductSelected((ProductItem item, Product? product) args)
{
    var wasEmpty = IsEmptyRow(args.item);
    
    // 更新選擇的商品
    args.item.SelectedProduct = args.product;
    args.item.ProductSearch = args.product != null ? $"{args.product.Code} - {args.product.Name}" : string.Empty;
    args.item.ShowDropdown = false;
    
    // 處理自動空行邏輯
    AutoEmptyRowHelper.ForAny<ProductItem>.HandleInputChange(
        ProductItems, args.item, IsEmptyRow, CreateEmptyItem, wasEmpty, !IsEmptyRow(args.item));
    
    await NotifyDetailsChanged();
    StateHasChanged();
}

private List<Product> FilterProducts(string? searchValue)
{
    if (string.IsNullOrWhiteSpace(searchValue))
        return AvailableProducts.Take(20).ToList();
        
    return AvailableProducts
        .Where(p => p.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true ||
                   p.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true)
        .Take(20)
        .ToList();
}
```

#### 2. 自訂 SearchableSelect 欄位
```csharp
// 使用通用的 SearchableSelectHelper.CreateSearchableSelect 方法
var customColumn = SearchableSelectHelper.CreateSearchableSelect<CustomerItem, Customer>(
    title: "客戶",
    width: "30%",
    searchValuePropertyName: "CustomerSearch",
    selectedItemPropertyName: "SelectedCustomer", 
    filteredItemsPropertyName: "FilteredCustomers",
    showDropdownPropertyName: "ShowDropdown",
    selectedIndexPropertyName: "SelectedIndex",
    availableItemsProvider: () => AvailableCustomers,
    itemDisplayFormatter: customer => $"{customer.Code} - {customer.Name}",
    searchFilter: (customer, searchValue) => 
        customer.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true ||
        customer.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true,
    onSearchInputChanged: EventCallback.Factory.Create<(CustomerItem, string?)>(this, OnCustomerSearchInput),
    onItemSelected: EventCallback.Factory.Create<(CustomerItem, Customer?)>(this, OnCustomerSelected),
    onInputFocus: EventCallback.Factory.Create<CustomerItem>(this, OnCustomerInputFocus),
    onInputBlur: EventCallback.Factory.Create<CustomerItem>(this, OnCustomerInputBlur),
    onItemMouseEnter: EventCallback.Factory.Create<(CustomerItem, int)>(this, OnCustomerItemMouseEnter),
    placeholder: "輸入客戶代碼或名稱...",
    maxDisplayItems: 15,
    isReadOnly: IsReadOnly
);
columns.Add(customColumn);
```

### 直接使用 InteractiveColumnType.SearchableSelect

如果您需要更細緻的控制，也可以直接使用 `SearchableSelect` 欄位類型：

```csharp
new InteractiveColumnDefinition
{
    Title = "供應商",
    PropertyName = "SelectedSupplier",
    ColumnType = InteractiveColumnType.SearchableSelect,
    Width = "25%",
    
    // SearchableSelect 專用屬性
    SearchValuePropertyName = "SupplierSearch",
    SelectedItemPropertyName = "SelectedSupplier",
    FilteredItemsPropertyName = "FilteredSuppliers", 
    ShowDropdownPropertyName = "ShowDropdown",
    SelectedIndexPropertyName = "SelectedIndex",
    
    // 資料提供者和格式化
    AvailableItemsProvider = () => AvailableSuppliers.Cast<object>().ToList(),
    ItemDisplayFormatter = item => 
    {
        var supplier = (Supplier)item;
        return $"{supplier.Code} - {supplier.Name}";
    },
    
    // 搜尋過濾邏輯
    SearchFilter = (item, searchValue) =>
    {
        var supplier = (Supplier)item;
        return supplier.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true ||
               supplier.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true;
    },
    
    // 事件處理
    OnSearchInputChanged = EventCallback.Factory.Create<(object, string?)>(this, OnSupplierSearchInput),
    OnItemSelected = EventCallback.Factory.Create<(object, object?)>(this, OnSupplierSelected),
    OnInputFocus = EventCallback.Factory.Create<object>(this, OnSupplierInputFocus),
    OnInputBlur = EventCallback.Factory.Create<object>(this, OnSupplierInputBlur),
    OnItemMouseEnter = EventCallback.Factory.Create<(object, int)>(this, OnSupplierItemMouseEnter),
    
    // 顯示設定
    Placeholder = "輸入供應商代碼或名稱...",
    MaxDisplayItems = 20,
    IsRequired = true
}
```

### 資料模型要求

使用 SearchableSelect 功能時，您的資料模型需要包含以下屬性：

```csharp
public class ProductItem
{
    // 搜尋相關屬性
    public string ProductSearch { get; set; } = string.Empty;
    public Product? SelectedProduct { get; set; }
    public List<Product> FilteredProducts { get; set; } = new List<Product>();
    public bool ShowDropdown { get; set; } = false;
    public int SelectedIndex { get; set; } = -1;
    
    // 其他業務屬性...
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    // ...
}
```

### 最佳實踐

1. **效能優化**：使用 `MaxDisplayItems` 限制顯示的選項數量
2. **使用者體驗**：提供有意義的 `Placeholder` 文字
3. **搜尋邏輯**：實作合理的模糊搜尋邏輯，支援代碼和名稱搜尋
4. **鍵盤支援**：確保所有事件處理器都正確實作
5. **自動空行整合**：與 AutoEmptyRowHelper 結合使用以提供最佳的輸入體驗

### 遷移指南

從自訂的 input + dropdown 實作遷移到 SearchableSelect：

**舊版寫法**：
```csharp
// 需要手動實作大量的事件處理和UI邏輯
CustomTemplate = item => 
{
    // 100+ 行的自訂下拉選單實作...
};
```

**新版寫法**：
```csharp
// 使用 SearchableSelectHelper，只需幾行程式碼
var column = SearchableSelectHelper.CreateProductSearchableSelect<ProductItem, Product>(
    title: "商品",
    availableProductsProvider: () => AvailableProducts,
    onSearchInputChanged: EventCallback.Factory.Create<(ProductItem, string?)>(this, OnProductSearchInput),
    onProductSelected: EventCallback.Factory.Create<(ProductItem, Product?)>(this, OnProductSelected),
    // ... 其他事件處理器
);
```

這大幅簡化了程式碼，提高了一致性和可維護性。

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

### 2.1 使用內建操作功能

```razor
<InteractiveTableComponent TItem="ProductModel" 
                          Items="@productItems"
                          ColumnDefinitions="@GetColumnDefinitions()"
                          IsReadOnly="@IsReadOnly"
                          ShowRowNumbers="true"
                          ShowBuiltInActions="true"
                          OnItemDelete="@HandleDelete" />
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
            
            // 🆕 智能搜尋選擇欄位 (使用 SearchableSelectHelper)
            SearchableSelectHelper.CreateProductSearchableSelect<ProductModel, Product>(
                title: "相關產品",
                availableProductsProvider: () => AvailableProducts,
                onSearchInputChanged: EventCallback.Factory.Create<(ProductModel, string?)>(this, OnRelatedProductSearchInput),
                onProductSelected: EventCallback.Factory.Create<(ProductModel, Product?)>(this, OnRelatedProductSelected),
                onInputFocus: EventCallback.Factory.Create<ProductModel>(this, OnRelatedProductInputFocus),
                onInputBlur: EventCallback.Factory.Create<ProductModel>(this, OnRelatedProductInputBlur),
                isReadOnly: IsReadOnly
            )
            
            // 注意：操作按鈕現在可以使用內建功能，不需要手動定義
            // 只需設定 ShowBuiltInActions="true" 和 OnItemDelete 事件即可
        };
    }
}
```

## 📝 完整範例

```razor
@page "/interactive-table-demo"
@using ERPCore2.Components.Shared.SubCollections
@using ERPCore2.Components.Shared.Buttons
@inject IJSRuntime JSRuntime

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
                                              ValidationErrors="@validationErrors"
                                              ShowBuiltInActions="true"
                                              OnItemDelete="@OnDeleteItem"
                                              IsDeleteDisabled="@(item => item.IsSystemDefault)" />
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

    // 🆕 SearchableSelect 事件處理方法
    private async Task OnRelatedProductSearchInput((ProductModel item, string? searchValue) args)
    {
        // 更新搜尋值和過濾結果
        args.item.RelatedProductSearch = args.searchValue ?? string.Empty;
        args.item.FilteredRelatedProducts = FilterRelatedProducts(args.searchValue);
        args.item.ShowRelatedProductDropdown = args.item.FilteredRelatedProducts.Any();
        StateHasChanged();
    }

    private async Task OnRelatedProductSelected((ProductModel item, Product? product) args)
    {
        // 更新選擇的相關產品
        args.item.SelectedRelatedProduct = args.product;
        args.item.RelatedProductSearch = args.product != null ? $"{args.product.Code} - {args.product.Name}" : string.Empty;
        args.item.ShowRelatedProductDropdown = false;
        StateHasChanged();
    }

    private async Task OnRelatedProductInputFocus(ProductModel item)
    {
        // 聚焦時顯示下拉選單
        if (!string.IsNullOrWhiteSpace(item.RelatedProductSearch))
        {
            item.FilteredRelatedProducts = FilterRelatedProducts(item.RelatedProductSearch);
        }
        else
        {
            item.FilteredRelatedProducts = AvailableProducts.Take(20).ToList();
        }
        item.ShowRelatedProductDropdown = item.FilteredRelatedProducts.Any();
        StateHasChanged();
    }

    private async Task OnRelatedProductInputBlur(ProductModel item)
    {
        // 延遲關閉下拉選單，讓用戶有時間點擊選項
        await Task.Delay(100);
        item.ShowRelatedProductDropdown = false;
        StateHasChanged();
    }

    private List<Product> FilterRelatedProducts(string? searchValue)
    {
        if (string.IsNullOrWhiteSpace(searchValue))
            return AvailableProducts.Take(20).ToList();
            
        return AvailableProducts
            .Where(p => p.Code?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true ||
                       p.Name?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) == true)
            .Take(20)
            .ToList();
    }

    private async Task OnDeleteItem(ProductModel item)
    {
        // 可以添加確認對話框
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"確定要刪除產品 {item.Name} 嗎？");
        if (!confirmed) return;
        
        productItems.Remove(item);
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
    
    // 🆕 SearchableSelect 相關屬性
    public string RelatedProductSearch { get; set; } = string.Empty;
    public Product? SelectedRelatedProduct { get; set; }
    public List<Product> FilteredRelatedProducts { get; set; } = new List<Product>();
    public bool ShowRelatedProductDropdown { get; set; } = false;
    public int SelectedRelatedProductIndex { get; set; } = -1;
}
```

## 🔧 進階功能

### 內建操作功能

#### 基本刪除按鈕
```razor
<InteractiveTableComponent ShowBuiltInActions="true" 
                          OnItemDelete="@HandleDelete" />
```

#### 自訂刪除按鈕樣式
```razor
<InteractiveTableComponent ShowBuiltInActions="true" 
                          DeleteButtonIcon="bi bi-x-circle"
                          DeleteButtonVariant="ButtonVariant.OutlineDanger"
                          DeleteButtonTitle="移除項目"
                          OnItemDelete="@HandleDelete" />
```

#### 條件禁用刪除
```razor
<InteractiveTableComponent ShowBuiltInActions="true" 
                          IsDeleteDisabled="@(item => item.IsSystemData)"
                          OnItemDelete="@HandleDelete" />
```

#### 結合自訂操作
```razor
<InteractiveTableComponent ShowBuiltInActions="true" 
                          OnItemDelete="@HandleDelete"
                          CustomActionsTemplate="@GetCustomActions" />

@code {
    private RenderFragment<ProductModel> GetCustomActions => item => __builder =>
    {
        <GenericButtonComponent IconClass="bi bi-pencil"
                               Variant="ButtonVariant.OutlinePrimary"
                               Size="ButtonSize.Small"
                               Title="編輯"
                               OnClick="() => NavigateToEdit(item)" />
    };
}
```

### 與現有 ActionsTemplate 的相容性

新的內建操作功能與現有的 `ActionsTemplate` 完全相容：

1. **可以同時使用**：`ShowActions="true"` 和 `ShowBuiltInActions="true"` 可以並存
2. **向後相容**：現有使用 `ActionsTemplate` 的代碼無需修改
3. **優先級**：如果同時設定，`ActionsTemplate` 會顯示在內建操作之前

#### 只要自訂操作（不要刪除按鈕）
```razor
<InteractiveTableComponent ShowBuiltInActions="true" 
                          ShowBuiltInDeleteButton="false"
                          CustomActionsTemplate="@GetCustomActions" />
```

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
- `ShowActions`: 是否顯示操作欄（舊版方式）
- `ShowTotalRow`: 是否顯示總計行

### 內建操作參數
- `ShowBuiltInActions`: 是否顯示內建操作欄
- `ShowBuiltInDeleteButton`: 是否顯示內建刪除按鈕
- `DeleteButtonIcon`: 刪除按鈕圖示（預設："bi bi-trash text-white"）
- `DeleteButtonVariant`: 刪除按鈕樣式（預設：Danger）
- `DeleteButtonSize`: 刪除按鈕尺寸（預設：Normal）
- `DeleteButtonTitle`: 刪除按鈕提示文字（預設："刪除"）
- `IsDeleteDisabled`: 判斷項目是否禁用刪除的函數
- `OnItemDelete`: 項目刪除事件
- `CustomActionsTemplate`: 自訂額外操作按鈕範本

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
6. **操作按鈕**：優先使用內建操作功能(`ShowBuiltInActions`)而非手動定義Button欄位
7. **刪除確認**：在 `OnItemDelete` 事件中實作確認對話框提升使用者體驗
8. **🆕 SearchableSelect 最佳實踐**：
   - 使用 `SearchableSelectHelper` 減少重複程式碼
   - 設定合理的 `MaxDisplayItems` 避免效能問題
   - 結合 `AutoEmptyRowHelper` 提供最佳輸入體驗
   - 實作有意義的搜尋過濾邏輯
   - 提供清楚的 Placeholder 文字指引使用者

## 🔄 遷移指南

### 將現有的 SubCollection 組件遷移到 `InteractiveTableComponent`：

1. **分析現有表格的欄位類型**
2. **建立對應的 `InteractiveColumnDefinition` 列表**
3. **將事件處理邏輯遷移到新的事件模型**
4. **使用內建操作功能取代手動的操作按鈕**
5. **測試並調整樣式和行為**
6. **移除舊的表格實作程式碼**

### 從手動操作按鈕遷移到內建操作：

**舊版寫法**：
```csharp
new InteractiveColumnDefinition
{
    Title = "操作",
    ColumnType = InteractiveColumnType.Button,
    ButtonIcon = "bi bi-trash",
    ButtonVariant = ButtonVariant.Danger,
    OnButtonClick = EventCallback.Factory.Create<object>(this, HandleDelete)
}
```

**新版寫法**：
```razor
<InteractiveTableComponent ShowBuiltInActions="true"
                          OnItemDelete="@HandleDelete" />
```

### 從自訂 input + dropdown 遷移到 SearchableSelect：

**舊版寫法**：
```csharp
// 需要手動實作 100+ 行的自訂 CustomTemplate
new InteractiveColumnDefinition
{
    Title = "商品",
    ColumnType = InteractiveColumnType.Custom,
    CustomTemplate = item => 
    {
        // 大量的手動 UI 和事件處理邏輯...
        return @<div class="position-relative">
            <input type="text" @oninput="..." @onfocus="..." @onblur="..." />
            @if (showDropdown) {
                <div class="dropdown-menu">
                    // 手動下拉選單實作...
                </div>
            }
        </div>;
    }
}
```

**新版寫法**：
```csharp
// 使用 SearchableSelectHelper，大幅簡化程式碼
var productColumn = SearchableSelectHelper.CreateProductSearchableSelect<ProductItem, Product>(
    title: "商品",
    availableProductsProvider: () => AvailableProducts,
    onSearchInputChanged: EventCallback.Factory.Create<(ProductItem, string?)>(this, OnProductSearchInput),
    onProductSelected: EventCallback.Factory.Create<(ProductItem, Product?)>(this, OnProductSelected),
    // ... 其他事件處理器
);
```
4. 測試並調整樣式和行為
5. 移除舊的表格實作程式碼

這個組件將大幅簡化您的程式碼維護，並提供一致的使用者體驗。

## 🎉 內建操作功能優勢

- **🔧 簡化開發**：不需要為每個表格手動定義操作按鈕
- **🎨 統一樣式**：所有刪除按鈕使用一致的視覺設計
- **⚡ 提升效率**：只需設定 `ShowBuiltInActions="true"` 即可獲得標準操作功能
- **🔧 高度可自訂**：支援自訂按鈕樣式、禁用條件和額外操作
- **🔄 完全相容**：與現有的 `ActionsTemplate` 完全相容，可平滑遷移

## 🔍 SearchableSelect 功能優勢

- **🚀 開發效率**：使用 `SearchableSelectHelper` 大幅減少重複程式碼
- **🎯 統一體驗**：所有 input + dropdown 功能使用一致的 UI 和互動邏輯
- **⌨️ 完整支援**：內建鍵盤操作、搜尋過濾、位置調整等功能
- **📱 響應式設計**：自動適應不同螢幕尺寸和裝置
- **🔧 高度靈活**：支援自訂顯示格式、過濾邏輯和事件處理
- **⚡ 效能優化**：智能限制顯示項目數量，確保大量資料下的流暢體驗

## 📚 實際應用範例

以下是在 `PurchaseOrderProductManagerComponent` 中使用新的 SearchableSelect 功能的實際範例：

```csharp
// 原本需要 100+ 行的自訂實作，現在只需要幾行程式碼
var productColumn = SearchableSelectHelper.CreateProductSearchableSelect<ProductItem, Product>(
    title: "商品",
    availableProductsProvider: () => AvailableProducts,
    onSearchInputChanged: EventCallback.Factory.Create<(ProductItem, string?)>(this, OnProductSearchInput),
    onProductSelected: EventCallback.Factory.Create<(ProductItem, Product?)>(this, OnProductSelected),
    onInputFocus: EventCallback.Factory.Create<ProductItem>(this, OnProductInputFocus),
    onInputBlur: EventCallback.Factory.Create<ProductItem>(this, OnProductInputBlur),
    onItemMouseEnter: EventCallback.Factory.Create<(ProductItem, int)>(this, OnProductItemMouseEnter),
    isReadOnly: IsReadOnly
);
productColumn.Width = "25%";
```

這種統一的方式不僅減少了程式碼重複，還確保了所有類似功能的一致性和可維護性。
