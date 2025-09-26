using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Tables;
using ERPCore2.FieldConfiguration;
using ERPCore2.Helpers;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Data.Enums;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 應收帳款沖款單欄位配置類別
    /// </summary>
    public class AccountsReceivableSetoffFieldConfiguration : BaseFieldConfiguration<AccountsReceivableSetoff>
    {
        private readonly INotificationService? _notificationService;
        private readonly List<Customer> _customers;
        private readonly List<PaymentMethod> _paymentMethods;

        public AccountsReceivableSetoffFieldConfiguration(
            List<Customer> customers,
            List<PaymentMethod> paymentMethods,
            INotificationService? notificationService = null)
        {
            _customers = customers;
            _paymentMethods = paymentMethods;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<AccountsReceivableSetoff>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<AccountsReceivableSetoff>>
                {
                    {
                        nameof(AccountsReceivableSetoff.SetoffNumber),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.SetoffNumber),
                            DisplayName = "沖款單號",
                            FilterPlaceholder = "輸入沖款單號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 150px;"
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.SetoffDate),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.SetoffDate),
                            DisplayName = "沖款日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 2,
                            FilterOrder = 2,
                            HeaderStyle = "width: 120px;"
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.CustomerId),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(AccountsReceivableSetoff.CustomerId),
                            DisplayName = "客戶",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 200px;",
                            Options = _customers.Select(c => new SelectOption 
                            { 
                                Text = c.CompanyName, 
                                Value = c.Id.ToString() 
                            }).ToList()
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.TotalSetoffAmount),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.TotalSetoffAmount),
                            DisplayName = "總沖款金額",
                            FilterType = SearchFilterType.NumberRange,
                            ColumnType = ColumnDataType.Currency,
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 120px; text-align: right;"
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.PaymentMethodId),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = "PaymentMethod.Name",
                            FilterPropertyName = nameof(AccountsReceivableSetoff.PaymentMethodId),
                            DisplayName = "收款方式",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 120px;",
                            ShowInFilter = true,
                            Options = _paymentMethods.Select(p => new SelectOption 
                            { 
                                Text = p.Name, 
                                Value = p.Id.ToString() 
                            }).ToList()
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.IsCompleted),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.IsCompleted),
                            DisplayName = "完成狀態",
                            FilterType = SearchFilterType.Select,
                            ColumnType = ColumnDataType.Boolean,
                            TableOrder = 6,
                            FilterOrder = 6,
                            HeaderStyle = "width: 100px; text-align: center;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "未完成", Value = "false" },
                                new SelectOption { Text = "已完成", Value = "true" }
                            }
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.ApprovalStatus),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.ApprovalStatus),
                            DisplayName = "審核狀態",
                            FilterType = SearchFilterType.Select,
                            ColumnType = ColumnDataType.Enum,
                            TableOrder = 7,
                            FilterOrder = 7,
                            HeaderStyle = "width: 100px; text-align: center;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "待審核", Value = ((int)ApprovalStatus.Pending).ToString() },
                                new SelectOption { Text = "已核准", Value = ((int)ApprovalStatus.Approved).ToString() },
                                new SelectOption { Text = "已拒絕", Value = ((int)ApprovalStatus.Rejected).ToString() }
                            }
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.ApproverId),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = "Approver.Name",
                            FilterPropertyName = nameof(AccountsReceivableSetoff.ApproverId),
                            DisplayName = "審核者",
                            TableOrder = 8,
                            FilterOrder = 8,
                            ShowInFilter = false,
                            HeaderStyle = "width: 120px;"
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.ApprovedDate),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.ApprovedDate),
                            DisplayName = "審核日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.DateTime,
                            TableOrder = 9,
                            FilterOrder = 9,
                            ShowInFilter = false,
                            HeaderStyle = "width: 140px;"
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.CompletedDate),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.CompletedDate),
                            DisplayName = "完成日期",
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.DateTime,
                            TableOrder = 10,
                            FilterOrder = 10,
                            ShowInFilter = false,
                            HeaderStyle = "width: 140px;"
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.PaymentAccount),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.PaymentAccount),
                            DisplayName = "收款帳戶",
                            FilterPlaceholder = "輸入收款帳戶",
                            TableOrder = 11,
                            FilterOrder = 11,
                            ShowInFilter = false,
                            HeaderStyle = "width: 150px;"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 錯誤處理
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await ErrorHandlingHelper.HandleServiceErrorAsync(
                            ex, nameof(GetFieldDefinitions), GetType());
                        await _notificationService.ShowErrorAsync("欄位配置載入失敗");
                    });
                }

                // 返回安全的後備配置
                return new Dictionary<string, FieldDefinition<AccountsReceivableSetoff>>();
            }
        }

        /// <summary>
        /// 自訂預設排序：依沖款日期遞減，然後依沖款單號
        /// </summary>
        protected override Func<IQueryable<AccountsReceivableSetoff>, IQueryable<AccountsReceivableSetoff>> GetDefaultSort()
        {
            return query => query.OrderByDescending(s => s.SetoffDate)
                                .ThenBy(s => s.SetoffNumber);
        }
    }
}