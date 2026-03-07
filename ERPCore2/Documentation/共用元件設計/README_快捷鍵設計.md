# 快捷鍵與快速功能設計

## 更新日期
2026-03-07

---

## 概述

ERPCore2 提供全域**快捷鍵**與**快速功能表**機制，整合在 `MainLayout.razor` 中統一管理。包含三個層次：**全域快捷鍵**（鍵盤組合鍵）、**快速功能表**（右下角浮動按鈕）、**通用搜尋 Modal**（頁面搜尋與報表搜尋）。

---

## 檔案結構

| 檔案 | 路徑 | 說明 |
|------|------|------|
| 整合入口 | `Components/Layout/MainLayout.razor` | 快捷鍵註冊、Action Registry、搜尋 Modal 宣告 |
| 快速功能表 | `Components/Shared/QuickAction/QuickActionMenu.razor` | 右下角浮動按鈕選單 |
| 通用搜尋 | `Components/Shared/QuickAction/GenericSearchModalComponent.razor` | 頁面搜尋 / 報表搜尋 Modal |
| 快捷鍵說明 | `Components/Shared/QuickAction/ShortcutKeysModalComponent.razor` | 顯示所有快捷鍵的說明 Modal |
| 導航配置 | `Data/Navigation/NavigationConfig.cs` | 唯一資料來源（Route / Action / QuickAction） |
| Action 輔助 | `Helpers/Common/NavigationActionHelper.cs` | `CreateActionItem()` + `ActionHandlerRegistry` |
| 導航項目模型 | `Models/Navigation/NavigationItem.cs` | 導航項目定義（含 QuickAction / ChartWidget 屬性） |
| 搜尋介面 | `Models/Navigation/ISearchableItem.cs` | 可搜尋項目介面 |
| 全域快捷鍵 JS | `wwwroot/js/keyboard-shortcuts.js` | 鍵盤事件監聽（輸入保護 + Modal 感知） |

---

## 系統快捷鍵一覽

| 快捷鍵 | 功能 | 實作位置 |
|--------|------|----------|
| `Alt + S` | 開啟頁面搜尋視窗 | `MainLayout.razor` |
| `Alt + R` | 開啟報表搜尋視窗 | `MainLayout.razor` |
| `Alt + Q` | 開啟 / 關閉快速功能表 | `QuickActionMenu.razor` |
| `Esc` | 關閉當前 Modal | `BaseModalComponent.razor` |

---

## GenericSearchModalComponent 參數

| 參數 | 型別 | 說明 |
|------|------|------|
| `IsVisible` | `bool` | Modal 是否顯示 |
| `IsVisibleChanged` | `EventCallback<bool>` | 顯示狀態變更事件 |
| `Title` | `string` | Modal 標題 |
| `Placeholder` | `string` | 搜尋框佔位符 |
| `SearchFunction` | `Func<string, List<ISearchableItem>>` | 搜尋函數（由呼叫端提供） |
| `ActionRegistry` | `ActionHandlerRegistry?` | Action 處理器（頁面搜尋時傳入） |
| `OnItemSelected` | `EventCallback<string>` | 選擇項目回呼（報表搜尋時使用） |

頁面搜尋傳入 `ActionRegistry`，由元件內部執行對應 Handler；報表搜尋傳入 `OnItemSelected`，由外部處理選取後的邏輯。

---

## QuickActionMenu 參數

| 參數 | 型別 | 說明 |
|------|------|------|
| `OnPageSearchClick` | `EventCallback` | 點擊頁面搜尋按鈕 |
| `OnReportSearchClick` | `EventCallback` | 點擊報表搜尋按鈕 |
| `OnShortcutKeysClick` | `EventCallback` | 點擊快捷鍵說明按鈕 |

**功能項目**：頁面搜尋（Alt+S）、報表搜尋（Alt+R）、快捷鍵說明。通知中心、最近使用、快速設定為預留功能，目前不顯示。

---

## NavigationActionHelper 機制

| 成員 | 說明 |
|------|------|
| `CreateActionItem(...)` | 工廠方法，建立 `ItemType = Action` 的 NavigationItem |
| `ActionHandlerRegistry` | Handler 字典：`actionId → Action` |
| `registry.Register(actionId, handler)` | 在 `MainLayout.OnInitialized` 中集中註冊所有 Handler |
| `registry.Execute(actionId)` | 由 `GenericSearchModalComponent` 呼叫，執行對應 Handler |

`CreateActionItem()` 不支援設定 `IsChartWidget`，圖表介面項目需直接宣告 `NavigationItem`（詳見首頁客製化設計文件）。

---

## keyboard-shortcuts.js 行為

| 功能 | 說明 |
|------|------|
| 輸入保護 | 偵測 focus 元素是否為 input / textarea / select / contenteditable，是則忽略快捷鍵 |
| Modal 感知 | 偵測是否有 Modal 開啟，有則忽略全域快捷鍵（避免多重 Modal 衝突） |
| .NET 互操作 | 透過 `dotNetHelper.invokeMethodAsync` 呼叫 `MainLayout` 中的 C# 方法 |
| 清理 | 提供 `dispose()` 方法移除事件監聽（元件 Dispose 時呼叫） |

---

## 重要設計規則

### 1. 統一入口

所有快捷鍵、Action Handler、搜尋 Modal 均在 `MainLayout.razor` 中宣告與註冊，不分散到子元件。

### 2. 不與系統快捷鍵衝突

新增快捷鍵建議使用 `Alt + 字母` 格式，避免與瀏覽器預設（`Ctrl+S`、`Ctrl+R` 等）衝突。

### 3. Modal 不使用 @if 包裹

MainLayout 中的全域 Modal（報表、圖表等）不使用 `@if` 條件渲染，以 `IsVisible = false` 控制顯示（原因同 QuickActionModalHost 設計）。

### 4. SearchKeywords 決定搜尋命中率

`NavigationItem.SearchKeywords` 是頁面搜尋的唯一關鍵字來源，新增功能時應補充中英文關鍵字。

---

## 新增功能快速參考

### 新增快捷鍵
1. 在 `keyboard-shortcuts.js` 的 `handleKeyDown` 新增鍵盤判斷，呼叫 `dotNetHelper.invokeMethodAsync`
2. 在 `MainLayout.razor` 新增對應的 `[JSInvokable]` C# 方法
3. 在 `ShortcutKeysModalComponent.razor` 的 `InitializeShortcutKeys` 補充說明條目

### 新增 Action 功能（可搜尋、可從首頁快速功能表啟動）
1. 在 `NavigationConfig.cs` 用 `NavigationActionHelper.CreateActionItem()` 新增項目（設定 `SearchKeywords`）
2. 在 `MainLayout.OnInitialized` 呼叫 `actionRegistry.Register(actionId, handler)` 註冊 Handler
3. 在 `MainLayout.razor` 宣告對應 Modal（不使用 `@if` 包裹）

---

## 相關文件

- [README_共用元件設計總綱.md](README_共用元件設計總綱.md)
- [README_首頁客製化設計.md](../完整頁面設計/README_首頁客製化設計.md)
