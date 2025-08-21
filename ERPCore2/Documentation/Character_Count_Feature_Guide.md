# 字數統計與位元組限制功能使用指南

## 概述
此功能解決中文字元在資料庫儲存時的位元組長度問題。中文字元在 UTF-8 編碼中通常佔用 3 個位元組，而英文字元只佔用 1 個位元組。

## 新增的元件和功能

### 1. CharacterCountTextAreaComponent
- **位置**: `Components/Shared/Forms/CharacterCountTextAreaComponent.razor`
- **功能**: 帶有即時字數統計和位元組檢查的 TextArea 元件
- **特色**:
  - 即時顯示字元數和位元組數
  - 自動截斷超過位元組限制的內容
  - 視覺化警告（90% 時顯示警告色，超過時顯示紅色）
  - 防止切斷中文字元的智慧截斷

### 2. 新的表單欄位類型
- **TextAreaWithCharacterCount**: 在 `FormFieldType` 列舉中新增
- **MaxBytes**: 在 `FormFieldDefinition` 中新增位元組限制屬性
- **ShowCharacterCount**: 在 `FormFieldDefinition` 中新增是否顯示字數統計

### 3. 後端驗證屬性
- **MaxBytesAttribute**: 驗證 UTF-8 位元組長度
- **SmartMaxLengthAttribute**: 同時驗證字元數和位元組數

## 使用方法

### 方法一：使用輔助方法（推薦）
```csharp
// 自動配置的 Remarks 欄位
FormFieldConfigurationHelper.CreateRemarksField<Department>(
    label: "部門備註",
    placeholder: "請輸入部門備註",
    helpText: "其他需要補充的資訊",
    rows: 2,
    containerCssClass: "col-12"
)

// 自訂的字數統計欄位
FormFieldConfigurationHelper.CreateTextAreaWithCharacterCount(
    propertyName: "Description",
    label: "描述",
    maxCharacters: 300,
    maxBytes: 300
)
```

### 方法二：直接配置
```csharp
new FormFieldDefinition
{
    PropertyName = nameof(Entity.Remarks),
    Label = "備註",
    FieldType = FormFieldType.TextAreaWithCharacterCount,
    Placeholder = "請輸入備註",
    Rows = 2,
    MaxLength = 500,        // 字元數限制
    MaxBytes = 500,         // 位元組數限制
    HelpText = "其他需要補充的資訊",
    ContainerCssClass = "col-12"
}
```

### 方法三：自動套用限制
```csharp
// 自動為所有欄位套用從實體屬性讀取的長度限制
var formFields = new List<FormFieldDefinition> { /* 欄位定義 */ };
formFields = FormFieldConfigurationHelper.ApplyMaxLengthLimits<YourEntity>(formFields);
```

## 後端實體配置

### BaseEntity 已更新
```csharp
[Display(Name = "備註")]
[MaxLength(500, ErrorMessage = "備註不可超過500個字元")]
[MaxBytes(500, ErrorMessage = "備註內容過長，請減少字數")]
public string? Remarks { get; set; }
```

### 自訂實體配置
```csharp
[Display(Name = "描述")]
[SmartMaxLength(200, 400)] // 200字元或400位元組
public string? Description { get; set; }
```

## 效果展示

### 前端顯示
- 輸入框下方會顯示：`150 / 120 字`
- 超過 90% (108字) 時變成橙色警告
- 達到限制 (120字) 時變成紅色
- HTML `maxlength` 屬性防止輸入超過字元限制
- 自動防止輸入超過位元組限制的內容

### 後端驗證
- 提交時會驗證字元數和位元組數
- 如果超過限制會回傳驗證錯誤
- 防止惡意或意外的過長內容儲存

## 注意事項

1. **中文字元**: 中文字元在 UTF-8 中通常佔用 3 個位元組
2. **表情符號**: 表情符號可能佔用 4 個位元組
3. **自動截斷**: 前端會自動截斷，確保不會切斷多位元組字元
4. **效能**: 字數統計是即時的，對效能影響很小

## 現有程式碼升級

### DepartmentEditModalComponent 已更新
原本的：
```csharp
FieldType = FormFieldType.TextArea
```

現在使用：
```csharp
FormFieldConfigurationHelper.CreateRemarksField<Department>()
```

### 建議的升級步驟
1. 找出所有使用 `FormFieldType.TextArea` 且字數較多的欄位
2. 評估是否需要字數統計功能
3. 使用輔助方法或直接改為 `FormFieldType.TextAreaWithCharacterCount`
4. 為實體屬性加入 `MaxBytes` 驗證屬性

## 範例檔案
- **DepartmentEditModalComponent.razor**: 已更新的範例
- **CharacterCountTextAreaComponent.razor**: 元件實作
- **FormFieldConfigurationHelper.cs**: 輔助方法
