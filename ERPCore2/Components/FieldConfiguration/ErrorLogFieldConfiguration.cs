using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using System.ComponentModel;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 錯誤記錄欄位配置
    /// </summary>
    public class ErrorLogFieldConfiguration : BaseFieldConfiguration<ErrorLog>
    {
        private readonly INotificationService? _notificationService;
        
        public ErrorLogFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<ErrorLog>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ErrorLog>>
                {
                    {
                        nameof(ErrorLog.ErrorId),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.ErrorId),
                            DisplayName = "錯誤ID",
                            FilterPlaceholder = "輸入錯誤ID搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 160px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ErrorLog.ErrorId), e => e.ErrorId)
                        }
                    },
                    {
                        nameof(ErrorLog.Level),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.Level),
                            DisplayName = "錯誤等級",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 2,
                            HeaderStyle = "width: 120px;",
                            Options = GetErrorLevelOptions(),
                            FilterFunction = (model, query) => ApplyErrorLevelFilter(model, query)
                        }
                    },
                    {
                        nameof(ErrorLog.Source),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.Source),
                            DisplayName = "錯誤來源",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            HeaderStyle = "width: 120px;",
                            Options = GetErrorSourceOptions(),
                            FilterFunction = (model, query) => ApplyErrorSourceFilter(model, query)
                        }
                    },
                    {
                        nameof(ErrorLog.Category),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.Category),
                            DisplayName = "錯誤分類",
                            FilterPlaceholder = "輸入錯誤分類搜尋",
                            TableOrder = 4,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ErrorLog.Category), e => e.Category)
                        }
                    },
                    {
                        nameof(ErrorLog.Message),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.Message),
                            DisplayName = "錯誤訊息",
                            FilterPlaceholder = "輸入錯誤訊息搜尋",
                            TableOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ErrorLog.Message), e => e.Message)
                        }
                    },
                    {
                        nameof(ErrorLog.Module),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.Module),
                            DisplayName = "模組",
                            FilterPlaceholder = "輸入模組名稱搜尋",
                            TableOrder = 6,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ErrorLog.Module), e => e.Module, allowNull: true)
                        }
                    },
                    {
                        nameof(ErrorLog.OccurredAt),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.OccurredAt),
                            DisplayName = "發生時間",
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 7,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(ErrorLog.OccurredAt), e => e.OccurredAt)
                        }
                    },
                    {
                        nameof(ErrorLog.IsResolved),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.IsResolved),
                            DisplayName = "已解決",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 8,
                            HeaderStyle = "width: 100px;",
                            Options = new List<SelectOption>
                            {
                                new() { Text = "全部", Value = "" },
                                new() { Text = "已解決", Value = "true" },
                                new() { Text = "未解決", Value = "false" }
                            },
                            FilterFunction = (model, query) => ApplyBooleanFilter(model, query, nameof(ErrorLog.IsResolved))
                        }
                    },
                    {
                        nameof(ErrorLog.UserId),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.UserId),
                            DisplayName = "使用者",
                            FilterPlaceholder = "輸入使用者ID搜尋",
                            TableOrder = 9,
                            HeaderStyle = "width: 120px;",
                            ShowInTable = false, // 預設不在表格中顯示
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ErrorLog.UserId), e => e.UserId, allowNull: true)
                        }
                    },
                    {
                        nameof(ErrorLog.RequestPath),
                        new FieldDefinition<ErrorLog>
                        {
                            PropertyName = nameof(ErrorLog.RequestPath),
                            DisplayName = "請求路徑",
                            FilterPlaceholder = "輸入請求路徑搜尋",
                            TableOrder = 10,
                            ShowInTable = false, // 預設不在表格中顯示
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ErrorLog.RequestPath), e => e.RequestPath, allowNull: true)
                        }
                    }
                };
            }
            catch
            {
                // 記錄錯誤到檔案（避免循環依賴）
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (_notificationService != null)
                        {
                            await _notificationService.ShowErrorAsync("載入錯誤記錄欄位配置失敗");
                        }
                    }
                    catch
                    {
                        // 忽略通知錯誤
                    }
                });

                // 回傳安全的預設值
                return new Dictionary<string, FieldDefinition<ErrorLog>>();
            }
        }

        /// <summary>
        /// 取得錯誤等級選項
        /// </summary>
        private List<SelectOption> GetErrorLevelOptions()
        {
            try
            {
                var options = new List<SelectOption> { new() { Text = "全部", Value = "" } };
                
                foreach (ErrorLevel level in Enum.GetValues<ErrorLevel>())
                {
                    var displayName = level switch
                    {
                        ErrorLevel.Info => "資訊",
                        ErrorLevel.Warning => "警告",
                        ErrorLevel.Error => "錯誤",
                        ErrorLevel.Critical => "嚴重",
                        _ => level.ToString()
                    };
                    
                    options.Add(new SelectOption
                    {
                        Text = displayName,
                        Value = ((int)level).ToString()
                    });
                }
                
                return options;
            }
            catch
            {
                return new List<SelectOption> { new() { Text = "全部", Value = "" } };
            }
        }

        /// <summary>
        /// 取得錯誤來源選項
        /// </summary>
        private List<SelectOption> GetErrorSourceOptions()
        {
            try
            {
                var options = new List<SelectOption> { new() { Text = "全部", Value = "" } };
                
                foreach (ErrorSource source in Enum.GetValues<ErrorSource>())
                {
                    var displayName = source switch
                    {
                        ErrorSource.Database => "資料庫",
                        ErrorSource.BusinessLogic => "業務邏輯",
                        ErrorSource.UserInterface => "使用者介面",
                        ErrorSource.System => "系統",
                        ErrorSource.API => "API",
                        ErrorSource.Security => "安全",
                        _ => source.ToString()
                    };
                    
                    options.Add(new SelectOption
                    {
                        Text = displayName,
                        Value = ((int)source).ToString()
                    });
                }
                
                return options;
            }
            catch
            {
                return new List<SelectOption> { new() { Text = "全部", Value = "" } };
            }
        }

        /// <summary>
        /// 錯誤等級篩選器
        /// </summary>
        private IQueryable<ErrorLog> ApplyErrorLevelFilter(ERPCore2.Components.Shared.Forms.SearchFilterModel searchModel, IQueryable<ErrorLog> query)
        {
            var levelValue = searchModel.SelectFilters.TryGetValue(nameof(ErrorLog.Level), out var value) ? value : null;
            if (!string.IsNullOrEmpty(levelValue) && int.TryParse(levelValue, out var levelInt))
            {
                var level = (ErrorLevel)levelInt;
                query = query.Where(e => e.Level == level);
            }
            return query;
        }

        /// <summary>
        /// 錯誤來源篩選器
        /// </summary>
        private IQueryable<ErrorLog> ApplyErrorSourceFilter(ERPCore2.Components.Shared.Forms.SearchFilterModel searchModel, IQueryable<ErrorLog> query)
        {
            var sourceValue = searchModel.SelectFilters.TryGetValue(nameof(ErrorLog.Source), out var value) ? value : null;
            if (!string.IsNullOrEmpty(sourceValue) && int.TryParse(sourceValue, out var sourceInt))
            {
                var source = (ErrorSource)sourceInt;
                query = query.Where(e => e.Source == source);
            }
            return query;
        }

        /// <summary>
        /// 布林值篩選器
        /// </summary>
        private IQueryable<ErrorLog> ApplyBooleanFilter(ERPCore2.Components.Shared.Forms.SearchFilterModel searchModel, IQueryable<ErrorLog> query, string fieldName)
        {
            var value = searchModel.SelectFilters.TryGetValue(fieldName, out var val) ? val : null;
            if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out var boolValue))
            {
                if (fieldName == nameof(ErrorLog.IsResolved))
                {
                    query = query.Where(e => e.IsResolved == boolValue);
                }
            }
            return query;
        }

        #region 樣式和顯示文本輔助方法
        
        /// <summary>
        /// 取得錯誤等級徽章樣式
        /// </summary>
        public static string GetLevelBadgeStyle(ErrorLevel level)
        {
            return level switch
            {
                ErrorLevel.Info => "background-color: #17a2b8; color: white;",
                ErrorLevel.Warning => "background-color: #ffc107; color: black;",
                ErrorLevel.Error => "background-color: #dc3545; color: white;",
                ErrorLevel.Critical => "background-color: #6f42c1; color: white;",
                _ => "background-color: #6c757d; color: white;"
            };
        }

        /// <summary>
        /// 取得錯誤等級顯示文本
        /// </summary>
        public static string GetLevelDisplayText(ErrorLevel level)
        {
            return level switch
            {
                ErrorLevel.Info => "資訊",
                ErrorLevel.Warning => "警告",
                ErrorLevel.Error => "錯誤",
                ErrorLevel.Critical => "嚴重",
                _ => level.ToString()
            };
        }

        /// <summary>
        /// 取得錯誤來源顯示文本
        /// </summary>
        public static string GetSourceDisplayText(ErrorSource source)
        {
            return source switch
            {
                ErrorSource.Database => "資料庫",
                ErrorSource.BusinessLogic => "業務邏輯",
                ErrorSource.UserInterface => "使用者介面",
                ErrorSource.System => "系統",
                ErrorSource.API => "API",
                ErrorSource.Security => "安全",
                _ => source.ToString()
            };
        }

        #endregion
    }
}