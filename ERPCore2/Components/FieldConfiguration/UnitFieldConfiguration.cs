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
    /// 單位欄位配置
    /// </summary>
    public class UnitFieldConfiguration : BaseFieldConfiguration<Unit>
    {
        private readonly INotificationService? _notificationService;
        
        public UnitFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Unit>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Unit>>
                {
                    {
                        nameof(Unit.Code),
                        new FieldDefinition<Unit>
                        {
                            PropertyName = nameof(Unit.Code),
                            DisplayName = "單位編號",
                            FilterPlaceholder = "輸入單位編號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Code", u => u.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(Unit.Name),
                        new FieldDefinition<Unit>
                        {
                            PropertyName = nameof(Unit.Name),
                            DisplayName = "單位名稱",
                            FilterPlaceholder = "輸入單位名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Name", u => u.Name)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "單位欄位配置初始化失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("單位欄位配置初始化失敗，已使用預設配置");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<Unit>>();
            }
        }
    }
}


