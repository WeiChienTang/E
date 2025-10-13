# 相關單據查看功能 - 實作指南

## 📋 功能說明

當明細項目有退貨記錄或沖款記錄時，不能直接刪除。此時：
- **原本**：刪除按鈕會被隱藏
- **現在**：顯示「查看」按鈕，可以查看相關的退貨單和沖款單列表
- **功能**：點擊列表中的單據可以開啟對應的 EditModal（不關閉當前 Modal）

---

## 🏗️ 架構說明

### 新增的組件和類別

1. **RelatedDocument.cs** (Models)
   - `RelatedDocument`: 相關單據資料模型
   - `RelatedDocumentType`: 單據類型枚舉（退貨單/沖款單）
   - `RelatedDocumentsRequest`: 查詢請求模型

2. **RelatedDocumentsHelper.cs** (Helpers)
   - 提供查詢相關單據的方法
   - 支援不同類型的明細（進貨、銷貨、退貨等）

3. **RelatedDocumentsModalComponent.razor** (Components/Shared)
   - 通用的相關單據查看 Modal
   - 分組顯示退貨單和沖款單
   - 支援點擊單據觸發事件

---

## 📝 應用步驟

### 步驟 1：添加依賴注入

在 DetailManager 組件頂部添加：

```razor
@using ERPCore2.Models
@inject RelatedDocumentsHelper RelatedDocumentsHelper
```

### 步驟 2：添加狀態變數

在 `@code` 區塊中添加：

```csharp
// ===== 相關單據查看 =====
private bool showRelatedDocumentsModal = false;
private string selectedProductName = string.Empty;
private List<RelatedDocument>? relatedDocuments = null;
private bool isLoadingRelatedDocuments = false;
```

### 步驟 3：添加 Modal 組件

在組件的 HTML 部分（通常在 `@code` 之前）添加：

```razor
<!-- 相關單據查看 Modal -->
<RelatedDocumentsModalComponent IsVisible="@showRelatedDocumentsModal"
                               IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
                               ProductName="@selectedProductName"
                               RelatedDocuments="@relatedDocuments"
                               IsLoading="@isLoadingRelatedDocuments"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

### 步驟 4：修改 GetCustomActionsTemplate

將刪除按鈕邏輯改為：

```csharp
private RenderFragment<YourItemType> GetCustomActionsTemplate => item => __builder =>
{
    var canDelete = CanDeleteItem(item, out _);
    
    if (canDelete)
    {
        // 可以刪除：顯示刪除按鈕
        <GenericButtonComponent Variant="ButtonVariant.Danger"
                               IconClass="bi bi-trash text-white"
                               Size="ButtonSize.Large"
                               IsDisabled="@IsReadOnly"
                               Title="刪除"
                               OnClick="async () => await HandleItemDelete(item)"
                               StopPropagation="true"
                               CssClass="btn-square" />
    }
    else
    {
        // 不能刪除：顯示查看按鈕
        <GenericButtonComponent Variant="ButtonVariant.Info"
                               IconClass="bi bi-eye text-white"
                               Size="ButtonSize.Large"
                               Title="查看相關單據"
                               OnClick="async () => await ShowRelatedDocuments(item)"
                               StopPropagation="true"
                               CssClass="btn-square" />
    }
};
```

### 步驟 5：添加查看相關單據方法

根據不同的 DetailManager 類型，選擇對應的查詢方法：

#### 進貨明細 (PurchaseReceivingDetailManagerComponent)

```csharp
private async Task ShowRelatedDocuments(ReceivingItem item)
{
    if (item.ExistingDetailEntity is not PurchaseReceivingDetail detail || detail.Id <= 0)
    {
        await NotificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
        return;
    }

    selectedProductName = item.SelectedProduct?.Name ?? "未知商品";
    showRelatedDocumentsModal = true;
    isLoadingRelatedDocuments = true;
    relatedDocuments = null;
    StateHasChanged();

    try
    {
        relatedDocuments = await RelatedDocumentsHelper.GetRelatedDocumentsForPurchaseReceivingDetailAsync(detail.Id);
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

#### 銷貨訂單明細 (SalesOrderDetailManagerComponent)

```csharp
private async Task ShowRelatedDocuments(SalesItem item)
{
    if (item.ExistingDetailEntity is not SalesOrderDetail detail || detail.Id <= 0)
    {
        await NotificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
        return;
    }

    selectedProductName = item.SelectedProduct?.Name ?? "未知商品";
    showRelatedDocumentsModal = true;
    isLoadingRelatedDocuments = true;
    relatedDocuments = null;
    StateHasChanged();

    try
    {
        relatedDocuments = await RelatedDocumentsHelper.GetRelatedDocumentsForSalesOrderDetailAsync(detail.Id);
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

#### 採購退貨明細 (PurchaseReturnDetailManagerComponent)

```csharp
private async Task ShowRelatedDocuments(ReturnItem item)
{
    if (item.ExistingDetailEntity is not PurchaseReturnDetail detail || detail.Id <= 0)
    {
        await NotificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
        return;
    }

    selectedProductName = item.SelectedReceivingDetail?.Product?.Name ?? "未知商品";
    showRelatedDocumentsModal = true;
    isLoadingRelatedDocuments = true;
    relatedDocuments = null;
    StateHasChanged();

    try
    {
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

#### 銷貨退回明細 (SalesReturnDetailManagerComponent)

```csharp
private async Task ShowRelatedDocuments(ReturnItem item)
{
    if (item.ExistingDetailEntity is not SalesReturnDetail detail || detail.Id <= 0)
    {
        await NotificationService.ShowWarningAsync("此項目尚未儲存，無法查看相關單據", "提示");
        return;
    }

    selectedProductName = item.SelectedProduct?.Name ?? "未知商品";
    showRelatedDocumentsModal = true;
    isLoadingRelatedDocuments = true;
    relatedDocuments = null;
    StateHasChanged();

    try
    {
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

### 步驟 6：添加單據點擊處理方法

```csharp
private async Task HandleRelatedDocumentClick(RelatedDocument document)
{
    // 目前只顯示提示訊息
    await NotificationService.ShowInfoAsync(
        $"請在主畫面中開啟 {document.TypeDisplayName}: {document.DocumentNumber}", 
        "提示"
    );
    
    // TODO: 未來可以考慮添加 EventCallback 參數，讓父組件處理開啟 Modal 的邏輯
}
```

---

## 🎯 適用組件清單

| 組件名稱 | Item 類型 | Detail 實體類型 | Helper 方法 | 狀態 |
|---------|----------|----------------|------------|------|
| PurchaseReceivingDetailManagerComponent | ReceivingItem | PurchaseReceivingDetail | GetRelatedDocumentsForPurchaseReceivingDetailAsync | ✅ 已完成 |
| PurchaseReturnDetailManagerComponent | ReturnItem | PurchaseReturnDetail | GetRelatedDocumentsForPurchaseReturnDetailAsync | ⏳ 待實作 |
| SalesOrderDetailManagerComponent | SalesItem | SalesOrderDetail | GetRelatedDocumentsForSalesOrderDetailAsync | ⏳ 待實作 |
| SalesReturnDetailManagerComponent | ReturnItem | SalesReturnDetail | GetRelatedDocumentsForSalesReturnDetailAsync | ⏳ 待實作 |
| PurchaseOrderDetailManagerComponent | ProductItem | PurchaseOrderDetail | - | ❌ 不適用* |

\* PurchaseOrderDetailManagerComponent 不需要此功能，因為採購訂單明細不直接關聯退貨和沖款

---

## 🔧 已完成的變更

### 1. Models/RelatedDocument.cs
- ✅ 定義了相關單據資料模型
- ✅ 包含退貨單和沖款單的統一表示

### 2. Helpers/RelatedDocumentsHelper.cs
- ✅ 實作了查詢相關單據的方法
- ✅ 支援進貨明細、銷貨訂單明細、採購退貨明細、銷貨退回明細

### 3. Components/Shared/RelatedDocumentsModalComponent.razor
- ✅ 通用的相關單據查看 Modal
- ✅ 美觀的 UI，分組顯示退貨單和沖款單
- ✅ 支援點擊單據觸發事件

### 4. Data/ServiceRegistration.cs
- ✅ 註冊 RelatedDocumentsHelper 到 DI 容器

### 5. PurchaseReceivingDetailManagerComponent.razor
- ✅ 整合相關單據查看功能
- ✅ 修改操作按鈕邏輯（不能刪除時顯示查看按鈕）
- ✅ 添加查看相關單據的方法

---

## 📊 測試建議

### 測試案例 1：有退貨記錄
1. 建立進貨單並儲存
2. 針對進貨明細建立退貨單
3. 重新編輯進貨單
4. 點擊該明細的「查看」按鈕
5. **預期**：顯示退貨單列表

### 測試案例 2：有沖款記錄
1. 建立進貨單並儲存
2. 針對進貨單建立沖款單
3. 重新編輯進貨單
4. 點擊該明細的「查看」按鈕
5. **預期**：顯示沖款單列表

### 測試案例 3：同時有退貨和沖款
1. 建立進貨單並儲存
2. 建立退貨單
3. 建立沖款單
4. 重新編輯進貨單
5. 點擊該明細的「查看」按鈕
6. **預期**：同時顯示退貨單和沖款單列表

### 測試案例 4：新項目（未儲存）
1. 建立新的進貨單
2. 新增明細項目（但未儲存）
3. 嘗試點擊「查看」按鈕
4. **預期**：顯示提示訊息「此項目尚未儲存，無法查看相關單據」

---

## 🚀 未來改進方向

### 1. 直接開啟單據 Modal
目前點擊單據只會顯示提示訊息，未來可以：
- 在 DetailManager 組件添加 EventCallback 參數
- 父組件（EditModal）處理開啟其他單據 Modal 的邏輯
- 實現多層 Modal 的正確顯示（z-index 處理）

### 2. 更豐富的單據資訊
可以在 RelatedDocumentsModal 中顯示：
- 單據狀態（已核准、待核准等）
- 更詳細的金額資訊
- 相關的備註和附件

### 3. 操作快捷方式
可以添加：
- 複製單據編號按鈕
- 列印單據按鈕
- 匯出單據資料按鈕

---

## 📅 變更歷史

| 日期 | 版本 | 變更內容 | 作者 |
|------|------|----------|------|
| 2025-01-13 | 1.0 | 初始版本 - 實作相關單據查看功能 | GitHub Copilot |

---

## 🔗 相關文件

- [刪除限制設計](./README_刪除限制設計.md)
- [進貨單刪除限制增強](./README_PurchaseReceiving_刪除限制增強.md)
- [退貨明細刪除限制設計](./README_PurchaseReturnDetail_刪除限制設計.md)
