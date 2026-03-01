# 審核機制設計說明（總綱）

> 本文件為 ERPCore2 單據審核機制（Approval Workflow）的**總綱**，說明設計原則、核心元件與各模組現況摘要。詳細內容請參閱子文件。

---

## 一、設計原則

### 1-1 審核是「全域開關」，而非強制要求

審核功能以 **SystemParameter** 控制，每種單據類型各有一個布林開關：
- 開關關閉 → 可自由儲存、轉單、列印
- 開關開啟 → 必須先審核通過，才能執行後續操作（轉單、列印），且已核准後無法直接修改

### 1-2 「誰可以審核」由 Role/Permission 控制

SystemParameter 不存放指定審核人員。需要審核權限請在 `PermissionRegistry` 中設定 `Xxx.Approve` 權限。

### 1-3 審核欄位統一命名規範

| 欄位名 | 型別 | 說明 |
|--------|------|------|
| `IsApproved` | `bool` | 是否已審核通過（預設 `false`） |
| `ApprovedBy` | `int?` | FK → `Employee.Id`，審核者 |
| `ApprovedAt` | `DateTime?` | 審核時間戳 |
| `RejectReason` | `string?` | 駁回原因（核准後應清空） |
| `ApprovedByUser` | `Employee?` | Navigation Property |

---

## 二、核心元件快速參考

| 元件 | 位置 | 說明 |
|------|------|------|
| `ApprovalConfigHelper` | `Helpers/EditModal/ApprovalConfigHelper.cs` | **唯一**審核邏輯入口，不可在元件直接硬寫判斷 |
| `SystemParameter` | `Data/Entities/Systems/SystemParameter.cs` | 各模組審核開關儲存 |
| `ApprovalSettingsTab` | `Components/Pages/Systems/SystemParameter/ApprovalSettingsTab.razor` | 系統參數 UI — 審核開關切換 |
| `BatchApprovalModalComponent` | `Components/Pages/Purchase/BatchApprovalModalComponent.razor` | 通用批次審核 Modal（泛型 `TEntity`） |
| `BatchApprovalTable` | `Components/Pages/Purchase/BatchApprovalTable.razor` | 批次審核 Modal 內部表格 |
| `RejectConfirmModalComponent` | `Components/Pages/Purchase/RejectConfirmModalComponent.razor` | 駁回原因輸入 Modal |
| `GenericEditModalComponent` | `Components/Shared/Modal/GenericEditModalComponent.razor` | 核准/駁回按鈕區塊（`ShowApprovalSection` 系列參數） |

### ApprovalConfigHelper 邏輯摘要

```csharp
// 欄位是否唯讀
ShouldLockFieldByApproval(isApprovalEnabled, isApproved, hasUndeletableDetails)

// 轉單/列印等操作是否允許
CanPerformActionRequiringApproval(isApprovalEnabled, isApproved)

// 儲存是否允許（isPreApprovalSave=true 供核准前自動儲存用）
CanSaveWhenApproved(isApprovalEnabled, isApproved, isPreApprovalSave = false)
```

| 情境 | ShouldLockField | CanPerformAction | CanSave |
|------|-----------------|------------------|---------|
| 審核關閉，無下一步 | `false` | `true` | `true` |
| 審核關閉，有下一步 | `true` | `true` | `true` |
| 審核開啟，未審核 | `false` | `false` | `true` |
| 審核開啟，已核准 | `true` | `true` | `false` |
| 審核開啟，已核准，isPreApprovalSave | `true` | `true` | `true` |

---

## 三、各模組現況摘要

> 詳細說明與待辦項目請見 [README_審核_各模組狀態.md](README_審核_各模組狀態.md)

| 模組 | 實體欄位 | EditModal UI | Service方法 | 批次審核 | Index狀態欄 |
|------|---------|-------------|------------|---------|------------|
| 報價單 | ✅ | ✅ 完整 | ❌（用 UpdateAsync） | ✅ QuotationIndex | ❌ |
| 採購訂單 | ✅ | ✅ 完整 | ✅ ApproveOrderAsync | ✅ PurchaseOrderIndex | ❌ |
| **銷貨單** | ✅ | **❌ UI 缺失** | ❌ | ❌ | ❌ |
| 進貨單 | ❌ | ❌ | ❌ | ❌ | ❌ |
| 進貨退出 | ❌ | ❌ | ❌ | ❌ | ❌ |
| 銷售訂單 | ❌ | ❌ | ❌ | ❌ | ❌ |
| 銷貨退回 | ❌ | ❌ | ❌ | ❌ | ❌ |

**SystemParameter 審核開關現況**

| 開關欄位 | 存在 | ApprovalSettingsTab 可切換 |
|---------|------|---------------------------|
| `EnableQuotationApproval` | ✅ | ✅ |
| `EnablePurchaseOrderApproval` | ✅ | ✅ |
| `EnablePurchaseReceivingApproval` | ✅ | ✅ |
| `EnablePurchaseReturnApproval` | ✅ | ✅ |
| `EnableSalesOrderApproval` | ✅ | ✅ |
| `EnableSalesReturnApproval` | ✅ | ✅ |
| `EnableInventoryTransferApproval` | ✅ | ✅ |
| `EnableSalesDeliveryApproval` | **❌ 尚未加入** | ❌ |

---

## 四、文件導覽

| 文件 | 說明 |
|------|------|
| [README_審核_各模組狀態.md](README_審核_各模組狀態.md) | 各模組詳細現況、待辦清單、本輪修正項目 |
| [README_審核_新模組實作指南.md](README_審核_新模組實作指南.md) | 完整步驟：為新模組加入審核功能 |

---

## 五、完整審核流程

```
使用者編輯單據
     │
     ├─ 按「儲存」
     │       ↓
     │   ApprovalConfigHelper.CanSaveWhenApproved()
     │       ├─ 審核關閉 → 允許
     │       ├─ 審核開啟 + 未核准 → 允許
     │       └─ 審核開啟 + 已核准 → ❌ 顯示警告
     │
     ├─ 按「核准」（HandleApproveAsync）
     │       ↓
     │   SaveEntityWithDetails(isPreApprovalSave: true)  ← 先儲存含明細
     │       ↓
     │   EntityService.ApproveAsync(id, userId)
     │       ↓
     │   欄位變唯讀 / Detail Table 封鎖 / 轉單與列印開放
     │
     ├─ 按「駁回」（HandleRejectWithReasonAsync）
     │       ↓
     │   輸入駁回原因（RejectConfirmModalComponent）
     │       ↓
     │   EntityService.RejectAsync(id, userId, reason)
     │       ↓
     │   欄位恢復可編輯
     │
     └─ 執行需審核後才能做的操作（轉單、列印）
             ↓
         ApprovalConfigHelper.CanPerformActionRequiringApproval()
             ├─ 審核關閉 → 執行
             └─ 審核開啟 + 未核准 → ❌ 顯示「需先審核通過」
```

---

## 六、常見問題

### Q: 開啟審核後，歷史資料 IsApproved = false 怎麼辦？

欄位仍可編輯，但轉單/列印需先手動核准。批次核准請使用各模組 Index 的「批次審核」按鈕。

### Q: 核准後發現資料有誤？

按「駁回」→ 輸入原因 → 欄位解鎖 → 修改 → 重新核准。

### Q: 銷貨單（SalesDelivery）的審核狀態？

實體已有審核欄位，但 EditModal UI 尚未實作，`EnableSalesDeliveryApproval` 欄位也尚未加入 SystemParameter。屬於**本輪修正項目**。詳見 [README_審核_各模組狀態.md](README_審核_各模組狀態.md)。
