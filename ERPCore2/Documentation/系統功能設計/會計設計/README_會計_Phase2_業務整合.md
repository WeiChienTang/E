# 會計模組 Phase 2：業務模組傳票整合

## 更新日期
2026-03-21

## 優先等級
🟡 中優先（待薪資模組重新設計後執行）

---

## 概述

Phase 2 原規劃補齊三個業務模組的傳票自動產生：**薪資計提/發放（P2-A）**、**材料領用（P2-B）**、**生產完工入庫（P2-C）**。

**目前狀態：**
- **P2-B 材料領用傳票**：✅ 已完成（2026-03-19）
- **P2-C 生產完工傳票**：✅ 已完成（2026-03-19）
- **P2-A 薪資傳票**：⏸ 暫緩（薪資模組設計尚未完整）

本文件保留 **P2-A** 設計細節供日後參考。P2-B/P2-C 的實作細節已納入原始碼，不再另行記錄。

---

## P2-A：薪資傳票整合

> ⚠ **暫緩：薪資模組設計尚未完整，P2-A 推遲至薪資模組重新設計完成後執行。**
> 以下設計內容保留供參考。

### 已知問題（暫緩前記錄）

- `PayrollPeriod.Year` 使用**民國年**（如 114），而 `JournalEntry.FiscalYear` 使用**西元年**（如 2025）
- 薪資傳票建立時須執行 `payrollYear + 1911` 的年份轉換，否則 FiscalYear 會被誤存為 114
- 薪資模組缺少明確的「已確認可轉傳票」狀態，需補充 `PayrollPeriodStatus`

### 問題說明

薪資模組（`Data/Entities/Payroll/`、`Services/Payroll/`）已在 Phase 1 之前建立，
但批次轉傳票頁面（`JournalEntryBatchPage.razor`）缺少「薪資」分類，
薪資計算結果無法轉入帳務系統。

### 業務流程

```
薪資計算確認
  └─ 批次轉傳票（薪資計提）
       ├─ 借：薪資費用 (6221)  = 各員工薪資合計
       ├─ 借：勞健保費用 (6xxx) = 雇主負擔保費合計（若有）
       └─ 貸：應付薪資 (2202)  = 應付合計

薪資發放（連結銀行轉帳記錄）
  └─ 批次轉傳票（薪資發放）
       ├─ 借：應付薪資 (2202)  = 實發薪資合計
       └─ 貸：銀行存款 (1113)  = 實發薪資合計
```

### 科目常數（補充至 JournalEntryAutoGenerationService）

| 常數 | 代碼 | 科目名稱 |
|------|------|---------|
| SalaryExpenseCode | 6221 | 薪資費用 |
| AccruedPayrollCode | 2202 | 應付薪資 |
| EmployerInsuranceCode | 6223 | 保險費（視科目表調整） |

### IsJournalized 欄位

薪資相關 Entity 需新增：
```csharp
public bool IsJournalized { get; set; } = false;
public DateTime? JournalizedAt { get; set; }
```

| Entity | 檔案路徑 |
|--------|---------|
| PayrollBatch（薪資批次） | `Data/Entities/Payroll/PayrollBatch.cs` |

### 分錄規則

**薪資計提（每月月底）：**
- `SourceDocumentType = "PayrollBatch"`
- 一筆傳票對應一個薪資批次
- 各員工薪資**合計**為一筆貸方分錄（不拆員工明細，除非啟用員工子科目）
- 若啟用員工子科目（Phase 後期功能），應付薪資可拆員工別

**薪資發放：**
- 另一筆傳票，`SourceDocumentType = "PayrollBatch"`，`Description = "薪資發放"`
- 沖銷應付薪資，轉入銀行存款（支出）

### 批次轉傳票頁面新增

`JournalEntryBatchPage.razor` 新增薪資 Section：
- 標題：**薪資**
- 顯示欄位：薪資批次月份、應發薪資合計、員工人數、狀態
- 按鈕：計提傳票 / 發放傳票（各別操作）

---

## 完成標準（Definition of Done）

### P2-A 薪資傳票（暫緩）
- [ ] 薪資模組重新設計完成後處理
- [ ] 注意民國年 → 西元年轉換（`payrollYear + 1911`）
- [ ] `PayrollBatch` 新增 `IsJournalized`、`JournalizedAt` + Migration
- [ ] `JournalEntryAutoGenerationService` 新增薪資計提/發放方法
- [ ] 批次轉傳票頁面新增薪資 Section

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_批次轉傳票.md](README_會計_批次轉傳票.md)
