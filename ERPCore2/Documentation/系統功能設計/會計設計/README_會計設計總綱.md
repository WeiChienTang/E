# 會計模組設計總綱

## 更新日期
2026-02-25

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

| 文件 | 說明 | 適用場景 |
|------|------|----------|
| [README_會計_科目表.md](README_會計_科目表.md) | AccountItem Entity、Enum、Seeder、Service、UI 元件、FN005 報表 | 科目表管理與查詢 |
| [README_會計_傳票系統.md](README_會計_傳票系統.md) | JournalEntry / JournalEntryLine Entity、Service、EditModal | 傳票手動輸入與過帳 |
| [README_會計_批次轉傳票.md](README_會計_批次轉傳票.md) | IsJournalized 欄位、AutoGeneration Service、批次轉傳票頁面 | 業務單據自動轉傳票 |
| [README_會計_子科目系統.md](README_會計_子科目系統.md) | SubAccountService、系統參數設定、兩種代碼格式、批次補建 | 客戶 / 廠商 / 商品子科目自動產生 |
| [README_會計_財務報表.md](README_會計_財務報表.md) | FN006 試算表、FN007 損益表、FN008 資產負債表、FN009–FN011 帳冊報表 | 財務報表產生與查詢 |

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

> 詳細設計決策說明請見各子文件。

---

## 待辦項目

### 中長期

- [ ] 期初存貨建立功能（正式程序輸入量+成本，同時產生 InventoryTransaction + JournalEntry：借 商品存貨 / 貸 期初存貨調整）
- [ ] 期末結帳功能（Closing Entries：收入/費用科目歸零 → 轉本期損益 → 轉保留盈餘）
- [ ] 現金流量表（間接法）
- [ ] 報表匯出功能（PDF / Excel）
- [ ] 在業務單據 EditModal 加入「相關傳票」顯示區塊（透過 SourceDocumentType + SourceDocumentId 查詢）

---

## 相關文件

- [README_會計_科目表.md](README_會計_科目表.md)
- [README_會計_傳票系統.md](README_會計_傳票系統.md)
- [README_會計_批次轉傳票.md](README_會計_批次轉傳票.md)
- [README_會計_子科目系統.md](README_會計_子科目系統.md)
- [README_會計_財務報表.md](README_會計_財務報表.md)
