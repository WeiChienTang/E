# 個人化設定 — 顯示設定（主題顏色 + 字型縮放）

## 更新日期
2026-03-07

---

## 概述

每位使用者可獨立設定**主題顏色**（AppTheme）與**字型縮放**（ContentZoom），儲存至 DB（`EmployeePreference`），跨裝置同步。兩者均透過 cookie + inline script 在頁面載入前無閃爍套用；**主題與縮放變更不需要 reload**，語言變更才需要。

---

## 核心檔案

| 檔案 | 說明 |
|------|------|
| `Data/Entities/Employees/EmployeePreference.cs` | `AppTheme` + `ContentZoom` enum 定義 |
| `wwwroot/css/colors.css` | CSS 變數定義 + `[data-bs-theme=dark]` 深色覆寫（含 Bootstrap 變數覆寫） |
| `wwwroot/css/app.css` | `html { font-size: var(--content-zoom) }`、Blazor reconnect modal 樣式 |
| `wwwroot/js/theme-helper.js` | `setAppTheme(enumValue)` — enum → cookie → `html[data-bs-theme]` |
| `wwwroot/js/content-zoom-helper.js` | `setContentZoom(enumValue)` — enum → rem → cookie → `--content-zoom` CSS var |
| `Components/App.razor` | `<head>` 內 inline script（防止 flash，主題與縮放在同一 IIFE） |
| `Components/Layout/MainLayout.razor` | `OnAfterRenderAsync` 強制套用當前用戶的 DB 設定（跨用戶 cookie 修正） |
| `Components/Pages/Employees/PersonalPreference/DisplayTab.razor` | 顯示設定 Tab UI |

---

## 一、主題顏色（AppTheme）

### AppTheme 枚舉

| Enum 值 | int | 使用者標籤 | 行為 | cookie 值 |
|---------|-----|-----------|------|-----------|
| `Light` | 1 | 淺色 | 強制 `data-bs-theme=light`（Bootstrap 預設） | `light` |
| `Dark`  | 2 | 深色 | 強制 `data-bs-theme=dark` | `dark` |

### 四層架構

| 層 | 說明 |
|----|------|
| **DB** `EmployeePreference.Theme` | 使用者偏好持久化 |
| **Cookie** `ERPCore2.Theme='light'\|'dark'` | 跨請求快取（無用戶識別） |
| **App.razor inline script** | SSR 階段讀取 cookie → 設定 `html[data-bs-theme]`（防止 flash） |
| **MainLayout.OnAfterRenderAsync** | Circuit 連線後從 DB 讀取並覆寫 cookie + `data-bs-theme`（跨用戶修正） |

### theme-helper.js 函式

| 函式 | 說明 |
|------|------|
| `setAppTheme(enumValue)` | 透過 `themeEnumMap` 將 int 轉為 `'light'`/`'dark'`，寫入 cookie 並設定 `data-bs-theme`。未知值 fallback `'light'` |

---

## 二、字型縮放（ContentZoom）

### ContentZoom 枚舉

| Enum 值 | int | CSS rem |
|---------|-----|---------|
| `XSmall`  | 1 | `0.75rem` |
| `Small`   | 2 | `0.9rem`  |
| `Medium`  | 3 | `1rem`（預設）|
| `Large`   | 4 | `1.1rem`  |
| `XLarge`  | 5 | `1.25rem` |
| `XXLarge` | 6 | `1.5rem`  |

### content-zoom-helper.js 函式

| 函式 | 說明 |
|------|------|
| `setContentZoom(enumValue)` | 透過 `contentZoomMap` 將 int 轉為 rem 值，寫入 cookie 並設定 `--content-zoom` CSS 變數 |

---

## 儲存流程

`PersonalPreferenceModalComponent.HandleSave()` 儲存至 DB 後：

| 變更類型 | JS 呼叫 | 是否需要 reload |
|---------|---------|:--------------:|
| 主題變更 | `setAppTheme(enumValue)` — 更新 cookie + `data-bs-theme` | ❌ |
| 縮放變更 | `setContentZoom(enumValue)` — 更新 cookie + `--content-zoom` | ❌ |
| 語言變更 | `setCultureAndReload(cultureCode)` | ✅ |

---

## 深色模式 CSS 設計

### 調色盤（GitHub Dark）

| 用途 | CSS 變數 | 亮色 | 深色 |
|------|---------|------|------|
| 主要背景 | `--bg-primary` | `#FFFFFF` | `#161b22` |
| 次要背景 | `--bg-secondary` | `#F9FAFB` | `#0d1117` |
| 三級背景 | `--bg-tertiary` | `#F3F4F6` | `#21262d` |
| 主要文字 | `--text-primary` | `#374151` | `#e6edf3` |
| 次要文字 | `--text-secondary` | `#6B7280` | `#8b949e` |
| 邊框 | `--border-color` | `#D1D5DB` | `#30363d` |
| 主要藍色 | `--color-primary` | `#1E3A8A` | `#58a6ff` |
| 白色文字（側邊欄用）| `--primary-white` | `#FFFFFF` | `#FFFFFF`（不覆寫）|

`--primary-white` 不在深色模式中覆寫，NavMenu / 表頭等深色背景上的文字必須保持白色。

### Bootstrap 變數覆寫

`colors.css` 的 `[data-bs-theme=dark]` 區塊額外覆寫：`--bs-pagination-*`、`--bs-dropdown-*`、`--bs-table-*`、`--bs-body-color`、`--bs-body-bg`、`--bs-border-color`、`--bs-link-color`。

---

## 重要設計規則

### 1. Cookie 無用戶識別，須依賴 MainLayout 修正

cookie 為純瀏覽器層級，同台電腦切換帳號時 cookie 仍是前一用戶的值。`MainLayout.OnAfterRenderAsync` 在 circuit 連線後以 DB 值覆寫，修正跨用戶污染。**禁止移除此邏輯。**

### 2. JS 函式只能在 circuit 連線後呼叫

`setAppTheme` / `setContentZoom` 為 JSInterop 呼叫，不可在 SSR 階段使用。SSR 防閃爍由 `App.razor` 的 inline script 處理。

### 3. 禁止使用 Bootstrap 背景 utility class

`bg-light`、`bg-white` 使用 `!important` 宣告 `background-color`，會直接覆蓋 `.razor.css` 中的 CSS 變數規則，導致深色模式下仍顯示白色背景。新元件一律使用：
- 自訂 class + `.razor.css` 中的 `var(--bg-*)` CSS 變數（推薦）
- 或 inline `style="background-color: var(--bg-secondary)"` （一次性局部使用）

### 4. 禁止在 .razor.css 使用 @media (prefers-color-scheme: dark)

深色模式覆寫一律使用 `[data-bs-theme=dark]` 選擇器，不使用 OS-level media query。

### 5. ::deep 與 [data-bs-theme=dark] 的衝突

`[data-bs-theme=dark] ::deep .selector` 是錯誤寫法——Blazor 會將 scope attribute 套用到 `[data-bs-theme=dark]` 選擇器本身，編譯結果為 `[data-bs-theme=dark][b-xyz] .selector`，永遠不匹配。深色模式覆寫必須使用 `[data-bs-theme=dark] .selector`（不加 `::deep`），Blazor 會自動對 `.selector` 加 scope。

### 6. 元件 CSS 禁止硬編碼 hex 色碼

所有 `.razor.css` 中的顏色必須使用 CSS 變數（`var(--bg-primary)` 等），不可使用 `#FFFFFF`、`#F9FAFB` 等硬編碼值。

### 7. 新增主題或縮放級別需同步更新 JS

新增 `AppTheme` 或 `ContentZoom` 枚舉值時，必須同步更新 `theme-helper.js` 的 `themeEnumMap` / `content-zoom-helper.js` 的 `contentZoomMap`。

---

## 相關文件

- [README_個人化設定總綱.md](README_個人化設定總綱.md)
- [README_個人化設定_資料服務層.md](README_個人化設定_資料服務層.md)
- [README_個人化設定_UI框架.md](README_個人化設定_UI框架.md)
- [README_個人化設定_語言切換.md](README_個人化設定_語言切換.md)
