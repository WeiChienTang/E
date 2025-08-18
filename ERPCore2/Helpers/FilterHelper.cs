using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data;
using ERPCore2.Data.Enums;
using System.Linq.Expressions;
using System.Reflection;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 篩選輔助類別 - 提供通用的篩選邏輯，減少重複代碼
    /// </summary>
    public static class FilterHelper
    {
        /// <summary>
        /// 應用基礎實體的標準篩選（狀態、備註等）
        /// </summary>
        /// <typeparam name="T">實體類型，必須繼承自 BaseEntity</typeparam>
        /// <param name="searchModel">搜尋篩選模型</param>
        /// <param name="query">查詢</param>
        /// <returns>套用篩選後的查詢</returns>
        public static IQueryable<T> ApplyBaseEntityFilters<T>(SearchFilterModel searchModel, IQueryable<T> query) 
            where T : BaseEntity
        {
            // 狀態篩選
            query = ApplyStatusFilter(searchModel, query);
            
            // 備註篩選
            query = ApplyRemarksFilter(searchModel, query);
            
            return query;
        }

        /// <summary>
        /// 應用狀態篩選
        /// </summary>
        /// <typeparam name="T">實體類型，必須繼承自 BaseEntity</typeparam>
        /// <param name="searchModel">搜尋篩選模型</param>
        /// <param name="query">查詢</param>
        /// <param name="statusFieldName">狀態欄位名稱，預設為 "Status"</param>
        /// <returns>套用篩選後的查詢</returns>
        public static IQueryable<T> ApplyStatusFilter<T>(SearchFilterModel searchModel, IQueryable<T> query, string statusFieldName = "Status") 
            where T : BaseEntity
        {
            var statusFilter = searchModel.GetFilterValue(statusFieldName)?.ToString();
            if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<EntityStatus>(statusFilter, out var status))
            {
                query = query.Where(entity => entity.Status == status);
            }
            return query;
        }

        /// <summary>
        /// 應用備註篩選
        /// </summary>
        /// <typeparam name="T">實體類型，必須繼承自 BaseEntity</typeparam>
        /// <param name="searchModel">搜尋篩選模型</param>
        /// <param name="query">查詢</param>
        /// <param name="remarksFieldName">備註欄位名稱，預設為 "Remarks"</param>
        /// <returns>套用篩選後的查詢</returns>
        public static IQueryable<T> ApplyRemarksFilter<T>(SearchFilterModel searchModel, IQueryable<T> query, string remarksFieldName = "Remarks") 
            where T : BaseEntity
        {
            var remarksFilter = searchModel.GetFilterValue(remarksFieldName)?.ToString();
            if (!string.IsNullOrWhiteSpace(remarksFilter))
            {
                query = query.Where(entity => entity.Remarks != null && entity.Remarks.Contains(remarksFilter, StringComparison.OrdinalIgnoreCase));
            }
            return query;
        }

        /// <summary>
        /// 應用文字欄位包含篩選（不區分大小寫）
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="searchModel">搜尋篩選模型</param>
        /// <param name="query">查詢</param>
        /// <param name="filterName">篩選器名稱</param>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <param name="allowNull">是否允許屬性為 null，預設為 false</param>
        /// <returns>套用篩選後的查詢</returns>
        public static IQueryable<T> ApplyTextContainsFilter<T>(
            SearchFilterModel searchModel, 
            IQueryable<T> query, 
            string filterName, 
            Expression<Func<T, string?>> propertySelector,
            bool allowNull = false)
        {
            var filterValue = searchModel.GetFilterValue(filterName)?.ToString();
            if (!string.IsNullOrWhiteSpace(filterValue))
            {
                if (allowNull)
                {
                    // 允許屬性為 null，使用 null 檢查
                    query = query.Where(entity => 
                        propertySelector.Compile()(entity) != null && 
                        propertySelector.Compile()(entity)!.Contains(filterValue, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    // 不允許屬性為 null，直接使用 Contains
                    query = query.Where(BuildContainsExpression(propertySelector, filterValue));
                }
            }
            return query;
        }

        /// <summary>
        /// 應用整數 ID 篩選
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="searchModel">搜尋篩選模型</param>
        /// <param name="query">查詢</param>
        /// <param name="filterName">篩選器名稱</param>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <returns>套用篩選後的查詢</returns>
        public static IQueryable<T> ApplyIntIdFilter<T>(
            SearchFilterModel searchModel, 
            IQueryable<T> query, 
            string filterName, 
            Expression<Func<T, int>> propertySelector)
        {
            var filterValue = searchModel.GetFilterValue(filterName)?.ToString();
            if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out var id))
            {
                query = query.Where(BuildEqualsExpression(propertySelector, id));
            }
            return query;
        }

        /// <summary>
        /// 應用可為空整數 ID 篩選
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="searchModel">搜尋篩選模型</param>
        /// <param name="query">查詢</param>
        /// <param name="filterName">篩選器名稱</param>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <returns>套用篩選後的查詢</returns>
        public static IQueryable<T> ApplyNullableIntIdFilter<T>(
            SearchFilterModel searchModel, 
            IQueryable<T> query, 
            string filterName, 
            Expression<Func<T, int?>> propertySelector)
        {
            var filterValue = searchModel.GetFilterValue(filterName)?.ToString();
            if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out var id))
            {
                // 將 int 轉換為 int? 以匹配屬性型別
                int? nullableId = id;
                query = query.Where(BuildEqualsExpression(propertySelector, nullableId));
            }
            return query;
        }

        /// <summary>
        /// 應用日期範圍篩選
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="searchModel">搜尋篩選模型</param>
        /// <param name="query">查詢</param>
        /// <param name="filterName">篩選器名稱（會自動加上 _From 和 _To 後綴）</param>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <returns>套用篩選後的查詢</returns>
        public static IQueryable<T> ApplyDateRangeFilter<T>(
            SearchFilterModel searchModel, 
            IQueryable<T> query, 
            string filterName, 
            Expression<Func<T, DateTime>> propertySelector)
        {
            // 首先嘗試獲取 DateRange 物件（新的前端格式）
            var dateRangeFilter = searchModel.GetFilterValue(filterName) as DateRange;
            if (dateRangeFilter != null && dateRangeFilter.IsValid())
            {
                // 開始日期篩選
                if (dateRangeFilter.StartDate.HasValue)
                {
                    query = query.Where(BuildGreaterThanOrEqualsExpression(propertySelector, dateRangeFilter.StartDate.Value.Date));
                }

                // 結束日期篩選
                if (dateRangeFilter.EndDate.HasValue)
                {
                    var endDate = dateRangeFilter.EndDate.Value.Date.AddDays(1); // 包含整天
                    query = query.Where(BuildLessThanExpression(propertySelector, endDate));
                }

                return query;
            }

            // 如果沒有找到 DateRange 物件，則嘗試舊的格式（向下相容）
            // 開始日期篩選
            var dateFromFilter = searchModel.GetFilterValue($"{filterName}_From")?.ToString();
            if (!string.IsNullOrWhiteSpace(dateFromFilter) && DateTime.TryParse(dateFromFilter, out var dateFrom))
            {
                query = query.Where(BuildGreaterThanOrEqualsExpression(propertySelector, dateFrom.Date));
            }

            // 結束日期篩選
            var dateToFilter = searchModel.GetFilterValue($"{filterName}_To")?.ToString();
            if (!string.IsNullOrWhiteSpace(dateToFilter) && DateTime.TryParse(dateToFilter, out var dateTo))
            {
                var endDate = dateTo.Date.AddDays(1); // 包含整天
                query = query.Where(BuildLessThanExpression(propertySelector, endDate));
            }

            return query;
        }

        /// <summary>
        /// 應用可為空日期範圍篩選
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="searchModel">搜尋篩選模型</param>
        /// <param name="query">查詢</param>
        /// <param name="filterName">篩選器名稱（會自動加上 _From 和 _To 後綴）</param>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <returns>套用篩選後的查詢</returns>
        public static IQueryable<T> ApplyNullableDateRangeFilter<T>(
            SearchFilterModel searchModel, 
            IQueryable<T> query, 
            string filterName, 
            Expression<Func<T, DateTime?>> propertySelector)
        {
            // 首先嘗試獲取 DateRange 物件（新的前端格式）
            var dateRangeFilter = searchModel.GetFilterValue(filterName) as DateRange;
            if (dateRangeFilter != null && dateRangeFilter.IsValid())
            {
                // 開始日期篩選
                if (dateRangeFilter.StartDate.HasValue)
                {
                    query = query.Where(BuildNullableDateGreaterThanOrEqualsExpression(propertySelector, dateRangeFilter.StartDate.Value.Date));
                }

                // 結束日期篩選
                if (dateRangeFilter.EndDate.HasValue)
                {
                    var endDate = dateRangeFilter.EndDate.Value.Date.AddDays(1); // 包含整天
                    query = query.Where(BuildNullableDateLessThanExpression(propertySelector, endDate));
                }

                return query;
            }

            // 如果沒有找到 DateRange 物件，則嘗試舊的格式（向下相容）
            // 開始日期篩選
            var dateFromFilter = searchModel.GetFilterValue($"{filterName}_From")?.ToString();
            if (!string.IsNullOrWhiteSpace(dateFromFilter) && DateTime.TryParse(dateFromFilter, out var dateFrom))
            {
                query = query.Where(BuildNullableDateGreaterThanOrEqualsExpression(propertySelector, dateFrom.Date));
            }

            // 結束日期篩選
            var dateToFilter = searchModel.GetFilterValue($"{filterName}_To")?.ToString();
            if (!string.IsNullOrWhiteSpace(dateToFilter) && DateTime.TryParse(dateToFilter, out var dateTo))
            {
                var endDate = dateTo.Date.AddDays(1); // 包含整天
                query = query.Where(BuildNullableDateLessThanExpression(propertySelector, endDate));
            }

            return query;
        }

        /// <summary>
        /// 套用所有篩選的包裝方法，提供錯誤處理
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="searchModel">搜尋篩選模型</param>
        /// <param name="query">查詢</param>
        /// <param name="filterActions">篩選動作清單</param>
        /// <param name="defaultOrderBy">預設排序，當發生錯誤時使用</param>
        /// <param name="methodName">呼叫方法名稱，用於錯誤記錄</param>
        /// <param name="sourceType">來源類型，用於錯誤記錄</param>
        /// <returns>套用篩選後的查詢</returns>
        public static IQueryable<T> ApplyFiltersWithErrorHandling<T>(
            SearchFilterModel searchModel,
            IQueryable<T> query,
            List<Func<SearchFilterModel, IQueryable<T>, IQueryable<T>>> filterActions,
            Func<IQueryable<T>, IQueryable<T>> defaultOrderBy,
            string methodName,
            Type sourceType)
        {
            try
            {
                foreach (var filterAction in filterActions)
                {
                    query = filterAction(searchModel, query);
                }
                return defaultOrderBy(query);
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不阻擋執行
                _ = ErrorHandlingHelper.HandlePageErrorAsync(
                    ex,
                    methodName,
                    sourceType
                );
                return defaultOrderBy(query);
            }
        }

        #region 私有輔助方法

        /// <summary>
        /// 建立包含表示式
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <param name="value">要包含的值</param>
        /// <returns>包含表示式</returns>
        private static Expression<Func<T, bool>> BuildContainsExpression<T>(
            Expression<Func<T, string?>> propertySelector, 
            string value)
        {
            var parameter = propertySelector.Parameters[0];
            var property = propertySelector.Body;
            
            // 先檢查屬性是否為 null
            var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
            
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            var containsCall = Expression.Call(
                property, 
                method!, 
                Expression.Constant(value),
                Expression.Constant(StringComparison.OrdinalIgnoreCase));
            
            // 結合 null 檢查和 Contains 檢查
            var andExpression = Expression.AndAlso(nullCheck, containsCall);
            
            return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
        }

        /// <summary>
        /// 建立等於表示式
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <typeparam name="TProperty">屬性類型</typeparam>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <param name="value">要比較的值</param>
        /// <returns>等於表示式</returns>
        private static Expression<Func<T, bool>> BuildEqualsExpression<T, TProperty>(
            Expression<Func<T, TProperty>> propertySelector, 
            TProperty value)
        {
            var parameter = propertySelector.Parameters[0];
            var property = propertySelector.Body;
            
            // 確保常數的型別與屬性型別匹配
            var constant = Expression.Constant(value, typeof(TProperty));
            var equals = Expression.Equal(property, constant);
            
            return Expression.Lambda<Func<T, bool>>(equals, parameter);
        }

        /// <summary>
        /// 建立大於等於表示式
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <typeparam name="TProperty">屬性類型</typeparam>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <param name="value">要比較的值</param>
        /// <returns>大於等於表示式</returns>
        private static Expression<Func<T, bool>> BuildGreaterThanOrEqualsExpression<T, TProperty>(
            Expression<Func<T, TProperty>> propertySelector, 
            TProperty value)
        {
            var parameter = propertySelector.Parameters[0];
            var property = propertySelector.Body;
            var constant = Expression.Constant(value);
            var greaterThanOrEquals = Expression.GreaterThanOrEqual(property, constant);
            
            return Expression.Lambda<Func<T, bool>>(greaterThanOrEquals, parameter);
        }

        /// <summary>
        /// 建立小於表示式
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <typeparam name="TProperty">屬性類型</typeparam>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <param name="value">要比較的值</param>
        /// <returns>小於表示式</returns>
        private static Expression<Func<T, bool>> BuildLessThanExpression<T, TProperty>(
            Expression<Func<T, TProperty>> propertySelector, 
            TProperty value)
        {
            var parameter = propertySelector.Parameters[0];
            var property = propertySelector.Body;
            var constant = Expression.Constant(value);
            var lessThan = Expression.LessThan(property, constant);
            
            return Expression.Lambda<Func<T, bool>>(lessThan, parameter);
        }

        /// <summary>
        /// 建立可為空日期大於等於表示式
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <param name="value">要比較的值</param>
        /// <returns>大於等於表示式</returns>
        private static Expression<Func<T, bool>> BuildNullableDateGreaterThanOrEqualsExpression<T>(
            Expression<Func<T, DateTime?>> propertySelector, 
            DateTime value)
        {
            var parameter = propertySelector.Parameters[0];
            var property = propertySelector.Body;
            
            // property.HasValue && property.Value >= value
            var hasValue = Expression.Property(property, "HasValue");
            var propertyValue = Expression.Property(property, "Value");
            var constant = Expression.Constant(value);
            var greaterThanOrEquals = Expression.GreaterThanOrEqual(propertyValue, constant);
            var andExpression = Expression.AndAlso(hasValue, greaterThanOrEquals);
            
            return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
        }

        /// <summary>
        /// 建立可為空日期小於表示式
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="propertySelector">屬性選擇器</param>
        /// <param name="value">要比較的值</param>
        /// <returns>小於表示式</returns>
        private static Expression<Func<T, bool>> BuildNullableDateLessThanExpression<T>(
            Expression<Func<T, DateTime?>> propertySelector, 
            DateTime value)
        {
            var parameter = propertySelector.Parameters[0];
            var property = propertySelector.Body;
            
            // property.HasValue && property.Value < value
            var hasValue = Expression.Property(property, "HasValue");
            var propertyValue = Expression.Property(property, "Value");
            var constant = Expression.Constant(value);
            var lessThan = Expression.LessThan(propertyValue, constant);
            var andExpression = Expression.AndAlso(hasValue, lessThan);
            
            return Expression.Lambda<Func<T, bool>>(andExpression, parameter);
        }

        #endregion
    }
}
