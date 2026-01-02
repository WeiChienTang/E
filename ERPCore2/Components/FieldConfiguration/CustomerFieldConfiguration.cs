using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.PageTemplate;

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
                            DisplayName = "客戶代碼",
                            FilterPlaceholder = "輸入客戶代碼搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 180px;",
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
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Customer.ContactPerson), c => c.ContactPerson)
                        }
                    },
                    {
                        nameof(Customer.TaxNumber),
                        new FieldDefinition<Customer>
                        {
                            PropertyName = nameof(Customer.TaxNumber),
                            DisplayName = "統一編號",
                            FilterPlaceholder = "輸入統一編號搜尋",
                            TableOrder = 4,
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


