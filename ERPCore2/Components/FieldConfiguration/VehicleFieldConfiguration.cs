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
    /// 車輛欄位配置
    /// </summary>
    public class VehicleFieldConfiguration : BaseFieldConfiguration<Vehicle>
    {
        private readonly INotificationService? _notificationService;

        public VehicleFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<Vehicle>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Vehicle>>
                {
                    {
                        nameof(Vehicle.Code),
                        new FieldDefinition<Vehicle>
                        {
                            PropertyName = nameof(Vehicle.Code),
                            DisplayName = Dn("Field.VehicleCode", "車輛編號"),
                            FilterPlaceholder = Fp("Field.VehicleCode", "輸入車輛編號搜尋"),
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Vehicle.Code), v => v.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(Vehicle.LicensePlate),
                        new FieldDefinition<Vehicle>
                        {
                            PropertyName = nameof(Vehicle.LicensePlate),
                            DisplayName = Dn("Field.LicensePlate", "車牌號碼"),
                            FilterPlaceholder = Fp("Field.LicensePlate", "輸入車牌號碼搜尋"),
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Vehicle.LicensePlate), v => v.LicensePlate)
                        }
                    },
                    {
                        nameof(Vehicle.VehicleName),
                        new FieldDefinition<Vehicle>
                        {
                            PropertyName = nameof(Vehicle.VehicleName),
                            DisplayName = Dn("Field.VehicleName", "車輛名稱"),
                            FilterPlaceholder = Fp("Field.VehicleName", "輸入車輛名稱搜尋"),
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Vehicle.VehicleName), v => v.VehicleName)
                        }
                    },
                    {
                        nameof(Vehicle.VehicleTypeId),
                        new FieldDefinition<Vehicle>
                        {
                            PropertyName = "VehicleType.Name",
                            FilterPropertyName = nameof(Vehicle.VehicleTypeId),
                            DisplayName = Dn("Field.VehicleType", "車型"),
                            FilterPlaceholder = Fp("Field.VehicleType", "選擇車型"),
                            TableOrder = 4,
                            FilterOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Vehicle.VehicleTypeId), v => v.VehicleTypeId)
                        }
                    },
                    {
                        nameof(Vehicle.Brand),
                        new FieldDefinition<Vehicle>
                        {
                            PropertyName = nameof(Vehicle.Brand),
                            DisplayName = Dn("Field.Brand", "廠牌"),
                            FilterPlaceholder = Fp("Field.Brand", "輸入廠牌搜尋"),
                            TableOrder = 5,
                            FilterOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Vehicle.Brand), v => v.Brand, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "車輛欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("車輛欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<Vehicle>>();
            }
        }
    }
}
