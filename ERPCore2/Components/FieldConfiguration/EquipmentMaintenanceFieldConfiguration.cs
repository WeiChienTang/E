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
    /// 設備保養維修記錄欄位配置
    /// </summary>
    public class EquipmentMaintenanceFieldConfiguration : BaseFieldConfiguration<EquipmentMaintenance>
    {
        private readonly INotificationService? _notificationService;

        public EquipmentMaintenanceFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<EquipmentMaintenance>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<EquipmentMaintenance>>
                {
                    {
                        nameof(EquipmentMaintenance.Code),
                        new FieldDefinition<EquipmentMaintenance>
                        {
                            PropertyName = nameof(EquipmentMaintenance.Code),
                            DisplayName = Dn("Field.MaintenanceRecordCode", "記錄編號"),
                            FilterPlaceholder = Fp("Field.MaintenanceRecordCode", "輸入記錄編號搜尋"),
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(EquipmentMaintenance.Code), em => em.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(EquipmentMaintenance.EquipmentId),
                        new FieldDefinition<EquipmentMaintenance>
                        {
                            PropertyName = "Equipment.Name",
                            FilterPropertyName = nameof(EquipmentMaintenance.EquipmentId),
                            DisplayName = Dn("Field.Equipment", "設備"),
                            FilterPlaceholder = Fp("Field.Equipment", "選擇設備"),
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(EquipmentMaintenance.EquipmentId), em => em.EquipmentId)
                        }
                    },
                    {
                        nameof(EquipmentMaintenance.MaintenanceDate),
                        new FieldDefinition<EquipmentMaintenance>
                        {
                            PropertyName = nameof(EquipmentMaintenance.MaintenanceDate),
                            DisplayName = Dn("Field.MaintenanceDate", "保養日期"),
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(EquipmentMaintenance.MaintenanceDate), em => em.MaintenanceDate)
                        }
                    },
                    {
                        nameof(EquipmentMaintenance.MaintenanceType),
                        new FieldDefinition<EquipmentMaintenance>
                        {
                            PropertyName = nameof(EquipmentMaintenance.MaintenanceType),
                            DisplayName = Dn("Field.MaintenanceType", "維修類型"),
                            TableOrder = 4,
                            FilterOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(EquipmentMaintenance.MaintenanceType), em => (int)em.MaintenanceType)
                        }
                    },
                    {
                        nameof(EquipmentMaintenance.ServiceProvider),
                        new FieldDefinition<EquipmentMaintenance>
                        {
                            PropertyName = nameof(EquipmentMaintenance.ServiceProvider),
                            DisplayName = Dn("Field.ServiceProvider", "服務廠商"),
                            FilterPlaceholder = Fp("Field.ServiceProvider", "輸入廠商搜尋"),
                            TableOrder = 5,
                            FilterOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(EquipmentMaintenance.ServiceProvider), em => em.ServiceProvider, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "保養維修記錄欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("保養維修記錄欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<EquipmentMaintenance>>();
            }
        }
    }
}
