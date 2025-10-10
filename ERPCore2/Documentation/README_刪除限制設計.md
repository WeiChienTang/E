# 採購進貨明細刪除限制說明

## 📋 概述

本文件說明 `PurchaseReceivingDetailManagerComponent` 組件中，對於進貨明細項目的刪除限制邏輯。

## 🔒 刪除限制規則

進貨明細項目在以下兩種情況下**不可刪除、不可修改**：

### 1️⃣ 有退貨記錄的明細（已實作）

#### 檢查資料表與欄位
- **主要資料表**: `PurchaseReturnDetail` (採購退貨明細表)
- **關聯欄位**: `PurchaseReceivingDetailId` (關聯的進貨明細ID)
- **檢查方法**: `PurchaseReturnDetailService.GetReturnedQuantityByReceivingDetailAsync(detailId)`

#### 檢查邏輯流程
```
PurchaseReceivingDetail (進貨明細)
    ↓ (透過 Id 關聯)
PurchaseReturnDetail.PurchaseReceivingDetailId
    ↓ (查詢該進貨明細的所有退貨記錄)
    ↓ (計算退貨總數量)
已退貨數量 > 0 → 不可刪除
```

#### 資料結構關聯
```sql
-- 查詢邏輯示意（實際由 Service 層處理）
SELECT SUM(ReturnQuantity) as TotalReturnedQuantity
FROM PurchaseReturnDetail
WHERE PurchaseReceivingDetailId = @purchaseReceivingDetailId
  AND IsDeleted = 0
```

#### 限制原因
- 已有退貨記錄的進貨明細不可刪除
- 避免造成退貨記錄的孤兒資料
- 保持進貨與退貨之間的資料一致性

#### 實作位置
- **載入方法**: `LoadReturnedQuantitiesAsync()`
  - 在組件初始化時載入所有進貨明細的退貨數量
  - 儲存在 `_returnedQuantities` 字典中
  
- **檢查方法**: `HasReturnRecord(ReceivingItem item)`
  - 檢查 `_returnedQuantities` 字典是否包含該明細ID
  - 返回 `true` 表示有退貨記錄

- **取得數量**: `GetReturnedQuantity(ReceivingItem item)`
  - 從 `_returnedQuantities` 字典取得已退貨數量
  - 用於顯示和驗證

---

### 2️⃣ 有沖款記錄的明細（新增）

#### 檢查資料表與欄位
- **資料表**: `PurchaseReceivingDetail` (採購進貨明細表)
- **檢查欄位**: `TotalPaidAmount` (累計付款金額)
- **資料類型**: `decimal(18,2)`
- **預設值**: `0`

#### 檢查邏輯
```
PurchaseReceivingDetail.TotalPaidAmount > 0 → 有沖款記錄 → 不可刪除
```

#### 資料結構
```csharp
public class PurchaseReceivingDetail : BaseEntity
{
    // ... 其他屬性 ...
    
    [Display(Name = "累計付款金額")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPaidAmount { get; set; } = 0;
    
    // ... Navigation Properties ...
}
```

#### 限制原因
- 已沖款的進貨明細不可刪除
- 避免財務資料錯亂
- 保持應付帳款與進貨記錄的一致性
- 防止已付款的項目被意外刪除

#### 實作位置
- **檢查方法**: `HasPaymentRecord(ReceivingItem item)`
  - 直接檢查 `PurchaseReceivingDetail.TotalPaidAmount > 0`
  - 返回 `true` 表示有沖款記錄

- **取得金額**: `GetPaidAmount(ReceivingItem item)`
  - 返回 `PurchaseReceivingDetail.TotalPaidAmount`
  - 用於顯示訊息

---

## 🔍 綜合檢查方法

### `CanDeleteItem(ReceivingItem item, out string reason)`

這是一個綜合檢查方法，整合了所有刪除限制條件。

#### 檢查順序
1. **退貨記錄檢查** (`HasReturnRecord`)
   - 如果有退貨記錄，返回 `false`，並提供退貨數量訊息
   
2. **沖款記錄檢查** (`HasPaymentRecord`)
   - 如果有沖款記錄，返回 `false`，並提供已沖款金額訊息
   
3. **通過所有檢查**
   - 返回 `true`，項目可以刪除

#### 使用範例
```csharp
if (!CanDeleteItem(item, out string reason))
{
    await NotificationService.ShowWarningAsync(reason, "操作限制");
    return;
}
// 可以執行刪除...
```

---

## 🎯 UI 層級的限制

### 1. 刪除按鈕顯示控制

**位置**: `GetCustomActionsTemplate`

```csharp
// 只有當項目可以刪除時才顯示刪除按鈕
var canDelete = CanDeleteItem(item, out _);
if (canDelete)
{
    // 顯示刪除按鈕
}
// 否則不顯示刪除按鈕
```

### 2. 欄位唯讀鎖定

有退貨記錄或沖款記錄的項目，以下欄位會被鎖定為唯讀：

#### 🔒 商品選擇欄位
- 顯示鎖定圖示 (🔒)
- Tooltip 顯示限制原因
- 輸入框變為純文字顯示

#### 🔒 倉庫選擇欄位
- 下拉選單變為純文字顯示
- Tooltip 顯示限制原因

#### 🔒 庫位選擇欄位
- 下拉選單變為純文字顯示
- Tooltip 顯示限制原因

#### 實作方式
```csharp
var hasReturnRecord = HasReturnRecord(receivingItem);
var hasPaymentRecord = HasPaymentRecord(receivingItem);
var isFieldReadOnly = IsReadOnly || hasReturnRecord || hasPaymentRecord;

// 組合 tooltip 訊息
var tooltipMessages = new List<string>();
if (hasReturnRecord)
    tooltipMessages.Add("此商品已有退貨記錄");
if (hasPaymentRecord)
{
    var paidAmount = GetPaidAmount(receivingItem);
    tooltipMessages.Add($"此商品已有沖款記錄（已沖款 {paidAmount:N0} 元）");
}
var title = tooltipMessages.Any() 
    ? string.Join("；", tooltipMessages) + "，無法修改" 
    : "";
```

### 3. 事件處理限制

**倉庫選擇變更**: `OnWarehouseSelectionChanged`
```csharp
if (!CanDeleteItem(item, out string reason))
{
    var friendlyMessage = HasReturnRecord(item) 
        ? "此商品已有退貨記錄，無法修改倉庫設定" 
        : "此商品已有沖款記錄，無法修改倉庫設定";
    
    await NotificationService.ShowWarningAsync(friendlyMessage, "操作限制");
    return;
}
```

**庫位選擇變更**: `OnWarehouseLocationSelectionChanged`
```csharp
if (!CanDeleteItem(item, out string reason))
{
    var friendlyMessage = HasReturnRecord(item) 
        ? "此商品已有退貨記錄，無法修改庫位設定" 
        : "此商品已有沖款記錄，無法修改庫位設定";
    
    await NotificationService.ShowWarningAsync(friendlyMessage, "操作限制");
    return;
}
```

---

## 📊 資料流程圖

### 刪除操作完整流程

```
使用者點擊刪除按鈕
    ↓
HandleItemDelete(item)
    ↓
CanDeleteItem(item, out reason)
    ↓
    ├─→ HasReturnRecord(item)?
    │       ↓ YES
    │       檢查 _returnedQuantities 字典
    │       ↓
    │       查詢來源: PurchaseReturnDetail 表
    │       關聯欄位: PurchaseReceivingDetailId
    │       ↓
    │       返回 false，提供退貨數量訊息
    │
    ├─→ HasPaymentRecord(item)?
    │       ↓ YES
    │       檢查 PurchaseReceivingDetail.TotalPaidAmount
    │       ↓
    │       欄位: TotalPaidAmount > 0?
    │       ↓
    │       返回 false，提供已沖款金額訊息
    │
    └─→ 通過所有檢查
            ↓
            返回 true
            ↓
            RemoveItemAsync(index)
            ↓
            執行刪除
```

### 數量驗證流程

```
OnReceivedQuantityInput
    ↓
取得已退貨數量: GetReturnedQuantity(item)
    ↓
    └─→ 查詢 _returnedQuantities[detail.Id]
            ↓
            來源: PurchaseReturnDetail 表
            ↓
若輸入數量 < 已退貨數量
    ↓
    顯示警告
    ↓
    自動調整為最小允許值（已退貨數量）
```

---

## 🔧 相關方法清單

### 檢查方法
| 方法名稱 | 用途 | 檢查對象 | 返回值 |
|---------|------|---------|--------|
| `HasReturnRecord(item)` | 檢查是否有退貨記錄 | `_returnedQuantities` 字典 | `bool` |
| `GetReturnedQuantity(item)` | 取得已退貨數量 | `_returnedQuantities` 字典 | `int` |
| `HasPaymentRecord(item)` | 檢查是否有沖款記錄 | `TotalPaidAmount` 欄位 | `bool` |
| `GetPaidAmount(item)` | 取得已沖款金額 | `TotalPaidAmount` 欄位 | `decimal` |
| `CanDeleteItem(item, out reason)` | 綜合檢查是否可刪除 | 退貨+沖款檢查 | `bool` + 原因 |

### 載入方法
| 方法名稱 | 用途 | 執行時機 |
|---------|------|---------|
| `LoadReturnedQuantitiesAsync()` | 載入所有退貨數量 | `LoadExistingDetailsAsync()` 之後 |

### UI 控制方法
| 方法名稱 | 用途 | 檢查內容 |
|---------|------|---------|
| `GetCustomActionsTemplate` | 控制刪除按鈕顯示 | 呼叫 `CanDeleteItem` |
| `OnWarehouseSelectionChanged` | 倉庫變更檢查 | 呼叫 `CanDeleteItem` |
| `OnWarehouseLocationSelectionChanged` | 庫位變更檢查 | 呼叫 `CanDeleteItem` |

---

## 📝 使用者提示訊息

### 刪除限制訊息
- **有退貨記錄**: "此商品已有退貨記錄（已退貨 X 個），無法刪除"
- **有沖款記錄**: "此商品已有沖款記錄（已沖款 X 元），無法刪除"

### 修改限制訊息
- **商品選擇**: Tooltip 顯示 "此商品已有退貨記錄；此商品已有沖款記錄（已沖款 X 元），無法修改商品選擇"
- **倉庫選擇**: "此商品已有[退貨/沖款]記錄，無法修改倉庫設定"
- **庫位選擇**: "此商品已有[退貨/沖款]記錄，無法修改庫位設定"

### 數量限制訊息
- **低於退貨數量**: "進貨數量不可低於已退貨數量 X"

---

## 🎨 視覺提示

### 鎖定圖示 (🔒)
- 顯示位置: 商品選擇欄位右側
- 顯示條件: `hasReturnRecord || hasPaymentRecord`
- 圖示樣式: `fas fa-lock text-danger`
- Tooltip: 顯示限制原因

### 欄位樣式
- **唯讀欄位**: 文字變為 `text-muted small`
- **警告邊框**: 數量欄位若有退貨記錄，顯示 `border-warning`

---

## ⚠️ 注意事項

1. **資料載入順序**
   - 必須先執行 `LoadExistingDetailsAsync()`
   - 再執行 `LoadReturnedQuantitiesAsync()`
   - 確保 `_returnedQuantities` 字典已正確填充

2. **即時性考量**
   - `TotalPaidAmount` 是即時檢查（直接讀取實體屬性）
   - 退貨數量在組件初始化時載入，不會即時更新
   - 如需即時更新，需在新增退貨後重新載入

3. **效能考量**
   - 退貨數量使用字典快取，避免重複查詢資料庫
   - 沖款金額直接讀取實體屬性，無需額外查詢

4. **擴展性**
   - `CanDeleteItem` 方法可輕鬆擴展新的檢查條件
   - 所有檢查邏輯集中在一個方法中，便於維護

---

## 📌 總結

### 刪除限制檢查完整清單

| 限制類型 | 檢查資料表 | 檢查欄位/關聯 | 檢查方法 | 限制原因 |
|---------|-----------|--------------|---------|---------|
| 退貨記錄 | `PurchaseReturnDetail` | `PurchaseReceivingDetailId` | `HasReturnRecord` | 保持退貨資料一致性 |
| 沖款記錄 | `PurchaseReceivingDetail` | `TotalPaidAmount > 0` | `HasPaymentRecord` | 保持財務資料一致性 |

### 受限制的操作

| 操作 | 檢查點 | UI 控制 |
|-----|-------|--------|
| 刪除項目 | `HandleItemDelete` | 隱藏刪除按鈕 |
| 修改商品 | 欄位唯讀檢查 | 輸入框變文字 + 🔒 |
| 修改倉庫 | `OnWarehouseSelectionChanged` | 下拉變文字 |
| 修改庫位 | `OnWarehouseLocationSelectionChanged` | 下拉變文字 |
| 修改數量 | `OnReceivedQuantityInput` | 最小值限制 |

---

**文件版本**: 1.0  
**最後更新**: 2025年10月10日  
**維護者**: 開發團隊
