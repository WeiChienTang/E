using ERPCore2.Data.Entities;
using ERPCore2.FieldConfiguration;
using ERPCore2.Helpers;
using ERPCore2.Services;

namespace ERPCore2.Helpers.FieldConfiguration
{
    /// <summary>
    /// 進貨退出原因欄位配置
    /// </summary>
    public class PurchaseReturnReasonFieldConfiguration : BaseFieldConfiguration<PurchaseReturnReason>
    {
        private readonly INotificationService? _notificationService;

        public PurchaseReturnReasonFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PurchaseReturnReason>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<PurchaseReturnReason>>
                {
                    {
                        nameof(PurchaseReturnReason.Code),
                        new FieldDefinition<PurchaseReturnReason>
                        {
                            PropertyName = nameof(PurchaseReturnReason.Code),
                            DisplayName = "原因編號",
                            FilterPlaceholder = "輸入原因編號搜尋",
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PurchaseReturnReason.Code), r => r.Code)
                        }
                    },
                    {
                        nameof(PurchaseReturnReason.Name),
                        new FieldDefinition<PurchaseReturnReason>
                        {
                            PropertyName = nameof(PurchaseReturnReason.Name),
                            DisplayName = "原因名稱",
                            FilterPlaceholder = "輸入原因名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PurchaseReturnReason.Name), r => r.Name)
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "取得進貨退出原因欄位定義時發生錯誤");
                        if (_notificationService != null)
                            await _notificationService.ShowErrorAsync("載入欄位設定失敗，請稍後再試");
                    }
                    catch
                    {
                        // 靜默處理錯誤記錄的失敗，防止無限遞迴
                    }
                });

                return new Dictionary<string, FieldDefinition<PurchaseReturnReason>>();
            }
        }
    }
}
