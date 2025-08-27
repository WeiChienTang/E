using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 權限欄位配置
    /// </summary>
    public class PermissionFieldConfiguration : BaseFieldConfiguration<Permission>
    {
        private readonly INotificationService? _notificationService;
        
        public PermissionFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Permission>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Permission>>
                {
                    {
                        nameof(Permission.Code),
                        new FieldDefinition<Permission>
                        {
                            PropertyName = nameof(Permission.Code),
                            DisplayName = "權限代碼",
                            FilterPlaceholder = "輸入權限代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Permission.Code), p => p.Code)
                        }
                    },
                    {
                        nameof(Permission.Name),
                        new FieldDefinition<Permission>
                        {
                            PropertyName = nameof(Permission.Name),
                            DisplayName = "權限名稱",
                            FilterPlaceholder = "輸入權限名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Permission.Name), p => p.Name)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化權限欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化權限欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Permission>>();
            }
        }
    }
}
