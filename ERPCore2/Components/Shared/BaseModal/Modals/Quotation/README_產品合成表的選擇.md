# 產品合成表（BOM）選擇功能說明

## 📋 概述

本功能允許使用者在建立報價單或銷貨訂單明細時，針對具有多個 BOM 配方的產品，選擇要使用的特定配方，或選擇自定義模式手動建立組成。

## 🎯 功能背景

### 問題描述

系統對於具有多筆 BOM 配方的產品，需要讓使用者選擇要使用哪一種配方。這在以下情境非常重要：

- 同一產品針對不同客戶有不同的 BOM 配方
- 同一產品有不同規格的 BOM 配方
- 同一產品有不同類型的 BOM 配方（例如：標準版、加強版、經濟版）

### 解決方案

新增**配方選擇對話框**，讓使用者在建立明細的 BOM 組成時，可以：
1. 從所有可用的配方中選擇特定配方
2. 選擇「自定義」模式，手動建立 BOM 組成
3. **重新選擇**已設定的配方（支援已儲存的明細）

## 🏗️ 架構設計

### 組件結構

```
報價單系統（Quotation）
├─ CompositionSelectorModal.razor          // 配方選擇對話框
├─ QuotationCompositionEditModal.razor     // BOM 編輯器
└─ QuotationTable.razor                    // 報價明細表

銷貨訂單系統（SalesOrder）
├─ SalesOrderCompositionSelectorModal.razor    // 配方選擇對話框
├─ SalesOrderCompositionEditModal.razor        // BOM 編輯器
└─ SalesOrderTable.razor                       // 訂單明細表
```

### 資料流程

```
使用者點擊「BOM」按鈕
    ↓
檢查是否已有組合資料
    ├─ 已有資料 → 直接開啟編輯器
    └─ 尚未選擇 → 載入配方清單
    ↓
顯示 CompositionSelectorModal（配方選擇對話框）
    ├─ 配方 1 - [BOM001] 標準配方
    ├─ 配方 2 - [BOM002] 客戶 A 專用
    ├─ 配方 3 - [BOM003] 經濟型
    └─ ✨ 自定義（手動選擇組件）
    ↓
使用者選擇
    ├─ 選擇配方 → SelectedCompositionId = 配方ID
    └─ 選擇自定義 → IsCustomMode = true
    ↓
開啟 QuotationCompositionEditModal（BOM 編輯器）
    ├─ 如果選擇配方
    │   └─ 呼叫 CopyFromCompositionAsync(compositionId)
    │       └─ 載入該配方的所有組件明細
    └─ 如果選擇自定義
        └─ 顯示組件選擇介面
            ├─ 勾選要加入的組件
            └─ 自動建立明細項目
    ↓
使用者編輯數量、單位、成本
    ↓
儲存到 QuotationItem.CustomCompositionDetails（暫存）
    ↓
報價單儲存時
    ├─ 儲存報價單明細
    └─ 儲存組合明細（處理臨時索引映射）
```

## 📦 核心組件說明

### 1. CompositionSelectorModal.razor

**用途**：配方選擇對話框

**參數**：
```csharp
[Parameter] public bool IsVisible { get; set; }
[Parameter] public string ProductName { get; set; }
[Parameter] public List<ProductComposition> Compositions { get; set; }
[Parameter] public int? CurrentCustomerId { get; set; }
[Parameter] public EventCallback<(int? compositionId, bool isCustomMode)> OnSelected { get; set; }
```

**核心功能**：
- 顯示所有可用的配方清單
- 支援搜尋過濾（代碼、規格、客戶、類型）
- 智能排序：當前客戶配方 > 通用配方 > 其他配方
- 提供「自定義」選項
- 回傳使用者選擇的配方 ID 或自定義模式標記

### 2. QuotationCompositionEditModal.razor

**參數**：
```csharp
[Parameter] public bool IsVisible { get; set; }
[Parameter] public int? QuotationDetailId { get; set; }
[Parameter] public string ProductName { get; set; }
[Parameter] public int? ProductId { get; set; }
[Parameter] public int? SelectedCompositionId { get; set; }  // 選中的配方 ID
[Parameter] public bool IsCustomMode { get; set; }           // 是否為自定義模式
[Parameter] public bool IsReselecting { get; set; }          // 是否為重新選擇模式
[Parameter] public bool IsReadOnly { get; set; }
[Parameter] public EventCallback<List<QuotationCompositionDetail>> OnSave { get; set; }
[Parameter] public Func<List<QuotationCompositionDetail>?>? OnRequestCachedData { get; set; }
[Parameter] public EventCallback OnReselect { get; set; }    // 重新選擇配方事件
```

**核心邏輯**：

#### 生命週期與狀態追蹤
```csharp
private bool _previousIsVisible = false;  // 追蹤上一次的 IsVisible 狀態

protected override async Task OnParametersSetAsync()
{
    // 只在 Modal 從隱藏變為顯示時載入資料
    if (IsVisible && !_previousIsVisible && QuotationDetailId.HasValue)
    {
        await LoadDataAsync();
    }
    
    _previousIsVisible = IsVisible;
}
```

#### 載入資料流程
```csharp
private async Task LoadDataAsync()
{
    // 1. 如果是重新選擇模式，跳過快取和資料庫查詢
    if (IsReselecting)
    {
        if (SelectedCompositionId.HasValue)
        {
            // 從新選擇的配方複製
            compositionDetails = await QuotationCompositionDetailService
                .CopyFromCompositionAsync(QuotationDetailId!.Value, SelectedCompositionId.Value);
            return;
        }
        else if (IsCustomMode)
        {
            // 自定義模式 - 顯示組件選擇器
            compositionDetails = new List<QuotationCompositionDetail>();
            showComponentSelector = true;
            return;
        }
    }

    // 2. 優先從快取載入（父組件已有暫存資料）
    if (OnRequestCachedData != null)
    {
        var cachedData = OnRequestCachedData();
        if (cachedData?.Any() == true)
        {
            compositionDetails = cachedData;
            return;
        }
    }

    // 3. 從資料庫載入既有明細
    var existingDetails = await QuotationCompositionDetailService
        .GetByQuotationDetailIdAsync(QuotationDetailId!.Value);
    
    if (existingDetails.Any())
    {
        compositionDetails = existingDetails;
    }
    // 4. 從指定的配方複製
    else if (SelectedCompositionId.HasValue)
    {
        compositionDetails = await QuotationCompositionDetailService
            .CopyFromCompositionAsync(QuotationDetailId!.Value, SelectedCompositionId.Value);
    }
    // 5. 自定義模式 - 顯示組件選擇器
    else if (IsCustomMode)
    {
        compositionDetails = new List<QuotationCompositionDetail>();
        showComponentSelector = true;
    }
    // 6. 向後相容：使用最新的配方
    else if (ProductId.HasValue)
    {
        compositionDetails = await QuotationCompositionDetailService
            .CopyFromProductCompositionAsync(QuotationDetailId!.Value, ProductId.Value);
    }
}
```

### 3. QuotationTable.razor

**關鍵欄位**：
```csharp
private bool showCompositionSelectorModal = false;
private bool showCompositionModal = false;
private int? selectedQuotationItemIndex = null;
private int? selectedCompositionId = null;
private bool isCustomCompositionMode = false;
private bool isReselectingComposition = false;  // 標記是否為重新選擇配方
private List<ProductComposition> availableCompositions = new();
private Dictionary<int, List<QuotationCompositionDetail>> compositionDetailsCache = new();
```

**核心方法**：

#### HandleCompositionSelected
```csharp
private void HandleCompositionSelected((int? compositionId, bool isCustomMode) selection)
{
    showCompositionSelectorModal = false;
    
    selectedCompositionId = selection.compositionId;
    isCustomCompositionMode = selection.isCustomMode;
    
    // 記錄選擇資訊到 QuotationItem
    if (selectedQuotationItemIndex.HasValue)
    {
        var item = QuotationItems.ElementAtOrDefault(selectedQuotationItemIndex.Value);
        if (item != null)
        {
            item.SelectedCompositionId = selection.compositionId;
            item.IsCustomComposition = selection.isCustomMode;
            
            // 如果是重新選擇，清除舊的快取資料
            if (isReselectingComposition)
            {
                item.CustomCompositionDetails = null;
                if (item.SelectedProduct != null && compositionDetailsCache.ContainsKey(item.SelectedProduct.Id))
                {
                    compositionDetailsCache.Remove(item.SelectedProduct.Id);
                }
            }
        }
    }
    
    // 開啟 BOM 編輯器
    showCompositionModal = true;
    StateHasChanged();
}
```

#### HandleCompositionReselect（重新選擇配方）
```csharp
private async Task HandleCompositionReselect()
{
    // 先標記為重新選擇模式（在關閉 Modal 之前）
    isReselectingComposition = true;
    
    // 關閉當前的 BOM 編輯 Modal
    showCompositionModal = false;
    
    // 清除當前選擇的配方資訊
    if (selectedQuotationItemIndex.HasValue)
    {
        var item = QuotationItems.ElementAtOrDefault(selectedQuotationItemIndex.Value);
        if (item?.SelectedProduct != null)
        {
            // 重新載入配方清單
            availableCompositions = await ProductCompositionService
                .GetCompositionsByProductIdAsync(item.SelectedProduct.Id);
            
            // 顯示配方選擇器
            showCompositionSelectorModal = true;
            StateHasChanged();
        }
    }
}
```

#### HandleCompositionCancel
```csharp
private void HandleCompositionCancel()
{
    showCompositionModal = false;
    isReselectingComposition = false;  // 重置重新選擇標記
}
```

## ⚠️ 重要設計說明

### 重新選擇配方機制

**問題**：當使用者點擊「重新選擇」按鈕時，已儲存的明細會從資料庫載入，導致新選擇的配方無法生效。

**解決方案**：
1. 新增 `IsReselecting` 參數標記重新選擇模式
2. 在 `LoadDataAsync()` 中，如果 `IsReselecting == true`，跳過快取和資料庫查詢
3. 直接從新選擇的配方複製資料

**流程圖**：
```
使用者點擊「重新選擇」
    ↓
HandleCompositionReselect()
    ├─ isReselectingComposition = true  ← 先設置標記
    └─ showCompositionModal = false     ← 再關閉 Modal
    ↓
顯示 CompositionSelectorModal
    ↓
使用者選擇新配方
    ↓
HandleCompositionSelected()
    ├─ 更新 selectedCompositionId
    └─ 清除舊的快取資料（因為 isReselectingComposition = true）
    ↓
開啟 QuotationCompositionEditModal（IsReselecting = true）
    ↓
LoadDataAsync()
    ├─ IsReselecting == true
    │   └─ 直接從新配方複製，跳過快取和資料庫
    └─ 返回新的組件明細 ✅
```

### 臨時索引機制

**問題**：新增報價單時，明細尚未儲存到資料庫，`QuotationDetail.Id = 0`，無法作為組合明細的外鍵。

**解決方案**：
1. `GetCompositionDetails()` 對於新增明細（ID=0），使用臨時負數索引（-1, -2, -3...）
2. 報價單儲存後，明細取得實際 ID
3. `SaveQuotationCompositionDetails()` 將臨時索引映射到實際明細 ID

### Modal 參數綁定

**QuotationTable.razor 中的 Modal 宣告**：
```razor
<QuotationCompositionEditModal 
    IsVisible="@showCompositionModal"
    IsVisibleChanged="@((bool visible) => { showCompositionModal = visible; })"
    QuotationDetailId="@GetSelectedQuotationDetailId()"
    ProductName="@selectedCompositionProductName"
    ProductId="@selectedCompositionProductId"
    SelectedCompositionId="@selectedCompositionId"
    IsCustomMode="@isCustomCompositionMode"
    IsReselecting="@isReselectingComposition"
    IsReadOnly="@GetCompositionModalReadOnlyState()"
    OnSave="@HandleCompositionSave"
    OnCancel="@HandleCompositionCancel"
    OnRequestCachedData="@GetCachedCompositionData"
    OnReselect="@HandleCompositionReselect" />
```

**注意**：
- `IsVisibleChanged` 不應重置 `isReselectingComposition`
- 重置應在 `HandleCompositionSave` 和 `HandleCompositionCancel` 中進行

### 自定義模式的組件選擇機制

**問題**：原本的設計中，使用者勾選 checkbox 後會立即將組件加入 `compositionDetails`，導致只要勾選任一個組件就會顯示「已選擇 X 個組件」，而非等待使用者完成所有選擇後才加入。

**解決方案**：
1. 新增 `pendingComponentIds` 暫存列表，用於儲存使用者勾選但尚未確認的組件 ID
2. 勾選 checkbox 時只更新 `pendingComponentIds`，不立即加入 `compositionDetails`
3. 使用者按下「完成選擇」按鈕後，才將 `pendingComponentIds` 的內容同步到 `compositionDetails`

**關鍵欄位**：
```csharp
private HashSet<int> pendingComponentIds = new();  // 暫存待選擇的組件 ID（尚未確認加入明細）
private HashSet<int> selectedComponentIds = new(); // 已確認加入的組件 ID
```

**核心方法**：

#### ToggleComponent（切換組件選擇）
```csharp
private void ToggleComponent(Product product, bool isChecked)
{
    if (isChecked)
    {
        pendingComponentIds.Add(product.Id);
    }
    else
    {
        pendingComponentIds.Remove(product.Id);
    }
    StateHasChanged();
}
```

#### ConfirmComponentSelection（完成選擇）
```csharp
private void ConfirmComponentSelection()
{
    // 移除不在 pendingComponentIds 中的現有明細
    var toRemove = compositionDetails
        .Where(d => !pendingComponentIds.Contains(d.ComponentProductId))
        .ToList();
    foreach (var detail in toRemove)
    {
        compositionDetails.Remove(detail);
        selectedComponentIds.Remove(detail.ComponentProductId);
    }
    
    // 新增 pendingComponentIds 中但不在 compositionDetails 的組件
    foreach (var productId in pendingComponentIds)
    {
        if (!compositionDetails.Any(d => d.ComponentProductId == productId))
        {
            var product = availableProducts.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                var newDetail = new QuotationCompositionDetail
                {
                    QuotationDetailId = QuotationDetailId ?? 0,
                    ComponentProductId = product.Id,
                    ComponentProduct = product,
                    Quantity = 1,
                    UnitId = product.UnitId,
                    Unit = product.Unit
                };
                compositionDetails.Add(newDetail);
                selectedComponentIds.Add(product.Id);
            }
        }
    }
    
    showComponentSelector = false;
    StateHasChanged();
}
```

#### OpenComponentSelector（開啟組件選擇器）
```csharp
private void OpenComponentSelector()
{
    // 將已存在於明細中的組件 ID 加入暫存列表
    pendingComponentIds = compositionDetails
        .Where(d => d.ComponentProductId > 0)
        .Select(d => d.ComponentProductId)
        .ToHashSet();
    showComponentSelector = true;
    StateHasChanged();
}
```

**UI 流程**：
```
使用者開啟自定義模式
    ↓
顯示組件選擇清單（checkbox 列表）
    ↓
使用者勾選多個組件
    ├─ 勾選時只更新 pendingComponentIds
    └─ 顯示「已勾選 X 個組件（尚未確認）」（黃色警告樣式）
    ↓
使用者點擊「完成選擇」按鈕
    ├─ ConfirmComponentSelection() 執行
    ├─ 將 pendingComponentIds 同步到 compositionDetails
    └─ 關閉選擇器，顯示明細表格
    ↓
顯示「已加入 X 個組件」（綠色成功樣式）+ 「繼續新增」按鈕
```

### 排除當前產品本身

**問題**：在自定義模式選擇組件時，可用的組件清單不應包含當前正在編輯的產品本身，否則會造成無限迴圈（A 產品的 BOM 包含 A 產品）。

**解決方案**：
在 `FilteredAvailableProducts` 屬性中排除當前產品：

```csharp
private List<ERPCore2.Data.Entities.Product> FilteredAvailableProducts
{
    get
    {
        var products = availableProducts.AsEnumerable();
        
        // 排除當前產品本身，避免無限迴圈
        if (ProductId.HasValue)
        {
            products = products.Where(p => p.Id != ProductId.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(componentSearchTerm))
        {
            products = products.Where(p =>
                (p.Code?.Contains(componentSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Name?.Contains(componentSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }
        
        return products.OrderBy(p => p.Code).ToList();
    }
}
```

### 唯讀模式下的控制

**問題**：當報價單已審核通過或有明細已轉單時，主檔欄位和明細會被鎖定。此時 BOM 組合編輯 Modal 中的「繼續新增」按鈕和組件選擇器也應該被禁用，避免使用者誤操作。

**解決方案**：
1. `IsReadOnly` 參數從 `QuotationTable.razor` 的 `GetCompositionModalReadOnlyState()` 傳入
2. 在「繼續新增」按鈕外層加上 `@if (!IsReadOnly)` 條件
3. 在組件選擇器區塊加上 `&& !IsReadOnly` 條件

**程式碼範例**：
```razor
@* 已確認加入的組件數量提示 *@
@if (IsCustomMode && compositionDetails.Any() && !showComponentSelector)
{
    <div class="alert alert-success mb-3 d-flex align-items-center justify-content-between">
        <span>
            <i class="bi bi-check-circle me-2"></i>
            已加入 <strong>@compositionDetails.Count</strong> 個組件
        </span>
        @if (!IsReadOnly)
        {
            <GenericButtonComponent Text="繼續新增"
                                  Variant="ButtonVariant.OutlineGreen"
                                  Size="ButtonSize.Small"
                                  IconClass="bi bi-plus-circle"
                                  OnClick="@OpenComponentSelector" />
        }
    </div>
}

@* 自定義模式的組件選擇器（唯讀模式下不顯示）*@
@if (IsCustomMode && showComponentSelector && !IsReadOnly)
{
    // 組件選擇器內容...
}
```

**鎖定邏輯來源**（`QuotationTable.razor`）：
```csharp
private bool GetCompositionModalReadOnlyState()
{
    if (IsReadOnly) return true;
    
    if (!selectedQuotationItemIndex.HasValue || selectedQuotationItemIndex.Value < 0)
        return false;
        
    var item = QuotationItems.ElementAtOrDefault(selectedQuotationItemIndex.Value);
    if (item == null) return false;
    
    // 檢查是否已轉單（ConvertedQuantity > 0）
    return item.ConvertedQuantity > 0;
}
```

---

**版本**: 3.2  
**建立日期**: 2025-12-05  
**最後更新**: 2025-12-05  
**維護者**: 開發團隊
