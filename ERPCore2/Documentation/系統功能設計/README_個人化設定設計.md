# 個人化設定設計

## 更新日期
2026-02-24

---

## 概述

允許每位已登入的員工透過「個人資料」選單調整自己的設定，目前支援：
- **個人資料**：姓名、手機、Email、密碼（自助修改）
- **語言與地區**：介面語言選擇（UI 語言切換功能尚未實作）

設計核心原則：
- **設定記錄延遲建立**：首次儲存時才在 DB 寫入 `EmployeePreference`，不存在代表使用系統預設值
- **自助資料範圍受限**：個人資料僅允許修改 Name、Mobile、Email、Password；Account、RoleId 等敏感欄位由管理員控制
- **邏輯不重複**：密碼雜湊、資料更新邏輯統一在 `UpdateSelfProfileAsync`，不在 UI 層重複
- **Tab 元件統一用 GenericFormComponent**：`PersonalDataTab` 與 `LanguageRegionTab` 均以 `GenericFormComponent` 渲染欄位，結構一致

---

## 檔案一覽

### 新增檔案

| 檔案 | 說明 |
|------|------|
| `Data/Entities/Employees/EmployeePreference.cs` | Entity 定義，含 `UILanguage` 列舉 |
| `Services/Employees/IEmployeePreferenceService.cs` | 偏好設定服務介面 |
| `Services/Employees/EmployeePreferenceService.cs` | 偏好設定服務實作 |
| `Components/Pages/Employees/PersonalPreference/PersonalPreferenceModalComponent.razor` | 主 Modal（Tab 容器 + 儲存邏輯） |
| `Components/Pages/Employees/PersonalPreference/PersonalDataTab.razor` | 個人資料 Tab（GenericFormComponent + 密碼區塊） |
| `Components/Pages/Employees/PersonalPreference/LanguageRegionTab.razor` | 語言與地區 Tab（GenericFormComponent） |

### 修改檔案

| 檔案 | 變更說明 |
|------|----------|
| `Data/Entities/Employees/Employee.cs` | 加入導航屬性 `public EmployeePreference? Preference { get; set; }` |
| `Data/Context/AppDbContext.cs` | 加入 `DbSet<EmployeePreference>`，並在 `OnModelCreating` 設定 1-to-1 Fluent API |
| `Data/ServiceRegistration.cs` | 註冊 `IEmployeePreferenceService` → `EmployeePreferenceService` |
| `Services/Employees/IEmployeeService.cs` | 新增 `UpdateSelfProfileAsync` 方法 |
| `Services/Employees/EmployeeService.cs` | 實作 `UpdateSelfProfileAsync` |
| `Components/_Imports.razor` | 加入 `@using ERPCore2.Components.Pages.Employees.PersonalPreference` |
| `Components/Layout/NavMenu.razor` | 「個人資料」選單項目改為 `NavigationItemType.Action`，觸發 `OpenPersonalPreference` |
| `Components/Layout/MainLayout.razor` | 掛入 Modal、`showPersonalPreference` 狀態、`OpenPersonalPreference()` 方法及 actionRegistry 註冊 |

---

## 資料夾結構

```
Components/Pages/Employees/PersonalPreference/
├── PersonalPreferenceModalComponent.razor   ← 主 Modal
├── PersonalDataTab.razor                    ← 個人資料 Tab
└── LanguageRegionTab.razor                  ← 語言與地區 Tab
```

> 結構與 `Components/Pages/Systems/SystemParameter/` 相同，未來新增 Tab 只需在此資料夾新增元件。

---

## 資料層設計

### EmployeePreference Entity

```csharp
// Data/Entities/Employees/EmployeePreference.cs
[Index(nameof(EmployeeId), IsUnique = true)]
public class EmployeePreference : BaseEntity
{
    [Required]
    [ForeignKey(nameof(Employee))]
    public int EmployeeId { get; set; }

    public UILanguage Language { get; set; } = UILanguage.ZhTW;

    public Employee? Employee { get; set; }
}

public enum UILanguage
{
    ZhTW = 1,   // 繁體中文（預設）
    EnUS = 2    // English
}
```

### AppDbContext 1-to-1 關係

```csharp
// Data/Context/AppDbContext.cs — OnModelCreating
modelBuilder.Entity<EmployeePreference>(entity =>
{
    entity.Property(e => e.Id).ValueGeneratedOnAdd();

    entity.HasOne(ep => ep.Employee)
          .WithOne(e => e.Preference)
          .HasForeignKey<EmployeePreference>(ep => ep.EmployeeId)
          .OnDelete(DeleteBehavior.Cascade); // 員工刪除時設定一併刪除
});
```

---

## 服務層設計

### IEmployeePreferenceService

```csharp
public interface IEmployeePreferenceService : IGenericManagementService<EmployeePreference>
{
    // 不存在時回傳預設值（不寫入 DB）
    Task<EmployeePreference> GetByEmployeeIdAsync(int employeeId);

    // 不存在則 INSERT，存在則 UPDATE
    Task<ServiceResult> SavePreferenceAsync(int employeeId, EmployeePreference preference);
}
```

### SavePreferenceAsync — Upsert 核心邏輯

```csharp
var existing = await context.EmployeePreferences
    .FirstOrDefaultAsync(p => p.EmployeeId == employeeId);

if (existing == null)
{
    preference.EmployeeId = employeeId;
    preference.CreatedAt = DateTime.Now;
    context.EmployeePreferences.Add(preference);     // 新增
}
else
{
    existing.Language = preference.Language;          // 只更新設定欄位
    existing.UpdatedAt = DateTime.Now;
}
```

### IEmployeeService.UpdateSelfProfileAsync — 個人資料自助更新

```csharp
// 允許修改：Name、Mobile、Email、Password
// 不允許修改：Account、RoleId、DepartmentId 等（由管理員控制）
Task<ServiceResult> UpdateSelfProfileAsync(
    int employeeId,
    string name,
    string? mobile,
    string? email,
    string? newPassword  // 傳 null 或空白 = 不變更密碼
);
```

**實作重點：**
- 密碼雜湊在此方法內部執行（`SeedDataHelper.HashPassword`），UI 層無需處理
- `EmployeeEditModalComponent` 繼續使用 `UpdateAsync`（完整欄位），兩者邏輯不重疊

---

## UI 設計

### 元件關係

```
PersonalPreferenceModalComponent
 ├── GenericFormComponent（Tab 容器）
 │    ├── 個人資料 Tab  → PersonalDataTab
 │    └── 語言與地區 Tab → LanguageRegionTab
 └── HandleSave()
      ├── EmployeePreferenceService.SavePreferenceAsync()
      └── EmployeeService.UpdateSelfProfileAsync()
```

### Tab 元件設計原則

所有 Tab 元件均使用 `GenericFormComponent` 渲染欄位，並以 `FieldDefinitions` + `FieldSections` 資料驅動方式定義欄位配置。

**PersonalDataTab** — 接收 `Employee? Model`，GenericFormComponent 之外另有密碼區塊（`border-top` 分隔）：

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
| 姓名（Name） | 可編輯 | 必填，`IsRequired = true` |
| 手機號碼（Mobile） | 可編輯 | 選填，`FormFieldType.MobilePhone` |
| 電子郵件（Email） | 可編輯 | 選填 |
| 新密碼 | 可編輯 | 留空表示不變更，由 Modal 管理狀態 |
| 確認新密碼 | 可編輯 | 前端即時比對，不一致則阻止儲存 |

**LanguageRegionTab** — 接收 `EmployeePreference? Model`，GenericFormComponent 直接渲染語言下拉選單：

```razor
<GenericFormComponent TModel="EmployeePreference"
                      Model="@(Model ?? _fallback)"
                      FieldDefinitions="@allFields"
                      FieldSections="@fieldSections" />
```

| 欄位 | 說明 |
|------|------|
| 介面語言（Language） | `FormFieldType.Select`，選項：繁體中文（1）/ English（2） |

### 主 Modal 狀態管理

`PersonalPreferenceModalComponent` 持有兩份資料作為 Tab 元件的 Model：

| 欄位 | 型別 | 說明 |
|------|------|------|
| `employeePreference` | `EmployeePreference` | 傳給 `LanguageRegionTab`，GenericFormComponent 直接修改 |
| `currentEmployee` | `Employee?` | 傳給 `PersonalDataTab`，GenericFormComponent 直接修改 |
| `newPassword` / `confirmPassword` | `string` | 密碼區塊獨立狀態，透過 EventCallback 回傳 |

### 主 Modal 使用方式

```razor
@* 在 MainLayout 中掛入 *@
<PersonalPreferenceModalComponent IsVisible="@showPersonalPreference"
                                  IsVisibleChanged="@((bool visible) => showPersonalPreference = visible)" />
```

### 載入流程

```
IsVisible = true
    └── OnParametersSetAsync()
        └── LoadDataAsync()
            ├── GetCurrentEmployeeIdAsync()              → 從 Claims 取得目前員工 ID
            ├── EmployeePreferenceService.GetByEmployeeIdAsync() → 語言偏好（不存在則回傳預設值）
            └── EmployeeService.GetByIdAsync()           → 員工個人資料
```

### 儲存流程

```
HandleSave()
 ├── [前端] 密碼確認比對（不一致則中止）
 ├── EmployeePreferenceService.SavePreferenceAsync(employeePreference)  → 儲存語言偏好
 └── EmployeeService.UpdateSelfProfileAsync()                           → 儲存姓名/手機/Email/密碼
```

### 關閉時重置

`CloseModalAsync()` 重置以下狀態，確保下次開啟時重新從 DB 載入：

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

## Migration

每次加入新欄位後需執行：

```bash
dotnet ef migrations add AddEmployeePreference
dotnet ef database update
```

---

## 未來擴充指南

### 新增偏好設定項目（例如：字型大小）

#### 1. 加入 Enum（若需要）

```csharp
// EmployeePreference.cs
public enum FontSizePreference
{
    Small = 1,
    Medium = 2,  // 預設
    Large = 3
}
```

#### 2. 加入欄位至 Entity

```csharp
public FontSizePreference FontSize { get; set; } = FontSizePreference.Medium;
```

#### 3. 更新 SavePreferenceAsync

```csharp
// EmployeePreferenceService.cs — else 區塊
existing.Language = preference.Language;
existing.FontSize = preference.FontSize;   // 新增這行
existing.UpdatedAt = DateTime.Now;
```

#### 4. 新增 Tab 元件

在 `Components/Pages/Employees/PersonalPreference/` 新增對應的 Tab .razor 檔，使用 `GenericFormComponent` 渲染欄位。

#### 5. 在主 Modal 新增 Tab

```csharp
// PersonalPreferenceModalComponent.razor — OnInitialized
tabDefinitions = new List<FormTabDefinition>
{
    new() { Label = "個人資料",  Icon = "bi bi-person",    ... },
    new() { Label = "語言與地區", Icon = "bi bi-translate", ... },
    new() { Label = "顯示設定",  Icon = "bi bi-type",      ... }  // ← 新增
};
```

#### 6. 執行 Migration

```bash
dotnet ef migrations add AddEmployeePreferenceFontSize
dotnet ef database update
```

---

## 注意事項

1. **不要直接讀取 `Employee.Preference`**：除非在 `Include()` 時已載入，否則此導航屬性為 `null`。建議透過 `IEmployeePreferenceService.GetByEmployeeIdAsync()` 取得偏好設定
2. **語言切換功能尚未實作**：目前 Modal 已可儲存語言選擇至 DB，但實際的 UI 語言切換邏輯（Blazor i18n / `IStringLocalizer`）留待後續開發（詳見下方語言切換設計章節）
3. **BaseEntity 的 Code 欄位**：`EmployeePreference` 繼承自 `BaseEntity`，`Code` 欄位不使用，在 DB 中為 `null`，這是正常的
4. **個人資料 Tab 僅對有帳號的員工有意義**：非系統使用者（`IsSystemUser = false`）可開啟 Modal 但帳號欄位顯示「—」
5. **Select 欄位 enum 對應**：`FormSelectField` 將 enum 值轉為整數字串比對，`UILanguage.ZhTW = 1` 對應 option value `"1"`，`UILanguage.EnUS = 2` 對應 `"2"`

---

## 語言切換設計（待實作）

### Blazor Server 的切換限制

Blazor Server 使用 SignalR 長連線，culture 在 HTTP 請求建立時即已確定，**無法在連線存續期間動態切換**。因此語言切換的流程必須是：

```
儲存語言偏好至 DB → 寫入 culture cookie → window.location.reload()
```

### 四層架構

```
[DB] EmployeePreference.Language          ← 持久化使用者偏好（已完成）
    ↓ 登入後 / 語言變更後
[Cookie] .AspNetCore.Culture=c=zh-TW|uic=zh-TW
    ↓ 每次 HTTP 請求
[Middleware] UseRequestLocalization       → 設定 CultureInfo.CurrentUICulture
    ↓ 元件渲染時
[Component] IStringLocalizer<SharedResource> → @L["Employee.Name"]
```

### 新增檔案

| 檔案 | 說明 |
|------|------|
| `Resources/SharedResource.cs` | IStringLocalizer 定位用的空 marker class |
| `Resources/SharedResource.resx` | 繁體中文字串（預設，key = 英文點記法） |
| `Resources/SharedResource.en-US.resx` | 英文翻譯字串 |
| `Helpers/CultureHelper.cs` | `UILanguage` ↔ culture code 轉換工具 |
| `wwwroot/js/culture-helper.js` | 寫入 cookie 並執行 `window.location.reload()` |

### 修改檔案

| 檔案 | 變更說明 |
|------|----------|
| `Program.cs` | 加入 `AddLocalization`、`UseRequestLocalization` |
| `Components/App.razor` | `<html lang="...">` 改為動態讀取 `CultureInfo.CurrentUICulture` |
| 登入 handler | 登入成功後依 `EmployeePreference.Language` 寫入 culture cookie |
| `PersonalPreferenceModalComponent.razor` | `HandleSave()` 語言變更後呼叫 JS reload |
| `Components/Pages/Employees/PersonalPreference/LanguageRegionTab.razor` | `HelpText` 由「即將推出」改為正式說明 |

### CultureHelper

```csharp
// Helpers/CultureHelper.cs
public static class CultureHelper
{
    public static string ToCultureCode(UILanguage language) => language switch
    {
        UILanguage.ZhTW => "zh-TW",
        UILanguage.EnUS => "en-US",
        _ => "zh-TW"
    };

    public static readonly string[] SupportedCultures = ["zh-TW", "en-US"];
}
```

### Program.cs 設定

```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// UseAuthentication() 之前
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("zh-TW")
    .AddSupportedCultures(CultureHelper.SupportedCultures)
    .AddSupportedUICultures(CultureHelper.SupportedCultures));
// CookieRequestCultureProvider 預設已啟用，自動讀取 .AspNetCore.Culture cookie
```

### 登入後設置 Culture Cookie

```csharp
// 登入成功後（AuthController 或 LoginPage）
var preference = await employeePreferenceService.GetByEmployeeIdAsync(employeeId);
var cultureCode = CultureHelper.ToCultureCode(preference.Language);
var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureCode));
Response.Cookies.Append(
    CookieRequestCultureProvider.DefaultCookieName,  // ".AspNetCore.Culture"
    cookieValue,
    new CookieOptions { MaxAge = TimeSpan.FromDays(365), Path = "/" }
);
```

### 語言切換後 Reload（PersonalPreferenceModalComponent）

```csharp
// HandleSave() 語言偏好儲存成功後
await JSRuntime.InvokeVoidAsync("setCultureAndReload",
    CultureHelper.ToCultureCode(employeePreference.Language));
```

```javascript
// wwwroot/js/culture-helper.js
window.setCultureAndReload = function (culture) {
    const cookieValue = `c=${culture}|uic=${culture}`;
    document.cookie = `.AspNetCore.Culture=${cookieValue};path=/;max-age=31536000`;
    window.location.reload();
};
```

### 資源檔格式

`SharedResource.resx`（zh-TW 預設，key 採英文點記法）：

| Name | Value |
|------|-------|
| `Employee.Name` | 姓名 |
| `Employee.Account` | 登入帳號 |
| `Employee.Mobile` | 手機號碼 |
| `Button.Save` | 儲存 |
| `Button.Cancel` | 取消 |

`SharedResource.en-US.resx`：

| Name | Value |
|------|-------|
| `Employee.Name` | Name |
| `Employee.Account` | Account |
| `Employee.Mobile` | Mobile |
| `Button.Save` | Save |
| `Button.Cancel` | Cancel |

### 元件使用方式

```razor
@inject IStringLocalizer<SharedResource> L

<label class="form-label fw-medium">@L["Employee.Name"]</label>
<GenericButtonComponent Text="@L["Button.Save"]" ... />
```

### 遷移策略（漸進式）

目前所有字串均硬編碼為繁體中文。遷移分四個階段，不需要一次全部完成：

| 階段 | 工作 | 備註 |
|------|------|------|
| Phase 1 | 建立基礎設施（`AddLocalization`、`CultureHelper`、`Resources/`、JS） | 語言切換機制上線 |
| Phase 2 | 登入後寫入 cookie + 切換時 reload | 使用者能實際看到語言切換效果 |
| Phase 3 | 共用字串遷移（Button、通用標籤、錯誤訊息） | 從使用頻率最高處開始 |
| Phase 4 | 頁面逐一遷移 | 不影響系統穩定性，可按優先順序推進 |
