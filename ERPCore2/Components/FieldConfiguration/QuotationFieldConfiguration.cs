using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.PageTemplate;
using ERPCore2.Data.Entities;
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
        private readonly bool _enableApproval;
        
        public QuotationFieldConfiguration(
            List<Customer> customers, 
            List<Employee> employees,
            bool enableApproval = false,
            INotificationService? notificationService = null)
        {
            _customers = customers;
            _employees = employees;
            _enableApproval = enableApproval;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Quotation>> GetFieldDefinitions()
        {
            try
            {
                var fields = new Dictionary<string, FieldDefinition<Quotation>>
                {
                    {
                        nameof(Quotation.Code),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.Code),
                            DisplayName = "報價單號",
                            FilterPlaceholder = "輸入報價單號搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Quotation.Code), q => q.Code)
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
                            Options = _customers.Select(c => new SelectOption 
                            { 
                                Text = c.CompanyName ?? "", 
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
                            DisplayName = "總額",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 5,
                            HeaderStyle = "width: 130px; text-align: right;",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var quotation = (Quotation)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "text-success fw-bold");
                                builder.AddContent(2, quotation.TotalAmount.ToString("N0"));
                                builder.CloseElement();
                            }
                        }
                    }
                };

                // 只有在啟用審核時才加入核准狀態欄位
                if (_enableApproval)
                {
                    fields.Add(nameof(Quotation.IsApproved),
                        new FieldDefinition<Quotation>
                        {
                            PropertyName = nameof(Quotation.IsApproved),
                            DisplayName = "核准狀態",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 8,
                            HeaderStyle = "width: 100px;",
                            Options = new List<SelectOption>
                            {
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
                        });
                }

                return fields;
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
    }
}
