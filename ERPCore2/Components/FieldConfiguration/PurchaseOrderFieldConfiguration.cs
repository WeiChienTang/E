using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.PageTemplate;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 採購單欄位配置
    /// </summary>
    public class PurchaseOrderFieldConfiguration : BaseFieldConfiguration<PurchaseOrder>
    {
        private readonly List<Supplier> _suppliers;
        private readonly List<Company> _companies;
        private readonly INotificationService? _notificationService;
        private readonly bool _enableApproval;

        public PurchaseOrderFieldConfiguration(
            List<Supplier> suppliers, 
            List<Company> companies, 
            bool enableApproval = false,
            INotificationService? notificationService = null)
        {
            _suppliers = suppliers;
            _companies = companies;
            _enableApproval = enableApproval;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PurchaseOrder>> GetFieldDefinitions()
        {
            try
            {
                var fields = new Dictionary<string, FieldDefinition<PurchaseOrder>>
                {
                    {
                        nameof(PurchaseOrder.Code),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.Code),
                            DisplayName = "採購單號",
                            FilterPlaceholder = "輸入採購單號搜尋",
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(

                                model, query, nameof(PurchaseOrder.Code), po => po.Code)
                        }
                    },
                    {
                        nameof(PurchaseOrder.CompanyId),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = "Company.CompanyName", // 表格顯示用
                            FilterPropertyName = "Company.CompanyName", // 篩選器使用公司名稱
                            DisplayName = "採購公司",
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = "輸入公司名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "Company.CompanyName", po => po.Company.CompanyName)
                        }
                    },
                    {
                        nameof(PurchaseOrder.SupplierId),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = "Supplier.CompanyName", // 表格顯示用
                            FilterPropertyName = "Supplier.CompanyName", // 篩選器也使用公司名稱
                            DisplayName = "廠商",
                            FilterType = SearchFilterType.Text,
                            FilterPlaceholder = "輸入廠商名稱搜尋",
                            TableOrder = 3,
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
                            TableOrder = 4,
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
                            TableOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableDateRangeFilter(
                                model, query, nameof(PurchaseOrder.ExpectedDeliveryDate), po => po.ExpectedDeliveryDate)
                        }
                    },
                    {
                        nameof(PurchaseOrder.PurchaseTotalAmountIncludingTax),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.PurchaseTotalAmountIncludingTax),
                            DisplayName = "總額",
                            ColumnType = ColumnDataType.Number,
                            TableOrder = 6,
                            ShowInFilter = false, // 通常不會用金額篩選
                            CustomTemplate = item => builder =>
                            {
                                var purchaseOrder = (PurchaseOrder)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "text-success fw-bold");
                                builder.AddContent(2, purchaseOrder.PurchaseTotalAmountIncludingTax.ToString("N0"));
                                builder.CloseElement();
                            }
                        }
                    }
                };

                // 只有在啟用審核時才加入核准狀態欄位
                if (_enableApproval)
                {
                    fields.Add(nameof(PurchaseOrder.IsApproved),
                        new FieldDefinition<PurchaseOrder>
                        {
                            PropertyName = nameof(PurchaseOrder.IsApproved),
                            DisplayName = "核准狀態",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 7,
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "已核准", Value = "true" },
                                new SelectOption { Text = "未核准", Value = "false" }
                            },
                            CustomTemplate = item => builder =>
                            {
                                var purchaseOrder = (PurchaseOrder)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", purchaseOrder.IsApproved ? "badge bg-success" : "badge bg-warning");
                                builder.AddContent(2, purchaseOrder.IsApproved ? "已核准" : "待核准");
                                builder.CloseElement();
                            },
                            FilterFunction = (model, query) => {
                                var value = model.GetFilterValue(nameof(PurchaseOrder.IsApproved))?.ToString();
                                if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool boolValue))
                                {
                                    query = query.Where(po => po.IsApproved == boolValue);
                                }
                                return query;
                            }
                        });
                }

                return fields;
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
    }
}
