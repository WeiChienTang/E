# GenericTableComponent 分頁功能說明

## 📖 概述

`GenericTableComponent` 現在支援完整的分頁功能，可以讓您輕鬆地為大量資料提供分頁顯示。

## 🎯 功能特色

- ✅ **自動分頁控制** - 自動計算總頁數和分頁按鈕
- ✅ **頁碼按鈕群** - 智能顯示頁碼按鈕（可配置顯示數量）
- ✅ **每頁筆數選擇** - 使用者可以動態調整每頁顯示筆數
- ✅ **資料統計資訊** - 顯示當前頁次、總筆數等資訊
- ✅ **美觀的 UI** - 使用 Bootstrap 樣式，與 GenericButtonComponent 整合
- ✅ **完整的事件處理** - 支援頁面變更和每頁筆數變更事件

## 🚀 基本使用方法

### 1. 基本分頁表格

```razor
<GenericTableComponent TItem="Customer"
                     Items="customers"
                     ColumnDefinitions="GetColumnDefinitions()"
                     EnablePagination="true"
                     CurrentPage="currentPage"
                     PageSize="pageSize"
                     TotalItems="totalItems"
                     OnPageChanged="HandlePageChanged"
                     OnPageSizeChanged="HandlePageSizeChanged" />
```

### 2. 完整範例

```razor
@page "/customers"
@using ERPCore2.Data.Entities.Customers
@using ERPCore2.Services
@inject CustomerService CustomerService

<GenericTableComponent TItem="Customer"
                     Items="customers"
                     ColumnDefinitions="GetColumnDefinitions()"
                     EnablePagination="true"
                     CurrentPage="currentPage"
                     PageSize="pageSize"
                     TotalItems="totalItems"
                     OnPageChanged="HandlePageChanged"
                     OnPageSizeChanged="HandlePageSizeChanged"
                     ShowPageSizeSelector="true"
                     PageSizeOptions="pageSizeOptions"
                     MaxDisplayedPages="5"
                     IsStriped="true"
                     IsHoverable="true" />

@code {
    private List<Customer> customers = new();
    private int currentPage = 1;
    private int pageSize = 20;
    private int totalItems = 0;
    private List<int> pageSizeOptions = new() { 10, 20, 50, 100 };

    protected override async Task OnInitializedAsync()
    {
        await LoadCustomers();
    }

    private async Task LoadCustomers()
    {
        // 使用 GenericManagementService 的分頁方法
        var result = await CustomerService.GetPagedAsync(currentPage, pageSize);
        customers = result.Items;
        totalItems = result.TotalCount;
        StateHasChanged();
    }

    private async Task HandlePageChanged(int newPage)
    {
        currentPage = newPage;
        await LoadCustomers();
    }

    private async Task HandlePageSizeChanged(int newPageSize)
    {
        pageSize = newPageSize;
        currentPage = 1; // 重設為第一頁
        await LoadCustomers();
    }

    private List<TableColumnDefinition> GetColumnDefinitions()
    {
        return new List<TableColumnDefinition>
        {
            TableColumnDefinition.Text("客戶代碼", nameof(Customer.CustomerCode)),
            TableColumnDefinition.Text("客戶名稱", nameof(Customer.CustomerName)),
            // ... 其他欄位定義
        };
    }
}
```

## 📋 分頁相關參數

| 參數名稱 | 類型 | 預設值 | 說明 |
|---------|------|--------|------|
| `EnablePagination` | `bool` | `false` | 是否啟用分頁功能 |
| `CurrentPage` | `int` | `1` | 目前頁碼 |
| `PageSize` | `int` | `20` | 每頁顯示筆數 |
| `TotalItems` | `int` | `0` | 總資料筆數 |
| `OnPageChanged` | `EventCallback<int>` | - | 頁面變更事件 |
| `OnPageSizeChanged` | `EventCallback<int>` | - | 每頁筆數變更事件 |
| `ShowPageSizeSelector` | `bool` | `true` | 是否顯示每頁筆數選擇器 |
| `PageSizeOptions` | `List<int>` | `{10, 20, 50, 100}` | 每頁筆數選項 |
| `MaxDisplayedPages` | `int` | `5` | 最多顯示的頁碼按鈕數量 |

## 🎨 UI 元素說明

### 分頁控制區塊包含：

1. **資料統計資訊**（左側）
   - 顯示："顯示第 X - Y 筆，共 Z 筆資料 (第 N 頁，共 M 頁)"

2. **分頁按鈕群**（中間）
   - 第一頁按鈕
   - 上一頁按鈕  
   - 頁碼按鈕群（智能顯示）
   - 下一頁按鈕
   - 最後一頁按鈕

3. **每頁筆數選擇器**（右側）
   - 下拉選單讓使用者選擇每頁顯示筆數

## 🔧 與服務層整合

所有繼承自 `GenericManagementService<T>` 的服務都已經有 `GetPagedAsync` 方法：

```csharp
public virtual async Task<(List<T> Items, int TotalCount)> GetPagedAsync(
    int pageNumber, 
    int pageSize, 
    string? searchTerm = null)
```

### 使用範例：

```csharp
// 客戶服務
var customerResult = await CustomerService.GetPagedAsync(1, 20);

// 產品服務  
var productResult = await ProductService.GetPagedAsync(1, 20);

// 任何繼承 GenericManagementService 的服務
var result = await YourService.GetPagedAsync(pageNumber, pageSize, searchTerm);
```

## 💡 最佳實踐

### 1. 事件處理模式
```csharp
private async Task HandlePageChanged(int newPage)
{
    currentPage = newPage;
    await LoadData(); // 重新載入資料
}

private async Task HandlePageSizeChanged(int newPageSize)
{
    pageSize = newPageSize;
    currentPage = 1; // 重要：重設為第一頁
    await LoadData();
}
```

### 2. 錯誤處理
```csharp
private async Task LoadData()
{
    try
    {
        var result = await YourService.GetPagedAsync(currentPage, pageSize);
        items = result.Items;
        totalItems = result.TotalCount;
        StateHasChanged();
    }
    catch (Exception ex)
    {
        // 適當的錯誤處理
        await JSRuntime.InvokeVoidAsync("console.error", $"載入資料失敗: {ex.Message}");
    }
}
```

### 3. 載入狀態管理
```csharp
private bool isLoading = false;

private async Task LoadData()
{
    isLoading = true;
    StateHasChanged();
    
    try
    {
        // 載入資料邏輯
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

## 📊 進階功能

### 搜尋與分頁結合
```csharp
private string searchTerm = "";

private async Task HandleSearch(string term)
{
    searchTerm = term;
    currentPage = 1; // 搜尋時重設為第一頁
    await LoadData();
}

private async Task LoadData()
{
    var result = await YourService.GetPagedAsync(currentPage, pageSize, searchTerm);
    items = result.Items;
    totalItems = result.TotalCount;
    StateHasChanged();
}
```

### 自訂頁碼顯示邏輯
- 組件會自動計算要顯示的頁碼範圍
- 預設最多顯示 5 個頁碼按鈕
- 可以透過 `MaxDisplayedPages` 參數調整

## 🎯 範例頁面

參考 `/examples/pagination-table` 頁面查看完整的分頁功能演示。

## 🔗 相關文檔

- [GenericTableComponent 完整說明](../Tables/README_GenericTableComponent.md)
- [GenericManagementService 分頁方法](../../Services/README_Services.md)
- [GenericButtonComponent 說明](../Buttons/README_Buttons.md)
