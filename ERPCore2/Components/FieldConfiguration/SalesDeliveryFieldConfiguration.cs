using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 銷貨/出貨單欄位配置
    /// </summary>
    public class SalesDeliveryFieldConfiguration : BaseFieldConfiguration<SalesDelivery>
    {
        private readonly List<Customer> _customers;
        private readonly List<SalesOrder> _salesOrders;
        private readonly List<Employee> _employees;
        private readonly List<Warehouse> _warehouses;
        private readonly INotificationService? _notificationService;

        public SalesDeliveryFieldConfiguration(
            List<Customer> customers,
            List<SalesOrder> salesOrders,
            List<Employee> employees,
            List<Warehouse> warehouses,
            INotificationService? notificationService = null)
        {
            _customers = customers;
            _salesOrders = salesOrders;
            _employees = employees;
            _warehouses = warehouses;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<SalesDelivery>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<SalesDelivery>>
                {
                    {
                        nameof(SalesDelivery.Code),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = nameof(SalesDelivery.Code),
                            DisplayName = Dn("Field.SalesDeliveryCode", "出貨單號"),
                            FilterPlaceholder = Fp("Field.SalesDeliveryCode", "輸入出貨單號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesDelivery.Code), sd => sd.Code)
                        }
                    },
                    {
                        nameof(SalesDelivery.DeliveryDate),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = nameof(SalesDelivery.DeliveryDate),
                            DisplayName = Dn("Field.SalesDeliveryDate", "出貨日期"),
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(SalesDelivery.DeliveryDate), sd => sd.DeliveryDate)
                        }
                    },
                    {
                        nameof(SalesDelivery.CustomerId),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(SalesDelivery.CustomerId),
                            DisplayName = Dn("Field.Customer", "客戶"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            Options = _customers.Select(c => new SelectOption
                            {
                                Text = c.CompanyName ?? "",
                                Value = c.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(SalesDelivery.CustomerId), sd => sd.CustomerId)
                        }
                    },
                    {
                        nameof(SalesDelivery.EmployeeId),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = "Employee.Name",
                            FilterPropertyName = nameof(SalesDelivery.EmployeeId),
                            DisplayName = Dn("Field.SalesPerson", "業務人員"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            NullDisplayText = "-",
                            Options = _employees.Select(e => new SelectOption
                            {
                                Text = e.Name ?? "",
                                Value = e.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(SalesDelivery.EmployeeId), sd => sd.EmployeeId)
                        }
                    },
                    {
                        nameof(SalesDelivery.TotalAmountWithTax),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = nameof(SalesDelivery.TotalAmountWithTax),
                            DisplayName = Dn("Field.TotalAmount", "總額"),
                            FilterType = SearchFilterType.NumberRange,
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 7,
                            ShowInFilter = false, // 金額欄位暫時不提供篩選功能
                            CustomTemplate = item => builder =>
                            {
                                var salesDelivery = (SalesDelivery)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "text-success fw-bold");
                                builder.AddContent(2, salesDelivery.TotalAmountWithTax.ToString("N0"));
                                builder.CloseElement();
                            }
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                // 非同步錯誤處理
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(
                        ex,
                        nameof(GetFieldDefinitions),
                        typeof(SalesDeliveryFieldConfiguration),
                        additionalData: "建立銷貨/出貨單欄位定義失敗");

                    if (_notificationService != null)
                    {
                        await _notificationService.ShowErrorAsync("載入欄位配置時發生錯誤");
                    }
                });

                // 回傳空字典作為安全的後備值
                return new Dictionary<string, FieldDefinition<SalesDelivery>>();
            }
        }
    }
}
