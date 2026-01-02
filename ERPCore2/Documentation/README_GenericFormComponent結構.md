# GenericForm 表單組件結構說明

## 概述

`GenericFormComponent` 是一個基於配置驅動的動態表單組件，透過 `FormFieldDefinition` 定義欄位，自動渲染對應的表單 UI。此架構採用子組件模式，將各欄位類型拆分為獨立組件，提高可維護性與可重用性。

---

## 檔案結構

```
Components/Shared/
├── GenericComponent/
│   └── Form/                                    ← 表單欄位子組件
│       ├── AutoCompleteFieldState.cs            ← AutoComplete 狀態管理
│       ├── FormConstants.cs                     ← 常數定義
│       ├── FormFieldDefinition.cs               ← 欄位定義類別
│       ├── FormAutoCompleteField.razor          ← 自動完成欄位
│       ├── FormTextField.razor                  ← 文字欄位
│       ├── FormNumberField.razor                ← 數字欄位
│       ├── FormSelectField.razor                ← 下拉選單
│       ├── FormDateField.razor                  ← 日期欄位
│       ├── FormMobilePhoneField.razor           ← 手機號碼欄位
│       ├── FormCheckboxField.razor              ← 核取方塊
│       ├── FormRadioField.razor                 ← 單選按鈕
│       ├── FormTextAreaField.razor              ← 多行文字
│       └── CharacterCountTextAreaComponent.razor ← 帶字數統計的文字區域
│
└── PageModel/
    └── EditModalComponent/
        ├── GenericFormComponent.razor           ← 主表單組件（UI）
        ├── GenericFormComponent.razor.cs        ← 主表單組件（邏輯）
        ├── GenericFormComponent.razor.css       ← 樣式
        └── GenericEditModalComponent.razor      ← 編輯 Modal 容器
```

---

## 核心檔案說明

### 主組件

| 檔案 | 用途 |
|------|------|
| `GenericFormComponent.razor` | 表單主組件的 UI 渲染邏輯，負責 Tab 佈局、欄位容器渲染，並調用各子組件 |
| `GenericFormComponent.razor.cs` | Code-behind 檔案，包含所有業務邏輯：屬性值存取、事件處理、狀態管理 |
| `GenericFormComponent.razor.css` | 表單專用樣式（Tab、欄位間距、ActionButton 等） |

### 欄位定義

| 檔案 | 用途 |
|------|------|
| `FormFieldDefinition.cs` | 定義單一欄位的所有屬性：PropertyName、Label、FieldType、Options、SearchFunction 等 |
| `FormConstants.cs` | 集中管理常數：按鈕文字、圖示、CSS 類別、預設值，避免魔術字串 |

### 狀態管理

| 檔案 | 用途 |
|------|------|
| `AutoCompleteFieldState.cs` | 封裝 AutoComplete 欄位的狀態（Options、IsLoading、IsVisible、Timer 等） |

### 欄位子組件

| 檔案 | 對應 FieldType | 說明 |
|------|----------------|------|
| `FormTextField.razor` | Text, Email, Password | 基本文字輸入 |
| `FormNumberField.razor` | Number | 數字輸入，支援千分位顯示、Min/Max 驗證 |
| `FormSelectField.razor` | Select | 下拉選單，支援 Enum 類型 |
| `FormDateField.razor` | Date, DateTime | 日期/日期時間選擇器 |
| `FormMobilePhoneField.razor` | MobilePhone | 台灣手機號碼（自動格式化為 0912-345-678） |
| `FormCheckboxField.razor` | Checkbox | 核取方塊 |
| `FormRadioField.razor` | Radio | 單選按鈕群組 |
| `FormTextAreaField.razor` | TextArea | 多行文字輸入 |
| `FormAutoCompleteField.razor` | AutoComplete | 自動完成搜尋欄位，含下拉選單、鍵盤導航 |
| `CharacterCountTextAreaComponent.razor` | TextAreaWithCharacterCount | 帶字數/位元組統計的文字區域 |

---

## 資料流

```
┌─────────────────────────────────────────────────────────────────┐
│                    使用端（如 ProductEditModal）                  │
│  - 提供 Model（TModel）                                          │
│  - 提供 FieldDefinitions（List<FormFieldDefinition>）            │
│  - 監聽 OnFieldChanged 事件                                      │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                   GenericFormComponent<TModel>                   │
│  - 解析 FieldDefinitions                                         │
│  - 依 FieldSections 分組為 Tab                                   │
│  - 渲染對應的子組件                                               │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        欄位子組件                                 │
│  FormTextField / FormNumberField / FormAutoCompleteField ...     │
│  - 接收 Field、Value、ValueChanged                               │
│  - 處理使用者輸入                                                 │
│  - 觸發 ValueChanged 回傳新值                                     │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                   GenericFormComponent.razor.cs                  │
│  - SetPropertyValue() 更新 Model                                 │
│  - NotifyFieldChanged() 觸發 OnFieldChanged 事件                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 使用範例

```razor
<GenericFormComponent TModel="Product"
                      Model="@_product"
                      FieldDefinitions="@_fieldDefinitions"
                      FieldSections="@_fieldSections"
                      OnFieldChanged="HandleFieldChanged"
                      EnableTabLayout="true" />

@code {
    private Product _product = new();
    
    private List<FormFieldDefinition> _fieldDefinitions = new()
    {
        new FormFieldDefinition 
        { 
            PropertyName = "ProductName", 
            Label = "商品名稱", 
            FieldType = FormFieldType.Text,
            IsRequired = true 
        },
        new FormFieldDefinition 
        { 
            PropertyName = "Price", 
            Label = "單價", 
            FieldType = FormFieldType.Number,
            Min = 0,
            DecimalPlaces = 2
        },
        new FormFieldDefinition 
        { 
            PropertyName = "SupplierId", 
            Label = "供應商", 
            FieldType = FormFieldType.AutoComplete,
            SearchFunction = SearchSuppliers
        }
    };
    
    private Dictionary<string, string> _fieldSections = new()
    {
        { "ProductName", "基本資料" },
        { "Price", "價格資訊" },
        { "SupplierId", "供應商" }
    };
    
    private async Task<List<SelectOption>> SearchSuppliers(string keyword)
    {
        // 搜尋邏輯...
    }
    
    private void HandleFieldChanged((string PropertyName, object? Value) args)
    {
        // 處理欄位變更（如連動更新其他欄位）
    }
}
```

---

## 相依關係圖

```
GenericFormComponent.razor
    │
    ├── GenericFormComponent.razor.cs （code-behind）
    │       │
    │       ├── AutoCompleteStateManager （管理 AutoComplete 狀態）
    │       │       └── AutoCompleteFieldState
    │       │
    │       └── FormConstants （常數引用）
    │
    ├── FormTextField.razor
    │       └── FormConstants
    │
    ├── FormNumberField.razor
    │       ├── FormConstants
    │       └── NumberFormatHelper （Helpers/NumericHelpers/）
    │
    ├── FormSelectField.razor
    │       └── FormConstants
    │
    ├── FormDateField.razor
    │       └── FormConstants
    │
    ├── FormMobilePhoneField.razor
    │       └── FormConstants
    │
    ├── FormCheckboxField.razor
    │       └── FormConstants
    │
    ├── FormRadioField.razor
    │       └── FormConstants
    │
    ├── FormTextAreaField.razor
    │       └── FormConstants
    │
    ├── FormAutoCompleteField.razor
    │       ├── FormConstants
    │       └── AutoCompleteFieldState
    │
    └── CharacterCountTextAreaComponent.razor
```

---

## 擴充指南

### 新增欄位類型

1. 在 `FormFieldType` enum 新增類型（位於 `FormFieldDefinition.cs`）
2. 建立對應的子組件 `Form{TypeName}Field.razor`
3. 在 `GenericFormComponent.razor` 的 `RenderInputField` 方法新增 case

### 新增 ActionButton 圖示

在 `FormConstants.cs` 的 `ActionButtonText` 和 `Icons` 類別新增對應常數，並更新 `GetIconForButtonText()` 方法。

---

## 版本歷程

| 日期 | 變更說明 |
|------|----------|
| 2026-01-02 | 重構：拆分子組件、建立 code-behind、加入 ILogger、建立 FormConstants |
| - | 原始版本：單一 1400 行 .razor 檔案 |
