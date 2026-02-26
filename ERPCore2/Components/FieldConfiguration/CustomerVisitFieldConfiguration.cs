using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Models.Enums;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 客戶拜訪紀錄欄位配置
    /// </summary>
    public class CustomerVisitFieldConfiguration : BaseFieldConfiguration<CustomerVisit>
    {
        private readonly INotificationService? _notificationService;

        public CustomerVisitFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<CustomerVisit>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<CustomerVisit>>
                {
                    {
                        nameof(CustomerVisit.VisitDate),
                        new FieldDefinition<CustomerVisit>
                        {
                            PropertyName = nameof(CustomerVisit.VisitDate),
                            DisplayName = "拜訪日期",
                            FilterPlaceholder = "選擇拜訪日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(CustomerVisit.VisitDate), v => v.VisitDate)
                        }
                    },
                    {
                        nameof(CustomerVisit.CustomerId),
                        new FieldDefinition<CustomerVisit>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(CustomerVisit.CustomerId),
                            DisplayName = "客戶",
                            FilterPlaceholder = "輸入客戶名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(CustomerVisit.CustomerId), v => v.CustomerId)
                        }
                    },
                    {
                        nameof(CustomerVisit.VisitType),
                        new FieldDefinition<CustomerVisit>
                        {
                            PropertyName = nameof(CustomerVisit.VisitType),
                            DisplayName = "拜訪方式",
                            ShowInFilter = false,
                            TableOrder = 3,
                            CustomTemplate = item =>
                            {
                                var visit = item as CustomerVisit;
                                var text = visit?.VisitType switch
                                {
                                    VisitType.Phone => "電話",
                                    VisitType.OnSite => "現場拜訪",
                                    VisitType.VideoConference => "視訊",
                                    VisitType.Email => "Email",
                                    VisitType.Other => "其他",
                                    _ => visit?.VisitType.ToString() ?? "-"
                                };
                                return builder => builder.AddContent(0, text ?? "-");
                            },
                            FilterFunction = (model, query) => query
                        }
                    },
                    {
                        nameof(CustomerVisit.EmployeeId),
                        new FieldDefinition<CustomerVisit>
                        {
                            PropertyName = "Employee.Name",
                            FilterPropertyName = nameof(CustomerVisit.EmployeeId),
                            DisplayName = "業務人員",
                            FilterPlaceholder = "選擇業務人員",
                            TableOrder = 4,
                            FilterOrder = 3,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(CustomerVisit.EmployeeId), v => v.EmployeeId ?? 0)
                        }
                    },
                    {
                        nameof(CustomerVisit.Purpose),
                        new FieldDefinition<CustomerVisit>
                        {
                            PropertyName = nameof(CustomerVisit.Purpose),
                            DisplayName = "拜訪目的",
                            FilterPlaceholder = "輸入拜訪目的搜尋",
                            TableOrder = 5,
                            FilterOrder = 4,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CustomerVisit.Purpose), v => v.Purpose, allowNull: true)
                        }
                    },
                    {
                        nameof(CustomerVisit.Result),
                        new FieldDefinition<CustomerVisit>
                        {
                            PropertyName = nameof(CustomerVisit.Result),
                            DisplayName = "結果摘要",
                            ShowInFilter = false,
                            TableOrder = 6,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => query
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "客戶拜訪紀錄欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("客戶拜訪紀錄欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<CustomerVisit>>();
            }
        }
    }
}
