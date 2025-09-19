# 銷售退貨明細管理組件實作紀錄

## 📋 專案概述

本次開發實作了 **SalesReturnDetailManagerComponent**，一個專門用於管理銷售退貨明細的 Blazor 組件。此組件參考了現有的 `PurchaseReturnDetailManagerComponent` 架構，提供一致的使用者體驗和完整的業務邏輯支援。

**開發日期**: 2025年9月19日  
**版本**: ERPCore2 v1.0  
**主要目標**: 創建功能完整的銷售退貨明細管理系統

---

## 🏗️ 系統架構改動

### 1. 服務層擴展

#### 新增介面方法

**ISalesOrderDetailService.cs**
```csharp
/// <summary>
/// 根據客戶ID取得可退貨的銷貨訂單明細
/// </summary>
/// <param name="customerId">客戶ID</param>
/// <returns>可退貨的銷貨訂單明細清單</returns>
Task<List<SalesOrderDetail>> GetReturnableDetailsByCustomerAsync(int customerId);

/// <summary>
/// 更新銷貨訂單明細的庫存資訊
/// </summary>
/// <param name="details">要更新的明細清單</param>
Task UpdateDetailsWithInventoryAsync(List<SalesOrderDetail> details);
```

**ISalesReturnDetailService.cs**
```csharp
/// <summary>
/// 取得指定銷貨訂單明細的已退貨數量總計
/// </summary>
/// <param name="salesOrderDetailId">銷貨訂單明細ID</param>
/// <returns>已退貨數量總計</returns>
Task<decimal> GetReturnedQuantityByOrderDetailAsync(int salesOrderDetailId);
```

#### 實作類別擴展

**SalesOrderDetailService.cs**
- 實作 `GetReturnableDetailsByCustomerAsync()` 方法
  - 複雜的 LINQ 查詢，篩選可退貨的訂單明細
  - 排除已完全退貨的項目
  - 包含必要的導航屬性載入
- 實作 `UpdateDetailsWithInventoryAsync()` 方法
  - 基礎庫存更新邏輯
  - 錯誤處理和例外管理

**SalesReturnDetailService.cs**
- 實作 `GetReturnedQuantityByOrderDetailAsync()` 方法
  - 計算指定訂單明細的累計退貨數量
  - 完善的錯誤處理機制
  - 資料庫查詢最佳化

---

## 🧩 組件實作詳情

### 2. 核心組件創建

#### SalesReturnDetailManagerComponent.razor

**檔案位置**: `Components/Shared/SubCollections/SalesReturnDetailManagerComponent.razor`

**主要特性**:
- 🎯 **統一 UI 架構**: 使用 `InteractiveTableComponent` 提供一致的表格介面
- 📊 **智慧數據管理**: 自動計算可退貨數量，防止超量退貨
- 🔄 **雙模式支援**: 完整支援新增模式和編輯模式
- 🎨 **響應式設計**: 適配桌面和行動裝置的不同顯示需求

**參數配置**:
```csharp
[Parameter] public int? CustomerId { get; set; }
[Parameter] public int? FilterSalesOrderId { get; set; }
[Parameter] public int? FilterProductId { get; set; }
[Parameter] public List<SalesReturnDetail> ExistingReturnDetails { get; set; }
[Parameter] public EventCallback<List<SalesReturnDetail>> OnReturnDetailsChanged { get; set; }
[Parameter] public bool IsEditMode { get; set; } = false;
```

### 3. 內部資料模型

#### ReturnItem 類別
```csharp
public class ReturnItem
{
    public SalesOrderDetail SalesOrderDetail { get; set; } = null!;
    public decimal ReturnQuantity { get; set; } = 0;
    public decimal ReturnUnitPrice { get; set; } = 0;
    public int? WarehouseLocationId { get; set; }
    public string? ReturnReason { get; set; }
    public string? QualityCondition { get; set; }
    public string? ValidationError { get; set; }
    public SalesReturnDetail? ExistingReturnDetail { get; set; }
}
```

---

## 📊 表格欄位設計

### InteractiveTableComponent 欄位定義

| 欄位名稱 | 寬度 | 類型 | 說明 |
|---------|------|------|------|
| 銷售訂單 | 14% | Custom | 顯示銷售訂單號碼，藍色粗體樣式 |
| 訂單日期 | 10% | Custom | MM/dd 格式，行動裝置隱藏 |
| 商品 | 18% | Custom | 商品代碼與名稱，雙行顯示 |
| 訂單量 | 7% | Custom | 唯讀，右對齊顯示 |
| 已退貨 | 7% | Custom | 唯讀，完成項目綠色顯示 |
| 可退貨 | 7% | Custom | 唯讀，警告色彩提示 |
| 退貨量 | 7% | Custom | 數字輸入，即時驗證 |
| 退貨單價 | 10% | Custom | 小數輸入，預設原訂單單價 |
| 退貨位置 | 12% | Custom | 下拉選單，倉庫位置選擇 |
| 退貨原因 | 8% | Custom | 預設選項下拉選單 |

### 特殊功能欄位

**退貨數量欄位**:
- 自動範圍驗證（0 ~ 可退貨量）
- 即時錯誤提示
- 超量自動調整

**退貨原因選項**:
- 客戶不滿意
- 商品瑕疵
- 規格錯誤
- 過期商品
- 數量錯誤
- 其他

---

## 🔧 業務邏輯實作

### 4. 核心功能方法

#### 資料載入邏輯
```csharp
/// <summary>
/// 載入客戶的可退貨明細
/// </summary>
private async Task LoadReturnableDetailsAsync()
{
    if (IsEditMode)
    {
        // 編輯模式：根據現有退貨明細載入對應的銷售訂單明細
        foreach (var id in orderDetailIds)
        {
            var detail = await SalesOrderDetailService.GetWithIncludesAsync(id);
            if (detail != null)
            {
                ReturnableDetails.Add(detail);
            }
        }
    }
    else
    {
        // 新增模式：載入客戶的可退貨明細
        ReturnableDetails = await SalesOrderDetailService
            .GetReturnableDetailsByCustomerAsync(CustomerId.Value);
    }
}
```

#### 數量驗證機制
```csharp
/// <summary>
/// 退貨數量變更處理
/// </summary>
private async Task OnReturnQuantityChanged(ReturnItem item, string? value)
{
    if (decimal.TryParse(value, out decimal quantity))
    {
        var returnableQuantity = GetReturnableQuantity(item.SalesOrderDetail);
        if (quantity > returnableQuantity)
        {
            item.ReturnQuantity = returnableQuantity;
            item.ValidationError = $"退貨數量不可超過可退貨量 {returnableQuantity}";
        }
        else
        {
            item.ReturnQuantity = quantity;
            item.ValidationError = null;
        }
    }
}
```

#### 資料轉換邏輯
```csharp
/// <summary>
/// 轉換為退貨明細實體
/// </summary>
private List<SalesReturnDetail> ConvertToReturnDetails()
{
    var details = new List<SalesReturnDetail>();
    
    foreach (var item in ReturnItems.Where(x => x.ReturnQuantity > 0))
    {
        SalesReturnDetail detail = item.ExistingReturnDetail ?? new SalesReturnDetail();
        
        // 設定屬性
        detail.SalesOrderDetailId = item.SalesOrderDetail?.Id;
        detail.ProductId = item.SalesOrderDetail?.ProductId ?? 0;
        detail.ReturnQuantity = item.ReturnQuantity;
        detail.OriginalUnitPrice = item.SalesOrderDetail?.UnitPrice ?? 0;
        detail.ReturnUnitPrice = item.ReturnUnitPrice;
        detail.DetailRemarks = item.ReturnReason;
        detail.QualityCondition = item.QualityCondition;
        
        details.Add(detail);
    }
    
    return details;
}
```

---

## 🔗 系統整合

### 5. SalesReturnEditModalComponent 整合

#### 組件參數傳遞
```csharp
private RenderFragment CreateReturnDetailManagerContent() => __builder =>
{
    <SalesReturnDetailManagerComponent 
        CustomerId="@(editModalComponent?.Entity?.CustomerId)"
        FilterSalesOrderId="@FilterSalesOrderId"
        FilterProductId="@FilterProductId"
        ExistingReturnDetails="@salesReturnDetails"
        OnReturnDetailsChanged="@HandleReturnDetailsChanged"
        IsEditMode="@SalesReturnId.HasValue" />
};
```

#### 明細變更處理
```csharp
/// <summary>
/// 處理退貨明細變更
/// </summary>
private async Task HandleReturnDetailsChanged(List<SalesReturnDetail> details)
{
    salesReturnDetails = details;
    
    // 重新計算總金額
    var totalAmount = details.Where(d => d.ProductId > 0)
                            .Sum(d => d.ReturnQuantity * d.ReturnUnitPrice);
    
    // 更新主檔總金額
    if (editModalComponent?.Entity != null)
    {
        editModalComponent.Entity.TotalReturnAmount = totalAmount;
        StateHasChanged();
    }
}
```

---

## 📝 檔案異動清單

### 新增檔案
- `Components/Shared/SubCollections/SalesReturnDetailManagerComponent.razor`
- `Documentation/README_SalesReturnDetailManagerComponent.md` (本文件)

### 修改檔案

#### 服務層
- `Services/Sales/ISalesOrderDetailService.cs`
  - ➕ 新增 `GetReturnableDetailsByCustomerAsync()` 方法定義
  - ➕ 新增 `UpdateDetailsWithInventoryAsync()` 方法定義

- `Services/Sales/SalesOrderDetailService.cs`
  - ➕ 實作 `GetReturnableDetailsByCustomerAsync()` 方法
  - ➕ 實作 `UpdateDetailsWithInventoryAsync()` 方法

- `Services/Sales/ISalesReturnDetailService.cs`
  - ➕ 新增 `GetReturnedQuantityByOrderDetailAsync()` 方法定義

- `Services/Sales/SalesReturnDetailService.cs`
  - ➕ 實作 `GetReturnedQuantityByOrderDetailAsync()` 方法

#### 組件層
- `Components/Pages/Sales/SalesReturnEditModalComponent.razor`
  - 🔄 修改 `CreateReturnDetailManagerContent()` 方法
  - ➕ 新增 `FilterSalesOrderId` 屬性
  - ➕ 新增 `FilterProductId` 屬性包裝
  - 🔄 整合新的 `SalesReturnDetailManagerComponent`

---

## ⚡ 性能優化

### 資料快取機制
- **已退貨數量快取**: 使用 `Dictionary<int, decimal>` 快取已退貨數量，減少重複查詢
- **倉庫位置快取**: 一次性載入所有倉庫位置，避免重複 API 呼叫
- **參數變更檢測**: 智慧檢測參數變更，只在必要時重新載入資料

### 查詢最佳化
- **包含導航屬性**: 使用 `Include()` 預載相關實體，減少 N+1 查詢問題
- **條件篩選**: 在資料庫層級進行篩選，減少記憶體使用
- **索引友善查詢**: 查詢設計考慮資料庫索引效率

---

## 🧪 測試與驗證

### 編譯測試
```bash
PS C:\Users\cses3\source\repos\ERPCore2\ERPCore2> dotnet build
Restore complete (1.4s)
  ERPCore2 succeeded (11.6s) → bin\Debug\net9.0\ERPCore2.dll

Build succeeded in 14.2s
```

### 功能驗證項目
- ✅ **服務層方法**: 所有新增的服務方法編譯通過
- ✅ **組件創建**: SalesReturnDetailManagerComponent 創建成功
- ✅ **系統整合**: 與 SalesReturnEditModalComponent 整合無誤
- ✅ **介面統一**: 與 PurchaseReturnDetailManagerComponent 保持一致性
- ✅ **錯誤處理**: 完整的異常處理和使用者友善訊息

---

## 🎯 使用方式

### 基本用法
```razor
<SalesReturnDetailManagerComponent 
    CustomerId="@customerId"
    ExistingReturnDetails="@existingDetails"
    OnReturnDetailsChanged="@HandleDetailsChanged"
    IsEditMode="false" />
```

### 進階篩選
```razor
<SalesReturnDetailManagerComponent 
    CustomerId="@customerId"
    FilterSalesOrderId="@selectedOrderId"
    FilterProductId="@selectedProductId"
    ExistingReturnDetails="@existingDetails"
    OnReturnDetailsChanged="@HandleDetailsChanged"
    IsEditMode="true" />
```

---

## 🔮 未來擴展計劃

### 短期改進
- 🎨 **UI/UX 優化**: 增加更豐富的視覺回饋和動畫效果
- 📊 **報表功能**: 退貨明細統計和分析報表
- 🔍 **搜尋功能**: 增加商品搜尋和快速篩選

### 長期規劃
- 🔄 **批次作業**: 支援大量退貨明細的批次處理
- 📱 **行動優化**: 針對行動裝置的專用介面優化
- 🤖 **AI 輔助**: 智慧推薦退貨原因和品質評估

---

## 👥 開發團隊

**主要開發者**: GitHub Copilot  
**架構指導**: WeiChienTang  
**測試協助**: ERPCore2 團隊  

---

## 📚 相關文件

- [InteractiveTableComponent 使用指南](README_InteractiveTableComponent.md)
- [PurchaseReturnDetailManagerComponent 實作記錄](README_PurchaseReceiving_修改日誌_重構版.md)
- [Blazor 元件錯誤處理指南](README_Blazor_Error_Handling.md)
- [服務層架構文件](README_Services.md)

---

## 📄 授權資訊

本專案遵循 ERPCore2 專案的授權條款。  
© 2025 ERPCore2 Development Team. All rights reserved.