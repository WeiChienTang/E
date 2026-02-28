using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
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
                            DisplayName = Dn("Field.PurchaseReturnCode", "退回單號"),
                            FilterPlaceholder = Fp("Field.PurchaseReturnCode", "輸入退回單號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PurchaseReturn.Code), pr => pr.Code)
                        }
                    },
                    {
                        "SupplierName",
                        new FieldDefinition<PurchaseReturn>
                        {
                            PropertyName = "Supplier.CompanyName",
                            DisplayName = Dn("Field.Supplier", "廠商"),
                            TableOrder = 2,
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = Fp("Field.Supplier", "輸入廠商名稱搜尋"),
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "SupplierName", pr => pr.Supplier != null ? pr.Supplier.CompanyName : "")
                        }
                    },
                    {
                        nameof(PurchaseReturn.ReturnDate),
                        new FieldDefinition<PurchaseReturn>
                        {
                            PropertyName = nameof(PurchaseReturn.ReturnDate),
                            DisplayName = Dn("Field.PurchaseReturnDate", "退回日期"),
                            ColumnType = ColumnDataType.Date,
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(PurchaseReturn.ReturnDate), pr => pr.ReturnDate)
                        }
                    },
                    {
                        "PurchaseReceivingNumber",
                        new FieldDefinition<PurchaseReturn>
                        {
                            PropertyName = "PurchaseReceivingNumber",
                            DisplayName = Dn("Field.OriginalReceivingCode", "原進貨單號"),
                            TableOrder = 4,
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = Fp("Field.OriginalReceivingCode", "輸入原進貨單號搜尋"),
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "PurchaseReceivingNumber", pr => string.Empty, allowNull: true)
                        }
                    },
                    {
                        nameof(PurchaseReturn.TotalReturnAmountWithTax),
                        new FieldDefinition<PurchaseReturn>
                        {
                            PropertyName = nameof(PurchaseReturn.TotalReturnAmountWithTax),
                            DisplayName = Dn("Field.TotalAmount", "總額"),
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 6,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var purchaseReturn = (PurchaseReturn)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "text-success fw-bold");
                                builder.AddContent(2, purchaseReturn.TotalReturnAmountWithTax.ToString("N0"));
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
    }
}
