using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;

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
                        nameof(SalesOrder.Code),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.Code),
                            DisplayName = "訂單單號",
                            FilterPlaceholder = "輸入訂單單號搜尋",
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesOrder.Code), so => so.Code)
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
                            TableOrder = 2,
                            Options = _customers.Select(c => new SelectOption 
                            { 
                                Text = c.CompanyName ?? "", 
                                Value = c.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(SalesOrder.CustomerId), so => so.CustomerId)
                        }
                    },
                    {
                        nameof(SalesOrder.TotalAmountWithTax),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.TotalAmountWithTax),
                            DisplayName = "總額",
                            FilterType = SearchFilterType.NumberRange,
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 4,
                            ShowInFilter = false, // 金額欄位暫時不提供篩選功能
                            CustomTemplate = item => builder =>
                            {
                                var salesOrder = (SalesOrder)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "text-success fw-bold");
                                builder.AddContent(2, salesOrder.TotalAmountWithTax.ToString("N0"));
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(SalesOrder.OrderDate),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.OrderDate),
                            DisplayName = "訂單日",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(SalesOrder.OrderDate), so => so.OrderDate)
                        }
                    },
                    {
                        nameof(SalesOrder.ExpectedDeliveryDate),
                        new FieldDefinition<SalesOrder>
                        {
                            PropertyName = nameof(SalesOrder.ExpectedDeliveryDate),
                            DisplayName = "預交日",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 6,
                            NullDisplayText = "未設定",
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableDateRangeFilter(
                                model, query, nameof(SalesOrder.ExpectedDeliveryDate), so => so.ExpectedDeliveryDate)
                        }
                    },
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
