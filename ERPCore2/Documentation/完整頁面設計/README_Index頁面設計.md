# Index 頁面設計

## 更新日期
2026-03-07

---

## 概述

Index 頁面是每個業務實體的列表檢視，使用 `GenericIndexPageComponent<TEntity, TService>` 提供搜尋、篩選、表格顯示、分頁及刪除。每個 Index 頁面搭配一個 `FieldConfiguration` 類別來定義篩選器與表格欄位。

---

## 檔案結構

| 檔案 | 路徑 |
|------|------|
| Index 頁面 | `Components/Pages/{Module}/{Entity}IndexPage.razor` |
| 欄位配置 | `Components/FieldConfiguration/{Entity}FieldConfiguration.cs` |

---

## GenericIndexPageComponent 參數

### 必填

| 參數 | 說明 |
|------|------|
| `TEntity` / `TService` | 實體類型與服務介面 |
| `Service` | 注入的服務實例 |
| `DataLoader` | `Func<Task<List<TEntity>>>` — 載入資料 |
| `FilterApplier` | `Func<SearchFilterModel, IQueryable<TEntity>, IQueryable<TEntity>>` — 套用篩選 |
| `FilterDefinitions` | 篩選欄位定義清單（由 FieldConfiguration 產生） |
| `ColumnDefinitions` | 表格欄位定義清單（由 FieldConfiguration 產生） |
| `RequiredPermission` | 頁面權限字串，如 `"Customer.Read"` |

### 選填 — 頁面文字

| 參數 | 預設行為 |
|------|----------|
| `PageTitle` / `PageSubtitle` | 頁面標題與副標題 |
| `EntityName` | 實體名稱，用於自動產生按鈕文字、錯誤訊息等（支援多語系） |
| `AddButtonText` / `AddButtonTitle` | 未設定時由 `Button.AddEntity` 格式字串自動產生 |
| `SearchSectionTitle` | 未設定時由 `Label.SearchAndManage` 自動產生 |
| `EmptyMessage` | 未設定時由 `Message.EntityNoResults` 自動產生 |

### 選填 — 模組與除錯

| 參數 | 說明 |
|------|------|
| `RequiredModule` | 公司模組 key，未啟用時顯示封鎖畫面（SuperAdmin 可繞過） |
| `DebugPageName` | SuperAdmin 可見的除錯標籤，顯示頁面元件名稱 |

### 選填 — 自動標準欄位

| 參數 | 預設 | 說明 |
|------|------|------|
| `AutoAddRemarksColumn` | `true` | 自動在表格末尾加備註欄 |
| `AutoAddRemarksFilter` | `true` | 自動加備註篩選器 |
| `AutoAddCreatedAtColumn` | `false` | 自動加建立日期欄 |
| `RemarksColumnTitle` / `CreatedAtColumnTitle` / `RemarksFilterTitle` | `""` | 空時使用 L["Label.Remarks"] 等 key |

### 選填 — 操作按鈕

| 參數 | 說明 |
|------|------|
| `ShowAddButton` | 預設 `true` |
| `ShowExportExcelButton` / `ShowExportPdfButton` | 匯出按鈕，搭配對應 EventCallback |
| `ShowBatchPrintButton` / `ShowBarcodePrintButton` | 批次列印，搭配對應 EventCallback |
| `ShowBatchApprovalButton` / `ShowImportScheduleButton` | 批次審核 / 匯入排程 |
| `ShowBatchDeleteButton` / `IsBatchDeleteDisabled` | 批次刪除 |
| `CustomActionButtons` | 完全取代預設按鈕列 |
| `CustomIndexButtons` | 附加在預設按鈕列尾端 |

### 選填 — 刪除

| 參數 | 說明 |
|------|------|
| `ShowDeleteButton` | 預設 `true` |
| `EnableStandardActions` | 預設 `true`，自動加操作欄 |
| `EnableSystemDataProtection` | 預設 `true`，`CreatedBy = "System"` 的資料不可刪 |
| `CanDelete` | `Func<TEntity, bool>` — 自訂可刪判斷 |
| `CustomDeleteHandler` | `Func<TEntity, Task<bool>>` — 完全取代刪除邏輯 |
| `DeleteSuccessMessage` / `DeleteConfirmMessage` | 空時由多語系 key 自動產生 |
| `GetEntityDisplayName` | 刪除確認訊息中顯示的實體名稱，預設 `entity.Id.ToString()` |

### 選填 — 表格與分頁

| 參數 | 預設 |
|------|------|
| `EnableRowClick` | `true` |
| `EnableSorting` | `false` |
| `IsStriped` / `IsHoverable` / `IsBordered` | `true` / `true` / `false` |
| `TableSize` | `TableSize.Normal` |
| `EnablePagination` | `true` |
| `DefaultPageSize` | `20` |
| `ShowPageSizeSelector` | `true` |

### 選填 — 統計卡片

| 參數 | 說明 |
|------|------|
| `ShowStatisticsCards` | 預設 `false` |
| `StatisticsCardConfigs` | `List<StatisticsCardConfig>` — 卡片設定 |
| `StatisticsDataLoader` | `Func<Task<Dictionary<string, object>>>` — 統計資料載入，每次 Refresh 時呼叫 |

> 注意：`StatisticsData` 已不再是 `[Parameter]`，統計資料完全由 `StatisticsDataLoader` 提供，元件內部維護私有欄位 `_statisticsData`。

### 選填 — 自訂範本

| 參數 | 說明 |
|------|------|
| `ActionsTemplate` | `RenderFragment<TEntity>` — 完全取代操作欄 |
| `CustomActionsTemplate` | `RenderFragment<TEntity>` — 附加在刪除按鈕前 |

> 這兩個參數的變更會觸發欄位定義快取重建。

### 選填 — 導航

| 參數 | 說明 |
|------|------|
| `EntityBasePath` | 導航基礎路徑，如 `/customers` |
| `CreateUrl` / `EditUrl` / `DetailUrl` | 覆蓋自動產生的 URL，`{id}` 會被替換 |
| `OnAddClick` / `OnRowClick` | 覆蓋新增 / 列點擊的預設導航行為 |

---

## 公開 API（@ref 呼叫）

| 方法 | 說明 |
|------|------|
| `Refresh()` | 平滑刷新（重新載入資料，保留當前頁碼） |
| `Refresh(RefreshMode)` | 指定刷新方式（Smooth / ForceReload） |
| `SmoothRefresh()` / `ForceRefresh()` | 語意化版本 |
| `ReloadData()` | 只重載資料，不重載基礎資料與統計 |
| `ResetFilters()` | 清空篩選條件並重新套用 |
| `NavigateToCreate()` / `NavigateToEdit(entity)` / `NavigateToDetail(entity)` | 程式導航 |
| `DeleteEntityAsync(entity)` | 觸發刪除流程 |
| `GetStandardActionsTemplate()` | 取得預設操作欄範本（含自訂 + 刪除按鈕） |

---

## FieldConfiguration

### 繼承與職責

繼承 `BaseFieldConfiguration<TEntity>`，集中管理：
- 篩選器定義（`BuildFilters()`）
- 表格欄位定義（`BuildColumns()`）
- 篩選邏輯（`ApplyFilters()`）

### 多語系整合

在 Index 頁面 `OnInitializedAsync` 中建立後，必須呼叫 `SetLocalizer(L)`：

- `Dn(key, fallback)` — 解析 DisplayName
- `Fp(key, fallback)` — 解析 FilterPlaceholder（格式：`Placeholder.InputToSearch`）
- `Nd(key, fallback)` — 解析 NullDisplayText

---

## Index 頁面標準流程

`OnInitializedAsync` 中依序執行：

1. 初始化 `ModalHandler`（若使用 Modal 編輯）
2. 初始化麵包屑（`BreadcrumbHelper`）
3. 建立 `FieldConfiguration` 並呼叫 `SetLocalizer(L)`
4. `BuildFilters()` / `BuildColumns()` 產生定義清單
5. 將定義清單傳入 `GenericIndexPageComponent` 的參數

資料載入使用 `DataLoaderHelper.LoadAsync()`。

---

## 相關 Helper

| Helper | 主要用途 |
|--------|----------|
| `ModalHelper.CreateModalHandler` | Modal 新增 / 編輯狀態管理 |
| `BreadcrumbHelper` | 建立麵包屑導航 |
| `DataLoaderHelper.LoadAsync` | 統一資料載入與錯誤處理 |
| `FilterHelper` | 提供文字 / 數值 / 日期 / 外鍵篩選函數 |

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md)
- [README_EditModal設計.md](README_EditModal設計.md)
