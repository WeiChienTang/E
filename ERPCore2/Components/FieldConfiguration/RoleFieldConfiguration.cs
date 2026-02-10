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
    /// 權限組欄位配置
    /// </summary>
    public class RoleFieldConfiguration : BaseFieldConfiguration<Role>
    {
        private readonly INotificationService? _notificationService;
        
        public RoleFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Role>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Role>>
                {
                    {
                        nameof(Role.Code),
                        new FieldDefinition<Role>
                        {
                            PropertyName = nameof(Role.Code),
                            DisplayName = "權限組編號",
                            FilterPlaceholder = "輸入權限組編號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Role.Code), r => r.Code)
                        }
                    },
                    {
                        nameof(Role.Name),
                        new FieldDefinition<Role>
                        {
                            PropertyName = nameof(Role.Name),
                            FilterPropertyName = "RoleName", // 保持原有的篩選器屬性名稱
                            DisplayName = "權限組名稱",
                            FilterPlaceholder = "輸入權限組名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "RoleName", r => r.Name)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化權限組欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化權限組欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Role>>();
            }
        }
    }
}


