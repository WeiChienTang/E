using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.GenericComponents.IndexComponent;
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
                            DisplayName = "出貨單號",
                            FilterPlaceholder = "輸入出貨單號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesDelivery.Code), sd => sd.Code)
                        }
                    },
                    {
                        nameof(SalesDelivery.DeliveryDate),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = nameof(SalesDelivery.DeliveryDate),
                            DisplayName = "出貨日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 2,
                            FilterOrder = 2,
                            HeaderStyle = "width: 120px;",
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
                            DisplayName = "客戶",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 200px;",
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
                        nameof(SalesDelivery.SalesOrderId),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = "SalesOrder.SalesOrderNumber",
                            FilterPropertyName = nameof(SalesDelivery.SalesOrderId),
                            DisplayName = "來源訂單",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 150px;",
                            NullDisplayText = "-",
                            Options = _salesOrders.Select(so => new SelectOption
                            {
                                Text = so.Code ?? string.Empty,
                                Value = so.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(SalesDelivery.SalesOrderId), sd => sd.SalesOrderId)
                        }
                    },
                    {
                        nameof(SalesDelivery.EmployeeId),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = "Employee.Name",
                            FilterPropertyName = nameof(SalesDelivery.EmployeeId),
                            DisplayName = "業務人員",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 120px;",
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
                        nameof(SalesDelivery.WarehouseId),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = "Warehouse.Name",
                            FilterPropertyName = nameof(SalesDelivery.WarehouseId),
                            DisplayName = "出貨倉庫",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 6,
                            FilterOrder = 6,
                            HeaderStyle = "width: 120px;",
                            NullDisplayText = "-",
                            Options = _warehouses.Select(w => new SelectOption
                            {
                                Text = w.Name,
                                Value = w.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(SalesDelivery.WarehouseId), sd => sd.WarehouseId)
                        }
                    },
                    {
                        nameof(SalesDelivery.TotalAmount),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = nameof(SalesDelivery.TotalAmount),
                            DisplayName = "出貨金額",
                            FilterType = SearchFilterType.NumberRange,
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 7,
                            FilterOrder = 7,
                            HeaderStyle = "width: 120px; text-align: right;",
                            ShowInFilter = false // 金額欄位暫時不提供篩選功能
                        }
                    },
                    {
                        nameof(SalesDelivery.IsShipped),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = nameof(SalesDelivery.IsShipped),
                            DisplayName = "已出貨",
                            FilterType = SearchFilterType.Select,
                            ColumnType = ColumnDataType.Boolean,
                            TableOrder = 8,
                            FilterOrder = 8,
                            HeaderStyle = "width: 100px; text-align: center;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "全部", Value = "" },
                                new SelectOption { Text = "已出貨", Value = "true" },
                                new SelectOption { Text = "未出貨", Value = "false" }
                            },
                            FilterFunction = (model, query) => 
                            {
                                if (!string.IsNullOrWhiteSpace(model.TextFilters.GetValueOrDefault(nameof(SalesDelivery.IsShipped))))
                                {
                                    var value = model.TextFilters[nameof(SalesDelivery.IsShipped)];
                                    if (bool.TryParse(value, out bool isShipped))
                                    {
                                        query = query.Where(sd => sd.IsShipped == isShipped);
                                    }
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(SalesDelivery.IsApproved),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = nameof(SalesDelivery.IsApproved),
                            DisplayName = "已核准",
                            FilterType = SearchFilterType.Select,
                            ColumnType = ColumnDataType.Boolean,
                            TableOrder = 9,
                            FilterOrder = 9,
                            HeaderStyle = "width: 100px; text-align: center;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "全部", Value = "" },
                                new SelectOption { Text = "已核准", Value = "true" },
                                new SelectOption { Text = "未核准", Value = "false" }
                            },
                            FilterFunction = (model, query) => 
                            {
                                if (!string.IsNullOrWhiteSpace(model.TextFilters.GetValueOrDefault(nameof(SalesDelivery.IsApproved))))
                                {
                                    var value = model.TextFilters[nameof(SalesDelivery.IsApproved)];
                                    if (bool.TryParse(value, out bool isApproved))
                                    {
                                        query = query.Where(sd => sd.IsApproved == isApproved);
                                    }
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(SalesDelivery.DeliveryAddress),
                        new FieldDefinition<SalesDelivery>
                        {
                            PropertyName = nameof(SalesDelivery.DeliveryAddress),
                            DisplayName = "送貨地址",
                            FilterPlaceholder = "輸入送貨地址搜尋",
                            TableOrder = 10,
                            FilterOrder = 10,
                            HeaderStyle = "width: 250px;",
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesDelivery.DeliveryAddress), sd => sd.DeliveryAddress)
                        }
                    }
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

        protected override Func<IQueryable<SalesDelivery>, IQueryable<SalesDelivery>> GetDefaultSort()
        {
            return query => query.OrderByDescending(sd => sd.DeliveryDate)
                                 .ThenByDescending(sd => sd.Code);
        }
    }
}
