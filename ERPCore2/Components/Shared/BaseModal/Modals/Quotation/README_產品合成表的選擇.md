# 產品合成表（BOM）選擇功能說明

## 📋 概述

本功能允許使用者在建立報價單或銷貨訂單明細時，針對具有多個 BOM 配方的產品，選擇要使用的特定配方，或選擇自定義模式手動建立組成。

## 🎯 功能背景

### 問題描述

在舊版本中，系統對於具有多筆 BOM 配方的產品，只會自動選取**最新建立的配方**，無法讓使用者選擇要使用哪一種配方。這在以下情境會造成問題：

- 同一產品針對不同客戶有不同的 BOM 配方
- 同一產品有不同規格的 BOM 配方
- 同一產品有不同類型的 BOM 配方（例如：標準版、加強版、經濟版）

### 解決方案

新增**配方選擇對話框**，讓使用者在建立明細的 BOM 組成時，可以：
1. 從所有可用的配方中選擇特定配方
2. 選擇「自定義」模式，手動建立 BOM 組成

## 🏗️ 架構設計

### 組件結構

```
報價單系統（Quotation）
├─ CompositionSelectorModal.razor          // 配方選擇對話框
├─ QuotationCompositionEditModal.razor     // BOM 編輯器（已擴充）
└─ QuotationTable.razor                    // 報價明細表（已修改流程）

銷貨訂單系統（SalesOrder）
├─ SalesOrderCompositionSelectorModal.razor    // 配方選擇對話框
├─ SalesOrderCompositionEditModal.razor        // BOM 編輯器（待擴充）
└─ SalesOrderTable.razor                       // 訂單明細表（待修改流程）
```

### 資料流程

```
使用者點擊「BOM」按鈕
    ↓
載入該產品的所有配方
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
儲存到 QuotationCompositionDetail
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

**UI 特色**：
- 當前客戶的配方會有特殊標記（藍色背景 + 徽章）
- 顯示配方的詳細資訊（客戶、規格、類型、組件數、建立日期）
- 自定義選項有特殊樣式（黃色虛線邊框）

### 2. QuotationCompositionEditModal.razor（擴充版）

**新增參數**：
```csharp
[Parameter] public int? SelectedCompositionId { get; set; }  // 選中的配方 ID
[Parameter] public bool IsCustomMode { get; set; }           // 是否為自定義模式
```

**核心邏輯**：

#### 載入資料流程
```csharp
private async Task LoadDataAsync()
{
    // 優先從快取載入
    if (OnRequestCachedData != null)
    {
        var cachedData = OnRequestCachedData();
        if (cachedData?.Any() == true)
        {
            compositionDetails = cachedData;
            return;
        }
    }

    // 從資料庫載入既有明細
    var existingDetails = await QuotationCompositionDetailService
        .GetByQuotationDetailIdAsync(QuotationDetailId!.Value);
    
    if (existingDetails.Any())
    {
        // 已有自訂的組合明細
        compositionDetails = existingDetails;
    }
    else if (SelectedCompositionId.HasValue)
    {
        // 從指定的配方複製
        compositionDetails = await QuotationCompositionDetailService
            .CopyFromCompositionAsync(QuotationDetailId!.Value, SelectedCompositionId.Value);
    }
    else if (IsCustomMode)
    {
        // 自定義模式 - 顯示組件選擇器
        compositionDetails = new List<QuotationCompositionDetail>();
        showComponentSelector = true;
    }
    else if (ProductId.HasValue)
    {
        // 向後相容：使用最新的配方
        compositionDetails = await QuotationCompositionDetailService
            .CopyFromProductCompositionAsync(QuotationDetailId!.Value, ProductId.Value);
    }
}
```

#### 自定義模式 UI

**組件選擇介面**：
- 搜尋框：快速過濾組件
- 勾選式清單：勾選要加入的組件
- 已選提示：顯示已選組件數量
- 操作按鈕：完成選擇、全部清除

**切換組件方法**：
```csharp
private void ToggleComponent(Product product, bool isChecked)
{
    if (isChecked)
    {
        // 新增組件
        var newDetail = new QuotationCompositionDetail
        {
            QuotationDetailId = QuotationDetailId ?? 0,
            ComponentProductId = product.Id,
            ComponentProduct = product,
            Quantity = 1,  // 預設數量
            UnitId = product.UnitId,
            Unit = product.Unit
        };
        compositionDetails.Add(newDetail);
        selectedComponentIds.Add(product.Id);
    }
    else
    {
        // 移除組件
        var detail = compositionDetails.FirstOrDefault(d => d.ComponentProductId == product.Id);
        if (detail != null)
        {
            compositionDetails.Remove(detail);
            selectedComponentIds.Remove(product.Id);
        }
    }
}
```

### 3. QuotationTable.razor（修改流程）

**新增欄位**：
```csharp
private bool showCompositionSelectorModal = false;
private int? selectedCompositionId = null;
private bool isCustomCompositionMode = false;
private List<ProductComposition> availableCompositions = new();
```

**修改的方法**：

#### ShowCompositionEditor（修改為 async）
```csharp
private async Task ShowCompositionEditor(QuotationItem item)
{
    if (item.SelectedProduct == null) return;
    
    selectedQuotationItemIndex = QuotationItems.IndexOf(item);
    selectedCompositionProductName = item.SelectedProduct.Name;
    selectedCompositionProductId = item.SelectedProduct.Id;
    
    // 載入該產品的所有配方
    availableCompositions = await ProductCompositionService
        .GetCompositionsByProductIdAsync(item.SelectedProduct.Id);
    
    // 顯示配方選擇器
    showCompositionSelectorModal = true;
}
```

#### HandleCompositionSelected（新增）
```csharp
private void HandleCompositionSelected((int? compositionId, bool isCustomMode) selection)
{
    showCompositionSelectorModal = false;
    
    selectedCompositionId = selection.compositionId;
    isCustomCompositionMode = selection.isCustomMode;
    
    // 開啟 BOM 編輯器
    showCompositionModal = true;
}
```

**Modal 宣告**：
```razor
<!-- 配方選擇 Modal -->
<CompositionSelectorModal IsVisible="@showCompositionSelectorModal"
                         IsVisibleChanged="@((bool visible) => showCompositionSelectorModal = visible)"
                         ProductName="@selectedCompositionProductName"
                         Compositions="@availableCompositions"
                         CurrentCustomerId="@SelectedCustomerId"
                         OnSelected="@HandleCompositionSelected" />

<!-- BOM 組合編輯 Modal -->
<QuotationCompositionEditModal IsVisible="@showCompositionModal"
                              IsVisibleChanged="@((bool visible) => showCompositionModal = false)"
                              QuotationDetailId="@GetSelectedQuotationDetailId()"
                              ProductName="@selectedCompositionProductName"
                              ProductId="@selectedCompositionProductId"
                              SelectedCompositionId="@selectedCompositionId"
                              IsCustomMode="@isCustomCompositionMode"
                              IsReadOnly="@GetCompositionModalReadOnlyState()"
                              OnSave="@HandleCompositionSave"
                              OnCancel="@(() => showCompositionModal = false)"
                              OnRequestCachedData="@GetCachedCompositionData" />
```

## 🔧 Service 層修改

### IQuotationCompositionDetailService

**新增方法**：
```csharp
/// <summary>
/// 從指定的產品配方複製 BOM 到報價單明細
/// </summary>
Task<List<QuotationCompositionDetail>> CopyFromCompositionAsync(
    int quotationDetailId, 
    int compositionId);
```

**保留方法**（向後相容）：
```csharp
/// <summary>
/// 從產品合成表複製 BOM 到報價單明細（使用最新的配方）
/// </summary>
Task<List<QuotationCompositionDetail>> CopyFromProductCompositionAsync(
    int quotationDetailId, 
    int productId);
```

### QuotationCompositionDetailService

**實作方法**：
```csharp
public async Task<List<QuotationCompositionDetail>> CopyFromCompositionAsync(
    int quotationDetailId, 
    int compositionId)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    
    // 查詢指定的產品配方
    var productComposition = await context.ProductCompositions
        .Include(x => x.CompositionDetails)
            .ThenInclude(d => d.ComponentProduct)
        .Include(x => x.CompositionDetails)
            .ThenInclude(d => d.Unit)
        .FirstOrDefaultAsync(x => x.Id == compositionId);

    if (productComposition == null || !productComposition.CompositionDetails.Any())
    {
        return new List<QuotationCompositionDetail>();
    }

    // 複製組合明細
    var quotationCompositionDetails = new List<QuotationCompositionDetail>();
    
    foreach (var detail in productComposition.CompositionDetails)
    {
        quotationCompositionDetails.Add(new QuotationCompositionDetail
        {
            QuotationDetailId = quotationDetailId,
            ComponentProductId = detail.ComponentProductId,
            Quantity = detail.Quantity,
            UnitId = detail.UnitId,
            ComponentCost = detail.ComponentCost
        });
    }

    return quotationCompositionDetails;
}
```

## 🎨 使用者體驗設計

### 配方排序邏輯

```csharp
var sortedCompositions = compositions
    .OrderByDescending(c => c.CustomerId == CurrentCustomerId)  // 當前客戶的配方優先
    .ThenByDescending(c => c.CustomerId == null)                // 通用配方次之
    .ThenByDescending(c => c.CreatedAt)                         // 最新的優先
    .ToList();
```

### 搜尋過濾功能

支援搜尋以下欄位：
- 配方代碼（Code）
- 規格（Specification）
- 客戶名稱（Customer.CompanyName）
- 配方類型（CompositionCategory.Name）

### 視覺提示

**配方清單項目**：
- 當前客戶配方：藍色背景 + 「當前客戶」徽章
- 一般配方：白色背景
- 顯示資訊：代碼、客戶、規格、類型、組件數、建立日期

**自定義選項**：
- 黃色虛線邊框
- 星星圖示
- 明確說明「不使用既有配方，自行挑選組件」

**自定義模式介面**：
- 藍色提示框說明操作方式
- 搜尋框快速過濾組件
- 勾選式清單，直覺易用
- 已選提示顯示選擇進度

## 📊 使用情境範例

### 情境 1：選擇既有配方

```
1. 使用者在報價單明細選擇「產品 A」
2. 點擊「BOM」按鈕
3. 系統顯示配方選擇對話框：
   - [BOM001] 標準配方（通用）- 5 個組件
   - [BOM002] 客戶 A 專用（當前客戶）- 7 個組件 ✓
   - [BOM003] 經濟型 - 4 個組件
4. 使用者選擇「BOM002」
5. 系統開啟 BOM 編輯器，已載入 7 個組件
6. 使用者可調整數量、單位、成本
7. 儲存
```

### 情境 2：自定義 BOM

```
1. 使用者在報價單明細選擇「產品 B」
2. 點擊「BOM」按鈕
3. 系統顯示配方選擇對話框
4. 使用者選擇「✨ 自定義」
5. 系統開啟 BOM 編輯器，顯示組件選擇介面
6. 使用者在搜尋框輸入「螺絲」
7. 勾選「M3 螺絲」、「M5 螺絲」
8. 點擊「完成選擇」
9. 表格顯示 2 個組件，預設數量都是 1
10. 使用者調整數量為 10 和 8
11. 儲存
```

### 情境 3：沒有既有配方

```
1. 使用者選擇「產品 C」（沒有任何配方）
2. 點擊「BOM」按鈕
3. 系統顯示配方選擇對話框，只有「自定義」選項
4. 顯示提示：「此產品尚未建立 BOM 配方，請選擇自定義模式」
5. 使用者點擊「自定義」
6. 進入組件選擇流程...
```

## 🔄 向後相容性

### 既有資料處理

- 已存在的報價單/銷貨訂單明細不受影響
- 已儲存的 BOM 組成視為「自定義」模式
- 不記錄來源配方資訊（快照機制）

### API 相容性

- 保留 `CopyFromProductCompositionAsync` 方法
- 新增 `CopyFromCompositionAsync` 方法
- 新增的參數都有預設值
- 不影響現有的呼叫方式

## 🚀 未來擴充方向

### 可能的增強功能

1. **配方記憶功能**
   - 同一張報價單多次使用同一產品時，記住上次選擇的配方
   - 存儲在 `Dictionary<int, (int? compositionId, bool isCustom)>`

2. **快速建立新配方**
   - 在選擇對話框中加入「基於現有配方建立新配方」按鈕
   - 複製選中的配方，開啟 ProductCompositionEditModal

3. **權限控制**
   - 某些使用者可能只能使用既有配方，不能自定義
   - 透過角色權限控制「自定義」選項的顯示

4. **批次套用配方**
   - 選擇多筆明細，批次套用相同配方
   - 提升多明細操作效率

5. **配方預覽**
   - 在選擇對話框中加入「預覽」按鈕
   - 不用開啟編輯器即可查看配方內容

6. **配方比較**
   - 同時顯示多個配方，比較其差異
   - 幫助使用者做出更好的選擇

## ⚠️ 注意事項

### 開發注意事項

1. **導航屬性清理**
   - 儲存前務必清除導航屬性（`ComponentProduct`, `Unit`）
   - 避免 EF Core 嘗試插入關聯實體

2. **資料載入順序**
   - 優先使用快取資料
   - 再檢查資料庫既有資料
   - 最後才載入配方或自定義

3. **狀態管理**
   - `showComponentSelector` 控制組件選擇介面顯示
   - `selectedComponentIds` 追蹤已選組件
   - `_dataLoadCompleted` 控制空行檢查時機

### 效能考量

1. **配方數量**
   - 如果產品有數十個配方，考慮加入分頁
   - 搜尋功能可減輕此問題

2. **組件選擇**
   - 使用 `FilteredAvailableProducts` 計算屬性
   - 避免不必要的重複過濾

3. **快取機制**
   - `productsWithComposition` 快取有 BOM 的產品 ID
   - `compositionDetailsCache` 快取已載入的組合明細

## 📝 測試建議

### 功能測試

- [ ] 配方選擇對話框正確顯示所有配方
- [ ] 搜尋功能正確過濾配方
- [ ] 配方排序邏輯正確（當前客戶優先）
- [ ] 選擇配方後正確載入組件
- [ ] 自定義模式正確顯示組件選擇介面
- [ ] 勾選組件後正確建立明細
- [ ] 取消勾選正確移除明細
- [ ] 搜尋組件功能正常
- [ ] 儲存後資料正確寫入資料庫

### 邊界測試

- [ ] 產品沒有配方時的處理
- [ ] 配方沒有組件時的處理
- [ ] 自定義模式不選擇任何組件的處理
- [ ] 重複選擇相同組件的處理
- [ ] 大量配方（50+）的效能
- [ ] 大量組件（100+）的選擇體驗

### 整合測試

- [ ] 報價單明細 → BOM 選擇 → 儲存 → 轉銷貨訂單
- [ ] 銷貨訂單明細 → BOM 選擇 → 儲存
- [ ] 編輯模式下重新選擇配方
- [ ] 唯讀模式下的行為

## 📚 相關文件

- [README_報價單BOM組成編輯功能.md](../../../Documentation/README_報價單BOM組成編輯功能.md)
- [README_銷貨訂單BOM組成編輯功能.md](../../../Documentation/README_銷貨訂單BOM組成編輯功能.md)
- [ProductComposition.cs](../../../Data/Entities/ProductionManagement/ProductComposition.cs)
- [QuotationCompositionDetail.cs](../../../Data/Entities/Sales/QuotationCompositionDetail.cs)

---

**版本**: 1.0  
**建立日期**: 2025-12-05  
**最後更新**: 2025-12-05  
**維護者**: 開發團隊
