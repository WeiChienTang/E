using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 應收帳款沖抵欄位配置類別
    /// </summary>
    public class AccountsReceivableSetoffFieldConfiguration : BaseFieldConfiguration<AccountsReceivableSetoff>
    {
        private readonly List<Customer> _customers;
        private readonly List<PaymentMethod> _paymentMethods;
        private readonly List<Employee> _employees;
        private readonly INotificationService? _notificationService;

        public AccountsReceivableSetoffFieldConfiguration(
            List<Customer> customers, 
            List<PaymentMethod> paymentMethods,
            List<Employee> employees,
            INotificationService? notificationService = null)
        {
            _customers = customers ?? new List<Customer>();
            _paymentMethods = paymentMethods ?? new List<PaymentMethod>();
            _employees = employees ?? new List<Employee>();
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
                            DisplayName = "沖抵單號",
                            FilterPlaceholder = "輸入沖抵單號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(AccountsReceivableSetoff.SetoffNumber), s => s.SetoffNumber)
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.SetoffDate),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.SetoffDate),
                            DisplayName = "沖抵日期",
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 2,
                            FilterOrder = 2,
                            HeaderStyle = "width: 120px;",
                            CustomTemplate = item => builder =>
                            {
                                var setoff = (AccountsReceivableSetoff)item;
                                builder.OpenElement(0, "span");
                                builder.AddContent(1, setoff.SetoffDate.ToString("yyyy/MM/dd"));
                                builder.CloseElement();
                            },
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(AccountsReceivableSetoff.SetoffDate), s => s.SetoffDate)
                        }
                    },
                    {
                        "Customer",
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(AccountsReceivableSetoff.CustomerId),
                            DisplayName = "客戶",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 200px;",
                            NullDisplayText = "未設定",
                            Options = _customers.Select(c => new SelectOption 
                            { 
                                Text = c.CompanyName, 
                                Value = c.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(AccountsReceivableSetoff.CustomerId), s => s.CustomerId)
                        }
                    },
                    {
                        nameof(AccountsReceivableSetoff.TotalSetoffAmount),
                        new FieldDefinition<AccountsReceivableSetoff>
                        {
                            PropertyName = nameof(AccountsReceivableSetoff.TotalSetoffAmount),
                            DisplayName = "總沖抵金額",
                            FilterType = SearchFilterType.NumberRange,
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 120px; text-align: right;",
                            CustomTemplate = item => builder =>
                            {
                                var setoff = (AccountsReceivableSetoff)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "text-end fw-bold text-primary");
                                builder.AddContent(2, setoff.TotalSetoffAmount.ToString("N0"));
                                builder.CloseElement();
                            },
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(AccountsReceivableSetoff.TotalSetoffAmount), s => s.TotalSetoffAmount.ToString())
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                // 非同步錯誤處理
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(),
                            additionalData: new 
                            { 
                                CustomersCount = _customers?.Count ?? 0, 
                                PaymentMethodsCount = _paymentMethods?.Count ?? 0,
                                EmployeesCount = _employees?.Count ?? 0
                            });
                            
                        if (_notificationService != null)
                        {
                            await _notificationService.ShowErrorAsync("應收帳款沖抵欄位配置初始化失敗，已使用預設配置");
                        }
                    }
                    catch
                    {
                        // 避免錯誤處理本身產生例外
                    }
                });

                // 返回安全的後備配置
                return new Dictionary<string, FieldDefinition<AccountsReceivableSetoff>>();
            }
        }

        /// <summary>
        /// 取得預設排序
        /// </summary>
        protected override Func<IQueryable<AccountsReceivableSetoff>, IQueryable<AccountsReceivableSetoff>> GetDefaultSort()
        {
            try
            {
                return query => query.OrderByDescending(s => s.SetoffDate)
                                    .ThenByDescending(s => s.CreatedAt);
            }
            catch (Exception ex)
            {
                // 非同步錯誤處理
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetDefaultSort), GetType(), new { 
                            ServiceType = GetType().Name,
                            Method = nameof(GetDefaultSort)
                        });
                        
                        if (_notificationService != null)
                            await _notificationService.ShowErrorAsync("載入應收帳款沖抵排序設定時發生錯誤");
                    }
                    catch
                    {
                        // 避免錯誤處理本身產生例外
                    }
                });

                // 回傳安全的預設排序
                return query => query.OrderByDescending(s => s.SetoffDate);
            }
        }
    }
}