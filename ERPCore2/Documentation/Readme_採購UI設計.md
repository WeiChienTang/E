# 採購單 UI 設計規範

## 背景

採購單（`PurchaseOrderEditModalComponent`）是主從式單據的代表設計。
主表單（採購單頭）與明細表格（採購商品明細）同時顯示在一個頁面，
不需要切換 Tab，使用者可以一眼看到所有資訊。

此設計以 `SetoffDocumentEditModalComponent`（沖款單）為參考基準，
並在採購模組中實作完成，後續依序套用到以下 7 個模組：

| 模組 | EditModal | Table 元件 | 狀態 |
|------|-----------|------------|------|
| 採購單 | `PurchaseOrderEditModalComponent` | `PurchaseOrderTable` | ✅ 完成 |
| 入庫單 | `PurchaseReceivingEditModalComponent` | `PurchaseReceivingTable` | ✅ 完成 |
| 入庫退回 | `PurchaseReturnEditModalComponent` | `PurchaseReturnTable` | ✅ 完成 |
| 報價單 | `QuotationEditModalComponent` | `QuotationTable` | ✅ 完成 |
| 訂單 | `SalesOrderEditModalComponent` | `SalesOrderTable` | ✅ 完成 |
| 銷貨單 | `SalesDeliveryEditModalComponent` | `SalesDeliveryTable` | ✅ 完成 |
| 銷貨退回 | `SalesReturnEditModalComponent` | `SalesReturnTable` | ✅ 完成 |

---

## 設計概覽

```
┌─────────────────────────────────────────────────────┐
│  GenericEditModalComponent（Modal 框架）             │
│                                                     │
│  FormHeaderContent（鎖定警告訊息）                   │
│  CustomActionButtons（轉單 / 複製訊息按鈕）           │
│                                                     │
│  ┌─────────────────────────────────────────────┐    │
│  │  表單欄位（FormFields + FormSections）        │    │
│  │  BasicInfo: 單號、廠商、公司、日期            │    │
│  │  AmountInfo: 稅別、小計、稅額、含稅合計       │    │
│  │  AdditionalInfo: 備註                        │    │
│  └─────────────────────────────────────────────┘    │
│                                                     │
│  ┌─────────────────────────────────────────────┐    │
│  │  CustomModules（Order=1）                    │    │
│  │  XxxTable（明細表格）                        │    │
│  │  - 商品搜尋 / 數量 / 單位 / 數量 / 單價      │    │
│  │  - 稅率 / 小計 / 狀態 / 備註                 │    │
│  └─────────────────────────────────────────────┘    │
│                                                     │
│  ShowApprovalSection（審核區塊，依設定顯示）          │
│  ShowPrintButton（列印 / PDF）                       │
└─────────────────────────────────────────────────────┘
```

---

## 與 Tab 設計的差異

| 比較項目 | Tab 設計（舊） | 同頁設計（新） |
|----------|--------------|--------------|
| 欄位與明細 | 分兩個 Tab，需切換 | 同一頁面，直接可見 |
| Layout 建立 | `.GroupIntoTab()` + `.BuildAll()` | 直接 `.Build()` |
| 明細表格掛載 | `.GroupIntoCustomTab()` | `CustomModules` 參數 |
| `tabDefinitions` 欄位 | 需要 | 不需要，移除 |
| `AdditionalSections` | 用來放進貨/退回記錄 Tab | **移除**，不在底部顯示 |
| 相關記錄 Tab 元件 | 在 `AdditionalSections` 中渲染 | **不再渲染**（元件檔案保留但未使用） |

### 相關記錄 Tab 元件（已棄用）

下列 Tab 元件檔案仍存在於 codebase，但不再被任何 EditModal 渲染（`AdditionalSections` 已移除）：

```
Components/Pages/Purchase/PurchaseOrderEditModal/
├── PurchaseOrderReceivingTab.razor   ← 不再渲染
└── PurchaseOrderReturnTab.razor      ← 不再渲染

Components/Pages/Purchase/PurchaseReceivingEditModal/
├── PurchaseReceivingReturnTab.razor  ← 不再渲染
├── PurchaseReceivingSetoffTab.razor  ← 不再渲染
└── PurchaseReceivingOrderTab.razor   ← 不再渲染

Components/Pages/Purchase/PurchaseReturnEditModal/
├── PurchaseReturnReceivingTab.razor  ← 不再渲染
└── PurchaseReturnSetoffTab.razor     ← 不再渲染

Components/Pages/Sales/QuotationEditModal/
└── QuotationOrderTab.razor           ← 不再渲染

Components/Pages/Sales/SalesOrderEditModal/
├── SalesOrderDeliveryTab.razor       ← 不再渲染
├── SalesOrderReturnTab.razor         ← 不再渲染
└── SalesOrderQuotationTab.razor      ← 不再渲染

Components/Pages/Sales/SalesDeliveryEditModal/
├── SalesDeliveryReturnTab.razor      ← 不再渲染
├── SalesDeliverySetoffTab.razor      ← 不再渲染
└── SalesDeliveryOrderTab.razor       ← 不再渲染

Components/Pages/Sales/SalesReturnEditModal/
├── SalesReturnDeliveryTab.razor      ← 不再渲染
└── SalesReturnSetoffTab.razor        ← 不再渲染
```

> **注意**：這些元件仍可透過 `LoadAsync(id)` / `Clear()` 公開方法獨立運作，如未來需要在其他地方（如獨立抽屜/Drawer）顯示相關記錄，可直接復用。

---

## 元件層次結構

```
GenericEditModalComponent（Modal 框架）
    └── XxxEditModalComponent（主組件，協調資料流）
            └── XxxTable（明細表格）
                    ├── BaseDetailTableComponent（抽象基底：生命週期 + 共用邏輯）
                    └── InteractiveTableComponent（UI 原語：表格渲染 + 空白行）
```

**層次職責：**

| 層次 | 元件 | 職責 |
|------|------|------|
| UI 原語 | `InteractiveTableComponent<TItem>` | 表格渲染、空白行管理、行選取、批量操作 |
| 生命週期協調 | `BaseDetailTableComponent<TMainEntity, TDetailEntity, TItem>` | DataVersion 追蹤、SelectedSupplierId 追蹤、空白行觸發修正、不可刪除狀態通知 |
| 業務邏輯 | `XxxTable.razor` | 商品搜尋、欄位定義、Lock 判斷、金額計算、特殊業務規則 |
| 資料協調 | `XxxEditModalComponent.razor` | 主檔 + 明細協調、儲存、轉單、金額匯總 |

---

## BaseDetailTableComponent（抽象基底類別）

檔案位置：`Components/Shared/Table/BaseDetailTableComponent.cs`

### 設計目的

7 個模組（採購/入庫/入庫退回/報價/訂單/銷貨/銷貨退回）共用完全相同的生命週期邏輯：
- `DataVersion` 追蹤：父元件遞增 → 重載明細
- `SelectedSupplierId` 追蹤：切換對象（廠商/客戶）→ 清空重載
- `_isLoadingDetails` 防重入保護
- 空白行觸發修正（`_dataLoadCompleted false→true` 轉換）
- `_hasUndeletableDetails` 計算與通知

### 共用參數（子類別無需重複宣告）

```csharp
[Parameter] public TMainEntity? MainEntity { get; set; }
[Parameter] public List<TDetailEntity> ExistingDetails { get; set; } = new();
[Parameter] public EventCallback<List<TDetailEntity>> OnDetailsChanged { get; set; }
[Parameter] public bool IsReadOnly { get; set; } = false;
[Parameter] public bool IsParentLoading { get; set; } = false;
[Parameter] public bool IsApproved { get; set; } = false;
[Parameter] public int DataVersion { get; set; } = 0;
[Parameter] public int? SelectedSupplierId { get; set; }    // 也用於 SelectedCustomerId
[Parameter] public EventCallback<bool> OnHasUndeletableDetailsChanged { get; set; }
```

### 抽象方法（子類別必須實作）

```csharp
// 將 ExistingDetails 對應到 Items 列表（Items 已 Clear，只需 Add）
protected abstract Task LoadItemsFromDetailsAsync();

// 判斷是否有不可刪除明細（有下游記錄）
protected abstract bool ComputeHasUndeletableDetails();

// 將 Items 轉換回 TDetailEntity 列表（通知父元件）
protected abstract List<TDetailEntity> ConvertItemsToDetails();
```

### 虛方法（子類別可選擇性覆寫）

```csharp
// SelectedSupplierId 改變時（如重新載入廠商已知商品目錄）
protected virtual Task OnCounterpartyChangedAsync() => Task.CompletedTask;

// 資料載入完成後的額外操作（如檢查歷史採購記錄）
protected virtual Task OnAfterLoadAsync() => Task.CompletedTask;
```

### 共用方法（子類別可直接呼叫）

```csharp
// 任何明細異動後呼叫，通知父元件並更新不可刪除狀態
protected async Task NotifyDetailsChangedAsync();

// 公開方法：重載明細（子單據儲存後供父元件呼叫）
public async Task RefreshDetailsAsync();
```

### 空白行修正機制

`InvokeLoadWithCompletionAsync()` 的關鍵設計：

```csharp
private async Task InvokeLoadWithCompletionAsync()
{
    _dataLoadCompleted = false;   // ← 先設 false
    Items.Clear();
    await LoadItemsFromDetailsAsync();  // ← 子類別填充 Items
    // ...計算 hasUndeletable...
    _dataLoadCompleted = true;    // ← 設 true（即使 Items 為空也會執行）
    StateHasChanged();
}
```

無論 `ExistingDetails` 是否為空，`_dataLoadCompleted` 必然走 `false→true` 轉換，
`InteractiveTableComponent.OnAfterRenderAsync` 必然偵測到此轉換並補上空白行。

**舊設計的問題（已由基底類別修正）：**
```csharp
// ❌ 舊設計：ExistingDetails 為空時 early return，_dataLoadCompleted 永遠是 true
private async Task LoadExistingDetailsAsync()
{
    if (!ExistingDetails.Any()) return;  // ← 問題根源
    _dataLoadCompleted = false;
    // ...
    _dataLoadCompleted = true;
}
```

---

## 元件職責

### 主組件：`XxxEditModalComponent.razor`

職責：協調所有子元件，管理主檔與明細的資料流。

**關鍵欄位：**
```csharp
// 明細資料
private List<XxxDetail> details;
private XxxTable? detailManager;   // 不再有泛型型別參數

// 狀態旗標
private bool isApprovalEnabled;
private bool hasUndeletableDetails;
private int _detailsDataVersion;   // 遞增觸發 Table 重載
```

**DataVersion 使用模式：**
```csharp
// 父元件儲存或切換上下筆後：
_detailsDataVersion++;
// Table 的 OnParametersSetAsync 偵測到 DataVersion 變化，自動重載
```

### 明細元件：`XxxTable.razor`

職責：管理商品明細列表，業務規則、欄位定義。

**繼承方式：**
```razor
@inherits BaseDetailTableComponent<XxxMainEntity, XxxDetailEntity, XxxTable.XxxItem>
```

**實作抽象方法範例（採購單）：**
```csharp
protected override async Task LoadItemsFromDetailsAsync()
{
    foreach (var detail in ExistingDetails)
    {
        var item = new ProductItem
        {
            ProductId = detail.ProductId,
            Quantity = detail.OrderQuantity,
            Price = detail.UnitPrice,
            // ...直接存取屬性，不使用 reflection
        };
        Items.Add(item);
    }
}

protected override bool ComputeHasUndeletableDetails()
    => Items.Any(p => !DetailLockHelper.CanDeleteItem(p, out _, checkReceiving: true));

protected override List<PurchaseOrderDetail> ConvertItemsToDetails()
{
    return Items.Select(p => new PurchaseOrderDetail { ... }).ToList();
}
```

**對外事件：**

| EventCallback | 說明 |
|--------------|------|
| `OnDetailsChanged` | 明細變更時通知父元件（含金額計算） |
| `OnHasUndeletableDetailsChanged` | 不可刪除狀態變更時通知父元件（觸發欄位鎖定） |
| `OnOpenRelatedDocument` | 使用者點擊查看子單據時通知父元件開啟 Modal（部分模組） |

**公開方法：**

| 方法 | 說明 | 來源 |
|------|------|------|
| `RefreshDetailsAsync()` | 重載明細（子單據儲存後呼叫） | 繼承自基底類別 |

---

## 表單欄位鎖定邏輯

鎖定由 `ApprovalConfigHelper.ShouldLockFieldByApproval()` 統一判斷：

```
shouldLock = isApprovalEnabled && (IsApproved == true)
          OR hasUndeletableDetails == true
```

**鎖定時的行為：**
- 所有主表單欄位（單號、廠商/客戶、公司、日期、稅別、備註）→ `IsReadOnly = true`
- 對象欄位的 ActionButtons（新增/編輯按鈕）→ 回傳空列表
- Table 欄位：商品、數量、單價、稅率 → `IsDisabledFunc` 依 `DetailLockHelper` 判斷
- Table 欄位：狀態欄（完成進貨等）、備註 → **鎖定後仍可編輯**

**警告訊息（`FormHeaderContent`）：**
- 審核通過 → 顯示 `EditModalMessages.XxxApprovedWarning`
- 有下游記錄 → 顯示 `EditModalMessages.UndeletableDetailsWarning`

---

## 金額計算流程

```
使用者編輯明細（數量 / 單價 / 稅率）
    ↓
XxxTable.NotifyDetailsChangedAsync()
    ↓ OnDetailsChanged
XxxEditModalComponent.HandleDetailsChanged()
    ↓
TaxCalculationHelper.CalculateFromDetails()
（依稅別：外加稅 / 內含稅 / 免稅）
    ↓
entity.TotalAmount              ← 未稅總計
entity.XxxTaxAmount             ← 稅額
entity.XxxTotalAmountIncludingTax ← 含稅合計
    ↓
表單欄位（IsReadOnly=true）自動顯示最新數值
```

---

## 新增模式的特殊處理

新增模式時：
- 「轉子單」按鈕禁用
- 「複製訊息」按鈕禁用
- 儲存成功後啟用上述按鈕

部分模組支援從其他頁面帶入預填資料（`ShowAddModalWithPrefilledData`）：
- 預填對象 ID、商品 ID、建議單價
- `LoadXxxData()` 讀取預填值並建立第一筆明細

---

## 套用到其他模組的步驟

### EditModal 修改（Step 1-5 為舊 Tab 設計遷移，新增時跳過）

#### Step 1：修改 InitializeFormFieldsAsync（舊 Tab 遷移）

```csharp
// 改為（同頁設計）
formSections = FormSectionHelper<Xxx>.Create()
    .AddToSection(FormSectionNames.BasicInfo, ...)
    .AddToSection(FormSectionNames.AmountInfo, ...)
    .AddToSection(FormSectionNames.AdditionalInfo, ...)
    .Build();
```

#### Step 2：移除 tabDefinitions 欄位宣告

```csharp
// 刪除這行
private List<FormTabDefinition>? tabDefinitions;
```

#### Step 3：新增 GetCustomModules() 方法

```csharp
private List<GenericEditModalComponent<Xxx, IXxxService>.CustomModule> GetCustomModules()
{
    // ...
    return new List<...> { new ...CustomModule { Title = "", Content = CreateDetailContent(), Order = 1 } };
}
```

#### Step 4：更新 Razor 模板參數

```razor
@* 舊的 *@
TabDefinitions="@tabDefinitions"

@* 新的 *@
CustomModules="@GetCustomModules()"
```

#### Step 5：移除 AdditionalSections

若有不需要的 `AdditionalSections` 參數則移除。

### Table 元件修改（繼承基底類別）

#### Step 6：移除 @typeparam 並加上 @inherits

```razor
@* 移除這兩行 *@
@typeparam TMainEntity where TMainEntity : BaseEntity
@typeparam TDetailEntity where TDetailEntity : BaseEntity, new()

@* 加上這行 *@
@inherits BaseDetailTableComponent<XxxMainEntity, XxxDetailEntity, XxxTable.XxxItem>
```

#### Step 7：移除共用 Parameter

刪除以下已由基底類別提供的 `[Parameter]`：
- `MainEntity`、`ExistingDetails`、`OnDetailsChanged`
- `IsReadOnly`、`IsParentLoading`、`IsApproved`
- `DataVersion`、`SelectedSupplierId`（或 `SelectedCustomerId`）
- `OnHasUndeletableDetailsChanged`
- 所有 `*PropertyName` 參數（不再使用 reflection）

#### Step 8：移除共用欄位與生命週期

刪除以下已由基底類別提供的欄位和方法：
- `Items`（或舊名 `ProductItems` 等）→ 改用基底的 `Items`
- `tableComponent` 欄位 → 基底已宣告
- `_previousDataVersion`、`_previousSelectedSupplierId`、`_isLoadingDetails`
- `_dataLoadCompleted`、`_hasUndeletableDetails`
- `OnInitializedAsync()`、`OnParametersSetAsync()`（如無額外邏輯）
- `RefreshDetailsAsync()` → 基底已提供

#### Step 9：實作 3 個抽象方法

```csharp
protected override async Task LoadItemsFromDetailsAsync()
{
    // 直接存取屬性（不使用 reflection）
    foreach (var detail in ExistingDetails)
    {
        Items.Add(new XxxItem { ... = detail.XxxProperty });
    }
}

protected override bool ComputeHasUndeletableDetails()
    => Items.Any(p => !DetailLockHelper.CanDeleteItem(p, out _, checkXxx: true));

protected override List<XxxDetailEntity> ConvertItemsToDetails()
    => Items.Select(p => new XxxDetailEntity { ... = p.Field }).ToList();
```

#### Step 10：覆寫虛方法（依模組需求）

```csharp
// 如有廠商/客戶相關資料需重載
protected override async Task OnCounterpartyChangedAsync()
{
    await LoadCounterpartyCatalogAsync();
}

// 如有載入後額外查詢
protected override async Task OnAfterLoadAsync()
{
    await CheckLastPurchaseRecordAsync();
}
```

#### Step 11：更新 XxxItem 內部型別

```csharp
// 原本（泛型版本）
public TDetailEntity? ExistingDetailEntity { get; set; }

// 改為（具體型別）
public XxxDetailEntity? ExistingDetailEntity { get; set; }
```

#### Step 12：更新 EditModal 呼叫端

```razor
@* 移除泛型型別參數（ref 欄位也更新）*@
<XxxTable
    @* 移除: TMainEntity="XxxMainEntity" TDetailEntity="XxxDetailEntity" *@
    @* 移除: MainEntityIdPropertyName/QuantityPropertyName 等 *@
    MainEntity="@entity"
    ExistingDetails="@details"
    ...
```

---

## 檔案位置

```
Components/Shared/Table/
└── BaseDetailTableComponent.cs      ← 抽象基底（7 個模組共用）

Components/Pages/Purchase/
├── PurchaseOrderEditModalComponent.razor
├── PurchaseOrderTable.razor
├── PurchaseReceivingEditModalComponent.razor
├── PurchaseReceivingTable.razor
├── PurchaseReturnEditModalComponent.razor
└── PurchaseReturnTable.razor

Components/Pages/Sales/
├── QuotationEditModalComponent.razor
├── QuotationTable.razor
├── SalesOrderEditModalComponent.razor
├── SalesOrderTable.razor
├── SalesDeliveryEditModalComponent.razor
├── SalesDeliveryTable.razor
├── SalesReturnEditModalComponent.razor
└── SalesReturnTable.razor
```

---

## 注意事項

1. **`DataVersion` 必須遞增**：每次從父元件重新載入明細後，必須 `_detailsDataVersion++`，Table 才能偵測到需要重載。

2. **`IsParentLoading` 必須傳遞**：避免 Table 的 `OnInitializedAsync` 在父元件尚未載入完成時就嘗試載入資料，導致顯示舊資料。

3. **廠商/客戶未選時隱藏 Table**：使用 `style="display:none"` 而非 `@if`，避免 Table 元件被卸載重建（會丟失狀態）。

4. **`SaveXxxDetails` 要處理刪除**：儲存時需比較現有明細與畫面明細，刪除已移除的項目。

5. **`OnHasUndeletableDetailsChanged` 要觸發 UI 更新**：狀態變更後需呼叫 `InitializeFormFieldsAsync()` + `StateHasChanged()` 才能讓欄位鎖定生效。

6. **空白行由基底類別自動管理**：
   `BaseDetailTableComponent.InvokeLoadWithCompletionAsync()` 保證 `_dataLoadCompleted` 必然走 `false→true` 轉換，
   無論 `ExistingDetails` 是否為空，`InteractiveTableComponent` 都能偵測到並補上空白行。
   子類別只需在 `LoadItemsFromDetailsAsync()` 中填充 `Items`，**不需要**手動呼叫 `RefreshEmptyRow()`。

7. **`SelectedSupplierId` 同時用於客戶 ID**：銷售模組將 `SelectedCustomerId` 綁定到基底的 `SelectedSupplierId` 參數，
   參數名稱在 Razor 中用 `SelectedSupplierId="@(int?)selectedCustomerId"` 傳入。

8. **模組特有參數保留在子類別**：例如 `SalesOrderTable` 的 `SelectedQuotationId`、`FilterProductId`；
   `SalesDeliveryTable` 的 `ShowWarehouse`、`ShowWarehouseLocation`。這些不在基底類別，子類別自行宣告追蹤。

9. **相關記錄 Tab 元件不再需要"請先儲存"提示**：原本 Tab 元件（如 `PurchaseReceivingReturnTab`）在 `_receivingId <= 0` 時顯示
   "請先儲存進貨單，再查看退回記錄"。由於 `AdditionalSections` 已移除，這些元件不再被渲染，此判斷區塊已一併清除。
   若未來重新啟用這些元件，可根據需求重新加入 Guard 邏輯。
