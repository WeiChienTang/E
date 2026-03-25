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
    /// 磅秤類型欄位配置
    /// </summary>
    public class ScaleTypeFieldConfiguration : BaseFieldConfiguration<ScaleType>
    {
        private readonly INotificationService? _notificationService;

        public ScaleTypeFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<ScaleType>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ScaleType>>
                {
                    {
                        nameof(ScaleType.Code),
                        new FieldDefinition<ScaleType>
                        {
                            PropertyName = nameof(ScaleType.Code),
                            DisplayName = Dn("Field.WasteTypeCode", "磅秤類型編號"),
                            FilterPlaceholder = Fp("Field.WasteTypeCode", "輸入編號搜尋"),
                            TableOrder = 1,
                            Width = "130px",
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ScaleType.Code), st => st.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(ScaleType.Name),
                        new FieldDefinition<ScaleType>
                        {
                            PropertyName = nameof(ScaleType.Name),
                            DisplayName = Dn("Field.WasteTypeName", "磅秤類型名稱"),
                            FilterPlaceholder = Fp("Field.WasteTypeName", "輸入名稱搜尋"),
                            TableOrder = 2,
                            Width = "150px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ScaleType.Name), st => st.Name)
                        }
                    },
                    {
                        nameof(ScaleType.Unit),
                        new FieldDefinition<ScaleType>
                        {
                            PropertyName = nameof(ScaleType.Unit),
                            DisplayName = Dn("Field.MeasurementUnit", "計量單位"),
                            FilterPlaceholder = Fp("Field.MeasurementUnit", "輸入單位搜尋"),
                            TableOrder = 3,
                            Width = "100px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ScaleType.Unit), st => st.Unit, allowNull: true)
                        }
                    },
                    {
                        nameof(ScaleType.Description),
                        new FieldDefinition<ScaleType>
                        {
                            PropertyName = nameof(ScaleType.Description),
                            DisplayName = Dn("Field.Description", "描述"),
                            FilterPlaceholder = Fp("Field.Description", "輸入描述搜尋"),
                            TableOrder = 4,
                            Width = "160px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ScaleType.Description), st => st.Description, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "磅秤類型欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("磅秤類型欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<ScaleType>>();
            }
        }
    }
}
