using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Tables;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Helpers.FieldConfiguration
{
    /// <summary>
    /// 採購進貨單欄位配置
    /// </summary>
    public class PurchaseReceivingFieldConfiguration : BaseFieldConfiguration<PurchaseReceiving>
    {
        private readonly List<PurchaseOrder> _purchaseOrders;
        private readonly List<Warehouse> _warehouses;
        private readonly INotificationService? _notificationService;

        public PurchaseReceivingFieldConfiguration(
            List<PurchaseOrder> purchaseOrders, 
            List<Warehouse> warehouses, 
            INotificationService? notificationService = null)
        {
            _purchaseOrders = purchaseOrders;
            _warehouses = warehouses;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PurchaseReceiving>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<PurchaseReceiving>>
                {
                    {
                        nameof(PurchaseReceiving.ReceiptNumber),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.ReceiptNumber),
                            DisplayName = "進貨單號",
                            FilterPlaceholder = "輸入進貨單號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 160px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PurchaseReceiving.ReceiptNumber), pr => pr.ReceiptNumber)
                        }
                    },
                    {
                        nameof(PurchaseReceiving.PurchaseOrderId),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = "PurchaseOrder.PurchaseOrderNumber", // 表格顯示用
                            FilterPropertyName = nameof(PurchaseReceiving.PurchaseOrderId), // 篩選器用
                            DisplayName = "採購單號",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 2,
                            FilterOrder = 2,
                            HeaderStyle = "width: 160px;",
                            Options = _purchaseOrders.Select(po => new SelectOption 
                            { 
                                Text = po.PurchaseOrderNumber, 
                                Value = po.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(PurchaseReceiving.PurchaseOrderId), pr => pr.PurchaseOrderId)
                        }
                    },
                    {
                        "SupplierName",
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = "PurchaseOrder.Supplier.CompanyName", // 透過關聯顯示供應商名稱
                            DisplayName = "供應商",
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = "輸入供應商名稱搜尋",
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "SupplierName", pr => pr.PurchaseOrder.Supplier.CompanyName)
                        }
                    },
                    {
                        nameof(PurchaseReceiving.ReceiptDate),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.ReceiptDate),
                            DisplayName = "進貨日期",
                            ColumnType = ColumnDataType.Date,
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(PurchaseReceiving.ReceiptDate), pr => pr.ReceiptDate)
                        }
                    },
                    {
                        nameof(PurchaseReceiving.WarehouseId),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = "Warehouse.Name", // 表格顯示用
                            FilterPropertyName = nameof(PurchaseReceiving.WarehouseId), // 篩選器用
                            DisplayName = "倉庫",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 140px;",
                            Options = _warehouses.Select(w => new SelectOption 
                            { 
                                Text = w.Name, 
                                Value = w.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(PurchaseReceiving.WarehouseId), pr => pr.WarehouseId)
                        }
                    },
                    {
                        nameof(PurchaseReceiving.ReceiptStatus),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.ReceiptStatus),
                            DisplayName = "進貨狀態",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 6,
                            FilterOrder = 6,
                            HeaderStyle = "width: 100px;",
                            Options = Enum.GetValues<PurchaseReceivingStatus>()
                                .Select(s => new SelectOption 
                                { 
                                    Text = GetStatusDescription(s), 
                                    Value = ((int)s).ToString() 
                                }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(PurchaseReceiving.ReceiptStatus), pr => (int)pr.ReceiptStatus),
                            CustomTemplate = item => builder =>
                            {
                                var receipt = (PurchaseReceiving)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "badge");
                                
                                var badgeClass = receipt.ReceiptStatus switch
                                {
                                    PurchaseReceivingStatus.Draft => "bg-secondary",
                                    PurchaseReceivingStatus.Confirmed => "bg-primary",
                                    PurchaseReceivingStatus.Received => "bg-success",
                                    PurchaseReceivingStatus.Cancelled => "bg-danger",
                                    _ => "bg-secondary"
                                };
                                
                                builder.AddAttribute(2, "class", $"badge {badgeClass}");
                                builder.AddContent(3, GetStatusDescription(receipt.ReceiptStatus));
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(PurchaseReceiving.TotalAmount),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.TotalAmount),
                            DisplayName = "總金額",
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 7,
                            ShowInFilter = false,
                            HeaderStyle = "width: 120px; text-align: right;",
                        }
                    },
                    {
                        nameof(PurchaseReceiving.ConfirmedAt),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.ConfirmedAt),
                            DisplayName = "確認時間",
                            ColumnType = ColumnDataType.DateTime,
                            TableOrder = 8,
                            ShowInFilter = false,
                            HeaderStyle = "width: 140px;",
                        }
                    },
                    {
                        nameof(PurchaseReceiving.ConfirmedBy),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = "ConfirmedByUser.Name", // 顯示確認人員的名稱
                            DisplayName = "確認人員",
                            TableOrder = 9,
                            ShowInFilter = false,
                            HeaderStyle = "width: 120px;",
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
                            PurchaseOrdersCount = _purchaseOrders?.Count ?? 0,
                            WarehousesCount = _warehouses?.Count ?? 0
                        });
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化採購進貨單欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<PurchaseReceiving>>();
            }
        }

        /// <summary>
        /// 取得預設排序
        /// </summary>
        protected override Func<IQueryable<PurchaseReceiving>, IQueryable<PurchaseReceiving>> GetDefaultSort()
        {
            return q => q.OrderByDescending(pr => pr.ReceiptDate)
                         .ThenByDescending(pr => pr.ReceiptNumber);
        }

        /// <summary>
        /// 取得狀態描述
        /// </summary>
        private static string GetStatusDescription(PurchaseReceivingStatus status)
        {
            return status switch
            {
                PurchaseReceivingStatus.Draft => "草稿",
                PurchaseReceivingStatus.Confirmed => "已確認",
                PurchaseReceivingStatus.Received => "已入庫",
                PurchaseReceivingStatus.Cancelled => "已取消",
                _ => status.ToString()
            };
        }
    }
}
