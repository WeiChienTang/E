using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Tables;
using ERPCore2.Data;
using ERPCore2.Models;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 通用欄位配置基礎類別
    /// </summary>
    public abstract class BaseFieldConfiguration<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// 取得欄位定義
        /// </summary>
        public abstract Dictionary<string, FieldDefinition<TEntity>> GetFieldDefinitions();
        
        /// <summary>
        /// 建立篩選器定義
        /// </summary>
        public virtual List<SearchFilterDefinition> BuildFilters()
        {
            var builder = new SearchFilterBuilder<SearchFilterModel>();
            
            var fields = GetFieldDefinitions().Values
                .Where(f => f.ShowInFilter)
                .OrderBy(f => f.FilterOrder);
            
            foreach (var field in fields)
            {
                // 使用 FilterPropertyName，如果沒有則使用 PropertyName
                var filterPropertyName = field.FilterPropertyName ?? field.PropertyName;
                
                switch (field.FilterType)
                {
                    case SearchFilterType.Text:
                        builder.AddText(filterPropertyName, field.DisplayName, field.FilterPlaceholder);
                        break;
                    case SearchFilterType.Select:
                        builder.AddSelect(filterPropertyName, field.DisplayName, field.Options ?? new List<SelectOption>());
                        break;
                    case SearchFilterType.MultiSelect:
                        builder.AddMultiSelect(filterPropertyName, field.DisplayName, field.Options ?? new List<SelectOption>());
                        break;
                    case SearchFilterType.Number:
                        builder.AddNumber(filterPropertyName, field.DisplayName, field.FilterPlaceholder);
                        break;
                    case SearchFilterType.NumberRange:
                        builder.AddNumberRange(filterPropertyName, field.DisplayName);
                        break;
                    case SearchFilterType.Date:
                        builder.AddDate(filterPropertyName, field.DisplayName);
                        break;
                    case SearchFilterType.DateRange:
                        builder.AddDateRange(filterPropertyName, field.DisplayName);
                        break;
                    case SearchFilterType.Boolean:
                        builder.AddBoolean(filterPropertyName, field.DisplayName);
                        break;
                    // 可以根據需要添加更多類型
                }
            }
            
            return builder.Build();
        }
        
        /// <summary>
        /// 建立表格欄位定義
        /// </summary>
        public virtual List<TableColumnDefinition> BuildColumns()
        {
            return GetFieldDefinitions().Values
                .Where(f => f.ShowInTable)
                .OrderBy(f => f.TableOrder)
                .Select(f => f.CreateTableColumn())
                .ToList();
        }
        
        /// <summary>
        /// 取得篩選函式清單
        /// </summary>
        public virtual List<Func<SearchFilterModel, IQueryable<TEntity>, IQueryable<TEntity>>> GetFilterFunctions()
        {
            // 先添加基礎實體篩選
            var functions = new List<Func<SearchFilterModel, IQueryable<TEntity>, IQueryable<TEntity>>>
            {
                (model, query) => FilterHelper.ApplyBaseEntityFilters(model, query)
            };
            
            // 添加欄位特定的篩選函式
            var fieldFunctions = GetFieldDefinitions().Values
                .Where(f => f.FilterFunction != null)
                .Select(f => f.FilterFunction!)
                .ToList();
                
            functions.AddRange(fieldFunctions);
            
            return functions;
        }
        
        /// <summary>
        /// 取得預設排序
        /// </summary>
        protected virtual Func<IQueryable<TEntity>, IQueryable<TEntity>> GetDefaultSort()
        {
            return q => q.OrderBy(e => e.Code);
        }
        
        /// <summary>
        /// 應用篩選器（可在具體頁面中使用）
        /// </summary>
        public virtual IQueryable<TEntity> ApplyFilters(SearchFilterModel searchModel, IQueryable<TEntity> query, string methodName, Type pageType)
        {
            return FilterHelper.ApplyFiltersWithErrorHandling(
                searchModel,
                query,
                GetFilterFunctions(),
                GetDefaultSort(),
                methodName,
                pageType
            );
        }
    }
}
