# 審核機制 — 各模組狀態與待辦項目

> 本文件記錄各模組審核功能的現況、已知問題，以及本輪修正的完整項目清單。

---

## 一、各模組詳細現況

### 1-1 報價單（Quotation）✅ 基本完整

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | `Quotation.cs` 完整 |
| EditModal UI | ✅ | `ShowApprovalSection` + `OnApprove` + `OnRejectWithReason` |
| 核准邏輯 | ⚠️ 簡易版 | 直接呼叫 `UpdateAsync(entity)` 含 `IsApproved=true`，無核准前明細保存 |
| 駁回邏輯 | ⚠️ 簡易版 | 同上，直接呼叫 `UpdateAsync` |
| Service 方法 | ❌ | 無 `ApproveAsync` / `RejectAsync` 專用方法 |
| Detail Table 封鎖 | ❌ | `IsReadOnly` 未由 `ApprovalConfigHelper.ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ❌ | `HandlePrint` 無審核狀態檢查 |
| 批次審核 | ✅ | `QuotationIndex.razor` 有 `BatchApprovalModalComponent` |
| Index 審核狀態欄 | ❌ | FieldConfiguration 未加 `IsApproved` 欄位 |
| PermissionRegistry | ✅ | `Quotation.Approve` 存在 |

---

### 1-2 採購訂單（PurchaseOrder）✅ 最完整的參考實作

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | 完整 |
| EditModal UI | ✅ | `ShowApprovalSection` + `OnApprove` + `OnRejectWithReason` |
| 核准邏輯 | ✅ | 先 `SavePurchaseOrderWithDetails(isPreApprovalSave:true)` → 再 `ApproveOrderAsync` |
| 駁回邏輯 | ✅ | `RejectOrderAsync` Service 方法 |
| Service 方法 | ✅ | `ApproveOrderAsync` + `RejectOrderAsync` |
| Detail Table 封鎖 | ❌ | 待確認 `IsReadOnly` 是否由 `ShouldLockFieldByApproval` 控制 |
| 列印審核檢查 | ❌ | `HandlePrint` 無審核狀態檢查 |
| 批次審核 | ✅ | `PurchaseOrderIndex.razor` 有 `BatchApprovalModalComponent` |
| Index 審核狀態欄 | ❌ | FieldConfiguration 未加 `IsApproved` 欄位 |
| PermissionRegistry | ✅ | `PurchaseOrder.Approve` 存在 |

---

### 1-3 銷貨單（SalesDelivery）⚠️ 實體有欄位，但 UI 完全缺失

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | ✅ | `SalesDelivery.cs` 完整 |
| EditModal UI | **❌ 完全缺失** | 未加 `ShowApprovalSection`，僅在新建時設 `IsApproved = false` |
| SystemParameter 開關 | **❌ 缺失** | `EnableSalesDeliveryApproval` 欄位尚未加入 |
| ApprovalSettingsTab | **❌ 缺失** | Tab 內無銷貨單開關 |
| Service 方法 | ❌ | 無 `ApproveAsync` / `RejectAsync` |
| Detail Table 封鎖 | ❌ | |
| 列印審核檢查 | ❌ | |
| 批次審核 | ❌ | |
| Index 審核狀態欄 | ❌ | |
| PermissionRegistry | ❌ | 無 `SalesDelivery.Approve` |

---

### 1-4 進貨單（PurchaseReceiving）❌ 有已知錯誤

| 項目 | 狀態 | 備註 |
|------|------|------|
| 實體審核欄位 | **❌ 缺失** | |
| EditModal 錯誤代碼 | **⛔ 需清除** | L190/205：`isApprovalEnabled = IsPurchaseOrderApprovalEnabledAsync()`（呼叫錯誤的開關方法，欄位完全無用） |
| Service 方法 | ❌ | |
| 其餘項目 | ❌ | |
| PermissionRegistry | ❌ | 無 `PurchaseReceiving.Approve` |

---

### 1-5 進貨退出（PurchaseReturn）❌ 尚未開始

| 項目 | 狀態 |
|------|------|
| 所有項目 | ❌ |
| PermissionRegistry | ❌（無 `PurchaseReturn.Approve`） |

---

### 1-6 銷售訂單（SalesOrder）❌ 尚未開始

| 項目 | 狀態 |
|------|------|
| 所有項目 | ❌ |
| PermissionRegistry | ❌（無 `SalesOrder.Approve`） |

---

### 1-7 銷貨退回（SalesReturn）❌ 尚未開始

| 項目 | 狀態 |
|------|------|
| 所有項目 | ❌ |
| PermissionRegistry | ❌（無 `SalesReturn.Approve`） |

---

## 二、通用基礎設施現況

| 元件/檔案 | 狀態 | 備註 |
|----------|------|------|
| `ApprovalConfigHelper` | ✅ 完整 | |
| `ApprovalSettingsTab.razor` | ✅ 存在，7 個開關 | 缺 SalesDelivery；使用卡片 + switch 樣式 |
| `BatchApprovalModalComponent.razor` | ✅ 泛型通用 | |
| `BatchApprovalTable.razor` | ✅ 泛型通用 | |
| `RejectConfirmModalComponent.razor` | ✅ 存在 | |
| `GenericEditModalComponent` 審核參數 | ✅ 支援 | `ShowApprovalSection`、`ApprovalPermission`、`OnApprove`、`OnRejectWithReason` |
| `GenericEditModalComponent` 列印審核 | ❌ 缺失 | `HandlePrint` 無 `CanPerformActionRequiringApproval` 檢查 |

---

## 三、本輪修正項目（優先順序排列）

### 🔴 第一批：DB 基礎（需要 Migration）

| # | 任務 | 檔案 | 依賴 |
|---|------|------|------|
| A1 | 加入 4 個實體的審核欄位 | `PurchaseReceiving.cs`、`PurchaseReturn.cs`、`SalesOrder.cs`、`SalesReturn.cs` | — |
| A2 | 加入 `EnableSalesDeliveryApproval` 到 SystemParameter | `SystemParameter.cs` | — |
| A3 | 建立 Migration | — | A1、A2 |
| A4 | Service 加入 `Include(x => x.ApprovedByUser)` | 4 個 Service | A1 |

> **一次 Migration 包含 A1 + A2 的全部欄位**：`AddApprovalFieldsToRemainingModules`

---

### 🔴 第二批：Service 方法

| # | 任務 | 說明 | 參考 |
|---|------|------|------|
| B1 | 5 個 Service 加入 `ApproveAsync` + `RejectAsync` | SalesDelivery、PurchaseReceiving、PurchaseReturn、SalesOrder、SalesReturn | `PurchaseOrderService.ApproveOrderAsync` |

---

### 🟠 第三批：EditModal UI

| # | 任務 | 檔案 | 依賴 |
|---|------|------|------|
| C1 | 修正 PurchaseReceiving 錯誤代碼 | `PurchaseReceivingEditModalComponent.razor`（刪除 L190/205） | — |
| C2 | SalesDelivery 加 `EnableSalesDeliveryApproval` 開關讀取 + `ShowApprovalSection` | `SalesDeliveryEditModalComponent.razor` | A2、B1 |
| C3 | ApprovalSettingsTab 加 SalesDelivery 開關 | `ApprovalSettingsTab.razor` | A2 |
| C4 | 4 個新 EditModal 加審核 UI | 4 個 EditModal | A1、B1 |
| C5 | PermissionRegistry 加 5 個 Approve 權限 | `Models/PermissionRegistry.cs` | — |

---

### 🟠 第四批：GenericEditModal 列印修正

| # | 任務 | 檔案 |
|---|------|------|
| D1 | `HandlePrint` 加入 `CanPrintCheck` 參數支援 | `GenericEditModalComponent.razor` |
| D2 | 7 個 EditModal 傳入 `CanPrintCheck` | 各 EditModal |

---

### 🟡 第五批：Detail Table 封鎖

| # | 任務 | 說明 |
|---|------|------|
| E1 | 各 EditModal 將 Table 的 `IsReadOnly` 改為 `ApprovalConfigHelper.ShouldLockFieldByApproval(...)` | 確保審核後 Table 不可編輯 |

---

### 🟡 第六批：批次審核 + Index 狀態欄

| # | 任務 | 說明 |
|---|------|------|
| F1 | 5 個 Index 加 `BatchApprovalModalComponent` | PurchaseReceiving、PurchaseReturn、SalesOrder、SalesReturn、SalesDelivery |
| F2 | 7 個模組 FieldConfiguration 加 `IsApproved` 狀態欄 | 顯示「已審核 / 未審核 / 已駁回」badge |

---

### 🟢 第七批：審核歷史（可延後）

| # | 任務 |
|---|------|
| G1 | 建立 `ApprovalHistory` 實體（EntityType、EntityId、Action、ByUserId、At、Reason）|
| G2 | `IApprovalHistoryService` + `ApprovalHistoryService` |
| G3 | `ApprovalHistoryTab.razor`（通用，接收 entityType + entityId）|
| G4 | 各 EditModal 加入「審核歷史」Tab |

---

## 四、核准前自動儲存的設計決策

### 問題背景

現有兩種核准模式不一致：

| 模組 | 模式 | 說明 |
|------|------|------|
| Quotation | 簡易版 | 直接設 `IsApproved = true` → `UpdateAsync(entity)` |
| PurchaseOrder | 完整版 | 先 `SaveWithDetails(isPreApprovalSave: true)` → `ApproveOrderAsync()` |

Quotation 簡易版的風險：若明細有未儲存變更（Detail Table 的異動通常已同步到 entity，但邊界情況下可能有差異），不會在核准時一起保存。

### 統一決策

**所有模組統一使用「完整版」**（同 PurchaseOrder）：

```csharp
private async Task<bool> HandleXxxApprove()
{
    // 1. 驗證
    // 2. 確認對話框
    // 3. 先儲存含明細（isPreApprovalSave: true 允許在已核准狀態下儲存）
    var saveOk = await SaveXxxWithDetails(editModalComponent!.Entity, isPreApprovalSave: true);
    if (!saveOk) return false;
    // 4. 呼叫 Service.ApproveAsync(id, userId)
}
```

其中 `SaveXxxWithDetails(entity, isPreApprovalSave)` 必須在 `CanSaveWhenApproved` 檢查時傳入 `isPreApprovalSave: true`，使 `ApprovalConfigHelper` 允許此次儲存。
