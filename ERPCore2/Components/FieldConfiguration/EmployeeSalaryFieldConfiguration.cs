using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    public class EmployeeSalaryFieldConfiguration : BaseFieldConfiguration<EmployeeSalary>
    {
        private readonly INotificationService? _notificationService;

        public EmployeeSalaryFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<EmployeeSalary>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<EmployeeSalary>>
                {
                    {
                        "EmployeeName",
                        new FieldDefinition<EmployeeSalary>
                        {
                            PropertyName = "EmployeeName",
                            DisplayName = Dn("Field.Employee", "員工"),
                            FilterPlaceholder = Fp("Field.Employee", "輸入員工姓名搜尋"),
                            TableOrder = 1,
                            Width = "110px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "EmployeeName", x => x.Employee != null ? x.Employee.Name : ""),
                            CustomTemplate = item => builder =>
                            {
                                var entity = (EmployeeSalary)item;
                                builder.OpenElement(0, "div");
                                builder.AddContent(1, entity.Employee?.Name ?? "—");
                                if (entity.Employee?.Code != null)
                                {
                                    builder.OpenElement(2, "small");
                                    builder.AddAttribute(3, "class", "text-muted ms-1");
                                    builder.AddContent(4, $"({entity.Employee.Code})");
                                    builder.CloseElement();
                                }
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(EmployeeSalary.SalaryType),
                        new FieldDefinition<EmployeeSalary>
                        {
                            PropertyName = nameof(EmployeeSalary.SalaryType),
                            DisplayName = Dn("Field.SalaryType", "薪資制度"),
                            TableOrder = 2,
                            Width = "100px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (EmployeeSalary)item;
                                var (text, css) = entity.SalaryType switch
                                {
                                    SalaryType.Monthly => (L?["Payroll.Monthly"].ToString() ?? "月薪", "badge bg-primary"),
                                    SalaryType.Hourly  => (L?["Payroll.Hourly"].ToString()  ?? "時薪", "badge bg-info"),
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
                        nameof(EmployeeSalary.BaseSalary),
                        new FieldDefinition<EmployeeSalary>
                        {
                            PropertyName = nameof(EmployeeSalary.BaseSalary),
                            DisplayName = Dn("Field.BaseSalary", "本薪"),
                            TableOrder = 3,
                            Width = "110px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (EmployeeSalary)item;
                                builder.AddContent(0, entity.BaseSalary.ToString("N0"));
                            }
                        }
                    },
                    {
                        nameof(EmployeeSalary.EffectiveDate),
                        new FieldDefinition<EmployeeSalary>
                        {
                            PropertyName = nameof(EmployeeSalary.EffectiveDate),
                            DisplayName = Dn("Field.EffectiveDate", "生效日期"),
                            TableOrder = 4,
                            Width = "110px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (EmployeeSalary)item;
                                builder.AddContent(0, entity.EffectiveDate.ToString("yyyy-MM-dd"));
                            }
                        }
                    },
                    {
                        nameof(EmployeeSalary.ExpiryDate),
                        new FieldDefinition<EmployeeSalary>
                        {
                            PropertyName = nameof(EmployeeSalary.ExpiryDate),
                            DisplayName = Dn("Field.ExpiryDate", "失效日期"),
                            TableOrder = 5,
                            Width = "110px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (EmployeeSalary)item;
                                if (entity.ExpiryDate.HasValue)
                                {
                                    builder.AddContent(0, entity.ExpiryDate.Value.ToString("yyyy-MM-dd"));
                                }
                                else
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", "badge bg-success");
                                    builder.AddContent(2, L?["Payroll.CurrentActive"].ToString() ?? "目前有效");
                                    builder.CloseElement();
                                }
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
                    _ = Task.Run(async () => await _notificationService.ShowErrorAsync("初始化員工薪資欄位配置時發生錯誤"));
                return new Dictionary<string, FieldDefinition<EmployeeSalary>>();
            }
        }
    }
}
