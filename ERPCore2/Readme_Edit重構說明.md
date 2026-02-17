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
| DocumentConversionHelper | 轉單流程 | 已完成 |
| EntityCodeGenerationHelper | 單號產生 | 已完成 |
| FormFieldLockHelper | 欄位鎖定/解鎖 | 已完成 |
| FormSectionHelper | 表單分組 | 已完成 |
| ModalManagerInitHelper | Modal 管理器初始化 | 已完成 |
| PrefilledValueHelper | 預填值設定 | 已完成 |
| TaxCalculationHelper | 稅率載入 | **需擴充** |

---

## 待重構項目（依優先順序）

### P1：稅額計算 Switch 區塊 → 擴充 TaxCalculationHelper

**問題**：4 個以上的 Edit 都有幾乎一模一樣的 `switch(taxMethod)` 區塊（約 40-50 行），處理 TaxExclusive / TaxInclusive / NoTax 三種算法。唯一差異是明細型別和稅額屬性名稱。

**出現位置**：
- PurchaseOrderEdit → `HandleDetailsChanged` (L867-930)
- PurchaseReceivingEdit → `HandleReceivingDetailsChanged` (L1091-1157)
- SalesOrderEdit → `HandleDetailsChanged`
- SalesDeliveryEdit → `HandleDeliveryDetailsChanged`
- 新增模式的金額初始化也有部分重複（PurchaseOrderEdit L291-298）

**方案**：在 `TaxCalculationHelper` 新增泛型方法：

```csharp
public static class TaxCalculationHelper
{
    /// <summary>
    /// 根據稅別計算金額與稅額（從明細列表）
    /// 適用於所有具有 SubtotalAmount 和 TaxRate 的明細型別
    /// </summary>
    public static (decimal TotalAmount, decimal TaxAmount) CalculateFromDetails<TDetail>(
        List<TDetail> details,
        TaxCalculationMethod taxMethod,
        decimal defaultTaxRate,
        Func<TDetail, decimal> getSubtotal,
        Func<TDetail, decimal?> getTaxRate)
    {
        // 統一的三種稅別計算邏輯
    }
}
```

**重構後各 Edit 只需一行**：
```csharp
var (total, tax) = TaxCalculationHelper.CalculateFromDetails(
    purchaseOrderDetails, taxMethod, currentTaxRate,
    d => d.SubtotalAmount, d => d.TaxRate);
editModalComponent.Entity.TotalAmount = total;
editModalComponent.Entity.PurchaseTaxAmount = tax;
```

**預估減少行數**：每個 Edit 減少約 40 行，4+ 個 Edit 共減少 160+ 行。

---

### P2：HandleDetailsChanged 整體模式 → DetailChangedHelper

**問題**：每個 Edit 的 HandleDetailsChanged 方法結構相同：
1. 更新本地明細列表
2. 檢查 editModalComponent?.Entity
3. 呼叫稅額計算
4. （部分）呼叫 StateHasChanged

**方案**：建立 `DetailChangedHelper`：

```csharp
public static class DetailChangedHelper
{
    /// <summary>
    /// 處理明細變更事件的標準流程
    /// </summary>
    public static async Task HandleDetailsChanged<TEntity, TDetail>(
        List<TDetail> details,
        GenericEditModalComponent<TEntity, ...>? editModalComponent,
        TaxCalculationMethod taxMethod,
        decimal currentTaxRate,
        Func<TDetail, decimal> getSubtotal,
        Func<TDetail, decimal?> getTaxRate,
        Action<TEntity, decimal, decimal> setAmounts,
        Action? stateHasChanged = null)
    {
        // 統一流程
    }
}
```

**預估減少行數**：每個 Edit 減少約 50 行。

---

### P3：OnFieldValueChanged 中的 TaxCalculationMethod 分支 → 移入 GenericEditModalComponent

**問題**：每個有稅別的 Edit 都有相同的分支：
```csharp
else if (fieldChange.PropertyName == nameof(X.TaxCalculationMethod))
{
    if (editModalComponent?.Entity != null)
    {
        await HandleDetailsChanged(details);
        StateHasChanged();
    }
}
```

**方案 A（推薦）**：在 GenericEditModalComponent 新增參數：

```csharp
[Parameter] public Func<Task>? OnTaxMethodChanged { get; set; }
```

GenericEditModalComponent 在 `HandleFieldChanged` 中偵測 TaxCalculationMethod 變更時自動呼叫此委派。各 Edit 只需設定回呼，不需自己寫分支判斷。

**方案 B**：建立 `FieldChangeRouter` Helper，集中處理常見欄位變更邏輯。

---

### P4：UpdateFieldsReadOnlyState 樣板 → FieldLockOrchestrator

**問題**：每個有鎖定邏輯的 Edit 都有相同結構：
1. 定義要鎖定的欄位名稱陣列
2. 呼叫 `FormFieldLockHelper.LockMultipleFieldsSimple`
3. 特殊處理有 ActionButtons 的欄位（如 SupplierId）
4. 部分呼叫 `InitializeFormFieldsAsync` + `StateHasChanged`

**方案**：建立 `FieldLockOrchestrator`：

```csharp
public class FieldLockOrchestrator
{
    public FieldLockOrchestrator(List<FormFieldDefinition> formFields)

    /// <summary>
    /// 一次性設定鎖定規則，包含一般欄位和帶 ActionButtons 的特殊欄位
    /// </summary>
    public void ApplyLockState(
        bool isLocked,
        string[] simpleFields,
        Dictionary<string, Func<Task<List<FieldActionButton>>>>? actionButtonFields = null)
}
```

**重構後**：
```csharp
private void UpdateFieldsReadOnlyState()
{
    var orchestrator = new FieldLockOrchestrator(formFields);
    orchestrator.ApplyLockState(
        isLocked: hasUndeletableDetails,
        simpleFields: new[] { "CompanyId", "Code", "OrderDate", ... },
        actionButtonFields: new() { { "SupplierId", GetSupplierActionButtonsAsync } }
    );
}
```

**預估減少行數**：每個 Edit 減少約 20-30 行。

---

### P5：GetCustomModules 樣板 → 移入 GenericEditModalComponent 或 Helper

**問題**：每個有自訂模組的 Edit 都寫幾乎相同的 null 檢查 + 建立 CustomModule 物件：

```csharp
private List<...CustomModule> GetCustomModules()
{
    if (editModalComponent == null)
        return new List<...CustomModule>();
    return new List<...CustomModule>
    {
        new ...CustomModule { Order = 1, IsVisible = true, Content = CreateXxxContent() }
    };
}
```

**方案**：在 GenericEditModalComponent 內部處理 null 保護，或提供 Helper：

```csharp
public static class CustomModuleHelper
{
    public static List<GenericEditModalComponent<TEntity, TService>.CustomModule>
        CreateSingle<TEntity, TService>(
            object? editModalComponent,
            RenderFragment content,
            int order = 1)
    {
        if (editModalComponent == null)
            return new();
        return new() { new() { Order = order, IsVisible = true, Content = content } };
    }
}
```

---

### P6：RenderFragment 三態模式（明細管理器區塊）→ DetailSectionBuilder

**問題**：每個有明細管理器的 Edit 都有相同的三態判斷：
1. Entity == null → 載入中 spinner
2. SupplierId <= 0 → 提示先選擇廠商
3. 正常 → 渲染 Table 組件

**方案**：建立 `DetailSectionBuilder` 輔助方法，封裝三態判斷邏輯，只需傳入條件和內容：

```csharp
// 在 Edit 中只需寫：
private RenderFragment CreateDetailContent() => DetailSectionBuilder.Build(
    isEntityReady: () => editModalComponent?.Entity != null,
    isSupplierSelected: () => editModalComponent?.Entity?.SupplierId > 0,
    isDataReady: () => isDetailDataReady, // 可選
    tableContent: CreateTableRenderFragment()
);
```

注意：因為涉及 RenderFragment 和 Blazor 渲染，此項可能需要以 `.razor` 組件而非純 C# Helper 實現。

---

### P7：警告訊息 → WarningMessageBuilder 或常數化

**問題**：
- 「因部分明細有其他動作，為保護資料完整性主檔欄位已設唯讀。」 這段文字在多個 Edit 中完全相同
- 命名不一致：有的用屬性 `WarningMessage`，有的用方法 `GetWarningMessage()`

**方案**：
1. 將常用警告訊息文字定義為常數（放在共用位置）
2. 統一命名風格（建議統一使用方法 `GetWarningMessage()`）
3. 可建立 `WarningMessageHelper` 產生常見的 `GenericLockedFieldMessage` 組合

---

### P8：轉單前驗證 → DocumentConversionValidator

**問題**：`HandleCreateReceivingFromOrder`、`HandleCreateReturnFromReceiving`、`HandleCreateSetoffFromReceiving` 等轉單方法都有相同的驗證步驟：
1. 檢查 canCreateXxx / 是否已儲存
2. 檢查 Entity 是否存在
3. 檢查 SupplierId > 0
4. 檢查是否有明細
5. 檢查是否有可轉的明細（未完成的）
6. 呼叫目標 Modal 的公開方法

**方案**：建立 `DocumentConversionValidator`：

```csharp
public static class DocumentConversionValidator
{
    public static async Task<bool> ValidateBeforeConversion(
        bool canConvert,
        object? entity,
        int supplierId,
        int detailCount,
        Func<Task<bool>> hasConvertibleDetails,
        INotificationService notificationService,
        string sourceDocName,
        string targetDocName)
}
```

---

### P9：HandleSaveSuccess / HandleCancel 樣板 → 考慮移入 GenericEditModalComponent

**問題**：幾乎每個 Edit 的 HandleSaveSuccess 和 HandleCancel 都是：
```csharp
private async Task HandleSaveSuccess()
{
    if (OnXxxSaved.HasDelegate && editModalComponent?.Entity != null)
        await OnXxxSaved.InvokeAsync(editModalComponent.Entity);
}
private async Task HandleCancel()
{
    if (OnCancel.HasDelegate)
        await OnCancel.InvokeAsync();
}
```

**方案**：GenericEditModalComponent 已有 `OnSaveSuccess` 和 `OnCancel` EventCallback。可考慮新增一個 `EventCallback<TEntity> OnEntitySaved` 參數，讓 Generic 直接在儲存成功後觸發，省去每個 Edit 都要寫包裝方法。

---

### P10：命名規範統一

**問題**：同一概念在不同 Edit 中使用不同的命名風格：

| 概念 | PurchaseOrder | PurchaseReceiving |
|---|---|---|
| 警告訊息 | `WarningMessage`（屬性） | `GetWarningMessage()`（方法） |
| 操作按鈕 | `CustomActionButtons`（屬性） | `GetCustomActionButtons()`（方法） |
| 明細變更 | `HandleDetailsChanged` | `HandleReceivingDetailsChanged` |
| 稅額屬性 | `PurchaseTaxAmount` | `PurchaseReceivingTaxAmount` |

**規範建議**：
- RenderFragment 回傳：統一用方法 `GetXxx()` 或統一用屬性（建議方法，因為部分需要參數）
- 明細變更方法：統一命名為 `HandleDetailsChanged`（由泛型參數區分）
- 稅額屬性：這是 Model 層問題，不在此次重構範圍

---

## 重構優先順序與執行計劃

### 第一階段（高價值、低風險）

| 項目 | 對應 | 影響範圍 | 預估減少行數 |
|---|---|---|---|
| P1 | TaxCalculationHelper 擴充 | 全部有稅別的 Edit（6+） | 160+ 行 |
| P7 | 警告訊息常數化 + 命名統一 | 全部 Edit | 少量，但提升一致性 |
| P10 | 命名規範統一 | 逐步套用 | - |

### 第二階段（中等價值）

| 項目 | 對應 | 影響範圍 | 預估減少行數 |
|---|---|---|---|
| P2 | DetailChangedHelper | 全部有明細的 Edit（6+） | 200+ 行 |
| P4 | FieldLockOrchestrator | 全部有鎖定的 Edit（6+） | 120+ 行 |
| P5 | CustomModule 樣板簡化 | 全部有自訂模組的 Edit | 50+ 行 |

### 第三階段（需改動 GenericEditModalComponent）

| 項目 | 對應 | 影響範圍 | 說明 |
|---|---|---|---|
| P3 | TaxMethod 變更自動處理 | GenericEditModalComponent | 需新增參數 |
| P9 | OnEntitySaved 整合 | GenericEditModalComponent | 需新增 EventCallback |

### 第四階段（較大改動）

| 項目 | 對應 | 影響範圍 | 說明 |
|---|---|---|---|
| P6 | DetailSectionBuilder | 可能需新增 .razor 組件 | 涉及 RenderFragment |
| P8 | DocumentConversionValidator | 全部有轉單的 Edit | 需配合各業務邏輯 |

---

## 實施原則

1. **漸進式重構**：從 PurchaseOrder 和 PurchaseReceiving 開始，驗證後再推廣
2. **向下相容**：新 Helper 不破壞現有 Edit 的運作，舊寫法仍可使用
3. **先抽 Helper，後改 Generic**：避免一次改動過大
4. **每次重構一個模式**：不要同時重構多個模式，方便追蹤問題
5. **測試驗證**：每次重構後確認該 Edit 的新增、編輯、刪除、轉單、審核等功能正常
