using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
using ERPCore2.Data;
using ERPCore2.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 通用欄位配置基礎類別
    /// </summary>
    public abstract class BaseFieldConfiguration<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// 字串本地化器，由呼叫端注入以支援多語言
        /// </summary>
        protected IStringLocalizer<SharedResource>? L { get; private set; }

        /// <summary>
        /// 設定本地化器以啟用多語言支援
        /// </summary>
        public BaseFieldConfiguration<TEntity> SetLocalizer(IStringLocalizer<SharedResource> localizer)
        {
            L = localizer;
            return this;
        }

        /// <summary>
        /// 取得本地化顯示名稱，L 未設定時回傳 fallback（繁體中文）
        /// </summary>
        protected string Dn(string key, string fallback) =>
            L?[key].ToString() ?? fallback;

        /// <summary>
        /// 取得本地化篩選輸入框提示文字，使用 Placeholder.InputToSearch 格式字串
        /// L 未設定時回傳 fallback（繁體中文）
        /// </summary>
        protected string Fp(string displayNameKey, string fallback) =>
            L != null
                ? string.Format(L["Placeholder.InputToSearch"].ToString(), L[displayNameKey].ToString())
                : fallback;

        /// <summary>
        /// 取得本地化 NullDisplayText，L 未設定時回傳 fallback
        /// </summary>
        protected string Nd(string key, string fallback) =>
            L?[key].ToString() ?? fallback;
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
        /// 取得預設排序 - 預設按照 ID 降序排列(最新的在前面)
        /// </summary>
        protected virtual Func<IQueryable<TEntity>, IQueryable<TEntity>> GetDefaultSort()
        {
            return q => q.OrderByDescending(e => e.Id);
        }
        
        /// <summary>
        /// 應用篩選器（可在具體頁面中使用）
        /// </summary>
        public virtual IQueryable<TEntity> ApplyFilters(SearchFilterModel searchModel, IQueryable<TEntity> query, string methodName, Type pageType)
        {
            // 先檢查查詢是否為空或 null
            if (query == null)
            {
                return GetDefaultSort()(Enumerable.Empty<TEntity>().AsQueryable());
            }

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
