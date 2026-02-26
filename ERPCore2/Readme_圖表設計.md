# ERPCore2 圖表設計文件

> 本文件記錄 ERPCore2 圖表系統的完整設計，
> 供日後新增、修改或擴充其他模組圖表時參照。

---

## 目錄

1. [使用的圖表函式庫](#1-使用的圖表函式庫)
2. [整體架構](#2-整體架構)
3. [檔案清單](#3-檔案清單)
4. [關鍵注意事項](#4-關鍵注意事項)
5. [資料模型](#5-資料模型)
6. [Service 層設計](#6-service-層設計)
7. [ChartRegistry 登記方式](#7-chartregistry-登記方式)
8. [GenericChartModalComponent 使用方式](#8-genericchartmodalcomponent-使用方式)
9. [ApexCharts 語法速查](#9-apexcharts-語法速查)
10. [導覽整合方式](#10-導覽整合方式)
11. [如何新增圖表（同模組）](#11-如何新增圖表同模組)
12. [如何為其他模組新增圖表](#12-如何為其他模組新增圖表)

---

## 1. 使用的圖表函式庫

**Blazor-ApexCharts** v6.1.0（NuGet）

| 項目 | 說明 |
|------|------|
| NuGet ID | `Blazor-ApexCharts` |
| GitHub | https://github.com/apexcharts/Blazor-ApexCharts |
| 特點 | 原生 Blazor 整合，不需手寫 JS interop |
| 支援 SeriesType | Bar / Pie / Donut / Line / Area / Treemap / PolarArea / Radar / RadialBar 等 |

### 安裝與設定

```csharp
// ERPCore2.csproj
<PackageReference Include="Blazor-ApexCharts" Version="6.1.0" />

// Program.cs（必須加 using + AddApexCharts）
using ApexCharts;
builder.Services.AddApexCharts();
```

> **重要**：`@using ApexCharts` **不可**放在 `_Imports.razor`，
> 因為 `ApexCharts.Size` 會與 `ERPCore2.Data.Entities.Size`（商品尺寸實體）衝突。
> 必須只在需要直接操作 ApexCharts API 的 `.razor` 組件頂端個別加入。
> `GenericChartModalComponent.razor` 已包含此 using，模組包裝器（如 `CustomerChartModalComponent.razor`）不需再加。

---

## 2. 整體架構

圖表系統類比報表系統（ReportRegistry / ReportDefinition / GenericReportIndexPage）：

```
導覽列點擊「客戶圖表」
  ↓
NavigationConfig.cs（ActionId = "OpenCustomerCharts"）
  ↓
MainLayout.razor（註冊 handler → showCustomerCharts = true）
  ↓
CustomerChartModalComponent.razor（薄包裝器，提供 SummaryContent）
  ↓
GenericChartModalComponent.razor（通用 Modal，讀取 ChartRegistry）
  ↓
ChartRegistry.GetByCategory("Customer")（取得 ChartDefinition 清單）
  ↓
ChartDefinition.DataFetcher(IServiceProvider)（呼叫對應 Service 方法）
  ↓
ICustomerChartService / CustomerChartService（EF Core 查詢）
  ↓
AppDbContext（Customers / Employees / PaymentMethods）
```

### 架構對照表

| 報表系統 | 圖表系統 |
|---------|---------|
| `ReportIds.cs` | `ChartIds.cs` |
| `ReportDefinition.cs` + `ReportCategory` | `ChartDefinition.cs` + `ChartCategory` |
| `ReportRegistry.cs` | `ChartRegistry.cs` |
| `GenericReportIndexPage.razor` | `GenericChartModalComponent.razor` |
| `[Entity]ReportService` | `[Entity]ChartService` |

---

## 3. 檔案清單

### 核心框架（通用，不因模組而異）

| 路徑 | 說明 |
|------|------|
| `Models/Charts/ChartDataItem.cs` | 通用圖表資料點 `ChartDataItem` |
| `Models/Charts/ChartIds.cs` | 各模組圖表 ID 常數 |
| `Models/Charts/ChartDefinition.cs` | `ChartDefinition` 模型 + `ChartCategory` 常數 + `ChartSeriesTypeInfo` |
| `Data/Charts/ChartRegistry.cs` | 全域圖表登記表 |
| `Components/Shared/Chart/GenericChartModalComponent.razor` | 通用圖表 Modal（含圖表類型切換） |

### 客戶模組

| 路徑 | 說明 |
|------|------|
| `Services/Customers/ICustomerChartService.cs` | 6 個查詢方法定義 |
| `Services/Customers/CustomerChartService.cs` | EF Core 查詢實作 |
| `Components/Pages/Customers/CustomerChartModalComponent.razor` | 薄包裝器（摘要卡片 + 呼叫 GenericChartModalComponent） |
| `Data/ServiceRegistration.cs` | DI 容器中的 Service 註冊 |
| `Data/Navigation/NavigationConfig.cs` | 客戶群組下的 Action Item |
| `Components/Layout/MainLayout.razor` | state 變數 + handler + Modal 標記 |
| `Resources/SharedResource*.resx`（5 個） | `Nav.CustomerCharts` 翻譯鍵 |

---

## 4. 關鍵注意事項

### 命名衝突（必讀）

```
❌ _Imports.razor 中不可加 @using ApexCharts
✅ 只在 GenericChartModalComponent.razor 頂端有 @using ApexCharts
✅ 模組包裝器（CustomerChartModalComponent.razor 等）不需要加
```

原因：`ApexCharts` 套件內有 `Size` 型別，與 `ERPCore2.Data.Entities.Size`（商品尺寸）同名，
若全域引入會導致 `ProductEditModalComponent`、`SizeIndex` 等商品頁面編譯錯誤。

### `@using` 放置位置

`GenericChartModalComponent.razor` 頂端：
```razor
@using ApexCharts                          ← 只在這裡
@using ERPCore2.Data.Charts
@inject IServiceProvider ServiceProvider
```

模組包裝器（`CustomerChartModalComponent.razor`）頂端：
```razor
@using ERPCore2.Models.Charts              ← 已由 _Imports.razor 全域引入，可省略
@using ERPCore2.Services.Customers
@inject ICustomerChartService ChartService
```

> `_Imports.razor` 已全域加入 `@using ERPCore2.Models.Charts` 與 `@using ERPCore2.Components.Shared.Chart`，
> 模組包裝器不需再重複引入這兩個 namespace。

### DbContext 取得方式

使用 `IDbContextFactory`（與其他 Service 相同），每個方法獨立 context：

```csharp
using var context = await _factory.CreateDbContextAsync();
```

### 資料篩選規則

- **排除已刪除**：`c.Status != EntityStatus.Deleted`
- **僅啟用**：`c.Status == EntityStatus.Active`
- **已停用**：`c.Status == EntityStatus.Inactive`
- Customer 沒有 `IsDeleted` / `IsActive` 欄位，用 `EntityStatus` enum 判斷

---

## 5. 資料模型

位置：`Models/Charts/ChartDataItem.cs`

```csharp
// 通用圖表資料點（一筆 = 一個長條 / 一個扇形 / 一個折線點）
public class ChartDataItem
{
    public string Label { get; set; } = string.Empty;  // X 軸標籤或圓餅名稱
    public decimal Value { get; set; }                  // 數值
}

// 客戶模組專用摘要卡片資料（不通用）
public class CustomerChartSummary
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public int CustomersThisMonth { get; set; }
    public decimal? AverageCreditLimit { get; set; }
}
```

> `ChartDataItem` 為跨模組共用，所有模組的 Chart Service 均回傳 `List<ChartDataItem>`。
> 摘要模型（如 `CustomerChartSummary`）為各模組自行定義，放在 `Models/Charts/` 中。

---

## 6. Service 層設計

位置：`Services/[Module]/`

### 介面方法（以客戶為例）

```csharp
public interface ICustomerChartService
{
    Task<List<ChartDataItem>> GetCustomersByAccountManagerAsync();   // 業務負責人
    Task<List<ChartDataItem>> GetCustomersByPaymentMethodAsync();    // 付款方式
    Task<List<ChartDataItem>> GetCustomersByMonthAsync(int months);  // 每月趨勢
    Task<List<ChartDataItem>> GetCustomersByActiveStatusAsync();     // 啟用狀態
    Task<List<ChartDataItem>> GetCustomersByCreditLimitRangeAsync(); // 信用額度
    Task<CustomerChartSummary> GetSummaryAsync();                    // 摘要（可選）
}
```

### JOIN 查詢模式（左外連接）

當需要 join 可為 null 的關聯表（如 EmployeeId 可為空），使用 LINQ 左外連接：

```csharp
var result = await (
    from c in context.Customers
    where c.Status != EntityStatus.Deleted
    join e in context.Employees on c.EmployeeId equals e.Id into eg
    from emp in eg.DefaultIfEmpty()   // DefaultIfEmpty = LEFT JOIN
    select new { Name = emp != null && emp.Name != null ? emp.Name : "未分配" }
)
.GroupBy(x => x.Name)
.Select(g => new { Label = g.Key, Value = (decimal)g.Count() })
.ToListAsync();
```

### 月份趨勢補零模式

DB 查詢後在記憶體補齊缺少的月份，避免折線圖出現跳躍：

```csharp
var raw = await context.Customers
    .Where(c => c.Status != EntityStatus.Deleted && c.CreatedAt >= startDate)
    .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
    .Select(g => new { g.Key.Year, g.Key.Month, Count = (decimal)g.Count() })
    .ToListAsync();

// 補零：每個月都要有一筆資料
for (int i = months - 1; i >= 0; i--)
{
    var date = now.AddMonths(-i);
    var found = raw.FirstOrDefault(d => d.Year == date.Year && d.Month == date.Month);
    result.Add(new ChartDataItem { Label = date.ToString("yyyy/MM"), Value = found?.Count ?? 0 });
}
```

---

## 7. ChartRegistry 登記方式

位置：`Data/Charts/ChartRegistry.cs`

### ChartDefinition 關鍵欄位

```csharp
public class ChartDefinition
{
    public string ChartId { get; set; }                               // 唯一 ID（如 "CU001"）
    public string Title { get; set; }                                 // 頁籤標題
    public string Category { get; set; }                              // ChartCategory 常數
    public int SortOrder { get; set; }                                // 頁籤排序
    public SeriesType DefaultSeriesType { get; set; }                 // 預設圖表類型
    public List<SeriesType> AllowedSeriesTypes { get; set; }          // 使用者可切換的類型
    public Func<IServiceProvider, Task<List<ChartDataItem>>> DataFetcher { get; set; }  // 資料來源
}
```

### 登記範例（客戶圖表）

```csharp
_definitions.Add(new ChartDefinition
{
    ChartId            = ChartIds.CustomerByAccountManager,   // "CU001"
    Title              = "依業務負責人分布",
    Category           = ChartCategory.Customer,
    SortOrder          = 1,
    DefaultSeriesType  = SeriesType.Donut,
    AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.Treemap, SeriesType.PolarArea },
    DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByAccountManagerAsync()
});
```

### 可用 SeriesType（Group A，Label/Value 通用格式）

| SeriesType | 中文名稱 | 適用資料 |
|-----------|---------|---------|
| `Bar` | 長條圖 | 分類計數 |
| `Pie` | 圓餅圖 | 分類計數 |
| `Donut` | 甜甜圈圖 | 分類計數 |
| `Line` | 折線圖 | 時間序列 |
| `Area` | 面積圖 | 時間序列 |
| `Treemap` | 樹狀圖 | 分類計數 |
| `PolarArea` | 極區圖 | 分類計數 |
| `Radar` | 雷達圖 | 多維比較 |
| `RadialBar` | 儀表圖 | 比例/進度 |

> 時間序列資料（如每月趨勢）只允許 `Line` 和 `Area`，其他類型不適用。

### 現有客戶圖表 ID

| ID | 常數 | 標題 | 預設類型 |
|----|------|------|---------|
| CU001 | `ChartIds.CustomerByAccountManager` | 依業務負責人分布 | Donut |
| CU002 | `ChartIds.CustomerByPaymentMethod` | 依付款方式分布 | Bar |
| CU003 | `ChartIds.CustomerMonthlyTrend` | 每月新增趨勢 | Line |
| CU004 | `ChartIds.CustomerByStatus` | 啟用/停用狀態 | Pie |
| CU005 | `ChartIds.CustomerByCreditLimit` | 信用額度分布 | Bar |

---

## 8. GenericChartModalComponent 使用方式

位置：`Components/Shared/Chart/GenericChartModalComponent.razor`

### 參數

| 參數 | 型別 | 說明 |
|------|------|------|
| `IsVisible` | `bool` | Modal 開關 |
| `IsVisibleChanged` | `EventCallback<bool>` | 雙向綁定 |
| `Category` | `string` | `ChartCategory` 常數，決定顯示哪些圖表 |
| `Title` | `string` | Modal 標題（預設「分析圖表」） |
| `Icon` | `string` | Bootstrap Icon class（預設 `bi bi-bar-chart-fill`） |
| `SummaryContent` | `RenderFragment?` | 可選，顯示於頁籤上方的摘要區塊 |
| `OnDataLoading` | `EventCallback` | Modal 開啟時通知父組件同步載入（如摘要卡片） |

### 薄包裝器寫法（以客戶為例）

```razor
@using ERPCore2.Services.Customers
@inject ICustomerChartService ChartService

<GenericChartModalComponent IsVisible="@IsVisible"
                            IsVisibleChanged="@IsVisibleChanged"
                            Category="@ChartCategory.Customer"
                            Title="客戶分析圖表"
                            Icon="bi bi-bar-chart-fill"
                            OnDataLoading="@LoadSummaryAsync">
    <SummaryContent>
        @* 摘要卡片（此模組專屬，不通用） *@
        <div class="row g-3 mb-4">
            <div class="col-6 col-md-3">
                <div class="card text-center border-0 shadow-sm h-100">
                    <div class="card-body py-3">
                        <div class="fs-2 fw-bold text-primary">@(_summary?.TotalCustomers ?? 0)</div>
                        <div class="text-muted small">客戶總數</div>
                    </div>
                </div>
            </div>
            @* ... 其他卡片 *@
        </div>
    </SummaryContent>
</GenericChartModalComponent>

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    private CustomerChartSummary? _summary;

    private async Task LoadSummaryAsync()
    {
        _summary = await ChartService.GetSummaryAsync();
        StateHasChanged();  // 通知 Blazor 更新 SummaryContent
    }
}
```

### 圖表類型切換機制

`GenericChartModalComponent` 內部維護：
```csharp
private Dictionary<string, SeriesType> _selectedTypes; // chartId → 目前選擇的 SeriesType
```

切換時用 `@key` 強制重新建立 ApexChart 組件（清除舊資料、重新渲染）：
```razor
<ApexChart @key="@($"{chart.ChartId}_{selectedType}")"
           TItem="ChartDataItem" ...>
```

---

## 9. ApexCharts 語法速查

### 基本結構

```razor
@using ApexCharts   ← 只在 GenericChartModalComponent.razor 頂端

<ApexChart TItem="ChartDataItem"
           Title="圖表標題"
           Options="@options">
    <ApexPointSeries TItem="ChartDataItem"
                     Items="@data"
                     Name="系列名稱"
                     SeriesType="SeriesType.Donut"
                     XValue="@(e => e.Label)"
                     YValue="@(e => (decimal?)e.Value)" />
</ApexChart>
```

> `YValue` 必須回傳 `decimal?`（可為 null）

### GetDefaultOptions 內建設定

`GenericChartModalComponent` 的 `GetDefaultOptions(SeriesType)` 方法依圖表類型自動套用合適設定：

| SeriesType | 自動套用設定 |
|-----------|------------|
| `Donut` | Donut Size 65%，Legend Bottom |
| `Pie` | Legend Bottom |
| `Bar` | ColumnWidth 55%，無 Legend |
| `Line` | Smooth 曲線，Markers Size 5，無 Legend |
| `Area` | Smooth 曲線，無 Legend |
| `Treemap` | 無 Legend |
| `PolarArea` | Legend Bottom |
| `Radar` | 無 Legend |
| `RadialBar` | Legend Bottom |

---

## 10. 導覽整合方式

圖表功能透過 Navigation Action Item 觸發，與報表集、系統參數等相同模式：

### Step 1：NavigationConfig.cs 新增 Action Item

```csharp
NavigationActionHelper.CreateActionItem(
    name: "客戶圖表",
    description: "依多維度查看客戶統計分析圖表",
    iconClass: "bi bi-bar-chart-fill",
    actionId: "OpenCustomerCharts",          // 唯一 ID
    category: "客戶關係管理",
    requiredPermission: "Customer.Read",
    searchKeywords: new List<string> { "客戶圖表", "customer chart" },
    nameKey: "Nav.CustomerCharts"            // i18n key
),
```

### Step 2：MainLayout.razor 加入三處

```csharp
// (1) state 變數
private bool showCustomerCharts = false;

// (2) OnInitializedAsync 中註冊
actionRegistry.Register("OpenCustomerCharts", OpenCustomerCharts);

// (3) handler 方法
[JSInvokable]
public void OpenCustomerCharts()
{
    try { showCustomerCharts = true; StateHasChanged(); }
    catch { }
}
```

```razor
@* (4) markup 中加入 Modal 標記 *@
<CustomerChartModalComponent IsVisible="@showCustomerCharts"
                             IsVisibleChanged="@((bool visible) => showCustomerCharts = visible)" />
```

### Step 3：多語系 key

在 5 個 `SharedResource*.resx` 中加入 `Nav.CustomerCharts`：
- `SharedResource.resx` (zh-TW)：`客戶圖表`
- `SharedResource.en-US.resx`：`Customer Charts`
- `SharedResource.ja-JP.resx`：`顧客グラフ`
- `SharedResource.zh-CN.resx`：`客户图表`
- `SharedResource.fil.resx`：`Tsart ng Customer`

---

## 11. 如何新增圖表（同模組）

以在客戶模組新增「依地區分布」圖表為例：

### (1) 在 Service 介面新增方法

```csharp
// ICustomerChartService.cs
Task<List<ChartDataItem>> GetCustomersByRegionAsync();
```

### (2) 在 Service 實作查詢

```csharp
// CustomerChartService.cs
public async Task<List<ChartDataItem>> GetCustomersByRegionAsync()
{
    using var context = await _factory.CreateDbContextAsync();
    // ... EF Core 查詢，回傳 List<ChartDataItem>
}
```

### (3) 在 ChartIds.cs 新增常數

```csharp
public const string CustomerByRegion = "CU006";
```

### (4) 在 ChartRegistry.cs 登記

```csharp
_definitions.Add(new ChartDefinition
{
    ChartId            = ChartIds.CustomerByRegion,
    Title              = "依地區分布",
    Category           = ChartCategory.Customer,
    SortOrder          = 6,
    DefaultSeriesType  = SeriesType.Bar,
    AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut, SeriesType.Treemap },
    DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByRegionAsync()
});
```

完成。`GenericChartModalComponent` 會自動讀取新登記的圖表並顯示頁籤，不需修改 UI 組件。

---

## 12. 如何為其他模組新增圖表

以新增「廠商分析圖表」為例，完整步驟：

### 1. 建立 Service

```
Services/Suppliers/ISupplierChartService.cs
Services/Suppliers/SupplierChartService.cs
```

回傳型別統一為 `List<ChartDataItem>`，無需建立新的資料模型。

### 2. 在 ServiceRegistration.cs 註冊

```csharp
services.AddScoped<ISupplierChartService, SupplierChartService>();
```

### 3. 在 ChartIds.cs 新增常數

```csharp
public const string SupplierByCategory = "SU001";
public const string SupplierMonthlyTrend = "SU002";
// ...
```

### 4. 在 ChartRegistry.cs 登記

```csharp
_definitions.Add(new ChartDefinition
{
    ChartId            = ChartIds.SupplierByCategory,
    Title              = "依分類分布",
    Category           = ChartCategory.Supplier,
    SortOrder          = 1,
    DefaultSeriesType  = SeriesType.Donut,
    AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar },
    DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetSuppliersByCategoryAsync()
});
```

### 5. 建立薄包裝器

```
Components/Pages/Suppliers/SupplierChartModalComponent.razor
```

```razor
@using ERPCore2.Services.Suppliers
@inject ISupplierChartService ChartService

<GenericChartModalComponent IsVisible="@IsVisible"
                            IsVisibleChanged="@IsVisibleChanged"
                            Category="@ChartCategory.Supplier"
                            Title="廠商分析圖表"
                            Icon="bi bi-bar-chart-fill">
    @* SummaryContent 為可選，若無摘要卡片可省略整個 SummaryContent 參數 *@
</GenericChartModalComponent>

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
}
```

### 6. 加入導覽（NavigationConfig.cs）

```csharp
NavigationActionHelper.CreateActionItem(
    name: "廠商圖表",
    actionId: "OpenSupplierCharts",
    category: "廠商管理",
    nameKey: "Nav.SupplierCharts",
    // ...
),
```

### 7. 整合 MainLayout.razor

```csharp
private bool showSupplierCharts = false;
actionRegistry.Register("OpenSupplierCharts", OpenSupplierCharts);

public void OpenSupplierCharts()
{
    try { showSupplierCharts = true; StateHasChanged(); }
    catch { }
}
```

```razor
<SupplierChartModalComponent IsVisible="@showSupplierCharts"
                             IsVisibleChanged="@((bool visible) => showSupplierCharts = visible)" />
```

### 8. 新增多語系 key

在 5 個 `.resx` 檔加入 `Nav.SupplierCharts`。

---

*最後更新：2026-02-26*
