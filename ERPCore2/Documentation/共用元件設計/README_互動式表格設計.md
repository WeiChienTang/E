# 互動式表格設計（InteractiveTableComponent）

## 更新日期
2026-02-17

---

## 概述

`InteractiveTableComponent` 是系統中所有明細表格的統一元件，支援商品選擇、數量輸入、金額計算、自動空行管理等功能。目前共有 **18 個 Table 組件** 使用此元件，其中 15 個已完成 Helper 遷移。

---

## 架構總覽

```
InteractiveTableComponent<TItem>
├── 核心功能
│   ├── 欄位定義 (ColumnDefinitions)
│   ├── 自動空行管理 (EnableAutoEmptyRow)
│   ├── 行號顯示 (ShowRowNumbers)
│   └── 唯讀模式 (IsReadOnly)
│
└── 7 個 Helper（1,499 行，63 方法）
    ├── CalculationHelper.cs         (159 行, 9 方法) - 金額計算
    ├── InputEventHelper.cs          (162 行, 9 方法) - 輸入事件處理
    ├── SearchableSelectHelper.cs    (299 行, 10 方法) - 商品搜尋選擇
    ├── DetailSyncHelper.cs          (184 行, 8 方法) - 資料同步
    ├── ValidationHelper.cs          (318 行, 14 方法) - 驗證檢查
    ├── ItemManagementHelper.cs      (170 行, 5 方法) - 項目管理
    └── HistoryCheckHelper.cs        (207 行, 4 方法) - 歷史記錄檢查
```

---

## 使用 InteractiveTableComponent 的元件

| 模組 | 元件 | 說明 | Helper 遷移 |
|------|------|------|-------------|
| 銷售 | SalesOrderTable | 銷貨訂單明細 | ✅ |
| 銷售 | SalesDeliveryTable | 銷貨出貨明細 | ✅ |
| 銷售 | SalesReturnTable | 銷貨退回明細 | ✅ |
| 採購 | PurchaseOrderTable | 採購訂單明細 | ✅ |
| 採購 | PurchaseReceivingTable | 採購進貨明細 | ✅ |
| 採購 | PurchaseReturnTable | 採購退回明細 | ✅ |
| 報價 | QuotationTable | 報價單明細 | ✅ |
| 商品 | ProductCompositionTable | 商品組成/配方 | ✅ |
| 商品 | ProductSupplierTable | 商品供應商 | ✅ |
| 商品 | ProductBarcodePrintTable | 條碼列印 | ⚪ 無需遷移 |
| 庫存 | InventoryStockTable | 庫存盤點明細 | ✅ |
| 沖銷 | SetoffProductTable | 商品沖銷 | ✅ |
| 沖銷 | SetoffPrepaymentTable | 預付款沖銷 | ✅ |
| 沖銷 | SetoffPaymentTable | 付款沖銷 | ✅ |
| 廠商 | SupplierProductTable | 廠商商品 | ✅ |
| 領料 | MaterialIssueTable | 領料單明細 | ✅ |
| 採購 | BatchApprovalTable | 批次審核 | ⚪ 無需遷移 |
| 測試 | TestTable | 測試用途 | ⚪ 無需遷移 |

---

## 7 個 Helper 說明

### 1. CalculationHelper - 金額計算

統一所有金額計算邏輯，支援多種稅率算法和折扣。

```csharp
// 計算小計（支援稅外加、稅內含、免稅）
var subtotal = CalculationHelper.CalculateSubtotal(
    quantity, unitPrice, discountPercentage, taxRate, taxMethod);

// 計算稅額
var tax = CalculationHelper.CalculateTaxAmount(subtotal, taxRate, taxMethod);

// 計算總計
var total = CalculationHelper.CalculateTotal(items, i => i.Subtotal);
```

### 2. InputEventHelper - 輸入事件處理

統一數量、價格、百分比、文字等輸入事件的處理。

```csharp
item.Quantity = InputEventHelper.HandleQuantityInput(value);
item.Price = InputEventHelper.HandlePriceInput(value);
item.TaxRate = InputEventHelper.HandlePercentageInput(value, min: 0, max: 100);
```

### 3. SearchableSelectHelper - 商品搜尋選擇

完整的商品搜尋、選擇、焦點管理、鍵盤導航處理。

```csharp
// 處理商品搜尋
SearchableSelectHelper.HandleProductSearch(item, searchValue, availableProducts, ...);

// 處理商品選擇（含稅率自動帶入）
await SearchableSelectHelper.HandleProductSelectionAsync(item, product, ...);

// 格式化顯示文字
var text = SearchableSelectHelper.FormatProductDisplayText(product);
// → "[P001] 商品名稱"
```

### 4. DetailSyncHelper - 資料同步

統一明細資料的載入、同步、轉換邏輯。

```csharp
// 同步明細到父組件
await DetailSyncHelper.SyncToParentAsync(items, onItemsChanged, StateHasChanged);

// 載入現有明細
var items = DetailSyncHelper.LoadExistingDetails(existingDetails, converter);
```

### 5. ValidationHelper - 驗證檢查

綜合刪除檢查、重複檢查、數量/價格驗證。

```csharp
// 檢查是否可刪除
if (!ValidationHelper.CanDeleteItem(item, out reason, checkReturn: HasReturn))
    await NotificationService.ShowWarningAsync(reason);

// 數量驗證
ValidationHelper.ValidateQuantity(quantity, maxQuantity, out error);
```

### 6. ItemManagementHelper - 項目管理

刪除項目、清除所有明細等操作。

```csharp
// 刪除項目（含驗證和通知）
await ItemManagementHelper.HandleItemDeleteAsync(item, items, canDeleteChecker, notificationService);

// 清除所有明細（含確認）
await ItemManagementHelper.ClearAllDetailsAsync(items, jsRuntime);
```

### 7. HistoryCheckHelper - 歷史記錄檢查

智能下單功能：載入、合併歷史記錄。

```csharp
// 載入歷史記錄
var history = await HistoryCheckHelper.LoadHistoryAsync(supplierId, service);
```

---

## 自動空行管理機制

### 核心特性

- 初始化時自動新增一個空行
- 用戶填寫資料後自動新增新的空行
- 支援指定「觸發欄位」，只有關鍵欄位有值才新增空行
- 刪除項目後自動補充空行
- 最後一個空行保護機制（防止刪除唯一空行）

### 參數配置

```razor
<InteractiveTableComponent TItem="ProductItem"
                          Items="@ProductItems"
                          ColumnDefinitions="@GetColumnDefinitions()"
                          EnableAutoEmptyRow="true"
                          DataLoadCompleted="@_dataLoadCompleted"
                          CreateEmptyItem="@CreateNewEmptyItem"
                          IsReadOnly="@IsReadOnly" />
```

| 參數 | 類型 | 說明 |
|------|------|------|
| `EnableAutoEmptyRow` | `bool` | 啟用自動空行管理 |
| `DataLoadCompleted` | `bool` | 資料載入完成標記（預設 true） |
| `CreateEmptyItem` | `Func<TItem>` | 建立空項目的工廠方法 |

### 空行判斷模式

**模式 A：觸發欄位模式（優先）** - 當有欄位設定 `TriggerEmptyRowOnFilled = true` 時啟用，只有觸發欄位有值才算非空行。

**模式 B：傳統模式** - 檢查所有欄位，任一欄位有值即算非空行。

### 載入資料時的控制

```csharp
private bool _dataLoadCompleted = true;

private async Task LoadExistingDetailsAsync()
{
    _dataLoadCompleted = false;  // 暫停空行檢查
    // ... 載入資料
    _dataLoadCompleted = true;   // 恢復空行檢查
    StateHasChanged();
}
```

### 資料模型建議

使用 nullable 類型避免數字 `0` 被誤判為有值：

```csharp
public class ProductItem
{
    public int? Quantity { get; set; } = null;     // null = 空
    public decimal? Price { get; set; } = null;    // null = 空
}
```

---

## 遷移狀態

所有 Table 元件已完成自動空行遷移（2026-01-02），統一使用 `InteractiveTableComponent` 的內建功能，不再使用已刪除的 `AutoEmptyRowHelper.cs`。

---

## 不建議抽離的部分

1. **GetColumnDefinitions()** - 每個 Table 的欄位配置差異大
2. **Item 內部類別** - 各 Table 的 Item 結構不同
3. **業務邏輯特有方法** - 如 `LoadSmartOrderItems()`、`HandleCompositionSave()` 等
4. **LoadExistingDetailsAsync()** - 各 Table 的載入邏輯、欄位映射差異過大

---

## 檔案位置

所有 Helper 位於 `Helpers/InteractiveTableComponentHelper/` 目錄：

```
Helpers/InteractiveTableComponentHelper/
├── CalculationHelper.cs
├── InputEventHelper.cs
├── SearchableSelectHelper.cs
├── DetailSyncHelper.cs
├── ValidationHelper.cs
├── ItemManagementHelper.cs
├── HistoryCheckHelper.cs
└── DetailLockHelper.cs
```

---

## 相關文件

- [README_共用元件設計總綱.md](README_共用元件設計總綱.md) - 共用元件設計總綱
