# 薪資系統設計總綱

> 適用地區：中華民國台灣
> 文件版本：v1.1（2026-03-09）
> ModuleKey：`Payroll`

---

## 目錄

1. [系統概觀](#一系統概觀)
2. [架構圖](#二架構圖)
3. [開發階段狀態](#三開發階段狀態)
4. [檔案索引](#四檔案索引)
5. [法規遵循摘要](#五法規遵循摘要)
6. [權限設計](#六權限設計)

---

## 一、系統概觀

薪資系統負責計算員工每月應發薪資，涵蓋收入項目、法定扣繳、社保申報及政府報表輸出。

### 核心原則

- **法規優先**：所有計算規則以勞動基準法、全民健康保險法、勞工保險條例為依據
- **費率可維護**：基本工資、投保薪資分級表、健保費率、所得稅扣繳表均存於 DB（Phase 1 暫以常數替代，Phase 2 完整移入）
- **月結鎖定**：薪資關帳後不可回溯修改
- **出勤連動**：計算依賴出勤模組提供實際出勤天數、加班時數（Phase 3）
- **會計連動**：關帳後自動產生會計傳票（Phase 4）

---

## 二、架構圖

```
薪資管理（NavMenu / ModuleKey = Payroll）
    │
    ├─ 薪資計算作業（/payroll）
    │       └─ PayslipModalComponent         唯讀薪資單 Modal
    │
    ├─ 員工薪資設定（/employee-salaries）    含薪資歷史 Tab
    │
    ├─ 員工銀行帳戶（/employee-bank-accounts）
    │
    ├─ 薪資項目維護（/payroll-items）
    │
    └─ 薪資週期管理（/payroll-periods）      含開帳 / 關帳按鈕

員工 EditModal
    └─ EmployeePayrollTab                   薪資歷史唯讀 Tab（Payroll 模組啟用時顯示）
```

### 計算流程

```
CalculateEmployeeAsync(employeeId, periodId)
    │
    ├─ 1. 取得員工有效薪資設定（EmployeeSalary，EffectiveDate ≤ 薪資月份）
    ├─ 2. 計算應出勤天數（GetWorkDaysInMonth，排除週末）
    ├─ 3. 計算本薪（BaseSalary × 出勤比例）
    ├─ 4. 加計職務加給（全額，不依出勤比例）
    ├─ 5. 加計津貼（餐飲/交通，全額）
    ├─ 6. 加計加班費（OT1 ×4/3、OT2 ×5/3、OT_HOLIDAY ×2）
    ├─ 7. 計算勞保費（員工負擔：InsuredSalary × 2%）
    ├─ 8. 計算健保費（員工負擔：HealthInsuredAmount × 2.35% × 眷屬加乘）
    ├─ 9. 計算勞退自提（BaseSalary × VoluntaryRetirementRate）
    ├─ 10. 計算課稅所得 → 查 WithholdingTaxTable → 代扣所得稅
    ├─ 11. 計算雇主負擔（勞保 10%、健保 6.11%、勞退強制 6%，存入 PayrollRecord）
    └─ 12. 寫入 PayrollRecord + PayrollRecordDetail
```

---

## 三、開發階段狀態

### Phase 1 — 核心薪資計算 ✅ 完成（2026-03-09）

**資料庫**

| 實體 | 檔案 | 狀態 |
|------|------|------|
| PayrollPeriod | `Data/Entities/Payroll/PayrollPeriod.cs` | ✅ |
| PayrollItem | `Data/Entities/Payroll/PayrollItem.cs` | ✅ |
| EmployeeSalary | `Data/Entities/Payroll/EmployeeSalary.cs` | ✅ |
| PayrollRecord | `Data/Entities/Payroll/PayrollRecord.cs` | ✅ |
| PayrollRecordDetail | `Data/Entities/Payroll/PayrollRecordDetail.cs` | ✅ |
| EmployeeBankAccount | `Data/Entities/Payroll/EmployeeBankAccount.cs` | ✅ |
| MinimumWage | `Data/Entities/Payroll/MinimumWage.cs` | ✅（種子資料已建立）|
| LaborInsuranceGrade | `Data/Entities/Payroll/LaborInsuranceGrade.cs` | ✅（種子資料已建立）|
| HealthInsuranceGrade | `Data/Entities/Payroll/HealthInsuranceGrade.cs` | ✅（種子資料已建立）|
| WithholdingTaxTable | `Data/Entities/Payroll/WithholdingTaxTable.cs` | ✅（種子資料已建立）|

**服務層**

| 服務 | 介面 | 狀態 |
|------|------|------|
| PayrollPeriodService | IPayrollPeriodService | ✅ |
| EmployeeSalaryService | IEmployeeSalaryService | ✅ |
| EmployeeBankAccountService | IEmployeeBankAccountService | ✅ |
| PayrollItemService | IPayrollItemService | ✅ |
| PayrollCalculationService | IPayrollCalculationService | ✅ |

**UI 元件**

| 元件 | 路由 / 位置 | 類型 | 狀態 |
|------|------------|------|------|
| PayrollIndex | `/payroll` | 客製（工作流程型） | ✅ |
| PayslipModalComponent | — | 客製 Modal（唯讀文件型） | ✅ |
| PayrollPeriodIndex | `/payroll-periods` | 標準 + 自訂按鈕 | ✅ |
| PayrollPeriodEditModalComponent | — | 標準 Modal | ✅ |
| EmployeeSalaryIndex | `/employee-salaries` | 標準 | ✅ |
| EmployeeSalaryEditModalComponent | — | 標準 Modal | ✅ |
| EmployeeBankAccountIndex | `/employee-bank-accounts` | 標準 | ✅ |
| EmployeeBankAccountEditModalComponent | — | 標準 Modal | ✅ |
| PayrollItemIndex | `/payroll-items` | 標準 | ✅ |
| PayrollItemEditModalComponent | — | 標準 Modal | ✅ |
| EmployeePayrollTab | Employee EditModal 薪資 Tab | 半客製嵌入型 | ✅ |

**整合**

| 項目 | 狀態 |
|------|------|
| NavigationConfig.cs — Payroll 導覽節點 | ✅ |
| CompanyModule 種子資料（ModuleKey = Payroll） | ✅ |
| PermissionRegistry.Payroll 權限定義 | ✅ |
| RolePermissionSeeder — Payroll 權限種子 | ✅ |
| PayrollItemSeeder — 系統預設項目 | ✅ |
| i18n — 所有 Payroll.* / Page.Payroll* 鍵值 | ✅（5 語系）|

> **Phase 1 限制**：計算費率（勞健保比率、餐費免稅上限）目前以常數寫於 `PayrollCalculationService`，Phase 2 移入 DB 費率表。

---

### Phase 2 — 費率表維護 ✅ 完成（2026-03-09）

**資料庫**

| 實體 | 檔案 | 狀態 |
|------|------|------|
| InsuranceRate | `Data/Entities/Payroll/InsuranceRate.cs` | ✅（種子資料已建立）|

**服務層**

| 服務 | 介面 | 狀態 |
|------|------|------|
| MinimumWageService | IMinimumWageService | ✅ |
| InsuranceRateService | IInsuranceRateService | ✅ |
| LaborInsuranceGradeService | ILaborInsuranceGradeService | ✅ |
| HealthInsuranceGradeService | IHealthInsuranceGradeService | ✅ |
| WithholdingTaxTableService | IWithholdingTaxTableService | ✅ |
| PayrollCalculationService | — | ✅ 修改：費率從 InsuranceRate DB 讀取，常數改為備援 |

**UI 元件**

| 元件 | 路由 | 狀態 |
|------|------|------|
| MinimumWageIndex + EditModal | `/payroll/minimum-wages` | ✅ |
| InsuranceRateIndex + EditModal | `/payroll/insurance-rates` | ✅ |
| LaborInsuranceGradeIndex + EditModal | `/payroll/labor-insurance-grades` | ✅ 含年度選擇 + 複製功能 |
| HealthInsuranceGradeIndex + EditModal | `/payroll/health-insurance-grades` | ✅ 含年度選擇 + 複製功能 |
| WithholdingTaxTableIndex + EditModal | `/payroll/withholding-tax-tables` | ✅ 含年度/扶養人數篩選 |

> **Phase 2 說明**：年度扣繳憑單（格式 50）、勞保/健保申報報表列為 Phase 2b，待規劃。

---

### Phase 3 — 出勤整合 ✅ 完成（2026-03-09）

**資料庫**

| 實體 | 檔案 | 狀態 |
|------|------|------|
| MonthlyAttendanceSummary | `Data/Entities/Payroll/MonthlyAttendanceSummary.cs` | ✅（每員工每月一筆，UniqueIndex: EmployeeId+Year+Month）|

**服務層**

| 服務 | 介面 | 狀態 |
|------|------|------|
| MonthlyAttendanceSummaryService | IMonthlyAttendanceSummaryService | ✅ |
| PayrollCalculationService | — | ✅ 修改：計算前自動同步 MonthlyAttendanceSummary 出勤資料 |

**UI 元件**

| 元件 | 路由 | 狀態 |
|------|------|------|
| MonthlyAttendanceSummaryIndex | `/payroll/attendance` | ✅ 含批次初始化 |
| MonthlyAttendanceSummaryEditModalComponent | — | ✅ 含鎖定狀態唯讀 |

**整合**

| 項目 | 狀態 |
|------|------|
| PermissionRegistry.Payroll.Attendance | ✅ |
| NavigationConfig — Nav.AttendanceSummary | ✅ |
| Migration: AddMonthlyAttendanceSummary | ✅ |
| i18n — Attendance.* / Field.* 鍵值 | ✅（5 語系）|

> **計算流程**：`CalculateEmployeeAsync` 會先查 `MonthlyAttendanceSummaries`，若該員工該月份有記錄則以其出勤資料覆蓋薪資單預設值（全勤）。鎖定機制：出勤記錄可設 `IsLocked = true` 防止修改。

---

### Phase 4 — 會計整合 ⏳ 規劃中

- 薪資科目對應設定介面
- 關帳後自動產生會計傳票
- 部門成本分攤
- 薪資成本分析報表

---

### Phase 5 — 進階功能 ⏳ 規劃中

- 員工自助薪資查詢（Payroll.SelfView）
- 薪資單 Email 發送（加密 PDF）
- 薪資調整申請審核流程

---

## 四、檔案索引

### 實體 & 資料庫

| 功能 | 路徑 |
|------|------|
| 所有 Payroll 實體 | `Data/Entities/Payroll/` |
| Enum 定義 | `Models/Enums/PayrollEnums.cs` |
| Migration（AddDepartmentFields 含薪資欄位） | `Migrations/20260308084020_AddDepartmentFields.cs` |
| 薪資項目種子資料 | `Data/SeedDataManager/Seeders/PayrollItemSeeder.cs` |

### 服務層

| 功能 | 路徑 |
|------|------|
| 所有 Payroll 服務 | `Services/Payroll/` |
| 服務注冊 | `Data/ServiceRegistration.cs` |

### UI 元件

| 功能 | 路徑 |
|------|------|
| 所有 Payroll 頁面 | `Components/Pages/Payroll/` |
| 員工薪資歷史 Tab | `Components/Pages/Employees/EmployeeEditModal/EmployeePayrollTab.razor` |
| FieldConfiguration | `Components/FieldConfiguration/PayrollItemFieldConfiguration.cs` |
| FieldConfiguration | `Components/FieldConfiguration/PayrollPeriodFieldConfiguration.cs` |
| FieldConfiguration | `Components/FieldConfiguration/EmployeeSalaryFieldConfiguration.cs` |
| FieldConfiguration | `Components/FieldConfiguration/EmployeeBankAccountFieldConfiguration.cs` |

### 設定 & 整合

| 功能 | 路徑 |
|------|------|
| 導覽設定 | `Data/Navigation/NavigationConfig.cs` |
| 權限定義 | `Models/PermissionRegistry.cs`（Payroll class） |
| i18n 資源 | `Resources/SharedResource*.resx`（Payroll.* 區段） |

---

## 五、法規遵循摘要

| 項目 | 費率 / 規則 | Phase 1 實作方式 |
|------|------------|----------------|
| 勞保（員工） | 2% × InsuredSalary | 常數 |
| 勞保（雇主） | 10% × InsuredSalary | 常數 |
| 健保（員工） | 2.35% × HealthInsuredAmount × 眷屬加乘 | 常數 |
| 健保（雇主） | 6.11% × HealthInsuredAmount × 眷屬加乘 | 常數 |
| 勞退（雇主強制） | 6% × BaseSalary | 常數 |
| 勞退（員工自提） | 0–6%（VoluntaryRetirementRate） | DB 欄位 |
| 餐費免稅上限 | 3,000 元/月 | 常數 |
| 交通費免稅上限 | 3,000 元/月 | 常數 |
| 健保眷屬加乘 | `Min(DependentCount, 3) + 1` | 常數邏輯 |
| 所得稅扣繳 | 查 WithholdingTaxTable | DB 查表 |

> Phase 2 將把所有「常數」欄改為從 DB 費率表讀取，支援年度費率更新。

---

## 六、權限設計

| 權限代碼 | 說明 | 敏感度 |
|---------|------|--------|
| `Payroll.Read` | 進入薪資計算頁面 | 一般 |
| `Payroll.ReadAmount` | 查看薪資金額 | **高** |
| `Payroll.Calculate` | 執行薪資計算 | 敏感 |
| `Payroll.Confirm` | 確認薪資單 | 敏感 |
| `Payroll.Close` | 薪資週期關帳 | 敏感 |
| `Payroll.SalaryConfig` | 維護員工薪資設定 | 敏感 |
| `Payroll.RateTable` | 維護費率參照表 | 敏感 |
| `Payroll.Declaration` | 產生申報文件（Phase 2） | 敏感 |
| `Payroll.Payslip` | 列印薪資單 | 一般 |
| `Payroll.SelfView` | 員工查看本人薪資（Phase 5） | 一般 |

> `Payroll.ReadAmount` 屬高度敏感個資，應僅授予 HR / 財務主管。

---

## 相關文件

- [Phase 1 資料庫設計](./Readme_薪資_Phase1_資料庫設計.md)
- [Phase 1 介面設計](./Readme_薪資_Phase1_介面設計.md)
