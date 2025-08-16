using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Helpers
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
                            DisplayName = "單位代碼",
                            FilterPlaceholder = "輸入單位代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 180px;",
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
                    },
                    {
                        nameof(Unit.Remarks),
                        new FieldDefinition<Unit>
                        {
                            PropertyName = nameof(Unit.Remarks),
                            DisplayName = "備註",
                            FilterPlaceholder = "輸入備註搜尋",
                            TableOrder = 3,
                            FilterOrder = 3,
                            ShowInFilter = false, // 不在篩選器中顯示
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Unit.Remarks), u => u.Remarks, allowNull: true)
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
