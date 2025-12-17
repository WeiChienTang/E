# InteractiveTableComponent 自動空行管理機制說明

## 📋 目錄
- [功能概述](#功能概述)
- [核心參數配置](#核心參數配置)
- [空行判斷邏輯](#空行判斷邏輯)
- [自動新增空行的觸發時機](#自動新增空行的觸發時機)
- [實際應用範例](#實際應用範例)
- [注意事項與最佳實踐](#注意事項與最佳實踐)

---

## 功能概述

`InteractiveTableComponent` 提供了自動空行管理機制，確保表格**隨時保持至少一個可輸入的空行**，提升用戶輸入體驗。

### 核心特性
✅ 初始化時自動新增一個空行  
✅ 用戶填寫資料後自動新增新的空行  
✅ 支援指定「觸發欄位」，只有關鍵欄位有值才新增空行  
✅ 刪除項目後自動補充空行  
✅ 靈活的空行判斷邏輯（觸發欄位模式 vs 傳統模式）

---

## 核心參數配置

### 1. InteractiveTableComponent 參數

```razor
<InteractiveTableComponent @ref="tableComponent"
                          TItem="ProductItem" 
                          Items="@ProductItems"
                          ColumnDefinitions="@GetColumnDefinitions()"
                          EnableAutoEmptyRow="true"              <!-- 🔑 啟用自動空行管理 -->
                          DataLoadCompleted="@_dataLoadCompleted" <!-- 🔑 資料載入完成標記 -->
                          CreateEmptyItem="@CreateNewEmptyItem"  <!-- 🔑 空項目建立方法 -->
                          IsReadOnly="@IsReadOnly"
                          ShowRowNumbers="true" />
```

| 參數 | 類型 | 說明 | 必要性 |
|-----|------|------|--------|
| `EnableAutoEmptyRow` | `bool` | 是否啟用自動空行管理 | ✅ 必要 |
| `DataLoadCompleted` | `bool` | 資料是否已載入完成（預設 true） | ⚠️ 建議（大量資料時必要） |
| `CreateEmptyItem` | `Func<TItem>` | 建立空項目的工廠方法 | ✅ 必要 |
| `Items` | `List<TItem>` | 資料集合（必須是 List） | ✅ 必要 |

### 2. 建立空項目方法

```csharp
private ProductItem CreateNewEmptyItem()
{
    var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
    Console.WriteLine($"[{timestamp}] CreateNewEmptyItem 被呼叫 | 當前數量: {ProductItems.Count}");
    
    return new ProductItem();  // 🔑 返回新的空物件，所有屬性都是預設值
}
```

**重點**:
- 返回一個新的物件實例
- 所有屬性應該是預設值（`null`、空字串、`0` 等）
- 建議使用 **nullable 類型**（如 `int?`、`decimal?`），避免數字 `0` 被誤判為有值

### 3. 資料模型建議

```csharp
public class ProductItem
{
    public string ProductName { get; set; } = string.Empty;  // 字串預設空字串
    public string? Category { get; set; } = null;            // 🔑 nullable，預設 null
    public int? Quantity { get; set; } = null;               // 🔑 nullable，預設 null
    public decimal? Price { get; set; } = null;              // 🔑 nullable，預設 null
}
```

**為什麼使用 nullable?**
- `int Quantity = 0` → 被視為「有值」（數字 0）
- `int? Quantity = null` → 被視為「無值」（真正的空）

---

## 空行判斷邏輯

`InteractiveTableComponent` 使用 `IsRowEmpty(TItem item)` 方法判斷一行是否為空，支援兩種模式：

### 模式 A: 觸發欄位模式（優先）

當有欄位設定 `TriggerEmptyRowOnFilled = true` 時啟用。

```csharp
// 邏輯：所有觸發欄位都必須有值，才算「非空行」
var triggerFields = ColumnDefinitions
    .Where(c => c.TriggerEmptyRowOnFilled)
    .ToList();

if (triggerFields.Any())
{
    foreach (var field in triggerFields)
    {
        var value = GetPropertyValue(item, field.PropertyName);
        if (IsValueNullOrEmpty(value))
            return true;  // ❌ 只要有一個觸發欄位是空的，整行就是空行
    }
    return false;  // ✅ 所有觸發欄位都有值，不是空行
}
```

**適用場景**: 
- 有明確的「關鍵欄位」（如商品名稱、客戶名稱）
- 只有關鍵欄位有值才算有效資料
- 其他欄位（如數量、備註）可以為空

### 模式 B: 傳統模式（無觸發欄位時）

檢查所有可編輯且未排除的欄位。

```csharp
var columnsToCheck = ColumnDefinitions
    .Where(c => !c.IsReadOnly && !c.ExcludeFromEmptyCheck)
    .ToList();

foreach (var column in columnsToCheck)
{
    var value = GetPropertyValue(item, column.PropertyName);
    if (!IsValueNullOrEmpty(value))
        return false;  // ✅ 只要有一個欄位有值，就不是空行
}
return true;  // ❌ 所有欄位都空才是空行
```

**適用場景**:
- 沒有明確的關鍵欄位
- 任何欄位有值都算有效資料

### 值的判斷規則 (`IsValueNullOrEmpty`)

```csharp
private bool IsValueNullOrEmpty(object? value)
{
    if (value == null) return true;                          // null → 空
    if (value is string str) return string.IsNullOrWhiteSpace(str);  // "" → 空
    
    // 其他類型：不是 null 就算有值
    // 數字 0、false 等都算有值（因為使用 nullable 類型）
    return false;
}
```

| 值類型 | 範例值 | 判斷結果 |
|-------|--------|---------|
| `null` | `null` | ❌ 空 |
| `string` | `""` 或 `"   "` | ❌ 空 |
| `string` | `"abc"` | ✅ 有值 |
| `int?` | `null` | ❌ 空 |
| `int?` | `0` | ✅ 有值 |
| `bool?` | `null` | ❌ 空 |
| `bool?` | `false` | ✅ 有值 |

---

## 自動新增空行的觸發時機

### 時機 1: 資料載入完成時（推薦用於編輯模式）

```csharp
// 私有欄位
private bool _dataLoadCompleted = true;  // 資料載入完成標記（預設 true 保持向下兼容）

// 載入資料時
private async Task LoadExistingDetailsAsync()
{
    if (ExistingDetails?.Any() != true) return;

    // 🔑 開始載入資料 - 設定為未完成
    _dataLoadCompleted = false;
    
    ProductItems.Clear();
    
    foreach (var detail in ExistingDetails)
    {
        // ... 載入資料到 ProductItems
        ProductItems.Add(item);
    }
    
    // 🔑 資料載入完成 - 觸發空行檢查
    _dataLoadCompleted = true;
    StateHasChanged();
}
```

**InteractiveTableComponent 內部邏輯**:
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    // 🔑 監控 DataLoadCompleted 狀態變化
    if (EnableAutoEmptyRow && DataLoadCompleted && !_previousDataLoadCompleted)
    {
        // DataLoadCompleted 從 false 變成 true,表示資料剛載入完成
        _previousDataLoadCompleted = DataLoadCompleted;
        
        await InvokeAsync(() =>
        {
            CheckAndAddEmptyRowIfNeeded();  // ✅ 確保空行在最後
            StateHasChanged();
        });
    }
}
```

**工作流程**:
```
父組件載入流程:
┌────────────────────────────────┐
│ LoadExistingDetailsAsync()     │
│ _dataLoadCompleted = false ←───┼─ 告知開始載入
├────────────────────────────────┤
│ ProductItems.Clear()           │
│ foreach (var detail in ...)    │
│   Items.Add(商品1)              │
│   Items.Add(商品2)              │
│   Items.Add(商品3)              │
│   ... 大量資料 ...              │
├────────────────────────────────┤
│ _dataLoadCompleted = true  ←───┼─ ✅ 觸發空行檢查
│ StateHasChanged()              │
└────────────────────────────────┘
         ↓
InteractiveTableComponent 偵測
false → true 狀態變化
         ↓
OnAfterRenderAsync 觸發
         ↓
CheckAndAddEmptyRowIfNeeded()
         ↓
空行加在最後 ✅
```

**優勢**:
- ✅ **精確控制** - 明確告知何時完成載入
- ✅ **無延遲** - 不依賴 `Task.Delay`
- ✅ **支援大量資料** - 無論資料量多大都準確
- ✅ **支援重複載入** - 可以重複觸發 `false → true`

### 時機 2: 初始化時（用於新增模式）

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    // 🔑 向下兼容:如果父組件沒有控制 DataLoadCompleted(預設 true),使用原有邏輯
    if (firstRender && EnableAutoEmptyRow && DataLoadCompleted && !_hasInitializedEmptyRow)
    {
        _hasInitializedEmptyRow = true;
        _previousDataLoadCompleted = DataLoadCompleted;
        
        await InvokeAsync(() =>
        {
            CheckAndAddEmptyRowIfNeeded();  // 確保至少有一個空行
            StateHasChanged();
        });
    }
}
```

### 時機 3: 輸入變更時（優先級：欄位級觸發）

```csharp
private async Task HandleInputChange(InteractiveColumnDefinition column, TItem item, string? value)
{
    var wasEmpty = IsRowEmpty(item);  // 記錄變更前狀態
    SetPropertyValue(item, column.PropertyName, value);
    
    // 🔑 優先檢查：欄位級別觸發
    if (column.TriggerEmptyRowOnFilled)
    {
        var fieldWasEmpty = IsValueNullOrEmpty(舊值);
        var fieldHasValueNow = !IsValueNullOrEmpty(新值);
        
        // 條件: 整行原本空 && 欄位原本空 && 欄位現在有值
        if (wasEmpty && fieldWasEmpty && fieldHasValueNow)
        {
            AutoAddEmptyRowIfNeeded();  // ✅ 立即新增空行
            return;
        }
    }
    
    // 🔑 次要檢查：整行級別觸發
    var isEmptyNow = IsRowEmpty(item);
    if (wasEmpty && !isEmptyNow)  // 整行從空變非空
    {
        AutoAddEmptyRowIfNeeded();  // ✅ 新增空行
    }
}
```

**觸發條件彙整**:

| 模式 | 觸發條件 | 優先級 |
|-----|---------|--------|
| 欄位級觸發 | 整行原本空 && 觸發欄位原本空 && 觸發欄位現在有值 | 🥇 高 |
| 整行級觸發 | 整行從空變非空（任何欄位有值） | 🥈 中 |

### 時機 4: 選擇變更時（下拉選單）
| 欄位級觸發 | 整行原本空 && 觸發欄位原本空 && 觸發欄位現在有值 | 🥇 高 |
| 整行級觸發 | 整行從空變非空（任何欄位有值） | 🥈 中 |

### 時機 4: 選擇變更時（下拉選單）

```csharp
private async Task HandleSelectionChange(InteractiveColumnDefinition column, TItem item, object? value)
{
    // 邏輯與 HandleInputChange 相同
    // 支援下拉選單、SearchableSelect 等控件
}
```

### 時機 5: 刪除項目後

```csharp
private async Task HandleBuiltInDelete(TItem item)
{
    if (OnItemDelete.HasDelegate)
    {
        await OnItemDelete.InvokeAsync(item);
        
        // 🔑 自動補充空行
        EnsureOneEmptyRow();
    }
}
```

### 核心檢查方法

```csharp
private void CheckAndAddEmptyRowIfNeeded()
{
    if (!EnableAutoEmptyRow || CreateEmptyItem == null || Items == null) 
        return;
    
    // 🔑 找出所有空行
    var emptyRows = Items.Where(IsRowEmpty).ToList();
    
    if (emptyRows.Count == 0)
    {
        // 沒有空行,新增一個在最後
        var newEmptyRow = CreateEmptyItem();
        Items.Add(newEmptyRow);
        _lastEmptyRow = newEmptyRow;
    }
    else if (emptyRows.Count > 1 || !Equals(Items.Last(), emptyRows[0]))
    {
        // 🔑 有多個空行,或空行不在最後 → 移除所有空行,只保留一個在最後
        foreach (var emptyRow in emptyRows)
        {
            Items.Remove(emptyRow);
        }
        
        var newEmptyRow = CreateEmptyItem();
        Items.Add(newEmptyRow);
        _lastEmptyRow = newEmptyRow;
    }
    else
    {
        // 只有一個空行且在最後,不需要處理
        _lastEmptyRow = emptyRows[0];
    }
}

private bool HasEmptyRow()
{
    return Items?.Any(IsRowEmpty) ?? false;
}
```

**重點說明**:
- ✅ 自動移除多餘的空行
- ✅ 確保空行永遠在最後
- ✅ 避免空行出現在中間位置

---

## 實際應用範例

### 範例 1: TestTable.razor（觸發欄位模式 - 使用內建類型 + DataLoadCompleted）

```csharp
<InteractiveTableComponent @ref="tableComponent"
                          TItem="ProductItem" 
                          Items="@ProductItems"
                          ColumnDefinitions="@GetColumnDefinitions()"
                          EnableAutoEmptyRow="true"
                          DataLoadCompleted="@_dataLoadCompleted"
                          CreateEmptyItem="@CreateNewEmptyItem" />

@code {
    private List<ProductItem> ProductItems { get; set; } = new();
    private bool _dataLoadCompleted = true;  // 資料載入完成標記
    
    private ProductItem CreateNewEmptyItem()
    {
        return new ProductItem();
    }
    
    // 載入資料時
    private async Task LoadDataAsync()
    {
        // 🔑 開始載入
        _dataLoadCompleted = false;
        
        ProductItems.Clear();
        
        // 載入大量資料...
        foreach (var item in await GetDataFromDatabase())
        {
            ProductItems.Add(item);
        }
        
        // 🔑 載入完成
        _dataLoadCompleted = true;
        StateHasChanged();
    }
    
    private List<InteractiveColumnDefinition> GetColumnDefinitions()
    {
        return new List<InteractiveColumnDefinition>
        {
            new()
            {
                Title = "商品名稱",
                PropertyName = "ProductName",
                ColumnType = InteractiveColumnType.Input,  // ✅ 內建 Input 類型
                TriggerEmptyRowOnFilled = true,  // 🔑 關鍵欄位：填入值後自動新增空行
                Tooltip = "商品名稱是關鍵欄位，填入後會自動新增下一行"
            },
            new()
            {
                Title = "類別",
                PropertyName = "Category",
                ColumnType = InteractiveColumnType.Select,  // ✅ 內建 Select 類型
                TriggerEmptyRowOnFilled = true,  // 🔑 也是觸發欄位
                Options = new List<InteractiveSelectOption>
                {
                    new() { Value = "", Text = "請選擇" },
                    new() { Value = "A", Text = "類別 A" },
                    new() { Value = "B", Text = "類別 B" }
                }
            },
            new()
            {
                Title = "數量",
                PropertyName = "Quantity",
                ColumnType = InteractiveColumnType.Number,  // ✅ 內建 Number 類型
                // 🔑 沒有設定 TriggerEmptyRowOnFilled，填值不會觸發新增空行
            },
            new()
            {
                Title = "單價",
                PropertyName = "Price",
                ColumnType = InteractiveColumnType.Number  // ✅ 內建 Number 類型
            }
        };
    }
    
    public class ProductItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; } = null;  // nullable
        public int? Quantity { get; set; } = null;     // nullable
        public decimal? Price { get; set; } = null;    // nullable
    }
}
```

**運作流程**:
1. 初始化時 `_dataLoadCompleted = true`，自動新增一個空行
2. 編輯模式載入資料時：
   - 設定 `_dataLoadCompleted = false`
   - 載入所有資料
   - 設定 `_dataLoadCompleted = true` → ✅ 觸發空行檢查，確保空行在最後
3. 用戶在「商品名稱」輸入 "鉛筆" → ✅ 立即新增新空行（觸發欄位）
4. 用戶在「類別」選擇 "A" → ✅ 如果是空行，立即新增新空行（觸發欄位）
5. 用戶在「數量」輸入 10 → ❌ 不會新增空行（非觸發欄位）

### 範例 2: 條件唯讀欄位（使用內建類型 + IsDisabledFunc + DataLoadCompleted）

```csharp
@code {
    private bool _dataLoadCompleted = true;  // 資料載入完成標記
    
    // 載入現有明細
    private async Task LoadExistingDetailsAsync()
    {
        if (ExistingDetails?.Any() != true) return;

        // 🔑 開始載入資料
        _dataLoadCompleted = false;
        
        ProductItems.Clear();
        
        foreach (var detail in ExistingDetails)
        {
            // ... 載入資料
            ProductItems.Add(item);
        }
        
        // 🔑 資料載入完成
        _dataLoadCompleted = true;
        StateHasChanged();
    }
    
    private List<InteractiveColumnDefinition> GetColumnDefinitions()
    {
        return new List<InteractiveColumnDefinition>
        {
            new()
            {
                Title = "商品",
                PropertyName = "SelectedProductId",
                EmptyCheckPropertyName = "SelectedProduct",  // 檢查物件屬性
                ColumnType = InteractiveColumnType.Select,  // ✅ 使用內建 Select
                TriggerEmptyRowOnFilled = true,  // ✅ 自動空行功能有效
                Width = "150px",
                Options = GetProductOptions(),
                IsDisabledFunc = item =>  // 🔑 條件唯讀：已入庫則鎖定
                {
                    var productItem = (ProductItem)item;
                    var hasReceiving = productItem.ReceivedQuantity > 0;
                    return IsReadOnly || hasReceiving;
                },
                TooltipFunc = item =>  // 🔑 動態提示訊息
                {
                    var productItem = (ProductItem)item;
                    var hasReceiving = productItem.ReceivedQuantity > 0;
                    return hasReceiving ? "此商品已有進貨記錄，無法修改商品選擇" : null;
                },
                OnSelectionChanged = EventCallback.Factory.Create<(object, object?)>(this, 
                    async args =>
                    {
                        var (item, value) = args;
                        await OnProductSelectionChanged((ProductItem)item, value);
                    })
            },
            new()
            {
                Title = "數量",
                PropertyName = "Quantity",
                ColumnType = InteractiveColumnType.Number,  // ✅ 使用內建 Number
                Width = "120px",
                IsDisabledFunc = item =>  // 🔑 條件唯讀
                {
                    var productItem = (ProductItem)item;
                    return productItem.ReceivedQuantity > 0;
                },
                TooltipFunc = item =>
                {
                    var productItem = (ProductItem)item;
                    return productItem.ReceivedQuantity > 0 
                        ? "此商品已有進貨記錄，無法修改數量" : null;
                }
            }
        };
    }

    // 🔑 資料模型需要同時有 ID 和物件屬性
    public class ProductItem
    {
        public Product? SelectedProduct { get; set; }       // 用於檢查空行
        public int? SelectedProductId { get; set; }         // 用於 Select 綁定
        public int Quantity { get; set; } = 0;
        public int ReceivedQuantity { get; set; } = 0;      // 用於判斷是否鎖定
    }
}
```

**重點說明**：
- ✅ 使用 `DataLoadCompleted` 控制載入時機，確保空行在最後
- ✅ 使用內建類型（`Select`、`Number`）自動空行功能正常運作
- ✅ 使用 `IsDisabledFunc` 實現條件唯讀，無需 CustomTemplate
- ✅ 使用 `TooltipFunc` 實現動態提示訊息
- ✅ `EmptyCheckPropertyName` 可以指定檢查物件屬性（`SelectedProduct`），而 `PropertyName` 綁定 ID（`SelectedProductId`）

### 範例 3: 傳統模式（無觸發欄位）

```csharp
private List<InteractiveColumnDefinition> GetColumnDefinitions()
{
    return new List<InteractiveColumnDefinition>
    {
        new()
        {
            Title = "項目名稱",
            PropertyName = "ItemName",
            ColumnType = InteractiveColumnType.Input  // ✅ 使用內建類型
            // 🔑 沒有設定 TriggerEmptyRowOnFilled
        },
        new()
        {
            Title = "金額",
            PropertyName = "Amount",
            ColumnType = InteractiveColumnType.Number  // ✅ 使用內建類型
        }
    };
}
```

**運作流程**:
1. 初始化時自動新增一個空行
2. 用戶在任何欄位填入值 → ✅ 整行變非空，自動新增新空行

---

## 注意事項與最佳實踐

### ✅ 建議做法

1. **使用 nullable 類型**
   ```csharp
   // ✅ 好
   public int? Quantity { get; set; } = null;
   public decimal? Price { get; set; } = null;
   
   // ❌ 不好（數字 0 會被視為有值）
   public int Quantity { get; set; } = 0;
   public decimal Price { get; set; } = 0;
   ```

2. **明確設定觸發欄位**
   ```csharp
   // ✅ 好：只有關鍵欄位觸發
   new() { 
       PropertyName = "ProductName", 
       ColumnType = InteractiveColumnType.Input,  // 使用內建類型
       TriggerEmptyRowOnFilled = true  // 商品名稱是關鍵
   }
   
   new() { 
       PropertyName = "Quantity",
       ColumnType = InteractiveColumnType.Number  // 使用內建類型
       // 數量可以為空，不觸發
   }
   
   // ❌ 不好：使用 CustomTemplate 會失效
   new() { 
       PropertyName = "", 
       ColumnType = InteractiveColumnType.Custom,  // ⚠️ 自訂類型
       TriggerEmptyRowOnFilled = true,  // ❌ 無效！
       CustomTemplate = item => @<input @oninput="..." />
   }
   ```

3. **Items 必須使用 List**
   ```csharp
   // ✅ 好
   private List<ProductItem> ProductItems { get; set; } = new();
   
   // ❌ 不好（IEnumerable 無法自動新增）
   private IEnumerable<ProductItem> ProductItems { get; set; }
   ```

4. **提供工具提示**
   ```csharp
   new()
   {
       Title = "商品名稱",
       PropertyName = "ProductName",
       ColumnType = InteractiveColumnType.Input,  // 使用內建類型
       TriggerEmptyRowOnFilled = true,
       Tooltip = "商品名稱是關鍵欄位，填入後會自動新增下一行"  // 🔑 提示用戶
   }
   ```

5. **優先使用內建欄位類型，避免使用 CustomTemplate**
   ```csharp
   // ✅ 好：使用內建類型，自動空行功能正常
   new() 
   { 
       Title = "商品",
       PropertyName = "SelectedProductId",
       ColumnType = InteractiveColumnType.Select,  // 內建 Select
       TriggerEmptyRowOnFilled = true,  // ✅ 有效
       Options = GetProductOptions(),
       IsDisabledFunc = item => ...,  // 條件唯讀
       TooltipFunc = item => ...       // 動態提示
   }
   
   // ❌ 不好：使用 CustomTemplate，自動空行功能失效
   new() 
   { 
       Title = "商品",
       PropertyName = "",
       ColumnType = InteractiveColumnType.Custom,  // 自訂類型
       TriggerEmptyRowOnFilled = true,  // ❌ 無效
       CustomTemplate = item => @<select @onchange="...">...</select>
   }
   ```

6. **內建類型涵蓋大多數需求**
   
   InteractiveTableComponent 提供的內建類型：
   - `InteractiveColumnType.Input` - 文字輸入
   - `InteractiveColumnType.Number` - 數字輸入
   - `InteractiveColumnType.Select` - 下拉選單
   - `InteractiveColumnType.Checkbox` - 複選框
   - `InteractiveColumnType.SearchableSelect` - 可搜尋下拉選單
   - `InteractiveColumnType.Date` - 日期選擇
   - `InteractiveColumnType.Button` - 按鈕
   - `InteractiveColumnType.Display` - 唯讀顯示
   
   配合 `IsDisabledFunc`、`TooltipFunc`、`DisplayFormatter` 等功能，幾乎可以滿足所有需求。

### ⚠️ 常見問題

1. **空行一直重複新增**
   - 原因：`CreateEmptyItem` 返回的物件屬性有預設值（如數字 `0`）
   - 解決：使用 nullable 類型，預設為 `null`

2. **空行一直重複新增**
   - 原因：`CreateEmptyItem` 返回的物件屬性有預設值（如數字 `0`）
   - 解決：使用 nullable 類型，預設為 `null`

3. **編輯模式下空行出現在中間位置**
   - 原因：資料載入過程中就觸發了空行檢查
   - 解決：使用 `DataLoadCompleted` 參數明確控制載入時機
   ```csharp
   // 🔑 設定載入狀態
   _dataLoadCompleted = false;
   // 載入資料...
   _dataLoadCompleted = true;  // ✅ 觸發空行檢查
   StateHasChanged();
   ```

4. **刪除後沒有空行**
   - 原因：刪除事件中沒有呼叫 `EnsureOneEmptyRow()`
   - 解決：使用內建刪除功能或手動呼叫 `tableComponent.RefreshEmptyRow()`

5. **非觸發欄位也新增空行**
   - 原因：沒有設定任何 `TriggerEmptyRowOnFilled`，使用傳統模式
   - 解決：明確設定關鍵欄位的 `TriggerEmptyRowOnFilled = true`

6. **❌ Entity 類別使用 int 屬性時，空行判斷失效（預設值 0 被視為有值）**
   - **原因**：當資料模型是 Entity 類別（如 `UnitConversion`），其屬性通常是 `int FromUnitId`（預設值 0），`IsValueNullOrEmpty` 會將 0 視為「有值」，導致空行判斷錯誤
   - **影響**：
     * 初始載入時可能出現 2 個空行（因為第一個空行被誤判為有值）
     * 每次輸入任何欄位都會新增空行（因為空行檢查失效）
   - **解決方案：使用包裝類別（Wrapper Class）**
   
   **✅ 方案：創建包裝類別使用 nullable 類型**
   
   當無法修改 Entity 類別本身時（如資料庫 Entity），創建一個包裝類別：
   
   ```csharp
   // ❌ 問題：Entity 無法修改為 nullable
   public class UnitConversion : BaseEntity  // 資料庫 Entity
   {
       public int FromUnitId { get; set; }  // ⚠️ 預設值 0 會被視為有值
       public int ToUnitId { get; set; }
       public decimal ConversionRate { get; set; }
       public bool IsActive { get; set; }
   }
   
   // ✅ 解決：創建包裝類別
   public class UnitConversionItem  // 包裝類別
   {
       public int? FromUnitId { get; set; }  // 🔑 nullable！null 表示未選擇
       public int? ToUnitId { get; set; }
       public decimal? ConversionRate { get; set; }
       public bool IsActive { get; set; } = true;
       public UnitConversion? ExistingEntity { get; set; }  // 保存原始 Entity
   }
   
   // 使用包裝類別
   private List<UnitConversionItem> conversions = new();
   
   // 載入資料時轉換
   private async Task LoadDataAsync()
   {
       var dbConversions = await UnitConversionService.GetAllAsync();
       conversions = dbConversions.Select(c => new UnitConversionItem
       {
           FromUnitId = c.FromUnitId,
           ToUnitId = c.ToUnitId,
           ConversionRate = c.ConversionRate,
           IsActive = c.IsActive,
           ExistingEntity = c  // 保存原始 Entity
       }).ToList();
   }
   
   // 建立空項目
   private UnitConversionItem CreateEmptyConversion()
   {
       return new UnitConversionItem
       {
           FromUnitId = null,  // 🔑 null 表示未選擇
           ToUnitId = null,
           ConversionRate = null,
           IsActive = true,
           ExistingEntity = null  // 新增項目沒有 Entity
       };
   }
   
   // 儲存時轉換回 Entity
   private async Task HandleSaveConversion(UnitConversionItem item)
   {
       // 建立 Entity
       var conversion = new UnitConversion
       {
           FromUnitId = item.FromUnitId.Value,  // 已驗證非 null
           ToUnitId = item.ToUnitId.Value,
           ConversionRate = item.ConversionRate.Value,
           IsActive = item.IsActive
       };
       
       var result = await UnitConversionService.CreateAsync(conversion);
       if (result.IsSuccess)
       {
           // 重新載入以更新包裝類別列表
           await LoadDataAsync();
       }
   }
   
   // InteractiveTableComponent 使用包裝類別
   builder.OpenComponent<InteractiveTableComponent<UnitConversionItem>>(sequence++);
   builder.AddAttribute(sequence++, nameof(InteractiveTableComponent<UnitConversionItem>.Items), conversions);
   builder.AddAttribute(sequence++, nameof(InteractiveTableComponent<UnitConversionItem>.CreateEmptyItem), 
       (Func<UnitConversionItem>)CreateEmptyConversion);
   ```
   
   **包裝類別的優勢**：
   - ✅ 使用 nullable 類型確保空值判斷正確（`null` 是真正的空）
   - ✅ 不需要修改資料庫 Entity（保持資料層完整性）
   - ✅ 編輯和新增使用相同的資料結構
   - ✅ 通過 `ExistingEntity` 區分新增行（null）和編輯行（有值）
   - ✅ 可在包裝類別中添加 UI 專用屬性（如搜尋狀態）
   
   **適用場景**：
   - 使用 EF Core Entity（無法修改為 nullable）
   - 需要管理現有資料的 CRUD Modal
   - 需要區分新增和編輯狀態
   - 單位換算、參數設定等系統設定類功能
   
   **實際案例參考**：
   - `UnitConversionManagementModal.razor` - 單位換算管理（使用 `UnitConversionItem` 包裝 `UnitConversion`）
   - `PurchaseOrderTable.razor` - 採購訂單明細（使用 `ProductItem` 包裝採購明細）

4. **❌ 使用 CustomTemplate 時，選擇或輸入後不會自動新增空行**
   - **原因**：`CustomTemplate` 使用自訂的 `@onchange` 或 `@oninput` 事件，繞過了 InteractiveTableComponent 的內建事件處理機制（`HandleInputChange`、`HandleSelectionChange`），因此**無法觸發自動空行檢查**
   - **影響**：即使設定了 `TriggerEmptyRowOnFilled = true`，自動空行功能也不會生效
   - **解決方案**：
   
   **✅ 方案 1：改用內建欄位類型（強烈建議）**
   
   不使用 `CustomTemplate`，改用 InteractiveTableComponent 提供的內建類型：
   
   ```csharp
   // ❌ 錯誤：使用 CustomTemplate
   new() 
   { 
       Title = "商品", 
       PropertyName = "",
       ColumnType = InteractiveColumnType.Custom,
       TriggerEmptyRowOnFilled = true,  // ⚠️ 無效！
       CustomTemplate = item => 
       {
           return @<select @onchange="...">  // 自訂事件，不會觸發空行
               <!-- options -->
           </select>;
       }
   }
   
   // ✅ 正確：使用內建 Select 類型
   new() 
   { 
       Title = "商品", 
       PropertyName = "SelectedProductId",
       EmptyCheckPropertyName = "SelectedProduct",  // 指定檢查物件屬性
       ColumnType = InteractiveColumnType.Select,   // 使用內建類型
       TriggerEmptyRowOnFilled = true,              // ✅ 有效！
       Options = GetProductOptions(),
       OnSelectionChanged = EventCallback.Factory.Create<(object, object?)>(this, 
           async args => await OnProductChanged(args))
   }
   ```
   
   **✅ 方案 2：在自訂事件中手動觸發空行檢查**
   
   如果必須使用 `CustomTemplate`（例如需要複雜的唯讀邏輯），需要手動呼叫空行刷新：
   
   ```csharp
   private async Task OnProductChanged(ProductItem item, object? value)
   {
       // 記錄變更前狀態
       var wasEmpty = item.SelectedProduct == null;
       
       // 更新資料
       var productIdStr = value?.ToString();
       if (!string.IsNullOrEmpty(productIdStr) && int.TryParse(productIdStr, out var productId))
       {
           var product = GetAvailableProducts().FirstOrDefault(p => p.Id == productId);
           item.SelectedProduct = product;
       }
       
       await NotifyDetailsChanged();
       
       // 🔑 手動觸發空行檢查
       var isEmptyNow = item.SelectedProduct == null;
       if (wasEmpty && !isEmptyNow)  // 從空變非空
       {
           tableComponent?.RefreshEmptyRow();
       }
   }
   ```
   
   **內建欄位類型對應表**：
   
   | 需求 | ❌ CustomTemplate | ✅ 內建類型 |
   |-----|------------------|------------|
   | 文字輸入 | `<input type="text" @oninput="...">` | `InteractiveColumnType.Input` |
   | 數字輸入 | `<input type="number" @oninput="...">` | `InteractiveColumnType.Number` |
   | 下拉選單 | `<select @onchange="...">` | `InteractiveColumnType.Select` |
   | 複選框 | `<input type="checkbox" @onchange="...">` | `InteractiveColumnType.Checkbox` |
   | 可搜尋下拉 | 自訂搜尋組件 | `InteractiveColumnType.SearchableSelect` |
   | 日期選擇 | `<input type="date" @onchange="...">` | `InteractiveColumnType.Date` |
   | 按鈕 | `<button @onclick="...">` | `InteractiveColumnType.Button` |
   
   **條件唯讀的處理方式**：
   
   ```csharp
   // ✅ 使用內建類型 + IsDisabledFunc 處理條件唯讀
   new() 
   { 
       Title = "商品", 
       PropertyName = "SelectedProductId",
       ColumnType = InteractiveColumnType.Select,
       TriggerEmptyRowOnFilled = true,
       Options = GetProductOptions(),
       IsDisabledFunc = item =>  // 🔑 條件唯讀
       {
           var productItem = (ProductItem)item;
           var hasReceiving = productItem.ReceivedQuantity > 0;
           return IsReadOnly || hasReceiving;  // 已入庫則鎖定
       },
       TooltipFunc = item =>  // 🔑 動態提示
       {
           var productItem = (ProductItem)item;
           var hasReceiving = productItem.ReceivedQuantity > 0;
           return hasReceiving ? "此商品已有進貨記錄，無法修改" : null;
       }
   }
   ```
   
   **重點總結**：
   - ✅ **優先使用內建欄位類型**，才能享受自動空行管理功能
   - ✅ 使用 `IsDisabledFunc`、`TooltipFunc` 處理條件唯讀和動態提示
   - ⚠️ 只在內建類型無法滿足需求時才使用 `CustomTemplate`
   - ⚠️ 使用 `CustomTemplate` 時必須手動呼叫 `tableComponent.RefreshEmptyRow()`

### 🔧 公開方法

組件提供公開方法供父組件手動控制：

```csharp
// 手動刷新空行檢查（適用於批量載入或清空資料後）
tableComponent.RefreshEmptyRow();
```

**使用場景**:
```csharp
private async Task LoadData()
{
    ProductItems = await GetProductsFromDatabase();
    
    // 🔑 載入資料後，確保有空行
    await InvokeAsync(() => 
    {
        tableComponent?.RefreshEmptyRow();
        StateHasChanged();
    });
}
```

---

## 技術細節

### 私有欄位

```csharp
private TItem? _lastEmptyRow = default;  // 記錄最後一個空行的參考
```

### 完整流程圖

```
[初始化]
   ↓
CheckAndAddEmptyRowIfNeeded()
   ↓
HasEmptyRow()? ─No→ CreateEmptyItem() → Items.Add()
   ↓ Yes
[等待用戶輸入]
   ↓
HandleInputChange()
   ↓
記錄 wasEmpty = IsRowEmpty(item)
   ↓
SetPropertyValue()
   ↓
[判斷觸發條件]
   ├→ 欄位級觸發? (TriggerEmptyRowOnFilled)
   │     ↓ Yes
   │  wasEmpty && fieldWasEmpty && fieldHasValueNow?
   │     ↓ Yes
   │  AutoAddEmptyRowIfNeeded() → return
   │
   └→ 整行級觸發
        ↓
     wasEmpty && !isEmptyNow?
        ↓ Yes
     AutoAddEmptyRowIfNeeded()
```

---

## 版本資訊

- **建立日期**: 2025年11月12日
- **適用版本**: InteractiveTableComponent v2.0+
- **作者**: ERPCore2 開發團隊

---

## 相關文件

- [README_互動Table說明.md](./README_互動Table說明.md)
- [InteractiveTableComponent.razor](../Components/Shared/BaseModal/BaseTableComponent/InteractiveTableComponent.razor)
- [TestTable.razor](../Components/Shared/BaseModal/Modals/Purchase/TestTable.razor)
