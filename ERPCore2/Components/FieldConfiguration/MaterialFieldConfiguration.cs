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
    /// 材質欄位配置
    /// </summary>
    public class MaterialFieldConfiguration : BaseFieldConfiguration<Material>
    {
        private readonly INotificationService? _notificationService;
        
        public MaterialFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Material>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Material>>
                {
                    {
                        nameof(Material.Code),
                        new FieldDefinition<Material>
                        {
                            PropertyName = nameof(Material.Code),
                            DisplayName = Dn("Field.MaterialCode", "材質編號"),
                            FilterPlaceholder = Fp("Field.MaterialCode", "輸入材質編號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Code", m => m.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(Material.Name),
                        new FieldDefinition<Material>
                        {
                            PropertyName = nameof(Material.Name),
                            DisplayName = Dn("Field.MaterialName", "材質名稱"),
                            FilterPlaceholder = Fp("Field.MaterialName", "輸入材質名稱搜尋"),
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Name", m => m.Name)
                        }
                    },
                    {
                        nameof(Material.Description),
                        new FieldDefinition<Material>
                        {
                            PropertyName = nameof(Material.Description),
                            DisplayName = Dn("Field.Description", "描述"),
                            FilterPlaceholder = Fp("Field.Description", "輸入描述搜尋"),
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Description", m => m.Description, allowNull: true)
                        }
                    },
                    {
                        nameof(Material.Remarks),
                        new FieldDefinition<Material>
                        {
                            PropertyName = nameof(Material.Remarks),
                            DisplayName = Dn("Field.Remarks", "備註"),
                            TableOrder = 4,
                            ShowInFilter = false, // 備註不加入篩選器
                            FilterFunction = (model, query) => query // 不需要篩選邏輯
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "材質欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("材質欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<Material>>();
            }
        }
    }
}


