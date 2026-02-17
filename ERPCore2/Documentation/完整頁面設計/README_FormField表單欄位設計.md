# FormField 表單欄位設計

## 更新日期
2026-02-17

---

## 概述

表單欄位是 EditModal 的核心，透過 `FormFieldDefinition` 定義欄位配置，由 `GenericFormComponent<TModel>` 自動渲染對應的 UI 子組件。支援 Tab 頁籤佈局、區段分組、以及多種欄位類型。

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
| `DecimalPlaces` | int | 小數位數 |

### TextArea 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Rows` | int? | 行數 |
| `MaxBytes` | int? | 最大位元組數（含字數統計時） |

### Select 欄位屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Options` | `List<SelectOption>?` | 下拉選項 |
| `EnumType` | Type? | Enum 類型（自動產生選項） |

### AutoComplete 欄位屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `SearchFunction` | `Func<string, Task<List<SelectOption>>>?` | 搜尋函數 |
| `MinSearchLength` | int | 最少搜尋字元數 |

### ActionButton 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `ActionButtons` | `List<FieldActionButton>?` | 欄位右側操作按鈕 |

### Label 說明屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `LabelHelpItems` | `List<LabelHelpItem>?` | Label 旁的問號說明 |

---

## FormFieldType 欄位類型

| 類型 | 子組件 | 說明 |
|------|--------|------|
| `Text` | FormTextField | 基本文字輸入 |
| `Email` | FormTextField | Email 輸入 |
| `Password` | FormTextField | 密碼輸入 |
| `MobilePhone` | FormMobilePhoneField | 台灣手機號碼（自動格式化 0912-345-678） |
| `Number` | FormNumberField | 數字輸入（支援千分位、Min/Max） |
| `Date` | FormDateField | 日期選擇器 |
| `DateTime` | FormDateField | 日期時間選擇器 |
| `TextArea` | FormTextAreaField | 多行文字 |
| `TextAreaWithCharacterCount` | CharacterCountTextAreaComponent | 帶字數/位元組統計 |
| `Select` | FormSelectField | 下拉選單 |
| `Checkbox` | FormCheckboxField | 核取方塊 |
| `Radio` | FormRadioField | 單選按鈕群組 |
| `AutoComplete` | FormAutoCompleteField | 自動完成搜尋（含下拉、鍵盤導航） |

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

### 下拉選單（Enum）

```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Employee.Gender),
    Label = "性別",
    FieldType = FormFieldType.Select,
    EnumType = typeof(GenderType),
    IsRequired = true
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

---

## Tab 佈局設計

### 架構層次

```
Tab（頁籤）
├── Section A（區段，水平並排）
│   ├── Field 1
│   └── Field 2
└── Section B
    ├── Field 3
    └── Field 4
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

### FormTabDefinition 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Label` | string | Tab 標籤文字 |
| `Icon` | string? | Tab 圖示 CSS class（如 `"bi-person-fill"`） |
| `SectionNames` | `List<string>` | 此 Tab 包含的 Section 名稱 |

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
| `PaymentInfo` | 付款資訊 |
| `EmploymentInfo` | 任職資訊 |
| `AccountInfo` | 帳號資訊 |
| `SalesInfo` | 業務資訊 |
| `TradingTerms` | 交易條件 |
| `AdditionalData` | 額外資料 |
| `OtherInfo` | 其他資訊 |
| `EquipmentAssignment` | 配給裝備 |

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

## 子組件相依關係

```
GenericFormComponent.razor
    │
    ├── GenericFormComponent.razor.cs （code-behind）
    │       │
    │       ├── AutoCompleteStateManager （管理 AutoComplete 狀態）
    │       │       └── AutoCompleteFieldState.cs
    │       │
    │       └── FormConstants.cs （常數引用）
    │
    ├── FormTextField.razor
    │       └── FormConstants.cs
    │
    ├── FormNumberField.razor
    │       ├── FormConstants.cs
    │       └── NumberFormatHelper （Helpers/NumericHelpers/）
    │
    ├── FormSelectField.razor
    │       └── FormConstants.cs
    │
    ├── FormDateField.razor
    │       └── FormConstants.cs
    │
    ├── FormMobilePhoneField.razor
    │       └── FormConstants.cs
    │
    ├── FormCheckboxField.razor
    │       └── FormConstants.cs
    │
    ├── FormRadioField.razor
    │       └── FormConstants.cs
    │
    ├── FormTextAreaField.razor
    │       └── FormConstants.cs
    │
    ├── FormAutoCompleteField.razor
    │       ├── FormConstants.cs
    │       └── AutoCompleteFieldState.cs
    │
    └── CharacterCountTextAreaComponent.razor
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
