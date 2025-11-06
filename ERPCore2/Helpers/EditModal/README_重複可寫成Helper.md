## 📋 分析檔案清單

1. `PurchaseOrderEditModalComponent.razor` - 採購單
2. `PurchaseReceivingEditModalComponent.razor` - 進貨單
3. `QuotationEditModalComponent.razor` - 報價單
4. `SalesOrderEditModalComponent.razor` - 銷貨訂單
5. `SalesReturnEditModalComponent.razor` - 銷貨退回
6. `PurchaseReturnEditModalComponent.razor` - 進貨退出

---

## 🎯 可提取 Helper 清單（依優先級）
### 🔴 高優先級（重複度高、影響範圍大）

#### 1. ✅ **FormFieldLockHelper** - 欄位鎖定邏輯 
**重複模式：**
```csharp
// 每個檔案都有幾乎完全相同的實作
private void UpdateFieldsReadOnlyState() {
    var field = formFields.FirstOrDefault(f => f.PropertyName == "XXX");
    if (field != null) {
        field.IsReadOnly = hasUndeletableDetails;
        
        if (hasUndeletableDetails) {
            field.ActionButtons = new List<FieldActionButton>();
        } else {
            field.ActionButtons = GetXxxActionButtonsAsync().Result;
        }
    }
}
```

**建議提取方法：**
```csharp
FormFieldLockHelper.LockField(formFields, "PropertyName", isLocked, actionButtonsGetter?);
FormFieldLockHelper.LockMultipleFields(formFields, fieldNames, isLocked, actionButtonsMap?);
```

**出現位置：**
- ✅ PurchaseOrderEditModalComponent (Line ~801-852) - 已重構
- ✅ PurchaseReceivingEditModalComponent (Line ~1143-1194) - 已重構
- ✅ PurchaseReturnEditModalComponent (Line ~626-633) - 已重構
- ✅ SalesOrderEditModalComponent (Line ~1060-1133) - 已重構
- ✅ SalesReturnEditModalComponent (Line ~1000-1051) - 已重構
- ✅ QuotationEditModalComponent (Line ~984-1051) - 已重構

**實作成果：**
- 📁 檔案位置：`Helpers/EditModal/FormFieldLockHelper.cs`

#### 2. ✅ **TaxCalculationHelper** - 稅額計算
**重複度：⭐⭐⭐⭐⭐**

**重複模式：**
```csharp
// 載入稅率（每個檔案都有）
private async Task LoadTaxRateAsync() {
    try {
        currentTaxRate = await SystemParameterService.GetTaxRateAsync();
    } catch {
        currentTaxRate = 5.0m;
    }
}

// 計算稅額（完全相同的公式）
entity.TaxAmount = Math.Round(entity.TotalAmount * (currentTaxRate / 100), 2);
```

**建議提取方法：**
```csharp
TaxCalculationHelper.LoadTaxRate(systemParameterService, defaultRate = 5.0m);
TaxCalculationHelper.CalculateTax(totalAmount, taxRate);
TaxCalculationHelper.CalculateTotalWithTax(totalAmount, taxRate);
```

**出現位置：**
- ✅ PurchaseOrderEditModalComponent - 已重構
- ✅ PurchaseReceivingEditModalComponent - 已重構
- ✅ PurchaseReturnEditModalComponent - 已重構
- ⏭️ QuotationEditModalComponent - 跳過（無稅額欄位）
- ✅ SalesOrderEditModalComponent - 已重構
- ✅ SalesReturnEditModalComponent - 已重構

**實作成果：**
- 📁 檔案位置：`Helpers/EditModal/TaxCalculationHelper.cs`
- 🎯 提供方法：
  - `LoadTaxRateAsync()` - 載入稅率（異步）
  - `CalculateTax()` - 計算稅額
  - `CalculateTotalWithTax()` - 計算含稅總額
  - `GenerateTaxAmountLabel()` - 產生稅額欄位標籤
  - `GenerateTaxAmountHelpText()` - 產生稅額欄位說明
  - 以及反向計算、驗證、格式化等進階功能

---

#### 3. **DetailAmountCalculator** - 明細金額計算
**重複度：⭐⭐⭐⭐**

**重複模式：**
```csharp
// HandleDetailsChanged - 計算總金額和稅額
private async Task HandleDetailsChanged(List<TDetail> details) {
    entity.TotalAmount = details.Sum(d => d.SubtotalAmount);
    entity.TaxAmount = Math.Round(entity.TotalAmount * (currentTaxRate / 100), 2);
    // entity.TotalWithTax 通常是計算屬性，自動計算
    StateHasChanged();
}
```

**建議提取方法：**
```csharp
DetailAmountCalculator.CalculateAndUpdateTotals<TEntity, TDetail>(
    entity, 
    details, 
    taxRate,
    totalAmountProperty: "TotalAmount",
    taxAmountProperty: "TaxAmount"
);
```

**出現位置：**
- PurchaseOrderEditModalComponent.HandleDetailsChanged
- PurchaseReceivingEditModalComponent.HandleReceivingDetailsChanged
- SalesOrderEditModalComponent.HandleDetailsChanged
- SalesReturnEditModalComponent.HandleReturnDetailsChanged
- QuotationEditModalComponent.HandleQuotationDetailsChanged

---

### 🟡 中優先級（有重複但可能需要微調）

#### 4. **DocumentPrintHelper** - 統一列印流程
**重複度：⭐⭐⭐⭐**

**重複模式：**
```csharp
// HandlePrint - 結構完全相同
private async Task HandlePrint() {
    var (isValid, errorMessage) = ReportPrintHelper.ValidateForPrint(...);
    if (!isValid) {
        await NotificationService.ShowWarningAsync(errorMessage);
        return;
    }
    await HandleDirectPrint(null);
}

// HandleDirectPrint - 只差 reportType 參數
private async Task HandleDirectPrint(ReportPrintConfiguration? printConfig) {
    var printUrl = ReportPrintHelper.BuildPrintUrl(
        baseUrl: NavigationManager.BaseUri,
        reportType: "xxx-report/yyy", // 唯一差異
        documentId: XxxId.Value,
        configuration: printConfig,
        autoprint: true
    );
    
    var success = await ReportPrintHelper.ExecutePrintWithHiddenIframeAsync(...);
    // 顯示訊息
}
```

**建議提取方法：**
```csharp
DocumentPrintHelper.ExecuteStandardPrint(
    documentId,
    reportType,
    navigationManager,
    jsRuntime,
    notificationService,
    printConfig?,
    requireApproval?
);
```

**備註：** ReportPrintHelper 已存在，但可以進一步封裝成「一鍵列印」方法

---

#### 5. **DetailLockHelper** - 明細鎖定狀態檢查
**重複度：⭐⭐⭐⭐**

**重複模式：**
```csharp
// LoadDetailRelatedDataAsync - 檢查是否有不可刪除的明細
private async Task LoadDetailRelatedDataAsync() {
    if (!details.Any()) {
        hasUndeletableDetails = false;
        return;
    }
    
    bool hasUndeletable = false;
    
    foreach (var detail in details.Where(d => d.Id > 0)) {
        // 檢查退貨記錄
        if (someReturnQty > 0) {
            hasUndeletable = true;
            break;
        }
        
        // 檢查沖款記錄
        if (detail.TotalPaidAmount > 0) {
            hasUndeletable = true;
            break;
        }
    }
    
    hasUndeletableDetails = hasUndeletable;
    
    if (hasUndeletableDetails) {
        UpdateFieldsReadOnlyState();
    }
}
```

**建議提取方法：**
```csharp
DetailLockHelper.CheckUndeletableDetails<TDetail>(
    details,
    checkFunctions: new[] {
        detail => CheckReturnRecords(detail.Id),
        detail => detail.TotalPaidAmount > 0
    }
);
```

**出現位置：**
- PurchaseReceivingEditModalComponent (Line ~994-1036)
- SalesOrderEditModalComponent (Line ~325-365)
- SalesReturnEditModalComponent (Line ~407-439)

---

#### 6. ✅ **DocumentConversionHelper** - 轉單邏輯
**重複度：⭐⭐⭐**

**重複模式：**
```csharp
// ShowAddModalWithPrefilledXxx - A單轉B單
public async Task ShowAddModalWithPrefilledOrder(int relatedId, int sourceId) {
    // 1. 設定預填值
    PrefilledXxxId = xxxId;
    PrefilledYyyId = yyyId;
    shouldAutoLoad = true;
    
    // 2. 顯示 Modal
    if (IsVisibleChanged.HasDelegate) {
        await IsVisibleChanged.InvokeAsync(true);
    }
    
    // 3. 等待子組件就緒
    await Task.Delay(500);
    
    // 4. 自動載入明細
    if (detailManager != null && shouldAutoLoad) {
        shouldAutoLoad = false;
        await InvokeAsync(async () => {
            await detailManager.LoadXxxItems();
            StateHasChanged();
        });
    }
}
```

**建議提取方法：**
```csharp
// 方法 1: 完全自訂（最靈活）
await DocumentConversionHelper.ShowConversionModalAsync(
    setPrefilledValues: () => { PrefilledXxxId = xxx; shouldAutoLoad = true; },
    isVisibleChanged: IsVisibleChanged,
    autoLoadAction: async () => { 
        shouldAutoLoad = false; 
        if (detailManager != null) await detailManager.LoadXxxItems(); 
    },
    detailManagerReady: () => detailManager != null,
    shouldAutoLoad: () => shouldAutoLoad,
    stateHasChangedAction: StateHasChanged,
    invokeAsync: InvokeAsync
);

// 方法 2: 簡化版（使用反射調用載入方法）
await DocumentConversionHelper.ShowConversionModalSimpleAsync(
    resetEntityId: () => EntityId = null,
    setPrefilledValues: () => { PrefilledXxxId = xxx; shouldAutoLoad = true; },
    isVisibleChanged: IsVisibleChanged,
    detailManager: detailManager,
    autoLoadMethodName: "LoadAllUnreceivedItems",
    resetShouldAutoLoad: () => shouldAutoLoad = false,
    shouldAutoLoad: () => shouldAutoLoad,
    stateHasChangedAction: StateHasChanged,
    invokeAsync: InvokeAsync
);

// 方法 3: 自訂載入邏輯（適用於複雜場景，如報價單轉銷貨訂單）
await DocumentConversionHelper.ShowConversionModalWithCustomLoadAsync(
    resetEntityId: () => EntityId = null,
    setPrefilledValues: () => { PrefilledXxxId = xxx; },
    isVisibleChanged: IsVisibleChanged,
    customLoadAction: async () => await LoadQuotationDetailsToSalesOrder(quotationId),
    detailManagerReady: () => detailManager != null,
    stateHasChangedAction: StateHasChanged,
    invokeAsync: InvokeAsync
);
```

**實作成果：**
- 📁 檔案位置：`Helpers/EditModal/DocumentConversionHelper.cs`
- 🎯 提供方法：
  - `ShowConversionModalAsync()` - 完全自訂的轉單流程
  - `ShowConversionModalSimpleAsync()` - 簡化版（使用反射）
  - `ShowConversionModalWithCustomLoadAsync()` - 自訂載入邏輯版本

**出現位置：**
- ✅ PurchaseOrderEditModalComponent → PurchaseReceivingEditModalComponent (轉進貨) - 已重構
- ✅ SalesOrderEditModalComponent → SalesReturnEditModalComponent (轉退貨) - 已重構
- ✅ QuotationEditModalComponent → SalesOrderEditModalComponent (轉銷貨) - 已重構
- ✅ PurchaseReceivingEditModalComponent → PurchaseReturnEditModalComponent (轉退出) - 已重構

---

#### 7. ✅ **ChildDocumentRefreshHelper** - 子單據儲存後刷新父單據
**重複度：⭐⭐⭐⭐⭐**

**重複模式：**
```csharp
// 當子單據儲存後，需要刷新父單據的明細資料
private async Task HandleChildDocumentSaved(ChildDocument savedDocument)
{
    try
    {
        // 1. 關閉子單據 Modal
        showChildModal = false;
        selectedChildId = null;
        
        // 2. 重新載入父單據明細
        if (ParentId.HasValue)
        {
            await LoadParentDetails(ParentId.Value);
        }
        
        // 3. 刷新明細組件顯示
        if (detailManager != null)
        {
            await InvokeAsync(async () =>
            {
                StateHasChanged();
                await Task.Delay(10);
                StateHasChanged();
            });
        }
        
        // 4. 顯示成功訊息
        await NotificationService.ShowSuccessAsync($"單據 {savedDocument.Number} 已更新");
        
        StateHasChanged();
    }
    catch (Exception ex) { ... }
}
```

**建議提取方法：**
```csharp
// 標準版：適用於大多數場景
await ChildDocumentRefreshHelper.HandleChildDocumentSavedAsync(
    closeModal: () => { showChildModal = false; selectedChildId = null; },
    reloadDetails: async () => { if (ParentId.HasValue) await LoadParentDetails(ParentId.Value); },
    detailManager: detailManager,
    notificationMessage: $"進貨單 {savedReceiving.ReceiptNumber} 已更新",
    stateHasChanged: StateHasChanged,
    invokeAsync: InvokeAsync
);

// 簡化版：不需要刷新明細組件
await ChildDocumentRefreshHelper.HandleChildDocumentSavedSimpleAsync(
    closeModal: () => { showModal = false; selectedId = null; },
    stateHasChanged: StateHasChanged
);

// 進階版：使用明細組件的特定刷新方法
await ChildDocumentRefreshHelper.HandleChildDocumentSavedWithCustomRefreshAsync(
    closeModal: () => { showModal = false; selectedId = null; },
    customRefresh: async () => await detailManager.LoadReturnedQuantitiesAsync(),
    stateHasChanged: StateHasChanged,
    invokeAsync: InvokeAsync
);

// 特殊版：報價單轉銷貨訂單（需要更新轉單狀態）
await ChildDocumentRefreshHelper.HandleQuotationConversionAsync(
    closeModal: () => { showModal = false; selectedId = null; },
    quotationId: QuotationId,
    savedSalesOrderId: savedOrder.Id,
    updateEntity: async () => { /* 更新轉單狀態 */ },
    reloadQuotation: async () => { /* 重新載入報價單 */ },
    checkUndeletable: () => details.Any(d => d.ConvertedQuantity > 0),
    updateHasUndeletableDetails: (hasUndeletable) => hasUndeletableDetails = hasUndeletable,
    reinitializeFields: InitializeFormFieldsAsync,
    stateHasChanged: StateHasChanged
);
```

**出現位置：**
- ✅ PurchaseOrderEditModalComponent.HandlePurchaseReceivingSaved - 已重構
- ✅ PurchaseReceivingEditModalComponent.HandlePurchaseReturnSaved - 已重構
- ✅ SalesOrderEditModalComponent.HandleSalesReturnSaved - 已重構
- ✅ SalesOrderEditModalComponent.HandleSetoffDocumentSaved - 待重構（簡單場景）
- ✅ SalesReturnEditModalComponent.HandleSetoffDocumentSaved - 已重構
- ✅ PurchaseReturnEditModalComponent.HandleSetoffDocumentSaved - 待重構（簡單場景）
- ✅ QuotationEditModalComponent.HandleSalesOrderSaved - 已重構（特殊版）

**實作成果：**
- 📁 檔案位置：`Helpers/EditModal/ChildDocumentRefreshHelper.cs`
- 🎯 提供方法：
  - `HandleChildDocumentSavedAsync()` - 標準版（含明細組件刷新）
  - `HandleChildDocumentSavedSimpleAsync()` - 簡化版（無明細刷新）
  - `HandleChildDocumentSavedWithCustomRefreshAsync()` - 進階版（自訂刷新方法）
  - `HandleQuotationConversionAsync()` - 特殊版（報價單轉單處理）

**效益：**
- ✅ 統一刷新邏輯，避免遺漏步驟
- ✅ 確保 UI 正確更新（雙重 StateHasChanged）
- ✅ 減少重複代碼（每個 Handle 方法約 20-30 行 → 5-10 行）
- ✅ 提升維護性（集中管理刷新流程）

---

### 🟢 低優先級（已有實作或影響較小）

#### 7. **StatusMessageHelper** - 狀態訊息生成
**重複度：⭐⭐⭐**

```csharp
// GetStatusMessage - 顯示鎖定狀態
private async Task<(string, BadgeVariant, string)?> GetStatusMessage() {
    if (!isDetailDataReady || entity == null) return null;
    
    if (hasUndeletableDetails) {
        return (
            "明細有其他動作，主檔欄位已鎖定",
            BadgeVariant.Warning,
            "fas fa-lock"
        );
    }
    
    return null;
}
```

**建議提取方法：**
```csharp
StatusMessageHelper.GetLockStatusMessage(isDataReady, hasLock, customMessage?);
```

---

#### 8. **WarningMessageHelper** - 警告訊息 RenderFragment
**重複度：⭐⭐⭐**

```csharp
private RenderFragment? GetWarningMessage() => __builder => {
    <GenericLockedFieldMessage IsVisible="@(condition)"
                              Message="鎖定原因說明" />
};
```

**建議提取方法：**
```csharp
WarningMessageHelper.CreateLockWarning(isVisible, message, type);
```

---

#### 9. **DocumentNumberHelper** - 單號生成
**重複度：⭐⭐**

**備註：** CodeGenerationHelper 已存在，只需要包裝成更簡潔的調用方式

```csharp
// 現況
return await CodeGenerationHelper.GenerateEntityCodeAsync(
    Service, "PREFIX", 
    (service, code, excludeId) => service.IsXxxNumberExistsAsync(code, excludeId)
);

// 可簡化為
return await DocumentNumberHelper.GenerateNumber(Service, "PREFIX", "XXX");
```

---