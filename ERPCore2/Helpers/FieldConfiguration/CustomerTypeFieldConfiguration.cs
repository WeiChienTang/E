using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 客戶類型欄位配置
    /// </summary>
    public class CustomerTypeFieldConfiguration : BaseFieldConfiguration<CustomerType>
    {
        private readonly INotificationService? _notificationService;
        
        public CustomerTypeFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<CustomerType>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<CustomerType>>
                {
                    {
                        nameof(CustomerType.Code),
                        new FieldDefinition<CustomerType>
                        {
                            PropertyName = nameof(CustomerType.Code),
                            DisplayName = "類型代碼",
                            FilterPlaceholder = "輸入類型代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CustomerType.Code), ct => ct.Code)
                        }
                    },
                    {
                        nameof(CustomerType.TypeName),
                        new FieldDefinition<CustomerType>
                        {
                            PropertyName = nameof(CustomerType.TypeName),
                            DisplayName = "類型名稱",
                            FilterPlaceholder = "輸入類型名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CustomerType.TypeName), ct => ct.TypeName)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化客戶類型欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化客戶類型欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<CustomerType>>();
            }
        }
    }
}
