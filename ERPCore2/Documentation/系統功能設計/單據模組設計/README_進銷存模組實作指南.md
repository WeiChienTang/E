# 進銷存模組實作指南

## 更新日期
2026-03-03（Step 1 更新：showApprovalSection、ApprovedByDisplayName）

---

## 📋 概述

本指南說明如何為新模組套用主從式進銷存設計，或將舊 Tab 設計遷移至同頁設計。

> **新增模組**：直接從 Step 6 開始（EditModal 部分跳過 Step 1-5）。
> **遷移舊 Tab 設計**：Step 1-12 全部執行。

---

## 📝 EditModal 修改（Step 1-5：舊 Tab 設計遷移）

### Step 1：修改 InitializeFormFieldsAsync

```csharp
// 改為同頁設計（移除 GroupIntoTab / BuildAll）
formSections = FormSectionHelper<Xxx>.Create()
    .AddToSection(FormSectionNames.BasicInfo, ...)
    .AddToSection(FormSectionNames.AmountInfo, ...)
    .AddToSection(FormSectionNames.AdditionalInfo, ...)
    .AddCustomFieldsIf(showApprovalSection && Id > 0, FormSectionNames.ApprovalInfo,
        "ApprovalStatusText", "ApprovedByDisplayName", "ApprovedAtText",
        nameof(Xxx.RejectReason))
    .Build();
```

> `showApprovalSection` 由系統參數 `HideApprovalInfoSection` 決定（非 `isManualApproval`）。
> `"ApprovedByDisplayName"` 為 `[NotMapped]` 計算屬性，自動處理 null（顯示「系統自動審核」）。

### Step 2：移除 tabDefinitions 欄位宣告

```csharp
// 刪除這行
private List<FormTabDefinition>? tabDefinitions;
```

### Step 3：新增 GetCustomModules() 方法

```csharp
private List<GenericEditModalComponent<Xxx, IXxxService>.CustomModule> GetCustomModules()
{
    return new List<...>
    {
        new CustomModule { Title = "", Content = CreateDetailContent(), Order = 1 }
    };
}
```

### Step 4：更新 Razor 模板參數

```razor
@* 舊的 *@
TabDefinitions="@tabDefinitions"

@* 新的 *@
CustomModules="@GetCustomModules()"
ShowApprovalSection="@ShouldShowApprovalSection()"
ApprovalPermission="@PermissionRegistry.Xxx.Approve"
OnApprove="@HandleXxxApprove"
OnRejectWithReason="@HandleXxxRejectWithReason"
```

### Step 5：移除 AdditionalSections

若有不需要的 `AdditionalSections` 參數則移除。

---

## 📝 Table 元件修改（Step 6-12）

### Step 6：移除 @typeparam 並加上 @inherits

```razor
@* 移除這兩行 *@
@typeparam TMainEntity where TMainEntity : BaseEntity
@typeparam TDetailEntity where TDetailEntity : BaseEntity, new()

@* 加上這行 *@
@inherits BaseDetailTableComponent<XxxMainEntity, XxxDetailEntity, XxxTable.XxxItem>
```

### Step 7：移除共用 Parameter

刪除以下已由基底類別提供的 `[Parameter]`：
- `MainEntity`、`ExistingDetails`、`OnDetailsChanged`
- `IsReadOnly`、`IsParentLoading`、`IsApproved`
- `DataVersion`、`SelectedSupplierId`（或 `SelectedCustomerId`）
- `OnHasUndeletableDetailsChanged`
- 所有 `*PropertyName` 參數（不再使用 reflection）

### Step 8：移除共用欄位與生命週期

刪除以下已由基底類別提供的欄位和方法：
- `Items`（或舊名 `ProductItems` 等）→ 改用基底的 `Items`
- `tableComponent` 欄位 → 基底已宣告
- `_previousDataVersion`、`_previousSelectedSupplierId`、`_isLoadingDetails`
- `_dataLoadCompleted`、`_hasUndeletableDetails`
- `OnInitializedAsync()`、`OnParametersSetAsync()`（如無額外邏輯）
- `RefreshDetailsAsync()` → 基底已提供

### Step 9：實作 3 個抽象方法

```csharp
protected override async Task LoadItemsFromDetailsAsync()
{
    // 直接存取屬性（不使用 reflection）
    foreach (var detail in ExistingDetails)
    {
        Items.Add(new XxxItem
        {
            ProductId = detail.ProductId,
            Quantity  = detail.XxxQuantity,
            // ...
        });
    }
}

protected override bool ComputeHasUndeletableDetails()
    => Items.Any(p => !DetailLockHelper.CanDeleteItem(p, out _, checkXxx: true));

protected override List<XxxDetailEntity> ConvertItemsToDetails()
    => Items.Select(p => new XxxDetailEntity
    {
        ProductId   = p.ProductId,
        XxxQuantity = p.Quantity,
        // ...
    }).ToList();
```

### Step 10：覆寫虛方法（依模組需求）

```csharp
// 如有廠商/客戶相關資料需重載
protected override async Task OnCounterpartyChangedAsync()
{
    await LoadCounterpartyCatalogAsync();
}

// 如有載入後額外查詢
protected override async Task OnAfterLoadAsync()
{
    await CheckLastPurchaseRecordAsync();
}
```

### Step 11：更新 XxxItem 內部型別

```csharp
// 改為具體型別（移除泛型 TDetailEntity）
public XxxDetailEntity? ExistingDetailEntity { get; set; }
```

### Step 12：更新 EditModal 呼叫端

```razor
@* 移除泛型型別參數 *@
<XxxTable
    @* 移除: TMainEntity="XxxMainEntity" TDetailEntity="XxxDetailEntity" *@
    @* 移除: MainEntityIdPropertyName/QuantityPropertyName 等 PropertyName 參數 *@
    MainEntity="@entity"
    ExistingDetails="@details"
    OnDetailsChanged="@HandleDetailsChanged"
    IsReadOnly="@shouldLock"
    IsParentLoading="@IsLoading"
    IsApproved="@(entity?.IsApproved ?? false)"
    DataVersion="@_detailsDataVersion"
    SelectedSupplierId="@(int?)entity?.SupplierId"
    OnHasUndeletableDetailsChanged="@HandleHasUndeletableDetailsChanged" />
```

---

## ⚠️ 注意事項

1. **`DataVersion` 必須遞增**：每次從父元件重新載入明細後，必須 `_detailsDataVersion++`，Table 才能偵測到需要重載。

2. **`IsParentLoading` 必須傳遞**：避免 Table 的 `OnInitializedAsync` 在父元件尚未載入完成時就嘗試載入資料，導致顯示舊資料。

3. **廠商/客戶未選時隱藏 Table**：使用 `style="display:none"` 而非 `@if`，避免 Table 元件被卸載重建（會丟失狀態）。

4. **`SaveXxxDetails` 要處理刪除**：儲存時需比較現有明細與畫面明細，刪除已移除的項目。

5. **`OnHasUndeletableDetailsChanged` 要觸發 UI 更新**：狀態變更後需呼叫 `InitializeFormFieldsAsync()` + `StateHasChanged()` 才能讓欄位鎖定生效。

6. **空白行由基底類別自動管理**：子類別只需在 `LoadItemsFromDetailsAsync()` 中填充 `Items`，**不需要**手動呼叫 `RefreshEmptyRow()`。

7. **`SelectedSupplierId` 同時用於客戶 ID**：銷售模組將 `SelectedCustomerId` 綁定到基底的 `SelectedSupplierId` 參數：
   ```razor
   SelectedSupplierId="@(int?)selectedCustomerId"
   ```

8. **模組特有參數保留在子類別**：例如 `SalesOrderTable` 的 `SelectedQuotationId`、`FilterProductId`；`SalesDeliveryTable` 的 `ShowWarehouse`。這些不在基底類別，子類別自行宣告追蹤。

9. **審核前自動儲存（部分模組）**：採購單等模組在核准前會先自動儲存，確保審核的是最新資料。若新增模組需要此行為，在 `HandleXxxApprove` 中實作儲存邏輯。

---

## 🔗 相關文件

- [README_進銷存模組設計總綱.md](README_進銷存模組設計總綱.md)
- [README_BaseDetailTableComponent設計.md](README_BaseDetailTableComponent設計.md)
- [README_表單設計規範.md](README_表單設計規範.md)
- [../README_審核_新模組實作指南.md](../README_審核_新模組實作指南.md)
