using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
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
                            DisplayName = Dn("Field.BankCode", "銀行編號"),
                            FilterPlaceholder = Fp("Field.BankCode", "輸入銀行編號搜尋"),
                            TableOrder = 0,
                            Width = "120px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Bank.Code), b => b.Code)
                        }  
                    },
                    {
                        nameof(Bank.BankName),
                        new FieldDefinition<Bank>
                        {
                            PropertyName = nameof(Bank.BankName),
                            DisplayName = Dn("Field.BankName", "銀行名稱"),
                            FilterPlaceholder = Fp("Field.BankName", "輸入銀行名稱搜尋"),
                            TableOrder = 1,
                            Width = "150px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Bank.BankName), b => b.BankName)
                        }
                    },
                    {
                        nameof(Bank.BankNameEn),
                        new FieldDefinition<Bank>
                        {
                            PropertyName = nameof(Bank.BankNameEn),
                            DisplayName = Dn("Field.EnglishName", "英文名稱"),
                            FilterPlaceholder = Fp("Field.EnglishName", "輸入英文名稱搜尋"),
                            TableOrder = 2,
                            Width = "150px",
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
                            DisplayName = Dn("Field.SwiftCode", "SWIFT編號"),
                            FilterPlaceholder = Fp("Field.SwiftCode", "輸入SWIFT編號搜尋"),
                            TableOrder = 3,
                            Width = "120px",
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Bank.SwiftCode), b => b.SwiftCode)
                        }
                    },
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


