# 審核機制 — 新模組實作指南

> 本文件提供為任意模組加入完整審核功能的逐步操作指南。以 PurchaseOrder 為最完整參考實作。

---

## 前置確認

實作前先確認：
- [ ] `SystemParameter` 已有該模組的審核開關欄位（`EnableXxxApproval`）
- [ ] `PermissionRegistry` 已有 `Xxx.Approve` 權限
- [ ] 實體已有審核欄位（`IsApproved`、`ApprovedBy`、`ApprovedAt`、`RejectReason`）

---

## Step 1：實體加入審核欄位

在實體類別加入（參考 `PurchaseOrder.cs`）：

```csharp
// ===== 審核欄位 =====
[Display(Name = "是否核准")]
public bool IsApproved { get; set; } = false;

[Display(Name = "核准人員")]
[ForeignKey(nameof(ApprovedByUser))]
public int? ApprovedBy { get; set; }

[Display(Name = "核准時間")]
public DateTime? ApprovedAt { get; set; }

[MaxLength(200, ErrorMessage = "駁回原因不可超過200個字元")]
[Display(Name = "駁回原因")]
public string? RejectReason { get; set; }

// Navigation Property
public Employee? ApprovedByUser { get; set; }
```

---

## Step 2：Migration

若有新欄位需執行：

```bash
dotnet ef migrations add AddApprovalFieldsToXxx
dotnet ef database update
```

同時在對應 Service 的 `GetByIdAsync` / `GetAllAsync` 查詢中加入：
```csharp
.Include(x => x.ApprovedByUser)
```

---

## Step 3：SystemParameter 加入開關（若尚未存在）

`Data/Entities/Systems/SystemParameter.cs` 加入：
```csharp
[Display(Name = "啟用XXX審核")]
public bool EnableXxxApproval { get; set; } = false;
```

Migration 包含此欄位（可與 Step 2 合併）。

---

## Step 4：ApprovalSettingsTab 加入開關 UI

`Components/Pages/Systems/SystemParameter/ApprovalSettingsTab.razor`，在 `OnInitialized()` 的清單加入：

```csharp
new("xxx", L["Field.EnableXxxApproval"], () => Model.EnableXxxApproval, v => Model.EnableXxxApproval = v),
```

同步更新各語言 resx 的 `Field.EnableXxxApproval` 鍵值。

---

## Step 5：PermissionRegistry 加入 Approve 權限

`Models/PermissionRegistry.cs`，在對應模組加入：

```csharp
public static class Xxx
{
    public const string Read   = "Xxx.Read";
    public const string Write  = "Xxx.Write";
    public const string Delete = "Xxx.Delete";
    public const string Approve = "Xxx.Approve";  // ← 新增
}
```

---

## Step 6：Service 加入 ApproveAsync / RejectAsync

在 `IXxxService` 加入介面：

```csharp
Task<ServiceResult> ApproveAsync(int id, int approvedBy);
Task<ServiceResult> RejectAsync(int id, int rejectedBy, string reason);
```

在 `XxxService` 實作（參考 `PurchaseOrderService.ApproveOrderAsync`）：

```csharp
public async Task<ServiceResult> ApproveAsync(int id, int approvedBy)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    using var transaction = await context.Database.BeginTransactionAsync();
    try
    {
        var entity = await context.XxxSet.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return ServiceResult.Failure("找不到資料");
        if (entity.IsApproved) return ServiceResult.Failure("已核准，無需重複核准");

        entity.IsApproved = true;
        entity.ApprovedBy = approvedBy;
        entity.ApprovedAt = DateTime.Now;
        entity.RejectReason = null;
        entity.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
        return ServiceResult.Success();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}

public async Task<ServiceResult> RejectAsync(int id, int rejectedBy, string reason)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    var entity = await context.XxxSet.FirstOrDefaultAsync(x => x.Id == id);
    if (entity == null) return ServiceResult.Failure("找不到資料");

    entity.IsApproved = false;
    entity.ApprovedBy = rejectedBy;
    entity.ApprovedAt = DateTime.Now;
    entity.RejectReason = reason;
    entity.UpdatedAt = DateTime.Now;

    await context.SaveChangesAsync();
    return ServiceResult.Success();
}
```

---

## Step 7：EditModal 加入審核 UI

### 7-1 加入審核開關狀態欄位與讀取

```csharp
private bool isApprovalEnabled = false;
```

在 `LoadXxxData()` 或 `InitializeFormFieldsAsync()` 中載入：
```csharp
var systemParam = await SystemParameterService.GetSystemParameterAsync();
isApprovalEnabled = systemParam?.EnableXxxApproval ?? false;
```

### 7-2 GenericEditModalComponent 加入審核參數

```razor
<GenericEditModalComponent ...
    ShowApprovalSection="@ShouldShowApprovalSection()"
    ApprovalPermission="@PermissionRegistry.Xxx.Approve"
    OnApprove="@HandleXxxApprove"
    OnRejectWithReason="@HandleXxxRejectWithReason"
    CanPrintCheck="@GetCanPrintCheck()">
```

### 7-3 判斷方法

```csharp
private bool ShouldShowApprovalSection()
    => isApprovalEnabled && XxxId.HasValue;

private Func<bool>? GetCanPrintCheck()
    => isApprovalEnabled
        ? () => ApprovalConfigHelper.CanPerformActionRequiringApproval(
              isApprovalEnabled,
              editModalComponent?.Entity?.IsApproved ?? false)
        : null;
```

### 7-4 欄位鎖定

在 `InitializeFormFieldsAsync()` 計算 `shouldLock`，套用至各欄位的 `IsReadOnly`：

```csharp
var shouldLock = ApprovalConfigHelper.ShouldLockFieldByApproval(
    isApprovalEnabled,
    editModalComponent?.Entity?.IsApproved ?? false,
    hasUndeletableDetails);
```

### 7-5 核准方法

```csharp
private async Task<bool> HandleXxxApprove()
{
    try
    {
        if (editModalComponent?.Entity == null || !XxxId.HasValue)
        {
            await NotificationService.ShowErrorAsync("找不到資料");
            return false;
        }

        var entity = editModalComponent.Entity;

        if (entity.IsApproved)
        {
            await NotificationService.ShowWarningAsync("已經審核通過");
            return false;
        }

        // 驗證明細（視情況）
        if (!xxxDetails.Any())
        {
            await NotificationService.ShowWarningAsync("無明細資料，無法審核");
            return false;
        }

        var confirmed = await NotificationService.ShowConfirmAsync(
            "確定要審核通過？系統將先儲存當前資料，審核後無法修改。", "審核確認");
        if (!confirmed) return false;

        // 核准前先儲存（含明細）
        var saveOk = await SaveXxxWithDetails(entity, isPreApprovalSave: true);
        if (!saveOk)
        {
            await NotificationService.ShowErrorAsync("儲存失敗，無法進行審核");
            return false;
        }

        var userId = await CurrentUserHelper.GetCurrentEmployeeIdAsync(AuthenticationStateProvider);
        if (!userId.HasValue)
        {
            await NotificationService.ShowErrorAsync("無法取得使用者資訊");
            return false;
        }

        var result = await XxxService.ApproveAsync(XxxId.Value, userId.Value);
        if (result.IsSuccess)
            return true;

        await NotificationService.ShowErrorAsync(result.ErrorMessage ?? "審核失敗");
        return false;
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleXxxApprove), GetType());
        return false;
    }
}
```

### 7-6 駁回方法

```csharp
private async Task<bool> HandleXxxRejectWithReason(string rejectReason)
{
    try
    {
        if (editModalComponent?.Entity == null || !XxxId.HasValue)
            return false;

        var userId = await CurrentUserHelper.GetCurrentEmployeeIdAsync(AuthenticationStateProvider);
        if (!userId.HasValue) return false;

        var result = await XxxService.RejectAsync(XxxId.Value, userId.Value, rejectReason);
        if (result.IsSuccess)
            return true;

        await NotificationService.ShowErrorAsync(result.ErrorMessage ?? "駁回失敗");
        return false;
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleXxxRejectWithReason), GetType());
        return false;
    }
}
```

### 7-7 儲存方法加入 isPreApprovalSave 參數

若 EditModal 有自訂 SaveHandler，在方法簽名加入：

```csharp
private async Task<bool> SaveXxxWithDetails(XxxEntity entity, bool isPreApprovalSave = false)
{
    // 判斷是否可儲存
    if (!ApprovalConfigHelper.CanSaveWhenApproved(isApprovalEnabled, entity.IsApproved, isPreApprovalSave))
    {
        await NotificationService.ShowWarningAsync("已核准的單據不可修改");
        return false;
    }

    // ... 實際儲存邏輯 ...
}
```

---

## Step 8：Detail Table 封鎖

在傳入 Table 組件的地方加入正確的 `IsReadOnly`：

```razor
<XxxTable ...
    IsReadOnly="@ApprovalConfigHelper.ShouldLockFieldByApproval(
        isApprovalEnabled,
        editModalComponent?.Entity?.IsApproved ?? false,
        hasUndeletableDetails)" />
```

---

## Step 9：Index 加入批次審核

在 `XxxIndex.razor` 加入（參考 `PurchaseOrderIndex.razor`）：

```razor
@* 批次審核 Modal *@
<BatchApprovalModalComponent TEntity="XxxEntity"
    IsVisible="@showBatchApprovalModal"
    IsVisibleChanged="@((bool v) => showBatchApprovalModal = v)"
    Title="批次審核XXX"
    EntityName="XXX"
    LoadPendingItems="@LoadPendingApprovalItemsAsync"
    OnApproveAll="@HandleBatchApproveAsync"
    OnViewClick="@HandleViewFromApprovalModal"
    OnApprovalCompleted="@HandleApprovalCompleted" />
```

在 `GenericIndexPageComponent` 加入批次審核按鈕：

```razor
<GenericIndexPageComponent ...>
    <CustomButtons>
        @if (isApprovalEnabled)
        {
            <GenericButtonComponent Variant="ButtonVariant.Warning"
                                   Text="批次審核"
                                   OnClick="@(() => showBatchApprovalModal = true)" />
        }
    </CustomButtons>
</GenericIndexPageComponent>
```

---

## Step 10：Index FieldConfiguration 加入審核狀態欄

在 `XxxFieldConfiguration.cs` 加入：

```csharp
new FieldConfiguration<XxxEntity>
{
    PropertyName = nameof(XxxEntity.IsApproved),
    DisplayName = Dn("Label.ApprovalStatus", "審核狀態"),
    IsFilterable = true,
    CustomTemplate = (entity) => builder =>
    {
        var text   = entity.IsApproved ? (L?["Label.Approved"].ToString() ?? "已審核")
                   : !string.IsNullOrEmpty(entity.RejectReason) ? (L?["Label.Rejected"].ToString() ?? "已駁回")
                   : (L?["Label.PendingApproval"].ToString() ?? "待審核");
        var css    = entity.IsApproved ? "badge bg-success"
                   : !string.IsNullOrEmpty(entity.RejectReason) ? "badge bg-danger"
                   : "badge bg-secondary";
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", css);
        builder.AddContent(2, text);
        builder.CloseElement();
    }
}
```

> 若要只在審核啟用時顯示此欄，可在 Index Page 的 `OnInitializedAsync` 根據 `isApprovalEnabled` 動態加入或移除此欄。

---

## 快速檢查清單

完成後逐項驗證：

- [ ] 新增模式開啟後看不到核准/駁回按鈕（`XxxId.HasValue = false`）
- [ ] 編輯模式、審核關閉 → 無按鈕、欄位可編輯
- [ ] 編輯模式、審核開啟、未審核 → 顯示核准/駁回按鈕、欄位可編輯
- [ ] 按核准 → 確認對話框 → 儲存明細 → 設為已核准 → 按鈕消失、欄位變唯讀
- [ ] 按駁回 → 輸入原因 → 設為未核准 + 原因 → 欄位恢復可編輯
- [ ] 已核准時按儲存 → 顯示警告，無法儲存
- [ ] 已核准時按列印 → 允許（`CanPerformActionRequiringApproval = true`）
- [ ] 未審核時按列印（審核開啟）→ 顯示警告，無法列印
- [ ] Detail Table 在已核准時完全唯讀（無法新增/刪除/修改明細）
- [ ] 批次審核 Modal 正常載入待審核清單並可核准
- [ ] Index 審核狀態 badge 正確顯示
