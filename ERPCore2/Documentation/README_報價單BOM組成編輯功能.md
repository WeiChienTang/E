# 報價單 BOM 組成編輯功能

## 📋 需求概述

在報價單明細中，當商品具有 BOM 組成時，允許使用者查看並編輯該商品的組成明細。編輯後的 BOM 資料僅影響當前報價單，不會修改商品物料清單（ProductCompositionDetail）的原始資料。

### 核心需求
- ✅ 滑鼠移過或點擊商品時，可查看該商品的 BOM 組成
- ✅ 可編輯 BOM 組成的數量、單位、成本等資訊
- ✅ 編輯結果僅儲存在報價單中，不影響商品物料清單
- ✅ 使用 `BaseModalComponent` 和 `InteractiveTableComponent` 保持 UI 一致性

---

## 🗂️ 資料層修改

### 1. 新增實體：QuotationCompositionDetail.cs

**檔案位置：** `Data/Entities/QuotationCompositionDetail.cs`

**用途：** 儲存報價單專屬的 BOM 組成明細

**欄位說明：**
```csharp
public class QuotationCompositionDetail : BaseEntity
{
    // 關聯欄位
    public int QuotationDetailId { get; set; }        // 報價明細 ID
    public int ComponentProductId { get; set; }       // 組成商品 ID
    
    // 數量與單位
    public decimal Quantity { get; set; }             // 組成數量
    public int? UnitId { get; set; }                  // 單位 ID
    
    // 成本資訊
    public decimal? ComponentCost { get; set; }       // 組成成本
    
    // Navigation Properties
    public virtual QuotationDetail QuotationDetail { get; set; }
    public virtual Product ComponentProduct { get; set; }
    public virtual Unit? Unit { get; set; }
}
```

**設計重點：**
- 繼承 `BaseEntity`，包含 Id, Code, Status, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, Remarks
- 使用複合唯一索引 `(QuotationDetailId, ComponentProductId)` 避免重複組成項目
- 級聯刪除：當報價明細刪除時，自動刪除相關的組成明細

### 2. 修改 QuotationDetail.cs

**新增 Navigation Property：**
```csharp
/// <summary>
/// 自訂的組合明細（報價單專屬）
/// </summary>
public virtual ICollection<QuotationCompositionDetail> CompositionDetails { get; set; } = new List<QuotationCompositionDetail>();
```

### 3. 更新 AppDbContext.cs

**註冊 DbSet：**
```csharp
public DbSet<QuotationCompositionDetail> QuotationCompositionDetails { get; set; }
```

**配置實體關聯（OnModelCreating）：**
```csharp
// QuotationCompositionDetail 設定
modelBuilder.Entity<QuotationCompositionDetail>(entity =>
{
    // 複合唯一索引：同一報價明細中，同一組成商品只能出現一次
    entity.HasIndex(e => new { e.QuotationDetailId, e.ComponentProductId })
          .IsUnique();

    // 與 QuotationDetail 的關聯
    entity.HasOne(d => d.QuotationDetail)
          .WithMany(p => p.CompositionDetails)
          .HasForeignKey(d => d.QuotationDetailId)
          .OnDelete(DeleteBehavior.Cascade);  // 級聯刪除

    // 與 Product 的關聯
    entity.HasOne(d => d.ComponentProduct)
          .WithMany()
          .HasForeignKey(d => d.ComponentProductId)
          .OnDelete(DeleteBehavior.Cascade);

    // 與 Unit 的關聯
    entity.HasOne(d => d.Unit)
          .WithMany()
          .HasForeignKey(d => d.UnitId)
          .OnDelete(DeleteBehavior.ClientSetNull);
});
```

### 4. 建立 Migration

```bash
dotnet ef migrations add AddQuotationCompositionDetail
dotnet ef database update
```

**資料表結構：**
- 表名：`QuotationCompositionDetails`
- 主鍵：`Id` (Identity)
- 索引：
  - `IX_QuotationCompositionDetails_ComponentProductId`
  - `IX_QuotationCompositionDetails_QuotationDetailId_ComponentProductId` (Unique)
  - `IX_QuotationCompositionDetails_UnitId`

---

## 🔧 服務層修改

### 1. 介面：IQuotationCompositionDetailService.cs

**檔案位置：** `Services/IQuotationCompositionDetailService.cs`

**主要方法：**
```csharp
public interface IQuotationCompositionDetailService : IGenericService<QuotationCompositionDetail>
{
    /// <summary>
    /// 取得指定報價明細的組合明細
    /// </summary>
    Task<List<QuotationCompositionDetail>> GetByQuotationDetailIdAsync(int quotationDetailId);
    
    /// <summary>
    /// 從商品物料清單複製 BOM 資料到報價單
    /// </summary>
    Task<List<QuotationCompositionDetail>> CopyFromProductCompositionAsync(
        int quotationDetailId, 
        int productId);
    
    /// <summary>
    /// 批次儲存組合明細（新增、更新、刪除）
    /// </summary>
    Task SaveBatchAsync(
        int quotationDetailId, 
        List<QuotationCompositionDetail> compositionDetails);
    
    /// <summary>
    /// 刪除指定報價明細的所有組合明細
    /// </summary>
    Task DeleteByQuotationDetailIdAsync(int quotationDetailId);
}
```

### 2. 實作：QuotationCompositionDetailService.cs

**檔案位置：** `Services/QuotationCompositionDetailService.cs`

**核心邏輯：**

#### CopyFromProductCompositionAsync
從商品物料清單複製 BOM 資料，但不直接儲存到資料庫：
```csharp
public async Task<List<QuotationCompositionDetail>> CopyFromProductCompositionAsync(
    int quotationDetailId, int productId)
{
    using var context = await _dbContextFactory.CreateDbContextAsync();
    
    var productCompositions = await context.ProductCompositionDetails
        .Include(p => p.ComponentProduct)
        .Include(p => p.Unit)
        .Where(p => p.ProductId == productId && p.Status == EntityStatus.Active)
        .ToListAsync();

    return productCompositions.Select(pc => new QuotationCompositionDetail
    {
        QuotationDetailId = quotationDetailId,
        ComponentProductId = pc.ComponentProductId,
        Quantity = pc.Quantity,
        UnitId = pc.UnitId,
        ComponentCost = pc.ComponentCost,
        Status = EntityStatus.Active
    }).ToList();
}
```

#### SaveBatchAsync
批次儲存，處理新增、更新、刪除：
```csharp
public async Task SaveBatchAsync(
    int quotationDetailId, 
    List<QuotationCompositionDetail> compositionDetails)
{
    using var context = await _dbContextFactory.CreateDbContextAsync();
    
    // 取得現有資料
    var existingDetails = await context.QuotationCompositionDetails
        .Where(x => x.QuotationDetailId == quotationDetailId)
        .ToListAsync();
    
    // 刪除不在新列表中的項目
    var toDelete = existingDetails
        .Where(e => !compositionDetails.Any(n => n.Id == e.Id && e.Id > 0))
        .ToList();
    context.QuotationCompositionDetails.RemoveRange(toDelete);
    
    // 新增或更新
    foreach (var detail in compositionDetails)
    {
        detail.QuotationDetailId = quotationDetailId;
        
        if (detail.Id > 0)
        {
            context.QuotationCompositionDetails.Update(detail);
        }
        else
        {
            context.QuotationCompositionDetails.Add(detail);
        }
    }
    
    await context.SaveChangesAsync();
}
```

### 3. 註冊服務

**檔案位置：** `Data/ServiceRegistration.cs`

```csharp
// 報價組合明細服務
builder.Services.AddScoped<IQuotationCompositionDetailService, QuotationCompositionDetailService>();
```

---

## 🎨 UI 層修改

### 1. 新增組件：QuotationCompositionEditModal.razor

**檔案位置：** `Components/Shared/BaseModal/Modals/QuotationCompositionEditModal.razor`

**功能：**
- 使用 `BaseModalComponent` 建立 Modal 框架
- 使用 `InteractiveTableComponent` 顯示組合明細表格
- 支援編輯數量、單位、成本、備註

**參數：**
```csharp
[Parameter] public bool IsVisible { get; set; }
[Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
[Parameter] public int? QuotationDetailId { get; set; }
[Parameter] public string ProductName { get; set; } = string.Empty;
[Parameter] public int? ProductId { get; set; }
[Parameter] public EventCallback<List<QuotationCompositionDetail>> OnSave { get; set; }
[Parameter] public EventCallback OnCancel { get; set; }
```

**表格欄位定義：**
```csharp
private List<ColumnDefinition<QuotationCompositionDetail>> GetColumnDefinitions()
{
    return new List<ColumnDefinition<QuotationCompositionDetail>>
    {
        // 組成商品（唯讀）
        new ColumnDefinition<QuotationCompositionDetail>
        {
            Header = "組成商品",
            GetDisplayValue = item => item.ComponentProduct?.Name ?? "未知商品",
            CellCssClass = "text-start",
            ColumnType = ColumnType.ReadOnly
        },
        
        // 數量（可編輯）
        new ColumnDefinition<QuotationCompositionDetail>
        {
            Header = "數量",
            PropertyName = nameof(QuotationCompositionDetail.Quantity),
            ColumnType = ColumnType.Numeric,
            Width = "120px"
        },
        
        // 單位（下拉選單）
        new ColumnDefinition<QuotationCompositionDetail>
        {
            Header = "單位",
            PropertyName = nameof(QuotationCompositionDetail.UnitId),
            ColumnType = ColumnType.Select,
            SelectOptions = Units.Select(u => new SelectOption 
            { 
                Value = u.Id.ToString(), 
                Label = u.Name 
            }).ToList(),
            Width = "120px"
        },
        
        // 成本（可編輯）
        new ColumnDefinition<QuotationCompositionDetail>
        {
            Header = "成本",
            PropertyName = nameof(QuotationCompositionDetail.ComponentCost),
            ColumnType = ColumnType.Numeric,
            Width = "120px"
        },
        
        // 備註
        new ColumnDefinition<QuotationCompositionDetail>
        {
            Header = "備註",
            PropertyName = nameof(QuotationCompositionDetail.Remarks),
            ColumnType = ColumnType.Text
        }
    };
}
```

**初始化邏輯：**
```csharp
protected override async Task OnParametersSetAsync()
{
    if (IsVisible && QuotationDetailId.HasValue && ProductId.HasValue)
    {
        await LoadCompositionDetailsAsync();
    }
}

private async Task LoadCompositionDetailsAsync()
{
    try
    {
        // 先嘗試載入已儲存的報價組合明細
        var savedDetails = await QuotationCompositionDetailService
            .GetByQuotationDetailIdAsync(QuotationDetailId.Value);
        
        if (savedDetails.Any())
        {
            compositionDetails = savedDetails;
        }
        else
        {
            // 如果沒有，從商品物料清單複製
            compositionDetails = await QuotationCompositionDetailService
                .CopyFromProductCompositionAsync(
                    QuotationDetailId.Value, 
                    ProductId.Value);
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入組合明細失敗：{ex.Message}");
        compositionDetails = new List<QuotationCompositionDetail>();
    }
}
```

### 2. 修改 QuotationTable.razor

**主要修改點：**

#### A. QuotationItem 內部類別新增屬性
```csharp
public class QuotationItem
{
    // ... 原有屬性 ...
    
    /// <summary>
    /// 自訂的 BOM 組合明細（用於暫存編輯結果）
    /// </summary>
    public List<QuotationCompositionDetail>? CustomCompositionDetails { get; set; }
}
```

#### B. 注入服務
```csharp
@inject IQuotationCompositionDetailService QuotationCompositionDetailService
```

#### C. 加入 Modal 組件
```razor
<!-- BOM 組合編輯 Modal -->
<QuotationCompositionEditModal IsVisible="@showCompositionModal"
                              IsVisibleChanged="@((bool visible) => showCompositionModal = visible)"
                              QuotationDetailId="@GetSelectedQuotationDetailId()"
                              ProductName="@selectedCompositionProductName"
                              ProductId="@selectedCompositionProductId"
                              OnSave="@HandleCompositionSave"
                              OnCancel="@(() => showCompositionModal = false)" />
```

#### D. 新增私有欄位
```csharp
// BOM 組合編輯
private bool showCompositionModal = false;
private string selectedCompositionProductName = string.Empty;
private int? selectedCompositionProductId = null;
private int? selectedQuotationItemIndex = null;
```

#### E. 修改 GetCustomActionsTemplate（加入 BOM 編輯按鈕）
```csharp
private RenderFragment<QuotationItem> GetCustomActionsTemplate => item => __builder =>
{
    var quotationItem = (QuotationItem)item;
    var isEmptyRow = quotationItem.SelectedProduct == null;
    var canDelete = DetailLockHelper.CanDeleteItem(quotationItem, out _, checkConversion: true);
    var hasComposition = quotationItem.SelectedProduct != null && 
                        HasProductComposition(quotationItem.SelectedProduct.Id);
    
    <div class="d-flex gap-1">
        @* BOM 編輯按鈕（如果商品有 BOM 組成） *@
        @if (hasComposition && !isEmptyRow)
        {
            <GenericButtonComponent Variant="ButtonVariant.Blue"
                                   IconClass="bi bi-diagram-3 text-white"
                                   Size="ButtonSize.Large"
                                   Title="編輯 BOM 組成"
                                   OnClick="async () => await ShowCompositionEditor(quotationItem)"
                                   StopPropagation="true"
                                   CssClass="btn-square" />
        }
        
        @* 原有的操作按鈕（查看相關單據、刪除等） *@
        // ...
    </div>
};
```

#### F. 新增輔助方法

**檢查商品是否有 BOM 組成：**
```csharp
/// <summary>
/// 檢查商品是否有 BOM 組成
/// </summary>
private bool HasProductComposition(int productId)
{
    // 可以從快取或服務層查詢
    // 這裡簡化為檢查 Products 中是否有相關資料
    return true; // 實際應查詢 ProductCompositionDetail
}
```

**顯示 BOM 編輯器：**
```csharp
/// <summary>
/// 顯示 BOM 組合編輯器
/// </summary>
private async Task ShowCompositionEditor(QuotationItem item)
{
    if (item.SelectedProduct == null)
        return;
    
    // 找出 QuotationItem 的索引
    selectedQuotationItemIndex = QuotationItems.IndexOf(item);
    selectedCompositionProductName = item.SelectedProduct.Name;
    selectedCompositionProductId = item.SelectedProduct.Id;
    
    showCompositionModal = true;
    StateHasChanged();
}
```

**處理 BOM 儲存：**
```csharp
/// <summary>
/// 處理 BOM 組合儲存
/// </summary>
private async Task HandleCompositionSave(List<QuotationCompositionDetail> compositionDetails)
{
    if (!selectedQuotationItemIndex.HasValue || !selectedCompositionProductId.HasValue)
        return;
        
    try
    {
        // 暫存到快取（實際儲存會在報價單儲存時一併處理）
        var item = QuotationItems[selectedQuotationItemIndex.Value];
        item.CustomCompositionDetails = compositionDetails;
        
        showCompositionModal = false;
        await NotificationService.ShowSuccessAsync("BOM 組成已更新（將在報價單儲存時一併保存）");
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"儲存 BOM 組成失敗：{ex.Message}");
    }
}
```

**取得選中的報價明細 ID：**
```csharp
/// <summary>
/// 取得選中的報價明細 ID（用於 Modal）
/// </summary>
private int? GetSelectedQuotationDetailId()
{
    if (!selectedQuotationItemIndex.HasValue)
        return null;
    
    var item = QuotationItems[selectedQuotationItemIndex.Value];
    return (item.ExistingDetailEntity as QuotationDetail)?.Id;
}
```

### 3. 修改 QuotationEditModalComponent.razor

**新增儲存組合明細的邏輯：**

在 `HandleSave` 方法中，於儲存報價明細後加入：
```csharp
private async Task HandleSave()
{
    try
    {
        // ... 原有的儲存主檔邏輯 ...
        
        // 儲存報價明細
        await SaveQuotationDetails(savedQuotation.Id);
        
        // 儲存報價組合明細（新增）
        await SaveQuotationCompositionDetails();
        
        // ... 後續邏輯 ...
    }
    catch (Exception ex)
    {
        // ...
    }
}
```

**SaveQuotationCompositionDetails 方法實作：**
```csharp
/// <summary>
/// 儲存報價組合明細
/// </summary>
private async Task SaveQuotationCompositionDetails()
{
    try
    {
        // 從 QuotationTable 取得所有 QuotationItems（透過反射或公開方法）
        var quotationItems = await quotationTableRef.GetQuotationItemsAsync();
        
        foreach (var item in quotationItems)
        {
            // 只處理有自訂組合明細的項目
            if (item.CustomCompositionDetails != null && 
                item.ExistingDetailEntity is QuotationDetail detail)
            {
                await QuotationCompositionDetailService.SaveBatchAsync(
                    detail.Id, 
                    item.CustomCompositionDetails);
            }
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"儲存組合明細失敗：{ex.Message}");
        throw;
    }
}
```

**注意：** 需要在 QuotationTable.razor 公開 `GetQuotationItemsAsync` 方法：
```csharp
/// <summary>
/// 公開方法：取得所有 QuotationItems（供父組件使用）
/// </summary>
public Task<List<QuotationItem>> GetQuotationItemsAsync()
{
    return Task.FromResult(QuotationItems);
}
```

---

## 🐛 錯誤修復記錄

### 1. Razor 編譯錯誤：Unclosed tag

**問題：** QuotationTable.razor 出現大量 "Unclosed tag" 錯誤

**原因：** GetCustomActionsTemplate 方法結尾多了一個 `};`，導致 `@code` 區塊提前結束，所有後續 C# 程式碼被當作 Razor 標記解析

**錯誤程式碼：**
```csharp
private RenderFragment<QuotationItem> GetCustomActionsTemplate => item => __builder =>
{
    // ...
};
};  // ❌ 多餘的分號
```

**修正：**
```csharp
private RenderFragment<QuotationItem> GetCustomActionsTemplate => item => __builder =>
{
    // ...
};  // ✅ 正確
```

### 2. ButtonVariant.Info 不存在

**問題：** `ButtonVariant.Info` 編譯錯誤

**原因：** `ButtonVariant` 列舉沒有 `Info` 值

**修正：** 改為使用 `ButtonVariant.Blue`
```csharp
// ❌ 錯誤
<GenericButtonComponent Variant="ButtonVariant.Info" ... />

// ✅ 正確
<GenericButtonComponent Variant="ButtonVariant.Blue" ... />
```

### 3. 服務層建構子錯誤

**問題：** QuotationCompositionDetailService 使用 `AppDbContext` 而非 `IDbContextFactory`

**原因：** 泛型服務基類期望使用 DbContext Factory

**修正：**
```csharp
// ❌ 錯誤
public QuotationCompositionDetailService(AppDbContext context)
    : base(context)

// ✅ 正確
public QuotationCompositionDetailService(IDbContextFactory<AppDbContext> dbContextFactory)
    : base(dbContextFactory)
```

### 4. HasProductComposition 無法正確檢測 BOM

**問題：** 操作欄中「編輯 BOM」按鈕不顯示，即使商品有 ProductCompositionDetail

**原因：** 
1. `HasProductComposition` 方法依賴 `Products.ProductCompositions` Navigation Property
2. `ProductService.GetAllAsync()` 預設不會 Include ProductCompositions
3. 記憶體中的 Products 列表沒有載入 BOM 相關資料

**錯誤程式碼：**
```csharp
private bool HasProductComposition(int productId)
{
    // ❌ Products 列表沒有 Include ProductCompositions
    return Products.Any(p => p.Id == productId && p.ProductCompositions?.Any() == true);
}
```

**修正方案：**

**A. 新增快取欄位：**
```csharp
// 快取有 BOM 組成的商品 ID
private HashSet<int> productsWithComposition = new();
```

**B. 注入服務：**
```razor
@inject IProductCompositionService ProductCompositionService
```

**C. 初始化時載入 BOM 商品列表：**
```csharp
protected override async Task OnInitializedAsync()
{
    // ... 原有邏輯 ...
    
    // 載入有 BOM 組成的商品列表
    await LoadProductCompositionsAsync();
}

/// <summary>
/// 載入有 BOM 組成的商品列表（用於快取檢查）
/// </summary>
private async Task LoadProductCompositionsAsync()
{
    try
    {
        // 從 Products 參數中取得所有商品 ID
        var productIds = Products.Where(p => p.Id > 0).Select(p => p.Id).ToList();
        
        // 檢查每個商品是否有 ProductComposition
        foreach (var productId in productIds)
        {
            var compositions = await ProductCompositionService
                .GetCompositionsByProductIdAsync(productId);
            if (compositions?.Any() == true)
            {
                productsWithComposition.Add(productId);
            }
        }
    }
    catch (Exception ex)
    {
        // 載入失敗不影響主要功能，只是 BOM 按鈕可能不顯示
        Console.WriteLine($"載入商品組成資料失敗：{ex.Message}");
        productsWithComposition.Clear();
    }
}
```

**D. 簡化檢查方法：**
```csharp
private bool HasProductComposition(int productId)
{
    // ✅ 直接使用快取檢查
    return productsWithComposition.Contains(productId);
}
```

**優點：**
- ✅ 不需要修改 ProductService.GetAllAsync() 的查詢邏輯
- ✅ 不會載入不必要的 Navigation Property（節省記憶體）
- ✅ 使用 HashSet 提供 O(1) 查詢效能
- ✅ 在初始化時一次性載入，避免重複查詢

---

## 📊 資料流程圖

```
使用者操作
    ↓
點擊「編輯 BOM」按鈕
    ↓
QuotationTable.ShowCompositionEditor()
    ↓
開啟 QuotationCompositionEditModal
    ↓
載入資料：
  - 已有報價組合明細？→ 載入現有資料
  - 沒有？→ 從 ProductCompositionDetail 複製
    ↓
使用者編輯（InteractiveTableComponent）
    ↓
點擊「儲存」
    ↓
觸發 OnSave → HandleCompositionSave()
    ↓
暫存到 QuotationItem.CustomCompositionDetails
    ↓
關閉 Modal
    ↓
使用者點擊報價單「儲存」
    ↓
QuotationEditModalComponent.HandleSave()
    ↓
SaveQuotationCompositionDetails()
    ↓
呼叫 QuotationCompositionDetailService.SaveBatchAsync()
    ↓
寫入資料庫 QuotationCompositionDetails
```

---

## 🔍 測試檢查清單

### 功能測試
- [ ] 新增報價單，選擇有 BOM 的商品
- [ ] 點擊「編輯 BOM」按鈕，Modal 正常開啟
- [ ] 第一次開啟時，顯示從商品物料清單複製的資料
- [ ] 修改數量、單位、成本、備註
- [ ] 儲存後，資料暫存到 QuotationItem
- [ ] 儲存報價單後，資料寫入 QuotationCompositionDetails 資料表
- [ ] 再次開啟該報價單，編輯 BOM，應載入已儲存的資料
- [ ] 確認商品物料清單資料未被修改

### 邊界測試
- [ ] 商品沒有 BOM 組成時，不顯示「編輯 BOM」按鈕
- [ ] 空行不顯示「編輯 BOM」按鈕
- [ ] 唯讀模式下，可查看但不可編輯 BOM
- [ ] 刪除報價明細時，相關組合明細一併刪除（級聯刪除）

### 錯誤處理
- [ ] 載入失敗時顯示錯誤訊息
- [ ] 儲存失敗時顯示錯誤訊息
- [ ] 資料驗證失敗時顯示提示

---

## 📝 後續優化建議

### 1. 效能優化
- 在 `HasProductComposition` 方法中加入快取機制，避免重複查詢
- 使用 `IMemoryCache` 快取商品物料清單資料

### 2. 功能增強
- 支援從 BOM 編輯器中新增組成項目（目前僅能編輯現有項目）
- 顯示 BOM 成本小計
- 支援批次匯入 BOM 資料

### 3. UI 改善
- 在報價明細表格中，加入 BOM 圖示提示
- Tooltip 顯示 BOM 組成摘要
- 支援鍵盤快捷鍵（如 Ctrl+B 開啟 BOM 編輯器）

### 4. 資料驗證
- 驗證組成數量必須大於 0
- 驗證組成商品不能與主商品相同（避免循環參照）
- 驗證單位必須與商品相容

---

## 📚 相關文檔

- [README_互動Table說明.md](./README_互動Table說明.md) - InteractiveTableComponent 使用說明
- [README_Services.md](./README_Services.md) - 服務層架構說明
- [README_Data.md](./README_Data.md) - 資料層設計說明

---

## 👨‍💻 開發者備註

**修改日期：** 2025年12月1日  
**修改內容：** 新增報價單 BOM 組成編輯功能  
**影響範圍：** 
- 資料層：QuotationCompositionDetail, QuotationDetail, AppDbContext
- 服務層：IQuotationCompositionDetailService, QuotationCompositionDetailService
- UI 層：QuotationCompositionEditModal, QuotationTable, QuotationEditModalComponent
- 資料庫：新增 QuotationCompositionDetails 資料表

**相容性：** 
- 不影響現有報價單功能
- 向下相容（舊報價單不會有組合明細，不影響正常使用）

**注意事項：**
1. 確保 Migration 已套用（AddQuotationCompositionDetail）
2. 服務已在 ServiceRegistration 中註冊
3. 編輯 BOM 需要商品先有 ProductCompositionDetail 資料
