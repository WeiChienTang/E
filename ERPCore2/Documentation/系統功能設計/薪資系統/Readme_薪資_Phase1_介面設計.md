# 薪資系統 Phase 1 — 介面設計

> 關聯文件：[薪資系統設計總綱](./Readme_薪資系統設計總綱.md)
> 狀態：✅ 已完成（2026-03-09）

---

## 一、元件架構分類

薪資系統元件**不全都適合標準五層架構**。分類判斷依據：

| 判斷問題 | 標準架構 | 客製介面 |
|---------|---------|---------|
| 主要操作是 CRUD？ | ✓ | — |
| 核心流程是狀態轉換 / 批次操作？ | — | ✓ |
| 主要呈現是格式化文件（唯讀）？ | — | ✓ |
| 資料按時間週期組織（而非按實體）？ | — | ✓ |

### 元件分類總表（Phase 1 實際實作；含後續版本路由異動紀錄）

| 元件 | 分類 | 路由 / 位置 | 狀態 |
|------|------|-----------|------|
| PayrollIndex | 客製（工作流程型） | `/payroll` | ✅ |
| PayslipModalComponent | 客製（文件呈現型） | — | ✅ |
| EmployeeSalaryIndex | 標準 | `/payroll/salary-config`（原 `/employee-salaries`，v1.2 更名） | ✅ |
| EmployeeSalaryEditModalComponent | 標準 Modal | — | ✅ |
| ~~EmployeeBankAccountIndex~~ | ~~標準~~ | ~~`/employee-bank-accounts`~~ | 🗑 已移除（v1.2 整合至員工 EditModal 銀行帳號 Tab）|
| EmployeeBankAccountEditModalComponent | 標準 Modal | — | ✅ |
| PayrollItemIndex | 標準 | `/payroll/items`（原 `/payroll-items`，v1.2 更名） | ✅ |
| PayrollItemEditModalComponent | 標準 Modal | — | ✅ |
| ~~PayrollPeriodIndex~~ | ~~標準 + 自訂按鈕~~ | ~~`/payroll-periods`~~ | 🗑 已移除（v1.2 整合至 PayrollModalComponent Tab 5）|
| PayrollPeriodEditModalComponent | 標準 Modal + 自訂關帳按鈕 | — | ✅ |
| EmployeePayrollTab | 嵌入 Tab（唯讀） | Employee EditModal | ✅ |
| PayrollSettingsTab | 系統參數 Tab 擴充（嵌入型） | SystemParameterSettingsModal | ✅（依 `Payroll.RateTable` 權限動態加入；存放公司層級的薪資預設參數）|

---

## 二、客製介面設計決策

### PayrollIndex（薪資計算作業）

**選擇客製的理由：**
- 頁面中心是「薪資週期」（月份），而非實體清單
- 核心操作是工作流程（計算 → 確認 → 關帳），非 CRUD
- 週期狀態決定整頁 UI 的互動模式（草稿 vs 已關帳完全不同）
- GenericIndexPageComponent 無法表達「週期選擇器 + 全員狀態看板」的複合佈局

**實作結構：**
- 頂部週期下拉選單 + 狀態 Badge + 批次操作按鈕
- 統計卡片（總筆數 / 應發合計 / 實發合計 / 已確認筆數）
- 員工薪資狀態表格（自訂 ActionsTemplate：計算 / 確認 / 查看薪資單）
- 金額欄位依 `Payroll.ReadAmount` 權限控制顯示

### PayslipModalComponent（薪資單）

**選擇客製的理由：**
- 薪資單是格式化文件，不是可編輯表單
- 需要特定排版（收入/扣除分區 + 實發薪資置底強調）
- GenericEditModalComponent 是表單編輯器，不適合文件呈現場景

**實作結構：**
- 員工基本資訊 + 薪資週期 + 狀態 Badge（頂部）
- 收入明細表 / 扣除明細表（左右或上下並列）
- 實發薪資合計（視覺強調）
- 投保快照（可展開 `<details>`）

---

## 三、標準架構設計決策

### PayrollPeriodEditModalComponent

**自訂關帳按鈕：**
- 使用 `GenericEditModalComponent` 的 `CustomActionButtons`（`RenderFragment?`）參數
- 週期未關帳時顯示「關帳」按鈕（`btn-danger`）；已關帳時隱藏
- 關帳動作透過 `OnClosePeriod` EventCallback 回傳給父頁面處理

### EmployeeSalaryEditModalComponent

- ExpiryDate 欄位設為 `IsReadOnly = true`（由系統在調薪時自動管理）
- EffectiveDate 為必填
- 薪資金額欄位受 `Payroll.SalaryConfig` 權限保護

### EmployeePayrollTab（員工 EditModal 嵌入）

- 顯示條件：Payroll 模組啟用 **且** EmployeeId 有值（新增模式不顯示）
- 資料來源：`IPayrollCalculationService.GetByEmployeeAsync(employeeId)`
- 金額欄位依 `Payroll.ReadAmount` 權限控制
- 每列有「查看薪資單」按鈕，開啟嵌入的 `PayslipModalComponent`
- 公開方法：`LoadAsync(int employeeId)` / `Clear()`

---

## 四、介面設計規範

### 金額遮蔽規則

| 情況 | 顯示 |
|------|------|
| 有 `Payroll.ReadAmount` 權限 | 實際金額（如 41,230） |
| 無 `Payroll.ReadAmount` 權限 | `***` |
| 員工自助查看（Phase 5） | 本人金額（`Payroll.SelfView`） |

### 薪資單狀態 Badge

| 狀態 | CSS | 說明 |
|------|-----|------|
| 未計算 | `bg-secondary` | 尚未執行計算 |
| 草稿（試算） | `bg-warning text-dark` | 已計算，可重算 |
| 已確認 | `bg-success` | 確認後週期關帳前可取消 |

### 薪資週期狀態 Badge

| 狀態 | CSS |
|------|-----|
| 草稿 | `bg-warning text-dark` |
| 已關帳 | `bg-dark` |

---

## 五、Navigation 設定（現況，已更新至 v1.5）

實際設定於 `Data/Navigation/NavigationConfig.cs`，Payroll 父節點包含：

| 子項目 | 路由 / 類型 | 權限 | 備注 |
|--------|------------|------|------|
| 薪資計算作業 | `/payroll` | `Payroll.Calculate` | Phase 1 |
| 員工薪資設定 | `/payroll/salary-config` | `Payroll.SalaryConfig` | 原 `/employee-salaries`，v1.2 更名 |
| 薪資項目設定 | `/payroll/items` | `Payroll.RateTable` | 原 `/payroll-items`，v1.2 更名 |
| 基本工資維護 | `/payroll/minimum-wages` | `Payroll.RateTable` | Phase 2；v1.5 補加導覽項目 |
| 薪資圖表 | Action → Modal | `Payroll.ChartRead` | Phase 1 後加；ModuleKey="Charts" |
| ~~員工銀行帳戶~~ | ~~`/employee-bank-accounts`~~ | — | 🗑 v1.2 移除（整合至員工 EditModal）|
| ~~薪資週期管理~~ | ~~`/payroll-periods`~~ | — | 🗑 v1.2 移除（整合至 Modal Tab 5）|

---

## 六、Phase 1 實作順序（已完成）

```
Step 1-4：基礎設定、薪資項目、員工薪資設定、銀行帳戶      ✅
Step 5：薪資週期管理（標準 + 自訂開帳/關帳）              ✅
Step 6：PayrollCalculationService（計算引擎）             ✅
Step 7：PayrollIndex（客製工作流程頁面）                   ✅
Step 8：PayslipModalComponent（唯讀文件型 Modal）          ✅
Step 9：EmployeePayrollTab（員工 EditModal 嵌入 Tab）      ✅
```
