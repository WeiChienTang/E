using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 身分欄位配置
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
                            DisplayName = "身分代碼",
                            FilterPlaceholder = "輸入身分代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 180px;",
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
                            DisplayName = "身分名稱",
                            FilterPlaceholder = "輸入身分名稱搜尋",
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化身分欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化身分欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Role>>();
            }
        }
    }
}
