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

## 支援語言清單

| UILanguage 枚舉 | Culture Code | 顯示名稱 | .resx 檔案 |
|----------------|-------------|---------|------------|
| `ZhTW = 1` | `zh-TW` | 繁體中文 | `SharedResource.resx`（預設） |
| `EnUS = 2` | `en-US` | English | `SharedResource.en-US.resx` |
| `JaJP = 3` | `ja-JP` | 日本語 | `SharedResource.ja-JP.resx` |
| `ZhCN = 4` | `zh-CN` | 简体中文 | `SharedResource.zh-CN.resx` |
| `FilPH = 5` | `fil` | Filipino | `SharedResource.fil.resx` |

新增語言時，需同步更新以下三處（見下方「新增語言 SOP」）。

---

## 相關檔案

### 新增

| 檔案 | 說明 |
|------|------|
| `Resources/SharedResource.cs` | IStringLocalizer 定位用的空 marker class（namespace 必須為專案根命名空間 `ERPCore2`） |
| `Resources/SharedResource.resx` | 繁體中文字串（預設，key = 英文點記法） |
| `Resources/SharedResource.en-US.resx` | 英文翻譯字串 |
| `Resources/SharedResource.ja-JP.resx` | 日文翻譯字串 |
| `Resources/SharedResource.zh-CN.resx` | 簡體中文翻譯字串 |
| `Resources/SharedResource.fil.resx` | 菲律賓語翻譯字串 |
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
| `Data/Entities/Employees/EmployeePreference.cs` | `UILanguage` 枚舉加入 `JaJP`、`ZhCN`、`FilPH` |
| `Models/Navigation/NavigationItem.cs` | 新增 `NameKey` 屬性（可選，供 NavMenu 多語系顯示用） |
| `Helpers/Common/NavigationActionHelper.cs` | `CreateActionItem()` 加入可選 `nameKey` 參數 |
| `Data/Navigation/NavigationConfig.cs` | 所有 NavigationItem 加入 `NameKey = "Nav.xxx"` |
| `Components/Layout/NavMenu.razor` | 注入 `L`，改用 `L[item.NameKey]` 顯示導航項目名稱 |
| `Models/Reports/ReportDefinition.cs` | 新增 `NameKey` 屬性（可選，供報表列表多語系顯示用） |
| `Data/Reports/ReportRegistry.cs` | 所有 ReportDefinition 加入 `NameKey = "Report.xxx"` |
| `Components/Shared/Report/GenericReportIndexModalComponent.razor` | 注入 `L`，用 `DisplayFormatter` 顯示本地化報表名稱，UI 文字改用資源 key |
| `Components/Pages/Reports/GenericReportIndexPage.razor` | 注入 `L`，報表集標題改用 `L[$"ReportCategory.{Category}"]` |
| `Components/Pages/Employees/PersonalPreference/LanguageRegionTab.razor` | 語言選單加入 Filipino 選項 |

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
        UILanguage.JaJP => "ja-JP",
        UILanguage.ZhCN => "zh-CN",
        UILanguage.FilPH => "fil",
        _ => "zh-TW"
    };

    public static readonly string[] SupportedCultures = ["zh-TW", "en-US", "ja-JP", "zh-CN", "fil"];
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

## NavMenu 多語系設計

### 機制：NameKey + IStringLocalizer

`NavigationItem` 有可選的 `NameKey` 屬性，NavMenu 在渲染時將其對應到 `SharedResource` 的 key：

```csharp
// Models/Navigation/NavigationItem.cs
public string? NameKey { get; set; }
```

```razor
@* Components/Layout/NavMenu.razor *@
@inject IStringLocalizer<SharedResource> L

Text="@(menuItem.NameKey != null ? L[menuItem.NameKey].ToString() : menuItem.Name)"
```

`NameKey` 為 `null` 時 fallback 到 `Name`（繁體中文），向下相容。搜尋功能繼續使用 `Name` 與 `SearchKeywords`，不受語言影響。

### Nav.* key 命名規則

| 類型 | 格式 | 範例 |
|------|------|------|
| 父層選單 | `Nav.XxxGroup` | `Nav.HumanResources`、`Nav.SupplierGroup` |
| 子頁面 | `Nav.Xxxs`（複數）| `Nav.Employees`、`Nav.Products` |
| Action 項目 | `Nav.XxxReportIndex` | `Nav.HRReportIndex`、`Nav.SalesReportIndex` |
| NavMenu UI | `Nav.SignIn`、`Nav.SignOut`、`Nav.PersonalData` 等 | — |

目前共 75 個 `Nav.*` key，涵蓋全部選單項目與 NavMenu UI 文字。

---

## 報表集多語系設計

### 機制：ReportDefinition.NameKey + DisplayFormatter

`ReportDefinition` 有可選的 `NameKey` 屬性（同 NavigationItem 模式）：

```csharp
// Models/Reports/ReportDefinition.cs
public string? NameKey { get; set; }
```

`GenericReportIndexModalComponent` 透過 `InteractiveColumnDefinition.DisplayFormatter` 取得本地化名稱：

```csharp
// Components/Shared/Report/GenericReportIndexModalComponent.razor
@inject IStringLocalizer<SharedResource> L

new InteractiveColumnDefinition
{
    PropertyName = "Name",
    Title = L["Report.NameColumn"],
    DisplayFormatter = (item) =>
    {
        var report = item as ReportDefinition;
        return report?.NameKey != null
            ? L[report.NameKey].ToString() ?? report.Name
            : report?.Name ?? "";
    }
}
```

報表集標題在 `GenericReportIndexPage.razor` 透過分類 key 取得：

```razor
@* Components/Pages/Reports/GenericReportIndexPage.razor *@
@inject IStringLocalizer<SharedResource> L

Title="@L[$"ReportCategory.{Category}"].ToString()"
```

搜尋同時比對原始中文 `Name` 與本地化名稱（雙語搜尋）：

```csharp
result = result.Where(r =>
    r.Name.ToLower().Contains(keyword) ||
    (r.NameKey != null && L[r.NameKey].ToString()!.ToLower().Contains(keyword)));
```

### Report.* key 命名規則

| 類型 | 格式 | 範例 |
|------|------|------|
| 報表集標題 | `ReportCategory.{Category}` | `ReportCategory.Customer`、`ReportCategory.HR` |
| 報表名稱 | `Report.{ActionId前綴}` | `Report.CustomerStatement`、`Report.TrialBalance` |
| 報表 UI 文字 | `Report.{動作}` | `Report.GoTo`、`Report.SearchPlaceholder`、`Report.NoReports` |

目前共 57 個 `Report.*` / `ReportCategory.*` key，涵蓋全部 38 份報表名稱、11 個分類標題及 8 個 UI 文字。

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
| `Button.Save` | 儲存 |
| `Employee.Name` | 姓名 |
| `Nav.Home` | 首頁 |
| `Report.CustomerStatement` | 客戶對帳單 |
| `ReportCategory.Customer` | 客戶報表集 |

> key 一律使用英文點記法（`Module.Property`），不因語言而異。未提供翻譯的 key 會直接顯示 key 名稱（`IStringLocalizer` 預設行為）。

---

## 新增語言 SOP

新增一種新語言時，需依序完成以下 4 步：

### 1. 加入 UILanguage 枚舉值

```csharp
// Data/Entities/Employees/EmployeePreference.cs
public enum UILanguage
{
    ZhTW = 1,
    EnUS = 2,
    JaJP = 3,
    ZhCN = 4,
    FilPH = 5,
    // 新增在此，值從 6 開始遞增
    XxXX = 6,  // [Display(Name = "顯示名稱")]
}
```

> **注意**：枚舉值以 int 儲存於 DB，加入新值不需 Migration。現有資料不受影響。

### 2. 更新 CultureHelper

```csharp
// Helpers/CultureHelper.cs
UILanguage.XxXX => "xx-XX",    // 加入對應 culture code

public static readonly string[] SupportedCultures = [..., "xx-XX"];  // 加入陣列
```

### 3. 建立 .resx 翻譯檔

新建 `Resources/SharedResource.xx-XX.resx`，包含與 `SharedResource.resx` 相同的所有 key，值替換為目標語言。

**⚠️ resx XML 注意事項**（避免 MSB3103 build error）：
- 所有值中的 `&` 必須寫成 `&amp;`（例如 `Language &amp; Region`）
- 同理：`<` → `&lt;`，`>` → `&gt;`
- 若用 Node.js 腳本生成 resx，值一律使用 `\uXXXX` Unicode 轉義表示非 ASCII 字元，避免 buffer/parse 問題
- **生成後必須驗證**：確認值是目標語言（而非英文 placeholder）

### 4. 更新語言選單 UI

```csharp
// Components/Pages/Employees/PersonalPreference/LanguageRegionTab.razor
// OnInitialized() 中的 Options 清單加入新選項：
new() { Value = "6", Text = "顯示名稱" },
// Value 對應 UILanguage 枚舉的 int 值（強型別），Text 固定顯示（不走 L[]）
```

> **注意**：`Value` 必須與 UILanguage 枚舉的整數值完全對應。

---

## ⚠️ 已知問題與注意事項

### 1. resx XML 實體字元（MSB3103）

**問題**：建置報錯 `MSB3103: Invalid Resx file. System.Xml.XmlException: An error occurred while parsing EntityName`

**原因**：resx 是 XML 格式，值中若直接含 `&` 字元（如 `Language & Region`）會導致 XML 解析失敗。

**解法**：所有 resx 值中的 `&` 一律改寫為 `&amp;`。

```xml
<!-- ❌ 錯誤 -->
<value>Language & Region</value>

<!-- ✅ 正確 -->
<value>Language &amp; Region</value>
```

### 2. 腳本生成 resx 需驗證目標語言

**問題**：以 Node.js 腳本批次生成 resx 時，若腳本邏輯有誤，可能將英文 placeholder 寫入目標語言的 resx，導致 UI 顯示英文而非目標語言。

**實際案例**：`SharedResource.ja-JP.resx` 初次生成時，`Button.*`、`Label.*`、`Employee.*` 等 key 全部帶英文值（如 `Save`、`Name`），僅 `Preference.LanguageRegion` 例外。已於 2026-02-25 修正 59 個 key。

**預防方式**：
- 生成後抽查幾個 key，確認值是目標語言文字
- `zh-CN.resx` 和 `fil.resx` 可作為參考（生成正確）

### 3. LanguageRegionTab 語言選單需手動維護

**問題**：`LanguageRegionTab.razor` 的語言下拉選單是**硬編碼**的 `SelectOption` 清單，新增語言後必須手動加入對應選項。

**目前狀態**（5 種語言，已完整）：
```csharp
new() { Value = "1", Text = "繁體中文" },
new() { Value = "2", Text = "English" },
new() { Value = "3", Text = "日本語" },
new() { Value = "4", Text = "简体中文" },
new() { Value = "5", Text = "Filipino" },
```

**未來建議**：可改為從 `UILanguage` 枚舉動態產生選項（`Enum.GetValues` + `Display` Attribute）。

### 4. NavMenu 搜尋不支援多語系

**問題**：搜尋功能（CommandBar 等）使用 `NavigationItem.Name`（繁體中文）與 `SearchKeywords`，搜尋結果名稱不隨使用者語言變更。

**現況**：可接受，可在 `NavigationConfig.cs` 各項目的 `SearchKeywords` 中加入多語言詞彙補充。

---

## 遷移策略

| 階段 | 工作 | 狀態 |
|------|------|------|
| Phase 1 | 建立基礎設施（`AddLocalization`、`CultureHelper`、`Resources/`、JS） | ✅ 完成 |
| Phase 2 | 登入後寫入 cookie + 切換時 reload | ✅ 完成 |
| Phase 3 | PersonalPreference 元件字串遷移 | ✅ 完成 |
| Phase 4a | NavMenu + NavigationConfig 翻譯（75 個 `Nav.*` key，5 種語言） | ✅ 完成 |
| Phase 4b | `LanguageRegionTab.razor` 加入 Filipino 選項 | ✅ 完成 |
| Phase 4c | `ReportRegistry` + `GenericReportIndexModalComponent` 翻譯（57 個 `Report.*` key） | ✅ 完成 |
| Phase 5 | 其他頁面逐一遷移 | 待推進 |

### Phase 5 新頁面翻譯流程

新增頁面翻譯時：
1. 在所有語言的 `.resx` 加入新 key（命名格式：`模組.欄位`）
2. 在目標元件注入 `@inject IStringLocalizer<SharedResource> L`
3. 將硬編碼中文字串改為 `@L["Key"]`

> 翻譯範圍建議優先順序：高使用頻率的共用元件（`GenericIndexPageComponent`、`GenericFormComponent`）→ 各業務模組頁面。

---

## 相關文件

- [README_個人化設定總綱.md](README_個人化設定總綱.md)
- [README_個人化設定_資料服務層.md](README_個人化設定_資料服務層.md)
- [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md)
