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
    /// 車輛保養紀錄欄位配置
    /// </summary>
    public class VehicleMaintenanceFieldConfiguration : BaseFieldConfiguration<VehicleMaintenance>
    {
        private readonly INotificationService? _notificationService;

        public VehicleMaintenanceFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<VehicleMaintenance>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<VehicleMaintenance>>
                {
                    {
                        nameof(VehicleMaintenance.Code),
                        new FieldDefinition<VehicleMaintenance>
                        {
                            PropertyName = nameof(VehicleMaintenance.Code),
                            DisplayName = Dn("Field.MaintenanceCode", "保養編號"),
                            FilterPlaceholder = Fp("Field.MaintenanceCode", "輸入保養編號搜尋"),
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(VehicleMaintenance.Code), vm => vm.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(VehicleMaintenance.VehicleId),
                        new FieldDefinition<VehicleMaintenance>
                        {
                            PropertyName = "Vehicle.LicensePlate",
                            FilterPropertyName = nameof(VehicleMaintenance.VehicleId),
                            DisplayName = Dn("Field.Vehicle", "車輛"),
                            FilterPlaceholder = Fp("Field.Vehicle", "選擇車輛"),
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(VehicleMaintenance.VehicleId), vm => vm.VehicleId)
                        }
                    },
                    {
                        nameof(VehicleMaintenance.MaintenanceDate),
                        new FieldDefinition<VehicleMaintenance>
                        {
                            PropertyName = nameof(VehicleMaintenance.MaintenanceDate),
                            DisplayName = Dn("Field.MaintenanceDate", "保養日期"),
                            FilterPlaceholder = Fp("Field.MaintenanceDate", "選擇保養日期"),
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(VehicleMaintenance.MaintenanceDate), vm => vm.MaintenanceDate)
                        }
                    },
                    {
                        nameof(VehicleMaintenance.ServiceProvider),
                        new FieldDefinition<VehicleMaintenance>
                        {
                            PropertyName = nameof(VehicleMaintenance.ServiceProvider),
                            DisplayName = Dn("Field.ServiceProvider", "維修廠/服務商"),
                            FilterPlaceholder = Fp("Field.ServiceProvider", "輸入維修廠搜尋"),
                            TableOrder = 4,
                            FilterOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(VehicleMaintenance.ServiceProvider), vm => vm.ServiceProvider, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "保養紀錄欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("保養紀錄欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<VehicleMaintenance>>();
            }
        }
    }
}
