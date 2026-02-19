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
    /// 廢料類型欄位配置
    /// </summary>
    public class WasteTypeFieldConfiguration : BaseFieldConfiguration<WasteType>
    {
        private readonly INotificationService? _notificationService;

        public WasteTypeFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<WasteType>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<WasteType>>
                {
                    {
                        nameof(WasteType.Code),
                        new FieldDefinition<WasteType>
                        {
                            PropertyName = nameof(WasteType.Code),
                            DisplayName = "廢料類型編號",
                            FilterPlaceholder = "輸入編號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(WasteType.Code), wt => wt.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(WasteType.Name),
                        new FieldDefinition<WasteType>
                        {
                            PropertyName = nameof(WasteType.Name),
                            DisplayName = "廢料類型名稱",
                            FilterPlaceholder = "輸入名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(WasteType.Name), wt => wt.Name)
                        }
                    },
                    {
                        nameof(WasteType.Unit),
                        new FieldDefinition<WasteType>
                        {
                            PropertyName = nameof(WasteType.Unit),
                            DisplayName = "計量單位",
                            FilterPlaceholder = "輸入單位搜尋",
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(WasteType.Unit), wt => wt.Unit, allowNull: true)
                        }
                    },
                    {
                        nameof(WasteType.Description),
                        new FieldDefinition<WasteType>
                        {
                            PropertyName = nameof(WasteType.Description),
                            DisplayName = "描述",
                            FilterPlaceholder = "輸入描述搜尋",
                            TableOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(WasteType.Description), wt => wt.Description, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "廢料類型欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("廢料類型欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<WasteType>>();
            }
        }
    }
}
