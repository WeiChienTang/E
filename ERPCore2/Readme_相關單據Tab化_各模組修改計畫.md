# 相關單據 Tab 化 — 實作說明

## 背景與目標

各模組的 EditModal 新增「相關單據 Tab」，讓使用者在單一 Modal 內即可一覽所有關聯單據，並直接點擊開啟對應 EditModal。

### 設計目標

- **主檔層級的全覽** — 整張單據有哪些進貨、退回、沖款一目了然
- **設計一致性** — 與 Employee EditModal 的 Tab 架構統一
- **點擊即開啟** — Tab 內每筆相關單據可直接點擊開啟對應 EditModal

---

## 設計原則

### Tab 元件規格

所有相關單據 Tab 均為**唯讀**，遵循以下規格：

```
XxxRelatedTab.razor
  ├── public async Task LoadAsync(int id)  // 父元件在 HandleEntityLoaded 呼叫
  ├── public void Clear()                  // 父元件在 id == 0 時呼叫
  └── 無 SavePendingChangesAsync()         // 唯讀，不需儲存
```

### 與既有 Table 眼睛按鈕的關係

兩者**並存**，各司其職，**不移除眼睛按鈕**：

| 層級 | 工具 | 用途 |
|------|------|------|
| 明細層級 | Table 眼睛按鈕 + `RelatedDocumentsHelper` | 特定明細被哪些單據引用（細粒度） |
| 主檔層級 | EditModal Tab + Service 方法 | 整張單據的相關單據全覽（新增） |

### EditModal 整合方式

```csharp
// 1. 宣告 Tab ref 和狀態
private List<FormTabDefinition>? tabDefinitions;
private XxxTab? xxxTab;

// 2. BuildAll() 同時建立 FieldSections + TabDefinitions
var layout = FormSectionHelper<T>.Create()
    .AddToSection(...)
    .GroupIntoTab("資料", "bi-icon", ...)
    .GroupIntoCustomTab("明細", "bi-list-ul", CreateDetailContent())
    .GroupIntoCustomTab("Tab名稱", "bi-icon", CreateTabContent())
    .BuildAll();
formSections = layout.FieldSections;
tabDefinitions = layout.TabDefinitions;

// 3. HandleEntityLoaded 呼叫 Tab 的 LoadAsync / Clear
private async Task HandleEntityLoaded(int loadedEntityId)
{
    if (loadedEntityId > 0)
    {
        if (xxxTab != null) await xxxTab.LoadAsync(loadedEntityId);
    }
    else
    {
        xxxTab?.Clear();
    }
    StateHasChanged();
}

// 4. RenderFragment 工廠方法
private RenderFragment CreateTabContent() => __builder =>
{
    <XxxTab @ref="xxxTab" OnOpenDoc="@HandleOpenDocFromTab" />
};
```

---

## 單據關聯圖

```
採購訂單 (PO)
  └─ 採購進貨 (PR)          ← 透過 PurchaseReceivingDetail.PurchaseOrderDetailId
       └─ 採購退回 (PRT)    ← 透過 PurchaseReturnDetail.PurchaseReceivingDetailId
            └─ 沖款單        ← 透過 Setoff

報價單 (QT)
  └─ 銷貨訂單 (SO)          ← 透過 SalesOrderDetail.QuotationDetailId
       └─ 銷貨出貨 (SD)     ← 透過 SalesDeliveryDetail.SalesOrderDetailId
            └─ 銷貨退回 (SR) ← 透過 SalesReturnDetail.SalesDeliveryDetailId
                 └─ 沖款單   ← 透過 Setoff
```

---

## 各模組 Tab 結構

### 採購訂單 (PurchaseOrder) ✅

| Tab | 圖示 | 元件 | 資料來源 |
|-----|------|------|---------|
| 訂單資料 | `bi-file-earmark-text` | — | 表單欄位 |
| 訂單明細 | `bi-list-ul` | — | 內嵌 Detail Manager |
| 進貨記錄 | `bi-box-arrow-in-down` | `PurchaseOrderReceivingTab.razor` | `IPurchaseReceivingService.GetByPurchaseOrderAsync` |
| 退回記錄 | `bi-arrow-return-left` | `PurchaseOrderReturnTab.razor` | `IPurchaseReturnService.GetByPurchaseOrderIdAsync` |

---

### 採購進貨 (PurchaseReceiving) ✅

| Tab | 圖示 | 元件 | 資料來源 |
|-----|------|------|---------|
| 進貨資料 | `bi-file-earmark-text` | — | 表單欄位 |
| 進貨明細 | `bi-list-ul` | — | 內嵌 Detail Manager |
| 退回記錄 | `bi-arrow-return-left` | `PurchaseReceivingReturnTab.razor` | `IPurchaseReturnService.GetByPurchaseReceivingIdAsync` |
| 沖款記錄 | `bi-cash-coin` | `PurchaseReceivingSetoffTab.razor` | `ISetoffDocumentService.GetByPurchaseReceivingIdAsync` |
| 來源採購訂單 | `bi-file-earmark-arrow-up` | `PurchaseReceivingOrderTab.razor` | `IPurchaseOrderService.GetByPurchaseReceivingIdAsync` |

---

### 採購退回 (PurchaseReturn) ✅

| Tab | 圖示 | 元件 | 資料來源 |
|-----|------|------|---------|
| 退回資料 | `bi-file-earmark-text` | — | 表單欄位 |
| 退回明細 | `bi-list-ul` | — | 內嵌 Detail Manager |
| 原始進貨 | `bi-box-arrow-in-down` | `PurchaseReturnReceivingTab.razor` | `IPurchaseReceivingService.GetByPurchaseReturnIdAsync` |
| 沖款記錄 | `bi-cash-coin` | `PurchaseReturnSetoffTab.razor` | `ISetoffDocumentService.GetByPurchaseReturnIdAsync` |

---

### 報價單 (Quotation) ✅

| Tab | 圖示 | 元件 | 資料來源 |
|-----|------|------|---------|
| 報價資料 | `bi-file-earmark-text` | — | 表單欄位 |
| 報價明細 | `bi-list-ul` | — | 內嵌 Detail Manager |
| 轉換訂單 | `bi-arrow-right-circle` | `QuotationOrderTab.razor` | `ISalesOrderService.GetByQuotationIdAsync` |

---

### 銷貨訂單 (SalesOrder) ✅

| Tab | 圖示 | 元件 | 資料來源 |
|-----|------|------|---------|
| 訂單資料 | `bi-file-earmark-text` | — | 表單欄位 |
| 訂單明細 | `bi-list-ul` | — | 內嵌 Detail Manager |
| 出貨記錄 | `bi-truck` | `SalesOrderDeliveryTab.razor` | `ISalesDeliveryService.GetBySalesOrderIdAsync` |
| 退回記錄 | `bi-arrow-return-left` | `SalesOrderReturnTab.razor` | `ISalesReturnService.GetBySalesOrderIdAsync` |
| 來源報價 | `bi-file-earmark-arrow-up` | `SalesOrderQuotationTab.razor` | `IQuotationService.GetBySalesOrderIdAsync` |

---

### 銷貨出貨 (SalesDelivery) ✅

| Tab | 圖示 | 元件 | 資料來源 |
|-----|------|------|---------|
| 出貨資料 | `bi-file-earmark-text` | — | 表單欄位 |
| 出貨明細 | `bi-list-ul` | — | 內嵌 Detail Manager |
| 退回記錄 | `bi-arrow-return-left` | `SalesDeliveryReturnTab.razor` | `ISalesReturnService.GetBySalesDeliveryIdAsync` |
| 沖款記錄 | `bi-cash-coin` | `SalesDeliverySetoffTab.razor` | `ISetoffDocumentService.GetBySalesDeliveryIdAsync` |
| 來源訂單 | `bi-file-earmark-arrow-up` | `SalesDeliveryOrderTab.razor` | `ISalesOrderService.GetBySalesDeliveryIdAsync` |

---

### 銷貨退回 (SalesReturn) ✅

| Tab | 圖示 | 元件 | 資料來源 |
|-----|------|------|---------|
| 退回資料 | `bi-file-earmark-text` | — | 表單欄位 |
| 退回明細 | `bi-list-ul` | — | 內嵌 Detail Manager |
| 原始出貨 | `bi-truck` | `SalesReturnDeliveryTab.razor` | `ISalesDeliveryService.GetBySalesReturnIdAsync` |
| 沖款記錄 | `bi-cash-coin` | `SalesReturnSetoffTab.razor` | `ISetoffDocumentService.GetBySalesReturnIdAsync` |

---

## Tab 元件位置彙整

### 採購類

| 元件 | 位置 |
|------|------|
| `PurchaseOrderReceivingTab.razor` | `Components/Pages/Purchase/PurchaseOrderEditModal/` |
| `PurchaseOrderReturnTab.razor` | `Components/Pages/Purchase/PurchaseOrderEditModal/` |
| `PurchaseReceivingOrderTab.razor` | `Components/Pages/Purchase/PurchaseReceivingEditModal/` |
| `PurchaseReceivingReturnTab.razor` | `Components/Pages/Purchase/PurchaseReceivingEditModal/` |
| `PurchaseReceivingSetoffTab.razor` | `Components/Pages/Purchase/PurchaseReceivingEditModal/` |
| `PurchaseReturnReceivingTab.razor` | `Components/Pages/Purchase/PurchaseReturnEditModal/` |
| `PurchaseReturnSetoffTab.razor` | `Components/Pages/Purchase/PurchaseReturnEditModal/` |

### 銷售類

| 元件 | 位置 |
|------|------|
| `QuotationOrderTab.razor` | `Components/Pages/Sales/QuotationEditModal/` |
| `SalesOrderQuotationTab.razor` | `Components/Pages/Sales/SalesOrderEditModal/` |
| `SalesOrderDeliveryTab.razor` | `Components/Pages/Sales/SalesOrderEditModal/` |
| `SalesOrderReturnTab.razor` | `Components/Pages/Sales/SalesOrderEditModal/` |
| `SalesDeliveryOrderTab.razor` | `Components/Pages/Sales/SalesDeliveryEditModal/` |
| `SalesDeliveryReturnTab.razor` | `Components/Pages/Sales/SalesDeliveryEditModal/` |
| `SalesDeliverySetoffTab.razor` | `Components/Pages/Sales/SalesDeliveryEditModal/` |
| `SalesReturnDeliveryTab.razor` | `Components/Pages/Sales/SalesReturnEditModal/` |
| `SalesReturnSetoffTab.razor` | `Components/Pages/Sales/SalesReturnEditModal/` |

---

## Service 方法彙整

### 採購類

| Service | 方法 |
|---------|------|
| `IPurchaseReceivingService` | `GetByPurchaseOrderAsync(int purchaseOrderId)` |
| `IPurchaseReturnService` | `GetByPurchaseOrderIdAsync(int purchaseOrderId)` |
| `IPurchaseReturnService` | `GetByPurchaseReceivingIdAsync(int purchaseReceivingId)` |
| `IPurchaseOrderService` | `GetByPurchaseReceivingIdAsync(int purchaseReceivingId)` |
| `IPurchaseReceivingService` | `GetByPurchaseReturnIdAsync(int purchaseReturnId)` |

### 銷售類

| Service | 方法 |
|---------|------|
| `ISalesOrderService` | `GetByQuotationIdAsync(int quotationId)` |
| `ISalesDeliveryService` | `GetBySalesOrderIdAsync(int salesOrderId)` |
| `ISalesReturnService` | `GetBySalesOrderIdAsync(int salesOrderId)` |
| `IQuotationService` | `GetBySalesOrderIdAsync(int salesOrderId)` |
| `ISalesReturnService` | `GetBySalesDeliveryIdAsync(int salesDeliveryId)` |
| `ISalesOrderService` | `GetBySalesDeliveryIdAsync(int salesDeliveryId)` |
| `ISalesDeliveryService` | `GetBySalesReturnIdAsync(int salesReturnId)` |

### 沖款類

| Service | 方法 |
|---------|------|
| `ISetoffDocumentService` | `GetByPurchaseReceivingIdAsync(int purchaseReceivingId)` |
| `ISetoffDocumentService` | `GetByPurchaseReturnIdAsync(int purchaseReturnId)` |
| `ISetoffDocumentService` | `GetBySalesDeliveryIdAsync(int salesDeliveryId)` |
| `ISetoffDocumentService` | `GetBySalesReturnIdAsync(int salesReturnId)` |

---

## 注意事項

1. **眼睛按鈕保留**：各 Table 元件（PurchaseOrderTable、SalesOrderTable 等）的眼睛按鈕維持不動。Tab 是主檔層級全覽，眼睛按鈕是明細層級細查，兩者用途不同，並存互補。

2. **跨兩層 Detail 的查詢**：部分查詢需跨兩層 Detail（如採購訂單 → 進貨明細 → 退回明細），Service 方法以 JOIN 或子查詢在後端封裝，不在前端做多次查詢。

3. **`RelatedDocumentsHelper` 不刪除**：其他使用到 RelatedDocumentsHelper 的地方（倉庫模組、財務模組）維持現有行為。

4. **`RelatedDocumentsModalComponent` 不刪除**：仍有其他地方使用，維持現有行為。

---

*最後更新：2026-02-27*
