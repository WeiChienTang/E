using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 尺寸欄位配置
    /// </summary>
    public class SizeFieldConfiguration : BaseFieldConfiguration<Size>
    {
        private readonly INotificationService? _notificationService;
        
        public SizeFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Size>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Size>>
                {
                    {
                        nameof(Size.Code),
                        new FieldDefinition<Size>
                        {
                            PropertyName = nameof(Size.Code),
                            DisplayName = "尺寸代碼",
                            FilterPlaceholder = "輸入尺寸代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Size.Code), s => s.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(Size.Name),
                        new FieldDefinition<Size>
                        {
                            PropertyName = nameof(Size.Name),
                            DisplayName = "尺寸名稱",
                            FilterPlaceholder = "輸入尺寸名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Size.Name), s => s.Name)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "尺寸欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("尺寸欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<Size>>();
            }
        }
    }
}
