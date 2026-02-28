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
                            DisplayName = Dn("Field.PositionCode", "職位編號"),
                            FilterPlaceholder = Fp("Field.PositionCode", "輸入職位編號搜尋"),
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(EmployeePosition.Code), p => p.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(EmployeePosition.Name),
                        new FieldDefinition<EmployeePosition>
                        {
                            PropertyName = nameof(EmployeePosition.Name),
                            DisplayName = Dn("Field.PositionName", "職位名稱"),
                            FilterPlaceholder = Fp("Field.PositionName", "輸入職位名稱搜尋"),
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


