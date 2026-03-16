# 薪資系統改進項目

> 文件版本：v1.0（2026-03-16）
> 依據：與開發者討論 UX 設計缺口後整理

---

## 問題一：薪資項目（PayrollItem）與計算引擎脫節

### 現況描述

`/payroll/items` 薪資項目維護頁面目前實際用途極為有限：

- 計算引擎（`PayrollCalculationService.DoCalculateAsync`）只使用 PayrollItem 的 **Code** 作為標籤對應，硬寫 13 個固定 Code（BASE、MEAL、TRANSPORT、LI_EE 等）
- `IsTaxable`、`IsInsuranceBasis`、`IsRetirementBasis` 三個屬性**存在於資料表但計算引擎完全未讀取**，稅務規則全部 hardcode
- 使用者在維護頁面新增的任何自訂項目，計算引擎**永遠不會使用**
- 停用系統項目反而會導致計算結果缺少明細，是危險操作

### 根本原因

`EmployeeSalaryEditModalComponent` 裡的加給、津貼欄位（`PositionAllowance`、`MealAllowance`、`TransportAllowance`）是**寫死的資料庫欄位**，無法讓使用者自訂新的固定薪資項目類型。PayrollItem 目錄原本應作為「可選的項目庫」，但實作時計算引擎繞過了它。

### 改進方向

**將 EmployeeSalary 的固定津貼欄位改為動態 PayrollItem 關聯**，讓使用者可從 PayrollItem 目錄選取項目並填入金額，取代現有 hardcode 欄位。

#### 設計草案

```
PayrollItem（管理員維護）
  → 定義項目名稱、類別、IsTaxable、IsInsuranceBasis、免稅上限

EmployeeSalary（人事設定）
  ├─ BaseSalary（保留，有特殊出勤折算邏輯）
  ├─ 勞健保相關欄位（保留，有法規對應）
  └─ 固定月付項目（新增，動態）：
       EmployeeSalaryItem 關聯資料表
         ├─ PayrollItemId（FK → PayrollItem）
         └─ Amount（金額）

計算引擎
  → 讀取 EmployeeSalaryItems 動態組合計算
  → 依 PayrollItem.IsTaxable 決定是否計入課稅所得
  → 依 PayrollItem.IsInsuranceBasis 決定是否計入投保基礎
```

#### 需要修改的範圍

| 項目 | 說明 |
|------|------|
| 新增資料表 | `EmployeeSalaryItem`（EmployeeSalaryId + PayrollItemId + Amount）|
| 修改 EmployeeSalary | 移除 `PositionAllowance`、`MealAllowance`、`TransportAllowance` 欄位，改由關聯表取代 |
| 修改 Migration | 新增 EmployeeSalaryItem 資料表，移除舊欄位 |
| 修改 EmployeeSalaryEditModalComponent | 加入動態項目選擇 UI（從 PayrollItem 目錄選 + 填金額）|
| 修改 PayrollCalculationService | 改為讀取 EmployeeSalaryItems 動態計算，並依 PayrollItem 屬性決定稅務行為 |
| 修改 PayrollItem 維護頁面 | 標示系統項目不可刪除，補充說明欄位用途 |

#### 待討論問題

- `BaseSalary` 是否也納入動態項目，或繼續保留為獨立欄位（建議保留，因為出勤折算邏輯特殊）
- 現有已計算的 PayrollRecord 是否需要 Migration 補正

### 優先級

中（不影響現有計算正確性，但影響系統可擴充性）

---

## 問題二：薪資項目維護頁面缺乏使用說明

### 現況描述

使用者進入 `/payroll/items` 時，無法判斷：
- 哪些項目是系統內建不可亂動的
- 新增項目的實際效果是什麼
- `IsTaxable` 等屬性目前是否有作用

### 改進方向

在頁面加入提示說明，明確標示：
- 系統項目（`IsSystemItem = true`）顯示鎖定 badge，禁止刪除
- 頁面說明文字：「新增自訂項目後，可在員工薪資設定中選用為固定月付項目」（待問題一完成後才成立）
- `IsTaxable` 等屬性在問題一完成前標示為「規劃中，尚未生效」

### 優先級

低（待問題一完成後一併處理）

---

## 問題三：出勤彙總初始化預設全勤，無國定假日處理

### 現況描述

`GetWorkDaysInMonth()` 只排除週六日，不扣除國定假日。批次初始化後的應出勤天數可能偏高。

### 改進方向

Phase 4 或 Phase 5 視需求評估是否引入國定假日行事曆資料表。

### 優先級

低（現階段人工調整出勤記錄可補正）

---

## Bug-P1：ConfirmRecordAsync — 允許確認未計算的薪資單 ✅ 已修正（2026-03-16）

**影響：** `Services/Payroll/PayrollCalculationService.cs` → `ConfirmRecordAsync`

**現行問題：** `ConfirmRecordAsync` 只檢查 `period.PeriodStatus != Closed` 與 `record.RecordStatus != Confirmed`，未驗證薪資單是否已經過計算引擎執行（`CalculatedAt.HasValue`）。UI 在 `RecordStatus == Draft` 時就顯示「確認」按鈕，無論 `CalculatedAt` 是否有值。使用者可對一張 `CalculatedAt = null`（全部金額為 0）的空白薪資單點確認，產生零薪資確認記錄。

**修正：** 在 `record.RecordStatus == Confirmed` 檢查後加入：
```csharp
if (!record.CalculatedAt.HasValue)
    return ServiceResult.Failure("薪資單尚未計算，無法確認");
```

---

## Bug-P2：AddWithExpiryAsync — 未驗證新薪資生效日晚於現有薪資生效日 ✅ 已修正（2026-03-16）

**影響：** `Services/Payroll/EmployeeSalaryService.cs` → `AddWithExpiryAsync`

**現行問題：** 方法將現有 `ExpiryDate == null` 的薪資記錄的 `ExpiryDate` 設為 `newSalary.EffectiveDate.AddDays(-1)`。若 `newSalary.EffectiveDate <= existing.EffectiveDate`（使用者回填更早的生效日），會產生 `ExpiryDate < EffectiveDate` 的無效記錄（例如原 EffectiveDate = 2026-01-01，新 EffectiveDate = 2025-06-01 → 舊記錄 ExpiryDate = 2025-05-31 < EffectiveDate 2026-01-01），造成薪資覆蓋期間中斷，部分月份找不到有效薪資設定，`CalculateEmployeeAsync` 返回「找不到員工的有效薪資設定」錯誤。

**修正：** 設置 ExpiryDate 前先驗證新生效日必須晚於現有薪資的生效日：
```csharp
foreach (var existing in current)
{
    if (newSalary.EffectiveDate <= existing.EffectiveDate)
        return ServiceResult.Failure($"新薪資生效日期（{newSalary.EffectiveDate:yyyy-MM-dd}）必須晚於目前有效薪資的生效日期（{existing.EffectiveDate:yyyy-MM-dd}）");
}
```

---

## Bug-P3：RecalculateAsync — 強制重置 RecordStatus 未持久化至 DB ✅ 已修正（2026-03-16）

**影響：** `Services/Payroll/PayrollCalculationService.cs` → `RecalculateAsync`

**現行問題：** `RecalculateAsync` 在第 201 行將 `record.RecordStatus = PayrollRecordStatus.Draft`，但緊接著在第 203 行呼叫 `CalculateEmployeeAsync`，兩者使用**不同 DbContext**（`CalculateEmployeeAsync` 在第 47 行開啟新 context）。由於沒有在重置後呼叫 `SaveChangesAsync()`，新 context 從 DB 讀回的 `RecordStatus` 仍為 `Confirmed`，導致第 85 行的確認狀態保護觸發，`CalculateEmployeeAsync` 返回「薪資單已確認，請先取消確認再重算」錯誤。結果：`RecalculateAsync` 對已確認薪資單永遠失敗，強制重置完全無效。

**修正：** 在重置狀態後加入 `await context.SaveChangesAsync()`：
```csharp
record.RecordStatus = PayrollRecordStatus.Draft;
await context.SaveChangesAsync(); // 先持久化至 DB，CalculateEmployeeAsync 才能讀到 Draft 狀態
return await CalculateEmployeeAsync(record.EmployeeId, record.PayrollPeriodId, calculatedBy);
```
