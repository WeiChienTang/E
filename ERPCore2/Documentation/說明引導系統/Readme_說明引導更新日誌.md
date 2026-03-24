# 說明引導系統更新日誌

## 更新日期
2026-03-24

---

## 📊 總覽

| 項目 | 數量 |
|------|------|
| EditModal 總數 | 70 |
| 已接入 FeatureGuide | **64**（91.4%） |
| 未接入（原生 Modal） | **6**（8.6%） |
| Guide 定義檔 (.cs) | 65 |
| resx key 總數 | ~625 key × 5 語言 = ~3,125 條 |

---

## ✅ 已接入 FeatureGuide 的 EditModal（64 個）

### 銷售模組（Sales）— 6 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| SalesOrderEditModalComponent | SalesOrderGuide.cs | 9（概述、步驟、基本欄位、金額、明細、功能、審核、提示、FAQ） | ⭐⭐⭐ |
| QuotationEditModalComponent | QuotationGuide.cs | 7（概述、步驟、欄位、金額、功能、提示、FAQ） | ⭐⭐⭐ |
| SalesDeliveryEditModalComponent | SalesDeliveryGuide.cs | 7（概述、步驟、欄位、金額、功能、提示、FAQ） | ⭐⭐⭐ |
| SalesReturnEditModalComponent | SalesReturnGuide.cs | 7（概述、步驟、欄位、金額、功能、提示、FAQ） | ⭐⭐⭐ |
| SalesReturnReasonEditModalComponent | SalesReturnReasonGuide.cs | 2（概述、欄位） | ⭐ |
| SalesTargetEditModalComponent | SalesTargetGuide.cs | 2（概述、欄位） | ⭐ |

### 採購模組（Purchase）— 4 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| PurchaseOrderEditModalComponent | PurchaseOrderGuide.cs | 7（概述、步驟、欄位、金額、功能、提示、FAQ） | ⭐⭐⭐ |
| PurchaseReceivingEditModalComponent | PurchaseReceivingGuide.cs | 8（概述、步驟、欄位、金額、明細、功能、提示、FAQ） | ⭐⭐⭐ |
| PurchaseReturnEditModalComponent | PurchaseReturnGuide.cs | 7（概述、步驟、欄位、金額、功能、提示、FAQ） | ⭐⭐⭐ |
| PurchaseReturnReasonEditModalComponent | PurchaseReturnReasonGuide.cs | 2（概述、欄位） | ⭐ |

### 品項模組（Items）— 6 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| ItemEditModalComponent | ItemGuide.cs | 4（概述、欄位、功能、提示+FAQ） | ⭐⭐⭐ |
| ItemCompositionEditModalComponent | ItemCompositionGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| UnitEditModalComponent | UnitGuide.cs | 2（概述、欄位） | ⭐ |
| SizeEditModalComponent | SizeGuide.cs | 2（概述、欄位） | ⭐ |
| ItemCategoryEditModalComponent | ItemCategoryGuide.cs | 2（概述、欄位） | ⭐ |
| CompositionCategoryEditModalComponent | CompositionCategoryGuide.cs | 2（概述、欄位） | ⭐ |

### 客戶模組（Customers）— 4 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| CustomerEditModalComponent | CustomerGuide.cs | 4（概述、欄位、提示、FAQ） | ⭐⭐⭐ |
| CustomerVisitEditModalComponent | CustomerVisitGuide.cs | 2（概述、欄位） | ⭐⭐ |
| CustomerComplaintEditModalComponent | CustomerComplaintGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| CustomerBankAccountEditModalComponent | CustomerBankAccountGuide.cs | 2（概述、欄位） | ⭐ |

### 廠商模組（Suppliers）— 3 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| SupplierEditModalComponent | SupplierGuide.cs | 4（概述、欄位、提示、FAQ） | ⭐⭐⭐ |
| SupplierVisitEditModalComponent | SupplierVisitGuide.cs | 2（概述、欄位） | ⭐⭐ |
| SupplierBankAccountEditModalComponent | SupplierBankAccountGuide.cs | 2（概述、欄位） | ⭐ |

### 員工模組（Employees）— 7 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| EmployeeEditModalComponent | EmployeeGuide.cs | 3（概述、欄位、提示） | ⭐⭐⭐ |
| EmployeeLicenseEditModalComponent | EmployeeLicenseGuide.cs | 2（概述、欄位） | ⭐⭐ |
| EmployeeToolEditModalComponent | EmployeeToolGuide.cs | 2（概述、欄位） | ⭐ |
| EmployeeTrainingRecordEditModalComponent | EmployeeTrainingRecordGuide.cs | 2（概述、欄位） | ⭐ |
| DepartmentEditModalComponent | DepartmentGuide.cs | 2（概述、欄位） | ⭐ |
| EmployeePositionEditModalComponent | EmployeePositionGuide.cs | 2（概述、欄位） | ⭐ |
| RoleEditModalComponent | RoleGuide.cs | 2（概述、欄位） | ⭐ |

### 權限模組 — 1 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| PermissionEditModalComponent | PermissionGuide.cs | 2（概述、欄位） | ⭐ |

### 車輛模組（Vehicles）— 3 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| VehicleEditModalComponent | VehicleGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| VehicleMaintenanceEditModalComponent | VehicleMaintenanceGuide.cs | 2（概述、欄位） | ⭐⭐ |
| VehicleTypeEditModalComponent | VehicleTypeGuide.cs | 2（概述、欄位） | ⭐ |

### 設備模組（Equipment）— 3 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| EquipmentEditModalComponent | EquipmentGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| EquipmentMaintenanceEditModalComponent | EquipmentMaintenanceGuide.cs | 2（概述、欄位） | ⭐⭐ |
| EquipmentCategoryEditModalComponent | EquipmentCategoryGuide.cs | 2（概述、欄位） | ⭐ |

### CRM 模組 — 2 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| CrmLeadEditModalComponent | CrmLeadGuide.cs | 4（概述、欄位、功能、提示） | ⭐⭐ |
| CrmLeadFollowUpEditModalComponent | CrmLeadFollowUpGuide.cs | 2（概述、欄位） | ⭐ |

### 磅秤模組（ScaleManagement）— 2 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| ScaleRecordEditModalComponent | ScaleRecordGuide.cs | 3（概述、欄位、功能+提示） | ⭐⭐ |
| ScaleTypeEditModalComponent | ScaleTypeGuide.cs | 2（概述、欄位） | ⭐ |

### 倉庫模組（Warehouse）— 6 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| InventoryStockEditModalComponent | InventoryStockGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| InventoryTransactionEditModalComponent | InventoryTransactionGuide.cs | 1（概述） | ⭐ |
| MaterialIssueEditModalComponent | MaterialIssueGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| StockTakingEditModalComponent | StockTakingGuide.cs | 3（概述、欄位、提示+警告） | ⭐⭐⭐ |
| WarehouseEditModalComponent | WarehouseGuide.cs | 3（概述、欄位、提示） | ⭐ |
| WarehouseLocationEditModalComponent | WarehouseLocationGuide.cs | 2（概述、欄位） | ⭐ |

### 財務模組（FinancialManagement）— 5 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| SetoffDocumentEditModalComponent | SetoffDocumentGuide.cs | 3（概述、欄位、提示+警告） | ⭐⭐⭐ |
| BankStatementEditModalComponent | BankStatementGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| BankEditModalComponent | BankGuide.cs | 2（概述、欄位） | ⭐ |
| CurrencyEditModalComponent | CurrencyGuide.cs | 2（概述、欄位） | ⭐ |
| PaymentMethodEditModalComponent | PaymentMethodGuide.cs | 2（概述、欄位） | ⭐ |

### 會計模組（Accounting）— 3 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| AccountItemEditModalComponent | AccountItemGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| FiscalPeriodEditModalComponent | FiscalPeriodGuide.cs | 2（概述、欄位+警告） | ⭐⭐ |
| JournalEntryEditModalComponent | JournalEntryGuide.cs | 3（概述、欄位、提示+警告） | ⭐⭐⭐ |

### 文件模組（Documents）— 2 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| DocumentEditModalComponent | DocumentGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| DocumentCategoryEditModalComponent | DocumentCategoryGuide.cs | 2（概述、欄位） | ⭐ |

### 系統模組（Systems）— 4 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| CompanyEditModalComponent | CompanyGuide.cs | 3（概述、欄位、提示） | ⭐⭐⭐ |
| PaperSettingEditModalComponent | PaperSettingGuide.cs | 3（概述、欄位、提示） | ⭐⭐ |
| ReportPrintConfigurationEditModalComponent | ReportPrintConfigurationGuide.cs | 2（概述、欄位） | ⭐ |
| TextMessageTemplateEditModalComponent | TextMessageTemplateGuide.cs | 2（概述、步驟） | ⭐⭐ |

### 薪資模組（Payroll）— 4 個（使用 GenericEditModalComponent 的部分）

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| PayrollItemEditModalComponent | PayrollItemGuide.cs | 2（概述、欄位） | ⭐ |
| PayrollPeriodEditModalComponent | PayrollPeriodGuide.cs | 2（概述、欄位） | ⭐ |
| EmployeeSalaryEditModalComponent | EmployeeSalaryGuide.cs | 1（概述） | ⭐ |
| EmployeeBankAccountEditModalComponent | EmployeeBankAccountGuide.cs | 1（概述） | ⭐ |

### 生產模組（ProductionManagement）— 1 個

| EditModal | Guide 定義檔 | 書籤數 | 複雜度 |
|-----------|-------------|--------|--------|
| ManufacturingOrderEditModalComponent | ManufacturingOrderGuide.cs | 2（概述、欄位） | ⭐⭐ |

---

## ❌ 未接入 FeatureGuide 的 EditModal（6 個）

### 原因：使用原生 Bootstrap Modal

以下 6 個 Payroll 模組的 EditModal 使用原生 `<div class="modal">` HTML 結構，**未使用 `GenericEditModalComponent`**，因此無法直接傳入 `FeatureGuide` 參數。

| EditModal | 模組 | 未接入原因 |
|-----------|------|-----------|
| InsuranceRateEditModalComponent | 保費費率 | 原生 Bootstrap Modal，無 GenericEditModalComponent |
| HealthInsuranceGradeEditModalComponent | 健保分級表 | 原生 Bootstrap Modal，無 GenericEditModalComponent |
| LaborInsuranceGradeEditModalComponent | 勞保分級表 | 原生 Bootstrap Modal，無 GenericEditModalComponent |
| MinimumWageEditModalComponent | 基本工資 | 原生 Bootstrap Modal，無 GenericEditModalComponent |
| WithholdingTaxTableEditModalComponent | 扣繳稅額表 | 原生 Bootstrap Modal，無 GenericEditModalComponent |
| MonthlyAttendanceSummaryEditModalComponent | 月出勤彙總 | 原生 Bootstrap Modal，無 GenericEditModalComponent |

> **注意**：這 6 個模組的 Guide 定義檔（.cs）和 resx key 已預先建立，只是尚未在 EditModal 中接入。
>
> **解決方案（擇一）**：
> 1. 將原生 Modal 改造為使用 `GenericEditModalComponent`（建議，可統一架構）
> 2. 在原生 Modal 中手動引入 `BaseModalComponent` 的 Drawer 功能
> 3. 保持現狀，待後續 Payroll 模組整體重構時一併處理

---

## 📅 變更歷史

### 2026-03-24 — 全面擴展（第二、三批）

**新增 31 個 Guide 定義 + 接入 31 個 EditModal**

| 分類 | 新增模組 |
|------|---------|
| Items | Item, ItemComposition |
| Customers | Customer, CustomerVisit, CustomerComplaint, CustomerBankAccount |
| Suppliers | Supplier, SupplierVisit, SupplierBankAccount |
| Employees | Employee, EmployeeLicense, EmployeeTool, EmployeeTrainingRecord |
| Vehicles | Vehicle, VehicleMaintenance |
| Equipment | Equipment, EquipmentMaintenance |
| CRM | CrmLead, CrmLeadFollowUp |
| ScaleManagement | ScaleRecord |
| Documents | Document |
| Warehouse | InventoryStock, InventoryTransaction, MaterialIssue, StockTaking |
| FinancialManagement | SetoffDocument, BankStatement |
| Accounting | AccountItem, FiscalPeriod, JournalEntry |
| ProductionManagement | ManufacturingOrder |

**新增 resx key**：208 key × 5 語言 = 1,040 條

---

### 2026-03-24 — 簡單設定批次（第一批）

**新增 34 個 Guide 定義 + 接入 28 個 EditModal**（6 個原生 Modal 跳過）

| 分類 | 新增模組 |
|------|---------|
| Items | Unit, Size, ItemCategory, CompositionCategory |
| Sales | SalesReturnReason, SalesTarget |
| Purchase | PurchaseReturnReason |
| Employees | Department, EmployeePosition, Role, Permission |
| Vehicles | VehicleType |
| ScaleManagement | ScaleType |
| FinancialManagement | Bank, Currency, PaymentMethod |
| Documents | DocumentCategory |
| Equipment | EquipmentCategory |
| Warehouse | Warehouse, WarehouseLocation |
| Systems | Company, PaperSetting, ReportPrintConfiguration, TextMessageTemplate |
| Payroll | PayrollItem, PayrollPeriod, EmployeeSalary, EmployeeBankAccount + 6 個定義檔（未接入） |

**新增 resx key**：178 key × 5 語言 = 890 條

---

### 2026-03-24 — 進銷存核心模組

**新增 6 個 Guide 定義 + 接入 6 個 EditModal**（加上先前已完成的 SalesOrder）

| 分類 | 新增模組 |
|------|---------|
| Sales | Quotation, SalesDelivery, SalesReturn |
| Purchase | PurchaseOrder, PurchaseReceiving, PurchaseReturn |

**新增 resx key**：200 key × 5 語言 = 1,000 條

---

### 2026-03-23 — 系統建立與首個模組

**建立說明引導系統架構 + SalesOrder 範例**

- 建立核心數據模型：`FeatureGuideDefinition.cs`
- 建立自動渲染組件：`FeatureGuideRenderer.razor`
- 修改 `BaseModalComponent.razor`：Drawer 容器、書籤列、遮罩、浮動按鈕
- 修改 `GenericEditModalComponent.razor`：FeatureGuide 參數透傳
- 建立首個 Guide：`SalesOrderGuide.cs`（9 個書籤，最完整的範例）
- 新增共用 UI key 7 個 + SalesOrder 專用 60 個 = 67 key × 5 語言

**功能特性**：
- 右側滑出 Drawer + 左側彩色書籤導航
- 點擊書籤只顯示對應章節（非全部展開）
- 點擊遮罩或 ✕ 關閉
- 審計 popup 點擊外部關閉
- SuperAdmin Debug Bar 顯示 Guide 定義檔名
- 深色模式支援
- 手機版響應式佈局（書籤轉為水平排列）
- 5 語言 i18n 支援
