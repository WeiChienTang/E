# FieldDefinition 排序欄位改善說明

## 問題描述

在 `FieldDefinition<T>` 類別中，經常需要同時設定 `TableOrder` 和 `FilterOrder`，而這兩個值通常是相同的，造成重複設定的問題。

### 修改前的問題
```csharp
{
    nameof(PrinterConfiguration.Name),
    new FieldDefinition<PrinterConfiguration>
    {
        PropertyName = nameof(PrinterConfiguration.Name),
        DisplayName = "印表機名稱",
        TableOrder = 1,
        FilterOrder = 1,  // 重複設定相同的值
        // 其他屬性...
    }
}
```

## 解決方案

### 1. 修改 FieldDefinition 類別

在 `Components/FieldConfiguration/FieldDefinition.cs` 中新增了智能排序邏輯：

```csharp
public class FieldDefinition<T> where T : class
{
    private int? _tableOrder;
    private int? _filterOrder;

    /// <summary>
    /// 表格欄位排序 (主要排序設定)
    /// </summary>
    public int? TableOrder
    {
        get => _tableOrder;
        set => _tableOrder = value;
    }

    /// <summary>
    /// 篩選欄位排序 (若未設定則自動使用 TableOrder 的值)
    /// </summary>
    public int? FilterOrder
    {
        get => _filterOrder ?? _tableOrder;
        set => _filterOrder = value;
    }

    // 其他屬性保持不變...
}
```

### 2. 修改邏輯說明

- **預設行為**: `FilterOrder` 會自動使用 `TableOrder` 的值
- **特殊需求**: 如果需要不同的篩選順序，仍可以單獨設定 `FilterOrder`
- **向後相容**: 現有的程式碼不需要修改即可運作

## 使用方式

### 一般情況 (推薦)
只需要設定 `TableOrder`，`FilterOrder` 會自動跟隨：

```csharp
{
    nameof(PrinterConfiguration.Name),
    new FieldDefinition<PrinterConfiguration>
    {
        PropertyName = nameof(PrinterConfiguration.Name),
        DisplayName = "印表機名稱",
        TableOrder = 1,  // FilterOrder 會自動為 1
        // 其他屬性...
    }
}
```

### 特殊需求
如果表格順序和篩選順序需要不同：

```csharp
{
    nameof(SomeEntity.SomeProperty),
    new FieldDefinition<SomeEntity>
    {
        PropertyName = nameof(SomeEntity.SomeProperty),
        DisplayName = "某個欄位",
        TableOrder = 5,      // 表格中第5個顯示
        FilterOrder = 2,     // 篩選中第2個顯示
        // 其他屬性...
    }
}
```

## 修改建議

### 現有檔案的改善步驟

1. **檢查現有的 FieldConfiguration 檔案**
   - 尋找同時設定 `TableOrder` 和 `FilterOrder` 相同值的地方
   - 移除重複的 `FilterOrder` 設定

2. **優先處理的檔案**
   ```
   Components/FieldConfiguration/
   ├── PrinterConfigurationFieldConfiguration.cs ✅ (已修改)
   ├── [其他 FieldConfiguration 檔案]
   ```

3. **搜尋指令**
   使用以下搜尋模式找出需要修改的檔案：
   ```
   搜尋: TableOrder.*FilterOrder
   或
   搜尋: FilterOrder.*TableOrder
   ```

### 修改原則

- **移除重複**: 當 `TableOrder` 和 `FilterOrder` 值相同時，只保留 `TableOrder`
- **保留特殊**: 當 `FilterOrder` 確實需要不同值時，保留兩個設定
- **保持一致**: 確保同一個檔案內的修改風格一致

## 效益

1. **減少程式碼重複**: 避免設定相同的排序值兩次
2. **降低維護成本**: 只需要維護一個排序設定
3. **提高可讀性**: 程式碼更簡潔易懂
4. **向後相容**: 不會破壞現有功能

## 注意事項

- 此修改是**向後相容**的，現有程式碼仍會正常運作
- 新的程式碼建議只設定 `TableOrder`，除非有特殊需求
- 如果發現任何問題，可以隨時回復到明確設定兩個屬性的方式

---

