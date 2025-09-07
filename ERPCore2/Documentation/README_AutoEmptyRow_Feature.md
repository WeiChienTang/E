# 自動空行功能 (AutoEmptyRow Feature) 詳細說明

## 📋 功能概述

自動空行功能是為了提升使用者體驗而設計的智能表格功能，確保在使用 `InteractiveTableComponent` 的表格中始終保持一行空行供使用者輸入新資料，類似於 Excel 的操作體驗。

## 🎯 設計目標

- **類似 Excel 的體驗**：使用者不需要點擊「新增」按鈕，直接在空行開始輸入即可
- **智能偵測**：根據業務邏輯自動判斷何時需要新增空行
- **統一實作**：提供可重用的 Helper 類別，確保所有使用此功能的組件行為一致
- **效能優化**：避免不必要的空行重複新增
- **業務邏輯彈性**：支援不同組件定義不同的「空行」判定條件

## 🔧 技術架構

### 核心組件

#### 1. AutoEmptyRowHelper
位置：`ERPCore2/Helpers/AutoEmptyRowHelper.cs`

提供統一的自動空行管理邏輯，包含：
- 空行檢查
- 空行新增
- 輸入變更處理
- 項目移除處理
- 驗證輔助方法

#### 2. 組件整合方式
每個使用自動空行功能的組件需要實作：
- `IsEmptyRow()` 方法：定義業務邏輯的空行判定條件
- `CreateEmptyItem()` 方法：創建新的空項目
- `EnsureOneEmptyRow()` 方法：確保有空行的封裝方法
- 在各種輸入事件中調用 Helper 方法

## 📖 實作細節

### 業務邏輯設計

#### 空行判定原則
以 `ProductSupplierManagerComponent` 為例：
```csharp
private bool IsEmptyRow(TProductSupplierEntity item)
{
    var supplierId = GetSupplierId(item);
    // 檢查廠商ID是否為空（null 或 0 都算空）
    var isEmpty = !supplierId.HasValue || supplierId.Value <= 0;
    
    // 只要廠商有值（大於0），就不是空行
    // 廠商是核心欄位，其他欄位可以後續填寫
    return isEmpty;
}
```

**關鍵設計決策**：
- **核心欄位導向**：以最重要的業務欄位（如廠商）作為空行判定基準
- **寬鬆條件**：不要求所有欄位都填寫才算非空行
- **0 值處理**：考慮到 int 型別的預設值為 0，同時檢查 null 和 0

#### 觸發時機
```csharp
private async Task OnSupplierChanged((object item, object? value) args)
{
    var productSupplier = (TProductSupplierEntity)args.item;
    var wasEmpty = IsEmptyRow(productSupplier);  // 記錄變更前狀態
    
    // 執行業務邏輯設定值
    SetSupplierId(productSupplier, supplierId);
    
    // 使用 Helper 檢查是否需要新增空行
    AutoEmptyRowHelper.For<TProductSupplierEntity>.HandleInputChangeAdvanced(
        Items, productSupplier, IsEmptyRow, CreateEmptyItem, wasEmpty, SetParentId, ParentEntityId);
}
```

### 演算法流程

#### 1. 初始化階段
```
OnParametersSet() → EnsureOneEmptyRow() → 檢查空行數量 → 不足時新增
```

#### 2. 使用者輸入階段
```
使用者輸入 → 記錄 wasEmpty → 執行業務邏輯 → HandleInputChangeAdvanced()
    ↓
檢查條件：wasEmpty && !isEmptyNow && !hasOtherEmptyRows
    ↓
條件滿足 → 新增空行
```

#### 3. 刪除項目階段
```
刪除項目 → HandleItemRemove() → EnsureOneEmptyRow()
```

### 關鍵技術解決方案

#### 問題 1：int vs int? 的空值判定
**問題**：新建立的 Entity 中 int 型別屬性預設值為 0，但委派返回 int?，導致空行判定錯誤。

**解決方案**：
```csharp
// 同時檢查 null 和 0
var isEmpty = !supplierId.HasValue || supplierId.Value <= 0;
```

#### 問題 2：OnParametersSet 重複呼叫
**問題**：Blazor 組件生命週期會多次觸發 OnParametersSet，導致重複新增空行。

**解決方案**：
```csharp
// 直接呼叫 EnsureOneEmptyRow，其內部有防重複邏輯
protected override void OnParametersSet()
{
    base.OnParametersSet();
    EnsureOneEmptyRow(); // Helper 內部會檢查是否真的需要新增
}
```

#### 問題 3：防止無限新增空行
**解決方案**：
```csharp
public static bool HandleInputChangeAdvanced(...)
{
    // 只有在原本是空行，現在變成非空行，且沒有其他空行時才新增
    if (wasEmpty && !isEmptyNow)
    {
        var otherEmptyItems = items.Where(item => 
            !ReferenceEquals(item, currentItem) && isEmptyChecker(item)).ToList();
        
        if (!otherEmptyItems.Any())
        {
            // 新增空行
        }
    }
}
```

## 🧪 測試場景

### 場景 1：空白開始
```
1. 初始：[]
2. OnParametersSet: [空行1]
3. 使用者選擇廠商: [有廠商1, 空行2]
4. 繼續填寫其他欄位: [有廠商1, 空行2] (不重複新增)
```

### 場景 2：已有資料
```
1. 初始：[資料1, 資料2]
2. OnParametersSet: [資料1, 資料2, 空行3]
3. 使用者在空行3輸入: [資料1, 資料2, 有廠商3, 空行4]
```

### 場景 3：刪除項目
```
1. 當前：[資料1, 資料2, 空行3]
2. 刪除資料1: [資料2, 空行3]
3. 刪除空行3: [資料2, 空行4] (自動補充)
```

## 📊 效能考量

### 優化策略
1. **最小化檢查頻率**：只在真正的輸入變更時觸發檢查
2. **引用比較**：使用 `ReferenceEquals` 避免不必要的項目比較
3. **早期退出**：在條件不滿足時立即返回
4. **防止連鎖反應**：避免 StateHasChanged 導致的無限循環

### 記憶體影響
- 每個表格最多只會多一個空行項目
- 使用弱引用避免記憶體洩漏
- 適時清理不需要的空行

## 🔧 實作指南

### 新增自動空行功能到現有組件

#### 步驟 1：引入 Helper
```csharp
@using ERPCore2.Helpers
```

#### 步驟 2：實作必要方法
```csharp
private bool IsEmptyRow(TEntity item)
{
    // 根據業務邏輯定義空行條件
    // 建議以核心欄位為準
}

private TEntity CreateEmptyItem()
{
    var newItem = new TEntity();
    // 設定必要的預設值，如 ParentId
    return newItem;
}

private void EnsureOneEmptyRow()
{
    AutoEmptyRowHelper.For<TEntity>.EnsureOneEmptyRow(
        Items, IsEmptyRow, CreateEmptyItem, SetParentId, ParentEntityId);
}
```

#### 步驟 3：整合到生命週期
```csharp
protected override void OnParametersSet()
{
    base.OnParametersSet();
    EnsureOneEmptyRow();
}
```

#### 步驟 4：在輸入事件中調用
```csharp
private async Task OnFieldChanged((object item, object? value) args)
{
    var entity = (TEntity)args.item;
    var wasEmpty = IsEmptyRow(entity);
    
    // 執行業務邏輯
    SetFieldValue(entity, args.value);
    
    // 檢查是否需要新增空行
    AutoEmptyRowHelper.For<TEntity>.HandleInputChangeAdvanced(
        Items, entity, IsEmptyRow, CreateEmptyItem, wasEmpty, SetParentId, ParentEntityId);
    
    await ItemsChanged.InvokeAsync(Items);
    StateHasChanged();
}
```

## 🐛 常見問題與解決方案

### Q1: 空行一直被當作有資料
**原因**：空行判定邏輯太嚴格，或者 Entity 預設值設定問題
**解決**：檢查 IsEmptyRow 邏輯，確認是否正確處理 0 值和 null 值

### Q2: 一直新增空行停不下來
**原因**：OnParametersSet 被重複觸發，或者輸入事件處理有問題
**解決**：檢查是否有無限循環的 StateHasChanged 呼叫

### Q3: 已有資料時不會新增空行
**原因**：OnParametersSet 的條件判斷過於保守
**解決**：移除條件限制，直接呼叫 EnsureOneEmptyRow()

### Q4: 刪除空行後沒有自動補充
**原因**：刪除邏輯沒有調用 Helper 的 HandleItemRemove
**解決**：使用 Helper 的移除方法而非直接 List.Remove

## 📈 未來擴展方向

### 計畫功能
1. **多空行支援**：支援始終保持 N 行空行
2. **條件式空行**：根據不同狀態決定是否需要空行
3. **批次操作優化**：批次新增/刪除時的效能優化
4. **視覺提示**：為空行提供特殊的視覺樣式
5. **鍵盤導航**：支援 Tab/Enter 鍵在空行間導航

### 擴展性設計
Helper 設計為泛型和委派模式，便於：
- 支援不同的 Entity 類型
- 適應不同的業務邏輯
- 輕鬆擴展新功能
- 保持向後相容性

## 📝 總結

自動空行功能大幅提升了表格輸入的使用者體驗，通過統一的 Helper 類別確保了實作的一致性和可維護性。這個功能特別適合需要頻繁新增資料的場景，如產品廠商管理、聯絡人管理等。

關鍵成功因素：
- **業務邏輯導向**的空行判定
- **效能優化**的檢查機制
- **統一的實作模式**
- **充分的測試覆蓋**

透過這個功能，使用者可以享受到類似 Excel 的流暢輸入體驗，同時開發者也能保持程式碼的簡潔和一致性。
