# 會計模組 Phase 2：業務模組傳票整合

## 更新日期
2026-03-14

## 優先等級
🟠 高優先（Phase 1 完成後執行）

---

## 概述

Phase 2 補齊現有業務模組缺少的傳票自動產生整合：**薪資計提/發放**、**材料領用**、**生產完工入庫**。
完成後，系統所有主要業務流程均可產生對應傳票。

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

### Entity 現況（程式碼確認）

`MaterialIssueDetail` 已有：
```csharp
public decimal? UnitCost { get; set; }           // 單位成本（nullable）
[NotMapped]
public decimal? TotalCost => UnitCost * IssueQuantity;  // 計算屬性，非 DB 欄位
```

### 成本來源問題（待確認）

傳票金額需取自 `MaterialIssueDetail.TotalCost`，但 `UnitCost` 為 nullable。需確認：

| 問題 | 待確認內容 |
|------|-----------|
| **UnitCost 何時填入？** | 領料確認時系統自動填入當下移動均價？還是需要人工輸入？ |
| **有沒有出庫動作？** | 領料確認時是否執行 `ReduceStockAsync`？若有，InventoryTransaction 已建立可取 TotalAmount |
| **MaterialIssueDetail 有無 InventoryTransactionId？** | 目前無此 FK；若需追蹤成本來源，應考慮新增 |

**建議方案：** 領料確認時自動填入移動均價快照（`UnitCost = Product.AverageCost`），TotalCost 作為傳票金額來源，無需另加 FK。

### 缺口：MaterialIssue 缺少確認狀態

**現況：** `MaterialIssue` Entity 無 `ConfirmedStatus`、`IsConfirmed`、`ConfirmedAt` 等欄位，無法判斷哪些領料單已確認完成、哪些仍是草稿。

**影響：** 批次轉傳票頁面需篩選「已確認的領料單」，但沒有狀態欄位無法做到。

**需補充至 MaterialIssue Entity：**
```csharp
public bool IsConfirmed { get; set; } = false;
public DateTime? ConfirmedAt { get; set; }
public int? ConfirmedByEmployeeId { get; set; }
```

### 業務流程

```
領料單確認（IsConfirmed = true）
  └─ 批次轉傳票
       ├─ 借：在製品 / 製造費用 (視用途) = 領用成本（TotalCost 加總）
       └─ 貸：原料存貨 (1221) 或 商品存貨 (1231) = 領用成本
```

### 領用用途分類（需新增欄位）

| 用途 | 借方科目 |
|------|---------|
| 生產用料 | 在製品 (1321) 或 直接原料費用 |
| 一般消耗 | 物料費用 (6xxx) |
| 樣品 | 樣品費用 (6xxx) |

> **現況：** `MaterialIssue` Entity 目前無用途分類欄位（`Purpose` / `IssueType`），需新增後才能決定借方科目。

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
- 顯示欄位：領料單號、領料日期、用途、金額（`MaterialIssueDetail.TotalCost` 加總）
- 篩選條件：`IsConfirmed == true AND IsJournalized == false`

---

## P2-C：生產完工傳票整合

### 問題說明

生產排程模組（`ProductionBoardItemCard.razor`、`ProductionBoardItemEditModal.razor`）
有完工功能，但完工後沒有對應存貨增加傳票，導致：
- 完工品的存貨帳面不正確（實際庫存增加，但帳面沒有對應科目）
- 無法追蹤生產成本的流動（在製品 → 製成品）

### Entity 現況（程式碼確認）

`ProductionScheduleCompletion` 已有：
```csharp
public decimal? ActualUnitCost { get; set; }      // 入庫時單位成本（nullable）
public int? InventoryTransactionId { get; set; }  // FK → InventoryTransaction（nullable）
```

### 成本計算方式（決策：選項 C）

> **採用選項 C：取自 `InventoryTransaction.TotalAmount`**（與銷貨出貨 COGS 邏輯一致）

完工入庫時 `ReduceStockAsync`（或對應的 `AddStockAsync`）會建立 InventoryTransaction，
`TotalAmount` = 完工數量 × 當時移動均價，是最準確且與庫存模組一致的成本來源。

**⚠ 風險：兩個欄位均為 nullable**

若 `InventoryTransactionId == null` 或 `ActualUnitCost == null`，傳票金額將為零，建立空白分錄。
需確認完工入庫流程是否**一定**會填入這兩個欄位；若不確定，批次轉傳票前應加入檢查：
```
若 InventoryTransactionId == null AND ActualUnitCost == null
  → 顯示警告，跳過此筆，不建立傳票（避免零元分錄）
```

### IsJournalized 欄位（設計修正）

> ⚠ **重要：`IsJournalized` 應加在 `ProductionScheduleCompletion`，而非 `ProductionScheduleItem`**

理由：一個生產排程項目（`ProductionScheduleItem`）可有多次分批完工（`ProductionScheduleCompletion`），
每次完工獨立入庫、獨立建一張傳票，需要在 Completion 層級追蹤轉傳票狀態。

```csharp
// 新增至 ProductionScheduleCompletion Entity
public bool IsJournalized { get; set; } = false;
public DateTime? JournalizedAt { get; set; }
```

### 業務流程

```
生產完工確認（每次分批完工）
  └─ 批次轉傳票（每筆 ProductionScheduleCompletion 一張傳票）
       ├─ 借：製成品存貨 (1241) = InventoryTransaction.TotalAmount
       └─ 貸：在製品 (1321)    = InventoryTransaction.TotalAmount
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
| `AddPayrollBatchIsJournalized` | `PayrollBatch` 新增 `IsJournalized`、`JournalizedAt`（薪資暫緩） |
| `AddMaterialIssueFields` | `MaterialIssue` 新增 `IsConfirmed`、`ConfirmedAt`、`ConfirmedByEmployeeId`、`IsJournalized`、`JournalizedAt`、`Purpose`（用途分類） |
| `AddProductionCompletionIsJournalized` | `ProductionScheduleCompletion` 新增 `IsJournalized`、`JournalizedAt` |

---

## 完成標準（Definition of Done）

### P2-A 薪資傳票（暫緩）
- [ ] 薪資模組重新設計完成後處理
- [ ] 注意民國年 → 西元年轉換（`payrollYear + 1911`）

### P2-B 材料領用傳票
- [ ] 確認領料確認流程中 `UnitCost` 的填入時機
- [ ] `MaterialIssue` 新增 `IsConfirmed`、`Purpose` 欄位 + Migration
- [ ] `MaterialIssue` 新增 `IsJournalized`、`JournalizedAt` + Migration
- [ ] `JournalEntryAutoGenerationService` 新增材料領用方法
- [ ] 批次轉傳票頁面新增材料領用 Section（篩選 IsConfirmed = true、IsJournalized = false）

### P2-C 生產完工傳票
- [ ] 確認 `ProductionScheduleCompletion.InventoryTransactionId` 是否一定有值
- [ ] `ProductionScheduleCompletion` 新增 `IsJournalized`、`JournalizedAt` + Migration（不加在 ProductionScheduleItem 上）
- [ ] 批次轉傳票頁面顯示警告：InventoryTransactionId 為 null 的完工記錄跳過
- [ ] `JournalEntryAutoGenerationService` 新增生產完工方法（每筆 Completion 一張傳票）
- [ ] 批次轉傳票頁面新增生產完工 Section

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_批次轉傳票.md](README_會計_批次轉傳票.md)
- [README_會計_Phase1_基礎補強.md](README_會計_Phase1_基礎補強.md)
- [README_會計_Phase3_帳務管理.md](README_會計_Phase3_帳務管理.md)
