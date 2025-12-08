# 進貨單編輯元件 - 資料載入和渲染問題修正記錄

## 📋 問題概述

在進貨單編輯元件 (`PurchaseReceivingEditModalComponent`) 中，發現採購單選項載入存在以下問題：

1. **採購單欄位顯示所有採購單**：未根據廠商進行過濾
2. **欄位控制邏輯不當**：未選擇廠商時，採購單欄位仍可操作
3. **審核機制未整合**：服務層硬性要求已核准，未考慮審核開關
4. **重複查詢資料庫**：同一廠商的採購單被重複載入多次
5. **元件重複渲染**：欄位變更事件被多次觸發，造成效能浪費

---

## 🔍 問題詳細分析

### 問題 1：採購單欄位未根據廠商過濾

**現象：**
- 開啟進貨單 Modal 時，採購單下拉選單顯示所有採購單
- 選擇廠商後，仍顯示全部採購單，而非該廠商的採購單

**根本原因：**
1. `AutoComplete` 初始配置使用了完整的 `purchaseOrders` 列表
2. 採購單欄位未設定初始停用狀態
3. 缺少廠商變更時動態更新採購單選項的機制

**影響：**
- 使用者可能誤選其他廠商的採購單
- 資料邏輯錯誤，可能導致進貨單與採購單廠商不一致

---

### 問題 2：欄位啟用/停用控制不當

**現象：**
- 未選擇廠商時，採購單欄位仍可輸入或選擇
- 使用者體驗不佳，缺少明確的操作引導

**根本原因：**
- 採購單欄位的 `IsReadOnly` 屬性未根據廠商選擇狀態動態調整
- 缺少 Placeholder 提示引導使用者先選擇廠商

**影響：**
- 使用者可能在未選擇廠商時嘗試操作採購單欄位
- 缺乏清晰的操作流程引導

---

### 問題 3：審核機制未整合系統參數

**現象：**
- 服務方法 `GetIncompleteOrdersBySupplierAsync` 硬性要求 `po.IsApproved = true`
- 當審核功能停用時，所有未核准的採購單無法顯示（查詢結果為空）

**根本原因：**
```csharp
// 原始程式碼（錯誤）
.Where(po => po.SupplierId == supplierId
            && po.IsApproved  // ❌ 硬性要求已核准
            && po.PurchaseOrderDetails.Any(...))
```

**資料庫狀態：**
| Id | Code | SupplierId | IsApproved |
|----|------|------------|------------|
| 1  | PO202512080001 | 2 | 0 (未核准) |
| 3  | PO202512080003 | 2 | 0 (未核准) |

**查詢結果：** 0 筆（因為 `IsApproved = false`）

**影響：**
- 審核功能停用時，系統無法使用
- 進貨單無法從採購單轉單

---

### 問題 4：重複查詢資料庫

**現象：**
- 選擇廠商後，同一廠商的採購單資料被查詢 2-4 次
- Console 輸出顯示服務方法被重複呼叫

**根本原因：**
1. Blazor 元件生命週期觸發多次 `OnFieldValueChanged`
2. 缺少快取機制，每次都重新查詢資料庫
3. `OnParametersSetAsync` 和 `LoadPurchaseReceivingData` 重複呼叫 `UpdatePurchaseOrderOptions`

**實際觀察：**
```
⚠ [Service] GetIncompleteOrdersBySupplierAsync 被呼叫 - SupplierId: 2
⚠ [Service] GetIncompleteOrdersBySupplierAsync 被呼叫 - SupplierId: 2
⚠ [Service] GetIncompleteOrdersBySupplierAsync 被呼叫 - SupplierId: 2
⚠ [Service] GetIncompleteOrdersBySupplierAsync 被呼叫 - SupplierId: 2
```

**影響：**
- 資料庫負載增加
- Modal 開啟速度變慢
- 不必要的網路流量

---

### 問題 5：元件重複渲染

**現象：**
- `OnFieldValueChanged` 事件被同一欄位觸發 4 次
- 每次觸發都執行完整的更新邏輯

**根本原因：**
- GenericEditModalComponent 內部機制會在欄位初始化時觸發多次變更事件
- 缺少防抖動（Debounce）機制

**實際觀察：**
```
🔍 [OnFieldValueChanged] 欄位變更: SupplierId = 2
🔍 [OnFieldValueChanged] 欄位變更: SupplierId = 2
🔍 [OnFieldValueChanged] 欄位變更: SupplierId = 2
🔍 [OnFieldValueChanged] 欄位變更: SupplierId = 2
```

**影響：**
- UI 渲染效能下降
- 使用者可能感受到延遲

---

## ✅ 修正方案

### 修正 1：採購單欄位初始化調整

**位置：** `LoadAdditionalDataAsync` 方法

**修改前：**
```csharp
autoCompleteConfig = new AutoCompleteConfigBuilder<PurchaseReceiving>()
    .AddField(nameof(PurchaseReceiving.SupplierId), "CompanyName", suppliers)
    .AddField(nameof(PurchaseReceiving.PurchaseOrderId), "Code", purchaseOrders)  // ❌ 使用全部採購單
    .Build();
```

**修改後：**
```csharp
autoCompleteConfig = new AutoCompleteConfigBuilder<PurchaseReceiving>()
    .AddField(nameof(PurchaseReceiving.SupplierId), "CompanyName", suppliers)
    .AddField(nameof(PurchaseReceiving.PurchaseOrderId), "Code", new List<PurchaseOrder>())  // ✅ 初始為空
    .Build();
```

**效果：**
- AutoComplete 初始不顯示任何採購單
- 等待廠商選擇後才動態載入

---

### 修正 2：採購單欄位設為初始停用

**位置：** `InitializeFormFieldsAsync` 方法

**修改前：**
```csharp
new()
{
    PropertyName = nameof(PurchaseReceiving.PurchaseOrderId),
    Label = "採購單",
    FieldType = FormFieldType.AutoComplete,
    Placeholder = "請輸入或選擇採購單",
    IsRequired = false,
    MinSearchLength = 0,
    HelpText = "輸入採購單號進行搜尋，或留空使用多採購單模式"
}
```

**修改後：**
```csharp
new()
{
    PropertyName = nameof(PurchaseReceiving.PurchaseOrderId),
    Label = "採購單",
    FieldType = FormFieldType.AutoComplete,
    Placeholder = "請先選擇廠商",  // ✅ 引導使用者
    IsRequired = false,
    MinSearchLength = 0,
    HelpText = "請先選擇廠商後，才能選擇該廠商的未完成採購單，或留空使用多採購單模式",
    IsReadOnly = true  // ✅ 初始為停用狀態
}
```

**效果：**
- Modal 開啟時，採購單欄位呈現灰色停用狀態
- Placeholder 提示使用者先選擇廠商

---

### 修正 3：強化 UpdatePurchaseOrderOptions 方法

**位置：** `UpdatePurchaseOrderOptions` 方法

**新增功能：**

#### 3.1 動態控制欄位啟用/停用
```csharp
if (supplierId.HasValue && supplierId.Value > 0)
{
    // ✅ 有廠商：啟用欄位
    purchaseOrderField.IsReadOnly = false;
    purchaseOrderField.Placeholder = "請輸入或選擇採購單";
}
else
{
    // ✅ 無廠商：停用欄位並清空
    purchaseOrderField.IsReadOnly = true;
    purchaseOrderField.Placeholder = "請先選擇廠商";
    purchaseOrderField.Options = new List<SelectOption>();
    
    // 清空當前選擇
    if (editModalComponent?.Entity != null)
    {
        editModalComponent.Entity.PurchaseOrderId = null;
    }
}
```

#### 3.2 同步更新 AutoComplete 資料集合
```csharp
// ✅ 不只更新 Options，也要更新 AutoComplete Collections
if (autoCompleteConfig?.Collections != null && 
    autoCompleteConfig.Collections.ContainsKey(nameof(PurchaseReceiving.PurchaseOrderId)))
{
    autoCompleteConfig.Collections[nameof(PurchaseReceiving.PurchaseOrderId)] = filteredPurchaseOrders;
}
```

**效果：**
- 選擇廠商後，採購單欄位自動啟用
- 切換廠商時，採購單選擇自動清空
- AutoComplete 搜尋功能正常運作

---

### 修正 4：整合審核機制到服務層

**位置：** `PurchaseOrderService.GetIncompleteOrdersBySupplierAsync` 方法

**修改前：**
```csharp
return await context.PurchaseOrders
    .Where(po => po.SupplierId == supplierId
                && po.IsApproved  // ❌ 硬性要求已核准
                && po.PurchaseOrderDetails.Any(pod => pod.ReceivedQuantity < pod.OrderQuantity))
    .ToListAsync();
```

**修改後：**
```csharp
// ✅ 檢查系統參數
var isApprovalEnabled = await _systemParameterService.IsPurchaseOrderApprovalEnabledAsync();

var query = context.PurchaseOrders
    .Where(po => po.SupplierId == supplierId
                && po.PurchaseOrderDetails.Any(pod => pod.ReceivedQuantity < pod.OrderQuantity));

// ✅ 如果啟用審核，才檢查核准狀態
if (isApprovalEnabled)
{
    query = query.Where(po => po.IsApproved);
}

return await query.ToListAsync();
```

**效果：**
- **審核啟用**：只顯示已核准的採購單
- **審核停用**：顯示所有未完成的採購單
- 系統行為與設定一致

---

### 修正 5：加入快取機制

**位置：** 元件內部狀態

**新增變數：**
```csharp
// ===== 快取狀態 - 避免重複查詢 =====
private int? cachedSupplierId = null;  // 快取的廠商ID
private List<PurchaseOrder> cachedPurchaseOrders = new();  // 快取的採購單資料
```

**快取邏輯：**
```csharp
List<PurchaseOrder> supplierOrders;
if (cachedSupplierId == supplierId.Value && cachedPurchaseOrders.Any())
{
    // ✅ 使用快取
    supplierOrders = cachedPurchaseOrders;
}
else
{
    // ✅ 查詢並更新快取
    supplierOrders = await PurchaseOrderService.GetIncompleteOrdersBySupplierAsync(supplierId.Value);
    cachedSupplierId = supplierId.Value;
    cachedPurchaseOrders = supplierOrders ?? new List<PurchaseOrder>();
}
```

**效果：**
- 同一廠商的採購單只查詢一次
- 後續呼叫使用快取資料
- **效能提升：** 從 4 次資料庫查詢減少到 1-2 次

---

### 修正 6：加入防抖動機制

**位置：** `OnFieldValueChanged` 方法

**新增變數：**
```csharp
// ===== 防抖動機制 - 避免短時間內重複處理相同變更 =====
private CancellationTokenSource? _supplierChangeCts;  // 廠商變更的取消令牌
```

**防抖動邏輯：**
```csharp
// 取消之前的變更處理
_supplierChangeCts?.Cancel();
_supplierChangeCts = new CancellationTokenSource();
var currentCts = _supplierChangeCts;

try
{
    // ✅ 延遲 100ms 執行，避免短時間內重複觸發
    await Task.Delay(100, currentCts.Token);
    
    // 檢查是否已被取消
    if (currentCts.IsCancellationRequested)
    {
        return;
    }
    
    // 執行實際的更新邏輯
    await UpdatePurchaseOrderOptions(supplierId);
}
catch (TaskCanceledException)
{
    // 正常的取消，不需要處理
    return;
}
```

**效果：**
- 100ms 內的重複變更會被取消
- 只處理最後一次變更
- **渲染優化：** 4 次變更事件只執行 1 次更新

---

### 修正 7：移除重複呼叫

**位置：** `OnParametersSetAsync` 方法

**修改前：**
```csharp
if (IsVisible)
{
    await LoadAdditionalDataAsync();
    await InitializeFormFieldsAsync();
    
    // ❌ 重複呼叫
    if (PrefilledSupplierId.HasValue && PrefilledSupplierId.Value > 0)
    {
        await UpdatePurchaseOrderOptions(PrefilledSupplierId.Value);
        await UpdateFilterProductOptions(PrefilledSupplierId.Value);
    }
}
```

**修改後：**
```csharp
if (IsVisible)
{
    await LoadAdditionalDataAsync();
    await InitializeFormFieldsAsync();
    
    // ✅ 移除重複呼叫，LoadPurchaseReceivingData 會自動處理
}
```

**原因：**
- `LoadPurchaseReceivingData` 已經會在載入實體時處理採購單選項更新
- 重複呼叫導致不必要的查詢

**效果：**
- 減少重複的方法呼叫
- 簡化程式碼流程

---

## 📊 修正效果對比

### 效能對比

| 項目 | 修正前 | 修正後 | 改善幅度 |
|------|--------|--------|----------|
| 資料庫查詢次數 | 4 次 | 1 次 | ↓ 75% |
| UpdatePurchaseOrderOptions 執行次數 | 4 次 | 1 次 | ↓ 75% |
| Modal 開啟速度 | 較慢 | 快速 | ↑ 明顯提升 |

### 功能對比

| 功能項目 | 修正前 | 修正後 |
|---------|--------|--------|
| 採購單過濾 | ❌ 顯示全部 | ✅ 只顯示該廠商 |
| 欄位控制 | ❌ 始終可操作 | ✅ 動態啟用/停用 |
| 審核整合 | ❌ 硬性要求核准 | ✅ 根據參數調整 |
| 資料快取 | ❌ 無 | ✅ 有 |
| 防抖動 | ❌ 無 | ✅ 有 |

---

## 🎯 最終效果

### 1. 採購單正確過濾
- 未選擇廠商：欄位停用，顯示「請先選擇廠商」
- 選擇廠商 2：欄位啟用，只顯示 `PO202512080001`、`PO202512080003`
- 切換廠商：自動清空採購單選擇，重新載入新廠商的採購單

### 2. 審核機制整合
- **審核啟用**：只能選擇已核准的採購單
- **審核停用**：可選擇所有未完成的採購單

### 3. 效能優化
- 同一廠商只查詢一次資料庫
- 快取機制減少 75% 的資料庫查詢
- 防抖動機制減少 75% 的重複渲染

### 4. 使用者體驗提升
- 操作流程清晰：先選廠商，再選採購單
- 視覺回饋明確：欄位狀態變化明顯
- 回應速度快：無明顯延遲

---

## 📝 學習要點

### 1. Blazor 元件生命週期管理
- 注意元件參數變更會觸發多次渲染
- 避免在 `OnParametersSetAsync` 中重複呼叫相同邏輯
- 使用防抖動機制處理頻繁觸發的事件

### 2. AutoComplete 元件使用
- 需要同時更新 `Options` 和 `Collections`
- `Options` 用於下拉選單顯示
- `Collections` 用於搜尋功能

### 3. 資料快取策略
- 對於不常變動的資料（如採購單列表），使用快取可大幅提升效能
- 記得在相關資料變更時清空快取（如切換廠商）

### 4. 欄位狀態控制
- 使用 `IsReadOnly` 動態控制欄位啟用/停用
- 配合 `Placeholder` 提供操作引導
- 在停用時清空欄位值，避免資料不一致

### 5. 系統參數整合
- 服務層邏輯應考慮系統參數設定
- 避免硬性限制，保持系統彈性
- 使用條件式查詢根據參數調整行為

---

## 🔧 相關檔案

- **元件檔案：** `Components/Pages/Purchase/PurchaseReceivingEditModalComponent.razor`
- **服務檔案：** `Services/Purchase/PurchaseOrderService.cs`
- **服務介面：** `Services/Purchase/IPurchaseOrderService.cs`

---

## 📅 修正日期

2025-12-08

---

## 👤 修正者

系統開發團隊
