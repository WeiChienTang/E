using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using System.ComponentModel;
using System.Reflection;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 財務交易欄位配置
    /// </summary>
    public class FinancialTransactionFieldConfiguration : BaseFieldConfiguration<FinancialTransaction>
    {
        private readonly List<Customer> _customers;
        private readonly List<Company> _companies;
        private readonly List<PaymentMethod> _paymentMethods;
        private readonly INotificationService? _notificationService;
        
        public FinancialTransactionFieldConfiguration(
            List<Customer> customers, 
            List<Company> companies,
            List<PaymentMethod> paymentMethods,
            INotificationService? notificationService = null)
        {
            _customers = customers;
            _companies = companies;
            _paymentMethods = paymentMethods;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<FinancialTransaction>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<FinancialTransaction>>
                {
                    {
                        nameof(FinancialTransaction.TransactionNumber),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = nameof(FinancialTransaction.TransactionNumber),
                            DisplayName = "交易單號",
                            FilterPlaceholder = "輸入交易單號搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 140px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(FinancialTransaction.TransactionNumber), ft => ft.TransactionNumber)
                        }
                    },
                    {
                        nameof(FinancialTransaction.TransactionDate),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = nameof(FinancialTransaction.TransactionDate),
                            DisplayName = "交易日期",
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 2,
                            FilterOrder = 2,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(FinancialTransaction.TransactionDate), ft => ft.TransactionDate)
                        }
                    },
                    {
                        nameof(FinancialTransaction.TransactionType),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = nameof(FinancialTransaction.TransactionType),
                            DisplayName = "交易類型",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 120px;",
                            Options = GetTransactionTypeOptions(),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(FinancialTransaction.TransactionType), ft => (int)ft.TransactionType)
                        }
                    },
                    {
                        nameof(FinancialTransaction.Amount),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = nameof(FinancialTransaction.Amount),
                            DisplayName = "交易金額",
                            FilterType = SearchFilterType.NumberRange,
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 120px; text-align: right;",
                            FilterFunction = (model, query) => {
                                var filterValue = model.GetFilterValue(nameof(FinancialTransaction.Amount))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && decimal.TryParse(filterValue, out var amount))
                                {
                                    return query.Where(ft => ft.Amount == amount);
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(FinancialTransaction.CustomerId),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = "Customer.CompanyName", // 用於表格顯示
                            FilterPropertyName = nameof(FinancialTransaction.CustomerId), // 用於篩選器
                            DisplayName = "客戶",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 150px;",
                            Options = _customers.Select(c => new SelectOption 
                            { 
                                Text = c.CompanyName, 
                                Value = c.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(FinancialTransaction.CustomerId), ft => ft.CustomerId)
                        }
                    },
                    {
                        nameof(FinancialTransaction.CompanyId),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = "Company.CompanyName", // 用於表格顯示
                            FilterPropertyName = nameof(FinancialTransaction.CompanyId), // 用於篩選器
                            DisplayName = "公司",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 6,
                            FilterOrder = 6,
                            HeaderStyle = "width: 150px;",
                            Options = _companies.Select(c => new SelectOption 
                            { 
                                Text = c.CompanyName, 
                                Value = c.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(FinancialTransaction.CompanyId), ft => ft.CompanyId)
                        }
                    },
                    {
                        nameof(FinancialTransaction.PaymentMethodId),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = "PaymentMethod.Name", // 用於表格顯示
                            FilterPropertyName = nameof(FinancialTransaction.PaymentMethodId), // 用於篩選器
                            DisplayName = "收付款方式",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 7,
                            FilterOrder = 7,
                            HeaderStyle = "width: 120px;",
                            Options = _paymentMethods.Select(pm => new SelectOption 
                            { 
                                Text = pm.Name, 
                                Value = pm.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(FinancialTransaction.PaymentMethodId), ft => ft.PaymentMethodId)
                        }
                    },
                    {
                        nameof(FinancialTransaction.SourceDocumentType),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = nameof(FinancialTransaction.SourceDocumentType),
                            DisplayName = "來源單據類型",
                            FilterPlaceholder = "輸入來源單據類型搜尋",
                            TableOrder = 8,
                            FilterOrder = 8,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(FinancialTransaction.SourceDocumentType), ft => ft.SourceDocumentType, allowNull: true)
                        }
                    },
                    {
                        nameof(FinancialTransaction.SourceDocumentNumber),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = nameof(FinancialTransaction.SourceDocumentNumber),
                            DisplayName = "來源單據號碼",
                            FilterPlaceholder = "輸入來源單據號碼搜尋",
                            TableOrder = 9,
                            FilterOrder = 9,
                            HeaderStyle = "width: 130px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(FinancialTransaction.SourceDocumentNumber), ft => ft.SourceDocumentNumber, allowNull: true)
                        }
                    },
                    {
                        nameof(FinancialTransaction.BalanceAfter),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = nameof(FinancialTransaction.BalanceAfter),
                            DisplayName = "交易後餘額",
                            FilterType = SearchFilterType.NumberRange,
                            TableOrder = 10,
                            FilterOrder = 10,
                            HeaderStyle = "width: 120px; text-align: right;",
                            FilterFunction = (model, query) => {
                                var filterValue = model.GetFilterValue(nameof(FinancialTransaction.BalanceAfter))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && decimal.TryParse(filterValue, out var balance))
                                {
                                    return query.Where(ft => ft.BalanceAfter == balance);
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(FinancialTransaction.IsReversed),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = nameof(FinancialTransaction.IsReversed),
                            DisplayName = "沖銷狀態",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 11,
                            FilterOrder = 11,
                            HeaderStyle = "width: 90px; text-align: center;",
                            Options = new List<SelectOption>
                            {
                                new SelectOption { Text = "未沖銷", Value = "false" },
                                new SelectOption { Text = "已沖銷", Value = "true" }
                            },
                            FilterFunction = (model, query) => {
                                var filterValue = model.GetFilterValue(nameof(FinancialTransaction.IsReversed))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && bool.TryParse(filterValue, out var isReversed))
                                {
                                    return query.Where(ft => ft.IsReversed == isReversed);
                                }
                                return query;
                            }
                        }
                    },
                    {
                        nameof(FinancialTransaction.Description),
                        new FieldDefinition<FinancialTransaction>
                        {
                            PropertyName = nameof(FinancialTransaction.Description),
                            DisplayName = "交易描述",
                            FilterPlaceholder = "輸入交易描述搜尋",
                            TableOrder = 12,
                            FilterOrder = 12,
                            HeaderStyle = "width: 200px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(FinancialTransaction.Description), ft => ft.Description, allowNull: true)
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
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "建立財務交易欄位配置失敗");
                        await _notificationService.ShowErrorAsync("建立財務交易欄位配置失敗");
                    });
                }
                
                // 回傳安全的預設配置
                return new Dictionary<string, FieldDefinition<FinancialTransaction>>();
            }
        }
        
        
        protected override Func<IQueryable<FinancialTransaction>, IQueryable<FinancialTransaction>> GetDefaultSort()
        {
            return query => query.OrderByDescending(ft => ft.TransactionDate)
                                 .ThenByDescending(ft => ft.TransactionNumber);
        }
        
        /// <summary>
        /// 取得交易類型選項
        /// </summary>
        private List<SelectOption> GetTransactionTypeOptions()
        {
            return Enum.GetValues<FinancialTransactionTypeEnum>()
                .Select(e => new SelectOption
                {
                    Text = GetEnumDescription(e),
                    Value = ((int)e).ToString()
                })
                .ToList();
        }
        
        /// <summary>
        /// 取得枚舉描述
        /// </summary>
        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }
    }
}