# 會計模組 Phase 4：進階功能

## 更新日期
2026-03-21

## 優先等級
🟢 低優先（Phase 1-3 完成後執行）

---

## 概述

Phase 4 包含進一步完善帳務管理的功能：**現金流量表**、**報表匯出**、**銀行對帳**、**傳票附件**。
這些功能不影響基礎帳務正確性，但能顯著提升財務管理品質。

---

## P4-A：現金流量表（Cash Flow Statement）

### 定義

現金流量表為三大財務報表之一（損益表、資產負債表、現金流量表），
顯示現金在特定期間的流入/流出，按三大類別分類：
- **營業活動**：日常業務產生的現金
- **投資活動**：購置/出售長期資產
- **籌資活動**：股東投資、借款、還款

### 採用方法：間接法（Indirect Method）

間接法從**稅前損益**出發，調整非現金項目得出營業活動現金流量。
較直接法容易實作（不需追蹤每筆現金流向）。

### FN014 — 現金流量表

**篩選條件：** StartDate / EndDate（FilterDateRange）

**報表結構（間接法）：**
```
一、營業活動之現金流量
  稅前損益（來自 FN007）                    xxx
  調整項目：
    加：折舊費用（Depreciation）              xxx
    加：應收帳款減少（或減：應收帳款增加）   (xxx)
    加：存貨減少（或減：存貨增加）           (xxx)
    加：應付帳款增加（或減：應付帳款減少）    xxx
  營業活動淨現金流量                         xxx

二、投資活動之現金流量
  購置固定資產                              (xxx)
  出售固定資產                               xxx
  投資活動淨現金流量                        (xxx)

三、籌資活動之現金流量
  股東增資                                   xxx
  償還借款                                  (xxx)
  籌資活動淨現金流量                         xxx

本期現金增減                                 xxx
期初現金餘額                                 xxx
期末現金餘額                                 xxx
```

### 實作方式（已完成）

採用 **`CashFlowCategory` 標記法**（非科目代碼前綴）：
- 會計師在「會計科目表」頁面為每個科目設定 `CashFlowCategory`
- Service 查詢 JournalEntryLines，依類別彙總借貸金額
- 無需維護科目代碼對應表，更具彈性

**公式：**
- `OperatingNonCash`：`normal_direction_balance`（Debit-normal = debit - credit；Credit-normal = credit - debit）
- `OperatingWorkingCapital` / `Investing` / `Financing`：`credit - debit`（借方多 = 現金流出；貸方多 = 現金流入）
- `Cash`（期初）：累計至期初前一刻的正常方向餘額

**依賴：** 科目須先設定 CashFlowCategory 才能正確計算。

---

## P4-B：報表匯出（Excel / PDF）✅ 2026-03-21 確認完成

### 實際狀態

Excel 匯出已透過**現有報表系統**完整支援，無需另行實作：

- `ExcelExportService`：統一負責將報表資料輸出為 `.xlsx`
- `ReportPreviewModalComponent`：已有「匯出 Excel」按鈕，觸發下載
- 所有加入 `ReportRegistry.cs` 的報表（FN005–FN014）均自動獲得此能力

**PDF 匯出：**
- 選項 A（已支援）：瀏覽器列印對話框選擇「儲存為 PDF」
- 選項 B（未來需求）：伺服器端 PDF，使用 `PuppeteerSharp` 或 `IronPdf`

---

## P4-C：銀行對帳（Bank Reconciliation）✅ 2026-03-21 完成

### 功能描述

將銀行存款科目的帳面餘額與銀行對帳單（Bank Statement）進行核對，
找出未入帳項目和差異，是 IAS 7 與內部控制的基本要求。

### 為什麼需要銀行對帳

| 差異來源 | 說明 |
|----------|------|
| 時間差 | 已開支票未兌現、已匯款銀行未入帳 |
| 帳費未入帳 | 銀行手續費、利息收入未開傳票 |
| 人為錯誤 | 傳票或銀行記錯金額 |
| 未授權交易 | 舞弊、盜領偵測 |

### 已實作架構

**Entity：** `BankStatement`（銀行對帳單主檔）
| 欄位 | 型別 | 說明 |
|------|------|------|
| `CompanyId` | int FK | 公司 |
| `CompanyBankAccountId` | int FK | 銀行帳號（`CompanyBankAccount`）|
| `StatementDate` | DateTime | 對帳單日期 |
| `PeriodStart` / `PeriodEnd` | DateTime | 對帳期間 |
| `OpeningBalance` | decimal | 期初餘額 |
| `ClosingBalance` | decimal | 期末餘額（銀行所示）|
| `StatementStatus` | enum | Draft / InProgress / Completed |

> ⚠️ 注意：`BankStatement` 繼承 `BaseEntity`，其中已有 `Status`（EntityStatus），因此對帳狀態欄位命名為 `StatementStatus`（非 `Status`）以避免衝突。

**Entity：** `BankStatementLine`（對帳明細行）
| 欄位 | 型別 | 說明 |
|------|------|------|
| `BankStatementId` | int FK | 所屬對帳單 |
| `TransactionDate` | DateTime | 交易日期 |
| `Description` | string(200) | 交易說明 |
| `DebitAmount` | decimal | 支出金額 |
| `CreditAmount` | decimal | 收入金額 |
| `IsMatched` | bool | 是否已配對傳票 |
| `MatchedJournalEntryLineId` | int? FK | 配對的傳票分錄（OnDelete=SetNull）|
| `SortOrder` | int | 排序 |

**Migration：** `20260321011125_AddBankStatement`

**Service：** `BankStatementService`（繼承 `GenericManagementService<BankStatement>`）
- `GetWithLinesAsync(id)` — 含明細行載入
- `SaveWithLinesAsync(statement, lines, user)` — 主檔 + 明細行 Upsert
- `ToggleLineMatchAsync(lineId, journalEntryLineId, user)` — 配對/取消配對
- `GetByBankAccountAsync(companyBankAccountId)` — 依銀行帳號查詢

**UI 路徑：** `/bank-statements`
- `BankStatementIndex.razor` — 列表頁（GenericIndexPage 模式）
- `BankStatementEditModalComponent.razor` — 新增/編輯 Modal
- `BankStatementLineTable.razor` — 明細行互動表格
- `BankStatementFieldConfiguration.cs` — 欄位定義（對帳狀態 badge、配對狀態、期間顯示）

**權限：** `BankStatement.Read`（在 `PermissionRegistry` 中，`Accounting Group`）

**對帳流程（初版）：**
1. 輸入銀行對帳單主檔（銀行帳號、期間、期初/期末餘額）
2. 逐行輸入銀行交易明細（日期、說明、借/貸金額）
3. 為每行選擇對應的 `JournalEntryLine` → 標記為已配對
4. 確認所有差異後，將狀態改為 Completed

**FN015 銀行存款餘額調節表（2026-03-21 完成）：**
- `BankReconciliationCriteria`（公司 + 對帳期間篩選）
- `IBankReconciliationReportService` / `BankReconciliationReportService`
- 報表區塊：對帳單概況 → 配對狀況摘要 → 已配對明細（含傳票編號）→ 未配對明細 → 差異分析
- 加入 `ReportRegistry`（FN015，SortOrder=11）及 `FilterTemplateRegistry`
- DI 注冊：`ServiceRegistration.cs`
- i18n：`Report.BankReconciliation` / `Report.BankReconciliationDesc` × 5 語言

**未來延伸（尚未實作）：**
- 自動比對演算法（依金額 + 日期模糊配對）
- CSV/OFX 匯入銀行對帳單

---

## P4-D：傳票附件（Journal Entry Attachments）

### 功能描述

傳票支援上傳掃描文件（發票、收據、合約等），提供數位存檔。

### 設計方案

沿用系統現有附件機制（若已有其他模組實作）：

**新 Entity：** `JournalEntryAttachment`
| 欄位 | 說明 |
|------|------|
| JournalEntryId | 傳票 FK |
| FileName | 原始檔名 |
| StoredFileName | 儲存路徑/名稱 |
| FileSize | 檔案大小（bytes） |
| ContentType | MIME 類型 |
| UploadedAt | 上傳時間 |
| UploadedByEmployeeId | 上傳人員 FK |

**UI：** `JournalEntryEditModalComponent.razor` 新增附件區塊
- 上傳按鈕（拖曳或點擊選檔）
- 已上傳清單（顯示檔名、大小、上傳人）
- 預覽按鈕（圖片/PDF inline 顯示）
- 下載按鈕
- 刪除按鈕（已過帳傳票的附件仍可刪除，因附件不影響帳務）

**儲存位置：** `wwwroot/uploads/journal-attachments/{year}/{month}/` 或 Azure Blob Storage

---

## 完成標準（Definition of Done）

### P4-A ✅（2026-03-21 完成）
- [x] `AccountItem.CashFlowCategory` 欄位新增（Migration: AddAccountItemCashFlowCategory）
- [x] `CashFlowCategory` enum 新增（6 值：Cash/OperatingWorkingCapital/OperatingNonCash/Investing/Financing/Excluded）
- [x] `AccountItemEditModalComponent.razor` 新增 CashFlowCategory 下拉欄位
- [x] `AccountItemFieldConfiguration.cs` 新增 CashFlowCategory 篩選器 + 色彩 badge
- [x] `CashFlowCriteria` 篩選條件（需指定完整期間）
- [x] `ICashFlowReportService` 介面
- [x] `CashFlowReportService` 間接法實作
- [x] FN014 加入 `ReportRegistry.cs`（SortOrder=10）
- [x] FN014 加入 `FilterTemplateRegistry.cs`
- [x] DI 注冊（`ServiceRegistration.cs`）
- [x] i18n: `Report.CashFlow` / `Report.CashFlowDesc` × 5 語言
- [x] 期末現金餘額驗證（`CashFlowData.HasCashAccounts`；未設定時報表頂端顯示橘色警告，不阻擋報表產生，2026-03-21 完成）

### P4-B ✅（2026-03-21 確認，透過現有報表系統完成）
- [x] Excel 匯出：`ExcelExportService` + `ReportPreviewModalComponent` 匯出按鈕
- [x] 所有加入 `ReportRegistry` 的報表自動獲得匯出能力（FN005–FN015）
- [x] PDF：瀏覽器列印轉 PDF（選項 A）

### P4-C ✅（2026-03-21 完成）
- [x] `BankStatement` Entity（繼承 BaseEntity，`StatementStatus` 避免與 `Status` 衝突）
- [x] `BankStatementLine` Entity（FK → `JournalEntryLine` OnDelete=SetNull；含 `BankReference` 欄位）
- [x] Migration：`20260321011125_AddBankStatement`、`AddBankStatementLineBankReference`
- [x] `IBankStatementService` / `BankStatementService`（含 `SaveWithLinesAsync` / `ToggleLineMatchAsync`）
- [x] `BankStatementFieldConfiguration.cs`（狀態 badge、配對狀態、期間欄位）
- [x] `BankStatementIndex.razor`（`/bank-statements`，GenericIndexPage 模式）
- [x] `BankStatementEditModalComponent.razor`（主檔 + 明細行 Custom Module + CSV 匯入按鈕）
- [x] `BankStatementLineTable.razor`（InteractiveTable，IsMatched badge 顯示）
- [x] **CSV 匯入功能**（`IBankStatementImportService` / `BankStatementImportService`）
  - 自動偵測編碼：UTF-8（含 BOM 判斷）→ Big5 fallback
  - 6 欄格式：交易日期、交易說明、支出金額、收入金額、銀行流水號（選填）、交易後餘額（選填，僅預覽）
  - 前提驗證：存在未配對明細行時阻擋匯入，避免資料衝突
  - 期間驗證：超出對帳期間的日期軟性警告（不阻擋），訊息中告知筆數
  - `BankStatementCsvImportModal.razor`：預覽確認 Modal（統計摘要 badge、逐列驗證顯示、範本下載）
  - `BankStatementImportRow` / `BankStatementImportResult` 資料模型（`Models/Import/`）
- [x] 權限：`BankStatement.Read` 加入 `PermissionRegistry`
- [x] 導覽：`Nav.BankStatements` 加入 `NavigationConfig`
- [x] DI 注冊：`ServiceRegistration.cs`（含 `IBankStatementImportService`）
- [x] i18n：28 個基礎 key + 21 個 `CsvImport.*` key × 5 語言

### P4-D ✅（2026-03-21 完成）
- [x] `JournalEntryAttachment` Entity（FK → JournalEntry OnDelete=Cascade，FK → Employee OnDelete=SetNull）
- [x] Migration：`AddJournalEntryAttachment`
- [x] `IJournalEntryAttachmentService` / `JournalEntryAttachmentService`
  - `GetByJournalEntryAsync(journalEntryId)` — 查詢傳票所有附件
  - `UploadAsync(journalEntryId, file, employeeId, env)` — 上傳至 `wwwroot/uploads/journal-attachments/{year}/{month}/`
  - `DeleteAsync(attachmentId, env)` — 同時刪除實體檔案與 DB 記錄
- [x] `FileUploadHelper.UploadDocumentToFolderAsync()` — 新增自訂子目錄上傳方法
- [x] `JournalEntryEditModalComponent.razor` 加入附件模組（第 2 個 CustomModule）
  - 「上傳附件」按鈕（InputFile，accept PDF/Word/Excel/圖片）
  - 附件清單表格（顯示檔名、大小、上傳人員、時間、刪除按鈕）
  - 檔案圖示依類型顯示（PDF=紅色、圖片=藍色、Excel=綠色、Word=藍色）
  - 新建傳票（ID=null）不顯示附件模組
- [x] DI 注冊：`ServiceRegistration.cs`
- [x] i18n：12 個 `Attachment.*` key × 5 語言
- **儲存策略**：本地 `wwwroot/uploads/journal-attachments/`（單機部署），未來可換 Azure Blob Storage

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_財務報表.md](README_會計_財務報表.md)
