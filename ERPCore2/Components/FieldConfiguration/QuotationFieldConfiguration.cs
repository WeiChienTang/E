using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Tables;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 報價單欄位配置
    /// </summary>
    public class QuotationFieldConfiguration : BaseFieldConfiguration<Quotation>
    {
        private readonly List<Customer> _customers;
        private readonly List<Employee> _employees;
        private readonly INotificationService? _notificationService;
        
        public QuotationFieldConfiguration(
            List<Customer> customers, 
            List<Employee> employees,
            INotificationService? notificationService = null)
        {
            _customers = customers;
            _employees = employees;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Quotation>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Quotation>>
                {
                    {
                        nameof(Quotation.QuotationNumber),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.QuotationNumber),
                            DisplayName = "報價單號",
                            FilterPlaceholder = "輸入報價單號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Quotation.QuotationNumber), q => q.QuotationNumber)
                        }
                    },
                    {
                        nameof(Quotation.QuotationDate),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.QuotationDate),
                            DisplayName = "報價日期",
                            ColumnType = ColumnDataType.Date,
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 2,
                            FilterOrder = 2,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(Quotation.QuotationDate), q => q.QuotationDate)
                        }
                    },
                    {
                        nameof(Quotation.CustomerId),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(Quotation.CustomerId),
                            DisplayName = "客戶",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            Options = _customers.Select(c => new SelectOption 
                            { 
                                Text = c.CompanyName, 
                                Value = c.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(Quotation.CustomerId), q => q.CustomerId)
                        }
                    },
                    {
                        nameof(Quotation.EmployeeId),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = "Employee.Name",
                            FilterPropertyName = nameof(Quotation.EmployeeId),
                            DisplayName = "業務人員",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 120px;",
                            NullDisplayText = "未指派",
                            Options = _employees.Select(e => new SelectOption 
                            { 
                                Text = e.Name ?? "", 
                                Value = e.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Quotation.EmployeeId), q => q.EmployeeId)
                        }
                    },
                    {
                        nameof(Quotation.TotalAmount),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.TotalAmount),
                            DisplayName = "報價金額",
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 130px; text-align: right;",
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(Quotation.ValidUntilDate),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.ValidUntilDate),
                            DisplayName = "有效期限",
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 6,
                            FilterOrder = 6,
                            HeaderStyle = "width: 120px;",
                            NullDisplayText = "-",
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(Quotation.QuotationStatus),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.QuotationStatus),
                            DisplayName = "報價狀態",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 7,
                            FilterOrder = 7,
                            HeaderStyle = "width: 110px;",
                            Options = Enum.GetValues<QuotationStatus>().Select(s => new SelectOption
                            {
                                Text = GetEnumDisplayName(s),
                                Value = ((int)s).ToString()
                            }).ToList(),
                            CustomTemplate = item => builder =>
                            {
                                var quotation = (Quotation)item;
                                var badgeClass = GetStatusBadgeClass(quotation.QuotationStatus);
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", $"badge {badgeClass}");
                                builder.AddContent(2, GetEnumDisplayName(quotation.QuotationStatus));
                                builder.CloseElement();
                            },
                            FilterFunction = (model, query) => {
                                var statusValue = model.GetFilterValue(nameof(Quotation.QuotationStatus))?.ToString();
                                if (!string.IsNullOrWhiteSpace(statusValue) && int.TryParse(statusValue, out int statusInt))
                                {
                                    query = query.Where(q => (int)q.QuotationStatus == statusInt);
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(Quotation.IsApproved),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.IsApproved),
                            DisplayName = "核准狀態",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 8,
                            FilterOrder = 8,
                            HeaderStyle = "width: 100px;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "全部", Value = "" },
                                new SelectOption { Text = "已核准", Value = "true" },
                                new SelectOption { Text = "未核准", Value = "false" }
                            },
                            CustomTemplate = item => builder =>
                            {
                                var quotation = (Quotation)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", quotation.IsApproved ? "badge bg-success" : "badge bg-warning");
                                builder.AddContent(2, quotation.IsApproved ? "已核准" : "待核准");
                                builder.CloseElement();
                            },
                            FilterFunction = (model, query) => {
                                var value = model.GetFilterValue(nameof(Quotation.IsApproved))?.ToString();
                                if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool boolValue))
                                {
                                    query = query.Where(q => q.IsApproved == boolValue);
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(Quotation.IsConverted),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.IsConverted),
                            DisplayName = "轉單狀態",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 9,
                            FilterOrder = 9,
                            HeaderStyle = "width: 100px;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "全部", Value = "" },
                                new SelectOption { Text = "已轉單", Value = "true" },
                                new SelectOption { Text = "未轉單", Value = "false" }
                            },
                            CustomTemplate = item => builder =>
                            {
                                var quotation = (Quotation)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", quotation.IsConverted ? "badge bg-info" : "badge bg-secondary");
                                builder.AddContent(2, quotation.IsConverted ? "已轉單" : "未轉單");
                                builder.CloseElement();
                            },
                            FilterFunction = (model, query) => {
                                var value = model.GetFilterValue(nameof(Quotation.IsConverted))?.ToString();
                                if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool boolValue))
                                {
                                    query = query.Where(q => q.IsConverted == boolValue);
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(Quotation.Description),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.Description),
                            DisplayName = "說明",
                            FilterPlaceholder = "輸入說明搜尋",
                            TableOrder = 10,
                            FilterOrder = 10,
                            NullDisplayText = "-",
                            CustomTemplate = item => builder =>
                            {
                                var quotation = (Quotation)item;
                                var desc = quotation.Description;
                                var displayText = !string.IsNullOrEmpty(desc) 
                                    ? (desc.Length > 30 ? desc.Substring(0, 30) + "..." : desc)
                                    : "-";
                                builder.AddContent(0, displayText);
                            },
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Quotation.Description), q => q.Description, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), 
                        additionalData: new { 
                            CustomersCount = _customers?.Count ?? 0,
                            EmployeesCount = _employees?.Count ?? 0
                        });
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化報價單欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Quotation>>();
            }
        }

        /// <summary>
        /// 取得枚舉的顯示名稱
        /// </summary>
        private static string GetEnumDisplayName(Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetField(enumValue.ToString())?
                .GetCustomAttribute<DisplayAttribute>();
            
            return displayAttribute?.Name ?? enumValue.ToString();
        }

        /// <summary>
        /// 取得報價狀態徽章樣式類別
        /// </summary>
        private static string GetStatusBadgeClass(QuotationStatus status)
        {
            return status switch
            {
                QuotationStatus.Draft => "bg-secondary",
                QuotationStatus.Submitted => "bg-primary",
                QuotationStatus.Accepted => "bg-success",
                QuotationStatus.Rejected => "bg-danger",
                QuotationStatus.Expired => "bg-warning",
                QuotationStatus.Converted => "bg-info",
                QuotationStatus.Cancelled => "bg-dark",
                _ => "bg-secondary"
            };
        }
    }
}
