# 會計模組 — 實作狀態與缺口分析

> ⚠️ **此文件已過期（截至 2026-03-17），請勿作為當前狀態參考。**
>
> 最新實作狀態請查閱：
> - [README_會計設計總綱.md](README_會計設計總綱.md) — 已知設計缺陷與修正說明、Roadmap 各項勾選狀態
> - [README_會計_Phase4_進階功能.md](README_會計_Phase4_進階功能.md) — Phase 4 各項完成標準（Definition of Done）
>
> 此文件保留作為歷史快照，未來不再維護。

## 更新日期
2026-03-17（Phase 1 完成 + 缺口補強）⚠️ 已停止更新

## 說明

本文件記錄截至 2026-03-17 的會計模組**實際程式碼狀態**，
與設計文件（Phase 1–4）逐項比對，列出缺口、文件錯誤、及待決策事項。

---

## 一、已完成項目確認

以下項目程式碼與文件一致，可視為完成：

| 項目 | 主要檔案 | 備註 |
|------|---------|------|
| 科目表管理 | `Data/Entities/FinancialManagement/AccountItem.cs` | |
| 科目表 UI | `Components/Pages/Accounting/AccountItemIndex.razor` | |
| 傳票主檔 Entity | `Data/Entities/FinancialManagement/JournalEntry.cs` | |
| 傳票分錄 Entity | `Data/Entities/FinancialManagement/JournalEntryLine.cs` | |
| 傳票 Service | `Services/FinancialManagement/JournalEntryService.cs` | 含 PostAsync、ReverseAsync、CancelDraftAsync（2026-03-16 新增） |
| 傳票 Index / EditModal | `Components/Pages/Accounting/JournalEntryIndex.razor` | 注意：在 **Accounting** 資料夾，非文件所述 FinancialManagement |
| 批次轉傳票頁面 | `Components/Pages/Accounting/JournalEntryBatchPage.razor` | |
| 批次轉傳票 Service | `Services/FinancialManagement/JournalEntryAutoGenerationService.cs` | 含 5 種單據×2（Pending+Journalize） |
| 沖款單 IsJournalized | `Data/Entities/FinancialManagement/SetoffDocument.cs` | line 100 |
| 子科目服務 | `Services/FinancialManagement/SubAccountService.cs` | |
| FN005 科目表報表 | `Services/Reports/AccountItemListReportService.cs` | |
| FN006 試算表 | `Services/Reports/TrialBalanceReportService.cs` | ✅ 2026-03-17 改為八欄（期初借/貸 + 本期借/貸 + 期末借/貸） |
| 會計期間管理 | `Data/Entities/FinancialManagement/FiscalPeriod.cs` | ✅ 2026-03-17 新增（含稽核欄位 ClosedAt/ReopenedAt/ReopenReason） |
| 期初餘額精靈 | `Components/Pages/Accounting/OpeningBalancePage.razor` | ✅ 2026-03-17 新增（含已過帳唯讀模式） |
| P3-C 業務單據顯示傳票 | 五個 EditModal | ✅ 2026-03-17 完成，含相關傳票折疊區塊 |
| FN007 損益表 | `Services/Reports/IncomeStatementReportService.cs` | |
| FN008 資產負債表 | `Services/Reports/BalanceSheetReportService.cs` | ✅ 2026-03-16 修正：移除 StartDate 篩選 + 加入本期淨利合成行 |
| FN009 總分類帳 | `Services/Reports/GeneralLedgerReportService.cs` | |
| FN010 明細分類帳 | `Services/Reports/SubsidiaryLedgerReportService.cs` | |
| FN011 明細科目餘額表 | `Services/Reports/DetailAccountBalanceReportService.cs` | |

---

## 二、文件錯誤修正清單

**以下設計文件內容與實際程式碼不符，需更正：**

| 錯誤文件 | 錯誤內容 | 實際狀況 |
|---------|---------|---------|
| Phase1、Phase2 | 傳票頁面路徑寫作 `Components/Pages/FinancialManagement/` | 實際在 **`Components/Pages/Accounting/`** |
| Phase2（P2-A） | Entity 名稱 `PayrollBatch` | 薪資模組實際 Entity 為 `PayrollPeriod` + `PayrollRecord` + `PayrollRecordDetail`，**無 `PayrollBatch`** |
| Phase2（P2-B） | 說 `MaterialIssue` 在 Warehouse 模組 | 實際在 **`Data/Entities/Inventory/MaterialIssue.cs`** |
| 批次轉傳票文件 | 說路徑為 `Components/Pages/FinancialManagement/JournalEntryBatchPage.razor` | 實際為 **`Components/Pages/Accounting/JournalEntryBatchPage.razor`** |

---

## 三、Phase 1 缺口（✅ 已完成 — 2026-03-17）

### P1-A：會計期間管理 ✅

| 項目 | 檔案 | 狀態 |
|------|------|------|
| `FiscalPeriod` Entity | `Data/Entities/FinancialManagement/FiscalPeriod.cs` | ✅ 含稽核欄位（ClosedAt、ClosedByEmployeeId、ReopenedAt、ReopenReason） |
| `FiscalPeriodStatus` Enum | `Models/Enums/AccountingEnums.cs` | ✅ Open / Closed / Locked |
| `IFiscalPeriodService` | `Services/FinancialManagement/IFiscalPeriodService.cs` | ✅ 含 InitializeYearAsync、ClosePeriodAsync（記錄關帳人）、ReopenPeriodAsync（記錄原因） |
| `FiscalPeriodService` | `Services/FinancialManagement/FiscalPeriodService.cs` | ✅ |
| `FiscalPeriodIndex.razor` | `Components/Pages/Accounting/FiscalPeriodIndex.razor` | ✅ |
| Migration | `Migrations/AddFiscalPeriodTable` | ✅（待 `dotnet ef migrations add` 執行） |
| `PostEntryAsync` 期間鎖定 | `JournalEntryService.cs` | ✅ Closed/Locked 期間拒絕過帳，OpeningBalance 傳票豁免 |
| `ReverseEntryAsync` 期間驗證 | `JournalEntryService.cs` | ✅ 沖銷日期對應期間若 Closed/Locked 拒絕並提示改用次月 |
| `Accounting.*` 權限 | `PermissionRegistry.cs` | ✅ PostEntry / ClosePeriod / OpeningBalance / YearEndClosing（均 Sensitive 級） |

### P1-B：期初餘額機制 ✅

| 項目 | 說明 |
|------|------|
| `JournalEntryType.OpeningBalance = 6` | ✅ |
| `GetOpeningBalanceEntryAsync` / `SaveOpeningBalanceAsync` | ✅ JournalEntryService 已實作 |
| `OpeningBalancePage.razor` | ✅ 三步驟精靈；草稿可編輯；**已過帳後切換唯讀模式**，顯示已過帳資料並引導建立調整分錄 |
| 借貸平衡強制檢查 | ✅ 差額不為零時過帳按鈕禁用 |

### P1-C：FN006 試算表格式 ✅

改為八欄標準格式：`科目代碼 | 科目名稱 | 期初借方 | 期初貸方 | 本期借方 | 本期貸方 | 期末借方 | 期末貸方`

---

## 四、Phase 2 缺口（高優先 🟠）

> Phase 1 完成後執行。

### P2-A：薪資傳票整合 — 暫緩（薪資模組設計尚未完整）

> **P2-A 推遲：薪資模組本身仍有許多設計缺陷尚待修正，待薪資模組重新設計後再處理傳票整合。**

**⚠️ 已知問題（暫緩前記錄，待薪資重設計後確認）：**

- `PayrollPeriod.Year` 使用民國年（如 114），而 `JournalEntry.FiscalYear` 使用西元年（2025）
- 薪資傳票建立時必須做 `payrollYear + 1911` 年份轉換
- 原文件以 `PayrollBatch` 為整合目標，但薪資模組實際 Entity 結構為：

| Entity | 路徑 | 說明 |
|--------|------|------|
| `PayrollPeriod` | `Data/Entities/Payroll/PayrollPeriod.cs` | 薪資期間（年月） |
| `PayrollRecord` | `Data/Entities/Payroll/PayrollRecord.cs` | 員工薪資記錄（一期一人） |
| `PayrollRecordDetail` | `Data/Entities/Payroll/PayrollRecordDetail.cs` | 薪資項目明細 |

**建議整合方式（需確認）：**

以 `PayrollPeriod`（薪資期間）為轉傳票單位：
- 一個 `PayrollPeriod` 對應一張薪資計提傳票
- 一個 `PayrollPeriod` 發放後對應一張薪資發放傳票
- `IsJournalized`（計提）、`IsPaymentJournalized`（發放）加到 `PayrollPeriod`

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| `PayrollPeriod.IsJournalized` | 未新增 |
| `PayrollPeriod.IsPaymentJournalized` | 未新增 |
| Migration `AddPayrollPeriodIsJournalized` | 未建立 |
| `JournalEntryAutoGenerationService.GetPendingPayrollPeriodsAsync()` | 未新增 |
| `JournalEntryAutoGenerationService.JournalizePayrollAccrualAsync()` | 薪資計提，未新增 |
| `JournalEntryAutoGenerationService.JournalizePayrollPaymentAsync()` | 薪資發放，未新增 |
| 批次轉傳票頁面 — 薪資 Section | 未新增 |

---

### P2-B：材料領用傳票整合 — 部分缺失，有待決策項目

**現有狀況（程式碼確認）：**
- `MaterialIssue` Entity 存在於 `Data/Entities/Inventory/`
- `MaterialIssueDetail.UnitCost` 欄位**已存在**（nullable decimal），`TotalCost` 為計算屬性
- `ProductionScheduleDetailId`（關聯生產排程明細）已有
- **無 `IsConfirmed` 欄位**（缺少領料確認狀態）
- **無 `IsJournalized` 欄位**
- **無正式用途分類欄位**（Remarks 為自由文字）

**⚠️ 成本來源待確認：**
`MaterialIssueDetail.UnitCost` 為 nullable，需確認領料確認時是否自動填入品項當下的移動均價（`Product.AverageCost`）。
若未自動填入，傳票金額將為 null，建立時需有警告機制。

**⚠️ 缺少確認狀態（新發現缺口）：**
`MaterialIssue` 沒有任何代表「已確認」的狀態欄位，批次轉傳票無法篩選「可轉傳票的領料單」（已確認）vs「草稿中的領料單」。
必須在 P2-B 前先補充 `IsConfirmed`、`ConfirmedAt`、`ConfirmedByEmployeeId`。

**⚠️ 待決策：材料領用用途分類**

目前 `MaterialIssue` 無 Enum 型用途欄位，轉傳票時無法判斷借方科目。
需決定：

| 選項 | 做法 | 優點 |
|------|------|------|
| A（建議） | 新增 `MaterialIssuePurpose` Enum + 欄位 | 明確，可擴充 |
| B | 根據 `ProductionScheduleId != null` 判斷生產用料，其餘歸費用 | 免改 Entity，但彈性差 |

```csharp
// 選項 A：新增 Enum（建議值）
public enum MaterialIssuePurpose
{
    Production = 1,     // 生產用料 → 借：在製品 (1321)
    GeneralExpense = 2, // 一般消耗 → 借：物料費用 (6xxx)
    Sample = 3,         // 樣品     → 借：樣品費用 (6xxx)
}
```

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| `MaterialIssue.IsConfirmed` | 未新增（**必須先補充，才能篩選可轉傳票的領料單**） |
| `MaterialIssue.ConfirmedAt` | 未新增 |
| `MaterialIssue.ConfirmedByEmployeeId` | 未新增 |
| `MaterialIssuePurpose` Enum（若選 A） | 未建立 |
| `MaterialIssue.IssuePurpose`（若選 A） | 未新增 |
| `MaterialIssue.IsJournalized` | 未新增 |
| `MaterialIssue.JournalizedAt` | 未新增 |
| Migration `AddMaterialIssueFields` | 未建立（含以上所有新欄位） |
| `JournalEntryAutoGenerationService.GetPendingMaterialIssuesAsync()` | 未新增 |
| `JournalEntryAutoGenerationService.JournalizeMaterialIssueAsync()` | 未新增 |
| 批次轉傳票頁面 — 材料領用 Section | 未新增 |

---

### P2-C：生產完工傳票整合 — 完全未實作，核心決策已定

**成本計算方式（已決策：選項 C — 取自 InventoryTransaction）：**

`ProductionScheduleCompletion` 已有：
- `ActualUnitCost` (decimal?, nullable)
- `InventoryTransactionId` (int?, nullable FK)

取 `InventoryTransaction.TotalAmount` 作為傳票金額，與銷貨出貨 COGS 邏輯一致。

**⚠️ Nullable 風險：**
兩個欄位均為 nullable。若完工入庫流程未確實填入，傳票金額將為零。
批次轉傳票前需加檢查：`InventoryTransactionId == null` 的完工記錄顯示警告並跳過。

**IsJournalized 層級設計修正（新發現缺口）：**

> ⚠ `IsJournalized` 應加在 **`ProductionScheduleCompletion`**，而非 `ProductionScheduleItem`
>
> 一個 `ProductionScheduleItem` 可有多筆分批完工記錄（`ProductionScheduleCompletion`），
> 每次完工獨立入庫、獨立建傳票，必須在 Completion 層級追蹤。

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| `ProductionScheduleCompletion.IsJournalized` | 未新增（加在 Completion 層，非 Item 層）|
| `ProductionScheduleCompletion.JournalizedAt` | 未新增 |
| Migration `AddProductionCompletionIsJournalized` | 未建立 |
| `JournalEntryAutoGenerationService.GetPendingProductionCompletionsAsync()` | 未新增 |
| `JournalEntryAutoGenerationService.JournalizeProductionCompletionAsync()` | 未新增（每筆 Completion 建一張傳票）|
| 批次轉傳票頁面 — 生產完工 Section | 未新增（需顯示 InventoryTransactionId 為 null 的警告）|

---

## 五、Phase 3 缺口（中優先 🟡）

### P3-A：AR/AP 帳齡分析 — 部分缺失

**現有狀況：**
- `CustomerStatementReportService` 存在，但這是「客戶對帳單」（逐筆流水）
- `SupplierStatementReportService` 存在，同上
- **無帳齡區間分析報表**（0-30 / 31-60 / 61-90 / 91-120 / 120+ 天分組）

**計算設計決策（已確認）：**
- 帳齡計算單位：**SalesDelivery 主檔**（非逐行品項明細）
- **未收金額：`(SalesDelivery.TotalAmount + SalesDelivery.TaxAmount) - SUM(SetoffProductDetail.CurrentSetoffAmount WHERE 關聯此單據)`**
  - 必須用含稅金額（TotalAmount + TaxAmount），因為傳票 AR 借方 = `TotalAmountWithTax`
  - `SetoffProductDetail` 也使用含稅（`CalculateTaxInclusiveAmount`），口徑一致
  - `SalesDelivery.DiscountAmount` 已計入 `TotalAmount`，不需另外扣除
- 帳齡天數基準：`DeliveryDate + Customer.PaymentDays`（需新增欄位，見下方）
- `SetoffProductDetail` 為多型 FK，查詢需批量處理避免 N+1

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| `Customer.PaymentDays` (int) | **不存在**，只有 `PaymentTerms` (string，自由文字）；需新增整數欄位（各客戶不同天數） |
| `Supplier.PaymentDays` (int) | 同上，需新增 |
| Migration `AddCustomerSupplierPaymentDays` | 未建立 |
| `Models/Reports/FilterCriteria/ARAgingCriteria.cs` | 未建立 |
| `Services/Reports/IARAgingReportService.cs` | 未建立 |
| `Services/Reports/ARAgingReportService.cs` | 未建立 |
| `Models/Reports/FilterCriteria/APAgingCriteria.cs` | 未建立 |
| `Services/Reports/IAPAgingReportService.cs` | 未建立 |
| `Services/Reports/APAgingReportService.cs` | 未建立 |
| FN012 / FN013 加入 `ReportRegistry.cs` | 未新增 |

---

### P3-B：年底結帳 — 完全未實作

**依賴：P1-A（FiscalPeriod）必須先完成。**

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| `Services/FinancialManagement/IFiscalYearClosingService.cs` | 未建立 |
| `Services/FinancialManagement/FiscalYearClosingService.cs` | 未建立 |
| FiscalPeriodIndex.razor 年度結帳按鈕 | 未新增 |

**注意：** `JournalEntryType.Closing = 4` 已定義，但無任何執行邏輯使用此類型。

---

### P3-C：業務單據顯示相關傳票 — 完全未實作（但可立即執行）

> ✅ `JournalEntryService.GetBySourceDocumentAsync` 已實作，後端無任何依賴。
> 這是 Phase 3 中最容易完成的項目，建議優先執行。

**需在以下 5 個 EditModal 底部新增「相關傳票」折疊區塊：**

| EditModal | 路徑 |
|-----------|------|
| 進貨入庫 | `Components/Pages/Purchase/PurchaseReceivingEditModal.razor` |
| 銷貨出貨 | `Components/Pages/Sales/SalesDeliveryEditModal.razor` |
| 進貨退回 | `Components/Pages/Purchase/PurchaseReturnEditModal.razor` |
| 銷貨退回 | `Components/Pages/Sales/SalesReturnEditModal.razor` |
| 沖款單 | `Components/Pages/FinancialManagement/SetoffDocumentEditModalComponent.razor` |

**查詢方式（已有 Service 支援）：**
```csharp
var entries = await JournalEntryService.GetBySourceDocumentAsync("PurchaseReceiving", document.Id);
```

---

## 六、程式碼 Bug 清單（審查紀錄）

> 本節為 2026-03-16 程式碼審查發現。已修正項目以 ✅ 標記。

### Bug-1：沖款傳票 AR/AP 子科目繞過 ✅ 已修正

**影響：** `JournalEntryAutoGenerationService.JournalizeSetoffDocumentAsync`

| 位置 | 現行程式碼 | 問題 |
|------|-----------|------|
| line 609（AR setoff） | `GetByCodeAsync(AccountReceivableCode)` | 永遠用 1191，不查客戶子科目 |
| line 667（AP setoff） | `GetByCodeAsync(AccountPayableCode)` | 永遠用 2171，不查廠商子科目 |

**正確行為：**
- AR setoff 應呼叫 `GetARAccountForCustomerAsync(doc.RelatedPartyId)`
- AP setoff 應呼叫 `GetAPAccountForSupplierAsync(doc.RelatedPartyId)`

**後果（子科目已啟用時）：**
```
銷貨出貨傳票：借 1191.C001  ← 正確（GetARAccountForCustomerAsync）
沖款傳票：    貸 1191       ← 錯誤（直接用統制科目）
→ 子科目 1191.C001 餘額永遠不會被沖掉
```

**修正範圍：** 2 行程式碼改動，風險極低。

---

### Bug-2：沖款選擇包含 SalesOrderDetail（設計錯誤）✅ 已修正

**影響：** `SetoffProductDetailService.GetUnsettledReceivableDetailsAsync`

**現行程式碼（line 210-244）：** 查詢「未結清的銷貨**訂單**明細」並作為沖款對象返回。

**設計決策確認：** 沖款只在真實發生交易時才能進行：
- ✅ `SalesDelivery`（銷貨出貨）
- ✅ `SalesReturn`（銷貨退回）
- ✅ `PurchaseReceiving`（進貨入庫）
- ✅ `PurchaseReturn`（採購退貨）
- ❌ `SalesOrder`（銷貨訂單）— 訂單尚未出貨，應收帳款未成立

**後果：** 訂單尚未出貨即可被沖款，且沖款傳票建立的 AR 貸方抵消的是 `SalesDelivery` 產生的借方，造成帳務錯配。

**修正：** 從 `GetUnsettledReceivableDetailsAsync` 移除 salesOrderRawData 查詢（lines 211-244），並從 `SetoffDetailType` enum 移除或棄用 `SalesOrderDetail = 1`。

---

### Bug-3：沖銷傳票缺少反向指向欄位（部分緩解）

**影響：** `JournalEntryService.ReverseEntryAsync`、`JournalEntry` Entity

**現行：** 原傳票 A 建立後設 `ReversalEntryId = B.Id`（A → B 有關聯）。
但沖銷傳票 B 本身沒有任何欄位指向原傳票 A（B → A 無關聯）。

**2026-03-16 部分緩解：**
- `ReverseEntryAsync` 已修改為沖銷傳票不繼承 SourceDocumentType/Id，避免唯一索引衝突
- `SourceDocumentCode` 保留（純文字顯示用），Description 字串亦保留原傳票號碼

**剩餘缺口：** 沖銷傳票 B 仍無程式化 FK 指回原傳票 A，UI 無法做超連結跳轉。
**待辦：** `JournalEntry` 新增 `ReversedEntryId` (int?)，需 Migration。

---

### Bug-4：傳票沖銷後 IsJournalized 未重置 ✅ 已修正

**影響：** `JournalEntryService.ReverseEntryAsync`（未觸及業務單據）

**情境：**
1. 沖款單 D001 → 轉傳票（`D001.IsJournalized = true`）
2. 傳票被沖銷 → `JournalEntry.IsReversed = true`，但 `D001.IsJournalized` 仍是 **true**
3. 批次轉傳票頁面只顯示 `IsJournalized == false` 的記錄 → D001 消失
4. D001 的帳務空缺永遠無法補轉

**影響範圍：** SetoffDocument、SalesDelivery、PurchaseReceiving、SalesReturn、PurchaseReturn。

**修正（2026-03-16）：** `ReverseEntryAsync` 在同一 DB 交易內，透過 `ResetSourceDocumentJournalizedAsync` 根據 `SourceDocumentType` + `SourceDocumentId` 找到對應業務單據，將其 `IsJournalized = false`、`JournalizedAt = null`，使其重新出現在待轉傳票清單。

---

### Bug-5：財務報表未依 CompanyId 篩選（待修正）

**影響：** FN006 ~ FN011 全部財務報表 Service

**確認需要多公司支援。**

現行查詢（以試算表為例）：
```csharp
context.JournalEntryLines
    .Where(l => l.JournalEntry.JournalEntryStatus == JournalEntryStatus.Posted)
    // 沒有 CompanyId 篩選！
```
`GetPrimaryCompanyAsync()` 只用於報表抬頭，不用於資料篩選。若有 2 家公司同時使用，試算表、損益表、資產負債表的數字都會混在一起。

**修正：** 在各 Criteria（`TrialBalanceCriteria`、`IncomeStatementCriteria`等）加入 `CompanyId`，查詢時加 `.Where(l => l.JournalEntry.CompanyId == criteria.CompanyId)`。

---

### Bug-6：GetUnsettledReceivableDetailsAsync N+1 查詢 ✅ 已修正（2026-03-16）

**影響：** `SetoffProductDetailService.GetUnsettledReceivableDetailsAsync` / `GetUnsettledPayableDetailsAsync`

**現行問題：** 載入所有未結清明細後，對**每一筆**明細分別執行 2 次 `SumAsync`，共 2N 次 DB 查詢。

**修正（2026-03-16）：** 將所有明細 Id 集中為兩個列表，執行一次批次 `GroupBy` 查詢，在記憶體中建立 Dictionary，再逐筆填入，總計從 2N 次降為 1 次 DB 查詢：
```csharp
var setoffSums = await context.SetoffProductDetails
    .Where(d =>
        (d.SourceDetailType == SalesReturnDetail   && returnIds.Contains(d.SourceDetailId)) ||
        (d.SourceDetailType == SalesDeliveryDetail && deliveryIds.Contains(d.SourceDetailId)))
    .GroupBy(d => new { d.SourceDetailId, d.SourceDetailType })
    .Select(g => new { ..., TotalSetoff = g.Sum(...), TotalAllowance = g.Sum(...) })
    .ToListAsync();
```
同樣修正適用於 `GetUnsettledPayableDetailsAsync`（PurchaseReceivingDetail / PurchaseReturnDetail）。

---

### Bug-7：自動產生傳票可被手動修改 ✅ 已修正

**影響：** `JournalEntryService.SaveWithLinesAsync`

**現行問題：** 系統自動產生（`JournalEntryType.AutoGenerated`）的傳票，手動編輯時未被阻擋，可能造成帳務與業務單據金額不一致。

**修正（2026-03-16）：** 在 `SaveWithLinesAsync` 的更新分支中加入 EntryType 檢查：
```csharp
if (existing.EntryType == JournalEntryType.AutoGenerated)
    return (false, "自動產生的傳票不可手動修改，請沖銷後由系統重新產生");
```

---

### Bug-8：FiscalYear/FiscalPeriod 未從 EntryDate 自動推導 ✅ 已修正

**影響：** `JournalEntryService.SaveWithLinesAsync`

**現行問題：** FiscalYear 與 FiscalPeriod 由前端傳入，若前端填寫錯誤（如誤填 113 年、或期間填成 0），帳務期間歸屬即錯誤。

**修正（2026-03-16）：** 在 `SaveWithLinesAsync` 強制從 EntryDate 推導：
```csharp
var fiscalYear   = journalEntry.EntryDate.Year;
var fiscalPeriod = journalEntry.EntryDate.Month;
```
確保 FiscalYear 永遠等於 `EntryDate.Year`，FiscalPeriod 等於 `EntryDate.Month`。

---

### Bug-9：並行批次轉傳票可能建立重複傳票 ✅ 已修正（已加索引）

**影響：** `JournalEntryService.GetBySourceDocumentAsync`、`AppDbContext`

**現行問題（修正前）：** `GetBySourceDocumentAsync` 不排除 Reversed/Cancelled 傳票，沖銷後舊傳票仍佔位，且無資料庫唯一索引，並行時兩個請求可能同時通過「已有傳票？」檢查並雙重建立。

**修正（2026-03-16）：**

1. `GetBySourceDocumentAsync` 排除 Reversed/Cancelled：
```csharp
je.JournalEntryStatus != JournalEntryStatus.Reversed &&
je.JournalEntryStatus != JournalEntryStatus.Cancelled
```

2. `ReverseEntryAsync` 沖銷傳票清空來源單據欄位（避免唯一索引衝突）：
```csharp
SourceDocumentType = null,
SourceDocumentId   = null,
```

3. `AppDbContext` 加入過濾唯一索引（SourceDocumentType IS NOT NULL 時才強制唯一）：
```csharp
entity.HasIndex(e => new { e.SourceDocumentType, e.SourceDocumentId })
      .IsUnique()
      .HasFilter("[SourceDocumentType] IS NOT NULL")
      .HasDatabaseName("UX_JournalEntry_SourceDocument");
```

**待辦：** 需執行 Migration — `dotnet ef migrations add AddJournalEntrySourceDocumentUniqueIndex`

---

### Bug-10：資產負債表截止前年末結帳尚未執行時等式不平衡 ✅ 已修正

**影響：** `BalanceSheetReportService.BuildAccountLinesAsync`

**現行問題：** 未到年底結帳前，損益科目（Revenue/Cost/Expense 等）尚未轉入保留盈餘，資產負債表「權益」區段金額偏低，`資產 ≠ 負債 + 權益`。

**修正（2026-03-16）：** 額外查詢所有損益科目分錄計算本期淨利/損，建立「本期淨利」或「本期淨損」合成行加入權益區段：
```csharp
var netIncome = totalPLCredit - totalPLDebit;
if (netIncome != 0)
{
    result.Add(new AccountSummaryLine {
        Name        = netIncome >= 0 ? "本期淨利" : "本期淨損",
        AccountType = AccountType.Equity,
        SortOrder   = int.MaxValue,
        Balance     = netIncome
    });
}
```

---

### Bug-11：資產負債表錯誤使用 StartDate 篩選（期初餘額消失）✅ 已修正

**影響：** `BalanceSheetReportService.BuildAccountLinesAsync`

**現行問題：** 若 Criteria 含 StartDate 篩選，資產/負債/權益科目的**歷史累積餘額會被截斷**，期初已存在的餘額消失，資產負債表數字嚴重失真。

**修正（2026-03-16）：** 移除 StartDate 篩選。資產負債表必須從公司成立第一天累計至截止日，只設 `AsOfDate` 上限：
```csharp
var asOfDate = criteria.AsOfDate.Date.AddDays(1).AddTicks(-1);
// 查詢只有 EntryDate <= asOfDate，不設下限
```

---

### Bug-13：試算表負餘額科目期末欄位均顯示為零 ✅ 已修正

**影響：** `TrialBalanceReportService.AccountBalanceLine`

**現行問題：**
```csharp
// 修正前：NetCumulative < 0 時兩欄均回傳 0，隱藏異常反向餘額
EndingDebitBalance  = NetCumulative > 0 && NormalDirection == Debit   ? NetCumulative : 0
EndingCreditBalance = NetCumulative > 0 && NormalDirection == Credit  ? NetCumulative : 0
```
若資產科目貸方 > 借方（異常但合法），試算表兩欄皆為 0，報表與實際帳不符，期末借/貸合計也無法平衡。

**修正（2026-03-16）：** 負值時絕對值顯示於對面欄位：
```csharp
EndingDebitBalance  = (NetCumulative > 0 && NormalDirection == Debit)  || (NetCumulative < 0 && NormalDirection == Credit)  ? |NetCumulative| : 0
EndingCreditBalance = (NetCumulative > 0 && NormalDirection == Credit) || (NetCumulative < 0 && NormalDirection == Debit)   ? |NetCumulative| : 0
```

---

### Bug-14：損益表 ComprehensiveIncome 口徑與資產負債表不一致 ✅ 已修正

**影響：** `IncomeStatementReportService.IncomeStatementTypes`

**現行問題：** 損益表 `IncomeStatementTypes` 未包含 `AccountType.ComprehensiveIncome`，但 `BalanceSheetReportService` 計算本期淨利時包含。若有 ComprehensiveIncome 傳票，資產負債表淨利 ≠ 損益表淨利，兩份報表數字不一致。

**修正（2026-03-16）：** 將 `AccountType.ComprehensiveIncome` 加入 `IncomeStatementReportService.IncomeStatementTypes`，使兩份報表使用相同科目範圍計算損益。

---

### Bug-15：過帳失敗後草稿傳票殘留鎖死後續轉帳 ✅ 已修正

**影響：** `JournalEntryAutoGenerationService.CreateAndPostEntryAsync`

**現行問題：** `SaveWithLinesAsync`（建立草稿）成功後，`PostEntryAsync` 失敗時草稿傳票仍保留在 DB，且已佔用 `SourceDocumentType`/`SourceDocumentId`。下次再轉傳票時，`GetBySourceDocumentAsync` 找到此草稿（只排除 Reversed/Cancelled，Draft 不排除），誤報「已有對應傳票，請先沖銷」，但草稿無法沖銷，用戶陷入死局。

**修正（2026-03-16）：** 過帳失敗時自動呼叫 `CancelDraftEntryAsync` 清除殘留草稿：
```csharp
if (!posted)
{
    await _journalEntryService.CancelDraftEntryAsync(entry.Id, createdBy);
    return (false, $"過帳失敗：{postError}");
}
```

---

### Bug-16：銷貨出貨/退回 COGS 為零時靜默跳過 ✅ 已修正

**影響：** `JournalEntryAutoGenerationService.JournalizeSalesDeliveryAsync`、`JournalizeSalesReturnAsync`

**現行問題：** 若 `InventoryTransaction` 不存在（出庫流程未正常執行），`cogsAmount == 0` 時 COGS 分錄被靜默跳過。傳票借貸平衡可正常過帳，但存貨成本認列完全缺失，損益表收入/成本嚴重失真，且無任何警告。

**修正（2026-03-16）：** 加入 Warning log 提醒：
```csharp
if (cogsAmount == 0)
    _logger.LogWarning("銷貨出貨 {Id}（{Code}）找不到對應的庫存異動記錄，COGS 分錄未加入傳票...", id, doc.Code);
```
銷貨退回同樣處理。

---

### Bug-17：總分類帳/明細分類帳/明細科目餘額表 — StartDate 為空時期初餘額含 EndDate 後的資料 ✅ 已修正

**影響：** `GeneralLedgerReportService`、`SubsidiaryLedgerReportService`、`DetailAccountBalanceReportService`（三份）

**現行問題：**
```csharp
if (criteria.StartDate.HasValue)
    openingQuery = openingQuery.Where(l => l.JournalEntry.EntryDate < criteria.StartDate.Value.Date);
// StartDate 為 null 時此 if 被跳過 → openingQuery 無日期限制 → 載入所有歷史分錄（含 EndDate 後）
```

當 StartDate 未設定：
1. `openingLines` = 全部 Posted 分錄（含截止日後的未來傳票）→ `openingBalance` 數字失真
2. `periodLines` 同樣無 StartDate 限制 → 同一筆傳票在 openingBalance 和 period entries 各計一次 → **雙重計算**

**修正（2026-03-16）：** 加入 `else` 分支，StartDate 為 null 時 openingQuery 強制回傳空結果：
```csharp
if (criteria.StartDate.HasValue)
    openingQuery = openingQuery.Where(l => l.JournalEntry.EntryDate < criteria.StartDate.Value.Date);
else
    openingQuery = openingQuery.Where(l => false); // 無 StartDate → 無期初（避免全表載入）
```

---

### Bug-18：SubAccountService EntityCode 格式下相同代碼產生重複科目碼衝突 ✅ 已修正

**影響：** `SubAccountService.GenerateSubAccountCodeAsync`

**現行問題：** EntityCode 格式直接用 `{parent.Code}.{sanitized}` 回傳，不檢查代碼唯一性。若兩個客戶的 Code 相同（重複使用、或代碼曾被更改），第二個客戶嘗試建立子科目時會因 AccountItem.Code 唯一索引衝突，拋出 `DbUpdateException`。

**修正（2026-03-16）：** 建立前先查詢代碼是否已存在，衝突時 fallthrough 至流水號模式：
```csharp
var candidateCode = $"{parent.Code}.{sanitized}";
var codeConflict = await context.AccountItems.AnyAsync(a => a.Code == candidateCode);
if (!codeConflict)
    return candidateCode;
// 代碼已被其他科目佔用 → fallthrough 至流水號
```

---

### Bug-19：GeneralLedgerReportService — 無任何篩選條件時全表掃描（效能防護）✅ 已修正

**影響：** `GeneralLedgerReportService.BuildAccountLedgersAsync`

**現行問題：** 若 `criteria.AccountTypes` 為空且 `criteria.StartDate` 為 null，兩個查詢（openingQuery + periodQuery）均無任何篩選，對全部 JournalEntryLines 做全表掃描，加上 Include Navigation properties，可能載入數十萬筆資料佔用大量記憶體。

**修正（2026-03-16）：** 在方法入口加入防護，若無 StartDate 且無 AccountType 篩選，拒絕執行並回傳空：
```csharp
if (!criteria.StartDate.HasValue && !criteria.AccountTypes.Any())
{
    _logger?.LogWarning("GeneralLedger 未設定開始日期且未選擇科目大類，拒絕執行以避免全表掃描");
    return new List<AccountLedger>();
}
```

> **待辦：** UI 層篩選面板應將「開始日期」或「科目大類」設為必填，當兩者均為空時禁用「預覽列印」按鈕，避免用戶困惑於空白報表。

---

### Bug-20：SaveWithLinesAsync 更新分支 — 狀態保護檢查前端傳入值而非 DB 實際值 ✅ 已修正

**影響：** `JournalEntryService.SaveWithLinesAsync`

**現行問題（修正前）：**
```csharp
// 錯誤：檢查的是前端傳入的 journalEntry.JournalEntryStatus，非 DB 實際狀態
if (journalEntry.JournalEntryStatus == JournalEntryStatus.Posted)
    return (false, "已過帳的傳票不可修改");
var existing = await context.JournalEntries...;
```

若前端送入 `JournalEntryStatus = Draft` 但 DB 裡該傳票實際為 `Reversed` 或 `Cancelled`，檢查通過（`Draft != Posted`），導致已作廢/已沖銷傳票被強制覆寫。`Reversed`、`Cancelled` 兩個狀態也完全未被攔截。

**修正（2026-03-16）：** 先載入 `existing`，再對 DB 實際狀態做完整三態驗證：
```csharp
var existing = await context.JournalEntries...;
if (existing.JournalEntryStatus == JournalEntryStatus.Posted)
    return (false, "已過帳的傳票不可修改，請先沖銷後重新建立");
if (existing.JournalEntryStatus == JournalEntryStatus.Reversed)
    return (false, "已沖銷的傳票不可修改");
if (existing.JournalEntryStatus == JournalEntryStatus.Cancelled)
    return (false, "已作廢的傳票不可修改");
```

---

### Bug-21：GeneralLedgerCriteria.Validate() 永遠回傳 true — 缺少篩選條件必填驗證 ✅ 已修正

**影響：** `Models/Reports/FilterCriteria/GeneralLedgerCriteria.cs`

**現行問題：** `Validate()` 無任何邏輯，永遠回傳 `true`。搭配 Bug-19 修正後，無篩選條件時服務層回傳空集合，UI 顯示「無符合條件的傳票資料」，用戶不知道是自己未設定篩選條件還是真的無資料。

**修正（2026-03-16）：** 加入必填驗證，StartDate 和 AccountTypes 至少需有一：
```csharp
if (!StartDate.HasValue && !AccountTypes.Any())
{
    errorMessage = "總分類帳需至少設定「開始日期」或選擇「科目大類」";
    return false;
}
```

> **注意：** `TrialBalanceCriteria` 不做相同修改——試算表的累計餘額固定從最早傳票開始計算，無 StartDate 是合理使用情境，不應限制。

---

### Bug-22：ReverseEntryAsync — 沖銷傳票 UpdatedAt/UpdatedBy 未初始化 ✅ 已修正

**影響：** `JournalEntryService.ReverseEntryAsync`

**現行問題：** `reversalEntry` 初始化時只設 `CreatedAt`/`CreatedBy`，未設 `UpdatedAt`/`UpdatedBy`，導致沖銷傳票的 `UpdatedAt` 為 `DateTime.MinValue (0001-01-01)`，UI 時間欄位顯示異常。

**修正（2026-03-16）：** 加入初始化：
```csharp
CreatedAt  = DateTime.Now,
CreatedBy  = updatedBy,
UpdatedAt  = DateTime.Now,
UpdatedBy  = updatedBy,
```

---

### Bug-23：SetoffDocumentService.SearchAsync — 搜尋邏輯為 AND 而非 OR ✅ 已修正

**影響：** `SetoffDocumentService.SearchAsync`

**現行問題：** DB 查詢只撈出 Code 或 CompanyName 符合的文件，再對其套用記憶體 RelatedPartyName 過濾，實際邏輯為 `(Code OR CompanyName) AND RelatedPartyName`。使用者輸入客戶／廠商名稱搜尋時，DB 層根本不回傳，記憶體過濾永遠是空結果。

**修正（2026-03-16）：** 先無條件載入全部沖款單並解析 RelatedPartyName，再以 OR 同時比對三個欄位：
```csharp
var setoffDocuments = allDocuments
    .Where(s =>
        (s.Code != null && s.Code.Contains(searchTerm, OrdinalIgnoreCase)) ||
        s.Company.CompanyName.Contains(searchTerm, OrdinalIgnoreCase) ||
        s.RelatedPartyName.Contains(searchTerm, OrdinalIgnoreCase))
    .ToList();
```

---

### Bug-24：CreateBatchWithValidationAsync — 批次內重複 SourceDetailId 驗證漏洞 ✅ 已修正

**影響：** `SetoffProductDetailService.CreateBatchWithValidationAsync`

**現行問題：** 批次驗證時，每筆 detail 的 `ValidateSetoffAmountAsync` 讀取 DB 中的 `TotalReceivedAmount`（尚未被本批次更新）。若同批次中有兩筆明細指向同一 `SourceDetailId`，第二筆不會看到第一筆的金額，可能使合計超額仍通過驗證。

**舉例：** 來源明細餘額 $200，同批次加入 $150 + $100 → 各自驗證均 ≤ $200 通過，但合計 $250 超過餘額。

**修正（2026-03-16）：** 驗證前先以 LINQ GroupBy 將批次內同 SourceDetailId 的金額加總，對合計值整體驗證；若批次內同來源有多筆既有記錄（編輯模式），則改為逐筆驗證差值：
```csharp
var batchTotals = details
    .GroupBy(d => (d.SourceDetailId, d.SourceDetailType))
    .ToDictionary(g => g.Key,
                  g => (Setoff: g.Sum(d => d.CurrentSetoffAmount),
                         Allowance: g.Sum(d => d.CurrentAllowanceAmount)));

foreach (var ((sourceDetailId, sourceType), totals) in batchTotals)
{
    // 以合計金額驗證，防止批次內超額
    var validation = await ValidateSetoffAmountAsync(
        sourceDetailId, sourceType, totals.Setoff, totals.Allowance, excludeId);
    if (!validation.IsSuccess) return validation;
}
```

---

### Bug-25：JournalizedAt 使用 DateTime.Now，其他時間戳用 DateTime.UtcNow ✅ 已修正

**影響：** `JournalEntryAutoGenerationService`（5 處 `JournalizedAt = DateTime.Now`）

**現行問題：** `GenericManagementService` 的 `CreateAsync`/`UpdateAsync` 一律使用 `DateTime.UtcNow`（line 158、226），但各 `JournalizeXxxAsync` 寫入的 `doc.JournalizedAt` 使用本地時間 `DateTime.Now`。同一筆資料的 `CreatedAt` 是 UTC，`JournalizedAt` 是本地時間，在 UTC+0 以外的伺服器環境會出現時區不一致，導致稽核日誌時序紊亂。

**修正（2026-03-16）：** 全檔 replace，5 處 `JournalizedAt = DateTime.Now` → `JournalizedAt = DateTime.UtcNow`：
- `JournalizePurchaseReceivingAsync` line 228
- `JournalizePurchaseReturnAsync` line 312
- `JournalizeSalesDeliveryAsync` line 432
- `JournalizeSalesReturnAsync` line 553
- `JournalizeSetoffDocumentAsync` line 746

---

### Bug-26：多個 Service 的實體時間戳使用 DateTime.Now 而非 DateTime.UtcNow ✅ 已修正

**影響：** `JournalEntryService`（10 處）、`SetoffPrepaymentService`（1 處）、`SetoffPrepaymentUsageService`（1 處）

**現行問題：** `GenericManagementService` 的標準為 `DateTime.UtcNow`（line 158、226），但上述三個 Service 的部分方法直接使用本地時間 `DateTime.Now`，導致同一筆資料的 `CreatedAt`/`UpdatedAt` 時區不一致，在 UTC+0 以外的伺服器上稽核日誌時序會錯亂。

**修正（2026-03-16）：**
- `JournalEntryService.cs`：全檔 replace_all，10 處 `DateTime.Now` → `DateTime.UtcNow`（`PostEntryAsync`、`ReverseEntryAsync`、`CancelDraftEntryAsync`、`SaveWithLinesAsync`）
- `SetoffPrepaymentService.cs` line 409：`UpdateUsedAmountAsync` 中 `prepayment.UpdatedAt = DateTime.Now` → `DateTime.UtcNow`
- `SetoffPrepaymentUsageService.cs` line 370：`UpdatePrepaymentUsedAmountAsync` 中 `prepayment.UpdatedAt = DateTime.Now` → `DateTime.UtcNow`

---

### Bug-27：ReverseEntryAsync 兩次 SaveChangesAsync 不在同一個交易中 ✅ 已修正

**影響：** `JournalEntryService.ReverseEntryAsync`

**現行問題：** 沖銷流程分為兩個連續的 `SaveChangesAsync`：
1. 第一次：`ADD reversalEntry` → 取得 DB 自動產生的 `reversalEntry.Id`
2. 第二次：更新 `original.IsReversed = true`、`original.ReversalEntryId = reversalEntry.Id`

兩次提交之間沒有明確的資料庫交易保護，若第二次 `SaveChangesAsync` 失敗（如 DB 連線中斷、死鎖），DB 將存在沖銷傳票但原傳票狀態未更新的不一致狀態，業務上等同於憑空多出一張沖銷傳票。

**修正（2026-03-16）：** 在兩次 `SaveChangesAsync` 外層加入明確交易：
```csharp
await using var transaction = await context.Database.BeginTransactionAsync();

context.JournalEntries.Add(reversalEntry);
await context.SaveChangesAsync();

original.IsReversed = true;
original.JournalEntryStatus = JournalEntryStatus.Reversed;
original.ReversalEntryId = reversalEntry.Id;
original.UpdatedAt = DateTime.UtcNow;
original.UpdatedBy = updatedBy;

await context.SaveChangesAsync();
await transaction.CommitAsync();
```
若任一步驟拋出例外，交易自動回滾，DB 保持一致。

---

### Bug-28：GeneralLedger / SubsidiaryLedger — 流水餘額對貸方正常科目顯示方向錯誤 ✅ 已修正

**影響：** `GeneralLedgerReportService`、`SubsidiaryLedgerReportService`

**現行問題：** 兩個報表的 `FormatBalance` 方法使用純「借正貸負」慣例：
```csharp
// 餘額計算（借正）
decimal openingBalance = openingDebit - openingCredit;   // 貸方正常科目 → 負數
runningBalance += entry.DebitAmount - entry.CreditAmount; // 同上

// 顯示
private static string FormatBalance(decimal balance)
    => balance > 0 ? balance.ToString("N2") : $"({Math.Abs(balance):N2})"; // 負數→括號
```

**後果：** 負債、權益、收入等「貸方正常科目」在正常的貸方餘額下，餘額欄永遠顯示括號（如 `(500,000.00)`）。會計慣例中括號代表「反方向異常餘額」，造成閱讀者誤判所有此類科目均異常。例：
- 應付帳款期末餘額 200,000（正常貸方）→ 顯示 `(200,000.00)` ← 看起來「欠人家借方錢」
- 正確顯示應為 `200,000.00`；括號應保留給應付帳款出現借方餘額時才使用

`AccountLedger` 已有 `NormalDirection` 欄位，問題在於 `FormatBalance` 未利用它。

**修正（2026-03-16）：** 兩個 Service 的 `FormatBalance` 加入 `AccountDirection normalDirection` 參數，依正常方向調整符號：
```csharp
private static string FormatBalance(decimal balance, AccountDirection normalDirection = AccountDirection.Debit)
{
    decimal displayBalance = normalDirection == AccountDirection.Debit ? balance : -balance;
    if (displayBalance == 0) return "0.00";
    return displayBalance > 0
        ? displayBalance.ToString("N2")
        : $"({Math.Abs(displayBalance):N2})";
}
```
列印時對所有 3 個呼叫點（期初餘額、逐筆流水、本期合計）傳入 `ledger.NormalDirection`。

---

### Bug-29：SetoffPrepaymentUsageService.DeleteBySetoffDocumentIdAsync — 刪除 usages 與更新 UsedAmount 不在同一交易 ✅ 已修正

**影響：** `SetoffPrepaymentUsageService.DeleteBySetoffDocumentIdAsync`

**現行問題：**
```csharp
// Step 1：刪除所有 usage（一個 context，一次 SaveChanges）
context.SetoffPrepaymentUsages.RemoveRange(usages);
await context.SaveChangesAsync();  // commit 1 — usages 已永久刪除

// Step 2：逐一更新各 prepayment 的 UsedAmount（各自開新 context）
foreach (var prepaymentId in affectedPrepaymentIds)
    await UpdatePrepaymentUsedAmountAsync(prepaymentId); // 每次獨立 commit，內部吞例外
```
若 `UpdatePrepaymentUsedAmountAsync` 中途失敗（例外被內部 catch 吞掉），方法仍回傳 `ServiceResult.Success()`，但部分 prepayment 的 `UsedAmount` 仍停留在舊值。業務上造成「usages 已刪除，但 prepayment 顯示仍有使用金額」，後續驗證會錯誤地拒絕合法的新使用操作。

**修正（2026-03-16）：** 在同一 context 內加入明確交易，刪除 usages 後在同一追蹤上下文重新計算並更新 prepayments，最後一次 `CommitAsync`：
```csharp
await using var transaction = await context.Database.BeginTransactionAsync();

context.SetoffPrepaymentUsages.RemoveRange(usages);
await context.SaveChangesAsync();

foreach (var prepaymentId in affectedPrepaymentIds)
{
    var totalUsed = await context.SetoffPrepaymentUsages
        .Where(u => u.SetoffPrepaymentId == prepaymentId)
        .SumAsync(u => u.UsedAmount); // 在同一 context 查詢，已反映刪除後的結果

    var prepayment = await context.SetoffPrepayments.FindAsync(prepaymentId);
    if (prepayment != null) { prepayment.UsedAmount = totalUsed; prepayment.UpdatedAt = DateTime.UtcNow; }
}

await context.SaveChangesAsync();
await transaction.CommitAsync();
```
若任一步驟失敗，交易自動回滾，usages 和 prepayment UsedAmount 保持一致。

---

### Bug-30：DetailAccountBalanceReportService — FormatBalance 同 Bug-28，未考慮 NormalDirection ✅ 已修正

**影響：** `DetailAccountBalanceReportService`（明細科目餘額表）

**現行問題：** 與 Bug-28 完全相同的問題模式。`AccountBalanceLine` 已攜帶 `NormalDirection`（line 188），但列印段落的所有 `FormatBalance` 呼叫點（個別科目 line 269/272、各大類小計 line 285/288）均未傳入方向參數，導致負債/權益/收入類科目的正常貸方餘額以括號呈現，看起來像「異常」。

**修正（2026-03-16）：**
- `FormatBalance` 加入 `AccountDirection normalDirection = AccountDirection.Debit` 參數（同 Bug-28 修法）
- 個別科目行：傳入 `item.NormalDirection`
- 各大類小計行：因同一大類內 `NormalDirection` 相同，取 `group.First().NormalDirection` 傳入

---

### Bug-31：AccountItemService.ValidateAsync — 未防止科目自我參照（ParentId == Id）✅ 已修正

**影響：** `AccountItemService.ValidateAsync`

**現行問題：** 若使用者將某科目的「上層科目」設定為自身（`ParentId == entity.Id`），驗證完全通過，DB 中形成自我指向的循環參照。任何向上走訪父節點的程式碼將陷入無窮迴圈；EF Core 在 Include 子科目樹時也可能造成無窮遞迴。

**修正（2026-03-16）：** 在 `ValidateAsync` 加入明確檢查：
```csharp
if (entity.Id > 0 && entity.ParentId.HasValue && entity.ParentId.Value == entity.Id)
    errors.Add("科目不可設定自身為上層科目");
```

---

### Bug-32：AccountItemService.ValidateAsync — 子科目層級未驗證必須大於父科目層級 ✅ 已修正

**影響：** `AccountItemService.ValidateAsync`

**現行問題：** 驗證只確認 `AccountLevel` 介於 1-4，但未驗證子科目層級必須大於父科目層級。例如可在 Level 3 的科目下建立 Level 1 的子科目，形成語意矛盾的層級關係，破壞科目樹的整體完整性。

**修正（2026-03-16）：** 若 `ParentId` 有值，則讀取父科目的 `AccountLevel`，並驗證 `entity.AccountLevel > parent.AccountLevel`：
```csharp
if (entity.ParentId.HasValue && entity.ParentId.Value != entity.Id)
{
    var parent = await parentCtx.AccountItems.AsNoTracking()
        .FirstOrDefaultAsync(a => a.Id == entity.ParentId.Value);
    if (parent != null && entity.AccountLevel <= parent.AccountLevel)
        errors.Add($"子科目層級（{entity.AccountLevel}）必須大於上層科目層級（{parent.AccountLevel}）");
}
```

---

### Bug-33：SetoffDocumentService.PermanentDeleteAsync — 未防止刪除已轉傳票的沖款單 ✅ 已修正

**影響：** `SetoffDocumentService.PermanentDeleteAsync`（及 `DeleteAsync`，後者直接呼叫前者）

**現行問題：** `PermanentDeleteAsync` 呼叫基礎類別的 `CanDeleteAsync`，該方法只檢查外鍵參照（FK references），完全未檢查 `document.IsJournalized`。結果是：已轉傳票的沖款單可被直接刪除，留下孤兒 `JournalEntry`（傳票的 `SourceDocumentId` 指向已不存在的沖款單），造成稽核日誌斷鏈與財務資料不一致。

**修正（2026-03-16）：** 在載入文件後、呼叫 `CanDeleteAsync` 前，加入明確的 `IsJournalized` 防衛：
```csharp
if (document.IsJournalized)
{
    await transaction.RollbackAsync();
    return ServiceResult.Failure("此沖款單已轉傳票，無法刪除。請先沖銷對應傳票後再操作。");
}
```

---

### Bug-34：CustomerStatementReportService / SupplierStatementReportService — GetAllAsync 無 CompanyId 篩選 ✅ 評估後關閉

**影響：** `CustomerStatementReportService`（line 91）、`SupplierStatementReportService`（對應行）

**重新評估結論（2026-03-16）：**
- `SalesDelivery`、`SalesReturn`、`PurchaseReceiving`、`PurchaseReturn` 等 Entity **無 `CompanyId` 欄位**，屬於單公司設計。
- 對帳單 Service 改呼叫有 `GetByDateRangeAsync()` 方法，按日期篩選資料，無法加 CompanyId 篩選。
- 此為系統整體多公司隔離缺口（已在 Bug-5 涵蓋），而非對帳單 Service 特有問題。

**現況：** 此項目為 Bug-5（多公司隔離）的子集，不另行追蹤。

---

### Bug-35：SetoffDocumentService.IsSetoffNumberExistsAsync — 缺少 CompanyId 篩選 ✅ 評估後關閉

**影響：** `SetoffDocumentService.IsSetoffNumberExistsAsync`（line 171）

**重新評估結論（2026-03-16）：**
- `AppDbContext` 對 `SetoffDocument.Code` 建立了**全系統唯一索引**（`entity.HasIndex(e => e.Code).IsUnique()`，AppDbContext.cs line 826）。
- `IsSetoffNumberExistsAsync` 不含 CompanyId 篩選，**與 DB 層唯一性語義完全一致**，兩者均為全系統唯一。
- 若要改為每公司唯一，必須同時修改 DB 索引（複合索引 `Code + CompanyId`）與 Service。
- 此為**設計決策**，並非程式碼矛盾，不列為 Bug。

---

### Bug-36：CustomerStatementReportService / SupplierStatementReportService — earlyDate 使用魔術數字 2000-01-01 ✅ 已修正

**影響：** `CustomerStatementReportService`（line 166）、`SupplierStatementReportService`（line 165）

**現行問題：**
```csharp
// 修正前
var earlyDate = new DateTime(2000, 1, 1);
var preReturns = await _salesReturnService.GetByDateRangeAsync(earlyDate, startDate.AddDays(-1));
```
若公司在 2000 年以前有退貨記錄，期初餘額計算將遺漏這些資料，導致期初餘額偏高（應沖抵的金額未被扣除）。以 `DateTime.MinValue` 取代才能正確涵蓋所有歷史資料。

**修正（2026-03-16）：** 移除 `earlyDate` 變數，直接傳入 `DateTime.MinValue`：
```csharp
// 修正後（兩個 Service 均已更新）
var preReturns = await _salesReturnService.GetByDateRangeAsync(DateTime.MinValue, startDate.AddDays(-1));
var preReturns = await _purchaseReturnService.GetByDateRangeAsync(DateTime.MinValue, startDate.AddDays(-1));
```

---

### Bug-37：SetoffDocumentService.PermanentDeleteAsync — 刪除使用預收付款的沖款單時 PrepaymentUsages 未清理 ✅ 已修正

**影響：** `SetoffDocumentService.PermanentDeleteAsync`

**現行問題：** `SetoffPrepaymentUsage.SetoffDocumentId` 在資料庫設定 `OnDelete(DeleteBehavior.Restrict)`（避免循環刪除）。`PermanentDeleteAsync` 只 Include `Prepayments`（owned 預收付款），未 Include `PrepaymentUsages`（此沖款單消費的預收付款使用記錄）。若沖款單有任何 `PrepaymentUsages`：
1. 直接呼叫 `context.SetoffDocuments.Remove(document)` 會觸發 DB Restrict 約束錯誤
2. 即使強制刪除，`SetoffPrepayment.UsedAmount` 不會自動更新，造成預收付款「已用金額」虛高

**後果：** 沖款單含預收付款使用時，刪除操作返回「刪除沖款單時發生錯誤: ...」（非業務導向的 DB 錯誤訊息），且 `SetoffPrepayment.UsedAmount` 若未更新，後續驗證可能誤拒合法使用。

**修正（2026-03-16）：**
1. 查詢時加入 `.Include(d => d.PrepaymentUsages)`
2. 刪除文件前，先明確刪除 `PrepaymentUsages`，再依 Bug-29 同樣模式重新計算各 `SetoffPrepayment.UsedAmount`，最後才移除沖款單主體，全程在同一交易內：
```csharp
if (document.PrepaymentUsages.Any())
{
    var affectedPrepaymentIds = document.PrepaymentUsages
        .Select(pu => pu.SetoffPrepaymentId).Distinct().ToList();
    context.SetoffPrepaymentUsages.RemoveRange(document.PrepaymentUsages);
    await context.SaveChangesAsync();
    foreach (var prepaymentId in affectedPrepaymentIds)
    {
        var totalUsed = await context.SetoffPrepaymentUsages
            .Where(u => u.SetoffPrepaymentId == prepaymentId)
            .SumAsync(u => u.UsedAmount);
        var prepayment = await context.SetoffPrepayments.FindAsync(prepaymentId);
        if (prepayment != null) { prepayment.UsedAmount = totalUsed; prepayment.UpdatedAt = DateTime.UtcNow; }
    }
    await context.SaveChangesAsync();
}
// 接著才 context.SetoffDocuments.Remove(document)
```

---

### Bug-38：SetoffPrepaymentService.ValidateAsync — 編輯模式跳過來源單號唯一性檢查 ✅ 已修正

**影響：** `SetoffPrepaymentService.ValidateAsync`（line 188）

**現行問題：**
```csharp
// 修正前：僅在新增模式（Id = 0）檢查唯一性，編輯模式完全跳過
if (!string.IsNullOrWhiteSpace(entity.SourceDocumentCode) && entity.Id == 0)
{
    var exists = await IsSourceDocumentCodeExistsAsync(entity.SourceDocumentCode, null);
    ...
}
```
`IsSourceDocumentCodeExistsAsync` 已支援 `excludeId` 參數，但編輯模式（`Id > 0`）直接跳過整個檢查。使用者可編輯預收付款將 `SourceDocumentCode` 改成另一筆記錄已使用的單號，驗證通過後造成系統內存在重複來源單號，破壞對帳一致性。

**修正（2026-03-16）：** 移除 `entity.Id == 0` 限制，新增與編輯均檢查，編輯時傳入 `entity.Id` 作為 `excludeId`：
```csharp
// 修正後
if (!string.IsNullOrWhiteSpace(entity.SourceDocumentCode))
{
    var exists = await IsSourceDocumentCodeExistsAsync(entity.SourceDocumentCode, entity.Id == 0 ? null : entity.Id);
    if (exists)
        errors.Add("來源單號已存在");
}
```

---

### Bug-39：SetoffDocumentService — RollbackSourceDetailAmountAsync 與 RebuildCacheByTypeAsync 遺漏 SalesDeliveryDetail case ✅ 已修正

**影響：** `SetoffDocumentService.RollbackSourceDetailAmountAsync`、`SetoffDocumentService.RebuildCacheByTypeAsync`

**背景：** Bug-2 已將應收帳款沖款來源從 `SalesOrderDetail`（廢棄）改為 `SalesDeliveryDetail = 5`。新建立的沖款明細 `SourceDetailType` 皆為 `SalesDeliveryDetail`，對應的快取欄位為 `SalesDeliveryDetail.TotalReceivedAmount` + `IsSettled`。

**現行問題：** 兩個方法的 `switch` 語句均包含 `SalesOrderDetail` case，但完全沒有 `SalesDeliveryDetail` case（落入 `default: break`）：

1. **`RollbackSourceDetailAmountAsync`**：刪除沖款單時，`SalesDeliveryDetail` 類型的明細不會更新 `TotalReceivedAmount` 和 `IsSettled`，刪除後快取金額仍顯示已結清狀態，後續系統誤認該出貨明細已被沖款，導致重複計算或拒絕合法的新沖款操作。

2. **`RebuildCacheByTypeAsync`**：管理員觸發快取重建工具時，`SalesDeliveryDetail` 類型完全被跳過，重建結果不完整。

**修正（2026-03-16）：** 在兩個 switch 語句中各加入 `SalesDeliveryDetail` case：
```csharp
case SetoffDetailType.SalesDeliveryDetail:
    var salesDeliveryDetail = await context.SalesDeliveryDetails.FindAsync(detailToDelete.SourceDetailId);
    if (salesDeliveryDetail != null)
    {
        salesDeliveryDetail.TotalReceivedAmount = newTotalSetoff;
        salesDeliveryDetail.IsSettled = newTotalSetoff >= salesDeliveryDetail.SubtotalAmount;
    }
    break;
```

---

### Bug-40：SetoffDocumentService.PermanentDeleteAsync — RollbackSourceDetailAmountAsync 使用錯誤欄位導致來源明細快取錯誤 ✅ 已修正

**影響：** `SetoffDocumentService.PermanentDeleteAsync`、`RollbackSourceDetailAmountAsync`

**現行問題：** `RollbackSourceDetailAmountAsync` 計算 `newTotalSetoff` 時使用：
```csharp
SumAsync(spd => spd.TotalSetoffAmount)
```
`TotalSetoffAmount` 是每筆 `SetoffProductDetail` **建立當下**的累計快照，並非各筆記錄的個別貢獻值（`CurrentSetoffAmount`）。若同一來源有 3 筆記錄（A=100、B=200、C=300），其 TotalSetoffAmount 為 100、300、600，加總為 1000，遠超過正確答案 600。

更嚴重的問題：`PermanentDeleteAsync` 在迴圈中對同一文件的每筆明細分別呼叫 `RollbackSourceDetailAmountAsync`，但每次只排除「當前那一筆」，其他同文件的明細仍被計入。刪除含 A、B 的文件（另有來自其他文件的 C=300）時：

| 迭代 | 排除 | 計算結果 | 正確值 |
|------|------|---------|--------|
| 處理 A | 排除 A | Sum(B.Total + C.Total) = 300 + 600 = **900** | 300（C 獨享） |
| 處理 B | 排除 B | Sum(A.Total + C.Total) = 100 + 600 = **700** | 300 |
最終快取 = 700，正確值應為 300（= C.CurrentSetoffAmount）。

**後果：** 刪除沖款單後，來源明細（SalesDeliveryDetail、PurchaseReceivingDetail 等）的 `TotalReceivedAmount` / `TotalPaidAmount` 偏高，`IsSettled` 可能錯誤標記為「已結清」，導致後續建立的沖款無法通過餘額驗證。

**修正（2026-03-16）：**
1. 在 `PermanentDeleteAsync` 刪除文件**之前**，收集所有受影響的來源 `(SourceDetailId, SourceDetailType)`
2. 刪除文件（`context.SetoffDocuments.Remove(document)` + `SaveChangesAsync`）— 級聯刪除所有 `SetoffProductDetails`
3. 刪除後，對每個受影響來源在**同一 context**（反映刪除後的狀態）呼叫新方法 `RebuildSourceDetailCacheAsync`，從頭計算：`SUM(CurrentSetoffAmount + CurrentAllowanceAmount)` for REMAINING records
4. 移除舊的 `RollbackSourceDetailAmountAsync` 迴圈

新增 `RebuildSourceDetailCacheAsync` 方法處理所有 5 種 `SetoffDetailType`（PurchaseReceiving/SalesOrder/SalesDelivery/SalesReturn/PurchaseReturn）。

---

### Bug-41：SetoffProductDetailService.CreateBatchWithValidationAsync — 來源明細快取更新無交易保護、返回值未檢查 ✅ 已修正

**影響：** `SetoffProductDetailService.CreateBatchWithValidationAsync`（lines 857-874）

**現行問題：**
1. **無交易保護**：`SaveChangesAsync`（line 862）儲存明細後，呼叫 `UpdateSourceDetailTotalAmountAsync`（line 871）——後者內部建立獨立 context，若執行失敗，明細已永久儲存但來源明細快取（TotalReceivedAmount、IsSettled）仍為舊值，且無法回滾。
2. **返回值被忽略**：`UpdateSourceDetailTotalAmountAsync` 返回 `ServiceResult.Failure` 時，呼叫端沒有捕捉，方法仍回傳 `ServiceResult.Success()`，造成 UI 顯示成功但快取實際未更新。

**後果：** 若 `UpdateSourceDetailTotalAmountAsync` 失敗（例如連線中斷），`SalesDeliveryDetail.TotalReceivedAmount` 仍顯示舊值，導致：
- 相同來源明細在下次沖款時餘額驗證基於舊（過低的）已收金額，允許超額沖款
- `IsSettled` 標記錯誤，影響未結清明細篩選

**修正（2026-03-16）：**
1. 在 `CreateBatchWithValidationAsync` 加入明確交易（`await using var transaction`）
2. 新增私有方法 `UpdateSourceDetailCacheInContextAsync`（接受現有 context，不自建 context，不自呼 `SaveChangesAsync`），涵蓋所有 5 種 `SetoffDetailType`，含稅計算邏輯與 `UpdateSourceDetailTotalAmountAsync` 一致
3. 在同一 context 內先 `SaveChanges`（儲存明細），再呼叫 `UpdateSourceDetailCacheInContextAsync` 更新快取，最後統一 `SaveChanges` + `CommitAsync`
4. 如此明細儲存與快取更新原子完成；任一步驟失敗，交易自動回滾

---

### Bug-42：SetoffDocumentService.RebuildCacheByTypeAsync — 使用 TotalSetoffAmount（快照）而非 CurrentSetoffAmount（個別貢獻）✅ 已修正

**影響：** `SetoffDocumentService.RebuildCacheByTypeAsync`

**現行問題（修正前）：** 管理員觸發快取重建的工具方法，5 個 case 均使用：
```csharp
.SumAsync(spd => spd.TotalSetoffAmount)
```
`TotalSetoffAmount` 是每筆 `SetoffProductDetail` **建立當下**的累計快照（含先前所有記錄的總和），不是該筆的個別貢獻值。

**雙重計算舉例：** 同一來源明細有三筆 SetoffProductDetail（A=100、B=200、C=300）：
- A.TotalSetoffAmount = 100（建立時只有 A）
- B.TotalSetoffAmount = 300（建立時有 A+B）
- C.TotalSetoffAmount = 600（建立時有 A+B+C）
- SUM(TotalSetoffAmount) = 1000 ← 遠超正確答案 600

**後果：** `RebuildCacheByTypeAsync` 作為管理員的「資料修復工具」，使用後反而將來源明細的 `TotalReceivedAmount` / `TotalPaidAmount` 設成嚴重偏高的錯誤值，`IsSettled` 全部被標記為已結清。原本要修復資料，反而造成更大的帳務混亂。

**修正（2026-03-16）：** 全部 5 個 case 改為使用各筆記錄的個別貢獻值：
```csharp
// 修正前（全部 5 個 case）
.SumAsync(spd => spd.TotalSetoffAmount)

// 修正後（全部 5 個 case）
.SumAsync(spd => spd.CurrentSetoffAmount + spd.CurrentAllowanceAmount)
```

---

### Bug-43：SetoffDocumentService — RebuildSourceDetailCacheAsync / RebuildCacheByTypeAsync 的 IsSettled 使用含稅前金額比較 ✅ 已修正

**影響：** `SetoffDocumentService.RebuildSourceDetailCacheAsync`、`SetoffDocumentService.RebuildCacheByTypeAsync`

**現行問題（修正前）：** 兩個方法計算 `IsSettled` 時直接用 `SubtotalAmount`（含稅前）比較，而 `SetoffProductDetailService.UpdateSourceDetailCacheInContextAsync`（建立/更新沖款時呼叫）正確使用 `CalculateTaxInclusiveAmount(subtotal, taxRate, parentDoc.TaxCalculationMethod)`（含稅後）。

**典型情境（5% 外加稅）：**
- 品項 `SubtotalAmount = 10,000`，含稅 `10,500`
- 建立沖款時：`IsSettled = totalReceived >= 10,500` ← 正確
- 刪除沖款後 RebuildSourceDetailCacheAsync：`IsSettled = remaining >= 10,000` ← 錯誤（偏低 500 即結清）
- 管理員觸發 RebuildCacheAsync（呼叫 RebuildCacheByTypeAsync）：`IsSettled = total >= 10,000` ← 同樣錯誤

**後果：** 外加稅（TaxExclusive）情況下，刪除沖款單後或管理員執行修復工具後，部分來源明細會被提早標記為「已結清」（`IsSettled = true`），導致後續建立沖款時這些明細不再出現於「未結清」清單，喪失剩餘 5% 稅額的沖款機會。

**修正（2026-03-16）：**
1. 在 `SetoffDocumentService` 新增與 `SetoffProductDetailService.CalculateTaxInclusiveAmount` 邏輯相同的本地靜態方法：
```csharp
private static decimal CalculateTaxInclusiveAmount(decimal subtotal, decimal? taxRate, TaxCalculationMethod taxMethod)
{
    var rate = taxRate ?? 0;
    return taxMethod switch
    {
        TaxCalculationMethod.TaxExclusive => Math.Round(subtotal * (1 + rate / 100), 0),
        _ => Math.Round(subtotal, 0)
    };
}
```
2. `RebuildSourceDetailCacheAsync`：5 個 case 從 `FindAsync` 改為 `.Include(父導覽).FirstOrDefaultAsync`，`IsSettled` 改用含稅金額比較
3. `RebuildCacheByTypeAsync`：5 個 case 從 `.ToListAsync()` 改為 `.Include(父導覽).ToListAsync()`，`IsSettled` 同步修正

---

### Bug-44：JournalizeSetoffDocumentAsync — 折讓與新預收付款沖款分錄借貸不平衡 ✅ 已修正

**影響：** `JournalEntryAutoGenerationService.JournalizeSetoffDocumentAsync`

**現行問題（修正前）：**

UI 驗證的平衡公式：`TotalCollectionAmount + PrepaymentSetoffAmount = CurrentSetoffAmount + TotalAllowanceAmount + CurrentPrepaymentAmount`
（其中 `TotalCollectionAmount = 現金 + TotalAllowanceAmount`，折讓從付款記錄與品項明細兩側同時記錄）

應收沖款分錄（原始）：
- 借方：銀行存款 `TotalCollectionAmount`（含折讓！）+ 銷貨折讓 `TotalAllowanceAmount` + 預收貨款沖回 `PrepaymentSetoffAmount`
- 貸方：應收帳款 `CurrentSetoffAmount`（不含折讓！）

例：現金 800、折讓 200，則：
- 借方 = (800+200) + 200 + 0 = 1200
- 貸方 = 800
- 差額 400（= 2×折讓）→ `PostEntryAsync` 強制報錯「借貸不平衡」，沖款單無法轉傳票。

同時缺少 `CurrentPrepaymentAmount`（本期新建預收/預付）的對應分錄，導致建立新預收款時也必然不平衡。

**後果：** 只要沖款單含折讓（`TotalAllowanceAmount > 0`）或含新建預收付款項（`CurrentPrepaymentAmount > 0`），轉傳票必定失敗。

**修正（2026-03-16）：**
正確的分錄邏輯（AR 應收沖款，AP 對稱相反）：
```
借方：
  銀行存款  = TotalCollectionAmount - TotalAllowanceAmount  （純現金，不含折讓）
  銷貨折讓  = TotalAllowanceAmount                          （若 > 0）
  預收沖回  = PrepaymentSetoffAmount                        （若 > 0）
貸方：
  應收帳款  = CurrentSetoffAmount + TotalAllowanceAmount    （全額應收消除）
  預收新增  = CurrentPrepaymentAmount                       （若 > 0，客戶多付款建立預收）
```
平衡驗證：借 = (現金) + 折讓 + 預收沖回 = (現金 + 預收沖回 - 新預收 + 折讓) + 新預收 = 貸 ✓

AP 應付沖款對稱修正：AP借方改為 `CurrentSetoffAmount + TotalAllowanceAmount`，銀行貸方改為 `TotalCollectionAmount - TotalAllowanceAmount`，新增預付新增借方 `CurrentPrepaymentAmount`（若 > 0）。

---

### Bug-45：SetoffDocumentService.PermanentDeleteAsync — 刪除含預收/預付款項的沖款單可能靜默破壞其他沖款單 ✅ 已修正

**影響：** `SetoffDocumentService.PermanentDeleteAsync`

**現行問題（修正前）：**
`SetoffPrepaymentUsage.SetoffPrepaymentId` 設為 `DeleteBehavior.Cascade`，意即刪除 `SetoffPrepayment` 時，所有引用它的 `SetoffPrepaymentUsage` 都會被級聯刪除。

當 Document A 被刪除時：
1. `PermanentDeleteAsync` 先手動刪除 Document A 的 `PrepaymentUsages`（因 `SetoffDocumentId` 為 Restrict）✓
2. 然後刪除 Document A 本身 → 級聯刪除 Document A 的 `Prepayments`
3. 第 2 步進一步級聯刪除使用這些 Prepayments 的所有 `SetoffPrepaymentUsage` 記錄，**包括屬於 Document B 的記錄**

Document B 的 `PrepaymentSetoffAmount` 欄位仍保留舊值，但 UsageRecord 已被靜默刪除，造成帳務資料不一致。`CanDeleteAsync`（base class 反射版）不會檢查此級聯影響。

**後果：** 若 Document A 建立的預收款被 Document B 使用，刪除 Document A 時靜默破壞 Document B 的資料完整性，且不返回任何錯誤訊息。

**修正（2026-03-16）：** 在 `PermanentDeleteAsync` 的 `IsJournalized` 檢查之後、`CanDeleteAsync` 之前加入明確guard：
```csharp
if (document.Prepayments.Any())
{
    var prepaymentIds = document.Prepayments.Select(p => p.Id).ToList();
    var hasExternalUsage = await context.SetoffPrepaymentUsages
        .AnyAsync(pu => prepaymentIds.Contains(pu.SetoffPrepaymentId) && pu.SetoffDocumentId != id);

    if (hasExternalUsage)
    {
        await transaction.RollbackAsync();
        return ServiceResult.Failure("此沖款單的預收/預付款項已被其他沖款單使用，無法刪除。請先清除相關使用記錄後再操作。");
    }
}
```

---

### Bug-46：ReverseEntryAsync — 沖銷後 UX_JournalEntry_SourceDocument 唯一索引阻擋重新轉傳票 ✅ 已修正（2026-03-16）

**影響：** `Services/FinancialManagement/JournalEntryService.cs` → `ReverseEntryAsync`

**現行問題：**
Bug-4 修正讓 `ReverseEntryAsync` 在沖銷傳票後重置來源業務單據的 `IsJournalized = false`，使其重新出現在待轉傳票清單。但原始傳票（已設為 `Reversed` 狀態）的 `SourceDocumentType` 與 `SourceDocumentId` 欄位仍保留原值。

`AppDbContext` 定義了 `UX_JournalEntry_SourceDocument` 唯一過濾索引：
- 覆蓋範圍：`SourceDocumentType IS NOT NULL` 的**所有**傳票（不分狀態，包含 `Reversed`）
- 欄位組合：`(SourceDocumentType, SourceDocumentId)`

因此重新轉傳票時：
1. `GetBySourceDocumentAsync` 防重複保護**通過**（該方法排除 Reversed/Cancelled 傳票）
2. INSERT 新傳票時，`SourceDocumentType` + `SourceDocumentId` 與原始 Reversed 傳票相同
3. **DB 拋出唯一索引違反例外**，轉傳票失敗

**後果：** 使用者沖銷傳票並修正業務單據後，無法重新轉傳票，系統顯示不明確的 DB 錯誤。

**修正（2026-03-16）：** 在呼叫 `ResetSourceDocumentJournalizedAsync` 後（需要 Type/Id 值才能找到來源單據），立即將原始傳票的 `SourceDocumentType = null`、`SourceDocumentId = null`，釋放唯一索引佔位。`SourceDocumentCode`（字串）保留供稽核查詢，不清除。

```csharp
if (!string.IsNullOrWhiteSpace(original.SourceDocumentType) && original.SourceDocumentId.HasValue)
{
    await ResetSourceDocumentJournalizedAsync(context, original.SourceDocumentType, original.SourceDocumentId.Value);
    // 清除原傳票的來源單據類型和 ID，釋放 UX_JournalEntry_SourceDocument 唯一索引佔位（修正 Bug-46）
    original.SourceDocumentType = null;
    original.SourceDocumentId = null;
}
```

---

### Bug-12：SubAccountService 子科目批次建立競態條件（待修正）

**影響：** `SubAccountService.GetOrCreateAllSubAccountsAsync`

**現行問題：** `GetOrCreateAllSubAccountsAsync` 在迴圈中對每個 Customer/Supplier 呼叫 `GetOrCreateCustomerSubAccountAsync`，後者先查詢 `LinkedCustomerId == id` 是否存在，若無則 INSERT。並行兩個請求同時進入「不存在」分支後，第二個請求的 INSERT 將因唯一索引（或業務唯一規則）拋出例外，但目前未有重試或衝突處理。

**建議修正：** 加 `UNIQUE` 索引保護 `(ParentAccountItemId, LinkedCustomerId)` / `(ParentAccountItemId, LinkedSupplierId)`，並在 Service 層 catch `DbUpdateException` 後重試讀取（樂觀並行模式）。

---

### Bug-47：SetoffPaymentService.ValidateAsync — 未驗證 ReceivedAmount 與 AllowanceAmount 金額合法性 ✅ 已修正（2026-03-16）

**影響：** `Services/FinancialManagement/SetoffPaymentService.cs` → `ValidateAsync`

**現行問題：** `ValidateAsync` 只驗證 `SetoffDocumentId`（必填）與支票號碼唯一性，完全未驗證：
- `ReceivedAmount` 不能為負數
- `AllowanceAmount` 不能為負數
- 兩者不能同時為零（無意義的收款記錄）

UI 層有 `min=0` 的輸入限制，但 Service 層缺少防護，導致直接呼叫 API 可提交負數金額或全零記錄，污染財務資料。

**修正：** 在 ValidateAsync 中加入三項金額驗證：
```csharp
if (entity.ReceivedAmount < 0)
    errors.Add("收款金額不能為負數");
if (entity.AllowanceAmount < 0)
    errors.Add("折讓金額不能為負數");
if (entity.ReceivedAmount == 0 && entity.AllowanceAmount == 0)
    errors.Add("收款金額與折讓金額不能同時為零");
```

---

### Bug-48：SetoffDocumentEditModalComponent.IsReadOnly 硬寫 false — 已傳票化沖款單可任意修改 ✅ 已修正（2026-03-16）

**影響：**
- `Components/Pages/FinancialManagement/SetoffDocumentEditModalComponent.razor` → line 99
- `Services/FinancialManagement/SetoffDocumentService.cs` → `ValidateAsync`

**現行問題：** `private bool IsReadOnly => false;` 硬寫為 `false`，此屬性被傳遞至三個子表格（SetoffProductTable、SetoffPaymentTable、SetoffPrepaymentTable），導致即使沖款單已傳票化（`IsJournalized = true`），使用者仍可透過 UI 新增、修改、刪除品項明細、收款記錄及預收付款項，破壞已產生傳票的會計憑證完整性。`SetoffDocumentService.ValidateAsync` 也未對 `IsJournalized` 單據加以攔截。

**修正：**
1. `SetoffDocumentEditModalComponent.razor` line 99：
```csharp
private bool IsReadOnly => editModalComponent?.Entity?.IsJournalized == true;
```
2. `SetoffDocumentService.ValidateAsync` 開頭加入：
```csharp
if (entity.IsJournalized && entity.Id > 0)
    return ServiceResult.Failure("沖款單已傳票化，不可再修改");
```

---

## 七、Phase 4 缺口（低優先 🟢）

| 項目 | 狀態 |
|------|------|
| FN014 現金流量表 Service | 完全未實作 |
| 所有報表 Excel 匯出 | 完全未實作 |
| `BankStatement` Entity（銀行對帳） | 完全未實作 |
| `BankStatementLine` Entity | 完全未實作 |
| `JournalEntryAttachment` Entity（傳票附件） | 完全未實作 |

---

## 七、待決策彙總

| # | 問題 | 影響 Phase | 狀態 | 決策 |
|---|------|-----------|------|------|
| 1 | FiscalPeriod FK vs 整數查詢 | P1-A | ✅ 已決策 | 維持整數，Service 層驗證（不加 FK） |
| 2 | FiscalPeriod 初始化策略 | P1-A | ✅ 已決策 | 混合式：當年度自動建立，過去期間自動建立+警告，Closed/Locked 嚴格阻擋 |
| 3 | 期初餘額借貸平衡規則 | P1-B | ✅ 已決策 | **強制平衡**，不允許不平衡過帳（舊設計允許需修正） |
| 4 | 期初餘額修正方式 | P1-B | ✅ 已決策 | 草稿可直接編輯；已過帳只能建調整分錄（移除重設按鈕） |
| 5 | 薪資傳票整合 | P2-A | ⏸ 暫緩 | 等薪資模組重新設計完成後處理 |
| 6 | 材料領用用途分類欄位 | P2-B | 🔲 待決策 | 建議新增 `MaterialIssuePurpose` Enum |
| 7 | 材料領用 UnitCost 填入時機 | P2-B | 🔲 待確認 | 確認領料確認流程是否自動填入移動均價 |
| 8 | 生產完工成本計算方式 | P2-C | ✅ 已決策 | 取 `InventoryTransaction.TotalAmount`（與 COGS 邏輯一致） |
| 9 | 生產完工 IsJournalized 層級 | P2-C | ✅ 已決策 | 加在 `ProductionScheduleCompletion`，非 `ProductionScheduleItem` |
| 10 | 帳齡計算單位 | P3-A | ✅ 已決策 | SalesDelivery / PurchaseReceiving 主檔層級 |
| 11 | 帳齡天數基準欄位 | P3-A | ✅ 已決策 | `Customer.PaymentDays` / `Supplier.PaymentDays` 欄位不存在，需新增 int 欄位（各客戶不同天數，預設 30） |
| 12 | 帳齡金額基準（含稅 vs 稅前） | P3-A | ✅ 已決策 | 使用含稅（`TotalAmount + TaxAmount`）；SetoffProductDetail 同樣含稅，口徑一致 |
| 13 | `DiscountAmount` 是否需從 AR 扣除 | P3-A | ✅ 已確認 | 無需扣除；`TotalAmount` = `Sum(SubtotalAmount)`，每行 `SubtotalAmount` 已含行項折扣 |
| 14 | 會計功能操作權限設計 | 全 Phase | ✅ 已決策 | 不用 SuperAdmin；改用 `Accounting.*` 系列權限，透過 Role+Permission 系統授予指定員工 |
| 15 | 年底結帳冪等性保護 | P3-B | ✅ 已決策 | 執行前檢查該年度是否已有 `JournalEntryType.Closing` 傳票，若有則拒絕重複執行 |
| 16 | FiscalPeriod 重開（Reopen）機制 | P1-A | ✅ 已決策 | 補充 Closed → Open 重開功能（需 `Accounting.ClosePeriod` 權限）；Locked 永遠不可重開 |
| 17 | 期初餘額傳票豁免期間鎖定 | P1-B | ✅ 已決策 | 不豁免；混合式初始化自動建立對應期間（Open），正常走期間驗證即可 |
| 18 | 沖銷傳票後 IsJournalized 重置策略 | Bug-4 | ✅ 已決策 | 重置為 false（可補轉）— `ResetSourceDocumentJournalizedAsync` 在交易內執行（2026-03-16）|
| 19 | 沖銷傳票雙向索引 | Bug-3 | 🔲 待決策 | `JournalEntry` 新增 `ReversedEntryId` (int?) 反向指回原傳票 |
| 20 | 財務報表 CompanyId 篩選 | Bug-5 | ✅ 確認需要 | 多公司支援需求確認，各 Criteria 加入 CompanyId 參數 |
| 21 | SalesOrderDetail 從沖款流程移除 | Bug-2 | ✅ 確認需移除 | 沖款只允許出貨/入庫/退回，訂單未成立 AR 不可沖款 |

---

## 八、實作順序建議

```
Phase 1（必須優先）
  ├─ P1-A：FiscalPeriod Entity + Service + UI           ← 所有關帳/鎖定的基礎
  │          含：ReverseEntryAsync 期間驗證
  │          含：混合式初始化（自動建立 + 警告）
  ├─ P1-B：OpeningBalance Enum + 精靈頁面（強制借貸平衡）
  └─ P1-C：FN006 六欄格式修正

Phase 2（Phase 1 後執行）
  ├─ P2-B 前置：MaterialIssue 補充 IsConfirmed 欄位（Mission Critical）
  ├─ P2-B：決策用途分類 → 新增欄位 → 確認 UnitCost 填入機制 → 實作材料領用傳票
  ├─ P2-C：IsJournalized 加到 Completion 層 → 確認 InventoryTransactionId nullable 處理 → 實作完工傳票
  └─ P2-A：薪資暫緩（等薪資模組重新設計）

Phase 3（可與 Phase 2 平行推進）
  ├─ P3-C：業務單據顯示相關傳票（✅ 最快，立即可做，後端已完備）
  ├─ P3-A：確認 Customer/Supplier.PaymentDays → 建立帳齡分析 Service + 報表
  └─ P3-B：年底結帳（需 P1-A 完成，自動初始化下年度期間）

Phase 4（最後執行）
  ├─ P4-B：Excel 匯出
  ├─ P4-A：現金流量表（依賴 Phase 1 期初餘額）
  ├─ P4-C：銀行對帳
  └─ P4-D：傳票附件
```

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_Phase1_基礎補強.md](README_會計_Phase1_基礎補強.md)
- [README_會計_Phase2_業務整合.md](README_會計_Phase2_業務整合.md)
- [README_會計_Phase3_帳務管理.md](README_會計_Phase3_帳務管理.md)
- [README_會計_Phase4_進階功能.md](README_會計_Phase4_進階功能.md)
