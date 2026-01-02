using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.PageTemplate;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 銀行欄位配置
    /// </summary>
    public class BankFieldConfiguration : BaseFieldConfiguration<Bank>
    {
        private readonly INotificationService? _notificationService;
        
        public BankFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Bank>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Bank>>
                {
                    {
                      nameof(Bank.Code),
                        new FieldDefinition<Bank>
                        {
                            PropertyName = nameof(Bank.Code),
                            DisplayName = "銀行代碼",
                            FilterPlaceholder = "輸入銀行代碼搜尋",
                            TableOrder = 0,
                            HeaderStyle = "width: 100px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Bank.Code), b => b.Code)
                        }  
                    },
                    {
                        nameof(Bank.BankName),
                        new FieldDefinition<Bank>
                        {
                            PropertyName = nameof(Bank.BankName),
                            DisplayName = "銀行名稱",
                            FilterPlaceholder = "輸入銀行名稱搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 200px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Bank.BankName), b => b.BankName)
                        }
                    },
                    {
                        nameof(Bank.BankNameEn),
                        new FieldDefinition<Bank>
                        {
                            PropertyName = nameof(Bank.BankNameEn),
                            DisplayName = "英文名稱",
                            FilterPlaceholder = "輸入英文名稱搜尋",
                            TableOrder = 2,
                            HeaderStyle = "width: 200px;",
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Bank.BankNameEn), b => b.BankNameEn)
                        }
                    },
                    {
                        nameof(Bank.SwiftCode),
                        new FieldDefinition<Bank>
                        {
                            PropertyName = nameof(Bank.SwiftCode),
                            DisplayName = "SWIFT代碼",
                            FilterPlaceholder = "輸入SWIFT代碼搜尋",
                            TableOrder = 3,
                            HeaderStyle = "width: 150px;",
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Bank.SwiftCode), b => b.SwiftCode)
                        }
                    },
                    {
                        nameof(Bank.Phone),
                        new FieldDefinition<Bank>
                        {
                            PropertyName = nameof(Bank.Phone),
                            DisplayName = "電話",
                            FilterPlaceholder = "輸入電話搜尋",
                            TableOrder = 4,
                            HeaderStyle = "width: 150px;",
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Bank.Phone), b => b.Phone)
                        }
                    },
                    {
                        nameof(Bank.Address),
                        new FieldDefinition<Bank>
                        {
                            PropertyName = nameof(Bank.Address),
                            DisplayName = "地址",
                            ShowInFilter = false,
                            TableOrder = 5,
                            NullDisplayText = "-"
                        }
                    },
                    {
                        nameof(Bank.Fax),
                        new FieldDefinition<Bank>
                        {
                            PropertyName = nameof(Bank.Fax),
                            DisplayName = "傳真",
                            ShowInFilter = false,
                            TableOrder = 6,
                            NullDisplayText = "-"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化銀行欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化銀行欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Bank>>();
            }
        }
    }
}


