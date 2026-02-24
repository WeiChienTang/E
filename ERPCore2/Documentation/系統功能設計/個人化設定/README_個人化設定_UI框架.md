# 個人化設定 — UI 框架設計

## 更新日期
2026-02-25

---

## 元件關係

```
PersonalPreferenceModalComponent
 ├── GenericFormComponent（Tab 容器）
 │    ├── 個人資料 Tab  → PersonalDataTab.razor
 │    └── 語言與地區 Tab → LanguageRegionTab.razor
 └── HandleSave()
      ├── EmployeePreferenceService.SavePreferenceAsync()
      └── EmployeeService.UpdateSelfProfileAsync()
```

**主 Modal** 持有兩份資料作為 Tab 元件的 Model：

| 欄位 | 型別 | 說明 |
|------|------|------|
| `employeePreference` | `EmployeePreference` | 傳給 `LanguageRegionTab`，GenericFormComponent 直接修改 |
| `currentEmployee` | `Employee?` | 傳給 `PersonalDataTab`，GenericFormComponent 直接修改 |
| `newPassword` / `confirmPassword` | `string` | 密碼區塊獨立狀態，透過 EventCallback 回傳 |

---

## Tab 元件設計原則

所有 Tab 元件均使用 `GenericFormComponent` 渲染欄位，並以 `FieldDefinitions` + `FieldSections` 資料驅動方式定義欄位配置。

**規則：**
- `allFields` 與 `fieldSections` 必須在 `OnInitialized()` 中初始化（而非宣告為 `readonly` 欄位），原因是 `IStringLocalizer` 僅在 DI 完成後才可用
- Tab 元件以 `RenderFragment` 的方式由主 Modal 的 `OnInitialized()` 建立，並傳入 `FormTabDefinition.CustomContent`

---

## PersonalDataTab

接收 `Employee? Model`，在 GenericFormComponent 之外另有密碼區塊（`border-top` 分隔）：

```razor
<GenericFormComponent TModel="Employee"
                      Model="@(Model ?? _fallback)"
                      FieldDefinitions="@allFields"
                      FieldSections="@fieldSections" />

@* 密碼區塊（非 Model 欄位，獨立管理） *@
<div class="mt-4 pt-3 border-top">...</div>
```

| 欄位 | 可編輯 | 說明 |
|------|--------|------|
| 帳號（Account） | 唯讀 | 由管理員設定，`IsReadOnly = true` |
| 姓名（Name） | 可編輯 | 必填，`IsRequired = true`，MaxLength = 10 |
| 手機號碼（Mobile） | 可編輯 | 選填，`FormFieldType.MobilePhone` |
| 電子郵件（Email） | 可編輯 | 選填，MaxLength = 50 |
| 新密碼 | 可編輯 | 留空表示不變更，由 Modal 管理狀態 |
| 確認新密碼 | 可編輯 | 前端即時比對，不一致則阻止儲存 |

密碼輸入欄位透過 `EventCallback<string>` 回傳至主 Modal，主 Modal 在 `HandleSave()` 時統一驗證與傳遞：

```csharp
// PersonalDataTab.razor 參數
[Parameter] public string NewPassword { get; set; } = string.Empty;
[Parameter] public EventCallback<string> NewPasswordChanged { get; set; }
[Parameter] public string ConfirmPassword { get; set; } = string.Empty;
[Parameter] public EventCallback<string> ConfirmPasswordChanged { get; set; }
```

---

## LanguageRegionTab

接收 `EmployeePreference? Model`，GenericFormComponent 直接渲染語言下拉選單：

```razor
<GenericFormComponent TModel="EmployeePreference"
                      Model="@(Model ?? _fallback)"
                      FieldDefinitions="@allFields"
                      FieldSections="@fieldSections" />
```

| 欄位 | 說明 |
|------|------|
| 介面語言（Language） | `FormFieldType.Select`，選項：繁體中文（1）/ English（2） |

> `UILanguage` enum 值對應：`ZhTW = 1` → option value `"1"`；`EnUS = 2` → option value `"2"`。`GenericFormComponent.ConvertValue()` 負責將字串轉回 enum。

---

## 主 Modal 使用方式

```razor
@* 在 MainLayout 中掛入 *@
<PersonalPreferenceModalComponent IsVisible="@showPersonalPreference"
                                  IsVisibleChanged="@((bool visible) => showPersonalPreference = visible)" />
```

---

## 載入流程

```
IsVisible = true
    └── OnParametersSetAsync()
        └── LoadDataAsync()  （僅在 currentEmployeeId == null 時執行，避免重複載入）
            ├── GetCurrentEmployeeIdAsync()              → 從 Claims 取得目前員工 ID
            ├── EmployeePreferenceService.GetByEmployeeIdAsync() → 語言偏好（不存在則回傳預設值）
            └── EmployeeService.GetByIdAsync()           → 員工個人資料
```

---

## 儲存流程

```
HandleSave()
 ├── [前端] 密碼確認比對（不一致則中止）
 ├── EmployeePreferenceService.SavePreferenceAsync(employeePreference)  → 儲存語言偏好
 ├── EmployeeService.UpdateSelfProfileAsync()                           → 儲存姓名/手機/Email/密碼
 ├── 顯示成功通知
 └── [語言變更判斷] 若新語言 culture code ≠ CultureInfo.CurrentUICulture.Name
      └── JSRuntime.InvokeVoidAsync("setCultureAndReload", newCultureCode)  → reload
          （若相同則直接呼叫 CloseModalAsync）
```

---

## 關閉時重置

`CloseModalAsync()` 重置所有狀態，確保下次開啟時重新從 DB 載入：

```csharp
currentEmployeeId = null;
currentEmployee = null;
employeePreference = new();
newPassword = string.Empty;
confirmPassword = string.Empty;
```

---

## 觸發路徑

```
NavMenu 「個人資料」（NavigationItemType.Action）
    └── NavDropdownItem.HandleActionClickAsync()
        └── NavMenuInstance.TriggerActionAsync("OpenPersonalPreference")
            └── MainLayout.HandleNavigationAction()
                └── actionRegistry.Execute("OpenPersonalPreference")
                    └── MainLayout.OpenPersonalPreference()
                        └── showPersonalPreference = true
```

---

## 新增 Tab 步驟

1. 在 `Components/Pages/Employees/PersonalPreference/` 建立新的 `[Name]Tab.razor`
2. 接收對應 Model（通常是 `EmployeePreference?`，若有新 Entity 則另行定義）
3. 在 `OnInitialized()` 初始化 `allFields` 與 `fieldSections`，使用 `IStringLocalizer`
4. 在 `PersonalPreferenceModalComponent.OnInitialized()` 新增 Tab 定義：

```csharp
new() {
    Label = L["Preference.NewTab"],
    Icon = "bi bi-xxx",
    SectionNames = new(),
    CustomContent = CreateNewTabContent()
}
```

5. 新增 `CreateNewTabContent()` RenderFragment 方法

---

## 相關文件

- [README_個人化設定總綱.md](README_個人化設定總綱.md)
- [README_個人化設定_資料服務層.md](README_個人化設定_資料服務層.md)
- [README_個人化設定_語言切換.md](README_個人化設定_語言切換.md)
