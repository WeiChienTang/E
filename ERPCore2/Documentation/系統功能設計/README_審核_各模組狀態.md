# 審核機制 — 各模組狀態與待辦項目

> 本文件記錄各模組審核功能的現況、已知問題，以及各輪修正的完整項目清單。
> 最後更新：2026-03-02（本輪修正：補齊 5 個模組的完整審核 UI + Service 方法）

---

## 一、各模組詳細現況

### 1-1 報價單（Quotation）⚠️ 基本可用，細節待補

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | `Quotation.cs` 完整 |
| EditModal UI | ✅ | `ShowApprovalSection` + `OnApprove` + `OnRejectWithReason` |
| 核准邏輯 | ⚠️ 簡易版 | 直接呼叫 `UpdateAsync(entity)` 含 `IsApproved=true`，無 ApproveAsync |
| 駁回邏輯 | ⚠️ 簡易版 | 直接呼叫 `UpdateAsync`，無 RejectAsync |
| Service 方法 | ❌ | 無 `ApproveAsync` / `RejectAsync` 專用方法 |
| Detail Table 封鎖 | ❌ | `IsReadOnly` 未由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ❌ | 未傳入 `CanPrintCheck` 參數 |
| 批次審核 | ✅ | `QuotationIndex.razor` 有 `BatchApprovalModalComponent` |
| Index 審核狀態欄 | ❌ | FieldConfiguration 未加 `IsApproved` 欄位 |
| PermissionRegistry | ✅ | `Quotation.Approve` 存在 |

---

### 1-2 採購訂單（PurchaseOrder）✅ 最完整的參考實作

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | 完整 |
| EditModal UI | ✅ | `ShowApprovalSection` + `OnApprove` + `OnRejectWithReason` + `CanPrintCheck` |
| 核准邏輯 | ✅ | 先 `SavePurchaseOrderWithDetails(isPreApprovalSave:true)` → 再 `ApproveOrderAsync` |
| 駁回邏輯 | ✅ | `RejectOrderAsync` Service 方法 |
| Service 方法 | ✅ | `ApproveOrderAsync` + `RejectOrderAsync` |
| Detail Table 封鎖 | ❌ | 待確認 `IsReadOnly` 是否由 `ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ✅ | 有傳入 `CanPrintCheck` |
| 批次審核 | ✅ | `PurchaseOrderIndex.razor` 有 `BatchApprovalModalComponent` |
| Index 審核狀態欄 | ❌ | FieldConfiguration 未加 `IsApproved` 欄位 |
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
| 批次審核 | ❌ | 尚未實作 |
| Index 審核狀態欄 | ❌ | 尚未實作 |
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
| 批次審核 | ❌ | 尚未實作 |
| Index 審核狀態欄 | ❌ | 尚未實作 |
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
| 批次審核 | ❌ | 尚未實作 |
| Index 審核狀態欄 | ❌ | 尚未實作 |
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
| 批次審核 | ❌ | 尚未實作 |
| Index 審核狀態欄 | ❌ | 尚未實作 |
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
| 批次審核 | ❌ | 尚未實作 |
| Index 審核狀態欄 | ❌ | 尚未實作 |
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

---

## 四、尚未完成項目（待下一輪）

### 🟡 A4 — Service Include 查詢

4 個新 Service 的 `GetByIdAsync` / `GetAllAsync` 尚未加入：
```csharp
.Include(x => x.ApprovedByUser)
```
影響：EditModal「核准人員」顯示為空（功能不中斷，但 UI 資訊不完整）。

### 🟡 F1 — 5 個 Index 批次審核

| Index | 所需工作 |
|-------|---------|
| `PurchaseReceivingIndex.razor` | 加入 `BatchApprovalModalComponent` + 批次審核按鈕 |
| `PurchaseReturnIndex.razor` | 同上 |
| `SalesOrderIndex.razor` | 同上 |
| `SalesReturnIndex.razor` | 同上 |
| `SalesDeliveryIndex.razor` | 同上 |

### 🟡 F2 — 7 個模組 Index 審核狀態欄

所有模組的 `XxxFieldConfiguration.cs` 尚未加入 `IsApproved` 狀態 Badge 欄位。

### 🟡 Quotation/PurchaseOrder 補強

- `QuotationService`：補 `ApproveAsync` / `RejectAsync`（目前直接用 `UpdateAsync` 含 `IsApproved = true`）
- `PurchaseOrderEditModal` Detail Table：確認是否已由 `ShouldLockFieldByApproval` 控制

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
