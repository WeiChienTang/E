# 薪資系統改進項目

> 文件版本：v1.0（2026-03-16）
> 依據：與開發者討論 UX 設計缺口後整理

---

## 問題一：薪資項目（PayrollItem）與計算引擎脫節 ✅ 已完成

> 完成時間：2026-03-19

### 完成內容

- `EmployeeSalaryItem` 關聯資料表已建立（`EmployeeSalaryId + PayrollItemId + Amount`）
- `EmployeeSalary` 已移除硬寫的 `PositionAllowance`、`MealAllowance`、`TransportAllowance` 欄位，改由 `AllowanceItems` collection 取代
- `EmployeeSalaryEditModalComponent` 已加入動態津貼項目 UI（從 PayrollItemCategory.Allowance 選取項目 + 填金額）
- `PayrollCalculationService` 已改為讀取 `salary.AllowanceItems` 動態組合計算
- `PayrollCalculationService` 課稅所得計算已改用 `PayrollItem.IsTaxable` 旗標（MEAL/TRANSPORT 仍套用法定免稅上限）

### 目前架構

```
PayrollItem（管理員維護）
  → 定義項目名稱、類別、IsTaxable、IsInsuranceBasis、免稅上限

EmployeeSalary（人事設定）
  ├─ BaseSalary（保留，有特殊出勤折算邏輯）
  ├─ 勞健保相關欄位（保留，有法規對應）
  └─ AllowanceItems：List<EmployeeSalaryItem>（動態關聯）
       ├─ PayrollItemId（FK → PayrollItem.Category = Allowance）
       └─ Amount（每月固定金額）

計算引擎
  → 讀取 AllowanceItems 動態組合計算
  → 依 IsTaxable=false 決定免稅額（MEAL/TRANSPORT 有法定上限）
```

### 未完成：IsInsuranceBasis / IsRetirementBasis 尚未接入

`PayrollItem.IsInsuranceBasis` 和 `IsRetirementBasis` 兩個旗標目前尚未用於計算勞健保投保基礎。目前計算邏輯仍直接使用 `EmployeeSalary.LaborInsuredSalary` 和 `HealthInsuredAmount` 作為投保薪資，這符合台灣法規（投保薪資依分級表選取，非薪資項目加總）。兩個旗標暫保留供未來申報用途。

### 優先級

✅ 主體已完成，IsInsuranceBasis/IsRetirementBasis 接入屬低優先度

---

## 問題二：薪資項目維護頁面缺乏使用說明

### 現況描述

使用者進入 `/payroll/items` 時，無法判斷：
- 哪些項目是系統內建不可亂動的
- 新增項目的實際效果是什麼

### 改進方向

在頁面加入提示說明（問題一已完成，此問題現可處理）：
- 系統項目（`IsSystemItem = true`）顯示鎖定 badge，禁止刪除或修改 Code
- 頁面說明文字：「自訂類別 = Allowance 的項目可在員工薪資設定中選用為固定月付項目；設定 IsTaxable=false 可使該項目計入課稅所得時的免稅額」
- `IsInsuranceBasis`、`IsRetirementBasis` 說明：「保留欄位，目前不影響計算，供未來申報用途」

### 優先級

低

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
