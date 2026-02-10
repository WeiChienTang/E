using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 付款方式欄位配置類別
    /// </summary>
    public class PaymentMethodFieldConfiguration : BaseFieldConfiguration<PaymentMethod>
    {
        private readonly INotificationService? _notificationService;

        public PaymentMethodFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PaymentMethod>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<PaymentMethod>>
                {
                    {
                        nameof(PaymentMethod.Code),
                        new FieldDefinition<PaymentMethod>
                        {
                            PropertyName = nameof(PaymentMethod.Code),
                            DisplayName = "付款方式編號",
                            FilterPlaceholder = "輸入付款方式編號搜尋",
                            TableOrder = 0,
                            ColumnType = ColumnDataType.Text,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PaymentMethod.Code), pm => pm.Code)
                        }
                    },
                    {
                        nameof(PaymentMethod.Name),
                        new FieldDefinition<PaymentMethod>
                        {
                            PropertyName = nameof(PaymentMethod.Name),
                            DisplayName = "付款方式名稱",
                            FilterPlaceholder = "輸入付款方式名稱搜尋",
                            TableOrder = 1,
                            ColumnType = ColumnDataType.Text,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PaymentMethod.Name), pm => pm.Name)
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                // 非同步錯誤處理 - 避免阻塞主執行緒
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), new { 
                            ServiceType = GetType().Name,
                            Method = nameof(GetFieldDefinitions)
                        });
                        
                        if (_notificationService != null)
                            await _notificationService.ShowErrorAsync("載入付款方式欄位配置時發生錯誤");
                    }
                    catch
                    {
                        // 避免錯誤處理本身產生例外
                    }
                });

                // 回傳安全的預設值
                return new Dictionary<string, FieldDefinition<PaymentMethod>>();
            }
        }
    }
}
