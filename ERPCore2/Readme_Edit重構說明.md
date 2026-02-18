# Edit Modal 重構說明

## 現況分析

專案中有 **42 個 EditModalComponent**，全部包裝 `GenericEditModalComponent`。經比對多個代表性檔案（進銷存單據 + 簡單主檔），發現大量重複模式。

### 已抽出的 Helper（/Helpers/EditModal/）

| Helper | 用途 | 狀態 |
|---|---|---|
| ActionButtonHelper | ActionButton 產生與更新 | 已完成 |
| ApprovalConfigHelper | 審核鎖定判斷 | 已完成 |
| AutoCompleteConfigHelper | AutoComplete 配置建置 | 已完成 |
| ChildDocumentRefreshHelper | 子單據儲存後刷新 | 已完成 |
| CustomModuleHelper | 自訂模組建立（null 保護 + 樣板簡化） | 已完成 |
| DocumentConversionHelper | 轉單流程 | 已完成 |
| EditDataLoaderHelper | DataLoader 共用載入/建立邏輯 | 已完成 |
| EditModalMessages | 共用警告訊息常數 | 已完成 |
| EntityCodeGenerationHelper | 單號產生 | 已完成 |
| FormFieldLockHelper | 欄位鎖定/解鎖 | 已完成 |
| FormSectionHelper | 表單分組 | 已完成 |
| ModalManagerInitHelper | Modal 管理器初始化 | 已完成 |
| PrefilledValueHelper | 預填值設定 | 已完成 |
| TaxCalculationHelper | 稅率載入 + 依稅別計算 | 已擴充 |

---

## 已完成的重構

### P1：稅額計算 Switch 區塊 → 擴充 TaxCalculationHelper ✅

**問題**：4 個以上的 Edit 都有幾乎一模一樣的 `switch(taxMethod)` 區塊（約 40-50 行），處理 TaxExclusive / TaxInclusive / NoTax 三種算法。

**解法**：在 `TaxCalculationHelper` 新增泛型方法 `CalculateFromDetails<TDetail>()`，接受明細列表、稅別、預設稅率和取值委派，回傳 `(decimal TotalAmount, decimal TaxAmount)` tuple。

**已套用**（7 個 Edit）：
- `PurchaseOrderEditModalComponent` — `HandleDetailsChanged` + 新增模式初始化（CalculateFromDetails）
- `PurchaseReceivingEditModalComponent` — `HandleReceivingDetailsChanged`（CalculateFromDetails）
- `PurchaseReturnEditModalComponent` — `HandleReturnDetailsChanged`（CalculateFromDetails）
- `SalesReturnEditModalComponent` — `HandleReturnDetailsChanged`（CalculateFromDetails）
- `QuotationEditModalComponent` — `HandleQuotationDetailsChanged`（CalculateFromDetailsWithDiscount）
- `SalesOrderEditModalComponent` — `HandleDetailsChanged`（CalculateFromDetailsWithDiscount）
- `SalesDeliveryEditModalComponent` — `HandleDeliveryDetailsChanged`（CalculateFromDetailsWithDiscount）

**新增方法**：`CalculateFromDetailsWithDiscount<TDetail>()` — 在 `CalculateFromDetails` 基礎上額外計算折扣前金額（GrossAmount）和折扣金額（DiscountAmount），適用於有折扣欄位的銷貨類單據。

**重構後寫法**（無折扣）：
```csharp
var (total, tax) = TaxCalculationHelper.CalculateFromDetails(
    purchaseReturnDetails,
    editModalComponent.Entity.TaxCalculationMethod,
    currentTaxRate,
    d => d.ReturnSubtotalAmount,
    d => d.TaxRate);
editModalComponent.Entity.TotalReturnAmount = total;
editModalComponent.Entity.ReturnTaxAmount = tax;
```

**重構後寫法**（含折扣）：
```csharp
var (net, tax, gross, discount) = TaxCalculationHelper.CalculateFromDetailsWithDiscount(
    salesOrderDetails,
    editModalComponent.Entity.TaxCalculationMethod,
    currentTaxRate,
    d => d.SubtotalAmount,
    d => d.TaxRate,
    d => d.OrderQuantity * d.UnitPrice);
editModalComponent.Entity.TotalAmount = net;
editModalComponent.Entity.SalesTaxAmount = tax;
editModalComponent.Entity.DiscountAmount = discount;
```

**實際刪除行數**：5 個 Edit × ~60-100 行 ≈ **~380 行**

---

### P4：UpdateFieldsReadOnlyState 簡化 ✅

**問題**：每個 Edit 的 `UpdateFieldsReadOnlyState` 分別呼叫 `LockMultipleFieldsSimple` 和 `LockField`（if/else 處理 ActionButtons），但 `FormFieldLockHelper.LockMultipleFields` 已支援 `actionButtonsMap` 參數。

**解法**：不需新建 Helper，改用現有 `LockMultipleFields` 一次處理所有欄位（含 ActionButtons）。

**已套用**（7 個 Edit）：
- PurchaseOrderEdit、PurchaseReceivingEdit（第一階段）
- QuotationEdit、PurchaseReturnEdit、SalesOrderEdit、SalesDeliveryEdit、SalesReturnEdit（第二階段）

**重構後寫法**：
```csharp
FormFieldLockHelper.LockMultipleFields(
    formFields,
    new[] { "CompanyId", "Code", "OrderDate", ..., "SupplierId" },
    isLocked: hasUndeletableDetails,
    actionButtonsMap: new Dictionary<string, Func<Task<List<FieldActionButton>>>>
    {
        { nameof(PurchaseOrder.SupplierId), GetSupplierActionButtonsAsync }
    }
);
```

**備註**：SalesOrder 原本使用 `InitializeFormFieldsAsync()` 重建整個 formFields 的方式，已統一改為 `LockMultipleFields` 模式，並移除 ActionButton getter 中的 `hasUndeletableDetails` 內部檢查（由 `LockMultipleFields` 統一管理）。

---

### P5：GetCustomModules 樣板 → CustomModuleHelper ✅

**問題**：每個 Edit 都寫相同的 null 檢查 + CustomModule 物件建立（約 15-20 行）。

**解法**：新增 `CustomModuleHelper.CreateSingle<TEntity, TService>()`，封裝 null 保護和物件建立。

**已套用**（7 個 Edit）：
- PurchaseOrderEdit、PurchaseReceivingEdit（第一階段）
- QuotationEdit、PurchaseReturnEdit、SalesOrderEdit、SalesDeliveryEdit、SalesReturnEdit（第二階段）

**重構後寫法**：
```csharp
private List<...CustomModule> GetCustomModules()
{
    return CustomModuleHelper.CreateSingle(editModalComponent, CreateProductManagerContent());
}
```

---

### P7：警告訊息常數化 ✅

**問題**：「因部分明細有其他動作，為保護資料完整性主檔欄位已設唯讀。」等文字在多個 Edit 中硬編碼，且各 Edit 用詞略有不同（沖款記錄、退貨記錄等）。

**解法**：新增 `EditModalMessages` 靜態類別，集中定義共用訊息常數，統一用詞。

**已定義常數**：
- `UndeletableDetailsWarning` — 明細有其他動作的鎖定警告（統一用語，涵蓋沖款、退貨、轉單等情境）
- `PurchaseOrderApprovedWarning` — 採購單審核通過警告
- `SalesOrderApprovedWarning` — 銷貨單審核通過警告

**已套用**（7 個 Edit）：
- PurchaseOrderEdit、PurchaseReceivingEdit（第一階段）
- QuotationEdit、SalesOrderEdit、SalesReturnEdit、PurchaseReturnEdit、SalesDeliveryEdit（第二階段）

---

### P10：命名規範統一 ✅

**已統一**：
- `WarningMessage`（屬性）→ `GetWarningMessage()`（方法）
- `CustomActionButtons`（屬性）→ `GetCustomActionButtons()`（方法）
- Razor 模板參考也一併更新

**已套用**（7 個 Edit）：
- PurchaseOrderEdit、PurchaseReceivingEdit（第一階段）
- QuotationEdit（兩者）、SalesOrderEdit（WarningMessage）、SalesDeliveryEdit（CustomActionButtons）（第二階段）
- PurchaseReturnEdit、SalesReturnEdit 本身已是 `GetXxx()` 命名，無需修改

**規範**：RenderFragment 回傳統一使用方法 `GetXxx()` 風格。

---

### P11：HandleCancel + CloseModal 冗餘消除 ✅

**問題**：幾乎所有 Edit 都有相同的 `HandleCancel` + `CloseModal` 方法（約 15-20 行 × 38 個）。GenericEditModalComponent 內部已處理 `OnCancel.InvokeAsync()` + `CloseModal()`，子 Edit 包裝後造成重複呼叫。

**解法**：所有 Edit 改為 `OnCancel="@OnCancel"` 直接傳遞，移除 `HandleCancel` 和 `CloseModal` 方法。

**已套用**：37 個 Edit（ProductionScheduleEdit 因有額外清除邏輯保留簡化版 HandleCancel）

**實際刪除行數**：**~850 行**

---

### P12：HandleSaveSuccess → OnEntitySaved EventCallback ✅

**問題**：所有 Edit 的 `HandleSaveSuccess` 核心邏輯相同（null 檢查 + InvokeAsync 轉發），唯一差異是 EventCallback 名稱。

**解法**：在 GenericEditModalComponent 新增 `[Parameter] public EventCallback<TEntity> OnEntitySaved`，Generic 在 `HandleSave` 成功後自動觸發。子 Edit 直接綁定：

```razor
@* 重構前 *@
<GenericEditModalComponent OnSaveSuccess="@HandleSaveSuccess" ... />

@* 重構後 *@
<GenericEditModalComponent OnEntitySaved="@OnDepartmentSaved" ... />
```

**已套用**：36 個 Edit（含 DepartmentEdit 手動示範 + 35 個批次處理）

**例外**：CustomerEdit 因 HandleSaveSuccess 有額外業務邏輯（SavePendingVehicleChangesAsync），保留 `OnSaveSuccess` 自訂處理。

**實際刪除行數**：**~600+ 行**

---

### P13：HandleEntityLoaded 預設行為 ✅

**問題**：大部分純主檔 Edit 的 `HandleEntityLoaded` 都是空殼（僅 `StateHasChanged()` + `Task.CompletedTask`）。

**解法**：GenericEditModalComponent 的 `OnEntityLoaded` 未提供時已有預設行為（內部自動 `StateHasChanged()`）。簡單 Edit 移除此方法和參數綁定。

**已套用**：18 個 Edit（DepartmentEdit 手動示範 + 3 個初次批次 + 14 個 P14 連帶移除）

**備註**：其他 Edit 的 HandleEntityLoaded 含有實際業務邏輯（載入明細、車輛、子單據等），保留不動。

**實際刪除行數**：**~180 行**

---

### P14：LoadEntityData 新增模式樣板 → EditDataLoaderHelper ✅

**問題**：所有簡單 Edit 的 DataLoader 方法都有相同結構（30-40 行）：新增模式產生編號 + 設預設狀態 + 預填值，編輯模式 GetByIdAsync，外加 try/catch 錯誤處理。

**解法**：新增 `EditDataLoaderHelper.LoadOrCreateAsync<TEntity, TService>()`，封裝完整的 load-or-create 流程。支援 `Action<TEntity>` 額外初始化回呼。

**已套用**（16 個 Edit）：
- DepartmentEdit（手動示範）
- BankEdit、CurrencyEdit、PaymentMethodEdit
- SizeEdit、UnitEdit、ProductCategoryEdit
- EmployeePositionEdit、RoleEdit
- CompositionCategoryEdit、VehicleTypeEdit
- WarehouseEdit、WarehouseLocationEdit
- PaperSettingEdit、CompanyEdit、PrinterConfigurationEdit

**重構後寫法**：
```csharp
private Task<Bank?> LoadBankData()
    => EditDataLoaderHelper.LoadOrCreateAsync<Bank, IBankService>(
        BankId, BankService, NotificationService, "銀行", "BANK", PrefilledValues,
        b => b.BankName = string.Empty);
```

**例外**：複雜 Edit（PurchaseOrder、SalesOrder 等）使用 DbContext 策略產生編號或有大量額外邏輯，不適用此 Helper。

**實際刪除行數**：每個 Edit 約 30-40 行 × 16 個 ≈ **~500 行**

---

### P15：ShowAddModal / ShowEditModal 樣板 ✅

**問題**：部分 Edit 有重複的 ShowAddModal + ShowEditModal 公開方法（各 ~7 行），邏輯相同：設定 Id + 開啟 Modal。

**解法**：在 GenericEditModalComponent 新增 `ShowAddModal()` 和 `ShowEditModal(int entityId)` 方法，子 Edit 直接委派：

```csharp
public Task ShowAddModal() => editModalComponent!.ShowAddModal();
public Task ShowEditModal(int id) => editModalComponent!.ShowEditModal(id);
```

**已套用**（15 個 Edit）：DepartmentEdit（手動示範）+ 14 個批次處理

**實際刪除行數**：每個 Edit 約 14 行 × 15 個 ≈ **~210 行**

---

### P2：HandleDetailsChanged 整體模式 → 已由 P1 擴充涵蓋 ✅

**問題**：每個 Edit 的 HandleDetailsChanged 方法結構相同：
1. 更新本地明細列表
2. 檢查 editModalComponent?.Entity
3. 呼叫稅額計算（已用 TaxCalculationHelper.CalculateFromDetails 簡化）
4. （部分）呼叫 StateHasChanged

**結果**：P1 擴充完成後，所有 7 個複雜 Edit 的 HandleDetailsChanged 已從 ~60-100 行縮減至 ~15-20 行。剩餘的樣板碼（更新 list + null 檢查 + try/catch）量不大，進一步提取的價值低。此項視為已完成。

---

### P3：OnFieldValueChanged 中的 TaxCalculationMethod 分支 → 移入 GenericEditModalComponent

**方案**：在 GenericEditModalComponent 新增 `[Parameter] public Func<Task>? OnTaxMethodChanged` 參數，自動偵測 TaxCalculationMethod 變更並呼叫。

---

### P6：RenderFragment 三態模式 → DetailSectionWrapper 組件 ✅

**問題**：每個有明細管理器的 Edit 都有相同的三態/五態判斷模式。

**解法**：建立 `Components/Shared/UI/Section/DetailSectionWrapper.razor` Blazor 組件，封裝五態邏輯：
1. `!IsEntityReady` → Spinner / Text / Silent（LoadingVariant 枚舉）
2. `!IsDetailDataReady` → 小 spinner + 文字
3. `!IsDataAvailable` → alert-warning
4. `!IsPreconditionMet` → alert-info
5. 正常 → `@ChildContent`

**已轉換 10 個檔案、11 個方法**：

| 檔案 | 方法 | 特殊處理 |
|---|---|---|
| QuotationEdit | CreateQuotationDetailManagerContent | Silent + 商品檢查 + CustomerId |
| ProductCompositionEdit | CreateDetailManagerContent | isDetailDataReady + 雙資料檢查 |
| SalesOrderEdit | CreateProductManagerContent | isDetailDataReady + 雙資料檢查 |
| SalesReturnEdit | CreateReturnDetailManagerContent | isDetailDataReady + 商品&倉庫檢查 |
| SalesDeliveryEdit | CreateDeliveryDetailManagerContent | Text variant |
| PurchaseReturnEdit | CreateReturnDetailManagerContent | isDetailDataReady + SupplierId |
| PurchaseReceivingEdit | CreateReceivingDetailManagerContent | Text variant + isDetailDataReady |
| InventoryStockEdit | CreateStockDetailContent | isDetailDataReady + 倉庫檢查 |
| SetoffDocumentEdit | CreateSetoffProductDetailManagerContent | Silent + 動態訊息 |
| InventoryTransactionEdit | CreateDetailTableContent | currentTransaction 非 Entity |

**跳過不轉換**：
- StockTakingEdit — 僅 20 行，含業務邏輯（readOnly 狀態計算）
- MaterialIssueEdit — 僅 10 行，極簡單
- SetoffDocument 的 Payment/Prepayment — 僅 24 行，靜默模式不適合 wrapper
- PurchaseOrderEdit — 使用 `display:none` CSS 隱藏優化
- Customer/Employee/SupplierEdit — 車輛 Tab 無狀態檢查
- TextMessageTemplateEdit、ProductionScheduleEdit、WarehouseEdit — 不同內容/佈局

**預估節省 ~400 行**

---

### P8：轉單前驗證 → DocumentConversionValidator

**問題**：各轉單方法有相同的驗證步驟（檢查儲存狀態、Entity、SupplierId、明細數量等）。

---

## 重構進度總覽

### 已完成（第一階段 + 第二階段）

| 項目 | 對應 | 已套用 Edit 數量 |
|---|---|---|
| P1 | TaxCalculationHelper.CalculateFromDetails / CalculateFromDetailsWithDiscount | 7 個（全部主要進銷存單據） |
| P4 | FormFieldLockHelper.LockMultipleFields | 7 個（全部主要進銷存單據） |
| P5 | CustomModuleHelper.CreateSingle | 7 個（全部主要進銷存單據） |
| P7 | EditModalMessages 常數 | 7 個（全部主要進銷存單據） |
| P10 | 命名規範（GetXxx 方法） | 7 個（全部主要進銷存單據） |

### 已套用的 Edit 清單

| Edit | P1 | P4 | P5 | P7 | P10 |
|---|---|---|---|---|---|
| PurchaseOrderEdit | ✅ | ✅ | ✅ | ✅ | ✅ |
| PurchaseReceivingEdit | ✅ | ✅ | ✅ | ✅ | ✅ |
| QuotationEdit | ✅ | ✅ | ✅ | ✅ | ✅ |
| PurchaseReturnEdit | ✅ | ✅ | ✅ | ✅ | ✅（原已符合） |
| SalesOrderEdit | ✅ | ✅ | ✅ | ✅ | ✅ |
| SalesDeliveryEdit | ✅ | ✅ | ✅ | ✅ | ✅ |
| SalesReturnEdit | ✅ | ✅ | ✅ | ✅ | ✅（原已符合） |

### 已完成（第三階段）

| 項目 | 說明 | 已套用 Edit 數 | 刪除行數 |
|---|---|---|---|
| P11 | HandleCancel + CloseModal 冗餘消除 | 37 個 | ~850 行 |
| P12 | OnEntitySaved EventCallback（改 Generic） | 36 個 | ~600+ 行 |
| P13 | HandleEntityLoaded 預設行為（改 Generic） | 18 個 | ~180 行 |
| P14 | EditDataLoaderHelper 新增模式樣板 | 16 個 | ~500 行 |
| P15 | ShowAddModal/ShowEditModal 委派 Generic | 15 個 | ~210 行 |

### 已完成（第四階段）

| 項目 | 說明 | 已套用 Edit 數 | 節省行數 |
|---|---|---|---|
| P6 | DetailSectionWrapper 組件（五態判斷封裝） | 10 個 | ~400 行 |
| P18 | OnXxxSavedWrapper 消除（RelatedEntityModalManager.OnSavedAsync） | 14 個 | ~160 行 |

### 未來（需較大改動 GenericEditModalComponent）

| 項目 | 說明 |
|---|---|
| P3 | OnTaxMethodChanged 參數 |
| P8 | DocumentConversionValidator |

---

## 全面模式分析報告（2026-02-18）

以下是對所有 43 個 EditModalComponent 的全面模式分析，按三個複雜度層級分類，識別可提取為 Helper 的重複模式。

### Edit 複雜度分類

| 層級 | 特徵 | 數量 | 代表範例 |
|---|---|---|---|
| **簡單型** | 無 Table、無 ModalManager、UseGenericSave | 15 個 | DepartmentEdit, BankEdit, CurrencyEdit |
| **中等型** | 有 ModalManager + ActionButton，可能有 CustomModule | 11 個 | EmployeeEdit, SupplierEdit, VehicleEdit |
| **複雜型** | ModalManager + 嵌入 Table + SaveHandler + 稅額計算 | 17 個 | PurchaseOrderEdit, SalesOrderEdit, InventoryStockEdit |

#### 簡單型（15 個）
UnitEdit, SizeEdit, ProductCategoryEdit, BankEdit, CurrencyEdit, PaymentMethodEdit,
RoleEdit, EmployeePositionEdit, CompositionCategoryEdit, PaperSettingEdit,
PrinterConfigurationEdit, ReportPrintConfigurationEdit, WarehouseLocationEdit,
VehicleTypeEdit, SalesReturnReasonEdit

#### 中等型（11 個）
CustomerEdit, EmployeeEdit, SupplierEdit, ProductEdit, VehicleEdit,
VehicleMaintenanceEdit, CompanyEdit, WarehouseEdit, PermissionEdit,
TextMessageTemplateEdit, InventoryTransactionEdit

#### 複雜型（17 個）
PurchaseOrderEdit, PurchaseReceivingEdit, PurchaseReturnEdit,
QuotationEdit, SalesOrderEdit, SalesDeliveryEdit, SalesReturnEdit,
ProductCompositionEdit, InventoryStockEdit, StockTakingEdit,
MaterialIssueEdit, SetoffDocumentEdit, ProductionScheduleEdit,
ProductionScheduleItemEdit, QuotationCompositionEdit

---

### 模式 1：CreateXxxContent RenderFragment「三態判斷」

**影響範圍**：24 個 RenderFragment 方法，跨 20 個 Edit 檔案

**共通結構**（每個方法約 30-65 行）：
```
State 1: Entity == null         → Spinner（載入中）
State 2: 資料未就緒/條件不足    → alert-info 提示訊息（如「請先選擇廠商」）
State 3: 正常                   → 渲染 Table 組件
State 4: catch                  → alert-warning/danger 錯誤訊息
```

**所有實例**：

| 檔案 | 方法名稱 | State 2 條件 |
|---|---|---|
| PurchaseOrderEdit | CreateProductManagerContent | SupplierId <= 0 → 請先選擇廠商 |
| SalesOrderEdit | CreateProductManagerContent | CustomerId <= 0 → 請先選擇客戶 |
| SalesDeliveryEdit | CreateDeliveryDetailManagerContent | CustomerId <= 0 → 請先選擇客戶 |
| SalesReturnEdit | CreateReturnDetailManagerContent | CustomerId <= 0 → 請先選擇客戶 |
| PurchaseReturnEdit | CreateReturnDetailManagerContent | SupplierId <= 0 → 請先選擇廠商 |
| PurchaseReceivingEdit | CreateReceivingDetailManagerContent | SupplierId <= 0 → 請先選擇廠商 |
| QuotationEdit | CreateQuotationDetailManagerContent | CustomerId <= 0 → 請先選擇客戶 |
| InventoryStockEdit | CreateStockDetailContent | ProductId <= 0 → 請先選擇商品 |
| StockTakingEdit | CreateDetailTableContent | （無前置條件） |
| MaterialIssueEdit | CreateDetailManagerContent | （無前置條件） |
| ProductCompositionEdit | CreateDetailManagerContent | ProductId <= 0 → 請先選擇商品 |
| ProductionScheduleEdit | CreateScheduleItemsContent | ProductCompositionId <= 0 → 請先選擇成品 |
| InventoryTransactionEdit | CreateDetailTableContent | ProductId <= 0 → 請先選擇商品 |
| SetoffDocumentEdit | CreateSetoffProductDetailManagerContent | CustomerId/SupplierId 條件 |
| SetoffDocumentEdit | CreateSetoffPaymentDetailManagerContent | 同上 |
| SetoffDocumentEdit | CreateSetoffPrepaymentDetailManagerContent | 同上 |
| WarehouseEdit | CreateWarehouseLocationTableContent | （無前置條件） |
| CustomerEdit | CreateVehicleTabContent | （Tab 型，無前置條件） |
| EmployeeEdit | CreateVehicleTabContent | （Tab 型，無前置條件） |
| SupplierEdit | CreateVehicleTabContent | （Tab 型，無前置條件） |
| TextMessageTemplateEdit | CreateHeaderTextContent / CreateDetailFormatContent / CreateFooterTextContent / CreatePreviewContent | （特殊型） |

**可提取性評估**：⭐⭐⭐ 高

**建議方案**：由於 RenderFragment 本身包含 Razor 語法，無法直接用純 C# Helper 封裝。但可以**將外層三態邏輯封裝成 Blazor 組件**或**提供 Builder 模式產生 RenderFragment**。

**候選方案 A — DetailSectionWrapper 組件**（推薦）：
```razor
<DetailSectionWrapper Entity="@editModalComponent?.Entity"
                      IsDataReady="@(availableProducts != null && availableProducts.Any())"
                      PreconditionMet="@(editModalComponent?.Entity?.SupplierId > 0)"
                      PreconditionMessage="請先選擇廠商後再進行採購明細管理"
                      NoDataMessage="無可用的商品資料，請聯繫系統管理員"
                      ErrorMessage="載入明細管理器時發生錯誤">
    <PurchaseOrderTable ... />
</DetailSectionWrapper>
```

**預估效果**：每個方法從 ~40-65 行 → ~10-15 行（節省 ~600-1000 行）

---

### 模式 2：OnFieldValueChanged — ActionButton 更新

**影響範圍**：21 個 Edit 檔案，共 39 個 OnXxxSavedWrapper 方法

**共通結構**：
```csharp
private async Task OnFieldValueChanged((string PropertyName, object? Value) fieldChange)
{
    try
    {
        if (fieldChange.PropertyName == nameof(Entity.XxxId))
        {
            await ActionButtonHelper.UpdateFieldActionButtonsAsync(
                xxxModalManager, formFields, fieldChange.PropertyName, fieldChange.Value);
        }
        // ... 其他欄位的 if 判斷
    }
    catch (Exception) { await NotificationService.ShowErrorAsync("欄位變更處理時發生錯誤"); }
}
```

**變體分類**：

| 類型 | 說明 | 檔案數 |
|---|---|---|
| **純 ActionButton** | 只有 ActionButtonHelper 呼叫 | 8 個（Employee, Supplier, Customer 等） |
| **ActionButton + TaxMethod** | 額外處理 TaxCalculationMethod 變更 | 7 個（PurchaseOrder, SalesOrder 等） |
| **ActionButton + 自訂邏輯** | 額外處理特殊欄位（如載入關聯選項） | 6 個（SalesReturn, SetoffDocument 等） |

**可提取性評估**：⭐⭐ 中

**分析**：
- 「純 ActionButton」型可以完全自動化：GenericEditModalComponent 透過 ModalManagerCollection 自動處理 ActionButton 更新
- 「ActionButton + TaxMethod」型：TaxMethod 變更觸發 HandleDetailsChanged 重算，可作為 Generic 內建行為
- 「ActionButton + 自訂邏輯」型：自訂邏輯差異大，需保留各自的 OnFieldValueChanged

**建議方案**：
1. **P3（已規劃）**：GenericEditModalComponent 新增 `OnTaxMethodChanged` 參數，自動偵測 TaxCalculationMethod 變更
2. **新增 P16**：GenericEditModalComponent 新增 `ModalManagers` 參數，自動處理 ActionButton 更新，消除「純 ActionButton」型的 OnFieldValueChanged

**預估效果**：8 個「純 ActionButton」型 Edit 可完全移除 OnFieldValueChanged 方法（每個 ~15-25 行 ≈ ~160 行）

---

### 模式 3：OnParametersSetAsync — 資料載入守衛

**影響範圍**：21 個 Edit 檔案（中等型 + 複雜型）

**共通結構**（每個方法約 10-25 行）：
```csharp
protected override async Task OnParametersSetAsync()
{
    if (IsVisible && !isDataLoaded)
    {
        // 可能有額外重置邏輯
        await LoadAdditionalDataAsync();
        await InitializeFormFieldsAsync();
        isDataLoaded = true;
    }
    else if (!IsVisible)
    {
        isDataLoaded = false;
    }
}
```

**變體分類**：

| 類型 | 說明 | 檔案數 |
|---|---|---|
| **標準型** | 僅 Load + Init + flag | 13 個 |
| **重置型** | 額外重置 details list / flags | 6 個（InventoryStock, MaterialIssue 等） |
| **複雜型** | 額外載入子單據、條件選項更新 | 2 個（SalesReturn, ProductionSchedule） |

**可提取性評估**：⭐⭐⭐ 高

**分析**：核心守衛邏輯（IsVisible + isDataLoaded flag）完全一致。差異僅在額外重置動作。

**建議方案 — 移入 GenericEditModalComponent**：
- Generic 新增 `[Parameter] public Func<Task>? LoadAdditionalDataAsync` 和 `[Parameter] public Func<Task>? InitializeFormFieldsAsync`
- Generic 內部管理 `isDataLoaded` flag，自動在 `OnParametersSetAsync` 呼叫
- 需要額外重置的 Edit 可提供 `[Parameter] public Action? OnModalOpening` 回呼

**預估效果**：21 個 Edit 可移除 OnParametersSetAsync override（每個 ~10-25 行 ≈ ~350 行）

**風險**：中等。涉及 GenericEditModalComponent 生命週期修改，需注意現有 Generic 的 OnParametersSetAsync 行為。

---

### 模式 4：OnXxxSavedWrapper — Modal 儲存轉發 ✅ 已完成（P18）

**影響範圍**：17 個 Edit 檔案，共 39 個 Wrapper 方法（32 個標準型 + 7 個非標準型）

**解法**：在 `RelatedEntityModalManager<T>` 新增 `OnSavedAsync(T saved)` 方法，Razor 直接綁定：
```razor
@* 重構前 *@
OnDepartmentSaved="@OnDepartmentSavedWrapper"

@* 重構後 *@
OnDepartmentSaved="@departmentModalManager.OnSavedAsync"
```

**已消除 32 個標準型 Wrapper**（14 個檔案），跳過 7 個非標準型（含額外業務邏輯）。

**實際節省**：~160 行

---

### 模式 5：LoadAdditionalDataAsync — 下拉資料載入

**影響範圍**：21 個 Edit 檔案

**共通操作**：

| 操作 | 說明 | 出現次數 |
|---|---|---|
| GetAllAsync() | 載入關聯實體清單（Supplier, Product, Customer 等） | 全部 21 個 |
| TaxCalculationHelper.LoadTaxRateAsync() | 載入稅率 | 7 個（進銷存單據） |
| AutoCompleteConfigBuilder.Build() | 建立 AutoComplete 配置 | ~15 個 |
| 下拉選項初始化 | TaxCalculationMethod 等 SelectOption | ~7 個 |

**可提取性評估**：⭐ 低

**分析**：雖然所有 Edit 都有此方法，但每個 Edit 載入的資料集完全不同（不同的 Service、不同的實體）。無法用單一 Helper 涵蓋。AutoCompleteConfigBuilder 已經是簡化後的寫法。

**建議**：維持現狀。各 Edit 的 LoadAdditionalDataAsync 是業務特定的，無法進一步通用化。

---

### 模式 6：SaveHandler — 明細儲存

**影響範圍**：12 個 Edit 使用自訂 SaveHandler，27 個使用 UseGenericSave

**自訂 SaveHandler 分類**：

| 類型 | 說明 | 檔案 |
|---|---|---|
| **明細儲存型** | 將明細 attach 到主檔後儲存 | PurchaseOrder, Quotation, SalesOrder, SalesDelivery, SalesReturn, PurchaseReturn, ProductComposition, MaterialIssue, StockTaking |
| **關聯管理型** | 管理多對多或特殊關聯 | Supplier, Employee, Permission |
| **特殊型** | 特殊業務邏輯 | TextMessageTemplate |

**可提取性評估**：⭐ 低

**分析**：「明細儲存型」的共通部分僅是 `entity.Details = detailsList` 賦值，其餘都是業務特定驗證。已有的 Service 層處理了大部分儲存邏輯。

---

### 優先度排序

| 優先序 | 模式 | 項目編號 | 預估節省行數 | 風險 | 建議 |
|---|---|---|---|---|---|
| 1 | CreateXxxContent 三態判斷 | **P6** ✅ | ~400 行 | 低 | DetailSectionWrapper 組件 |
| 2 | OnParametersSetAsync 守衛 | **P17** | ~350 行 | 中 | 移入 GenericEditModalComponent |
| 3 | OnXxxSavedWrapper 轉發 | **P18** ✅ | ~160 行 | 低 | RelatedEntityModalManager.OnSavedAsync |
| 4 | OnFieldValueChanged 純 ActionButton | **P16** | ~160 行 | 中 | Generic 自動處理 ModalManagers |
| 5 | OnTaxMethodChanged 分支 | **P3** | ~70 行 | 低 | Generic 新增參數 |

**總計預估**：~1,375-1,775 行

---

### 各層級 Edit 剩餘可簡化空間

| 層級 | 可受益的模式 | 已完成的模式 |
|---|---|---|
| **簡單型** | P17（OnParametersSetAsync） | P11-P15 全部適用 ✅ |
| **中等型** | P6, P16, P17, P18 | P11-P15 部分適用 ✅ |
| **複雜型** | P3, P6, P16, P17, P18 | P1, P4, P5, P7, P10, P11-P15 ✅ |

---

## 實施原則

1. **漸進式重構**：先從 DepartmentEdit 等簡單 Edit 驗證，再推廣到複雜 Edit
2. **向下相容**：新 Helper / Generic 參數不破壞現有 Edit 的運作，舊寫法仍可使用
3. **先抽 Helper，後改 Generic**：避免一次改動過大
4. **每次重構一個模式**：不要同時重構多個模式，方便追蹤問題
5. **測試驗證**：每次重構後確認該 Edit 的新增、編輯、刪除、轉單、審核等功能正常
