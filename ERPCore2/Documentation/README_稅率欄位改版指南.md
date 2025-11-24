# 稅率欄位改版指南

## 📋 概述

本文件說明如何將單據從「統一稅率」改為「明細獨立稅率」的完整步驟。

### 改版目的
- **舊設計**：整張單據使用系統統一稅率（5%）計算稅額
- **新設計**：每筆明細可設定獨立稅率，支援不同商品不同稅率的需求

### 適用範圍
以下單據已完成或需要進行稅率改版：
- ✅ **採購單（PurchaseOrder）** - 已完成改版（範例）
- ⏳ **進貨單（PurchaseReceiving）** - 待改版
- ⏳ **進貨退出單（PurchaseReturn）** - 待改版
- ⏳ **報價單（Quotation）** - 待改版
- ⏳ **銷貨單（SalesDelivery）** - 待改版
- ⏳ **銷貨訂單（SalesOrder）** - 待改版

---

## 🔧 改版三步驟

### **步驟 1：資料表增加稅率欄位**

#### 1.1 修改明細實體（Detail Entity）

**檔案位置**：`Data/Entities/Purchase/PurchaseOrderDetail.cs`（以採購單為例）

```csharp
/// <summary>
/// 採購訂單明細實體 - 記錄採購訂單商品明細
/// </summary>
public class PurchaseOrderDetail : BaseEntity
{
    // ... 其他欄位 ...
    
    [Display(Name = "稅率 (%)")]
    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100, ErrorMessage = "稅率必須介於0到100之間")]
    public decimal? TaxRate { get; set; }  // 👈 新增此欄位
    
    [Display(Name = "小計金額")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubtotalAmount => OrderQuantity * UnitPrice;
    
    // ... 其他欄位 ...
}
```

**重點說明**：
- 欄位類型：`decimal?`（nullable，允許為空）
- 資料庫類型：`decimal(5,2)`（例如：99.99%）
- 驗證範圍：0 ~ 100
- **為何使用 nullable**：若明細未設定稅率，可自動使用系統預設值

#### 1.2 執行 Migration

```powershell
# 在專案根目錄執行
dotnet ef migrations add AddTaxRateToPurchaseOrderDetail
dotnet ef database update
```

---

### **步驟 2：Table 組件增加稅率欄位**

#### 2.1 修改 Table 組件

**檔案位置**：`Components/Shared/BaseModal/Modals/Purchase/PurchaseOrderTable.razor`

在 `GetColumnDefinitions()` 方法中增加稅率欄位：

```csharp
private List<InteractiveColumnDefinition> GetColumnDefinitions()
{
    var columns = new List<InteractiveColumnDefinition>();
    
    // ... 前面的欄位（商品、數量、單價等）...
    
    // 稅率欄位（只讀顯示）
    columns.Add(new()
    {
        Title = "稅率%",
        Tooltip = "商品的稅率（0% ~ 100%），預設為系統設定值",
        PropertyName = "",
        ColumnType = InteractiveColumnType.Custom,
        Width = "80px",
        CustomTemplate = item =>
        {
            var productItem = (ProductItem)item;
            // 只有選擇商品後才顯示稅率
            var displayValue = productItem.SelectedProduct != null && productItem.TaxRate > 0 
                ? $"{productItem.TaxRate}%" 
                : "";
            return @<div class="text-end text-info">@displayValue</div>;
        }
    });
    
    // 小計欄位（含稅）
    columns.Add(new()
    {
        Title = "小計",
        Tooltip = "數量 × 單價 × (1 + 稅率%) 的含稅總計",
        PropertyName = "",
        ColumnType = InteractiveColumnType.Custom,
        Width = "120px",
        CustomTemplate = item =>
        {
            var productItem = (ProductItem)item;
            // 小計含稅 = 數量 × 單價 × (1 + 稅率%)
            var subtotalWithTax = productItem.Quantity * productItem.Price * (1 + productItem.TaxRate / 100);
            var displayValue = NumberFormatHelper.FormatSmartZeroAsEmpty(subtotalWithTax);
            return @<div class="text-end fw-bold text-success">@displayValue</div>;
        }
    });
    
    // ... 後面的欄位（備註等）...
    
    return columns;
}
```

#### 2.2 修改 ProductItem 類別

確保 `ProductItem` 有 `TaxRate` 屬性：

```csharp
public class ProductItem
{
    public Product? SelectedProduct { get; set; }
    public int Quantity { get; set; } = 0;
    public decimal Price { get; set; } = 0;
    public decimal TaxRate { get; set; } = 5.0m;  // 👈 預設 5%
    // ... 其他屬性 ...
}
```

#### 2.3 商品選擇時自動帶入稅率

在 `OnProductSelected` 方法中：

```csharp
private async Task OnProductSelected(ProductItem item, Product? selectedProduct)
{
    if (selectedProduct != null)
    {
        item.SelectedProduct = selectedProduct;
        item.SelectedProductId = selectedProduct.Id;
        
        // 優先使用商品的稅率，如果為 null 則從系統參數取得預設值
        if (selectedProduct.TaxRate.HasValue)
        {
            item.TaxRate = selectedProduct.TaxRate.Value;
        }
        else
        {
            // 從系統參數取得預設稅率
            item.TaxRate = await SystemParameterService.GetTaxRateAsync();
        }
    }
    
    await NotifyDetailsChanged();
}
```

#### 2.4 載入現有明細時設定稅率

在 `LoadExistingDetailsAsync` 方法中：

```csharp
private async Task LoadExistingDetailsAsync()
{
    foreach (var detail in ExistingDetails)
    {
        var taxRate = GetPropertyValue<decimal?>(detail, "TaxRate");
        var item = new ProductItem
        {
            // ... 其他屬性 ...
            TaxRate = taxRate ?? await SystemParameterService.GetTaxRateAsync(),
            // ... 其他屬性 ...
        };
        ProductItems.Add(item);
    }
}
```

#### 2.5 儲存時寫入稅率

在 `ConvertToDetailEntities` 方法中：

```csharp
private List<TDetailEntity> ConvertToDetailEntities()
{
    foreach (var item in ProductItems.Where(x => x.SelectedProduct != null))
    {
        // ... 設定其他屬性 ...
        SetPropertyValue(detail, "TaxRate", item.TaxRate);
        // ... 設定其他屬性 ...
    }
    return details;
}
```

---

### **步驟 3：EditModal 改用明細稅率計算**

#### 3.1 修改 HandleDetailsChanged 方法

**檔案位置**：`Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor`

```csharp
/// <summary>
/// 處理採購明細變更
/// </summary>
private async Task HandleDetailsChanged(List<PurchaseOrderDetail> details)
{
    try
    {
        purchaseOrderDetails = details ?? new List<PurchaseOrderDetail>();            
        
        // 更新主檔的總金額、稅額、含稅總額
        if (editModalComponent?.Entity != null)
        {
            // 1. 計算總金額（未稅）
            editModalComponent.Entity.TotalAmount = purchaseOrderDetails.Sum(d => d.SubtotalAmount);
            
            // 2. 【新式算法】每筆明細分別計算稅額，再加總（支援不同商品不同稅率）
            editModalComponent.Entity.PurchaseTaxAmount = purchaseOrderDetails.Sum(d => 
            {
                // 使用明細的稅率，若無則使用系統預設值
                var detailTaxRate = d.TaxRate ?? currentTaxRate;
                // 計算此筆明細的稅額 = 小計 × 稅率%
                var detailTaxAmount = d.SubtotalAmount * (detailTaxRate / 100m);
                return Math.Round(detailTaxAmount, 2);
            });
            
            // 3. 含稅總金額會自動計算（PurchaseTotalAmountIncludingTax 是計算屬性）
            //    = TotalAmount + PurchaseTaxAmount
            
            StateHasChanged();
        }
    }
    catch (Exception ex)
    {
        // ... 錯誤處理 ...
    }
}
```

#### 3.2 修改 SavePurchaseOrderWithDetails 方法

確保儲存時也使用相同算法：

```csharp
private async Task<bool> SavePurchaseOrderWithDetails(PurchaseOrder purchaseOrder, bool isPreApprovalSave = false)
{
    try
    {
        // 更新總金額和稅額
        purchaseOrder.TotalAmount = purchaseOrderDetails.Sum(d => d.SubtotalAmount);
        
        // 【新式算法】每筆明細分別計算稅額，再加總（支援不同商品不同稅率）
        purchaseOrder.PurchaseTaxAmount = purchaseOrderDetails.Sum(d => 
        {
            // 使用明細的稅率，若無則使用系統預設值
            var detailTaxRate = d.TaxRate ?? currentTaxRate;
            // 計算此筆明細的稅額 = 小計 × 稅率%
            var detailTaxAmount = d.SubtotalAmount * (detailTaxRate / 100m);
            return Math.Round(detailTaxAmount, 2);
        });
        
        // ... 儲存邏輯 ...
    }
    catch (Exception ex)
    {
        // ... 錯誤處理 ...
    }
}
```

**⚠️ 重要提醒**：
- 必須在 **兩個地方** 都使用相同的計算邏輯
- 避免一個地方用新算法，另一個用舊算法，導致儲存後稅額錯誤

---

## 📊 計算範例

### 範例 1：單一稅率
| 商品 | 數量 | 單價 | 稅率 | 小計（未稅） | 稅額 | 小計（含稅） |
|------|------|------|------|--------------|------|-------------|
| A商品 | 10 | 100 | 5% | 1,000 | 50 | 1,050 |

**計算公式**：
```csharp
小計未稅 = 10 × 100 = 1,000
稅額 = 1,000 × 5% = 50
小計含稅 = 1,000 × (1 + 5%) = 1,050
```

### 範例 2：多種稅率（新式算法優勢）
| 商品 | 數量 | 單價 | 稅率 | 小計（未稅） | 稅額 | 小計（含稅） |
|------|------|------|------|--------------|------|-------------|
| A商品 | 10 | 100 | 5% | 1,000 | 50 | 1,050 |
| B商品 | 5 | 200 | 10% | 1,000 | 100 | 1,100 |
| **合計** | | | | **2,000** | **150** | **2,150** |

**新式算法（正確）**：
```csharp
TotalAmount = 1,000 + 1,000 = 2,000
PurchaseTaxAmount = 50 + 100 = 150  ✅
PurchaseTotalAmountIncludingTax = 2,000 + 150 = 2,150
```

**舊式算法（錯誤）**：
```csharp
TotalAmount = 2,000
PurchaseTaxAmount = 2,000 × 5% = 100  ❌ 少算了 B 商品的額外 5% 稅額
PurchaseTotalAmountIncludingTax = 2,100  ❌ 錯誤
```

---

## 🎯 改版檢查清單

### ✅ 步驟 1：資料表檢查
- [ ] 明細實體增加 `TaxRate` 欄位（`decimal?` 類型）
- [ ] 執行 Migration 並更新資料庫
- [ ] 確認資料庫欄位正確建立

### ✅ 步驟 2：Table 組件檢查
- [ ] `GetColumnDefinitions` 增加「稅率」欄位（只讀顯示）
- [ ] 「小計」欄位改為含稅計算：`數量 × 單價 × (1 + 稅率%)`
- [ ] `ProductItem` 類別增加 `TaxRate` 屬性
- [ ] `OnProductSelected` 方法自動帶入商品稅率
- [ ] `LoadExistingDetailsAsync` 方法載入明細稅率
- [ ] `ConvertToDetailEntities` 方法儲存明細稅率

### ✅ 步驟 3：EditModal 檢查
- [ ] `HandleDetailsChanged` 方法使用明細稅額加總算法
- [ ] `SavePurchaseOrderWithDetails` 方法使用相同算法
- [ ] 兩個方法的計算邏輯完全一致

### ✅ 測試檢查
- [ ] 新增單據時，稅率自動帶入（商品稅率 > 系統預設值）
- [ ] 編輯單據時，稅率正確顯示
- [ ] 儲存後稅額計算正確（不會被覆蓋）
- [ ] 混合不同稅率商品時，稅額計算正確
- [ ] 空行（未選商品）不顯示稅率

---

## 🔍 常見問題

### Q1：為何 TaxRate 要用 `decimal?` 而非 `decimal`？
**A**：使用 nullable 類型可以區分「未設定」和「設定為 0」兩種情況。當明細未設定稅率時，可自動使用系統預設值或商品主檔的稅率。

### Q2：稅率應該從哪裡取得？
**A**：優先順序如下：
1. **第一優先**：商品主檔的稅率（`Product.TaxRate`）
2. **第二優先**：系統參數的預設稅率（`SystemParameter.TaxRate`）

### Q3：儲存後稅額顯示錯誤怎麼辦？
**A**：檢查是否有兩個地方都使用新式算法：
- `HandleDetailsChanged`（明細變更時）
- `SavePurchaseOrderWithDetails`（儲存時）

確保兩者邏輯一致，避免儲存時用舊算法覆蓋。

### Q4：小計欄位應該顯示含稅還是不含稅？
**A**：建議顯示**含稅金額**，原因：
- 使用者更關心實際支付金額
- 與主檔的「含稅總額」對應
- 範例：`數量 × 單價 × (1 + 稅率%)`

如果需要同時顯示未稅和含稅，可增加兩個欄位：
- 「小計（未稅）」：`數量 × 單價`
- 「小計（含稅）」：`數量 × 單價 × (1 + 稅率%)`

### Q5：舊資料的稅率欄位會是什麼值？
**A**：Migration 後，舊資料的 `TaxRate` 欄位為 `NULL`。程式會自動使用系統預設值，確保向下相容。

---

## 📚 參考範例

### 完整範例：採購單（PurchaseOrder）

以下檔案已完成改版，可作為其他單據的參考：

1. **實體**：`Data/Entities/Purchase/PurchaseOrderDetail.cs`
2. **Table**：`Components/Shared/BaseModal/Modals/Purchase/PurchaseOrderTable.razor`
3. **EditModal**：`Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor`

### 待改版清單

使用相同模式改版以下單據：

| 單據 | 實體檔案 | Table 組件 | EditModal 組件 |
|------|---------|-----------|---------------|
| 進貨單 | `PurchaseReceivingDetail.cs` | `PurchaseReceivingTable.razor` | `PurchaseReceivingEditModalComponent.razor` |
| 進貨退出 | `PurchaseReturnDetail.cs` | `PurchaseReturnTable.razor` | `PurchaseReturnEditModalComponent.razor` |
| 報價單 | `QuotationDetail.cs` | `QuotationTable.razor` | `QuotationEditModalComponent.razor` |
| 銷貨單 | `SalesDeliveryDetail.cs` | `SalesDeliveryTable.razor` | `SalesDeliveryEditModalComponent.razor` |
| 銷貨訂單 | `SalesOrderDetail.cs` | `SalesOrderTable.razor` | `SalesOrderEditModalComponent.razor` |

---

## 🎓 總結

改版的核心概念：

1. **資料層**：明細表增加 `TaxRate` 欄位（nullable）
2. **展示層**：Table 顯示稅率，小計改為含稅計算
3. **邏輯層**：EditModal 使用「明細稅額加總」取代「統一稅率」

**關鍵成功要素**：
- ✅ 兩個計算點使用相同邏輯（HandleDetailsChanged + SavePurchaseOrderWithDetails）
- ✅ 優先使用商品稅率，回退到系統預設值
- ✅ 稅額計算精確到小數點後 2 位
- ✅ 向下相容舊資料（`TaxRate = NULL` 時使用預設值）

---

**文件版本**：1.0  
**最後更新**：2025-01-11  
**範例單據**：採購單（PurchaseOrder）
