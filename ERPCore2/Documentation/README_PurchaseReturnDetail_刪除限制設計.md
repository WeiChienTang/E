# 採購退回明細刪除限制說明

## 📋 概述

本文件說明 `PurchaseReturnDetailManagerComponent` 組件中，對於退回明細項目的刪除限制邏輯。

## 🔒 刪除限制規則

退回明細項目在以下情況下**不可刪除**：

### 1️⃣ 有沖款記錄的明細（已實作）

#### 檢查資料表與欄位
- **資料表**: `PurchaseReturnDetail` (採購退回明細表)
- **檢查欄位**: `TotalReceivedAmount` (累計收款金額)
- **資料類型**: `decimal(18,2)`
- **預設值**: `0`

#### 檢查邏輯
```
PurchaseReturnDetail.TotalReceivedAmount > 0 → 有沖款記錄 → 不可刪除
```

#### 資料結構
```csharp
public class PurchaseReturnDetail : BaseEntity
{
    // ... 其他屬性 ...
    
    [Display(Name = "累計收款金額")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalReceivedAmount { get; set; } = 0;
    
    // ... Navigation Properties ...
}
```

#### 限制原因
- 已收款的退回明細不可刪除
- 避免財務資料錯亂
- 保持應收帳款與退回記錄的一致性
- 防止已收款的項目被意外刪除

#### 實作位置
- **檢查方法**: `HasPaymentRecord(ReturnItem item)`
  - 直接檢查 `PurchaseReturnDetail.TotalReceivedAmount > 0`
  - 返回 `true` 表示有沖款記錄

- **取得金額**: `GetReceivedAmount(ReturnItem item)`
  - 返回 `PurchaseReturnDetail.TotalReceivedAmount`
  - 用於顯示訊息

---

## 🔍 綜合檢查方法

### `CanDeleteItem(ReturnItem item, out string reason)`

這是一個綜合檢查方法，用於驗證項目是否可以刪除。

#### 檢查內容
1. **沖款記錄檢查** (`HasPaymentRecord`)
   - 如果有沖款記錄，返回 `false`，並提供已收款金額訊息

2. **通過所有檢查**
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

### 2. 事件處理限制

**刪除事件處理**: `HandleItemDelete`
```csharp
if (!CanDeleteItem(item, out string reason))
{
    await NotificationService.ShowWarningAsync(reason, "操作限制");
    return;
}
```

---

## 📊 資料流程圖

### 刪除操作完整流程

```
使用者點擊刪除按鈕（如果顯示）
    ↓
HandleItemDelete(item)
    ↓
CanDeleteItem(item, out reason)
    ↓
    └─→ HasPaymentRecord(item)?
            ↓ YES
            檢查 PurchaseReturnDetail.TotalReceivedAmount
            ↓
            欄位: TotalReceivedAmount > 0?
            ↓
            返回 false，提供已收款金額訊息
            ↓
            顯示警告訊息，阻止刪除
    
    └─→ 通過檢查
            ↓
            返回 true
            ↓
            RemoveItemAsync(index)
            ↓
            執行刪除
```

---

## 🔧 相關方法清單

### 檢查方法
| 方法名稱 | 用途 | 檢查對象 | 返回值 |
|---------|------|---------|--------|
| `HasPaymentRecord(item)` | 檢查是否有沖款記錄 | `TotalReceivedAmount` 欄位 | `bool` |
| `GetReceivedAmount(item)` | 取得已收款金額 | `TotalReceivedAmount` 欄位 | `decimal` |
| `CanDeleteItem(item, out reason)` | 綜合檢查是否可刪除 | 沖款檢查 | `bool` + 原因 |

### UI 控制方法
| 方法名稱 | 用途 | 檢查內容 |
|---------|------|---------|
| `GetCustomActionsTemplate` | 控制刪除按鈕顯示 | 呼叫 `CanDeleteItem` |
| `HandleItemDelete` | 刪除前檢查 | 呼叫 `CanDeleteItem` |

---

## 📝 使用者提示訊息

### 刪除限制訊息
- **有沖款記錄**: "此商品已有沖款記錄（已收款 X 元），無法刪除"

---

## 🎨 視覺提示

### 刪除按鈕控制
- **有沖款記錄**: 不顯示刪除按鈕
- **無沖款記錄**: 顯示刪除按鈕

---

## ⚠️ 注意事項

1. **檢查方式**
   - `TotalReceivedAmount` 是即時檢查（直接讀取實體屬性）
   - 不需要額外查詢 `SetoffProductDetail` 表
   - `TotalReceivedAmount` 欄位由系統自動維護更新

2. **效能考量**
   - 直接讀取實體屬性，無需額外查詢
   - 檢查速度快，不會影響UI效能

3. **與進貨明細的對應關係**
   - 進貨明細 (`PurchaseReceivingDetail`) 使用 `TotalPaidAmount`（付款金額）
   - 退回明細 (`PurchaseReturnDetail`) 使用 `TotalReceivedAmount`（收款金額）
   - 檢查邏輯完全相同，只是欄位名稱不同

4. **擴展性**
   - `CanDeleteItem` 方法可輕鬆擴展新的檢查條件
   - 所有檢查邏輯集中在一個方法中，便於維護

---

## 📌 總結

### 刪除限制檢查完整清單

| 限制類型 | 檢查資料表 | 檢查欄位 | 檢查方法 | 限制原因 |
|---------|-----------|---------|---------|---------|
| 沖款記錄 | `PurchaseReturnDetail` | `TotalReceivedAmount > 0` | `HasPaymentRecord` | 保持財務資料一致性 |

### 受限制的操作

| 操作 | 檢查點 | UI 控制 |
|-----|-------|--------|
| 刪除項目 | `HandleItemDelete` | 隱藏刪除按鈕 |

---

## 🔄 與進貨明細的比較

| 項目 | 進貨明細 | 退回明細 |
|-----|---------|---------|
| 實體類別 | `PurchaseReceivingDetail` | `PurchaseReturnDetail` |
| 檢查欄位 | `TotalPaidAmount` (付款) | `TotalReceivedAmount` (收款) |
| 檢查方法 | `HasPaymentRecord` | `HasPaymentRecord` |
| 限制原因 | 避免應付帳款錯亂 | 避免應收帳款錯亂 |
| 額外檢查 | 退貨記錄檢查 | _(無)_ |

---

**文件版本**: 1.0  
**最後更新**: 2025年10月11日  
**維護者**: 開發團隊  
**參考文件**: `README_刪除限制設計.md` (進貨明細版本)
