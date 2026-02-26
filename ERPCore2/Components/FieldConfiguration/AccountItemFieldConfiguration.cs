using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using ERPCore2.Services;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 會計科目欄位配置
    /// </summary>
    public class AccountItemFieldConfiguration : BaseFieldConfiguration<AccountItem>
    {
        private readonly INotificationService? _notificationService;

        public AccountItemFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<AccountItem>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<AccountItem>>
                {
                    {
                        nameof(AccountItem.Code),
                        new FieldDefinition<AccountItem>
                        {
                            PropertyName = nameof(AccountItem.Code),
                            DisplayName = "科目代碼",
                            FilterPlaceholder = "輸入科目代碼搜尋",
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(AccountItem.Code), a => a.Code)
                        }
                    },
                    {
                        nameof(AccountItem.Name),
                        new FieldDefinition<AccountItem>
                        {
                            PropertyName = nameof(AccountItem.Name),
                            DisplayName = "科目名稱",
                            FilterPlaceholder = "輸入科目名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(AccountItem.Name), a => a.Name)
                        }
                    },
                    {
                        nameof(AccountItem.AccountType),
                        new FieldDefinition<AccountItem>
                        {
                            PropertyName = nameof(AccountItem.AccountType),
                            DisplayName = "科目大類",
                            TableOrder = 4,
                            FilterType = SearchFilterType.Select,
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "資產", Value = ((int)AccountType.Asset).ToString() },
                                new SelectOption { Text = "負債", Value = ((int)AccountType.Liability).ToString() },
                                new SelectOption { Text = "權益", Value = ((int)AccountType.Equity).ToString() },
                                new SelectOption { Text = "營業收入", Value = ((int)AccountType.Revenue).ToString() },
                                new SelectOption { Text = "營業成本", Value = ((int)AccountType.Cost).ToString() },
                                new SelectOption { Text = "營業費用", Value = ((int)AccountType.Expense).ToString() },
                                new SelectOption { Text = "營業外收益及費損", Value = ((int)AccountType.NonOperatingIncomeAndExpense).ToString() },
                                new SelectOption { Text = "綜合損益總額", Value = ((int)AccountType.ComprehensiveIncome).ToString() }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(AccountItem.AccountType))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out var typeValue) &&
                                    Enum.IsDefined(typeof(AccountType), typeValue))
                                {
                                    var accountType = (AccountType)typeValue;
                                    return query.Where(a => a.AccountType == accountType);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var account = (AccountItem)item;
                                var (text, cssClass) = account.AccountType switch
                                {
                                    AccountType.Asset => ("資產", "badge bg-primary"),
                                    AccountType.Liability => ("負債", "badge bg-danger"),
                                    AccountType.Equity => ("權益", "badge bg-success"),
                                    AccountType.Revenue => ("營業收入", "badge bg-info"),
                                    AccountType.Cost => ("營業成本", "badge bg-warning text-dark"),
                                    AccountType.Expense => ("營業費用", "badge bg-secondary"),
                                    AccountType.NonOperatingIncomeAndExpense => ("營業外", "badge bg-dark"),
                                    AccountType.ComprehensiveIncome => ("綜合損益", "badge bg-light text-dark border"),
                                    _ => (account.AccountType.ToString(), "badge bg-secondary")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", cssClass);
                                builder.AddContent(2, text);
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(AccountItem.Direction),
                        new FieldDefinition<AccountItem>
                        {
                            PropertyName = nameof(AccountItem.Direction),
                            DisplayName = "借貸方向",
                            TableOrder = 5,
                            FilterType = SearchFilterType.Select,
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "借方", Value = ((int)AccountDirection.Debit).ToString() },
                                new SelectOption { Text = "貸方", Value = ((int)AccountDirection.Credit).ToString() }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(AccountItem.Direction))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out var dirValue) &&
                                    Enum.IsDefined(typeof(AccountDirection), dirValue))
                                {
                                    var direction = (AccountDirection)dirValue;
                                    return query.Where(a => a.Direction == direction);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var account = (AccountItem)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", account.Direction == AccountDirection.Debit
                                    ? "badge bg-primary"
                                    : "badge bg-success");
                                builder.AddContent(2, account.Direction == AccountDirection.Debit ? "借方" : "貸方");
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(AccountItem.IsDetailAccount),
                        new FieldDefinition<AccountItem>
                        {
                            PropertyName = nameof(AccountItem.IsDetailAccount),
                            DisplayName = "明細科目",
                            TableOrder = 6,
                            FilterType = SearchFilterType.Select,
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "是", Value = "true" },
                                new SelectOption { Text = "否", Value = "false" }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(AccountItem.IsDetailAccount))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && bool.TryParse(filterValue, out var isDetail))
                                    return query.Where(a => a.IsDetailAccount == isDetail);
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var account = (AccountItem)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", account.IsDetailAccount
                                    ? "badge bg-success"
                                    : "badge bg-secondary");
                                builder.AddContent(2, account.IsDetailAccount ? "是" : "否");
                                builder.CloseElement();
                            }
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(
                        ex, nameof(GetFieldDefinitions), GetType(),
                        additionalData: "初始化會計科目欄位配置失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化會計科目欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<AccountItem>>();
            }
        }
    }
}
