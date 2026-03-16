# 會計模組 Phase 1：基礎補強

## 更新日期
2026-03-14

## 優先等級
🔴 最高優先（基礎補強完成前，帳務資料不可靠）

---

## 概述

Phase 1 解決三個互相依存的根本缺口：**會計期間管理**、**期初餘額機制**、**財務報表邏輯修正**。
這三項必須按順序完成，缺少任一項都會導致報表數字不正確。

---

## P1-A：會計期間管理（FiscalPeriod Entity）

### 問題說明

目前傳票只有 `FiscalYear`（int）與 `FiscalPeriod`（int，1-12）兩個數字欄位，沒有對應的 Entity。導致：
- 無法鎖定已關帳的期間（任何時候都可對任意歷史月份補登傳票）
- 沒有「目前開放期間」概念，使用者不知道應記到哪一期
- 無法做跨年結帳的狀態管理

### Entity 設計

**檔案：** `Data/Entities/FinancialManagement/FiscalPeriod.cs`

| 欄位 | 型別 | 說明 |
|------|------|------|
| FiscalYear | int | 會計年度（西元） |
| PeriodNumber | int (1-12) | 期間月份 |
| StartDate | DateTime | 期間起始日 |
| EndDate | DateTime | 期間結束日 |
| Status | FiscalPeriodStatus | 期間狀態（見下） |
| ClosedAt | DateTime? | 關帳時間 |
| ClosedByEmployeeId | int? | 關帳人員 FK |
| CompanyId | int | 公司 FK |

**Enum：FiscalPeriodStatus**

| 值 | 名稱 | 說明 | 可重開 |
|----|------|------|--------|
| 1 | Open | 開放（可記帳） | — |
| 2 | Closing | 結帳處理中（暫時鎖定，避免並行） | — |
| 3 | Closed | 已關帳（不可新增/修改傳票） | ✓ 可重開（需 `Accounting.ClosePeriod` 權限） |
| 4 | Locked | 永久鎖定（年度結帳後） | ✗ 永久不可重開 |

**期間重開（Reopen）機制：**

`Closed` 狀態允許重開為 `Open`（補記漏帳等需求），需有 `Accounting.ClosePeriod` 權限。
重開時系統記錄重開原因及操作人員，並在 FiscalPeriodIndex UI 顯示「本期間已曾關閉」警示。
`Locked` 狀態（年度結帳後）永遠不可重開。

### FiscalPeriod 與 JournalEntry 關係決策

> **決策：不在 JournalEntry 上新增 FiscalPeriodId FK**

理由：
- JournalEntry 已有 `FiscalYear int` + `FiscalPeriod int`，維持整數方式符合 SAP/Oracle 業界慣例
- 加 FK 會造成「雞蛋問題」：期間不存在就無法建傳票，妨礙初始導入
- 現有資料不需複雜 Migration 更新
- 期間鎖定邏輯僅在 `PostEntryAsync` Service 層驗證即可

### FiscalPeriod 初始化策略（混合式）

| 情境 | 行為 |
|------|------|
| 當年度期間不存在 | 在公司設定精靈或年底結帳後**自動建立** 12 個期間（Open 狀態） |
| 過去期間不存在（補登歷史）| 自動建立（Open 狀態）並寫入 Warning Log，允許過帳 |
| 期間存在且 Status == Closed | **嚴格阻擋**，PostEntryAsync 拋出錯誤 |
| 期間存在且 Status == Locked | **嚴格阻擋**，任何傳票（含沖銷）均不可寫入 |
| 未來期間 | 允許建立但顯示提示「此期間尚未正式開始」 |

初始化觸發點：
1. 公司第一次使用會計功能 → 自動初始化當年度
2. P3-B 年底結帳 → 自動呼叫 `InitializeYearAsync(year+1)`
3. 管理員手動在 `FiscalPeriodIndex.razor` 操作

### Service 設計

**檔案：** `Services/FinancialManagement/IFiscalPeriodService.cs` / `FiscalPeriodService.cs`

| 方法 | 說明 |
|------|------|
| `GetCurrentOpenPeriodAsync()` | 取得目前開放期間 |
| `GetByYearAsync(year)` | 取得指定年度的所有期間 |
| `IsOpenAsync(year, period)` | 確認指定期間是否可記帳（不存在則自動建立） |
| `CloseAsync(year, period)` | 執行關帳（Open → Closed），需 `Accounting.ClosePeriod` 權限 |
| `ReopenAsync(year, period, reason)` | 重開期間（Closed → Open），需 `Accounting.ClosePeriod` 權限；記錄重開原因 |
| `InitializeYearAsync(year)` | 初始化年度的 12 個期間（若已存在則跳過） |

### 傳票過帳整合

`JournalEntryService.PostEntryAsync` 需新增：
```
1. 取得傳票的 FiscalYear + FiscalPeriod
2. FiscalPeriodService.IsOpenAsync(...)
   └─ false → 拋出 "會計期間 {Year}-{Period} 已關帳，不允許過帳"
```

### 沖銷傳票（Reversing Entry）期間驗證

`JournalEntryService.ReverseEntryAsync(entryId, reversalDate)` 需同步新增驗證：
```
1. 從 reversalDate 推算 reversalYear, reversalPeriod
2. FiscalPeriodService.IsOpenAsync(reversalYear, reversalPeriod)
   └─ false → 拋出 "沖銷日期所在期間已關帳，請選擇開放期間（建議下個月份）"
```

> **實務說明：** 沖銷傳票通常建議日期為「次月 1 日」，避免沖到當月已關帳期間。
> 系統應在 UI 上提示建議日期，但不強制（允許使用者選擇同月開放期間）。

### UI 設計

**檔案：** `Components/Pages/FinancialManagement/FiscalPeriodIndex.razor`（實際路徑應為 `Components/Pages/Accounting/FiscalPeriodIndex.razor`）
- 路由：`/fiscal-periods`
- 依年度顯示 12 個月期間卡片，顯示狀態 badge（Open / Closed / Locked）
- 操作：初始化年度 / 關帳 / 重開（均需 `Accounting.ClosePeriod` 權限）
- 已曾關閉後重開的期間顯示黃色警示 badge「已重開」

**權限設計（會計功能相關）：**

> 所有會計管理操作均使用 `Accounting.*` 權限，透過現有 Role+Permission 系統授予指定員工，**不綁定 SuperAdmin**。

| 權限 | 說明 | 建議授予對象 |
|------|------|------------|
| `Accounting.ClosePeriod` | 關帳 / 重開期間 | 財務主管 |
| `Accounting.OpeningBalance` | 建立期初餘額傳票 | 財務主管 |
| `Accounting.PostEntry` | 傳票過帳 | 財務人員 |
| `Accounting.YearEndClosing` | 執行年底結帳 | 財務主管 |

---

## P1-B：期初餘額機制（Opening Balance）

### 問題說明

系統若非從頭開始使用，歷史帳務無法帶入。所有報表的餘額均從第一筆傳票累計，
導致：
- 資產負債表缺少啟用前的應收帳款、庫存、應付帳款等餘額
- 試算表餘額從零開始，不能反映歷史帳務
- 新公司導入系統無法正確建帳

### 設計方案：期初餘額傳票（推薦）

採用**特殊傳票類型**而非另建 Entity，原因：
- 沿用現有 JournalEntry 架構，報表查詢邏輯不需大改
- 期初餘額本質上就是一批貸方/借方分錄
- `EntryType` 新增 `OpeningBalance = 6`

**Enum 補充（JournalEntryType）：**

| 值 | 名稱 | 說明 |
|----|------|------|
| 6 | OpeningBalance | 期初餘額建立傳票（每公司一般只有一筆） |

**期初餘額傳票規則：**
- 每家公司只允許一筆 `OpeningBalance` 傳票
- `EntryDate` = 系統啟用前一天（例如 2024-12-31）
- 建立後自動過帳，不可沖銷
- 建立/編輯需要特別權限 `JournalEntry.OpeningBalance`
- 科目可選所有明細科目（Level 4 + 子科目）

**借貸平衡規則（決策：強制平衡）：**

> 期初餘額傳票**必須借貸平衡才可過帳**，不允許不平衡的期初餘額。

理由：
- 若資產 + 費用 ≠ 負債 + 收入 + 權益，代表輸入有誤，而非歷史資料的真實差異
- 允許不平衡過帳會造成試算表永久無法平衡，所有後續報表數字均不可信
- 正確的歷史帳務在任何一個時間點都應該符合會計恆等式

修正流程：
- 系統顯示「借方合計：$X / 貸方合計：$Y / 差額：$Z」
- 使用者需補齊差額（如：將差額記入「保留盈餘」或「業主往來」科目）
- 完全平衡後方可過帳

若已過帳後發現輸入錯誤：
- **草稿狀態（Draft）：** 可直接編輯
- **已過帳（Posted）：** 建立**調整分錄**（Adjusting Entry）修正差額，不得重設或刪除

### UI 設計

**檔案：** `Components/Pages/FinancialManagement/OpeningBalancePage.razor`
- 路由：`/opening-balance`
- 顯示「期初餘額設定精靈」
  - Step 1：輸入系統啟用日期（設定前一天為傳票日期，例如 2024-12-31）
  - Step 2：依科目大類分組，輸入各明細科目期初餘額
  - Step 3：確認借貸合計（**差額不為零則拒絕提交，顯示錯誤提示**）
  - Step 4：確認並過帳
- 已過帳後改為「唯讀檢視」
- 若需修正，應引導使用者建立**調整分錄**（連結至傳票建立頁面）

> **移除「重設按鈕」設計**：重設會破壞稽核軌跡，改以調整分錄方式修正。

---

## P1-C：財務報表邏輯修正

加入期初餘額機制後，以下報表的查詢邏輯需要調整：

### FN006 試算表（修正）

**現行問題：** 「期末餘額」用 `EntryDate <= EndDate` 全域累計，
若啟用前有期初餘額傳票（`EntryDate` = 啟用前一天），會自動被納入計算，**這是正確的**。
但「期初餘額」欄位（StartDate 前的累計）目前是「本期前的全部」，
需確認是否顯示「期初餘額傳票」的影響，通常應納入。

**修正方向：** 目前邏輯在加入 `OpeningBalance` 傳票後**可自動相容**，
但需驗證顯示格式：
- 應增加「**期初餘額**」欄：`EntryDate < StartDate` 的餘額
- 欄位：科目代碼 | 科目名稱 | **期初餘額（借）** | **期初餘額（貸）** | 本期借方 | 本期貸方 | 期末借方餘額 | 期末貸方餘額

> 目前 FN006 沒有「期初餘額」欄，建議補充，改為**標準試算表格式（六欄）**。

### FN007 損益表 / FN008 資產負債表（修正）

**現行問題（根源相同）：**
- FN007 損益表：跨年度損益科目不歸零，下年度報表包含歷史累計
- FN008 資產負債表：「本期損益（3351）」欄位顯示所有年度累計數字，非當年度淨損益

**修正方向：** 等 Phase 3-B 年底結帳完成後，Closing Entry 會將損益科目歸零，此問題自動解決。

> **⚠ 注意：** 在 P3-B 完成前，FN007 與 FN008 的損益數字均**不可靠**，應告知使用者這是已知限制。

### FN009 總分類帳 / FN011 明細科目餘額表（修正）

**修正：** 期初餘額的計算需包含 `OpeningBalance` 類型傳票，
目前查詢邏輯為 `JournalEntryStatus == Posted` 且 `EntryDate < StartDate`，
只要期初餘額傳票的 `EntryDate` 設定為系統啟用前一天，**可自動相容**。

**建議驗證：** 加入期初餘額後，執行以下驗算：
- `FN008 資產合計` = `FN011 資產類期末餘額合計`
- `FN006 借方合計` = `FN006 貸方合計`（試算平衡）

---

## Migration 計畫

| Migration | 說明 |
|-----------|------|
| `AddFiscalPeriodTable` | 新增 `FiscalPeriods` 表（所有欄位）+ 索引（CompanyId + Year + Period 唯一） |
| `AddOpeningBalanceJournalType` | `JournalEntryType` Enum 補充 value 6（僅文件說明，Enum 不需 migration） |

---

## 完成標準（Definition of Done）

### P1-A 會計期間管理
- [ ] `FiscalPeriod` Entity + Migration 完成
- [ ] `FiscalPeriodService` 實作（含 IsOpenAsync、混合式初始化邏輯）
- [ ] `PostEntryAsync` 加入期間開放檢查（不存在則自動建立 Open 期間）
- [ ] `ReverseEntryAsync` 加入 reversalDate 對應期間的開放檢查
- [ ] `FiscalPeriodIndex.razor` UI 可操作（初始化年度 / 關帳）

### P1-B 期初餘額機制
- [ ] `JournalEntryType.OpeningBalance = 6` Enum 補充
- [ ] 期初餘額傳票強制借貸平衡後才可過帳
- [ ] `OpeningBalancePage.razor` 精靈 UI（Step 1-4）可操作
- [ ] 已過帳後顯示唯讀 + 提示建立調整分錄（不提供重設按鈕）

### P1-C 報表修正
- [ ] FN006 試算表補充「期初餘額借/貸」兩欄（標準六欄格式）
- [ ] 期初餘額後 FN008 資產負債表餘額正確
- [ ] 確認 FN007/FN008 損益科目跨年問題的 UI 提示文字

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_傳票系統.md](README_會計_傳票系統.md)
- [README_會計_財務報表.md](README_會計_財務報表.md)
- [README_會計_Phase2_業務整合.md](README_會計_Phase2_業務整合.md)