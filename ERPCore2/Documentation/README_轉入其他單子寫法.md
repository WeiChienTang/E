# 轉單功能實作指南

## 📋 目錄
- [概述](#概述)
- [架構設計](#架構設計)
- [實作步驟](#實作步驟)
- [完整範例](#完整範例)
- [最佳實踐](#最佳實踐)
- [常見問題](#常見問題)

---

## 概述

本文件說明如何在 ERPCore2 系統中實作「轉單」功能，例如：
- **採購單** → **進貨單**（轉入庫）
- **進貨單** → **進貨退出單**（轉入庫退出）
- **應收帳款** → **沖款單**（轉沖款）
- **銷貨單** → **銷貨退回單**（轉銷貨退回）

### 設計原則
1. **在來源單據的 EditModal 中添加轉單按鈕**
2. **使用 `CustomActionButtons` 參數將按鈕顯示在 Modal 頂部**
3. **按鈕只在滿足特定條件時顯示**（例如：單據已核准）
4. **提供完整的驗證和錯誤處理**

---

## 架構設計

### 1. GenericEditModalComponent 架構

`GenericEditModalComponent` 提供了 `CustomActionButtons` 參數，用於在 Modal 頂部顯示自訂業務流程按鈕。

#### 按鈕區域布局（從左到右）：

```
┌─────────────────────────────────────────────────────────────────┐
│ [自訂操作按鈕]  [審核狀態]        [通過][駁回]  [取消][儲存][列印] │
│  ↑                ↑                 ↑              ↑              │
│  CustomActionButtons  ApprovalStatus  Approval    Main Actions   │
└─────────────────────────────────────────────────────────────────┘
```

### 2. 關鍵參數

```csharp
[Parameter] public RenderFragment? CustomActionButtons { get; set; }
```

---

## 實作步驟

### 步驟 1: 在來源單據的 EditModalComponent 中添加按鈕

以 **採購單轉進貨單** 為例：

#### 1.1 在 `GenericEditModalComponent` 中使用 `CustomActionButtons`

```razor
<GenericEditModalComponent TEntity="PurchaseOrder" 
                          TService="IPurchaseOrderService"
                          @ref="editModalComponent"
                          IsVisible="@IsVisible"
                          IsVisibleChanged="@IsVisibleChanged"
                          ... 其他參數 ...>
    
    @* 自訂操作按鈕 *@
    <CustomActionButtons>
        @* 只有在編輯模式且採購單已核准時才顯示轉單按鈕 *@
        @if (PurchaseOrderId.HasValue && editModalComponent?.Entity?.IsApproved == true)
        {
            <GenericButtonComponent Text="轉入庫單" 
                                  Variant="ButtonVariant.OutlinePrimary" 
                                  IconClass="fas fa-warehouse"
                                  OnClick="HandleConvertToPurchaseReceiving" 
                                  IsDisabled="@isConvertingToReceiving"
                                  IsLoading="@isConvertingToReceiving"
                                  Title="將此採購單轉為進貨單" />
        }
    </CustomActionButtons>
    
</GenericEditModalComponent>
```

#### 1.2 添加狀態變數

在 `@code` 區塊中添加：

```csharp
@code {
    // ... 現有變數 ...
    
    // 轉單操作狀態
    private bool isConvertingToReceiving = false;
}
```

### 步驟 2: 實作轉單處理方法

#### 2.1 基本架構

```csharp
/// <summary>
/// 處理轉為進貨單
/// </summary>
private async Task HandleConvertToPurchaseReceiving()
{
    if (isConvertingToReceiving) return;
    
    try
    {
        isConvertingToReceiving = true;
        StateHasChanged();
        
        // 1️⃣ 驗證來源單據狀態
        if (!ValidateSourceDocument())
        {
            return;
        }
        
        // 2️⃣ 檢查是否已經轉過單
        if (await CheckIfAlreadyConverted())
        {
            await NotificationService.ShowWarningAsync("此採購單已經轉過進貨單");
            return;
        }
        
        // 3️⃣ 創建目標單據
        var targetDocument = await CreateTargetDocument();
        
        if (targetDocument == null)
        {
            await NotificationService.ShowErrorAsync("創建進貨單失敗");
            return;
        }
        
        // 4️⃣ 更新來源單據狀態（標記已轉單）
        await UpdateSourceDocumentStatus();
        
        // 5️⃣ 顯示成功訊息並導航
        await NotificationService.ShowSuccessAsync($"成功創建進貨單：{targetDocument.ReceiptNumber}");
        
        // 關閉當前 Modal
        await CloseModal();
        
        // 導航到目標單據編輯頁面
        NavigationManager.NavigateTo($"/purchase/receiving/edit/{targetDocument.Id}");
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(HandleConvertToPurchaseReceiving), GetType(), 
            additionalData: $"轉入庫單失敗 - 採購單ID: {PurchaseOrderId}");
        await NotificationService.ShowErrorAsync("轉入庫單時發生錯誤");
    }
    finally
    {
        isConvertingToReceiving = false;
        StateHasChanged();
    }
}
```

#### 2.2 驗證來源單據

```csharp
/// <summary>
/// 驗證來源單據是否符合轉單條件
/// </summary>
private bool ValidateSourceDocument()
{
    // 檢查單據ID
    if (!PurchaseOrderId.HasValue)
    {
        await NotificationService.ShowErrorAsync("無法轉單：採購單ID不存在");
        return false;
    }
    
    // 檢查單據資料
    if (editModalComponent?.Entity == null)
    {
        await NotificationService.ShowErrorAsync("無法轉單：採購單資料不存在");
        return false;
    }
    
    // 檢查核准狀態
    if (!editModalComponent.Entity.IsApproved)
    {
        await NotificationService.ShowErrorAsync("無法轉單：採購單尚未核准");
        return false;
    }
    
    // 檢查明細資料
    if (purchaseOrderDetails == null || !purchaseOrderDetails.Any())
    {
        await NotificationService.ShowErrorAsync("無法轉單：此採購單沒有明細資料");
        return false;
    }
    
    return true;
}
```

#### 2.3 檢查是否已轉單

```csharp
/// <summary>
/// 檢查此採購單是否已經轉過進貨單
/// </summary>
private async Task<bool> CheckIfAlreadyConverted()
{
    try
    {
        // 方式 1: 如果來源單據有 IsConverted 欄位
        if (editModalComponent?.Entity?.IsConverted == true)
        {
            return true;
        }
        
        // 方式 2: 查詢目標單據資料表
        var existingReceivings = await PurchaseReceivingService
            .GetByPurchaseOrderIdAsync(PurchaseOrderId!.Value);
        
        return existingReceivings != null && existingReceivings.Any();
    }
    catch (Exception ex)
    {
        // 記錄錯誤但不阻止轉單流程
        Console.WriteLine($"檢查轉單狀態失敗: {ex.Message}");
        return false;
    }
}
```

#### 2.4 創建目標單據

```csharp
/// <summary>
/// 創建進貨單並複製採購單資料
/// </summary>
private async Task<PurchaseReceiving?> CreateTargetDocument()
{
    try
    {
        var sourceOrder = editModalComponent!.Entity!;
        
        // 創建新的進貨單
        var receiving = new PurchaseReceiving
        {
            // 基本資訊
            ReceiptNumber = await GenerateReceiptNumberAsync(),
            PurchaseOrderId = sourceOrder.Id,
            SupplierId = sourceOrder.SupplierId,
            CompanyId = sourceOrder.CompanyId,
            
            // 日期資訊
            ReceiptDate = DateTime.Now,
            
            // 金額資訊（從採購單複製）
            Subtotal = sourceOrder.Subtotal,
            TaxAmount = sourceOrder.TaxAmount,
            TotalAmount = sourceOrder.TotalAmount,
            
            // 備註
            Remarks = $"由採購單 {sourceOrder.OrderNumber} 轉入",
            
            // 狀態
            Status = EntityStatus.Active,
            IsApproved = false // 新建的進貨單預設未核准
        };
        
        // 儲存進貨單主檔
        var savedReceiving = await PurchaseReceivingService.CreateAsync(receiving);
        
        if (savedReceiving == null || savedReceiving.Id <= 0)
        {
            return null;
        }
        
        // 複製明細資料
        await CopyDetailsToTargetDocument(savedReceiving.Id);
        
        return savedReceiving;
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(CreateTargetDocument), GetType());
        return null;
    }
}
```

#### 2.5 複製明細資料

```csharp
/// <summary>
/// 將採購單明細複製到進貨單明細
/// </summary>
private async Task CopyDetailsToTargetDocument(int targetDocumentId)
{
    try
    {
        var receivingDetails = new List<PurchaseReceivingDetail>();
        
        foreach (var orderDetail in purchaseOrderDetails)
        {
            var receivingDetail = new PurchaseReceivingDetail
            {
                PurchaseReceivingId = targetDocumentId,
                PurchaseOrderDetailId = orderDetail.Id,
                ProductId = orderDetail.ProductId,
                WarehouseId = null, // 需要使用者選擇
                WarehouseLocationId = null, // 需要使用者選擇
                
                // 數量資訊
                OrderedQuantity = orderDetail.Quantity,
                ReceivedQuantity = 0, // 預設為 0，需要使用者填寫
                
                // 單價資訊
                UnitPrice = orderDetail.UnitPrice,
                
                // 計算金額
                Subtotal = 0, // 根據實際進貨數量計算
                TaxAmount = 0,
                TotalAmount = 0,
                
                Remarks = orderDetail.Remarks
            };
            
            receivingDetails.Add(receivingDetail);
        }
        
        // 批次儲存明細
        await PurchaseReceivingDetailService.CreateRangeAsync(receivingDetails);
    }
    catch (Exception ex)
    {
        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(CopyDetailsToTargetDocument), GetType());
        throw; // 重新拋出例外，讓外層處理
    }
}
```

#### 2.6 更新來源單據狀態

```csharp
/// <summary>
/// 更新採購單狀態為已轉單
/// </summary>
private async Task UpdateSourceDocumentStatus()
{
    try
    {
        var sourceOrder = editModalComponent!.Entity!;
        
        // 如果有 IsConverted 欄位
        sourceOrder.IsConverted = true;
        sourceOrder.ConvertedDate = DateTime.Now;
        
        await PurchaseOrderService.UpdateAsync(sourceOrder);
    }
    catch (Exception ex)
    {
        // 記錄錯誤但不阻止流程
        Console.WriteLine($"更新來源單據狀態失敗: {ex.Message}");
    }
}
```

---

## 完整範例

### 範例 1: 採購單轉進貨單

詳細實作請參考：`Components/Pages/Purchase/PurchaseOrderEditModalComponent.razor`

**按鈕條件**：
- ✅ 編輯模式（非新增模式）
- ✅ 採購單已核准
- ✅ 有明細資料

**轉單流程**：
1. 驗證採購單狀態
2. 檢查是否已轉過單
3. 創建進貨單主檔
4. 複製採購明細到進貨明細
5. 更新採購單狀態
6. 導航到進貨單編輯頁面

### 範例 2: 進貨單轉進貨退出單

```razor
<CustomActionButtons>
    @if (PurchaseReceivingId.HasValue && editModalComponent?.Entity?.IsApproved == true)
    {
        <GenericButtonComponent Text="轉退貨單" 
                              Variant="ButtonVariant.OutlineWarning" 
                              IconClass="fas fa-undo"
                              OnClick="HandleConvertToReturn" 
                              IsDisabled="@isConvertingToReturn"
                              IsLoading="@isConvertingToReturn"
                              Title="將此進貨單轉為進貨退出單" />
    }
</CustomActionButtons>
```

### 範例 3: 多個轉單按鈕

```razor
<CustomActionButtons>
    @if (PurchaseReceivingId.HasValue && editModalComponent?.Entity?.IsApproved == true)
    {
        @* 轉沖款單 *@
        <GenericButtonComponent Text="轉沖款" 
                              Variant="ButtonVariant.OutlineSuccess" 
                              IconClass="fas fa-money-bill-wave"
                              OnClick="HandleConvertToPayment" 
                              IsDisabled="@isConvertingToPayment"
                              IsLoading="@isConvertingToPayment"
                              Title="將此進貨單轉為沖款單" />
        
        @* 轉退貨單 *@
        <GenericButtonComponent Text="轉退貨單" 
                              Variant="ButtonVariant.OutlineWarning" 
                              IconClass="fas fa-undo"
                              OnClick="HandleConvertToReturn" 
                              IsDisabled="@isConvertingToReturn"
                              IsLoading="@isConvertingToReturn"
                              Title="將此進貨單轉為進貨退出單" />
    }
</CustomActionButtons>
```

---

## 最佳實踐

### 1. 按鈕顏色規範

建議使用以下顏色方案：

| 轉單類型 | 建議顏色 | 理由 |
|---------|---------|------|
| 轉入庫/進貨 | `OutlinePrimary` (藍色) | 一般業務流程 |
| 轉沖款 | `OutlineSuccess` (綠色) | 財務相關，正向操作 |
| 轉退貨/退款 | `OutlineWarning` (黃色) | 需要注意的逆向操作 |
| 轉作廢 | `OutlineDanger` (紅色) | 危險操作 |

### 2. 圖示選擇

```csharp
// 倉庫相關
IconClass="fas fa-warehouse"      // 轉入庫
IconClass="fas fa-dolly"          // 轉出貨

// 財務相關
IconClass="fas fa-money-bill-wave" // 轉沖款
IconClass="fas fa-receipt"         // 轉發票

// 逆向流程
IconClass="fas fa-undo"            // 轉退貨
IconClass="fas fa-exchange-alt"    // 轉換單據
```

### 3. 驗證檢查清單

每個轉單功能都應該包含以下驗證：

- [ ] 來源單據ID存在
- [ ] 來源單據資料完整
- [ ] 來源單據已核准（如果需要）
- [ ] 來源單據有明細資料
- [ ] 檢查是否已經轉過單
- [ ] 目標單據的必要欄位都有值
- [ ] 權限檢查（如果需要）

### 4. 錯誤處理

```csharp
try
{
    // 轉單邏輯
}
catch (Exception ex)
{
    // 1. 記錄詳細錯誤
    await ErrorHandlingHelper.HandlePageErrorAsync(
        ex, 
        nameof(HandleConvertToXXX), 
        GetType(), 
        additionalData: $"轉單失敗 - 來源單據ID: {SourceId}"
    );
    
    // 2. 顯示友善的錯誤訊息給使用者
    await NotificationService.ShowErrorAsync("轉單時發生錯誤，請稍後再試");
    
    // 3. 如果需要，回滾部分操作
    await RollbackPartialChanges();
}
finally
{
    // 4. 重置狀態
    isConverting = false;
    StateHasChanged();
}
```

### 5. 使用者體驗優化

#### 5.1 載入狀態

```razor
<GenericButtonComponent Text="轉入庫單" 
                      IsLoading="@isConvertingToReceiving"
                      IsDisabled="@isConvertingToReceiving" />
```

#### 5.2 確認對話框（可選）

```csharp
private async Task HandleConvertToPurchaseReceiving()
{
    // 顯示確認對話框
    var confirmed = await JSRuntime.InvokeAsync<bool>(
        "confirm", 
        "確定要將此採購單轉為進貨單嗎？"
    );
    
    if (!confirmed) return;
    
    // 執行轉單邏輯...
}
```

#### 5.3 進度提示

```csharp
await NotificationService.ShowInfoAsync("正在創建進貨單...");
// 執行轉單操作
await NotificationService.ShowSuccessAsync("進貨單創建成功");
```

### 6. 資料一致性

#### 6.1 使用交易（Transaction）

```csharp
using var transaction = await DbContext.Database.BeginTransactionAsync();
try
{
    // 1. 創建目標單據
    await CreateTargetDocument();
    
    // 2. 更新來源單據
    await UpdateSourceDocument();
    
    // 3. 提交交易
    await transaction.CommitAsync();
}
catch
{
    // 回滾交易
    await transaction.RollbackAsync();
    throw;
}
```

#### 6.2 防止重複轉單

```csharp
// 方法 1: 在來源單據添加 IsConverted 欄位
if (sourceDocument.IsConverted)
{
    await NotificationService.ShowWarningAsync("此單據已經轉過單");
    return;
}

// 方法 2: 在目標單據添加外鍵索引
// 資料庫層級防止重複轉單
```

---

## 常見問題

### Q1: 按鈕應該放在哪裡？

**A:** 使用 `CustomActionButtons` 參數，按鈕會自動顯示在 Modal 頂部的最左側，與其他預設按鈕（儲存、取消、審核等）並列。

### Q2: 如何控制按鈕的顯示條件？

**A:** 在 `CustomActionButtons` 內使用 `@if` 條件判斷：

```razor
<CustomActionButtons>
    @if (條件1 && 條件2 && 條件3)
    {
        <GenericButtonComponent ... />
    }
</CustomActionButtons>
```

### Q3: 轉單後應該導航到哪裡？

**A:** 有幾種選擇：

1. **導航到目標單據的編輯頁面**（推薦）
   ```csharp
   NavigationManager.NavigateTo($"/purchase/receiving/edit/{targetId}");
   ```

2. **導航到目標單據的列表頁面**
   ```csharp
   NavigationManager.NavigateTo("/purchase/receiving");
   ```

3. **停留在當前頁面並刷新**
   ```csharp
   await CloseModal();
   await OnPurchaseOrderSaved.InvokeAsync(sourceOrder);
   ```

### Q4: 如何處理部分轉單的情況？

**A:** 如果需要支援部分轉單（例如採購100件，只進貨50件）：

1. 在目標單據明細中保留 `SourceDetailId` 欄位
2. 在來源單據明細中添加 `ConvertedQuantity` 欄位
3. 計算剩餘未轉數量：`RemainingQuantity = OrderedQuantity - ConvertedQuantity`
4. 允許多次轉單，直到 `ConvertedQuantity >= OrderedQuantity`

### Q5: 如何處理轉單時的權限檢查？

**A:** 有兩種方式：

1. **在按鈕層級控制**
   ```razor
   <PermissionCheck Permission="PurchaseReceiving.Create">
       <GenericButtonComponent Text="轉入庫單" ... />
   </PermissionCheck>
   ```

2. **在方法內檢查**
   ```csharp
   private async Task HandleConvertToPurchaseReceiving()
   {
       if (!await HasPermission("PurchaseReceiving.Create"))
       {
           await NotificationService.ShowErrorAsync("您沒有權限創建進貨單");
           return;
       }
       // ...
   }
   ```

### Q6: 轉單失敗後如何回滾？

**A:** 使用資料庫交易確保原子性操作（參考「資料一致性」章節）。

### Q7: 如何追蹤轉單歷史？

**A:** 建議方案：

1. **在來源單據添加欄位**
   ```csharp
   public bool IsConverted { get; set; }
   public DateTime? ConvertedDate { get; set; }
   public int? TargetDocumentId { get; set; }
   ```

2. **創建轉單記錄表**
   ```csharp
   public class DocumentConversion
   {
       public int Id { get; set; }
       public string SourceType { get; set; }  // "PurchaseOrder"
       public int SourceId { get; set; }
       public string TargetType { get; set; }  // "PurchaseReceiving"
       public int TargetId { get; set; }
       public DateTime ConvertedAt { get; set; }
       public int ConvertedByUserId { get; set; }
   }
   ```

---

## 延伸閱讀

- `README_GenericEditModalComponent.md` - 了解 Modal 組件的完整功能
- `README_GenericButtonComponent.md` - 了解按鈕組件的使用方式
- `README_Services.md` - 了解服務層的設計模式
- `README_Data.md` - 了解資料模型的設計規範

---

## 更新紀錄

| 日期 | 版本 | 更新內容 |
|------|------|---------|
| 2025/01/09 | 1.0.0 | 初版建立 |

---

**維護者**: ERPCore2 開發團隊  
**最後更新**: 2025年1月9日
