# 業務員欄位拆分與業績目標管理功能設計

> 本文件記錄「業務員 ≠ 製表者」問題的修正方案，以及後續業績目標管理（KPI）模組的完整設計規劃。

---

## 一、背景與問題說明

### 現況問題

目前銷售流程中，「業務員」與「製表者（打單人員）」是同一個欄位，造成以下問題：

| 單據 | 欄位 | Entity Display | UI 顯示標籤 | 語義矛盾 |
|------|------|---------------|------------|---------|
| 報價單 | `EmployeeId` | 業務人員 | 製表者 | ✅ 矛盾 |
| 銷貨訂單 | `EmployeeId` | 員工 | 業務員 | — |
| 出貨單 | `EmployeeId` | 員工 | 員工 | — |

**轉單邏輯的錯誤傳遞**（`SalesOrderEditModalComponent.razor:1952`）：

```csharp
// 報價單轉銷貨訂單時，把「製表者」誤當「業務員」複製
salesOrder.EmployeeId = quotation.EmployeeId;
```

**圖表計算基準錯誤**（`SalesChartService.cs`）：

SA002「業務員業績排行」目前使用 `SalesDelivery.EmployeeId`（出貨操作員），而非 `SalesOrder.EmployeeId`（業務員），導致業績歸屬不正確。

### 根本原因

在小公司環境下，制表者通常即是業務員，兩者角色重疊，因此初期設計未做區分。隨著使用規模擴大，需要正式拆分兩個概念。

---

## 二、資料模型修改

### 2-1. Quotation（報價單）新增 `SalespersonId`

**檔案**：`Data/Entities/Sales/Quotation.cs`

```csharp
// 新增：業務員（負責拿下這張案子的人）
[Display(Name = "業務員")]
[ForeignKey(nameof(Salesperson))]
public int? SalespersonId { get; set; }

// 原有：製表者（負責輸入這張單據的人）
[Display(Name = "製表者")]
[ForeignKey(nameof(Employee))]
public int? EmployeeId { get; set; }

// 新增導覽屬性
public Employee? Salesperson { get; set; }
public Employee? Employee { get; set; }    // 原有，保留
```

**欄位語義區分**：

| 欄位 | 意義 | 必填 |
|------|------|------|
| `SalespersonId` | 負責業務開發、與客戶洽談的業務員 | 否（可不填） |
| `EmployeeId` | 在系統內建立此報價單的操作人員 | 否 |

### 2-2. SalesOrder（銷貨訂單）不需新增欄位

`SalesOrder.EmployeeId` 標籤已是「業務員」（`Field.SalesPerson`），語義正確，保留不動。

### 2-3. 資料庫 Migration

```
新增欄位：Quotations.SalespersonId  int?  FK → Employees(Id)
建立索引：IX_Quotations_SalespersonId
```

> 舊資料 `SalespersonId` 預設為 `null`，不影響現有功能。

---

## 三、轉單邏輯修正

**檔案**：`Components/Pages/Sales/SalesOrderEditModalComponent.razor`（約第 1952 行）

```csharp
// 修改前（錯誤）
salesOrder.EmployeeId = quotation.EmployeeId;  // 製表者 → 業務員

// 修改後（正確）
salesOrder.EmployeeId = quotation.SalespersonId;  // 業務員 → 業務員
```

> **注意**：刻意不 Fallback 到 `EmployeeId`（製表者）。若報價單未填業務員，轉出來的訂單業務員欄位為 `null`，需由使用者手動補填，避免將錯誤的人誤植為業務員。

---

## 四、UI 修改（報價單編輯 Modal）

**檔案**：`Components/Pages/Sales/QuotationEditModalComponent.razor`

### 4-1. 新增 AutoComplete 設定

```csharp
autoCompleteConfig = new AutoCompleteConfigBuilder<Quotation>()
    .AddField(nameof(Quotation.CustomerId),    "CompanyName", availableCustomers)
    .AddField(nameof(Quotation.CompanyId),     "CompanyName", availableCompanies)
    .AddField(nameof(Quotation.SalespersonId), "Name",        availableEmployees) // 新增
    .AddField(nameof(Quotation.EmployeeId),    "Name",        availableEmployees)
    .Build();
```

### 4-2. 新增 ModalManager

```csharp
modalManagers = ModalManagerInitHelper.CreateBuilder<Quotation, IQuotationService>(...)
    .AddManager<Employee>(nameof(Quotation.SalespersonId), "業務員")  // 新增
    .AddManager<Employee>(nameof(Quotation.EmployeeId),    "製表者")  // 原有，重新標記
    .AddManager<Customer>(nameof(Quotation.CustomerId),   "客戶")
    .AddManager<Company> (nameof(Quotation.CompanyId),    "公司")
    .Build();
```

### 4-3. 新增表單欄位

在 `InitializeFormFieldsAsync()` 的 `EmployeeId`（製表者）欄位之前插入：

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Quotation.SalespersonId),
    Label        = L["Field.SalesPerson"],
    FieldType    = FormFieldType.AutoComplete,
    Placeholder  = L["Placeholder.PleaseInputOrSelect", L["Field.SalesPerson"]],
    IsRequired   = false,
    MinSearchLength = 0,
    HelpText     = "負責此次業務洽談的業務員",
    ActionButtons = await GetSalespersonActionButtonsAsync(),
    IsReadOnly   = shouldLock
},
```

### 4-4. FormSection 調整

```csharp
.AddToSection(FormSectionNames.BasicInfo,
    q => q.Code,
    q => q.CustomerId,
    q => q.CompanyId,
    q => q.SalespersonId,  // 新增（排在製表者之前）
    q => q.EmployeeId,
    q => q.ProjectName,
    q => q.QuotationDate)
```

---

## 五、圖表查詢修正

**檔案**：`Services/Sales/SalesChartService.cs`

### SA002 — 業務員業績排行修正

```csharp
// 修改前（錯誤：使用出貨單的員工）
from d in context.SalesDeliveries
join e in context.Employees on d.EmployeeId equals e.Id into eg

// 修改後（正確：透過訂單取業務員）
from d  in context.SalesDeliveries
join dd in context.SalesDeliveryDetails  on d.Id  equals dd.SalesDeliveryId
join od in context.SalesOrderDetails     on dd.SalesOrderDetailId equals od.Id
join o  in context.SalesOrders           on od.SalesOrderId equals o.Id
join e  in context.Employees             on o.EmployeeId    equals e.Id into eg
from emp in eg.DefaultIfEmpty()
group new { d.TotalAmount, d.TaxAmount } by
    (emp != null ? emp.Name : "未分配") into g
```

同步修正 `GetDeliveryDetailsByEmployeeAsync()` 的 JOIN 路徑，使 Drill-down 明細與排行榜一致。

### 注意事項

修正後，歷史資料中「出貨員 ≠ 業務員」的訂單，業績歸屬將重新計算，圖表數字會改變。**這是預期中的修正行為，不是 Bug。**

---

## 六、業績目標管理（KPI）模組設計

以上修正完成後，業務員欄位已正確，可開始開發 KPI 模組。

### 6-1. 新增 Entity

**檔案**：`Data/Entities/Sales/SalesTarget.cs`

```csharp
public class SalesTarget : BaseEntity
{
    [Required]
    public int Year { get; set; }           // 年度，例如 2026

    [Required]
    [Range(0, 12)]
    public int Month { get; set; }          // 月份，0 = 年度目標，1~12 = 月度目標

    public int? EmployeeId { get; set; }    // null = 公司整體目標
    public int? CustomerId { get; set; }    // null = 不限客戶
    public int? ProductId  { get; set; }    // null = 不限商品

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TargetAmount { get; set; }

    // 導覽屬性
    public Employee? Employee { get; set; }
    public Customer? Customer { get; set; }
    public Product?  Product  { get; set; }
}
```

**設計說明**：三個維度（員工 / 客戶 / 商品）皆可獨立設定，不強制同時填寫：

| 情境 | EmployeeId | CustomerId | ProductId |
|------|-----------|-----------|----------|
| 公司整體目標 | null | null | null |
| 業務員月度目標 | 有值 | null | null |
| 特定客戶目標 | null | 有值 | null |
| 特定商品目標 | null | null | 有值 |

### 6-2. Service 設計

**檔案**：`Services/Sales/ISalesTargetService.cs` / `SalesTargetService.cs`

```csharp
public interface ISalesTargetService : IGenericManagementService<SalesTarget>
{
    // 取得特定期間的目標清單
    Task<List<SalesTarget>> GetByPeriodAsync(int year, int? month = null);

    // 計算業務員達成率列表（目標 + 實績合併）
    Task<List<SalesAchievementItem>> GetEmployeeAchievementListAsync(int year, int? month = null);

    // 批次新增/更新（整年 12 個月一次設定）
    Task<ServiceResult> UpsertBatchAsync(List<SalesTarget> targets);

    // 複製上一年度目標（快速初始化）
    Task<ServiceResult> CopyFromPreviousYearAsync(int targetYear);
}

// 達成率計算結果模型
public class SalesAchievementItem
{
    public int?    EmployeeId    { get; set; }
    public string  EmployeeName  { get; set; } = string.Empty;
    public decimal TargetAmount  { get; set; }
    public decimal ActualAmount  { get; set; }
    public decimal AchievementRate => TargetAmount > 0
        ? Math.Round(ActualAmount / TargetAmount * 100, 1)
        : 0;
    public bool    IsOnTrack     => AchievementRate >= 80;
}
```

**業績計算基準**：以 `SalesDelivery.DeliveryDate`（出貨日期）為準，金額為 `TotalAmount + TaxAmount`（含稅）。業務員來源為 `SalesOrder.EmployeeId`（透過 JOIN 取得）。

### 6-3. 圖表擴充

新增兩個圖表定義（`Services/Sales/SalesChartDefinitions.cs`）：

| ChartId | 標題 | 類型 | 說明 |
|---------|------|------|------|
| `SA006` | 目標 vs 實績月度趨勢 | 雙折線 | 全公司每月目標線 vs 實績線 |
| `SA007` | 業務員達成率排行 | 水平長條 | 含達成率 % 標籤，由高到低排列 |

SA006 需要 `GenericChartModalComponent` 支援多 Series（詳見下方架構升級章節）。

### 6-4. 儀表板 Widget

在 `NavigationConfig.cs` Sales 區段新增：

```csharp
new NavigationItem
{
    Name             = "業績達成率",
    NameKey          = "Nav.SalesAchievement",
    IsChartWidget    = true,
    IconClass        = "bi bi-bullseye",
    ItemType         = NavigationItemType.Action,
    ActionId         = "OpenSalesAchievement",
    Category         = "銷售管理",
    ModuleKey        = "Charts",
    RequiredPermission = PermissionRegistry.SalesTarget.Read,
    SearchKeywords   = new List<string> { "業績達成率", "KPI", "目標管理", "sales target" }
},
```

Widget 開啟後顯示 `SalesAchievementModalComponent.razor`，內容：
- 本月 / 本年整體進度條（目標 vs 實績）
- SA006 目標 vs 實績折線圖
- SA007 業務員達成率排行

### 6-5. 目標設定頁

路由：`/sales-targets`，加入 NavigationConfig Sales 子選單。

**UI 設計（矩陣式編輯）**：

```
┌──────────────────────────────────────────────────────────────────┐
│ 業績目標設定           年度: [2026▼]   [+ 新增業務員]  [複製去年] │
├──────────────┬────┬────┬────┬─────┬────┬──────────────────────────┤
│ 業務員        │ 1月 │ 2月 │ 3月 │ ... │12月│ 年度合計                │
├──────────────┼────┼────┼────┼─────┼────┼──────────────────────────┤
│ 王小明        │ 50 │ 50 │ 60 │ ... │ 70 │ 720 萬                   │
│ 陳大華        │ 30 │ 30 │ 35 │ ... │ 40 │ 420 萬                   │
├──────────────┼────┼────┼────┼─────┼────┼──────────────────────────┤
│ 公司整體      │200 │200 │220 │ ... │240 │ 2,400 萬                 │
└──────────────┴────┴────┴────┴─────┴────┴──────────────────────────┘
```

- 數字欄位 Inline 直接編輯
- 「複製去年目標」按鈕：呼叫 `CopyFromPreviousYearAsync()`
- 儲存：呼叫 `UpsertBatchAsync()`

---

## 七、架構升級：GenericChartModalComponent 多 Series 支援

SA006 需要在同一圖表中渲染兩條折線（目標 + 實績），現有架構僅支援單 Series。

### 7-1. ChartDefinition 擴充

**檔案**：`Models/Charts/ChartDefinition.cs`

```csharp
// 新增模型
public class ChartSeriesData
{
    public string SeriesName   { get; set; } = string.Empty;
    public List<ChartDataItem> Items { get; set; } = new();
    public bool IsDashed       { get; set; } = false;   // 目標線顯示為虛線
}

// ChartDefinition 新增屬性
public class ChartDefinition
{
    // 現有單 Series（保留不動）
    public Func<IServiceProvider, Task<List<ChartDataItem>>>? DataFetcher { get; set; }

    // 新增多 Series（設定後自動切換至多 Series 渲染模式）
    public Func<IServiceProvider, Task<List<ChartSeriesData>>>? MultiSeriesDataFetcher { get; set; }

    public bool IsMultiSeries => MultiSeriesDataFetcher != null;
}
```

### 7-2. GenericChartModalComponent 渲染分支

**檔案**：`Components/Shared/Chart/GenericChartModalComponent.razor`

```razor
@if (chart.IsMultiSeries)
{
    // 多 Series 渲染（目標 vs 實績）
    var multiData = _multiData.GetValueOrDefault(chart.ChartId, new());
    <ApexChart TItem="ChartDataItem" Options="@GetMultiSeriesOptions(chart)">
        @foreach (var series in multiData)
        {
            <ApexPointSeries TItem="ChartDataItem"
                             Items="@series.Items"
                             Name="@series.SeriesName"
                             SeriesType="SeriesType.Line"
                             XValue="@(e => e.Label)"
                             YValue="@(e => (decimal?)e.Value)" />
        }
    </ApexChart>
}
else
{
    // 原有單 Series 渲染（不變）
}
```

**向下相容**：SA001~SA005 使用 `DataFetcher`，完全不受影響。

---

## 八、ChartModalHost 重構（MainLayout 優化）

目前 MainLayout 每加一個圖表 Widget 需要在四個地方修改。重構後統一由 `ChartModalHost` 管理。

### 8-1. 新增 ChartModalHost.razor

```csharp
// 對外只暴露一個方法
public void Open(string category, RenderFragment? summaryContent = null)
{
    _currentCategory = category;
    _summaryContent  = summaryContent;
    _isVisible       = true;
    StateHasChanged();
}
```

### 8-2. MainLayout 簡化

```csharp
// 修改前：每個 Widget 三行
private bool showSalesCharts = false;
actionRegistry.Register("OpenSalesCharts", () => { showSalesCharts = true; StateHasChanged(); });
public void OpenSalesCharts() { showSalesCharts = true; StateHasChanged(); }

// 修改後：每個 Widget 一行
actionRegistry.Register("OpenSalesCharts",      () => chartModalHost.Open("Sales"));
actionRegistry.Register("OpenSalesAchievement", () => chartModalHost.Open("SalesAchievement"));
```

---

## 九、權限設計

新增 `SalesTarget` 模組權限：

| Permission Key | 說明 | 建議角色 |
|---------------|------|---------|
| `SalesTarget.Read` | 查看業績目標與達成率圖表 | 業務員、主管 |
| `SalesTarget.Write` | 新增 / 修改業績目標 | 主管、管理員 |

---

## 十、實作順序

```
Phase 1 — 資料修正（先做，影響所有後續）
  ├── Quotation.cs 新增 SalespersonId 欄位
  ├── 建立 Migration
  ├── QuotationEditModalComponent 新增業務員欄位
  ├── SalesOrderEditModalComponent 修正轉單邏輯
  └── SalesChartService SA002 修正 JOIN 路徑

Phase 2 — 架構升級
  ├── ChartDefinition 新增 MultiSeriesDataFetcher
  ├── GenericChartModalComponent 新增多 Series 分支
  └── ChartModalHost 重構 MainLayout

Phase 3 — KPI 功能開發
  ├── SalesTarget Entity + Migration
  ├── ISalesTargetService / SalesTargetService
  ├── /sales-targets 目標設定頁
  ├── SA006 / SA007 圖表定義與 DataFetcher
  ├── SalesAchievementModalComponent
  └── NavigationConfig 新增 Widget + 目標設定頁入口
```

---

## 十一、開工前確認事項

- [ ] 確認「製表者」與「業務員」在實際作業中是否真的不同人（已確認：需分開）
- [ ] 確認業績計算基準：以**出貨日期**計算（與現有 SA002/SA003 一致）
- [ ] 確認目標設定頁的存取權限（`SalesTarget.Write`，僅主管可編輯）
- [ ] 確認 SA002 圖表數字變動是否需要對使用者說明（建議在版本記錄中標註「業績歸屬修正」）

---

*最後更新：2026-03-13*
