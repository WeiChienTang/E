# 廠商綁定商品功能設計文件

## 📋 文件資訊
- **建立日期**: 2025/12/11
- **功能目的**: 建立商品與供應商的關聯機制，實現從低庫存警戒直接尋找供應商進貨
- **採用方案**: 混合方案（主綁定 + 輔助歷史）

---

## 🎯 需求背景

### 客戶需求
當商品庫存低於警戒線時，使用者希望能夠：
1. 快速找到可供貨的廠商
2. 直接開啟採購單並預填商品資訊
3. 查看歷史採購價格和交易記錄

### 現況分析
- ✅ 已有完整的採購單歷史資料 (`PurchaseOrder` + `PurchaseOrderDetail`)
- ✅ 已有低庫存警戒檢視功能 (`StockAlertViewModalComponent`)
- ❌ 商品與供應商之間沒有直接關聯
- ❌ 無法快速找到供應商進行採購

---

## 💡 方案設計：混合策略

### 核心概念
**主要機制（綁定）** + **輔助機制（歷史）** 相結合

### 推薦邏輯
```
當商品低於庫存警戒線時：
┌─────────────────────────────┐
│ 1. 優先顯示：主要供應商      │ ← ProductSupplier (IsPrimary = true)
├─────────────────────────────┤
│ 2. 其次顯示：其他綁定供應商  │ ← ProductSupplier (IsPrimary = false)
├─────────────────────────────┤
│ 3. 參考顯示：歷史採購廠商    │ ← PurchaseOrderDetail 統計
├─────────────────────────────┤
│ 4. 提示：尚未設定供應商      │ ← 引導使用者建立綁定
└─────────────────────────────┘
```

---

## 🗄️ 資料表設計

### 新增資料表：ProductSupplier（簡化版）

```csharp
/// <summary>
/// 商品-供應商關聯表
/// 維護商品與供應商之間的採購關係
/// </summary>
public class ProductSupplier : BaseEntity
{
    // ===== 關聯欄位 =====
    
    /// <summary>
    /// 商品ID
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// 供應商ID
    /// </summary>
    public int SupplierId { get; set; }
    
    // ===== 供應商優先順序 =====
    
    /// <summary>
    /// 是否為常用供應商（可以有多個常用供應商）
    /// 用於在推薦清單中優先顯示
    /// </summary>
    public bool IsPreferred { get; set; } = false;
    
    /// <summary>
    /// 優先順序（數字越小越優先，用於排序顯示順序）
    /// 當有多個常用供應商時，決定推薦的先後順序
    /// 例如：1=第一順位, 2=第二順位...
    /// </summary>
    public int Priority { get; set; } = 999;
    
    // ===== 採購資訊 =====
    
    /// <summary>
    /// 最近採購單價（參考用，採購單完成時自動更新）
    /// </summary>
    public decimal? LastPurchasePrice { get; set; }
    
    /// <summary>
    /// 最近採購日期（採購單完成時自動更新）
    /// </summary>
    public DateTime? LastPurchaseDate { get; set; }
    
    /// <summary>
    /// 供應商料號（供應商自己的商品編號，方便採購時對應）
    /// </summary>
    public string? SupplierProductCode { get; set; }
    
    // ===== 交貨條件 =====
    
    /// <summary>
    /// 預計交貨天數（從下單到交貨的天數，可選填）
    /// </summary>
    public int? LeadTimeDays { get; set; }
    
    // ===== 備註 =====
    
    /// <summary>
    /// 備註（採購注意事項、供應商特殊條件等）
    /// </summary>
    public string? Remarks { get; set; }
    
    // ===== 導航屬性 =====
    
    /// <summary>
    /// 關聯的商品
    /// </summary>
    public virtual Product? Product { get; set; }
    
    /// <summary>
    /// 關聯的供應商
    /// </summary>
    public virtual Supplier? Supplier { get; set; }
}
```

### 欄位設計說明

#### IsPreferred（常用供應商）vs Priority（優先順序）的差異

**使用場景範例**：
假設「iPhone 手機殼」這個商品有以下供應商：

| 供應商 | IsPreferred | Priority | 說明 |
|--------|-------------|----------|------|
| A廠商 | ✅ true | 1 | 最常配合的供應商，價格好、交貨快 |
| B廠商 | ✅ true | 2 | 備用供應商，A廠缺貨時用 |
| C廠商 | ❌ false | 999 | 曾經買過，但現在不常用 |
| D廠商 | ❌ false | 999 | 很久以前買過一次（歷史記錄） |

**推薦邏輯**：
```
當「iPhone 手機殼」低於庫存警戒時，系統顯示：

┌─────────────────────────────────┐
│ 🌟 常用供應商（優先推薦）        │
├─────────────────────────────────┤
│ 1️⃣ A廠商 (Priority=1)           │  ← 最優先
│    最近採購價: $50              │
│    最後採購: 2025/12/01         │
│    [立即採購] 按鈕              │
├─────────────────────────────────┤
│ 2️⃣ B廠商 (Priority=2)           │  ← 次優先
│    最近採購價: $52              │
│    最後採購: 2025/11/15         │
│    [立即採購] 按鈕              │
├─────────────────────────────────┤
│ 📋 其他採購記錄（參考）          │
├─────────────────────────────────┤
│ C廠商                           │
│    最近採購價: $55              │
│    最後採購: 2025/09/10         │
├─────────────────────────────────┤
│ D廠商                           │
│    最近採購價: $60              │
│    最後採購: 2024/03/20         │
└─────────────────────────────────┘
```

**總結**：
- `IsPreferred`: 標記「這個供應商是我們想推薦的」
- `Priority`: 當有多個常用供應商時，決定「先推薦哪一個」

### 現有資料表調整

#### Product 資料表
```csharp
// 新增導航屬性
/// <summary>
/// 供應商關聯列表
/// </summary>
public virtual ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
```

#### Supplier 資料表
```csharp
// 新增導航屬性
/// <summary>
/// 供應商品列表
/// </summary>
public virtual ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
```

---

## 🚀 實作階段規劃

### Phase 1：基礎架構建置（優先實作）

**目標**：快速上線，先用採購歷史資料

#### 1.1 低庫存警戒 - 供應商推薦功能
- [ ] 在 `StockAlertViewModalComponent` 新增「尋找供應商」按鈕
- [ ] 建立 `SupplierRecommendationService` 服務
- [ ] 實作從採購歷史查詢供應商的邏輯
- [ ] 顯示供應商列表（含最近採購價格、採購次數、最後採購日期）
- [ ] 提供「立即採購」按鈕，開啟採購單並預填資料

#### 1.2 採購歷史分析 API
```csharp
// ISupplierRecommendationService.cs
/// <summary>
/// 取得商品的供應商推薦清單（混合綁定資料與歷史資料）
/// </summary>
Task<List<SupplierRecommendation>> GetRecommendedSuppliersAsync(int productId);

/// <summary>
/// 供應商推薦資訊
/// </summary>
public class SupplierRecommendation
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; }
    public string SupplierCode { get; set; }  // 供應商編號
    public string? SupplierProductCode { get; set; }  // 供應商料號
    
    // 價格資訊
    public decimal? LastPurchasePrice { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public int PurchaseCount { get; set; }  // 總採購次數
    public decimal? AveragePrice { get; set; }  // 平均價格（參考用）
    public decimal? LowestPrice { get; set; }  // 最低價格
    public decimal? HighestPrice { get; set; }  // 最高價格
    
    // 推薦資訊
    public bool IsPreferred { get; set; }  // 是否為常用供應商
    public int Priority { get; set; }  // 優先順序
    public string RecommendationSource { get; set; }  // "Preferred"(常用), "History"(歷史), "Both"(兩者)
    public int? LeadTimeDays { get; set; }  // 預計交貨天數
    public string? Remarks { get; set; }  // 備註
    
    // UI 輔助屬性
    public string DisplayOrder => IsPreferred ? $"⭐ {Priority}" : "📋";
    public string PriceRange => LowestPrice.HasValue && HighestPrice.HasValue 
        ? $"${LowestPrice:N0} - ${HighestPrice:N0}" 
        : LastPurchasePrice?.ToString("C") ?? "無報價";
}
```

---

### Phase 2：建立綁定機制

**目標**：建立正式的商品-供應商關聯管理

#### 2.1 資料表與 Migration
- [ ] 建立 `ProductSupplier` 實體類別
- [ ] 建立 Migration 腳本
- [ ] 更新 `Product` 和 `Supplier` 導航屬性
- [ ] 設定 EF Core FluentAPI 配置（唯一約束、索引）

#### 2.2 基礎服務層
- [ ] 建立 `IProductSupplierService` 介面
- [ ] 實作 `ProductSupplierService`
- [ ] 提供 CRUD 操作
- [ ] 提供查詢方法（依商品查供應商、依供應商查商品）

#### 2.3 商品編輯頁面 - 供應商管理分頁
- [ ] 在 `ProductEditModalComponent` 新增「供應商」自訂模組
- [ ] 使用 `InteractiveTableComponent` 顯示已綁定的供應商列表
- [ ] 提供新增/編輯/刪除綁定功能
- [ ] 欄位包含：供應商、供應商料號、常用標記、優先順序、交貨天數、最近價格、備註
- [ ] 勾選「常用供應商」（可多選）
- [ ] 調整供應商優先順序（直接輸入數字）

#### 2.4 供應商編輯頁面 - 供應商品列表 ⭐ 優先實作
- [ ] 在 `SupplierEditModalComponent` 新增「供應商品」自訂模組
- [ ] 使用 `InteractiveTableComponent` 顯示該供應商可提供的商品列表
- [ ] 提供新增/編輯/刪除綁定功能
- [ ] 欄位包含：商品、供應商料號、常用標記、優先順序、交貨天數、備註
- [ ] 自動從採購歷史載入最近價格（只顯示，不可編輯）
- [ ] 支援批次匯入（從採購歷史快速建立綁定）

---

### Phase 3：智能推薦整合

**目標**：結合綁定資料與歷史資料，提供最佳推薦

#### 3.1 混合推薦邏輯
- [ ] 更新 `SupplierRecommendationService`
- [ ] 優先查詢 `ProductSupplier` 綁定資料（常用供應商）
- [ ] 查詢所有採購歷史資料（不限時間範圍，顯示所有曾經採購過的供應商）
- [ ] 合併排序規則：
  - 第一優先：常用供應商（IsPreferred=true），依 Priority 排序
  - 第二優先：其他歷史供應商，依最近採購日期排序
- [ ] 整合 `RelatedDocumentsModalComponent` 顯示供應商推薦清單
- [ ] 顯示交貨期預估（如有設定）
- [ ] 顯示價格範圍（最低～最高）
- [ ] 顯示採購次數統計
- [ ] 視覺化標示：常用供應商（⭐ 星號）vs 歷史記錄（📋
- [ ] 顯示交貨期預估
- [ ] 顯示價格趨勢（折線圖）
- [ ] 顯示庫存建議訂購量（基於 MOQ、包裝單位）

#### 3.3 一鍵採購功能
- [ ] 從推薦清單直接開啟採購單
- [ ] 自動預填：供應商、商品、建議數量
- [ ] 自動計算：預計交貨日（今天 + 交貨天數）

---
優化（後續擴展）

**目標**：自動化維護

#### 4.1 自動更新機制
- [ ] 採購單完成時，自動更新 `LastPurchasePrice` 和 `LastPurchaseDate`
- [ ] 定期分析採購頻率，建議將常用供應商標記為 `IsPreferred`

#### 4.2 批次匯入功能
- [ ] 從採購歷史快速建立商品-供應商綁定
- [ ] Excel 匯入供應商料號對照表度）
- [ ] 成本趨勢分析（價格波動圖表）

---

## 🔍 待討論事項

### 1. 主要供應商的唯一性約束

**問題**：一個商品是否只能有一個主要供應商？

**選項**：
- [✅ 已確認的設計決策

### 1. 常用供應商設定 ✅

**決策**：一個商品可以有多個常用供應商

**實作方式**：
- 使用 `IsPreferred = true` 標記常用供應商（可多選）
- 使用 `Priority` 決定多個常用供應商的推薦順序
- 不設定資料庫唯一性約束

**範例**：
```
商品「iPhone 14 手機殼」的供應商設定：
- A廠商：IsPreferred=true, Priority=1  ← 第一順位
- B廠商：IsPreferred=true, Priority=2  ← 第二順位
- C廠商：IsPreferred=false              ← 歷史記錄
```

---

### 2. 優先順序 (Priority) 的用途 ✅

**決策**：保留 `Priority` 欄位，用於排序常用供應商

**使用場景說明**：
當一個商品有多個常用供應商時，`Priority` 決定推薦的先後順序：
- `Priority = 1`: 最優先推薦（例如：價格最優、交貨最快）
- `Priority = 2`: 次優先（例如：A廠缺貨時的備選）
- `Priority = 3`: 第三順位（以此類推）
- `Priority = 999`: 預設值（未設定優先順序的綁定供應商）

**顯示效果**：
```
┌─────────────────────┐
│ ⭐ 常用供應商       │
│ 1️⃣ A廠商 (最優先)   │
│ 2️⃣ B廠商 (次優先)   │
├─────────────────────┤
│ 📋 歷史採購記錄     │
│ C廠商              │
│ D廠商              │
└─────────────────────┘
```

---

### 3. 歷史資料查詢範圍 ✅

**決策**：查詢所有採購歷史記錄（不限時間範圍）

**理由**：使用者需要知道「誰賣過這個商品」，而非限制在最近 N 筆或 N 個月

**實作方式**：
- 查詢該商品的所有採購記錄（`PurchaseOrderDetail`）
- 依最近採購日期排序顯示
- 顯示採購次數、價格範圍等統計資訊

**SQL 概念**：
```sql
SELECT SupplierId, 
       COUNT(*) as PurchaseCount,
       MAX(OrderDate) as LastPurchaseDate,
       AVG(UnitPrice) as AveragePrice,
       MIN(UnitPrice) as LowestPrice,
       MAX(UnitPrice) as HighestPrice
FROM PurchaseOrderDetail pod
JOIN PurchaseOrder po ON pod.PurchaseOrderId = po.Id
WHERE pod.ProductId = @ProductId
GROUP BY SupplierId
ORDER BY LastPurchaseDate DESC
```

---

### 4. 價格資訊處理 ✅

**決策**：保留 `LastPurchasePrice`，採購單完成時自動更新

**實作方式**：
- `ProductSupplier.LastPurchasePrice`: 快取最近採購價格（參考用）
- `ProductSupplier.LastPurchaseDate`: 快取最近採購日期
- 當採購單狀態變更為「已完成」時，自動更新這兩個欄位

**不實作的欄位**：
- ❌ `ContractPrice`（合約價格）
- ❌ `ContractStartDate`/`ContractEndDate`（合約日期）

---

### 5. 權限管理 ✅

**決策**：不需要特別設定權限

**實作方式**：
- 依照既有的權限架構（`Product.Write` 權限）
- 有商品編輯權限的人即可管理供應商綁定

---

### 6. 供應商評分 ✅

**決策**：暫不實作評分功能

**移除的欄位**：
- ❌ `Rating`（供應商評分）
- ❌ `IsCertified`（是否為認證供應商）

---

### 7. 低庫存警戒 - 推薦畫面設計 ✅

**決策**：使用 `RelatedDocumentsModalComponent` 顯示供應商推薦清單

**優點**：
- ✅ 重用現有的 Modal 組件架構
- ✅ 一致的使用者體驗
- ✅ 支援分組顯示（常用 vs 歷史）
- ✅ 已有完善的互動設計

**實作方式**：
1. 擴展 `RelatedDocumentType` 加入 `SupplierRecommendation`
2. 新增供應商推薦的 Section 配置
3. 新增供應商推薦的顯示範本
4. 在 `StockAlertViewModalComponent` 每列加入「尋找供應商」按鈕

---

### 8-10. 簡化設計 ✅

**決策**：以下功能暫不實作

**移除的欄位**：
- ❌ `MinOrderQuantity`（MOQ 最小訂購量）
- ❌ `PackageQuantity`（包裝單位數量）
- ❌ `ContractNumber`（合約編號）
- ❌ `ContractStartDate`/`ContractEndDate`（合約日期）
- ❌ `ContractPrice`（合約價格）eferred_Priority 
ON ProductSupplier(ProductId, IsPreferred DESC, Priority ASC);

-- 供應商查詢商品
CREATE INDEX IX_ProductSupplier_SupplierId 
ON ProductSupplier(SupplierId);

-- 確保同一商品-供應商組合不重複
CREATE UNIQUE INDEX UX_ProductSupplier_ProductId_SupplierId 
ON ProductSupplier(ProductId, SupplierId)te IS NOT NULL;

-- 確保一個商品只有一個主要供應商（可選）
CREATE UNIQUE INDEX UX_ProductSupplier_ProductId_Primary 
ON ProductSupplier(ProductId) 
WHERE IsPrimary = 1;
```

---

## 🔗 相關檔案

### 主要檔案
- **低庫存警戒組件**: `Components/Shared/BaseModal/Modals/Warehouse/StockAlertViewModalComponent.razor`
- **庫存編輯組件**: `Components/Pages/Warehouse/InventoryStockEditModalComponent.razor`
- **相關單據Modal組件**: `Components/Shared/BaseModal/Modals/RelatedDocument/RelatedDocumentsModalComponent.razor` ⭐ 重用此組件顯示供應商推薦
- **庫存編輯組件**: `Components/Pages/Warehouse/InventoryStockEditModalComponent.razor`
- **商品編輯組件**: `Components/Pages/Product/ProductEditModalComponent.razor`
- **供應商編輯組件**: `Components/Pages/Supplier/SupplierEditModalComponent.razor`

### 資料實體
- **商品**: `Data/Entities/Product.cs`
- **供應商**: `Data/Entities/Supplier.cs`
- **商品-供應商關聯**: `Data/Entities/ProductSupplier.cs` ⭐ 新增
- **採購單**: `Data/Entities/PurchaseOrder.cs`
- **採購明細**: `Data/Entities/PurchaseOrderDetail.cs`

### 服務層
- **庫存服務**: `Services/InventoryStockService.cs`
- **商品服務**: `Services/ProductService.cs`
- **供應商服務**: `Services/SupplierService.cs`
- **商品-供應商服務**: `Services/ProductSupplierService.cs` ⭐ 新增
- **供應商推薦服務**: `Services/SupplierRecommendationService.cs` ⭐ 新增
---

## 📝 決策記錄

### 為什麼採用混合方案？

1. **漸進式實作**：可以先快速上線（Phase 1），再逐步完善
2. **資料驗證**：綁定資料可與實際採購對比，發現異常
3. **新舊商品兼顧**：新商品用綁定、舊商品可參考歷史
4. **彈性高**：正常情況走設定，緊急情況查歷史

### 與其他 ERP 系統的比較

| ERP 系統 | 商品-供應商關聯方式 | 備註 |
|---------|-------------------|------|
| SAP | 物料主檔 - 採購資訊記錄 (PIR) | 強綁定 + 合約管理 |
| Oracle ERP | 供應商-商品關聯 + 採購協議 | 支援多供應商比價 |
| 鼎新 ERP | 供應商主檔綁定 | 較簡單的綁定機制 |
| **本系統** | 混合方案（綁定 + 歷史） | 兼具彈性與結構化 |

---

## ✅ 下一步行動
## 🛠️ SupplierEditModalComponent 實作方案

### 在供應商編輯頁面加入商品管理功能

#### 整體架構

```
SupplierEditModalComponent
├── 基本資訊表單（現有）
│   ├── 廠商編號
│   ├── 公司名稱
│   ├── 聯絡人
│   └── ...
└── 自訂模組：供應商品管理 ⭐ 新增
    └── InteractiveTableComponent<ProductSupplier>
        ├── 商品選擇（SearchableSelect）
        ├── 供應商料號（Text）
        ├── 常用標記（Checkbox）
        ├── 優先順序（Number）
        ├── 交貨天數（Number）
        ├── 最近價格（Display，唯讀）
        ├── 備註（Text）
        └── 操作（刪除按鈕）
```

---

### Table 欄位定義

```csharp
// SupplierEditModalComponent.razor.cs

/// <summary>
/// 建立供應商品管理的欄位定義
/// </summary>
private List<InteractiveColumnDefinition> GetProductSupplierColumnDefinitions()
{
    return new List<InteractiveColumnDefinition>
    {
        // 商品選擇（必填，SearchableSelect）
        new InteractiveColumnDefinition
        {
            Title = "商品",
            PropertyName = nameof(ProductSupplier.ProductId),
            ColumnType = InteractiveColumnType.SearchableSelect,
            Width = "25%",
            IsRequired = true,
            Placeholder = "請選擇商品",
            TriggerEmptyRowOnFilled = true,  // 選擇商品後自動新增空行
            
            // SearchableSelect 配置
            GetDropdownItems = (item) => 
            {
                var productSupplier = item as ProductSupplier;
                var searchText = productSupplier?.SearchText?.Trim().ToLower() ?? "";
                
                if (string.IsNullOrEmpty(searchText))
                {
                    return availableProducts;
                }
                
                return availableProducts
                    .Where(p => p.Name.ToLower().Contains(searchText) || 
                               p.Code.ToLower().Contains(searchText))
                    .ToList();
            },
            GetDisplayText = (item) => 
            {
                var productSupplier = item as ProductSupplier;
                if (productSupplier?.ProductId > 0)
                {
                    var product = availableProducts.FirstOrDefault(p => p.Id == productSupplier.ProductId);
                    return product != null ? $"{product.Code} - {product.Name}" : "";
                }
                return productSupplier?.SearchText ?? "";
            },
            GetDropdownItemText = (dropdownItem) => 
            {
                var product = dropdownItem as Product;
                return product != null ? $"{product.Code} - {product.Name}" : "";
            },
            
            // 事件處理
            OnSearchInputChanged = EventCallback.Factory.Create<(object, string?)>(
                this, 
                async args => await HandleProductSearchChanged(args.Item1, args.Item2)
            ),
            OnItemSelected = EventCallback.Factory.Create<(object, object)>(
                this,
                async args => await HandleProductSelected(args.Item1, args.Item2)
            )
        },
        
        // 供應商料號
        new InteractiveColumnDefinition
        {
            Title = "供應商料號",
            PropertyName = nameof(ProductSupplier.SupplierProductCode),
            ColumnType = InteractiveColumnType.Text,
            Width = "15%",
            Placeholder = "供應商料號",
            MaxLength = 50,
            HelpText = "供應商自己的商品編號"
        },
        
        // 常用標記
        new InteractiveColumnDefinition
        {
            Title = "常用",
            PropertyName = nameof(ProductSupplier.IsPreferred),
            ColumnType = InteractiveColumnType.Checkbox,
            Width = "8%",
            TextAlign = "center",
            HelpText = "勾選為常用供應商，優先推薦"
        },
        
        // 優先順序
        new InteractiveColumnDefinition
        {
            Title = "優先順序",
            PropertyName = nameof(ProductSupplier.Priority),
            ColumnType = InteractiveColumnType.Number,
            Width = "10%",
            Placeholder = "999",
            MinValue = 1,
            MaxValue = 999,
            HelpText = "數字越小越優先（1=最優先）",
            CellCssClass = "text-center"
        },
        
        // 預計交貨天數
        new InteractiveColumnDefinition
        {
            Title = "交貨天數",
            PropertyName = nameof(ProductSupplier.LeadTimeDays),
            ColumnType = InteractiveColumnType.Number,
            Width = "10%",
            Placeholder = "天數",
            MinValue = 0,
            HelpText = "預計交貨天數",
            CellCssClass = "text-center"
        },
        
        // 最近採購價格（唯讀，從採購歷史載入）
        new InteractiveColumnDefinition
        {
            Title = "最近價格",
            PropertyName = nameof(ProductSupplier.LastPurchasePrice),
            ColumnType = InteractiveColumnType.Display,
            Width = "12%",
            IsReadOnly = true,
            CellCssClass = "text-end text-muted",
            FormatString = "{0:C}",
            HelpText = "系統自動更新"
        },
        
        // 備註
        new InteractiveColumnDefinition
        {
            Title = "備註",
            PropertyName = nameof(ProductSupplier.Remarks),
            ColumnType = InteractiveColumnType.Text,
            Width = "20%",
            Placeholder = "採購注意事項",
            MaxLength = 200
        }
    };
}
```

---

### 自訂模組定義

```csharp
// SupplierEditModalComponent.razor.cs

/// <summary>
/// 配置自訂模組 - 供應商品管理
/// </summary>
private List<GenericEditModalComponent<Supplier, ISupplierService>.CustomModule> GetCustomModules()
{
    if (editModalComponent == null)
    {
        return new List<GenericEditModalComponent<Supplier, ISupplierService>.CustomModule>();
    }

    return new List<GenericEditModalComponent<Supplier, ISupplierService>.CustomModule>
    {
        new GenericEditModalComponent<Supplier, ISupplierService>.CustomModule
        {
            Title = "供應商品管理",
            Order = 1,
            Content = CreateProductSupplierContent()
        }
    };
}

/// <summary>
/// 創建供應商品管理內容的 RenderFragment
/// </summary>
private RenderFragment CreateProductSupplierContent() => __builder =>
{
    <div class="card border-0 shadow-sm">
        <div class="card-header bg-light">
            <div class="d-flex justify-content-between align-items-center">
                <h6 class="mb-0">
                    <i class="bi bi-box-seam me-2"></i>
                    此供應商可提供的商品
                </h6>
                <button type="button" 
                        class="btn btn-sm btn-primary"
                        @onclick="HandleBatchImportFromHistory"
                        disabled="@(editModalComponent?.Entity?.Id <= 0)">
                    <i class="bi bi-upload me-1"></i>
                    從採購歷史匯入
                </button>
            </div>
        </div>
        
        <div class="card-body p-0">
            @if (editModalComponent?.Entity != null)
            {
                @if (!isProductSupplierDataReady)
                {
                    <div class="d-flex justify-content-center align-items-center py-4">
                        <div class="spinner-border spinner-border-sm text-primary me-2" role="status"></div>
                        <span class="text-muted">載入供應商品資料中...</span>
                    </div>
                }
                else if (availableProducts != null && availableProducts.Any())
                {
                    <InteractiveTableComponent TItem="ProductSupplier"
                                             Items="@productSuppliers"
                                             ColumnDefinitions="@GetProductSupplierColumnDefinitions()"
                                             ShowHeader="true"
                                             ShowRowNumbers="true"
                                             IsStriped="true"
                                             IsHoverable="true"
                                             IsBordered="true"
                                             ShowBuiltInActions="true"
                                             ShowBuiltInDeleteButton="true"
                                             DeleteButtonVariant="ButtonVariant.Red"
                                             OnItemDelete="@HandleDeleteProductSupplier"
                                             EnableAutoEmptyRow="true"
                                             AllowAddNewRow="@(!IsReadOnly)"
                                             DataLoadCompleted="@isProductSupplierDataReady"
                                             CreateEmptyItem="@CreateEmptyProductSupplier"
                                             EmptyMessage="尚未設定供應商品" />
                }
                else
                {
                    <div class="alert alert-warning text-center m-3" role="alert">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        無可用的商品資料，請先建立商品
                    </div>
                }
            }
        </div>
    </div>
};
```

---

### 私有欄位和資料管理

```csharp
// SupplierEditModalComponent.razor.cs

// 供應商品列表
private List<ProductSupplier> productSuppliers = new();

// 可用商品列表
private List<Product> availableProducts = new();

// 資料載入狀態
private bool isProductSupplierDataReady = false;

/// <summary>
/// 載入供應商品資料
/// </summary>
private async Task LoadProductSupplierData()
{
    try
    {
        isProductSupplierDataReady = false;
        
        // 載入商品列表
        availableProducts = await ProductService.GetAllAsync();
        
        // 如果是編輯模式，載入已綁定的商品
        if (SupplierId.HasValue && SupplierId.Value > 0)
        {
            var supplier = await SupplierService.GetByIdAsync(SupplierId.Value);
            
            if (supplier?.ProductSuppliers != null && supplier.ProductSuppliers.Any())
            {
                productSuppliers = supplier.ProductSuppliers.ToList();
            }
            else
            {
                productSuppliers = new List<ProductSupplier>();
            }
        }
        else
        {
            // 新增模式，清空列表
            productSuppliers = new List<ProductSupplier>();
        }
        
        isProductSupplierDataReady = true;
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入供應商品資料時發生錯誤：{ex.Message}");
        availableProducts = new List<Product>();
        productSuppliers = new List<ProductSupplier>();
        isProductSupplierDataReady = true;
    }
}

/// <summary>
/// 建立空的 ProductSupplier 項目
/// </summary>
private ProductSupplier CreateEmptyProductSupplier()
{
    return new ProductSupplier
    {
        SupplierId = SupplierId ?? 0,
        ProductId = 0,
        IsPreferred = false,
        Priority = 999,
        SearchText = "",  // 用於 SearchableSelect 的搜尋文字
        Status = EntityStatus.Active
    };
}
```

---

### 事件處理方法

```csharp
// SupplierEditModalComponent.razor.cs

/// <summary>
/// 處理商品搜尋變更
/// </summary>
private async Task HandleProductSearchChanged(object item, string? searchText)
{
    if (item is ProductSupplier productSupplier)
    {
        productSupplier.SearchText = searchText ?? "";
        StateHasChanged();
    }
    
    await Task.CompletedTask;
}

/// <summary>
/// 處理商品選擇
/// </summary>
private async Task HandleProductSelected(object item, object selectedItem)
{
    if (item is ProductSupplier productSupplier && selectedItem is Product product)
    {
        // 檢查是否已經綁定此商品
        var existingBinding = productSuppliers.FirstOrDefault(ps => 
            ps.ProductId == product.Id && ps != productSupplier);
        
        if (existingBinding != null)
        {
            await NotificationService.ShowWarningAsync($"商品「{product.Name}」已經綁定，請勿重複新增");
            
            // 清空選擇
            productSupplier.ProductId = 0;
            productSupplier.SearchText = "";
            StateHasChanged();
            return;
        }
        
        // 設定商品
        productSupplier.ProductId = product.Id;
        productSupplier.Product = product;
        productSupplier.SearchText = $"{product.Code} - {product.Name}";
        
        // 自動載入最近採購價格（如果有）
        await LoadLastPurchasePriceAsync(productSupplier);
        
        StateHasChanged();
    }
}

/// <summary>
/// 載入最近採購價格
/// </summary>
private async Task LoadLastPurchasePriceAsync(ProductSupplier productSupplier)
{
    try
    {
        if (SupplierId.HasValue && productSupplier.ProductId > 0)
        {
            // 查詢該供應商對此商品的最近採購記錄
            var lastPurchase = await SupplierRecommendationService
                .GetLastPurchasePriceAsync(SupplierId.Value, productSupplier.ProductId);
            
            if (lastPurchase != null)
            {
                productSupplier.LastPurchasePrice = lastPurchase.Price;
                productSupplier.LastPurchaseDate = lastPurchase.PurchaseDate;
            }
        }
    }
    catch (Exception ex)
    {
        // 載入價格失敗不影響主流程，只記錄錯誤
        Console.WriteLine($"載入最近採購價格失敗：{ex.Message}");
    }
}

/// <summary>
/// 處理刪除供應商品綁定
/// </summary>
private async Task HandleDeleteProductSupplier(ProductSupplier item)
{
    try
    {
        var productName = item.Product?.Name ?? "此商品";
        var confirmed = await NotificationService.ShowConfirmAsync(
            $"確定要移除「{productName}」的綁定嗎？",
            "確認刪除"
        );
        
        if (confirmed)
        {
            productSuppliers.Remove(item);
            
            // 如果是已儲存的綁定，需要從資料庫刪除
            if (item.Id > 0)
            {
                await ProductSupplierService.DeleteAsync(item.Id);
                await NotificationService.ShowSuccessAsync("綁定已刪除");
            }
            
            StateHasChanged();
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"刪除綁定時發生錯誤：{ex.Message}");
    }
}

/// <summary>
/// 從採購歷史批次匯入
/// </summary>
private async Task HandleBatchImportFromHistory()
{
    try
    {
        if (!SupplierId.HasValue || SupplierId.Value <= 0)
        {
            await NotificationService.ShowWarningAsync("請先儲存供應商資料後再匯入");
            return;
        }
        
        var confirmed = await NotificationService.ShowConfirmAsync(
            "系統將自動分析此供應商的採購歷史記錄，並建立商品綁定。\n\n" +
            "已存在的綁定不會被覆蓋。\n\n" +
            "確定要繼續嗎？",
            "從採購歷史匯入"
        );
        
        if (!confirmed) return;
        
        // 呼叫服務批次匯入
        var importedCount = await ProductSupplierService
            .ImportFromPurchaseHistoryAsync(SupplierId.Value);
        
        if (importedCount > 0)
        {
            await NotificationService.ShowSuccessAsync($"已成功匯入 {importedCount} 筆商品綁定");
            
            // 重新載入資料
            await LoadProductSupplierData();
            StateHasChanged();
        }
        else
        {
            await NotificationService.ShowInfoAsync("沒有找到可匯入的採購記錄");
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"匯入失敗：{ex.Message}");
    }
}
```

---

### 更新 OnParametersSetAsync

```csharp
protected override async Task OnParametersSetAsync()
{
    if (IsVisible && !isDataLoaded)
    {
        await LoadAdditionalDataAsync();
        await LoadProductSupplierData();  // ⭐ 新增：載入供應商品資料
        await InitializeFormFieldsAsync();
        isDataLoaded = true;
    }
    else if (!IsVisible)
    {
        isDataLoaded = false;
    }
}
```

---

### 更新 SaveSupplier 方法

```csharp
private async Task<bool> SaveSupplier(Supplier entity)
{
    try
    {
        // 使用服務的完整驗證邏輯（包含重複檢查）
        var validationResult = await SupplierService.ValidateAsync(entity);
        if (!validationResult.IsSuccess)
        {
            _ = NotificationService.ShowErrorAsync(validationResult.ErrorMessage);
            return false;
        }

        // ⭐ 新增：將 productSuppliers 賦值給 entity
        entity.ProductSuppliers = productSuppliers
            .Where(ps => ps.ProductId > 0)  // 過濾掉空行
            .ToList();

        ServiceResult result;
        
        if (SupplierId.HasValue)
        {
            // 更新現有廠商
            result = await SupplierService.UpdateAsync(entity);
        }
        else
        {
            // 新增廠商
            result = await SupplierService.CreateAsync(entity);
        }

        // 讓 GenericEditModalComponent 處理通用的成功/失敗訊息
        return result.IsSuccess;
    }
    catch (Exception)
    {
        _ = NotificationService.ShowErrorAsync("儲存廠商資料時發生錯誤");
        return false;
    }
}
```

---

### 需要注入的服務

```csharp
@inject IProductService ProductService
@inject IProductSupplierService ProductSupplierService
@inject ISupplierRecommendationService SupplierRecommendationService
```

---
1. 🎬 RelatedDocumentsModalComponent 整合方案

### 擴展 RelatedDocumentType

```csharp
public enum RelatedDocumentType
{
    // ... 現有的單據類型 ...
    
    /// <summary>
    /// 供應商推薦（用於低庫存警戒）
    /// </summary>
    SupplierRecommendation
}
```

### 新增 Section 配置

```csharp
// Config/DocumentSectionConfig.cs
public static DocumentSectionConfig GetConfig(RelatedDocumentType type)
{
    return type switch
    {
        RelatedDocumentType.SupplierRecommendation => new DocumentSectionConfig
        {
            Title = "供應商推薦",
            Icon = "bi-shop",
            EmptyMessage = "尚無供應商資料",
            ShowAddButton = false,  // 不顯示新增按鈕（在商品編輯頁面管理）
            CollapsedByDefault = false,
            HeaderCssClass = "bg-primary text-white"
        },
        // ... 其他類型 ...
    };
}
```

### 新增顯示範本

```razor
<!-- Templates/SupplierRecommendationDetailsTemplate.razor -->
@* 供應商推薦明細範本 *@

<div class="row g-2 align-items-center">
    <!-- 供應商基本資訊 -->
    <div class="col-md-3">
        <div class="d-flex align-items-center">
            @if (Document.IsPreferred)
            {
                <span class="badge bg-warning text-dark me-2">⭐ 常用</span>
            }
            <strong>@Document.SupplierName</strong>
        </div>
        <small class="text-muted">編號: @Document.DocumentCode</small>
    </div>
    
    <!-- 價格資訊 -->
    <div class="col-md-3">
        <div class="small">
            <i class="bi bi-currency-dollar text-success me-1"></i>
            最近價格: <strong>$@Document.LastPurchasePrice?.ToString("N2")</strong>
        </div>
        @if (Document.PriceRange != null)
        {
            <div class="small text-muted">
                價格範圍: @Document.PriceRange
            </div>
        }
    </div>
    
    <!-- 採購統計 -->
    <div class="col-md-2">
        <div class="small">
            <i class="bi bi-cart text-primary me-1"></i>
            採購次數: <strong>@Document.PurchaseCount</strong>
        </div>
        <div class="small text-muted">
            最後採購: @Document.LastPurchaseDate?.ToString("yyyy/MM/dd")
        </div>
    </div>
    
    <!-- 交貨資訊 -->
    <div class="col-md-2">
        @if (Document.LeadTimeDays.HasValue)
        {
            <div class="small">
                <i class="bi bi-truck text-info me-1"></i>
                交貨: @Document.LeadTimeDays 天
            </div>
        }
    </div>
    
    <!-- 操作按鈕 -->
    <div class="col-md-2 text-end">
        <button class="btn btn-sm btn-primary" 
                @onclick="() => OnPurchaseClick.InvokeAsync(Document)">
            <i class="bi bi-cart-plus me-1"></i>
            立即採購
        </button>
    </div>
</div>

@if (!string.IsNullOrWhiteSpace(Document.Remarks))
{
    <div class="row mt-2">
        <div class="col-12">
            <small class="text-muted">
                <i class="bi bi-chat-left-text me-1"></i>
                @Document.Remarks
            </small>
        </div>
    </div>
}

@code {
    [Parameter] public RelatedDocument Document { get; set; } = null!;
    [Parameter] public EventCallback<RelatedDocument> OnPurchaseClick { get; set; }
}
```

### 在 StockAlertViewModalComponent 中使用

```csharp
// StockAlertViewModalComponent.razor.cs
private async Task HandleFindSuppliers(StockAlertViewItem item)
{
    try
    {
        // 1. 查詢供應商推薦清單
        var recommendations = await SupplierRecommendationService
            .GetRecommendedSuppliersAsync(item.ProductId);
        
        // 2. 轉換為 RelatedDocument 格式
        var relatedDocs = recommendations.Select(r => new RelatedDocument
        {
            DocumentType = RelatedDocumentType.SupplierRecommendation,
            DocumentId = r.SupplierId,
            DocumentCode = r.SupplierCode,
            DocumentDate = r.LastPurchaseDate,
            SupplierName = r.SupplierName,
            IsPreferred = r.IsPreferred,
            Priority = r.Priority,
            LastPurchasePrice = r.LastPurchasePrice,
            PurchaseCount = r.PurchaseCount,
            PriceRange = r.PriceRange,
            LeadTimeDays = r.LeadTimeDays,
            Remarks = r.Remarks
        }).ToList();
        
        // 3. 開啟 RelatedDocumentsModalComponent
        relatedDocuments = relatedDocs;
        productName = item.ProductName;
        isRelatedDocsModalVisible = true;
        
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入供應商推薦失敗：{ex.Message}");
    }
}

private async Task HandlePurchaseClick(RelatedDocument document)
{
    try
    {
        // 開啟採購單編輯 Modal，並預填資料
        var prefilledData = new Dictionary<string, object?>
        {
            { "SupplierId", document.DocumentId },
            { "ProductId", currentProductId },
            { "UnitPrice", document.LastPurchasePrice }
        };
        
        await purchaseOrderEditModal.ShowAddModalWithPrefilledData(prefilledData);
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"開啟採購單失敗：{ex.Message}");
    }
}
```

---

## ✅ 實作順序與優先級

### 第一階段：基礎建置（2-3 天）

**目標**：建立資料表和基礎服務

1. **建立 ProductSupplier 實體** ✅
   - 定義資料表結構
   - 設定 EF Core FluentAPI 配置
   - 建立 Migration

2. **實作服務層** ✅
   - `IProductSupplierService` + `ProductSupplierService`
   - `ISupplierRecommendationService` + `SupplierRecommendationService`
   - 提供 CRUD 和查詢方法

3. **更新現有實體** ✅
   - `Product` 加入 `ProductSuppliers` 導航屬性
   - `Supplier` 加入 `ProductSuppliers` 導航屬性

---

### 第二階段：供應商頁面管理（2-3 天）⭐ 優先

**目標**：在供應商編輯頁面加入商品管理功能

1. **SupplierEditModalComponent 擴展**
   - 新增自訂模組：供應商品管理
   - 使用 `InteractiveTableComponent` 顯示商品列表
   - 實作 SearchableSelect 商品選擇
   - 實作常用標記、優先順序、交貨天數等欄位
   - 自動載入最近採購價格

2. **批次匯入功能**
   - 從採購歷史分析並建立綁定
   - 顯示匯入結果

3. **驗證邏輯**
   - 防止重複綁定同一商品
   - 檢查必填欄位

---

### 第三階段：低庫存警戒整合（1-2 天）

**目標**：實現從低庫存直接尋找供應商

1. **擴展 RelatedDocumentsModalComponent**
   - 新增 `SupplierRecommendation` 類型
   - 建立供應商推薦顯示範本
   - 實作「立即採購」按鈕

2. **StockAlertViewModalComponent 擴展**
   - 加入「尋找供應商」按鈕
   - 呼叫 `SupplierRecommendationService`
   - 顯示推薦清單（常用 + 歷史）

3. **採購單預填**
   - 從推薦清單開啟採購單
   - 自動預填供應商、商品、建議價格

---

### 第四階段：商品頁面管理（選配）

**目標**：在商品編輯頁面加入供應商管理

1. **ProductEditModalComponent 擴展**
   - 新增自訂模組：供應商管理
   - 使用 `InteractiveTableComponent` 顯示供應商列表
   - 功能與供應商頁面類似（反向關聯）

---

### 第五階段：自動化與優化（選配）

**目標**：自動更新和智能推薦

1. **採購單完成時自動更新**
   - 更新 `LastPurchasePrice` 和 `LastPurchaseDate`
   - 建議將頻繁採購的供應商設為常用

2. **報表與統計**
   - 供應商績效分析
   - 商品採購來源分析

---

## 📋 開發 Checklist

### Phase 1: 基礎建置
- [ ] 建立 `ProductSupplier.cs` 實體
- [ ] 設定 `AppDbContext` 的 FluentAPI 配置
- [ ] 建立 Migration：`AddProductSupplierTable`
- [ ] 更新 `Product.cs` 導航屬性
- [ ] 更新 `Supplier.cs` 導航屬性
- [ ] 建立 `IProductSupplierService.cs` 介面
- [ ] 實作 `ProductSupplierService.cs`
- [ ] 建立 `ISupplierRecommendationService.cs` 介面
- [ ] 實作 `SupplierRecommendationService.cs`
- [ ] 註冊服務到 DI 容器

### Phase 2: 供應商頁面 ⭐
- [ ] 在 `SupplierEditModalComponent` 加入 `ProductSupplier` 列表
- [ ] 實作 `GetProductSupplierColumnDefinitions()` 方法
- [ ] 實作 `CreateProductSupplierContent()` RenderFragment
- [ ] 實作 `HandleProductSearchChanged()` 事件
- [ ] 實作 `HandleProductSelected()` 事件
- [ ] 實作 `LoadLastPurchasePriceAsync()` 方法
- [ ] 實作 `HandleDeleteProductSupplier()` 事件
- [ ] 實作 `HandleBatchImportFromHistory()` 批次匯入
- [ ] 更新 `SaveSupplier()` 儲存邏輯
- [ ] 測試新增/編輯/刪除綁定功能

### Phase 3: 低庫存警戒
- [ ] 擴展 `RelatedDocumentType` 列舉
- [ ] 新增 `SupplierRecommendationDetailsTemplate.razor`
- [ ] 更新 `DocumentSectionConfig` 配置
- [ ] 在 `StockAlertViewModalComponent` 加入「尋找供應商」欄位
- [ ] 實作 `HandleFindSuppliers()` 方法
- [ ] 實作 `HandlePurchaseClick()` 事件
- [ ] 測試推薦清單顯示
- [ ] 測試一鍵開啟採購單

### Phase 4: 商品頁面（選配）
- [ ] 在 `ProductEditModalComponent` 加入供應商管理模組
- [ ] 實作類似的 Table 和事件處理
- [ ] 測試雙向綁定一致性

### Phase 5: 自動化（選配）
- [ ] 採購單狀態變更時更新價格
- [ ] 建立定期分析 Job
- [ ] 實作推薦演算法

---

## 📊 預估時程

| 階段 | 預估時間 | 優先級 | 說明 |
|------|---------|--------|------|
| Phase 1 | 2-3 天 | 🔴 必要 | 資料表和服務基礎 |
| Phase 2 | 2-3 天 | 🔴 必要 | 供應商頁面管理（優先） |
| Phase 3 | 1-2 天 | 🟡 重要 | 低庫存警戒整合 |
| Phase 4 | 1-2 天 | 🟢 選配 | 商品頁面管理 |
| Phase 5 | 2-3 天 | 🟢 選配 | 自動化與優化 |
| **總計** | **約 1-2 週** | | 核心功能約 5-8 天 |

## 📚 參考資料

- [README_A單轉B單.md](./README_A單轉B單.md) - 單據轉換機制參考
- [README_庫存異動正確撰寫方式.md](./README_庫存異動正確撰寫方式.md) - 庫存相關邏輯
- [README_使用者控制審核機制.md](./README_使用者控制審核機制.md) - 權限管理參考

---

**文件結束** | 建立者: GitHub Copilot | 最後更新: 2025/12/11
