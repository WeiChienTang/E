# 審核機制設計說明（總綱）

> 本文件為 ERPCore2 單據審核機制（Approval Workflow）的**總綱**，說明設計原則、核心元件與各模組現況摘要。詳細內容請參閱子文件。
> 最後更新：2026-03-03（架構重構：審核永遠啟用，改為自動/人工模式）

---

## 一、設計原則

### 1-1 審核永遠啟用，設定控制「方式」

審核功能**永遠開啟**，不可關閉。**SystemParameter** 控制各單據類型的審核方式：

| 模式 | C# 屬性值 | 行為 |
|------|-----------|------|
| **系統自動審核** | `XxxManualApproval = false`（預設） | 儲存時系統自動設 `IsApproved = true`，無需人工操作 |
| **人工審核** | `XxxManualApproval = true` | 需人工按「核准」鍵，欄位在核准後鎖定 |

**設計目的**：消除「開關切換前歷史資料 IsApproved=false」的語意模糊問題。所有單據的 `IsApproved` 永遠有明確意義。

### 1-2 「誰可以審核」由 Role/Permission 控制

SystemParameter 不存放指定審核人員。需要審核權限請在 `PermissionRegistry` 中設定 `Xxx.Approve` 權限。

### 1-3 審核欄位統一命名規範

| 欄位名 | 型別 | 說明 |
|--------|------|------|
| `IsApproved` | `bool` | 是否已審核通過（預設 `false`，儲存後由系統或人工設為 `true`） |
| `ApprovedBy` | `int?` | FK → `Employee.Id`，審核者（自動審核時為儲存者） |
| `ApprovedAt` | `DateTime?` | 審核時間戳 |
| `RejectReason` | `string?` | 駁回原因（人工審核才有意義；核准後應清空） |
| `ApprovedByUser` | `Employee?` | Navigation Property |

---

## 二、核心元件快速參考

| 元件 | 位置 | 說明 |
|------|------|------|
| `ApprovalConfigHelper` | `Helpers/EditModal/ApprovalConfigHelper.cs` | **唯一**審核邏輯入口，不可在元件直接硬寫判斷 |
| `SystemParameter` | `Data/Entities/Systems/SystemParameter.cs` | 各模組審核方式設定（`XxxManualApproval`）|
| `ApprovalSettingsTab` | `Components/Pages/Systems/SystemParameter/ApprovalSettingsTab.razor` | 系統參數 UI — 審核方式切換（8 個模組）|
| `BatchApprovalModalComponent` | `Components/Pages/Purchase/BatchApprovalModalComponent.razor` | 通用批次審核 Modal（泛型 `TEntity`），人工審核模式才顯示 |
| `BatchApprovalTable` | `Components/Pages/Purchase/BatchApprovalTable.razor` | 批次審核 Modal 內部表格 |
| `RejectConfirmModalComponent` | `Components/Pages/Purchase/RejectConfirmModalComponent.razor` | 駁回原因輸入 Modal |
| `GenericEditModalComponent` | `Components/Shared/Modal/GenericEditModalComponent.razor` | 核准/駁回按鈕區塊（`ShowApprovalSection` 系列參數）+ `CanPrintCheck` 列印守衛 |
| `FormSectionNames.ApprovalInfo` | `Helpers/EditModal/FormSectionHelper.cs` | 審核資訊顯示 Section，人工審核模式才顯示 |

### ApprovalConfigHelper 邏輯摘要

```csharp
// 欄位是否唯讀（isManualApproval 取代舊的 isApprovalEnabled）
ShouldLockFieldByApproval(isManualApproval, isApproved, hasUndeletableDetails)

// 轉單/列印等操作是否允許
CanPerformActionRequiringApproval(isManualApproval, isApproved)

// 儲存是否允許（isPreApprovalSave=true 供核准前自動儲存用）
CanSaveWhenApproved(isManualApproval, isApproved, isPreApprovalSave = false)
```

| 情境 | ShouldLockField | CanPerformAction | CanSave |
|------|-----------------|------------------|---------|
| 自動審核，任意狀態 | `false` | `true` | `true` |
| 人工審核，未審核 | `false` | `false` | `true` |
| 人工審核，已核准 | `true` | `true` | `false` |
| 人工審核，已核准，isPreApprovalSave | `true` | `true` | `true` |

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

**SystemParameter 審核方式欄位現況（DB 欄位保留舊名，C# 屬性已更新）**

| C# 屬性 | DB 欄位（原名） | 存在 | ApprovalSettingsTab 可切換 |
|---------|---------------|------|---------------------------|
| `QuotationManualApproval` | `EnableQuotationApproval` | ✅ | ✅ |
| `PurchaseOrderManualApproval` | `EnablePurchaseOrderApproval` | ✅ | ✅ |
| `PurchaseReceivingManualApproval` | `EnablePurchaseReceivingApproval` | ✅ | ✅ |
| `PurchaseReturnManualApproval` | `EnablePurchaseReturnApproval` | ✅ | ✅ |
| `SalesOrderManualApproval` | `EnableSalesOrderApproval` | ✅ | ✅ |
| `SalesReturnManualApproval` | `EnableSalesReturnApproval` | ✅ | ✅ |
| `SalesDeliveryManualApproval` | `EnableSalesDeliveryApproval` | ✅ | ✅ |
| `InventoryTransferManualApproval` | `EnableInventoryTransferApproval` | ✅ | ✅ |

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
     │       ├─ 自動審核 → 允許儲存
     │       ├─ 人工審核 + 未核准 → 允許
     │       └─ 人工審核 + 已核准 → ❌ 顯示警告
     │
     ├─ 儲存成功（自動審核模式）
     │       ↓
     │   !isManualApproval && !entity.IsApproved
     │       ↓
     │   EntityService.ApproveAsync(id, currentUserId)  ← 系統自動核准
     │       ↓
     │   IsApproved = true（對使用者無感）
     │
     ├─ 按「核准」（HandleApproveAsync）— 人工審核模式
     │       ↓
     │   UpdateAsync(entity) + SaveXxxDetailsAsync(entity)  ← 先儲存含明細
     │       ↓
     │   EntityService.ApproveAsync(id, userId)
     │       ↓
     │   欄位變唯讀 / Detail Table 封鎖 / 轉單與列印開放
     │
     ├─ 按「駁回」（HandleRejectWithReasonAsync）— 人工審核模式
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
             ├─ 自動審核 → 執行（IsApproved 已為 true）
             └─ 人工審核 + 未核准 → ❌ 顯示「需先審核通過」
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

**人工審核模式**下：有 `Approve` 權限的使用者可看到核准/駁回按鈕。
**自動審核模式**下：`ShouldShowApprovalSection()` 回傳 `false`，整個審核操作區塊不顯示。

### 6-3 角色授權設定

在系統管理 → 角色管理中，對應角色需開啟 `Xxx.Approve` 權限，該使用者才會看到「核准」與「駁回」按鈕。

---

## 七、審核資訊顯示 Section（EditModal 內）

### 7-1 設計目的

讓編輯人員在單據中直接看到「誰審核、何時審核、是否核准、駁回原因」，無需另開其他頁面查詢。

### 7-2 顯示條件

| 條件 | 是否顯示 ApprovalInfo Section |
|------|-------------------------------|
| 新建單據（EntityId = null） | ❌ 不顯示 |
| 編輯現有單據 + 自動審核模式 | ❌ 不顯示 |
| 編輯現有單據 + 人工審核模式 | ✅ 顯示 |

### 7-3 欄位一覽

| 欄位 | FormFieldType | 說明 |
|------|---------------|------|
| 審核狀態（`IsApproved`） | `Select`（`IsDisabled=true`） | 選項：已核准 / 待審核 |
| 審核者（`ApprovedByUser.FullName`） | `Text`（`IsDisabled=true`） | 使用嵌套屬性路徑，自動解析 navigation property |
| 審核時間（`ApprovedAt`） | `DateTime`（`IsDisabled=true`） | 核准前空白 |
| 駁回原因（`RejectReason`） | `Text`（`IsDisabled=true`） | 無駁回時空白 |

> **重要**：所有欄位均設 `IsDisabled=true`（非單純 `IsReadOnly`），完全無法互動。所有審核狀態異動只能透過工具列的「核准」/「駁回」按鈕執行。

### 7-4 實作位置

```csharp
// 在各 EditModal 的 formSections 建立處（BuildFormSections / InitializeFormFieldsAsync）

formSections = FormSectionHelper<TEntity>.Create()
    .AddToSection(FormSectionNames.BasicInfo, ...)
    // ...
    .AddCustomFieldsIf(
        isManualApproval && XxxId.HasValue && XxxId.Value > 0,  // 人工審核模式才顯示
        FormSectionNames.ApprovalInfo,
        nameof(XxxEntity.IsApproved),
        "ApprovedByUser.FullName",
        nameof(XxxEntity.ApprovedAt),
        nameof(XxxEntity.RejectReason))
    .Build();
```

### 7-5 Resx Keys

| Key | zh-TW | en-US |
|-----|-------|-------|
| `Section.ApprovalInfo` | 審核資訊 | Approval Info |
| `Approval.Status` | 審核狀態 | Approval Status |
| `Approval.ApprovedBy` | 審核者 | Approved By |
| `Approval.ApprovedAt` | 審核時間 | Approved At |

（已於 2026-03-02 加入全 5 語言 resx）

---

## 八、常見問題

### Q: 從舊版（開關式審核）升級後，歷史資料 IsApproved=false 怎麼辦？

舊版中「審核關閉」等同新版「自動審核」（`XxxManualApproval=false`）。歷史資料在新版中：
- 若切換為自動審核：下次儲存時系統自動核准，無需補審
- 若切換為人工審核：需要手動補核准（使用 Index 頁批次審核按鈕）

### Q: 核准後發現資料有誤？

**人工審核模式**：按「駁回」→ 輸入原因 → 欄位解鎖 → 修改 → 重新核准。
**自動審核模式**：直接修改並儲存，系統自動重新核准。

### Q: 批次審核按鈕什麼時候顯示？

只在**人工審核模式**（`isManualApproval=true`）下顯示批次審核按鈕。自動審核模式下按鈕隱藏（無意義）。

### Q: 轉單後建立的新單據會自動審核嗎？

- **自動審核模式**：使用者開啟新單據並儲存後，系統自動核准（Phase 2 可在 service 層補充轉單時直接核准）
- **人工審核模式**：需人工審核後才可執行後續操作

### Q: 為什麼 SalesOrderEditModal 在 OnInitializedAsync 要先初始化 ModalManager 再 await？

因為 Razor 模板直接存取 `customerModalManager.IsModalVisible`（非 null-conditional），若在 ModalManager 初始化前發生 `await` 掛起，Blazor 中間渲染會 NullReferenceException。規則：有 `= default!` Manager 的 EditModal，`await` 必須在 `.Build()` 之後。詳見 [README_審核_各模組狀態.md](README_審核_各模組狀態.md) 第五節。
