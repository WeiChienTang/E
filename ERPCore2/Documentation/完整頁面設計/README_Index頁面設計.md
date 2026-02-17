# Index 頁面設計

## 更新日期
2026-02-17

---

## 概述

Index 頁面是每個業務實體的列表檢視，使用 `GenericIndexPageComponent<TEntity, TService>` 提供搜尋、篩選、表格顯示、以及與 EditModal 的整合。每個 Index 頁面搭配一個 `FieldConfiguration` 類別來定義篩選器與表格欄位。

---

## 檔案結構

每個業務實體的 Index 頁面需要兩個檔案：

| 檔案 | 路徑 | 說明 |
|------|------|------|
| Index 頁面 | `Components/Pages/{Module}/{Entity}Index.razor` | 列表頁面 |
| 欄位配置 | `Components/FieldConfiguration/{Entity}FieldConfiguration.cs` | 篩選與表格欄位定義 |

---

## FieldConfiguration 設計

### 繼承 BaseFieldConfiguration

```csharp
using ERPCore2.Components.FieldConfiguration;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;

namespace ERPCore2.Components.FieldConfiguration
{
    public class YourEntityFieldConfiguration : BaseFieldConfiguration<YourEntity>
    {
        private readonly INotificationService? _notificationService;

        public YourEntityFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<YourEntity>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<YourEntity>>
                {
                    {
                        nameof(YourEntity.Code),
                        new FieldDefinition<YourEntity>
                        {
                            PropertyName = nameof(YourEntity.Code),
                            DisplayName = "編號",
                            FilterPlaceholder = "輸入編號搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) =>
                                FilterHelper.ApplyTextContainsFilter(
                                    model, query, nameof(YourEntity.Code), e => e.Code)
                        }
                    },
                    {
                        nameof(YourEntity.Name),
                        new FieldDefinition<YourEntity>
                        {
                            PropertyName = nameof(YourEntity.Name),
                            DisplayName = "名稱",
                            FilterPlaceholder = "輸入名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) =>
                                FilterHelper.ApplyTextContainsFilter(
                                    model, query, nameof(YourEntity.Name), e => e.Name)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(
                        ex, nameof(GetFieldDefinitions), GetType());
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化欄位配置時發生錯誤");
                    });
                }

                return new Dictionary<string, FieldDefinition<YourEntity>>();
            }
        }
    }
}
```

### FieldDefinition 屬性

| 屬性 | 說明 |
|------|------|
| `PropertyName` | 實體屬性名稱（使用 `nameof()`） |
| `DisplayName` | 顯示名稱（用於表頭與篩選標籤） |
| `FilterPlaceholder` | 篩選欄位提示文字 |
| `TableOrder` | 表格欄位排列順序 |
| `HeaderStyle` | 表頭樣式（如 `"width: 180px;"`） |
| `FilterFunction` | 篩選函數 |
| `ShowInTable` | 是否顯示在表格中（預設 `true`） |
| `ShowInFilter` | 是否顯示在篩選區域（預設 `true`） |
| `ValueGetter` | 自訂值顯示邏輯（如 `e => e.Customer?.CompanyName`） |

### FilterHelper 篩選類型

```csharp
// 文字包含篩選
FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
    model, query, nameof(Entity.Code), e => e.Code)

// 允許 null 的文字篩選
FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
    model, query, nameof(Entity.TaxNumber), e => e.TaxNumber, allowNull: true)

// 數值篩選
FilterFunction = (model, query) => FilterHelper.ApplyNumericFilter(
    model, query, nameof(Entity.Price), e => e.Price)

// 日期篩選
FilterFunction = (model, query) => FilterHelper.ApplyDateFilter(
    model, query, nameof(Entity.OrderDate), e => e.OrderDate)

// 外鍵關聯篩選
FilterFunction = (model, query) => FilterHelper.ApplyForeignKeyFilter(
    model, query, nameof(Entity.CustomerId), e => e.CustomerId)
```

---

## Index 頁面設計

### 完整範例

```razor
@page "/your-entities"
@inject IYourEntityService YourEntityService
@rendermode InteractiveServer
@inject INotificationService NotificationService

<GenericIndexPageComponent TEntity="YourEntity"
                          TService="IYourEntityService"
                          Service="@YourEntityService"
                          EntityBasePath="/your-entities"
                          PageTitle="實體管理"
                          PageSubtitle="管理所有實體資料"
                          EntityName="實體"
                          BreadcrumbItems="@breadcrumbItems"
                          FilterDefinitions="@filterDefinitions"
                          ColumnDefinitions="@columnDefinitions"
                          DataLoader="@LoadDataAsync"
                          FilterApplier="@ApplyFilters"
                          GetEntityDisplayName="@(e => e.Name)"
                          RequiredPermission="YourEntity.Read"
                          OnAddClick="@modalHandler.ShowAddModalAsync"
                          OnRowClick="@modalHandler.ShowEditModalAsync"
                          @ref="indexComponent" />

<YourEntityEditModalComponent IsVisible="@showEditModal"
                              IsVisibleChanged="@((bool v) => showEditModal = v)"
                              YourEntityId="@editingId"
                              OnYourEntitySaved="@modalHandler.OnEntitySavedAsync"
                              OnCancel="@modalHandler.OnModalCancelAsync" />

@code {
    // 組件參考
    private GenericIndexPageComponent<YourEntity, IYourEntityService> indexComponent = default!;

    // Modal 狀態
    private bool showEditModal = false;
    private int? editingId = null;

    // Modal 處理器
    private ModalHandler<YourEntity, GenericIndexPageComponent<YourEntity, IYourEntityService>> modalHandler = default!;

    // 欄位配置
    private YourEntityFieldConfiguration fieldConfiguration = default!;
    private List<SearchFilterDefinition> filterDefinitions = new();
    private List<TableColumnDefinition> columnDefinitions = new();
    private List<BreadcrumbItem> breadcrumbItems = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // 1. 初始化 Modal 處理器
            modalHandler = ModalHelper.CreateModalHandler<YourEntity,
                GenericIndexPageComponent<YourEntity, IYourEntityService>>(
                id => editingId = id,
                visible => showEditModal = visible,
                () => indexComponent,
                StateHasChanged,
                GetType());

            // 2. 初始化麵包屑
            await InitializeBreadcrumbsAsync();

            // 3. 建立欄位配置
            fieldConfiguration = new YourEntityFieldConfiguration(NotificationService);

            // 4. 建立篩選器和表格欄位
            filterDefinitions = fieldConfiguration.BuildFilters();
            columnDefinitions = fieldConfiguration.BuildColumns();
        }
        catch (Exception ex)
        {
            await ErrorHandlingHelper.HandlePageErrorAsync(
                ex, nameof(OnInitializedAsync), GetType(),
                additionalData: "初始化頁面失敗");
            await NotificationService.ShowErrorAsync("初始化頁面失敗");
        }
    }

    // 麵包屑
    private async Task InitializeBreadcrumbsAsync() =>
        breadcrumbItems = await BreadcrumbHelper.CreateSimpleAsync(
            "實體管理", NotificationService, GetType());

    // 資料載入
    private Task<List<YourEntity>> LoadDataAsync() =>
        DataLoaderHelper.LoadAsync(
            () => YourEntityService.GetAllAsync(),
            "實體",
            NotificationService,
            GetType());

    // 篩選邏輯
    private IQueryable<YourEntity> ApplyFilters(
        SearchFilterModel searchModel, IQueryable<YourEntity> query)
    {
        return fieldConfiguration.ApplyFilters(
            searchModel, query, nameof(ApplyFilters), GetType());
    }
}
```

---

## Helper 使用說明

### ModalHelper - Modal 狀態管理

```csharp
// 建立 Modal 處理器
modalHandler = ModalHelper.CreateModalHandler<TEntity,
    GenericIndexPageComponent<TEntity, TService>>(
    id => editingId = id,           // 設定編輯 ID
    visible => showEditModal = visible, // 設定 Modal 顯示狀態
    () => indexComponent,           // 取得 Index 組件參考
    StateHasChanged,                // 狀態變更回呼
    GetType());                     // 呼叫者類型

// 使用
OnAddClick="@modalHandler.ShowAddModalAsync"     // 新增
OnRowClick="@modalHandler.ShowEditModalAsync"     // 編輯（傳入 ID）
OnSaved="@modalHandler.OnEntitySavedAsync"        // 儲存後重新載入
OnCancel="@modalHandler.OnModalCancelAsync"       // 取消
```

### BreadcrumbHelper - 麵包屑導航

```csharp
// 兩層（首頁 > 頁面名稱）
breadcrumbItems = await BreadcrumbHelper.CreateSimpleAsync(
    "客戶管理", NotificationService, GetType());

// 三層（首頁 > 模組 > 頁面）
breadcrumbItems = await BreadcrumbHelper.CreateThreeLevelAsync(
    "基本資料", "客戶管理", "/master-data", NotificationService, GetType());

// 自訂層級
breadcrumbItems = await BreadcrumbHelper.InitializeAsync(
    new[] {
        new BreadcrumbItem("基本資料", "/master-data"),
        new BreadcrumbItem("客戶管理")
    },
    NotificationService, GetType());
```

### DataLoaderHelper - 資料載入

```csharp
private Task<List<YourEntity>> LoadDataAsync() =>
    DataLoaderHelper.LoadAsync(
        () => YourEntityService.GetAllAsync(),  // 載入函數
        "實體名稱",                              // 用於錯誤訊息
        NotificationService,                    // 通知服務
        GetType());                             // 呼叫者類型
```

---

## 開發檢查清單

- [ ] 建立 `{Entity}FieldConfiguration.cs`，繼承 `BaseFieldConfiguration<T>`
- [ ] 使用 `nameof()` 定義 `PropertyName`
- [ ] 設定 `FilterFunction` 使用 `FilterHelper`
- [ ] 建立 Index 頁面，使用 `GenericIndexPageComponent`
- [ ] 使用 `ModalHelper.CreateModalHandler` 管理 Modal
- [ ] 使用 `BreadcrumbHelper` 初始化麵包屑
- [ ] 使用 `DataLoaderHelper.LoadAsync` 載入資料
- [ ] 篩選邏輯委派給 `fieldConfiguration.ApplyFilters()`

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md) - 總綱
- [README_EditModal設計.md](README_EditModal設計.md) - EditModal 設計
