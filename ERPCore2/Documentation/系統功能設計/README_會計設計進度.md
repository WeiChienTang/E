# 會計模組設計進度

## 更新日期
2026-02-22

---

## 概述

本文件記錄 ERPCore2 會計模組的設計進度，目標是建立完整的會計科目表（Chart of Accounts）系統，並支援未來的自動財務報表產生功能。

---

## 設計目標

1. **會計科目表管理** — 建立符合台灣「商業會計項目表（112年度）」的標準科目表
2. **樹狀階層結構** — 支援多層級科目，透過自我參照（ParentId）建立父子關係
3. **自動產生報表** — 未來可透過科目階層彙總，自動產出資產負債表、損益表等財務報表
4. **種子資料載入** — 系統初始化時自動載入 553 筆標準會計科目

---

## 已完成項目

### 1. Entity 設計 — AccountItem

**檔案位置：** `Data/Entities/FinancialManagement/AccountItem.cs`

繼承 `BaseEntity`，額外定義以下欄位：

| 欄位 | 型別 | 說明 |
|------|------|------|
| Name | string (Required, Max 100) | 會計項目名稱（中文） |
| EnglishName | string? (Max 200) | 英文名稱 |
| Description | string? (Max 500) | 項目說明（中文） |
| EnglishDescription | string? (Max 500) | 英文說明 |
| AccountLevel | int (Required) | 科目層級（1~4） |
| ParentId | int? | 父層科目 ID（自我參照外鍵） |
| AccountType | AccountType (Required) | 科目大類（列舉） |
| Direction | AccountDirection (Required) | 借貸方向（列舉） |
| IsDetailAccount | bool | 是否為明細科目（僅明細科目可記帳） |
| SortOrder | int | 排序順序 |

**BaseEntity 已提供：** Id、Code、Status、CreatedAt、UpdatedAt、CreatedBy、UpdatedBy、Remarks

### 2. Enum 定義

**檔案位置：** `Models/Enums/AccountingEnums.cs`

#### AccountType（科目大類）

| 值 | 名稱 | 說明 |
|----|------|------|
| 1 | Asset | 資產 |
| 2 | Liability | 負債 |
| 3 | Equity | 權益 |
| 4 | Revenue | 營業收入 |
| 5 | Cost | 營業成本 |
| 6 | Expense | 營業費用 |
| 7 | NonOperatingIncomeAndExpense | 營業外收益及費損 |
| 8 | ComprehensiveIncome | 綜合損益總額 |

#### AccountDirection（借貸方向）

| 值 | 名稱 | 說明 |
|----|------|------|
| 1 | Debit | 借方（資產、成本、費用類） |
| 2 | Credit | 貸方（負債、權益、收入類） |

### 3. DbContext 設定

**檔案位置：** `Data/Context/AppDbContext.cs`

已新增內容：

- `DbSet<AccountItem> AccountItems` 屬性
- OnModelCreating 中的自我參照關聯配置（Parent ↔ Children）
- 刪除行為設定為 `DeleteBehavior.Restrict`（防止孤立記錄）
- 唯一索引：Code
- 查詢索引：AccountType、AccountLevel、ParentId、IsDetailAccount

### 4. Seeder 種子資料

**檔案位置：** `Data/SeedDataManager/Seeders/AccountItemSeeder.cs`

**資料來源：** 商業會計項目表 112 年度（Excel）

資料統計：

| 層級 | 數量 | 說明 |
|------|------|------|
| Level 1 | 8 筆 | 大分類（資產、負債、權益、收入、成本、費用、營業外、綜合損益） |
| Level 2 | 20 筆 | 子分類（流動資產、非流動資產、流動負債等） |
| Level 3 | 94 筆 | 中分類（現金及約當現金、應收帳款等） |
| Level 4 | 431 筆 | 明細科目（庫存現金、銀行存款等）— 實際記帳使用 |
| **合計** | **553 筆** | |

**Seeder 運作邏輯（兩階段式）：**

1. **第一階段：** 建立所有 553 筆 AccountItem（不含 ParentId），儲存取得自動產生的 Id
2. **第二階段：** 根據科目代碼前綴對應，回頭設定每筆資料的 ParentId，建立完整樹狀階層

### 5. SeedData 註冊

**檔案位置：** `Data/SeedData.cs`

已將 `AccountItemSeeder` 加入種子器清單，排在最後執行。

### 6. Migration 狀態

已執行 `Add-Migration` 與 `Update-Database`，資料表已建立並載入種子資料。

---

## 階層結構說明

科目表採用四層樹狀結構，以「庫存現金」為例：

```
Level 1: Code "1"     → 資產（ParentId: null）
  Level 2: Code "11-12" → 流動資產（ParentId → "1"）
    Level 3: Code "111"    → 現金及約當現金（ParentId → "11-12"）
      Level 4: Code "1111"   → 庫存現金（ParentId → "111"）← 實際記帳科目
```

**原始 Excel 的層級對應：**

- Excel「一級項目」中的**單位數代碼**（1~8）→ 資料庫 Level 1
- Excel「一級項目」中的**多位數代碼**（11-12、31 等）→ 資料庫 Level 2
- Excel「二級項目」（111、112 等三位數代碼）→ 資料庫 Level 3
- Excel「四級項目」（1111、1112 等四位數代碼）→ 資料庫 Level 4
- Excel「三級項目」在原始資料中完全為空，故不使用

---

## 待辦項目（未來規劃）

### 近期

- [ ] 建立 AccountItem 的 Service 層（IAccountItemService / AccountItemService）
- [ ] 建立 FieldConfiguration（篩選器與表格欄位定義）
- [ ] 建立 Index 列表頁面（含樹狀結構顯示）
- [ ] 建立 EditModal 編輯表單

### 中期

- [ ] 建立會計分錄（Journal Entry）Entity — 記帳的核心功能
- [ ] 建立科目餘額查詢功能
- [ ] 實作科目層級彙總邏輯

### 長期（報表系統）

- [ ] 自動產生資產負債表（Balance Sheet）
- [ ] 自動產生損益表（Income Statement）
- [ ] 自動產生綜合損益表
- [ ] 報表期間篩選（月報、季報、年報）
- [ ] 報表匯出功能（PDF / Excel）

---

## 相關檔案一覽

| 檔案 | 路徑 | 說明 |
|------|------|------|
| AccountItem.cs | `Data/Entities/FinancialManagement/` | Entity 定義 |
| AccountingEnums.cs | `Models/Enums/` | AccountType、AccountDirection 列舉 |
| AccountItemSeeder.cs | `Data/SeedDataManager/Seeders/` | 553 筆種子資料 |
| AppDbContext.cs | `Data/Context/` | DbSet 與關聯設定 |
| SeedData.cs | `Data/` | Seeder 註冊 |

---

## 設計決策記錄

### 為什麼採用單一表格 + 自我參照？

考量過兩種方案：

- **方案 A：單一表格 + ParentId 自我參照**（已採用）— 彈性最高，層級數量不固定，未來可擴充
- **方案 B：分兩張表（分類表 + 明細表）** — 查詢簡單，但層級固定，擴充彈性差

選擇方案 A 的原因：原始資料中第三級為空，實際層級數量不固定，樹狀結構可自然處理。且未來報表產生需要逐層彙總，樹狀結構是最適合的資料模型。

### 為什麼每筆資料都標記 AccountType？

雖然可以透過遞迴查詢頂層來判斷科目大類，但為了報表產生效能，每筆資料都直接標記 AccountType，避免每次查詢都需要遞迴到根節點。

### 借貸方向的預設規則

- **借方（Debit）：** 資產（1）、營業成本（5）、營業費用（6）、營業外收益及費損（7）
- **貸方（Credit）：** 負債（2）、權益（3）、營業收入（4）、綜合損益總額（8）
