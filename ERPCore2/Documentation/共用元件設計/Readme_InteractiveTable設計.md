# GenericInteractiveTableComponent 設計說明

## 相關檔案總覽

```
Components/Shared/Table/
├── GenericInteractiveTableComponent.razor      # 主組件（Razor + C#）
├── InteractiveColumnDefinition.cs              # 欄位定義類別
├── InteractiveColumnType.cs                    # 欄位類型列舉
└── Readme_InteractiveTable.md                  # 本文件

Helpers/
├── DragDropState.cs                            # 拖放全域靜態狀態
├── DragDropEventArgs.cs                        # 跨表格拖放事件參數
└── InteractiveTableComponentHelper/
    ├── SearchableSelectHelper.cs               # 可搜尋下拉選單通用邏輯
    ├── CalculationHelper.cs                    # 計算輔助（小計、總計等）
    ├── DetailLockHelper.cs                     # 明細鎖定狀態管理
    ├── DetailSyncHelper.cs                     # 明細同步邏輯
    ├── HistoryCheckHelper.cs                   # 歷史記錄查詢輔助
    ├── InputEventHelper.cs                     # 輸入事件處理輔助
    ├── ItemManagementHelper.cs                 # 項目新增/刪除管理
    └── ValidationHelper.cs                     # 欄位驗證輔助

wwwroot/js/
├── interactiveTableDropdown.js                 # SearchableSelect 下拉定位（position: fixed）
├── drag-drop-helper.js                         # 拖放初始化（reinitDragDrop）
└── number-input-helper.js                      # 數字輸入輔助 + setInputValue
```

---

## 欄位類型（InteractiveColumnType）

| 類型 | 說明 |
|------|------|
| `Display` | 純顯示文字，支援 DisplayFormatter、NumberFormat、SmartDecimalDisplay |
| `Input` | 文字輸入框 |
| `Number` | 數字輸入框，預設啟用千分位顯示（UseThousandsSeparator），聚焦時自動切換為純數字 |
| `Select` | 固定選項的下拉選單，選項由 `Options: List<InteractiveSelectOption>` 提供 |
| `SearchableSelect` | 可輸入搜尋的動態下拉選單，下拉位置使用 position: fixed 避免被裁切 |
| `Checkbox` | 切換開關（form-switch 樣式），可設定 CheckedText / UncheckedText |
| `Button` | 行內按鈕，使用 GenericButtonComponent |
| `Custom` | 自訂 RenderFragment，父組件完全控制渲染內容 |

---

## 主要功能

### 自動空行管理（EnableAutoEmptyRow）
- 啟用後，表格底部始終保留一個空白輸入行，供使用者直接輸入新增資料
- `CreateEmptyItem`：父組件提供建立空行物件的工廠方法
- `TriggerEmptyRowOnFilled`：欄位設定此旗標後，該欄位有值時立即新增下一個空行
- `AllowAddNewRow`：設為 false 時清除所有空行（例如唯讀模式）
- `DataLoadCompleted`：父組件控制資料載入完成時機，避免資料尚未載入就新增空行
- 空行判斷邏輯：優先使用有 `TriggerEmptyRowOnFilled` 的觸發欄位；否則檢查所有非唯讀、非排除欄位
- `ExcludeFromEmptyCheck`：讓備註等輔助欄位不參與空行判斷
- `EmptyCheckPropertyName`：Custom 欄位指定實際要檢查的屬性（例如 SelectedProduct）
- 公開方法 `RefreshEmptyRow()`：父組件手動觸發空行重整
- 公開方法 `IsLastEmptyRow(item)`：判斷某行是否為唯一空行（供父組件決定是否顯示刪除按鈕）

### 行選取（Row Selection）
兩種模式可同時使用：
- `EnableRowSelection + OnRowClick`：點擊整行觸發選取，不顯示額外欄位
- `ShowSelectColumn`：顯示獨立的 Checkbox 欄位；支援單選／多選（AllowMultipleSelection）
- `IsItemSelectable`：動態判斷特定行是否可選取
- `UnselectableTemplate`：不可選取時的替代顯示內容
- 全選 Checkbox 支援 indeterminate 狀態（部分選取）

### 拖放（Drag & Drop）
- `EnableDrag`：此表格的行可被拖曳
- `EnableDrop`：此表格可接受放置
- `DragDropGroup`：相同 group 字串的表格之間才能互相拖放
- 同表格拖放：觸發 `OnRowReordered(oldIndex, newIndex)`，父組件負責實際重排
- 跨表格拖放：觸發 `OnItemDropped(DragDropEventArgs<TItem>)`
- `DragDropState`（靜態類別）：跨組件傳遞拖曳中的項目與來源資訊
- `IsDraggable`：動態判斷特定行是否可拖曳

### 內建刪除按鈕（ShowBuiltInActions）
- `ShowBuiltInDeleteButton`：顯示垃圾桶刪除按鈕，觸發 `OnItemDelete`
- `CustomActionsTemplate`：在刪除按鈕左側插入自訂操作按鈕
- `IsDeleteDisabled`：動態判斷特定行是否禁止刪除
- 啟用 `EnableAutoEmptyRow` 時，最後一個空行的刪除按鈕自動禁用

### 鍵盤導航（Keyboard Navigation）
- 適用於 `SearchableSelect` 和 `Custom` 欄位中的下拉選單
- 欄位設定 `EnableKeyboardNavigation = true` 後，支援 ArrowUp、ArrowDown、Enter（選取）、Escape（關閉）
- 使用 `scrollDropdownItemIntoView` JS 函式確保選中項目在可視區域內

### 數字千分位（Number 欄位）
- `UseThousandsSeparator = true`（預設）：使用 text 輸入框，顯示千分位格式（如 1,234,567）
- 聚焦時：透過 `setInputValue` JS 函式切換為無格式純數字
- 失焦時：`StateHasChanged()` 重新格式化顯示
- `UseThousandsSeparator = false`：改用原生 `<input type="number">`，禁止輸入 e/E/+/-

### 驗證（Validation）
- `IsRequired`：欄位必填驗證
- `ValidationPattern`：正規表達式格式驗證
- `MinValue / MaxValue`：數字範圍驗證
- `ValidationErrors`：父組件注入錯誤字典，key 格式為 `{item.GetHashCode()}_{propertyName}`，顯示 `is-invalid` CSS
- `OnValidationFailed`：驗證失敗回呼

### 顯示格式化（Display 欄位）
- `DisplayFormatter(item)`：接收整個 row 物件，可跨屬性組合顯示，回傳支援 HTML 標籤
- `SmartDecimalDisplay`：整數不顯示小數點，有小數才顯示（例如 100 vs 100.50）
- `NumberFormat`：標準數字格式字串（N0、N2、C、P 等）
- 優先順序：`DisplayFormatter` > `SmartDecimalDisplay` > `NumberFormat` > 預設顯示

---

## SearchableSelect 進階設計

SearchableSelect 欄位的狀態（搜尋文字、過濾清單、下拉開關、選中索引）儲存在**每個 row 物件本身的屬性**中，而非組件內部狀態。欄位定義中的 `*PropertyName` 屬性告訴組件到哪個屬性讀寫：

| 屬性 | 說明 |
|------|------|
| `SearchValuePropertyName` | row 物件上儲存搜尋文字的屬性名稱 |
| `FilteredItemsPropertyName` | row 物件上儲存過濾結果清單的屬性名稱 |
| `ShowDropdownPropertyName` | row 物件上儲存下拉開關 bool 的屬性名稱 |
| `SelectedIndexPropertyName` | row 物件上儲存鍵盤導航索引的屬性名稱 |
| `ItemDisplayFormatter` | 下拉選單項目的顯示格式（例如 `[CODE] Name`） |

`SearchableSelectHelper` 提供通用的搜尋過濾和商品選取邏輯，減少各頁面重複程式碼。

---

## 效能設計說明

| 機制 | 說明 |
|------|------|
| Reflection 快取 | `_propCache`（靜態 ConcurrentDictionary）跨實例共享 PropertyInfo，避免重複反射 |
| 參數快取 | `_cachedTableClass`、`_cachedActionsColumnStyle`、`_cachedColspan` 在 `OnParametersSet` 計算一次 |
| 空行數量快取 | `cachedEmptyRowCount` 在 render 開始前計算一次（razor 區域），避免每行重複 Count |
| Drag 事件防抖 | `HandleTableDragOver`、`HandleTableDragLeave`、`HandleRowDragEnter` 加 guard 條件，狀態未變時不呼叫 `StateHasChanged` |
| JS Interop 守衛 | `reinitDragDrop` 和 `setCheckboxIndeterminate` 僅在相關狀態改變時才呼叫，避免每次 render 觸發 |
| 無 List 分配 | `GetCombinedRowClass`、`GetCellStyle`、`GetRowStyle`、`GetTableBodyClass` 常見路徑直接 return 字串，不建立 List |
