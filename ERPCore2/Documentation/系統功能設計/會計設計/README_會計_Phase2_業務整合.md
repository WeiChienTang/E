# 會計模組 Phase 2：業務模組傳票整合

## 更新日期
2026-03-08

## 優先等級
🟠 高優先（Phase 1 完成後執行）

---

## 概述

Phase 2 補齊現有業務模組缺少的傳票自動產生整合：**薪資計提/發放**、**材料領用**、**生產完工入庫**。
完成後，系統所有主要業務流程均可產生對應傳票。

---

## P2-A：薪資傳票整合

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

`JournalEntryBatchPage.razor` 新增第六個 Section：
- 標題：**薪資**
- 顯示欄位：薪資批次月份、應發薪資合計、員工人數、狀態
- 按鈕：計提傳票 / 發放傳票（各別操作）

---

## P2-B：材料領用傳票整合

### 問題說明

`MaterialIssue`（領料單）Entity 存在於 Warehouse 模組，
但 `MaterialIssueModal.razor` 已被刪除（重構為 `MaterialIssueEditModalComponent.razor`，
見 git status `M Components/Pages/Warehouse/MaterialIssueEditModalComponent.razor`）。
領料確認後沒有自動產生傳票，存貨帳與財務帳脫節。

### 業務流程

```
領料單確認
  └─ 批次轉傳票
       ├─ 借：在製品 / 製造費用 (視用途) = 領用成本
       └─ 貸：原料存貨 (1221) 或 商品存貨 (1231) = 領用成本
```

### 領用用途分類（需確認現有 Entity 是否有 Purpose 欄位）

| 用途 | 借方科目 |
|------|---------|
| 生產用料 | 在製品 (1321) 或 直接原料費用 |
| 一般消耗 | 物料費用 (6xxx) |
| 樣品 | 樣品費用 (6xxx) |

> **待確認：** `MaterialIssue` Entity 是否有用途分類欄位？若無需先新增。

### IsJournalized 欄位

```csharp
// 新增至 MaterialIssue Entity
public bool IsJournalized { get; set; } = false;
public DateTime? JournalizedAt { get; set; }
```

### 科目常數（補充）

| 常數 | 代碼 | 科目名稱 |
|------|------|---------|
| RawMaterialCode | 1221 | 原料 |
| WorkInProgressCode | 1321 | 在製品 |
| SuppliesExpenseCode | 6311 | 物料費用（視科目表） |

### 批次轉傳票頁面新增

`JournalEntryBatchPage.razor` 新增第七個 Section：
- 標題：**材料領用**
- 顯示欄位：領料單號、領料日期、用途、金額（移動均價 × 數量）
- 金額來源：`InventoryTransaction.TotalAmount`（同出庫邏輯）

---

## P2-C：生產完工傳票整合

### 問題說明

生產排程模組（`ProductionBoardItemCard.razor`、`ProductionBoardItemEditModal.razor`）
有完工功能，但完工後沒有對應存貨增加傳票，導致：
- 完工品的存貨帳面不正確（實際庫存增加，但帳面沒有對應科目）
- 無法追蹤生產成本的流動（在製品 → 製成品）

### 業務流程

```
生產訂單完工確認
  └─ 批次轉傳票
       ├─ 借：製成品存貨 (1241) = 完工品入庫成本
       └─ 貸：在製品 (1321)    = 完工品入庫成本
```

> **成本計算方式待確認：** 目前生產完工成本如何計算？
> 選項A：按標準成本（需先設定商品標準成本）
> 選項B：按實際投入原料成本（需彙總對應領料單）
> 選項C：按出庫均價倒推（與現行庫存邏輯一致）

### IsJournalized 欄位

需確認生產相關 Entity 名稱，新增：
```csharp
public bool IsJournalized { get; set; } = false;
public DateTime? JournalizedAt { get; set; }
```

### 科目常數（補充）

| 常數 | 代碼 | 科目名稱 |
|------|------|---------|
| FinishedGoodsCode | 1241 | 製成品 |
| WorkInProgressCode | 1321 | 在製品 |

### 批次轉傳票頁面新增

`JournalEntryBatchPage.razor` 新增第八個 Section：
- 標題：**生產完工**
- 顯示欄位：生產單號、完工日期、商品名稱、完工數量、成本金額

---

## 批次轉傳票頁面整體架構（更新後）

```
JournalEntryBatchPage.razor
├─ Section 1：進貨入庫
├─ Section 2：進貨退回
├─ Section 3：銷貨出貨
├─ Section 4：銷貨退回
├─ Section 5：沖款單
├─ Section 6：薪資（P2-A 新增）
├─ Section 7：材料領用（P2-B 新增）
└─ Section 8：生產完工（P2-C 新增）
```

---

## Migration 計畫

| Migration | 說明 |
|-----------|------|
| `AddPayrollBatchIsJournalized` | `PayrollBatch` 新增 `IsJournalized`、`JournalizedAt` |
| `AddMaterialIssueIsJournalized` | `MaterialIssue` 新增 `IsJournalized`、`JournalizedAt`（若尚未有） |
| `AddProductionOrderIsJournalized` | 生產單 Entity 新增 `IsJournalized`、`JournalizedAt` |

---

## 完成標準（Definition of Done）

- [ ] `JournalEntryAutoGenerationService` 新增薪資計提、薪資發放方法
- [ ] `PayrollBatch` 新增 `IsJournalized`、`JournalizedAt`
- [ ] 批次轉傳票頁面新增薪資 Section，可正確產生傳票
- [ ] `JournalEntryAutoGenerationService` 新增材料領用方法
- [ ] 材料領用 Entity 新增 `IsJournalized`
- [ ] 批次轉傳票頁面新增材料領用 Section
- [ ] 確認生產完工成本計算方式，實作完工傳票方法
- [ ] 批次轉傳票頁面新增生產完工 Section

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_批次轉傳票.md](README_會計_批次轉傳票.md)
- [README_會計_Phase1_基礎補強.md](README_會計_Phase1_基礎補強.md)
- [README_會計_Phase3_帳務管理.md](README_會計_Phase3_帳務管理.md)
