using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 員工職位欄位配置
    /// </summary>
    public class EmployeePositionFieldConfiguration : BaseFieldConfiguration<EmployeePosition>
    {
        private readonly INotificationService? _notificationService;
        
        public EmployeePositionFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<EmployeePosition>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<EmployeePosition>>
                {
                    {
                        nameof(EmployeePosition.Code),
                        new FieldDefinition<EmployeePosition>
                        {
                            PropertyName = nameof(EmployeePosition.Code),
                            DisplayName = "職位代碼",
                            FilterPlaceholder = "輸入職位代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(EmployeePosition.Code), p => p.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(EmployeePosition.Name),
                        new FieldDefinition<EmployeePosition>
                        {
                            PropertyName = nameof(EmployeePosition.Name),
                            DisplayName = "職位名稱",
                            FilterPlaceholder = "輸入職位名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(EmployeePosition.Name), p => p.Name)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化員工職位欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化員工職位欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<EmployeePosition>>();
            }
        }
    }
}
