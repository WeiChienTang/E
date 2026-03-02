# 審核機制 — 各模組狀態與待辦項目

> 本文件記錄各模組審核功能的現況、已知問題，以及各輪修正的完整項目清單。
> 最後更新：2026-03-02（本輪補強：Quotation + PurchaseOrder Detail Table 封鎖 + CanPrintCheck）

---

## 一、各模組詳細現況

### 1-1 報價單（Quotation）✅ 全部完成

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | `Quotation.cs` 完整 |
| EditModal UI | ✅ | `ShowApprovalSection` + `OnApprove` + `OnRejectWithReason` + `CanPrintCheck` |
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
| `ApprovalConfigHelper` | ✅ 完整 | |
| `ApprovalSettingsTab.razor` | ✅ 8 個開關 | 本輪加入 SalesDelivery；共 8 個模組開關 |
| `BatchApprovalModalComponent.razor` | ✅ 泛型通用 | |
| `BatchApprovalTable.razor` | ✅ 泛型通用 | |
| `RejectConfirmModalComponent.razor` | ✅ 存在 | |
| `GenericEditModalComponent` 審核參數 | ✅ 完整 | `ShowApprovalSection`、`ApprovalPermission`、`OnApprove`、`OnRejectWithReason` |
| `GenericEditModalComponent` 列印審核 | ✅ 本輪加入 | `CanPrintCheck` 參數 + `HandlePrint` 前置檢查 |

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

### isApprovalEnabled 的載入時機規則

若 EditModal 在 Razor 模板中有直接存取 `default!` 初始化的 Manager（如 `customerModalManager.IsModalVisible`），則 `await IsSalesOrderApprovalEnabledAsync()` **必須放在** 所有同步初始化完成之後，否則 Blazor 中間渲染時會 NullReferenceException：

```csharp
protected override async Task OnInitializedAsync()
{
    // ✅ 先做同步初始化
    modalManagers = ModalManagerInitHelper.CreateBuilder(...)...Build();
    customerModalManager = modalManagers.Get<Customer>(...);
    // ✅ 同步初始化完成後，才進行第一個 await
    isApprovalEnabled = await SystemParameterService.IsXxxApprovalEnabledAsync();
}
```
