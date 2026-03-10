# Table 右鍵選單與多選功能

## 相關檔案

| 檔案 | 說明 |
|------|------|
| `Components/Shared/Table/ContextMenuItem.cs` | 右鍵選單項目定義（泛型資料模型） |
| `Components/Shared/Table/GenericTableComponent.razor` | 唯讀表格組件，內建右鍵選單與多選 |
| `Components/Shared/Table/GenericTableComponent.razor.css` | 右鍵選單樣式、選取列高亮樣式 |
| `Components/Shared/Table/GenericInteractiveTableComponent.razor` | 互動式表格組件，內建右鍵選單（多選由父組件管理） |
| `Components/Shared/Table/GenericInteractiveTableComponent.razor.css` | 同上，CSS 隔離因此需獨立重複定義 |
| `Components/Shared/Page/GenericIndexPageComponent.razor` | Index 頁面基底，右鍵選單自動生成、多選操作列與 Modal |
| `Components/Shared/Page/GenericIndexPageComponent.razor.cs` | 右鍵選單、多選相關參數與私有狀態 |
| `Components/Shared/Page/GenericIndexPageComponent.Actions.cs` | 刪除執行邏輯：`DeleteEntityAsync`、`ExecuteContextMenuDeleteAsync`、`ExecuteMultiDeleteAsync` |
| `Components/Shared/Page/GenericIndexPageComponent.DataLoader.cs` | `RefreshData()` 刷新時自動清除 `_selectedItems` |
| `Components/Shared/UI/GenericConfirmModalComponent.razor` | 右鍵刪除確認與多選批次刪除所使用的確認 Modal |
| `wwwroot/js/` | 全域 JS：`setCheckboxIndeterminate`（indeterminate 狀態）、`tableContextMenu.adjustPosition`（邊界修正） |

---

## 右鍵選單（Context Menu）

### 運作機制

- 在 `<table>` 外層包裝 `<div data-ctx-menu="1|0">`，標記是否啟用攔截
- 全域 JavaScript 在**捕獲階段**監聽 `contextmenu` 事件，當目標元素有 `data-ctx-menu="1"` 時呼叫 `e.preventDefault()`，防止瀏覽器預設選單出現
- 每個 `<tr>` 綁定 `@oncontextmenu`，記錄點擊位置與目標資料
- 選單以 `position: fixed` 渲染，不受父層 `overflow: hidden` 影響
- 顯示後呼叫 `tableContextMenu.adjustPosition` 修正超出視窗邊界的位置
- 點擊覆蓋層（透明全螢幕 `overlay`）或任一選單項目後關閉

### ContextMenuItem\<TItem\> 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Label` | `string` | 顯示文字 |
| `IconClass` | `string?` | 圖示 CSS class（Font Awesome / Bootstrap Icons） |
| `CssClass` | `string` | 額外 CSS class（如 `text-danger`） |
| `IsDivider` | `bool` | `true` 時渲染為分隔線，其他屬性無效 |
| `OnClick` | `Func<TItem, Task>?` | 點擊事件；`null` 時點擊無反應 |
| `IsVisible` | `Func<TItem, bool>?` | 動態控制是否顯示；`null` 代表永遠顯示 |
| `IsDisabled` | `Func<TItem, bool>?` | 動態控制是否停用；`null` 代表永遠啟用 |

### GenericTableComponent 右鍵選單參數

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `ContextMenuItems` | `List<ContextMenuItem<TItem>>?` | `null` | 選單項目清單；`null` 停用選單 |

### GenericInteractiveTableComponent 右鍵選單參數

同 `GenericTableComponent`，參數相同。

### GenericIndexPageComponent 右鍵選單參數與自動生成

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `ContextMenuItems` | `List<ContextMenuItem<TEntity>>?` | `null` | `null` 時自動生成；空列表時停用 |

**自動生成邏輯（`BuildEffectiveContextMenuItems`）：**

- `ContextMenuItems == null`（未傳入）→ 自動生成，所有 Index 頁面無需額外修改
  - `EnableRowClick && OnRowClick.HasDelegate` → 加入「編輯」項目，呼叫 `HandleRowClick`
  - `ShowDeleteButton == true` → 加入分隔線 + 「刪除」項目，點擊後開啟確認 Modal
- `ContextMenuItems` 為空列表（`[]`）→ 停用選單
- `ContextMenuItems` 為非空列表 → 直接使用，不自動生成

**刪除確認 Modal：**

- 右鍵選單的「刪除」點擊後設定 `_contextMenuDeleteTarget` 與 `_showContextMenuDeleteModal = true`
- Modal 確認後呼叫 `ExecuteContextMenuDeleteAsync`，以 `skipConfirm: true` 跳過 JS confirm
- Modal 取消後清除 `_contextMenuDeleteTarget`

---

## 多選功能（Multi-Select）

### 運作範圍

多選功能目前僅整合於 `GenericTableComponent` + `GenericIndexPageComponent`。
`GenericInteractiveTableComponent` 已有獨立的多選實作（`EnableRowSelection`、`ShowSelectColumn`），設計上分開管理。

### GenericTableComponent 多選參數

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `ShowSelectColumn` | `bool` | `false` | 是否顯示 Checkbox 欄位 |
| `AllowMultipleSelection` | `bool` | `true` | `true` = 多選；`false` = 單選 |
| `SelectedItems` | `HashSet<TItem>?` | `null` | 雙向綁定的選取狀態集合 |
| `OnSelectionChanged` | `EventCallback<HashSet<TItem>>` | — | 選取狀態改變事件 |
| `IsItemSelectable` | `Func<TItem, bool>?` | `null` | 判斷特定列是否可選取；`null` 代表全部可選 |
| `SelectColumnWidth` | `string` | `"40px"` | Checkbox 欄寬度 |

**行為說明：**

- 標頭 Checkbox 支援三態：全選、全取消、indeterminate（部分選取），由 `setCheckboxIndeterminate` JS 設定
- 選取列自動套用 `.row-selected` CSS class（高亮背景）
- Checkbox 欄的 `<td>` 加有 `@onclick:stopPropagation="true"`，避免觸發行點擊事件
- `IsItemSelectable` 返回 `false` 的列不顯示 Checkbox

### GenericIndexPageComponent 多選參數

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `EnableMultiSelect` | `bool` | `false` | 啟用多選功能（同時啟用 Checkbox 欄與操作列） |
| `IsMultiSelectItemSelectable` | `Func<TEntity, bool>?` | `null` | 自訂可選取條件；`null` 時使用 `IsEntityDeletable` |

**內部狀態：**

| 欄位 | 類型 | 說明 |
|------|------|------|
| `_selectedItems` | `HashSet<TEntity>` | 目前選取的資料集合 |
| `_showMultiDeleteModal` | `bool` | 批次刪除確認 Modal 顯示狀態 |

**多選操作列（Selection Action Bar）：**

- 條件：`EnableMultiSelect && _selectedItems.Count > 0`
- 顯示目前選取筆數（`Label.SelectedCount`）
- 「刪除」按鈕 → 開啟批次刪除確認 Modal
- 「清除選取」按鈕（`Button.ClearSelection`） → 清空 `_selectedItems`

**批次刪除確認 Modal：**

- 條件：`_showMultiDeleteModal && _selectedItems.Count > 0`
- 確認訊息使用 `Message.ConfirmBatchDelete`（含筆數與 EntityName）
- 確認後呼叫 `ExecuteMultiDeleteAsync`
- 逐筆執行刪除，優先使用 `CustomDeleteHandler`，否則呼叫 `Service.PermanentDeleteAsync`
- 統計成功/失敗筆數，分別顯示通知

**自動清除選取：**

- `RefreshData()` 開始時自動清除 `_selectedItems`（換頁、搜尋、刷新後不殘留）
- `ExecuteMultiDeleteAsync` 執行前清除 `_selectedItems`

---

## 多選功能（GenericInteractiveTableComponent 原有實作）

此為獨立的舊有設計，與 GenericIndexPageComponent 的 `EnableMultiSelect` 無關。

| 參數 | 類型 | 說明 |
|------|------|------|
| `EnableRowSelection` | `bool` | 啟用點擊列選取（切換選取狀態） |
| `AllowMultipleSelection` | `bool` | 允許多選（否則為單選） |
| `ShowSelectColumn` | `bool` | 顯示獨立的 Checkbox 欄位 |
| `SelectedItems` | `HashSet<TItem>?` | 雙向綁定選取狀態 |
| `OnSelectionChanged` | `EventCallback<HashSet<TItem>>` | 選取變更事件 |
| `IsItemSelectable` | `Func<TItem, bool>?` | 特定列是否可選取 |
| `UnselectableTemplate` | `RenderFragment<TItem>?` | 不可選取列的替代內容 |
| `SelectColumnWidth` | `string` | Checkbox 欄寬（預設 `50px`） |

---

## 相關 Localization Keys

| Key | 說明 |
|-----|------|
| `Label.SelectAll` | 全選 Checkbox tooltip |
| `Label.SelectItem` | 單列 Checkbox tooltip |
| `Label.SelectedCount` | 操作列顯示筆數（格式：`{0}`） |
| `Button.ClearSelection` | 清除選取按鈕文字 |
| `Button.Delete` | 刪除按鈕文字 |
| `Button.ConfirmDelete` | 確認刪除 Modal 標題 |
| `Message.ConfirmBatchDelete` | 批次刪除確認訊息（格式：`{0}` 筆數、`{1}` EntityName） |

---

## CSS 類別

| Class | 定義位置 | 說明 |
|-------|----------|------|
| `.generic-table-wrapper` | `GenericTableComponent.razor.css` / `GenericInteractiveTableComponent.razor.css` | 外層包裝，啟用 `position: fixed` 基準 |
| `.table-context-menu-overlay` | 同上 | 透明全螢幕覆蓋層，點擊關閉選單 |
| `.table-context-menu` | 同上 | 選單本體（`position: fixed`） |
| `.table-context-menu-item` | 同上 | 選單按鈕項目 |
| `.table-context-menu-divider` | 同上 | 選單分隔線 |
| `.row-selected` | `GenericTableComponent.razor.css` | 已選取列的高亮背景 |

---

## 注意事項

- **Blazor CSS 隔離**：`GenericTableComponent.razor.css` 的樣式不會套用到 `GenericInteractiveTableComponent` 元素，因此兩個組件各自有獨立的 `.razor.css` 檔案
- **右鍵攔截原理**：使用 JS 捕獲階段（`addEventListener('contextmenu', ..., true)`）同步攔截，Blazor Server 的非同步特性導致 `@oncontextmenu:preventDefault` 動態布林值不可靠，故改用此方案
- **`data-ctx-menu` 屬性**：Razor 中 `"@(_hasContextMenu ? "1" : "0")"` 因引號衝突無法直接使用，改以私有欄位 `_ctxMenuAttr` 暫存字串值
- **dark mode 選取高亮**：使用 `rgba(88, 166, 255, 0.15)` 而非 CSS 變數，因 `--bs-primary-bg-subtle` 在 dark mode 下顏色不合適
