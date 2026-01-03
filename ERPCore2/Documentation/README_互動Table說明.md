# InteractiveTableComponent 互動式表格元件使用說明

## 📋 目錄
- [概述](#概述)
- [主要功能](#主要功能)
- [元件參數](#元件參數)
- [欄位類型說明](#欄位類型說明)
- [基本使用範例](#基本使用範例)
- [進階功能](#進階功能)
- [完整使用案例](#完整使用案例)
- [注意事項](#注意事項)

---

## 概述

`InteractiveTableComponent` 是一個功能強大且高度可客製化的 Blazor 互動式表格元件，支援多種輸入控件類型、資料驗證、行選取、鍵盤導航等功能，並提供統一的 UI 風格。

**檔案位置**: `Components/Shared/BaseTable/InteractiveTableComponent.razor`

**命名空間**: `ERPCore2.Components.Shared.SubCollections`

---

## 主要功能

### ✨ 核心特性

1. **多種欄位類型支援**
   - 純顯示文字 (Display)
   - 文字輸入框 (Input)
   - 數字輸入框 (Number)
   - 下拉選單 (Select)
   - 可搜尋下拉選單 (SearchableSelect)
   - 勾選框 (Checkbox)
   - 按鈕 (Button)
   - 自訂模板 (Custom)

2. **互動功能**
   - 行點擊事件
   - 行選取功能（單選/多選）
   - 鍵盤導航（方向鍵、Enter、Escape）
   - 即時資料驗證

3. **UI/UX 特性**
   - 響應式設計（支援手機版隱藏欄位）
   - 自動空白行管理
   - 總計列顯示
   - 行號顯示
   - 自訂 CSS 樣式
   - 內建操作按鈕（刪除等）

4. **資料處理**
   - 泛型支援 (TItem)
   - 支援巢狀屬性存取（例如: `Customer.Name`）
   - 自動型別轉換
   - 驗證規則與錯誤訊息

---

## 元件參數

### 🔧 基本參數

| 參數名稱 | 類型 | 預設值 | 說明 |
|---------|------|--------|------|
| `Items` | `IEnumerable<TItem>?` | - | 要顯示的資料項目列表 |
| `ColumnDefinitions` | `List<InteractiveColumnDefinition>?` | - | 欄位定義列表 |
| `ActionsTemplate` | `RenderFragment<TItem>?` | - | 自訂操作欄模板 |
| `EmptyTemplate` | `RenderFragment?` | - | 無資料時顯示的自訂模板 |
| `TotalRowTemplate` | `RenderFragment<InteractiveColumnDefinition>?` | - | 總計列自訂模板 |

### 🎨 表格樣式設定

| 參數名稱 | 類型 | 預設值 | 說明 |
|---------|------|--------|------|
| `ShowHeader` | `bool` | `true` | 是否顯示表頭 |
| `ShowActions` | `bool` | `false` | 是否顯示自訂操作欄 |
| `ShowRowNumbers` | `bool` | `false` | 是否顯示行號 |
| `ShowTotalRow` | `bool` | `false` | 是否顯示總計列 |
| `IsStriped` | `bool` | `true` | 是否使用條紋樣式 |
| `IsHoverable` | `bool` | `true` | 是否啟用滑鼠懸停效果 |
| `IsBordered` | `bool` | `true` | 是否顯示邊框 |
| `IsReadOnly` | `bool` | `false` | 是否為唯讀模式 |
| `CssClass` | `string` | `""` | 額外的 CSS 類別 |
| `EmptyMessage` | `string` | `"沒有找到資料"` | 無資料時顯示的訊息 |
| `ActionsHeader` | `string` | `"操作"` | 操作欄標題 |
| `ActionsColumnWidth` | `string` | `"auto"` | 操作欄寬度 (如 "120px", "10%", "auto") |

### 🖱️ 行互動設定

| 參數名稱 | 類型 | 預設值 | 說明 |
|---------|------|--------|------|
| `GetRowCssClass` | `Func<TItem, int, string>?` | - | 自訂行的 CSS 類別函數 |
| `OnRowClick` | `EventCallback<TItem>` | - | 行點擊事件回呼 |
| `EnableRowClick` | `bool` | `false` | 是否啟用行點擊 |
| `RowClickCursor` | `string` | `"pointer"` | 行點擊時的游標樣式 |

### ✅ 行選取功能設定

| 參數名稱 | 類型 | 預設值 | 說明 |
|---------|------|--------|------|
| `EnableRowSelection` | `bool` | `false` | 是否啟用行選取功能 |
| `AllowMultipleSelection` | `bool` | `false` | 是否允許多選 |
| `SelectedItems` | `HashSet<TItem>?` | - | 已選取的項目集合 |
| `OnSelectionChanged` | `EventCallback<HashSet<TItem>>` | - | 選取狀態變更事件 |

### ⚠️ 驗證相關

| 參數名稱 | 類型 | 預設值 | 說明 |
|---------|------|--------|------|
| `OnValidationFailed` | `EventCallback<(TItem item, string propertyName, string? errorMessage)>` | - | 驗證失敗事件 |
| `ValidationErrors` | `Dictionary<string, string>?` | - | 驗證錯誤字典 |

### 🔘 內建操作按鈕設定

| 參數名稱 | 類型 | 預設值 | 說明 |
|---------|------|--------|------|
| `ShowBuiltInActions` | `bool` | `false` | 是否顯示內建操作欄 |
| `ShowBuiltInDeleteButton` | `bool` | `true` | 是否顯示內建刪除按鈕 |
| `DeleteButtonIcon` | `string` | `"bi bi-trash text-white"` | 刪除按鈕圖示 |
| `DeleteButtonVariant` | `ButtonVariant` | `ButtonVariant.Danger` | 刪除按鈕樣式 |
| `DeleteButtonSize` | `ButtonSize` | `ButtonSize.Large` | 刪除按鈕大小 |
| `DeleteButtonTitle` | `string` | `"刪除"` | 刪除按鈕提示文字 |
| `IsDeleteDisabled` | `Func<TItem, bool>?` | - | 判斷刪除按鈕是否禁用的函數 |
| `OnItemDelete` | `EventCallback<TItem>` | - | 刪除項目事件 |
| `CustomActionsTemplate` | `RenderFragment<TItem>?` | - | 內建操作欄的自訂模板 |

---

## 欄位類型說明

### 📊 InteractiveColumnDefinition 基本屬性

| 屬性名稱 | 類型 | 說明 |
|---------|------|------|
| `Title` | `string` | 欄位標題（必填） |
| `PropertyName` | `string` | 對應的資料屬性名稱，支援巢狀屬性（如 `Customer.Name`） |
| `ColumnType` | `InteractiveColumnType` | 欄位類型（必填） |
| `Width` | `string?` | 欄位寬度（CSS 值，如 "200px", "20%"） |
| `IconClass` | `string?` | 標題圖示 CSS 類別 |
| `HeaderCssClass` | `string?` | 標題 CSS 類別 |
| `CellCssClass` | `string?` | 儲存格 CSS 類別 |
| `HideOnMobile` | `bool` | 是否在手機版隱藏 |
| `IsRequired` | `bool` | 是否必填（顯示紅色星號） |
| `IsDisabled` | `bool` | 是否禁用 |
| `IsReadOnly` | `bool` | 是否唯讀 |
| `Placeholder` | `string?` | 佔位符文字 |
| `Tooltip` | `string?` | 工具提示文字 |

### 1️⃣ Display (純顯示文字)

**用途**: 顯示不可編輯的文字內容

**專用屬性**:
```csharp
DisplayFormatter       // Func<object?, string>? - 格式化顯示函數
NullDisplayText       // string? - 空值時顯示的文字（預設為 "-"）
```

**範例**:
```csharp
new InteractiveColumnDefinition
{
    Title = "商品編號",
    PropertyName = "ProductCode",
    ColumnType = InteractiveColumnType.Display,
    Width = "150px",
    DisplayFormatter = (value) => value?.ToString()?.ToUpper() ?? "N/A"
}
```

### 2️⃣ Input (文字輸入框)

**用途**: 提供文字輸入功能

**專用屬性**:
```csharp
OnInputChanged        // EventCallback<(object item, string? value)>? - 輸入變更事件
ValidationPattern     // string? - 驗證用正規表達式
```

**範例**:
```csharp
new InteractiveColumnDefinition
{
    Title = "備註",
    PropertyName = "Remarks",
    ColumnType = InteractiveColumnType.Input,
    Width = "200px",
    Placeholder = "請輸入備註",
    OnInputChanged = EventCallback.Factory.Create<(object, string?)>(this, async (tuple) => 
    {
        await HandleRemarksChange(tuple.Item1, tuple.Item2);
    })
}
```

### 3️⃣ Number (數字輸入框)

**用途**: 提供數字輸入功能，支援範圍驗證

**專用屬性**:
```csharp
MinValue              // decimal? - 最小值
MaxValue              // decimal? - 最大值
Step                  // decimal? - 步進值
OnInputChanged        // EventCallback<(object item, string? value)>? - 輸入變更事件
```

**範例**:
```csharp
new InteractiveColumnDefinition
{
    Title = "數量",
    PropertyName = "Quantity",
    ColumnType = InteractiveColumnType.Number,
    Width = "120px",
    IsRequired = true,
    MinValue = 0,
    MaxValue = 9999,
    Step = 1,
    Placeholder = "0"
}
```

### 4️⃣ Select (下拉選單)

**用途**: 提供固定選項的下拉選單

**專用屬性**:
```csharp
Options               // List<InteractiveSelectOption>? - 選項列表
OnSelectionChanged    // EventCallback<(object item, object? value)>? - 選擇變更事件
IsMultiSelect         // bool - 是否支援多選
```

**範例**:
```csharp
new InteractiveColumnDefinition
{
    Title = "狀態",
    PropertyName = "Status",
    ColumnType = InteractiveColumnType.Select,
    Width = "150px",
    Options = new List<InteractiveSelectOption>
    {
        new() { Value = "1", Text = "啟用" },
        new() { Value = "0", Text = "停用" }
    },
    OnSelectionChanged = EventCallback.Factory.Create<(object, object?)>(this, 
        async (tuple) => await HandleStatusChange(tuple.Item1, tuple.Item2))
}
```

### 5️⃣ SearchableSelect (可搜尋下拉選單)

**用途**: 提供輸入框搜尋 + 動態下拉選單功能，適用於大量選項

**專用屬性**:
```csharp
// 關聯屬性名稱
SearchValuePropertyName        // string? - 搜尋值屬性名稱
SelectedItemPropertyName       // string? - 選中項目屬性名稱
FilteredItemsPropertyName      // string? - 過濾項目列表屬性名稱
ShowDropdownPropertyName       // string? - 顯示下拉選單屬性名稱
SelectedIndexPropertyName      // string? - 選中索引屬性名稱

// 資料與格式化
AvailableItemsProvider         // Func<IEnumerable<object>>? - 所有可用項目提供函數
ItemDisplayFormatter           // Func<object, string>? - 項目顯示格式化函數
SearchFilter                   // Func<object, string, bool>? - 搜尋過濾函數

// 事件處理
OnSearchInputChanged           // EventCallback<(object item, string? searchValue)>?
OnItemSelected                 // EventCallback<(object item, object? selectedItem)>?
OnInputFocus                   // EventCallback<object>?
OnInputBlur                    // EventCallback<object>?
OnItemMouseEnter              // EventCallback<(object item, int index)>?

// UI 設定
MaxDisplayItems               // int - 最大顯示項目數量（預設 20）
DropdownMaxHeight             // string - 下拉選單最大高度（預設 "200px"）
DropdownMinWidth              // string - 下拉選單最小寬度（預設 "300px"）
DropdownMaxWidth              // string - 下拉選單最大寬度（預設 "500px"）

// 鍵盤導航
EnableKeyboardNavigation      // bool - 是否啟用鍵盤導航
```

**使用建議**: 優先使用 `SearchableSelectHelper` 輔助類別來簡化設定（詳見進階功能章節）

### 6️⃣ Checkbox (勾選框)

**用途**: 提供布林值勾選功能

**專用屬性**:
```csharp
OnCheckboxChanged     // EventCallback<(object item, bool isChecked)>? - 勾選狀態變更事件
CheckedText           // string? - 勾選時顯示的文字
UncheckedText         // string? - 未勾選時顯示的文字
```

**範例**:
```csharp
new InteractiveColumnDefinition
{
    Title = "啟用",
    PropertyName = "IsActive",
    ColumnType = InteractiveColumnType.Checkbox,
    Width = "100px",
    CheckedText = "已啟用",
    UncheckedText = "已停用"
}
```

### 7️⃣ Button (按鈕)

**用途**: 在儲存格中顯示操作按鈕

**專用屬性**:
```csharp
ButtonText            // string? - 按鈕文字
ButtonIcon            // string? - 按鈕圖示 CSS 類別
ButtonVariant         // ButtonVariant - 按鈕樣式（Primary, Secondary, Success, Danger 等）
ButtonSize            // ButtonSize - 按鈕大小（Small, Medium, Large）
OnButtonClick         // EventCallback<object>? - 按鈕點擊事件
IsButtonDisabled      // Func<object, bool>? - 判斷按鈕是否禁用的函數
```

**範例**:
```csharp
new InteractiveColumnDefinition
{
    Title = "操作",
    PropertyName = "",
    ColumnType = InteractiveColumnType.Button,
    Width = "120px",
    ButtonText = "選擇",
    ButtonIcon = "bi bi-check-circle",
    ButtonVariant = ButtonVariant.Primary,
    ButtonSize = ButtonSize.Small,
    OnButtonClick = EventCallback.Factory.Create<object>(this, 
        async (item) => await HandleSelectItem(item))
}
```

### 8️⃣ Custom (自訂模板)

**用途**: 完全自訂儲存格內容

**專用屬性**:
```csharp
CustomTemplate            // RenderFragment<object>? - 自訂 Razor 模板
EnableKeyboardNavigation  // bool - 是否啟用鍵盤導航（用於自訂下拉選單等）
```

**範例**:
```csharp
new InteractiveColumnDefinition
{
    Title = "商品",
    PropertyName = "",
    ColumnType = InteractiveColumnType.Custom,
    Width = "300px",
    CustomTemplate = item =>
    {
        var myItem = (MyItemType)item;
        return @<div class="d-flex align-items-center">
            <img src="@myItem.ImageUrl" style="width: 40px; height: 40px;" class="me-2" />
            <div>
                <div class="fw-bold">@myItem.Name</div>
                <small class="text-muted">@myItem.Code</small>
            </div>
        </div>;
    }
}
```

---

## 基本使用範例

### 範例 1: 簡單的唯讀表格

```razor
@using ERPCore2.Components.Shared.SubCollections

<InteractiveTableComponent TItem="Product"
                          Items="@products"
                          ColumnDefinitions="@GetProductColumns()"
                          ShowRowNumbers="true"
                          EmptyMessage="沒有商品資料" />

@code {
    private List<Product> products = new();

    private List<InteractiveColumnDefinition> GetProductColumns()
    {
        return new List<InteractiveColumnDefinition>
        {
            new()
            {
                Title = "商品編號",
                PropertyName = "Code",
                ColumnType = InteractiveColumnType.Display,
                Width = "150px"
            },
            new()
            {
                Title = "商品名稱",
                PropertyName = "Name",
                ColumnType = InteractiveColumnType.Display,
                Width = "250px"
            },
            new()
            {
                Title = "單價",
                PropertyName = "Price",
                ColumnType = InteractiveColumnType.Display,
                Width = "120px",
                DisplayFormatter = (value) => 
                    value is decimal price ? $"NT$ {price:N2}" : "-"
            }
        };
    }
}
```

### 範例 2: 可編輯表格

```razor
<InteractiveTableComponent TItem="OrderDetail"
                          Items="@orderDetails"
                          ColumnDefinitions="@GetEditableColumns()"
                          ShowBuiltInActions="true"
                          ShowBuiltInDeleteButton="true"
                          OnItemDelete="@HandleDeleteItem"
                          EmptyMessage="請新增訂單明細" />

@code {
    private List<OrderDetail> orderDetails = new();

    private List<InteractiveColumnDefinition> GetEditableColumns()
    {
        return new List<InteractiveColumnDefinition>
        {
            new()
            {
                Title = "商品名稱",
                PropertyName = "ProductName",
                ColumnType = InteractiveColumnType.Input,
                Width = "200px",
                IsRequired = true,
                Placeholder = "請輸入商品名稱"
            },
            new()
            {
                Title = "數量",
                PropertyName = "Quantity",
                ColumnType = InteractiveColumnType.Number,
                Width = "120px",
                IsRequired = true,
                MinValue = 1,
                MaxValue = 9999
            },
            new()
            {
                Title = "單價",
                PropertyName = "UnitPrice",
                ColumnType = InteractiveColumnType.Number,
                Width = "150px",
                IsRequired = true,
                MinValue = 0,
                Step = 0.01m
            }
        };
    }

    private async Task HandleDeleteItem(OrderDetail item)
    {
        orderDetails.Remove(item);
        await InvokeAsync(StateHasChanged);
    }
}
```

### 範例 3: 帶行選取功能的表格

```razor
<InteractiveTableComponent TItem="Customer"
                          Items="@customers"
                          ColumnDefinitions="@GetCustomerColumns()"
                          EnableRowSelection="true"
                          AllowMultipleSelection="true"
                          SelectedItems="@selectedCustomers"
                          OnSelectionChanged="@HandleSelectionChanged"
                          ShowRowNumbers="true" />

<div class="mt-3">
    <p>已選取 @selectedCustomers.Count 位客戶</p>
</div>

@code {
    private List<Customer> customers = new();
    private HashSet<Customer> selectedCustomers = new();

    private List<InteractiveColumnDefinition> GetCustomerColumns()
    {
        return new List<InteractiveColumnDefinition>
        {
            new()
            {
                Title = "客戶編號",
                PropertyName = "Code",
                ColumnType = InteractiveColumnType.Display,
                Width = "150px"
            },
            new()
            {
                Title = "客戶名稱",
                PropertyName = "Name",
                ColumnType = InteractiveColumnType.Display,
                Width = "250px"
            }
        };
    }

    private async Task HandleSelectionChanged(HashSet<Customer> selected)
    {
        selectedCustomers = selected;
        Console.WriteLine($"選取了 {selected.Count} 個項目");
        await InvokeAsync(StateHasChanged);
    }
}
```

---

## 進階功能

### 🔍 SearchableSelect 輔助設定

使用 `SearchableSelectHelper` 可以大幅簡化可搜尋下拉選單的設定：

```csharp
// 在你的資料項目類別中定義必要的屬性
public class OrderItem
{
    public string ProductSearchValue { get; set; } = "";
    public Product? SelectedProduct { get; set; }
    public List<Product> FilteredProducts { get; set; } = new();
    public bool ShowProductDropdown { get; set; }
    public int SelectedProductIndex { get; set; } = -1;
}

// 在元件中使用 Helper 建立欄位定義
private List<InteractiveColumnDefinition> GetColumns()
{
    return new List<InteractiveColumnDefinition>
    {
        SearchableSelectHelper.CreateProductSearchColumn<OrderItem>(
            title: "商品選擇",
            width: "300px",
            availableItems: () => allProducts,
            onItemSelected: async (item, selectedProduct) => 
            {
                item.SelectedProduct = selectedProduct as Product;
                await CalculateAmount(item);
            }
        )
    };
}
```

### ⌨️ 鍵盤導航

可搜尋下拉選單支援以下鍵盤操作：

- **↑ (ArrowUp)**: 向上選擇項目
- **↓ (ArrowDown)**: 向下選擇項目
- **Enter**: 確認選擇當前項目
- **Escape**: 關閉下拉選單

啟用方式：
```csharp
new InteractiveColumnDefinition
{
    // ... 其他設定
    EnableKeyboardNavigation = true,
    GetDropdownItems = (item) => ((MyItem)item).FilteredItems,
    GetSelectedIndex = (item) => ((MyItem)item).SelectedIndex,
    SetSelectedIndex = (item, index) => ((MyItem)item).SelectedIndex = index,
    GetShowDropdown = (item) => ((MyItem)item).ShowDropdown,
    SetShowDropdown = (item, show) => ((MyItem)item).ShowDropdown = show
}
```

### ✅ 資料驗證

```csharp
<InteractiveTableComponent TItem="Product"
                          Items="@products"
                          ColumnDefinitions="@columns"
                          ValidationErrors="@validationErrors"
                          OnValidationFailed="@HandleValidationFailed" />

@code {
    private Dictionary<string, string> validationErrors = new();

    private async Task HandleValidationFailed(
        (Product item, string propertyName, string? errorMessage) validation)
    {
        var key = $"{validation.item.GetHashCode()}_{validation.propertyName}";
        
        if (!string.IsNullOrEmpty(validation.errorMessage))
        {
            validationErrors[key] = validation.errorMessage;
            await NotificationService.ShowError(validation.errorMessage);
        }
        else
        {
            validationErrors.Remove(key);
        }
        
        StateHasChanged();
    }
}
```

### 📊 總計列顯示

```razor
<InteractiveTableComponent TItem="InvoiceDetail"
                          Items="@details"
                          ColumnDefinitions="@GetColumns()"
                          ShowTotalRow="true"
                          TotalRowTemplate="@GetTotalRowTemplate()" />

@code {
    private RenderFragment<InteractiveColumnDefinition> GetTotalRowTemplate()
    {
        return column => __builder =>
        {
            if (column.PropertyName == "Quantity")
            {
                var totalQty = details.Sum(d => d.Quantity);
                <strong>總數量: @totalQty</strong>
            }
            else if (column.PropertyName == "Amount")
            {
                var totalAmount = details.Sum(d => d.Amount);
                <strong class="text-primary">總金額: NT$ @totalAmount.ToString("N2")</strong>
            }
        };
    }
}
```

### 🎨 自訂行樣式

```csharp
<InteractiveTableComponent TItem="Order"
                          Items="@orders"
                          ColumnDefinitions="@columns"
                          GetRowCssClass="@GetOrderRowClass" />

@code {
    private string GetOrderRowClass(Order order, int index)
    {
        if (order.Status == "已取消")
            return "table-danger";
        if (order.Status == "已完成")
            return "table-success";
        if (order.IsUrgent)
            return "table-warning";
        
        return "";
    }
}
```

---

## 完整使用案例

### 案例：訂單明細管理元件

```razor
@using ERPCore2.Components.Shared.SubCollections
@using ERPCore2.Helpers

@inject IProductService ProductService
@inject INotificationService NotificationService

<div class="card">
    <div class="card-body">
        <InteractiveTableComponent TItem="OrderDetailItem"
                                  Items="@orderItems"
                                  ColumnDefinitions="@GetColumnDefinitions()"
                                  IsReadOnly="@IsReadOnly"
                                  ShowRowNumbers="true"
                                  ShowBuiltInActions="true"
                                  ShowBuiltInDeleteButton="true"
                                  DeleteButtonTitle="刪除"
                                  IsDeleteDisabled="@((item) => IsReadOnly || item.IsLocked)"
                                  OnItemDelete="@HandleDeleteItem"
                                  ValidationErrors="@validationErrors"
                                  OnValidationFailed="@HandleValidationFailed"
                                  ActionsColumnWidth="80px"
                                  EmptyMessage="尚未新增訂單明細" />
    </div>
    
    <div class="card-footer">
        <div class="row">
            <div class="col-md-6">
                <GenericButtonComponent Text="新增空白列"
                                      IconClass="bi bi-plus-circle"
                                      Variant="ButtonVariant.Primary"
                                      OnClick="@AddEmptyRow"
                                      IsDisabled="@IsReadOnly" />
            </div>
            <div class="col-md-6 text-end">
                <h5>訂單總金額: <span class="text-primary">NT$ @CalculateTotalAmount().ToString("N2")</span></h5>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public bool IsReadOnly { get; set; } = false;
    [Parameter] public EventCallback<List<OrderDetailItem>> OnItemsChanged { get; set; }
    
    private List<OrderDetailItem> orderItems = new();
    private List<Product> allProducts = new();
    private Dictionary<string, string> validationErrors = new();

    protected override async Task OnInitializedAsync()
    {
        allProducts = await ProductService.GetAllAsync();
        AddEmptyRow();
    }

    private List<InteractiveColumnDefinition> GetColumnDefinitions()
    {
        return new List<InteractiveColumnDefinition>
        {
            // 使用 SearchableSelectHelper 建立商品搜尋欄位
            SearchableSelectHelper.CreateProductSearchColumn<OrderDetailItem>(
                title: "商品",
                width: "300px",
                availableItems: () => allProducts,
                onItemSelected: async (item, selectedProduct) =>
                {
                    if (selectedProduct is Product product)
                    {
                        item.SelectedProduct = product;
                        item.UnitPrice = product.DefaultPrice;
                        await CalculateItemAmount(item);
                        EnsureOneEmptyRow();
                    }
                }
            ),
            
            // 數量欄位
            new InteractiveColumnDefinition
            {
                Title = "數量",
                PropertyName = "Quantity",
                ColumnType = InteractiveColumnType.Number,
                Width = "120px",
                IsRequired = true,
                MinValue = 1,
                MaxValue = 9999,
                Placeholder = "0",
                OnInputChanged = EventCallback.Factory.Create<(object, string?)>(this, 
                    async (tuple) =>
                    {
                        if (tuple.Item1 is OrderDetailItem item)
                        {
                            await CalculateItemAmount(item);
                        }
                    })
            },
            
            // 單價欄位
            new InteractiveColumnDefinition
            {
                Title = "單價",
                PropertyName = "UnitPrice",
                ColumnType = InteractiveColumnType.Number,
                Width = "150px",
                IsRequired = true,
                MinValue = 0,
                Step = 0.01m,
                OnInputChanged = EventCallback.Factory.Create<(object, string?)>(this, 
                    async (tuple) =>
                    {
                        if (tuple.Item1 is OrderDetailItem item)
                        {
                            await CalculateItemAmount(item);
                        }
                    })
            },
            
            // 折扣欄位
            new InteractiveColumnDefinition
            {
                Title = "折扣(%)",
                PropertyName = "DiscountPercentage",
                ColumnType = InteractiveColumnType.Number,
                Width = "100px",
                MinValue = 0,
                MaxValue = 100,
                Step = 0.1m,
                Placeholder = "0",
                OnInputChanged = EventCallback.Factory.Create<(object, string?)>(this, 
                    async (tuple) =>
                    {
                        if (tuple.Item1 is OrderDetailItem item)
                        {
                            await CalculateItemAmount(item);
                        }
                    })
            },
            
            // 金額欄位（唯讀）
            new InteractiveColumnDefinition
            {
                Title = "金額",
                PropertyName = "Amount",
                ColumnType = InteractiveColumnType.Display,
                Width = "150px",
                DisplayFormatter = (value) => 
                    value is decimal amount ? $"NT$ {amount:N2}" : "NT$ 0.00",
                CellCssClass = "fw-bold text-end"
            },
            
            // 備註欄位
            new InteractiveColumnDefinition
            {
                Title = "備註",
                PropertyName = "Remarks",
                ColumnType = InteractiveColumnType.Input,
                Width = "200px",
                Placeholder = "輸入備註..."
            }
        };
    }

    private async Task CalculateItemAmount(OrderDetailItem item)
    {
        if (item.Quantity > 0 && item.UnitPrice > 0)
        {
            var subtotal = item.Quantity * item.UnitPrice;
            var discount = subtotal * (item.DiscountPercentage / 100m);
            item.Amount = subtotal - discount;
        }
        else
        {
            item.Amount = 0;
        }
        
        await OnItemsChanged.InvokeAsync(orderItems);
        StateHasChanged();
    }

    private decimal CalculateTotalAmount()
    {
        return orderItems
            .Where(item => item.SelectedProduct != null)
            .Sum(item => item.Amount);
    }

    private void AddEmptyRow()
    {
        orderItems.Add(new OrderDetailItem());
        StateHasChanged();
    }

    private void EnsureOneEmptyRow()
    {
        AutoEmptyRowHelper.ForAny<OrderDetailItem>.EnsureOneEmptyRow(
            orderItems,
            item => item.SelectedProduct == null,
            () => new OrderDetailItem()
        );
        StateHasChanged();
    }

    private async Task HandleDeleteItem(OrderDetailItem item)
    {
        orderItems.Remove(item);
        await OnItemsChanged.InvokeAsync(orderItems);
        EnsureOneEmptyRow();
    }

    private async Task HandleValidationFailed(
        (OrderDetailItem item, string propertyName, string? errorMessage) validation)
    {
        var key = $"{validation.item.GetHashCode()}_{validation.propertyName}";
        
        if (!string.IsNullOrEmpty(validation.errorMessage))
        {
            validationErrors[key] = validation.errorMessage;
            await NotificationService.ShowError(validation.errorMessage);
        }
        else
        {
            validationErrors.Remove(key);
        }
        
        StateHasChanged();
    }

    // 資料項目類別
    public class OrderDetailItem
    {
        public int Id { get; set; }
        public Product? SelectedProduct { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
        public bool IsLocked { get; set; }
        
        // SearchableSelect 必要屬性
        public string ProductSearchValue { get; set; } = "";
        public List<Product> FilteredProducts { get; set; } = new();
        public bool ShowProductDropdown { get; set; }
        public int SelectedProductIndex { get; set; } = -1;
    }
}
```

---

## 注意事項

### ⚠️ 重要提醒

1. **TItem 泛型約束**
   - `InteractiveTableComponent` 使用泛型 `TItem`，需在使用時明確指定類型
   - 範例: `<InteractiveTableComponent TItem="Product" ... />`

2. **PropertyName 與巢狀屬性**
   - 支援巢狀屬性存取，例如: `"Customer.Name"`, `"Order.Customer.Company.Name"`
   - 屬性必須是可讀寫的 (除非是 Display 類型)

3. **SearchableSelect 必要屬性**
   - 使用 `SearchableSelect` 類型時，資料項目必須包含以下屬性：
     ```csharp
     public string SearchValue { get; set; } = "";
     public object? SelectedItem { get; set; }
     public List<object> FilteredItems { get; set; } = new();
     public bool ShowDropdown { get; set; }
     public int SelectedIndex { get; set; } = -1;
     ```
   - 建議使用 `SearchableSelectHelper` 來簡化設定

4. **事件處理**
   - EventCallback 必須使用 `EventCallback.Factory.Create` 建立
   - 事件處理函數應該是 `async Task` 方法

5. **自動空白行管理**
   - 可搭配 `AutoEmptyRowHelper` 來自動維護表格底部的空白行
   - 詳見專案中的 `README_自動空行說明.md`

6. **驗證**
   - 驗證錯誤字典的 Key 格式: `{item.GetHashCode()}_{propertyName}`
   - 驗證失敗時會自動加上 `is-invalid` CSS 類別

7. **效能考量**
   - 大量資料時建議使用虛擬化或分頁
   - SearchableSelect 預設最多顯示 20 筆，可透過 `MaxDisplayItems` 調整

8. **響應式設計**
   - 使用 `HideOnMobile = true` 可在手機版隱藏非必要欄位
   - 建議為重要欄位設定適當的 `Width`

9. **JavaScript 依賴**
   - 元件內建的 JavaScript 用於 SearchableSelect 的下拉選單定位
   - 確保頁面載入完成後才渲染元件

10. **行選取模式**
    - 單選模式: 點擊已選取項目會取消選取
    - 多選模式: 可同時選取多個項目
    - 選取狀態會自動加上 `row-selected` CSS 類別

---

## 相關文件

- 📄 `README_自動空行說明.md` - AutoEmptyRowHelper 使用說明
- 📄 `README_Services.md` - Service 層說明
- 📄 `README_Data.md` - 資料層說明

---

## 更新歷史

| 版本 | 日期 | 說明 |
|------|------|------|
| 1.0 | 2025-01-03 | 初始版本 - 完整功能說明文件 |

---

**提示**: 如有任何問題或建議，請聯繫開發團隊或參考專案中的其他使用範例。
