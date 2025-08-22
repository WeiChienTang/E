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
                            DisplayName = "單號",
                            FilterPlaceholder = "輸入採購單號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PurchaseOrder.PurchaseOrderNumber), po => po.PurchaseOrderNumber)
                        }
                    },
                    {
                        nameof(PurchaseOrder.SupplierId),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = "Supplier.CompanyName", // 表格顯示用
                            FilterPropertyName = "Supplier.CompanyName", // 篩選器也使用公司名稱
                            DisplayName = "供應商",
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = "輸入供應商名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Supplier.CompanyName", po => po.Supplier != null ? po.Supplier.CompanyName : null, allowNull: true)
                        }
                    },
                    {
                        nameof(PurchaseOrder.OrderDate),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.OrderDate),
                            DisplayName = "採購日",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(PurchaseOrder.OrderDate), po => po.OrderDate)
                        }
                    },
                    {
                        nameof(PurchaseOrder.ExpectedDeliveryDate),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.ExpectedDeliveryDate),
                            DisplayName = "預交日",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 4,
                            FilterOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableDateRangeFilter(
                                model, query, nameof(PurchaseOrder.ExpectedDeliveryDate), po => po.ExpectedDeliveryDate)
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
                        }
                    },
                    {
                        nameof(PurchaseOrder.IsApproved),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.IsApproved),
                            DisplayName = "是否核准",
                            ShowInFilter = false,
                            TableOrder = 6,
                            HeaderStyle = "width: 90px;",
                            CustomTemplate = item => builder =>
                            {
                                var purchaseOrder = (PurchaseOrder)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "badge text-white");
                                builder.AddAttribute(2, "style", purchaseOrder.IsApproved ? "background-color: #28a745;" : "background-color: #dc3545;");
                                builder.AddContent(3, purchaseOrder.IsApproved ? "已核准" : "未核准");
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(PurchaseOrder.RejectReason),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.RejectReason),
                            DisplayName = "駁回原因",
                            ColumnType = ColumnDataType.Text,
                            TableOrder = 7,
                            ShowInFilter = false,
                        }
                    },
                    {
                        nameof(PurchaseOrder.ApprovedBy),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = "ApprovedByUser.Name", // 顯示審核人員的名稱
                            DisplayName = "審核人",
                            ColumnType = ColumnDataType.Text,
                            TableOrder = 8,
                            ShowInFilter = false,
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
