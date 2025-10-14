# 進貨明細管理組件 - 鎖定明細保護機制

## 📋 問題描述

在 `PurchaseReceivingDetailManagerComponent` 組件中發現一個重要的安全性問題：

當進貨明細已有退貨記錄或沖款記錄時，這些明細應該被鎖定不可修改。雖然在單一明細的欄位層級已經實作了鎖定機制，但 **Footer 的批次操作按鈕** 仍然可能對這些被鎖定的明細進行操作，造成資料不一致的風險。

### 具體問題

1. **「載入所有未入庫」按鈕**：會清空所有明細（包括被鎖定的），這會破壞已有退貨/沖款記錄的資料完整性
2. **「統一倉庫」按鈕**：會修改所有明細的倉庫設定，包括被鎖定的明細
3. **「進貨量全填」按鈕**：會修改所有明細的數量，包括被鎖定的明細
4. **「進貨量清空」按鈕**：會清空所有明細的數量，包括被鎖定的明細
5. **「清除明細」按鈕**：會刪除所有明細，包括被鎖定的明細

## ✅ 解決方案

採用 **智能過濾策略**，針對不同按鈕實作不同的保護機制：

### 1. 「載入所有未入庫」按鈕

**策略**：完全禁用

當編輯模式下存在任何被鎖定的明細時，完全禁用此按鈕，避免誤刪重要資料。

```csharp
/// <summary>
/// 判斷是否可以使用「載入所有未入庫」功能
/// 規則：
/// 1. 必須選擇廠商
/// 2. 必須有可載入的採購明細
/// 3. 如果已有明細資料：
///    - 若存在被鎖定的明細（有退貨或沖款記錄），則禁用此功能（避免誤刪重要資料）
///    - 若都是未鎖定的明細，則可以使用（會清空現有資料並載入新的）
/// </summary>
private bool CanLoadAllUnreceived => 
    SelectedSupplierId.HasValue && 
    SelectedSupplierId.Value > 0 && 
    GetAvailablePurchaseDetails().Any() &&
    !HasLockedDetails(); // 新增：如果有被鎖定的明細，禁用此功能

/// <summary>
/// 檢查是否有被鎖定的明細（有退貨或沖款記錄的明細）
/// </summary>
private bool HasLockedDetails()
{
    return ReceivingItems.Any(item => 
        !IsEmptyRow(item) && 
        (HasReturnRecord(item) || HasPaymentRecord(item)));
}
```

**行為**：
- ✅ 沒有被鎖定的明細時：按鈕可用，執行時會清空現有資料並載入所有未入庫項目
- ❌ 有被鎖定的明細時：按鈕禁用（變灰），滑鼠懸停時不會有任何操作

### 2. 「統一倉庫」按鈕

**策略**：智能過濾，只處理未鎖定的明細

```csharp
// 區分可修改和被鎖定的明細
var unlocked = nonEmptyItems.Where(item => CanDeleteItem(item, out _)).ToList();
var locked = nonEmptyItems.Where(item => !CanDeleteItem(item, out _)).ToList();

if (!unlocked.Any())
{
    await NotificationService.ShowWarningAsync(
        "所有明細都已有退貨或沖款記錄，無法統一設定倉庫", 
        "操作限制");
    return;
}

// 批量更新未鎖定的明細項目的倉庫
foreach (var item in unlocked)
{
    item.SelectedWarehouse = selectedWarehouse;
    item.SelectedWarehouseLocation = null;
}

// 顯示成功通知，並提示跳過的項目數量
var message = $"已統一設定 {unlocked.Count} 項明細的倉庫為：{selectedWarehouse.Name}";
if (locked.Any())
{
    message += $"\n（已跳過 {locked.Count} 項已鎖定的明細）";
}
```

**行為**：
- ✅ 只修改未鎖定的明細
- 📌 被鎖定的明細保持不變
- 💬 操作完成後顯示清楚的提示訊息，告知處理了多少項，跳過了多少項

### 3. 「進貨量全填」按鈕

**策略**：智能過濾，只處理未鎖定的明細

```csharp
var unlocked = nonEmptyItems.Where(item => CanDeleteItem(item, out _)).ToList();
var locked = nonEmptyItems.Where(item => !CanDeleteItem(item, out _)).ToList();

if (!unlocked.Any())
{
    await NotificationService.ShowWarningAsync(
        "所有明細都已有退貨或沖款記錄，無法批次填入數量", 
        "操作限制");
    return;
}

foreach (var item in unlocked)
{
    if (item.SelectedPurchaseDetail != null)
    {
        item.ReceivedQuantity = item.OrderQuantity;
    }
}

var message = $"已填入 {unlocked.Count} 項明細的進貨數量";
if (locked.Any())
{
    message += $"\n（已跳過 {locked.Count} 項已鎖定的明細）";
}
```

**行為**：
- ✅ 只填入未鎖定明細的數量（設為採購數量）
- 📌 被鎖定的明細數量保持不變
- 💬 顯示操作結果和跳過的項目數量

### 4. 「進貨量清空」按鈕

**策略**：智能過濾，只處理未鎖定的明細

```csharp
var unlocked = nonEmptyItems.Where(item => CanDeleteItem(item, out _)).ToList();
var locked = nonEmptyItems.Where(item => !CanDeleteItem(item, out _)).ToList();

if (!unlocked.Any())
{
    await NotificationService.ShowWarningAsync(
        "所有明細都已有退貨或沖款記錄，無法批次清空數量", 
        "操作限制");
    return;
}

foreach (var item in unlocked)
{
    // 如果有退貨記錄，保留已退貨的數量作為最小值
    var returnedQty = GetReturnedQuantity(item);
    item.ReceivedQuantity = returnedQty;
}

var message = $"已清空 {unlocked.Count} 項明細的進貨數量";
if (locked.Any())
{
    message += $"\n（已跳過 {locked.Count} 項已鎖定的明細）";
}
```

**行為**：
- ✅ 只清空未鎖定明細的數量（設為 0 或已退貨數量）
- 📌 被鎖定的明細數量保持不變
- 💬 顯示操作結果和跳過的項目數量

### 5. 「清除明細」按鈕

**策略**：智能過濾，只刪除未鎖定的明細

```csharp
// 區分可刪除和被鎖定的明細
var unlocked = nonEmptyItems.Where(item => CanDeleteItem(item, out _)).ToList();
var locked = nonEmptyItems.Where(item => !CanDeleteItem(item, out _)).ToList();

if (!unlocked.Any())
{
    await NotificationService.ShowWarningAsync(
        "所有明細都已有退貨或沖款記錄，無法清除", 
        "操作限制");
    return;
}

// 從列表中移除未鎖定的項目，保留已鎖定的項目
foreach (var item in unlocked)
{
    ReceivingItems.Remove(item);
}

var message = $"已清除 {unlocked.Count} 項明細";
if (locked.Any())
{
    message += $"\n（已保留 {locked.Count} 項已鎖定的明細）";
}
```

**行為**：
- ✅ 只刪除未鎖定的明細
- 📌 被鎖定的明細保留在列表中
- 💬 顯示清除了多少項，保留了多少項

## 🔒 什麼是「被鎖定的明細」？

明細會在以下情況下被鎖定：

1. **有退貨記錄**：`HasReturnRecord(item)` 為 true
   - 檢查方式：透過 `PurchaseReturnDetailService.GetReturnedQuantityByReceivingDetailAsync()` 查詢
   - 鎖定原因：已有退貨記錄的進貨明細不可刪除或大幅修改，以保持退貨資料的完整性

2. **有沖款記錄**：`HasPaymentRecord(item)` 為 true
   - 檢查方式：檢查 `PurchaseReceivingDetail.TotalPaidAmount > 0`
   - 鎖定原因：已沖款的進貨明細不可刪除或修改，避免財務資料錯亂

綜合判斷方法：
```csharp
private bool CanDeleteItem(ReceivingItem item, out string reason)
{
    if (HasReturnRecord(item)) { ... return false; }
    if (HasPaymentRecord(item)) { ... return false; }
    return true;
}
```

## 📊 使用者體驗改善

### 視覺回饋

1. **按鈕禁用**：當「載入所有未入庫」按鈕被禁用時，按鈕會變灰並且無法點擊

2. **明確的操作結果通知**：
   ```
   已統一設定 5 項明細的倉庫為：主倉庫
   （已跳過 2 項已鎖定的明細）
   ```

3. **全部鎖定時的友善提示**：
   ```
   所有明細都已有退貨或沖款記錄，無法統一設定倉庫
   ```

### 操作流程

#### 情境 1：有部分被鎖定的明細

```
使用者點擊「統一倉庫」→ 選擇倉庫
↓
系統處理：
  - 修改 3 個未鎖定的明細倉庫
  - 保留 2 個被鎖定的明細不變
↓
顯示通知：
  已統一設定 3 項明細的倉庫為：主倉庫
  （已跳過 2 項已鎖定的明細）
```

#### 情境 2：全部都被鎖定

```
使用者點擊「統一倉庫」→ 選擇倉庫
↓
系統檢查：所有明細都被鎖定
↓
顯示警告：
  所有明細都已有退貨或沖款記錄，無法統一設定倉庫
```

#### 情境 3：沒有被鎖定的明細

```
使用者點擊「統一倉庫」→ 選擇倉庫
↓
系統處理：修改所有 5 個明細的倉庫
↓
顯示通知：
  已統一設定 5 項明細的倉庫為：主倉庫
```

## 🎯 設計理念

### 為什麼採用不同策略？

1. **「載入所有未入庫」採用完全禁用**：
   - 這個操作會 **清空所有現有明細**，風險極高
   - 如果誤操作，被鎖定的明細會永久遺失（即使儲存前可復原，但容易被忽略）
   - 因此採取最保守的策略：有被鎖定的明細時，完全不允許使用

2. **其他批次操作採用智能過濾**：
   - 這些操作是 **修改** 而非 **刪除**，風險較低
   - 使用者可能確實需要批次修改未鎖定的明細，而不想影響已鎖定的
   - 透過清楚的提示訊息，使用者可以知道哪些被修改、哪些被跳過

### 設計原則

1. **資料完整性優先**：絕不允許破壞已有退貨/沖款記錄的明細
2. **使用者友善**：提供清楚的回饋訊息，讓使用者知道發生了什麼
3. **操作彈性**：在保護資料的前提下，盡可能提供批次操作的便利性

## 🧪 測試建議

### 測試案例

#### TC1：載入所有未入庫（有被鎖定明細）
1. 建立一筆進貨單，包含 3 個明細
2. 其中 1 個明細已有退貨記錄
3. 進入編輯模式
4. **預期結果**：「載入所有未入庫」按鈕被禁用（灰色）

#### TC2：統一倉庫（混合明細）
1. 建立一筆進貨單，包含 5 個明細
2. 其中 2 個明細已有退貨記錄
3. 點擊「統一倉庫」→ 選擇「主倉庫」
4. **預期結果**：
   - 3 個未鎖定明細的倉庫改為「主倉庫」
   - 2 個被鎖定明細的倉庫不變
   - 顯示訊息：「已統一設定 3 項明細的倉庫為：主倉庫（已跳過 2 項已鎖定的明細）」

#### TC3：進貨量全填（全部被鎖定）
1. 建立一筆進貨單，包含 3 個明細
2. 所有明細都有退貨記錄
3. 點擊「進貨量全填」
4. **預期結果**：
   - 所有明細數量不變
   - 顯示警告：「所有明細都已有退貨或沖款記錄，無法批次填入數量」

#### TC4：清除明細（混合明細）
1. 建立一筆進貨單，包含 4 個明細
2. 其中 1 個明細已有沖款記錄
3. 點擊「清除明細」
4. **預期結果**：
   - 3 個未鎖定明細被刪除
   - 1 個被鎖定明細保留
   - 顯示訊息：「已清除 3 項明細（已保留 1 項已鎖定的明細）」

## 📝 相關檔案

- **主要組件**：`Components/Shared/SubCollections/PurchaseReceivingDetailManagerComponent.razor`
- **相關文件**：
  - `README_刪除限制設計.md`：說明單一明細的刪除限制邏輯
  - `README_PurchaseReceiving_刪除限制增強.md`：進貨單的刪除限制整體設計

## 🔄 版本資訊

- **修改日期**：2025-10-13
- **修改原因**：防止批次操作影響被鎖定（有退貨/沖款記錄）的明細
- **影響範圍**：`PurchaseReceivingDetailManagerComponent` 的 Footer 批次操作按鈕

## 💡 未來可能的改進

1. **視覺標示**：在表格中用顏色或圖示標示被鎖定的明細
2. **批次操作預覽**：在執行批次操作前，顯示對話框預覽哪些明細會被影響
3. **更細緻的權限控制**：根據使用者角色決定是否允許強制操作被鎖定的明細
