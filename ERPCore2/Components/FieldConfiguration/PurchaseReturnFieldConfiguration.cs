using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.GenericComponents.IndexComponent;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 採購退回欄位配置
    /// </summary>
    public class PurchaseReturnFieldConfiguration : BaseFieldConfiguration<PurchaseReturn>
    {
        private readonly List<Supplier> _suppliers;
        private readonly List<PurchaseOrder> _purchaseOrders;
        private readonly List<PurchaseReceiving> _purchaseReceivings;
        private readonly List<Warehouse> _warehouses;
        private readonly List<Employee> _employees;
        private readonly INotificationService? _notificationService;

        public PurchaseReturnFieldConfiguration(
            List<Supplier> suppliers,
            List<PurchaseOrder> purchaseOrders,
            List<PurchaseReceiving> purchaseReceivings,
            List<Warehouse> warehouses,
            List<Employee> employees,
            INotificationService? notificationService = null)
        {
            _suppliers = suppliers;
            _purchaseOrders = purchaseOrders;
            _purchaseReceivings = purchaseReceivings;
            _warehouses = warehouses;
            _employees = employees;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PurchaseReturn>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<PurchaseReturn>>
                {
                    {
                        nameof(PurchaseReturn.Code),
                        new FieldDefinition<PurchaseReturn>
                        {
                            PropertyName = nameof(PurchaseReturn.Code),
                            DisplayName = "退回單號",
                            FilterPlaceholder = "輸入退回單號搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 160px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PurchaseReturn.Code), pr => pr.Code)
                        }
                    },
                    {
                        "SupplierName",
                        new FieldDefinition<PurchaseReturn>
                        {
                            PropertyName = "Supplier.CompanyName",
                            DisplayName = "供應商",
                            TableOrder = 2,
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = "輸入供應商名稱搜尋",
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "SupplierName", pr => pr.Supplier != null ? pr.Supplier.CompanyName : "")
                        }
                    },
                    {
                        nameof(PurchaseReturn.ReturnDate),
                        new FieldDefinition<PurchaseReturn>
                        {
                            PropertyName = nameof(PurchaseReturn.ReturnDate),
                            DisplayName = "退回日期",
                            ColumnType = ColumnDataType.Date,
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 3,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(PurchaseReturn.ReturnDate), pr => pr.ReturnDate)
                        }
                    },
                    {
                        "PurchaseReceivingNumber",
                        new FieldDefinition<PurchaseReturn>
                        {
                            PropertyName = "PurchaseReceiving.ReceiptNumber",
                            DisplayName = "原進貨單號",
                            TableOrder = 4,
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = "輸入原進貨單號搜尋",
                            HeaderStyle = "width: 160px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "PurchaseReceivingNumber", pr => pr.PurchaseReceiving != null ? pr.PurchaseReceiving.Code : "", allowNull: true)
                        }
                    },
                    {
                        nameof(PurchaseReturn.TotalReturnAmountWithTax),
                        new FieldDefinition<PurchaseReturn>
                        {
                            PropertyName = nameof(PurchaseReturn.TotalReturnAmountWithTax),
                            DisplayName = "退回總金額",
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 6,
                            ShowInFilter = false,
                            HeaderStyle = "width: 120px; text-align: right;",
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
                            SuppliersCount = _suppliers?.Count ?? 0,
                            PurchaseOrdersCount = _purchaseOrders?.Count ?? 0,
                            PurchaseReceivingsCount = _purchaseReceivings?.Count ?? 0,
                            WarehousesCount = _warehouses?.Count ?? 0,
                            EmployeesCount = _employees?.Count ?? 0
                        });
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化採購退回欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<PurchaseReturn>>();
            }
        }

        /// <summary>
        /// 取得預設排序
        /// </summary>
        protected override Func<IQueryable<PurchaseReturn>, IQueryable<PurchaseReturn>> GetDefaultSort()
        {
            return q => q.OrderByDescending(pr => pr.ReturnDate)
                         .ThenByDescending(pr => pr.Code);
        }
    }
}
