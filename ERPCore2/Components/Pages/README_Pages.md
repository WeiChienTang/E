# ERPCore2 頁面開發指南

## 概述

本文件說明 ERPCore2 系統中頁面（Pages）的開發規範與架構設計。所有頁面都應遵循統一的開發模式，確保程式碼的一致性、可維護性和可擴展性。

## 頁面架構模式

### 1. 套裝版模式

適用於標準的 CRUD 操作頁面，可直接使用 `PageModels` 底下的三個範本檔案：

- **Index.razor** - 列表頁面範本
- **Detail.razor** - 詳細頁面範本  
- **Edit.razor** - 編輯頁面範本

### 2. 客製版模式

根據特定業務需求設計的頁面，如庫存查詢總覽頁面，需要根據 HTML 設計決定要套用的模板和元件組合。

## 權限保護規範

### 必須包含的權限控制

每個頁面都**必須**包含以下權限保護機制：

```razor
@page "/your-page"
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize]

<PagePermissionCheck RequiredPermission="YourModule.YourAction">
    <!-- 頁面內容 -->
</PagePermissionCheck>
```

### 權限保護層級說明

1. **`@attribute [Authorize]`** - 基本登入驗證
   - 確保使用者已登入系統
   - ASP.NET Core 內建機制，執行速度快
   - 未登入用戶會被自動導向登入頁面

2. **`PagePermissionCheck`** - 功能權限驗證
   - 檢查使用者是否具備特定功能權限
   - 提供友善的權限不足錯誤頁面
   - 支援動態權限檢查

### 權限命名規範

權限命名遵循 `模組.動作` 的格式：

| 頁面類型 | 權限格式 | 範例 |
|---------|---------|------|
| 列表頁面 | `{Module}.Read` | `Customer.Read` |
| 詳細頁面 | `{Module}.Read` | `Customer.Read` |
| 新增頁面 | `{Module}.Create` | `Customer.Create` |
| 編輯頁面 | `{Module}.Update` | `Customer.Update` |
| 刪除功能 | `{Module}.Delete` | `Customer.Delete` |
| 系統管理 | `System.Admin` | `System.Admin` |

### 新增權限說明

如需新增新的權限項目，請在 `Data\SeedDataManager\Seeders\PermissionSeeder.cs` 檔案中的權限陣列中添加相應的權限定義。權限將在系統初始化時自動建立。

### 動態權限範例

編輯頁面可以根據是新增還是修改動態設定權限：

```razor
<PagePermissionCheck RequiredPermission="@(Id.HasValue ? "Customer.Update" : "Customer.Create")">
    <GenericEditPageComponent ...>
        <!-- 編輯表單內容 -->
    </GenericEditPageComponent>
</PagePermissionCheck>
```

## 核心構成元件

### 1. 共用元件 (Components/Shared/)

所有頁面都大量使用共用元件來保持一致性：

#### 版面配置元件
- `GenericHeaderComponent` - 頁面標題與麵包屑
- `LayoutView` - 主要版面配置

#### 表單元件 (Forms/)
- `GenericSearchFilterComponent` - 通用搜尋篩選器
- `SearchFilterBuilder` - 篩選器建構器
- `SelectOption` - 下拉選項

#### 表格元件 (Tables/)
- `GenericTableComponent` - 通用資料表格
- `TableColumnDefinition` - 欄位定義
- `ColumnDataType` - 欄位資料類型

#### 其他共用元件
- `InventoryStatisticsCards` - 統計卡片（特定業務元件）

### 2. 資料模型

每個頁面都應該定義清楚的資料模型：

```csharp
// 主要資料清單
private List<[EntityType]> items = new();
private List<[EntityType]> filteredItems = new();
private List<[EntityType]> pagedItems = new();

// 選項清單（用於下拉選單等）
private List<[OptionType]> options = new();

// 篩選相關
private SearchFilterModel searchModel = new();
private List<SearchFilterDefinition> filterDefinitions = new();

// 表格相關
private List<TableColumnDefinition> columnDefinitions = new();

// 分頁相關
private int currentPage = 1;
private int pageSize = 20;
private List<int> pageSizeOptions = new() { 10, 20, 50, 100 };

// 狀態管理
private bool isLoading = true;
```

## 錯誤處理規範

### 必須使用 Try-Catch

所有方法都必須使用 Try-Catch 結構，並調用 `ErrorHandlingHelper.HandlePageErrorAsync` 進行錯誤處理：

```csharp
private async Task LoadDataAsync()
{
    try
    {
        // 業務邏輯
        items = await SomeService.GetAllAsync();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(
            ex,                    // 例外物件
            nameof(LoadDataAsync), // 方法名稱
            GetType(),             // 當前類別類型
            Navigation            // 導航管理器（可選）
        );
    }
}
```

### 錯誤處理的變體用法

```csharp
// 1. 基本用法（同步方法）
catch (Exception ex)
{
    _ = ErrorHandlingHelper.HandlePageErrorAsync(
        ex,
        nameof(MethodName),
        GetType(),
        additionalData: "額外的錯誤資訊"
    );
}

// 2. 含導航的用法（重要錯誤需要跳轉）
catch (Exception ex)
{
    await ErrorHandlingHelper.HandlePageErrorAsync(
        ex,
        "頁面初始化",  // 可用中文描述
        GetType(),
        Navigation
    );
}
```

## 頁面生命週期

### OnInitializedAsync 標準流程

```csharp
protected override async Task OnInitializedAsync()
{
    try
    {
        // 1. 載入基礎資料（選項清單等）
        await LoadBasicDataAsync();
        
        // 2. 初始化篩選器
        InitializeFilters();
        
        // 3. 初始化表格欄位
        InitializeColumns();
        
        // 4. 載入主要資料
        await LoadMainDataAsync();
        
        // 5. 載入統計資料（如需要）
        await LoadStatisticsAsync();
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(
            ex, 
            "頁面初始化", 
            GetType(), 
            Navigation
        );
    }
    finally
    {
        isLoading = false;
    }
}
```

## 篩選器實作模式

### 1. 篩選器定義

```csharp
private void InitializeFilters()
{
    try
    {
        // 準備選項清單
        var options = items.Select(i => new SelectOption
        {
            Text = i.DisplayName,
            Value = i.Id.ToString()
        }).ToList();

        // 建立篩選定義
        filterDefinitions = new SearchFilterBuilder<SearchFilterModel>()
            .AddSelect("FieldId", "欄位標籤", options)
            .AddText("SearchTerm", "搜尋", "請輸入搜尋關鍵字...")
            .AddDateRange("DateRange", "日期範圍")
            .Build();

        // 設定 CSS 類別（響應式佈局）
        SetFilterContainerCssClasses();
    }
    catch (Exception ex)
    {
        _ = ErrorHandlingHelper.HandlePageErrorAsync(
            ex,
            nameof(InitializeFilters),
            GetType(),
            additionalData: "初始化篩選器失敗"
        );
    }
}
```

### 2. 篩選事件處理

```csharp
private void HandleFilterChanged(SearchFilterModel filterModel)
{
    try
    {
        searchModel = filterModel;
        currentPage = 1; // 重設為第一頁
        ApplyFilters();
        StateHasChanged();
    }
    catch (Exception ex)
    {
        _ = ErrorHandlingHelper.HandlePageErrorAsync(
            ex,
            nameof(HandleFilterChanged),
            GetType(),
            additionalData: "處理篩選變更失敗"
        );
    }
}
```

## 表格實作模式

### 1. 欄位定義

```csharp
private void InitializeColumns()
{
    try
    {
        columnDefinitions = new List<TableColumnDefinition>
        {
            // 文字欄位
            new TableColumnDefinition
            {
                Title = "欄位標題",
                PropertyName = "PropertyName",
                DataType = ColumnDataType.Text,
                HeaderStyle = "width: 120px;",
                NullDisplayText = "-"
            },

            // 數字欄位
            new TableColumnDefinition
            {
                Title = "數量",
                PropertyName = "Quantity",
                DataType = ColumnDataType.Number,
                Format = "N0",
                HeaderCssClass = "text-end",
                CellCssClass = "text-end"
            },

            // 自訂範本欄位
            new TableColumnDefinition
            {
                Title = "狀態",
                PropertyName = "Status",
                DataType = ColumnDataType.Html,
                CustomTemplate = item => builder =>
                {
                    var entity = (EntityType)item;
                    builder.OpenElement(0, "span");
                    builder.AddAttribute(1, "class", $"badge bg-{GetStatusColor(entity.Status)}");
                    builder.AddContent(2, entity.StatusText);
                    builder.CloseElement();
                }
            }
        };
    }
    catch (Exception ex)
    {
        _ = ErrorHandlingHelper.HandlePageErrorAsync(
            ex,
            nameof(InitializeColumns),
            GetType(),
            additionalData: "初始化欄位定義失敗"
        );
    }
}
```

### 2. 分頁處理

```csharp
private void UpdatePagination()
{
    try
    {
        // 確保當前頁面在有效範圍內
        var totalPages = (int)Math.Ceiling((double)filteredItems.Count / pageSize);
        if (totalPages == 0) totalPages = 1;
        currentPage = Math.Min(currentPage, Math.Max(1, totalPages));
        
        // 計算當前頁面要顯示的資料
        pagedItems = filteredItems
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
    catch (Exception ex)
    {
        _ = ErrorHandlingHelper.HandlePageErrorAsync(
            ex,
            nameof(UpdatePagination),
            GetType(),
            additionalData: "更新分頁失敗"
        );
    }
}
```

## 樣式規範

### CSS 檔案規則

- 每個頁面如需額外 CSS，一律使用 `.razor.css` 檔案
- 檔案命名：`[頁面名稱].razor.css`
- 使用 CSS 隔離，避免全域樣式污染

### 範例結構

```
Components/Pages/Inventory/
├── InventoryOverviewIndex.razor
├── InventoryOverviewIndex.razor.css  # 頁面專用樣式
├── InventoryDetail.razor
└── InventoryDetail.razor.css
```

### CSS 內容範例

```css
/* InventoryOverviewIndex.razor.css */

/* 統計卡片樣式 */
.statistics-card {
    transition: transform 0.2s ease-in-out;
}

.statistics-card:hover {
    transform: translateY(-2px);
}

/* 表格列狀態顏色 */
.inventory-row.low-stock {
    background-color: #fff3cd;
}

.inventory-row.zero-stock {
    background-color: #f8d7da;
}

/* 響應式調整 */
@media (max-width: 768px) {
    .table-container {
        overflow-x: auto;
    }
}
```

## 最佳實踐

### 1. 權限安全

- **必須包含登入驗證**：每個頁面都要加 `@attribute [Authorize]`
- **必須包含功能權限**：使用 `PagePermissionCheck` 包裹頁面內容
- **權限命名一致**：遵循 `模組.動作` 格式
- **不要在中間件定義一般頁面**：只在 `PagePermissionCheck` 定義權限即可

### 2. 效能優化

- 使用 `StateHasChanged()` 明確控制 UI 更新
- 大量資料使用分頁載入
- 避免在 UI 執行緒中進行耗時操作

### 3. 使用者體驗

- 載入狀態顯示 (`isLoading`)
- 空資料狀態處理
- 錯誤訊息友善顯示

### 4. 程式碼組織

- 將複雜邏輯拆分為小方法
- 使用有意義的方法命名
- 保持方法的單一職責原則

### 5. 資料處理

- 避免直接修改原始資料清單
- 使用不同的清單管理篩選和分頁狀態
- 確保資料狀態的一致性

## 範例頁面分析

以 `InventoryOverviewIndex.razor` 為例：

1. **頁面結構**：使用 `GenericHeaderComponent` + 統計卡片 + 篩選器 + 表格
2. **資料流程**：基礎資料 → 篩選器初始化 → 主要資料載入 → 統計資料
3. **錯誤處理**：每個方法都有完整的 try-catch 結構
4. **元件使用**：大量使用共用元件，減少重複程式碼
5. **響應式設計**：使用 Bootstrap 網格系統和 CSS 隔離

這個範例展示了一個完整的客製版頁面應該如何實作，包含了所有必要的功能和最佳實踐。

## 新頁面開發範本

### 標準頁面範本

建立新頁面時，請使用以下範本確保包含所有必要的權限保護：

```razor
@page "/your-new-page"
@using Microsoft.AspNetCore.Authorization
@using ERPCore2.Components.Shared.Auth
@attribute [Authorize]
@inject IYourService YourService
@inject NavigationManager Navigation
@inject IJSRuntime JSRuntime
@using ERPCore2.Helpers
@rendermode InteractiveServer

<PageTitle>您的頁面標題</PageTitle>

<PagePermissionCheck RequiredPermission="YourModule.YourAction">
    <!-- 選擇適當的組件類型 -->
    
    <!-- 列表頁面 -->
    <GenericIndexPageComponent TEntity="YourEntity" 
                              TService="IYourService"
                              Service="@YourService"
                              EntityBasePath="/your-entities"
                              PageTitle="實體管理"
                              EntityName="實體"
                              BreadcrumbItems="@breadcrumbItems"
                              ... />
    
    <!-- 或詳細頁面 -->
    <GenericDetailPageComponent TEntity="YourEntity" 
                               TService="IYourService"
                               EntityId="@EntityId"
                               Entity="@entity"
                               Service="@YourService"
                               ... />
    
    <!-- 或編輯頁面 -->
    <GenericEditPageComponent TEntity="YourEntity" 
                             TService="IYourService"
                             Id="@Id"
                             Entity="@entity"
                             Service="@YourService"
                             ... />
</PagePermissionCheck>

@code {
    // 參數和變數定義
    [Parameter] public int? Id { get; set; }
    
    private YourEntity entity = new();
    private List<BreadcrumbItem> breadcrumbItems = new();
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            // 初始化邏輯
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(
                ex, 
                nameof(OnInitializedAsync), 
                GetType(),
                additionalData: new { PageName = "YourPageName" }
            );
        }
    }
    
    // 其他方法...
}
```

### 重要提醒

- **不需要**在 `PermissionCheckMiddleware.cs` 中定義一般業務頁面的路由
- 中間件只處理系統管理等高敏感頁面
- 一般頁面的權限完全由 `PagePermissionCheck` 處理

## 注意事項

1. **權限安全**：
   - 每個頁面都必須包含 `@attribute [Authorize]`
   - 每個頁面都必須使用 `PagePermissionCheck` 包裹內容
   - 權限命名必須遵循 `模組.動作` 格式
   - 不要在中間件中定義一般業務頁面的權限

2. **服務注入**：確保正確注入所需的服務

3. **導航管理**：重要錯誤需要包含 `Navigation` 參數

4. **記憶體管理**：適當處理大量資料的載入和釋放

5. **安全性**：確保資料存取權限和輸入驗證

6. **測試**：每個頁面都應該有對應的單元測試