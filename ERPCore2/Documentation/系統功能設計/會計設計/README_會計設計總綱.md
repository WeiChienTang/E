# 會計模組設計總綱

## 更新日期
2026-03-14

---

## 概述

ERPCore2 會計模組涵蓋科目表管理、傳票系統、批次轉傳票、子科目自動產生，及完整的財務報表（試算表、損益表、資產負債表、總分類帳、明細分類帳、明細科目餘額表）。

---

## 系統架構圖

```
┌─────────────────────────────────────────────────────────────────┐
│                         會計模組架構                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌────────────────┐    ┌───────────────┐    ┌───────────────┐  │
│  │   科目表管理   │    │   傳票系統    │    │  財務報表     │  │
│  │  AccountItem   │───▶│ JournalEntry  │───▶│  FN005~FN011  │  │
│  └────────────────┘    └──────┬────────┘    └───────────────┘  │
│           ▲                   │                                  │
│  ┌────────┴───────┐    ┌──────▼────────┐                       │
│  │  子科目系統    │    │  批次轉傳票   │                       │
│  │  SubAccount    │───▶│  AutoGen Svc  │                       │
│  └────────────────┘    └───────────────┘                       │
│                                                                 │
│  業務單據（進貨入庫 / 銷貨出貨 / 沖款單 等）                      │
│    └── JournalEntryBatchPage（/journal-entry-batch）             │
│          └── JournalEntryAutoGenerationService                  │
│                └── JournalEntry（Draft → Posted）               │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📚 子文件導覽

### 現有功能文件

| 文件 | 說明 | 適用場景 |
|------|------|----------|
| [README_會計_科目表.md](README_會計_科目表.md) | AccountItem Entity、Enum、Seeder、Service、UI 元件、FN005 報表 | 科目表管理與查詢 |
| [README_會計_傳票系統.md](README_會計_傳票系統.md) | JournalEntry / JournalEntryLine Entity、Service、EditModal | 傳票手動輸入與過帳 |
| [README_會計_批次轉傳票.md](README_會計_批次轉傳票.md) | IsJournalized 欄位、AutoGeneration Service、批次轉傳票頁面 | 業務單據自動轉傳票 |
| [README_會計_子科目系統.md](README_會計_子科目系統.md) | SubAccountService、系統參數設定、兩種代碼格式、批次補建 | 客戶 / 廠商 / 品項子科目自動產生 |
| [README_會計_財務報表.md](README_會計_財務報表.md) | FN006 試算表、FN007 損益表、FN008 資產負債表、FN009–FN011 帳冊報表 | 財務報表產生與查詢 |

### 開發計畫文件（按優先順序）

| 文件 | 說明 | 優先等級 |
|------|------|----------|
| [README_會計_Phase1_基礎補強.md](README_會計_Phase1_基礎補強.md) | 會計期間管理（FiscalPeriod）、期初餘額機制、FN006 試算表格式修正 | 🔴 最高 |
| [README_會計_Phase2_業務整合.md](README_會計_Phase2_業務整合.md) | 薪資傳票、材料領用傳票、生產完工傳票 | 🟠 高 |
| [README_會計_Phase3_帳務管理.md](README_會計_Phase3_帳務管理.md) | AR/AP 帳齡分析（FN012/FN013）、年底結帳、業務單據相關傳票顯示 | 🟡 中 |
| [README_會計_Phase4_進階功能.md](README_會計_Phase4_進階功能.md) | 現金流量表（FN014）、報表匯出、銀行對帳、傳票附件 | 🟢 低 |

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

| 決策 | 說明 |
|------|------|
| 單一表格 + ParentId 自我參照 | 層級數量不固定，樹狀模型最適合報表遞迴彙總 |
| 每筆資料直接標記 AccountType | 避免報表查詢每次遞迴到根節點，提升效能 |
| 批次轉傳票（非即時） | 業務單據確認後仍可修改，讓會計月底審核無誤後再執行 |
| COGS 取自 InventoryTransaction.TotalAmount | 移動加權均價，出庫時由 `ReduceStockAsync` 自動寫入，無須重新計算 |
| 子科目延遲建立（GetOrCreate） | 未啟用時自動 fallback 統制科目，不影響既有流程 |
| FiscalPeriod 不對 JournalEntry 加 FK | JournalEntry 維持 `FiscalYear int` + `FiscalPeriod int`，FiscalPeriod 管理表獨立維護，期間驗證在 Service 層完成（符合業界 SAP/Oracle 慣例，避免雞蛋問題與 Migration 負擔） |
| 期初餘額傳票必須借貸平衡 | 不允許不平衡的期初餘額傳票過帳；差額代表輸入有誤，系統應拒絕並提示哪邊相差多少（詳見 Phase 1） |
| AR/AP 帳齡以主檔（Invoice）層級計算 | 帳齡追蹤單位為 SalesDelivery / PurchaseReceiving 整張單據，而非逐行品項明細；帳齡天數基準 = 交貨日 + 客戶信用天數 |
| FiscalPeriod 混合式初始化 | 當年度自動建立；過去期間不存在時自動建立（記錄警告）；已 Closed/Locked 期間嚴格阻擋過帳（詳見 Phase 1） |
| FiscalPeriod 重開機制 | Closed 可重開（需 `Accounting.ClosePeriod` 權限）；Locked 永久不可重開；重開記錄原因與操作人員 |
| 會計功能操作權限 | 不使用 SuperAdmin；改用 `Accounting.*` 系列權限（PostEntry / ClosePeriod / OpeningBalance / YearEndClosing），透過現有 Role+Permission 授予財務人員 |
| 年底結帳冪等性保護 | `ExecuteYearEndClosingAsync` 執行前先確認該年度是否已有 Closing 類型傳票，若有則拒絕重複執行 |
| AR/AP 帳齡金額口徑 | 使用含稅金額（`TotalAmount + TaxAmount`）；`DiscountAmount` 已含入 `TotalAmount`，無需另扣 |

> 詳細設計決策說明請見各子文件。

---

## 開發路線圖（Roadmap）

> 詳細設計請見各 Phase 文件。

### 🔴 Phase 1：基礎補強（最高優先）

> 缺少 Phase 1，財務報表數字不可靠，請優先完成。

- [ ] **P1-A：會計期間管理** — `FiscalPeriod` Entity、期間開/關帳、傳票過帳期間鎖定
- [ ] **P1-B：期初餘額機制** — `JournalEntryType.OpeningBalance = 6`、期初餘額設定精靈
- [ ] **P1-C：FN006 試算表修正** — 補充「期初餘額借/貸」欄，改為標準六欄格式

### 🟠 Phase 2：業務模組傳票整合

- [ ] **P2-A：薪資傳票** — 薪資計提 + 薪資發放兩種分錄，整合新薪資模組
- [ ] **P2-B：材料領用傳票** — 領料確認後自動產生存貨移轉分錄
- [ ] **P2-C：生產完工傳票** — 完工入庫產生在製品 → 製成品分錄（確認成本計算方式）

### 🟡 Phase 3：帳務管理功能

- [ ] **P3-A：應收帳款帳齡分析（FN012）** — 依客戶×帳齡區間顯示未收金額
- [ ] **P3-A：應付帳款帳齡分析（FN013）** — 依廠商×帳齡區間顯示未付金額
- [ ] **P3-B：年底結帳功能** — Closing Entries：收入/費用科目歸零 → 轉本期損益 → 轉保留盈餘
- [ ] **P3-C：業務單據顯示相關傳票** — EditModal 底部折疊區塊顯示對應傳票

### 🟢 Phase 4：進階功能

- [ ] **P4-A：現金流量表（FN014）** — 間接法，三大類別（營業/投資/籌資）
- [ ] **P4-B：報表匯出（Excel）** — 所有財務報表支援 `.xlsx` 下載
- [ ] **P4-C：銀行對帳** — 銀行對帳單輸入 + 手動配對傳票分錄
- [ ] **P4-D：傳票附件** — 傳票支援上傳發票/收據掃描檔

### 已完成項目（歷史記錄）

- [x] 科目表管理（553 筆台灣商業會計項目表 112 年度）
- [x] 傳票系統（手動輸入/過帳/沖銷）
- [x] 批次轉傳票（進貨入庫/退回、銷貨出貨/退回、沖款單）
- [x] 子科目系統（客戶/廠商/品項子科目自動產生）
- [x] 財務報表 FN005-FN011（科目表、試算表、損益表、資產負債表、總分類帳、明細分類帳、明細科目餘額表）

---

## 已知設計缺陷與修正說明

| # | 問題 | 嚴重程度 | 修正計畫 |
|---|------|----------|----------|
| 1 | 試算表（FN006）缺少「期初餘額」欄，為五欄而非標準六欄 | 🔴 高 | Phase 1-C |
| 2 | 無法鎖定已關帳期間，任何時候都可補登歷史傳票 | 🔴 高 | Phase 1-A |
| 3 | 損益表（FN007）科目跨年不歸零，下年度報表包含歷史累計 | 🔴 高 | Phase 3-B（FN008 資產負債表的「本期損益」同根源） |
| 4 | 薪資模組已建立，但無對應傳票自動產生（薪資暫緩，待薪資模組設計完整後處理） | 🟠 中 | Phase 2-A |
| 5 | 材料領用無傳票，存貨帳與財務帳脫節；且 MaterialIssue 缺少「確認狀態」欄位，無法判斷哪些領料單已確認可轉傳票 | 🔴 高 | Phase 2-B + Entity 補充 |
| 6 | 生產完工 `IsJournalized` 旗標應設在 `ProductionScheduleCompletion`（分批完工記錄），而非 `ProductionScheduleItem`（排程主檔）；目前設計層級有誤 | 🟠 中 | Phase 2-C 設計修正 |
| 7 | `ProductionScheduleCompletion.ActualUnitCost` 與 `InventoryTransactionId` 皆為 nullable；若未填入，傳票金額將為零 | 🟠 中 | Phase 2-C 確認必填規則 |
| 8 | 無 AR/AP 帳齡分析，無法管理應收/應付逾期 | 🟡 中 | Phase 3-A |
| 9 | 沖銷傳票（ReverseEntryAsync）無驗證目標期間是否已關帳，可能沖到已鎖定期間 | 🟠 中 | Phase 1-A 整合 |
| 10 | P3-C（業務單據顯示相關傳票）`JournalEntryService.GetBySourceDocumentAsync` 已存在，但五個 EditModal 皆尚未實作 UI | 🟡 低 | Phase 3-C（可立即執行） |

## 相關文件

- [README_會計_科目表.md](README_會計_科目表.md)
- [README_會計_傳票系統.md](README_會計_傳票系統.md)
- [README_會計_批次轉傳票.md](README_會計_批次轉傳票.md)
- [README_會計_子科目系統.md](README_會計_子科目系統.md)
- [README_會計_財務報表.md](README_會計_財務報表.md)
- [README_會計_Phase1_基礎補強.md](README_會計_Phase1_基礎補強.md)
- [README_會計_Phase2_業務整合.md](README_會計_Phase2_業務整合.md)
- [README_會計_Phase3_帳務管理.md](README_會計_Phase3_帳務管理.md)
- [README_會計_Phase4_進階功能.md](README_會計_Phase4_進階功能.md)
