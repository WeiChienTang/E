using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 權限明細欄位配置
    /// </summary>
    public class PermissionFieldConfiguration : BaseFieldConfiguration<Permission>
    {
        private readonly INotificationService? _notificationService;
        
        public PermissionFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Permission>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Permission>>
                {
                    {
                        nameof(Permission.Code),
                        new FieldDefinition<Permission>
                        {
                            PropertyName = nameof(Permission.Code),
                            DisplayName = "權限明細編號",
                            FilterPlaceholder = "輸入權限明細編號搜尋",
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Permission.Code), p => p.Code)
                        }
                    },
                    {
                        nameof(Permission.Name),
                        new FieldDefinition<Permission>
                        {
                            PropertyName = nameof(Permission.Name),
                            DisplayName = "權限明細名稱",
                            FilterPlaceholder = "輸入權限明細名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Permission.Name), p => p.Name)
                        }
                    },
                    {
                        nameof(Permission.Level),
                        new FieldDefinition<Permission>
                        {
                            PropertyName = nameof(Permission.Level),
                            DisplayName = "權限級別",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "一般權限", Value = ((int)PermissionLevel.Normal).ToString() },
                                new SelectOption { Text = "敏感權限", Value = ((int)PermissionLevel.Sensitive).ToString() }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var levelFilter = model.GetFilterValue(nameof(Permission.Level))?.ToString();
                                if (!string.IsNullOrWhiteSpace(levelFilter) && int.TryParse(levelFilter, out int levelValue))
                                {
                                    query = query.Where(p => (int)p.Level == levelValue);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var permission = (Permission)item;
                                var isSensitive = permission.Level == PermissionLevel.Sensitive;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "badge text-white");
                                builder.AddAttribute(2, "style", isSensitive ? "background-color: #dc3545;" : "background-color: #28a745;");
                                builder.AddContent(3, isSensitive ? "敏感權限" : "一般權限");
                                builder.CloseElement();
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化權限明細欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化權限明細欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Permission>>();
            }
        }
    }
}


