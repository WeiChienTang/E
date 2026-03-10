# Table 多選功能（Multi-Select）

## 相關檔案

| 檔案 | 說明 |
|------|------|
| `Components/Shared/Table/GenericTableComponent.razor` | 多選 Checkbox 欄位實作 |
| `Components/Shared/Table/GenericTableComponent.razor.css` | `.row-selected` 已選取列高亮樣式 |
| `Components/Shared/Page/GenericIndexPageComponent.razor.cs` | `EnableMultiSelect`、`IsMultiSelectItemSelectable` 參數與 `_selectedItems` 狀態 |
| `Components/Shared/Page/GenericIndexPageComponent.razor` | 多選操作列與批次刪除 Modal |
| `Components/Shared/Page/GenericIndexPageComponent.Actions.cs` | `ExecuteMultiDeleteAsync` 批次刪除邏輯 |
| `Components/Shared/Page/GenericIndexPageComponent.DataLoader.cs` | `RefreshData()` 清除 `_selectedItems` |
| `Components/Shared/UI/GenericConfirmModalComponent.razor` | 批次刪除確認 Modal |
| `wwwroot/js/` | `setCheckboxIndeterminate`（標頭 Checkbox 三態設定） |

---

## 運作範圍

多選功能分為兩套獨立設計：

| 設計 | 適用組件 | 管理方式 |
|------|----------|----------|
| **標準多選** | `GenericTableComponent` + `GenericIndexPageComponent` | `GenericIndexPageComponent` 統一管理狀態與操作列 |
| **InteractiveTable 多選** | `GenericInteractiveTableComponent` | 父組件自行管理，與 GenericIndexPageComponent 無關 |

---

## 標準多選（GenericTableComponent）

### GenericTableComponent 參數

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `ShowSelectColumn` | `bool` | `false` | 是否顯示 Checkbox 欄位 |
| `AllowMultipleSelection` | `bool` | `true` | `true` = 多選；`false` = 單選 |
| `SelectedItems` | `HashSet<TItem>?` | `null` | 雙向綁定的選取狀態集合 |
| `OnSelectionChanged` | `EventCallback<HashSet<TItem>>` | — | 選取狀態改變事件 |
| `IsItemSelectable` | `Func<TItem, bool>?` | `null` | 判斷特定列是否可選取；`null` 代表全部可選 |
| `SelectColumnWidth` | `string` | `"40px"` | Checkbox 欄寬度 |

### 行為說明

- 標頭 Checkbox 支援三態：全選、全取消、indeterminate（部分選取），由 `setCheckboxIndeterminate` JS 設定
- 已選取列自動套用 `.row-selected` CSS class（高亮背景）
- Checkbox 欄的 `<td>` 加有 `@onclick:stopPropagation="true"`，避免觸發行點擊事件
- `IsItemSelectable` 返回 `false` 的列不顯示 Checkbox

---

## 標準多選（GenericIndexPageComponent）

### 參數

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `EnableMultiSelect` | `bool` | `false` | 啟用多選（同時啟用 Checkbox 欄與多選操作列） |
| `IsMultiSelectItemSelectable` | `Func<TEntity, bool>?` | `null` | 自訂可選取條件；`null` 時使用 `IsEntityDeletable` |

### 內部狀態

| 欄位 | 類型 | 說明 |
|------|------|------|
| `_selectedItems` | `HashSet<TEntity>` | 目前選取的資料集合 |
| `_showMultiDeleteModal` | `bool` | 批次刪除確認 Modal 顯示狀態 |

### 多選操作列（Selection Action Bar）

- 條件：`EnableMultiSelect && _selectedItems.Count > 0`
- 顯示目前選取筆數（`Label.SelectedCount`）
- 「刪除」按鈕 → 開啟批次刪除確認 Modal
- 「清除選取」按鈕 → 清空 `_selectedItems`

### 批次刪除流程

- 確認訊息使用 `Message.ConfirmBatchDelete`（含筆數與 EntityName）
- 確認後呼叫 `ExecuteMultiDeleteAsync`
- 逐筆執行刪除：優先使用 `CustomDeleteHandler`，否則呼叫 `Service.PermanentDeleteAsync`
- 統計成功/失敗筆數，分別顯示通知

### 自動清除選取

- `RefreshData()` 開始時自動清除 `_selectedItems`（換頁、搜尋、刷新後不殘留）
- `ExecuteMultiDeleteAsync` 執行前清除 `_selectedItems`

---

## InteractiveTable 多選（GenericInteractiveTableComponent）

此為獨立的舊有設計，與 GenericIndexPageComponent 的 `EnableMultiSelect` 無關。選取狀態由父組件自行管理。

### 參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `EnableRowSelection` | `bool` | 啟用點擊列選取（切換選取狀態） |
| `AllowMultipleSelection` | `bool` | 允許多選（否則為單選） |
| `ShowSelectColumn` | `bool` | 顯示獨立的 Checkbox 欄位 |
| `SelectedItems` | `HashSet<TItem>?` | 雙向綁定選取狀態（父組件管理） |
| `OnSelectionChanged` | `EventCallback<HashSet<TItem>>` | 選取變更事件 |
| `IsItemSelectable` | `Func<TItem, bool>?` | 特定列是否可選取 |
| `UnselectableTemplate` | `RenderFragment<TItem>?` | 不可選取列的替代內容 |
| `SelectColumnWidth` | `string` | Checkbox 欄寬（預設 `50px`） |

---

## Localization Keys

| Key | 說明 |
|-----|------|
| `Label.SelectAll` | 標頭全選 Checkbox tooltip |
| `Label.SelectItem` | 單列 Checkbox tooltip |
| `Label.SelectedCount` | 操作列顯示筆數（格式：`{0}` 為筆數） |
| `Button.Delete` | 批次刪除按鈕文字 |
| `Button.ClearSelection` | 清除選取按鈕文字 |
| `Button.ConfirmDelete` | 批次刪除確認 Modal 標題 |
| `Message.ConfirmBatchDelete` | 批次刪除確認訊息（格式：`{0}` 筆數、`{1}` EntityName） |

---

## 注意事項

- **dark mode 選取高亮**：`.row-selected` 使用 `rgba(88, 166, 255, 0.15)` 而非 CSS 變數，因 `--bs-primary-bg-subtle` 在 dark mode 下顏色不合適
- **Blazor CSS 隔離**：`GenericTableComponent.razor.css` 的 `.row-selected` 不會套用到 `GenericInteractiveTableComponent`，後者有獨立的 `.razor.css`