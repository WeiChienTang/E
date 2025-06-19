# 自動完成下拉組件說明

## 📋 概述

`GenericFormComponent` 支援自動完成 (AutoComplete) 功能，提供現代化的搜尋體驗。使用者可以透過輸入關鍵字即時搜尋資料庫，並支援完整的鍵盤導航操作。

## 🚀 主要功能

### ✨ 核心特色
- **即時搜尋** - 使用者輸入時自動查詢資料庫
- **防抖動機制** - 可配置延遲避免頻繁查詢
- **鍵盤導航** - 完整支援鍵盤操作
- **視覺回饋** - 高亮顯示和 hover 效果
- **自動滾動** - 確保選項始終在可視範圍內
- **滑鼠協調** - 滑鼠與鍵盤操作無縫切換

### 🎯 使用者體驗
1. 使用者在輸入框中輸入關鍵字
2. 系統自動搜尋相關資料並顯示下拉選單
3. 使用者可透過滑鼠點擊或鍵盤選擇項目
4. 選擇後自動填入表單並關閉下拉選單

## ⌨️ 鍵盤操作

| 按鍵 | 功能 |
|------|------|
| `↓` (向下鍵) | 進入下拉選單或向下移動高亮 |
| `↑` (向上鍵) | 向上移動高亮 |
| `Enter` | 選擇當前高亮的選項 |
| `Escape` | 關閉下拉選單 |
| `Tab` | 關閉下拉選單並移到下一個欄位 |

## 🔧 技術實作

### 1. 欄位類型定義

```csharp
// 在 FormFieldDefinition.cs 中新增
public enum FormFieldType
{
    // ...其他類型...
    AutoComplete,  // 自動完成類型
}
```

### 2. 欄位配置屬性

```csharp
public class FormFieldDefinition
{
    // 自動完成搜尋函式
    public Func<string, Task<List<SelectOption>>>? SearchFunction { get; set; }
    
    // 搜尋延遲毫秒數 (預設: 300ms)
    public int AutoCompleteDelayMs { get; set; } = 300;
    
    // 最小搜尋字符數 (預設: 1)
    public int MinSearchLength { get; set; } = 1;
}
```

### 3. 使用範例

```csharp
// 在頁面組件中配置自動完成欄位
private void InitializeFormFields()
{
    var customerTypeSearchFunction = new Func<string, Task<List<SelectOption>>>(async (keyword) =>
    {
        List<CustomerType> results;
        if (string.IsNullOrWhiteSpace(keyword))
        {
            // 空搜尋時返回預設選項
            results = customerTypes.Take(10).ToList();
        }
        else
        {
            // 關鍵字搜尋
            results = await CustomerService.SearchCustomerTypesAsync(keyword);
        }
        
        return results.Select(ct => new SelectOption
        {
            Text = ct.TypeName,
            Value = ct.Id.ToString()
        }).ToList();
    });

    var formField = new FormFieldDefinition
    {
        PropertyName = nameof(Customer.CustomerTypeId),
        Label = "客戶類型",
        FieldType = FormFieldType.AutoComplete,
        Placeholder = "輸入客戶類型名稱進行搜尋...",
        SearchFunction = customerTypeSearchFunction,
        MinSearchLength = 1,
        AutoCompleteDelayMs = 300,
        ContainerCssClass = "col-md-6"
    };
}
```

## 🎨 視覺設計

### CSS 樣式類別

```css
/* 下拉選單容器 */
.dropdown-menu {
    border: 1px solid var(--border-color);
    border-radius: var(--radius);
    box-shadow: var(--shadow-md);
    max-height: 200px;
    overflow-y: auto;
}

/* 選單項目 */
.dropdown-item {
    padding: 0.5rem 1rem;
    cursor: pointer;
    transition: background-color 0.15s ease-in-out;
}

/* Hover 效果 */
.dropdown-item:hover {
    background-color: var(--table-hover-bg-striped);
    color: var(--table-hover-text);
}

/* 鍵盤高亮效果 */
.dropdown-item.highlighted {
    background-color: var(--table-hover-bg-striped);
    color: var(--table-hover-text);
}
```

### 顏色變數 (定義於 variables.css)

```css
:root {
    --table-hover-bg-striped: #fde68a;  /* 黃色背景 */
    --table-hover-text: #92400e;        /* 棕色文字 */
}
```

## 🔍 服務層實作

### 介面定義

```csharp
// 在服務介面中添加搜尋方法
public interface ICustomerService : IGenericManagementService<Customer>
{
    /// <summary>
    /// 根據關鍵字搜尋客戶類型
    /// </summary>
    Task<List<CustomerType>> SearchCustomerTypesAsync(string keyword);
    
    /// <summary>
    /// 根據關鍵字搜尋行業類型
    /// </summary>
    Task<List<IndustryType>> SearchIndustryTypesAsync(string keyword);
}
```

### 服務實作

```csharp
// 在服務實作中添加搜尋邏輯
public async Task<List<CustomerType>> SearchCustomerTypesAsync(string keyword)
{
    try
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<CustomerType>();
            
        return await _context.CustomerTypes
            .Where(ct => ct.Status == EntityStatus.Active && 
                        ct.TypeName.Contains(keyword))
            .OrderBy(ct => ct.TypeName)
            .Take(10) // 限制結果數量
            .ToListAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error searching customer types with keyword: {Keyword}", keyword);
        throw;
    }
}
```

## 📱 JavaScript 支援

### 自動滾動函式

```javascript
// 在 bootstrap-helpers.js 中定義
window.scrollToElement = function (elementId) {
    try {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({
                behavior: 'smooth',    // 平滑滾動
                block: 'nearest',      // 最小距離滾動
                inline: 'nearest'      // 水平方向最小滾動
            });
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error scrolling to element:', error);
        return false;
    }
};
```

## 🏗️ 架構設計

### 組件狀態管理

```csharp
// 自動完成相關狀態
private readonly Dictionary<string, List<SelectOption>> autoCompleteOptions = new();
private readonly Dictionary<string, bool> autoCompleteLoading = new();
private readonly Dictionary<string, bool> autoCompleteVisible = new();
private readonly Dictionary<string, Timer?> autoCompleteTimers = new();
private readonly Dictionary<string, string> autoCompleteDisplayValues = new();

// 鍵盤導航狀態
private readonly Dictionary<string, int> highlightedOptionIndex = new();
private readonly Dictionary<string, bool> keyboardNavigationActive = new();
```

### 事件處理流程

1. **輸入事件** → 觸發搜尋延遲計時器
2. **搜尋執行** → 調用 SearchFunction 獲取結果
3. **結果顯示** → 更新選項清單並顯示下拉選單
4. **鍵盤導航** → 處理方向鍵和選擇操作
5. **選項選擇** → 更新表單值並關閉下拉選單

## 🛠️ 檔案結構

```
Components/Shared/Forms/
├── GenericFormComponent.razor         # 主要組件檔案
├── GenericFormComponent.razor.css     # 組件專用樣式
├── FormFieldDefinition.cs             # 欄位定義類別
└── README_自動下拉說明.md              # 本說明文件

wwwroot/js/
└── bootstrap-helpers.js               # JavaScript 輔助函式

wwwroot/css/
├── variables.css                      # CSS 變數定義
└── app.css                           # 全域樣式
```

## 🔧 效能最佳化

### 防抖動機制
- 預設延遲 300ms 避免頻繁查詢
- 可透過 `AutoCompleteDelayMs` 屬性調整

### 結果限制
- 建議搜尋結果限制在 10-20 筆以內
- 使用 `Take(10)` 限制資料庫查詢結果

### 資源清理
- 組件銷毀時自動清理計時器
- 實作 `IDisposable` 介面確保記憶體釋放

## 🎯 最佳實踐

### 1. 搜尋函式設計
- 支援空字串搜尋以提供初始選項
- 實作適當的錯誤處理
- 限制搜尋結果數量避免效能問題

### 2. 使用者體驗
- 提供有意義的 placeholder 文字
- 設定適當的最小搜尋字符數
- 考慮載入狀態的視覺回饋

### 3. 無障礙支援
- 支援完整的鍵盤導航
- 提供適當的 ARIA 標籤 (未來改進項目)
- 確保顏色對比度符合標準

## 📝 更新日誌

### v1.0.0 (2025-06-19)
- ✅ 基本自動完成功能
- ✅ 鍵盤導航支援
- ✅ 視覺高亮效果
- ✅ 自動滾動功能
- ✅ 防抖動機制
- ✅ 組件範圍 CSS

## 🔮 未來改進

- [ ] ARIA 無障礙標籤支援
- [ ] 更多鍵盤快捷鍵 (Ctrl+A 全選等)
- [ ] 支援群組化選項顯示
- [ ] 快取機制減少重複查詢
- [ ] 支援自訂模板渲染
