using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.PageTemplate;
using ERPCore2.Data;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 欄位定義類別
    /// </summary>
    public class FieldDefinition<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// 屬性名稱
        /// </summary>
        public string PropertyName { get; set; } = string.Empty;
        
        /// <summary>
        /// 篩選器屬性名稱（如果與 PropertyName 不同）
        /// </summary>
        public string? FilterPropertyName { get; set; }
        
        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// 篩選器佔位符
        /// </summary>
        public string? FilterPlaceholder { get; set; }
        
        /// <summary>
        /// 篩選器類型
        /// </summary>
        public SearchFilterType FilterType { get; set; } = SearchFilterType.Text;
        
        /// <summary>
        /// 表格欄位類型
        /// </summary>
        public ColumnDataType ColumnType { get; set; } = ColumnDataType.Text;
        
        /// <summary>
        /// 是否在表格中顯示
        /// </summary>
        public bool ShowInTable { get; set; } = true;
        
        /// <summary>
        /// 是否在篩選器中顯示
        /// </summary>
        public bool ShowInFilter { get; set; } = true;
        
        private int _tableOrder = 0;
        private int? _filterOrder = null;
        
        /// <summary>
        /// 表格欄位排序
        /// </summary>
        public int TableOrder 
        { 
            get => _tableOrder; 
            set => _tableOrder = value; 
        }
        
        /// <summary>
        /// 篩選器排序（如果未設定，則使用 TableOrder 的值）
        /// </summary>
        public int FilterOrder 
        { 
            get => _filterOrder ?? _tableOrder; 
            set => _filterOrder = value; 
        }
        
        /// <summary>
        /// 下拉選單選項（用於 Select 類型）
        /// </summary>
        public List<SelectOption>? Options { get; set; }
        
        /// <summary>
        /// 篩選函式
        /// </summary>
        public Func<SearchFilterModel, IQueryable<TEntity>, IQueryable<TEntity>>? FilterFunction { get; set; }
        
        /// <summary>
        /// 自訂模板
        /// </summary>
        public RenderFragment<object>? CustomTemplate { get; set; }
        
        /// <summary>
        /// 欄位寬度樣式
        /// </summary>
        public string? HeaderStyle { get; set; } = "width: 120px;";
        
        /// <summary>
        /// 空值顯示文字
        /// </summary>
        public string? NullDisplayText { get; set; }
        
        /// <summary>
        /// 是否可排序
        /// </summary>
        public bool IsSortable { get; set; } = true;
        
        /// <summary>
        /// 建立表格欄位定義
        /// </summary>
        public TableColumnDefinition CreateTableColumn()
        {
            if (CustomTemplate != null)
            {
                return new TableColumnDefinition
                {
                    Title = DisplayName,
                    PropertyName = PropertyName,
                    DataType = ColumnType,
                    CustomTemplate = CustomTemplate,
                    HeaderStyle = HeaderStyle,
                    NullDisplayText = NullDisplayText,
                    IsSortable = IsSortable
                };
            }
            
            // 根據 ColumnType 建立適當的欄位定義
            var column = ColumnType switch
            {
                ColumnDataType.Date => TableColumnDefinition.Date(DisplayName, PropertyName),
                ColumnDataType.DateTime => TableColumnDefinition.DateTime(DisplayName, PropertyName),
                ColumnDataType.Number => TableColumnDefinition.Number(DisplayName, PropertyName),
                ColumnDataType.Currency => TableColumnDefinition.Currency(DisplayName, PropertyName),
                ColumnDataType.Boolean => TableColumnDefinition.Boolean(DisplayName, PropertyName),
                _ => TableColumnDefinition.Text(DisplayName, PropertyName)
            };
            
            if (!string.IsNullOrEmpty(HeaderStyle))
                column.HeaderStyle = HeaderStyle;
            if (!string.IsNullOrEmpty(NullDisplayText))
                column.NullDisplayText = NullDisplayText;
            column.IsSortable = IsSortable;
            
            return column;
        }
    }
}
