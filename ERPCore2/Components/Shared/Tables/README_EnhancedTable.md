# Tables 資料夾 - 增強版說明

## 概述

Tables 資料夾現在包含兩個主要的表格元件：

1. **TableComponent.razor** - 原有的基礎表格元件
2. **EnhancedTableComponent.razor** - 新的增強版表格元件（**推薦使用**）

## 元件比較

### TableComponent（原有元件）
- 需要手動撰寫 `<RowTemplate>` 
- 每個欄位需要手動處理格式化
- 適用於需要完全自定義每個單元格的場景

### EnhancedTableComponent（新增元件）
- **配置式設計** - 透過 `ColumnDefinitions` 定義欄位
- **自動格式化** - 支援多種資料類型的自動格式化
- **減少重複代碼** - 符合 Shared 元件的設計理念
- **類型安全** - 支援巢狀屬性存取

## EnhancedTableComponent 功能特點

### 支援的資料類型
- **Text** - 一般文字
- **Number** - 數值格式化
- **Currency** - 貨幣格式化
- **Date** - 日期格式化
- **DateTime** - 日期時間格式化
- **Boolean** - 布林值顯示
- **Status** - 狀態徽章顯示
- **Html** - HTML 內容

### 進階功能
- **巢狀屬性支援** - 例如 `"CustomerType.TypeName"`
- **自定義範本** - 透過 `CustomTemplate` 
- **排序功能** - 透過 `IsSortable` 和 `OnSort`
- **行點擊** - 透過 `EnableRowClick` 和 `OnRowClick`
- **狀態徽章對應** - 透過 `StatusBadgeMap`

## 使用方式

### 基本使用

```razor
@page "/my-page"
@using ERPCore2.Components.Shared.Tables

<EnhancedTableComponent TItem="Customer" 
                      Items="customers"
                      ColumnDefinitions="columnDefinitions"
                      ShowActions="true"
                      EnableRowClick="true"
                      OnRowClick="OnRowClick">
    <ActionsTemplate Context="customer">
        <button class="btn btn-sm btn-outline-primary" @onclick="() => Edit(customer.Id)">
            <i class="fas fa-edit"></i>
        </button>
    </ActionsTemplate>
</EnhancedTableComponent>

@code {
    private List<Customer> customers = new();
    private List<TableColumnDefinition> columnDefinitions = new();
    
    protected override void OnInitialized()
    {
        columnDefinitions = new List<TableColumnDefinition>
        {
            // 文字欄位
            TableColumnDefinition.Text("公司名稱", "CompanyName"),
            
            // 巢狀屬性
            TableColumnDefinition.Text("客戶類型", "CustomerType.TypeName"),
            
            // 日期格式化
            TableColumnDefinition.Date("建立日期", "CreatedAt", "yyyy/MM/dd"),
            
            // 貨幣格式化
            TableColumnDefinition.Currency("金額", "Amount", "NT$", "N0"),
            
            // 自定義範本
            TableColumnDefinition.Template(
                "狀態", 
                customer => @<StatusBadgeComponent Status="((Customer)customer).Status" />
            ),
            
            // 狀態徽章（使用預設對應）
            TableColumnDefinition.Status("狀態", "Status", GetStatusBadgeMap())
        };
    }
    
    private Dictionary<object, string> GetStatusBadgeMap()
    {
        return new Dictionary<object, string>
        {
            { EntityStatus.Active, "bg-success" },
            { EntityStatus.Inactive, "bg-secondary" }
        };
    }
    
    private void OnRowClick(Customer customer)
    {
        // 處理行點擊事件
    }
}
```

### 進階範例

```razor
// 數值欄位
TableColumnDefinition.Number("數量", "Quantity", "N0", "text-end"),

// 貨幣欄位
TableColumnDefinition.Currency("價格", "Price", "USD", "N2", "text-end"),

// 布林值欄位
TableColumnDefinition.Boolean("啟用", "IsActive", "啟用", "停用"),

// 帶圖示的欄位標題
new TableColumnDefinition
{
    Title = "重要客戶",
    PropertyName = "IsVIP",
    DataType = ColumnDataType.Boolean,
    IconClass = "fas fa-star",
    TrueText = "是",
    FalseText = "否"
}
```

## 工廠方法

`TableColumnDefinition` 提供便利的靜態工廠方法：

```csharp
// 文字欄位
TableColumnDefinition.Text(title, propertyName, cssClass)

// 數值欄位
TableColumnDefinition.Number(title, propertyName, format, cssClass)

// 貨幣欄位
TableColumnDefinition.Currency(title, propertyName, symbol, format, cssClass)

// 日期欄位
TableColumnDefinition.Date(title, propertyName, format, cssClass)

// 日期時間欄位
TableColumnDefinition.DateTime(title, propertyName, format, cssClass)

// 布林值欄位
TableColumnDefinition.Boolean(title, propertyName, trueText, falseText, cssClass)

// 狀態欄位
TableColumnDefinition.Status(title, propertyName, badgeMap, cssClass)

// 自定義範本欄位
TableColumnDefinition.Template(title, template, cssClass)
```

## 遷移指南

### 從 TableComponent 遷移到 EnhancedTableComponent

**原有寫法：**
```razor
<TableComponent TItem="Customer" 
              Items="customers"
              Headers="tableHeaders">
    <RowTemplate Context="customer">
        <td>@customer.CustomerCode</td>
        <td>@customer.CompanyName</td>
        <td>@customer.CreatedAt.ToString("yyyy/MM/dd")</td>
    </RowTemplate>
</TableComponent>

@code {
    private List<string> tableHeaders = new() { "客戶代碼", "公司名稱", "建立日期" };
}
```

**新寫法：**
```razor
<EnhancedTableComponent TItem="Customer" 
                      Items="customers"
                      ColumnDefinitions="columnDefinitions" />

@code {
    private List<TableColumnDefinition> columnDefinitions = new()
    {
        TableColumnDefinition.Text("客戶代碼", "CustomerCode"),
        TableColumnDefinition.Text("公司名稱", "CompanyName"),
        TableColumnDefinition.Date("建立日期", "CreatedAt", "yyyy/MM/dd")
    };
}
```

## 效益

1. **減少代碼重複** - 不需要為每個表格重複撰寫格式化邏輯
2. **提高一致性** - 統一的格式化規則
3. **易於維護** - 集中化的格式化邏輯
4. **類型安全** - 編譯時期檢查屬性名稱（使用 nameof 時）
5. **功能豐富** - 內建多種常用格式化選項

## 注意事項

1. 對於需要複雜自定義顯示的欄位，仍建議使用 `CustomTemplate`
2. 巢狀屬性存取使用反射，對效能有輕微影響
3. 狀態徽章需要預先定義對應關係
4. HTML 內容類型需要謹慎使用，避免 XSS 攻擊

## 範例頁面

參考 `EnhancedTableDemo.razor` 獲得完整的使用範例。
