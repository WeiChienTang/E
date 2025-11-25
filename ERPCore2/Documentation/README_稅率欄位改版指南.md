# 稅率欄位改版指南

## 📋 概述

本文件說明如何將單據從「統一稅率」改為「明細獨立稅率」+ 「主檔稅率算法」的完整步驟。

### 改版目的
- **舊設計**：整張單據使用系統統一稅率（5%）計算稅額
- **新設計（兩層架構）**：
  1. **主檔層**：增加「稅率算法」欄位（外加稅/內含稅/不含稅）
  2. **明細層**：每筆明細可設定獨立稅率，支援不同商品不同稅率的需求

### 適用範圍
以下單據已完成或需要進行稅率改版：
- ✅ **採購單（PurchaseOrder）** - 已完成改版（範例）
- ⏳ **進貨單（PurchaseReceiving）** - 待改版
- ⏳ **進貨退出單（PurchaseReturn）** - 待改版
- ⏳ **報價單（Quotation）** - 待改版
- ⏳ **銷貨單（SalesDelivery）** - 待改版
- ⏳ **銷貨訂單（SalesOrder）** - 待改版

---

## 🔧 改版四步驟

### **步驟 0：主檔增加稅率算法欄位（新增）**

#### 0.1 建立稅率算法 Enum

**檔案位置**：`Data/Enums/TaxCalculationMethod.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 稅率計算方式
    /// </summary>
    public enum TaxCalculationMethod
    {
        /// <summary>
        /// 外加稅 - 稅額 = 金額 × 稅率，總計 = 金額 + 稅額
        /// </summary>
        [Display(Name = "外加稅")]
        TaxExclusive = 1,

        /// <summary>
        /// 內含稅 - 總計已含稅，稅額 = 總計 / (1 + 稅率) × 稅率
        /// </summary>
        [Display(Name = "內含稅")]
        TaxInclusive = 2,

        /// <summary>
        /// 不含稅 - 不計算稅額，稅額為 0
        /// </summary>
        [Display(Name = "不含稅")]
        NoTax = 3
    }
}
```

#### 0.2 修改主檔實體（Main Entity）

**檔案位置**：`Data/Entities/Purchase/PurchaseOrder.cs`（以採購單為例）

在主檔實體中增加 `TaxCalculationMethod` 欄位：

```csharp
using ERPCore2.Data.Enums;

public class PurchaseOrder : BaseEntity
{
    // ... 其他欄位 ...
    
    [Display(Name = "預計到貨日期")]
    public DateTime? ExpectedDeliveryDate { get; set; }

    [Required(ErrorMessage = "稅率算法為必填")]
    [Display(Name = "稅率算法")]
    public TaxCalculationMethod TaxCalculationMethod { get; set; } = TaxCalculationMethod.TaxExclusive;  // 👈 新增此欄位，預設為外加稅

    [MaxLength(100, ErrorMessage = "採購人員不可超過100個字元")]
    [Display(Name = "採購人員")]
    public string? PurchasePersonnel { get; set; }
    
    // ... 其他欄位 ...
}
```

**重點說明**：
- 欄位類型：`TaxCalculationMethod`（Enum，非 nullable）
- 預設值：`TaxCalculationMethod.TaxExclusive`（外加稅）
- 必填欄位：確保每張單據都有明確的稅率算法

#### 0.3 執行 Migration

```powershell
# 在專案根目錄執行
dotnet ef migrations add AddTaxCalculationMethodToPurchaseOrder
dotnet ef database update
```

#### 0.4 修改 EditModal 組件 - 增加下拉選項

**檔案位置**：`Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor`

**步驟 A：增加選項清單變數**

在組件的變數宣告區域加入：

```csharp
// 下拉選單選項（向下相容）
private List<Supplier> suppliers = new();
private List<Company> companies = new();
private List<SelectOption> supplierOptions = new();
private List<SelectOption> companyOptions = new();
private List<SelectOption> statusOptions = new();
private List<SelectOption> taxCalculationMethodOptions = new();  // 👈 新增此行
```

**步驟 B：在 LoadAdditionalDataAsync 方法中初始化選項**

```csharp
private async Task LoadAdditionalDataAsync()
{
    try
    {
        // ... 前面的程式碼（載入廠商、公司等）...
        
        // 初始化狀態選項
        statusOptions = new List<SelectOption>
        {
            new SelectOption { Text = "啟用", Value = "Active" },
            new SelectOption { Text = "停用", Value = "Inactive" }
        };
        
        // 👇 新增：初始化稅率算法選項
        taxCalculationMethodOptions = new List<SelectOption>
        {
            new SelectOption { Text = "外加稅", Value = ((int)TaxCalculationMethod.TaxExclusive).ToString() },
            new SelectOption { Text = "內含稅", Value = ((int)TaxCalculationMethod.TaxInclusive).ToString() },
            new SelectOption { Text = "不含稅", Value = ((int)TaxCalculationMethod.NoTax).ToString() }
        };
    }
    catch (Exception ex)
    {
        // ... 錯誤處理 ...
        
        // 設定安全的預設值
        supplierOptions = new List<SelectOption>();
        companyOptions = new List<SelectOption>();
        statusOptions = new List<SelectOption>();
        taxCalculationMethodOptions = new List<SelectOption>();  // 👈 新增此行
        availableProducts = new List<Product>();
    }
}
```

**步驟 C：在 InitializeFormFieldsAsync 方法中增加表單欄位**

```csharp
private async Task InitializeFormFieldsAsync()
{
    try
    {
        // 使用 ApprovalConfigHelper 統一判斷是否鎖定欄位
        var shouldLock = ApprovalConfigHelper.ShouldLockFieldByApproval(
            isApprovalEnabled,
            editModalComponent?.Entity?.IsApproved ?? false,
            hasUndeletableDetails
        );
        
        formFields = new List<FormFieldDefinition>
        {
            // ... 前面的欄位（單號、廠商、公司、日期等）...
            
            new()
            {
                PropertyName = nameof(PurchaseOrder.ExpectedDeliveryDate),
                Label = "交貨日",
                FieldType = FormFieldType.Date,
                IsRequired = true,
                HelpText = "預計廠商交貨的日期",
                IsReadOnly = shouldLock
            },
            // 👇 新增：稅率算法欄位
            new()
            {
                PropertyName = nameof(PurchaseOrder.TaxCalculationMethod),
                Label = "稅率算法",
                FieldType = FormFieldType.Select,
                IsRequired = true,
                Options = taxCalculationMethodOptions,  // 👈 關鍵：設定下拉選項
                HelpText = "選擇稅額計算方式：外加稅（稅額另計）、內含稅（總額已含稅）、不含稅（免稅）",
                IsReadOnly = shouldLock
            },
            // ... 後面的欄位（總金額、稅額等）...
        };

        formSections = FormSectionHelper<PurchaseOrder>.Create()
            .AddToSection(FormSectionNames.BasicInfo,
                po => po.Code,
                po => po.SupplierId,
                po => po.CompanyId,
                po => po.OrderDate,
                po => po.ExpectedDeliveryDate,
                po => po.TaxCalculationMethod)  // 👈 新增：加入區段
            .AddToSection(FormSectionNames.AmountInfoAutoCalculated,
                po => po.TotalAmount,
                po => po.PurchaseTaxAmount,
                po => po.PurchaseTotalAmountIncludingTax)
            // ... 其他區段 ...
            .Build();
    }
    catch (Exception ex)
    {
        // ... 錯誤處理 ...
    }
}
```

**⚠️ 重要提醒**：
- 必須設定 `Options = taxCalculationMethodOptions`，否則下拉選單會是空的
- Enum 值需要轉換為字串：`((int)TaxCalculationMethod.TaxExclusive).ToString()`
- 欄位應該加入「基本資訊」區段，與日期欄位放在一起

---

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

#### 2.1 修改 Table 組件 - 增加參數和欄位

**檔案位置**：`Components/Shared/BaseModal/Modals/Purchase/PurchaseOrderTable.razor`

**步驟 A：增加稅率算法參數**

在組件的參數區域加入：

```csharp
// ===== 稅率算法參數（新增）=====
[Parameter] public TaxCalculationMethod TaxCalculationMethod { get; set; } = TaxCalculationMethod.TaxExclusive;

// ===== 輔助計算屬性 =====
private bool IsTaxCalculationMethodNoTax => TaxCalculationMethod == TaxCalculationMethod.NoTax;
```

**步驟 B：在 GetColumnDefinitions() 方法中增加稅率欄位**

```csharp
private List<InteractiveColumnDefinition> GetColumnDefinitions()
{
    var columns = new List<InteractiveColumnDefinition>();
    
    // ... 前面的欄位（商品、數量、單價等）...
    
    // 稅率欄位（可編輯 Number 類型）
    columns.Add(new()
    {
        Title = "稅率%",
        PropertyName = "TaxRate",
        ColumnType = InteractiveColumnType.Number,  // 👈 改為 Number 類型，可編輯
        Width = "80px",
        Tooltip = "商品的稅率（0% ~ 100%），可手動調整。當主檔選擇免稅時此欄位將被禁用",
        IsDisabledFunc = item =>
        {
            var productItem = (ProductItem)item;
            // 當主檔選擇免稅時，禁用稅率欄位
            return IsReadOnly || IsTaxCalculationMethodNoTax || 
                   !DetailLockHelper.CanDeleteItem(productItem, out _, checkReceiving: true);
        },
        TooltipFunc = item =>
        {
            var productItem = (ProductItem)item;
            if (IsTaxCalculationMethodNoTax) return "主檔已選擇免稅，此欄位已停用";
            if (!DetailLockHelper.CanDeleteItem(productItem, out _, checkReceiving: true))
                return "此商品已有進貨記錄，無法修改稅率";
            return null;
        },
        OnInputChanged = EventCallback.Factory.Create<(object, string?)>(this, async args =>
        {
            var (item, valueString) = args;
            await OnTaxRateInput((ProductItem)item, valueString);  // 👈 新增輸入處理方法
        })
    });
    
    // 小計欄位（根據稅率算法動態計算）
    columns.Add(new()
    {
        Title = "小計",
        Tooltip = GetSubtotalTooltip(),  // 👈 動態提示文字
        PropertyName = "",
        ColumnType = InteractiveColumnType.Custom,
        Width = "120px",
        CustomTemplate = item =>
        {
            var productItem = (ProductItem)item;
            var subtotal = CalculateItemSubtotal(productItem);  // 👈 調用計算方法
            var displayValue = NumberFormatHelper.FormatSmartZeroAsEmpty(subtotal);
            return @<div class="text-end fw-bold text-success">@displayValue</div>;
        }
    });
    
    // ... 後面的欄位（備註等）...
    
    return columns;
}
```

**步驟 C：增加稅率輸入處理方法**

```csharp
/// <summary>
/// 處理稅率輸入
/// </summary>
private async Task OnTaxRateInput(ProductItem item, string? valueString)
{
    if (string.IsNullOrWhiteSpace(valueString))
    {
        item.TaxRate = 0;
    }
    else if (decimal.TryParse(valueString, out var taxRate))
    {
        // 限制範圍 0 ~ 100
        item.TaxRate = Math.Max(0, Math.Min(100, taxRate));
    }
    
    await NotifyDetailsChanged();
}
```

**步驟 D：增加小計計算方法（支援三種稅率算法，四捨五入到整數）**

```csharp
/// <summary>
/// 計算明細項目的小計（根據稅率算法，四捨五入到整數）
/// </summary>
private decimal CalculateItemSubtotal(ProductItem item)
{
    if (item.SelectedProduct == null || item.Quantity <= 0 || item.Price <= 0)
    {
        return 0;
    }
    
    var baseAmount = item.Quantity * item.Price;
    
    switch (TaxCalculationMethod)
    {
        case TaxCalculationMethod.TaxExclusive:
            // 外加稅：小計 = 數量 × 單價 × (1 + 稅率%)（四捨五入到整數）
            return Math.Round(baseAmount * (1 + item.TaxRate / 100m), 0, MidpointRounding.AwayFromZero);
            
        case TaxCalculationMethod.TaxInclusive:
            // 內含稅：小計 = 數量 × 單價（單價已含稅，四捨五入到整數）
            return Math.Round(baseAmount, 0, MidpointRounding.AwayFromZero);
            
        case TaxCalculationMethod.NoTax:
            // 免稅：小計 = 數量 × 單價（四捨五入到整數）
            return Math.Round(baseAmount, 0, MidpointRounding.AwayFromZero);
            
        default:
            return Math.Round(baseAmount, 0, MidpointRounding.AwayFromZero);
    }
}

/// <summary>
/// 取得小計欄位的提示文字
/// </summary>
private string GetSubtotalTooltip()
{
    return TaxCalculationMethod switch
    {
        TaxCalculationMethod.TaxExclusive => "外加稅：數量 × 單價 × (1 + 稅率%)（四捨五入到整數）",
        TaxCalculationMethod.TaxInclusive => "內含稅：數量 × 單價（單價已含稅，四捨五入到整數）",
        TaxCalculationMethod.NoTax => "免稅：數量 × 單價（四捨五入到整數）",
        _ => "數量 × 單價（四捨五入到整數）"
    };
}
```

**⚠️ 重要說明：四捨五入規則**
- 所有金額和稅額都必須四捨五入到整數（0 位小數）
- 使用 `Math.Round(value, 0, MidpointRounding.AwayFromZero)`
- 這確保符合台灣稅務規定（稅額不可有小數點）

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
        // ⚠️ 重要：載入現有明細時，稅率優先順序必須正確！
        // 錯誤寫法：taxRate ?? defaultTaxRate（會跳過商品稅率）
        // 正確寫法：taxRate ?? product?.TaxRate ?? defaultTaxRate
        
        var taxRate = GetPropertyValue<decimal?>(detail, "TaxRate");
        var defaultTaxRate = await SystemParameterService.GetTaxRateAsync();
        
        var item = new ProductItem
        {
            // ... 其他屬性 ...
            // 優先順序：明細稅率 > 商品稅率 > 系統預設值
            TaxRate = taxRate ?? item.SelectedProduct?.TaxRate ?? defaultTaxRate,
            // ... 其他屬性 ...
        };
        ProductItems.Add(item);
    }
}
```

**⚠️ 常見錯誤：漏掉商品稅率檢查**

```csharp
// ❌ 錯誤：直接從明細稅率跳到系統預設值
TaxRate = purchaseDetail.TaxRate ?? defaultTaxRate

// ✅ 正確：明細 > 商品 > 系統預設
TaxRate = purchaseDetail.TaxRate ?? purchaseDetail.Product?.TaxRate ?? defaultTaxRate
```

**實際案例**：
- 商品主檔設定稅率 = 3%
- 系統預設稅率 = 5%
- 明細的 TaxRate 欄位 = NULL（舊資料或新建明細）

如果只寫 `purchaseDetail.TaxRate ?? defaultTaxRate`，會直接使用 5%（系統預設），**忽略商品的 3% 稅率**，導致稅額計算錯誤！

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

#### 2.6 A單轉B單時的稅率設定

**⚠️ 重要：轉單功能也要設定稅率！**

如果有「A單轉B單」功能（例如：採購單轉進貨單），在載入未完成項目時，也要正確設定稅率。

**檔案位置**：例如 `PurchaseReceivingTable.razor` 的 `LoadUnreceivedItemsInternal` 方法

```csharp
private async Task LoadUnreceivedItemsInternal(List<PurchaseOrderDetail> details)
{
    try
    {
        ReceivingItems.Clear();

        foreach (var detail in details)
        {
            // 獲取該採購明細的預設倉庫
            var defaultWarehouse = GetDefaultWarehouse(detail);
            
            // ⚠️ 重要：取得稅率（優先順序：採購明細 > 商品 > 系統預設）
            var taxRate = detail.TaxRate ?? detail.Product?.TaxRate ?? await SystemParameterService.GetTaxRateAsync();
            
            var receivingItem = new ReceivingItem
            {
                SelectedPurchaseDetail = detail,
                SelectedProduct = detail.Product,
                PurchaseDetailSearchValue = FormatPurchaseDetailDisplayText(detail),
                ReceivedQuantity = detail.OrderQuantity - detail.ReceivedQuantity,
                UnitPrice = detail.UnitPrice,
                TaxRate = taxRate,  // 👈 必須設定此屬性！
                
                SelectedWarehouse = defaultWarehouse,
                SelectedWarehouseLocation = GetDefaultWarehouseLocation(defaultWarehouse)
            };
            
            ReceivingItems.Add(receivingItem);
        }
        
        await NotifyDetailsChanged();
    }
    catch (Exception ex)
    {
        // ... 錯誤處理 ...
    }
}
```

**常見錯誤**：
```csharp
// ❌ 錯誤：忘記設定 TaxRate，會使用類別預設值 5.0m
var receivingItem = new ReceivingItem
{
    SelectedProduct = detail.Product,
    UnitPrice = detail.UnitPrice,
    // 缺少 TaxRate = ... 這一行！
};
```

**影響**：
- 從採購單轉進貨單時，即使商品稅率是 3%，也會顯示系統預設的 5%
- 使用者必須手動修改稅率，造成操作不便
- 可能導致稅額計算錯誤

---

### **步驟 3：EditModal 傳遞稅率算法並改用明細稅率計算**

#### 3.1 EditModal 傳遞稅率算法給 Table 組件

**檔案位置**：`Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor`

在 Table 組件標籤中傳遞 `TaxCalculationMethod` 參數：

```csharp
<PurchaseOrderTable @ref="purchaseOrderDetailManager"
                   TMainEntity="PurchaseOrder"
                   TDetailEntity="PurchaseOrderDetail"
                   Products="@filteredProductsBySupplier"
                   SelectedSupplierId="@editModalComponent?.Entity?.SupplierId"
                   MainEntity="@editModalComponent?.Entity"
                   ExistingDetails="@existingPurchaseOrderDetails"
                   OnDetailsChanged="@HandleDetailsChanged"
                   TaxCalculationMethod="@editModalComponent.Entity.TaxCalculationMethod"  // 👈 新增此行
                   MainEntityIdPropertyName="@nameof(PurchaseOrderDetail.PurchaseOrderId)"
                   QuantityPropertyName="@nameof(PurchaseOrderDetail.OrderQuantity)"
                   ReceivedQuantityPropertyName="@nameof(PurchaseOrderDetail.ReceivedQuantity)"
                   UnitPricePropertyName="@nameof(PurchaseOrderDetail.UnitPrice)"
                   RemarksPropertyName="@nameof(PurchaseOrderDetail.Remarks)"
                   UnitIdPropertyName="@nameof(PurchaseOrderDetail.UnitId)"
                   IsReceivingCompletedPropertyName="@nameof(PurchaseOrderDetail.IsReceivingCompleted)"
                   IsReadOnly="@shouldLock"
                   IsApproved="@(editModalComponent?.Entity?.IsApproved ?? false)"
                   HasUndeletableDetails="@hasUndeletableDetails" />
```

#### 3.2 修改 HandleDetailsChanged 方法 - 支援三種稅率算法

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
            var taxMethod = editModalComponent.Entity.TaxCalculationMethod;
            
            switch (taxMethod)
            {
                case TaxCalculationMethod.TaxExclusive:
                    // 外加稅：金額 = 明細小計（四捨五入到整數），稅額 = 金額 × 稅率%（四捨五入到整數）
                    editModalComponent.Entity.TotalAmount = Math.Round(purchaseOrderDetails.Sum(d => d.SubtotalAmount), 0, MidpointRounding.AwayFromZero);
                    editModalComponent.Entity.PurchaseTaxAmount = purchaseOrderDetails.Sum(d => 
                    {
                        var detailTaxRate = d.TaxRate ?? currentTaxRate;
                        var detailTaxAmount = d.SubtotalAmount * (detailTaxRate / 100m);
                        return Math.Round(detailTaxAmount, 0, MidpointRounding.AwayFromZero);
                    });
                    break;
                    
                case TaxCalculationMethod.TaxInclusive:
                    // 內含稅：總額 = 明細小計，金額 = 總額 / (1 + 稅率%)（四捨五入到整數），稅額 = 總額 - 金額
                    var totalWithTax = purchaseOrderDetails.Sum(d => d.SubtotalAmount);
                    var totalTax = purchaseOrderDetails.Sum(d =>
                    {
                        var detailTaxRate = d.TaxRate ?? currentTaxRate;
                        // 反推稅額 = 含稅總額 / (1 + 稅率%) × 稅率%（四捨五入到整數）
                        var detailTaxAmount = d.SubtotalAmount / (1 + detailTaxRate / 100m) * (detailTaxRate / 100m);
                        return Math.Round(detailTaxAmount, 0, MidpointRounding.AwayFromZero);
                    });
                    editModalComponent.Entity.TotalAmount = Math.Round(totalWithTax - totalTax, 0, MidpointRounding.AwayFromZero);
                    editModalComponent.Entity.PurchaseTaxAmount = totalTax;
                    break;
                    
                case TaxCalculationMethod.NoTax:
                    // 免稅：金額 = 明細小計（四捨五入到整數），稅額 = 0
                    editModalComponent.Entity.TotalAmount = Math.Round(purchaseOrderDetails.Sum(d => d.SubtotalAmount), 0, MidpointRounding.AwayFromZero);
                    editModalComponent.Entity.PurchaseTaxAmount = 0;
                    break;
                    
                default:
                    // 預設使用外加稅（四捨五入到整數）
                    editModalComponent.Entity.TotalAmount = Math.Round(purchaseOrderDetails.Sum(d => d.SubtotalAmount), 0, MidpointRounding.AwayFromZero);
                    editModalComponent.Entity.PurchaseTaxAmount = purchaseOrderDetails.Sum(d => 
                    {
                        var detailTaxRate = d.TaxRate ?? currentTaxRate;
                        var detailTaxAmount = d.SubtotalAmount * (detailTaxRate / 100m);
                        return Math.Round(detailTaxAmount, 0, MidpointRounding.AwayFromZero);
                    });
                    break;
            }
            
            // 3. 含稅總金額會自動計算（PurchaseTotalAmountIncludingTax 是計算屬性）
            //    = TotalAmount + PurchaseTaxAmount
            
            StateHasChanged();
        }
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleDetailsChanged), GetType(), 
            additionalData: "處理採購明細變更失敗");
    }
}
```

#### 3.3 修改 SavePurchaseOrderWithDetails 方法

確保儲存時也使用相同算法：

```csharp
private async Task<bool> SavePurchaseOrderWithDetails(PurchaseOrder purchaseOrder, bool isPreApprovalSave = false)
{
    try
    {
        // 更新總金額和稅額（使用與 HandleDetailsChanged 相同的邏輯）
        var taxMethod = purchaseOrder.TaxCalculationMethod;
        
        switch (taxMethod)
        {
            case TaxCalculationMethod.TaxExclusive:
                // 外加稅：金額 = 明細小計（四捨五入到整數），稅額 = 金額 × 稅率%（四捨五入到整數）
                purchaseOrder.TotalAmount = Math.Round(purchaseOrderDetails.Sum(d => d.SubtotalAmount), 0, MidpointRounding.AwayFromZero);
                purchaseOrder.PurchaseTaxAmount = purchaseOrderDetails.Sum(d => 
                {
                    var detailTaxRate = d.TaxRate ?? currentTaxRate;
                    var detailTaxAmount = d.SubtotalAmount * (detailTaxRate / 100m);
                    return Math.Round(detailTaxAmount, 0, MidpointRounding.AwayFromZero);
                });
                break;
                
            case TaxCalculationMethod.TaxInclusive:
                // 內含稅：總額 = 明細小計，金額 = 總額 / (1 + 稅率%)（四捨五入到整數），稅額 = 總額 - 金額
                var totalWithTax = purchaseOrderDetails.Sum(d => d.SubtotalAmount);
                var totalTax = purchaseOrderDetails.Sum(d =>
                {
                    var detailTaxRate = d.TaxRate ?? currentTaxRate;
                    var detailTaxAmount = d.SubtotalAmount / (1 + detailTaxRate / 100m) * (detailTaxRate / 100m);
                    return Math.Round(detailTaxAmount, 0, MidpointRounding.AwayFromZero);
                });
                purchaseOrder.TotalAmount = Math.Round(totalWithTax - totalTax, 0, MidpointRounding.AwayFromZero);
                purchaseOrder.PurchaseTaxAmount = totalTax;
                break;
                
            case TaxCalculationMethod.NoTax:
                // 免稅：金額 = 明細小計（四捨五入到整數），稅額 = 0
                purchaseOrder.TotalAmount = Math.Round(purchaseOrderDetails.Sum(d => d.SubtotalAmount), 0, MidpointRounding.AwayFromZero);
                purchaseOrder.PurchaseTaxAmount = 0;
                break;
                
            default:
                // 預設使用外加稅（四捨五入到整數）
                purchaseOrder.TotalAmount = Math.Round(purchaseOrderDetails.Sum(d => d.SubtotalAmount), 0, MidpointRounding.AwayFromZero);
                purchaseOrder.PurchaseTaxAmount = purchaseOrderDetails.Sum(d => 
                {
                    var detailTaxRate = d.TaxRate ?? currentTaxRate;
                    var detailTaxAmount = d.SubtotalAmount * (detailTaxRate / 100m);
                    return Math.Round(detailTaxAmount, 0, MidpointRounding.AwayFromZero);
                });
                break;
        }
        
        // ... 儲存邏輯 ...
    }
    catch (Exception ex)
    {
        // ... 錯誤處理 ...
    }
}
```

#### 3.4 增加稅率算法變更時的連動更新

在 `OnFieldValueChanged` 方法中增加處理：

```csharp
private async Task OnFieldValueChanged(FieldChangeEvent fieldChange)
{
    try
    {
        // ... 其他欄位的處理 ...
        
        // 當稅率算法變更時，重新計算金額、稅額、總額
        else if (fieldChange.PropertyName == nameof(PurchaseOrder.TaxCalculationMethod))
        {
            // 觸發明細重新計算
            await HandleDetailsChanged(purchaseOrderDetails);
            StateHasChanged();
        }
    }
    catch (Exception ex)
    {
        // ... 錯誤處理 ...
    }
}
```

**⚠️ 重要提醒**：
- 必須在 **兩個地方** 都使用相同的計算邏輯（HandleDetailsChanged 和 SavePurchaseOrderWithDetails）
- 當主檔的 TaxCalculationMethod 改變時，要觸發 HandleDetailsChanged 重新計算
- 避免一個地方用新算法，另一個用舊算法，導致儲存後稅額錯誤

---

## 📊 計算範例

### 範例 1：外加稅（TaxExclusive）- 稅額另外加上

**主檔設定**：稅率算法 = 外加稅

| 商品 | 數量 | 單價 | 稅率 | 小計（未稅）| 稅額 | 小計（含稅）|
|------|------|------|------|------------|------|-------------|
| A商品 | 10 | 100 | 5% | 1,000 | 50 | 1,050 |
| B商品 | 5 | 200 | 10% | 1,000 | 100 | 1,100 |
| **合計** | | | | **2,000** | **150** | **2,150** |

**計算公式**：
```csharp
// 明細層（Table 顯示）
明細小計含稅 = 數量 × 單價 × (1 + 稅率%)
  A商品: 10 × 100 × (1 + 5%) = 1,050
  B商品: 5 × 200 × (1 + 10%) = 1,100

// 主檔層（EditModal 計算）
金額（未稅）= Σ(數量 × 單價) = 1,000 + 1,000 = 2,000
稅額 = Σ(小計 × 稅率%) = (1,000 × 5%) + (1,000 × 10%) = 50 + 100 = 150
總額（含稅）= 金額 + 稅額 = 2,000 + 150 = 2,150
```

---

### 範例 2：內含稅（TaxInclusive）- 總價已包含稅

**主檔設定**：稅率算法 = 內含稅

| 商品 | 數量 | 單價 | 稅率 | 小計（含稅）| 稅額 | 金額（未稅）|
|------|------|------|------|------------|------|-------------|
| A商品 | 10 | 100 | 5% | 1,000 | 48 | 952 |
| B商品 | 5 | 200 | 10% | 1,000 | 91 | 909 |
| **合計** | | | | **2,000** | **139** | **1,861** |

**計算公式**：
```csharp
// 明細層（Table 顯示）
明細小計 = 數量 × 單價（單價已含稅，四捨五入到整數）
  A商品: Math.Round(10 × 100, 0) = 1,000
  B商品: Math.Round(5 × 200, 0) = 1,000

// 主檔層（EditModal 反推計算）
總額（含稅）= Σ(數量 × 單價) = 1,000 + 1,000 = 2,000
稅額 = Σ(小計 / (1 + 稅率%) × 稅率%)（每筆四捨五入到整數）
     = Math.Round(1,000 / 1.05 × 5%, 0) + Math.Round(1,000 / 1.10 × 10%, 0)
     = Math.Round(47.62, 0) + Math.Round(90.91, 0)
     = 48 + 91 = 139
金額（未稅）= Math.Round(總額 - 稅額, 0) = Math.Round(2,000 - 139, 0) = 1,861
```

---

### 範例 3：免稅（NoTax）- 完全不計稅

**主檔設定**：稅率算法 = 不含稅

| 商品 | 數量 | 單價 | 稅率 | 小計 | 稅額 | 總額 |
|------|------|------|------|------|------|------|
| A商品 | 10 | 100 | ~~5%~~ | 1,000 | 0 | 1,000 |
| B商品 | 5 | 200 | ~~10%~~ | 1,000 | 0 | 1,000 |
| **合計** | | | | **2,000** | **0** | **2,000** |

**計算公式**：
```csharp
// 明細層（Table 顯示）
明細小計 = 數量 × 單價
  A商品: 10 × 100 = 1,000
  B商品: 5 × 200 = 1,000
// 稅率欄位被禁用，不顯示稅率

// 主檔層（EditModal 計算）
金額 = Σ(數量 × 單價) = 1,000 + 1,000 = 2,000
稅額 = 0（免稅）
總額 = 金額 + 稅額 = 2,000 + 0 = 2,000
```

---

### 範例 4：新式算法 vs 舊式算法的差異

**情境**：商品有不同稅率

| 商品 | 數量 | 單價 | 稅率 | 小計（未稅） |
|------|------|------|------|------------|
| A商品 | 10 | 100 | 5% | 1,000 |
| B商品 | 5 | 200 | 10% | 1,000 |
| **合計** | | | | **2,000** |

**新式算法（正確）- 每筆明細分別計算**：
```csharp
TotalAmount = 1,000 + 1,000 = 2,000
PurchaseTaxAmount = (1,000 × 5%) + (1,000 × 10%) = 50 + 100 = 150  ✅
PurchaseTotalAmountIncludingTax = 2,000 + 150 = 2,150
```

**舊式算法（錯誤）- 統一稅率計算**：
```csharp
TotalAmount = 2,000
PurchaseTaxAmount = 2,000 × 5% = 100  ❌ 少算了 B 商品的額外 5% 稅額
PurchaseTotalAmountIncludingTax = 2,100  ❌ 錯誤
```

**結論**：新式算法支援每筆明細獨立稅率，更符合實務需求（例如：免稅商品混搭應稅商品）。

---

## 🎯 改版檢查清單

### ✅ 步驟 0：主檔稅率算法檢查（新增）
- [ ] 建立 `TaxCalculationMethod` Enum（三種選項：外加稅/內含稅/不含稅）
- [ ] 主檔實體增加 `TaxCalculationMethod` 欄位（非 nullable，預設為外加稅）
- [ ] 引入 `using ERPCore2.Data.Enums;` 命名空間
- [ ] 執行 Migration 並更新資料庫
- [ ] EditModal 組件增加 `taxCalculationMethodOptions` 變數
- [ ] `LoadAdditionalDataAsync` 方法中初始化選項清單
- [ ] `InitializeFormFieldsAsync` 方法中增加表單欄位（設定 `Options`）
- [ ] 表單欄位加入對應的區段（通常是「基本資訊」）

### ✅ 步驟 1：資料表檢查
- [ ] 明細實體增加 `TaxRate` 欄位（`decimal?` 類型）
- [ ] 執行 Migration 並更新資料庫
- [ ] 確認資料庫欄位正確建立

### ✅ 步驟 2：Table 組件檢查
- [ ] **增加 `TaxCalculationMethod` 參數**
- [ ] **增加 `IsTaxCalculationMethodNoTax` 計算屬性**
- [ ] `GetColumnDefinitions` 增加「稅率」欄位（**Number 類型，可編輯**）
- [ ] **稅率欄位設定 `IsDisabledFunc`（免稅時禁用）**
- [ ] **稅率欄位設定 `TooltipFunc`（動態提示）**
- [ ] **稅率欄位設定 `OnInputChanged` 事件處理**
- [ ] **增加 `OnTaxRateInput` 方法（處理稅率輸入）**
- [ ] **「小計」欄位改為調用 `CalculateItemSubtotal` 方法**
- [ ] **增加 `CalculateItemSubtotal` 方法（支援三種稅率算法）**
- [ ] **增加 `GetSubtotalTooltip` 方法（動態提示文字）**
- [ ] `ProductItem` 類別增加 `TaxRate` 屬性
- [ ] `OnProductSelected` 方法自動帶入商品稅率
- [ ] `LoadExistingDetailsAsync` 方法載入明細稅率（**⚠️ 優先順序：明細 > 商品 > 系統**）
- [ ] `ConvertToDetailEntities` 方法儲存明細稅率
- [ ] **如有 A單轉B單功能，檢查轉單方法是否設定稅率（例如 `LoadUnreceivedItemsInternal`）**

### ✅ 步驟 3：EditModal 檢查
- [ ] **傳遞 `TaxCalculationMethod` 參數給 Table 組件**
- [ ] **`HandleDetailsChanged` 方法改為 switch 語句（支援三種稅率算法）**
- [ ] **`SavePurchaseOrderWithDetails` 方法改為 switch 語句（與 HandleDetailsChanged 邏輯一致）**
- [ ] **`OnFieldValueChanged` 方法增加 `TaxCalculationMethod` 變更處理**
- [ ] 兩個方法的計算邏輯完全一致

### ✅ 測試檢查
- [ ] **新增單據時，稅率算法預設為外加稅**
- [ ] **切換稅率算法時，金額、稅額、總額立即更新**
- [ ] **選擇免稅時，明細稅率欄位被禁用**
- [ ] **選擇免稅時，主檔稅額顯示為 0**
- [ ] **外加稅計算正確：小計 = 數量 × 單價 × (1 + 稅率%)**
- [ ] **內含稅計算正確：小計 = 數量 × 單價（反推稅額）**
- [ ] **免稅計算正確：小計 = 數量 × 單價，稅額 = 0**
- [ ] 新增單據時，稅率自動帶入（商品稅率 > 系統預設值）
- [ ] 編輯單據時，稅率正確顯示
- [ ] **載入現有明細時，稅率優先順序正確（明細 > 商品 > 系統）**
- [ ] 儲存後稅額計算正確（不會被覆蓋）
- [ ] 混合不同稅率商品時，稅額計算正確
- [ ] 空行（未選商品）不顯示稅率
- [ ] **舊資料（TaxRate = NULL）能正確顯示商品稅率**
- [ ] **A單轉B單時，稅率正確轉移（例如：採購單3%轉進貨單也是3%）**

---

## 🔍 常見問題

### Q0：為什麼需要在主檔增加「稅率算法」欄位？
**A**：因為不同的交易情境需要不同的稅額處理方式：
- **外加稅**：稅額另外加上（例如：報價 $100，外加 5% 稅 = $105）
- **內含稅**：總價已包含稅（例如：零售價 $105 內含 5% 稅，未稅價 = $100）
- **不含稅**：完全免稅（例如：出口或特殊優惠）

單據層級的設定可以讓整張單據統一使用同一種計算方式，避免混淆。

### Q0-1：稅率算法下拉選單沒有資料怎麼辦？
**A**：這是最常見的問題！必須完成以下三個步驟：

1. **宣告變數**：`private List<SelectOption> taxCalculationMethodOptions = new();`
2. **初始化選項**（在 `LoadAdditionalDataAsync` 中）：
   ```csharp
   taxCalculationMethodOptions = new List<SelectOption>
   {
       new SelectOption { Text = "外加稅", Value = "1" },
       new SelectOption { Text = "內含稅", Value = "2" },
       new SelectOption { Text = "不含稅", Value = "3" }
   };
   ```
3. **設定欄位 Options**（在表單欄位定義中）：`Options = taxCalculationMethodOptions`

缺少任何一步都會導致下拉選單是空的！

### Q0-2：修改稅率算法後，金額沒有自動更新？
**A**：需要在 `OnFieldValueChanged` 方法中增加處理：

```csharp
else if (fieldChange.PropertyName == nameof(PurchaseOrder.TaxCalculationMethod))
{
    // 觸發明細重新計算
    await HandleDetailsChanged(purchaseOrderDetails);
    StateHasChanged();
}
```

這樣當使用者切換「外加稅/內含稅/不含稅」時，金額、稅額、總額會立即更新。

### Q0-3：選擇免稅時，明細稅率欄位如何禁用？
**A**：在 Table 組件中：

1. **增加參數**：`[Parameter] public TaxCalculationMethod TaxCalculationMethod { get; set; }`
2. **增加計算屬性**：`private bool IsTaxCalculationMethodNoTax => TaxCalculationMethod == TaxCalculationMethod.NoTax;`
3. **設定欄位禁用規則**：
   ```csharp
   IsDisabledFunc = item =>
   {
       return IsReadOnly || IsTaxCalculationMethodNoTax || /* 其他條件 */;
   }
   ```

### Q0-4：三種稅率算法的計算差異？
**A**：以商品 10個 × $100 = $1,000，稅率 5% 為例：

| 稅率算法 | 明細小計 | 主檔金額 | 主檔稅額 | 主檔總額 |
|---------|---------|---------|---------|---------|
| **外加稅** | $1,050 | $1,000 | $50 | $1,050 |
| **內含稅** | $1,000 | $952.38 | $47.62 | $1,000 |
| **免稅** | $1,000 | $1,000 | $0 | $1,000 |

- **外加稅**：明細顯示含稅價（給使用者看實付金額），主檔拆分為金額+稅額
- **內含稅**：明細顯示含稅價，主檔反推未稅金額（用於會計分錄）
- **免稅**：明細和主檔都只顯示金額，稅額為 0

### Q1：為何 TaxRate 要用 `decimal?` 而非 `decimal`？
**A**：使用 nullable 類型可以區分「未設定」和「設定為 0」兩種情況。當明細未設定稅率時，可自動使用系統預設值或商品主檔的稅率。

### Q2：稅率應該從哪裡取得？
**A**：優先順序如下：
1. **第一優先**：明細的稅率（`Detail.TaxRate`）
2. **第二優先**：商品主檔的稅率（`Product.TaxRate`）
3. **第三優先**：系統參數的預設稅率（`SystemParameter.TaxRate`）

### Q3：儲存後稅額顯示錯誤怎麼辦？
**A**：檢查是否有三個地方都使用正確的算法：
- `HandleDetailsChanged`（明細變更時）
- `SavePurchaseOrderWithDetails`（儲存時）
- `OnFieldValueChanged`（主檔欄位變更時）

確保三者邏輯一致，都使用 switch 語句根據 `TaxCalculationMethod` 計算。

### Q4：小計欄位應該顯示什麼？
**A**：根據稅率算法不同而不同：
- **外加稅**：顯示含稅金額（數量 × 單價 × (1 + 稅率%)）- 使用者更關心實付金額
- **內含稅**：顯示含稅金額（數量 × 單價）- 單價已含稅
- **免稅**：顯示未稅金額（數量 × 單價）- 無稅額

使用 `CalculateItemSubtotal` 方法統一處理，並用 `GetSubtotalTooltip` 動態顯示提示。

### Q5：舊資料的稅率欄位會是什麼值？
**A**：Migration 後，舊資料的 `TaxRate` 欄位為 `NULL`，`TaxCalculationMethod` 欄位預設為 `TaxExclusive`（外加稅）。程式會自動使用優先順序（明細 > 商品 > 系統預設值），確保向下相容。

### Q6：為什麼載入明細時顯示的是系統預設稅率，而不是商品稅率？
**A**：這是最常見的錯誤！在 `LoadExistingDetailsAsync` 方法中，必須檢查**完整的優先順序**：

```csharp
// ❌ 錯誤寫法（跳過商品稅率）
TaxRate = purchaseDetail.TaxRate ?? defaultTaxRate

// ✅ 正確寫法（完整優先順序）
TaxRate = purchaseDetail.TaxRate ?? purchaseDetail.Product?.TaxRate ?? defaultTaxRate
```

**檢查要點**：
1. 明細載入時，要從 Navigation Property 讀取商品資料
2. 如果明細的 `TaxRate` 是 NULL，先檢查 `Product.TaxRate`
3. 最後才使用系統預設值

**影響範圍**：所有有 `LoadExistingDetailsAsync` 或類似方法的組件都要檢查此問題。

### Q7：為什麼從採購單轉進貨單時，稅率顯示 5% 而不是採購單的 3%？
**A**：這是「A單轉B單」功能的常見錯誤！在轉單載入方法（如 `LoadUnreceivedItemsInternal`）中，必須設定稅率：

```csharp
// ❌ 錯誤：忘記設定 TaxRate
var receivingItem = new ReceivingItem
{
    SelectedProduct = detail.Product,
    UnitPrice = detail.UnitPrice,
    // 缺少 TaxRate 設定！會使用類別預設值 5.0m
};

// ✅ 正確：設定完整的稅率優先順序
var taxRate = detail.TaxRate ?? detail.Product?.TaxRate ?? await SystemParameterService.GetTaxRateAsync();
var receivingItem = new ReceivingItem
{
    SelectedProduct = detail.Product,
    UnitPrice = detail.UnitPrice,
    TaxRate = taxRate,  // 必須設定！
};
```

**檢查要點**：
1. 所有「載入項目」的方法都要設定稅率
2. 包括：`LoadExistingDetailsAsync`、`LoadUnreceivedItemsInternal`、`OnDetailSelected` 等
3. 稅率優先順序：來源明細 > 商品 > 系統預設

**影響範圍**：所有有轉單功能的組件（採購單→進貨單、報價單→銷貨單等）

### Q8：稅率欄位為什麼要用 Number 類型而非 Custom？
**A**：使用 `InteractiveColumnType.Number` 的優勢：
- 自動驗證數字格式（防止輸入文字）
- 支援小數點輸入（例如：5.5%）
- 提供 `OnInputChanged` 事件（即時更新）
- 行動裝置會顯示數字鍵盤

如果用 Custom 類型，需要自己實作所有輸入驗證和事件處理。

### Q9：為什麼要有 `CalculateItemSubtotal` 和 `GetSubtotalTooltip` 兩個方法？
**A**：
- **`CalculateItemSubtotal`**：根據稅率算法計算實際金額（給程式用）
- **`GetSubtotalTooltip`**：動態顯示計算公式說明（給使用者看）

當使用者切換稅率算法時，不僅數字會變，提示文字也會自動更新，提升 UX。

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

改版的核心概念（四層架構）：

1. **主檔層（新增）**：增加 `TaxCalculationMethod` 欄位，定義單據的稅額計算方式（外加稅/內含稅/不含稅）
2. **資料層**：明細表增加 `TaxRate` 欄位（nullable），支援每筆明細獨立稅率
3. **展示層**：Table 增加稅率欄位（Number 可編輯），小計根據稅率算法動態計算
4. **邏輯層**：EditModal 使用 switch 語句，根據稅率算法分別計算金額和稅額

**關鍵成功要素**：
- ✅ 主檔稅率算法 Enum 正確建立（外加稅/內含稅/不含稅）
- ✅ EditModal 下拉選項正確初始化（變數宣告 + 選項初始化 + Options 設定）
- ✅ Table 組件接收並使用 TaxCalculationMethod 參數
- ✅ 稅率欄位改為 Number 類型（可編輯）
- ✅ 稅率欄位在免稅時自動禁用
- ✅ 小計計算方法（CalculateItemSubtotal）支援三種算法
- ✅ 三個計算點使用相同邏輯（HandleDetailsChanged + SavePurchaseOrderWithDetails + OnFieldValueChanged）
- ✅ 優先使用商品稅率，回退到系統預設值
- ✅ 稅額和金額都四捨五入到整數（使用 `Math.Round(..., 0, MidpointRounding.AwayFromZero)`）
- ✅ 向下相容舊資料（`TaxRate = NULL` 時使用預設值）

**最容易遺漏的地方**：
- ❌ 忘記在 `LoadAdditionalDataAsync` 中初始化 `taxCalculationMethodOptions`
- ❌ 忘記在表單欄位中設定 `Options = taxCalculationMethodOptions`
- ❌ 忘記在安全預設值區塊加入 `taxCalculationMethodOptions = new List<SelectOption>();`
- ❌ 忘記在 Table 組件中增加 `TaxCalculationMethod` 參數
- ❌ 忘記在 `OnFieldValueChanged` 中處理稅率算法變更
- ❌ `HandleDetailsChanged` 和 `SavePurchaseOrderWithDetails` 使用不同的計算邏輯
- ❌ 稅率欄位用 Custom 類型而非 Number 類型
- ❌ 小計欄位寫死計算公式，而非調用 `CalculateItemSubtotal` 方法

**三種稅率算法的關鍵差異**：

| 項目 | 外加稅 | 內含稅 | 免稅 |
|-----|-------|-------|------|
| **明細小計** | 數量 × 單價 × (1 + 稅率%) | 數量 × 單價 | 數量 × 單價 |
| **主檔金額** | Σ小計未稅 | Σ小計 - Σ稅額 | Σ小計 |
| **主檔稅額** | Σ(小計未稅 × 稅率%) | Σ(小計 / (1+稅率%) × 稅率%) | 0 |
| **稅率欄位** | 可編輯 | 可編輯 | 禁用 |
| **使用情境** | 一般採購/銷售 | 零售價含稅 | 出口/免稅優惠 |

---

## 📌 快速檢查表（常見錯誤）

### 步驟 0：主檔稅率算法

| 檢查項目 | 檔案位置 | 檢查內容 |
|---------|---------|---------|
| ✅ Enum 是否建立 | `Data/Enums/TaxCalculationMethod.cs` | 是否有三個選項：TaxExclusive(1), TaxInclusive(2), NoTax(3) |
| ✅ 主檔欄位 | `Data/Entities/.../XXXOrder.cs` | 是否有 `TaxCalculationMethod` 欄位，預設值是否為 `TaxExclusive` |
| ✅ using 引用 | `Data/Entities/.../XXXOrder.cs` | 是否有 `using ERPCore2.Data.Enums;` |
| ✅ 選項變數 | `XXXOrderEditModalComponent.razor` | 是否宣告 `taxCalculationMethodOptions` |
| ✅ 選項初始化 | `LoadAdditionalDataAsync` 方法 | 是否建立三個 SelectOption（文字 + Value） |
| ✅ 選項預設值 | `LoadAdditionalDataAsync` catch 區塊 | 是否有 `taxCalculationMethodOptions = new();` |
| ✅ 欄位 Options | `InitializeFormFieldsAsync` 方法 | 欄位定義是否有 `Options = taxCalculationMethodOptions` |
| ✅ 欄位加入區段 | `FormSectionHelper` | 是否將欄位加入區段（通常是 BasicInfo） |
| ✅ Migration | 終端機 | 是否執行 `dotnet ef migrations add` 和 `database update` |

### 步驟 1：明細稅率欄位

| 檢查項目 | 檔案位置 | 檢查內容 |
|---------|---------|---------|
| ✅ 明細欄位 | `Data/Entities/.../XXXOrderDetail.cs` | 是否有 `TaxRate` 欄位（`decimal?` 類型）|
| ✅ 資料類型 | `Data/Entities/.../XXXOrderDetail.cs` | `[Column(TypeName = "decimal(5,2)")]` |
| ✅ 驗證範圍 | `Data/Entities/.../XXXOrderDetail.cs` | `[Range(0, 100, ErrorMessage = "...")]` |
| ✅ Migration | 終端機 | 是否執行 `dotnet ef migrations add` 和 `database update` |

### 步驟 2：Table 組件

| 檢查項目 | 檔案位置 | 檢查內容 |
|---------|---------|---------|
| ✅ 參數宣告 | `XXXOrderTable.razor` | 是否有 `[Parameter] public TaxCalculationMethod TaxCalculationMethod { get; set; }` |
| ✅ 計算屬性 | `XXXOrderTable.razor` | 是否有 `private bool IsTaxCalculationMethodNoTax => ...` |
| ✅ 稅率欄位類型 | `GetColumnDefinitions` | `ColumnType = InteractiveColumnType.Number`（**不是 Custom**） |
| ✅ 稅率欄位禁用 | `GetColumnDefinitions` | 是否有 `IsDisabledFunc`（免稅時禁用） |
| ✅ 稅率欄位提示 | `GetColumnDefinitions` | 是否有 `TooltipFunc`（動態提示） |
| ✅ 稅率輸入處理 | `GetColumnDefinitions` | 是否有 `OnInputChanged` 事件處理 |
| ✅ OnTaxRateInput 方法 | `XXXOrderTable.razor` | 是否實作 `OnTaxRateInput` 方法 |
| ✅ 小計計算方法 | `XXXOrderTable.razor` | 是否實作 `CalculateItemSubtotal` 方法（switch 三種算法） |
| ✅ 小計提示方法 | `XXXOrderTable.razor` | 是否實作 `GetSubtotalTooltip` 方法 |
| ✅ 小計欄位調用 | `GetColumnDefinitions` | 小計欄位是否調用 `CalculateItemSubtotal(productItem)` |
| ✅ ProductItem 屬性 | `XXXOrderTable.razor` | ProductItem 類別是否有 `TaxRate` 屬性 |
| ✅ 載入明細稅率 | `LoadExistingDetailsAsync` | 是否正確設定稅率優先順序（明細 > 商品 > 系統） |
| ✅ 儲存明細稅率 | `ConvertToDetailEntities` | 是否有 `SetPropertyValue(detail, "TaxRate", item.TaxRate)` |

### 步驟 3：EditModal 組件

| 檢查項目 | 檔案位置 | 檢查內容 |
|---------|---------|---------|
| ✅ 傳遞參數 | `XXXOrderEditModalComponent.razor` | Table 組件是否有 `TaxCalculationMethod="@editModalComponent.Entity.TaxCalculationMethod"` |
| ✅ HandleDetailsChanged | `XXXOrderEditModalComponent.razor` | 是否改為 switch 語句（三種算法） |
| ✅ SavePurchaseOrderWithDetails | `XXXOrderEditModalComponent.razor` | 是否改為 switch 語句（與 HandleDetailsChanged 一致） |
| ✅ OnFieldValueChanged | `XXXOrderEditModalComponent.razor` | 是否增加 `TaxCalculationMethod` 變更處理 |
| ✅ 邏輯一致性 | 兩個方法 | 兩個方法的計算邏輯是否完全相同 |

### 功能測試

| 測試項目 | 測試步驟 | 預期結果 |
|---------|---------|---------|
| ✅ 新增單據預設值 | 新增單據 | 稅率算法預設為「外加稅」 |
| ✅ 切換外加稅 | 選擇「外加稅」| 小計 = 數量 × 單價 × (1 + 稅率%)，金額、稅額、總額立即更新 |
| ✅ 切換內含稅 | 選擇「內含稅」| 小計 = 數量 × 單價，金額反推計算，稅額正確 |
| ✅ 切換免稅 | 選擇「不含稅」| 稅率欄位禁用，稅額 = 0 |
| ✅ 稅率欄位編輯 | 修改明細稅率 | 小計和主檔金額立即更新 |
| ✅ 稅率欄位驗證 | 輸入 -5 或 150 | 自動限制在 0~100 範圍 |
| ✅ 混合稅率計算 | 明細有 5% 和 10% | 稅額 = 分別計算後加總（不是統一稅率） |
| ✅ 儲存後金額 | 儲存後重新開啟 | 金額、稅額不變（不會被覆蓋） |
| ✅ 舊資料相容 | 載入舊單據 | 自動使用商品或系統預設稅率 |
| ✅ 動態提示 | Hover 小計欄位 | 顯示對應稅率算法的公式說明 |

---

**文件版本**：3.1  
**最後更新**：2025-11-25  
**範例單據**：採購單（PurchaseOrder）  
**主要更新**：
- 步驟 0：主檔增加稅率算法欄位（外加稅/內含稅/不含稅）
- 步驟 2：稅率欄位改為 Number 可編輯類型，免稅時自動禁用，小計四捨五入到整數
- 步驟 3：支援三種稅率算法的計算邏輯，金額和稅額都四捨五入到整數
- 增加完整的計算範例（外加稅/內含稅/免稅），所有金額都為整數
- 增加詳細的檢查清單和常見問題解答
- **重要**：所有金額和稅額計算都使用 `Math.Round(..., 0, MidpointRounding.AwayFromZero)` 四捨五入到整數
