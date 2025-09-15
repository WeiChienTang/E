# 表格欄位寬度調整功能使用指南

## 功能概述

GenericTableComponent 現在支援使用者手動調整表格欄位寬度的功能。使用者可以透過拖拽欄位標題右側的調整手柄來改變欄位寬度。

## 啟用方式

設定 `EnableColumnResize="true"` 來啟用欄位寬度調整功能：

```razor
<GenericTableComponent TItem="YourDataType" 
                      Items="@items"
                      ColumnDefinitions="@columnDefinitions"
                      EnableColumnResize="true" />
```

## 欄位初始寬度設定

在 `TableColumnDefinition` 中使用 `Width` 屬性來設定欄位的初始寬度：

```csharp
var columnDefinitions = new List<TableColumnDefinition>
{
    new TableColumnDefinition 
    { 
        Title = "產品名稱", 
        PropertyName = "Name", 
        Width = "200px" // 設定初始寬度為 200 像素
    },
    new TableColumnDefinition 
    { 
        Title = "價格", 
        PropertyName = "Price", 
        Width = "15%" // 設定初始寬度為表格的 15%
    }
};
```

## 支援的寬度單位

- `px`：像素 (例如: "150px")
- `%`：百分比 (例如: "25%")
- `em`, `rem`：相對單位
- `auto`：自動寬度

## 功能特點

1. **精確拖拽調整**：拖拽欄位邊界時，只調整被拖拽的那一側欄位寬度，右側相鄰欄位會相應縮小以保持表格總寬度不變
2. **視覺回饋**：調整時手柄會變色，提供清楚的視覺回饋
3. **最小寬度**：每個欄位都有 50px 的最小寬度限制，防止欄位被縮得太小
4. **表格佈局**：啟用調整功能時，表格會自動使用固定佈局 (table-layout: fixed)
5. **平滑調整**：相鄰欄位會同步調整，確保表格佈局的穩定性

## 使用範例

```razor
@page "/products"

<GenericTableComponent TItem="Product" 
                      Items="@products"
                      ColumnDefinitions="@GetColumnDefinitions()"
                      EnableColumnResize="true"
                      ShowHeader="true"
                      IsStriped="true"
                      IsHoverable="true" />

@code {
    private List<TableColumnDefinition> GetColumnDefinitions()
    {
        return new List<TableColumnDefinition>
        {
            new() { Title = "編號", PropertyName = "Id", Width = "80px" },
            new() { Title = "名稱", PropertyName = "Name", Width = "250px" },
            new() { Title = "價格", PropertyName = "Price", DataType = ColumnDataType.Currency, Width = "120px" },
            new() { Title = "分類", PropertyName = "Category", Width = "180px" }
        };
    }
}
```

## 注意事項

1. **相容性**：此功能與分頁、排序等其他表格功能完全相容
2. **響應式**：在行動裝置上，建議關閉此功能以確保最佳使用體驗
3. **效能**：對大量資料的表格不會影響效能，因為調整邏輯僅作用於 DOM 層級

## 樣式自訂

可透過 CSS 自訂調整手柄的外觀：

```css
.column-resizer:hover {
    border-right-color: #your-color;
    background-color: rgba(your-color, 0.1);
}
```