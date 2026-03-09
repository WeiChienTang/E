using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    public class PayrollPeriodFieldConfiguration : BaseFieldConfiguration<PayrollPeriod>
    {
        private readonly INotificationService? _notificationService;

        public PayrollPeriodFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PayrollPeriod>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<PayrollPeriod>>
                {
                    {
                        nameof(PayrollPeriod.Year),
                        new FieldDefinition<PayrollPeriod>
                        {
                            PropertyName = nameof(PayrollPeriod.Year),
                            DisplayName = Dn("Field.PayrollYear", "年份（民國）"),
                            FilterPlaceholder = Fp("Field.PayrollYear", "輸入年份搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(PayrollPeriod.Year), x => x.Year)
                        }
                    },
                    {
                        nameof(PayrollPeriod.Month),
                        new FieldDefinition<PayrollPeriod>
                        {
                            PropertyName = nameof(PayrollPeriod.Month),
                            DisplayName = Dn("Field.PayrollMonth", "月份"),
                            TableOrder = 2,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (PayrollPeriod)item;
                                builder.AddContent(0, $"{entity.Month} 月");
                            }
                        }
                    },
                    {
                        nameof(PayrollPeriod.PeriodStatus),
                        new FieldDefinition<PayrollPeriod>
                        {
                            PropertyName = nameof(PayrollPeriod.PeriodStatus),
                            DisplayName = Dn("Field.PeriodStatus", "週期狀態"),
                            TableOrder = 3,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (PayrollPeriod)item;
                                var (text, css) = entity.PeriodStatus switch
                                {
                                    PayrollPeriodStatus.Draft      => (L?["Payroll.PeriodDraft"].ToString()      ?? "草稿",   "badge bg-secondary"),
                                    PayrollPeriodStatus.Processing => (L?["Payroll.PeriodProcessing"].ToString() ?? "計算中", "badge bg-warning text-dark"),
                                    PayrollPeriodStatus.Closed     => (L?["Payroll.PeriodClosed"].ToString()     ?? "已關帳", "badge bg-success"),
                                    _ => ("—", "text-muted")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", css);
                                builder.AddContent(2, text);
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        "RecordCount",
                        new FieldDefinition<PayrollPeriod>
                        {
                            PropertyName = "RecordCount",
                            DisplayName = Dn("Field.PayrollRecordCount", "薪資筆數"),
                            TableOrder = 4,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (PayrollPeriod)item;
                                builder.AddContent(0, entity.Records.Count.ToString("N0"));
                            }
                        }
                    },
                    {
                        nameof(PayrollPeriod.ClosedAt),
                        new FieldDefinition<PayrollPeriod>
                        {
                            PropertyName = nameof(PayrollPeriod.ClosedAt),
                            DisplayName = Dn("Field.ClosedAt", "關帳時間"),
                            TableOrder = 5,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (PayrollPeriod)item;
                                if (entity.ClosedAt.HasValue)
                                    builder.AddContent(0, entity.ClosedAt.Value.ToString("yyyy-MM-dd HH:mm"));
                                else
                                    builder.AddContent(0, "—");
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType());
                });
                if (_notificationService != null)
                    _ = Task.Run(async () => await _notificationService.ShowErrorAsync("初始化薪資週期欄位配置時發生錯誤"));
                return new Dictionary<string, FieldDefinition<PayrollPeriod>>();
            }
        }
    }
}
