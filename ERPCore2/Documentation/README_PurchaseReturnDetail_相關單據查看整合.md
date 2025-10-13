# 採購退貨明細 - 相關單據查看功能整合

## 📋 變更說明

本次變更將相關單據查看功能（查看沖款單）整合到 `PurchaseReturnDetailManagerComponent.razor`，使其能夠正確開啟 EditModal，而不只是顯示提示訊息。

---

## 🔧 變更內容

### 1. PurchaseReturnDetailManagerComponent.razor

#### 新增參數
```csharp
// ===== 相關單據開啟事件 =====
[Parameter] public EventCallback<(RelatedDocumentType type, int id)> OnOpenRelatedDocument { get; set; }
```

#### 修改方法
```csharp
/// <summary>
/// 處理點擊相關單據的事件
/// </summary>
private async Task HandleRelatedDocumentClick(RelatedDocument document)
{
    // 觸發事件，讓父組件（EditModal）處理開啟相關單據
    if (OnOpenRelatedDocument.HasDelegate)
    {
        await OnOpenRelatedDocument.InvokeAsync((document.DocumentType, document.DocumentId));
    }
    else
    {
        // 如果父組件沒有處理，顯示提示訊息
        await NotificationService.ShowInfoAsync(
            $"請在主畫面中開啟 {document.TypeDisplayName}: {document.DocumentNumber}", 
            "提示"
        );
    }
}
```

### 2. PurchaseReturnEditModalComponent.razor

#### 新增 Service 注入
```csharp
@inject ISetoffDocumentService SetoffDocumentService
```

#### 新增狀態變數
```csharp
// ===== 相關單據 Modal =====
private SetoffDocumentEditModalComponent? setoffDocumentEditModal;
private bool showSetoffDocumentModal = false;
private int? selectedSetoffDocumentId = null;
```

#### 新增 Modal 組件
```razor
@* 沖款單編輯 Modal *@
<SetoffDocumentEditModalComponent @ref="setoffDocumentEditModal"
                                 IsVisible="@showSetoffDocumentModal"
                                 IsVisibleChanged="@((bool visible) => showSetoffDocumentModal = visible)"
                                 SetoffDocumentId="@selectedSetoffDocumentId"
                                 OnSetoffDocumentSaved="@HandleSetoffDocumentSaved"
                                 OnCancel="@(() => showSetoffDocumentModal = false)" />
```

#### 更新 PurchaseReturnDetailManagerComponent 參數傳遞
```razor
<PurchaseReturnDetailManagerComponent @ref="purchaseReturnDetailManager"
    SupplierId="@editModalComponent.Entity.SupplierId"
    FilterPurchaseReceivingId="@filterPurchaseReceivingId"
    FilterProductId="@filterProductId"
    IsEditMode="@(PurchaseReturnId.HasValue)"
    ExistingReturnDetails="@(purchaseReturnDetails ?? new List<PurchaseReturnDetail>())"
    OnReturnDetailsChanged="@HandleReturnDetailsChanged"
    OnDeletedDetailsChanged="@HandleDeletedDetailsChanged"
    OnHasUndeletableDetailsChanged="@HandleHasUndeletableDetailsChanged"
    OnOpenRelatedDocument="@HandleOpenRelatedDocument" />
```

#### 新增處理方法
```csharp
/// <summary>
/// 處理開啟相關單據的事件
/// </summary>
private async Task HandleOpenRelatedDocument((RelatedDocumentType type, int id) args)
{
    try
    {
        if (args.type == RelatedDocumentType.SetoffDocument)
        {
            // 開啟沖款單
            selectedSetoffDocumentId = args.id;
            showSetoffDocumentModal = true;
            StateHasChanged();
        }
        else
        {
            await NotificationService.ShowWarningAsync("不支援的單據類型", "提示");
        }
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"開啟單據失敗：{ex.Message}");
    }
}

/// <summary>
/// 處理沖款單儲存後的事件
/// </summary>
private async Task HandleSetoffDocumentSaved(SetoffDocument savedDocument)
{
    try
    {
        showSetoffDocumentModal = false;
        await NotificationService.ShowSuccessAsync("沖款單已更新");
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"處理沖款單儲存事件失敗：{ex.Message}");
    }
}
```

---

## 🎯 功能說明

### 檢查邏輯
採購退貨明細的刪除限制檢查：
- **沖款記錄檢查**：
  - 資料表：`PurchaseReturnDetail` (採購退貨明細)
  - 欄位：`TotalReceivedAmount` (累計收款金額)
  - 條件：`TotalReceivedAmount > 0` 表示已有沖款記錄，不可刪除

### 相關單據類型
採購退貨明細目前支援查看：
- ✅ **沖款單** (SetoffDocument)

未來可擴展：
- ⏳ 其他相關單據類型（如需要）

---

## 🔄 工作流程

1. **使用者點擊「查看」按鈕**
   - 在不可刪除的退貨明細項目上點擊「查看」按鈕
   
2. **載入相關單據**
   - `PurchaseReturnDetailManagerComponent` 調用 `RelatedDocumentsHelper.GetRelatedDocumentsForPurchaseReturnDetailAsync()`
   - 顯示 `RelatedDocumentsModalComponent`，列出所有相關沖款單

3. **點擊單據項目**
   - 使用者在列表中點擊沖款單
   - 觸發 `HandleRelatedDocumentClick` 事件
   - 通過 `OnOpenRelatedDocument` EventCallback 通知父組件

4. **開啟 EditModal**
   - `PurchaseReturnEditModalComponent` 接收事件
   - 根據單據類型開啟對應的 Modal（沖款單）
   - 設定單據 ID 並顯示 Modal

5. **Modal 層疊顯示**
   - 原本的進貨退出 EditModal 保持開啟
   - 沖款單 EditModal 疊加顯示在上層
   - 使用者可以查看或編輯沖款單

---

## 📊 與 PurchaseReceivingDetailManagerComponent 的對比

| 功能 | PurchaseReceivingDetailManagerComponent | PurchaseReturnDetailManagerComponent |
|------|----------------------------------------|-------------------------------------|
| 檢查退貨記錄 | ✅ 是 | ❌ 否（退貨明細本身就是退貨） |
| 檢查沖款記錄 | ✅ 是 (TotalPaidAmount) | ✅ 是 (TotalReceivedAmount) |
| 支援的相關單據 | 退貨單、沖款單 | 沖款單 |
| 可開啟的 Modal | PurchaseReturnEditModal、SetoffDocumentEditModal | SetoffDocumentEditModal |

---

## ✅ 測試建議

### 測試案例 1：查看沖款單
1. 建立採購退貨單並儲存
2. 針對退貨單建立沖款單
3. 重新編輯採購退貨單
4. 點擊該明細的「查看」按鈕
5. **預期**：顯示沖款單列表
6. 點擊沖款單項目
7. **預期**：正確開啟沖款單 EditModal

### 測試案例 2：無相關單據
1. 建立採購退貨單但未建立沖款單
2. 編輯採購退貨單
3. **預期**：明細項目顯示「刪除」按鈕，而非「查看」按鈕

### 測試案例 3：Modal 層疊
1. 按照測試案例 1 開啟沖款單 Modal
2. **預期**：進貨退出 EditModal 仍在背景顯示
3. 關閉沖款單 Modal
4. **預期**：回到進貨退出 EditModal，資料保持不變

---

## 🔗 參考文件

- [相關單據查看功能 - 實作指南](./README_RelatedDocumentsView.md)
- [刪除限制設計](./README_刪除限制設計.md)
- [退貨明細刪除限制設計](./README_PurchaseReturnDetail_刪除限制設計.md)

---

## 📅 變更歷史

| 日期 | 版本 | 變更內容 | 作者 |
|------|------|----------|------|
| 2025-01-13 | 1.0 | 整合相關單據查看功能到採購退貨明細管理器 | GitHub Copilot |

---

## 🚀 未來改進方向

### 1. 支援更多單據類型
如果未來採購退貨明細需要關聯其他類型的單據，可以：
- 在 `HandleOpenRelatedDocument` 方法中添加新的 case
- 添加對應的 EditModal 組件和狀態變數

### 2. Modal 關閉時重新載入資料
當沖款單 Modal 關閉後，可以考慮：
- 重新載入退貨明細的沖款金額
- 更新不可刪除狀態
- 刷新相關單據列表

### 3. 提供快速操作
可以在相關單據列表中添加：
- 列印單據按鈕
- 複製單據編號
- 顯示單據預覽
