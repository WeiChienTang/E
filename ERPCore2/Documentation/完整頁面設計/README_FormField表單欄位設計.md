# FormField 表單欄位設計

## 更新日期
2026-02-17

---

## 概述

表單欄位是 EditModal 的核心，透過 `FormFieldDefinition` 定義欄位配置，由 `GenericFormComponent<TModel>` 自動渲染對應的 UI 子組件。支援 Tab 頁籤佈局、區段分組、自訂內容 Tab、以及多種欄位類型。

---

## 檔案結構

```
Components/Shared/UI/Form/
├── GenericFormComponent.razor           # 主表單組件（UI 渲染）
├── GenericFormComponent.razor.cs        # 主表單組件（邏輯）
├── GenericFormComponent.razor.css       # 表單樣式（含 Tab 樣式）
├── FormFieldDefinition.cs              # 欄位定義類別
├── FormConstants.cs                    # 常數定義
├── AutoCompleteFieldState.cs           # AutoComplete 狀態
├── FormTextField.razor                 # 文字欄位
├── FormNumberField.razor               # 數字欄位
├── FormSelectField.razor               # 下拉選單
├── FormDateField.razor                 # 日期欄位
├── FormAutoCompleteField.razor         # 自動完成欄位
├── FormMobilePhoneField.razor          # 手機號碼欄位
├── FormCheckboxField.razor             # 核取方塊
├── FormRadioField.razor                # 單選按鈕
├── FormTextAreaField.razor             # 多行文字
└── CharacterCountTextAreaComponent.razor # 帶字數統計的文字區域

Helpers/EditModal/
└── FormSectionHelper.cs                # 區段與 Tab 定義 Helper
```

---

## FormFieldDefinition 屬性

### 基本屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `PropertyName` | string | 實體屬性名稱（使用 `nameof()`） |
| `Label` | string | 欄位標籤文字 |
| `FieldType` | FormFieldType | 欄位類型 |
| `Placeholder` | string? | 佔位符文字 |
| `HelpText` | string? | 說明文字（Tooltip） |
| `IsRequired` | bool | 是否必填 |
| `IsReadOnly` | bool | 是否唯讀 |
| `IsDisabled` | bool | 是否停用 |
| `IsVisible` | bool | 是否可見（預設 `true`） |
| `DefaultValue` | object? | 預設值 |
| `Order` | int | 排列順序 |

### 文字欄位屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `MaxLength` | int? | 最大長度 |
| `MinLength` | int? | 最小長度 |

### 數字欄位屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Min` | decimal? | 最小值 |
| `Max` | decimal? | 最大值 |
| `Step` | decimal? | 步進值 |
| `DecimalPlaces` | int | 小數位數（預設 0 表示整數） |
| `UseThousandsSeparator` | bool | 是否使用千分位（預設 `true`），唯讀模式下顯示千分位格式 |

### TextArea 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Rows` | int? | 行數 |
| `MaxBytes` | int? | 最大位元組數（UTF-8，用於中文字處理） |
| `ShowCharacterCount` | bool | 是否顯示字數統計 |

### AutoComplete 欄位屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `SearchFunction` | `Func<string, Task<List<SelectOption>>>?` | 搜尋函數 |
| `MinSearchLength` | int | 最少搜尋字元數（預設 1，設為 0 可在空白時顯示全部選項） |
| `AutoCompleteDelayMs` | int | 搜尋延遲毫秒（預設 300） |

### ActionButton 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `ActionButtons` | `List<FieldActionButton>?` | 欄位右側操作按鈕 |

### Label 說明屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `LabelHelpItems` | `List<LabelHelpItem>?` | Label 旁的問號說明（Popover） |

### CSS 樣式屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `CssClass` | string? | 自訂 CSS 類別 |
| `ContainerCssClass` | string? | 容器 CSS 類別（如 `"col-4"` 控制欄位寬度） |

### 其他屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `AutoCompleteAttribute` | string? | HTML autocomplete 屬性（`"off"`、`"new-password"` 等） |
| `GroupName` | string? | 分組名稱 |
| `ValidationRules` | `List<ValidationRule>?` | 驗證規則 |

---

## FormFieldType 欄位類型

| 類型 | 子組件 | 說明 |
|------|--------|------|
| `Text` | FormTextField | 基本文字輸入 |
| `Email` | FormTextField | Email 輸入 |
| `Password` | FormTextField | 密碼輸入 |
| `MobilePhone` | FormMobilePhoneField | 台灣手機號碼（自動格式化 0912-345-678） |
| `Number` | FormNumberField | 數字輸入（支援千分位、Min/Max、小數位數） |
| `Date` | FormDateField | 日期選擇器 |
| `DateTime` | FormDateField | 日期時間選擇器 |
| `Time` | FormDateField | 時間選擇器 |
| `TextArea` | FormTextAreaField | 多行文字 |
| `TextAreaWithCharacterCount` | CharacterCountTextAreaComponent | 帶字數/位元組統計 |
| `Select` | FormSelectField | 下拉選單 |
| `MultiSelect` | FormSelectField | 多選下拉 |
| `Checkbox` | FormCheckboxField | 核取方塊 |
| `Radio` | FormRadioField | 單選按鈕群組 |
| `AutoComplete` | FormAutoCompleteField | 自動完成搜尋（含下拉、鍵盤導航） |
| `File` | - | 檔案上傳 |
| `Hidden` | - | 隱藏欄位 |
| `Custom` | - | 自定義欄位 |

---

## 欄位定義範例

### 文字欄位

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Customer.Code),
    Label = "客戶編號",
    FieldType = FormFieldType.Text,
    Placeholder = "請輸入客戶編號",
    IsRequired = true,
    MaxLength = 20,
    HelpText = "客戶的唯一識別編號"
}
```

### 數字欄位

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(SystemParameter.TaxRate),
    Label = "稅率 (%)",
    FieldType = FormFieldType.Number,
    IsRequired = true,
    Min = 0,
    Max = 100,
    Step = 0.01m,
    DecimalPlaces = 2,
    Placeholder = "請輸入稅率"
}
```

數字欄位行為：
- **唯讀模式**：顯示千分位格式（如 `1,234,567`）
- **可編輯 + UseThousandsSeparator=true**：Focus 時顯示原始數字，Blur 時顯示千分位
- **可編輯 + UseThousandsSeparator=false**：標準 HTML number input

### 日期欄位

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Employee.BirthDate),
    Label = "出生日期",
    FieldType = FormFieldType.Date,
    Placeholder = "請選擇日期"
}
```

### 下拉選單（手動選項）

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Entity.CategoryId),
    Label = "類別",
    FieldType = FormFieldType.Select,
    Options = categories.Select(c => new SelectOption
    {
        Value = c.Id.ToString(),
        Text = c.Name
    }).ToList()
}
```

### AutoComplete 欄位

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Customer.EmployeeId),
    Label = "業務負責人",
    FieldType = FormFieldType.AutoComplete,
    Placeholder = "請輸入或選擇業務負責人",
    MinSearchLength = 0,
    ActionButtons = await GetEmployeeActionButtonsAsync()
}
```

AutoComplete 特性：
- 鍵盤導航（ArrowUp/Down、Enter、Tab、Escape）
- 延遲搜尋（預設 300ms）
- 失焦時智慧比對
- 搭配 `AutoCompleteConfigBuilder` 使用

### 核取方塊

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(SystemParameter.EnablePurchaseOrderApproval),
    Label = "採購單審核",
    FieldType = FormFieldType.Checkbox
}
```

### 帶字數統計的文字區域

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Entity.Description),
    Label = "描述",
    FieldType = FormFieldType.TextAreaWithCharacterCount,
    Placeholder = "請輸入描述",
    Rows = 5,
    MaxLength = 500,
    MaxBytes = 1500
}
```

### 手機號碼欄位

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Customer.MobilePhone),
    Label = "行動電話",
    FieldType = FormFieldType.MobilePhone,
    Placeholder = "0912-345-678",
    HelpText = "聯絡人行動電話（10碼，以09開頭）"
}
```

### 自訂寬度欄位

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Customer.Website),
    Label = "公司網址",
    FieldType = FormFieldType.Text,
    ContainerCssClass = "col-4",  // 佔 4/12 欄寬
    MaxLength = 50
}
```

### 複製按鈕（ActionButton）

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Customer.InvoiceTitle),
    Label = "發票抬頭",
    FieldType = FormFieldType.Text,
    ActionButtons = new List<FieldActionButton>
    {
        new FieldActionButton
        {
            Text = "複製",
            Variant = "OutlinePrimary",
            Size = "Small",
            Title = "將公司名稱複製到發票抬頭",
            OnClick = async () => await CopyCompanyNameToInvoiceTitle()
        }
    }
}
```

---

## Tab 佈局設計

### 架構層次

```
Tab（頁籤）
├── Section A（區段，水平並排）
│   ├── Field 1
│   └── Field 2
├── Section B
│   ├── Field 3
│   └── Field 4
└── Custom Tab（自訂內容）
    └── RenderFragment（如子表格 VehicleTable）
```

### 使用 FormSectionHelper

```csharp
// 不需要 Tab 時 → 使用 .Build()
formSections = FormSectionHelper<Product>.Create()
    .AddToSection("基本資訊", p => p.Code, p => p.Name)
    .AddToSection("價格資訊", p => p.Price, p => p.Cost)
    .Build(); // 回傳 Dictionary<string, string>

// 需要 Tab 時 → 使用 .GroupIntoTab() + .BuildAll()
var layout = FormSectionHelper<Employee>.Create()
    .AddToSection(FormSectionNames.BasicInfo,
        e => e.Code, e => e.Name, e => e.Gender, e => e.BirthDate)
    .AddToSection(FormSectionNames.ContactInfo,
        e => e.Mobile, e => e.Email)
    .AddToSection(FormSectionNames.EmploymentInfo,
        e => e.DepartmentId, e => e.EmployeePositionId, e => e.HireDate)
    .AddToSection(FormSectionNames.AdditionalData,
        e => e.Remarks)
    // 一個 Tab 可包含多個 Section
    .GroupIntoTab("員工資料", "bi-person-fill",
        FormSectionNames.BasicInfo,
        FormSectionNames.ContactInfo,
        FormSectionNames.EmploymentInfo,
        FormSectionNames.AdditionalData)
    .GroupIntoTab("配給裝備", "bi-tools",
        FormSectionNames.EquipmentAssignment)
    .BuildAll();

formSections = layout.FieldSections;
tabDefinitions = layout.TabDefinitions;
```

### 自訂內容 Tab（GroupIntoCustomTab）

用於在 Tab 中嵌入非表單欄位的自訂內容（如子表格）：

```csharp
var layout = FormSectionHelper<Customer>.Create()
    .AddToSection(FormSectionNames.BasicInfo, c => c.Code, c => c.CompanyName)
    .AddToSection(FormSectionNames.ContactPersonInfo, c => c.ContactPerson, c => c.MobilePhone)
    .GroupIntoTab("客戶資料", "bi-building",
        FormSectionNames.BasicInfo, FormSectionNames.ContactPersonInfo)
    // 自訂內容 Tab - 嵌入子元件（如 VehicleTable）
    .GroupIntoCustomTab("配給車輛", "bi-truck", CreateVehicleTabContent())
    .BuildAll();

// RenderFragment 建立方法
private RenderFragment CreateVehicleTabContent() => __builder =>
{
    <VehicleTable Vehicles="@customerVehicles"
                 OnAddVehicle="@HandleAddVehicle"
                 OnEditVehicle="@HandleEditVehicle"
                 OnUnlinkVehicle="@HandleUnlinkVehicle" />
};
```

### 條件式區段

```csharp
var layout = FormSectionHelper<Employee>.Create()
    .AddToSection(FormSectionNames.BasicInfo, e => e.Code, e => e.Name)
    .AddIf(hasPermission, FormSectionNames.AccountInfo,
        e => e.Account, e => e.Password, e => e.RoleId)
    .AddCustomFields("篩選區", "FilterProductId", "FilterCategory")
    .AddCustomFieldsIf(showAdvanced, "進階篩選", "FilterDateRange")
    .BuildAll();
```

### FormTabDefinition 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Label` | string | Tab 標籤文字 |
| `Icon` | string? | Tab 圖示 CSS class（如 `"bi-person-fill"`） |
| `SectionNames` | `List<string>` | 此 Tab 包含的 Section 名稱 |
| `CustomContent` | `RenderFragment?` | 自訂內容（非 null 時渲染自訂內容而非表單欄位） |

### Tab 行為

- Tab 使用**按鈕樣式**（非傳統底線 nav-tabs），緊貼上方功能按鈕
- 非作用中的 Tab 使用 `display:none` 隱藏（保留表單欄位狀態）
- Model 變更時（如上下筆切換）自動重置到第一個 Tab
- 沒有 `TabDefinitions` 時保持現有水平並排 column 佈局（向後相容）

### FormSectionNames 常用常數

| 常數 | 值 |
|------|------|
| `BasicInfo` | 基本資訊 |
| `ContactInfo` | 聯絡資訊 |
| `ContactPersonInfo` | 聯絡人資訊 |
| `AmountInfo` | 金額資訊 |
| `AmountInfoAutoCalculated` | 金額資訊(系統自動計算) |
| `PaymentInfo` | 付款資訊 |
| `EmploymentInfo` | 任職資訊 |
| `AccountInfo` | 帳號資訊 |
| `SalesInfo` | 業務資訊 |
| `TradingTerms` | 交易條件 |
| `CompanyData` | 公司資料 |
| `OrganizationStructure` | 組織架構 |
| `UnitSettings` | 單位設定 |
| `CategoryAndSpecification` | 分類與規格 |
| `FinanceAndRemarks` | 財務與備註 |
| `AdditionalData` | 額外資料 |
| `AdditionalInfo` | 額外資訊 |
| `OtherInfo` | 其他資訊 |
| `EquipmentAssignment` | 配給裝備 |

---

## FormSectionHelper API 一覽

| 方法 | 說明 |
|------|------|
| `Create()` | 建立新的 Helper 實例 |
| `AddToSection(name, params selectors)` | 將屬性加入指定區段（Lambda Expression） |
| `AddCustomFields(name, params fieldNames)` | 將自訂欄位名稱加入指定區段 |
| `AddIf(condition, name, params selectors)` | 條件式加入屬性 |
| `AddCustomFieldsIf(condition, name, params fields)` | 條件式加入自訂欄位 |
| `GroupIntoTab(label, icon, params sectionNames)` | 將 Section 歸組到 Tab |
| `GroupIntoCustomTab(label, icon, renderFragment)` | 建立自訂內容 Tab |
| `Build()` | 回傳 `Dictionary<string, string>`（無 Tab 時使用） |
| `BuildAll()` | 回傳 `FormLayoutResult`（有 Tab 時使用） |

---

## GenericFormComponent 參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `TModel` | Type parameter | 資料模型類型 |
| `Model` | TModel | 資料模型實例 |
| `FieldDefinitions` | `List<FormFieldDefinition>` | 欄位定義清單 |
| `FieldSections` | `Dictionary<string, string>?` | 欄位到區段的映射 |
| `TabDefinitions` | `List<FormTabDefinition>?` | Tab 頁籤定義（有則啟用 Tab） |
| `ShowValidationSummary` | bool | 是否顯示驗證摘要 |
| `OnFieldChanged` | EventCallback | 欄位變更事件 |

---

## 資料流

```
使用端（EditModalComponent）
  │ 提供 Model、FieldDefinitions、FieldSections、TabDefinitions
  ▼
GenericFormComponent<TModel>
  │ 解析配置 → 渲染 Tab/Section → 呼叫子組件
  ▼
欄位子組件（FormTextField / FormNumberField / ...）
  │ 處理使用者輸入 → 觸發 ValueChanged
  ▼
GenericFormComponent.razor.cs
  │ SetPropertyValue() 更新 Model
  │ NotifyFieldChanged() 觸發 OnFieldChanged
  ▼
使用端接收變更事件
```

---

## 擴充指南

### 新增欄位類型

1. 在 `FormFieldType` enum 新增類型（`FormFieldDefinition.cs`）
2. 建立對應的子組件 `Form{TypeName}Field.razor`
3. 在 `GenericFormComponent.razor` 的 `RenderInputField` 方法新增 `case`

### 新增 ActionButton 圖示

在 `FormConstants.cs` 的 `ActionButtonText` 和 `Icons` 類別新增常數，並更新 `GetIconForButtonText()` 方法。

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md) - 總綱
- [README_EditModal設計.md](README_EditModal設計.md) - EditModal 設計
