using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Components.Shared.UI.Badge;
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
    /// 客戶欄位配置
    /// </summary>
    public class CustomerFieldConfiguration : BaseFieldConfiguration<Customer>
    {
        private readonly INotificationService? _notificationService;
        
        public CustomerFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Customer>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Customer>>
                {
                    {
                        nameof(Customer.Code),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.Code),
                            DisplayName = Dn("Field.CustomerCode", "客戶編號"),
                            FilterPlaceholder = Fp("Field.CustomerCode", "輸入客戶編號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.Code), c => c.Code)
                        }
                    },
                    {
                        nameof(Customer.CompanyName),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.CompanyName),
                            DisplayName = Dn("Field.CompanyName", "公司名稱"),
                            FilterPlaceholder = Fp("Field.CompanyName", "輸入公司名稱搜尋"),
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.CompanyName), c => c.CompanyName)
                        }
                    },
                    {
                        nameof(Customer.ContactPerson),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.ContactPerson),
                            DisplayName = Dn("Field.ContactPerson", "聯絡人"),
                            FilterPlaceholder = Fp("Field.ContactPerson", "輸入聯絡人姓名搜尋"),
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.ContactPerson), c => c.ContactPerson)
                        }
                    },
                    {
                        nameof(Customer.TaxNumber),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.TaxNumber),
                            DisplayName = Dn("Field.TaxNumber", "統一編號"),
                            FilterPlaceholder = Fp("Field.TaxNumber", "輸入統一編號搜尋"),
                            TableOrder = 4,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.TaxNumber), c => c.TaxNumber, allowNull: true)
                        }
                    },
                    {
                        nameof(Customer.CustomerStatus),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.CustomerStatus),
                            DisplayName = Dn("Field.CustomerStatus", "客戶狀態"),
                            TableOrder = 5,
                            FilterType = SearchFilterType.Select,
                            FilterPlaceholder = "選擇狀態",
                            Options = new List<SelectOption>
                            {
                                new() { Value = "",                                             Text = "全部" },
                                new() { Value = ((int)CustomerStatus.Active).ToString(),      Text = "正常往來" },
                                new() { Value = ((int)CustomerStatus.Inactive).ToString(),    Text = "停用" },
                                new() { Value = ((int)CustomerStatus.Blacklisted).ToString(), Text = "黑名單" }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var val = model.GetFilterValue(nameof(Customer.CustomerStatus))?.ToString();
                                if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out var intVal))
                                {
                                    var status = (CustomerStatus)intVal;
                                    query = query.Where(c => c.CustomerStatus == status);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var customer = (Customer)item;
                                var (cssClass, text) = customer.CustomerStatus switch
                                {
                                    CustomerStatus.Active      => ("bg-success", "正常往來"),
                                    CustomerStatus.Inactive    => ("bg-secondary", "停用"),
                                    CustomerStatus.Blacklisted => ("bg-danger", "黑名單"),
                                    _                          => ("bg-secondary", "未知")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", $"badge text-white {cssClass}");
                                builder.AddContent(2, text);
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType());
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化客戶欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Customer>>();
            }
        }
    }
}


