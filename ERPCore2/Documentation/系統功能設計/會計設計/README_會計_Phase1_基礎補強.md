# 會計模組 Phase 1：基礎補強

## 更新日期
2026-03-08

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

| 值 | 名稱 | 說明 |
|----|------|------|
| 1 | Open | 開放（可記帳） |
| 2 | Closing | 結帳處理中（暫時鎖定，避免並行） |
| 3 | Closed | 已關帳（不可新增/修改傳票） |
| 4 | Locked | 永久鎖定（年度結帳後） |

### Service 設計

**檔案：** `Services/FinancialManagement/IFiscalPeriodService.cs` / `FiscalPeriodService.cs`

| 方法 | 說明 |
|------|------|
| `GetCurrentOpenPeriodAsync()` | 取得目前開放期間 |
| `GetByYearAsync(year)` | 取得指定年度的所有期間 |
| `IsOpenAsync(year, period)` | 確認指定期間是否可記帳 |
| `CloseAsync(year, period)` | 執行關帳（Open → Closed） |
| `InitializeYearAsync(year)` | 初始化年度的 12 個期間 |

### 傳票過帳整合

`JournalEntryService.PostEntryAsync` 需新增：
```
1. 取得傳票的 FiscalYear + FiscalPeriod
2. FiscalPeriodService.IsOpenAsync(...)
   └─ false → 拋出 "會計期間 {Year}-{Period} 已關帳，不允許過帳"
```

### UI 設計

**檔案：** `Components/Pages/FinancialManagement/FiscalPeriodIndex.razor`
- 路由：`/fiscal-periods`
- 依年度顯示 12 個月期間卡片，顯示狀態 badge
- 操作：初始化年度 / 關帳（需確認）

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

**借貸平衡特殊規則：**
期初餘額傳票借貸**不一定平衡**（歷史資料可能有差異），
系統允許不平衡過帳，但顯示警告並記錄差額於「期初差額調整」科目。

### UI 設計

**檔案：** `Components/Pages/FinancialManagement/OpeningBalancePage.razor`
- 路由：`/opening-balance`
- 顯示「期初餘額設定精靈」
  - Step 1：輸入系統啟用日期
  - Step 2：依科目大類分組，輸入各明細科目期初餘額
  - Step 3：確認借貸合計，提交過帳
- 已設定後改為「唯讀檢視 + 重設按鈕（需管理員權限）」

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

### FN007 損益表（修正）

**現行問題：** 年度結帳後，下一年度損益表的「期初損益」不應包含上年度科目餘額。

**修正：** 損益表的日期範圍需對應**結帳後的期間**。
若結帳功能（Phase 3）完成後，上年度損益科目會被歸零（Closing Entries），則此問題自動解決。
**目前暫不修正**，等 Phase 3 結帳功能完成後一併處理。

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

- [ ] `FiscalPeriod` Entity + Migration 完成
- [ ] `FiscalPeriodService` 實作（含 IsOpenAsync）
- [ ] `PostEntryAsync` 加入期間開放檢查
- [ ] `FiscalPeriodIndex.razor` UI 可操作
- [ ] `JournalEntryType.OpeningBalance = 6` Enum 補充
- [ ] 期初餘額傳票可建立並過帳
- [ ] `OpeningBalancePage.razor` 精靈 UI 可操作
- [ ] FN006 試算表補充「期初餘額借/貸」兩欄
- [ ] 期初餘額後 FN008 資產負債表餘額正確

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_傳票系統.md](README_會計_傳票系統.md)
- [README_會計_財務報表.md](README_會計_財務報表.md)
- [README_會計_Phase2_業務整合.md](README_會計_Phase2_業務整合.md)