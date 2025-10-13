# 相關單據查看功能套用總結

## 📅 實作日期
2025年10月13日

## 📋 實作範圍

本次套用將相關單據查看功能加入到以下兩個組件：

1. ✅ **PurchaseReturnDetailManagerComponent.razor** (採購退回明細管理組件)
2. ✅ **SalesReturnDetailManagerComponent.razor** (銷售退貨明細管理組件)

---

## 🔧 變更內容

### 1. PurchaseReturnDetailManagerComponent.razor

#### 1.1 添加依賴注入和命名空間
```razor
@using ERPCore2.Models
@inject RelatedDocumentsHelper RelatedDocumentsHelper
```

#### 1.2 添加 Modal 組件
```razor
<!-- 相關單據查看 Modal -->
<RelatedDocumentsModalComponent IsVisible="@showRelatedDocumentsModal"
                               IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
                               ProductName="@selectedProductName"
                               RelatedDocuments="@relatedDocuments"
                               IsLoading="@isLoadingRelatedDocuments"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

#### 1.3 添加狀態變數
```csharp
// ===== 相關單據查看 =====
private bool showRelatedDocumentsModal = false;
private string selectedProductName = string.Empty;
private List<RelatedDocument>? relatedDocuments = null;
private bool isLoadingRelatedDocuments = false;
```

#### 1.4 修改 GetCustomActionsTemplate
- **修改前**：只有能刪除時顯示刪除按鈕，不能刪除時不顯示任何按鈕
- **修改後**：
  - 能刪除時：顯示刪除按鈕
  - 不能刪除時：顯示查看按鈕（眼睛圖示），可查看相關的退貨單和沖款單

#### 1.5 添加查看相關單據方法
```csharp
private async Task ShowRelatedDocuments(ReturnItem item)
{
    // 檢查是否有現有的明細實體 ID
    if (item.ExistingDetailEntity is not PurchaseReturnDetail detail || detail.Id <= 0)
    {
        await NotificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
        return;
    }

    // 設定商品名稱
    selectedProductName = item.SelectedReceivingDetail?.Product?.Name ?? "未知商品";

    // 顯示 Modal 並開始載入
    showRelatedDocumentsModal = true;
    isLoadingRelatedDocuments = true;
    relatedDocuments = null;
    StateHasChanged();

    try
    {
        // 查詢相關單據
        relatedDocuments = await RelatedDocumentsHelper.GetRelatedDocumentsForPurchaseReturnDetailAsync(detail.Id);
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入相關單據失敗：{ex.Message}");
    }
    finally
    {
        isLoadingRelatedDocuments = false;
        StateHasChanged();
    }
}
```

#### 1.6 添加單據點擊處理方法
```csharp
private async Task HandleRelatedDocumentClick(RelatedDocument document)
{
    await NotificationService.ShowInfoAsync(
        $"請在主畫面中開啟 {document.TypeDisplayName}: {document.DocumentNumber}", 
        "提示"
    );
}
```

---

### 2. SalesReturnDetailManagerComponent.razor

#### 2.1 添加依賴注入和命名空間
```razor
@using ERPCore2.Models
@inject RelatedDocumentsHelper RelatedDocumentsHelper
```

#### 2.2 添加 Modal 組件
```razor
<!-- 相關單據查看 Modal -->
<RelatedDocumentsModalComponent IsVisible="@showRelatedDocumentsModal"
                               IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
                               ProductName="@selectedProductName"
                               RelatedDocuments="@relatedDocuments"
                               IsLoading="@isLoadingRelatedDocuments"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

#### 2.3 添加狀態變數
```csharp
// ===== 相關單據查看 =====
private bool showRelatedDocumentsModal = false;
private string selectedProductName = string.Empty;
private List<RelatedDocument>? relatedDocuments = null;
private bool isLoadingRelatedDocuments = false;
```

#### 2.4 修改 GetCustomActionsTemplate
- **修改前**：只有能刪除時顯示刪除按鈕，不能刪除時不顯示任何按鈕
- **修改後**：
  - 能刪除時：顯示刪除按鈕
  - 不能刪除時：顯示查看按鈕（眼睛圖示），可查看相關的退貨單和沖款單

#### 2.5 添加查看相關單據方法
```csharp
private async Task ShowRelatedDocuments(ReturnItem item)
{
    // 檢查是否有現有的明細實體 ID
    if (item.ExistingDetailEntity is not SalesReturnDetail detail || detail.Id <= 0)
    {
        await NotificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
        return;
    }

    // 設定商品名稱
    selectedProductName = item.SelectedProduct?.Name ?? "未知商品";

    // 顯示 Modal 並開始載入
    showRelatedDocumentsModal = true;
    isLoadingRelatedDocuments = true;
    relatedDocuments = null;
    StateHasChanged();

    try
    {
        // 查詢相關單據
        relatedDocuments = await RelatedDocumentsHelper.GetRelatedDocumentsForSalesReturnDetailAsync(detail.Id);
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"載入相關單據失敗：{ex.Message}");
    }
    finally
    {
        isLoadingRelatedDocuments = false;
        StateHasChanged();
    }
}
```

#### 2.6 添加單據點擊處理方法
```csharp
private async Task HandleRelatedDocumentClick(RelatedDocument document)
{
    await NotificationService.ShowInfoAsync(
        $"請在主畫面中開啟 {document.TypeDisplayName}: {document.DocumentNumber}", 
        "提示"
    );
}
```

---

## 🎯 功能說明

### 操作邏輯
1. **可刪除的明細**：顯示紅色刪除按鈕（垃圾桶圖示）
2. **不可刪除的明細**：顯示藍色查看按鈕（眼睛圖示）
   - 點擊查看按鈕會開啟相關單據 Modal
   - Modal 會顯示該明細相關的：
     - 退貨單列表
     - 沖款單列表
   - 可點擊單據項目（目前顯示提示訊息）

### 不可刪除的原因
- **採購退回明細**：已有沖款記錄（TotalReceivedAmount > 0）
- **銷售退貨明細**：已有沖款記錄（TotalPaidAmount > 0）

---

## 📊 實作狀態總覽

| 組件名稱 | 狀態 | Helper 方法 | 商品名稱來源 |
|---------|------|------------|-------------|
| PurchaseReceivingDetailManagerComponent | ✅ 已完成（之前） | GetRelatedDocumentsForPurchaseReceivingDetailAsync | item.SelectedProduct?.Name |
| PurchaseReturnDetailManagerComponent | ✅ 本次完成 | GetRelatedDocumentsForPurchaseReturnDetailAsync | item.SelectedReceivingDetail?.Product?.Name |
| SalesOrderDetailManagerComponent | ✅ 已完成（之前） | GetRelatedDocumentsForSalesOrderDetailAsync | item.SelectedProduct?.Name |
| SalesReturnDetailManagerComponent | ✅ 本次完成 | GetRelatedDocumentsForSalesReturnDetailAsync | item.SelectedProduct?.Name |

---

## ✅ 驗證結果

### 編譯狀態
- ✅ PurchaseReturnDetailManagerComponent.razor：無錯誤
- ✅ SalesReturnDetailManagerComponent.razor：無錯誤

### 功能完整性
- ✅ 依賴注入已添加
- ✅ Modal 組件已添加
- ✅ 狀態變數已定義
- ✅ GetCustomActionsTemplate 已更新
- ✅ ShowRelatedDocuments 方法已實作
- ✅ HandleRelatedDocumentClick 方法已實作

---

## 🧪 測試建議

### 測試案例 1：採購退回明細 - 有沖款記錄
1. 建立採購退貨單並儲存
2. 針對退貨單建立沖款單
3. 重新編輯採購退貨單
4. 點擊該明細的「查看」按鈕
5. **預期**：顯示沖款單列表

### 測試案例 2：銷售退貨明細 - 有沖款記錄
1. 建立銷售退貨單並儲存
2. 針對退貨單建立沖款單
3. 重新編輯銷售退貨單
4. 點擊該明細的「查看」按鈕
5. **預期**：顯示沖款單列表

### 測試案例 3：新項目（未儲存）
1. 建立新的退貨單
2. 新增明細項目（但未儲存）
3. 嘗試點擊「查看」按鈕
4. **預期**：顯示提示訊息「此項目尚未儲存，無法查看相關單據」

### 測試案例 4：可刪除的明細
1. 建立退貨單並儲存
2. 編輯退貨單，查看沒有沖款記錄的明細
3. **預期**：顯示紅色刪除按鈕

### 測試案例 5：不可刪除的明細
1. 建立退貨單並儲存
2. 建立沖款單
3. 重新編輯退貨單，查看有沖款記錄的明細
4. **預期**：顯示藍色查看按鈕（眼睛圖示）

---

## 🔗 相關文件

- [相關單據查看功能 - 實作指南](./README_RelatedDocumentsView.md)
- [相關單據查看功能 - 快速參考](./README_RelatedDocumentsView_QuickRef.md)
- [刪除限制設計](./README_刪除限制設計.md)

---

## 📝 備註

1. 本次實作遵循 `README_RelatedDocumentsView.md` 的指南
2. 參考已完成的 `SalesOrderDetailManagerComponent.razor` 和 `PurchaseReceivingDetailManagerComponent.razor`
3. 所有變更已完成編譯檢查，無錯誤
4. UI 一致性：查看按鈕使用統一的樣式（藍色背景，眼睛圖示）

---

## 🎉 結論

成功在 `PurchaseReturnDetailManagerComponent` 和 `SalesReturnDetailManagerComponent` 兩個組件中套用了相關單據查看功能，讓使用者在遇到不可刪除的明細時，能夠清楚了解是哪些單據阻止了刪除操作，提升了系統的可用性和透明度。
