using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.PageTemplate;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 合成表類型欄位配置
    /// </summary>
    public class CompositionCategoryFieldConfiguration : BaseFieldConfiguration<CompositionCategory>
    {
        private readonly INotificationService? _notificationService;
        
        public CompositionCategoryFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<CompositionCategory>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<CompositionCategory>>
                {
                    {
                        nameof(CompositionCategory.Code),
                        new FieldDefinition<CompositionCategory>
                        {
                            PropertyName = nameof(CompositionCategory.Code),
                            DisplayName = "類型代碼",
                            FilterPlaceholder = "輸入類型代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CompositionCategory.Code), cc => cc.Code)
                        }
                    },
                    {
                        nameof(CompositionCategory.Name),
                        new FieldDefinition<CompositionCategory>
                        {
                            PropertyName = nameof(CompositionCategory.Name),
                            DisplayName = "類型名稱",
                            FilterPlaceholder = "輸入類型名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CompositionCategory.Name), cc => cc.Name)
                        }
                    },
                    {
                        "UsageCount",
                        new FieldDefinition<CompositionCategory>
                        {
                            PropertyName = "UsageCount",
                            DisplayName = "使用次數",
                            ShowInFilter = false,
                            TableOrder = 3,
                            HeaderStyle = "width: 120px; text-align: center;",
                            CustomTemplate = item => builder =>
                            {
                                var category = (CompositionCategory)item;
                                var count = category.ProductCompositions?.Count ?? 0;
                                builder.OpenElement(0, "div");
                                builder.AddAttribute(1, "class", "text-center");
                                builder.OpenElement(2, "span");
                                builder.AddAttribute(3, "class", count > 0 ? "badge bg-primary" : "badge bg-secondary");
                                builder.AddContent(4, count);
                                builder.CloseElement();
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化合成表類型欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化合成表類型欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<CompositionCategory>>();
            }
        }
    }
}


