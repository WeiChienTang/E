# 薪資系統設計總綱

> 適用地區：中華民國台灣
> 文件版本：v1.6（2026-03-21）
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
    │       └─ PayrollModalComponent          薪資作業主介面（含 7 個 Tab，Tab 0–6）
    │               ├─ Tab 0：薪資計算         員工列表、試算、確認、薪資單列印
    │               ├─ Tab 1：保費費率         InsuranceRateTab（需 RateTable 權限）
    │               ├─ Tab 2：勞保分級表       LaborInsuranceGradeTab（需 RateTable 權限）
    │               ├─ Tab 3：健保分級表       HealthInsuranceGradeTab（需 RateTable 權限）
    │               ├─ Tab 4：扣繳稅額表       WithholdingTaxTableTab（需 RateTable 權限）
    │               ├─ Tab 5：薪資週期         PayrollPeriodTab（需 Close 權限）
    │               └─ Tab 6：出勤彙總         AttendanceTab 內聯可編輯表格（需 Attendance 權限）
    │
    ├─ 員工薪資設定（/payroll/salary-config）  薪資設定列表 + 薪資設定歷史 Tab（各期設定紀錄）
    │
    ├─ 薪資項目設定（/payroll/items）          系統項目 + 自訂項目（⚠ 見改進項目）
    │
    ├─ 基本工資維護（/payroll/minimum-wages）  獨立 CRUD 頁面（需 RateTable 權限）
    │                                         ← MinimumWageManageModalComponent 亦可從 EmployeeSalaryEditModal 內快速開啟
    │
    └─ 薪資圖表（NavMenu Action → Modal）     PayrollChartModalComponent（需 ChartRead 權限）

員工 EditModal
    └─ EmployeePayrollTab                   已計算薪資單唯讀 Tab（列出歷次 PayrollRecord；Payroll 模組啟用時顯示）
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
    ├─ 5. 加計固定月付津貼（動態讀取 EmployeeSalaryItem，全額不依出勤比例）
    ├─ 6. 加計平日加班費（OT1 前2hr ×4/3、OT2 後2hr ×5/3）— 勞基法第24條第1項
    ├─ 7. 計算休息日加班費（OT_HOLIDAY：前2hr ×4/3、後續 ×5/3）— 勞基法第24條第2項
    ├─ 8. 計算國定假日加班費（OT_NATIONAL：月薪已含假日薪，加給1倍時薪）— 勞基法第39條
    ├─ 9. 曠職扣薪（ABSENT：日薪 × 曠職天數）
    ├─ 10. 病假半薪扣除（SICK：日薪 × 0.5 × 病假天數）
    ├─ 11. 計算勞保費（員工負擔：InsuredSalary × 費率）
    ├─ 12. 計算健保費（員工負擔：HealthInsuredAmount × 費率 × 眷屬加乘）
    ├─ 13. 計算勞退自提（BaseSalary × VoluntaryRetirementRate）
    ├─ 14. 計算課稅所得（依 PayrollItem.IsTaxable 旗標，MEAL/TRANSPORT 有法定上限）→ 查 WithholdingTaxTable → 代扣所得稅
    ├─ 15. 計算雇主負擔（勞保、健保、勞退強制，存入 PayrollRecord）
    └─ 16. 寫入 PayrollRecord + PayrollRecordDetail
```

> 步驟 6–8 的加班費率自 Phase 2 起從 InsuranceRate DB 讀取，常數（4/3、5/3、1.0）為備援值。步驟 9–10 的曠職／病假扣薪依日薪計算，不使用 InsuranceRate。
>
> **⚠ 設計缺口（待確認）**：`DailyAttendanceStatus` 定義了 8 種狀態，但計算流程目前僅處理 `Absent`（步驟 9）與 `SickLeave`（步驟 10）。以下三種狀態的薪資處理邏輯尚未在計算流程中說明：
> - `PersonalLeave`（事假，無薪）— 邏輯應等同曠職扣薪，待確認是否已在計算引擎中合併處理
> - `AnnualLeave`（特休，有薪）— 不扣薪，待確認是否已在出勤天數計算中視為正常出勤
> - `MakeUpWork`（補班）— 應計入出勤，待確認補班加班費計算規則

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
| EmployeeSalaryItem | `Data/Entities/Payroll/EmployeeSalaryItem.cs` | ✅（動態津貼關聯表，取代舊有硬寫欄位）|
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
| ~~EmployeeBankAccountIndex~~ | ~~`/payroll/employee-bank-accounts`~~ | ~~獨立頁面~~ | 🗑 已移除（整合至員工 EditModal 銀行帳號 Tab）|
| EmployeeBankAccountEditModalComponent | — | 標準 Modal | ✅ |
| PayrollItemIndex | `/payroll/items` | 標準 | ✅ ⚠ 見改進項目問題一 |
| PayrollItemEditModalComponent | — | 標準 Modal | ✅ |
| EmployeePayrollTab | Employee EditModal 薪資 Tab | 半客製嵌入型 | ✅ |
| PayrollChartModalComponent | NavMenu Action → Modal | 統計圖表（趨勢／部門分布／收入結構）| ✅ |
| PayrollEmployeeTable | PayrollModalComponent Tab 0 內嵌 | 員工薪資狀態總表 | ✅ |

---

### Phase 2 — 費率表維護 ✅ 完成（2026-03-09；DB 結構於 v1.3/2026-03-19 補充 5 個加班費率欄位）

**資料庫**

| 實體 | 檔案 | 狀態 |
|------|------|------|
| InsuranceRate | `Data/Entities/Payroll/InsuranceRate.cs` | ✅（種子資料已建立；v1.3 新增 OvertimeRate1/2、RestDayRate1/2、NationalHolidayRate 5 欄位）|

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
| MinimumWageIndex + EditModal | `/payroll/minimum-wages` | ✅ 獨立頁面（v1.5 補加導覽項目） |
| MinimumWageManageModalComponent | EmployeeSalaryEditModal 內嵌 | ✅ 薪資設定時快速查閱／管理基本工資 |
| InsuranceRateTab + EditModal | Modal Tab 1 | ✅ 已整合至 PayrollModalComponent |
| LaborInsuranceGradeTab + EditModal | Modal Tab 2 | ✅ 含年度選擇 + 複製功能 |
| HealthInsuranceGradeTab + EditModal | Modal Tab 3 | ✅ 含年度選擇 + 複製功能 |
| WithholdingTaxTableTab + EditModal | Modal Tab 4 | ✅ 含年度/扶養人數篩選 |
| ~~InsuranceRateIndex~~ | ~~獨立頁面~~ | 🗑 已移除（整合至 Modal） |
| ~~PayrollSettingsPage~~ | ~~/payroll/settings~~ | 🗑 已移除（整合至 Modal） |

> **Phase 2b（待規劃）**：年度扣繳憑單（格式 50）、勞保/健保申報報表。
>
> 待確認設計項目：
> - 扣繳憑單資料來源（從 PayrollRecord 彙總，或另建申報彙總表）
> - 申報報表格式（勞保局電子申報 / 健保署格式 / 列印用 PDF）
> - 對應權限：`Payroll.Declaration`（已定義，待實作）

---

### Phase 3 — 出勤整合 ✅ 完成（2026-03-17 更新）

**資料庫**

| 實體 | 檔案 | 狀態 |
|------|------|------|
| MonthlyAttendanceSummary | `Data/Entities/Payroll/MonthlyAttendanceSummary.cs` | ✅（每員工每月一筆，UniqueIndex: EmployeeId+Year+Month）|
| AttendanceDailyRecord | `Data/Entities/Payroll/AttendanceDailyRecord.cs` | ✅（每員工每天一筆；記錄出勤狀態、工時、各類加班時數；不繼承 BaseEntity）|

> `AttendanceDailyRecord` 與 `MonthlyAttendanceSummary` 的關係：逐日記錄為詳細打卡層；月彙總為薪資計算輸入層。兩者獨立存在，月彙總可由逐日記錄匯總產生，也可手動輸入。

**服務層**

| 服務 | 介面 | 狀態 |
|------|------|------|
| MonthlyAttendanceSummaryService | IMonthlyAttendanceSummaryService | ✅ |
| AttendanceDailyRecordService | IAttendanceDailyRecordService | ✅（含 BatchInitAsync 批次初始化；支援 WorkdaysAsPresent / AllAsRestDay 兩種模式）|
| PayrollCalculationService | — | ✅ 修改：計算前查詢 MonthlyAttendanceSummary，有記錄則以出勤資料覆蓋預設值 |

**UI 元件（v1.2 更新）**

| 元件 | 位置 | 狀態 |
|------|------|------|
| AttendanceTab | Modal Tab 6 | ✅ 內聯可編輯表格，自動對應選定薪資週期 |
| AttendanceDailyDetailModal | AttendanceTab 內開啟 | ✅ 日曆式逐日出勤編輯 Modal（點擊員工列展開）|
| ~~MonthlyAttendanceSummaryIndex~~ | ~~/payroll/attendance~~ | 🗑 已移除（整合至 Modal Tab 6）|
| MonthlyAttendanceSummaryEditModalComponent | — | ✅（保留，AttendanceTab 複雜編輯時使用）|

**整合**

| 項目 | 狀態 |
|------|------|
| PermissionRegistry.Payroll.Attendance | ✅ |
| Migration: AddMonthlyAttendanceSummary | ✅ |
| Migration: AddAttendanceDailyRecord | ✅ |
| i18n — Attendance.* / Field.* 鍵值 | ✅（5 語系）|

> **AttendanceTab 設計**：顯示所有在職員工為列，每月出勤欄位為內聯輸入格，批次初始化後直接在表格中輸入，「儲存全部」一鍵批次儲存。鎖定記錄顯示唯讀。Tab 自動從 PayrollModalComponent 接收當前選定週期的年份與月份。
>
> **AttendanceDailyDetailModal 設計**：以日曆格式呈現單月逐日出勤，每格顯示出勤狀態（出勤／休息日／病假／曠職等）及加班時數，直接點格編輯。月薪員工 WorkHours 留 0，時薪員工填入實際工時（允許超過 8 小時）。

---

### Phase 4 — 會計整合 ⏳ 規劃中

**功能範圍**

| 子功能 | 說明 |
|--------|------|
| 薪資科目對應設定 | 設定各薪資項目（PayrollItem）對應的會計科目（AccountItem）|
| 關帳後自動產生傳票 | 薪資週期關帳時，依員工薪資記錄彙總產生應付薪資傳票 |
| 部門成本分攤 | 依員工所屬部門將薪資費用分攤至對應成本中心科目 |
| 薪資成本分析報表 | 按部門／期間彙總薪資成本，支援匯出 |

**待確認設計項目**

- 科目對應粒度：按 PayrollItem 類型對應，或逐項目設定？
- 傳票草稿機制：產生後需人工審核確認，或直接寫入已確認狀態？
- 部門分攤邏輯：無部門員工的薪資歸屬？
- 新增 Permission：`Payroll.AccountMapping`（維護科目對應）—待加入 PermissionRegistry
- 對應會計模組：依賴 AccountItem 模組已完成（✅）

**預計新增實體（待設計）**

| 實體 | 說明 |
|------|------|
| PayrollAccountMapping | PayrollItem ↔ AccountItem 對應設定表 |

**預計新增服務（待設計）**

| 服務 | 說明 |
|------|------|
| PayrollAccountMappingService | 維護科目對應 |
| PayrollJournalGenerationService | 關帳時產生傳票，呼叫 JournalEntryService |

**預計新增 UI（待設計）**

| 元件 | 位置 | 說明 |
|------|------|------|
| AccountMappingTab | PayrollModalComponent 或獨立頁面 | 科目對應設定介面 |
| 薪資成本報表 | ReportRegistry | 依部門彙總，需對應 ReportId |

---

### Phase 5 — 進階功能 ⏳ 規劃中

**功能範圍**

| 子功能 | 說明 | 對應權限 |
|--------|------|---------|
| 員工自助薪資查詢 | 員工登入後查看本人歷次薪資單（唯讀） | `Payroll.SelfView`（已定義）|
| 薪資單 Email 發送 | 以加密 PDF 寄送薪資單至員工信箱 | 待定（操作人需 `Payroll.Payslip`）|
| 薪資調整申請審核流程 | 員工或主管提交調薪申請，主管/HR 審核後更新 EmployeeSalary | 待定（需新增 `Payroll.AdjustRequest`、`Payroll.AdjustApprove`）|

**待確認設計項目**

- 自助查詢入口位置：獨立頁面（`/payroll/self`）或員工個人資料頁嵌入 Tab？
- Email 寄送觸發方式：手動逐人寄、批次寄，或關帳後自動寄？
- Email 加密 PDF：加密金鑰為員工身分證末四碼或另行設定？
- 審核流程依賴哪個審核框架（現有 Approval 模組？）
- 調薪申請生效方式：直接寫入 EmployeeSalary，或另建申請暫存表？

---

## 四、檔案索引

### 實體 & 資料庫

| 功能 | 路徑 |
|------|------|
| 所有 Payroll 實體 | `Data/Entities/Payroll/` |
| Enum 定義（主要）| `Models/Enums/PayrollEnums.cs`（PayrollPeriodStatus、PayrollItemType、PayrollItemCategory、SalaryType、TaxWithholdingType、PayrollRecordStatus、AttendanceInitMode）|
| Enum 定義（出勤狀態）| `Models/Enums/DailyAttendanceStatus.cs`（DailyAttendanceStatus，8 種逐日出勤狀態）|
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
| 逐日出勤編輯 Modal | `Components/Pages/Payroll/AttendanceDailyDetailModal.razor` |
| 薪資圖表 Modal | `Components/Pages/Payroll/PayrollChartModalComponent.razor` |
| 基本工資快速管理 Modal | `Components/Pages/Payroll/MinimumWageManageModalComponent.razor` |
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
| 平日加班費 | 依 InsuranceRate.OvertimeRate1/OvertimeRate2（法定4/3、5/3） | DB 讀取，含 fallback（v1.3）|
| 休息日加班費 | 依 InsuranceRate.RestDayRate1/RestDayRate2（法定4/3、5/3） | DB 讀取，含 fallback（v1.3）|
| 國定假日加班費 | 依 InsuranceRate.NationalHolidayRate（法定1.0，額外加給） | DB 讀取，含 fallback（v1.3）|

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
| `Payroll.ChartRead`（值：`"PayrollChart.Read"`） | 查看薪資統計圖表 | NavMenu Action | 一般 |

> `Payroll.ReadAmount` 屬高度敏感個資，應僅授予 HR / 財務主管。
>
> **命名備注**：`Payroll.ChartRead` 的實際權限值為 `"PayrollChart.Read"`，前綴與其他 Payroll 權限（`"Payroll.XXX"`）不同。這是因為圖表功能在 NavigationConfig 中以 `ModuleKey = "Charts"` 獨立掛載。若以 `"Payroll.*"` 前綴進行模組級別檢查，**不會**匹配此權限，需額外處理。

---

## 相關文件

- [Phase 1 資料庫設計](./Readme_薪資_Phase1_資料庫設計.md)
- [Phase 1 介面設計](./Readme_薪資_Phase1_介面設計.md)
- [改進項目（設計缺口與待辦）](./Readme_薪資改進項目.md)
