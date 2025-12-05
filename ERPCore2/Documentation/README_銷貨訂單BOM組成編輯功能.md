# 銷貨訂單 BOM 組成編輯功能實作記錄

## 📋 概述

實作日期: 2025年12月2日

參考文件: `README_報價單BOM組成編輯功能.md`

### 需求背景
- 報價單 (`QuotationTable.razor`) 已有 BOM 組成編輯功能 (`QuotationCompositionEditModal.razor`)
- 銷貨訂單 (`SalesOrderTable.razor`) 需要相同功能
- 關鍵差異: 銷貨訂單需顯示**庫存狀態**,協助判斷是否需要生產排程

### 業務邏輯
1. 訂單可以不經報價單直接下單,因此必須在訂單階段提供 BOM 編輯功能
2. 編輯 BOM 時需顯示各組件的現有庫存數量
3. 顯示商品的 `CanSchedule` 屬性,判斷該商品是否可排程生產
4. 採用「有下一步則鎖定」的彈性設計

---

## 🗂️ 資料層修改

### 1. 新增實體: `SalesOrderCompositionDetail.cs`

**路徑**: `Data/Entities/SalesManagement/SalesOrderCompositionDetail.cs`

**用途**: 儲存銷貨訂單專屬的 BOM 組成明細

**主要欄位**:
```csharp
- SalesOrderDetailId: int (外鍵 → SalesOrderDetail)
- ComponentProductId: int (外鍵 → Product, 組件商品)
- Quantity: decimal(18,2) (組件用量)
- UnitId: int? (外鍵 → Unit, 單位)
- ComponentCost: decimal(18,2)? (組件成本)
```

**導航屬性**:
```csharp
- SalesOrderDetail: SalesOrderDetail (所屬訂單明細)
- ComponentProduct: Product (組件商品)
- Unit: Unit (單位)
```

**索引設定**:
- 唯一索引: `(SalesOrderDetailId, ComponentProductId)` - 防止重複組件

**刪除行為**:
- Cascade Delete: 刪除訂單明細時,自動刪除相關組成明細

### 2. 修改: `SalesOrderDetail.cs`

**新增導航屬性**:
```csharp
/// <summary>
/// 銷貨訂單組成明細 (BOM)
/// </summary>
public ICollection<SalesOrderCompositionDetail>? CompositionDetails { get; set; }
```

### 3. 修改: `AppDbContext.cs`

**新增 DbSet**:
```csharp
public DbSet<SalesOrderCompositionDetail> SalesOrderCompositionDetails { get; set; }
public DbSet<QuotationCompositionDetail> QuotationCompositionDetails { get; set; }
```

**實體配置** (OnModelCreating):
```csharp
// 銷貨訂單組成明細配置
modelBuilder.Entity<SalesOrderCompositionDetail>(entity =>
{
    entity.HasOne(d => d.SalesOrderDetail)
        .WithMany(p => p.CompositionDetails)
        .HasForeignKey(d => d.SalesOrderDetailId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(d => d.ComponentProduct)
        .WithMany()
        .HasForeignKey(d => d.ComponentProductId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(d => d.Unit)
        .WithMany()
        .HasForeignKey(d => d.UnitId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasIndex(e => new { e.SalesOrderDetailId, e.ComponentProductId })
        .IsUnique();
});

// 報價單組成明細配置 (同時補上)
modelBuilder.Entity<QuotationCompositionDetail>(entity =>
{
    // ... 類似配置
});
```

### 4. 資料庫遷移

**遷移檔案**: `20251201232152_AddSalesOrderCompositionDetail.cs`

**執行命令**:
```powershell
dotnet ef migrations add AddSalesOrderCompositionDetail
dotnet ef database update
```

**建立的資料表**: `SalesOrderCompositionDetails`

**建立的索引**:
- `IX_SalesOrderCompositionDetails_ComponentProductId`
- `IX_SalesOrderCompositionDetails_SalesOrderDetailId_ComponentProductId` (UNIQUE)
- `IX_SalesOrderCompositionDetails_UnitId`

---

## 🔧 服務層修改

### 1. 新增介面: `ISalesOrderCompositionDetailService.cs`

**路徑**: `Services/ISalesOrderCompositionDetailService.cs`

**繼承**: `IGenericManagementService<SalesOrderCompositionDetail>`

**主要方法**:
```csharp
/// <summary>
/// 取得指定銷貨訂單明細的組合明細
/// </summary>
Task<List<SalesOrderCompositionDetail>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId);

/// <summary>
/// 從商品合成表複製 BOM 資料到銷貨訂單
/// </summary>
Task<List<SalesOrderCompositionDetail>> CopyFromProductCompositionAsync(
    int salesOrderDetailId, int productId);

/// <summary>
/// 批次儲存組合明細(新增、更新、刪除)
/// </summary>
Task SaveBatchAsync(int salesOrderDetailId, List<SalesOrderCompositionDetail> compositionDetails);

/// <summary>
/// 刪除指定銷貨訂單明細的所有組合明細
/// </summary>
Task DeleteBySalesOrderDetailIdAsync(int salesOrderDetailId);
```

### 2. 新增服務實作: `SalesOrderCompositionDetailService.cs`

**路徑**: `Services/SalesOrderCompositionDetailService.cs`

**繼承**: `GenericManagementService<SalesOrderCompositionDetail>`

**實作**: `ISalesOrderCompositionDetailService`

**依賴注入**:
```csharp
- IDbContextFactory<AppDbContext> contextFactory
- IProductCompositionDetailService productCompositionDetailService
- ILogger<GenericManagementService<SalesOrderCompositionDetail>> logger (可選)
```

**關鍵實作邏輯**:

#### CopyFromProductCompositionAsync
```csharp
// 從 ProductComposition 複製 BOM
var productCompositions = await context.ProductCompositionDetails
    .Include(p => p.ComponentProduct)
    .Include(p => p.Unit)
    .Where(p => p.ProductCompositionId == context.ProductCompositions
        .Where(pc => pc.ParentProductId == productId)  // 注意: 是 ParentProductId
        .Select(pc => pc.Id)
        .FirstOrDefault())
    .ToListAsync();

// 轉換為 SalesOrderCompositionDetail
return productCompositions.Select(pc => new SalesOrderCompositionDetail
{
    SalesOrderDetailId = salesOrderDetailId,
    ComponentProductId = pc.ComponentProductId,
    Quantity = pc.Quantity,
    UnitId = pc.UnitId,
    ComponentCost = pc.ComponentCost,
    Status = EntityStatus.Active
}).ToList();
```

#### SaveBatchAsync
```csharp
// 取得現有資料
var existingDetails = await context.SalesOrderCompositionDetails
    .Where(x => x.SalesOrderDetailId == salesOrderDetailId)
    .ToListAsync();

// 刪除不在新列表中的項目
var toDelete = existingDetails
    .Where(e => !compositionDetails.Any(n => n.Id == e.Id && e.Id > 0))
    .ToList();
context.SalesOrderCompositionDetails.RemoveRange(toDelete);

// 新增或更新
foreach (var detail in compositionDetails)
{
    detail.SalesOrderDetailId = salesOrderDetailId;
    
    if (detail.Id == 0)
    {
        detail.CreatedAt = DateTime.Now;
        context.SalesOrderCompositionDetails.Add(detail);
    }
    else
    {
        detail.UpdatedAt = DateTime.Now;
        context.SalesOrderCompositionDetails.Update(detail);
    }
}
```

#### SearchAsync (覆寫)
```csharp
return await context.SalesOrderCompositionDetails
    .Include(x => x.ComponentProduct)
    .Include(x => x.Unit)
    .Include(x => x.SalesOrderDetail)
    .Where(x => (x.ComponentProduct.Name != null && x.ComponentProduct.Name.Contains(keyword)) || 
               (x.ComponentProduct.Code != null && x.ComponentProduct.Code.Contains(keyword)))
    .ToListAsync();
```

#### ValidateAsync (覆寫)
```csharp
if (entity.SalesOrderDetailId <= 0)
    return ServiceResult.Failure("銷貨訂單明細ID無效");

if (entity.ComponentProductId <= 0)
    return ServiceResult.Failure("組件商品ID無效");

if (entity.Quantity <= 0)
    return ServiceResult.Failure("數量必須大於0");

return ServiceResult.Success();
```

### 3. 服務註冊: `ServiceRegistration.cs`

**新增**:
```csharp
services.AddScoped<ISalesOrderCompositionDetailService, SalesOrderCompositionDetailService>();
```

---

## 🎨 UI 層修改

### 1. 新增元件: `SalesOrderCompositionEditModal.razor`

**路徑**: `Components/Shared/BaseModal/Modals/Sales/SalesOrderCompositionEditModal.razor`

**參考**: `QuotationCompositionEditModal.razor`

**關鍵差異**: 顯示庫存資訊

#### 依賴注入
```csharp
@inject ISalesOrderCompositionDetailService SalesOrderCompositionDetailService
@inject IProductCompositionDetailService ProductCompositionDetailService
@inject IInventoryStockService InventoryStockService
@inject IProductService ProductService
@inject IUnitService UnitService
@inject INotificationService NotificationService
```

#### 主要參數
```csharp
[Parameter] public SalesOrderDetail? SalesOrderDetail { get; set; }
[Parameter] public EventCallback OnSaved { get; set; }
[Parameter] public bool IsReadOnly { get; set; }
```

#### 庫存顯示功能
```csharp
// 儲存每個商品的庫存數量
private Dictionary<int, int> productStockQuantities = new();

// 載入庫存資料
private async Task LoadProductStockQuantitiesAsync()
{
    try
    {
        productStockQuantities.Clear();
        
        foreach (var product in availableProducts)
        {
            // 取得該商品在所有倉庫的庫存並加總
            var stocks = await InventoryStockService.GetByProductIdAsync(product.Id);
            var totalStock = stocks.Sum(s => s.TotalCurrentStock);
            productStockQuantities[product.Id] = totalStock;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"載入庫存失敗:{ex.Message}");
    }
}
```

#### UI 呈現 - 庫存徽章
```html
<!-- 商品選擇下拉選單 -->
<option value="@product.Id">
    @product.Name (@product.Code)
    @if (productStockQuantities.TryGetValue(product.Id, out var stock))
    {
        <text> - 庫存: @stock</text>
    }
</option>

<!-- 表格中的庫存狀態徽章 -->
<td class="text-center">
    @if (productStockQuantities.TryGetValue(detail.ComponentProductId, out var stockQty))
    {
        <span class="badge @(stockQty > 0 ? "bg-success" : "bg-danger")">
            @stockQty
        </span>
    }
    else
    {
        <span class="badge bg-secondary">N/A</span>
    }
</td>

<!-- 商品排程提示 -->
<td class="text-center">
    @if (detail.ComponentProduct?.CanSchedule == true)
    {
        <i class="bi bi-check-circle text-success" title="可排程生產"></i>
    }
    else
    {
        <i class="bi bi-x-circle text-muted" title="不可排程"></i>
    }
</td>
```

#### 從商品 BOM 複製功能
```csharp
private async Task CopyFromProductCompositionAsync()
{
    if (SalesOrderDetail?.ProductId == null) return;

    try
    {
        var copiedDetails = await SalesOrderCompositionDetailService
            .CopyFromProductCompositionAsync(SalesOrderDetail.Id, SalesOrderDetail.ProductId.Value);

        if (copiedDetails.Any())
        {
            compositionDetails = copiedDetails;
            await NotificationService.ShowSuccessAsync($"已從商品 BOM 複製 {copiedDetails.Count} 筆組成");
        }
        else
        {
            await NotificationService.ShowWarningAsync("該商品無 BOM 資料");
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"複製失敗: {ex.Message}");
    }
}
```

#### 儲存邏輯
```csharp
private async Task SaveCompositionAsync()
{
    if (SalesOrderDetail == null) return;

    try
    {
        isSaving = true;

        // 驗證數量
        foreach (var detail in compositionDetails.Where(d => !d.IsDeleted))
        {
            if (detail.Quantity <= 0)
            {
                await NotificationService.ShowWarningAsync("組件數量必須大於 0");
                return;
            }
        }

        // 過濾掉已刪除的項目
        var validDetails = compositionDetails
            .Where(d => !d.IsDeleted)
            .ToList();

        await SalesOrderCompositionDetailService.SaveBatchAsync(
            SalesOrderDetail.Id, 
            validDetails);

        await NotificationService.ShowSuccessAsync("BOM 組成已儲存");
        await OnSaved.InvokeAsync();
        await CloseModalAsync();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"儲存失敗: {ex.Message}");
    }
    finally
    {
        isSaving = false;
    }
}
```

### 2. 修改: `SalesOrderTable.razor`

**路徑**: `Components/Shared/BaseModal/Modals/Sales/SalesOrderTable.razor`

#### 新增依賴注入
```csharp
@inject ISalesOrderCompositionDetailService SalesOrderCompositionDetailService
@inject IProductCompositionService ProductCompositionService
```

#### 新增狀態變數
```csharp
// BOM 編輯相關
private SalesOrderDetail? selectedDetailForComposition;
private bool showCompositionModal = false;
private HashSet<int> productsWithComposition = new();
private Dictionary<int, List<SalesOrderCompositionDetail>> compositionCache = new();
```

#### 初始化時載入 BOM 資料
```csharp
protected override async Task OnInitializedAsync()
{
    await base.OnInitializedAsync();
    await LoadProductCompositionsAsync();
}

private async Task LoadProductCompositionsAsync()
{
    try
    {
        // 載入所有有 BOM 組成的商品 ID
        var allCompositions = await ProductCompositionService.GetAllAsync();
        productsWithComposition = allCompositions
            .Select(pc => pc.ParentProductId)  // 注意: 是 ParentProductId
            .ToHashSet();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入 BOM 資料失敗: {ex.Message}");
        productsWithComposition = new HashSet<int>();
    }
}
```

#### 新增表格欄位 - BOM 編輯按鈕
```html
<!-- 在操作欄位中 -->
<td class="text-center">
    <!-- 現有的編輯/刪除按鈕 -->
    
    <!-- BOM 編輯按鈕 -->
    @if (detail.ProductId.HasValue && HasProductComposition(detail.ProductId.Value))
    {
        <button type="button" 
                class="btn btn-sm btn-outline-info" 
                @onclick="() => ShowCompositionEditor(detail)"
                title="編輯 BOM 組成">
            <i class="bi bi-list-ul"></i>
        </button>
    }
</td>
```

#### 新增表格欄位 - BOM 狀態徽章
```html
<!-- 新增欄位顯示 BOM 組成狀態 -->
<th>BOM</th>

<!-- 資料列 -->
<td class="text-center">
    @if (detail.ProductId.HasValue && HasProductComposition(detail.ProductId.Value))
    {
        var compositionCount = GetCompositionDetails(detail.Id)?.Count ?? 0;
        
        if (compositionCount > 0)
        {
            <span class="badge bg-success" title="已設定 @compositionCount 個組件">
                <i class="bi bi-check-circle"></i> @compositionCount
            </span>
        }
        else
        {
            <span class="badge bg-warning" title="尚未設定 BOM">
                <i class="bi bi-exclamation-circle"></i>
            </span>
        }
    }
    else
    {
        <span class="text-muted">-</span>
    }
</td>
```

#### BOM 相關方法
```csharp
private bool HasProductComposition(int productId)
{
    return productsWithComposition.Contains(productId);
}

private async Task ShowCompositionEditor(SalesOrderDetail detail)
{
    selectedDetailForComposition = detail;
    
    // 載入該明細的組成資料到快取
    try
    {
        var compositions = await SalesOrderCompositionDetailService
            .GetBySalesOrderDetailIdAsync(detail.Id);
        compositionCache[detail.Id] = compositions;
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入 BOM 資料失敗: {ex.Message}");
    }
    
    showCompositionModal = true;
}

private List<SalesOrderCompositionDetail>? GetCompositionDetails(int detailId)
{
    return compositionCache.TryGetValue(detailId, out var details) ? details : null;
}

private async Task OnCompositionSaved()
{
    if (selectedDetailForComposition != null)
    {
        // 重新載入該明細的組成資料
        try
        {
            var compositions = await SalesOrderCompositionDetailService
                .GetBySalesOrderDetailIdAsync(selectedDetailForComposition.Id);
            compositionCache[selectedDetailForComposition.Id] = compositions;
        }
        catch (Exception ex)
        {
            await NotificationService.ShowErrorAsync($"重新載入 BOM 資料失敗: {ex.Message}");
        }
    }
    
    showCompositionModal = false;
    StateHasChanged();
}
```

#### Modal 元件使用
```html
@if (showCompositionModal && selectedDetailForComposition != null)
{
    <SalesOrderCompositionEditModal 
        SalesOrderDetail="selectedDetailForComposition"
        OnSaved="OnCompositionSaved"
        IsReadOnly="false" />
}
```

### 3. 修改: `SalesOrderEditModalComponent.razor`

**路徑**: `Components/Shared/BaseModal/Modals/Sales/SalesOrderEditModalComponent.razor`

#### 新增依賴注入
```csharp
@inject ISalesOrderCompositionDetailService SalesOrderCompositionDetailService
```

#### 儲存時同步更新 BOM
```csharp
private async Task SaveAsync()
{
    try
    {
        isSaving = true;

        // ... 儲存主檔和明細的邏輯 ...

        // 儲存 BOM 組成明細
        if (Entity.Details != null)
        {
            await SaveSalesOrderCompositionDetails(Entity.Details);
        }

        await NotificationService.ShowSuccessAsync("儲存成功");
        await OnSaved.InvokeAsync();
        await CloseModalAsync();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"儲存失敗: {ex.Message}");
    }
    finally
    {
        isSaving = false;
    }
}

private async Task SaveSalesOrderCompositionDetails(List<SalesOrderDetail> details)
{
    foreach (var detail in details)
    {
        if (detail.CompositionDetails != null && detail.CompositionDetails.Any())
        {
            await SalesOrderCompositionDetailService.SaveBatchAsync(
                detail.Id,
                detail.CompositionDetails.ToList());
        }
    }
}
```

---

## 🐛 問題修正記錄

### 問題 1: 報價單轉銷貨訂單時 BOM 組成未轉入

**日期**: 2025年12月2日

**問題描述**:
- 報價單中已設定的 BOM 組成明細 (`QuotationCompositionDetail`)
- 按下「轉訂單」按鈕時,只轉入了基本明細資料
- 銷貨訂單中查看 BOM 組成時為空白,沒有資料

**原因分析**:
在 `SalesOrderTable.razor` 的 `LoadQuotationDetails` 方法中:
- 只轉換了報價單明細 (`QuotationDetail`) 的基本資料
- 沒有同步載入並轉換 BOM 組成明細 (`QuotationCompositionDetail`)
- 導致 `SalesItem.CustomCompositionDetails` 為 null

**解決方案**:

1. **注入服務**:
```csharp
@inject IQuotationCompositionDetailService QuotationCompositionDetailService
```

2. **修改 LoadQuotationDetails 方法**:
在轉換每個報價單明細時,同步載入其 BOM 組成:

```csharp
// 🔑 載入報價單明細的 BOM 組成並轉換為銷貨訂單 BOM 組成
try
{
    var quotationCompositions = await QuotationCompositionDetailService
        .GetByQuotationDetailIdAsync(quotationDetail.Id);
    
    if (quotationCompositions?.Any() == true)
    {
        // 轉換 QuotationCompositionDetail 為 SalesOrderCompositionDetail
        salesItem.CustomCompositionDetails = quotationCompositions
            .Select(qc => new SalesOrderCompositionDetail
            {
                ComponentProductId = qc.ComponentProductId,
                ComponentProduct = qc.ComponentProduct,
                Quantity = qc.Quantity,
                UnitId = qc.UnitId,
                Unit = qc.Unit,
                ComponentCost = qc.ComponentCost,
                Remarks = qc.Remarks,
                Status = qc.Status
            }).ToList();
    }
}
catch (Exception ex)
{
    // BOM 載入失敗不影響主流程,僅記錄錯誤
    Console.WriteLine($"載入報價單明細 {quotationDetail.Id} 的 BOM 組成失敗: {ex.Message}");
}
```

**修改檔案**:
- `Components/Shared/BaseModal/Modals/Sales/SalesOrderTable.razor`

**轉換流程**:
```
報價單明細 (QuotationDetail)
    └─ 報價單 BOM 組成 (QuotationCompositionDetail)
         ↓ 轉單時載入並轉換
銷貨訂單明細 (SalesItem)
    └─ 暫存 BOM 組成 (SalesItem.CustomCompositionDetails)
         ↓ 儲存時
銷貨訂單明細 (SalesOrderDetail)
    └─ 銷貨訂單 BOM 組成 (SalesOrderCompositionDetail)
```

**注意事項**:
1. BOM 組成的轉換不會影響原始的商品合成表 (`ProductCompositionDetail`)
2. 轉換時會複製所有 BOM 組成明細的屬性
3. `SalesOrderDetailId` 會在儲存時自動設定(因為此時訂單明細還未存入資料庫)
4. 即使 BOM 載入失敗,也不會影響基本明細的轉換

### 編譯錯誤修正

#### 1. 基底類別名稱錯誤
**問題**: `IGenericService<>` 和 `GenericService<>` 找不到

**原因**: 專案使用的是 `IGenericManagementService<>` 和 `GenericManagementService<>`

**修正**:
```csharp
// 錯誤
public interface ISalesOrderCompositionDetailService : IGenericService<SalesOrderCompositionDetail>
public class SalesOrderCompositionDetailService : GenericService<SalesOrderCompositionDetail>

// 正確
public interface ISalesOrderCompositionDetailService : IGenericManagementService<SalesOrderCompositionDetail>
public class SalesOrderCompositionDetailService : GenericManagementService<SalesOrderCompositionDetail>
```

#### 2. Context Factory 變數名稱不一致
**問題**: `_dbContextFactory` 不存在

**原因**: 基底類別使用 `_contextFactory`

**修正**: 全部改用 `_contextFactory`

#### 3. ProductComposition 屬性名稱錯誤
**問題**: `ProductComposition.ProductId` 找不到

**原因**: 實際屬性名稱是 `ParentProductId`

**修正**:
```csharp
// 錯誤
.Where(pc => pc.ProductId == productId)

// 正確
.Where(pc => pc.ParentProductId == productId)
```

#### 4. InventoryStock 屬性名稱錯誤
**問題**: `InventoryStock.CurrentQuantity` 找不到

**原因**: 實際屬性名稱是 `TotalCurrentStock` (NotMapped 計算屬性)

**修正**:
```csharp
// 錯誤
var totalStock = stocks.Sum(s => s.CurrentQuantity);

// 正確
var totalStock = stocks.Sum(s => s.TotalCurrentStock);
```

#### 5. IInventoryStockService 方法不存在
**問題**: `GetTotalAvailableStockByProductAsync` 方法不存在

**原因**: 該服務只有 `GetTotalAvailableStockByWarehouseAsync` (需要指定倉庫)

**修正**: 改用 `GetByProductIdAsync` 取得所有倉庫庫存後自行加總
```csharp
// 錯誤
var totalStock = await InventoryStockService.GetTotalAvailableStockByProductAsync(product.Id);

// 正確
var stocks = await InventoryStockService.GetByProductIdAsync(product.Id);
var totalStock = stocks.Sum(s => s.TotalCurrentStock);
```

---

## ✅ 測試檢查清單

### 資料層測試
- [ ] 建立 SalesOrderCompositionDetail 記錄
- [ ] 更新 SalesOrderCompositionDetail 記錄
- [ ] 刪除 SalesOrderCompositionDetail 記錄
- [ ] 刪除 SalesOrderDetail 時,相關 CompositionDetails 是否級聯刪除
- [ ] 唯一索引是否正常運作 (同一訂單明細不可有重複組件)

### 服務層測試
- [ ] GetBySalesOrderDetailIdAsync 正確回傳資料
- [ ] CopyFromProductCompositionAsync 正確複製 BOM
- [ ] SaveBatchAsync 正確處理新增/更新/刪除
- [ ] DeleteBySalesOrderDetailIdAsync 正確刪除所有組成
- [ ] SearchAsync 可根據組件名稱/代碼搜尋
- [ ] ValidateAsync 正確驗證資料

### UI 測試
- [ ] SalesOrderTable 正確顯示 BOM 編輯按鈕 (僅顯示有 BOM 的商品)
- [ ] 點擊 BOM 按鈕可開啟編輯 Modal
- [ ] Modal 正確載入現有 BOM 資料
- [ ] Modal 顯示正確的庫存數量
- [ ] Modal 顯示商品的 CanSchedule 狀態
- [ ] 從商品 BOM 複製功能正常
- [ ] 新增組件功能正常
- [ ] 編輯組件數量功能正常
- [ ] 刪除組件功能正常
- [ ] 儲存後資料正確更新到資料庫
- [ ] BOM 狀態徽章正確顯示組件數量

### 整合測試
- [ ] 新增訂單明細後可編輯 BOM
- [ ] 儲存訂單時 BOM 資料一併儲存
- [ ] 刪除訂單明細時 BOM 資料一併刪除
- [ ] BOM 組成快取機制正常運作
- [ ] 錯誤處理正常 (網路錯誤、資料驗證錯誤等)

---

## 📝 注意事項

### 1. 屬性名稱統一
- `ProductComposition` 使用 **`ParentProductId`** (不是 `ProductId`)
- `InventoryStock` 使用 **`TotalCurrentStock`** (不是 `CurrentQuantity`)
- Context Factory 使用 **`_contextFactory`** (不是 `_dbContextFactory`)

### 2. 繼承基底類別
- 服務介面繼承: `IGenericManagementService<T>`
- 服務實作繼承: `GenericManagementService<T>`
- 必須實作抽象方法: `SearchAsync` 和 `ValidateAsync`

### 3. 庫存查詢邏輯
```csharp
// 取得商品在所有倉庫的總庫存
var stocks = await InventoryStockService.GetByProductIdAsync(productId);
var totalStock = stocks.Sum(s => s.TotalCurrentStock);
```

### 4. 刪除行為設定
```csharp
// 主檔刪除時級聯刪除明細
.OnDelete(DeleteBehavior.Cascade)

// 防止循環刪除問題
.OnDelete(DeleteBehavior.NoAction)
```

### 5. 唯一索引防止重複
```csharp
entity.HasIndex(e => new { e.SalesOrderDetailId, e.ComponentProductId })
    .IsUnique();
```

### 6. 批次儲存邏輯
- 先刪除不在新列表中的項目
- 再新增 (Id == 0) 或更新 (Id > 0) 項目
- 確保 `SalesOrderDetailId` 正確設定

---

## 🔗 相關文件

- [README_報價單BOM組成編輯功能.md](README_報價單BOM組成編輯功能.md) - 參考實作
- [README_Services.md](README_Services.md) - 服務層架構說明
- [README_Data.md](README_Data.md) - 資料層架構說明

---

## 📊 檔案清單

### 新增檔案
1. `Data/Entities/SalesManagement/SalesOrderCompositionDetail.cs`
2. `Services/ISalesOrderCompositionDetailService.cs`
3. `Services/SalesOrderCompositionDetailService.cs`
4. `Components/Shared/BaseModal/Modals/Sales/SalesOrderCompositionEditModal.razor`
5. `Migrations/20251201232152_AddSalesOrderCompositionDetail.cs`
6. `Migrations/20251201232152_AddSalesOrderCompositionDetail.Designer.cs`

### 修改檔案
1. `Data/Entities/SalesManagement/SalesOrderDetail.cs`
2. `Data/Context/AppDbContext.cs`
3. `Data/ServiceRegistration.cs`
4. `Components/Shared/BaseModal/Modals/Sales/SalesOrderTable.razor`
5. `Components/Shared/BaseModal/Modals/Sales/SalesOrderEditModalComponent.razor`

---

## 🎯 未來改進方向

1. **效能優化**
   - 考慮使用 Redis 快取商品 BOM 資料
   - 批次載入庫存資料而非逐一查詢

2. **功能增強**
   - 支援 BOM 版本控制
   - 支援 BOM 成本計算
   - 支援 BOM 匯入/匯出

3. **UI/UX 改善**
   - 拖拉排序 BOM 組件
   - 即時計算 BOM 總成本
   - 庫存不足時的視覺警告

4. **業務邏輯擴展**
   - 自動檢查庫存並建議採購
   - 整合生產排程系統
   - 支援替代料件設定
