# 允許草稿功能設計說明

> 本文件說明 ERPCore2 草稿（Draft）功能的設計原則與實作步驟。
> 以 **員工資料（Employee）** 作為第一個實作範例，確認可行後再推廣至其他模組。
> 最後更新：2026-03-09

---

## 一、設計背景與需求

### 為什麼需要草稿？

業務場景：建立員工資料時，員工未帶身分證，其他資料都已填寫完成。
目前系統若離開表單，已填寫的資料全部遺失，下次必須重新輸入。

草稿功能允許使用者在**資料不完整的情況下儲存記錄**，並在稍後繼續補齊。

### 草稿 vs 審核的區別

| | 草稿（Draft） | 審核（Approval） |
|---|---|---|
| **目的** | 資料尚未填寫完整 | 資料已完整，等待主管確認 |
| **觸發** | 使用者主動選擇「儲存草稿」 | 使用者完成填寫後「提交」 |
| **限制** | 跳過前端必填驗證 | 完整驗證後才可提交 |
| **適用對象** | 主檔與交易單據皆可 | 主要用於交易單據 |

---

## 二、設計決策

### 2-1 在 BaseEntity 加入 IsDraft 欄位（而非修改 EntityStatus）

`BaseEntity` 目前已有 `EntityStatus`（Active / Inactive / Deleted），語意為「記錄是否啟用」。
草稿狀態是另一個維度：「記錄是否填寫完整」，兩者**獨立**，不應合併。

```csharp
// BaseEntity 新增欄位
public bool IsDraft { get; set; } = false;
```

**優點：**
- 一次 Migration 套用至所有資料表
- 現有資料全部預設 `false`，完全向下相容
- 索引頁可統一用 `entity.IsDraft` 顯示草稿標記
- 草稿與啟用/停用狀態互不干擾

### 2-2 Employee 是最佳第一個範例

`Employee` 實體的所有欄位均為 nullable（`string?`, `DateTime?`, `int?`），
**不存在 DB NOT NULL 約束阻擋草稿儲存**，可以直接實作，無需修改資料庫欄位型別。

> ⚠️ 有 NOT NULL FK 欄位的實體（如 `SalesOrder.CustomerId int`）需要額外將 FK 改為 nullable，
> 這類模組應等 Employee 範例確認穩定後再另行規劃。

### 2-3 草稿儲存跳過的驗證範圍

| 驗證類型 | 正式儲存 | 草稿儲存 |
|---------|---------|---------|
| 前端 `[Required]` DataAnnotations | ✅ 執行 | ❌ 跳過 |
| `CustomValidator`（業務邏輯） | ✅ 執行 | ❌ 跳過 |
| `BeforeSave` 前置處理 | ✅ 執行 | ✅ 執行（設定 IsDraft = true） |
| DB 層 NOT NULL 約束 | ✅ 執行 | ✅ 執行（無法繞過） |

---

## 三、影響範圍

```
Data/BaseEntity.cs                              ← 新增 IsDraft 欄位
Migrations/                                     ← 新增 Migration
Components/Shared/Modal/
  GenericEditModalComponent.razor               ← 新增「儲存草稿」按鈕
  GenericEditModalComponent.Save.cs             ← 新增 HandleSaveDraft() 方法
Components/Pages/Employees/EmployeeEditModal/
  EmployeeEditModalComponent.razor              ← 連接 DraftHandler 參數
Components/Pages/Employees/
  EmployeeIndexPage.razor（或對應的 Index 頁）  ← 顯示草稿標記
```

---

## 四、實作步驟

### Step 1：修改 BaseEntity — 新增 IsDraft

**檔案：** `Data/BaseEntity.cs`

```csharp
public abstract class BaseEntity
{
    // ... 現有欄位不變 ...

    /// <summary>
    /// 是否為草稿（資料尚未填寫完整）
    /// true = 草稿，允許必填欄位為空；false = 正式記錄（預設）
    /// </summary>
    [Display(Name = "草稿")]
    public bool IsDraft { get; set; } = false;
}
```

---

### Step 2：新增 Migration

```bash
dotnet ef migrations add AddBaseEntityIsDraft
dotnet ef database update
```

Migration 會在所有資料表加入：
```sql
IsDraft bit NOT NULL DEFAULT 0
```

現有所有記錄自動為 `false`，無須資料轉換。

---

### Step 3：修改 GenericEditModalComponent — 新增草稿參數與按鈕

**檔案：** `Components/Shared/Modal/GenericEditModalComponent.razor`

#### 3-1 新增參數（`@code` 區塊）

```csharp
/// <summary>
/// 是否顯示「儲存草稿」按鈕（預設 false）
/// </summary>
[Parameter] public bool ShowDraftButton { get; set; } = false;

/// <summary>
/// 草稿儲存處理委派（設定後啟用草稿功能）
/// 若未設定，草稿儲存將使用通用流程（IsDraft = true + 一般 Save）
/// </summary>
[Parameter] public Func<TEntity, Task<bool>>? DraftHandler { get; set; }

/// <summary>
/// 草稿儲存成功的訊息
/// </summary>
[Parameter] public string DraftSuccessMessage { get; set; } = "草稿已儲存";
```

#### 3-2 新增草稿按鈕 UI（在儲存按鈕左側）

在 `HeaderButtons` 的右側按鈕群組中，於 `ShowSaveButton` 按鈕之前加入：

```razor
@* 草稿按鈕（在儲存按鈕左側） *@
@if (ShowDraftButton && !IsLoading)
{
    <GenericButtonComponent Text="@L["Button.SaveDraft"]"
                           Variant="ButtonVariant.Secondary"
                           OnClick="HandleSaveDraft"
                           IsDisabled="@(IsSubmitting || IsLoading)"
                           Title="@L["Button.SaveDraft"]" />
}
```

---

### Step 4：修改 GenericEditModalComponent.Save.cs — 新增 HandleSaveDraft()

**檔案：** `Components/Shared/Modal/GenericEditModalComponent.Save.cs`

```csharp
/// <summary>
/// 草稿儲存：跳過 CustomValidator，設定 IsDraft = true
/// </summary>
private async Task HandleSaveDraft()
{
    if (IsSubmitting) return;

    try
    {
        IsSubmitting = true;
        StateHasChanged();

        if (Entity == null)
        {
            await ShowErrorMessage("實體資料不存在");
            return;
        }

        // 設定草稿旗標
        Entity.IsDraft = true;

        // 自動設定審計欄位
        try
        {
            var currentUserName = await CurrentUserHelper.GetCurrentUserFullNameAsync(AuthenticationStateProvider);
            if (!string.IsNullOrEmpty(currentUserName))
            {
                if (!Id.HasValue)
                    Entity.CreatedBy = currentUserName;
                Entity.UpdatedBy = currentUserName;
            }
        }
        catch { }

        bool success;

        // 草稿儲存：優先使用 DraftHandler，否則使用 SaveHandler（跳過 CustomValidator）
        if (DraftHandler != null)
        {
            success = await DraftHandler(Entity);
        }
        else if (UseGenericSave)
        {
            // GenericSave 但跳過 CustomValidator
            success = await GenericSaveDraft(Entity);
        }
        else if (SaveHandler != null)
        {
            success = await SaveHandler(Entity);
        }
        else
        {
            await ShowErrorMessage("未設定儲存處理程序");
            return;
        }

        if (success)
        {
            _isDirty = false;
            await ShowSuccessMessage(DraftSuccessMessage);

            bool wasNewRecord = !Id.HasValue;
            if (wasNewRecord && Entity != null && Entity.Id > 0)
            {
                _lastId = Entity.Id;
                Id = Entity.Id;
                if (IdChanged.HasDelegate)
                    await IdChanged.InvokeAsync(Entity.Id);
                await LoadAllData();
            }

            if (OnEntitySaved.HasDelegate && Entity != null)
                await OnEntitySaved.InvokeAsync(Entity);
            if (OnSaveSuccess.HasDelegate)
                await OnSaveSuccess.InvokeAsync();
        }
    }
    catch (Exception ex)
    {
        await ShowErrorMessage($"草稿儲存時發生錯誤：{ex.Message}");
        LogError("HandleSaveDraft", ex);
    }
    finally
    {
        IsSubmitting = false;
        StateHasChanged();
    }
}

/// <summary>
/// 通用草稿儲存：跳過 CustomValidator，但執行 BeforeSave / AfterSave
/// </summary>
private async Task<bool> GenericSaveDraft(TEntity entity)
{
    try
    {
        // 草稿不執行 CustomValidator
        if (BeforeSave != null)
            await BeforeSave(entity);

        var genericService = Service as IGenericManagementService<TEntity>;
        if (genericService == null)
        {
            await ShowErrorMessage("服務未實作泛型管理介面");
            return false;
        }

        ServiceResult<TEntity> serviceResult = Id.HasValue
            ? await genericService.UpdateAsync(entity)
            : await genericService.CreateAsync(entity);

        if (serviceResult.IsSuccess)
        {
            if (AfterSave != null)
                await AfterSave(entity);
            return true;
        }
        else
        {
            var errorMsg = !string.IsNullOrEmpty(serviceResult.ErrorMessage)
                ? serviceResult.ErrorMessage : "草稿儲存失敗";
            await ShowErrorMessage(errorMsg);
            return false;
        }
    }
    catch (Exception ex)
    {
        await ShowErrorMessage($"草稿儲存失敗：{ex.Message}");
        LogError("GenericSaveDraft", ex);
        return false;
    }
}
```

---

### Step 5：修改未儲存變更對話框 — 新增「儲存草稿」選項

**檔案：** `Components/Shared/Modal/GenericEditModalComponent.razor`（未儲存確認 Modal 區塊）

目前選項：`儲存關閉` / `繼續編輯` / `確定離開`

新增第四個選項（僅在 `ShowDraftButton == true` 時顯示）：

```razor
<HeaderButtons>
    <GenericButtonComponent Text="@L["Modal.SaveAndClose"]"    Variant="ButtonVariant.DarkBlue" OnClick="HandleUnsavedChangesSaveAndClose" />
    @if (ShowDraftButton)
    {
        <GenericButtonComponent Text="@L["Button.SaveDraft"]"  Variant="ButtonVariant.Secondary" OnClick="HandleUnsavedChangesSaveDraft" />
    }
    <GenericButtonComponent Text="@L["Modal.ContinueEditing"]" Variant="ButtonVariant.Green"    OnClick="HandleUnsavedChangesCancel" />
    <GenericButtonComponent Text="@L["Modal.ConfirmLeave"]"    Variant="ButtonVariant.Red"      OnClick="HandleUnsavedChangesLeave" />
</HeaderButtons>
```

在 `GenericEditModalComponent.Save.cs` 新增對應處理：

```csharp
// 未儲存確認的 result 新增 case 3 = 儲存草稿後離開
private void HandleUnsavedChangesSaveDraft()
{
    _showUnsavedChangesModal = false;
    _unsavedChangesConfirmTcs?.TrySetResult(3);
}
```

並在 `HandleCancel()` 的 switch 加入：
```csharp
case 3: // 儲存草稿後離開
    _forceCloseOnNextSave = true;
    _cancelledByUser = true;
    await HandleSaveDraft();
    return;
```

> ⚠️ **注意**：離開確認框的草稿選項**只在 `ShowDraftButton == true` 時顯示**，
> 防止在未啟用草稿的模組中誤觸，也防止已核准/待審核記錄意外降格為草稿。

---

### Step 6：修改 EmployeeEditModalComponent — 啟用草稿

**檔案：** `Components/Pages/Employees/EmployeeEditModal/EmployeeEditModalComponent.razor`

在 `GenericEditModalComponent` 加入草稿參數：

```razor
<GenericEditModalComponent TEntity="Employee"
                          ...（現有參數不變）...
                          ShowDraftButton="true"
                          DraftSuccessMessage="員工草稿已儲存，請記得補齊必填資料" />
```

> Employee 使用 `SaveHandler="@SaveEmployee"`（自訂儲存），
> 草稿儲存時 `DraftHandler` 未設定，系統會呼叫 `SaveHandler` 並帶入 `IsDraft = true` 的 Entity。
> `SaveEmployee` 方法本身不需要修改，`IsDraft` 欄位會隨 Entity 一併寫入 DB。

---

### Step 7：在索引頁加入草稿切換按鈕

#### 7-1 佈局設計

索引頁的草稿切換採用**篩選按鈕**形式，位於搜尋篩選區與 Table 之間：

```
┌──────────────────────────────────────────────┐
│ 搜尋篩選區（關鍵字、條件等）                   │
├──────────────────────────────────────────────┤  ← search-table-divider（已有）
│  [正式資料]  [草稿資料 3]                     │  ← 新增於此（緊貼 Table 上方）
├──────────────────────────────────────────────┤
│ Table                                        │
│ 分頁                                         │
└──────────────────────────────────────────────┘
```

設計說明：
- 搜尋條件在上方獨立，適用於兩種狀態（切換 Tab 不清空搜尋條件）
- 按鈕緊貼 Table 上方，視覺上明確對應「切換 Table 顯示什麼」
- 不是導航 Tab，是資料篩選，語意上更精確

#### 7-2 修改 GenericIndexPageComponent — 新增草稿 Tab 參數

**檔案：** `Components/Shared/Page/GenericIndexPageComponent.razor`（`@code` 區塊）

```csharp
/// <summary>
/// 是否顯示正式/草稿切換按鈕（預設 false，只有啟用草稿的模組才設為 true）
/// </summary>
[Parameter] public bool ShowDraftTab { get; set; } = false;

// 內部狀態
private bool _showingDrafts = false;
private int _draftCount = 0;
```

#### 7-3 新增切換按鈕 HTML

**位置：** 在 `search-table-divider` 之後、`GenericTableComponent` 之前插入：

```razor
@* 草稿/正式資料切換（僅在 ShowDraftTab 時顯示） *@
@if (ShowDraftTab)
{
    <div class="index-draft-tab-btn-group">
        <button type="button"
                class="index-draft-tab-btn @(!_showingDrafts ? "active" : "")"
                @onclick="@(() => SwitchDraftTab(false))">
            <i class="bi bi-check-circle me-1"></i>
            @L["Label.Official"]
        </button>
        <button type="button"
                class="index-draft-tab-btn @(_showingDrafts ? "active" : "")"
                @onclick="@(() => SwitchDraftTab(true))">
            <i class="bi bi-pencil-square me-1"></i>
            @L["Label.Draft"]
            @if (_draftCount > 0)
            {
                <span class="badge bg-danger ms-1">@_draftCount</span>
            }
        </button>
    </div>
}
```

Tab 切換方法：

```csharp
private async Task SwitchDraftTab(bool showDrafts)
{
    _showingDrafts = showDrafts;
    await Refresh();  // 重新載入資料（帶入新的 _showingDrafts 條件）
}
```

資料載入時自動套用 IsDraft 過濾（在 `LoadData()` 內部執行，不需各模組處理）：

```csharp
// GenericIndexPageComponent 內部的資料過濾邏輯
if (ShowDraftTab)
    filteredItems = filteredItems.Where(e => e.IsDraft == _showingDrafts).ToList();
```

草稿筆數計算（頁面初始化時執行一次）：

```csharp
if (ShowDraftTab)
    _draftCount = allItems.Count(e => e.IsDraft);
```

#### 7-4 新增 CSS — 樣式與 form-tab-btn 一致

**檔案：** `Components/Shared/Page/GenericIndexPageComponent.razor.css`

> Blazor CSS 隔離機制使每個組件的 CSS 獨立，因此雖然視覺樣式與 `form-tab-btn`（`GenericFormComponent.razor.css`）完全相同，仍需在此另行定義同名規則。

```css
/* 草稿切換按鈕群組 */
.index-draft-tab-btn-group {
    display: flex;
    flex-wrap: wrap;
    gap: 0.35rem;
    padding: 0.5rem 0;
}

/* 按鈕基礎樣式（與 form-tab-btn 視覺一致） */
.index-draft-tab-btn {
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
    padding: 0.25rem 0.75rem;
    font-size: 1rem;
    font-weight: 500;
    color: var(--text-secondary);
    background-color: var(--bg-secondary);
    border: 1px solid var(--border-light);
    border-radius: 0.375rem;
    cursor: pointer;
    transition: all 0.15s ease-in-out;
}

.index-draft-tab-btn:hover {
    color: var(--primary-blue);
    background-color: rgba(var(--primary-blue-rgb), 0.1);
    border-color: rgba(var(--primary-blue-rgb), 0.4);
}

/* 作用中（已選取）狀態 */
.index-draft-tab-btn.active {
    color: #fff;
    background-color: #0d6efd;
    border-color: #0d6efd;
    font-weight: 600;
}

.index-draft-tab-btn.active:hover {
    background-color: #0b5ed7;
    border-color: #0a58ca;
    color: #fff;
}

/* 按鈕圖示 */
.index-draft-tab-btn i {
    font-size: 1rem;
}

/* 響應式 */
@media (max-width: 576px) {
    .index-draft-tab-btn-group {
        gap: 0.25rem;
    }

    .index-draft-tab-btn {
        padding: 0.3rem 0.6rem;
        font-size: 0.9rem;
    }
}

/* Dark mode */
[data-bs-theme=dark] .index-draft-tab-btn.active {
    background-color: #0d6efd;
    border-color: #0d6efd;
}
```

#### 7-5 啟用草稿 Tab — 員工索引頁

**檔案：** `Components/Pages/Employees/EmployeeIndex.razor`（或對應的 Index 頁）

```razor
<GenericIndexPageComponent TEntity="Employee"
                          TService="IEmployeeService"
                          ...（現有參數不變）...
                          ShowDraftTab="true" />
```

---

### Step 8：新增 i18n 資源鍵

在所有 5 個 resx 檔案加入：

| Key | zh-TW | en-US | ja-JP | zh-CN | fil |
|-----|-------|-------|-------|-------|-----|
| `Button.SaveDraft` | 儲存草稿 | Save Draft | 下書き保存 | 保存草稿 | I-save Bilang Draft |
| `Label.Draft` | 草稿 | Draft | 下書き | 草稿 | Draft |
| `Label.Official` | 正式 | Official | 正式 | 正式 | Opisyal |
| `Message.DraftWarning` | 此記錄為草稿，請記得補齊必填資料 | This record is a draft. Please complete all required fields. | この記録は下書きです。必須フィールドを入力してください。 | 此记录为草稿，请记得补齐必填资料 | Ang rekord na ito ay draft. Pakumpleto ang lahat ng kinakailangang field. |

---

## 五、服務層草稿過濾

### 5-1 問題說明

`IsDraft = true` 的記錄存入 DB 後，若不加以過濾，會出現在：
- AutoComplete 下拉選單（如「選擇員工」）
- 報表查詢統計
- 薪資計算
- 儀表板人數計數
- 其他模組的關聯查詢

草稿資料應**只在索引頁（使用者主動切換草稿 Tab）和 Edit Modal（透過 ID 直接載入）時出現**。

### 5-2 解法：在 GenericManagementService 的列表查詢加入預設過濾

**檔案：** `Services/GenericManagementService.cs`（或對應的 Base Service）

```csharp
// ✅ 列表查詢：過濾草稿（GetAll、Search、AutoComplete 等）
protected IQueryable<TEntity> GetBaseListQuery(DbContext context)
{
    return context.Set<TEntity>()
        .Where(e => e.Status != EntityStatus.Deleted)
        .Where(e => !e.IsDraft);   // 草稿不出現在一般列表
}

// ✅ 單筆查詢：不過濾草稿（Edit Modal 需要能載入草稿記錄）
public async Task<TEntity?> GetByIdAsync(int id)
{
    return await context.Set<TEntity>()
        .Where(e => e.Id == id)   // 不加 IsDraft 過濾
        .FirstOrDefaultAsync();
}
```

### 5-3 索引頁的草稿資料來源

`GenericIndexPageComponent` 的 `DataLoader` 由各模組提供，通常載入「所有正式資料」。
啟用 `ShowDraftTab` 後，需要 `DataLoader` **同時回傳草稿和正式資料**，再由 component 內部依 `_showingDrafts` 過濾。

因此各模組的 `DataLoader` 需針對草稿 Tab 調整：

```csharp
// 員工索引頁的 DataLoader（啟用 ShowDraftTab 時）
private async Task<List<Employee>> LoadEmployeesAsync()
{
    // 載入所有記錄（含草稿），讓 GenericIndexPageComponent 自行過濾
    return await EmployeeService.GetAllIncludingDraftsAsync();
}
```

對應 Service 新增：

```csharp
// IEmployeeService / EmployeeService
public async Task<List<Employee>> GetAllIncludingDraftsAsync()
{
    return await context.Set<Employee>()
        .Where(e => e.Status != EntityStatus.Deleted)
        // 不加 !IsDraft 過濾 → 草稿和正式都回傳
        .ToListAsync();
}
```

> 💡 若模組不啟用 `ShowDraftTab`，`DataLoader` 維持原有邏輯不變（只回傳非草稿資料）。

---

## 六、正式儲存時清除草稿旗標

使用者草稿補齊資料後按「儲存」（正式儲存），應自動將 `IsDraft` 改回 `false`。

**在 `GenericEditModalComponent.Save.cs` 的 `HandleSave()` 中確保：**

```csharp
private async Task HandleSave()
{
    // ... 現有邏輯 ...

    // 正式儲存時清除草稿旗標
    if (Entity != null)
        Entity.IsDraft = false;

    // ... 繼續執行 GenericSave 或 SaveHandler ...
}
```

---

## 七、測試清單

Employee 草稿功能驗收標準：

**Edit Modal 草稿儲存：**
- [ ] 新增員工時，只填姓名不填身分證，點「儲存草稿」→ 成功儲存
- [ ] 開啟草稿員工，補齊所有資料，點「儲存」→ `IsDraft` 變為 `false`
- [ ] 開啟員工資料，修改後按 ESC → 出現「儲存草稿」選項
- [ ] 非草稿模組（如客戶，`ShowDraftButton = false`）按 ESC → 不出現「儲存草稿」選項
- [ ] 已有資料的員工進行草稿儲存 → `IsDraft = true`，其他欄位資料保留

**索引頁草稿 Tab：**
- [ ] 索引頁預設顯示「正式資料」Tab，只顯示 `IsDraft = false` 的記錄
- [ ] 切換至「草稿資料」Tab → 只顯示 `IsDraft = true` 的記錄
- [ ] 草稿 Tab 按鈕上顯示正確筆數 badge（如「草稿資料 3」）
- [ ] 搜尋條件在切換 Tab 後保留（搜尋「王」，切草稿 Tab 仍顯示含「王」的草稿）
- [ ] 正式記錄不出現在草稿 Tab，草稿記錄不出現在正式 Tab

**服務層過濾：**
- [ ] AutoComplete 選員工的下拉選單 → 草稿員工不出現
- [ ] Migration 執行後，現有所有員工 `IsDraft = false`

---

## 八、未來擴展（待確認 Employee 穩定後）

1. **交易單據（SalesOrder 等）**：需額外將 `CustomerId` 等 FK 改為 `int?`，另立 Migration
2. **草稿到期提醒**：草稿超過 N 天未更新，在儀表板或通知中提醒
3. **Edit Modal 草稿狀態提示**：開啟草稿記錄時，Header 的 StatusMessage 顯示「⚠ 草稿 - 資料尚未完整」
4. **草稿 + 審核防護**：草稿記錄不允許提交審核，審核按鈕加入 `!Entity.IsDraft` 條件
5. **草稿的 Code 產生時機**：草稿儲存時不產生 Code（避免佔用號碼），正式儲存時才產生

---

## 九、不在本次範圍內的設計

| 項目 | 原因 |
|------|------|
| DocumentStatus enum | 草稿與審核是不同維度，`IsDraft bool` + `IsApproved bool` 各司其職，不合併 |
| 草稿專用資料表 | 影響過大，Employee 範例不需要 |
| 跳過 DB NOT NULL 約束 | DB 約束不可繞過，有 NOT NULL FK 的模組須另行規劃 nullable 欄位 |
| 草稿記錄的 Code | 草稿不佔 Code 號碼，正式儲存才產生，屬第八點未來擴展項目 |
