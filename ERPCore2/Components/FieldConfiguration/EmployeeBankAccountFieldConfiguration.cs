using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    public class EmployeeBankAccountFieldConfiguration : BaseFieldConfiguration<EmployeeBankAccount>
    {
        private readonly INotificationService? _notificationService;

        public EmployeeBankAccountFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<EmployeeBankAccount>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<EmployeeBankAccount>>
                {
                    {
                        "EmployeeName",
                        new FieldDefinition<EmployeeBankAccount>
                        {
                            PropertyName = "EmployeeName",
                            DisplayName = Dn("Field.Employee", "員工"),
                            FilterPlaceholder = Fp("Field.Employee", "輸入員工姓名搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "EmployeeName", x => x.Employee != null ? x.Employee.Name ?? "" : ""),
                            CustomTemplate = item => builder =>
                            {
                                var entity = (EmployeeBankAccount)item;
                                builder.OpenElement(0, "div");
                                builder.AddContent(1, entity.Employee?.Name ?? "—");
                                if (entity.Employee?.Code != null)
                                {
                                    builder.OpenElement(2, "small");
                                    builder.AddAttribute(3, "class", "text-muted ms-1");
                                    builder.AddContent(4, $"({entity.Employee.Code})");
                                    builder.CloseElement();
                                }
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(EmployeeBankAccount.BankCode),
                        new FieldDefinition<EmployeeBankAccount>
                        {
                            PropertyName = nameof(EmployeeBankAccount.BankCode),
                            DisplayName = Dn("Field.BankCode", "銀行代號"),
                            FilterPlaceholder = Fp("Field.BankCode", "輸入銀行代號搜尋"),
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(EmployeeBankAccount.BankCode), x => x.BankCode),
                            CustomTemplate = item => builder =>
                            {
                                var entity = (EmployeeBankAccount)item;
                                builder.AddContent(0, entity.BankCode);
                                if (!string.IsNullOrEmpty(entity.BankName))
                                {
                                    builder.OpenElement(1, "small");
                                    builder.AddAttribute(2, "class", "text-muted ms-1");
                                    builder.AddContent(3, entity.BankName);
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        nameof(EmployeeBankAccount.AccountNumber),
                        new FieldDefinition<EmployeeBankAccount>
                        {
                            PropertyName = nameof(EmployeeBankAccount.AccountNumber),
                            DisplayName = Dn("Field.AccountNumber", "帳號"),
                            TableOrder = 3,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (EmployeeBankAccount)item;
                                // 遮蔽帳號中間碼
                                var num = entity.AccountNumber;
                                var masked = num.Length > 6
                                    ? num[..3] + new string('*', num.Length - 6) + num[^3..]
                                    : num;
                                builder.AddContent(0, masked);
                            }
                        }
                    },
                    {
                        nameof(EmployeeBankAccount.AccountName),
                        new FieldDefinition<EmployeeBankAccount>
                        {
                            PropertyName = nameof(EmployeeBankAccount.AccountName),
                            DisplayName = Dn("Field.BankAccountHolder", "戶名"),
                            TableOrder = 4,
                            ShowInFilter = false
                        }
                    },
                    {
                        nameof(EmployeeBankAccount.IsPrimary),
                        new FieldDefinition<EmployeeBankAccount>
                        {
                            PropertyName = nameof(EmployeeBankAccount.IsPrimary),
                            DisplayName = Dn("Field.IsPrimary", "主要帳號"),
                            TableOrder = 5,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (EmployeeBankAccount)item;
                                if (entity.IsPrimary)
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", "badge bg-success");
                                    builder.AddContent(2, L?["Payroll.PrimaryAccount"].ToString() ?? "主要");
                                    builder.CloseElement();
                                }
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType());
                });
                if (_notificationService != null)
                    _ = Task.Run(async () => await _notificationService.ShowErrorAsync("初始化銀行帳戶欄位配置時發生錯誤"));
                return new Dictionary<string, FieldDefinition<EmployeeBankAccount>>();
            }
        }
    }
}
