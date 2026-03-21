# 會計模組設計總綱

## 更新日期
2026-03-21

---

## 概述

ERPCore2 會計模組涵蓋科目表管理、傳票系統、批次轉傳票、子科目自動產生，AR/AP 帳齡分析（FN012/FN013）、年底結帳，及完整的財務報表（試算表、損益表、資產負債表、總分類帳、明細分類帳、明細科目餘額表）。

---

## 系統架構圖

```
┌─────────────────────────────────────────────────────────────────────┐
│                           會計模組架構                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌────────────────┐    ┌─────────────────┐    ┌───────────────┐    │
│  │   科目表管理   │    │    傳票系統      │    │  財務報表     │    │
│  │  AccountItem   │───▶│  JournalEntry   │───▶│  FN005~FN015  │    │
│  └────────────────┘    └────────┬────────┘    └───────────────┘    │
│           ▲                     │  ▲                                │
│  ┌────────┴───────┐    ┌────────▼──┴─────┐    ┌───────────────┐   │
│  │  子科目系統    │    │    批次轉傳票    │    │  會計期間管理 │   │
│  │  SubAccount    │───▶│  AutoGen Svc    │    │ FiscalPeriod  │   │
│  └────────────────┘    └─────────────────┘    └───────┬───────┘   │
│                                                         │ 期間驗證   │
│                                               ┌─────────▼───────┐  │
│                                               │  PostEntryAsync  │  │
│                                               │ ReverseEntryAsync│  │
│                                               │（Open 才可過帳） │  │
│                                               └─────────────────┘  │
│                                                                     │
│  業務單據（進貨入庫 / 銷貨出貨 / 沖款單 等）                          │
│    └── JournalEntryBatchPage（/journal-entry-batch）                 │
│          └── JournalEntryAutoGenerationService                      │
│                └── JournalEntry（Draft → Posted）                   │
│                      └── FiscalPeriod 期間驗證                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📚 子文件導覽

### 現有功能文件

| 文件 | 說明 | 適用場景 |
|------|------|----------|
| [README_會計_科目表.md](README_會計_科目表.md) | AccountItem Entity、Enum、Seeder、Service、UI 元件、FN005 報表 | 科目表管理與查詢 |
| [README_會計_傳票系統.md](README_會計_傳票系統.md) | JournalEntry / JournalEntryLine Entity、Service、EditModal、**期初餘額機制（OpeningBalance）** | 傳票手動輸入、過帳、沖銷、期初餘額設定 |
| [README_會計_會計期間管理.md](README_會計_會計期間管理.md) | FiscalPeriod Entity、FiscalPeriodService、期間狀態機、傳票過帳期間驗證 | 關帳、鎖定期間、控制可過帳範圍 |
| [README_會計_批次轉傳票.md](README_會計_批次轉傳票.md) | IsJournalized 欄位、AutoGeneration Service、批次轉傳票頁面、**業務單據相關傳票顯示（P3-C）** | 業務單據自動轉傳票、EditModal 傳票查詢 |
| [README_會計_子科目系統.md](README_會計_子科目系統.md) | SubAccountService、系統參數設定、兩種代碼格式、批次補建 | 客戶 / 廠商 / 品項子科目自動產生 |
| [README_會計_財務報表.md](README_會計_財務報表.md) | FN006 試算表（八欄格式）、FN007 損益表、FN008 資產負債表、FN009–FN011 帳冊報表、FN012/FN013 帳齡分析、FN014 現金流量表、FN015 銀行存款餘額調節表 | 財務報表產生與查詢 |
| [README_會計_Phase4_進階功能.md](README_會計_Phase4_進階功能.md)（銀行對帳章節） | BankStatement / BankStatementLine Entity、BankStatementService、UI 元件、FN015 報表、P4-D 傳票附件設計 | 銀行對帳功能實作與附件待辦 |

### 開發計畫文件（按優先順序）

| 文件 | 說明 | 優先等級 |
|------|------|----------|
| [README_會計_Phase2_業務整合.md](README_會計_Phase2_業務整合.md) | **薪資傳票（P2-A，暫緩）**：薪資計提/發放分錄設計，待薪資模組重新設計完成後處理 | ⏸️ 暫緩 |
| [README_會計_Phase4_進階功能.md](README_會計_Phase4_進階功能.md) | P4-A~C 已完成（FN014/FN015/Excel 匯出/銀行對帳）；**P4-D 傳票附件** 未完成 | 🟠 中（僅剩 P4-D） |

### 準則參考文件

| 文件 | 說明 |
|------|------|
| [README_IFRS準則.md](README_IFRS準則.md) | IFRS/IAS 完整準則清單、台灣 TIFRS 差異、各準則對 ERP 的設計要求、ERPCore2 目前合規狀態總覽 |

### 狀態追蹤文件

| 文件 | 說明 |
|------|------|
| [README_會計_實作狀態與缺口分析.md](README_會計_實作狀態與缺口分析.md) | 截至 2026-03-12 的程式碼實際狀態、缺口清單、文件錯誤修正、待決策事項 |

---

## 科目階層結構

科目表採用四層樹狀結構（單一表格 + ParentId 自我參照），以「庫存現金」為例：

```
Level 1: Code "1"      → 資產（ParentId: null）
  Level 2: Code "11-12"  → 流動資產（ParentId → "1"）
    Level 3: Code "111"    → 現金及約當現金（ParentId → "11-12"）
      Level 4: Code "1111"   → 庫存現金（ParentId → "111"）← 實際記帳科目
```

**原始 Excel 層級對應：**
- Excel「一級項目」單位數代碼（1~8）→ Level 1
- Excel「一級項目」多位數代碼（11-12、31 等）→ Level 2
- Excel「二級項目」（111、112 等）→ Level 3
- Excel「四級項目」（1111、1112 等）→ Level 4（Excel「三級項目」為空，不使用）

---

## 設計決策摘要

| 決策 | 說明 | 實作狀態 |
|------|------|----------|
| 單一表格 + ParentId 自我參照 | 層級數量不固定，樹狀模型最適合報表遞迴彙總 | ✅ 已實作 |
| 每筆資料直接標記 AccountType | 避免報表查詢每次遞迴到根節點，提升效能 | ✅ 已實作 |
| 批次轉傳票（非即時） | 業務單據確認後仍可修改，讓會計月底審核無誤後再執行 | ✅ 已實作 |
| COGS 取自 InventoryTransaction.TotalAmount | 移動加權均價，出庫時由 `ReduceStockAsync` 自動寫入，無須重新計算 | ✅ 已實作 |
| 子科目延遲建立（GetOrCreate） | 未啟用時自動 fallback 統制科目，不影響既有流程 | ✅ 已實作 |
| FiscalPeriod 不對 JournalEntry 加 FK | JournalEntry 維持 `FiscalYear int` + `FiscalPeriod int`，FiscalPeriod 管理表獨立維護，期間驗證在 Service 層完成（符合業界 SAP/Oracle 慣例，避免雞蛋問題與 Migration 負擔） | ✅ 已實作 |
| 期初餘額傳票必須借貸平衡 | 不允許不平衡的期初餘額傳票過帳；差額代表輸入有誤，系統應拒絕並提示哪邊相差多少 | ✅ 已實作 |
| FiscalPeriod 混合式初始化 | 當年度自動建立；過去期間不存在時自動建立（記錄警告）；已 Closed/Locked 期間嚴格阻擋過帳 | ✅ 已實作 |
| FiscalPeriod 重開機制 | Closed 可重開（需 `Accounting.ClosePeriod` 權限）；Locked 永久不可重開；重開記錄原因與操作人員 | ✅ 已實作 |
| 會計功能操作權限 | 不使用 SuperAdmin；改用 `Accounting.*` 系列權限（PostEntry / ClosePeriod / OpeningBalance / YearEndClosing），透過現有 Role+Permission 授予財務人員 | ✅ 已實作 |
| AR/AP 帳齡以主檔（Invoice）層級計算 | 帳齡追蹤單位為 SalesDelivery / PurchaseReceiving 整張單據，而非逐行品項明細；帳齡天數基準 = 交貨日 + 客戶信用天數（`Customer.PaymentDays`） | ✅ 已實作 |
| 年底結帳冪等性保護 | `ExecuteYearEndClosingAsync` 執行前先確認該年度是否已有 Closing 類型傳票，若有則拒絕重複執行 | ✅ 已實作 |
| AR/AP 帳齡金額口徑 | 使用含稅金額（`TotalAmount + TaxAmount`）；`DiscountAmount` 已含入 `TotalAmount`，無需另扣 | ✅ 已實作 |
| 年底結帳科目代碼 | **Step 1**：損益科目歸零轉入「本期損益」`3353`；**Step 2**：本期損益轉入「累積盈虧」`3351`。⚠️ 注意：`3351=累積盈虧`（保留盈餘帳），`3353=本期損益`（年底結帳彙總帳），兩者均存在於種子資料。`3361` 不存在，**不可使用**。 | ✅ 已實作 |

> 詳細設計決策說明請見各子文件。

---

## 開發路線圖（Roadmap）

> 詳細設計請見各 Phase 文件。

### 🔴 Phase 1：基礎補強（最高優先）

> ✅ Phase 1 已完成（2026-03-17）

- [x] **P1-A：會計期間管理** — `FiscalPeriod` Entity、期間開/關帳、傳票過帳期間鎖定（`PostEntryAsync`、`ReverseEntryAsync` 均已加入期間驗證）
- [x] **P1-B：期初餘額機制** — `JournalEntryType.OpeningBalance = 6`、期初餘額設定精靈（已過帳後顯示唯讀並引導建立調整分錄）
- [x] **P1-C：FN006 試算表修正** — 已補充「期初餘額借/貸」兩欄，改為八欄標準格式（科目代碼、名稱 + 期初借/貸 + 本期借/貸 + 期末借/貸）

### 🟠 Phase 2：業務模組傳票整合

- [ ] **P2-A：薪資傳票** ⏸️ 暫緩 — 薪資計提 + 薪資發放兩種分錄，待薪資模組設計完整後處理
- [x] **P2-B：材料領用傳票** — 領料確認後自動產生存貨移轉分錄（Entity ✅ Service ✅ UI ✅ i18n ✅，2026-03-19 完成）
- [x] **P2-C：生產完工傳票** — 完工入庫產生在製品 → 製成品分錄（Entity ✅ Service ✅ UI ✅ i18n ✅，2026-03-19 完成）

### 🟠 Phase 3：帳務管理功能

> ✅ Phase 3 已完成（2026-03-21）

- [x] **P3-A：應收帳款帳齡分析（FN012）** — 依客戶×帳齡區間顯示未收金額，篩選條件支援客戶 FK + 業務員 FK + 截止日（2026-03-21 完成）
- [x] **P3-A：應付帳款帳齡分析（FN013）** — 依廠商×帳齡區間顯示未付金額（2026-03-21 完成）
- [x] **P3-B：年底結帳功能** — Service/UI/i18n 完整，修正 PostEntryAsync Closing 傳票可過帳到 Closed 期間（2026-03-19 完成）
- [x] **P3-C：業務單據顯示相關傳票** — EditModal 底部折疊區塊顯示對應傳票（2026-03-17 完成）

### 🟢 Phase 4：進階功能

- [x] **P4-A：現金流量表（FN014）** — 間接法，三大類別（營業/投資/籌資）（2026-03-21 完成）
- [x] **P4-B：報表匯出（Excel）** — 透過現有報表系統 `ExcelExportService` + `ReportPreviewModal` 匯出按鈕已支援（2026-03-21 確認）
- [x] **P4-C：銀行對帳** — 銀行對帳單輸入 + 手動配對傳票分錄（Entity/Migration/Service/UI/i18n 完整，2026-03-21 完成）
- [x] **P4-D：傳票附件** — 傳票支援上傳發票/收據掃描檔，本地儲存 `wwwroot/uploads/journal-attachments/{year}/{month}/`（Entity/Migration/Service/UI/i18n 完整，2026-03-21 完成）

### 已完成項目（歷史記錄）

- [x] 科目表管理（553 筆台灣商業會計項目表 112 年度）
- [x] 傳票系統（手動輸入/過帳/沖銷）
- [x] 期初餘額機制（`JournalEntryType.OpeningBalance`、`OpeningBalancePage` 精靈、過帳後唯讀）
- [x] 批次轉傳票（進貨入庫/退回、銷貨出貨/退回、沖款單）；COGS 分錄為必要項目（非可選）
- [x] 子科目系統（客戶/廠商/品項子科目自動產生）
- [x] 財務報表 FN005-FN011（科目表、試算表[八欄格式]、損益表、資產負債表、總分類帳、明細分類帳、明細科目餘額表）
- [x] 會計期間管理（`FiscalPeriod` Entity、Open/Closed/Locked 狀態機、`PostEntryAsync`/`ReverseEntryAsync` 期間驗證）
- [x] `Accounting.*` 權限體系（PostEntry / ClosePeriod / OpeningBalance / YearEndClosing）
- [x] P3-C：五個業務單據 EditModal 底部加入相關傳票折疊區塊（2026-03-17）
- [x] P2-B：材料領用傳票（Entity/Service/UI/i18n 完整，2026-03-19）
- [x] P2-C：生產完工傳票（Entity/Service/UI/i18n 完整，2026-03-19）
- [x] P3-B：年底結帳（Service/UI/i18n 完整，修正 Closing 傳票期間驗證 Bug，2026-03-19）
- [x] P3-A：AR/AP 帳齡分析 FN012/FN013（Criteria/Service/FilterTemplate/ReportRegistry/i18n 完整，2026-03-21）
- [x] P4-B：Excel 匯出（透過現有報表系統，確認 2026-03-21）
- [x] P4-C：銀行對帳（Entity/Migration/Service/UI/i18n 完整，2026-03-21）

---

## 已知設計缺陷與修正說明

| # | 問題 | 嚴重程度 | 修正計畫 |
|---|------|----------|----------|
| 1 | ~~試算表（FN006）原為五欄（缺期初餘額借/貸），修正為八欄標準格式~~ | ✅ 已修正 | Phase 1-C（2026-03-17） |
| 2 | ~~無法鎖定已關帳期間，任何時候都可補登歷史傳票~~ | ✅ 已修正 | Phase 1-A（2026-03-17） |
| 3 | ~~損益表（FN007）科目跨年不歸零，下年度試算表期初餘額包含歷史累計~~ | ✅ 已修正 | Phase 3-B（年底結帳，2026-03-19） |
| 4 | 薪資模組已建立，但無對應傳票自動產生（薪資暫緩，待薪資模組設計完整後處理） | 🟠 中 | Phase 2-A |
| 5 | ~~材料領用無傳票，存貨帳與財務帳脫節~~ | ✅ 已修正 | Phase 2-B（Entity/Service/UI/i18n 全部完成，2026-03-19） |
| 6 | ~~生產完工傳票尚未實作~~ | ✅ 已修正 | Phase 2-C（Entity/Service/UI/i18n 全部完成，2026-03-19） |
| 7 | `ProductionScheduleCompletion.ActualUnitCost` 與 `InventoryTransactionId` 皆為 nullable；若未填入，傳票金額將為零 | 🟠 中 | Phase 2-C 確認必填規則 |
| 8 | ~~無 AR/AP 帳齡分析，無法管理應收/應付逾期~~ | ✅ 已修正 | Phase 3-A（FN012/FN013 完整實作，2026-03-21） |
| 9 | ~~沖銷傳票（ReverseEntryAsync）無驗證目標期間是否已關帳~~ | ✅ 已修正 | Phase 1-A（2026-03-17，程式碼早已實作） |
| 10 | ~~P3-C（業務單據顯示相關傳票）五個 EditModal 皆尚未實作 UI~~ | ✅ 已修正 | 2026-03-17 完成 |
| 11 | ~~年底結帳使用不存在的科目代碼（`3361` 不存在於種子資料）~~ | ✅ 已修正 | 2026-03-21：`CurrentPeriodIncomeCode` 改為 `3353`（本期損益），`RetainedEarningsCode` 改為 `3351`（累積盈虧） |
| 12 | ~~年底結帳 PreCheckAsync 未檢查 Draft 傳票，存在草稿時仍允許結帳~~ | ✅ 已修正 | 2026-03-21：加入 Draft 傳票計數檢查 |
| 13 | ~~OpeningBalance 導航使用錯誤的權限（JournalEntry.Read 而非 Accounting.OpeningBalance）~~ | ✅ 已修正 | 2026-03-21：改為 `PermissionRegistry.Accounting.OpeningBalance` |
| 14 | ~~`JournalizeMaterialIssueAsync` 部分明細 UnitCost 為 null 時靜默略過，導致傳票金額不完整（存貨異動數量與傳票金額不符）~~ | ✅ 已修正 | 2026-03-21：改為先檢查全部明細均有 UnitCost，有任何 null 則回傳明確錯誤，不再允許部分轉傳票 |
| 15 | ~~`IncomeStatementReportService` 查詢包含 `ComprehensiveIncome` 科目但完全不顯示，導致 OCI 科目有餘額時損益表與資產負債表淨利數字不一致~~ | ✅ 已修正 | 2026-03-21：損益表新增「五、其他綜合損益（OCI）」區塊，有餘額才顯示；頁尾摘要同步更新 |
| 16 | `AccountItem` 缺少 `CashFlowCategory` 欄位，無法支援 IAS 7 現金流量表間接法的科目分類映射 | ✅ 已新增 | 2026-03-21：新增 `CashFlowCategory?` nullable 欄位 + `CashFlowCategory` enum（6 類別），Migration: `AddAccountItemCashFlowCategory` |
| 17 | FN014 現金流量表未實作 | ✅ 已完成 | 2026-03-21：`CashFlowReportService`（間接法）、`CashFlowCriteria`、`ICashFlowReportService`，加入 ReportRegistry/FilterTemplateRegistry/ServiceRegistration，i18n × 5 語言 |

## 相關文件

### 現有功能文件
- [README_會計_科目表.md](README_會計_科目表.md)
- [README_會計_傳票系統.md](README_會計_傳票系統.md)（含期初餘額機制）
- [README_會計_會計期間管理.md](README_會計_會計期間管理.md)
- [README_會計_批次轉傳票.md](README_會計_批次轉傳票.md)（含 EditModal 相關傳票顯示）
- [README_會計_子科目系統.md](README_會計_子科目系統.md)
- [README_會計_財務報表.md](README_會計_財務報表.md)

### 開發計畫文件
- [README_會計_Phase2_業務整合.md](README_會計_Phase2_業務整合.md)（僅剩 P2-A 薪資傳票，暫緩）
- [README_會計_Phase4_進階功能.md](README_會計_Phase4_進階功能.md)
