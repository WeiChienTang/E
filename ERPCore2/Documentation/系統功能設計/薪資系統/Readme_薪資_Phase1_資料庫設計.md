# 薪資系統 Phase 1 — 資料庫設計

> 關聯文件：[薪資系統設計總綱](./Readme_薪資系統設計總綱.md)
> 狀態：✅ 已完成（2026-03-09）

---

## 一、Enum vs 資料表判斷準則

| 條件 | 使用方式 |
|------|---------|
| 選項固定、由系統邏輯決定、不會因使用者增加 | **Enum** |
| 選項由法規定義且全球通用 | **Enum** |
| 資料筆數不固定，隨使用者操作增加 | **資料表** |
| 每年更新（如費率），需保留歷史版本 | **資料表 + EffectiveDate 欄位** |
| 可由使用者自訂名稱或新增 | **資料表** |

---

## 二、Enum 清單

定義於 `Models/Enums/PayrollEnums.cs`：

| Enum | 值 | 理由 |
|------|-----|------|
| `PayrollPeriodStatus` | Draft/Processing/Closed | 三個狀態由系統工作流程控制 |
| `PayrollItemType` | Income/Deduction | 只有收入與扣除兩種，固定 |
| `PayrollItemCategory` | Salary/Allowance/Overtime/Bonus/Legal/Other | 分類由系統定義，對應計算邏輯 |
| `SalaryType` | Monthly/Hourly | 台灣勞基法規定範疇 |
| `TaxWithholdingType` | Standard/Resident/NonResident | 財政部所得稅法定義 |
| `PayrollRecordStatus` | Draft/Confirmed | 由系統計算流程控制 |

---

## 三、資料表清單

| 資料表 | 檔案 | 原因 |
|--------|------|------|
| `PayrollPeriod` | `Data/Entities/Payroll/PayrollPeriod.cs` | 每月一筆，持續成長 |
| `PayrollItem` | `Data/Entities/Payroll/PayrollItem.cs` | 使用者可新增自訂薪資項目 |
| `EmployeeSalary` | `Data/Entities/Payroll/EmployeeSalary.cs` | 每位員工多筆（含歷史），調薪時新增記錄 |
| `PayrollRecord` | `Data/Entities/Payroll/PayrollRecord.cs` | 每人每月一筆，持續成長 |
| `PayrollRecordDetail` | `Data/Entities/Payroll/PayrollRecordDetail.cs` | 每筆 Record 有多筆明細 |
| `EmployeeBankAccount` | `Data/Entities/Payroll/EmployeeBankAccount.cs` | 員工可設定多個帳號 |
| `MinimumWage` | `Data/Entities/Payroll/MinimumWage.cs` | 每年更新，需保留歷史 |
| `LaborInsuranceGrade` | `Data/Entities/Payroll/LaborInsuranceGrade.cs` | 34個等級，每年可能調整 |
| `HealthInsuranceGrade` | `Data/Entities/Payroll/HealthInsuranceGrade.cs` | 分級表，每年可能調整 |
| `WithholdingTaxTable` | `Data/Entities/Payroll/WithholdingTaxTable.cs` | 多行多列，每年財政部重新公告 |

---

## 四、BaseEntity 使用說明

繼承 `BaseEntity` 的實體自動具備 `Id`、`Code`、`Status`（EntityStatus）、`CreatedAt`、`UpdatedAt`、`CreatedBy`、`UpdatedBy`、`Remarks` 欄位。

### 特殊命名決策

| 實體 | 問題 | 解法 |
|------|------|------|
| `PayrollPeriod` | `BaseEntity.Status`（EntityStatus）≠ 週期流程狀態 | 另設 `PeriodStatus`（`PayrollPeriodStatus`）欄位 |
| `PayrollItem` | `BaseEntity.Code` = 項目代碼；`BaseEntity.Status` = 啟用/停用 | 直接使用，不重複定義 `IsEnabled` 或 `Code` |
| `PayrollRecord` | `BaseEntity.Status`（EntityStatus）≠ 薪資單流程狀態 | 另設 `RecordStatus`（`PayrollRecordStatus`）欄位；`Remarks` 已由 BaseEntity 提供 |
| 費率參照表（MinimumWage 等） | 不需要業務欄位（CreatedBy/Code 等） | **不繼承** BaseEntity |
| `PayrollRecordDetail` | 子明細表，無獨立業務語意 | **不繼承** BaseEntity（與 SalesOrderDetail 等慣例一致）|

---

## 五、唯一索引

| 資料表 | 索引欄位 | 說明 |
|--------|---------|------|
| `PayrollPeriod` | `(Year, Month)` | 同年同月只能有一個週期 |
| `PayrollItem` | `Code` | 薪資項目代碼唯一 |
| `PayrollRecord` | `(PayrollPeriodId, EmployeeId)` | 同週期同員工只能有一筆薪資單 |

---

## 六、費率資料說明

費率參照表僅供查詢，不關聯到員工實體，均透過 `EffectiveDate` 管理年度版本。

| 資料表 | 查詢方式 |
|--------|---------|
| `MinimumWage` | 取 `EffectiveDate ≤ 目標日期` 的最新一筆 |
| `LaborInsuranceGrade` | 取 `EffectiveDate ≤ 目標日期` + `SalaryFrom ≤ 薪資 ≤ SalaryTo` |
| `HealthInsuranceGrade` | 同上 |
| `WithholdingTaxTable` | 取 `EffectiveDate ≤ 目標日期` + `薪資區間` + `DependentCount` |

---

## 七、種子資料

| 資料表 | 種子檔案 | 內容 |
|--------|---------|------|
| `PayrollItem` | `Data/SeedDataManager/Seeders/PayrollItemSeeder.cs` | 16 個系統預設薪資項目（BASE、MEAL、LI_EE 等） |
| `MinimumWage` | 同上或 Migration | 2025年（民國114年）月薪 28,590 / 時薪 190 |
| `LaborInsuranceGrade` | 同上或 Migration | 114年度 34 個等級 |
| `HealthInsuranceGrade` | 同上或 Migration | 114年度分級表 |
| `WithholdingTaxTable` | 同上或 Migration | 114年度財政部扣繳表 |

---

## 八、SystemParameter 薪資欄位

全公司統一、無歷史版本、不隨員工增加的常數，存入 `SystemParameter`。

| 欄位 | 說明 | 預設 |
|------|------|------|
| `PayrollPayDay` | 每月發薪日（1-31） | 5 |
| `PayrollCutoffDay` | 薪資結算截止日 | 25 |
| `SalaryMonthDivisor` | 月薪計算除數（0 = 依當月實際天數） | 30 |
| `OvertimeRoundingUnit` | 加班計時單位（分鐘） | 30 |
| `LateTolerance` | 遲到寬限分鐘數 | 0 |
| `Payroll*AccountCode` × 10 | 薪資相關會計科目（Phase 4 使用） | null |

**不放入 SystemParameter 的項目：**

| 項目 | 正確位置 |
|------|---------|
| 基本工資金額 | `MinimumWage` 資料表 |
| 勞健保費率 | `PayrollCalculationService` 常數（Phase 2 改為 DB） |
| 勞健保投保分級表 | `LaborInsuranceGrade` / `HealthInsuranceGrade` |
| 扣繳稅額表 | `WithholdingTaxTable` |
| 員工薪資設定 | `EmployeeSalary` 資料表 |

---

## 九、實體關係圖

```
Employee
    ├─ EmployeeSalary (1:N)         一位員工多筆薪資設定（歷史）
    ├─ EmployeeBankAccount (1:N)    一位員工多個帳號
    └─ PayrollRecord (1:N)          一位員工多筆薪資單（每月一筆）
             ├─ PayrollPeriod (N:1) 每筆薪資單屬於某個月份週期
             └─ PayrollRecordDetail (1:N)
                       └─ PayrollItem (N:1)

費率參照表（獨立）：
    MinimumWage、LaborInsuranceGrade、HealthInsuranceGrade、WithholdingTaxTable
```
