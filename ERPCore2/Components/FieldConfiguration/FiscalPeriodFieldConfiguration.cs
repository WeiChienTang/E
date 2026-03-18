using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using ERPCore2.Services;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 會計期間欄位配置
    /// </summary>
    public class FiscalPeriodFieldConfiguration : BaseFieldConfiguration<FiscalPeriod>
    {
        private readonly INotificationService? _notificationService;

        public FiscalPeriodFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<FiscalPeriod>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<FiscalPeriod>>
                {
                    {
                        nameof(FiscalPeriod.FiscalYear),
                        new FieldDefinition<FiscalPeriod>
                        {
                            PropertyName = nameof(FiscalPeriod.FiscalYear),
                            DisplayName = Dn("Field.FiscalYear", "會計年度"),
                            FilterPlaceholder = Fp("Field.FiscalYear", "輸入會計年度搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) =>
                            {
                                var v = model.GetFilterValue(nameof(FiscalPeriod.FiscalYear))?.ToString();
                                if (!string.IsNullOrWhiteSpace(v) && int.TryParse(v, out var year))
                                    return query.Where(f => f.FiscalYear == year);
                                return query;
                            }
                        }
                    },
                    {
                        nameof(FiscalPeriod.PeriodNumber),
                        new FieldDefinition<FiscalPeriod>
                        {
                            PropertyName = nameof(FiscalPeriod.PeriodNumber),
                            DisplayName = Dn("Field.PeriodNumber", "期間編號"),
                            FilterPlaceholder = Fp("Field.PeriodNumber", "輸入期間編號搜尋"),
                            TableOrder = 2,
                            FilterFunction = (model, query) =>
                            {
                                var v = model.GetFilterValue(nameof(FiscalPeriod.PeriodNumber))?.ToString();
                                if (!string.IsNullOrWhiteSpace(v) && int.TryParse(v, out var num))
                                    return query.Where(f => f.PeriodNumber == num);
                                return query;
                            }
                        }
                    },
                    {
                        nameof(FiscalPeriod.StartDate),
                        new FieldDefinition<FiscalPeriod>
                        {
                            PropertyName = nameof(FiscalPeriod.StartDate),
                            DisplayName = Dn("Field.StartDate", "開始日期"),
                            TableOrder = 3,
                            ShowInFilter = false,
                            FilterFunction = (model, query) => query
                        }
                    },
                    {
                        nameof(FiscalPeriod.EndDate),
                        new FieldDefinition<FiscalPeriod>
                        {
                            PropertyName = nameof(FiscalPeriod.EndDate),
                            DisplayName = Dn("Field.EndDate", "結束日期"),
                            TableOrder = 4,
                            ShowInFilter = false,
                            FilterFunction = (model, query) => query
                        }
                    },
                    {
                        nameof(FiscalPeriod.PeriodStatus),
                        new FieldDefinition<FiscalPeriod>
                        {
                            PropertyName = nameof(FiscalPeriod.PeriodStatus),
                            DisplayName = Dn("Field.PeriodStatus", "期間狀態"),
                            TableOrder = 5,
                            FilterType = SearchFilterType.Select,
                            Options = new List<SelectOption>
                            {
                                new() { Text = L?["FiscalPeriodStatus.Open"].ToString() ?? "開放中",    Value = ((int)FiscalPeriodStatus.Open).ToString() },
                                new() { Text = L?["FiscalPeriodStatus.Closed"].ToString() ?? "已關帳",  Value = ((int)FiscalPeriodStatus.Closed).ToString() },
                                new() { Text = L?["FiscalPeriodStatus.Locked"].ToString() ?? "已鎖定",  Value = ((int)FiscalPeriodStatus.Locked).ToString() }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var v = model.GetFilterValue(nameof(FiscalPeriod.PeriodStatus))?.ToString();
                                if (!string.IsNullOrWhiteSpace(v) && int.TryParse(v, out var statusVal) &&
                                    Enum.IsDefined(typeof(FiscalPeriodStatus), statusVal))
                                {
                                    var status = (FiscalPeriodStatus)statusVal;
                                    return query.Where(f => f.PeriodStatus == status);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var fp = (FiscalPeriod)item;
                                var (cssClass, label) = fp.PeriodStatus switch
                                {
                                    FiscalPeriodStatus.Open   => ("badge bg-success",   L?["FiscalPeriodStatus.Open"].ToString()   ?? "開放中"),
                                    FiscalPeriodStatus.Closed => ("badge bg-secondary", L?["FiscalPeriodStatus.Closed"].ToString() ?? "已關帳"),
                                    FiscalPeriodStatus.Locked => ("badge bg-dark",      L?["FiscalPeriodStatus.Locked"].ToString() ?? "已鎖定"),
                                    _ => ("badge bg-secondary", fp.PeriodStatus.ToString())
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", cssClass);
                                builder.AddContent(2, label);
                                builder.CloseElement();
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType());
                return new Dictionary<string, FieldDefinition<FiscalPeriod>>();
            }
        }
    }
}
