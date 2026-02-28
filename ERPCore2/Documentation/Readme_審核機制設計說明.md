# 審核機制設計說明

> 本文件說明 ERPCore2 的單據審核機制（Approval Workflow）架構、現有實作狀態，以及未來新增審核支援時的完整步驟。

---

## 一、設計原則

### 1-1 審核是「全域開關」，而非強制要求

並非每間公司都需要審核機制。因此審核功能以 **SystemParameter** 控制，每種單據類型各有一個開關（布林值）。
- 開關關閉 → 該類型單據不需審核，可自由儲存、轉單
- 開關開啟 → 必須先審核通過，才能執行後續操作（轉單、列印、已核准後無法修改）

### 1-2 「誰可以審核」由 Role/Permission 控制

SystemParameter **不存放指定審核人員**，原因：
- 指定人員有離職、異動的維護問題
- 權限系統已有完整的角色與權限管理
- 未來若需要「指定部門或角色才能審核」，在 Permission 系統擴充即可

### 1-3 審核欄位統一命名規範

所有支援審核的實體，審核相關欄位命名如下：

| 欄位名 | 型別 | 說明 |
|--------|------|------|
| `IsApproved` | `bool` | 是否已審核通過（預設 `false`） |
| `ApprovedBy` | `int?` | FK → `Employee.Id`，審核者 |
| `ApprovedAt` | `DateTime?` | 審核時間戳 |
| `RejectReason` | `string?` | 駁回原因（核准後應清空） |
| `ApprovedByUser` | `Employee?` | Navigation Property |

---

## 二、核心元件說明

### 2-1 SystemParameter — 審核開關

位置：`Data/Entities/Systems/SystemParameter.cs`

| 欄位 | 預設值 | 對應單據 |
|------|--------|---------|
| `EnableQuotationApproval` | `false` | 報價單 |
| `EnablePurchaseOrderApproval` | `false` | 採購訂單 |
| `EnablePurchaseReceivingApproval` | `false` | 進貨單 |
| `EnablePurchaseReturnApproval` | `false` | 進貨退出 |
| `EnableSalesOrderApproval` | `false` | 銷售訂單 |
| `EnableSalesReturnApproval` | `false` | 銷貨退回 |
| `EnableInventoryTransferApproval` | `false` | 庫存調撥 |

> **注意**：`SalesDelivery`（銷貨單）目前實體有審核欄位，但 SystemParameter **尚未加入** `EnableSalesDeliveryApproval` 開關。未來若需要，需補充此欄位。

### 2-2 ApprovalConfigHelper — 審核邏輯統一入口

位置：`Helpers/EditModal/ApprovalConfigHelper.cs`

此靜態類別是所有 EditModal 元件判斷審核行為的**唯一依據**，不可在元件中直接硬寫審核邏輯。

```csharp
// 決定欄位是否應唯讀（審核後鎖定）
bool ShouldLockFieldByApproval(bool isApprovalEnabled, bool isApproved, bool hasUndeletableDetails)

// 決定是否可執行需要審核通過才能做的操作（如轉單）
bool CanPerformActionRequiringApproval(bool isApprovalEnabled, bool isApproved)

// 決定在審核狀態下是否可以儲存
bool CanSaveWhenApproved(bool isApprovalEnabled, bool isApproved, bool isPreApprovalSave = false)

// 取得審核狀態警告訊息（已核准則回傳警告文字）
string? GetApprovalWarningMessage(bool isApprovalEnabled, bool isApproved, string entityName = "單據")
```

**邏輯摘要**：

| 情境 | `ShouldLockField` | `CanPerformAction` | `CanSave` |
|------|------------------|--------------------|----------|
| 審核關閉，無下一步 | `false` | `true` | `true` |
| 審核關閉，有下一步 | `true` | `true` | `true` |
| 審核開啟，未審核 | `false` | `false` | `true` |
| 審核開啟，已核准 | `true` | `true` | `false` |
| 審核開啟，已核准，isPreApprovalSave | `true` | `true` | `true` |

### 2-3 SystemParameterSettingsModal — 系統參數設定入口

位置：`Components/Pages/Systems/SystemParameter/SystemParameterSettingsModal.razor`

審核機制的開關設定需要新增一個 Tab `ApprovalSettingsTab.razor`（**尚未建立，見第四節**）。

---

## 三、各單據審核實作現況

### 3-1 實體審核欄位

| 單據 | 實體檔案 | 審核欄位 | 狀態 |
|------|---------|---------|------|
| 報價單 | `Data/Entities/Sales/Quotation.cs` | `IsApproved`, `ApprovedBy`, `ApprovedAt`, `RejectReason` | ✅ 完整 |
| 採購訂單 | `Data/Entities/Purchase/PurchaseOrder.cs` | `IsApproved`, `ApprovedBy`, `ApprovedAt`, `RejectReason` | ✅ 完整 |
| 銷貨單 | `Data/Entities/Sales/SalesDelivery.cs` | `IsApproved`, `ApprovedBy`, `ApprovedAt`, `RejectReason` | ✅ 完整 |
| 進貨單 | `Data/Entities/Purchase/PurchaseReceiving.cs` | — | ❌ 缺審核欄位 |
| 進貨退出 | `Data/Entities/Purchase/PurchaseReturn.cs` | — | ❌ 缺審核欄位 |
| 銷售訂單 | `Data/Entities/Sales/SalesOrder.cs` | — | ❌ 缺審核欄位 |
| 銷貨退回 | `Data/Entities/Sales/SalesReturn.cs` | — | ❌ 缺審核欄位 |

### 3-2 EditModal 審核 UI

| 單據 | EditModal 元件 | 審核按鈕 | 使用 ApprovalConfigHelper | 狀態 |
|------|--------------|---------|--------------------------|------|
| 報價單 | `QuotationEditModalComponent.razor` | ✅ 核准 / 駁回 | ✅ | ✅ **參考實作** |
| 採購訂單 | `PurchaseOrderEditModal/` | ✅ 核准 / 駁回 | ✅ | ✅ 完整 |
| 銷貨單 | `SalesDeliveryEditModal/` | ✅ 核准 / 駁回 | ✅ | ✅ 完整 |
| 進貨單 | `PurchaseReceivingEditModal/` | ❌ | — | ❌ 待實作 |
| 進貨退出 | `PurchaseReturnEditModal/` | ❌ | — | ❌ 待實作 |
| 銷售訂單 | `SalesOrderEditModal/` | ❌ | — | ❌ 待實作 |
| 銷貨退回 | `SalesReturnEditModal/` | ❌ | — | ❌ 待實作 |

---

## 四、未完成項目與實作步驟

### Step 1：建立 ApprovalSettingsTab.razor（SystemParameter UI）

**目標**：讓使用者可在系統參數設定介面中開啟/關閉各單據的審核機制。

**新增檔案**：`Components/Pages/Systems/SystemParameter/ApprovalSettingsTab.razor`

**參考**：`TaxSettingsTab.razor`（最簡單的 Tab 範例）

此 Tab 使用 `GenericFormComponent`，欄位對應 `SystemParameter` 現有欄位，依模組分群：

```
採購模組
  ☐ 啟用採購訂單審核   (EnablePurchaseOrderApproval)
  ☐ 啟用進貨單審核     (EnablePurchaseReceivingApproval)
  ☐ 啟用進貨退出審核   (EnablePurchaseReturnApproval)

銷售模組
  ☐ 啟用報價單審核     (EnableQuotationApproval)
  ☐ 啟用銷售訂單審核   (EnableSalesOrderApproval)
  ☐ 啟用銷貨退回審核   (EnableSalesReturnApproval)
```

**在 `SystemParameterSettingsModal.razor` 的 `OnInitialized()` 中加入此 Tab**：

```csharp
new() { Label = "審核機制", Icon = "bi bi-check2-circle", SectionNames = new(), CustomContent = CreateApprovalTabContent() },
```

> 此步驟**不需要 Migration**，SystemParameter 欄位已存在。

---

### Step 2：補齊 4 個實體的審核欄位

**目標**：讓 PurchaseReceiving、PurchaseReturn、SalesOrder、SalesReturn 支援審核欄位。

**每個實體加入以下欄位**：

```csharp
[Display(Name = "核准人員")]
[ForeignKey(nameof(ApprovedByUser))]
public int? ApprovedBy { get; set; }

[Display(Name = "核准時間")]
public DateTime? ApprovedAt { get; set; }

[Display(Name = "是否核准")]
public bool IsApproved { get; set; } = false;

[MaxLength(200, ErrorMessage = "駁回原因不可超過200個字元")]
[Display(Name = "駁回原因")]
public string? RejectReason { get; set; }

// Navigation Property
public Employee? ApprovedByUser { get; set; }
```

**需要修改的實體**：
- `Data/Entities/Purchase/PurchaseReceiving.cs`
- `Data/Entities/Purchase/PurchaseReturn.cs`
- `Data/Entities/Sales/SalesOrder.cs`
- `Data/Entities/Sales/SalesReturn.cs`

**需要新增 Migration**：

```bash
dotnet ef migrations add AddApprovalFieldsToRemainingEntities
dotnet ef database update
```

**同時確認 AppDbContext** 中對應的 `Include(x => x.ApprovedByUser)` 已加入到各查詢中（參考 PurchaseOrder 或 Quotation 的 Service 實作）。

---

### Step 3：各 EditModal 加入審核按鈕

**目標**：4 個 EditModal 元件加入「核准」/「駁回」按鈕，邏輯參照已完成的 `QuotationEditModalComponent.razor`。

#### 參考實作：QuotationEditModalComponent.razor

**1. 讀取 SystemParameter 審核開關**（在 `OnParametersSetAsync` 或 `InitializeFormFieldsAsync`）：

```csharp
var systemParameter = await SystemParameterService.GetSystemParameterAsync();
isApprovalEnabled = systemParameter?.EnableQuotationApproval ?? false;
```

**2. 在 Header 加入審核按鈕**（`<HeaderButtons>` 區塊）：

```razor
@if (isApprovalEnabled && entity.Id > 0)
{
    if (!entity.IsApproved)
    {
        <GenericButtonComponent Variant="ButtonVariant.Green"
                               Text="核准"
                               OnClick="HandleApproveAsync"
                               IsDisabled="@(isSaving || isLoading)"
                               Title="核准此單據" />
        <GenericButtonComponent Variant="ButtonVariant.Red"
                               Text="駁回"
                               OnClick="HandleRejectAsync"
                               IsDisabled="@(isSaving || isLoading)"
                               Title="駁回此單據" />
    }
    else
    {
        <span class="badge bg-success align-self-center">已核准</span>
    }
}
```

**3. 審核狀態控制欄位唯讀**（在欄位定義中）：

```csharp
// 使用 ApprovalConfigHelper 決定是否鎖定
bool shouldLock = ApprovalConfigHelper.ShouldLockFieldByApproval(
    isApprovalEnabled, entity.IsApproved, hasUndeletableDetails);
```

**4. 核准方法**：

```csharp
private async Task HandleApproveAsync()
{
    if (entity.IsApproved)
    {
        await NotificationService.ShowWarningAsync("此單據已經審核通過");
        return;
    }

    // 審核前先自動儲存（isPreApprovalSave = true）
    var saveResult = await HandleSaveAsync(isPreApprovalSave: true);
    if (!saveResult) return;

    entity.IsApproved = true;
    entity.ApprovedBy = currentUserId;
    entity.ApprovedAt = DateTime.Now;
    entity.RejectReason = null;

    var result = await EntityService.UpdateAsync(entity);
    if (result.IsSuccess)
        await NotificationService.ShowSuccessAsync("審核通過");
    else
        await NotificationService.ShowErrorAsync("審核失敗：" + result.ErrorMessage);
}
```

**5. 駁回方法**（需要 `GenericConfirmModalComponent` 或輸入原因的 Dialog）：

```csharp
private async Task HandleRejectAsync()
{
    // 顯示輸入駁回原因的確認框
    // 駁回後：IsApproved = false, ApprovedBy = currentUserId, RejectReason = 原因
    entity.IsApproved = false;
    entity.ApprovedBy = currentUserId;
    entity.ApprovedAt = DateTime.Now;
    entity.RejectReason = rejectReason;

    var result = await EntityService.UpdateAsync(entity);
    if (result.IsSuccess)
        await NotificationService.ShowWarningAsync("已駁回");
}
```

**4 個 EditModal 對應的開關欄位**：

| EditModal | SystemParameter 開關欄位 |
|-----------|------------------------|
| `PurchaseReceivingEditModal` | `EnablePurchaseReceivingApproval` |
| `PurchaseReturnEditModal` | `EnablePurchaseReturnApproval` |
| `SalesOrderEditModal` | `EnableSalesOrderApproval` |
| `SalesReturnEditModal` | `EnableSalesReturnApproval` |

---

## 五、審核流程全貌

```
使用者編輯單據
     │
     ├─ 按「儲存」
     │       ↓
     │   ApprovalConfigHelper.CanSaveWhenApproved()
     │       ├─ 審核關閉 → 允許儲存
     │       ├─ 審核開啟 + 未核准 → 允許儲存
     │       └─ 審核開啟 + 已核准 → ❌ 不允許（顯示警告）
     │
     ├─ 按「核准」
     │       ↓
     │   自動先儲存（isPreApprovalSave = true）
     │       ↓
     │   entity.IsApproved = true
     │   entity.ApprovedBy = 目前使用者 ID
     │   entity.ApprovedAt = DateTime.Now
     │   entity.RejectReason = null
     │       ↓
     │   呼叫 UpdateAsync → 儲存至 DB
     │       ↓
     │   欄位全部變唯讀（ShouldLockFieldByApproval = true）
     │
     ├─ 按「駁回」
     │       ↓
     │   entity.IsApproved = false
     │   entity.ApprovedBy = 目前使用者 ID（記錄是誰駁回的）
     │   entity.RejectReason = 輸入的原因
     │       ↓
     │   呼叫 UpdateAsync → 儲存至 DB
     │       ↓
     │   欄位恢復可編輯（IsApproved = false）
     │
     └─ 執行「需審核後才能做的操作」（如轉單）
             ↓
         ApprovalConfigHelper.CanPerformActionRequiringApproval()
             ├─ 審核關閉 → 直接執行
             └─ 審核開啟 + 未核准 → ❌ 顯示「需先審核通過」
```

---

## 六、常見問題

### Q: 開啟審核開關後，歷史資料（IsApproved = false）會有什麼影響？

歷史資料的 `IsApproved` 預設為 `false`。當開關開啟後：
- 歷史單據會顯示為「未審核」狀態
- 欄位仍可編輯（因為 `IsApproved = false`）
- 若要執行轉單等操作，需先手動核准

建議：若要對既有資料批次核准，可由系統管理員操作批次核准功能（尚未實作）。

### Q: 核准後發現資料有誤，如何修改？

只能透過「駁回」來解除核准鎖定：
1. 按「駁回」→ 輸入駁回原因 → 欄位解鎖
2. 修改資料後 → 重新核准

### Q: 審核人員資訊 ApprovedBy 存的是什麼？

存的是 `Employee.Id`，Navigation Property 為 `ApprovedByUser`。
若要顯示審核人姓名，使用 `entity.ApprovedByUser?.Name`。

### Q: SalesDelivery（銷貨單）已有審核欄位，但 SystemParameter 沒有對應開關？

目前 `SalesDelivery` 實體有審核欄位，`SalesDeliveryEditModal` 也有審核按鈕，但 `SystemParameter` 沒有 `EnableSalesDeliveryApproval` 欄位。
目前的行為：**SalesDelivery 審核永遠啟用**（硬寫為 true）。
若要改為可設定，需：
1. `SystemParameter` 加 `EnableSalesDeliveryApproval` 欄位
2. Migration
3. `ApprovalSettingsTab` 加對應 checkbox
4. `SalesDeliveryEditModal` 改為讀取 SystemParameter 開關

---

## 七、相關檔案索引

| 功能 | 檔案路徑 |
|------|---------|
| 審核開關（DB 欄位） | `Data/Entities/Systems/SystemParameter.cs` |
| 審核邏輯統一入口 | `Helpers/EditModal/ApprovalConfigHelper.cs` |
| 系統參數設定 Modal | `Components/Pages/Systems/SystemParameter/SystemParameterSettingsModal.razor` |
| 審核設定 Tab（待建立） | `Components/Pages/Systems/SystemParameter/ApprovalSettingsTab.razor` |
| 參考實作：報價單 | `Components/Pages/Sales/QuotationEditModalComponent.razor` |
| 參考實作：採購訂單 | `Components/Pages/Purchase/PurchaseOrderEditModal/` |
| 參考實作：銷貨單 | `Components/Pages/Sales/SalesDeliveryEditModal/` |
| 待補：進貨單 | `Components/Pages/Purchase/PurchaseReceivingEditModal/` |
| 待補：進貨退出 | `Components/Pages/Purchase/PurchaseReturnEditModal/` |
| 待補：銷售訂單 | `Components/Pages/Sales/SalesOrderEditModal/` |
| 待補：銷貨退回 | `Components/Pages/Sales/SalesReturnEditModal/` |
| 採購訂單實體（已有審核欄位） | `Data/Entities/Purchase/PurchaseOrder.cs` |
| 報價單實體（已有審核欄位） | `Data/Entities/Sales/Quotation.cs` |
| 銷貨單實體（已有審核欄位） | `Data/Entities/Sales/SalesDelivery.cs` |
| 進貨單實體（待補欄位） | `Data/Entities/Purchase/PurchaseReceiving.cs` |
| 進貨退出實體（待補欄位） | `Data/Entities/Purchase/PurchaseReturn.cs` |
| 銷售訂單實體（待補欄位） | `Data/Entities/Sales/SalesOrder.cs` |
| 銷貨退回實體（待補欄位） | `Data/Entities/Sales/SalesReturn.cs` |
