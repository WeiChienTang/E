# Edit Modal 重構說明

## 現況分析

專案中有 **42 個 EditModalComponent**，全部包裝 `GenericEditModalComponent`。經比對 `PurchaseOrderEditModalComponent`、`PurchaseReceivingEditModalComponent`、`SalesOrderEditModalComponent`、`SalesDeliveryEditModalComponent` 四個代表性檔案，發現大量重複模式。

### 已抽出的 Helper（/Helpers/EditModal/）

| Helper | 用途 | 狀態 |
|---|---|---|
| ActionButtonHelper | ActionButton 產生與更新 | 已完成 |
| ApprovalConfigHelper | 審核鎖定判斷 | 已完成 |
| AutoCompleteConfigHelper | AutoComplete 配置建置 | 已完成 |
| ChildDocumentRefreshHelper | 子單據儲存後刷新 | 已完成 |
| CustomModuleHelper | 自訂模組建立（null 保護 + 樣板簡化） | 已完成 |
| DocumentConversionHelper | 轉單流程 | 已完成 |
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

**已套用**：
- `PurchaseOrderEditModalComponent` — `HandleDetailsChanged` + 新增模式初始化
- `PurchaseReceivingEditModalComponent` — `HandleReceivingDetailsChanged`

**重構後寫法**：
```csharp
var (total, tax) = TaxCalculationHelper.CalculateFromDetails(
    purchaseOrderDetails,
    editModalComponent.Entity.TaxCalculationMethod,
    currentTaxRate,
    d => d.SubtotalAmount,
    d => d.TaxRate);
editModalComponent.Entity.TotalAmount = total;
editModalComponent.Entity.PurchaseTaxAmount = tax;
```

**待推廣**：SalesOrderEdit、SalesDeliveryEdit、PurchaseReturnEdit、SalesReturnEdit 等（這些有折扣計算等額外邏輯，需評估是否擴充 Helper）。

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

## 待重構項目

### P2：HandleDetailsChanged 整體模式 → DetailChangedHelper

**問題**：每個 Edit 的 HandleDetailsChanged 方法結構相同：
1. 更新本地明細列表
2. 檢查 editModalComponent?.Entity
3. 呼叫稅額計算（已用 TaxCalculationHelper.CalculateFromDetails 簡化）
4. （部分）呼叫 StateHasChanged

**備註**：P1 完成後，此方法已大幅縮短（從 ~50 行降至 ~15 行），進一步提取的價值降低。可視實際需要決定是否執行。

---

### P3：OnFieldValueChanged 中的 TaxCalculationMethod 分支 → 移入 GenericEditModalComponent

**方案**：在 GenericEditModalComponent 新增 `[Parameter] public Func<Task>? OnTaxMethodChanged` 參數，自動偵測 TaxCalculationMethod 變更並呼叫。

---

### P6：RenderFragment 三態模式（明細管理器區塊）→ DetailSectionBuilder

**問題**：每個有明細管理器的 Edit 都有相同的三態判斷：
1. Entity == null → 載入中 spinner
2. SupplierId <= 0 → 提示先選擇廠商
3. 正常 → 渲染 Table 組件

**注意**：涉及 RenderFragment 和 Blazor 渲染，可能需以 `.razor` 組件實現。

---

### P8：轉單前驗證 → DocumentConversionValidator

**問題**：各轉單方法有相同的驗證步驟（檢查儲存狀態、Entity、SupplierId、明細數量等）。

---

### P9：HandleSaveSuccess / HandleCancel 樣板 → 移入 GenericEditModalComponent

**方案**：新增 `EventCallback<TEntity> OnEntitySaved` 參數，讓 Generic 在儲存成功後直接觸發。

---

## 重構進度總覽

### 已完成（第一階段 + 第二階段）

| 項目 | 對應 | 已套用 Edit 數量 |
|---|---|---|
| P1 | TaxCalculationHelper.CalculateFromDetails | 2 個（PurchaseOrder、PurchaseReceiving） |
| P4 | FormFieldLockHelper.LockMultipleFields | 7 個（全部主要進銷存單據） |
| P5 | CustomModuleHelper.CreateSingle | 7 個（全部主要進銷存單據） |
| P7 | EditModalMessages 常數 | 7 個（全部主要進銷存單據） |
| P10 | 命名規範（GetXxx 方法） | 7 個（全部主要進銷存單據） |

### 已套用的 Edit 清單

| Edit | P1 | P4 | P5 | P7 | P10 |
|---|---|---|---|---|---|
| PurchaseOrderEdit | ✅ | ✅ | ✅ | ✅ | ✅ |
| PurchaseReceivingEdit | ✅ | ✅ | ✅ | ✅ | ✅ |
| QuotationEdit | — | ✅ | ✅ | ✅ | ✅ |
| PurchaseReturnEdit | — | ✅ | ✅ | ✅ | ✅（原已符合） |
| SalesOrderEdit | — | ✅ | ✅ | ✅ | ✅ |
| SalesDeliveryEdit | — | ✅ | ✅ | ✅ | ✅ |
| SalesReturnEdit | — | ✅ | ✅ | ✅ | ✅（原已符合） |

### 未來（需改動 GenericEditModalComponent）

| 項目 | 說明 |
|---|---|
| P3 | OnTaxMethodChanged 參數 |
| P6 | DetailSectionBuilder 組件 |
| P8 | DocumentConversionValidator |
| P9 | OnEntitySaved EventCallback |

---

## 實施原則

1. **漸進式重構**：從 PurchaseOrder 和 PurchaseReceiving 開始，驗證後再推廣
2. **向下相容**：新 Helper 不破壞現有 Edit 的運作，舊寫法仍可使用
3. **先抽 Helper，後改 Generic**：避免一次改動過大
4. **每次重構一個模式**：不要同時重構多個模式，方便追蹤問題
5. **測試驗證**：每次重構後確認該 Edit 的新增、編輯、刪除、轉單、審核等功能正常
