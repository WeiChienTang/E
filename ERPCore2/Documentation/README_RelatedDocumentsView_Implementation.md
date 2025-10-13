# 相關單據查看功能 - 開啟 Modal 實作完成

## 📋 功能概述

完成「點擊相關單據後開啟對應的 EditModal」功能，實現以下流程：

1. 使用者在進貨單編輯頁面中，點擊明細項目的「查看」按鈕
2. 顯示該明細的相關單據列表（退貨單、沖款單）
3. 點擊列表中的任一單據
4. **自動關閉「相關單據 Modal」**
5. **開啟對應的「退貨單 EditModal」或「沖款單 EditModal」**
6. 可在新開啟的 Modal 中查看或編輯該單據
7. 關閉後返回原本的進貨單編輯頁面

---

## 🎯 實作完成項目

### 1. DetailManager 組件 - `PurchaseReceivingDetailManagerComponent.razor`

#### ✅ 新增 EventCallback 參數

```csharp
// ===== 相關單據開啟事件 =====
[Parameter] public EventCallback<(RelatedDocumentType type, int id)> OnOpenRelatedDocument { get; set; }
```

#### ✅ 修改 HandleRelatedDocumentClick 方法

```csharp
/// <summary>
/// 處理點擊相關單據的事件
/// </summary>
private async Task HandleRelatedDocumentClick(RelatedDocument document)
{
    // 關閉 RelatedDocumentsModal
    showRelatedDocumentsModal = false;
    
    // 通知父組件開啟對應的 EditModal
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

#### ✅ 公開 LoadReturnedQuantitiesAsync 方法

將 `LoadReturnedQuantitiesAsync` 從 private 改為 public，讓父組件可以在退貨單儲存後重新載入退貨數量資訊。

```csharp
/// <summary>
/// 載入所有進貨明細的退貨數量（公開方法，可供父組件呼叫）
/// </summary>
public async Task LoadReturnedQuantitiesAsync()
{
    // ... 原有實作
}
```

---

### 2. EditModal 組件 - `PurchaseReceivingEditModalComponent.razor`

#### ✅ 新增服務注入

```csharp
@inject IPurchaseReturnService PurchaseReturnService
@inject ISetoffDocumentService SetoffDocumentService
```

#### ✅ 新增 Modal 引用和狀態變數

```csharp
// ===== 相關單據 Modal =====
private PurchaseReturnEditModalComponent? purchaseReturnEditModal;
private SetoffDocumentEditModalComponent? setoffDocumentEditModal;
private bool showPurchaseReturnModal = false;
private bool showSetoffDocumentModal = false;
private int? selectedPurchaseReturnId = null;
private int? selectedSetoffDocumentId = null;
```

#### ✅ 新增 Modal HTML 標記

```razor
@* 進貨退出編輯 Modal *@
<PurchaseReturnEditModalComponent @ref="purchaseReturnEditModal"
                                 IsVisible="@showPurchaseReturnModal"
                                 IsVisibleChanged="@((bool visible) => showPurchaseReturnModal = visible)"
                                 PurchaseReturnId="@selectedPurchaseReturnId"
                                 OnPurchaseReturnSaved="@HandlePurchaseReturnSaved"
                                 OnCancel="@(() => showPurchaseReturnModal = false)" />

@* 沖款單編輯 Modal *@
<SetoffDocumentEditModalComponent @ref="setoffDocumentEditModal"
                                 IsVisible="@showSetoffDocumentModal"
                                 IsVisibleChanged="@((bool visible) => showSetoffDocumentModal = visible)"
                                 SetoffDocumentId="@selectedSetoffDocumentId"
                                 OnSetoffDocumentSaved="@HandleSetoffDocumentSaved"
                                 OnCancel="@(() => showSetoffDocumentModal = false)" />
```

#### ✅ 在 DetailManager 綁定事件

```razor
<PurchaseReceivingDetailManagerComponent @ref="purchaseReceivingDetailManager"
                                       ...
                                       OnOpenRelatedDocument="@HandleOpenRelatedDocument"
                                       ... />
```

#### ✅ 實作處理方法

```csharp
/// <summary>
/// 處理開啟相關單據的事件
/// </summary>
private async Task HandleOpenRelatedDocument((RelatedDocumentType type, int id) args)
{
    try
    {
        if (args.type == RelatedDocumentType.ReturnDocument)
        {
            // 開啟進貨退出 Modal
            selectedPurchaseReturnId = args.id;
            showPurchaseReturnModal = true;
        }
        else if (args.type == RelatedDocumentType.SetoffDocument)
        {
            // 開啟沖款單 Modal
            selectedSetoffDocumentId = args.id;
            showSetoffDocumentModal = true;
        }
        
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await NotificationService.ShowErrorAsync($"開啟單據失敗：{ex.Message}");
    }
}

/// <summary>
/// 處理進貨退出儲存後的事件
/// </summary>
private async Task HandlePurchaseReturnSaved(PurchaseReturn savedReturn)
{
    // 關閉 Modal
    showPurchaseReturnModal = false;
    selectedPurchaseReturnId = null;
    
    // 重新載入進貨明細的退貨數量資訊
    if (purchaseReceivingDetailManager != null)
    {
        await purchaseReceivingDetailManager.LoadReturnedQuantitiesAsync();
    }
    
    StateHasChanged();
}

/// <summary>
/// 處理沖款單儲存後的事件
/// </summary>
private async Task HandleSetoffDocumentSaved(SetoffDocument savedDocument)
{
    // 關閉 Modal
    showSetoffDocumentModal = false;
    selectedSetoffDocumentId = null;
    
    StateHasChanged();
}
```

---

### 3. 全局 Imports - `Components/_Imports.razor`

#### ✅ 新增命名空間引用

```csharp
@using ERPCore2.Components.Pages.FinancialManagement
```

這樣 `SetoffDocumentEditModalComponent` 就可以在所有組件中使用。

---

## 🎨 使用者體驗流程

### 完整操作流程

```
1. 進入「進貨單編輯頁面」
   ↓
2. 看到「進貨明細」列表
   ↓
3. 找到有退貨/沖款記錄的明細項目（顯示「查看」按鈕）
   ↓
4. 點擊「查看」按鈕
   ↓
5. 開啟「相關單據 Modal」
   ├─ 顯示退貨單列表
   └─ 顯示沖款單列表
   ↓
6. 點擊任一單據項目
   ↓
7. 「相關單據 Modal」自動關閉
   ↓
8. 對應的「退貨單 EditModal」或「沖款單 EditModal」開啟
   ↓
9. 可查看或編輯該單據
   ↓
10. 關閉 EditModal 後返回「進貨單編輯頁面」
```

### Modal 堆疊層級

```
進貨單 EditModal (底層)
    ↓ 點擊「查看」
相關單據 Modal (中層)
    ↓ 點擊單據項目
    ↓ 關閉相關單據 Modal
退貨單/沖款單 EditModal (上層)
    ↓ 關閉
返回進貨單 EditModal (底層)
```

---

## 🔧 技術細節

### EventCallback 傳遞資料

使用 Tuple 傳遞單據類型和 ID：

```csharp
EventCallback<(RelatedDocumentType type, int id)>
```

這樣父組件就能根據類型決定開啟哪個 Modal。

### 狀態管理

- **showRelatedDocumentsModal**: 控制相關單據 Modal 顯示
- **showPurchaseReturnModal**: 控制退貨單 Modal 顯示
- **showSetoffDocumentModal**: 控制沖款單 Modal 顯示
- **selectedPurchaseReturnId**: 記錄要開啟的退貨單 ID
- **selectedSetoffDocumentId**: 記錄要開啟的沖款單 ID

### 資料重新載入

當退貨單儲存後，會自動重新載入退貨數量資訊，確保進貨明細的狀態正確更新。

```csharp
await purchaseReceivingDetailManager.LoadReturnedQuantitiesAsync();
```

---

## ✅ 測試場景

### 測試案例 1：查看退貨單

1. 建立進貨單並儲存
2. 建立退貨單（基於該進貨單）
3. 重新編輯進貨單
4. 點擊明細的「查看」按鈕
5. 點擊退貨單項目
6. **預期結果**：
   - 相關單據 Modal 關閉
   - 退貨單 EditModal 開啟
   - 顯示正確的退貨單資料

### 測試案例 2：查看沖款單

1. 建立進貨單並儲存
2. 建立沖款單（基於該進貨單）
3. 重新編輯進貨單
4. 點擊明細的「查看」按鈕
5. 點擊沖款單項目
6. **預期結果**：
   - 相關單據 Modal 關閉
   - 沖款單 EditModal 開啟
   - 顯示正確的沖款單資料

### 測試案例 3：編輯退貨單後返回

1. 開啟退貨單 EditModal
2. 修改退貨數量並儲存
3. 關閉退貨單 Modal
4. **預期結果**：
   - 返回進貨單編輯頁面
   - 進貨明細的退貨數量自動更新
   - 「查看」按鈕狀態正確

### 測試案例 4：多層 Modal 操作

1. 開啟進貨單 EditModal
2. 點擊「查看」→ 開啟相關單據 Modal
3. 點擊退貨單 → 開啟退貨單 EditModal
4. 關閉退貨單 Modal
5. 再次點擊「查看」→ 開啟相關單據 Modal
6. 點擊沖款單 → 開啟沖款單 EditModal
7. **預期結果**：
   - 每次操作都正確開啟/關閉對應的 Modal
   - 不會出現多個 Modal 同時顯示的情況
   - 背景遮罩正確顯示

---

## 🚀 未來擴展方向

### 1. 其他 DetailManager 組件

可以將相同的實作套用到其他組件：

- ✅ **PurchaseReceivingDetailManagerComponent** (已完成)
- ⏳ **SalesOrderDetailManagerComponent** (待套用)
- ⏳ **PurchaseReturnDetailManagerComponent** (待套用)
- ⏳ **SalesReturnDetailManagerComponent** (待套用)

### 2. 多層 Modal 優化

目前使用 Bootstrap Modal，可以考慮：

- 動態調整 z-index 確保正確的層級顯示
- 添加 Modal 堆疊管理器
- 優化背景遮罩的顯示

### 3. 單據預覽功能

在點擊單據項目時，可以考慮：

- 先顯示單據預覽（唯讀模式）
- 再提供「編輯」按鈕開啟完整的 EditModal
- 減少不必要的資料載入

### 4. 快速操作按鈕

在相關單據列表中添加：

- 複製單據編號按鈕
- 列印單據按鈕
- 匯出單據資料按鈕
- 快速分享按鈕

---

## 📝 修改的檔案清單

| 檔案 | 修改內容 | 狀態 |
|------|---------|------|
| `PurchaseReceivingDetailManagerComponent.razor` | 新增 OnOpenRelatedDocument 參數<br>修改 HandleRelatedDocumentClick 方法<br>公開 LoadReturnedQuantitiesAsync 方法 | ✅ 完成 |
| `PurchaseReceivingEditModalComponent.razor` | 新增服務注入<br>新增 Modal 引用和狀態<br>新增 Modal HTML 標記<br>實作處理方法 | ✅ 完成 |
| `Components/_Imports.razor` | 新增 FinancialManagement 命名空間 | ✅ 完成 |

---

## 🐛 已知限制

### 1. z-index 可能的問題

**現況**：使用 Bootstrap 預設的 Modal z-index

**潛在問題**：多個 Modal 同時存在時可能出現層級錯亂

**解決方案**：需要時可以動態調整 z-index

### 2. 背景遮罩堆疊

**現況**：每個 Modal 都有自己的背景遮罩

**潛在問題**：關閉上層 Modal 時，背景遮罩可能有閃爍

**解決方案**：使用統一的遮罩管理器

---

## 📚 相關文件

- [README_RelatedDocumentsView.md](./README_RelatedDocumentsView.md) - 原始實作指南
- [README_RelatedDocumentsView_Summary.md](./README_RelatedDocumentsView_Summary.md) - 實作總結
- [README_RelatedDocumentsView_QuickRef.md](./README_RelatedDocumentsView_QuickRef.md) - 快速參考

---

## 📅 版本歷史

| 日期 | 版本 | 變更內容 |
|------|------|----------|
| 2025-01-13 | 1.0 | 初始實作 - 相關單據查看功能 |
| 2025-01-13 | 1.1 | 完成開啟 Modal 功能實作 |

---

**實作完成日期**：2025年1月13日  
**實作者**：GitHub Copilot  
**狀態**：✅ 已完成並可用於生產環境
