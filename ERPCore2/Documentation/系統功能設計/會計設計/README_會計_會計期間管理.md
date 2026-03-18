# 會計期間管理設計（FiscalPeriod）

## 更新日期
2026-03-17

---

## 概述

會計期間管理（FiscalPeriod）控制傳票過帳的有效期間。每個公司每年有 12 個月份期間，每個期間可處於 Open（可過帳）、Closed（已關帳，可重開）、Locked（永久鎖定）三種狀態。傳票過帳與沖銷均需驗證目標期間是否開放。

---

## 1. Enum 定義

**檔案：** `Models/Enums/AccountingEnums.cs`

### FiscalPeriodStatus（期間狀態）

| 值 | 名稱 | 說明 |
|----|------|------|
| 1 | Open | 開放中，可過帳傳票 |
| 2 | Closed | 已關帳，不可過帳，可重開（需 Accounting.ClosePeriod 權限） |
| 3 | Locked | 永久鎖定，不可過帳，不可重開 |

---

## 2. FiscalPeriod Entity

**檔案：** `Data/Entities/FinancialManagement/FiscalPeriod.cs`

**主要欄位：**

| 欄位 | 型別 | 說明 |
|------|------|------|
| FiscalYear | int | 會計年度（1900–2100） |
| PeriodNumber | int | 期間編號（1–12，對應月份） |
| StartDate | DateTime | 期間起始日 |
| EndDate | DateTime | 期間截止日 |
| PeriodStatus | FiscalPeriodStatus | 期間狀態（Open / Closed / Locked） |
| CompanyId | int | 公司 FK |
| ClosedAt | DateTime? | 關帳時間 |
| ClosedByEmployeeId | int? | 關帳人員 FK |
| ReopenReason | string? | 重開原因（最長 500 字元） |
| ReopenedAt | DateTime? | 重開時間（有值表示此期間曾被重開過） |

**計算屬性（NotMapped）：**

| 屬性 | 說明 |
|------|------|
| DisplayName | `{FiscalYear}/{PeriodNumber:D2}`（例：`2026/01`） |
| IsOpen | `PeriodStatus == FiscalPeriodStatus.Open` |
| WasReopened | `ReopenedAt.HasValue`（用於稽核警示） |

**資料庫索引：** 複合唯一索引 `UX_FiscalPeriod_Year_Period_Company`（FiscalYear + PeriodNumber + CompanyId），確保同一公司同年度同月份不重複。

**設計決策：** FiscalPeriod 不對 JournalEntry 設 FK。JournalEntry 維持 `FiscalYear int` + `FiscalPeriod int` 欄位，期間驗證在 Service 層完成（符合 SAP/Oracle 業界慣例，避免雞蛋問題與 Migration 負擔）。

---

## 3. DbContext 設定

**檔案：** `Data/Context/AppDbContext.cs`

```csharp
public DbSet<FiscalPeriod> FiscalPeriods { get; set; }
```

---

## 4. FiscalPeriodService

**檔案：** `Services/FinancialManagement/IFiscalPeriodService.cs` / `FiscalPeriodService.cs`

| 方法 | 說明 |
|------|------|
| `GetByYearAsync(year, companyId)` | 取得指定年度所有 12 個期間 |
| `GetByYearAndPeriodAsync(year, periodNumber, companyId)` | 取得指定年度+月份的單一期間 |
| `GetCurrentPeriodAsync(companyId)` | 取得今日日期所在的 Open 期間 |
| `GetOpenPeriodsAsync(companyId)` | 取得所有 Open 狀態的期間 |
| `IsDateInOpenPeriodAsync(date, companyId)` | 判斷日期是否在某個 Open 期間內 |
| `IsYearPeriodExistsAsync(year, periodNumber, companyId, excludeId)` | 重複期間驗證（供 EditModal 使用） |
| `ClosePeriodAsync(id, closedByEmployeeId)` | 關帳：Open → Closed，記錄關帳時間與人員 |
| `LockPeriodAsync(id)` | 永久鎖定：Closed → Locked |
| `ReopenPeriodAsync(id, reason)` | 重開：Closed → Open，記錄重開原因與時間 |
| `InitializeYearAsync(year, companyId)` | 一鍵初始化指定年度 12 個月份（已存在的跳過） |
| `GetPagedWithFiltersAsync(...)` | 伺服器端分頁查詢 |

---

## 5. 期間生命週期（狀態機）

```
         InitializeYearAsync
               │
               ▼
          ┌─────────┐
          │  Open   │◄──── ReopenPeriodAsync（需填重開原因；Locked 不可重開）
          └────┬────┘
               │ ClosePeriodAsync
               ▼
          ┌─────────┐
          │ Closed  │──── LockPeriodAsync ──▶ ┌─────────┐
          └─────────┘                          │ Locked  │（永久，無法回頭）
                                               └─────────┘
```

**狀態轉換規則：**

| 動作 | 來源狀態 | 目標狀態 | 備註 |
|------|---------|---------|------|
| ClosePeriodAsync | Open | Closed | 記錄 ClosedAt / ClosedByEmployeeId |
| LockPeriodAsync | Closed | Locked | 永久鎖定，不可逆 |
| ReopenPeriodAsync | Closed | Open | 記錄 ReopenReason / ReopenedAt；Locked 狀態拒絕 |
| — | Locked | — | 所有操作均拒絕 |

所有狀態變更均需 `Accounting.ClosePeriod` 權限。

---

## 6. 傳票過帳期間驗證

傳票相關操作透過 `JournalEntryService` 呼叫 `FiscalPeriodService` 進行驗證。

### PostEntryAsync 期間驗證

**檔案：** `Services/FinancialManagement/JournalEntryService.cs`

- **期初餘額傳票（OpeningBalance）**：跳過期間鎖定驗證，允許在任何期間過帳。
- **其他所有傳票類型**：驗證 `entry.FiscalYear` + `entry.FiscalPeriod` 所對應期間狀態。

```
若期間 Locked → 拒絕：「會計期間 {year}/{period:D2} 已永久鎖定，不允許過帳」
若期間 Closed → 拒絕：「會計期間 {year}/{period:D2} 已關帳，如需補登請先重開期間」
若期間不存在 → 允許過帳（自動 fallback，記錄警告）
```

### ReverseEntryAsync 期間驗證

沖銷傳票的目標期間（沖銷日期所在月份）同樣需要驗證：

```
若期間 Locked → 拒絕：「沖銷日期所在期間已永久鎖定，請選擇開放中的期間（建議：次月 1 日）」
若期間 Closed → 拒絕：「沖銷日期所在期間已關帳，請選擇開放中的期間（建議：次月 1 日）」
```

---

## 7. UI 元件

### FiscalPeriodIndex 頁面

**檔案：** `Components/Pages/Accounting/FiscalPeriodIndex.razor`

- 路由：`/fiscal-periods`
- 標準 GenericIndexPageComponent 架構
- 欄位：年度、期間編號、起訖日期、狀態（badge）、關帳時間

### FiscalPeriodEditModal

**檔案：** `Components/Pages/Accounting/FiscalPeriodEditModalComponent.razor`

- 標準 GenericEditModalComponent 架構
- 編輯欄位：FiscalYear、PeriodNumber、StartDate、EndDate、PeriodStatus

### FieldConfiguration

**檔案：** `Components/FieldConfiguration/FiscalPeriodFieldConfiguration.cs`

---

## 8. 導覽與權限

**導覽設定（NavigationConfig.cs）：**

```csharp
new NavigationItem
{
    Name = "會計期間管理",
    NameKey = "Nav.FiscalPeriods",
    Route = "/fiscal-periods",
    RequiredPermission = PermissionRegistry.FiscalPeriod.Read,
    SearchKeywords = new[] { "會計期間", "關帳", "鎖定期間", "fiscal period" }
}
```

**權限定義（PermissionRegistry.cs）：**

| 權限 | 說明 | 等級 |
|------|------|------|
| `FiscalPeriod.Read` | 檢視會計期間 | Normal |
| `Accounting.ClosePeriod` | 關帳／重開期間（所有狀態變更） | Sensitive |
| `Accounting.PostEntry` | 傳票過帳（被 PostEntryAsync 驗證機制依賴） | Sensitive |
| `Accounting.OpeningBalance` | 期初餘額設定 | Sensitive |

---

## 9. Migration

| Migration | 日期 | 說明 |
|-----------|------|------|
| `20260317113430_CreateFiscalPeriodTable` | 2026-03-17 | 建立 FiscalPeriods 資料表，含基本欄位與唯一索引 |
| `20260317120711_AddFiscalPeriodAuditFields` | 2026-03-17 | 新增 ClosedByEmployeeId、ReopenReason、ReopenedAt 稽核欄位 |

---

## 相關文件

- [README_會計設計總綱.md](README_會計設計總綱.md)
- [README_會計_傳票系統.md](README_會計_傳票系統.md)（PostEntryAsync / ReverseEntryAsync 詳細流程，以及期初餘額機制）
- [README_會計_Phase1_基礎補強.md](README_會計_Phase1_基礎補強.md)（已完成，歷史設計記錄）
