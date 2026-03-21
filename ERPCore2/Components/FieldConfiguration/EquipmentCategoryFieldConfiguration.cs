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
    /// 設備類別欄位配置
    /// </summary>
    public class EquipmentCategoryFieldConfiguration : BaseFieldConfiguration<EquipmentCategory>
    {
        private readonly INotificationService? _notificationService;

        public EquipmentCategoryFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<EquipmentCategory>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<EquipmentCategory>>
                {
                    {
                        nameof(EquipmentCategory.Code),
                        new FieldDefinition<EquipmentCategory>
                        {
                            PropertyName = nameof(EquipmentCategory.Code),
                            DisplayName = Dn("Field.EquipmentCategoryCode", "類別編號"),
                            FilterPlaceholder = Fp("Field.EquipmentCategoryCode", "輸入類別編號搜尋"),
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(EquipmentCategory.Code), ec => ec.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(EquipmentCategory.Name),
                        new FieldDefinition<EquipmentCategory>
                        {
                            PropertyName = nameof(EquipmentCategory.Name),
                            DisplayName = Dn("Field.EquipmentCategoryName", "類別名稱"),
                            FilterPlaceholder = Fp("Field.EquipmentCategoryName", "輸入類別名稱搜尋"),
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(EquipmentCategory.Name), ec => ec.Name)
                        }
                    },
                    {
                        nameof(EquipmentCategory.Description),
                        new FieldDefinition<EquipmentCategory>
                        {
                            PropertyName = nameof(EquipmentCategory.Description),
                            DisplayName = Dn("Field.Description", "描述"),
                            FilterPlaceholder = Fp("Field.Description", "輸入描述搜尋"),
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(EquipmentCategory.Description), ec => ec.Description, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "設備類別欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("設備類別欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<EquipmentCategory>>();
            }
        }
    }
}
