# 個人化設定 — 顯示設定（字型縮放）

## 更新日期
2026-02-28

---

## 功能概述

讓每位使用者可以獨立調整介面字型大小，效果類似瀏覽器的 Ctrl+滾輪縮放，但僅影響此專案的介面，不影響瀏覽器本身的 Chrome。

**特點**：
- 儲存後**即時套用**，不需要整頁 reload（與語言切換不同）
- 共 6 個縮放級別：75% / 90% / 100%（預設）/ 110% / 125% / 150%
- 偏好存入 DB（`EmployeePreference.Zoom`），跨裝置同步
- 同一台電腦切換帳號時，MainLayout 會從 DB 強制修正為正確用戶的設定

---

## 套用範圍

縮放套用在 `html` 根元素，透過 CSS 的 `font-size` + `rem` 單位系統讓全站所有元素等比縮放，包含：

- 側邊欄（Sidebar / NavMenu）
- 主要內容區
- Modal 視窗
- 表格、按鈕、表單欄位

---

## 四層架構

```
[DB] EmployeePreference.Zoom (ContentZoom enum)   ← 使用者偏好持久化
    ↓ 儲存時 / 登入時
[Cookie] ERPCore2.ContentZoom=1.1rem              ← 跨請求快取（無用戶識別）
    ↓ 頁面載入時
[CSS] html { font-size: var(--content-zoom, 1rem) } ← CSS variable 控制全站縮放
    ↓ 由 JS 動態寫入
[App.razor inline script] / [MainLayout 修正]      ← 確保正確值在第一幀即生效
```

---

## ContentZoom Enum

```csharp
// Data/Entities/Employees/EmployeePreference.cs
public enum ContentZoom
{
    XSmall  = 1,   // 75%
    Small   = 2,   // 90%
    Medium  = 3,   // 100%（預設）
    Large   = 4,   // 110%
    XLarge  = 5,   // 125%
    XXLarge = 6    // 150%
}
```

Enum 整數值對應的 CSS rem 字串：

| Enum 值 | int | CSS rem |
|---------|-----|---------|
| XSmall  | 1 | `0.75rem` |
| Small   | 2 | `0.9rem`  |
| Medium  | 3 | `1rem`    |
| Large   | 4 | `1.1rem`  |
| XLarge  | 5 | `1.25rem` |
| XXLarge | 6 | `1.5rem`  |

轉換邏輯集中在 `content-zoom-helper.js` 的 `contentZoomMap` 物件，C# 層只傳遞 enum 整數值。

---

## 相關檔案

| 檔案 | 說明 |
|------|------|
| `Data/Entities/Employees/EmployeePreference.cs` | `ContentZoom` enum 定義 + `Zoom` 欄位 |
| `Services/Employees/EmployeePreferenceService.cs` | `SavePreferenceAsync` else 區塊加入 `existing.Zoom = preference.Zoom` |
| `wwwroot/css/app.css` | `html { font-size: var(--content-zoom, 1rem); }` |
| `wwwroot/js/content-zoom-helper.js` | `setContentZoom(enumValue)` — enum 轉 rem、寫 cookie、更新 CSS variable |
| `Components/App.razor` | `<head>` 內 inline script（防止 flash）+ `<body>` 內引入 JS 檔 |
| `Components/Layout/MainLayout.razor` | `OnAfterRenderAsync(firstRender)` 強制套用當前用戶的縮放（跨用戶修正） |
| `Components/Pages/Employees/PersonalPreference/DisplayTab.razor` | 顯示設定 Tab UI（視覺化按鈕組） |
| `Components/Pages/Employees/PersonalPreference/DisplayTab.razor.css` | Tab scoped 樣式 |
| `Components/Pages/Employees/PersonalPreference/PersonalPreferenceModalComponent.razor` | 新增第三個 Tab + HandleSave 呼叫 JS + `_originalZoom` 記錄 |

---

## CSS 設計

```css
/* wwwroot/css/app.css */
html {
    font-size: var(--content-zoom, 1rem); /* 字型縮放：由 content-zoom-helper.js 動態設定 */
}
```

預設值 `1rem` = 瀏覽器基礎字型大小（通常為 16px），不設定時與原本行為完全一致。所有使用 `rem` 單位的元素（Bootstrap 元件、自訂 CSS）均會自動跟著縮放。

---

## JavaScript 設計

```javascript
// wwwroot/js/content-zoom-helper.js

var contentZoomMap = {
    1: '0.75rem',   // XSmall
    2: '0.9rem',    // Small
    3: '1rem',      // Medium
    4: '1.1rem',    // Large
    5: '1.25rem',   // XLarge
    6: '1.5rem'     // XXLarge
};

window.setContentZoom = function (enumValue) {
    var zoomRem = contentZoomMap[enumValue] || '1rem';
    document.cookie = 'ERPCore2.ContentZoom=' + zoomRem + ';path=/;max-age=31536000';
    document.documentElement.style.setProperty('--content-zoom', zoomRem);
};
```

呼叫時機：
1. **儲存設定**：`PersonalPreferenceModalComponent.HandleSave()` 在 Zoom 有變更時呼叫
2. **登入後修正**：`MainLayout.OnAfterRenderAsync(firstRender)` 強制呼叫

---

## 防止 Flash（App.razor inline script）

頁面首次載入時，Blazor circuit 尚未連線（SSR 階段），無法呼叫 C# 讀取 DB。為避免頁面從預設大小（100%）閃現到用戶設定值，在 `<head>` 加入同步執行的 inline script：

```html
<!-- Components/App.razor — <head> 內 -->
@* 字型縮放：在 CSS 載入後立即套用，避免頁面 flash *@
<script>
    (function () {
        var match = document.cookie.match(/ERPCore2\.ContentZoom=([^;]+)/);
        if (match) document.documentElement.style.setProperty('--content-zoom', decodeURIComponent(match[1]));
    })();
</script>
```

此 script 在瀏覽器解析 HTML 時**同步執行**，讀取 cookie 中的 rem 字串並立即套用 CSS variable，在任何元素渲染前就完成，因此用戶看不到閃爍。

> ⚠️ 此 script 讀取的是上次儲存的 cookie 值，**不區分用戶**。跨用戶的正確修正由下方的 MainLayout 機制負責。

---

## 跨用戶 Cookie 修正（MainLayout）

### 問題

Cookie 是瀏覽器層級、不區分登入帳號的。若用戶 A（150%）登出後，用戶 B（100%）在同一台電腦登入，App.razor 的 inline script 讀到的仍是用戶 A 留下的 `150%` cookie，導致用戶 B 看到錯誤大小。

### 解決機制

`MainLayout.OnAfterRenderAsync(firstRender)` 在 circuit 連線後（可呼叫 JS 的最早時機）從 DB 讀取當前登入用戶的正確偏好，並強制覆寫：

```csharp
// Components/Layout/MainLayout.razor
@inject IEmployeePreferenceService EmployeePreferenceService

// OnInitializedAsync 儲存 ID（重用已有的 DB 查詢，不重複查詢）
var employeeId = await NavigationPermissionService.GetCurrentEmployeeIdAsync();
if (employeeId > 0)
{
    _currentEmployeeId = employeeId;
    _ = await NavigationPermissionService.GetAllEmployeePermissionsAsync(employeeId);
}

// OnAfterRenderAsync(firstRender) 強制套用
if (_currentEmployeeId > 0)
{
    var preference = await EmployeePreferenceService.GetByEmployeeIdAsync(_currentEmployeeId);
    await JSRuntime.InvokeVoidAsync("setContentZoom", (int)preference.Zoom);
}
```

`setContentZoom` 同時更新 CSS variable 與 cookie，確保下次頁面載入時 inline script 讀到的也是正確值。

### 時序說明

```
瀏覽器請求頁面
  └── App.razor 渲染 (SSR)
      └── inline script 同步執行 → 讀 cookie → 套用（可能是上一個用戶的值）
  └── HTML 回傳、瀏覽器渲染
      └── blazor.web.js 載入 → Circuit 建立
          └── MainLayout.OnInitializedAsync → 取得 employeeId
          └── MainLayout 渲染
          └── MainLayout.OnAfterRenderAsync(firstRender)
              └── 從 DB 載入 preference
              └── setContentZoom(正確值) → 覆寫 CSS variable + cookie ✅
```

對於「同一用戶」場景，inline script 的值與 DB 值一致，`setContentZoom` 是冪等操作，不會造成視覺變化。

> ⚠️ **禁止移除 MainLayout 的縮放套用邏輯**：移除後，同一台電腦切換帳號將導致縮放設定被前一用戶污染。

---

## DisplayTab 元件設計

`DisplayTab.razor` 不使用 `GenericFormComponent`（視覺化按鈕組無法用欄位定義驅動），而是直接渲染自訂 HTML。

```
┌──────────────────────────────────────────────────────┐
│ 顯示設定                                              │
│                                                      │
│  [Aa]   [Aa]   [●Aa]  [Aa]   [Aa]   [Aa]           │
│  75%    90%   100%   110%   125%   150%              │
└──────────────────────────────────────────────────────┘
```

- 6 個按鈕，每個顯示對應大小的 "Aa" 預覽文字（`font-size` 直接套用在 span）
- 選中狀態使用 `btn-primary`，未選中使用 `btn-outline-secondary`
- 按鈕點擊立即更新 `Model.Zoom`，**不呼叫 JS**（JS 呼叫集中在 HandleSave）

```csharp
// DisplayTab.razor — 參數
[Parameter] public EmployeePreference? Model { get; set; }

// 點擊時只更新 Model，不做 JS 呼叫
private void SelectZoom(ContentZoom value)
{
    (Model ?? _fallback).Zoom = value;
    StateHasChanged();
}
```

> `DisplayTab` 不符合「所有 Tab 使用 GenericFormComponent」的原則，因視覺化按鈕組需要自訂 HTML 結構。這是設計上刻意的例外，類似 `PersonalDataTab` 中密碼區塊不走 GenericFormComponent 的處理方式。

---

## 儲存流程

```
HandleSave()（PersonalPreferenceModalComponent）
 ├── EmployeePreferenceService.SavePreferenceAsync() → Zoom 存入 DB
 ├── 顯示成功通知
 ├── [Zoom 變更判斷] 若 employeePreference.Zoom != _originalZoom
 │    └── JSRuntime.InvokeVoidAsync("setContentZoom", (int)Zoom)
 │         ├── 更新 cookie ERPCore2.ContentZoom
 │         └── 更新 CSS variable --content-zoom → 全站即時縮放
 └── [語言變更判斷] 若語言改變 → setCultureAndReload → 頁面 reload
```

`_originalZoom` 在 `LoadDataAsync()` 載入偏好時記錄，關閉 Modal 時重置。

---

## 資源 Key

| Key | zh-TW | en-US | ja-JP | zh-CN | fil |
|-----|-------|-------|-------|-------|-----|
| `Preference.Display` | 顯示設定 | Display | 表示設定 | 显示设置 | Display |
| `Preference.FontSize` | 字型大小 | Font Size | フォントサイズ | 字体大小 | Laki ng Font |

> 縮放百分比（75%、90%...）直接以字串顯示在按鈕上，不走資源 key。

---

## 注意事項

1. **Cookie 無用戶識別**：`ERPCore2.ContentZoom` cookie 是純瀏覽器層級，不含 employeeId。跨用戶修正完全依賴 `MainLayout.OnAfterRenderAsync` 的 DB 查詢
2. **JS 函數需在 circuit 連線後才可呼叫**：`setContentZoom` 不可在 SSR 階段（`OnInitializedAsync` 且 `!RendererInfo.IsInteractive`）呼叫，否則 JSInterop 會例外
3. **新增縮放級別**：需同時更新 `ContentZoom` enum（C#）與 `contentZoomMap`（JS），兩者整數值必須對應
4. **Migration**：新增 `Zoom` 欄位後需執行 `dotnet ef migrations add AddEmployeePreferenceZoom` 與 `dotnet ef database update`

---

## 相關文件

- [README_個人化設定總綱.md](README_個人化設定總綱.md)
- [README_個人化設定_資料服務層.md](README_個人化設定_資料服務層.md)
- [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md)
- [README_個人化設定_語言切換.md](README_個人化設定_語言切換.md)
