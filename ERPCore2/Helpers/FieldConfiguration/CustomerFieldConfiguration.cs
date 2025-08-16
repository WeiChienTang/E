using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 客戶欄位配置
    /// </summary>
    public class CustomerFieldConfiguration : BaseFieldConfiguration<Customer>
    {
        private readonly List<CustomerType> _customerTypes;
        private readonly INotificationService? _notificationService;
        
        public CustomerFieldConfiguration(List<CustomerType> customerTypes, INotificationService? notificationService = null)
        {
            _customerTypes = customerTypes;
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
                            DisplayName = "客戶代碼",
                            FilterPlaceholder = "輸入客戶代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.Code), c => c.Code)
                        }
                    },
                    {
                        nameof(Customer.CompanyName),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.CompanyName),
                            DisplayName = "公司名稱",
                            FilterPlaceholder = "輸入公司名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.CompanyName), c => c.CompanyName)
                        }
                    },
                    {
                        nameof(Customer.ContactPerson),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.ContactPerson),
                            DisplayName = "聯絡人",
                            FilterPlaceholder = "輸入聯絡人姓名搜尋",
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.ContactPerson), c => c.ContactPerson)
                        }
                    },
                    {
                        nameof(Customer.CustomerTypeId),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = "CustomerType.TypeName", // 用於表格顯示
                            FilterPropertyName = nameof(Customer.CustomerTypeId), // 用於篩選器
                            DisplayName = "客戶類型",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            FilterOrder = 4,
                            Options = _customerTypes.Select(ct => new SelectOption 
                            { 
                                Text = ct.TypeName, 
                                Value = ct.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Customer.CustomerTypeId), c => c.CustomerTypeId)
                        }
                    },
                    {
                        nameof(Customer.TaxNumber),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.TaxNumber),
                            DisplayName = "統一編號",
                            FilterPlaceholder = "輸入統一編號搜尋",
                            TableOrder = 5,
                            FilterOrder = 5,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.TaxNumber), c => c.TaxNumber, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: new { CustomerTypesCount = _customerTypes?.Count ?? 0 });
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
