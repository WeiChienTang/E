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
                            DisplayName = Dn("Field.SizeCode", "尺寸編號"),
                            FilterPlaceholder = Fp("Field.SizeCode", "輸入尺寸編號搜尋"),
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Size.Code), s => s.Code, allowNull: true)
                        }
                    },
                    {
                        nameof(Size.Name),
                        new FieldDefinition<Size>
                        {
                            PropertyName = nameof(Size.Name),
                            DisplayName = Dn("Field.SizeName", "尺寸名稱"),
                            FilterPlaceholder = Fp("Field.SizeName", "輸入尺寸名稱搜尋"),
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


