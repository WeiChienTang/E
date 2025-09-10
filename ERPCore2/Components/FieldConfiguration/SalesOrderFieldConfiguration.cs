using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Tables;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Data.Enums;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 銷貨訂單欄位配置
    /// </summary>
    public class SalesOrderFieldConfiguration : BaseFieldConfiguration<SalesOrder>
    {
        private readonly List<Customer> _customers;
        private readonly List<Employee> _employees;
        private readonly INotificationService? _notificationService;
        
        public SalesOrderFieldConfiguration(
            List<Customer> customers, 
            List<Employee> employees,
            INotificationService? notificationService = null)
        {
            _customers = customers;
            _employees = employees;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<SalesOrder>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<SalesOrder>>
                {
                    {
                        nameof(SalesOrder.SalesOrderNumber),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.SalesOrderNumber),
                            DisplayName = "銷貨單號",
                            FilterPlaceholder = "輸入銷貨單號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesOrder.SalesOrderNumber), so => so.SalesOrderNumber)
                        }
                    },
                    {
                        nameof(SalesOrder.OrderDate),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.OrderDate),
                            DisplayName = "訂單日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 2,
                            FilterOrder = 2,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(SalesOrder.OrderDate), so => so.OrderDate)
                        }
                    },
                    {
                        nameof(SalesOrder.CustomerId),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(SalesOrder.CustomerId),
                            DisplayName = "客戶",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 200px;",
                            Options = _customers.Select(c => new SelectOption 
                            { 
                                Text = c.CompanyName, 
                                Value = c.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(SalesOrder.CustomerId), so => so.CustomerId)
                        }
                    },
                    {
                        nameof(SalesOrder.EmployeeId),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = "Employee.Name",
                            FilterPropertyName = nameof(SalesOrder.EmployeeId),
                            DisplayName = "負責業務",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 120px;",
                            NullDisplayText = "未指定",
                            Options = _employees.Where(e => !string.IsNullOrEmpty(e.Name)).Select(e => new SelectOption 
                            { 
                                Text = e.Name!, 
                                Value = e.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(SalesOrder.EmployeeId), so => so.EmployeeId)
                        }
                    },
                    {
                        nameof(SalesOrder.TotalAmount),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.TotalAmount),
                            DisplayName = "訂單總額",
                            FilterType = SearchFilterType.NumberRange,
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 120px; text-align: right;",
                            ShowInFilter = false // 金額欄位暫時不提供篩選功能
                        }
                    },
                    {
                        nameof(SalesOrder.ExpectedDeliveryDate),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.ExpectedDeliveryDate),
                            DisplayName = "預計交貨日",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 6,
                            FilterOrder = 6,
                            HeaderStyle = "width: 120px;",
                            NullDisplayText = "未設定",
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableDateRangeFilter(
                                model, query, nameof(SalesOrder.ExpectedDeliveryDate), so => so.ExpectedDeliveryDate)
                        }
                    },
                    {
                        nameof(SalesOrder.Status),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.Status),
                            DisplayName = "狀態",
                            FilterType = SearchFilterType.Select,
                            ColumnType = ColumnDataType.Status,
                            TableOrder = 7,
                            FilterOrder = 7,
                            HeaderStyle = "width: 100px;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "啟用", Value = EntityStatus.Active.ToString() },
                                new SelectOption { Text = "停用", Value = EntityStatus.Inactive.ToString() }
                            },
                            FilterFunction = (model, query) => FilterHelper.ApplyStatusFilter(
                                model, query, nameof(SalesOrder.Status))
                        }
                    },
                    {
                        nameof(SalesOrder.PaymentTerms),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.PaymentTerms),
                            DisplayName = "付款條件",
                            FilterPlaceholder = "輸入付款條件搜尋",
                            ShowInTable = false, // 不在表格中顯示，但可用於篩選
                            FilterOrder = 8,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesOrder.PaymentTerms), so => so.PaymentTerms, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 錯誤處理
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), 
                            typeof(SalesOrderFieldConfiguration), additionalData: "取得銷貨訂單欄位定義時發生錯誤");
                        
                        if (_notificationService != null)
                        {
                            await _notificationService.ShowErrorAsync("載入欄位配置時發生錯誤");
                        }
                    }
                    catch
                    {
                        // 避免在錯誤處理中再次發生錯誤
                    }
                });
                
                return new Dictionary<string, FieldDefinition<SalesOrder>>();
            }
        }
    }
}
