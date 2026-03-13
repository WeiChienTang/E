# 業務員欄位拆分與業績目標管理功能設計

> 本文件記錄「業務員 ≠ 製表者」問題的修正方案，以及後續業績目標管理（KPI）模組的完整設計規劃。

---

## 一、業務流程說明

### 情況一：有業務員的銷售流程

```
報價單 (Quotation)
  ├── 業務員：負責與客戶洽談、拿下訂單的人
  └── 製表者：辦公室人員，將報價內容輸入系統
        ↓ 轉單
銷貨訂單 (SalesOrder)
  ├── 業務員：從報價單帶入，可調整
  └── 製表者：建立訂單的辦公室人員
        ↓ 完成排程後
銷貨單 (SalesDelivery)
  ├── 業務員：從訂單帶入
  └── 製表者：執行銷貨作業的辦公室人員
```

### 情況二：客戶直接到店購買（無業務員）

```
銷貨單 (SalesDelivery)
  ├── 業務員：null（無業務，列入「其他」）
  └── 製表者：處理此次交易的辦公室人員
```

---

## 二、欄位設計原則

**三張主單全部同時具備兩個欄位**，無例外：

| 單據 | `SalespersonId`（業務員） | `EmployeeId`（製表者） |
|------|--------------------------|----------------------|
| Quotation（報價單） | 談成生意的業務員 | 建立報價單的人員 |
| SalesOrder（銷貨訂單） | 從報價單帶入，可修改 | 建立訂單的人員 |
| SalesDelivery（銷貨單） | 從訂單帶入；無訂單則 null | 執行銷貨的人員 |

> 目前系統仍在測試階段，無歷史資料包袱，可進行完整的欄位重構。

---

## 三、資料模型修改

### 3-1. 三張 Entity 統一新增 `SalespersonId`

**Quotation.cs**、**SalesOrder.cs**、**SalesDelivery.cs** 各自新增：

```csharp
// 新增：業務員
[Display(Name = "業務員")]
[ForeignKey(nameof(Salesperson))]
public int? SalespersonId { get; set; }

// 原有 EmployeeId：重新定位為「製表者」
[Display(Name = "製表者")]
[ForeignKey(nameof(Employee))]
public int? EmployeeId { get; set; }

// 新增導覽屬性
public Employee? Salesperson { get; set; }
```

### 3-2. 資料庫 Migration

三張表各新增一欄：

```sql
ALTER TABLE Quotations    ADD SalespersonId INT NULL REFERENCES Employees(Id);
ALTER TABLE SalesOrders   ADD SalespersonId INT NULL REFERENCES Employees(Id);
ALTER TABLE SalesDeliveries ADD SalespersonId INT NULL REFERENCES Employees(Id);
```

> 建議合併為一個 Migration：`AddSalespersonIdToSalesTables`

---

## 四、各單據 UI 修改

### 共用原則

每張單據的表單都需要：
1. 新增「業務員」欄位（`SalespersonId`，AutoComplete，非必填）
2. 原有 `EmployeeId` 欄位標籤改為「製表者」
3. `AutoCompleteConfig` 加入 `SalespersonId` mapping
4. `ModalManager` 加入 `SalespersonId` Manager
5. `FormFieldLockHelper.LockMultipleFields` 加入 `SalespersonId`
6. `FormSection` 的 BasicInfo 加入 `SalespersonId`（排在製表者之前）

### QuotationEditModalComponent.razor

| 項目 | 修改內容 |
|------|---------|
| 現有 `EmployeeId` 標籤 | `Field.QuotationCreator`（製表者）維持不變 |
| 新增 `SalespersonId` 欄位 | 標籤 `Field.SalesPerson`，排在製表者之前 |
| ModalManager | 新增 `SalespersonId` → "業務員" |

### SalesOrderEditModalComponent.razor

| 項目 | 修改內容 |
|------|---------|
| 現有 `EmployeeId` 標籤 | 改為 `Field.DocumentCreator`（製表者）|
| 新增 `SalespersonId` 欄位 | 標籤 `Field.SalesPerson` |
| ModalManager | 新增 `SalespersonId` → "業務員" |

### SalesDeliveryEditModalComponent.razor

| 項目 | 修改內容 |
|------|---------|
| 現有 `EmployeeId` 標籤 | 改為 `Field.DocumentCreator`（製表者）|
| 新增 `SalespersonId` 欄位 | 標籤 `Field.SalesPerson`，情況二可不填 |
| ModalManager | 新增 `SalespersonId` → "業務員" |

---

## 五、轉單邏輯

### 報價單 → 銷貨訂單

**檔案**：`SalesOrderEditModalComponent.razor`（`ShowAddModalWithPrefilledQuotation`）

```csharp
// 修改前（錯誤：把製表者當業務員）
salesOrder.EmployeeId = quotation.EmployeeId;

// 修改後
salesOrder.SalespersonId = quotation.SalespersonId;  // 業務員帶入
salesOrder.EmployeeId    = currentUserId;             // 製表者 = 當前操作人員
```

### 銷貨訂單 → 銷貨單

建立 SalesDelivery 時帶入：

```csharp
salesDelivery.SalespersonId = salesOrder.SalespersonId;  // 業務員帶入
salesDelivery.EmployeeId    = currentUserId;              // 製表者 = 當前操作人員
```

> 情況二（直接建立銷貨單）：`SalespersonId` = null，`EmployeeId` = 操作人員。

---

## 六、報表修正

### QuotationReportService.cs

```csharp
// 修改前（讀製表者，但列印標籤寫「業務員」）
employee = await _employeeService.GetByIdAsync(quotation.EmployeeId.Value);

// 修改後（分別讀取兩欄）
var salesperson  = quotation.SalespersonId.HasValue
    ? await _employeeService.GetByIdAsync(quotation.SalespersonId.Value) : null;
var documentCreator = quotation.EmployeeId.HasValue
    ? await _employeeService.GetByIdAsync(quotation.EmployeeId.Value) : null;

// 列印時分兩行顯示
("業務員", salesperson?.Name ?? ""),
("製表者", documentCreator?.Name ?? "")
```

同理修正：`SalesOrderReportService.cs`、`SalesDeliveryReportService.cs`

---

## 七、圖表修正（SA002）

原本 SA002「業務員業績排行」使用 `SalesDelivery.EmployeeId`（製表者），**現在直接改用新欄位，不需要跨表 JOIN**：

**檔案**：`Services/Sales/SalesChartService.cs`

```csharp
// 修改前（錯誤：使用製表者）
join e in context.Employees on d.EmployeeId equals e.Id into eg

// 修改後（正確：直接使用業務員）
join e in context.Employees on d.SalespersonId equals e.Id into eg
from emp in eg.DefaultIfEmpty()
// SalespersonId = null → emp = null → 歸入「其他」
group new { d.TotalAmount, d.TaxAmount } by
    (emp != null ? emp.Name : "其他") into g
```

同步修正 `GetDeliveryDetailsByEmployeeAsync()`，查詢條件改用 `SalespersonId`。

---

## 八、CustomerSalesAnalysis 報表修正（現有 Bug）

**檔案**：`Services/Reports/CustomerSalesAnalysisReportService.cs`

`criteria.EmployeeIds` 篩選在服務層完全未被執行，需要補實作：

```csharp
// 補上業務員篩選（依 SalesDelivery.SalespersonId）
if (criteria.EmployeeIds.Any())
{
    deliveries = deliveries
        .Where(d => d.SalespersonId.HasValue &&
                    criteria.EmployeeIds.Contains(d.SalespersonId.Value))
        .ToList();
}
```

---

## 九、KPI 業績歸屬規則

| 銷貨單來源 | `SalespersonId` | KPI 歸屬 |
|-----------|----------------|---------|
| 情況一（報價 → 訂單 → 銷貨） | 有值（業務員） | 計入業務員個人業績 |
| 情況二（直接到店購買） | null | 計入「其他」，列入公司整體但不分配給個人 |

### 達成率圖表顯示

```
業務員業績達成率
  王小明   ████████████ 98%
  陳大華   ██████░░░░░░ 62%
  ────────────────────
  其他     ██████████░░ 81%   ← 情況二的銷貨匯總
  ════════════════════
  公司整體 ████████░░░░ 75%   ← 全部加總
```

> 「其他」不設定個人 KPI 目標，只統計實績，用於顯示非業務員管道的銷售佔比。

---

## 十、業績目標管理（KPI）模組設計

### 10-1. 新增 SalesTarget Entity

```csharp
public class SalesTarget : BaseEntity
{
    [Required]
    public int Year  { get; set; }   // 年度

    [Required, Range(0, 12)]
    public int Month { get; set; }   // 0 = 年度目標，1~12 = 月度目標

    public int? EmployeeId { get; set; }   // null = 公司整體目標（含「其他」）

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal TargetAmount { get; set; }

    public Employee? Employee { get; set; }
}
```

> `SalespersonId = null` 的銷貨（「其他」管道）會計入公司整體目標，不計入任何個人目標。

### 10-2. Service 介面

```csharp
public interface ISalesTargetService : IGenericManagementService<SalesTarget>
{
    Task<List<SalesTarget>>            GetByPeriodAsync(int year, int? month = null);
    Task<List<SalesAchievementItem>>   GetEmployeeAchievementListAsync(int year, int? month = null);
    Task<ServiceResult>                UpsertBatchAsync(List<SalesTarget> targets);
    Task<ServiceResult>                CopyFromPreviousYearAsync(int targetYear);
}

public class SalesAchievementItem
{
    public int?    EmployeeId      { get; set; }  // null = 公司整體
    public string  EmployeeName    { get; set; } = string.Empty;
    public decimal TargetAmount    { get; set; }
    public decimal ActualAmount    { get; set; }  // SalesDelivery.SalespersonId 匯總
    public decimal OtherAmount     { get; set; }  // SalespersonId = null 的銷貨金額
    public decimal AchievementRate => TargetAmount > 0
        ? Math.Round(ActualAmount / TargetAmount * 100, 1) : 0;
}
```

### 10-3. 目標設定頁 UI（矩陣式編輯）

路由：`/sales-targets`，權限：`SalesTarget.Write`（主管限定）

```
┌──────────────────────────────────────────────────────┐
│ 業績目標設定    年度: [2026▼]   [複製去年]  [儲存]    │
├──────────────┬────┬────┬─────┬────┬──────────────────┤
│ 業務員        │ 1月 │ 2月 │ ... │12月 │ 年度合計         │
├──────────────┼────┼────┼─────┼────┼──────────────────┤
│ 王小明        │ 50 │ 50 │ ... │ 70 │ 720 萬           │
│ 陳大華        │ 30 │ 30 │ ... │ 40 │ 420 萬           │
├──────────────┼────┼────┼─────┼────┼──────────────────┤
│ 公司整體      │200 │200 │ ... │240 │ 2,400 萬         │
└──────────────┴────┴────┴─────┴────┴──────────────────┘
    ℹ 「其他」管道銷售會自動計入公司整體，無需設定個人目標
```

---

## 十一、架構升級（配合 KPI 圖表）

### ChartModalHost 重構

新增 `ChartModalHost.razor`，統一管理所有圖表 Widget 的開啟，取代 MainLayout 目前的多個 bool 狀態：

```csharp
// MainLayout 只需 ref + 一行 Register
actionRegistry.Register("OpenSalesAchievement", () => chartModalHost.Open("SalesAchievement"));
```

### GenericChartModalComponent 多 Series 支援

SA006（目標 vs 實績月度折線）需要雙 Series，擴充 `ChartDefinition`：

```csharp
// 新增多 Series 模式（與現有單 Series 向下相容）
public Func<IServiceProvider, Task<List<ChartSeriesData>>>? MultiSeriesDataFetcher { get; set; }
public bool IsMultiSeries => MultiSeriesDataFetcher != null;

public class ChartSeriesData
{
    public string SeriesName { get; set; } = string.Empty;
    public List<ChartDataItem> Items { get; set; } = new();
    public bool IsDashed { get; set; } = false;  // 目標線顯示虛線
}
```

---

## 十二、新圖表定義

| ChartId | 標題 | 類型 | 資料來源 |
|---------|------|------|---------|
| `SA006` | 目標 vs 實績月度趨勢 | 雙折線 | SalesTarget + SalesDelivery.SalespersonId |
| `SA007` | 業務員達成率排行 | 水平長條 | SalesAchievementItem（含「其他」列） |

---

## 十三、權限設計

| Permission Key | 說明 | 建議角色 |
|---------------|------|---------|
| `SalesTarget.Read` | 查看達成率圖表與報表 | 業務員、主管 |
| `SalesTarget.Write` | 新增 / 修改業績目標 | 主管、管理員 |

---

## 十四、實作順序

```
Phase 1 — 欄位拆分（三張單據同步，趁測試階段一次完成）
  ├── Quotation.cs / SalesOrder.cs / SalesDelivery.cs 各新增 SalespersonId
  ├── 一個 Migration：AddSalespersonIdToSalesTables
  ├── 三張 EditModalComponent 各自新增業務員欄位、改製表者標籤
  ├── 轉單邏輯修正（Quotation → SalesOrder → SalesDelivery）
  └── 三份 ReportService 修正（業務員欄位改讀 SalespersonId）

Phase 2 — 圖表與報表修正
  ├── SA002 改用 SalesDelivery.SalespersonId（最簡化，無需 JOIN）
  └── CustomerSalesAnalysis EmployeeIds 篩選補實作

Phase 3 — 架構升級
  ├── ChartDefinition 新增 MultiSeriesDataFetcher
  ├── GenericChartModalComponent 多 Series 渲染分支
  └── ChartModalHost 重構 MainLayout

Phase 4 — KPI 功能開發
  ├── SalesTarget Entity + Migration
  ├── ISalesTargetService / SalesTargetService
  ├── /sales-targets 目標設定頁
  ├── SA006 / SA007 圖表定義
  ├── SalesAchievementModalComponent
  └── NavigationConfig 新增 Widget + 目標設定頁入口
```

---

## 十五、resx 需新增的 key

| Key | zh-TW | en-US | ja-JP | zh-CN | fil |
|-----|-------|-------|-------|-------|-----|
| `Field.DocumentCreator` | 製表者 | Created By | 作成者 | 制表者 | Ginawa ng |
| `Field.SalesPerson`（已存在） | 業務員 | — | — | — | — |

> `Field.SalesPerson` 已存在於所有 resx，只需新增 `Field.DocumentCreator`。

---

## 十六、開工前確認事項

- [x] 業務員 ≠ 製表者，需分開（已確認）
- [x] 三張主單都需要兩個欄位（已確認）
- [x] 直接到店購買（情況二）：`SalespersonId = null`，歸入「其他」（已確認）
- [x] 「其他」計入公司整體 KPI，不計入個人（已確認）
- [x] 目前無歷史資料，可完整重構（已確認）
- [x] SalesDelivery EditModal 需顯示業務員欄位，讓使用者可見並可調整（已確認）

---

*最後更新：2026-03-13*
