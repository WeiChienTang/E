# 個人化設定 — 語言切換設計

## 更新日期
2026-02-27

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
| `Components/Shared/Modal/GenericEditModalComponent.razor` | 注入 L，14 個硬編碼中文字串（按鈕文字、Modal 標題、確認訊息）改用資源 key |
| `Components/Shared/Page/GenericIndexPageComponent.razor` | 注入 L，模組停用訊息、載入文字、欄位標題預設值改用資源 key；`ActionsHeader` 等參數預設改 `""` 加 L[] fallback |
| `Components/Shared/Table/InteractiveTableComponent.razor` | 注入 L，選取 checkbox 提示文字、動作欄標題預設改用資源 key；`ActionsHeader`/`DeleteButtonTitle` 參數預設改 `""` 加 L[] fallback |

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

## 共用元件多語系設計（Phase 5a）

### 新增資源 key（18 個，5 種語言）

| 分類 | Key | zh-TW | en-US |
|------|-----|-------|-------|
| Button | `Button.Approve` | 通過 | Approve |
| Button | `Button.Reject` | 駁回 | Reject |
| Button | `Button.Prev` | 上筆 | Previous |
| Button | `Button.Next` | 下筆 | Next |
| Button | `Button.Refresh` | 重整 | Refresh |
| Button | `Button.Preview` | 預覽 | Preview |
| Button | `Button.ReturnToLast` | 返回編輯 | Return to Last |
| Label | `Label.Actions` | 操作 | Actions |
| Label | `Label.Remarks` | 備註 | Remarks |
| Label | `Label.CreatedAt` | 建立日期 | Created Date |
| Label | `Label.SelectAll` | 全選/取消全選 | Select All / Deselect All |
| Label | `Label.SelectItem` | 選取此項目 | Select this item |
| Label | `Label.KeepOneEmptyRow` | 至少需要保留一個空行 | At least one empty row must be kept |
| Modal | `Modal.ConfirmReject` | 確認駁回 | Confirm Reject |
| Modal | `Modal.ConfirmClose` | 確認關閉 | Confirm Close |
| Modal | `Modal.ConfirmLeave` | 確定離開 | Confirm Leave |
| Modal | `Modal.SaveAndClose` | 儲存關閉 | Save &amp; Close |
| Modal | `Modal.ContinueEditing` | 繼續編輯 | Continue Editing |
| Modal | `Modal.UnsavedChanges` | 此單有修改，請選擇操作 | This record has changes. Please select an action. |
| Module | `Module.NotAvailable` | 此功能未開放 | This feature is not available |
| Module | `Module.DisabledMessage` | 您的系統目前未啟用此功能模組，請聯絡系統管理員。 | This module is not enabled in your system. Please contact your system administrator. |
| Placeholder | `Placeholder.Remarks` | 輸入備註關鍵字... | Enter remarks keyword... |

### [Parameter] 預設值本地化模式

Blazor 元件的 `[Parameter]` 屬性無法在宣告時使用 `L[]`（DI 尚未注入），解決方式：

```csharp
// ✅ 正確：預設改為 ""，在使用處加 L[] fallback
[Parameter] public string ActionsHeader { get; set; } = "";
[Parameter] public string DeleteButtonTitle { get; set; } = "";

// 使用處（OnInitialized 或 template 中）
Title = string.IsNullOrEmpty(ActionsHeader) ? L["Label.Actions"] : ActionsHeader,

// Razor template 中（需 .ToString()，因 LocalizedHtmlString 不隱式轉 string）
@(string.IsNullOrEmpty(ActionsHeader) ? L["Label.Actions"].ToString() : ActionsHeader)
```

**向下相容**：呼叫端若傳入明確值仍正常運作；若不傳入則自動取得本地化預設值。

### GenericEditModalComponent 替換清單

| 原始硬編碼 | 替換後 key |
|-----------|-----------|
| `通過` | `Button.Approve` |
| `駁回` | `Button.Reject` |
| `新增` | `Button.Add` |
| `上筆` | `Button.Prev` |
| `下筆` | `Button.Next` |
| `重整` | `Button.Refresh` |
| `刪除` | `Button.Delete` |
| `確認駁回` | `Modal.ConfirmReject` |
| `確認關閉` | `Modal.ConfirmClose` |
| `確定離開` | `Modal.ConfirmLeave` |
| `儲存關閉` | `Modal.SaveAndClose` |
| `繼續編輯` | `Modal.ContinueEditing` |
| `此單有修改，請選擇操作` | `Modal.UnsavedChanges` |
| `預覽` | `Button.Preview` |

---

## 業務模組多語系設計（Phase 5b）

### 新增資源 key（5 種語言）

#### Label / Message 格式字串

| Key | zh-TW | en-US | 說明 |
|-----|-------|-------|------|
| `Label.DocumentPreview` | `{0}預覽` | `{0} Preview` | 報表預覽標題 |
| `Message.EditTitle` | `編輯{0}` | `Edit {0}` | Modal 標題（編輯模式） |
| `Message.CreateTitle` | `新增{0}` | `Create {0}` | Modal 標題（新增模式） |
| `Message.UpdateSuccess` | `{0}更新成功` | `{0} updated successfully` | 儲存成功通知 |
| `Message.CreateSuccess` | `{0}新增成功` | `{0} created successfully` | 新增成功通知 |
| `Message.SaveFailure` | `{0}儲存失敗` | `Failed to save {0}` | 儲存失敗通知 |
| `Message.ViewReadOnly` | `檢視{0} (唯讀)` | `View {0} (Read-only)` | 唯讀檢視標題 |

格式字串搭配 `Entity.*` key 組合，例如：

```razor
ModalTitle="@(Id.HasValue ? L["Message.EditTitle", L["Entity.PurchaseOrder"]].ToString()
                          : L["Message.CreateTitle", L["Entity.PurchaseOrder"]].ToString())"
```

#### Entity.* 實體名稱 key（55 個）

| Key | zh-TW | en-US |
|-----|-------|-------|
| `Entity.PurchaseOrder` | 採購單 | Purchase Order |
| `Entity.PurchaseReceiving` | 進貨 | Goods Receipt |
| `Entity.PurchaseReceivingDoc` | 進貨單 | Goods Receipt Note |
| `Entity.PurchaseReturn` | 進貨退出 | Purchase Return |
| `Entity.PurchaseReturnDoc` | 進貨退出單 | Purchase Return Note |
| `Entity.PurchaseReturnReason` | 退出原因 | Return Reason |
| `Entity.Quotation` | 報價單 | Quotation |
| `Entity.SalesOrder` | 訂單 | Sales Order |
| `Entity.SalesDelivery` | 出貨單 | Delivery Note |
| `Entity.SalesDeliveryDoc` | 銷貨單 | Sales Delivery Note |
| `Entity.SalesReturn` | 銷售退貨 | Sales Return |
| `Entity.SalesReturnDoc` | 退貨單 | Return Note |
| `Entity.SalesReturnReason` | 退貨原因 | Return Reason |
| `Entity.Customer` | 客戶 | Customer |
| `Entity.Supplier` | 廠商 | Supplier |
| `Entity.Employee` | 員工 | Employee |
| `Entity.Department` | 部門 | Department |
| `Entity.Warehouse` | 倉庫 | Warehouse |
| ... | （其餘約 36 個，涵蓋 Accounting、Products、Vehicles 等模組） | |

> **Entity 雙 key 規則**：部分實體的 EditModal 與 Index 頁面使用不同名稱時，分別建立兩個 key。例如 `Entity.PurchaseReceiving`（進貨，EditModal 用）與 `Entity.PurchaseReceivingDoc`（進貨單，Index 的 EntityName 用）。

#### Button.* 業務操作按鈕 key（25 個）

| Key | zh-TW | en-US |
|-----|-------|-------|
| `Button.ConvertToReceiving` | 轉進貨 | Convert to Receiving |
| `Button.ConvertToReturn` | 轉退出 | Convert to Return |
| `Button.ConvertToSalesReturn` | 轉退貨 | Convert to Sales Return |
| `Button.ConvertToSetoff` | 轉沖款 | Convert to Setoff |
| `Button.ConvertToDelivery` | 轉銷貨 | Convert to Delivery |
| `Button.ConvertToOrder` | 轉訂單 | Convert to Order |
| `Button.ConvertToAdjustment` | 轉調整 | Convert to Adjustment |
| `Button.CopyMessage` | 複製訊息 | Copy Message |
| `Button.CheckInventory` | 查庫存 | Check Inventory |
| `Button.ScheduleManagement` | 排程管理 | Schedule Management |
| `Button.CopyToSupplier` | 複製至廠商 | Copy to Supplier |
| `Button.CopyToCustomer` | 複製至客戶 | Copy to Customer |
| `Button.AccountManagement` | 帳號管理 | Account Management |
| `Button.ViewBOM` | 查看清單 | View BOM |
| `Button.CreateBOM` | 新增清單 | Create BOM |
| `Button.Post` | 過帳 | Post |
| `Button.WriteOff` | 沖銷 | Write Off |
| `Button.Reselect` | 重選 | Reselect |
| `Button.LoadData` | 載入 | Load |
| `Button.ClearData` | 清除 | Clear |
| `Button.ContinueAdd` | 繼續新增 | Continue Adding |
| `Button.CreateSubAccount` | 建立子科目 | Create Sub-Account |
| `Button.ComponentIssue` | 組件領料 | Component Issue |
| `Button.StartProduction` | 開始生產 | Start Production |
| `Button.ProductionComplete` | 完工登錄 | Production Complete |

### EditModal 元件更新模式

每個 EditModal 元件的標準更新步驟：

```razor
@* 1. 加入 L 注入（在最後一個 @inject 之後）*@
@inject IStringLocalizer<SharedResource> L

@* 2. 替換 GenericEditModalComponent 的硬編碼參數 *@
<GenericEditModalComponent ...
    EntityName="@L["Entity.XxxYyy"]"
    EntityNamePlural="@L["Entity.XxxYyy"]"
    ModalTitle="@(Id.HasValue ? L["Message.EditTitle", L["Entity.XxxYyy"]].ToString()
                              : L["Message.CreateTitle", L["Entity.XxxYyy"]].ToString())"
    SaveSuccessMessage="@(Id.HasValue ? L["Message.UpdateSuccess", L["Entity.XxxYyy"]].ToString()
                                      : L["Message.CreateSuccess", L["Entity.XxxYyy"]].ToString())"
    SaveFailureMessage="@L["Message.SaveFailure", L["Entity.XxxYyy"]]"
    PrintButtonText="@L["Button.Print"]"
    ReportPreviewTitle="@L["Label.DocumentPreview", L["Entity.XxxYyy"]]" />

@* 3. 替換自訂操作按鈕文字 *@
<GenericButtonComponent Text="@L["Button.ConvertToReceiving"]" ... />
```

Index 頁面只需加入 L 注入並替換 `EntityName` 參數（部分有多處，用 `replace_all`）。

### Phase 5b 模組完成進度

| 模組 | EditModal（檔數）| Index（檔數）| 狀態 |
|------|----------------|-------------|------|
| Purchase | 4（PurchaseOrder、Receiving、Return、ReturnReason） | 4 | ✅ 完成 |
| Sales | 5（Quotation、SalesOrder、Delivery、Return、ReturnReason） | 5 | ✅ 完成 |
| Customers | 2（Customer、CustomerVisit） | 2 | ✅ 完成 |
| Suppliers | 1（Supplier） | 1 | ✅ 完成 |
| Products | 2（Product、ProductComposition） | 1 | ✅ 完成 |
| Employees | 1（Employee，含 Button.AccountManagement） | 1 | ✅ 完成 |
| Warehouse | 3（InventoryStock、MaterialIssue、StockTaking） | 3 | ✅ 完成 |
| FinancialManagement | 1（SetoffDocument，ReportPreviewTitle 保留動態方法） | 0 | ✅ 完成 |
| Accounting | 1（AccountItem） | 1 | ✅ 完成 |
| **合計** | **20 個 EditModal** | **18 個 Index** | ✅ **Phase 5b 完成** |

> **新增 Entity key（Phase 5b 本輪）**：`Entity.BOM`（物料清單）、`Entity.StockTakingReport`（盤點差異表）— 5 種語言均已更新。
>
> **特殊處理**：`StockTakingEditModalComponent` 的 `ModalTitle` 使用 `@GetModalTitle()` 方法（多種盤點狀態，非單一格式），不予替換。`SetoffDocumentEditModalComponent` 的 `ReportPreviewTitle` 使用 `@GetReportPreviewTitle()` 方法（動態報表類型），不予替換。

---

## 剩餘模組多語系設計（Phase 5c）

### 新增資源 key（Phase 5c）

#### Entity.* 補充 key（4 個）

| Key | zh-TW | en-US | 說明 |
|-----|-------|-------|------|
| `Entity.ProductionScheduleReport` | 生產排程表 | Production Schedule Report | 生產排程表預覽用 |
| `Entity.EmployeeTrainingRecord` | 受訓紀錄 | Training Record | 員工受訓紀錄 |
| `Entity.EmployeeLicense` | 證照紀錄 | License Record | 員工證照 |
| `Entity.EmployeeTool` | 工具配給紀錄 | Tool Assignment Record | 員工工具配給 |

> 注意：`Entity.ProductionScheduleReport` 用於「生產排程表預覽」標題，區別於 `Entity.ProductionSchedule`（生產排程）用於 EntityName。

#### Label.* 標題 key（1 個）

| Key | zh-TW | en-US | 說明 |
|-----|-------|-------|------|
| `Label.PurchaseOrderMessageSetting` | 採購單訊息設定 | Purchase Order Message Settings | TextMessageTemplateEditModal 標題 |

#### Button.* 系統操作按鈕 key（4 個）

| Key | zh-TW | en-US | 說明 |
|-----|-------|-------|------|
| `Button.QueryPrinter` | 查詢印表機 | Query Printer | PrinterConfigurationEditModal |
| `Button.TestPrint` | 測試列印 | Test Print | PrinterConfigurationEditModal |
| `Button.PreviewLogo` | 預覽 LOGO | Preview LOGO | CompanyEditModal |
| `Button.QueryPaper` | 查詢紙張 | Query Paper | PaperSettingEditModal |

### Phase 5c 完成模組

| 模組 | EditModal（檔數）| Index（檔數）| 說明 |
|------|----------------|-------------|------|
| Accounting | 1（JournalEntry） | 1 | JournalEntry 有 SaveHandler，SaveSuccessMessage 仍使用 L |
| WasteManagement | 2（WasteType、WasteRecord） | 2 | WasteRecord 含 PrintButton/ReportPreviewTitle |
| Employees | 5（Role、Department、Permission、EmployeePosition、+3 EmployeeEditModal sub-modals） | 4 | EmployeeTrainingRecord、EmployeeLicense、EmployeeTool 需新增 Entity key |
| Warehouse | 3（InventoryTransaction、Warehouse、WarehouseLocation） | 3 | InventoryTransaction 為唯讀，ModalTitle 用 `Message.ViewReadOnly` |
| FinancialManagement | 3（Currency、PaymentMethod、Bank） | 4（含 SetoffDocumentIndex） | |
| Products | 6（Size、ProductCategory、CompositionCategory、Unit、ProductionSchedule、VehicleType→移到 Vehicles） | 7 | ProductionSchedule 含 PrintButton，ReportPreviewTitle 用 Entity.ProductionScheduleReport |
| Vehicles | 3（Vehicle、VehicleMaintenance、VehicleType） | 3 | Vehicle 和 VehicleMaintenance 含 PrintButton/ReportPreviewTitle |
| Systems | 5（ReportPrintConfiguration、TextMessageTemplate、PrinterConfiguration、Company、PaperSetting） | 5 | 4 個含自訂按鈕；TextMessageTemplate 的 ModalTitle 使用 `Label.PurchaseOrderMessageSetting` |
| 其他（ErrorLog、Material 等） | — | 4（ErrorLog、MaterialIndex 等） | 僅 Index 頁面 |
| **合計** | **~30 個 EditModal** | **29 個 Index** | ✅ **Phase 5c 完成** |

> **特殊處理**：
> - `InventoryTransactionEditModalComponent` ModalTitle 使用 `Message.ViewReadOnly`（唯讀檢視），無 SaveSuccessMessage/SaveFailureMessage
> - `TextMessageTemplateEditModalComponent` 的 `ModalTitle` 使用 `Label.PurchaseOrderMessageSetting` key（固定設定頁面標題，非 CRUD 模式，5 種語言均已翻譯）
> - `ProductionScheduleEditModal` 的 `ReportPreviewTitle` 使用 `Entity.ProductionScheduleReport`（「生產排程表」），區別於 EntityName 的 `Entity.ProductionSchedule`（「生產排程」）

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
| Phase 5a | 高頻共用元件翻譯（`GenericEditModalComponent`、`GenericIndexPageComponent`、`InteractiveTableComponent`）— 新增 18+ 個 `Button.*`、`Label.*`、`Modal.*`、`Module.*`、`Placeholder.*` key | ✅ 完成 |
| Phase 5b | 各業務模組頁面逐一遷移（9 個模組、30 檔） | ✅ 完成 |
| Phase 5c | 剩餘所有模組（Accounting、Warehouse、FinancialManagement、Employees、Products、Vehicles、WasteManagement、Systems）— ~30 EditModal + 29 Index | ✅ 完成 |
| Phase 6a | 通用欄位資源 key（7 個 `Placeholder.*` 格式字串 + 27 個泛用 `Field.*`） | ✅ 完成 |
| Phase 6b | 採購 / 銷售模組表單欄位（22 個 `Field.*` + 24 個 `Column.*`，更新 16 個元件） | ✅ 完成 |
| Phase 6c～d | 客戶 / 廠商 / 員工模組表單欄位（56 個 `Field.*`，更新 11 個元件） | ✅ 完成 |
| Phase 6e～f | 商品 / 倉庫 / 會計 / 車輛 / 財務 / 系統模組表單欄位（~174 個 `Field.*`、`Column.*`，更新 25 個元件） | ✅ 完成 |
| Phase 6g | WasteManagement 模組 + 系統參數設定（37 個 key，更新 6 個元件）— 所有表單欄位翻譯完成 | ✅ 完成 |

### 新頁面翻譯流程（持續維護）

新增頁面或元件時，翻譯步驟：
1. 在所有 5 個語言的 `.resx` 加入新 `Entity.*` key（若實體尚未有對應 key）
2. 在目標元件注入 `@inject IStringLocalizer<SharedResource> L`
3. 依標準模式替換 `EntityName`、`ModalTitle`、`SaveSuccessMessage`、`SaveFailureMessage`、`PrintButtonText`、`ReportPreviewTitle`
4. 若有自訂操作按鈕（`Text="..."` 硬編碼），先加入 `Button.*` key 再替換
5. 將 `FormFieldDefinition` 中的 `Label`、`Placeholder` 替換為 `L["Field.*"]` / `L["Placeholder.*"]`；優先複用已有的 `Entity.*` key（當標籤與實體名稱完全相同時）
6. Table 元件的欄位標題改用 `L["Column.*"]`，同時確認已注入 L（Table 元件不自動帶入）

> **Phase 5a～5c + Phase 6a～6g 已完成**：所有既有業務模組、系統模組、共用元件的硬編碼中文字串均已遷移至資源 key，含 GenericEditModal 參數、按鈕文字、表單欄位標籤、表格欄位標題。後續僅需對新增頁面套用此流程。

---

## 表單欄位多語系設計（Phase 6）

### 翻譯策略

Phase 6 聚焦於所有 `FormFieldDefinition` 的 `Label` 與 `Placeholder` 欄位，以及 Table 元件的欄位標題（`InteractiveColumnDefinition.Title`）。

**不翻譯** `HelpText`（說明文字，內容較長、使用頻率低）。

### key 命名規則擴充

| 前綴 | 用途 | 範例 |
|------|------|------|
| `Field.*` | 表單欄位標籤 | `Field.PurchaseOrderCode`、`Field.TaxRate` |
| `Column.*` | 表格欄位標題 | `Column.Quantity`、`Column.UnitPrice` |
| `Placeholder.*` | 輸入提示（格式字串或特定提示）| `Placeholder.PleaseInput`、`Placeholder.AutoGenerated` |
| `Section.*` | 表單區段標題 | `Section.TaxSettings`、`Section.FeeInfo` |
| `Option.*` | 下拉選項文字 | `Option.Sequential`、`Option.EntityCode` |
| `Tab.*` | Tab 標籤文字 | `Tab.SubAccountSettings`、`Tab.ModuleManagement` |

### Placeholder 格式字串模式

減少 Placeholder 重複 key 的方式：使用 3 個格式字串搭配欄位名稱組合。

```csharp
// 格式字串（使用 IStringLocalizer 的 params object[] 索引器）
// Placeholder.PleaseInput  = "請輸入{0}"
// Placeholder.PleaseSelect = "請選擇{0}"
// Placeholder.PleaseInputOrSelect = "請輸入或選擇{0}"

Placeholder = L["Placeholder.PleaseInput", L["Field.PurchaseOrderCode"]]
// → "請輸入採購單號"（zh-TW）/ "Please enter Purchase Order Code"（en-US）
```

非格式字串情境使用獨立 key，例如 `Placeholder.AutoGenerated`（系統自動產生）、`Placeholder.SelectWarehouseFirst`（請先選擇倉庫）。

### Entity.* 複用規則

當欄位標籤與實體名稱完全相同時，直接複用 `Entity.*` key，不建立重複 `Field.*`：

```csharp
Label = L["Entity.Customer"]   // 而非新建 Field.Customer
Label = L["Entity.WasteType"]  // 而非新建 Field.WasteType
Label = L["Entity.Warehouse"]  // 而非新建 Field.Warehouse
```

### readonly 欄位定義重構模式

Blazor 元件的 `[Parameter]` 屬性無法在欄位宣告時使用 `L[]`（DI 尚未注入）。若原本使用 `private readonly List<FormFieldDefinition> allFields = new() { ... }` 的內聯初始化，需改為 `OnInitialized()` 模式：

```csharp
// ❌ 原本（無法使用 L[]）
private readonly List<FormFieldDefinition> allFields = new()
{
    new FormFieldDefinition { Label = "稅率 (%)", ... }
};
private readonly Dictionary<string, string> fieldSections = new()
{
    { "TaxRate", "稅務設定" }
};

// ✅ 改為（OnInitialized 後 L 已注入）
private List<FormFieldDefinition> allFields = new();
private Dictionary<string, string> fieldSections = new();

protected override void OnInitialized()
{
    var taxSection = L["Section.TaxSettings"].ToString();
    allFields = new List<FormFieldDefinition>
    {
        new FormFieldDefinition { Label = L["Field.TaxRatePercent"], ... }
    };
    fieldSections = new Dictionary<string, string>
    {
        { "TaxRate", taxSection }
    };
}
```

此模式同時解決 fieldSections 區段名稱的本地化（區段名稱變數在 `OnInitialized` 中計算，再同時用於 `allFields` 和 `fieldSections`）。

### Phase 6 完成進度

| 子階段 | 模組 | 新增 key | 更新元件 |
|--------|------|---------|---------|
| 6a | 通用（Placeholder 格式字串、泛用 Field.*） | 34 | — |
| 6b | Purchase + Sales | 46 | 16（9 EditModal + 7 Table） |
| 6c | Customer + Supplier + CustomerVisit | 18 | 3 EditModal |
| 6d | Employee（含 sub-modals） | 39 | 8 EditModal |
| 6e | Product + Warehouse | 67 | 11（8 EditModal + 3 Table） |
| 6f | Accounting + Vehicle + Financial + Systems | 107 | 14（12 EditModal + 2 Table） |
| 6g | WasteManagement + 系統參數設定 Tab | 37 | 6 |
| **合計** | **全模組** | **~348 個新 key** | **~58 個元件** |

> **注意**：Phase 6e、6f 同時執行導致部分 `Column.*` key 重複插入，已事後清除。最終 `SharedResource.resx` 共 680 筆 data 元素（Phase 6 前為 643 筆）。

### Phase 6 新增 key 類別摘要

| 類別 | key 數量 | 說明 |
|------|---------|------|
| `Field.*` | ~310 | 表單欄位標籤（涵蓋所有模組） |
| `Column.*` | ~24 | 表格欄位標題 |
| `Placeholder.*` | 10 | 格式字串（3 個）+ 特定提示（7 個） |
| `Section.*` | 4 | TaxSettings、AutoGenerateSetting、ControlAccountCode、FeeInfo |
| `Option.*` | 2 | Sequential、EntityCode |
| `Tab.*` | 2 | SubAccountSettings、ModuleManagement |
| `Button.*` | 2 | Confirm、ResetToDefault |

---

## 相關文件

- [README_個人化設定總綱.md](README_個人化設定總綱.md)
- [README_個人化設定_資料服務層.md](README_個人化設定_資料服務層.md)
- [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md)
