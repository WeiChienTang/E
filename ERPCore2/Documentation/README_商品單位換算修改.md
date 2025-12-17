# 商品單位換算修改說明

## 📋 問題描述

### 業務場景
在實際業務中，商品的**進貨單位**與**製程使用單位**經常不一致：

- **進貨單位**：以「包」為單位採購和入庫（如：1包 = 30公斤）
- **製程單位**：生產時以「公斤」計算消耗
- **需求計算**：產品A需要2公斤，但庫存記錄顯示5包

### 核心需求
**首要目標**：建立單位換算管理介面，讓使用者可以設定單位之間的換算關係（如：1包 = 30公斤）

**後續應用**：
1. 訂單庫存檢查時自動換算單位
2. BOM 計算支援跨單位需求
3. 製程領料單位自動轉換
4. 數量顯示支援雙單位

---

## ✅ 現有架構分析

### 已有的基礎設施

#### 1. UnitConversion 實體（單位換算表）
```csharp
public class UnitConversion : BaseEntity
{
    [Column(TypeName = "decimal(18,6)")]
    public decimal ConversionRate { get; set; }    // 轉換比例（如：30）
    
    public bool IsActive { get; set; }             // 是否啟用
    
    public int FromUnitId { get; set; }            // 來源單位（包）
    public int ToUnitId { get; set; }              // 目標單位（公斤）
    
    public Unit FromUnit { get; set; }
    public Unit ToUnit { get; set; }
}
```

**特性：**
- ✅ 支援雙向轉換（包→公斤、公斤→包）
- ✅ 精度高達 6 位小數
- ✅ 可啟用/停用控制
- ✅ 唯一索引：`[Index(nameof(FromUnitId), nameof(ToUnitId), IsUnique = true)]`

#### 2. Product 實體（商品表）
```csharp
public class Product : BaseEntity
{
    [ForeignKey(nameof(Unit))]
    public int? UnitId { get; set; }               // 庫存/進貨單位
    
    public Unit? Unit { get; set; }
    // ... 其他屬性
}
```

**現狀：**
- ✅ 有基礎單位欄位
- ❌ 缺少製程單位欄位
- ❌ 無法表達單位換算需求

---

## 🎯 解決方案設計

### Phase 1: 單位換算管理介面（本次實作）

**目標**：建立單位換算管理 Modal，讓使用者能夠：
- ✅ 新增換算規則（如：1包 = 30公斤）
- ✅ 檢視所有換算規則
- ✅ 啟用/停用換算規則
- ✅ 刪除未使用的換算規則

**設計原則**：
1. **單向儲存**：只儲存一個方向（包→公斤），反向計算時除法運算
2. **集中管理**：所有換算規則統一在 `UnitConversion` 表維護
3. **安全刪除**：檢查是否有產品使用中，防止誤刪
4. **簡單實用**：MVP 版本，聚焦核心功能

### Phase 2（Phase 1 無需修改）

### 現有資料表已足夠

#### UnitConversion 表（已存在）
```csharp
public class UnitConversion : BaseEntity
{
    [Column(TypeName = "decimal(18,6)")]
    public decimal ConversionRate { get; set; }    // 轉換比例（如：30）
    
    public bool IsActive { get; set; }             // 是否啟用
    
    public int FromUnitId { get; set; }            // 來源單位（包）
    public int ToUnitId { get; set; }              // 目標單位（公斤）
    
    public Unit FromUnit { get; set; }
    public Unit ToUnit { get; set; }
}
```

**優點**：
- ✅ 表結構完整，無需 Migration
- ✅ 唯一索引防止重複規則
- ✅ IsActive 支援啟用/停用
- ✅ 高精度 decimal(18,6)

### Phase 2 才需要的修改（暫緩）

未來擴充 `Product` 表時才需要新增：
- `ProductionUnitId`：製程單位欄位
- `CustomConversionRate`：產品自訂換算係數

**本次實作不涉及 Product 表修改** column: "ProductionUnitId");

migrationBuilder.AddForeignKey(
    name: "FK_Products_Units_ProductionUnitId",
    table: "Products",
    column: "ProductionUnitId",
    principalTable: "Units",
    principalColumn: "Id");
```

---

## 🔧 服務層實作

### 1. IUnitConversionService（新建服務）

```csharp
public interface IUnitConversionService : IBaseService<UnitConversion>
{
    /// <summary>
    /// 取得兩個單位之間的換算係數
    /// </summary>
    /// <param name="fromUnitId">來源單位ID</param>
    /// <param name="toUnitId">目標單位ID</param>
    /// <returns>換算係數，如果沒有則返回 null</returns>
    Task<decimal?> GetConversionRateAsync(int fromUnitId, int toUnitId);
    
    /// <summary>
    /// 換算數量（從來源單位轉換到目標單位）
    /// </summary>
    /// <param name="quantity">數量</param>
    /// <param name="fromUnitId">來源單位ID</param>
    /// <param name="toUnitId">目標單位ID</param>
    /// <returns>換算後的數量</returns>
    Task<decimal?> ConvertQuantityAsync(decimal quantity, int fromUnitId, int toUnitId);
    
    /// <summary>
    /// 取得產品的單位換算係數（優先使用產品自訂，其次使用全域規則）
    /// </summary>
    /// <param name="productId">產品ID</param>
    /// <param name="targetUnitId">目標單位ID（如果為 null，使用產品的製程單位）</param>
    /// <returns>換算係數</returns>
    Task<decimal?> GetProductConversionRateAsync(int productId, int? targetUnitId = null);
    
    /// <summary>
    /// 將產品數量換算成製程單位
    /// </summary>
    /// <param name="productId">產品ID</param>
    /// <pa（簡化版）

### 1. IUnitConversionService（新建服務）

```csharp
namespace ERPCore2.Services.Products
{
    public interface IUnitConversionService : IBaseService<UnitConversion>
    {
        /// <summary>
        /// 取得所有換算規則（含單位資訊）- 用於 Modal 顯示
        /// </summary>
        Task<List<UnitConversionDto>> GetAllWithUnitsAsync();
        
        /// <summary>
        /// 檢查是否可以刪除（檢查是否有產品使用）
        /// </summary>
        Task<ServiceResult> CanDeleteAsync(int unitConversionId);
        
        /// <summary>
        /// 切換啟用狀態
        /// </summary>
        Task<ServiceResult> ToggleActiveAsync(int unitConversionId);
        
        /// <summary>
        /// 驗證換算規則（防止重複、來源=目標等）
        /// </summary>
        Task<ServiceResult> ValidateConversionAsync(int fromUnitId, int toUnitId, int? excludeId = null);
    }
    
    /// <summary>
    /// 單位換算 DTO（用於顯示）
    /// </summary>
    public class UnitConversionDto
    {
        public int Id { get; set; }
        public int FromUnitId { get; set; }
        public string FromUnitName { get; set; } = string.Empty;
        public string FromUnitCode { get; set; } = string.Empty;
        public int ToUnitId { get; set; }
        public string ToUnitName { get; set; } = string.Empty;
        public string ToUnitCode { get; set; } = string.Empty;
        public decimal ConversionRate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 顯示用文字: "1 包 = 30 公斤"
        /// </summary>
        public string DisplayText => $"1 {FromUnitName} = {ConversionRate:N2} {ToUnitName}";
    }
}
```

### 2. UnitConversionService 實作（關鍵邏輯）

```csharp
public class UnitConversionService : BaseService<UnitConversion>, IUnitConversionService
{
    public async Task<List<UnitConversionDto>> GetAllWithUnitsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        return await context.UnitConversions
            .Include(uc => uc.FromUnit)
            .Include(uc => uc.ToUnit)
            .OrderByDescending(uc => uc.IsActive)
            .ThenBy(uc => uc.FromUnit.Name)
            .Select(uc => new UnitConversionDto
            {
                Id = uc.Id,
                FromUnitId = uc.FromUnitId,
                FromUnitName = uc.FromUnit.Name,
                FromUnitCode = uc.FromUnit.Code,
                ToUnitId = uc.ToUnitId,
                ToUnitName = uc.ToUnit.Name,
                ToUnitCode = uc.ToUnit.Code,
                ConversionRate = uc.ConversionRate,
                IsActive = uc.IsActive,
                CreatedAt = uc.CreatedAt
            })
            .ToListAsync();
    }
    
    public async Task<ServiceResult> CanDeleteAsync(int unitConversionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var conversion = await context.UnitConversions
            .Include(uc => uc.FromUnit)
            .Include(uc => uc.ToUnit)
            .FirstOrDefaultAsync(uc => uc.Id == unitConversionId);
            
        if (conversion == null)
            return ServiceResult.Failure("找不到此換算規則");
        
        // 檢查是否有產品使用此換算（Phase 2 才會有 CustomConversionRate）
        // 目前僅做基本檢查
        
        return ServiceResult.Success();
    }
    
    public async Task<ServiceResult> ToggleActiveAsync(int unitConversionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var conversion = await context.UnitConversions.FindAsync(unitConversionId);
        if (conversion == null)
            return ServiceResult.Failure("找不到此換算規則");
            
        conversion.IsActive = !conversion.IsActive;
        conversion.UpdatedAt = DateTime.Now;
        
        await context.SaveChangesAsync();
        
        return ServiceResult.Success();
    }
    
    public async Task<ServiceResult> ValidateConversionAsync(int fromUnitId, int toUnitId, int? excludeId = null)
    {
        // 1. 檢查來源和目標不能相同
        if (fromUnitId == toUnitId)
            return ServiceResult.Failure("來源單位與目標單位不能相同");
        
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // 2. 檢查是否已存在相同的換算規則
        var exists = await context.UnitConversions
            .AnyAsync(uc => 
                uc.FromUnitId == fromUnitId && 
                uc.ToUnitId == toUnitId &&
                (excludeId == null || uc.Id != excludeId.Value));
                
        if (exists)
        {
            var fromUnit = await context.Units.FindAsync(fromUnitId);
            var toUnit = await context.Units.FindAsync(toUnitId);
            return ServiceResult.Failure(
                $"換算規則已存在: {fromUnit?.Name} → {toUnit?.Name}");
        }
        
        return ServiceResult.Success();
    }
}
```

### 3. Phase 2 擴充方法（暫不實作）

以下方法將在 Phase 2（產品單位擴充）時才需要：
- `GetConversionRateAsync()` - 取得兩單位間換算係數
- `ConvertQuantityAsync()` - 換算數量
- `GetProductConversionRateAsync()` - 取得產品換算係數
- `ConvertToProductionUnitAsync()` - 轉換為製程單位
- `ConvertToStockUnitAsync()` - 轉換為庫存單位                    ?? product?.Unit?.Name 
                       ?? detail.Unit?.Name 
                       ?? "";
    
    // 3. 換算需求數量到比較單位
    var requiredInCompareUnit = await _unitConversionService.ConvertQuantityAsync(
        detail.OrderQuantity, 
        detail.UnitId ?? product.UnitId.Value, 
        compareUnitId.Value
    ) ?? detail.OrderQuantity;
    
    // 4. 換算庫存到比較單位
    var availableInCompareUnit = await _unitConversionService.ConvertQuantityAsync(
        stockInBaseUnit, 
        product.UnitId.Value, 
        compareUnitId.Value
    ) ?? stockInBaseUnit;
    
    var item = new OrderInventoryCheckItem
    {
        // ... 其他屬性
        UnitName = compareUnitName,
        RequiredQuantity = requiredInCompareUnit,
        AvailableStock = availableInCompareUnit,
        
        // 新增：原始數量資訊（用於顯示）
   🎨 UI 設計（表格式 + RenderTreeBuilder）

### Modal 整體結構

```
╔══════════════════════════════════════════════════════════════════╗
║  單位換算管理                                        [+新增] [X] ║
╠══════════════════════════════════════════════════════════════════╣
║  [新增表單區 - 可收合]                                           ║
║  ┌────────────────────────────────────────────────────────────┐ ║
║  │ 來源單位: [包 ▾]  →  目標單位: [公斤 ▾]  係數: [30.00]    │ ║
║  │ ☑ 啟用          [取消] [確認新增]                          │ ║
║  └────────────────────────────────────────────────────────────┘ ║
║                                                                  ║
║  [列表區]                                                        ║
║  ┌────────────────────────────────────────────────────────────┐ ║
║  │ 來源單位 │  →  │ 目標單位 │ 換算係數 │  狀態  │   操作    │ ║
║  ├────────────────────────────────────────────────────────────┤ ║
║  │ 包       │  →  │ 公斤     │  30.00   │ ✓啟用  │ ☑停用 🗑️ │ ║
║  │ 箱       │  →  │ 個       │  12.00   │ ✓啟用  │ ☑停用 🗑️ │ ║
║  │ 噸       │  →  │ 公斤     │ 1000.00  │ ⊗停用  │ ☑啟用 🗑️ │ ║
║  └────────────────────────────────────────────────────────────┘ ║
║                                                                  ║
║  💡 說明: 1個來源單位 = N個目標單位                             ║
║                                                      [關閉]      ║
╚══════════════════════════════════════════════════════════════════╝
```

### 關鍵實作點

1. **使用 RenderTreeBuilder**：與現有 Modal 一致（如 OrderInventoryCheckModal）
2. **新增表單內嵌**：點擊「新增」按鈕後，在列表上方展開表單
3. **即時驗證**：選擇單位時檢查是否重複
4. **操作按鈕**：啟用/停用 + 刪除（檢查使用中）
5. **排序**：啟用優先，然後按單位名稱

### 元件結構

```
UnitConversionManagementModal.razor
├── BaseModalComponent
│   ├── BodyContent (RenderFragment)
│   │   ├── RenderAddForm() - 新增表單區
│   │   └── RenderConversionList() - 列表區
│   │       └── RenderConversionRow() - 單筆資料行
│   └── FooterContent (RenderFragment)
│       ├── 左側：新增按鈕
│       └── 右側：關閉按鈕
```

---

## 📊 業務邏輯調整（Phase 2）inalRequiredQuantity = detail.OrderQuantity,
        OriginalRequiredUnitName = detail.Unit?.Name,
        OriginalStockQuantity = stockInBaseUnit,
        OriginalStockUnitName = product?.Unit?.Name,
        
        Status = DetermineInventoryStatus(requiredInCompareUnit, availableInCompareUnit, product)
    };
    
    return item;
}
```

### 2. BOM 計算（ProductComposition）

```csharp
// 計算 BOM 總需求時，考慮單位換算
public async Task<decimal> CalculateComponentRequirementAsync(
    int parentProductId, 
    decimal parentQuantity,
    int componentProductId)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    
    var composition = await context.ProductCompositionDetails
        .Include(cd => cd.ComponentProduct)
        .FirstOrDefaultAsync(cd => 
            cd.ProductComposition.ParentProductId == parentProductId &&
            cd.ComponentProductId == componentProductId);
            
    if (composition == null)
        return 0;
        
    // 1. BOM 配方中的數量（可能已經是製程單位）
    var componentQtyPerUnit = composition.Quantity;
    
    // 2. 計算總需求
    var totalRequired = componentQtyPerUnit * parentQuantity;
    
    // 3. 如果 BOM 單位與元件庫存單位不同，需要換算
    var component = composition.ComponentProduct;
    if (composition.UnitId.HasValue && 
        component.UnitId.HasValue && 
        composition.UnitId != component.UnitId)
    {
        var converted = await _unitConversionService.ConvertQuantityAsync(
            totalRequired,
            composition.UnitId.Value,
            component.UnitId.Value
        );
        
        return converted ?? totalRequired;
    }
    
    return totalRequired;
}
```

### 3. 領料單生成（MaterialIssue）

```csharp
// 生成領料單時，使用製程單位
public async Task<ServiceResult<MaterialIssue>> CreateMaterialIssueFromProductionOrderAsync(
    int productionOrderId)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    
    var productionOrder = await context.ProductionOrders
        .Include(po => po.ProductionOrderDetails)
            .ThenInclude(pod => pod.Product)
                .ThenInclude(p => p.ProductionUnit)
        .FirstOrDefaultAsync(po => po.Id == productionOrderId);
        
    if (productionOrder == null)
        return ServiceResult<MaterialIssue>.Failure("找不到生產單");
        
    var materialIssue = new MaterialIssue
    {
        // ... 基本資訊
    };
    
    foreach (var detail in productionOrder.ProductionOrderDetails)
    {
        // 取得 BOM 元件
        var components = await GetBOMComponentsAsync(detail.ProductId);
        
        foreach (var component in components)
        {
            /（Phase 2）

**Phase 1 不涉及既有 UI 修改**，僅建立新的換算管理 Modal。

### 1. OrderInventoryCheckModal 雙單位顯示 - Phase 2
                detail.Quantity,
                component.ComponentProductId
            );
            
            // 取得製程單位
            var unitId = component.ComponentProduct.ProductionUnitId 
                      ?? component.ComponentProduct.UnitId;
            var unitName = component.ComponentProduct.ProductionUnit?.Name 
                        ?? component.ComponentProduct.Unit?.Name;
            
            materialIssue.MaterialIssueDetails.Add(new MaterialIssueDetail
            {
                ProductId = component.ComponentProductId,
                Quantity = requiredQty,
                UnitId = unitId,
                UnitName = unitName,
                // ... 其他屬性
            });
        }
    }
    
    return ServiceResult<MaterialIssue>.Success(materialIssue);
}
```

---

## 🎨 UI 顯示優化

### 1. OrderInventoryCheckModal 雙單位顯示

```razor
@* 顯示需求和庫存（附帶原始單位資訊）*@
<div class="row g-2 small">
    <div class="col-auto">
        <span>需求: </span>
        <strong>@item.RequiredQuantity.ToString("N2") @item.UnitName</strong>
        @if (!string.IsNullOrEmpty(item.OriginalRequiredUnitName) && 
             item.OriginalRequiredUnitName != item.UnitName)
        {
            <span class="text-muted ms-1">
                (@item.OriginalRequiredQuantity.ToString("N2") @item.OriginalRequiredUnitName)
            </span>
        }
    </div>
    
    <div class="col-auto">
        <span>庫存: </span>
        <strong class="@item.StatusClass">
            @item.AvailableStock.ToString("N2") @item.UnitName
        </strong>
        @if (!string.IsNullOrEmpty(item.OriginalStockUnitName) && 
             item.OriginalStockUnitName != item.UnitName)
        {
            <span class="text-muted ms-1">
                (@item.OriginalStockQuantity.ToString("N2") @item.OriginalStockUnitName)
            </span>
        }
    </div>
</div>
```

**顯示效果：**
```
需求: 2.00 公斤
庫存: 150.00 公斤 (5.00 包)
```

### 2. OrderInventoryCheckModels 擴充

```csharp
public class OrderInventoryCheckItem
{
    // ... 現有屬性
    
    // === 新增：原始單位資訊 ===
    （Phase 1）

### 測試案例 1：新增換算規則

```csharp
[Test]
public async Task AddConversion_ShouldSucceed_WhenValid()
{
    // Arrange
    var unitPackage = new Unit { Id = 1, Code = "PKG", Name = "包" };
    var unitKg = new Unit { Id = 2, Code = "KG", Name = "公斤" };
    
    var conversion = new UnitConversion
    {
        FromUnitId = unitPackage.Id,
        ToUnitId = unitKg.Id,
        ConversionRate = 30m,
        IsActive = true
    };
    
    // Act
    var result = await _unitConversionService.AddAsync(conversion);
    
    // Assert
    Assert.IsTrue(result.IsSuccess);
}
```

### 測試案例 2：驗證重複規則

```csharp
[Test]
public async Task ValidateConversion_ShouldFail_WhenDuplicate()
{
    // Arrange
    var fromUnitId = 1; // 包
    var toUnitId = 2;   // 公斤
    
    // 已存在規則
    await _unitConversionService.AddAsync(new UnitConversion
    {
        FromUnitId = fromUnitId,
        ToUnitId = toUnitId,
        ConversionRate = 30m,
        IsActive = true
    });
    
    // Act
    var result = await _unitConversionService.ValidateConversionAsync(fromUnitId, toUnitId);
    
    // Assert
    Assert.IsFalse(result.IsSuccess);
    Assert.IsTrue(result.Message.Contains("已存在"));
}
```

### 測試案例 3：切換啟用狀態

```csharp
[Test]
public async Task ToggleActive_ShouldChangeStatus()
{
    // Arrange
    var conversion = new UnitConversion
    {
        FromUnitId = 1,
        ToUnitId = 2,
        ConversionRate = 30m,
        IsActive = true
    };
    var addResult = await _unitConversionService.AddAsync(conversion);
    
    // Act
    var toggleResult = await _unitConversionService.ToggleActiveAsync(conversion.Id);
    
    // Assert
    Assert.IsTrue(toggleResult.IsSuccess);
    var updated = await _unitConversionService.GetByIdAsync(conversion.Id);
    Assert.IsFalse(updated.IsActive);
}
```

### 測試案例 4：檢查刪除權限

```csharp
[Test]
public async Task CanDelete_ShouldSucceed_WhenNotInUse()
{
    // Arrange
    var conversion = new UnitConversion
    {
        FromUnitId = 1,
        ToUnitId = 2,
        ConversionRate = 30m,
        IsActive = true
    };
    await _unitConversionService.AddAsync(conversion);
    
    // Act
    var result = await _unitConversionService.CanDeleteAsync(conversion.Id);
    
    // Assert
    Assert.IsTrue(result.IsSuccess);
    // Act
    var result = await _salesOrderService.GetOrderInventoryCheckAsync(salesOrder.Id);
    
    // Assert
    var item = result.Items.First();
    Assert.AreEqual(2m, item.RequiredQuantity);        // 需求：2公斤
    Assert.AreEqual(150m, item.AvailableStock);        // 庫存：150公斤
    Assert.AreEqual(InventoryStatus.Sufficient, item.Status); // 充足
}
```

### 測試案例 3：自訂換算係數

```csharp
[Test]
public async Task CustomConversionRate_ShouldOverride_GlobalRate()
{（Phase 1）

### Step 1：服務層
- [ ] 建立 `Services/Products/IUnitConversionService.cs`
- [ ] 建立 `Services/Products/UnitConversionService.cs`
- [ ] 建立 `UnitConversionDto` 類別
- [ ] 修改 `Data/ServiceRegistration.cs` 註冊服務

### Step 2：UI 層（Modal）
- [ ] 建立 `Components/Shared/BaseModal/Modals/System/UnitConversionManagementModal.razor`
- [ ] 實作 RenderAddForm（新增表單）
- [ ] 實作 RenderConversionList（列表）
- [ ] 實作 RenderConversionRow（單筆資料行）
- [ ] 實作事件處理：新增、刪除、啟停

### Step 3：導航整合
- [ ] 修改 `Data/Navigation/NavigationConfig.cs` 新增 Action 項目
- [ ] 修改 `Components/Layout/MainLayout.razor` 註冊 Modal
- [ ] 修改 `Components/Layout/MainLayout.razor` 新增 Action Handler

### Step 4：測試
- [ ] 單元測試：驗證重複規則
- [ ] 單元測試：切換啟用狀態
- [ ] 單元測試：檢查刪除權限
- [ ] UI 測試：Modal 開啟/關閉
- [ ] UI 測試：新增換算規則
- [ ] UI 測試：刪除換算規則

---

## 📝 未來擴充檢查清單（Phase 2）

### Phase 2A：資料表擴充
- [ ] Product 表新增 `ProductionUnitId` 欄位
- [ ] Product 表新增 `CustomConversionRate` 欄位
- [ ] 建立 Migration 並執行

### Phase 2B：服務層擴充
- [ ] `IUnitConversionService` 新增換算方法
- [ ] `IIn（Phase 1）

### 1. 資料驗證
- **來源 ≠ 目標*（Phase 3+）

### Phase 3A：編輯功能
- **編輯換算係數**：允許修改現有規則的係數
- **批次啟停**：一次啟用/停用多個規則
- **搜尋過濾**：按單位名稱或狀態過濾

### Phase 3B：進階驗證
- **循環檢測**：防止 A→B, B→C, C→A 的循環換算
- **合理性檢查**：警告過大或過小的係數（如：1包=10000公斤）
- **使用提醒**：刪除前顯示有哪些產品使用此換算

### Phase 3C：多層次換算
- **鏈式換算**：支援「噸 → 公斤 → 克」的自動轉換
- **換算路徑**：使用 Dijkstra 演算法尋找最短路徑
- **換算樹**：視覺化單位換算關係

### Phase 3D：歷史記錄
- **變更記錄**：記錄換算係數的修改歷史
- **審計追蹤**：誰在何時修改了哪個換算規則
- **版本管理**：支援換算規則的版本回溯

### Phase 3E：智慧功能
- **單位建議**：根據產品類別推薦製程單位
- **換算測試**：即時換算計算器（輸入數量，顯示換算結果）
- **匯入匯出**：批次匯入常用換算規則（Excel）
### 2. 效能考量
- **快取換算規則**：頻繁查詢的換算係數可以快取
- **批次查詢**：避免在迴圈中逐個查詢換算規則
- **資料庫索引**：確保 UnitConversion 表有適當索引

### 3. 業務規則
- **換算精度**：使用 `decimal(18,6)` 確保精度
- **四捨五入**：顯示時才四捨五入，計算時保持完整精度
- **零除錯誤**：換算時檢查 `ConversionRate > 0`
- **循環換算**：避免 A→B→C→A 的循環換算定義

### 4. 使用者體驗
- **單位顯示**：同時顯示換算前後的單位，讓使用者清楚理解
- **換算提示**：在 UI 上明確標示「已換算」或顯示換算公式
- **錯誤處理**：換算失敗時，使用原始單位並給予警告

---

## 🔄 後續擴充建議

### 1. 多層次單位換算
支援「噸 → 公斤 → 克」的鏈式換算：
```csharp
public async Task<decimal?> ConvertQuantityChainAsync(
    decimal quantity, 
    int fromUnitId, 
    int toUnitId)
{
    // 使用 Dijkstra 演算法尋找最短換算路徑
    // 或使用預先建立的單位換算樹
}
```

### 2. 單位換算歷史記錄
記錄換算係數的變更歷史，用於追溯：
```cPhase 1 核心成果
1. ✅ **UnitConversionManagementModal**：單位換算管理介面
2. ✅ **UnitConversionService**：換算規則 CRUD 服務
3. ✅ **NavigationConfig 整合**：透過選單 Action 開啟
4. ✅ **驗證機制**：防止重複、來源=目標等錯誤

### 設計優勢
- 🎯 **聚焦 MVP**：先建立管理介面，後續才整合業務邏輯
- 🔒 **安全刪除**：檢查使用中，避免影響現有資料
- 📊 **單向儲存**：避免資料重複，計算時反向運算
- 🚀 **易於擴充**：Phase 2 可以無縫整合到產品和業務邏輯

### 實作範圍
- ✅ **新增換算規則**
## 🎯 實作決策記錄

| 項目 | 決定 | 理由 |
|------|------|------|
| **UI 實作** | 表格式 + RenderTreeBuilder | 與現有 Modal 一致、效能較好 |
| **資料儲存** | 單向（1包=30公斤） | 避免重複、計算時反向運算 |
| **刪除檢查** | 嚴格檢查使用中 | 避免影響現有產品（Phase 2） |
| **權限控制** | `Unit.Read` | 無需新權限，與單位管理綁定 |
| **預設資料** | 無 | 由使用者自行建立 |
| **功能範圍** | 新增、刪除、啟停 | MVP - 簡單實用 |
| **架構設計** | 分階段實作 | Phase 1 管理介面、Phase 2 業務整合 |

---

**文件版本**：v2.0 (Phase 1)  
**建立日期**：2025-12-16  
**最後更新**：2025-12-16  
**作者**：GitHub Copilot  
**階段**：Phase 1 - 單位換算管理介面
- ❌ 業務邏輯調整（Phase 2）
- ❌ 既有 UI 修改（Phase 2）
### 3. 智慧單位建議
根據產品類別自動建議適合的製程單位：
```csharp
public async Task<Unit?> SuggestProductionUnitAsync(int productId)
{
    // 根據產品分類、歷史資料等智慧推薦
}
```

### 4. 單位換算驗證器
在儲存前驗證換算設定的合理性：
```csharp
public async Task<ServiceResult> ValidateUnitConversionAsync(UnitConversion conversion)
{
    // 檢查是否會產生循環換算
    // 檢查換算係數是否合理（如：不應該 < 0）
    // 檢查是否與現有規則衝突
}
```

---

## 📚 相關文件

- [README_商品資料表重構.md](./README_商品資料表重構.md) - 產品表結構設計
- [README_庫存異動正確撰寫方式.md](./README_庫存異動正確撰寫方式.md) - 庫存計算邏輯
- [README_訂單庫存檢查修改.md](./README_訂單庫存檢查修改.md) - 訂單檢查機制
- [README_商品排程製作.md](./README_商品排程製作.md) - BOM 與生產排程

---

## ✅ 總結

### 核心改變
1. ✅ **Product 表**：新增 `ProductionUnitId` 和 `CustomConversionRate`
2. ✅ **UnitConversionService**：統一管理所有單位換算邏輯
3. ✅ **業務邏輯**：訂單檢查、BOM 計算、領料單都支援單位換算
4. ✅ **UI 顯示**：雙單位顯示，讓使用者清楚理解換算

### 優勢
- 🎯 **靈活性**：支援全域規則和產品自訂
- 🔒 **資料完整性**：庫存依然以原始單位儲存，不影響歷史資料
- 📊 **可追蹤性**：保留原始單位資訊，便於審計
- 🚀 **可擴充性**：易於擴展多層次換算、單位組等功能

---

**文件版本**：v1.0  
**建立日期**：2025-12-16  
**最後更新**：2025-12-16  
**作者**：GitHub Copilot
