# 個人化設定 — 語言切換設計

## 更新日期
2026-02-25

---

## Blazor Server 的切換限制

Blazor Server 使用 SignalR 長連線，culture 在 HTTP 請求建立時即已確定，**無法在連線存續期間動態切換**。因此語言切換的流程必須是：

```
儲存語言偏好至 DB → 比對目前 UI 文化 → 寫入 culture cookie → window.location.reload()
```

---

## 四層架構

```
[DB] EmployeePreference.Language          ← 持久化使用者偏好
    ↓ 登入後 / 語言變更後
[Cookie] .AspNetCore.Culture=c=zh-TW|uic=zh-TW
    ↓ 每次 HTTP 請求
[Middleware] UseRequestLocalization       → 設定 CultureInfo.CurrentUICulture
    ↓ 元件渲染時
[Component] IStringLocalizer<SharedResource> → @L["Employee.Name"]
```

---

## 相關檔案

### 新增

| 檔案 | 說明 |
|------|------|
| `Resources/SharedResource.cs` | IStringLocalizer 定位用的空 marker class（namespace 必須為專案根命名空間 `ERPCore2`） |
| `Resources/SharedResource.resx` | 繁體中文字串（預設，key = 英文點記法） |
| `Resources/SharedResource.en-US.resx` | 英文翻譯字串 |
| `Helpers/CultureHelper.cs` | `UILanguage` ↔ culture code 轉換工具 |
| `wwwroot/js/culture-helper.js` | 寫入 cookie 並執行 `window.location.reload()` |

### 修改

| 檔案 | 變更說明 |
|------|----------|
| `Program.cs` | 加入 `AddLocalization()`、`UseRequestLocalization` |
| `Components/App.razor` | `<html lang="...">` 改為動態讀取 `CultureInfo.CurrentUICulture`；引入 `culture-helper.js` |
| `Controllers/AuthController.cs` | 登入成功後依 `EmployeePreference.Language` 寫入 culture cookie |
| `PersonalPreferenceModalComponent.razor` | `HandleSave()` 語言變更後呼叫 JS reload |
| `Components/_Imports.razor` | 加入 `@using Microsoft.Extensions.Localization` |

---

## CultureHelper

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

新增語言時，在此加入對應的 culture code 以及 `SupportedCultures` 陣列。

---

## Program.cs 設定

```csharp
// ⚠️ 不可設 ResourcesPath = "Resources"（見下方說明）
builder.Services.AddLocalization();

// UseAuthentication() 之前
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("zh-TW")
    .AddSupportedCultures(CultureHelper.SupportedCultures)
    .AddSupportedUICultures(CultureHelper.SupportedCultures));
// CookieRequestCultureProvider 預設已啟用，自動讀取 .AspNetCore.Culture cookie
```

> **⚠️ 重要：AddLocalization() 不可設 ResourcesPath**
>
> `SharedResource.cs` 與 `.resx` 同在 `Resources/` 資料夾，SDK 的 **DependentUpon** 慣例會以 class 的實際 namespace（`ERPCore2`）命名 embedded resource，編譯後資源名稱為 `ERPCore2.SharedResource`。
>
> 若設 `ResourcesPath = "Resources"`，localizer 會尋找 `ERPCore2.Resources.SharedResource` → **找不到 → 回傳 key 名稱**。
>
> 保持 `AddLocalization()`（預設 `ResourcesPath = ""`），localizer 直接找 `ERPCore2.SharedResource` → ✅ 正確。

---

## 登入後設置 Culture Cookie

```csharp
// Controllers/AuthController.cs — 登入成功後
var preference = await _employeePreferenceService.GetByEmployeeIdAsync(employee.Id);
var cultureCode = CultureHelper.ToCultureCode(preference.Language);
var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultureCode));
Response.Cookies.Append(
    CookieRequestCultureProvider.DefaultCookieName,  // ".AspNetCore.Culture"
    cookieValue,
    new CookieOptions { MaxAge = TimeSpan.FromDays(365), Path = "/" }
);
```

---

## 語言切換後 Reload

```csharp
// PersonalPreferenceModalComponent.HandleSave() — 語言偏好儲存成功後

// 與 CultureInfo.CurrentUICulture（目前 cookie 文化）比對，而非與 DB 原始值比對
// 原因：若前次儲存已更新 DB 但 JS reload 失敗（cookie 未更新），
//       下次開啟時若改與 DB 值比對，DB 值 = 目標值 → 永遠不觸發 reload
var newCultureCode = ERPCore2.Helpers.CultureHelper.ToCultureCode(employeePreference.Language);
if (newCultureCode != System.Globalization.CultureInfo.CurrentUICulture.Name)
{
    await JSRuntime.InvokeVoidAsync("setCultureAndReload", newCultureCode);
    return; // reload 後頁面會重整，不需要繼續執行
}
```

```javascript
// wwwroot/js/culture-helper.js
window.setCultureAndReload = function (culture) {
    const cookieValue = `c=${culture}|uic=${culture}`;
    document.cookie = `.AspNetCore.Culture=${cookieValue};path=/;max-age=31536000`;
    window.location.reload();
};
```

---

## IStringLocalizer 使用方式

### Marker class

```csharp
// Resources/SharedResource.cs
namespace ERPCore2;  // ← 必須是根命名空間，不可為 ERPCore2.Resources

public class SharedResource { }
```

### 元件注入

```razor
@inject IStringLocalizer<SharedResource> L

<label>@L["Employee.Name"]</label>
<GenericButtonComponent Text="@L["Button.Save"]" ... />
```

### 欄位定義中使用（需在 OnInitialized 初始化）

```csharp
// ✅ 正確：在 OnInitialized() 中使用 L（DI 已注入）
protected override void OnInitialized()
{
    allFields = new List<FormFieldDefinition>
    {
        new() { Label = L["Employee.Name"], ... }
    };
}

// ❌ 錯誤：在欄位宣告處使用 L（DI 尚未注入）
private readonly List<FormFieldDefinition> allFields = new()
{
    new() { Label = L["Employee.Name"], ... }  // NullReferenceException
};
```

### 資源檔格式（key 採英文點記法）

`SharedResource.resx`（zh-TW 預設）：

| Name | Value |
|------|-------|
| `Employee.Name` | 姓名 |
| `Button.Save` | 儲存 |
| `Preference.Title` | 個人化設定 |
| `Error.PasswordMismatch` | 兩次輸入的密碼不一致，請重新確認 |

`SharedResource.en-US.resx`：

| Name | Value |
|------|-------|
| `Employee.Name` | Name |
| `Button.Save` | Save |
| `Preference.Title` | Personalization |
| `Error.PasswordMismatch` | Passwords do not match, please confirm again |

> key 一律使用英文點記法（`Module.Property`），不因語言而異。未提供翻譯的 key 會直接顯示 key 名稱（`IStringLocalizer` 預設行為）。

---

## 遷移策略

目前翻譯範圍：PersonalPreference 系列元件（Phase 1–3 完成）。

| 階段 | 工作 | 狀態 |
|------|------|------|
| Phase 1 | 建立基礎設施（`AddLocalization`、`CultureHelper`、`Resources/`、JS） | ✅ 完成 |
| Phase 2 | 登入後寫入 cookie + 切換時 reload | ✅ 完成 |
| Phase 3 | PersonalPreference 元件字串遷移 | ✅ 完成 |
| Phase 4 | 其他頁面逐一遷移 | 待推進 |

新增頁面翻譯時（Phase 4）：
1. 在兩份 `.resx` 加入新 key
2. 在目標元件注入 `@inject IStringLocalizer<SharedResource> L`
3. 將硬編碼中文字串改為 `@L["Key"]`

---

## 相關文件

- [README_個人化設定總綱.md](README_個人化設定總綱.md)
- [README_個人化設定_資料服務層.md](README_個人化設定_資料服務層.md)
- [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md)
