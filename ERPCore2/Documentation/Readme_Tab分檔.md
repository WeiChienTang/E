# EditModal Tab 分檔設計規範

## 背景

隨著 EditModal 功能越來越豐富（基本資料、車輛、拜訪紀錄、工具配給…），
單一 `XxxEditModalComponent.razor` 檔案會超過千行，難以維護。

本規範參考 `SystemParameterSettingsModal` 的 Tab 分離模式，
定義一套標準作法，讓每個 Tab 成為獨立元件，主組件只負責協調。

---

## 資料夾結構

以 `EmployeeEditModal` 為例：

```
Components/Pages/Employees/
├── EmployeeIndex.razor               ← 列表頁（不移動）
├── EmployeeAccountModalComponent.razor
├── DepartmentEditModalComponent.razor
├── EmployeePositionEditModalComponent.razor
├── PersonalPreference/               ← 個人設定（獨立功能）
│   ├── PersonalPreferenceModalComponent.razor
│   ├── PersonalDataTab.razor
│   └── LanguageRegionTab.razor
└── EmployeeEditModal/                ← 員工編輯 Modal 專屬資料夾
    ├── EmployeeEditModalComponent.razor  ← 主組件（精簡版）
    ├── EmployeeVehicleTab.razor          ← 車輛資訊 Tab
    ├── EmployeeToolTab.razor             ← 個人工具 Tab
    ├── EmployeeToolTable.razor           ← 工具列表子元件
    └── EmployeeToolEditModalComponent.razor
```

**規則：**
- 資料夾名稱為 `{Entity}EditModal/`（如 `EmployeeEditModal/`、`CustomerEditModal/`）
- 主組件 `XxxEditModalComponent.razor` 搬進資料夾
- 每個 Custom Tab → 一個 `Xxx{功能}Tab.razor` 檔案
- Tab 專屬的子元件（Table、EditModal）也放入同一資料夾
- 獨立的 Index 頁面、獨立功能的 Modal 留在父資料夾

---

## Tab 元件設計

### 兩種 Tab 類型

#### 1. 標準表單 Tab（由主組件的 GenericEditModalComponent 管理）

適用：主要業務欄位（如「員工資料」、「客戶資料」）

這些 Tab 的欄位定義留在主組件的 `InitializeFormFieldsAsync()` 中，
透過 `.GroupIntoTab(...)` 掛入 `tabDefinitions`，不需要獨立 Tab 元件。

#### 2. 自訂 Tab（透過 `@ref` 由主組件協調）

適用：需要獨立 CRUD 的子資料（車輛、拜訪紀錄、工具配給…）

---

### Tab 元件模板

```razor
@* 員工工具 Tab - 自包含，主組件透過 @ref 協調 *@
@inject IEmployeeToolService EmployeeToolService
@inject INotificationService NotificationService

@* ── UI ── *@
<EmployeeToolTable ... />
<EmployeeToolEditModalComponent ... />
<GenericConfirmModalComponent ... />

@code {
    // ── 參數 ──
    [Parameter] public int? EmployeeId { get; set; }

    // ── 內部狀態 ──
    private List<EmployeeTool> employeeTools = new();
    private bool isToolModalVisible = false;
    // ...

    // ── 公開方法（主組件透過 @ref 呼叫） ──
    public async Task LoadAsync(int employeeId) { ... }
    public void Clear() { ... }

    // ── 私有事件處理 ──
    private void HandleAddTool() { ... }
    private void HandleEditTool(EmployeeTool tool) { ... }
    // ...
}
```

**設計原則：**
- Tab 元件注入自己需要的 Service，不依賴主組件傳入
- 狀態（列表、Modal 開關）完全自管
- 對外只暴露必要的**公開方法**：

| 方法 | 用途 | 常見場景 |
|------|------|----------|
| `LoadAsync(int id)` | 從 DB 載入資料 | 切換記錄、儲存後刷新 |
| `Clear()` | 清空列表（新增模式） | 開啟新增 Modal 時 |
| `SavePendingChangesAsync(int id)` | 提交暫存變更 | 主檔儲存後寫入子資料（Vehicle 配給模式）|
| `ReloadAsync(int id)` | 重新載入 | 儲存成功後確保 UI 同步 |

> **注意：** 並非每個 Tab 都需要全部方法。
> - 直接寫入 DB 的 Tab（工具、拜訪紀錄）：只需 `LoadAsync` + `Clear`
> - 有暫存緩衝的 Tab（車輛配給）：額外需要 `SavePendingChangesAsync` + `ReloadAsync`

---

## 主組件設計

主組件職責收縮為「協調者」：

```razor
@code {
    // Tab 元件參考
    private CustomerVehicleTab? vehicleTab;
    private CustomerVisitTab? visitTab;

    // Tab 內容 RenderFragment
    private RenderFragment CreateVehicleTabContent() => __builder =>
    {
        <CustomerVehicleTab @ref="vehicleTab" CustomerId="@CustomerId" />
    };

    private RenderFragment CreateVisitTabContent() => __builder =>
    {
        <CustomerVisitTab @ref="visitTab" CustomerId="@CustomerId" />
    };

    // 實體切換時，通知各 Tab 載入新資料
    private async Task HandleEntityLoaded(int loadedEntityId)
    {
        if (loadedEntityId > 0)
        {
            if (vehicleTab != null) await vehicleTab.LoadAsync(loadedEntityId);
            if (visitTab != null) await visitTab.LoadAsync(loadedEntityId);
        }
        else
        {
            vehicleTab?.Clear();
            visitTab?.Clear();
        }
        StateHasChanged();
    }

    // 儲存成功後的處理
    private async Task HandleSaveSuccess()
    {
        if (editModalComponent?.Entity != null)
        {
            // 有暫存緩衝的 Tab 需要在這裡提交
            if (vehicleTab != null)
                await vehicleTab.SavePendingChangesAsync(editModalComponent.Entity.Id);

            await OnCustomerSaved.InvokeAsync(editModalComponent.Entity);
        }
    }

    // 表單欄位定義（只保留主要資料的欄位）
    private async Task InitializeFormFieldsAsync()
    {
        // ...formFields 定義...

        var layout = FormSectionHelper<Customer>.Create()
            .AddToSection(...)
            .GroupIntoTab("客戶資料", "bi-building", ...)   // 標準表單 Tab
            .GroupIntoCustomTab("車輛資訊", "bi-truck", CreateVehicleTabContent())  // 自訂 Tab
            .GroupIntoCustomTab("拜訪紀錄", "bi-journal-text", CreateVisitTabContent())
            .BuildAll();
    }
}
```

---

## 新增 Tab 的步驟

1. **建立 Tab 元件** `XxxYyyTab.razor`（放入 `XxxEditModal/` 資料夾）
2. **加入必要的子元件**（Table、EditModal）至同一資料夾
3. **在主組件**加入：
   - `private XxxYyyTab? yyyTab;`（@ref 變數）
   - `CreateYyyTabContent()` RenderFragment
   - `HandleEntityLoaded` 中呼叫 `yyyTab?.LoadAsync(...)`
   - `InitializeFormFieldsAsync` 中加 `.GroupIntoCustomTab(...)`
4. **如需資料服務**，在 Tab 元件直接 `@inject`，不透過主組件傳遞
5. **確認 `_Imports.razor`** 已加入 Tab 的命名空間（資料夾第一次使用時）

---

## _Imports.razor 更新

每建立新的 `XxxEditModal/` 資料夾，需在 `_Imports.razor` 加入：

```razor
@using ERPCore2.Components.Pages.Employees.EmployeeEditModal
@using ERPCore2.Components.Pages.Customers.CustomerEditModal
```

---

## 已套用的 EditModal

| EditModal | 資料夾 | 自訂 Tab |
|-----------|--------|----------|
| EmployeeEditModalComponent | `Employees/EmployeeEditModal/` | 車輛資訊、個人工具 |
| CustomerEditModalComponent | `Customers/CustomerEditModal/` | 車輛資訊、拜訪紀錄、交易紀錄 |
