# 各類單子轉沖款功能實作指南

## 概述

本文件記錄「轉沖款」功能的實作方式，用於將來源單據（如進貨單、銷貨單等）的未結清明細自動轉入沖款單進行沖銷。

### 功能目的
- 減少使用者重複輸入資料的時間
- 自動預填廠商/客戶資訊
- 自動載入來源單據的未結清商品明細
- 預設全額沖款，使用者可依需求調整

### 已實作的轉單功能
| 來源單據 | 目標單據 | 沖款類型 | 狀態 |
|---------|---------|---------|------|
| 進貨單 (PurchaseReceiving) | 沖款單 (SetoffDocument) | 應付帳款 | ✅ 已完成 |
| 銷貨單 (SalesOrder) | 沖款單 (SetoffDocument) | 應收帳款 | ⏳ 待實作 |

---

## 涉及的檔案

### 1. 來源單據編輯組件
- 路徑：`Components/Pages/[模組]/[來源單據]EditModalComponent.razor`
- 範例：`Components/Pages/Purchase/PurchaseReceivingEditModalComponent.razor`

### 2. 沖款單編輯組件
- 路徑：`Components/Pages/FinancialManagement/SetoffDocumentEditModalComponent.razor`

### 3. 沖款商品明細表
- 路徑：`Components/Pages/FinancialManagement/SetoffProductTable.razor`

---

## 實作步驟

### 步驟 1：在 SetoffProductTable.razor 新增載入方法

#### 1.1 新增篩選參數（如需要）
```csharp
// 轉單篩選參數（用於進貨轉沖款等場景）
[Parameter] public int? FilterPurchaseReceivingId { get; set; }
```

#### 1.2 新增相關 Service 注入
```csharp
@inject IPurchaseReceivingService PurchaseReceivingService
// 或
@inject ISalesOrderService SalesOrderService
```

#### 1.3 新增公開載入方法
```csharp
/// <summary>
/// 從進貨單載入未結清明細（用於「進貨轉沖款」轉單功能）
/// </summary>
/// <param name="purchaseReceivingId">進貨單ID</param>
public async Task LoadDetailsFromPurchaseReceiving(int purchaseReceivingId)
{
    try
    {
        // 1. 檢查關聯方是否已設定
        if (!RelatedPartyId.HasValue || RelatedPartyId.Value <= 0)
        {
            await NotificationService.ShowWarningAsync("請先選擇廠商");
            return;
        }
        
        // 2. 取得來源單據的編號（用於篩選）
        var purchaseReceiving = await PurchaseReceivingService.GetByIdAsync(purchaseReceivingId);
        if (purchaseReceiving == null)
        {
            await NotificationService.ShowWarningAsync("找不到指定的進貨單");
            return;
        }
        var purchaseReceivingCode = purchaseReceiving.Code;
        
        // 3. 確保已載入商品資料
        if (!Products.Any())
        {
            await LoadProductsAsync();
        }
        
        // 4. 載入該廠商的所有未結清明細
        await LoadUnsettledDetailsAsync();
        
        // 5. 篩選出指定來源單據的明細（透過單號比對）
        var matchingDetails = UnsettledDetails
            .Where(d => d.SourceDetailType == SetoffDetailType.PurchaseReceivingDetail)
            .Where(d => !string.IsNullOrEmpty(d.SourceDocumentNumber) && 
                       d.SourceDocumentNumber == purchaseReceivingCode)
            .ToList();
        
        // 6. 轉換為 SetoffItems，預設本次沖款為未沖款餘額（全額沖款）
        SetoffItems = matchingDetails
            .Select(detail => new SetoffProductDetailItem
            {
                SourceDetailType = detail.SourceDetailType,
                SourceDetailId = detail.SourceDetailId,
                ProductId = detail.ProductId,
                Product = Products.FirstOrDefault(p => p.Id == detail.ProductId),
                CurrentSetoffAmount = detail.RemainingAmount, // 預設全額沖款
                CurrentAllowanceAmount = 0,
                Remarks = null,
                ExistingDetail = null,
                UnsettledDetail = detail,
                NeedsSourceDetailLoad = false
            }).ToList();
        
        // 7. 標記已載入
        _dataLoaded = true;
        
        // 8. 顯示載入結果
        var loadedCount = SetoffItems.Count;
        if (loadedCount > 0)
        {
            await NotificationService.ShowSuccessAsync($"已載入 {loadedCount} 筆進貨明細", "載入完成");
        }
        else
        {
            await NotificationService.ShowInfoAsync("該進貨單沒有未結清的應付明細", "提示");
        }
        
        // 9. 通知父組件資料已變更
        await NotifyDetailsChanged();
        
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入進貨明細時發生錯誤：{ex.Message}");
    }
}
```

---

### 步驟 2：在 SetoffDocumentEditModalComponent.razor 新增預填參數和公開方法

#### 2.1 新增預填參數
```csharp
// ===== 預填參數（用於轉單功能）=====
[Parameter] public int? PrefilledSupplierId { get; set; }
[Parameter] public int? PrefilledPurchaseReceivingId { get; set; }
// 如果是銷貨轉沖款，則新增：
// [Parameter] public int? PrefilledCustomerId { get; set; }
// [Parameter] public int? PrefilledSalesOrderId { get; set; }
```

#### 2.2 新增內部狀態
```csharp
// 轉單自動載入控制
private bool shouldAutoLoadFromReceiving = false;
private int? filterPurchaseReceivingId = null;
```

#### 2.3 修改 LoadSetoffDocumentData 處理預填參數
在 `if (!SetoffDocumentId.HasValue)` 區塊內（新增模式），加入：

```csharp
// 處理預填參數（進貨轉沖款等場景）
if (PrefilledSupplierId.HasValue && PrefilledSupplierId.Value > 0)
{
    setoffDocument.RelatedPartyId = PrefilledSupplierId.Value;
    setoffDocument.RelatedPartyType = "Supplier";
    setoffDocument.SetoffType = SetoffType.AccountsPayable;
    
    // 設定進貨單篩選ID
    if (PrefilledPurchaseReceivingId.HasValue)
    {
        filterPurchaseReceivingId = PrefilledPurchaseReceivingId.Value;
    }
}

// 如果是銷貨轉沖款：
// if (PrefilledCustomerId.HasValue && PrefilledCustomerId.Value > 0)
// {
//     setoffDocument.RelatedPartyId = PrefilledCustomerId.Value;
//     setoffDocument.RelatedPartyType = "Customer";
//     setoffDocument.SetoffType = SetoffType.AccountsReceivable;
//     
//     if (PrefilledSalesOrderId.HasValue)
//     {
//         filterSalesOrderId = PrefilledSalesOrderId.Value;
//     }
// }
```

#### 2.4 新增公開方法
```csharp
// ===== 公開方法（供外部組件調用） =====

/// <summary>
/// 開啟新增沖款單 Modal 並預填進貨單資訊（公開方法供外部調用）
/// </summary>
/// <param name="supplierId">廠商ID</param>
/// <param name="purchaseReceivingId">進貨單ID</param>
public async Task ShowAddModalWithPrefilledReceiving(int supplierId, int purchaseReceivingId)
{
    try
    {
        // 使用 DocumentConversionHelper 統一處理轉單邏輯
        var success = await DocumentConversionHelper.ShowConversionModalAsync(
            setPrefilledValues: () =>
            {
                SetoffDocumentId = null;
                PrefilledSupplierId = supplierId;
                PrefilledPurchaseReceivingId = purchaseReceivingId;
                DefaultSetoffType = SetoffType.AccountsPayable;
                // 設定篩選ID
                filterPurchaseReceivingId = purchaseReceivingId;
                shouldAutoLoadFromReceiving = true;
            },
            isVisibleChanged: IsVisibleChanged,
            autoLoadAction: async () =>
            {
                shouldAutoLoadFromReceiving = false;
                if (setoffProductDetailManager != null)
                {
                    await setoffProductDetailManager.LoadDetailsFromPurchaseReceiving(purchaseReceivingId);
                }
            },
            detailManagerReady: () => setoffProductDetailManager != null,
            shouldAutoLoad: () => shouldAutoLoadFromReceiving,
            stateHasChangedAction: StateHasChanged,
            invokeAsync: InvokeAsync
        );

        if (!success)
        {
            throw new Exception("轉單流程執行失敗");
        }
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(ShowAddModalWithPrefilledReceiving), GetType(), 
            additionalData: $"開啟預填沖款單Modal失敗 - SupplierId: {supplierId}, ReceivingId: {purchaseReceivingId}");
        await NotificationService.ShowErrorAsync("開啟沖款單時發生錯誤");
    }
}
```

---

### 步驟 3：在來源單據編輯組件新增轉沖款按鈕和處理方法

#### 3.1 在 GetCustomActionButtons 新增按鈕
```csharp
private RenderFragment? GetCustomActionButtons() => __builder =>
{
    @if (editModalComponent?.Entity != null && PurchaseReceivingId.HasValue)
    {
        <GenericButtonComponent Text="轉退出" 
                              Variant="ButtonVariant.Yellow" 
                              OnClick="HandleCreateReturnFromReceiving" />
        <GenericButtonComponent Text="轉沖款" 
                              Variant="ButtonVariant.Green" 
                              OnClick="HandleCreateSetoffFromReceiving" />
    }
};
```

#### 3.2 新增處理方法
```csharp
/// <summary>
/// 從進貨單轉沖款單
/// </summary>
private async Task HandleCreateSetoffFromReceiving()
{
    try
    {
        // 1. 驗證來源單據資料
        if (!PurchaseReceivingId.HasValue || editModalComponent?.Entity == null)
        {
            await NotificationService.ShowWarningAsync("無法轉沖款：進貨單資料不存在");
            return;
        }
        
        // 2. 驗證關聯方資料（廠商/客戶）
        if (editModalComponent.Entity.SupplierId <= 0)
        {
            await NotificationService.ShowWarningAsync("進貨單缺少廠商資訊，無法轉沖款");
            return;
        }
        
        // 3. 檢查是否有明細
        if (!purchaseReceivingDetails.Any())
        {
            await NotificationService.ShowWarningAsync("進貨單沒有明細資料，無法轉沖款");
            return;
        }
        
        // 4. 檢查是否有可沖款的明細（有未結清餘額）
        bool hasUnsettledDetails = purchaseReceivingDetails
            .Any(detail => detail.SubtotalAmount > detail.TotalPaidAmount && !detail.IsSettled);
        
        if (!hasUnsettledDetails)
        {
            await NotificationService.ShowWarningAsync("進貨單所有明細皆已完成沖款，無需再轉沖款");
            return;
        }
        
        // 5. 呼叫沖款單組件的公開方法
        await setoffDocumentEditModal!.ShowAddModalWithPrefilledReceiving(
            editModalComponent.Entity.SupplierId,
            PurchaseReceivingId.Value
        );
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleCreateSetoffFromReceiving), GetType(), 
            additionalData: $"轉沖款失敗 - ReceivingId: {PurchaseReceivingId}");
        await NotificationService.ShowErrorAsync("轉沖款時發生錯誤");
    }
}
```

---

## 銷貨單轉沖款實作清單

如需實作銷貨單轉沖款，需要修改以下項目：

### SetoffProductTable.razor
- [ ] 注入 `ISalesOrderService`
- [ ] 新增 `FilterSalesOrderId` 參數
- [ ] 新增 `LoadDetailsFromSalesOrder(int salesOrderId)` 方法
  - 篩選條件改為 `SetoffDetailType.SalesOrderDetail`

### SetoffDocumentEditModalComponent.razor
- [ ] 新增 `PrefilledCustomerId` 參數
- [ ] 新增 `PrefilledSalesOrderId` 參數
- [ ] 新增 `filterSalesOrderId` 內部狀態
- [ ] 新增 `shouldAutoLoadFromSalesOrder` 內部狀態
- [ ] 在 `LoadSetoffDocumentData` 處理客戶預填
- [ ] 新增 `ShowAddModalWithPrefilledSalesOrder(int customerId, int salesOrderId)` 方法

### SalesOrderEditModalComponent.razor
- [ ] 確認已有沖款單 Modal 引用
- [ ] 在 `GetCustomActionButtons` 新增「轉沖款」按鈕
- [ ] 新增 `HandleCreateSetoffFromSalesOrder()` 方法
  - 驗證客戶資訊
  - 檢查未結清明細
  - 呼叫 `ShowAddModalWithPrefilledSalesOrder`

---

## 注意事項

### 1. SetoffDetailType 對應
| 來源單據 | SetoffDetailType | 沖款類型 |
|---------|------------------|---------|
| 銷貨訂單明細 | SalesOrderDetail | AccountsReceivable |
| 銷貨退回明細 | SalesReturnDetail | AccountsReceivable |
| 採購進貨明細 | PurchaseReceivingDetail | AccountsPayable |
| 採購退回明細 | PurchaseReturnDetail | AccountsPayable |

### 2. 預設沖款金額
- 預設使用 `detail.RemainingAmount`（未沖款餘額）進行全額沖款
- 使用者可在沖款單中手動調整金額

### 3. DocumentConversionHelper 使用
此 Helper 統一處理轉單的 Modal 開啟流程，包括：
- 設定預填參數
- 顯示 Modal
- 等待子組件就緒（預設 500ms）
- 自動觸發載入

### 4. 按鈕顏色規範
| 功能 | 顏色 | ButtonVariant |
|-----|------|---------------|
| 轉退出 | 黃色 | Yellow |
| 轉沖款 | 綠色 | Green |
| 轉進貨 | 藍色 | Blue |

### 5. 驗證順序
1. 驗證來源單據是否存在
2. 驗證關聯方（廠商/客戶）是否存在
3. 驗證是否有明細資料
4. 驗證是否有可沖款的明細（未結清）

### 6. 稅率計算一致性

#### 問題描述
來源單據（如進貨單）的「小計」欄位會根據 `TaxCalculationMethod` 計算含稅金額，但沖款單的未結清明細原本使用的是不含稅的 `SubtotalAmount`，導致金額不一致。

#### 解決方案
在 `SetoffProductDetailService` 中新增 `CalculateTaxInclusiveAmount` 方法，統一計算含稅金額：

```csharp
private static decimal CalculateTaxInclusiveAmount(decimal subtotal, decimal? taxRate, TaxCalculationMethod taxMethod)
{
    var rate = taxRate ?? 0;
    return taxMethod switch
    {
        // 稅外加：金額 = 小計 × (1 + 稅率/100)
        TaxCalculationMethod.TaxExclusive => Math.Round(subtotal * (1 + rate / 100), 0),
        // 稅內含：金額 = 小計（稅已包含在單價中）
        TaxCalculationMethod.TaxInclusive => Math.Round(subtotal, 0),
        // 免稅：金額 = 小計
        TaxCalculationMethod.NoTax => Math.Round(subtotal, 0),
        _ => Math.Round(subtotal, 0)
    };
}
```

#### 修改的方法
| 方法名稱 | 影響的單據類型 |
|---------|--------------|
| `GetUnsettledReceivableDetailsAsync` | 銷貨訂單明細、銷貨退回明細 |
| `GetUnsettledPayableDetailsAsync` | 採購進貨明細、採購退回明細 |

#### 實作方式
查詢時需額外選取 `TaxRate`（明細層級）和 `TaxCalculationMethod`（主檔層級），然後在記憶體中計算含稅金額：

```csharp
// 查詢時選取必要欄位
var rawData = await context.PurchaseReceivingDetails
    .Select(d => new
    {
        d.SubtotalAmount,
        d.TaxRate,
        d.PurchaseReceiving.TaxCalculationMethod,
        // ... 其他欄位
    })
    .ToListAsync();

// 轉換時計算含稅金額
var details = rawData.Select(d => new UnsettledDetailDto
{
    TotalAmount = CalculateTaxInclusiveAmount(d.SubtotalAmount, d.TaxRate, d.TaxCalculationMethod),
    // ... 其他欄位
}).ToList();
```

---

## 更新記錄

| 日期 | 更新內容 | 修改者 |
|-----|---------|-------|
| 2026-01-31 | 初版 - 進貨單轉沖款功能 | - |
| 2026-01-31 | 新增稅率計算一致性說明 - 修正 SetoffProductDetailService 使用含稅金額 | - |

