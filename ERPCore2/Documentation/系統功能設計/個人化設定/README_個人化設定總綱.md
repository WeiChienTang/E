# 個人化設定系統總綱

## 更新日期
2026-02-25

---

## 概述

允許每位已登入的員工透過「個人資料」選單調整自己的設定。設計以**可擴充的 Tab 架構**為核心，新增偏好設定類別只需加入新 Tab 元件，不影響現有結構。

目前支援的設定項目：

| 設定項目 | Tab | 說明 |
|----------|-----|------|
| 個人資料 | 個人資料 | 姓名、手機、Email、密碼（自助修改） |
| 介面語言 | 語言與地區 | 繁體中文 / English，儲存後自動 reload |

---

## 架構圖

```
NavMenu 「個人資料」（Action 類型）
    └── MainLayout.OpenPersonalPreference()
        └── PersonalPreferenceModalComponent
             ├── GenericFormComponent（Tab 容器）
             │    ├── Tab：個人資料   → PersonalDataTab.razor
             │    └── Tab：語言與地區 → LanguageRegionTab.razor
             └── HandleSave()
                  ├── EmployeePreferenceService.SavePreferenceAsync()
                  └── EmployeeService.UpdateSelfProfileAsync()
```

### 設計核心原則

- **設定記錄延遲建立**：首次儲存時才在 DB 寫入 `EmployeePreference`，不存在代表使用系統預設值
- **自助資料範圍受限**：個人資料僅允許修改 Name、Mobile、Email、Password；Account、RoleId 等敏感欄位由管理員控制
- **Tab 元件統一用 GenericFormComponent**：所有 Tab 以 `GenericFormComponent` 渲染欄位，結構一致，只有密碼區塊例外（非 Model 欄位）
- **語言切換透過 cookie + reload**：Blazor Server 限制，culture 確定後不可動態切換，須整頁重載

---

## 📚 子文件導覽

| 文件 | 說明 | 適用場景 |
|------|------|----------|
| [README_個人化設定_資料服務層.md](README_個人化設定_資料服務層.md) | Entity、DB 關係、Service 介面與實作 | 了解資料模型或修改偏好設定欄位 |
| [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md) | Tab 架構、元件關係、載入 / 儲存流程、觸發路徑 | 新增 Tab、調整 UI 行為 |
| [README_個人化設定_語言切換.md](README_個人化設定_語言切換.md) | IStringLocalizer、cookie、reload 完整設計 | 了解語言切換機制或新增語言 |

---

## 資料夾結構

```
Components/Pages/Employees/PersonalPreference/
├── PersonalPreferenceModalComponent.razor   ← 主 Modal（Tab 容器 + 儲存邏輯）
├── PersonalDataTab.razor                    ← 個人資料 Tab
└── LanguageRegionTab.razor                  ← 語言與地區 Tab
```

> 結構與 `Components/Pages/Systems/SystemParameter/` 相同，新增 Tab 只需在此資料夾加入元件。

---

## 新增偏好設定項目（快速指南）

以新增「字型大小」偏好為例，完整流程如下：

### 1. 加入 Enum（若需要）

```csharp
// Data/Entities/Employees/EmployeePreference.cs
public enum FontSizePreference { Small = 1, Medium = 2, Large = 3 }
```

### 2. 加入欄位至 Entity

```csharp
public FontSizePreference FontSize { get; set; } = FontSizePreference.Medium;
```

### 3. 更新 SavePreferenceAsync

```csharp
// EmployeePreferenceService.cs — else 區塊
existing.Language = preference.Language;
existing.FontSize = preference.FontSize;   // ← 新增
existing.UpdatedAt = DateTime.Now;
```

### 4. 新增 Tab 元件

在 `Components/Pages/Employees/PersonalPreference/` 建立 `DisplayTab.razor`，使用 `GenericFormComponent` 渲染欄位。

📖 詳見 [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md)

### 5. 在主 Modal 新增 Tab

```csharp
// PersonalPreferenceModalComponent.razor — OnInitialized
tabDefinitions = new List<FormTabDefinition>
{
    new() { Label = L["Preference.PersonalData"],    Icon = "bi bi-person",    ... },
    new() { Label = L["Preference.LanguageRegion"],  Icon = "bi bi-translate", ... },
    new() { Label = L["Preference.Display"],         Icon = "bi bi-type",      ... }  // ← 新增
};
```

### 6. 新增 resx 字串鍵值

在 `Resources/SharedResource.resx`（中文）與 `Resources/SharedResource.en-US.resx`（英文）加入對應鍵值。

### 7. 執行 Migration

```bash
dotnet ef migrations add AddEmployeePreferenceFontSize
dotnet ef database update
```

---

## 注意事項

1. **不要直接讀取 `Employee.Preference`**：除非在 `Include()` 時已載入，否則此導航屬性為 `null`。建議透過 `IEmployeePreferenceService.GetByEmployeeIdAsync()` 取得偏好設定
2. **BaseEntity 的 Code 欄位**：`EmployeePreference` 繼承自 `BaseEntity`，`Code` 欄位不使用，在 DB 中為 `null`，這是正常的
3. **個人資料 Tab 僅對有帳號的員工有意義**：非系統使用者（`IsSystemUser = false`）可開啟 Modal 但帳號欄位顯示「—」
4. **Select 欄位 enum 對應**：`FormSelectField` 將 enum 值轉為整數字串比對，`UILanguage.ZhTW = 1` 對應 option value `"1"`，`UILanguage.EnUS = 2` 對應 `"2"`
5. **語言切換比對對象**：`HandleSave()` 比對 `CultureInfo.CurrentUICulture.Name`（目前 cookie 文化），而非 DB 儲存值，避免 DB 已更新但 cookie 未跟上時永遠無法觸發 reload

---

## 相關文件

- [README_個人化設定_資料服務層.md](README_個人化設定_資料服務層.md)
- [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md)
- [README_個人化設定_語言切換.md](README_個人化設定_語言切換.md)
