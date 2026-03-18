# 薪資系統設計總綱

> 適用地區：中華民國台灣
> 文件版本：v1.2（2026-03-17）
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
- **費率可維護**：基本工資、投保薪資分級表、健保費率、所得稅扣繳表均存於 DB
- **月結鎖定**：薪資關帳後不可回溯修改
- **出勤連動**：計算依賴出勤模組提供實際出勤天數、加班時數（Phase 3 已完成）
- **會計連動**：關帳後自動產生會計傳票（Phase 4 規劃中）

---

## 二、架構圖

```
薪資管理（NavMenu / ModuleKey = Payroll）
    │
    ├─ 薪資計算作業（/payroll）
    │       └─ PayrollModalComponent          薪資作業主介面（含 6 個 Tab）
    │               ├─ Tab 0：薪資計算         員工列表、試算、確認、薪資單列印
    │               ├─ Tab 1：保費費率         InsuranceRateTab（需 RateTable 權限）
    │               ├─ Tab 2：勞保分級表       LaborInsuranceGradeTab（需 RateTable 權限）
    │               ├─ Tab 3：健保分級表       HealthInsuranceGradeTab（需 RateTable 權限）
    │               ├─ Tab 4：扣繳稅額表       WithholdingTaxTableTab（需 RateTable 權限）
    │               ├─ Tab 5：薪資週期         PayrollPeriodTab（需 Close 權限）
    │               └─ Tab 6：出勤彙總         AttendanceTab 內聯可編輯表格（需 Attendance 權限）
    │
    ├─ 員工薪資設定（/payroll/salary-config）  含薪資歷史 Tab
    │
    ├─ 薪資項目設定（/payroll/items）          系統項目 + 自訂項目（⚠ 見改進項目）
    │
    └─ 基本工資維護（/payroll/minimum-wages）

員工 EditModal
    └─ EmployeePayrollTab                   薪資歷史唯讀 Tab（Payroll 模組啟用時顯示）
```

> **設計決策（v1.2）**：將薪資週期管理、出勤彙總、參考費率表統一整合進 `PayrollModalComponent` 的 Tab 系統，以減少頁面跳轉、提升月結操作效率。各功能依權限顯示對應 Tab。

### 計算流程（Phase 3 更新版）

```
CalculateEmployeeAsync(employeeId, periodId)
    │
    ├─ 1. 取得員工有效薪資設定（EmployeeSalary，EffectiveDate ≤ 薪資月份）
    ├─ 2. 計算應出勤天數（GetWorkDaysInMonth，排除週末）
    ├─ 3. 查詢 MonthlyAttendanceSummary（若有該月出勤記錄，以實際出勤資料覆蓋步驟 2）
    ├─ 4. 計算本薪（BaseSalary × 出勤比例）
    ├─ 5. 加計職務加給（全額，不依出勤比例）
    ├─ 6. 加計津貼（餐飲/交通，全額）
    ├─ 7. 加計加班費（OT1 ×4/3、OT2 ×5/3、OT_HOLIDAY ×2）
    ├─ 8. 計算勞保費（員工負擔：InsuredSalary × 費率）
    ├─ 9. 計算健保費（員工負擔：HealthInsuredAmount × 費率 × 眷屬加乘）
    ├─ 10. 計算勞退自提（BaseSalary × VoluntaryRetirementRate）
    ├─ 11. 計算課稅所得 → 查 WithholdingTaxTable → 代扣所得稅
    ├─ 12. 計算雇主負擔（勞保、健保、勞退強制，存入 PayrollRecord）
    └─ 13. 寫入 PayrollRecord + PayrollRecordDetail
```

> 步驟 8-9 的費率自 Phase 2 起從 InsuranceRate DB 讀取，常數為備援值。

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

**UI 元件（v1.2 更新）**

| 元件 | 位置 | 類型 | 狀態 |
|------|------|------|------|
| PayrollIndex | `/payroll` | 客製頁面（開啟 Modal） | ✅ |
| PayrollModalComponent | Modal（Tab 0-6） | 薪資作業主控台 | ✅ |
| PayslipModalComponent | — | 唯讀薪資單 Modal | ✅ |
| ~~PayrollPeriodIndex~~ | ~~`/payroll/periods`~~ | ~~已移入 Modal Tab 5~~ | 🗑 已移除 |
| PayrollPeriodEditModalComponent | — | 標準 Modal | ✅ |
| EmployeeSalaryIndex | `/payroll/salary-config` | 標準 | ✅ |
| EmployeeSalaryEditModalComponent | — | 標準 Modal | ✅ |
| EmployeeBankAccountIndex | `/payroll/employee-bank-accounts` | 標準 | ✅ |
| EmployeeBankAccountEditModalComponent | — | 標準 Modal | ✅ |
| PayrollItemIndex | `/payroll/items` | 標準 | ✅ ⚠ 見改進項目問題一 |
| PayrollItemEditModalComponent | — | 標準 Modal | ✅ |
| EmployeePayrollTab | Employee EditModal 薪資 Tab | 半客製嵌入型 | ✅ |

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

**UI 元件（v1.2 更新）**

| 元件 | 位置 | 狀態 |
|------|------|------|
| MinimumWageIndex + EditModal | `/payroll/minimum-wages` | ✅ 獨立頁面 |
| InsuranceRateTab + EditModal | Modal Tab 1 | ✅ 已整合至 PayrollModalComponent |
| LaborInsuranceGradeTab + EditModal | Modal Tab 2 | ✅ 含年度選擇 + 複製功能 |
| HealthInsuranceGradeTab + EditModal | Modal Tab 3 | ✅ 含年度選擇 + 複製功能 |
| WithholdingTaxTableTab + EditModal | Modal Tab 4 | ✅ 含年度/扶養人數篩選 |
| ~~InsuranceRateIndex~~ | ~~獨立頁面~~ | 🗑 已移除（整合至 Modal） |
| ~~PayrollSettingsPage~~ | ~~/payroll/settings~~ | 🗑 已移除（整合至 Modal） |

> **Phase 2b（待規劃）**：年度扣繳憑單（格式 50）、勞保/健保申報報表。

---

### Phase 3 — 出勤整合 ✅ 完成（2026-03-17 更新）

**資料庫**

| 實體 | 檔案 | 狀態 |
|------|------|------|
| MonthlyAttendanceSummary | `Data/Entities/Payroll/MonthlyAttendanceSummary.cs` | ✅（每員工每月一筆，UniqueIndex: EmployeeId+Year+Month）|

**服務層**

| 服務 | 介面 | 狀態 |
|------|------|------|
| MonthlyAttendanceSummaryService | IMonthlyAttendanceSummaryService | ✅ |
| PayrollCalculationService | — | ✅ 修改：計算前查詢 MonthlyAttendanceSummary，有記錄則以出勤資料覆蓋預設值 |

**UI 元件（v1.2 更新）**

| 元件 | 位置 | 狀態 |
|------|------|------|
| AttendanceTab | Modal Tab 6 | ✅ 內聯可編輯表格，自動對應選定薪資週期 |
| ~~MonthlyAttendanceSummaryIndex~~ | ~~/payroll/attendance~~ | 🗑 已移除（整合至 Modal Tab 6）|
| MonthlyAttendanceSummaryEditModalComponent | — | ✅（保留，AtendanceTab 複雜編輯時使用）|

**整合**

| 項目 | 狀態 |
|------|------|
| PermissionRegistry.Payroll.Attendance | ✅ |
| Migration: AddMonthlyAttendanceSummary | ✅ |
| i18n — Attendance.* / Field.* 鍵值 | ✅（5 語系）|

> **AttendanceTab 設計**：顯示所有在職員工為列，每月出勤欄位為內聯輸入格，批次初始化後直接在表格中輸入，「儲存全部」一鍵批次儲存。鎖定記錄顯示唯讀。Tab 自動從 PayrollModalComponent 接收當前選定週期的年份與月份。

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
| 薪資主入口頁面 | `Components/Pages/Payroll/PayrollIndex.razor` |
| 薪資作業主 Modal（Tab 0-6） | `Components/Pages/Payroll/PayrollModalComponent.razor` |
| 所有 Tab 元件 | `Components/Pages/Payroll/PayrollSettings/` |
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

| 項目 | 費率 / 規則 | 實作方式 |
|------|------------|---------|
| 勞保（員工） | 依 InsuranceRate.LaborInsuranceEmployeeRate | DB 讀取（Phase 2）|
| 勞保（雇主） | 依 InsuranceRate.LaborInsuranceEmployerRate | DB 讀取（Phase 2）|
| 健保（員工） | 依 InsuranceRate.HealthInsuranceEmployeeRate × 眷屬加乘 | DB 讀取（Phase 2）|
| 健保（雇主） | 依 InsuranceRate.HealthInsuranceEmployerRate × 眷屬加乘 | DB 讀取（Phase 2）|
| 勞退（雇主強制） | 依 InsuranceRate.RetirementEmployerRate | DB 讀取（Phase 2）|
| 勞退（員工自提） | 0–6%（VoluntaryRetirementRate） | DB 欄位 |
| 餐費免稅上限 | 依 InsuranceRate.MealTaxFreeLimit | DB 讀取（Phase 2）|
| 交通費免稅上限 | 依 InsuranceRate.TransportTaxFreeLimit | DB 讀取（Phase 2）|
| 健保眷屬加乘 | `Min(DependentCount, 3) + 1` | 常數邏輯 |
| 所得稅扣繳 | 查 WithholdingTaxTable | DB 查表 |

---

## 六、權限設計

| 權限代碼 | 說明 | 對應 Modal Tab | 敏感度 |
|---------|------|---------------|--------|
| `Payroll.Read` | 進入薪資計算頁面 | — | 一般 |
| `Payroll.ReadAmount` | 查看薪資金額 | Tab 0 | **高** |
| `Payroll.Calculate` | 執行薪資計算 | Tab 0 | 敏感 |
| `Payroll.Confirm` | 確認薪資單 | Tab 0 | 敏感 |
| `Payroll.Payslip` | 列印薪資單 | Tab 0 | 一般 |
| `Payroll.RateTable` | 維護費率參照表 | Tab 1-4 | 敏感 |
| `Payroll.Close` | 薪資週期關帳 | Tab 5 | 敏感 |
| `Payroll.Attendance` | 維護出勤彙總 | Tab 6 | 一般 |
| `Payroll.SalaryConfig` | 維護員工薪資設定 | 獨立頁面 | 敏感 |
| `Payroll.Declaration` | 產生申報文件（Phase 2b） | 待設計 | 敏感 |
| `Payroll.SelfView` | 員工查看本人薪資（Phase 5） | 待設計 | 一般 |

> `Payroll.ReadAmount` 屬高度敏感個資，應僅授予 HR / 財務主管。

---

## 相關文件

- [Phase 1 資料庫設計](./Readme_薪資_Phase1_資料庫設計.md)
- [Phase 1 介面設計](./Readme_薪資_Phase1_介面設計.md)
- [改進項目（設計缺口與待辦）](./Readme_薪資改進項目.md)
