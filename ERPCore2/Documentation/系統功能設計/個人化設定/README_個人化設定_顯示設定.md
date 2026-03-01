# 個人化設定 — 顯示設定（主題顏色 + 字型縮放）

## 更新日期
2026-02-28

---

## 功能概述

讓每位使用者可以獨立設定：

1. **主題顏色**（AppTheme）：兩種模式（淺色 Bootstrap 預設亮色 / 深色）
2. **字型縮放**（ContentZoom）：6 個縮放級別（75% ~ 150%）

兩者均儲存至 DB（`EmployeePreference`），跨裝置同步，並在頁面重新載入前透過 cookie + inline script 無閃爍套用。

---

## 一、主題顏色（AppTheme）

### AppTheme Enum

```csharp
// Data/Entities/Employees/EmployeePreference.cs
public enum AppTheme
{
    Light = 1,   // 強制亮色（Bootstrap 預設）
    Dark  = 2    // 強制深色
}
```

| Enum 值 | 使用者標籤 | 行為 | cookie 值 |
|---------|-----------|------|-----------|
| Light = 1 | 淺色 | 強制 `data-bs-theme=light`（Bootstrap 預設亮色） | `light` |
| Dark = 2 | 深色 | 強制 `data-bs-theme=dark` | `dark` |

---

### 四層架構（主題）

```
[DB] EmployeePreference.Theme (AppTheme enum)       ← 使用者偏好持久化
    ↓ 儲存時 / 登入後
[Cookie] ERPCore2.Theme = 'light'|'dark'            ← 跨請求快取（無用戶識別）
    ↓ 頁面載入時（SSR 階段）
[App.razor inline script] 讀取 cookie → 設定 html[data-bs-theme]   ← 防止 flash
    ↓ Circuit 連線後
[MainLayout.OnAfterRenderAsync] → setAppTheme(DB值) → 覆寫 cookie + data-bs-theme   ← 跨用戶修正
```

---

### JavaScript 設計（theme-helper.js）

```javascript
// wwwroot/js/theme-helper.js

var themeEnumMap = {
    1: 'light',  // Light → 強制亮色
    2: 'dark'    // Dark  → 強制深色
};

// 將主題值套用到 html[data-bs-theme]
function applyThemeValue(themeValue) {
    document.documentElement.setAttribute('data-bs-theme', themeValue);
}

window.setAppTheme = function (enumValue) {
    var themeValue = themeEnumMap[enumValue] || 'light';
    document.cookie = 'ERPCore2.Theme=' + themeValue + ';path=/;max-age=31536000';
    applyThemeValue(themeValue);
};
```

---

### 防止 Flash（App.razor inline script）

頁面 SSR 階段，Blazor circuit 未連線，無法呼叫 JS。為避免閃爍，在 `<head>` 加入同步 inline script，主題與字型縮放在同一個 IIFE 中處理：

```html
<!-- Components/App.razor — <head> 內 -->
<script>
    (function () {
        // 主題：讀取 cookie 並設定 data-bs-theme 屬性
        var themeMatch = document.cookie.match(/ERPCore2\.Theme=([^;]+)/);
        var theme = themeMatch ? decodeURIComponent(themeMatch[1]) : 'light';
        document.documentElement.setAttribute('data-bs-theme', theme);
        // 字型縮放：讀取 cookie 並設定 CSS variable
        var zoomMatch = document.cookie.match(/ERPCore2\.ContentZoom=([^;]+)/);
        if (zoomMatch) document.documentElement.style.setProperty('--content-zoom', decodeURIComponent(zoomMatch[1]));
    })();
</script>
```

此 script 同步執行，在任何 CSS 渲染前就完成，因此用戶看不到閃爍。

---

### 跨用戶 Cookie 修正（MainLayout）

`MainLayout.OnAfterRenderAsync(firstRender)` 在 circuit 連線後從 DB 讀取當前登入用戶的偏好，並覆寫：

```csharp
var preference = await EmployeePreferenceService.GetByEmployeeIdAsync(_currentEmployeeId);
await JSRuntime.InvokeVoidAsync("setAppTheme", (int)preference.Theme);
await JSRuntime.InvokeVoidAsync("setContentZoom", (int)preference.Zoom);
```

兩者一起呼叫，確保主題和縮放都以 DB 值為準。

---

### 深色模式 CSS 設計（GitHub Dark 調色盤）

深色模式以 `[data-bs-theme=dark]` 選擇器覆寫所有 CSS 變數，定義在 `wwwroot/css/colors.css`。

#### 主要顏色對應

| 用途 | 亮色 | 深色（GitHub Dark）|
|------|------|-------------------|
| 主要背景 | `#FFFFFF` | `#161b22` |
| 次要背景 | `#F9FAFB` | `#0d1117` |
| 三級背景 | `#F3F4F6` | `#21262d` |
| 主要文字 | `#374151` | `#e6edf3` |
| 次要文字 | `#6B7280` | `#8b949e` |
| 邊框 | `#D1D5DB` | `#30363d` |
| 主要藍色 | `#1E3A8A` | `#58a6ff` |
| 側邊欄/表頭背景 | `#1F2937` | `#1c2d3e` |
| 白色文字（側邊欄用）| `#FFFFFF` | `#FFFFFF`（不覆寫）|

> ⚠️ `--primary-white` **不覆寫於深色模式**：NavMenu、表頭等深色背景上的文字必須保持白色。

#### 開發規則

- **禁止** 在 `.razor.css` 中使用 `@media (prefers-color-scheme: dark)`，應使用 `[data-bs-theme=dark]`
- **禁止** 在元件 CSS 中使用硬編碼 hex 色碼，必須使用 CSS 變數（`var(--bg-primary)` 等）
- **禁止** 在 HTML 使用 Bootstrap 背景 utility class（`bg-light`、`bg-white`），見下方說明
- `--primary-white` 始終為 `#FFFFFF`，用於深色背景（側邊欄、表頭）上的文字

#### Bootstrap Utility Class 衝突問題

Bootstrap 的背景 utility class（`bg-light`、`bg-white`）使用 `!important` 宣告 `background-color`，會**直接覆蓋** `.razor.css` 中所有 CSS 變數規則，導致深色模式下元素仍顯示為白色背景。

**錯誤寫法（會在深色模式破版）：**
```razor
<div class="modal-footer bg-light">...</div>
<pre class="bg-light rounded">...</pre>
<div class="filter-bar bg-white">...</div>
```

**正確寫法：**

方式一 — 移除 Bootstrap class，在 `.razor.css` 以 CSS 變數定義（推薦）：
```css
/* ComponentName.razor.css */
.filter-bar {
    background-color: var(--bg-secondary);
}
```

方式二 — 使用 inline style（適用於一次性局部覆蓋）：
```razor
<pre style="background-color: var(--bg-secondary); color: var(--text-primary);">...</pre>
```

方式三 — 在 `.razor.css` 以 `[data-bs-theme=dark]` 選擇器加 `!important` 強制覆蓋（不推薦，僅作最後手段）：
```css
[data-bs-theme=dark] .some-element {
    background-color: var(--bg-primary) !important;
}
```

#### Bootstrap 變數覆寫

`colors.css` 的深色模式區塊也覆寫了以下 Bootstrap CSS 變數，確保與 GitHub Dark 調色盤整合：

- `--bs-pagination-*`：分頁元件顏色
- `--bs-dropdown-*`：下拉選單顏色
- `--bs-table-*`：表格顏色
- `--bs-body-color`、`--bs-body-bg`、`--bs-border-color`、`--bs-link-color` 等

#### Blazor 重新連線 UI

Blazor Server 斷線重連的系統 UI（`#components-reconnect-modal`）樣式定義在 `wwwroot/css/app.css`，使用 CSS 變數確保深色模式正常顯示：

```css
#components-reconnect-modal {
    display: none;
    position: fixed; top: 0; right: 0; bottom: 0; left: 0;
    z-index: 9999;
    background-color: rgba(0, 0, 0, 0.5);
    backdrop-filter: blur(2px);
    align-items: center; justify-content: center;
}
#components-reconnect-modal.components-reconnect-show,
#components-reconnect-modal.components-reconnect-failed,
#components-reconnect-modal.components-reconnect-rejected { display: flex; }

#components-reconnect-modal > * {
    background-color: var(--bg-primary);
    color: var(--text-primary);
    border: 1px solid var(--border-color);
    /* ... */
}
```

#### 已審計並修正的組件（深色模式）

以下組件已完成深色模式審計，移除所有硬編碼色碼與 Bootstrap utility class：

| 組件 | 修正內容 |
|------|----------|
| `Components/Shared/Modal/BaseModalComponent.razor` | 移除 action-bar、modal-footer、modal-metadata 上的 `bg-light` |
| `Components/Shared/Modal/BaseModalComponent.razor.css` | `.modal-action-bar` 改用 `var(--bg-secondary)` |
| `Components/Shared/Report/ReportPreviewModalComponent.razor.css` | print-settings-panel、export-progress 等加入 `[data-bs-theme=dark]` 覆寫 |
| `Components/Pages/Customers/CustomerTransactionTable.razor` | filter-bar、stats-bar、net-sales-cell 移除 `bg-light`/`bg-white`，改用自訂 class |
| `Components/Pages/Customers/CustomerTransactionTable.razor.css` | 新建，三個自訂 class 使用 CSS 變數 |
| `Components/Pages/Systems/TextMessageTemplateEditModalComponent.razor` | 預覽區 `pre` 移除 `bg-light`，改用 inline CSS 變數 |
| `Components/Shared/GenericIndexPageComponent.razor.css` | tbody hover、selected row、readonly 控制項深色覆寫 |
| `Components/Shared/InteractiveTableComponent.razor.css` | 同上 |
| `Components/Shared/GenericStatisticsCards.razor.css` | badge bg/border/title/value 改用 CSS 變數 |
| `Components/Shared/Report/GenericChartModalComponent.razor.css` | chart-tab-btn 所有狀態改用 CSS 變數 |
| `Components/Shared/FilterSectionColumn.razor.css` | filter-section-header 改用 CSS 變數 |
| `Components/Layout/Home.razor.css` | add-shortcut-card、panel-icon-edit 改用 CSS 變數 |
| `Components/Shared/Modal/GenericEditModalComponent.razor.css` | mobile custom-buttons-section 改用 CSS 變數 |
| `Components/Shared/FilterFieldRow.razor.css` | filter-field-label 改用 CSS 變數 |
| `Components/Shared/ShortcutKeysModalComponent.razor.css` | table td:first-child 改用 CSS 變數 |
| `Components/Shared/CharacterCountTextAreaComponent.razor.css` | textarea[readonly] 改用 CSS 變數 |

---

### DB 欄位 & Migration

- `EmployeePreference.Theme`（`AppTheme` enum，預設 `Light=1`）
- Migration：`AddEmployeePreferenceTheme`
- 位置：`Migrations/20260228083502_AddEmployeePreferenceTheme.cs`
- 套用：停止應用程式後執行 `dotnet ef database update`

---

## 二、字型縮放（ContentZoom）

### ContentZoom Enum

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

| Enum 值 | int | CSS rem |
|---------|-----|---------|
| XSmall  | 1 | `0.75rem` |
| Small   | 2 | `0.9rem`  |
| Medium  | 3 | `1rem`    |
| Large   | 4 | `1.1rem`  |
| XLarge  | 5 | `1.25rem` |
| XXLarge | 6 | `1.5rem`  |

### JavaScript 設計（content-zoom-helper.js）

```javascript
window.setContentZoom = function (enumValue) {
    var zoomRem = contentZoomMap[enumValue] || '1rem';
    document.cookie = 'ERPCore2.ContentZoom=' + zoomRem + ';path=/;max-age=31536000';
    document.documentElement.style.setProperty('--content-zoom', zoomRem);
};
```

### 儲存流程

```
HandleSave()（PersonalPreferenceModalComponent）
 ├── EmployeePreferenceService.SavePreferenceAsync() → 存入 DB
 ├── [Theme 變更] → setAppTheme(enumValue)
 │    ├── 更新 cookie ERPCore2.Theme
 │    └── 更新 html[data-bs-theme]（即時，無需 reload）
 ├── [Zoom 變更] → setContentZoom(enumValue)
 │    ├── 更新 cookie ERPCore2.ContentZoom
 │    └── 更新 CSS variable --content-zoom（即時，無需 reload）
 └── [語言變更] → setCultureAndReload()（需要 reload）
```

---

## 相關檔案

| 檔案 | 說明 |
|------|------|
| `Data/Entities/Employees/EmployeePreference.cs` | `AppTheme` + `ContentZoom` enum 定義 |
| `Migrations/20260228083502_AddEmployeePreferenceTheme.cs` | 新增 Theme 欄位的 EF migration |
| `wwwroot/css/colors.css` | CSS 變數定義 + `[data-bs-theme=dark]` 深色覆寫（含 Bootstrap 變數） |
| `wwwroot/css/app.css` | `html { font-size: var(--content-zoom) }`、Blazor reconnect modal 樣式 |
| `wwwroot/js/theme-helper.js` | `setAppTheme(enumValue)` — enum → cookie → data-bs-theme |
| `wwwroot/js/content-zoom-helper.js` | `setContentZoom(enumValue)` — enum → rem → cookie → CSS var |
| `Components/App.razor` | `<head>` 內 inline script（防止 flash：主題 + 縮放同一 IIFE） |
| `Components/Layout/MainLayout.razor` | `OnAfterRenderAsync` 強制套用當前用戶的設定（跨用戶修正） |
| `Components/Pages/Employees/PersonalPreference/DisplayTab.razor` | 顯示設定 Tab UI |
| `Components/Pages/Employees/PersonalPreference/PersonalPreferenceModalComponent.razor` | HandleSave 觸發 JS |
| `Components/Shared/Modal/BaseModalComponent.razor` | Modal 底層結構，已移除 `bg-light` |
| `Components/Shared/Modal/BaseModalComponent.razor.css` | Modal action-bar 背景使用 CSS 變數 |

---

## 注意事項

1. **Cookie 無用戶識別**：兩個 cookie 均為純瀏覽器層級。跨用戶修正依賴 `MainLayout.OnAfterRenderAsync` 的 DB 查詢。
2. **JS 需在 circuit 連線後呼叫**：`setAppTheme` / `setContentZoom` 不可在 SSR 階段呼叫（JSInterop 例外）。
3. **未知 enum 值 fallback 至 `'light'`**：`themeEnumMap[enumValue] || 'light'`，不會套用深色。
4. **新增主題或縮放級別**：需同時更新 C# enum 與 JS 的 `themeEnumMap` / `contentZoomMap`。
5. **禁止移除 MainLayout 的套用邏輯**：移除後，同台電腦切換帳號將導致設定被前一用戶污染。
6. **禁止使用 Bootstrap 背景 utility class**：`bg-light`、`bg-white` 因 `!important` 會覆蓋 CSS 變數，導致深色模式破版。新組件一律使用自訂 class + CSS 變數，或 inline `var(--bg-*)` style。

---

## 相關文件

- [README_個人化設定總綱.md](README_個人化設定總綱.md)
- [README_個人化設定_資料服務層.md](README_個人化設定_資料服務層.md)
- [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md)
- [README_個人化設定_語言切換.md](README_個人化設定_語言切換.md)
