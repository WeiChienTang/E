# GenericTableComponent 通用表格元件

一個功能豐富的 Blazor Server 表格元件，支援多種資料型別的自動格式化、排序、點擊事件等功能。

## 📝 用途與時機

### 適用場景
- **資料列表展示**：顯示任何型別的物件集合
- **管理後台介面**：用戶、訂單、產品等資料管理頁面
- **報表呈現**：需要格式化數值、日期、狀態的表格
- **互動式表格**：需要排序、點擊、自訂操作按鈕的場景

### 主要特色
- ✅ **型別安全**：使用泛型支援任何資料型別
- ✅ **自動格式化**：內建數值、日期、貨幣、布林值、狀態等格式化
- ✅ **巢狀屬性**：支援 `Customer.Company.Name` 這樣的巢狀屬性存取
- ✅ **彈性樣式**：Bootstrap 整合，支援多種表格樣式
- ✅ **互動功能**：排序、行點擊、自訂操作按鈕
- ✅ **空資料處理**：可自訂空資料顯示樣式
- ✅ **自訂範本**：某些欄位可使用完全自訂的 RenderFragment

## 🔧 參數說明

### 核心參數
| 參數 | 型別 | 預設值 | 說明 |
|------|------|--------|------|
| `Items` | `IEnumerable<TItem>?` | - | 要顯示的資料集合 |
| `ColumnDefinitions` | `List<TableColumnDefinition>?` | - | 欄位定義列表 |
| `ActionsTemplate` | `RenderFragment<TItem>?` | - | 操作按鈕的自訂範本 |
| `EmptyTemplate` | `RenderFragment?` | - | 空資料時的自訂範本 |

### 顯示控制
| 參數 | 型別 | 預設值 | 說明 |
|------|------|--------|------|
| `ShowHeader` | `bool` | `true` | 是否顯示表格標題列 |
| `ShowActions` | `bool` | `false` | 是否顯示操作欄 |
| `EmptyMessage` | `string` | `"沒有找到資料"` | 空資料時顯示的訊息 |
| `ActionsHeader` | `string` | `"操作"` | 操作欄的標題 |

### 樣式設定
| 參數 | 型別 | 預設值 | 說明 |
|------|------|--------|------|
| `IsStriped` | `bool` | `true` | 是否使用條紋樣式 |
| `IsHoverable` | `bool` | `true` | 是否啟用滑鼠懸停效果 |
| `IsBordered` | `bool` | `false` | 是否顯示邊框 |
| `Size` | `TableSize` | `Normal` | 表格大小 (Small/Normal/Large) |
| `CssClass` | `string` | `""` | 額外的 CSS 類別 |
| `GetRowCssClass` | `Func<TItem, string>?` | - | 動態取得每行的 CSS 類別 |

### 互動功能
| 參數 | 型別 | 預設值 | 說明 |
|------|------|--------|------|
| `EnableRowClick` | `bool` | `false` | 是否啟用行點擊功能 |
| `OnRowClick` | `EventCallback<TItem>` | - | 行點擊事件回調 |
| `RowClickCursor` | `string` | `"pointer"` | 行點擊時的滑鼠游標樣式 |
| `EnableSorting` | `bool` | `false` | 是否啟用排序功能 |
| `OnSort` | `EventCallback<string>` | - | 排序事件回調 |

### TableColumnDefinition 欄位設定
| 屬性 | 型別 | 說明 |
|------|------|------|
| `Title` | `string` | 欄位標題 |
| `PropertyName` | `string` | 對應的屬性名稱，支援巢狀屬性 |
| `DataType` | `ColumnDataType` | 資料型別 (Text/Number/Currency/Date/DateTime/Boolean/Status/Html) |
| `CustomTemplate` | `RenderFragment<object>?` | 自訂單元格範本 |
| `IsSortable` | `bool` | 是否可排序 |
| `Format` | `string?` | 格式化字串 |
| `CurrencySymbol` | `string?` | 貨幣符號 |
| `TrueText/FalseText` | `string?` | 布林值顯示文字 |
| `StatusBadgeMap` | `Dictionary<object, string>?` | 狀態徽章樣式對應 |

## 📚 用法範例

### 基本用法
```csharp
@page "/customers"

<GenericTableComponent TItem="Customer" 
                      Items="customers" 
                      ColumnDefinitions="columnDefinitions" />

@code {
    private List<Customer> customers = new();
    private List<TableColumnDefinition> columnDefinitions = new();

    protected override void OnInitialized()
    {
        columnDefinitions = new List<TableColumnDefinition>
        {
            TableColumnDefinition.Text("客戶名稱", nameof(Customer.Name)),
            TableColumnDefinition.Text("公司", "Company.CompanyName"), // 巢狀屬性
            TableColumnDefinition.Date("註冊日期", nameof(Customer.RegisterDate)),
            TableColumnDefinition.Boolean("是否啟用", nameof(Customer.IsActive))
        };
        
        LoadCustomers();
    }
}
```

### 進階用法 - 完整功能展示
```csharp
@page "/orders"

<GenericTableComponent TItem="Order" 
                      Items="orders" 
                      ColumnDefinitions="columnDefinitions"
                      ShowActions="true"
                      ActionsTemplate="ActionsTemplate"
                      EnableRowClick="true"
                      OnRowClick="HandleRowClick"
                      EnableSorting="true"
                      OnSort="HandleSort"
                      GetRowCssClass="GetOrderRowClass"
                      IsStriped="true"
                      IsHoverable="true" />

@code {
    private List<Order> orders = new();
    private List<TableColumnDefinition> columnDefinitions = new();

    protected override void OnInitialized()
    {
        columnDefinitions = new List<TableColumnDefinition>
        {
            // 基本文字欄位
            TableColumnDefinition.Text("訂單編號", nameof(Order.OrderNo)),
            
            // 巢狀屬性
            TableColumnDefinition.Text("客戶", "Customer.Name"),
            
            // 格式化貨幣
            TableColumnDefinition.Currency("金額", nameof(Order.Amount), "NT$", "N0"),
            
            // 格式化日期時間
            TableColumnDefinition.DateTime("訂單日期", nameof(Order.OrderDate), "yyyy/MM/dd HH:mm"),
            
            // 狀態徽章
            TableColumnDefinition.Status("狀態", nameof(Order.Status), new Dictionary<object, string>
            {
                { OrderStatus.Pending, "bg-warning" },
                { OrderStatus.Processing, "bg-info" },
                { OrderStatus.Completed, "bg-success" },
                { OrderStatus.Cancelled, "bg-danger" }
            }),
            
            // 自訂範本
            TableColumnDefinition.Template("進度", context => 
                @<div class="progress" style="height: 20px;">
                    <div class="progress-bar" style="width: @(((Order)context).Progress)%">
                        @(((Order)context).Progress)%
                    </div>
                </div>
            )
        };
        
        LoadOrders();
    }

    // 操作按鈕範本
    private RenderFragment<Order> ActionsTemplate => order => 
        @<div class="btn-group">
            <button class="btn btn-sm btn-outline-primary" @onclick="() => EditOrder(order)">
                <i class="fas fa-edit"></i> 編輯
            </button>
            <button class="btn btn-sm btn-outline-danger" @onclick="() => DeleteOrder(order)">
                <i class="fas fa-trash"></i> 刪除
            </button>
        </div>;

    // 動態行樣式
    private string GetOrderRowClass(Order order)
    {
        return order.Status switch
        {
            OrderStatus.Cancelled => "table-danger",
            OrderStatus.Completed => "table-success",
            _ => ""
        };
    }

    // 事件處理
    private void HandleRowClick(Order order)
    {
        NavigationManager.NavigateTo($"/orders/{order.Id}");
    }

    private async Task HandleSort(string propertyName)
    {
        // 實作排序邏輯
        await LoadOrdersSorted(propertyName);
    }

    private void EditOrder(Order order) { /* 編輯邏輯 */ }
    private void DeleteOrder(Order order) { /* 刪除邏輯 */ }
}
```

### 各種資料型別展示
```csharp
columnDefinitions = new List<TableColumnDefinition>
{
    // 文字 (預設)
    TableColumnDefinition.Text("產品名稱", "ProductName"),
    
    // 數值格式化
    TableColumnDefinition.Number("數量", "Quantity", "N0"), // 整數
    TableColumnDefinition.Number("單價", "UnitPrice", "N2"), // 兩位小數
    
    // 貨幣格式化
    TableColumnDefinition.Currency("總價", "TotalPrice", "NT$", "N0"),
    TableColumnDefinition.Currency("美金價格", "UsdPrice", "USD $", "N2"),
    
    // 日期格式化
    TableColumnDefinition.Date("生產日期", "ProductionDate", "yyyy-MM-dd"),
    TableColumnDefinition.DateTime("最後更新", "LastUpdated", "MM/dd HH:mm"),
    
    // 布林值
    TableColumnDefinition.Boolean("有庫存", "InStock", "有", "缺貨"),
    TableColumnDefinition.Boolean("推薦", "IsRecommended", "⭐", ""),
    
    // 狀態徽章
    TableColumnDefinition.Status("狀態", "Status", new Dictionary<object, string>
    {
        { "Active", "bg-success" },
        { "Inactive", "bg-secondary" },
        { "Discontinued", "bg-danger" }
    })
};
```

### 空資料自訂範本
```csharp
<GenericTableComponent TItem="Product" 
                      Items="products" 
                      ColumnDefinitions="columnDefinitions"
                      EmptyTemplate="EmptyTemplate" />

@code {
    private RenderFragment EmptyTemplate => 
        @<div class="text-center py-5">
            <i class="fas fa-box-open fa-3x text-muted mb-3"></i>
            <h5 class="text-muted">目前沒有產品資料</h5>
            <button class="btn btn-primary mt-2" @onclick="CreateNewProduct">
                <i class="fas fa-plus"></i> 新增產品
            </button>
        </div>;
}
```