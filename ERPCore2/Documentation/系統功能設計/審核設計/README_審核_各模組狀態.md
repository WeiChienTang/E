# 審核機制 — 各模組狀態與待辦項目

> 本文件記錄各模組審核功能的現況、已知問題，以及各輪修正的完整項目清單。
> 最後更新：2026-03-03（第五輪：審核語意重構 — 啟用/停用 → 系統自動/人工審核）

---

## 一、各模組詳細現況

### 1-1 報價單（Quotation）✅ 全部完成

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | `Quotation.cs` 完整 |
| EditModal UI | ✅ | `ShowApprovalSection` + `OnApprove` + `OnRejectWithReason` + `CanPrintCheck` |
| EditModal 審核資訊 Section | ✅ | 第四輪：IsApproved/ApprovedByUser.FullName/ApprovedAt/RejectReason 唯讀顯示 |
| 核准邏輯 | ✅ | 先 `SaveQuotationWithDetails(entity)` → 再 `ApproveAsync(id, userId)` |
| 駁回邏輯 | ✅ | 呼叫 `RejectAsync(id, userId, reason)` |
| Service 方法 | ✅ | `ApproveAsync` + `RejectAsync`；`GetAllAsync` 加入 `Include(ApprovedByUser)` |
| Detail Table 封鎖 | ✅ | `IsReadOnly` 由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ✅ | 已傳入 `CanPrintCheck` |
| 批次審核 | ✅ | `QuotationIndex.razor` 使用 `ApproveAsync(id, userId)` |
| Index 審核狀態欄 | ✅ | `QuotationFieldConfiguration` 已有 `IsApproved` Badge 欄位 |
| PermissionRegistry | ✅ | `Quotation.Approve` 存在 |

---

### 1-2 採購訂單（PurchaseOrder）✅ 全部完成（最完整的參考實作）

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | 完整 |
| EditModal UI | ✅ | `ShowApprovalSection` + `OnApprove` + `OnRejectWithReason` + `CanPrintCheck` |
| EditModal 審核資訊 Section | ✅ | 第四輪：IsApproved/ApprovedByUser.FullName/ApprovedAt/RejectReason 唯讀顯示 |
| 核准邏輯 | ✅ | 先 `SavePurchaseOrderWithDetails(isPreApprovalSave:true)` → 再 `ApproveOrderAsync` |
| 駁回邏輯 | ✅ | `RejectOrderAsync` Service 方法 |
| Service 方法 | ✅ | `ApproveOrderAsync` + `RejectOrderAsync` |
| Detail Table 封鎖 | ✅ | `IsReadOnly` 由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ✅ | 已傳入 `CanPrintCheck` |
| 批次審核 | ✅ | `PurchaseOrderIndex.razor` 使用 `ApproveOrderAsync(id, userId)` |
| Index 審核狀態欄 | ✅ | `PurchaseOrderFieldConfiguration` 已有 `IsApproved` Badge 欄位 |
| PermissionRegistry | ✅ | `PurchaseOrder.Approve` 存在 |

---

### 1-3 銷貨出貨（SalesDelivery）✅ 本輪完成

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | `SalesDelivery.cs` 本輪前已有欄位 |
| EditModal UI | ✅ | 本輪加入完整審核 UI |
| EditModal 審核資訊 Section | ✅ | 第四輪：IsApproved/ApprovedByUser.FullName/ApprovedAt/RejectReason 唯讀顯示 |
| SystemParameter 開關 | ✅ | `EnableSalesDeliveryApproval` 本輪加入（含 Migration） |
| ApprovalSettingsTab | ✅ | 本輪加入 SalesDelivery 切換開關 |
| Service 方法 | ✅ | `ApproveAsync` + `RejectAsync` 本輪加入 |
| Detail Table 封鎖 | ✅ | `IsReadOnly` 已由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ✅ | 已傳入 `CanPrintCheck` |
| 批次審核 | ✅ | `SalesDeliveryIndex.razor` 加入 `BatchApprovalModalComponent` |
| Index 審核狀態欄 | ✅ | `SalesDeliveryFieldConfiguration` 加入 `IsApproved` Badge 欄位 |
| PermissionRegistry | ✅ | `SalesDelivery.Approve` 本輪加入 |

---

### 1-4 進貨單（PurchaseReceiving）✅ 本輪完成

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | 本輪加入（含 Migration） |
| EditModal UI | ✅ | 本輪加入完整審核 UI；同時修正原有死代碼（錯誤呼叫 PurchaseOrder 開關） |
| EditModal 審核資訊 Section | ✅ | 第四輪：IsApproved/ApprovedByUser.FullName/ApprovedAt/RejectReason 唯讀顯示 |
| SystemParameter 開關 | ✅ | `EnablePurchaseReceivingApproval` 本輪前已存在 |
| Service 方法 | ✅ | `ApproveAsync` + `RejectAsync` 本輪加入 |
| Detail Table 封鎖 | ✅ | `IsReadOnly` 已由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ✅ | 已傳入 `CanPrintCheck` |
| 批次審核 | ✅ | `PurchaseReceivingIndex.razor` 加入 `BatchApprovalModalComponent` |
| Index 審核狀態欄 | ✅ | `PurchaseReceivingFieldConfiguration` 加入 `IsApproved` Badge 欄位 |
| PermissionRegistry | ✅ | `PurchaseReceiving.Approve` 本輪加入 |

---

### 1-5 進貨退回（PurchaseReturn）✅ 本輪完成

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | 本輪加入（含 Migration） |
| EditModal UI | ✅ | 本輪加入完整審核 UI |
| EditModal 審核資訊 Section | ✅ | 第四輪：IsApproved/ApprovedByUser.FullName/ApprovedAt/RejectReason 唯讀顯示 |
| SystemParameter 開關 | ✅ | `EnablePurchaseReturnApproval` 本輪前已存在 |
| Service 方法 | ✅ | `ApproveAsync` + `RejectAsync` 本輪加入 |
| Detail Table 封鎖 | ✅ | `IsReadOnly` 已由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ✅ | 已傳入 `CanPrintCheck` |
| 批次審核 | ✅ | `PurchaseReturnIndex.razor` 加入 `BatchApprovalModalComponent` |
| Index 審核狀態欄 | ✅ | `PurchaseReturnFieldConfiguration` 加入 `IsApproved` Badge 欄位 |
| PermissionRegistry | ✅ | `PurchaseReturn.Approve` 本輪加入 |

---

### 1-6 銷售訂單（SalesOrder）✅ 本輪完成

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | 本輪加入（含 Migration） |
| EditModal UI | ✅ | 本輪加入完整審核 UI |
| EditModal 審核資訊 Section | ✅ | 第四輪：IsApproved/ApprovedByUser.FullName/ApprovedAt/RejectReason 唯讀顯示 |
| SystemParameter 開關 | ✅ | `EnableSalesOrderApproval` 本輪前已存在 |
| Service 方法 | ✅ | `ApproveAsync` + `RejectAsync` 本輪加入 |
| Detail Table 封鎖 | ✅ | `IsReadOnly` 已由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ✅ | 已傳入 `CanPrintCheck` |
| 批次審核 | ✅ | `SalesOrderIndex.razor` 加入 `BatchApprovalModalComponent` |
| Index 審核狀態欄 | ✅ | `SalesOrderFieldConfiguration` 加入 `IsApproved` Badge 欄位 |
| PermissionRegistry | ✅ | `SalesOrder.Approve` 本輪加入 |

---

### 1-7 銷貨退回（SalesReturn）✅ 本輪完成

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | 本輪加入（含 Migration） |
| EditModal UI | ✅ | 本輪加入完整審核 UI |
| EditModal 審核資訊 Section | ✅ | 第四輪：IsApproved/ApprovedByUser.FullName/ApprovedAt/RejectReason 唯讀顯示 |
| SystemParameter 開關 | ✅ | `EnableSalesReturnApproval` 本輪前已存在 |
| Service 方法 | ✅ | `ApproveAsync` + `RejectAsync` 本輪加入 |
| Detail Table 封鎖 | ✅ | `IsReadOnly` 已由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ✅ | 已傳入 `CanPrintCheck` |
| 批次審核 | ✅ | `SalesReturnIndex.razor` 加入 `BatchApprovalModalComponent` |
| Index 審核狀態欄 | ✅ | `SalesReturnFieldConfiguration` 加入 `IsApproved` Badge 欄位 |
| PermissionRegistry | ✅ | `SalesReturn.Approve` 本輪加入 |

---

## 二、通用基礎設施現況

| 元件/檔案 | 狀態 | 備註 |
|----------|------|------|
| `ApprovalConfigHelper` | ✅ 完整 | 第五輪：參數 `isApprovalEnabled` → `isManualApproval`（語意更新） |
| `ApprovalSettingsTab.razor` | ✅ 8 個開關 | 第五輪：UI 改為人工/自動切換（badge 顏色 warning/success） |
| `BatchApprovalModalComponent.razor` | ✅ 泛型通用 | |
| `BatchApprovalTable.razor` | ✅ 泛型通用 | |
| `RejectConfirmModalComponent.razor` | ✅ 存在 | |
| `GenericEditModalComponent` 審核參數 | ✅ 完整 | `ShowApprovalSection`、`ApprovalPermission`、`OnApprove`、`OnRejectWithReason` |
| `GenericEditModalComponent` 列印審核 | ✅ 第四輪加入 | `CanPrintCheck` 參數 + `HandlePrint` 前置檢查 |

---

## 三、本輪（2026-03-02）已完成項目

### ✅ Batch A — 實體欄位 + SystemParameter（含 Migration）

- `PurchaseReceiving.cs`：加入 `IsApproved`、`ApprovedBy`、`ApprovedAt`、`RejectReason`、`ApprovedByUser`
- `PurchaseReturn.cs`：同上
- `SalesOrder.cs`：同上
- `SalesReturn.cs`：同上
- `SystemParameter.cs`：加入 `EnableSalesDeliveryApproval`
- Migration：`AddApprovalFieldsToRemainingModules` 執行完畢

### ✅ Batch B — Service 方法

5 個 Service 各加入 `ApproveAsync(int id, int approvedBy)` + `RejectAsync(int id, int rejectedBy, string reason)`：
- `PurchaseReceivingService` / `IPurchaseReceivingService`
- `PurchaseReturnService` / `IPurchaseReturnService`
- `SalesOrderService` / `ISalesOrderService`
- `SalesReturnService` / `ISalesReturnService`
- `SalesDeliveryService` / `ISalesDeliveryService`

同步加入：`ISystemParameterService` 的 `ApprovalType.SalesDelivery` 列舉值與 `IsSalesDeliveryApprovalEnabledAsync()` 方法。

### ✅ Batch C — PermissionRegistry + ApprovalSettingsTab + Resx

- `PermissionRegistry.cs`：加入 5 個 `Xxx.Approve` 常數及 `GetAllPermissions()` 定義
- `ApprovalSettingsTab.razor`：加入 SalesDelivery 切換開關
- 5 個語言 resx：加入 `Field.EnableSalesDeliveryApproval` 鍵值

### ✅ Batch D — GenericEditModalComponent CanPrintCheck

- 加入 `[Parameter] public Func<bool>? CanPrintCheck { get; set; }`
- `HandlePrint()` 開頭加入 `CanPrintCheck` 檢查

### ✅ Batch E+F — 5 個 EditModal 完整審核 UI + Detail Table 封鎖

| EditModal | 注入方式 | isApprovalEnabled 載入位置 |
|-----------|----------|--------------------------|
| `PurchaseReceivingEditModalComponent` | UseGenericSave + AfterSave | `LoadSalesReceivingData()` |
| `SalesDeliveryEditModalComponent` | UseGenericSave + AfterSave | `OnInitializedAsync` |
| `PurchaseReturnEditModalComponent` | SaveHandler | `OnInitializedAsync`（override 新增） |
| `SalesOrderEditModalComponent` | SaveHandler | `OnInitializedAsync`（在 ModalManager 初始化**之後** await） |
| `SalesReturnEditModalComponent` | SaveHandler + CustomValidator | `OnParametersSetAsync`（首次載入時） |

每個 EditModal 均完成：
- `isApprovalEnabled` 欄位與載入
- `@inject AuthenticationStateProvider`（原本缺少的補上）
- GenericEditModalComponent 5 個審核參數
- `CanSaveWhenApproved` 守衛
- `ShouldLockFieldByApproval` 控制 Detail Table `IsReadOnly`
- `ShouldShowApprovalSection()`、`GetCanPrintCheck()`、`HandleXxxApprove()`、`HandleXxxRejectWithReason()` 方法

### ✅ Bug 修正

**SalesOrderEditModalComponent NullReferenceException**：`customerModalManager` 等欄位宣告為 `= default!`（實際為 null），當 `OnInitializedAsync` 在 ModalManager 初始化前 `await` 時，Blazor 中間渲染會存取 `customerModalManager.IsModalVisible` 導致 NullReferenceException 無限循環。修正方式：將 `await IsSalesOrderApprovalEnabledAsync()` 移至 ModalManager `.Build()` 之後。

### ✅ F1 — 5 個 Index 批次審核

| Index | 狀態 |
|-------|------|
| `PurchaseReceivingIndex.razor` | ✅ 加入 `BatchApprovalModalComponent` + 完整 Handler 方法 |
| `PurchaseReturnIndex.razor` | ✅ 同上 |
| `SalesOrderIndex.razor` | ✅ 同上 |
| `SalesReturnIndex.razor` | ✅ 同上 |
| `SalesDeliveryIndex.razor` | ✅ 同上 |

每個 Index 均使用 `CurrentUserHelper.GetCurrentEmployeeIdAsync` 取得使用者 ID，並呼叫 `ApproveAsync(entity.Id, userId.Value)`。

### ✅ F2 — 5 個模組 Index 審核狀態欄

5 個 `XxxFieldConfiguration.cs` 加入條件式 `IsApproved` Badge 欄位：
- `PurchaseReceivingFieldConfiguration` — TableOrder=6
- `PurchaseReturnFieldConfiguration` — TableOrder=7
- `SalesOrderFieldConfiguration` — TableOrder=7
- `SalesDeliveryFieldConfiguration` — TableOrder=8
- `SalesReturnFieldConfiguration` — TableOrder=6

Badge 顯示：`已核准（bg-success）` / `待核准（bg-warning）`；只在 `_enableApproval = true` 時加入欄位。

### ✅ Quotation + PurchaseOrder 補強（2026-03-02 第二輪）

**報價單（QuotationEditModalComponent）**：
- `CanPrintCheck="@GetCanPrintCheck()"` 加入 `GenericEditModalComponent` 呼叫
- `QuotationTable` 的 `IsReadOnly` 改由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制
- 新增 `GetCanPrintCheck()` 方法

**採購訂單（PurchaseOrderEditModalComponent）**：
- `CanPrintCheck="@GetCanPrintCheck()"` 加入 `GenericEditModalComponent` 呼叫
- `PurchaseOrderTable` 的 `IsReadOnly` 改由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制
- 新增 `GetCanPrintCheck()` 方法

**A4 — Service Include**（確認完成）：
- 所有 5 個新 Service 的 `GetAllAsync` / `GetByIdAsync` 均已加入 `.Include(x => x.ApprovedByUser)`
- `SalesDeliveryService` 本輪補加；其他 4 個（PurchaseReceiving/PurchaseReturn/SalesOrder/SalesReturn）在 E 批次時已一併加入

---

### ✅ 第三輪補強（2026-03-02）— 警告訊息精簡 + 權限修正

**問題背景**：部分單據（如採購訂單）會同時顯示「已審核鎖定」與「明細有關聯操作」兩條訊息，造成版面冗餘。另外，`ApprovalPermission` 在兩個元件中使用原始字串（非 PermissionRegistry 常數）。

**G1 — GenericLockedFieldMessage 新增 Compact 模式**

`Components/Shared/UI/Message/GenericLockedFieldMessage.razor` 加入 `Compact` bool 參數（預設 `false`）：
- `Compact=false`（原有）：Bootstrap alert 全寬顯示
- `Compact=true`（新增）：左側邊框小字樣式（`border-start border-3 border-warning` + 鎖頭圖示），節省版面

**G2 — EditModalMessages 新增並簡化訊息常數**

`Helpers/EditModal/EditModalMessages.cs`：
- 新增 `ApprovedWarning`（通用）：`"已審核通過 — 主檔欄位已鎖定，如需修改請先執行「駁回」"`
- 縮短 `UndeletableDetailsWarning`：`"部分明細有關聯操作 — 主檔欄位已鎖定"`
- 縮短 `PurchaseOrderApprovedWarning`：`"採購單已審核 — 欄位已鎖定（仍可修改明細完成進貨狀態）"`
- 縮短 `SalesOrderApprovedWarning`：`"銷貨訂單已審核 — 欄位已鎖定（仍可修改明細完成出貨狀態）"`

**G3 — 全 7 個 EditModal 的 GetWarningMessage() 統一採用優先邏輯**

審核訊息優先，明細關聯訊息其次，均使用 `Compact="true"` 縮排顯示。舊的雙訊息同時顯示問題已解決。

```csharp
private RenderFragment? GetWarningMessage() => __builder =>
{
    if (!isDetailDataReady) return;

    if (isApprovalEnabled && editModalComponent?.Entity?.IsApproved == true)
    {
        <GenericLockedFieldMessage IsVisible="true"
                                  Message="@EditModalMessages.ApprovedWarning"
                                  Compact="true" />
        return;  // ← 優先顯示審核訊息，不再疊加
    }

    <GenericLockedFieldMessage IsVisible="@hasUndeletableDetails"
                              Message="@EditModalMessages.UndeletableDetailsWarning"
                              Compact="true" />
};
```

已修改的 EditModal（7 個）：
- `QuotationEditModalComponent`（第二輪補強時即已更新）
- `PurchaseOrderEditModalComponent`（第二輪補強時即已更新）
- `SalesOrderEditModalComponent`（第二輪補強時即已更新）
- `SalesDeliveryEditModalComponent`（本輪）
- `SalesReturnEditModalComponent`（本輪）
- `PurchaseReceivingEditModalComponent`（本輪）
- `PurchaseReturnEditModalComponent`（本輪）

**G4 — 修正 ApprovalPermission 原始字串**

`QuotationEditModalComponent` 與 `PurchaseOrderEditModalComponent` 中，將：
```razor
ApprovalPermission="Quotation.Approve"
ApprovalPermission="PurchaseOrder.Approve"
```
改為：
```razor
ApprovalPermission="@PermissionRegistry.Quotation.Approve"
ApprovalPermission="@PermissionRegistry.PurchaseOrder.Approve"
```
（防止拼寫錯誤、享有編譯期型別檢查）

---

## 三-B、第四輪（2026-03-02）已完成項目 — 審核資訊 Section

### ✅ 目標

EditModal 在「審核功能啟用 + 編輯現有單據」時，於表單最下方顯示唯讀的「審核資訊」區段，讓使用者直接看到誰審核、何時審核、目前狀態與駁回原因，無需另查。

### ✅ 基礎設施

- `FormSectionNames.ApprovalInfo = "Section.ApprovalInfo"` — 常數加入 `FormSectionHelper.cs`
- `Section.ApprovalInfo`、`Approval.Status`、`Approval.ApprovedBy`、`Approval.ApprovedAt` — 4 個 key 加入全 5 語言 resx

### ✅ 7 個 EditModal 統一改動

每個 EditModal 的 `InitializeFormFieldsAsync`（或等效方法）各加入：

**formFields**（4 個新欄位，全設 `IsDisabled=true`）：
- `IsApproved` → `FormFieldType.Select`，選項：已核准 / 待審核
- `ApprovedByUser.FullName` → `FormFieldType.Text`（dot-notation 嵌套屬性）
- `ApprovedAt` → `FormFieldType.DateTime`
- `RejectReason` → `FormFieldType.Text`（PurchaseOrder 已有此欄位，本輪重構位置並加 `IsDisabled=true`）

**formSections**（條件式 AddCustomFieldsIf）：
```csharp
.AddCustomFieldsIf(
    isApprovalEnabled && XxxId.HasValue && XxxId.Value > 0,
    FormSectionNames.ApprovalInfo,
    nameof(XxxEntity.IsApproved),
    "ApprovedByUser.FullName",
    nameof(XxxEntity.ApprovedAt),
    nameof(XxxEntity.RejectReason))
```

| EditModal | 實體類型 | 條件變數 |
|-----------|---------|---------|
| `QuotationEditModalComponent` | `Quotation` | `QuotationId` |
| `PurchaseOrderEditModalComponent` | `PurchaseOrder` | `PurchaseOrderId` |
| `PurchaseReceivingEditModalComponent` | `PurchaseReceiving` | `PurchaseReceivingId` |
| `PurchaseReturnEditModalComponent` | `PurchaseReturn` | `PurchaseReturnId` |
| `SalesOrderEditModalComponent` | `SalesOrder` | `SalesOrderId` |
| `SalesDeliveryEditModalComponent` | `SalesDelivery` | `SalesDeliveryId` |
| `SalesReturnEditModalComponent` | `SalesReturn` | `SalesReturnId` |

### ✅ Service Include 確認（無需改動）

全 7 個 Service 的 `GetByIdAsync` 均已有 `.Include(x => x.ApprovedByUser)`，`ApprovedByUser.FullName` 可正常顯示。

---

---

## 三-C、第五輪（2026-03-03）— 審核語意重構：啟用/停用 → 系統自動/人工審核

### ✅ 核心概念異動

| 舊設計 | 新設計 |
|--------|--------|
| `false` = 審核停用（不需審核，直接通過） | `false` = **系統自動審核**（儲存後系統自動核准） |
| `true` = 審核啟用（需人工審核） | `true` = **人工審核**（需人員點擊核准按鈕） |
| 停用時單據永遠不呼叫 `ApproveAsync` | 自動模式時儲存後立即呼叫 `ApproveAsync` |

> 審核永遠存在，差異在「由誰觸發」：系統自動 vs. 人工點擊。

---

### ✅ P1 — SystemParameter 欄位重命名（DB 欄位名稱不變）

`Data/Entities/Systems/SystemParameter.cs`：

| 舊 C# 屬性名 | 新 C# 屬性名 | DB 欄位（不變） |
|------------|------------|----------------|
| `EnableQuotationApproval` | `QuotationManualApproval` | `EnableQuotationApproval` |
| `EnablePurchaseOrderApproval` | `PurchaseOrderManualApproval` | `EnablePurchaseOrderApproval` |
| `EnablePurchaseReceivingApproval` | `PurchaseReceivingManualApproval` | `EnablePurchaseReceivingApproval` |
| `EnablePurchaseReturnApproval` | `PurchaseReturnManualApproval` | `EnablePurchaseReturnApproval` |
| `EnableSalesOrderApproval` | `SalesOrderManualApproval` | `EnableSalesOrderApproval` |
| `EnableSalesReturnApproval` | `SalesReturnManualApproval` | `EnableSalesReturnApproval` |
| `EnableSalesDeliveryApproval` | `SalesDeliveryManualApproval` | `EnableSalesDeliveryApproval` |
| `EnableInventoryTransferApproval` | `InventoryTransferManualApproval` | `EnableInventoryTransferApproval` |

使用 `[Column("OriginalName")]` attribute 讓 EF Core 對應舊欄位名稱，**不需 Migration**。

同步更新：`SystemParameterDefaults.cs` — 8 個常數重命名 + 補加缺失的 `DefaultSalesDeliveryManualApproval`。

---

### ✅ P2 — ISystemParameterService / SystemParameterService 方法重命名

| 舊方法名 | 新方法名 |
|---------|---------|
| `IsApprovalEnabledAsync(ApprovalType)` | `IsManualApprovalAsync(ApprovalType)` |
| `IsQuotationApprovalEnabledAsync()` | `IsQuotationManualApprovalAsync()` |
| `IsPurchaseOrderApprovalEnabledAsync()` | `IsPurchaseOrderManualApprovalAsync()` |
| `IsPurchaseReceivingApprovalEnabledAsync()` | `IsPurchaseReceivingManualApprovalAsync()` |
| `IsPurchaseReturnApprovalEnabledAsync()` | `IsPurchaseReturnManualApprovalAsync()` |
| `IsSalesOrderApprovalEnabledAsync()` | `IsSalesOrderManualApprovalAsync()` |
| `IsSalesReturnApprovalEnabledAsync()` | `IsSalesReturnManualApprovalAsync()` |
| `IsSalesDeliveryApprovalEnabledAsync()` | `IsSalesDeliveryManualApprovalAsync()` |
| `IsInventoryTransferApprovalEnabledAsync()` | `IsInventoryTransferManualApprovalAsync()` |

---

### ✅ P3 — ApprovalConfigHelper 參數重命名

所有 4 個方法的 `isApprovalEnabled` 參數改為 `isManualApproval`：
- `ShouldLockFieldByApproval(bool isManualApproval, ...)`
- `CanPerformActionRequiringApproval(bool isManualApproval, ...)`
- `CanSaveWhenApproved(bool isManualApproval, ...)`
- `GetApprovalWarningMessage(bool isManualApproval, ...)`

邏輯本身不變：`false` 在舊設計中代表「停用」（不鎖），在新設計中代表「自動審核」（也不鎖，因為自動審核後 `IsApproved=true`）。

---

### ✅ P4 — 7 個 EditModal：isApprovalEnabled → isManualApproval + 新增自動審核邏輯

所有 7 個 EditModal 完成以下變更：

1. `private bool isApprovalEnabled = X` → `private bool isManualApproval = false`
2. 服務方法呼叫 → `IsXxxManualApprovalAsync()`
3. 所有 `isApprovalEnabled` 參考 → `isManualApproval`（replace_all）
4. **新增儲存後自動審核邏輯**（在各自的儲存成功後）：

```csharp
// 自動審核：若為系統自動審核模式且尚未審核，儲存後自動核准
if (!isManualApproval && !result.Data.IsApproved)
{
    var autoApproveUserId = await CurrentUserHelper.GetCurrentEmployeeIdAsync(AuthenticationStateProvider);
    if (autoApproveUserId.HasValue)
        await XxxService.ApproveAsync(result.Data.Id, autoApproveUserId.Value);
}
```

> `PurchaseOrder` 例外：呼叫 `ApproveOrderAsync` 而非 `ApproveAsync`。

| EditModal | 自動審核插入位置 |
|-----------|----------------|
| `QuotationEditModalComponent` | `HandleQuotationSave()` 成功後 |
| `PurchaseOrderEditModalComponent` | `HandlePurchaseOrderSave()` 成功後 |
| `PurchaseReturnEditModalComponent` | `SavePurchaseReturn()` 成功後 |
| `SalesOrderEditModalComponent` | `SaveSalesOrder()` 成功後 |
| `SalesReturnEditModalComponent` | `SaveSalesReturn()` 成功後 |
| `SalesDeliveryEditModalComponent` | AfterSave callback 末尾（`StateHasChanged` 之前） |
| `PurchaseReceivingEditModalComponent` | AfterSave callback 末尾（`StateHasChanged` 之前） |

---

### ✅ P5 — 7 個 Index 頁面：isApprovalEnabled → isManualApproval

| Index | 異動類型 |
|-------|---------|
| `PurchaseReceivingIndex` | replace_all + 服務方法更新 |
| `PurchaseReturnIndex` | replace_all + 服務方法更新 |
| `SalesOrderIndex` | replace_all + 服務方法更新 |
| `SalesDeliveryIndex` | replace_all + 服務方法更新 |
| `SalesReturnIndex` | replace_all + 服務方法更新 |
| `QuotationIndex` | 原本 `ShowBatchApprovalButton="true"` → 改為動態載入 `@isManualApproval` |
| `PurchaseOrderIndex` | 同上 |

---

### ✅ P6 — ApprovalSettingsTab UI 重新設計

| 項目 | 舊 | 新 |
|------|-----|-----|
| 卡片邊框 | `border-success`（啟用）/ `border-secondary`（停用） | `border-warning`（人工）/ `border-success`（自動） |
| 按鈕樣式 | `btn-success`（啟用）/ `btn-secondary`（停用） | `btn-warning`（人工）/ `btn-success`（自動） |
| Badge 顯示 | `Toggle.Enabled` / `Toggle.Disabled` | `Toggle.Manual` / `Toggle.Auto` |
| 批次按鈕 | `Button.EnableAll` / `Button.DisableAll` | `Button.SetAllManual` / `Button.SetAllAuto` |
| 批次方法 | `SetEnableAll()` / `SetDisableAll()` | `SetAllManual()` / `SetAllAuto()` |

新增 resx keys（全 5 語言）：`Toggle.Manual`、`Toggle.Auto`、`Button.SetAllManual`、`Button.SetAllAuto`

---

## 四、尚未完成項目（待下一輪）

### 🟢 G — 審核歷史（可延後）

| # | 任務 |
|---|------|
| G1 | 建立 `ApprovalHistory` 實體 |
| G2 | `IApprovalHistoryService` + `ApprovalHistoryService` |
| G3 | `ApprovalHistoryTab.razor` |
| G4 | 各 EditModal 加入「審核歷史」Tab |

---

## 五、設計備忘

### RejectAsync 欄位行為

本輪實作的 `RejectAsync` 採用「清空核准資訊」策略：

```csharp
entity.IsApproved  = false;
entity.ApprovedBy  = null;   // 清空
entity.ApprovedAt  = null;   // 清空
entity.RejectReason = reason; // 保留駁回原因
```

> 注意：實作指南範本中 `ApprovedBy = rejectedBy`、`ApprovedAt = DateTime.Now`（記錄駁回人與時間）。
> 兩種策略皆可運作，未來統一規範時可選擇其一並全面對齊。

### isManualApproval 的載入時機規則（第三輪原名 isApprovalEnabled，第五輪改名）

若 EditModal 在 Razor 模板中有直接存取 `default!` 初始化的 Manager（如 `customerModalManager.IsModalVisible`），則 `await IsXxxManualApprovalAsync()` **必須放在** 所有同步初始化完成之後，否則 Blazor 中間渲染時會 NullReferenceException：

```csharp
protected override async Task OnInitializedAsync()
{
    // ✅ 先做同步初始化
    modalManagers = ModalManagerInitHelper.CreateBuilder(...)...Build();
    customerModalManager = modalManagers.Get<Customer>(...);
    // ✅ 同步初始化完成後，才進行第一個 await
    isManualApproval = await SystemParameterService.IsXxxManualApprovalAsync();
}
```

### 自動審核 vs 人工審核的 IsApproved 狀態

| 模式 | 儲存後 IsApproved | 批次審核按鈕 |
|------|-----------------|------------|
| 自動審核（isManualApproval=false） | 立即 = true（由 EditModal 呼叫 ApproveAsync） | 隱藏（Index 頁 ShowBatchApprovalButton="@isManualApproval"） |
| 人工審核（isManualApproval=true） | 儲存後 = false，需人工核准 | 顯示 |

> 注意：自動核准不會觸發任何 Notification，若日後需要審核通知，應在 `ApproveAsync` Service 中統一發送。
