# Debug 開發輔助工具設計

> 最後更新：2026-03-28
> 專案：ERPCore2

---

## 一、設計背景

在開發與維護過程中，經常需要快速定位某個頁面或 Modal 對應的 `.razor` 檔案，或查看欄位的 C# PropertyName / i18n Key。傳統做法是透過搜尋或翻找原始碼，效率低且容易出錯。

本設計提供 **SuperAdmin 專屬的 Debug 輔助系統**，讓開發者可以：

- 自動看到目前頁面與 Modal 的 `.razor` 檔案名稱
- 透過快捷鍵即時顯示欄位的 PropertyName 與 i18n Key
- 點擊任何 Debug 資訊即可複製到剪貼簿

> **重要**：所有 Debug 功能僅 SuperAdmin 可見，一般使用者完全不受影響。

---

## 二、架構總覽

```
JavaScript（快捷鍵與複製）
  └─ wwwroot/js/debug-helper.js           -- 快捷鍵監聽、點擊複製、狀態通知

UI（SuperAdmin 設定面板）
  └─ Components/Pages/Employees/PersonalPreference/
      └─ SuperAdminDebugTab.razor          -- 個人化設定 > Debug Tab（說明面板）

CSS（Debug 樣式控制）
  └─ wwwroot/css/colors.css               -- 方式三/四/五的顯示/隱藏與顏色樣式

元件整合
  ├─ Components/Shared/Modal/BaseModalComponent.razor              -- 方式一：Modal 底部 Bar
  ├─ Components/Shared/Page/GenericIndexPageComponent.razor         -- 方式二：頁面底部 Badge
  ├─ Components/Shared/UI/Form/GenericFormComponent.razor           -- 方式三/四：欄位 Debug span
  └─ Components/Shared/Table/GenericInteractiveTableComponent.razor -- 方式五：表格名稱 Badge
```

---

## 三、功能總覽

| 方式 | 功能名稱 | 觸發方式 | 顯示位置 | 顯示顏色 | 點擊複製 |
|------|---------|---------|---------|---------|---------|
| 方式一 | Modal 組件名稱 | 自動顯示 | Modal 底部深色 Bar | 青色 `#7ec8e3` | 是 |
| 方式二 | 頁面組件名稱 | 自動顯示 | Index 頁面底部置中 Badge | 青色 `#7ec8e3` | 是 |
| 方式三 | i18n Key 顯示 | `Shift+Alt+K` | 表單 Label 旁 / 表格欄頭 | 綠色 `#90ee90` | 是 |
| 方式四 | PropertyName 顯示 | `Shift+Alt+P` | 表單 Label 旁 / 表格欄頭 | 橘色 `#f8c8a0` | 是 |
| 方式五 | 表格名稱 Badge | 隨方式三/四顯示 | 表格上方 | 藍色 `#7ec8e3` | 是 |

---

## 四、各功能詳細說明

### 4.1 方式一：Modal 組件名稱（自動顯示）

**用途**：快速定位目前開啟的 Modal 對應哪個 `.razor` 檔案。

**實作位置**：`BaseModalComponent.razor`

**參數**：
```csharp
[Parameter]
public string? DebugComponentName { get; set; }
```

**運作方式**：
- SuperAdmin 登入後，每個 Modal 底部自動出現深色 Bar
- 顯示該 Modal 的 `.razor` 檔案名稱
- 支援階層式名稱（以 ` > ` 分隔），如：`PurchaseOrderEditModal > DetailTab`
- 點擊即複製組件名稱到剪貼簿

**使用範例**：
```razor
<GenericEditModalComponent DebugName="@nameof(SalesOrderEditModalComponent)" ... />
```

---

### 4.2 方式二：頁面組件名稱（自動顯示）

**用途**：快速定位目前瀏覽的 Index 頁面對應哪個 `.razor` 檔案。

**實作位置**：`GenericIndexPageComponent.razor`

**參數**：
```csharp
[Parameter]
public string? DebugPageName { get; set; }
```

**運作方式**：
- SuperAdmin 登入後，Index 列表頁面底部置中自動出現 Badge
- 顯示該頁面的 `.razor` 檔案名稱
- 固定定位於頁面底部中央（`position:fixed; bottom:1rem; left:50%`）
- 點擊即複製頁面名稱到剪貼簿

---

### 4.3 方式三：i18n Key 顯示（Shift+Alt+K）

**用途**：查看每個表單欄位與表格欄頭的 i18n 資源鍵名稱，方便多語系開發。

**實作位置**：
- 表單欄位：`GenericFormComponent.razor`
- 表格欄頭：`GenericInteractiveTableComponent.razor`

**運作方式**：
- 按下 `Shift+Alt+K` 啟用，再按一次關閉
- 在表單 Label 旁顯示推斷的 i18n Key，格式為 `Field.[PropertyName]`
- 在表格欄頭顯示 `Column.[PropertyName]`
- 右下角出現綠色狀態通知
- 點擊任何 i18n Key 即可複製

**Razor 渲染**：
```razor
<span class="field-debug-i18n" aria-hidden="true">Field.@field.PropertyName</span>
```

**CSS 控制**：
- 預設 `display: none`
- 當 `<html>` 有 `debug-i18n` class 時才顯示

---

### 4.4 方式四：PropertyName 顯示（Shift+Alt+P）

**用途**：查看每個表單欄位與表格欄頭的 C# 屬性名稱，方便程式開發與除錯。

**實作位置**：
- 表單欄位：`GenericFormComponent.razor`
- 表格欄頭：`GenericInteractiveTableComponent.razor`

**運作方式**：
- 按下 `Shift+Alt+P` 啟用，再按一次關閉
- 在表單 Label 旁顯示 C# PropertyName
- 在表格欄頭顯示欄位 PropertyName
- 右下角出現橘色狀態通知
- 點擊任何 PropertyName 即可複製

**Razor 渲染**：
```razor
<span class="field-debug-name" aria-hidden="true">@field.PropertyName</span>
```

**CSS 控制**：
- 預設 `display: none`
- 當 `<html>` 有 `debug-props` class 時才顯示

---

### 4.5 方式五：表格名稱 Badge（隨方式三/四顯示）

**用途**：在啟用方式三或方式四時，顯示表格的識別名稱。

**實作位置**：`GenericInteractiveTableComponent.razor`

**參數**：
```csharp
[Parameter]
public string? DebugTableName { get; set; }
```

**運作方式**：
- 不需獨立快捷鍵，隨方式三或方式四啟用時自動出現
- 在每個表格上方顯示藍色 Badge
- 點擊即複製表格名稱

**Razor 渲染**：
```razor
@if (!string.IsNullOrEmpty(DebugTableName))
{
    <div class="table-debug-name">@DebugTableName</div>
}
```

---

## 五、JavaScript 模組：debug-helper.js

**檔案位置**：`wwwroot/js/debug-helper.js`

**初始化流程**：
1. `MainLayout.OnAfterRenderAsync` 確認使用者為 SuperAdmin
2. 呼叫 `DebugHelper.initDebugShortcuts()` 註冊鍵盤監聽與點擊處理
3. 一般使用者永遠不會觸發此模組

**主要 API**：

| 方法 | 說明 |
|------|------|
| `initDebugShortcuts()` | 註冊快捷鍵監聽器與 capture phase 點擊事件（只呼叫一次） |
| `copyText(text, color)` | 複製文字到剪貼簿並顯示確認通知，供 Blazor `@onclick` 與 JS 內部使用 |
| `_notify(message, color)` | 右下角顯示 2 秒短暫通知（z-index: 9999） |

**快捷鍵對照**：

| 快捷鍵 | CSS Class | 功能 |
|--------|-----------|------|
| `Shift+Alt+P` | `debug-props` | 切換 PropertyName 顯示 |
| `Shift+Alt+K` | `debug-i18n` | 切換 i18n Key 顯示 |

**點擊複製機制**（capture phase）：
- `field-debug-name` / `col-debug-name` → 複製 PropertyName（橘色通知）
- `field-debug-i18n` / `col-debug-i18n` → 複製 i18n Key（綠色通知）
- `table-debug-name` → 複製表格名稱（藍色通知）

---

## 六、安全性設計

- 所有 Debug 功能皆以 `IsSuperAdmin` 條件控制
- `debug-helper.js` 僅在 SuperAdmin 登入後才初始化
- 方式一/二的 HTML 僅在 SuperAdmin 條件下渲染（server-side 判斷）
- 方式三/四/五的 span 雖存在於 DOM，但透過 CSS 預設 `display: none`，且快捷鍵監聽未註冊時無法觸發
- 重新整理頁面後，所有快捷鍵狀態自動重設為隱藏

---

## 七、開發者工具（個人化設定 > Debug Tab）

除了上述五種 Debug 顯示功能，SuperAdmin 在「個人化設定」中還有 **Debug Tab**（`SuperAdminDebugTab.razor`），提供：

| 工具 | 說明 |
|------|------|
| 資料庫載入 | 從 Excel 匯入資料到系統資料表（精靈步驟：選表 → 上傳 → 對應 → 預覽 → 匯入） |
| 資料庫匯出 | 將系統資料表匯出為 Excel 檔案（可單選、多選或全選資料表） |

此 Tab 同時作為所有 Debug 快捷鍵的說明面板，列出各方式的觸發方式與顯示顏色。
