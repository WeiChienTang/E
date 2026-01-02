using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.PageTemplate;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 生產排程欄位配置
    /// </summary>
    public class ProductionScheduleFieldConfiguration : BaseFieldConfiguration<ProductionSchedule>
    {
        private readonly List<Customer> _customers;
        private readonly List<Employee> _employees;
        private readonly INotificationService? _notificationService;
        
        public ProductionScheduleFieldConfiguration(
            List<Customer> customers,
            List<Employee> employees,
            INotificationService? notificationService = null)
        {
            _customers = customers;
            _employees = employees;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<ProductionSchedule>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ProductionSchedule>>
                {
                    {
                        nameof(ProductionSchedule.Code),
                        new FieldDefinition<ProductionSchedule>
                        {
                            PropertyName = nameof(ProductionSchedule.Code),
                            DisplayName = "排程單號",
                            FilterPlaceholder = "輸入排程單號搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductionSchedule.Code), ps => ps.Code)
                        }
                    },
                    {
                        nameof(ProductionSchedule.ScheduleDate),
                        new FieldDefinition<ProductionSchedule>
                        {
                            PropertyName = nameof(ProductionSchedule.ScheduleDate),
                            DisplayName = "排程日期",
                            FilterType = SearchFilterType.Date,
                            TableOrder = 2,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(ProductionSchedule.ScheduleDate), ps => ps.ScheduleDate),
                            CustomTemplate = item => builder =>
                            {
                                var schedule = (ProductionSchedule)item;
                                builder.AddContent(0, schedule.ScheduleDate.ToString("yyyy/MM/dd"));
                            }
                        }
                    },
                    {
                        nameof(ProductionSchedule.CustomerId),
                        new FieldDefinition<ProductionSchedule>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(ProductionSchedule.CustomerId),
                            DisplayName = "客戶",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            HeaderStyle = "width: 200px;",
                            Options = _customers.Select(c => new SelectOption
                            {
                                Text = c.CompanyName ?? "",
                                Value = c.Id.ToString()
                            }).ToList(),
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(ProductionSchedule.CustomerId), ps => ps.CustomerId)
                        }
                    },
                    {
                        nameof(ProductionSchedule.CreatedByEmployeeId),
                        new FieldDefinition<ProductionSchedule>
                        {
                            PropertyName = "CreatedByEmployee.Name",
                            FilterPropertyName = nameof(ProductionSchedule.CreatedByEmployeeId),
                            DisplayName = "製單人員",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            HeaderStyle = "width: 150px;",
                            Options = _employees.Select(e => new SelectOption
                            {
                                Text = $"{e.Code} - {e.Name}".Trim(),
                                Value = e.Id.ToString()
                            }).ToList(),
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(ProductionSchedule.CreatedByEmployeeId), ps => ps.CreatedByEmployeeId),
                            CustomTemplate = item => builder =>
                            {
                                var schedule = (ProductionSchedule)item;
                                if (schedule.CreatedByEmployee != null)
                                {
                                    var employeeName = schedule.CreatedByEmployee.Name?.Trim() ?? "";
                                    if (string.IsNullOrWhiteSpace(employeeName))
                                    {
                                        employeeName = schedule.CreatedByEmployee.Code;
                                    }
                                    builder.AddContent(0, employeeName);
                                }
                                else
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", "text-muted");
                                    builder.AddContent(2, "-");
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        nameof(ProductionSchedule.SourceDocumentType),
                        new FieldDefinition<ProductionSchedule>
                        {
                            PropertyName = nameof(ProductionSchedule.SourceDocumentType),
                            DisplayName = "來源單據類型",
                            FilterPlaceholder = "輸入來源單據類型搜尋",
                            TableOrder = 5,
                            HeaderStyle = "width: 150px;",
                            ShowInFilter = false, // 通常不需要篩選
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ProductionSchedule.SourceDocumentType), ps => ps.SourceDocumentType ?? "")
                        }
                    },
                    {
                        nameof(ProductionSchedule.SourceDocumentId),
                        new FieldDefinition<ProductionSchedule>
                        {
                            PropertyName = nameof(ProductionSchedule.SourceDocumentId),
                            DisplayName = "來源單據ID",
                            ShowInFilter = false,
                            ShowInTable = false, // 通常不在表格顯示
                            NullDisplayText = "-"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化生產排程欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化生產排程欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<ProductionSchedule>>();
            }
        }
    }
}


