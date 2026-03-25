using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.FieldConfiguration;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;

namespace ERPCore2.Helpers.FieldConfiguration
{
    /// <summary>
    /// 業績目標欄位配置
    /// </summary>
    public class SalesTargetFieldConfiguration : BaseFieldConfiguration<SalesTarget>
    {
        private readonly INotificationService? _notificationService;

        public SalesTargetFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<SalesTarget>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<SalesTarget>>
                {
                    {
                        nameof(SalesTarget.Year),
                        new FieldDefinition<SalesTarget>
                        {
                            PropertyName = nameof(SalesTarget.Year),
                            DisplayName = Dn("SalesTarget.Year", "年度"),
                            TableOrder = 1,
                            Width = "90px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesTarget.Year), t => t.Year.ToString())
                        }
                    },
                    {
                        nameof(SalesTarget.Month),
                        new FieldDefinition<SalesTarget>
                        {
                            PropertyName = nameof(SalesTarget.Month),
                            DisplayName = Dn("SalesTarget.Month", "月份"),
                            TableOrder = 2,
                            Width = "80px",
                            CustomTemplate = (data) => (builder) =>
                            {
                                if (data is SalesTarget t)
                                    builder.AddContent(0, t.Month.HasValue ? $"{t.Month:D2}" : (L?["SalesTarget.AllYear"].ToString() ?? "全年"));
                            }
                        }
                    },
                    {
                        nameof(SalesTarget.SalespersonId),
                        new FieldDefinition<SalesTarget>
                        {
                            PropertyName = nameof(SalesTarget.SalespersonId),
                            DisplayName = Dn("Field.SalesPerson", "業務員"),
                            TableOrder = 3,
                            Width = "110px",
                            CustomTemplate = (data) => (builder) =>
                            {
                                if (data is SalesTarget t)
                                    builder.AddContent(0, t.SalespersonId.HasValue
                                        ? (t.Salesperson?.Name ?? "")
                                        : (L?["SalesTarget.CompanyTotal"].ToString() ?? "公司整體"));
                            }
                        }
                    },
                    {
                        nameof(SalesTarget.TargetAmount),
                        new FieldDefinition<SalesTarget>
                        {
                            PropertyName = nameof(SalesTarget.TargetAmount),
                            DisplayName = Dn("SalesTarget.TargetAmount", "目標金額"),
                            TableOrder = 4,
                            Width = "120px",
                            CustomTemplate = (data) => (builder) =>
                            {
                                if (data is SalesTarget t)
                                    builder.AddContent(0, t.TargetAmount.ToString("N0"));
                            }
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
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "取得業績目標欄位定義時發生錯誤");
                        if (_notificationService != null)
                            await _notificationService.ShowErrorAsync("載入欄位設定失敗，請稍後再試");
                    }
                    catch { }
                });
                return new Dictionary<string, FieldDefinition<SalesTarget>>();
            }
        }
    }
}
