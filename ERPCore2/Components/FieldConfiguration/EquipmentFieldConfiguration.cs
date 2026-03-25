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
    /// 設備欄位配置
    /// </summary>
    public class EquipmentFieldConfiguration : BaseFieldConfiguration<Equipment>
    {
        private readonly INotificationService? _notificationService;

        public EquipmentFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<Equipment>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Equipment>>
                {
                    {
                        nameof(Equipment.Code),
                        new FieldDefinition<Equipment>
                        {
                            PropertyName = nameof(Equipment.Code),
                            DisplayName = Dn("Field.EquipmentCode", "設備編號"),
                            FilterPlaceholder = Fp("Field.EquipmentCode", "輸入設備編號搜尋"),
                            TableOrder = 1,
                            Width = "120px",
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Equipment.Code), e => e.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(Equipment.Name),
                        new FieldDefinition<Equipment>
                        {
                            PropertyName = nameof(Equipment.Name),
                            DisplayName = Dn("Field.EquipmentName", "設備名稱"),
                            FilterPlaceholder = Fp("Field.EquipmentName", "輸入設備名稱搜尋"),
                            TableOrder = 2,
                            Width = "150px",
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Equipment.Name), e => e.Name)
                        }
                    },
                    {
                        nameof(Equipment.EquipmentCategoryId),
                        new FieldDefinition<Equipment>
                        {
                            PropertyName = "EquipmentCategory.Name",
                            FilterPropertyName = nameof(Equipment.EquipmentCategoryId),
                            DisplayName = Dn("Field.EquipmentCategory", "設備類別"),
                            FilterPlaceholder = Fp("Field.EquipmentCategory", "選擇設備類別"),
                            NullDisplayText = Nd("Label.Uncategorized", "未分類"),
                            TableOrder = 3,
                            Width = "120px",
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Equipment.EquipmentCategoryId), e => e.EquipmentCategoryId)
                        }
                    },
                    {
                        nameof(Equipment.Brand),
                        new FieldDefinition<Equipment>
                        {
                            PropertyName = nameof(Equipment.Brand),
                            DisplayName = Dn("Field.Brand", "品牌"),
                            FilterPlaceholder = Fp("Field.Brand", "輸入品牌搜尋"),
                            TableOrder = 4,
                            Width = "100px",
                            FilterOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Equipment.Brand), e => e.Brand, allowNull: true)
                        }
                    },
                    {
                        nameof(Equipment.Location),
                        new FieldDefinition<Equipment>
                        {
                            PropertyName = nameof(Equipment.Location),
                            DisplayName = Dn("Field.Location", "放置地點"),
                            FilterPlaceholder = Fp("Field.Location", "輸入地點搜尋"),
                            TableOrder = 5,
                            Width = "130px",
                            FilterOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Equipment.Location), e => e.Location, allowNull: true)
                        }
                    },
                    {
                        nameof(Equipment.NextMaintenanceDate),
                        new FieldDefinition<Equipment>
                        {
                            PropertyName = nameof(Equipment.NextMaintenanceDate),
                            DisplayName = Dn("Field.NextMaintenanceDate", "下次保養日期"),
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 6,
                            Width = "120px",
                            FilterOrder = 6,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableDateRangeFilter(
                                model, query, nameof(Equipment.NextMaintenanceDate), e => e.NextMaintenanceDate)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "設備欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("設備欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<Equipment>>();
            }
        }
    }
}
