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
AppDbContext（Customers / SalesDeliveries / SalesReturns 等）
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
| `Models/Charts/ChartDataItem.cs` | 通用圖表資料點 `ChartDataItem` + Drill-down 明細 `ChartDetailItem` |
| `Models/Charts/ChartIds.cs` | 各模組圖表 ID 常數 |
| `Models/Charts/ChartDefinition.cs` | `ChartDefinition` 模型 + `ChartCategory` 常數 + `ChartSeriesTypeInfo` |
| `Data/Charts/ChartRegistry.cs` | 全域圖表登記表（彙總器，只呼叫各模組的 `Register()`） |
| `Components/Shared/Chart/GenericChartModalComponent.razor` | 通用圖表 Modal（含圖表類型切換、Drill-down 明細面板） |
| `Components/Shared/Chart/GenericChartModalComponent.razor.css` | 圖表 Modal 專用 CSS（tab 按鈕樣式） |

### 客戶模組

| 路徑 | 說明 |
|------|------|
| `Services/Customers/ICustomerChartService.cs` | 圖表查詢 + Drill-down 明細查詢方法定義 |
| `Services/Customers/CustomerChartService.cs` | EF Core 查詢實作 |
| `Services/Customers/CustomerChartDefinitions.cs` | 客戶模組圖表定義（`Register()` 登記至 ChartRegistry） |
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
@using ERPCore2.Models.Charts
@using ERPCore2.Components.Shared.Table
@inject IServiceProvider ServiceProvider
```

模組包裝器（`CustomerChartModalComponent.razor`）頂端：
```razor
@using ERPCore2.Services.Customers
@inject ICustomerChartService ChartService
```

> `_Imports.razor` 已全域加入 `@using ERPCore2.Models.Charts` 與 `@using ERPCore2.Components.Shared.Chart`，
> 模組包裝器不需再重複引入這兩個 namespace。

### ApexChart 參數必須使用穩定物件參考（critical）

`ApexChart` 的 `Options` 與 `OnDataPointSelection` 若每次 re-render 傳入**新物件**，
`ApexChart` 會偵測到參數改變 → 呼叫 `chart.updateOptions()` 重新初始化 JS → **事件監聽器失效** → 後續點擊無反應。

`GenericChartModalComponent` 透過兩個快取字典解決此問題：

```csharp
// Options 快取：Key = "{ChartId}_{SeriesType int}"，金額圖與非金額圖各自快取
private readonly Dictionary<string, ApexChartOptions<ChartDataItem>> _optionsCache = new();

// Callback 快取：ChartId → 穩定的 EventCallback 實例（LoadDataAsync 時建立一次）
private readonly Dictionary<string, EventCallback<SelectedData<ChartDataItem>>> _callbackCache = new();
```

**Options 快取 Key 說明**：使用 `"{chartId}_{(int)type}"` 字串而非只用 SeriesType，
是因為同一個 SeriesType 在金額圖（`IsMoneyChart = true`）與非金額圖之間選項不同（Y 軸千分位格式），
必須分開快取避免共用到錯誤的 Options 物件。

**務必使用 `GetCachedOptions(chart, selectedType)` 而非每次建立新的 Options 物件。**

### DbContext 取得方式

使用 `IDbContextFactory`（與其他 Service 相同），每個方法獨立 context：

```csharp
using var context = await _factory.CreateDbContextAsync();
```

### EF Core GroupBy + Sum(運算式) 限制（重要）

當 LINQ GroupBy 的 group key 包含 JOIN 來的欄位，且 Sum() 內包含加法運算時，
EF Core 無法正確翻譯為 SQL，會導致只回傳一筆結果。

**解決方式：先 ToListAsync() 將資料載入記憶體，再用 LINQ to Objects 做 GroupBy：**

```csharp
// ❌ 錯誤：server-side GroupBy，JOIN 欄位在 group key 且 Sum 含運算式
var result = await (
    from sd in context.SalesDeliveries
    join c in context.Customers on sd.CustomerId equals c.Id
    group sd by new { c.Id, c.CompanyName } into g
    select new ChartDataItem {
        Label = g.Key.CompanyName,
        Value = g.Sum(x => x.TotalAmount + x.TaxAmount)  // ← 這裡會出問題
    }
).ToListAsync();

// ✅ 正確：先載入原始資料，再記憶體中 GroupBy
var rows = await (
    from sd in context.SalesDeliveries
    where sd.Status != EntityStatus.Deleted
    join c in context.Customers on sd.CustomerId equals c.Id
    select new { c.Id, c.CompanyName, c.Code, sd.TotalAmount, sd.TaxAmount }
).ToListAsync();

return rows
    .GroupBy(x => new { x.Id, x.CompanyName, x.Code })
    .Select(g => new ChartDataItem {
        Label = g.Key.CompanyName ?? g.Key.Code ?? $"ID:{g.Key.Id}",
        Value = g.Sum(x => x.TotalAmount + x.TaxAmount)
    })
    .OrderByDescending(x => x.Value)
    .Take(top)
    .ToList();
```

> 月份趨勢圖同樣適用：先 `.Select(sd => new { sd.DeliveryDate, sd.TotalAmount, sd.TaxAmount }).ToListAsync()`，
> 再記憶體 GroupBy 月份。

### 銷售金額來源：使用 SalesDelivery（出貨單）

- **`SalesDelivery`（出貨單）**：實際出貨、產生應收帳款，是銷售收入的正確來源
- **`SalesOrder`（銷售訂單）**：僅為承諾，尚未出貨即可存在，**不應作為收入統計基礎**
- 月銷售趨勢（`GetMonthlySalesTrendAsync`）與客戶銷售 Top 10（`GetTopCustomersBySalesAmountAsync`）均使用 `SalesDeliveries`
- 欄位注意：`SalesDelivery.TaxAmount`（非 `SalesTaxAmount`，與 SalesOrder 欄位名不同）

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

// Drill-down 明細項目（點擊圖表區塊後顯示的清單）
public class ChartDetailItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // 主要顯示欄位（預設欄位：名稱）
    public string? SubLabel { get; set; }               // 次要顯示欄位（預設欄位：代碼）
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

> `ChartDataItem` 與 `ChartDetailItem` 為跨模組共用，所有模組的 Chart Service 均回傳這兩個型別。
> 摘要模型（如 `CustomerChartSummary`）為各模組自行定義，放在 `Models/Charts/` 中。

位置：`Models/Charts/ChartDefinition.cs`

```csharp
public class ChartDefinition
{
    public string ChartId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public SeriesType DefaultSeriesType { get; set; } = SeriesType.Bar;
    public List<SeriesType> AllowedSeriesTypes { get; set; } = new();

    /// <summary>資料取得委派，由 IServiceProvider 解析服務</summary>
    public Func<IServiceProvider, Task<List<ChartDataItem>>> DataFetcher { get; set; } = null!;

    /// <summary>Drill-down 明細查詢委派（可選）；傳入被點擊的 Label，回傳對應明細清單</summary>
    public Func<IServiceProvider, string, Task<List<ChartDetailItem>>>? DetailFetcher { get; set; }

    /// <summary>Drill-down 明細表格欄位定義（可選）；若未設定則使用預設欄位（代碼 + 名稱）</summary>
    public List<InteractiveColumnDefinition>? DetailColumns { get; set; }

    /// <summary>是否為金額圖表（true 時 Y 軸顯示千分位格式）</summary>
    public bool IsMoneyChart { get; set; } = false;
}
```

---

## 6. Service 層設計

位置：`Services/[Module]/`

### 介面方法（以客戶為例）

```csharp
public interface ICustomerChartService
{
    // 圖表資料
    Task<List<ChartDataItem>> GetCustomersByAccountManagerAsync();
    Task<List<ChartDataItem>> GetCustomersByPaymentMethodAsync();
    Task<List<ChartDataItem>> GetCustomersByMonthAsync(int months = 12);
    Task<List<ChartDataItem>> GetCustomersByCustomerStatusAsync();
    Task<List<ChartDataItem>> GetCustomersByCreditLimitRangeAsync();
    Task<CustomerChartSummary> GetSummaryAsync();

    // 金錢數據圖表
    Task<List<ChartDataItem>> GetTopCustomersBySalesAmountAsync(int top = 10);
    Task<List<ChartDataItem>> GetMonthlySalesTrendAsync(int months = 12);
    Task<List<ChartDataItem>> GetCustomersByCurrentBalanceRangeAsync();
    Task<List<ChartDataItem>> GetTopCustomersByReturnAmountAsync(int top = 10);

    // Drill-down 明細查詢（對應各圖表 DetailFetcher）
    Task<List<ChartDetailItem>> GetCustomerDetailsByAccountManagerAsync(string label);
    Task<List<ChartDetailItem>> GetCustomerDetailsByPaymentMethodAsync(string label);
    Task<List<ChartDetailItem>> GetCustomerDetailsByStatusAsync(string label);
    Task<List<ChartDetailItem>> GetCustomerDetailsByCreditLimitRangeAsync(string label);
    Task<List<ChartDetailItem>> GetTopCustomerSalesOrderDetailsAsync(string customerLabel);
    Task<List<ChartDetailItem>> GetCustomersByCurrentBalanceRangeDetailsAsync(string label);
    Task<List<ChartDetailItem>> GetTopCustomerReturnDetailsAsync(string customerLabel);
}
```

> `label` 參數為使用者點擊的圖表區塊標籤，與 `ChartDataItem.Label` 一致。
> 月趨勢圖（`GetCustomersByMonthAsync`、`GetMonthlySalesTrendAsync`）不需要 Drill-down，無對應明細方法。

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
var rows = await context.SalesDeliveries
    .Where(sd => sd.Status != EntityStatus.Deleted && sd.DeliveryDate >= startDate)
    .Select(sd => new { sd.DeliveryDate, sd.TotalAmount, sd.TaxAmount })
    .ToListAsync();

// 記憶體中 GroupBy 月份（避免 EF Core GroupBy+Sum 翻譯問題）
var grouped = rows
    .GroupBy(x => new { x.DeliveryDate.Year, x.DeliveryDate.Month })
    .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Sum(x => x.TotalAmount + x.TaxAmount));

// 補零：每個月都要有一筆資料
var result = new List<ChartDataItem>();
for (int i = months - 1; i >= 0; i--)
{
    var date = now.AddMonths(-i);
    grouped.TryGetValue((date.Year, date.Month), out var amount);
    result.Add(new ChartDataItem { Label = date.ToString("yyyy/MM"), Value = amount });
}
```

---

## 7. 圖表定義架構（模組各自擁有）

圖表定義採**模組各自擁有**的設計：每個模組在自己的 `Services/[Module]/` 目錄下建立 `*ChartDefinitions.cs`，`ChartRegistry.cs` 只做彙總呼叫。

### ChartRegistry.cs（彙總器，不含任何圖表定義）

位置：`Data/Charts/ChartRegistry.cs`

```csharp
public static class ChartRegistry
{
    private static readonly List<ChartDefinition> _definitions = new();
    private static bool _initialized = false;
    private static readonly object _lock = new();

    public static void EnsureInitialized() { ... }  // 雙檢查鎖定，執行緒安全

    private static void Initialize()
    {
        CustomerChartDefinitions.Register(_definitions);
        // VendorChartDefinitions.Register(_definitions);    // 未來廠商圖表
        // InventoryChartDefinitions.Register(_definitions); // 未來庫存圖表
    }

    public static List<ChartDefinition> GetByCategory(string category) { ... }
}
```

### 模組圖表定義檔（以客戶為例）

位置：`Services/Customers/CustomerChartDefinitions.cs`

```csharp
public static class CustomerChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        // 此模組所有圖表在此定義，以 definitions.Add() 登記
    }
}
```

### ChartDefinition 關鍵欄位

```csharp
public class ChartDefinition
{
    public string ChartId { get; set; }                                // 唯一 ID（如 "CU001"）
    public string Title { get; set; }                                  // 頁籤標題
    public string Category { get; set; }                               // ChartCategory 常數
    public int SortOrder { get; set; }                                 // 頁籤排序
    public SeriesType DefaultSeriesType { get; set; }                  // 預設圖表類型
    public List<SeriesType> AllowedSeriesTypes { get; set; }           // 使用者可切換的類型
    public bool IsMoneyChart { get; set; }                             // Y 軸千分位格式（金額圖）
    public Func<IServiceProvider, Task<List<ChartDataItem>>> DataFetcher { get; set; }               // 圖表資料來源（必填）
    public Func<IServiceProvider, string, Task<List<ChartDetailItem>>>? DetailFetcher { get; set; }  // Drill-down 明細（選填）
    public List<InteractiveColumnDefinition>? DetailColumns { get; set; }                            // 明細欄位定義（選填，覆寫預設欄位）
}
```

### 定義範例（均寫在模組的 `Register()` 方法內）

```csharp
// 一般圖表（含 Drill-down，使用預設明細欄位：代碼 + 名稱）
definitions.Add(new ChartDefinition
{
    ChartId            = ChartIds.CustomerByAccountManager,   // "CU001"
    Title              = "依業務負責人分布",
    Category           = ChartCategory.Customer,
    SortOrder          = 1,
    DefaultSeriesType  = SeriesType.Donut,
    AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar, SeriesType.Treemap, SeriesType.PolarArea },
    DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByAccountManagerAsync(),
    DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetCustomerDetailsByAccountManagerAsync(label)
});

// 金額圖表（IsMoneyChart = true → Y 軸千分位；自訂明細欄位）
definitions.Add(new ChartDefinition
{
    ChartId            = ChartIds.CustomerTopSalesByAmount,
    Title              = "客戶銷售金額排行 Top 10",
    Category           = ChartCategory.Customer,
    SortOrder          = 6,
    DefaultSeriesType  = SeriesType.Bar,
    AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Treemap },
    IsMoneyChart       = true,
    DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetTopCustomersBySalesAmountAsync(),
    DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetTopCustomerSalesOrderDetailsAsync(label),
    DetailColumns      = new()
    {
        new() { Title = "出貨日期", PropertyName = "Name",     ColumnType = InteractiveColumnType.Display },
        new() { Title = "含稅金額", PropertyName = "SubLabel", ColumnType = InteractiveColumnType.Display, Width = "130px", TextAlign = "right" }
    }
});

// 時間序列圖（不支援 Drill-down，省略 DetailFetcher）
definitions.Add(new ChartDefinition
{
    ChartId            = ChartIds.CustomerMonthlyTrend,
    Title              = "每月新增趨勢",
    Category           = ChartCategory.Customer,
    SortOrder          = 3,
    DefaultSeriesType  = SeriesType.Line,
    AllowedSeriesTypes = new() { SeriesType.Line, SeriesType.Area },
    DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByMonthAsync()
    // DetailFetcher 省略 → 不顯示點擊提示，不支援 Drill-down
});
```

### 可用 SeriesType（Group A，Label/Value 通用格式）

| SeriesType | 中文名稱 | 適用資料 |
|-----------|---------|---------|
| `Bar` | 長條圖 | 分類計數 / 金額排行 |
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

| ID | 常數 | 標題 | 預設類型 | IsMoneyChart | Drill-down | 自訂欄位 |
|----|------|------|---------|:------------:|:----------:|:--------:|
| CU001 | `CustomerByAccountManager` | 依業務負責人分布 | Donut | ❌ | ✅ | ❌ |
| CU002 | `CustomerByPaymentMethod` | 依付款方式分布 | Bar | ❌ | ✅ | ❌ |
| CU003 | `CustomerMonthlyTrend` | 每月新增趨勢 | Line | ❌ | ❌ | - |
| CU004 | `CustomerByStatus` | 客戶狀態分布 | Pie | ❌ | ✅ | ❌ |
| CU005 | `CustomerByCreditLimit` | 信用額度分布 | Bar | ❌ | ✅ | ❌ |
| CU006 | `CustomerTopSalesByAmount` | 客戶銷售金額排行 Top 10 | Bar | ✅ | ✅ | ✅（出貨日期+含稅金額）|
| CU007 | `CustomerMonthlySalesTrend` | 每月銷售收入趨勢 | Area | ✅ | ❌ | - |
| CU008 | `CustomerByCurrentBalance` | 應收餘額分布 | Bar | ✅ | ✅ | ✅（公司名稱+應收餘額）|
| CU009 | `CustomerTopReturnsByAmount` | 客戶退貨金額排行 Top 10 | Bar | ✅ | ✅ | ✅（退回日期+含稅金額）|

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

### Drill-down 行為

`GenericChartModalComponent` 內建 Drill-down 功能，無需模組包裝器做任何額外設定：

- **有 `DetailFetcher`**：圖表下方顯示「點擊圖表區塊可查看明細」提示；點擊後載入明細並以 `InteractiveTableComponent` 呈現
- **有 `DetailColumns`**：使用自訂欄位定義（如「出貨日期」+「含稅金額」）；若未設定則使用預設欄位（代碼 + 名稱兩欄）
- **無 `DetailFetcher`**：點擊圖表不觸發任何動作
- 明細面板有 X 關閉鈕；切換頁籤或切換圖表類型時自動關閉
- 快速連續點擊不同區塊時，以 `_detailLoadSeq` 防止非同步競爭，確保只呈現最後一次點擊的結果

### 圖表類型切換機制

`GenericChartModalComponent` 內部維護每個圖表的當前 SeriesType。
切換時用 `@key` 強制重新建立 ApexChart 組件（清除舊資料、重新渲染）：
```razor
<ApexChart @key="@($"{chart.ChartId}_{selectedType}")"
           TItem="ChartDataItem" ...>
```

### 金額圖表千分位格式

當 `ChartDefinition.IsMoneyChart = true` 時，Y 軸自動套用千分位格式（JavaScript `toLocaleString()`）：

```csharp
private static readonly List<YAxis> _moneyYAxis = new()
{
    new YAxis
    {
        Labels = new YAxisLabels
        {
            Formatter = "function(val) { return val == null ? '' : Number(val).toLocaleString(); }"
        }
    }
};
```

Options 快取以 `"{chartId}_{seriesType}"` 為 key，確保金額圖與非金額圖的 Options 物件分開快取。

---

## 9. ApexCharts 語法速查

### 基本結構

```razor
@using ApexCharts   ← 只在 GenericChartModalComponent.razor 頂端

<ApexChart TItem="ChartDataItem"
           Title="圖表標題"
           Options="@GetCachedOptions(chart, selectedType)"
           OnDataPointSelection="@(_callbackCache.GetValueOrDefault(chart.ChartId))">
    <ApexPointSeries TItem="ChartDataItem"
                     Items="@data"
                     Name="系列名稱"
                     SeriesType="@selectedType"
                     XValue="@(e => e.Label)"
                     YValue="@(e => (decimal?)e.Value)" />
</ApexChart>
```

> `YValue` 必須回傳 `decimal?`（可為 null）

> **重要**：`Options` 必須傳入快取物件（`GetCachedOptions(chart, selectedType)`），`OnDataPointSelection` 必須傳入快取的 `EventCallback`（`_callbackCache`）。
> 若每次 render 傳入新物件，ApexChart 會重新初始化 JS 導致事件監聽器失效，後續點擊完全無反應。

### BuildDefaultOptions 內建設定

`GenericChartModalComponent` 的 `BuildDefaultOptions(SeriesType, isMoneyChart)` 依圖表類型自動套用合適設定，
結果被快取於 `_optionsCache`，透過 `GetCachedOptions(chart, type)` 取得：

| SeriesType | 自動套用設定 | 支援 IsMoneyChart |
|-----------|------------|:----------------:|
| `Donut` | Donut Size 65%，Legend Bottom | ❌ |
| `Pie` | Legend Bottom | ❌ |
| `Bar` | ColumnWidth 55%，無 Legend | ✅（Y 軸千分位）|
| `Line` | Smooth 曲線，Markers Size 5，無 Legend | ✅（Y 軸千分位）|
| `Area` | Smooth 曲線，無 Legend | ✅（Y 軸千分位）|
| `Treemap` | 無 Legend | ❌ |
| `PolarArea` | Legend Bottom | ❌ |
| `Radar` | 無 Legend | ❌ |
| `RadialBar` | Legend Bottom | ❌ |

> Pie / Donut / Treemap / PolarArea / Radar / RadialBar 本身以百分比或面積呈現，Y 軸千分位不適用。

### Tab 按鈕樣式

圖表頁籤使用自訂按鈕群組樣式（非 Bootstrap nav-tabs），CSS 定義於 `GenericChartModalComponent.razor.css`：

```razor
<div class="chart-tab-btn-group">
    @for (int i = 0; i < _charts.Count; i++)
    {
        var idx = i;
        var chart = _charts[i];
        <button type="button"
                class="chart-tab-btn @(_activeChartIndex == idx ? "active" : "")"
                @onclick="() => SetActiveChart(idx)">
            @chart.Title
        </button>
    }
</div>
```

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

// 若需要 Drill-down，一併新增明細方法
Task<List<ChartDetailItem>> GetCustomerDetailsByRegionAsync(string label);
```

### (2) 在 Service 實作查詢

```csharp
// CustomerChartService.cs
public async Task<List<ChartDataItem>> GetCustomersByRegionAsync()
{
    using var context = await _factory.CreateDbContextAsync();
    // ... EF Core 查詢，回傳 List<ChartDataItem>
}

public async Task<List<ChartDetailItem>> GetCustomerDetailsByRegionAsync(string label)
{
    using var context = await _factory.CreateDbContextAsync();
    // ChartDetailItem.SubLabel = 客戶代碼，ChartDetailItem.Name = 客戶名稱
}
```

### (3) 在 ChartIds.cs 新增常數

```csharp
public const string CustomerByRegion = "CU010";
```

### (4) 在 CustomerChartDefinitions.cs 的 `Register()` 內新增

位置：`Services/Customers/CustomerChartDefinitions.cs`

```csharp
definitions.Add(new ChartDefinition
{
    ChartId            = ChartIds.CustomerByRegion,
    Title              = "依地區分布",
    Category           = ChartCategory.Customer,
    SortOrder          = 10,
    DefaultSeriesType  = SeriesType.Bar,
    AllowedSeriesTypes = new() { SeriesType.Bar, SeriesType.Pie, SeriesType.Donut, SeriesType.Treemap },
    DataFetcher        = sp => sp.GetRequiredService<ICustomerChartService>().GetCustomersByRegionAsync(),
    DetailFetcher      = (sp, label) => sp.GetRequiredService<ICustomerChartService>().GetCustomerDetailsByRegionAsync(label)
    // 若不需要 Drill-down，省略 DetailFetcher 即可
});
```

完成。`GenericChartModalComponent` 會自動讀取新登記的圖表並顯示頁籤，不需修改 UI 組件或 `ChartRegistry.cs`。

---

## 12. 如何為其他模組新增圖表

以新增「廠商分析圖表」為例，完整步驟：

### 1. 建立 Service

```
Services/Suppliers/ISupplierChartService.cs
Services/Suppliers/SupplierChartService.cs
```

圖表資料方法回傳 `List<ChartDataItem>`；Drill-down 明細方法回傳 `List<ChartDetailItem>`，無需建立新的資料模型。

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

### 4. 建立 SupplierChartDefinitions.cs

位置：`Services/Suppliers/SupplierChartDefinitions.cs`

```csharp
using ApexCharts;
using ERPCore2.Models.Charts;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Services.Suppliers;

public static class SupplierChartDefinitions
{
    public static void Register(List<ChartDefinition> definitions)
    {
        definitions.Add(new ChartDefinition
        {
            ChartId            = ChartIds.SupplierByCategory,
            Title              = "依分類分布",
            Category           = ChartCategory.Supplier,
            SortOrder          = 1,
            DefaultSeriesType  = SeriesType.Donut,
            AllowedSeriesTypes = new() { SeriesType.Donut, SeriesType.Pie, SeriesType.Bar },
            DataFetcher        = sp => sp.GetRequiredService<ISupplierChartService>().GetSuppliersByCategoryAsync(),
            DetailFetcher      = (sp, label) => sp.GetRequiredService<ISupplierChartService>().GetSupplierDetailsByCategoryAsync(label)
        });
        // ... 其他廠商圖表
    }
}
```

然後在 `Data/Charts/ChartRegistry.cs` 的 `Initialize()` 加入一行：

```csharp
private static void Initialize()
{
    CustomerChartDefinitions.Register(_definitions);
    SupplierChartDefinitions.Register(_definitions);   // ← 新增這行
}
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

*最後更新：2026-02-28（ChartRegistry 重構為模組各自擁有圖表定義）*
