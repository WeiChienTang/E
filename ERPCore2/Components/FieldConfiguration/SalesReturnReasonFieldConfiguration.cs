using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.FieldConfiguration;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Components.Shared.PageTemplate;

namespace ERPCore2.Helpers.FieldConfiguration
{
    /// <summary>
    /// 銷貨退貨原因欄位配置
    /// </summary>
    public class SalesReturnReasonFieldConfiguration : BaseFieldConfiguration<SalesReturnReason>
    {
        private readonly INotificationService? _notificationService;

        public SalesReturnReasonFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<SalesReturnReason>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<SalesReturnReason>>
                {
                    {
                        nameof(SalesReturnReason.Code),
                        new FieldDefinition<SalesReturnReason>
                        {
                            PropertyName = nameof(SalesReturnReason.Code),
                            DisplayName = "原因編號",
                            FilterPlaceholder = "輸入原因編號搜尋",
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesReturnReason.Code), r => r.Code)
                        }
                    },
                    {
                        nameof(SalesReturnReason.Name),
                        new FieldDefinition<SalesReturnReason>
                        {
                            PropertyName = nameof(SalesReturnReason.Name),
                            DisplayName = "原因名稱",
                            FilterPlaceholder = "輸入原因名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesReturnReason.Name), r => r.Name)
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                // 使用 Task.Run 來處理非同步錯誤記錄，避免阻塞同步方法
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "取得銷貨退貨原因欄位定義時發生錯誤");
                        if (_notificationService != null)
                            await _notificationService.ShowErrorAsync("載入欄位設定失敗，請稍後再試");
                    }
                    catch
                    {
                        // 靜默處理錯誤記錄的失敗，防止無限遞迴
                    }
                });

                // 回傳安全的預設值
                return new Dictionary<string, FieldDefinition<SalesReturnReason>>();
            }
        }

        /// <summary>
    }
}


