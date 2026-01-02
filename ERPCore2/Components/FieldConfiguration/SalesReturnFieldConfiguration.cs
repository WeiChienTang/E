using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.PageTemplate;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;
using ERPCore2.Helpers;

// 使用別名來避免命名衝突
using EntitySalesReturnReason = ERPCore2.Data.Entities.SalesReturnReason;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 銷售退貨單欄位配置
    /// </summary>
    public class SalesReturnFieldConfiguration : BaseFieldConfiguration<SalesReturn>
    {
        private readonly List<SalesDelivery> _salesDeliveries;
        private readonly List<Warehouse> _warehouses;
        private readonly List<EntitySalesReturnReason> _returnReasons;
        private readonly INotificationService? _notificationService;

        public SalesReturnFieldConfiguration(
            List<SalesDelivery> salesDeliveries, 
            List<Warehouse> warehouses, 
            List<EntitySalesReturnReason> returnReasons,
            INotificationService? notificationService = null)
        {
            _salesDeliveries = salesDeliveries;
            _warehouses = warehouses;
            _returnReasons = returnReasons;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<SalesReturn>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<SalesReturn>>
                {
                    {
                        nameof(SalesReturn.Code),
                        new FieldDefinition<SalesReturn>
                        {
                            PropertyName = nameof(SalesReturn.Code),
                            DisplayName = "退貨單號",
                            FilterPlaceholder = "輸入退貨單號搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 160px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SalesReturn.Code), sr => sr.Code)
                        }
                    },
                    {
                        "CustomerName",
                        new FieldDefinition<SalesReturn>
                        {
                            PropertyName = "Customer.CompanyName", // 使用客戶公司名稱
                            DisplayName = "客戶",
                            TableOrder = 2,
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = "輸入客戶名稱搜尋",
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "CustomerName", sr => sr.Customer != null ? sr.Customer.CompanyName : "")
                        }
                    },
                    {
                        nameof(SalesReturn.ReturnDate),
                        new FieldDefinition<SalesReturn>
                        {
                            PropertyName = nameof(SalesReturn.ReturnDate),
                            DisplayName = "退貨日期",
                            ColumnType = ColumnDataType.Date,
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 3,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(SalesReturn.ReturnDate), sr => sr.ReturnDate)
                        }
                    },
                    {
                        "ReturnReason",
                        new FieldDefinition<SalesReturn>
                        {
                            PropertyName = "ReturnReason.Name",
                            DisplayName = "退貨原因",
                            TableOrder = 4,
                            FilterType = SearchFilterType.Select,
                            HeaderStyle = "width: 150px;",
                            Options = _returnReasons.Select(r => new SelectOption
                            {
                                Text = r.Name,
                                Value = r.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, "ReturnReason", sr => sr.ReturnReasonId)
                        }
                    },
                    {
                        nameof(SalesReturn.TotalReturnAmountWithTax),
                        new FieldDefinition<SalesReturn>
                        {
                            PropertyName = nameof(SalesReturn.TotalReturnAmountWithTax),
                            DisplayName = "總額",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 5,
                            ShowInFilter = false,
                            HeaderStyle = "width: 120px; text-align: right;",
                            CustomTemplate = item => builder =>
                            {
                                var salesReturn = (SalesReturn)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "text-success fw-bold");
                                builder.AddContent(2, salesReturn.TotalReturnAmountWithTax.ToString("N0"));
                                builder.CloseElement();
                            }
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
                            SalesDeliveriesCount = _salesDeliveries?.Count ?? 0,
                            WarehousesCount = _warehouses?.Count ?? 0,
                            ReturnReasonsCount = _returnReasons?.Count ?? 0
                        });
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化銷售退貨單欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<SalesReturn>>();
            }
        }

        /// <summary>
    }
}
