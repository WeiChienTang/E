# 會計模組設計進度

## 更新日期
2026-02-23（第五次更新）

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

所有 Enum 值同時標記 `[Description]` 和 `[Display(Name)]`，前者供一般顯示邏輯使用，後者供報表篩選的 `DynamicFilterTemplate` 讀取。

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

#### AccountLevelFilter（層級篩選，用於報表）

| 值 | 名稱 | 說明 |
|----|------|------|
| 1 | Level1 | 第1層 |
| 2 | Level2 | 第2層 |
| 3 | Level3 | 第3層 |
| 4 | Level4 | 第4層 |
| 5 | Level5 | 第5層 |

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

### 7. Service 層

**檔案位置：** `Services/FinancialManagement/IAccountItemService.cs` / `AccountItemService.cs`

**介面自訂方法：**

| 方法 | 說明 |
|------|------|
| `IsAccountItemCodeExistsAsync(code, excludeId?)` | 檢查科目代碼是否重複（供 EntityCodeGenerationHelper 反射呼叫） |
| `GetByAccountTypeAsync(accountType)` | 依科目大類查詢 |
| `GetByLevelAsync(level)` | 依層級查詢 |
| `GetDetailAccountsAsync()` | 查詢所有明細科目 |
| `GetAllWithParentAsync()` | 查詢所有科目並含上層科目資料（Include Parent） |

> **命名規則注意：** `IsAccountItemCodeExistsAsync` 必須完整包含實體名稱，`EntityCodeGenerationHelper` 以反射尋找 `Is{EntityName}CodeExistsAsync` 方法來產生不重複編號。

### 8. FieldConfiguration

**檔案位置：** `Components/FieldConfiguration/AccountItemFieldConfiguration.cs`

定義 7 個欄位：Code、Name、AccountLevel、AccountType、Direction、IsDetailAccount、ParentName

### 9. Index 列表頁面

**檔案位置：** `Components/Pages/FinancialManagement/AccountItemIndex.razor`

- 路由：`/account-items`
- 功能：科目列表、新增、編輯、刪除、狀態切換
- 導覽：已加入 `NavigationConfig.cs`，QuickActionId = `"NewAccountItem"`

### 10. EditModal 編輯表單

**檔案位置：** `Components/Pages/FinancialManagement/AccountItemEditModalComponent.razor`

三個欄位群組：
- **基本資料**：科目代碼（自動產生）、科目名稱、英文名稱
- **科目屬性**：科目大類、借貸方向、科目層級、上層科目、是否明細科目
- **附加資料**：排序、說明、英文說明、狀態、備注

### 11. QuickAction 按鈕

**檔案位置：** `Components/Shared/Dashboard/QuickActionModalHost.razor`

已在 `_registry` 字典中註冊 `"NewAccountItem"` → `AccountItemEditModalComponent`，首頁快速新增按鈕可正常運作。

### 12. 會計科目表報表（FN005）

**相關檔案：**
- `Models/Reports/ReportIds.cs` — 新增 `AccountItemList = "FN005"` 常數
- `Models/Reports/FilterCriteria/AccountItemListCriteria.cs` — 篩選條件
- `Services/Reports/Interfaces/IAccountItemListReportService.cs` — 介面
- `Services/Reports/AccountItemListReportService.cs` — 服務實作
- `Data/Reports/ReportRegistry.cs` — FN005 報表定義
- `Models/Reports/FilterTemplates/FilterTemplateRegistry.cs` — FN005 篩選配置
- `Data/ServiceRegistration.cs` — DI 註冊

**篩選條件（AccountItemListCriteria）：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| AccountTypes | FilterEnum(AccountType) | 科目大類多選 |
| AccountDirections | FilterEnum(AccountDirection) | 借貸方向多選 |
| AccountLevels | FilterEnum(AccountLevelFilter) | 層級多選（第1~5層） |
| CodeKeyword | FilterKeyword | 科目代碼搜尋 |
| NameKeyword | FilterKeyword | 科目名稱搜尋 |
| DetailAccountOnly | FilterToggle | 僅顯示明細科目 |

**報表格式：** 依科目大類分組，表格欄位包含項次、科目代碼、科目名稱（含層級縮排）、層級、借貸方向、明細標記、上層科目。頁尾顯示統計數量及製表/財務主管簽名欄。

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

## FinancialTransaction 現況調查（2026-02-22）

### 檔案位置

`Data/Entities/FinancialManagement/FinancialTransaction.cs`
`Models/Enums/FinancialTransactionTypeEnum.cs`

### 現況摘要

| 項目 | 狀態 |
|------|------|
| 資料表（Migration） | ✅ 已建立 |
| DbSet | ✅ 存在於 AppDbContext |
| SetoffDocument 導航屬性 | ✅ `ICollection<FinancialTransaction>` |
| SetoffDocumentService Include | ✅ 有 `.Include(s => s.FinancialTransactions)` |
| IFinancialTransactionService | ❌ 不存在 |
| FinancialTransactionService | ❌ 不存在 |
| 寫入邏輯 | ❌ 無任何程式碼實際寫入資料 |
| 查詢/UI 頁面 | ❌ 不存在 |

### 設計意圖 vs 實際狀況

**設計意圖（推測）：** 記錄沖款單相關的收付款流水，屬於現金流帳層面的記錄。

**實際狀況：** `SetoffDocumentEditModalComponent.razor`（整個沖款作業的核心 Modal）完全不知道 FinancialTransaction 的存在：

- 未注入任何 `IFinancialTransactionService`
- 未建立、讀取或操作任何 FinancialTransaction 物件
- 收付款記錄完全由 **SetoffPayment** 負責

`SetoffDocumentService` 中的 `.Include(s => s.FinancialTransactions)` 只是載入一個永遠為空的集合，沒有任何實質作用。

### SetoffPayment 已完整取代其功能

SetoffPayment 已包含沖款收付款所需的全部欄位：

| SetoffPayment 欄位 | 說明 |
|--------------------|------|
| BankId | 銀行別 |
| PaymentMethodId | 付款方式 |
| ReceivedAmount | 收款金額 |
| AllowanceAmount | 折讓金額 |
| CheckNumber | 支票號碼 |
| DueDate | 到期日 |

### 結論：FinancialTransaction 已無存在意義

- **沖款收付款**：已由 SetoffPayment 完整實作，功能重複
- **未來傳票**：JournalEntry 是正確的設計（含 AccountItem FK、借貸分錄），FinancialTransaction 的結構不適合這個用途
- **沖銷流水帳**：此功能若有需要，應在 JournalEntry 系統中以「沖銷傳票」的方式實作

### 處置結果（2026-02-23 完成）

**已完成刪除，變更清單：**

| 動作 | 檔案 |
|------|------|
| 移除 navigation property | `Data/Entities/FinancialManagement/SetoffDocument.cs` |
| 移除 2 個 Include 呼叫 | `Services/FinancialManagement/SetoffDocumentService.cs` |
| 移除 DbSet + ModelBuilder 設定 | `Data/Context/AppDbContext.cs` |
| 移除 FinancialTransaction.Read 權限 | `Data/SeedDataManager/Seeders/PermissionSeeder.cs` |
| 刪除 Entity 檔案 | `Data/Entities/FinancialManagement/FinancialTransaction.cs` |
| 刪除 Enum 檔案 | `Models/Enums/FinancialTransactionTypeEnum.cs` |
| Migration | `20260222201713_RemoveFinancialTransaction` — 刪除 `FinancialTransactions` 資料表及 `SetoffPrepayments.FinancialTransactionId` shadow 欄位 |

資料表已從資料庫移除，無任何資料損失。

---

## 完整會計流程分析（2026-02-23）

標準會計循環共六個步驟，各步驟在 ERP 系統中的自動化程度如下：

### 1. 分錄（Journalizing）— 大部分可自動化

| 情境 | 自動/手動 | 觸發點 |
|------|-----------|--------|
| 進貨入庫 | **自動** | 進貨單審核後 → 借：存貨 / 貸：應付帳款 |
| 銷貨出貨 | **自動** | 銷貨單審核後 → 借：應收帳款 / 貸：銷貨收入 + 借：銷貨成本 / 貸：存貨 |
| 收款（沖款） | **自動** | 沖款單審核後 → 借：銀行/現金 / 貸：應收帳款 |
| 付款 | **自動** | 付款單審核後 → 借：應付帳款 / 貸：銀行/現金 |
| 薪資、折舊 | **手動** | 非例行交易，需會計人員輸入 |
| 更正分錄 | **手動** | 錯誤更正，需人工判斷 |

### 2. 過帳（Posting）— 完全自動化

傳票建立後，系統即時更新科目餘額，無須人工操作。

### 3. 試算（Trial Balance）— 完全自動化

只是一張查詢報表，系統隨時可產生，確認借貸平衡，不需人工介入。

### 4. 調整（Adjusting Entries）— 主要需人工

這是整個流程中**最需要人工判斷**的環節：

| 項目 | 自動/手動 |
|------|-----------|
| 應計費用（薪資未付、水電未開票） | 手動 |
| 遞延收益（預收款分攤） | 手動 |
| 固定資產折舊（直線法） | 半自動（系統算金額，人工確認） |
| 存貨跌價損失 | 手動 |
| 呆帳準備 | 手動 |

### 5. 結帳（Closing Entries）— 系統執行，人工觸發

步驟固定（收入/費用歸零 → 轉本期損益 → 轉保留盈餘），系統可自動執行，但需要人工按下「執行結帳」並確認。屬於期末（月/季/年）的固定程序。

### 6. 編表（Financial Statements）— 完全自動化

| 報表 | 說明 |
|------|------|
| 資產負債表 | 按科目大類（資產/負債/權益）彙算 |
| 損益表 | 按科目大類（收入/成本/費用）彙算 |
| 現金流量表 | 間接法需配合科目分類設定 |
| 綜合損益表 | 包含其他綜合損益項目 |

### 使用者實際需要做的事

```
業務人員（每日）：
  ✅ 審核業務單據（進貨、銷貨、沖款）→ 傳票自動產生

會計人員（每月）：
  ✅ 輸入非例行調整分錄（薪資、折舊確認）
  ✅ 期末盤點差異調整

會計人員（每期）：
  ✅ 按下「執行結帳」
  ✅ 核閱並匯出財務報表
```

### 開發優先順序建議

1. **JournalEntry Entity** — 傳票主檔 + 傳票分錄結構
2. **自動分錄（業務單據觸發）** — 覆蓋 80% 日常交易
3. **試算表報表** — 即時驗證借貸平衡
4. **手動分錄介面** — 供會計人員輸入調整分錄
5. **結帳功能 + 財務報表** — 最後實作

---

---

## JournalEntry 傳票系統（2026-02-23 完成）

### 13. Enum 定義 — JournalEntryEnums

**檔案位置：** `Models/Enums/JournalEntryEnums.cs`

| Enum | 值 | 說明 |
|------|---|------|
| JournalEntryType.AutoGenerated | 1 | 業務單據自動產生 |
| JournalEntryType.Manual | 2 | 會計人員手動輸入 |
| JournalEntryType.Adjusting | 3 | 調整分錄（期末用） |
| JournalEntryType.Closing | 4 | 結帳分錄 |
| JournalEntryType.Reversing | 5 | 沖銷分錄 |
| JournalEntryStatus.Draft | 1 | 草稿（可編輯，不影響帳） |
| JournalEntryStatus.Posted | 2 | 已過帳（鎖定，影響科目餘額） |
| JournalEntryStatus.Cancelled | 3 | 已取消 |
| JournalEntryStatus.Reversed | 4 | 已沖銷（原始傳票被反向沖銷） |

### 14. Entity 設計 — JournalEntry / JournalEntryLine

**檔案位置：** `Data/Entities/FinancialManagement/JournalEntry.cs` / `JournalEntryLine.cs`

**JournalEntry 主要欄位：**

| 欄位 | 型別 | 說明 |
|------|------|------|
| EntryDate | DateTime | 傳票日期 |
| EntryType | JournalEntryType | 傳票類型 |
| JournalEntryStatus | JournalEntryStatus | 傳票狀態 |
| Description | string? | 傳票說明 |
| CompanyId | int | 公司 FK |
| FiscalYear | int | 會計年度（如 2026） |
| FiscalPeriod | int (1-12) | 會計期間（月份） |
| SourceDocumentType | string? | 來源單據類型（"PurchaseReceiving" 等） |
| SourceDocumentId | int? | 來源單據 ID |
| SourceDocumentCode | string? | 來源單號（方便搜尋，免 JOIN） |
| TotalDebitAmount | decimal(18,2) | 借方合計 |
| TotalCreditAmount | decimal(18,2) | 貸方合計 |
| IsReversed | bool | 是否已被沖銷 |
| ReversalEntryId | int? | 沖銷傳票 ID（自我參照） |
| IsBalanced | [NotMapped] | TotalDebit == TotalCredit && > 0 |

**JournalEntryLine 主要欄位：**

| 欄位 | 型別 | 說明 |
|------|------|------|
| JournalEntryId | int | 傳票主檔 FK |
| LineNumber | int | 行次 |
| AccountItemId | int | 會計科目 FK（僅明細科目） |
| Direction | AccountDirection | 借方 / 貸方 |
| Amount | decimal(18,2) | 金額 |
| LineDescription | string? | 分錄說明 |

### 15. Migration

**基準線策略（因 Migrations 資料夾為空）：**

1. 建立空的 `20260222204622_InitialSchema`（Up/Down 皆為 no-op）作為基準線標記
2. 執行 `Update-Database` 將 InitialSchema 寫入 `__EFMigrationsHistory`（不做任何資料庫變更）
3. 加入 JournalEntry 回 DbContext，執行 `20260222204809_AddJournalEntry`
4. 只建立 JournalEntries + JournalEntryLines 兩張新資料表

### 16. Service 層 — IJournalEntryService / JournalEntryService

**檔案位置：** `Services/FinancialManagement/IJournalEntryService.cs` / `JournalEntryService.cs`

**命名空間：** `ERPCore2.Services`（非 sub-namespace，符合 `_Imports.razor` 慣例）

| 自訂方法 | 說明 |
|----------|------|
| `IsJournalEntryCodeExistsAsync` | 供 EntityCodeGenerationHelper 反射呼叫 |
| `GetByFiscalPeriodAsync` | 依會計年度+期間查詢（含分錄明細） |
| `GetBySourceDocumentAsync` | 依來源單據查詢已產生的傳票 |
| `GetDraftEntriesAsync` | 查詢所有草稿傳票 |
| `GetWithLinesAsync` | 取得含分錄明細的傳票（供 EditModal 使用） |
| `PostEntryAsync` | 過帳：Draft → Posted，須借貸平衡 |
| `ReverseEntryAsync` | 沖銷：建立反向傳票，原傳票標記 Reversed |
| `SaveWithLinesAsync` | 儲存傳票及分錄（含新增/更新/刪除行） |

### 17. FieldConfiguration / Index / EditModal

**已建立檔案：**

- `Components/FieldConfiguration/JournalEntryFieldConfiguration.cs` — 8 欄位：Code、EntryDate（DateRange）、EntryType（badge）、JournalEntryStatus（badge）、Description、TotalDebitAmount、TotalCreditAmount、SourceDocumentCode
- `Components/Pages/FinancialManagement/JournalEntryIndex.razor` — 路由 `/journal-entries`，標準 GenericIndexPageComponent
- `Components/Pages/FinancialManagement/JournalEntryEditModalComponent.razor` — **全客製化 EditModal**（含動態分錄行），非 GenericEditModal

**EditModal 特殊設計：**

- 主表單：EntryDate、EntryType、FiscalYear / FiscalPeriod（自動同步至 EntryDate）、Status（唯讀顯示）、Description
- 分錄明細表：選科目（僅明細科目）、借/貸方向、金額、說明、刪除按鈕
- 頁腳即時顯示：借方合計 / 貸方合計 / 差額 badge（平衡=綠色✓；不平衡=紅色差額）
- 操作按鈕依狀態控制：
  - 草稿：「儲存草稿」+「過帳」（不平衡時禁用）
  - 已過帳（未沖銷）：「關閉」+「沖銷此傳票」
  - 其他（已取消/已沖銷）：「關閉」
- IsReadOnly = true（當狀態為 Posted/Cancelled/Reversed）

### 18. QuickAction / 導覽

- `NavigationConfig.cs`：新增傳票管理，路由 `/journal-entries`，QuickActionId = `"NewJournalEntry"`
- `PermissionSeeder.cs`：新增 `JournalEntry.Read` 權限
- `QuickActionModalHost.razor`：註冊 `"NewJournalEntry"` → `JournalEntryEditModalComponent`

---

## 批次轉傳票系統（2026-02-23 完成）

### 設計決策：為何採用批次而非即時轉傳票？

業務單據確認後仍可修改內容（金額、數量等），若即時產生傳票，後續修改會造成傳票與單據金額不符。
採用批次方式，讓會計人員在月底或指定時點，審核業務單據無誤後再一次執行轉換，轉換後才以 `IsJournalized = true` 鎖定記錄。

### 19. 業務 Entity 新增 IsJournalized 欄位

**修改了 4 個 Entity：**

| Entity | 檔案路徑 |
|--------|---------|
| PurchaseReceiving | `Data/Entities/Purchase/PurchaseReceiving.cs` |
| PurchaseReturn | `Data/Entities/Purchase/PurchaseReturn.cs` |
| SalesDelivery | `Data/Entities/Sales/SalesDelivery.cs` |
| SalesReturn | `Data/Entities/Sales/SalesReturn.cs` |

每個 Entity 新增：

```csharp
[Display(Name = "已轉傳票")]
public bool IsJournalized { get; set; } = false;

[Display(Name = "轉傳票時間")]
public DateTime? JournalizedAt { get; set; }
```

**Migration：** `20260223011823_AddIsJournalizedToSourceDocuments`
— 4 張資料表各加 `IsJournalized (bit, default 0)` + `JournalizedAt (datetime2, nullable)`

### 20. IAccountItemService 新增 GetByCodeAsync

**檔案位置：** `Services/FinancialManagement/IAccountItemService.cs` / `AccountItemService.cs`

新增方法：`Task<AccountItem?> GetByCodeAsync(string code)`
— 依科目代碼查詢狀態為 Active 的科目，用於自動分錄時取得 AccountItemId。

### 21. IJournalEntryAutoGenerationService（新建）

**檔案位置：** `Services/FinancialManagement/IJournalEntryAutoGenerationService.cs`

```csharp
public interface IJournalEntryAutoGenerationService
{
    // 查詢待轉傳票的單據（IsJournalized = false）
    Task<List<PurchaseReceiving>> GetPendingPurchaseReceivingsAsync(DateTime? from = null, DateTime? to = null);
    Task<List<PurchaseReturn>>    GetPendingPurchaseReturnsAsync   (DateTime? from = null, DateTime? to = null);
    Task<List<SalesDelivery>>     GetPendingSalesDeliveriesAsync   (DateTime? from = null, DateTime? to = null);
    Task<List<SalesReturn>>       GetPendingSalesReturnsAsync      (DateTime? from = null, DateTime? to = null);
    Task<List<SetoffDocument>>    GetPendingSetoffDocumentsAsync   (DateTime? from = null, DateTime? to = null);

    // 轉傳票（建立傳票、自動過帳、標記 IsJournalized）
    Task<(bool Success, string ErrorMessage)> JournalizePurchaseReceivingAsync(int id, string createdBy);
    Task<(bool Success, string ErrorMessage)> JournalizePurchaseReturnAsync   (int id, string createdBy);
    Task<(bool Success, string ErrorMessage)> JournalizeSalesDeliveryAsync    (int id, string createdBy);
    Task<(bool Success, string ErrorMessage)> JournalizeSalesReturnAsync      (int id, string createdBy);
    Task<(bool Success, string ErrorMessage)> JournalizeSetoffDocumentAsync   (int id, string createdBy);
}
```

### 22. JournalEntryAutoGenerationService（新建）

**檔案位置：** `Services/FinancialManagement/JournalEntryAutoGenerationService.cs`

**科目代碼常數（來自 AccountItemSeeder 商業會計項目表 112 年度）：**

| 常數名稱 | 代碼 | 科目名稱 | 方向 |
|---------|------|---------|------|
| `AccountReceivableCode` | `"1191"` | 應收帳款 | Debit |
| `AccountPayableCode` | `"2171"` | 應付帳款 | Credit |
| `InventoryCode` | `"1231"` | 商品存貨 | Debit |
| `SalesRevenueCode` | `"4111"` | 銷貨收入 | Credit |
| `InputVatCode` | `"1268"` | 進項稅額 | Debit |
| `OutputVatCode` | `"2204"` | 銷項稅額 | Credit |
| `BankDepositCode` | `"1113"` | 銀行存款 | Debit |
| `SalesAllowanceCode` | `"4114"` | 銷貨折讓 | Debit |
| `PurchaseAllowanceCode` | `"5124"` | 進貨折讓 | Credit |
| `AdvanceFromCustomerCode` | `"2221"` | 預收貨款 | Credit |
| `AdvanceToSupplierCode` | `"1266"` | 預付貨款 | Debit |

**完整三行分錄規則（稅額 > 0 時產生第三行；稅額 = 0 時僅兩行）：**

```
進貨入庫 (PurchaseReceiving)：
  借：商品存貨 (1231)    = TotalAmount
  借：進項稅額 (1268)    = PurchaseReceivingTaxAmount       ← 稅額 > 0 才加
    貸：應付帳款 (2171)  = PurchaseReceivingTotalAmountIncludingTax

進貨退回 (PurchaseReturn)：
  借：應付帳款 (2171)    = TotalReturnAmountWithTax
    貸：商品存貨 (1231)  = TotalReturnAmount
    貸：進項稅額 (1268)  = ReturnTaxAmount                  ← 稅額 > 0 才加（沖回進項）

銷貨出貨 (SalesDelivery)：
  借：應收帳款 (1191)    = TotalAmountWithTax
    貸：銷貨收入 (4111)  = TotalAmount
    貸：銷項稅額 (2204)  = TaxAmount                        ← 稅額 > 0 才加

銷貨退回 (SalesReturn)：
  借：銷貨收入 (4111)    = TotalReturnAmount
  借：銷項稅額 (2204)    = ReturnTaxAmount                  ← 稅額 > 0 才加（沖回銷項）
    貸：應收帳款 (1191)  = TotalReturnAmountWithTax

應收沖款 (SetoffDocument, IsAccountsReceivable = true)：
  借：銀行存款 (1113)    = TotalCollectionAmount            ← > 0 才加
  借：銷貨折讓 (4114)    = TotalAllowanceAmount             ← > 0 才加
  借：預收貨款 (2221)    = PrepaymentSetoffAmount           ← > 0 才加（沖回預收）
    貸：應收帳款 (1191)  = CurrentSetoffAmount              ← 應收沖銷總額

應付沖款 (SetoffDocument, IsAccountsReceivable = false)：
  借：應付帳款 (2171)    = CurrentSetoffAmount              ← 應付沖銷總額
    貸：銀行存款 (1113)  = TotalCollectionAmount            ← > 0 才加
    貸：進貨折讓 (5124)  = TotalAllowanceAmount             ← > 0 才加
    貸：預付貨款 (1266)  = PrepaymentSetoffAmount           ← > 0 才加（沖回預付）
```

> **餘額等式確認：** `CurrentSetoffAmount = TotalCollectionAmount + TotalAllowanceAmount + PrepaymentSetoffAmount`，確保分錄借貸自動平衡。

**JournalizeXxxAsync 執行流程：**

1. `GetBySourceDocumentAsync(sourceType, id)` → 防重複：若已有傳票，回傳失敗
2. 讀取原始單據（含 Include Supplier / Customer）
3. 用 `GetByCodeAsync` 取得所需 AccountItem
4. 建立 `JournalEntry` + `JournalEntryLine` 清單
5. 呼叫私有 helper `CreateAndPostEntryAsync`：
   - `GetPrimaryCompanyAsync()` 取得 CompanyId
   - `SaveWithLinesAsync(entry, createdBy)` → EF Core 回填 `entry.Id`
   - `PostEntryAsync(entry.Id, createdBy)` → 傳票狀態轉為 Posted
6. 標記原始單據 `IsJournalized = true`、`JournalizedAt = DateTime.Now`，執行 `SaveChangesAsync`

### 23. JournalEntryBatchPage + 導覽（新建）

**頁面位置：** `Components/Pages/FinancialManagement/JournalEntryBatchPage.razor`
**路由：** `/journal-entry-batch`
**導覽項目：** NavigationConfig.cs，權限 `JournalEntry.Read`

**頁面功能：**

- 頂部日期範圍篩選（從/至）+ 查詢、清除篩選按鈕
- 5 個可折疊 Section，各顯示 `IsJournalized = false` 的待轉清單：
  - 進貨入庫（Code、進貨日期、供應商、未稅金額、稅額、含稅總額）
  - 進貨退回（Code、退回日期、供應商、未稅金額、稅額、含稅總額）
  - 銷貨出貨（Code、出貨日期、客戶、未稅金額、稅額、含稅總額）
  - 沖款單（Code、沖款日期、應收/應付 badge、沖銷金額、收付款金額、折讓金額）
  - 銷貨退回（Code、退回日期、客戶、未稅金額、稅額、含稅總額）
- 每列有「轉傳票」按鈕（單筆）
- 右上角「全部轉傳票（N 筆）」批次按鈕（含 5 類）
- 轉換結果以 NotificationService 顯示成功/失敗通知，完成後重新載入清單
- 查詢使用 `Task.WhenAll` 並行取得五類待轉清單

### 24. SetoffDocument 新增 IsJournalized 欄位

**修改 Entity：** `Data/Entities/FinancialManagement/SetoffDocument.cs`

```csharp
[Display(Name = "已轉傳票")]
public bool IsJournalized { get; set; } = false;

[Display(Name = "轉傳票時間")]
public DateTime? JournalizedAt { get; set; }
```

**Migration：** `20260223031940_AddIsJournalizedToSetoffDocument`
— `SetoffDocuments` 資料表加 `IsJournalized (bit, default 0)` + `JournalizedAt (datetime2, nullable)`

---

## 財務報表系統（2026-02-23 完成）

### 25. 試算表（FN006）

**相關檔案：**
- `Models/Reports/FilterCriteria/TrialBalanceCriteria.cs` — 篩選條件
- `Services/Reports/Interfaces/ITrialBalanceReportService.cs` — 服務介面
- `Services/Reports/TrialBalanceReportService.cs` — 服務實作

**篩選條件（TrialBalanceCriteria）：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| StartDate / EndDate | FilterDateRange | 傳票日期範圍（本期發生額的起訖） |
| AccountTypes | FilterEnum(AccountType) | 科目大類多選（空=全部） |
| ShowZeroBalance | FilterToggle | 顯示零餘額科目（預設 false） |

**報表格式：**
- 依 AccountType 分組，每組顯示科目明細表格
- 欄位：科目代碼 | 科目名稱 | 本期借方 | 本期貸方 | 期末借方餘額 | 期末貸方餘額
- 頁尾：借方/貸方合計各驗證平衡（✓ 平衡 或顯示差額）、製表人員/財務主管簽名欄

**查詢邏輯（兩次查詢）：**
- Query 1：`EntryDate BETWEEN StartDate AND EndDate` → 本期借方/貸方發生額
- Query 2：`EntryDate <= EndDate`（不限起始）→ 期末累計借方/貸方餘額

**期末餘額計算：**
- 借方正常科目（資產/成本/費用）：`EndingDebitBalance = CumulativeDebit - CumulativeCredit`（正值）
- 貸方正常科目（負債/權益/收入）：`EndingCreditBalance = CumulativeCredit - CumulativeDebit`（正值）

---

### 26. 損益表（FN007）

**相關檔案：**
- `Models/Reports/FilterCriteria/IncomeStatementCriteria.cs` — 篩選條件
- `Services/Reports/Interfaces/IIncomeStatementReportService.cs` — 服務介面
- `Services/Reports/IncomeStatementReportService.cs` — 服務實作

**篩選條件（IncomeStatementCriteria）：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| StartDate / EndDate | FilterDateRange | 會計期間（傳票日期範圍） |

**固定查詢範圍：** AccountType in {Revenue(4), Cost(5), Expense(6), NonOperatingIncomeAndExpense(7)}

**報表結構：**
```
一、銷貨收入 xxx
    各科目明細...

二、減：銷貨成本 (xxx)
    各科目明細...

毛利潤：xxx

三、減：營業費用 (xxx)
    各科目明細...

營業損益：xxx

四、營業外收益及費損 xxx（淨額）
    各科目明細（收益加號/費損負號）...

稅前損益：xxx
```

**餘額計算規則：**
- Revenue (Credit 正常)：Balance = Credit - Debit
- Cost/Expense (Debit 正常)：Balance = Debit - Credit
- NonOperating：依各科目 `Direction` 屬性決定（Credit 正常則 Credit-Debit；Debit 正常則 Debit-Credit）

---

### 27. 資產負債表（FN008）

**相關檔案：**
- `Models/Reports/FilterCriteria/BalanceSheetCriteria.cs` — 篩選條件
- `Services/Reports/Interfaces/IBalanceSheetReportService.cs` — 服務介面
- `Services/Reports/BalanceSheetReportService.cs` — 服務實作

**篩選條件（BalanceSheetCriteria）：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| StartDate / EndDate | FilterDateRange | EndDate 作為截止日（StartDate 選填，若填則限制累計起點） |

**核心屬性：** `AsOfDate` = `EndDate ?? DateTime.Today`（截止日）

**固定查詢範圍：** AccountType in {Asset(1), Liability(2), Equity(3)}，`EntryDate <= AsOfDate`

**報表結構：**
```
【資產】
  各科目明細（餘額 = Debit - Credit）
資產合計 xxx

【負債】
  各科目明細（餘額 = Credit - Debit）
負債合計 xxx

【權益】
  各科目明細（餘額 = Credit - Debit）
權益合計 xxx

負債及權益合計 xxx
```
- 頁尾：驗證 `|資產合計 - 負債及權益合計| < 0.01`（容差），✓ 平衡 或 ⚠ 差額

**恆等式：** 資產合計 = 負債合計 + 權益合計（會計基本等式）

---

### 通用架構（FN006~FN008 共通）

| 項目 | 設計 |
|------|------|
| 服務介面模式 | **不繼承** `IEntityReportService<T>`，只定義 `RenderBatchToImagesAsync(XxxCriteria)` |
| 呼叫方式 | `GenericReportFilterModalComponent` 透過反射找到具體類型的 `RenderBatchToImagesAsync` 方法 |
| 入口點 | 報表中心（Alt+R 或選單）→ 搜尋報表名稱 → 篩選 → 預覽 → 列印 |
| DI 注冊 | `AddScoped<IXxxReportService, XxxReportService>()` 於 `ServiceRegistration.cs` |
| 報表定義 | `ReportRegistry.cs`，Category = `ReportCategory.Financial`，RequiredPermission = `JournalEntry.Read` |
| 篩選模板 | `FilterTemplateRegistry.cs`，使用 `DynamicFilterTemplate` |

---

## 待辦項目（未來規劃）

### 近期

- [ ] 在業務單據的 EditModal 加入「相關傳票」顯示區塊（透過 SourceDocumentType + SourceDocumentId 查詢）
- [ ] 銷貨成本分錄（COGS）：借 銷貨成本(5111) / 貸 商品存貨(1231)（需先確認庫存成本計算方式）

### 報表系統（2026-02-23 完成）

- [x] **FN006 試算表（Trial Balance）** — 本期發生額（StartDate ~ EndDate）＋期末累計餘額（~ EndDate），依科目大類分組，頁尾驗證借貸平衡
- [x] **FN007 損益表（Income Statement）** — 彙總 Revenue/Cost/Expense/NonOperating，顯示毛利潤、營業損益、稅前損益
- [x] **FN008 資產負債表（Balance Sheet）** — 累積至截止日（EndDate）的 Asset/Liability/Equity 餘額，頁尾驗證 資產 = 負債 + 權益

### 中長期

- [ ] 科目餘額細帳（依科目查傳票明細，類似帳卡）
- [ ] 期末結帳功能（Closing Entries：收入/費用科目歸零 → 轉本期損益 → 轉保留盈餘）
- [ ] 現金流量表（間接法）
- [ ] 報表匯出功能（PDF / Excel）

---

## 相關檔案一覽

| 檔案 | 路徑 | 說明 |
|------|------|------|
| AccountItem.cs | `Data/Entities/FinancialManagement/` | Entity 定義 |
| AccountingEnums.cs | `Models/Enums/` | AccountType、AccountDirection、AccountLevelFilter 列舉 |
| AccountItemSeeder.cs | `Data/SeedDataManager/Seeders/` | 553 筆種子資料 |
| AppDbContext.cs | `Data/Context/` | DbSet 與關聯設定 |
| SeedData.cs | `Data/` | Seeder 註冊 |
| IAccountItemService.cs | `Services/FinancialManagement/` | 服務介面 |
| AccountItemService.cs | `Services/FinancialManagement/` | 服務實作 |
| AccountItemFieldConfiguration.cs | `Components/FieldConfiguration/` | 欄位定義 |
| AccountItemIndex.razor | `Components/Pages/FinancialManagement/` | 列表頁面（/account-items） |
| AccountItemEditModalComponent.razor | `Components/Pages/FinancialManagement/` | 編輯表單 |
| QuickActionModalHost.razor | `Components/Shared/Dashboard/` | 首頁快速新增（NewAccountItem） |
| AccountItemListCriteria.cs | `Models/Reports/FilterCriteria/` | 報表篩選條件（FN005） |
| IAccountItemListReportService.cs | `Services/Reports/Interfaces/` | 報表服務介面 |
| AccountItemListReportService.cs | `Services/Reports/` | 報表服務實作 |
| JournalEntryEnums.cs | `Models/Enums/` | JournalEntryType、JournalEntryStatus 列舉 |
| JournalEntry.cs | `Data/Entities/FinancialManagement/` | 傳票主檔 Entity |
| JournalEntryLine.cs | `Data/Entities/FinancialManagement/` | 傳票分錄明細 Entity |
| IJournalEntryService.cs | `Services/FinancialManagement/` | 傳票服務介面 |
| JournalEntryService.cs | `Services/FinancialManagement/` | 傳票服務實作 |
| JournalEntryFieldConfiguration.cs | `Components/FieldConfiguration/` | 傳票欄位定義 |
| JournalEntryIndex.razor | `Components/Pages/FinancialManagement/` | 傳票列表頁面（/journal-entries） |
| JournalEntryEditModalComponent.razor | `Components/Pages/FinancialManagement/` | 傳票編輯 Modal（全客製，含分錄明細） |
| IJournalEntryAutoGenerationService.cs | `Services/FinancialManagement/` | 批次轉傳票服務介面（含沖款單） |
| JournalEntryAutoGenerationService.cs | `Services/FinancialManagement/` | 批次轉傳票服務實作（含稅三行分錄 + 沖款單） |
| JournalEntryBatchPage.razor | `Components/Pages/FinancialManagement/` | 批次轉傳票頁面（/journal-entry-batch，5 類單據） |
| SetoffDocument.cs | `Data/Entities/FinancialManagement/` | 沖款單 Entity（新增 IsJournalized + JournalizedAt） |
| TrialBalanceCriteria.cs | `Models/Reports/FilterCriteria/` | 試算表篩選條件（FN006） |
| IncomeStatementCriteria.cs | `Models/Reports/FilterCriteria/` | 損益表篩選條件（FN007） |
| BalanceSheetCriteria.cs | `Models/Reports/FilterCriteria/` | 資產負債表篩選條件（FN008） |
| ITrialBalanceReportService.cs | `Services/Reports/Interfaces/` | 試算表報表服務介面 |
| IIncomeStatementReportService.cs | `Services/Reports/Interfaces/` | 損益表報表服務介面 |
| IBalanceSheetReportService.cs | `Services/Reports/Interfaces/` | 資產負債表報表服務介面 |
| TrialBalanceReportService.cs | `Services/Reports/` | 試算表報表服務實作 |
| IncomeStatementReportService.cs | `Services/Reports/` | 損益表報表服務實作 |
| BalanceSheetReportService.cs | `Services/Reports/` | 資產負債表報表服務實作 |

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
