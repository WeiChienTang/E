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
    /// 銀行對帳單欄位配置
    /// </summary>
    public class BankStatementFieldConfiguration : BaseFieldConfiguration<BankStatement>
    {
        private readonly INotificationService? _notificationService;

        public BankStatementFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<BankStatement>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<BankStatement>>
                {
                    {
                        nameof(BankStatement.CompanyBankAccountId),
                        new FieldDefinition<BankStatement>
                        {
                            PropertyName = nameof(BankStatement.CompanyBankAccountId),
                            DisplayName = Dn("Field.BankAccount", "銀行帳號"),
                            TableOrder = 0,
                            Width = "140px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var bs = (BankStatement)item;
                                var text = bs.CompanyBankAccount != null
                                    ? $"{bs.CompanyBankAccount.Bank?.BankName ?? ""} {bs.CompanyBankAccount.AccountNumber} ({bs.CompanyBankAccount.AccountName})"
                                    : "-";
                                builder.AddContent(0, text);
                            }
                        }
                    },
                    {
                        nameof(BankStatement.StatementDate),
                        new FieldDefinition<BankStatement>
                        {
                            PropertyName = nameof(BankStatement.StatementDate),
                            DisplayName = Dn("Field.StatementDate", "對帳單日期"),
                            TableOrder = 1,
                            Width = "110px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var bs = (BankStatement)item;
                                builder.AddContent(0, bs.StatementDate.ToString("yyyy/MM/dd"));
                            }
                        }
                    },
                    {
                        "Period",
                        new FieldDefinition<BankStatement>
                        {
                            PropertyName = "Period",
                            DisplayName = Dn("Field.ReconciliationPeriod", "對帳期間"),
                            TableOrder = 2,
                            Width = "160px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var bs = (BankStatement)item;
                                builder.AddContent(0, $"{bs.PeriodStart:yyyy/MM/dd} ~ {bs.PeriodEnd:yyyy/MM/dd}");
                            }
                        }
                    },
                    {
                        nameof(BankStatement.OpeningBalance),
                        new FieldDefinition<BankStatement>
                        {
                            PropertyName = nameof(BankStatement.OpeningBalance),
                            DisplayName = Dn("Field.OpeningBalance", "期初餘額"),
                            TableOrder = 3,
                            Width = "120px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var bs = (BankStatement)item;
                                builder.AddContent(0, bs.OpeningBalance.ToString("N2"));
                            }
                        }
                    },
                    {
                        nameof(BankStatement.ClosingBalance),
                        new FieldDefinition<BankStatement>
                        {
                            PropertyName = nameof(BankStatement.ClosingBalance),
                            DisplayName = Dn("Field.ClosingBalance", "期末餘額"),
                            TableOrder = 4,
                            Width = "120px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var bs = (BankStatement)item;
                                builder.AddContent(0, bs.ClosingBalance.ToString("N2"));
                            }
                        }
                    },
                    {
                        "MatchStatus",
                        new FieldDefinition<BankStatement>
                        {
                            PropertyName = "MatchStatus",
                            DisplayName = Dn("Field.MatchStatus", "配對狀態"),
                            TableOrder = 5,
                            Width = "100px",
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var bs = (BankStatement)item;
                                var text = bs.TotalLines == 0 ? "-" : $"{bs.MatchedLines}/{bs.TotalLines}";
                                builder.AddContent(0, text);
                            }
                        }
                    },
                    {
                        nameof(BankStatement.StatementStatus),
                        new FieldDefinition<BankStatement>
                        {
                            PropertyName = nameof(BankStatement.StatementStatus),
                            DisplayName = Dn("Field.StatementStatus", "對帳狀態"),
                            TableOrder = 6,
                            Width = "100px",
                            FilterType = SearchFilterType.Select,
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = L?["BankStatement.Draft"].ToString() ?? "草稿",     Value = ((int)BankStatementStatus.Draft).ToString() },
                                new SelectOption { Text = L?["BankStatement.InProgress"].ToString() ?? "對帳中", Value = ((int)BankStatementStatus.InProgress).ToString() },
                                new SelectOption { Text = L?["BankStatement.Completed"].ToString() ?? "已完成", Value = ((int)BankStatementStatus.Completed).ToString() }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(BankStatement.StatementStatus))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out var v))
                                {
                                    var status = (BankStatementStatus)v;
                                    return query.Where(bs => bs.StatementStatus == status);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var bs = (BankStatement)item;
                                var (text, css) = bs.StatementStatus switch
                                {
                                    BankStatementStatus.Draft      => (L?["BankStatement.Draft"].ToString() ?? "草稿",     "badge bg-secondary"),
                                    BankStatementStatus.InProgress => (L?["BankStatement.InProgress"].ToString() ?? "對帳中", "badge bg-warning text-dark"),
                                    BankStatementStatus.Completed  => (L?["BankStatement.Completed"].ToString() ?? "已完成", "badge bg-success"),
                                    _ => (bs.StatementStatus.ToString(), "badge bg-secondary")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", css);
                                builder.AddContent(2, text);
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化銀行對帳單欄位配置失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化銀行對帳單欄位配置時發生錯誤");
                    });
                }

                return new Dictionary<string, FieldDefinition<BankStatement>>();
            }
        }
    }
}
