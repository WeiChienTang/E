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
    /// 採購進貨單欄位配置
    /// </summary>
    public class PurchaseReceivingFieldConfiguration : BaseFieldConfiguration<PurchaseReceiving>
    {
        private readonly List<PurchaseOrder> _purchaseOrders;
        private readonly List<Warehouse> _warehouses;
        private readonly INotificationService? _notificationService;
        private readonly bool _enableApproval;

        public PurchaseReceivingFieldConfiguration(
            List<PurchaseOrder> purchaseOrders,
            List<Warehouse> warehouses,
            bool enableApproval = false,
            INotificationService? notificationService = null)
        {
            _purchaseOrders = purchaseOrders;
            _warehouses = warehouses;
            _enableApproval = enableApproval;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PurchaseReceiving>> GetFieldDefinitions()
        {
            try
            {
                var fields = new Dictionary<string, FieldDefinition<PurchaseReceiving>>
                {
                    {
                        nameof(PurchaseReceiving.Code),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.Code),
                            DisplayName = Dn("Field.PurchaseReceivingCode", "進貨單號"),
                            FilterPlaceholder = Fp("Field.PurchaseReceivingCode", "輸入進貨單號搜尋"),
                            TableOrder = 1,
                            Width = "130px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PurchaseReceiving.Code), pr => pr.Code)
                        }
                    },
                    {
                        nameof(PurchaseReceiving.BatchNumber),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.BatchNumber),
                            DisplayName = Dn("Field.BatchNumber", "批號"),
                            FilterPlaceholder = Fp("Field.BatchNumber", "輸入批號搜尋"),
                            TableOrder = 2,
                            Width = "120px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PurchaseReceiving.BatchNumber), pr => pr.BatchNumber)
                        }
                    },
                    {
                        "SupplierName",
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = "Supplier.CompanyName", // 直接使用廠商關聯顯示廠商名稱
                            DisplayName = Dn("Field.Supplier", "廠商"),
                            TableOrder = 3,
                            Width = "160px",
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = Fp("Field.Supplier", "輸入廠商名稱搜尋"),
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "SupplierName", pr => pr.Supplier != null ? pr.Supplier.CompanyName : "")
                        }
                    },
                    {
                        nameof(PurchaseReceiving.ReceiptDate),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.ReceiptDate),
                            DisplayName = Dn("Field.ReceiptDate", "進貨日期"),
                            ColumnType = ColumnDataType.Date,
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 4,
                            Width = "110px",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(PurchaseReceiving.ReceiptDate), pr => pr.ReceiptDate)
                        }
                    },
                    {
                        nameof(PurchaseReceiving.PurchaseReceivingTotalAmountIncludingTax),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.PurchaseReceivingTotalAmountIncludingTax),
                            DisplayName = Dn("Field.TotalAmount", "總額"),
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 5,
                            Width = "110px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var purchaseReceiving = (PurchaseReceiving)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "text-success fw-bold");
                                builder.AddContent(2, purchaseReceiving.PurchaseReceivingTotalAmountIncludingTax.ToString("N0"));
                                builder.CloseElement();
                            }
                        }
                    }
                };

                if (_enableApproval)
                {
                    fields.Add(nameof(PurchaseReceiving.IsApproved),
                        new FieldDefinition<PurchaseReceiving>
                        {
                            PropertyName = nameof(PurchaseReceiving.IsApproved),
                            DisplayName = Dn("Field.ApprovalStatus", "核准狀態"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 6,
                            Width = "100px",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "已核准", Value = "true" },
                                new SelectOption { Text = "未核准", Value = "false" }
                            },
                            CustomTemplate = item => builder =>
                            {
                                var pr = (PurchaseReceiving)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", pr.IsApproved ? "badge bg-success" : !string.IsNullOrEmpty(pr.RejectReason) ? "badge bg-danger" : "badge bg-warning");
                                builder.AddContent(2, pr.ApprovalStatusText);
                                builder.CloseElement();
                            },
                            FilterFunction = (model, query) =>
                            {
                                var value = model.GetFilterValue(nameof(PurchaseReceiving.IsApproved))?.ToString();
                                if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool boolValue))
                                    query = query.Where(pr => pr.IsApproved == boolValue);
                                return query;
                            }
                        });
                }

                return fields;
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
    }
}
