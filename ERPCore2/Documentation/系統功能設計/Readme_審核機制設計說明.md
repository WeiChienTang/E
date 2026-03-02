# 審核機制設計說明（總綱）

> 本文件為 ERPCore2 單據審核機制（Approval Workflow）的**總綱**，說明設計原則、核心元件與各模組現況摘要。詳細內容請參閱子文件。
> 最後更新：2026-03-02（全 7 模組完整；警告訊息精簡；審核權限整合說明補充）

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
| `ApprovalSettingsTab` | `Components/Pages/Systems/SystemParameter/ApprovalSettingsTab.razor` | 系統參數 UI — 審核開關切換（8 個模組） |
| `BatchApprovalModalComponent` | `Components/Pages/Purchase/BatchApprovalModalComponent.razor` | 通用批次審核 Modal（泛型 `TEntity`） |
| `BatchApprovalTable` | `Components/Pages/Purchase/BatchApprovalTable.razor` | 批次審核 Modal 內部表格 |
| `RejectConfirmModalComponent` | `Components/Pages/Purchase/RejectConfirmModalComponent.razor` | 駁回原因輸入 Modal |
| `GenericEditModalComponent` | `Components/Shared/Modal/GenericEditModalComponent.razor` | 核准/駁回按鈕區塊（`ShowApprovalSection` 系列參數）+ `CanPrintCheck` 列印守衛 |

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
| 報價單 | ✅ | ✅ 完整 | ✅ ApproveAsync | ✅ QuotationIndex | ✅ |
| 採購訂單 | ✅ | ✅ 完整 | ✅ ApproveOrderAsync | ✅ PurchaseOrderIndex | ✅ |
| 進貨單 | ✅ | ✅ 完整 | ✅ ApproveAsync | ✅ | ✅ |
| 進貨退回 | ✅ | ✅ 完整 | ✅ ApproveAsync | ✅ | ✅ |
| 銷售訂單 | ✅ | ✅ 完整 | ✅ ApproveAsync | ✅ | ✅ |
| 銷貨出貨 | ✅ | ✅ 完整 | ✅ ApproveAsync | ✅ | ✅ |
| 銷貨退回 | ✅ | ✅ 完整 | ✅ ApproveAsync | ✅ | ✅ |

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
| `EnableSalesDeliveryApproval` | ✅ | ✅ |

---

## 四、文件導覽

| 文件 | 說明 |
|------|------|
| [README_審核_各模組狀態.md](README_審核_各模組狀態.md) | 各模組詳細現況、本輪完成項目、待辦清單 |
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
     │   UpdateAsync(entity) + SaveXxxDetailsAsync(entity)  ← 先儲存含明細
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

## 六、審核權限整合說明

> 詳見 [Readme_權限設計說明.md](../Readme_權限設計說明.md)

### 6-1 審核權限的位置

每個模組的 `Xxx.Approve` 審核權限集中定義於 `Models/PermissionRegistry.cs`：

```csharp
// 範例（Quotation 模組）
public static class Quotation
{
    public const string Approve = "Quotation.Approve";
    // ...
}
```

所有 7 個模組的 `Approve` 權限均已在 `GetAllPermissions()` 中以 `PermissionLevel.Sensitive` 等級登錄：

| 模組 | 常數 | 等級 |
|------|------|------|
| 報價單 | `PermissionRegistry.Quotation.Approve` | Sensitive |
| 採購訂單 | `PermissionRegistry.PurchaseOrder.Approve` | Sensitive |
| 進貨單 | `PermissionRegistry.PurchaseReceiving.Approve` | Sensitive |
| 進貨退回 | `PermissionRegistry.PurchaseReturn.Approve` | Sensitive |
| 銷售訂單 | `PermissionRegistry.SalesOrder.Approve` | Sensitive |
| 銷貨出貨 | `PermissionRegistry.SalesDelivery.Approve` | Sensitive |
| 銷貨退回 | `PermissionRegistry.SalesReturn.Approve` | Sensitive |

### 6-2 權限如何生效

`GenericEditModalComponent` 使用 `<PermissionCheck>` 包裹核准／駁回按鈕區塊：

```razor
<PermissionCheck Permission="@ApprovalPermission">
    @* 核准、駁回按鈕 *@
</PermissionCheck>
```

`ApprovalPermission` 由各 EditModal 傳入，**必須使用 PermissionRegistry 常數**（非原始字串），以享有編譯期型別檢查並避免拼寫錯誤：

```razor
@* ✅ 正確 *@
ApprovalPermission="@PermissionRegistry.SalesOrder.Approve"

@* ❌ 錯誤（原始字串，無編譯期保護） *@
ApprovalPermission="SalesOrder.Approve"
```

### 6-3 角色授權設定

在系統管理 → 角色管理中，對應角色需開啟 `Xxx.Approve` 權限，該使用者才會看到「核准」與「駁回」按鈕。

沒有 `Approve` 權限的使用者，`<PermissionCheck>` 會直接隱藏按鈕，整個審核操作區塊不可見。

---

## 八、常見問題

### Q: 開啟審核後，歷史資料 IsApproved = false 怎麼辦？

欄位仍可編輯，但轉單/列印需先手動核准。批次核准請使用各模組 Index 的「批次審核」按鈕（報價單、採購訂單、進貨單、進貨退回、銷售訂單、銷貨出貨、銷貨退回均已實作）。

### Q: 核准後發現資料有誤？

按「駁回」→ 輸入原因 → 欄位解鎖 → 修改 → 重新核准。

### Q: 為什麼 SalesOrderEditModal 在 OnInitializedAsync 要先初始化 ModalManager 再 await？

因為 Razor 模板直接存取 `customerModalManager.IsModalVisible`（非 null-conditional），若在 ModalManager 初始化前發生 `await` 掛起，Blazor 中間渲染會 NullReferenceException。規則：有 `= default!` Manager 的 EditModal，`await` 必須在 `.Build()` 之後。詳見 [README_審核_各模組狀態.md](README_審核_各模組狀態.md) 第五節。
