# 會計財務報表設計（FN005–FN011）

## 更新日期
2026-02-25

---

## 概述

會計模組提供七份財務報表，從科目表查詢（FN005）到完整帳冊（FN011），均整合至報表系統，透過報表中心（Alt+R 或選單）進行篩選、預覽與列印。

---

## 報表系統通用架構

| 項目 | 設計 |
|------|------|
| 服務介面模式 | 不繼承 `IEntityReportService<T>`，只定義 `RenderBatchToImagesAsync(XxxCriteria)` |
| 呼叫方式 | `GenericReportFilterModalComponent` 透過反射呼叫 |
| 入口點 | 報表中心（Alt+R 或選單）→ 搜尋 → 篩選 → 預覽 → 列印 |
| DI 注冊 | `AddScoped<IXxxReportService, XxxReportService>()` in `ServiceRegistration.cs` |
| 報表定義 | `ReportRegistry.cs`，FN005~FN011 Category = `ReportCategory.Accounting`，Permission = `JournalEntry.Read` |
| 篩選模板 | `FilterTemplateRegistry.cs`，使用 `DynamicFilterTemplate` |

### 導覽入口

- 財務管理 → 「財務報表集」→ `OpenFinancialReportIndex` action → `ReportCategory.Financial`
- 會計管理 → 「會計報表集」→ `OpenAccountingReportIndex` action → `ReportCategory.Accounting`

`OpenAccountingReportIndex` 定義於 `Components/Layout/MainLayout.razor`，並在 `actionRegistry.Register` 中註冊。

---

## FN005 — 會計科目表

> 詳細設計請見 [README_會計_科目表.md](README_會計_科目表.md) 第 7 節。

| 檔案 | 路徑 |
|------|------|
| `AccountItemListCriteria.cs` | `Models/Reports/FilterCriteria/` |
| `IAccountItemListReportService.cs` | `Services/Reports/Interfaces/` |
| `AccountItemListReportService.cs` | `Services/Reports/` |

**篩選：** 科目大類、借貸方向、層級、代碼/名稱關鍵字、僅明細科目。

---

## FN006 — 試算表

| 檔案 | 路徑 |
|------|------|
| `TrialBalanceCriteria.cs` | `Models/Reports/FilterCriteria/` |
| `ITrialBalanceReportService.cs` | `Services/Reports/Interfaces/` |
| `TrialBalanceReportService.cs` | `Services/Reports/` |

**篩選條件：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| StartDate / EndDate | FilterDateRange | 傳票日期範圍（本期發生額起訖） |
| AccountTypes | FilterEnum(AccountType) | 科目大類多選（空=全部） |
| ShowZeroBalance | FilterToggle | 顯示零餘額科目（預設 false） |

**查詢邏輯（兩次查詢）：**
- Query 1：`EntryDate BETWEEN StartDate AND EndDate` → 本期借方/貸方發生額
- Query 2：`EntryDate <= EndDate`（不限起始）→ 期末累計借方/貸方餘額

**期末餘額計算：**
- 借方正常科目：`EndingDebitBalance = CumulativeDebit - CumulativeCredit`
- 貸方正常科目：`EndingCreditBalance = CumulativeCredit - CumulativeDebit`

**報表格式：** 依 AccountType 分組，欄位：科目代碼 | 科目名稱 | 本期借方 | 本期貸方 | 期末借方餘額 | 期末貸方餘額。頁尾驗證借方/貸方合計平衡，製表人員/財務主管簽名欄。

---

## FN007 — 損益表

| 檔案 | 路徑 |
|------|------|
| `IncomeStatementCriteria.cs` | `Models/Reports/FilterCriteria/` |
| `IIncomeStatementReportService.cs` | `Services/Reports/Interfaces/` |
| `IncomeStatementReportService.cs` | `Services/Reports/` |

**篩選條件：** StartDate / EndDate（FilterDateRange）

**固定查詢範圍：** AccountType in {Revenue(4), Cost(5), Expense(6), NonOperatingIncomeAndExpense(7)}

**餘額計算規則：**
- Revenue（Credit 正常）：Balance = Credit - Debit
- Cost/Expense（Debit 正常）：Balance = Debit - Credit
- NonOperating：依各科目 `Direction` 屬性決定

**報表結構：**

```
一、銷貨收入             xxx
    各科目明細...

二、減：銷貨成本         (xxx)
    各科目明細...

毛利潤：                 xxx

三、減：營業費用         (xxx)
    各科目明細...

營業損益：               xxx

四、營業外收益及費損     xxx（淨額）
    各科目明細（收益+/費損-）...

稅前損益：               xxx
```

---

## FN008 — 資產負債表

| 檔案 | 路徑 |
|------|------|
| `BalanceSheetCriteria.cs` | `Models/Reports/FilterCriteria/` |
| `IBalanceSheetReportService.cs` | `Services/Reports/Interfaces/` |
| `BalanceSheetReportService.cs` | `Services/Reports/` |

**篩選條件：** StartDate / EndDate（FilterDateRange），EndDate 作為截止日

**核心屬性：** `AsOfDate = EndDate ?? DateTime.Today`

**固定查詢範圍：** AccountType in {Asset(1), Liability(2), Equity(3)}，`EntryDate <= AsOfDate`

**報表結構：**

```
【資產】
  各科目明細（Balance = Debit - Credit）
資產合計   xxx

【負債】
  各科目明細（Balance = Credit - Debit）
負債合計   xxx

【權益】
  各科目明細（Balance = Credit - Debit）
權益合計   xxx

負債及權益合計   xxx
```

頁尾：驗證 `|資產合計 - 負債及權益合計| < 0.01`（✓ 平衡 或 ⚠ 差額）

---

## FN009 — 總分類帳

| 檔案 | 路徑 |
|------|------|
| `GeneralLedgerCriteria.cs` | `Models/Reports/FilterCriteria/` |
| `IGeneralLedgerReportService.cs` | `Services/Reports/Interfaces/` |
| `GeneralLedgerReportService.cs` | `Services/Reports/` |

**定義：** 顯示指定期間內所有科目的帳戶卡片，按科目大類分組，每科目包含期初餘額、逐筆傳票明細（含流水餘額）、期末餘額。適合財務人員掌握全盤帳目與編製財務報表。

**篩選條件：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| StartDate / EndDate | FilterDateRange | 傳票日期範圍 |
| AccountTypes | FilterEnum(AccountType) | 科目大類多選（空=全部） |
| ShowZeroBalance | FilterToggle | 顯示零餘額科目（預設 false） |

**查詢邏輯（兩次查詢）：**
- 期初 Query：`EntryDate < StartDate`，`JournalEntryStatus == Posted`，依 AccountItemId 彙總借貸
- 本期 Query：`StartDate ≤ EntryDate ≤ EndDate`，`JournalEntryStatus == Posted`，取逐筆明細
- 餘額符號：借方為正（`balance += DebitAmount - CreditAmount`）
- 期末餘額 = 期初餘額 + 本期借方合計 − 本期貸方合計

**報表格式：**
```
科目大類分組標題
  [科目代碼] [科目名稱]
  日期        傳票號碼   摘要         借方      貸方      餘額
  ──────────────────────────────────────────────────────────
  期初餘額                                                xxx
  2026/01/01  JV0001    進貨入庫     xxx                  xxx
  2026/01/15  JV0002    銷貨出貨               xxx         xxx
  ──────────────────────────────────────────────────────────
  本期合計               xxx       xxx
  期末餘額（本期合計最後欄位顯示）                          xxx
```

**餘額顯示格式：** 正數 = `N2`；負數 = `(N2)` 括號；零 = `0.00`

---

## FN010 — 明細分類帳

| 檔案 | 路徑 |
|------|------|
| `SubsidiaryLedgerCriteria.cs` | `Models/Reports/FilterCriteria/` |
| `ISubsidiaryLedgerReportService.cs` | `Services/Reports/Interfaces/` |
| `SubsidiaryLedgerReportService.cs` | `Services/Reports/` |

**定義：** 與總分類帳格式相同，但允許依科目代碼/名稱關鍵字篩選，只顯示符合條件的科目。適合查詢應收帳款（依客戶子科目）、應付帳款（依廠商子科目）等特定帳戶明細，支援日常應收/應付管理。

**篩選條件：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| StartDate / EndDate | FilterDateRange | 傳票日期範圍 |
| AccountKeyword | FilterKeyword | 科目代碼或名稱關鍵字（空=顯示全部） |
| AccountTypes | FilterEnum(AccountType) | 科目大類多選（空=全部） |

**查詢邏輯：** 同 FN009，額外加入 `AccountItem.Code LIKE keyword OR AccountItem.Name LIKE keyword` 篩選。若關鍵字空白則行為與 FN009 相同。

**報表格式：** 同 FN009 帳戶卡片格式，頁首額外顯示科目關鍵字。

---

## FN011 — 明細科目餘額表

| 檔案 | 路徑 |
|------|------|
| `DetailAccountBalanceCriteria.cs` | `Models/Reports/FilterCriteria/` |
| `IDetailAccountBalanceReportService.cs` | `Services/Reports/Interfaces/` |
| `DetailAccountBalanceReportService.cs` | `Services/Reports/` |

**定義：** 彙總報表（無逐筆明細），顯示各科目的期初餘額、本期借方合計、本期貸方合計、期末餘額。適合月底快速核對各科目餘額，確認帳務數字整體正確。

**篩選條件：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| StartDate / EndDate | FilterDateRange | 傳票日期範圍 |
| AccountTypes | FilterEnum(AccountType) | 科目大類多選（空=全部） |
| ShowZeroBalance | FilterToggle | 顯示零餘額科目（預設 false） |

**查詢邏輯：**
- 期初 Query：`EntryDate < StartDate`，依 AccountItemId 彙總借貸各別金額
- 本期 Query：`StartDate ≤ EntryDate ≤ EndDate`，依 AccountItemId 彙總借貸各別金額
- 期初餘額 = `期初借方合計 - 期初貸方合計`（借方正號）
- 期末餘額 = 期初餘額 + 本期借方合計 − 本期貸方合計

**報表格式：**
```
科目大類分組標題
  科目代碼  科目名稱    期初餘額    本期借方    本期貸方    期末餘額
  1191      應收帳款    100,000     200,000    150,000    150,000
  ...
  [科目大類 小計]       xxx         xxx        xxx        xxx

頁尾合計：科目數 | 期初餘額合計 | 本期借方合計 | 本期貸方合計 | 期末餘額合計
```

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_傳票系統.md](README_會計_傳票系統.md)（報表資料來源：JournalEntry / JournalEntryLine）
- [報表系統總綱](../../報表系統/README_報表系統總綱.md)（報表架構與通用元件）
