using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Tables;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;

namespace ERPCore2.Helpers.FieldConfiguration
{
    /// <summary>
    /// 採購單欄位配置
    /// </summary>
    public class PurchaseOrderFieldConfiguration : BaseFieldConfiguration<PurchaseOrder>
    {
        private readonly List<Supplier> _suppliers;
        private readonly INotificationService? _notificationService;

        public PurchaseOrderFieldConfiguration(List<Supplier> suppliers, INotificationService? notificationService = null)
        {
            _suppliers = suppliers;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PurchaseOrder>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<PurchaseOrder>>
                {
                    {
                        nameof(PurchaseOrder.PurchaseOrderNumber),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.PurchaseOrderNumber),
                            DisplayName = "採購單號",
                            FilterPlaceholder = "輸入採購單號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PurchaseOrder.PurchaseOrderNumber), po => po.PurchaseOrderNumber)
                        }
                    },
                    {
                        nameof(PurchaseOrder.SupplierId),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = "Supplier.CompanyName", // 表格顯示用
                            FilterPropertyName = nameof(PurchaseOrder.SupplierId), // 篩選器用
                            DisplayName = "供應商",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 2,
                            FilterOrder = 2,
                            Options = _suppliers.Where(s => s.Status == EntityStatus.Active)
                                .Select(s => new SelectOption 
                                { 
                                    Text = s.CompanyName, 
                                    Value = s.Id.ToString() 
                                }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(PurchaseOrder.SupplierId), po => po.SupplierId)
                        }
                    },
                    {
                        nameof(PurchaseOrder.OrderDate),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.OrderDate),
                            DisplayName = "採購日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 140px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, "PurchaseOrderDate", po => po.OrderDate)
                        }
                    },
                    {
                        nameof(PurchaseOrder.ExpectedDeliveryDate),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.ExpectedDeliveryDate),
                            DisplayName = "預定交貨日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 4,
                            FilterOrder = 5,
                            HeaderStyle = "width: 140px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableDateRangeFilter(
                                model, query, "ExpectedDeliveryDate", po => po.ExpectedDeliveryDate)
                        }
                    },
                    {
                        nameof(PurchaseOrder.TotalAmount),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.TotalAmount),
                            DisplayName = "總金額",
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 5,
                            ShowInFilter = false, // 通常不會用金額篩選
                            HeaderStyle = "width: 120px; text-align: right;"
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
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, 
                            nameof(GetFieldDefinitions), 
                            GetType(), 
                            additionalData: "採購單欄位配置初始化失敗");
                        
                        if (_notificationService != null)
                        {
                            await _notificationService.ShowErrorAsync("欄位配置初始化失敗");
                        }
                    }
                    catch
                    {
                        // 防止無限循環錯誤
                    }
                });

                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<PurchaseOrder>>();
            }
        }

        /// <summary>
        /// 採購單預設排序：依採購日期降序，然後依 ID 降序
        /// </summary>
        protected override Func<IQueryable<PurchaseOrder>, IQueryable<PurchaseOrder>> GetDefaultSort()
        {
            return q => q.OrderByDescending(po => po.OrderDate)
                        .ThenByDescending(po => po.Id);
        }
    }
}
