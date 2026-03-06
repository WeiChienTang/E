# EditModal 設計

## 更新日期
2026-03-07

---

## 概述

EditModal 是每個業務實體的新增/編輯表單，使用 `GenericEditModalComponent<TEntity, TService>` 提供完整的 CRUD 操作介面，包括資料載入、表單渲染、驗證、儲存、上下筆導航、列印、自訂內容 Tab 等功能。

---

## 檔案結構

| 檔案 | 路徑 | 說明 |
|------|------|------|
| EditModal | `Components/Pages/{Module}/{Entity}EditModalComponent.razor` | 編輯 Modal 元件 |
| GenericEditModal（主體） | `Components/Shared/Modal/GenericEditModalComponent.razor` | 模板與參數宣告 |
| GenericEditModal（生命週期） | `GenericEditModalComponent.Lifecycle.cs` | OnParametersSetAsync、LoadAllData、ShouldRender |
| GenericEditModal（儲存） | `GenericEditModalComponent.Save.cs` | HandleSave、GenericSave、HandleCancel、HandleFieldChanged |
| GenericEditModal（刪除） | `GenericEditModalComponent.Delete.cs` | DeleteEntityAsync、CanDeleteAsync、PermanentDeleteAsync |
| GenericEditModal（導航） | `GenericEditModalComponent.Navigation.cs` | 上下筆、HandleAddClick、ApplyNavigatedEntityAsync |
| GenericEditModal（審核） | `GenericEditModalComponent.Approval.cs` | HandleApproveConfirm、HandleRejectConfirm |
| GenericEditModal（AutoComplete） | `GenericEditModalComponent.AutoComplete.cs` | BuildProcessedFormFields、UpdateAllActionButtons |
| GenericEditModal（輔助） | `GenericEditModalComponent.Helpers.cs` | 列印、Modal 開關、狀態訊息 |
| GenericEditModal（型別） | `GenericEditModalComponent.Types.cs` | CustomModule、ModalSize、BadgeVariant |

---

## GenericEditModalComponent 參數

### 核心參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `@bind-Id` | `int?` | 實體 ID（雙向綁定，上下筆導航時自動更新） |
| `Service` | `TService` | 實體服務 |
| `EntityName` | `string` | 實體名稱（用於訊息顯示） |
| `EntityNamePlural` | `string` | 實體複數名稱 |
| `ModalTitle` | `string` | Modal 標題 |
| `Size` | `ModalSize` | Modal 尺寸（Desktop / Large / ExtraLarge / Default / Small） |
| `RequiredPermission` | `string` | 必要權限（未設定時預設拒絕存取） |
| `ComponentName` | `string?` | Debug Bar 顯示名稱（僅 SuperAdmin 可見） |

### 表單參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `UseGenericForm` | `bool` | 使用 GenericFormComponent 渲染表單 |
| `FormFields` | `List<FormFieldDefinition>` | 直接綁定 `@formFields`（不使用 wrapper 方法） |
| `FormSections` | `Dictionary<string, string>` | 欄位區段映射 |
| `TabDefinitions` | `List<FormTabDefinition>?` | Tab 頁籤定義 |
| `FormHeaderContent` | `RenderFragment?` | 表單頂部自訂內容（欄位之前） |
| `CustomFormContent` | `RenderFragment?` | 自訂表單內容（欄位之後） |
| `AdditionalSections` | `RenderFragment?` | 額外區段 |
| `CustomModules` | `List<CustomModule>` | 自訂模組系統（有順序控制） |
| `CustomActionButtons` | `RenderFragment?` | Modal 頂部左側自訂按鈕區 |
| `OnFieldChanged` | `Func<(string, object?), Task>?` | 欄位變更回呼（僅有非 ActionButton 自訂邏輯時才綁定） |
| `OnTaxMethodChanged` | `Func<Task>?` | 稅別欄位變更時的專用回呼（自動偵測 TaxCalculationMethod 欄位） |

### AutoComplete 參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `AutoCompletePrefillers` | `Dictionary<string, Func<string, Dictionary<string, object?>>>?` | 新增時智能預填函數 |
| `AutoCompleteCollections` | `Dictionary<string, IEnumerable<object>>?` | 本地搜尋集合 |
| `AutoCompleteDisplayProperties` | `Dictionary<string, string>?` | 顯示屬性名稱（預設 `"Name"`） |
| `AutoCompleteValueProperties` | `Dictionary<string, string>?` | 值屬性名稱（預設 `"Id"`） |
| `AutoCompleteMaxResults` | `Dictionary<string, int>?` | 最大搜尋結果數 |
| `ModalManagers` | `Dictionary<string, object>?` | 使用 `modalManagers?.AsDictionary()` 直接綁定 |

### 儲存參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `DataLoader` | `Func<Task<TEntity?>>?` | 資料載入函數 |
| `UseGenericSave` | `bool` | 使用內建儲存流程（呼叫 `IGenericManagementService<T>.CreateAsync/UpdateAsync`） |
| `SaveHandler` | `Func<TEntity, Task<bool>>?` | 自訂儲存邏輯（覆蓋內建儲存） |
| `CustomValidator` | `Func<TEntity, Task<bool>>?` | 儲存前自訂驗證 |
| `BeforeSave` | `Func<TEntity, Task>?` | 儲存前鉤子 |
| `AfterSave` | `Func<TEntity, Task>?` | 儲存後鉤子 |
| `SaveSuccessMessage` | `string` | 儲存成功訊息 |
| `SaveFailureMessage` | `string` | 儲存失敗訊息 |
| `CloseOnSave` | `bool` | 儲存成功後自動關閉（預設 `false`） |
| `ShowUnsavedChangesWarning` | `bool` | 關閉時若有未儲存修改是否警告（預設 `true`） |

### 按鈕顯示參數

| 參數 | 類型 | 預設 | 說明 |
|------|------|------|------|
| `ShowSaveButton` | `bool` | `true` | 儲存按鈕 |
| `ShowAddButton` | `bool` | `true` | 新增按鈕（僅編輯模式顯示） |
| `ShowRefreshButton` | `bool` | `true` | 重新整理按鈕（僅編輯模式顯示） |
| `ShowDeleteButton` | `bool` | `true` | 刪除按鈕（僅編輯模式且可刪除時顯示） |
| `ShowReturnToLastButton` | `bool` | `true` | 返回編輯按鈕（新增模式且有上一筆時顯示） |
| `ShowPrintButton` | `bool` | `false` | 列印按鈕 |
| `ShowPreviewButton` | `bool` | `false` | 預覽按鈕 |
| `EnableNavigation` | `bool` | `true` | 上下筆導航按鈕（目前功能有已知問題，詳見 Readme_上下筆失敗測試） |
| `SaveButtonText` | `string` | `"儲存"` | 儲存按鈕文字 |
| `PrintButtonText` | `string` | `"列印"` | 列印按鈕文字 |
| `PreviewButtonText` | `string` | `"預覽"` | 預覽按鈕文字 |
| `ReturnToLastButtonText` | `string` | `"返回編輯"` | 返回按鈕文字 |

### 刪除參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `GetEntityDisplayName` | `Func<TEntity, string>` | 取得實體顯示名稱（用於刪除確認訊息） |
| `DeleteSuccessMessage` | `string` | 刪除成功訊息（空白時自動產生） |
| `DeleteConfirmMessage` | `string` | 刪除確認訊息（支援 `{0}` 替換為顯示名稱） |
| `CanDelete` | `Func<TEntity, bool>?` | 自訂可刪除條件 |
| `EnableSystemDataProtection` | `bool` | 是否保護 `CreatedBy="System"` 的資料（預設 `true`） |
| `CustomDeleteHandler` | `Func<TEntity, Task<bool>>?` | 自訂刪除邏輯 |

### 列印參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `ReportService` | `IEntityReportService<TEntity>?` | 報表服務（設定後自動整合列印功能） |
| `ReportId` | `string` | 報表識別碼（如 `"SO004"`） |
| `ReportPreviewTitle` | `string` | 報表預覽標題 |
| `GetReportDocumentName` | `Func<TEntity, string>?` | 產生報表文件名稱的函式 |
| `CanPrintCheck` | `Func<bool>?` | 列印前審核狀態檢查（回傳 `false` 時阻止列印） |

### 審核參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `ShowApprovalSection` | `bool` | 是否顯示審核按鈕區（預設 `false`） |
| `ApprovalPermission` | `string` | 審核操作所需權限 |
| `OnApprove` | `Func<Task<bool>>?` | 審核通過回呼 |
| `OnRejectWithReason` | `Func<string, Task<bool>>?` | 駁回（附原因）回呼 |

### 狀態訊息參數

| 參數 | 類型 | 說明 |
|------|------|------|
| `ShowStatusMessage` | `bool` | 是否顯示狀態訊息徽章（預設 `false`） |
| `StatusMessage` | `string?` | 靜態狀態訊息 |
| `StatusBadgeVariant` | `BadgeVariant` | 徽章顏色（Primary/Secondary/Success/Danger/Warning/Info/Light/Dark） |
| `StatusIconClass` | `string` | 徽章圖示 CSS 類別 |
| `GetStatusMessage` | `Func<Task<(string, BadgeVariant, string)?>>?` | 動態取得狀態訊息（優先於 `StatusMessage`） |

### 事件回呼

| 參數 | 類型 | 說明 |
|------|------|------|
| `OnEntitySaved` | `EventCallback<TEntity>` | 儲存成功，傳回已儲存實體 |
| `OnSaveSuccess` | `EventCallback` | 儲存成功（不傳參數，可與 `OnEntitySaved` 共存） |
| `OnSaveFailure` | `EventCallback` | 儲存失敗 |
| `OnDeleteSuccess` | `EventCallback` | 刪除成功（注意：語義上有別於 `OnSaveSuccess`） |
| `OnCancel` | `EventCallback` | 取消/關閉 |
| `OnPrint` | `EventCallback` | 列印（未設定 `ReportService` 時由外部處理） |
| `OnPreview` | `EventCallback` | 預覽 |
| `OnPrintSuccess` | `EventCallback` | 列印成功 |
| `OnEntityLoaded` | `EventCallback<int>` | 上下筆切換載入完成（傳回載入的實體 ID） |

---

## 重要設計規則

### 1. Lazy Loading（必須遵守）

子 EditModal 在 `OnInitializedAsync` 中只初始化 ModalManager，不載入資料。
資料載入統一在 `OnParametersSetAsync` 中以 `isDataLoaded` 旗標實作 Lazy Loading。
關閉時重置 `isDataLoaded = false` 讓下次開啟重新載入。

> **不要設定 `AdditionalDataLoader` 參數**，會導致重複載入。

### 2. `@bind-Id` 雙向綁定

必須使用 `@bind-Id` 而非單向的 `Id=`，讓上下筆導航時能自動更新父元件的 ID。

### 3. OnEntitySaved 與 OnCancel 直接傳遞

直接將父元件的 EventCallback 傳入，不需要寫 `HandleSaveSuccess` 或 `HandleCancel` wrapper 方法。

### 4. OnDeleteSuccess 與 OnSaveSuccess 語義分離

刪除成功觸發 `OnDeleteSuccess`，儲存成功觸發 `OnSaveSuccess`/`OnEntitySaved`，兩者不混用。
父元件若需要在刪除後刷新清單，應綁定 `OnDeleteSuccess` 而非 `OnSaveSuccess`。

### 5. 新增按鈕（ShowAddButton）的行為

「儲存後留在新增模式」**只在使用者明確點擊「新增」按鈕後**才會觸發（內部 `_stayInAddMode` 旗標）。
即使 `ShowAddButton=true`，直接點「儲存」不會自動切換到新增模式。
若在 `OnEntitySaved` 中關閉子 Modal（如 `showModal = false`），不會因此引發組件 Dispose 競爭問題。

### 6. ActionButton 更新機制

AutoComplete 欄位的新增/編輯按鈕（`ActionButtons`）由 `GenericEditModalComponent` 在 `OnFieldChanged` 時自動更新。
不需要在 `OnFieldChanged` 中手動呼叫 `UpdateFieldActionButtonsAsync`。
`OnFieldChanged` 僅在有「非 ActionButton 的自訂業務邏輯」時才需要綁定。

### 7. ModalManager 關聯實體

使用 `ModalManagerInitHelper.CreateBuilder()` 初始化，並以 `modalManagers?.AsDictionary()` 直接綁定到 `ModalManagers` 參數。
關聯 Modal 的儲存事件使用 `xxxModalManager.OnSavedAsync` 直接綁定，無需寫 wrapper。

### 8. 巢狀 Modal 條件式渲染

有可能造成循環實例化的巢狀 Modal（如 Customer → Vehicle → Customer）必須使用 `@if (isModalVisible)` 條件式渲染，避免常駐 DOM。

### 9. Table 明細 DirtyCheck

`HandleDetailsChanged` 的第一行必須呼叫 `editModalComponent?.MarkDirty()`，讓關閉時的未儲存警告正確觸發。
Table 的 `<InteractiveTableComponent>` 必須綁定 `OnDataChanged`，確保所有儲存格變更都能傳遞到 EditModal。

### 10. OnEntityLoaded（上下筆切換）

只有在上下筆切換時需要重載額外資料（如自訂 Tab 的子資料）才需要綁定此事件。
純主檔欄位的 EditModal 不需要，GenericEditModalComponent 內部已自動 `StateHasChanged()`。

---

## 常用 Helper

| Helper | 用途 |
|--------|------|
| `ModalManagerInitHelper` | 初始化關聯實體 Modal 管理器 |
| `AutoCompleteConfigBuilder<T>` | 建立 AutoComplete 配置（含多欄位搜尋、條件式配置） |
| `FormSectionHelper<T>` | 定義欄位區段與 Tab（需要 Tab 時用 `.BuildAll()`） |
| `ActionButtonHelper` | 產生 AutoComplete 欄位的新增/編輯按鈕 |
| `EntityCodeGenerationHelper` | 新增模式自動產生單據編號（讀取 `[EntityCode]` Attribute） |
| `FormFieldConfigurationHelper` | 建立常用欄位（備註、編號、狀態等） |
| `ApprovalConfigHelper` | 判斷審核狀態是否需要鎖定欄位或限制操作 |
| `TaxCalculationHelper` | 計算稅額與取得稅別選項 |
| `FormFieldLockHelper` | 批次鎖定/解鎖多個欄位（含 ActionButtons 同步） |

---

## 開發檢查清單

### 基本結構
- [ ] 使用 `GenericEditModalComponent<TEntity, TService>`，並宣告 `@ref="editModalComponent"`
- [ ] 使用 `@bind-Id` 綁定 ID（雙向）
- [ ] `FormFields="@formFields"` 直接綁定（不使用 wrapper 方法）
- [ ] `ModalManagers="@modalManagers?.AsDictionary()"` 直接綁定
- [ ] `OnEntitySaved="@OnYourEntitySaved"` 直接傳遞（不寫 HandleSaveSuccess wrapper）
- [ ] `OnCancel="@OnCancel"` 直接傳遞
- [ ] `OnDeleteSuccess` 與 `OnSaveSuccess` 分開使用，語義正確
- [ ] 關聯 Modal 使用 `OnXxxSaved="@xxxModalManager.OnSavedAsync"` 直接綁定

### 資料載入
- [ ] 實作 Lazy Loading（`OnParametersSetAsync` + `isDataLoaded`）
- [ ] `OnInitializedAsync` 中只初始化 ModalManager，不載入資料
- [ ] 未設定 `AdditionalDataLoader` 參數（避免重複載入）
- [ ] 若無需預先載入下拉選項，省略 `LoadAdditionalDataAsync`
- [ ] 若無需在上下筆切換時重載資料，省略 `OnEntityLoaded`

### Helper 使用
- [ ] 使用 `ModalManagerInitHelper` 管理關聯實體
- [ ] 使用 `AutoCompleteConfigBuilder` 建立 AutoComplete 配置
- [ ] 使用 `FormSectionHelper` 定義區段（需要 Tab 時用 `.BuildAll()`）
- [ ] 使用 `ActionButtonHelper` 產生欄位按鈕
- [ ] `OnFieldChanged` 僅在有非 ActionButton 自訂邏輯時才綁定

### Table 明細（含 Table 的 EditModal 必須檢查）
- [ ] `HandleDetailsChanged` 第一行是 `editModalComponent?.MarkDirty()`
- [ ] `<InteractiveTableComponent>` 已綁定 `OnDataChanged` 對應的通知方法
- [ ] `OnDataChanged` 綁定的方法存在於 Table 元件中（不存在會編譯錯誤）

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md)
- [README_FormField表單欄位設計.md](README_FormField表單欄位設計.md)
- [README_Index頁面設計.md](README_Index頁面設計.md)
- [Readme_DirtyCheck_EditModal與Table修改規範.md](../../Readme_DirtyCheck_EditModal與Table修改規範.md)
