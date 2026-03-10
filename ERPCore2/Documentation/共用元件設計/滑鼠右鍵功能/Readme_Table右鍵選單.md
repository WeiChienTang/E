# Table 右鍵選單（Context Menu）

## 相關檔案

| 檔案 | 說明 |
|------|------|
| `Components/Shared/Table/ContextMenuItem.cs` | 右鍵選單項目定義（泛型資料模型） |
| `Components/Shared/Table/GenericTableComponent.razor` | 唯讀表格組件，右鍵選單實作 |
| `Components/Shared/Table/GenericTableComponent.razor.css` | 右鍵選單相關樣式 |
| `Components/Shared/Table/GenericInteractiveTableComponent.razor` | 互動式表格組件，右鍵選單實作 |
| `Components/Shared/Table/GenericInteractiveTableComponent.razor.css` | 同上，CSS 隔離須獨立定義 |
| `Components/Shared/Page/GenericIndexPageComponent.razor.cs` | 右鍵選單相關參數、自動生成邏輯與私有狀態 |
| `Components/Shared/Page/GenericIndexPageComponent.Actions.cs` | 刪除執行邏輯：`ExecuteContextMenuDeleteAsync` |
| `Components/Shared/UI/GenericConfirmModalComponent.razor` | 右鍵刪除確認 Modal |
| `wwwroot/js/` | `tableContextMenu.adjustPosition`（邊界修正） |

---

## 運作機制

- 在 `<table>` 外層包裝 `<div data-ctx-menu="1|0">`，標記是否啟用攔截
- 全域 JavaScript 在**捕獲階段**監聽 `contextmenu` 事件，當目標元素有 `data-ctx-menu="1"` 時呼叫 `e.preventDefault()`，防止瀏覽器預設選單出現
- 每個 `<tr>` 綁定 `@oncontextmenu`，記錄點擊位置與目標資料
- 選單以 `position: fixed` 渲染，不受父層 `overflow: hidden` 影響
- 顯示後呼叫 `tableContextMenu.adjustPosition` 修正超出視窗邊界的位置
- 點擊覆蓋層（透明全螢幕 overlay）或任一選單項目後關閉

---

## ContextMenuItem\<TItem\> 屬性

| 屬性 | 類型 | 說明 |
|------|------|------|
| `Label` | `string` | 顯示文字 |
| `IconClass` | `string?` | 圖示 CSS class（Font Awesome / Bootstrap Icons） |
| `CssClass` | `string` | 額外 CSS class（如 `text-danger`） |
| `IsDivider` | `bool` | `true` 時渲染為分隔線，其他屬性無效 |
| `OnClick` | `Func<TItem, Task>?` | 點擊事件；`null` 時點擊無反應 |
| `IsVisible` | `Func<TItem, bool>?` | 動態控制是否顯示；`null` 代表永遠顯示 |
| `IsDisabled` | `Func<TItem, bool>?` | 動態控制是否停用；`null` 代表永遠啟用 |

---

## GenericTableComponent 參數

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `ContextMenuItems` | `List<ContextMenuItem<TItem>>?` | `null` | 選單項目清單；`null` 停用選單 |

---

## GenericInteractiveTableComponent 參數

同 `GenericTableComponent`，參數名稱與行為相同。

---

## GenericIndexPageComponent 參數與自動生成

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `ContextMenuItems` | `List<ContextMenuItem<TEntity>>?` | `null` | `null` 時自動生成；傳入空列表時停用選單 |

### 自動生成邏輯（`BuildEffectiveContextMenuItems`）

- `ContextMenuItems == null`（未傳入）→ 自動生成，所有 Index 頁面無需額外設定：
  - `EnableRowClick && OnRowClick.HasDelegate` → 加入「編輯」項目，呼叫 `HandleRowClick`
  - `RowPrintService != null || OnRowPrint.HasDelegate` → 加入「列印」項目（詳見 [Readme_Table右鍵列印.md](Readme_Table右鍵列印.md)）
  - `ShowDeleteButton == true` → 加入分隔線 + 「刪除」項目
- `ContextMenuItems` 為空列表（`[]`）→ 停用選單
- `ContextMenuItems` 為非空列表 → 直接使用，不自動生成

### 刪除確認 Modal

- 「刪除」點擊後設定 `_contextMenuDeleteTarget` 與 `_showContextMenuDeleteModal = true`
- Modal 確認後呼叫 `ExecuteContextMenuDeleteAsync`（以 `skipConfirm: true` 跳過 JS confirm）
- Modal 取消後清除 `_contextMenuDeleteTarget`

---

## Localization Keys

| Key | 說明 |
|-----|------|
| `Button.Edit` | 右鍵選單「編輯」項目文字 |
| `Button.Delete` | 右鍵選單「刪除」項目文字 |
| `Button.ConfirmDelete` | 刪除確認 Modal 標題 |

---

## 注意事項

- **右鍵攔截原理**：使用 JS 捕獲階段（`addEventListener('contextmenu', ..., true)`）同步攔截。Blazor Server 的非同步特性導致 `@oncontextmenu:preventDefault` 動態布林值不可靠，故改用此方案
- **`data-ctx-menu` 屬性**：Razor 中因引號衝突無法直接內嵌三元運算式，改以私有欄位 `_ctxMenuAttr` 暫存字串值
- **Blazor CSS 隔離**：`GenericTableComponent.razor.css` 的樣式不會套用到 `GenericInteractiveTableComponent`，兩個組件各自有獨立的 `.razor.css`