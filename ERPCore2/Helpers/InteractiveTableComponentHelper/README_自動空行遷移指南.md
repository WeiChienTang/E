# InteractiveTableComponent 自動空行管理 - 遷移指南

## 📋 文件資訊
- **建立日期**: 2025年1月12日
- **目的**: 統一所有 Table 元件的自動空行管理機制
- **參考文件**: [README_保持一行目前功能配置.md](./README_保持一行目前功能配置.md)

---

## 🎯 遷移目標

將所有使用 `InteractiveTableComponent` 的元件統一遷移到新的自動空行管理機制，確保：
- ✅ 所有空行控制邏輯統一寫在 `InteractiveTableComponent` 中
- ✅ Table 元件只需要配置參數，不再自行實作空行管理
- ✅ 使用內建欄位類型（避免 CustomTemplate）
- ✅ 明確設定觸發欄位（TriggerEmptyRowOnFilled）
- ✅ 使用 DataLoadCompleted 控制載入時機

---

## 📝 遷移檢查清單

### ✅ 已完成遷移的元件（2 個）

#### 1. PurchaseOrderTable.razor ✅
- [x] 移除舊版空行控制方法
- [x] 添加 `DataLoadCompleted` 參數
- [x] 第一個欄位設定 `TriggerEmptyRowOnFilled = true`
- [x] 使用內建 `Select` 類型（非 CustomTemplate）
- [x] 使用 `IsDisabledFunc` 和 `TooltipFunc` 實現條件唯讀
- [x] 載入資料時正確控制 `_dataLoadCompleted` 狀態

**關鍵修改**:
```csharp
// ===== 資料載入狀態控制 =====
private bool _dataLoadCompleted = true;  // 資料載入完成標記

// 商品選擇欄位 - 觸發欄位
columns.Add(new() 
{ 
    Title = "商品", 
    PropertyName = "SelectedProductId",
    EmptyCheckPropertyName = "SelectedProduct",
    TriggerEmptyRowOnFilled = true,  // 🔑 關鍵設定
    ColumnType = InteractiveColumnType.Select,  // 🔑 使用內建類型
    IsDisabledFunc = item => { ... },  // 條件唯讀
    TooltipFunc = item => { ... },     // 動態提示
});

// 載入資料時
private async Task LoadExistingDetailsAsync()
{
    _dataLoadCompleted = false;  // 🔑 開始載入
    // ... 載入資料
    _dataLoadCompleted = true;   // 🔑 觸發空行檢查
    StateHasChanged();
}
```

#### 2. TestTable.razor ✅
- [x] 完整的測試範例
- [x] 使用 `DataLoadCompleted` 參數
- [x] 設定 `TriggerEmptyRowOnFilled = true`
- [x] 使用內建類型

---

### ⚠️ 使用舊版空行管理的元件（6 個 - 需要遷移）

以下元件仍使用 `AutoEmptyRowHelper.EnsureOneEmptyRow()`，需要遷移：

#### 需要遷移
- [ ] `MaterialIssueTable.razor` - 領料單（使用 AutoEmptyRowHelper）
- [ ] `InventoryStockTable.razor` - 庫存明細（使用 AutoEmptyRowHelper）
- [ ] `ProductCompositionTable.razor` - 產品組成（使用 AutoEmptyRowHelper）
- [ ] `ProductSupplierTable.razor` - 產品供應商（使用 AutoEmptyRowHelper）
- [ ] `SetoffPrepaymentTable.razor` - 沖銷預付款（使用 AutoEmptyRowHelper）
- [ ] `QuotationTable.razor` - 報價單（使用舊版 EnsureOneEmptyRow）

---

### 📋 未確認狀態的元件（15 個 - 需要檢查）

#### 採購相關 (3 個)
- [ ] `PurchaseReceivingTable.razor` - 採購進貨
- [ ] `PurchaseReturnTable.razor` - 採購退貨
- [ ] `BatchApprovalTable.razor` - 批量審核

#### 銷售相關 (3 個)
- [ ] `SalesOrderTable.razor` - 銷貨單
- [ ] `SalesDeliveryTable.razor` - 銷貨出貨
- [ ] `SalesReturnTable.razor` - 銷貨退回

#### 產品相關 (1 個)
- [ ] `ProductBarcodePrintTable.razor` - 產品條碼列印

#### 倉庫相關 (2 個)
- [ ] `StockLevelAlertModalComponent.razor` - 庫存水位警示（可能不需要自動空行）
- [ ] `StockAlertViewModalComponent.razor` - 庫存警示檢視（可能不需要自動空行）

#### 沖銷相關 (2 個)
- [ ] `SetoffProductTable.razor` - 沖銷產品
- [ ] `SetoffPaymentTable.razor` - 沖銷付款

#### 供應商相關 (1 個)
- [ ] `SupplierProductTable.razor` - 供應商產品

#### 其他 (1 個)
- [ ] `ShortcutKeysModalComponent.razor` - 快捷鍵說明（可能不需要自動空行）

---

## 🔍 如何識別需要遷移的元件

### 舊版空行管理的特徵

如果元件中出現以下程式碼，表示需要遷移：

#### 1. 使用 `AutoEmptyRowHelper`
```csharp
// ❌ 舊版寫法
private void EnsureOneEmptyRow()
{
    AutoEmptyRowHelper.For<YourItemType>.EnsureOneEmptyRow(
        YourItems,
        () => new YourItemType(),
        item => item.SomeProperty == null
    );
}
```

#### 2. 自訂空行管理方法
```csharp
// ❌ 舊版寫法
private bool IsRowEmpty(YourItemType item)
{
    return item.SomeProperty == null && item.AnotherProperty == 0;
}

private void CheckAndAddEmptyRow()
{
    if (!HasEmptyRow())
    {
        YourItems.Add(new YourItemType());
    }
}
```

#### 3. 在事件中手動呼叫空行檢查
```csharp
// ❌ 舊版寫法
private async Task OnSomeFieldChanged(YourItemType item, object? value)
{
    // ... 處理邏輯
    EnsureOneEmptyRow();  // 手動呼叫
    StateHasChanged();
}
```

### 新版空行管理的特徵

遷移後的元件應該具備以下特徵：

#### 1. 有 `DataLoadCompleted` 參數
```csharp
// ✅ 新版寫法
private bool _dataLoadCompleted = true;

<InteractiveTableComponent DataLoadCompleted="@_dataLoadCompleted" ... />
```

#### 2. 第一個欄位設定 `TriggerEmptyRowOnFilled`
```csharp
// ✅ 新版寫法
columns.Add(new() 
{ 
    Title = "關鍵欄位",
    PropertyName = "YourPropertyId",
    EmptyCheckPropertyName = "YourProperty",
    TriggerEmptyRowOnFilled = true,  // 🔑 關鍵
    ColumnType = InteractiveColumnType.Select  // 使用內建類型
});
```

#### 3. 載入資料時控制 `_dataLoadCompleted`
```csharp
// ✅ 新版寫法
private async Task LoadExistingDetailsAsync()
{
    _dataLoadCompleted = false;
    // ... 載入資料
    _dataLoadCompleted = true;
    StateHasChanged();
}
```

#### 4. 無自訂空行管理方法
```csharp
// ✅ 新版寫法 - 不需要這些方法
// private void EnsureOneEmptyRow() { ... }  ← 已移除
// private bool IsRowEmpty() { ... }          ← 已移除
// private void CheckAndAddEmptyRow() { ... } ← 已移除
```

---

## �🔧 遷移步驟

### 步驟 1: 移除舊版空行控制程式碼

**需要移除的方法/屬性**:
```csharp
// ❌ 移除這些
private void EnsureOneEmptyRow() { ... }
private void CheckAndAddEmptyRow() { ... }
private bool IsRowEmpty(TItem item) { ... }
private bool HasEmptyRow() { ... }
private void AutoAddEmptyRowIfNeeded() { ... }
// ... 以及其他自訂的空行管理方法
```

### 步驟 2: 添加必要參數

```csharp
// ✅ 新增資料載入狀態控制
private bool _dataLoadCompleted = true;  // 資料載入完成標記
```

### 步驟 3: 更新 InteractiveTableComponent 參數

```csharp
<InteractiveTableComponent @ref="tableComponent"
                          TItem="YourItemType" 
                          Items="@YourItems"
                          ColumnDefinitions="@GetColumnDefinitions()"
                          EnableAutoEmptyRow="true"              // 🔑 啟用
                          DataLoadCompleted="@_dataLoadCompleted" // 🔑 新增
                          CreateEmptyItem="@CreateNewEmptyItem"
                          IsReadOnly="@IsReadOnly" />
```

### 步驟 4: 設定第一個欄位為觸發欄位

**必須使用內建欄位類型**：

```csharp
private List<InteractiveColumnDefinition> GetColumnDefinitions()
{
    var columns = new List<InteractiveColumnDefinition>();

    // ✅ 第一個欄位 - 觸發欄位
    columns.Add(new() 
    { 
        Title = "關鍵欄位名稱",
        PropertyName = "YourPropertyId",           // ID 屬性
        EmptyCheckPropertyName = "YourProperty",   // 物件屬性（用於檢查空行）
        TriggerEmptyRowOnFilled = true,            // 🔑 關鍵設定
        ColumnType = InteractiveColumnType.Select, // 🔑 使用內建類型
        // ❌ 不要使用 CustomTemplate
        IsDisabledFunc = item => { ... },          // 條件唯讀
        TooltipFunc = item => { ... },             // 動態提示
        OnSelectionChanged = EventCallback.Factory.Create<(object, object?)>(this, async args =>
        {
            var (item, value) = args;
            await OnYourSelectionChanged((YourItemType)item, value);
        })
    });
    
    return columns;
}
```

### 步驟 5: 更新載入資料方法

```csharp
private async Task LoadExistingDetailsAsync()
{
    if (ExistingDetails?.Any() != true) 
    {
        return;
    }

    // 🔑 開始載入資料 - 設定為未完成
    _dataLoadCompleted = false;
    
    YourItems.Clear();
    
    foreach (var detail in ExistingDetails)
    {
        // ... 載入資料
        YourItems.Add(item);
    }
    
    // 🔑 資料載入完成 - 觸發空行檢查
    _dataLoadCompleted = true;
    StateHasChanged();
}
```

### 步驟 6: 更新資料模型（使用 nullable）

```csharp
public class YourItemType
{
    // ✅ 物件屬性（用於檢查空行）
    public Product? SelectedProduct { get; set; }
    
    // ✅ ID 屬性（用於綁定）
    public int? SelectedProductId { get; set; }
    
    // ✅ 其他屬性使用 nullable
    public int? Quantity { get; set; } = null;
    public decimal? Price { get; set; } = null;
    
    // 非必要欄位可以不用 nullable
    public string Remarks { get; set; } = string.Empty;
}
```

---

## ⚠️ 重要注意事項

### 1. ❌ 不要使用 CustomTemplate 在觸發欄位

```csharp
// ❌ 錯誤：CustomTemplate 會使自動空行失效
new() 
{ 
    TriggerEmptyRowOnFilled = true,
    ColumnType = InteractiveColumnType.Custom,
    CustomTemplate = item => @<select @onchange="...">...</select>
}

// ✅ 正確：使用內建 Select 類型
new() 
{ 
    TriggerEmptyRowOnFilled = true,
    ColumnType = InteractiveColumnType.Select,
    Options = GetYourOptions(),
    OnSelectionChanged = EventCallback.Factory.Create<(object, object?)>(...)
}
```

### 2. ✅ 使用內建類型 + IsDisabledFunc 實現條件唯讀

```csharp
// ✅ 推薦做法
new() 
{ 
    Title = "商品",
    PropertyName = "SelectedProductId",
    ColumnType = InteractiveColumnType.Select,
    IsDisabledFunc = item =>
    {
        var yourItem = (YourItemType)item;
        return yourItem.SomeCondition;  // 動態判斷是否唯讀
    },
    TooltipFunc = item =>
    {
        var yourItem = (YourItemType)item;
        return yourItem.SomeCondition ? "無法修改的原因" : null;
    }
}
```

### 3. ✅ 備註欄位排除空行檢查

```csharp
columns.Add(new() 
{ 
    Title = "備註", 
    PropertyName = "Remarks",
    ColumnType = InteractiveColumnType.Input,
    ExcludeFromEmptyCheck = true,  // 🔑 備註不參與空行檢查
    Tooltip = "選填..."
});
```

### 4. ✅ 唯讀欄位也排除空行檢查

```csharp
columns.Add(new() 
{ 
    Title = "入庫量", 
    PropertyName = "ReceivedQuantity",
    ColumnType = InteractiveColumnType.Number,
    IsReadOnly = true,  // 🔑 唯讀欄位自動排除空行檢查
});
```

---

## 📊 內建欄位類型對照表

| 需求 | ❌ CustomTemplate | ✅ 內建類型 | 說明 |
|-----|------------------|------------|------|
| 下拉選單 | `<select>` | `InteractiveColumnType.Select` | 支援 IsDisabledFunc |
| 可搜尋下拉 | 自訂組件 | `InteractiveColumnType.SearchableSelect` | 完整功能 |
| 文字輸入 | `<input type="text">` | `InteractiveColumnType.Input` | 支援驗證 |
| 數字輸入 | `<input type="number">` | `InteractiveColumnType.Number` | 自動格式化 |
| 複選框 | `<input type="checkbox">` | `InteractiveColumnType.Checkbox` | 支援 switch |
| 日期選擇 | `<input type="date">` | `InteractiveColumnType.Date` | 日期格式化 |
| 按鈕 | `<button>` | `InteractiveColumnType.Button` | 支援 IsDisabledFunc |
| 唯讀顯示 | `<span>` | `InteractiveColumnType.Display` | 支援格式化 |

---

## 🎯 預期效果

遷移完成後，所有 Table 元件將具備：

1. **統一的空行管理** - 所有邏輯在 InteractiveTableComponent 中
2. **一致的使用體驗** - 用戶在所有 Table 中操作方式相同
3. **易於維護** - Table 元件不需要自行實作空行邏輯
4. **精確的觸發時機** - 使用 DataLoadCompleted 明確控制
5. **彈性的配置** - 透過參數即可調整行為

---

## 📚 相關文件

- [README_保持一行目前功能配置.md](./README_保持一行目前功能配置.md) - 完整功能說明
- [README_互動Table說明.md](../../Documentation/README_互動Table說明.md) - InteractiveTableComponent 使用指南
- [InteractiveTableComponent.razor](../../Components/Shared/BaseModal/BaseTableComponent/InteractiveTableComponent.razor) - 組件原始碼
- [PurchaseOrderTable.razor](../../Components/Shared/BaseModal/Modals/Purchase/PurchaseOrderTable.razor) - 參考範例

---

## 📝 遷移進度追蹤

**開始日期**: 2025年1月12日  
**目標**: 統一所有 Table 元件的空行管理機制  
**總計**: 22 個元件

### 📊 當前狀態

| 狀態 | 數量 | 元件 |
|-----|------|------|
| ✅ 已完成 | 2 | PurchaseOrderTable, TestTable |
| ⚠️ 使用舊版 | 6 | MaterialIssueTable, InventoryStockTable, ProductCompositionTable, ProductSupplierTable, SetoffPrepaymentTable, QuotationTable |
| 📋 待檢查 | 13 | 其他元件 |
| 🚫 不需要 | 1 | ShortcutKeysModalComponent（快捷鍵說明） |

### 📅 遷移時間表

| 日期 | 完成元件 | 備註 |
|-----|---------|------|
| 2025-01-12 | PurchaseOrderTable.razor | ✅ 首個範例，已完成測試 |
| 2025-01-12 | TestTable.razor | ✅ 測試用範例 |
| TBD | 其他 6 個使用舊版的元件 | ⚠️ 優先處理 |
| TBD | 其他 13 個待檢查元件 | 📋 逐步檢查 |
