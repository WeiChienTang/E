# 相關單據查看功能 - 快速參考

## 🎯 核心概念

當明細項目有**退貨記錄**或**沖款記錄**時：
- ❌ 不能刪除
- 👁️ 顯示「查看」按鈕
- 📋 可查看相關的退貨單和沖款單
- 🔗 **可直接開啟對應的 EditModal** (v1.1 新增)

---

## 📦 新增的檔案

```
Models/
  └── RelatedDocument.cs                    # 相關單據資料模型

Helpers/
  └── RelatedDocumentsHelper.cs             # 查詢相關單據的 Helper

Components/Shared/
  └── RelatedDocumentsModalComponent.razor  # 顯示相關單據的 Modal

Documentation/
  ├── README_RelatedDocumentsView.md        # 詳細實作指南
  └── README_RelatedDocumentsView_Summary.md # 實作完成總結
```

---

## 🔧 修改的檔案

```
Data/
  └── ServiceRegistration.cs
      └── 新增: services.AddScoped<RelatedDocumentsHelper>();

Components/Shared/SubCollections/
  ├── PurchaseReceivingDetailManagerComponent.razor
  │   ├── 新增: @inject RelatedDocumentsHelper
  │   ├── 新增: 相關單據查看狀態變數
  │   ├── 新增: RelatedDocumentsModalComponent
  │   ├── 修改: GetCustomActionsTemplate (顯示查看按鈕)
  │   ├── 新增: ShowRelatedDocuments()
  │   └── 新增: HandleRelatedDocumentClick()
  │
  └── SalesOrderDetailManagerComponent.razor
      └── (相同的修改)
```

---

## 💡 使用方式

### 使用者操作

1. 編輯單據（如進貨單）
2. 看到明細列表中的操作按鈕：
   - 🗑️ **刪除按鈕**（紅色）- 可以刪除的項目
   - 👁️ **查看按鈕**（藍色）- 有退貨/沖款記錄的項目
3. 點擊「查看」按鈕
4. 看到相關單據列表：
   - ⚠️ **退貨單**（黃色區塊）
   - ✅ **沖款單**（綠色區塊）
5. **點擊任一單據項目** (v1.1 新增)
6. **自動關閉相關單據 Modal，並開啟對應的 EditModal** (v1.1 新增)

### 程式呼叫

```csharp
// 查詢進貨明細的相關單據
var docs = await RelatedDocumentsHelper
    .GetRelatedDocumentsForPurchaseReceivingDetailAsync(detailId);

// 查詢銷貨訂單明細的相關單據
var docs = await RelatedDocumentsHelper
    .GetRelatedDocumentsForSalesOrderDetailAsync(detailId);

// 查詢採購退貨明細的相關單據
var docs = await RelatedDocumentsHelper
    .GetRelatedDocumentsForPurchaseReturnDetailAsync(detailId);

// 查詢銷貨退回明細的相關單據
var docs = await RelatedDocumentsHelper
    .GetRelatedDocumentsForSalesReturnDetailAsync(detailId);
```

---

## 🎨 UI 元素

### 查看按鈕
```razor
<GenericButtonComponent Variant="ButtonVariant.Info"
                       IconClass="bi bi-eye text-white"
                       Size="ButtonSize.Large"
                       Title="查看相關單據"
                       OnClick="async () => await ShowRelatedDocuments(item)" />
```

### Modal 組件
```razor
<RelatedDocumentsModalComponent IsVisible="@showRelatedDocumentsModal"
                               IsVisibleChanged="@((bool visible) => showRelatedDocumentsModal = visible)"
                               ProductName="@selectedProductName"
                               RelatedDocuments="@relatedDocuments"
                               IsLoading="@isLoadingRelatedDocuments"
                               OnDocumentClick="@HandleRelatedDocumentClick" />
```

---

## 📋 資料結構

### RelatedDocument

```csharp
public class RelatedDocument
{
    public int DocumentId { get; set; }
    public RelatedDocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime DocumentDate { get; set; }
    public decimal? Quantity { get; set; }      // 退貨數量
    public decimal? Amount { get; set; }        // 沖款金額
    public string? Remarks { get; set; }
    
    // 計算屬性
    public string Icon { get; }                 // bi-arrow-return-left 或 bi-cash-coin
    public string BadgeColor { get; }           // warning 或 success
    public string TypeDisplayName { get; }      // "退貨單" 或 "沖款單"
}
```

### RelatedDocumentType

```csharp
public enum RelatedDocumentType
{
    ReturnDocument,    // 退貨單
    SetoffDocument     // 沖款單
}
```

---

## ✅ 已整合的組件

| 組件 | 狀態 | 查詢方法 |
|------|------|---------|
| PurchaseReceivingDetailManagerComponent | ✅ | GetRelatedDocumentsForPurchaseReceivingDetailAsync |
| SalesOrderDetailManagerComponent | ✅ | GetRelatedDocumentsForSalesOrderDetailAsync |
| PurchaseReturnDetailManagerComponent | 📝 待套用 | GetRelatedDocumentsForPurchaseReturnDetailAsync |
| SalesReturnDetailManagerComponent | 📝 待套用 | GetRelatedDocumentsForSalesReturnDetailAsync |

---

## 🚀 套用到其他組件

### 簡化版步驟

1. **添加依賴**
   ```razor
   @using ERPCore2.Models
   @inject RelatedDocumentsHelper RelatedDocumentsHelper
   ```

2. **添加狀態變數**
   ```csharp
   private bool showRelatedDocumentsModal = false;
   private string selectedProductName = string.Empty;
   private List<RelatedDocument>? relatedDocuments = null;
   private bool isLoadingRelatedDocuments = false;
   ```

3. **添加 Modal**
   ```razor
   <RelatedDocumentsModalComponent IsVisible="@showRelatedDocumentsModal" ... />
   ```

4. **修改按鈕邏輯**
   ```csharp
   if (canDelete) {
       <刪除按鈕 />
   } else {
       <查看按鈕 />
   }
   ```

5. **添加方法**
   - `ShowRelatedDocuments(item)`
   - `HandleRelatedDocumentClick(document)`

詳細步驟請參考：`Documentation/README_RelatedDocumentsView.md`

---

## 🐛 疑難排解

### Q: 點擊「查看」按鈕沒有反應？
A: 檢查項目是否已儲存（ExistingDetailEntity.Id > 0）

### Q: Modal 沒有顯示資料？
A: 檢查 RelatedDocumentsHelper 是否已註冊到 DI 容器

### Q: 顯示「此項目尚未儲存」？
A: 這是正常行為，新增的項目需要先儲存才能查看相關單據

### Q: 查看按鈕沒有顯示？
A: 檢查 CanDeleteItem() 方法是否正確判斷退貨/沖款記錄

---

## � 相關文件

- 📖 [README_RelatedDocumentsView.md](./README_RelatedDocumentsView.md) - 完整實作指南
- 📊 [README_RelatedDocumentsView_Summary.md](./README_RelatedDocumentsView_Summary.md) - 實作總結
- � [README_RelatedDocumentsView_Implementation.md](./README_RelatedDocumentsView_Implementation.md) - 開啟 Modal 實作 (v1.1)
- �🔒 [README_刪除限制設計.md](./README_刪除限制設計.md) - 刪除限制整體設計

---

**版本**: 1.1  
**更新日期**: 2025-01-13  
**狀態**: ✅ 已完成（包含開啟 Modal 功能）
