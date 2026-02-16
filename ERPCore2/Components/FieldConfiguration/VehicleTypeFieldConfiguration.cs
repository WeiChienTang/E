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
    /// 車型欄位配置
    /// </summary>
    public class VehicleTypeFieldConfiguration : BaseFieldConfiguration<VehicleType>
    {
        private readonly INotificationService? _notificationService;

        public VehicleTypeFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<VehicleType>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<VehicleType>>
                {
                    {
                        nameof(VehicleType.Code),
                        new FieldDefinition<VehicleType>
                        {
                            PropertyName = nameof(VehicleType.Code),
                            DisplayName = "車型編號",
                            FilterPlaceholder = "輸入車型編號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(VehicleType.Code), vt => vt.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(VehicleType.Name),
                        new FieldDefinition<VehicleType>
                        {
                            PropertyName = nameof(VehicleType.Name),
                            DisplayName = "車型名稱",
                            FilterPlaceholder = "輸入車型名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(VehicleType.Name), vt => vt.Name)
                        }
                    },
                    {
                        nameof(VehicleType.Description),
                        new FieldDefinition<VehicleType>
                        {
                            PropertyName = nameof(VehicleType.Description),
                            DisplayName = "車型描述",
                            FilterPlaceholder = "輸入車型描述搜尋",
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(VehicleType.Description), vt => vt.Description, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "車型欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("車型欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<VehicleType>>();
            }
        }
    }
}
