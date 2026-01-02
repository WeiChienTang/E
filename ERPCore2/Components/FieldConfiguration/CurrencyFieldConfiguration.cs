using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using ERPCore2.Components.Shared.PageTemplate;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 貨幣欄位配置
    /// </summary>
    public class CurrencyFieldConfiguration : BaseFieldConfiguration<Currency>
    {
        private readonly INotificationService? _notificationService;
        
        public CurrencyFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Currency>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Currency>>
                {
                    {
                        nameof(Currency.Code),
                        new FieldDefinition<Currency>
                        {
                            PropertyName = nameof(Currency.Code),
                            DisplayName = "貨幣代碼",
                            FilterPlaceholder = "輸入貨幣代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Currency.Code), c => c.Code)
                        }
                    },
                    {
                        nameof(Currency.Name),
                        new FieldDefinition<Currency>
                        {
                            PropertyName = nameof(Currency.Name),
                            DisplayName = "貨幣名稱",
                            FilterPlaceholder = "輸入貨幣名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Currency.Name), c => c.Name)
                        }
                    },
                    {
                        nameof(Currency.Symbol),
                        new FieldDefinition<Currency>
                        {
                            PropertyName = nameof(Currency.Symbol),
                            DisplayName = "符號",
                            ShowInFilter = false,
                            TableOrder = 3,
                            HeaderStyle = "width: 100px;",
                            NullDisplayText = "-"
                        }
                    },
                    {
                        nameof(Currency.ExchangeRate),
                        new FieldDefinition<Currency>
                        {
                            PropertyName = nameof(Currency.ExchangeRate),
                            DisplayName = "匯率",
                            ShowInFilter = false,
                            TableOrder = 4,
                            HeaderStyle = "width: 150px;",
                            CustomTemplate = item => builder =>
                            {
                                var currency = (Currency)item;
                                builder.OpenElement(0, "span");
                                if (currency.IsBaseCurrency)
                                {
                                    builder.AddAttribute(1, "class", "badge bg-primary");
                                    builder.AddContent(2, "本位幣");
                                }
                                else
                                {
                                    builder.AddContent(1, currency.ExchangeRate.ToString("N4"));
                                }
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(Currency.IsBaseCurrency),
                        new FieldDefinition<Currency>
                        {
                            PropertyName = nameof(Currency.IsBaseCurrency),
                            DisplayName = "本位幣",
                            ShowInFilter = true,
                            TableOrder = 5,
                            FilterOrder = 3,
                            HeaderStyle = "width: 120px;",
                            FilterType = SearchFilterType.Select,
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "非本位幣", Value = "false" },
                                new SelectOption { Text = "本位幣", Value = "true" }
                            },
                            CustomTemplate = item => builder =>
                            {
                                var currency = (Currency)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", currency.IsBaseCurrency ? "badge bg-success" : "badge bg-secondary");
                                builder.AddContent(2, currency.IsBaseCurrency ? "是" : "否");
                                builder.CloseElement();
                            },
                            FilterFunction = (model, query) => {
                                var filterValue = model.GetFilterValue(nameof(Currency.IsBaseCurrency))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && bool.TryParse(filterValue, out var isBaseCurrency))
                                {
                                    return query.Where(c => c.IsBaseCurrency == isBaseCurrency);
                                }
                                return query;
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化貨幣欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化貨幣欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Currency>>();
            }
        }
    }
}


