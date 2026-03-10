# Table 右鍵選單與多選功能總綱

## 更新日期
2026-03-10

---

## 📋 概述

GenericTableComponent 與 GenericIndexPageComponent 提供完整的右鍵選單與多選功能。Index 頁面透過參數宣告即可自動生成包含「編輯、列印、刪除」的右鍵選單，以及多選批次操作列，無需在個別頁面撰寫重複邏輯。

---

## 🏗️ 功能架構

```
┌─────────────────────────────────────────────────────────────┐
│                  Table 右鍵與多選功能                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────────┐   ┌───────────────────────────┐   │
│  │   右鍵選單           │   │   多選功能                 │   │
│  │                     │   │                           │   │
│  │  ┌───────────────┐  │   │  ┌─────────────────────┐  │   │
│  │  │ 編輯          │  │   │  │ GenericTableComponent│  │   │
│  │  │ (OnRowClick)  │  │   │  │ Checkbox 欄位        │  │   │
│  │  ├───────────────┤  │   │  └──────────┬──────────┘  │   │
│  │  │ 列印          │  │   │             │              │   │
│  │  │ (RowPrint     │  │   │  ┌──────────▼──────────┐  │   │
│  │  │  Service)     │  │   │  │ GenericIndexPage     │  │   │
│  │  ├───────────────┤  │   │  │ 操作列 + 批次刪除    │  │   │
│  │  │ ─────────     │  │   │  └─────────────────────┘  │   │
│  │  ├───────────────┤  │   │                           │   │
│  │  │ 刪除          │  │   │  ┌─────────────────────┐  │   │
│  │  │ (Confirm      │  │   │  │ InteractiveTable     │  │   │
│  │  │  Modal)       │  │   │  │ 獨立多選設計         │  │   │
│  │  └───────────────┘  │   │  └─────────────────────┘  │   │
│  └─────────────────────┘   └───────────────────────────┘   │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📚 詳細文件

| 文件 | 功能說明 |
|------|----------|
| [Readme_Table右鍵選單.md](Readme_Table右鍵選單.md) | 右鍵選單機制、`ContextMenuItem<TItem>` 屬性、各組件參數、自動生成邏輯（編輯/刪除） |
| [Readme_Table右鍵列印.md](Readme_Table右鍵列印.md) | 右鍵直接列印功能、`RowPrintService` / `RowPrintReportId` 參數、使用示範 |
| [Readme_Table多選功能.md](Readme_Table多選功能.md) | 標準多選（GenericTableComponent + GenericIndexPageComponent）、InteractiveTable 多選 |

---

## 📁 相關原始碼

| 檔案 | 說明 |
|------|------|
| `Components/Shared/Table/ContextMenuItem.cs` | 右鍵選單項目定義 |
| `Components/Shared/Table/GenericTableComponent.razor` | 唯讀表格組件（右鍵 + 多選） |
| `Components/Shared/Table/GenericTableComponent.razor.css` | 選單樣式、已選取列高亮 |
| `Components/Shared/Table/GenericInteractiveTableComponent.razor` | 互動式表格組件（右鍵 + 獨立多選） |
| `Components/Shared/Table/GenericInteractiveTableComponent.razor.css` | 同上，CSS 隔離獨立定義 |
| `Components/Shared/Page/GenericIndexPageComponent.razor` | Index 基底：多選操作列、所有 Modal 渲染 |
| `Components/Shared/Page/GenericIndexPageComponent.razor.cs` | 所有參數、私有狀態、`BuildEffectiveContextMenuItems` |
| `Components/Shared/Page/GenericIndexPageComponent.Actions.cs` | `ExecuteContextMenuDeleteAsync`、`ExecuteMultiDeleteAsync` |
| `Components/Shared/Page/GenericIndexPageComponent.DataLoader.cs` | `RefreshData()` 刷新時清除 `_selectedItems` |
| `Components/Shared/Report/ReportPreviewModalComponent.razor` | 列印預覽 Modal（由右鍵列印功能使用） |
| `Components/Shared/UI/GenericConfirmModalComponent.razor` | 刪除確認 Modal（右鍵刪除 + 批次刪除） |
| `wwwroot/js/` | `setCheckboxIndeterminate`、`tableContextMenu.adjustPosition` |

---

## 🔄 自動生成選單項目對照

`GenericIndexPageComponent` 的 `BuildEffectiveContextMenuItems` 根據已設定的參數自動決定選單內容：

| 條件 | 自動加入項目 |
|------|-------------|
| `EnableRowClick && OnRowClick.HasDelegate` | 「編輯」 |
| `RowPrintService != null \|\| OnRowPrint.HasDelegate` | 「列印」 |
| `ShowDeleteButton == true` | 分隔線 + 「刪除」 |
| `ContextMenuItems` 傳入非空列表 | 直接使用，不自動生成 |
| `ContextMenuItems` 傳入空列表 | 停用選單 |

---

## CSS 類別

| Class | 定義位置 | 說明 |
|-------|----------|------|
| `.generic-table-wrapper` | `GenericTableComponent.razor.css` / `GenericInteractiveTableComponent.razor.css` | 外層包裝，作為 `position: fixed` 基準 |
| `.table-context-menu-overlay` | 同上 | 透明全螢幕覆蓋層，點擊後關閉選單 |
| `.table-context-menu` | 同上 | 選單本體（`position: fixed`） |
| `.table-context-menu-item` | 同上 | 選單按鈕項目 |
| `.table-context-menu-divider` | 同上 | 選單分隔線 |
| `.row-selected` | `GenericTableComponent.razor.css` | 已選取列的高亮背景 |
