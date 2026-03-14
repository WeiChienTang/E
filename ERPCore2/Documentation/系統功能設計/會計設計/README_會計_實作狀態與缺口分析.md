# 會計模組 — 實作狀態與缺口分析

## 更新日期
2026-03-14

## 說明

本文件記錄截至 2026-03-14 的會計模組**實際程式碼狀態**，
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
| 傳票 Service | `Services/FinancialManagement/JournalEntryService.cs` | 465 行，含 PostAsync、ReverseAsync |
| 傳票 Index / EditModal | `Components/Pages/Accounting/JournalEntryIndex.razor` | 注意：在 **Accounting** 資料夾，非文件所述 FinancialManagement |
| 批次轉傳票頁面 | `Components/Pages/Accounting/JournalEntryBatchPage.razor` | |
| 批次轉傳票 Service | `Services/FinancialManagement/JournalEntryAutoGenerationService.cs` | 812 行，含 5 種單據×2（Pending+Journalize） |
| 沖款單 IsJournalized | `Data/Entities/FinancialManagement/SetoffDocument.cs` | line 100 |
| 子科目服務 | `Services/FinancialManagement/SubAccountService.cs` | |
| FN005 科目表報表 | `Services/Reports/AccountItemListReportService.cs` | |
| FN006 試算表 | `Services/Reports/TrialBalanceReportService.cs` | ⚠ 五欄，缺期初餘額欄（見 P1-C） |
| FN007 損益表 | `Services/Reports/IncomeStatementReportService.cs` | |
| FN008 資產負債表 | `Services/Reports/BalanceSheetReportService.cs` | |
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

## 三、Phase 1 缺口（最高優先 🔴）

> 缺少 Phase 1，所有財務報表數字不可靠。

### P1-A：會計期間管理 — 完全未實作

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| `Data/Entities/FinancialManagement/FiscalPeriod.cs` | Entity 完全不存在 |
| `Models/Enums/FiscalPeriodStatus.cs` | Enum 不存在 |
| `Services/FinancialManagement/IFiscalPeriodService.cs` | Service 不存在 |
| `Services/FinancialManagement/FiscalPeriodService.cs` | Service 不存在 |
| `Components/Pages/Accounting/FiscalPeriodIndex.razor` | UI 不存在 |
| Migration `AddFiscalPeriodTable` | 尚未建立 |

**`JournalEntryService.PostEntryAsync` 目前無期間鎖定：**
目前任何時候均可對任意歷史月份過帳，無法防止補登已關帳期間。

**`JournalEntryService.ReverseEntryAsync` 同樣無期間驗證：**
沖銷傳票（ReverseEntryAsync）的 `reversalDate` 對應的期間若已關帳，目前不會阻擋，需同步補充驗證邏輯。

---

### P1-B：期初餘額機制 — 完全未實作

**設計決策（已確認）：**
- 期初餘額傳票**必須借貸平衡**才可過帳，系統拒絕不平衡提交
- 已過帳後不提供「重設」功能，改以建立**調整分錄**方式修正
- 借貸平衡是因為任何時間點的歷史帳務都應符合會計恆等式

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| `JournalEntryType.OpeningBalance = 6` | Enum 目前只到 value 5（Reversing）|
| `Components/Pages/Accounting/OpeningBalancePage.razor` | 精靈 UI 不存在 |
| 期初餘額傳票建立邏輯 | JournalEntryService 無相關方法 |
| 每公司一筆限制驗證 | 未設計 |
| 借貸平衡強制檢查 | 未設計（舊設計允許不平衡） |

---

### P1-C：FN006 試算表格式修正 — 部分缺失

**現行 FN006 格式（五欄）：**

```
科目代碼 | 科目名稱 | 本期借方 | 本期貸方 | 期末借方餘額 | 期末貸方餘額
```

**標準格式應為（六欄）：**

```
科目代碼 | 科目名稱 | 期初餘額（借）| 期初餘額（貸）| 本期借方 | 本期貸方 | 期末借方餘額 | 期末貸方餘額
```

需修改：`Services/Reports/TrialBalanceReportService.cs` 補充期初餘額查詢邏輯。

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
`MaterialIssueDetail.UnitCost` 為 nullable，需確認領料確認時是否自動填入商品當下的移動均價（`Product.AverageCost`）。
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
- 帳齡計算單位：**SalesDelivery 主檔**（非逐行商品明細）
- 未收金額：`SalesDelivery.TotalAmount - SUM(SetoffProductDetail.CurrentSetoffAmount WHERE 關聯此單據)`
- 帳齡天數基準：`DeliveryDate + Customer.PaymentDays`（需確認 `Customer` Entity 是否有 `PaymentDays` 欄位）
- `SetoffProductDetail` 為多型 FK，查詢需批量處理避免 N+1

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| `Customer.PaymentDays` 欄位 | 需確認是否存在，若無需先新增 |
| `Supplier.PaymentDays` 欄位 | 同上 |
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

## 六、Phase 4 缺口（低優先 🟢）

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
| 11 | 帳齡天數基準欄位 | P3-A | 🔲 待確認 | 確認 `Customer.PaymentDays` / `Supplier.PaymentDays` 是否存在 |

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
