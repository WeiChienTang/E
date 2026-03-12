# 會計模組 — 實作狀態與缺口分析

## 更新日期
2026-03-12

## 說明

本文件記錄截至 2026-03-12 的會計模組**實際程式碼狀態**，
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

---

### P1-B：期初餘額機制 — 完全未實作

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| `JournalEntryType.OpeningBalance = 6` | Enum 目前只到 value 5（Reversing）|
| `Components/Pages/Accounting/OpeningBalancePage.razor` | 精靈 UI 不存在 |
| 期初餘額傳票建立邏輯 | JournalEntryService 無相關方法 |
| 每公司一筆限制驗證 | 未設計 |

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

### P2-A：薪資傳票整合 — 完全未實作，且有設計問題

**⚠️ 設計文件問題：**

原文件以 `PayrollBatch` 為整合目標，但薪資模組實際 Entity 結構為：

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

**現有狀況：**
- `MaterialIssue` Entity 存在於 `Data/Entities/Inventory/`
- `ProductionScheduleId`（關聯生產排程）已有，可判斷是否為生產用料
- **無 `IsJournalized` 欄位**
- **無正式用途分類欄位**（Remarks 為自由文字）

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
| `MaterialIssuePurpose` Enum（若選 A） | 未建立 |
| `MaterialIssue.IssuePurpose`（若選 A） | 未新增 |
| `MaterialIssue.IsJournalized` | 未新增 |
| `MaterialIssue.JournalizedAt` | 未新增 |
| Migration | 未建立 |
| `JournalEntryAutoGenerationService.GetPendingMaterialIssuesAsync()` | 未新增 |
| `JournalEntryAutoGenerationService.JournalizeMaterialIssueAsync()` | 未新增 |
| 批次轉傳票頁面 — 材料領用 Section | 未新增 |

---

### P2-C：生產完工傳票整合 — 完全未實作，有核心待決策

**⚠️ 最關鍵待決策：生產完工成本計算方式**

| 選項 | 說明 | 優點 | 缺點 |
|------|------|------|------|
| A：標準成本 | 預設商品標準成本 × 完工數量 | 簡單 | 需先建立商品標準成本 |
| B：實際投入原料成本 | 彙總對應領料單金額 | 最準確 | 需與 P2-B 領料傳票聯動 |
| C（建議）：均價倒推 | 完工入庫時的移動加權均價 × 數量 | 與現有庫存邏輯一致，無需額外設定 | 依賴庫存入庫已有均價 |

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
| 生產相關 Entity 的 `IsJournalized` | 需確認以哪個 Entity 為單位（ProductionSchedule？） |
| Migration | 未建立 |
| `JournalEntryAutoGenerationService.GetPendingProductionCompletionsAsync()` | 未新增 |
| `JournalEntryAutoGenerationService.JournalizeProductionCompletionAsync()` | 未新增 |
| 批次轉傳票頁面 — 生產完工 Section | 未新增 |

---

## 五、Phase 3 缺口（中優先 🟡）

### P3-A：AR/AP 帳齡分析 — 部分缺失

**現有狀況：**
- `CustomerStatementReportService` 存在，但這是「客戶對帳單」（逐筆流水）
- `SupplierStatementReportService` 存在，同上
- **無帳齡區間分析報表**（0-30 / 31-60 / 61-90 / 91-120 / 120+ 天分組）

**缺少的程式碼：**

| 缺少項目 | 說明 |
|---------|------|
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

### P3-C：業務單據顯示相關傳票 — 完全未實作

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

在開始實作前，以下問題需要業務討論或架構決策：

| # | 問題 | 影響 Phase | 選項 |
|---|------|-----------|------|
| 1 | 薪資傳票的轉傳票單位：以 `PayrollPeriod`（期間）還是 `PayrollRecord`（員工）為單位？ | P2-A | 建議以 PayrollPeriod 為單位（一期一張傳票） |
| 2 | 材料領用用途分類：新增 Enum 還是靠 `ProductionScheduleId` 判斷？ | P2-B | 建議新增 `MaterialIssuePurpose` Enum |
| 3 | 生產完工成本計算：標準成本 / 實際投入成本 / 均價倒推？ | P2-C | 建議均價倒推（與現有庫存邏輯一致） |
| 4 | 帳齡分析（FN012/013）計算基礎：以業務單據計算還是以傳票分錄計算？ | P3-A | 建議以業務單據（銷貨出貨單/進貨入庫單）為基礎 |
| 5 | 生產完工的轉傳票單位：以 `ProductionSchedule` 還是另一個 Entity？ | P2-C | 需確認 ProductionSchedule 是否有完工確認欄位 |

---

## 八、實作順序建議

```
Phase 1（必須優先）
  ├─ P1-A：FiscalPeriod Entity + Service + UI           ← 所有關帳/鎖定的基礎
  ├─ P1-B：OpeningBalance Enum + 精靈頁面               ← 歷史帳務導入
  └─ P1-C：FN006 六欄格式修正                           ← 報表格式正確化

Phase 2（Phase 1 後執行）
  ├─ P2-A：決策薪資 Entity → 新增 IsJournalized → 實作薪資傳票方法
  ├─ P2-B：決策用途分類 → 新增欄位 → 實作材料領用傳票方法
  └─ P2-C：決策成本計算方式 → 實作生產完工傳票方法

Phase 3（可與 Phase 2 平行）
  ├─ P3-C：業務單據顯示相關傳票（最簡單，可優先做）
  ├─ P3-A：AR/AP 帳齡分析報表
  └─ P3-B：年底結帳（需 P1-A 完成後才可執行）

Phase 4（最後執行）
  ├─ P4-B：Excel 匯出（依賴套件評估）
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
