using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 廠商欄位配置
    /// </summary>
    public class SupplierFieldConfiguration : BaseFieldConfiguration<Supplier>
    {
        private readonly INotificationService? _notificationService;
        
        public SupplierFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Supplier>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Supplier>>
                {
                    {
                        nameof(Supplier.Code),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.Code),
                            DisplayName = Dn("Field.SupplierCode", "廠商編號"),
                            FilterPlaceholder = Fp("Field.SupplierCode", "輸入廠商編號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Supplier.Code), s => s.Code)
                        }
                    },
                    {
                        nameof(Supplier.SupplierType),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.SupplierType),
                            DisplayName = Dn("Field.SupplierType", "廠商類型"),
                            FilterType = SearchFilterType.Select,
                            FilterPlaceholder = "選擇類型",
                            TableOrder = 2,
                            Options = new List<SelectOption>
                            {
                                new() { Value = ((int)SupplierType.Manufacturer).ToString(),          Text = L?["SupplierType.Manufacturer"].ToString() ?? "製造商" },
                                new() { Value = ((int)SupplierType.Trader).ToString(),                Text = L?["SupplierType.Trader"].ToString() ?? "貿易商" },
                                new() { Value = ((int)SupplierType.Agent).ToString(),                 Text = L?["SupplierType.Agent"].ToString() ?? "代理商" },
                                new() { Value = ((int)SupplierType.ServiceProvider).ToString(),       Text = L?["SupplierType.ServiceProvider"].ToString() ?? "服務商" }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var val = model.GetFilterValue(nameof(Supplier.SupplierType))?.ToString();
                                if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out var intVal))
                                {
                                    var type = (SupplierType)intVal;
                                    query = query.Where(s => s.SupplierType == type);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var supplier = (Supplier)item;
                                var (cssClass, text) = supplier.SupplierType switch
                                {
                                    SupplierType.Manufacturer   => ("bg-primary",   L?["SupplierType.Manufacturer"].ToString()   ?? "製造商"),
                                    SupplierType.Trader         => ("bg-info",      L?["SupplierType.Trader"].ToString()         ?? "貿易商"),
                                    SupplierType.Agent          => ("bg-warning",   L?["SupplierType.Agent"].ToString()          ?? "代理商"),
                                    SupplierType.ServiceProvider => ("bg-success",  L?["SupplierType.ServiceProvider"].ToString() ?? "服務商"),
                                    _                           => ("bg-secondary", L?["Label.Unknown"].ToString()                ?? "未知")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", $"badge text-white {cssClass}");
                                builder.AddContent(2, text);
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(Supplier.CompanyName),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.CompanyName),
                            DisplayName = Dn("Field.CompanyName", "公司名稱"),
                            FilterPlaceholder = Fp("Field.CompanyName", "輸入公司名稱搜尋"),
                            TableOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Supplier.CompanyName), s => s.CompanyName)
                        }
                    },
                    {
                        nameof(Supplier.ContactPerson),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.ContactPerson),
                            DisplayName = Dn("Field.ContactPerson", "聯絡人"),
                            FilterPlaceholder = Fp("Field.ContactPerson", "輸入聯絡人姓名搜尋"),
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Supplier.ContactPerson), s => s.ContactPerson, allowNull: true)
                        }
                    },
                    {
                        nameof(Supplier.TaxNumber),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.TaxNumber),
                            DisplayName = Dn("Field.TaxNumber", "統一編號"),
                            FilterPlaceholder = Fp("Field.TaxNumber", "輸入統一編號搜尋"),
                            TableOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Supplier.TaxNumber), s => s.TaxNumber, allowNull: true)
                        }
                    },
                    {
                        nameof(Supplier.SupplierStatus),
                        new FieldDefinition<Supplier>
                        {
                            PropertyName = nameof(Supplier.SupplierStatus),
                            DisplayName = Dn("Field.SupplierStatus", "廠商狀態"),
                            TableOrder = 5,
                            FilterType = SearchFilterType.Select,
                            FilterPlaceholder = "選擇狀態",
                            Options = new List<SelectOption>
                            {
                                new() { Value = ((int)SupplierStatus.Active).ToString(),         Text = L?["SupplierStatus.Active"].ToString() ?? "正常往來" },
                                new() { Value = ((int)SupplierStatus.Inactive).ToString(),       Text = L?["SupplierStatus.Inactive"].ToString() ?? "停用" },
                                new() { Value = ((int)SupplierStatus.Suspended).ToString(),      Text = L?["SupplierStatus.Suspended"].ToString() ?? "暫停往來" }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var val = model.GetFilterValue(nameof(Supplier.SupplierStatus))?.ToString();
                                if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out var intVal))
                                {
                                    var status = (SupplierStatus)intVal;
                                    query = query.Where(s => s.SupplierStatus == status);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var supplier = (Supplier)item;
                                var (cssClass, text) = supplier.SupplierStatus switch
                                {
                                    SupplierStatus.Active    => ("bg-success",   L?["SupplierStatus.Active"].ToString()    ?? "正常往來"),
                                    SupplierStatus.Inactive  => ("bg-secondary", L?["SupplierStatus.Inactive"].ToString()  ?? "停用"),
                                    SupplierStatus.Suspended => ("bg-warning",   L?["SupplierStatus.Suspended"].ToString() ?? "暫停往來"),
                                    _                        => ("bg-secondary", L?["Label.Unknown"].ToString()             ?? "未知")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", $"badge text-white {cssClass}");
                                builder.AddContent(2, text);
                                builder.CloseElement();
                            }
                        }
                    },

                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType());
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化廠商欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Supplier>>();
            }
        }
    }
}


