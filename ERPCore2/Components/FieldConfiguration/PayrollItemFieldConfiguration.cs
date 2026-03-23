using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    public class PayrollItemFieldConfiguration : BaseFieldConfiguration<PayrollItem>
    {
        private readonly INotificationService? _notificationService;

        public PayrollItemFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PayrollItem>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<PayrollItem>>
                {
                    {
                        nameof(PayrollItem.Code),
                        new FieldDefinition<PayrollItem>
                        {
                            PropertyName = nameof(PayrollItem.Code),
                            DisplayName = Dn("Field.PayrollItemCode", "代碼"),
                            FilterPlaceholder = Fp("Field.PayrollItemCode", "輸入代碼搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PayrollItem.Code), x => x.Code)
                        }
                    },
                    {
                        nameof(PayrollItem.Name),
                        new FieldDefinition<PayrollItem>
                        {
                            PropertyName = nameof(PayrollItem.Name),
                            DisplayName = Dn("Field.PayrollItemName", "名稱"),
                            FilterPlaceholder = Fp("Field.PayrollItemName", "輸入名稱搜尋"),
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PayrollItem.Name), x => x.Name)
                        }
                    },
                    {
                        nameof(PayrollItem.ItemType),
                        new FieldDefinition<PayrollItem>
                        {
                            PropertyName = nameof(PayrollItem.ItemType),
                            DisplayName = Dn("Field.PayrollItemType", "類型"),
                            TableOrder = 3,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (PayrollItem)item;
                                var (text, css) = entity.ItemType switch
                                {
                                    PayrollItemType.Income    => (L?["Payroll.Income"].ToString()    ?? "收入", "badge bg-success"),
                                    PayrollItemType.Deduction => (L?["Payroll.Deduction"].ToString() ?? "扣除", "badge bg-danger"),
                                    _ => ("—", "text-muted")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", css);
                                builder.AddContent(2, text);
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(PayrollItem.Category),
                        new FieldDefinition<PayrollItem>
                        {
                            PropertyName = nameof(PayrollItem.Category),
                            DisplayName = Dn("Field.PayrollItemCategory", "類別"),
                            TableOrder = 4,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (PayrollItem)item;
                                var text = entity.Category switch
                                {
                                    PayrollItemCategory.Salary    => L?["Payroll.Salary"].ToString()    ?? "薪資",
                                    PayrollItemCategory.Allowance => L?["Payroll.Allowance"].ToString() ?? "津貼補助",
                                    PayrollItemCategory.Overtime  => L?["Payroll.Overtime"].ToString()  ?? "加班費",
                                    PayrollItemCategory.Bonus     => L?["Payroll.Bonus"].ToString()     ?? "獎金",
                                    PayrollItemCategory.Legal     => L?["Payroll.Legal"].ToString()     ?? "法定扣繳",
                                    PayrollItemCategory.Other     => L?["Payroll.Other"].ToString()     ?? "其他",
                                    _ => "—"
                                };
                                builder.AddContent(0, text);
                            }
                        }
                    },
                    {
                        nameof(PayrollItem.IsProrated),
                        new FieldDefinition<PayrollItem>
                        {
                            PropertyName = nameof(PayrollItem.IsProrated),
                            DisplayName = Dn("Field.PayrollItemIsProrated", "按出勤比例"),
                            TableOrder = 5,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (PayrollItem)item;
                                if (entity.IsProrated)
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", "badge bg-info");
                                    builder.AddContent(2, L?["Payroll.Prorated"].ToString() ?? "按比例");
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        nameof(PayrollItem.IsSystemItem),
                        new FieldDefinition<PayrollItem>
                        {
                            PropertyName = nameof(PayrollItem.IsSystemItem),
                            DisplayName = Dn("Field.PayrollItemIsSystem", "系統內建"),
                            TableOrder = 6,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (PayrollItem)item;
                                if (entity.IsSystemItem)
                                {
                                    builder.OpenElement(0, "i");
                                    builder.AddAttribute(1, "class", "bi bi-lock-fill text-secondary");
                                    builder.AddAttribute(2, "title", L?["Payroll.SystemItemLocked"].ToString() ?? "系統內建，不可刪除");
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        nameof(PayrollItem.Status),
                        new FieldDefinition<PayrollItem>
                        {
                            PropertyName = nameof(PayrollItem.Status),
                            DisplayName = Dn("Field.Status", "狀態"),
                            TableOrder = 7,
                            ShowInFilter = false,
                            CustomTemplate = item => builder =>
                            {
                                var entity = (PayrollItem)item;
                                var (text, css) = entity.Status == EntityStatus.Active
                                    ? (L?["Toggle.Enabled"].ToString()  ?? "啟用", "badge bg-success")
                                    : (L?["Toggle.Disabled"].ToString() ?? "停用", "badge bg-secondary");
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", css);
                                builder.AddContent(2, text);
                                builder.CloseElement();
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
                    _ = Task.Run(async () => await _notificationService.ShowErrorAsync("初始化薪資項目欄位配置時發生錯誤"));
                return new Dictionary<string, FieldDefinition<PayrollItem>>();
            }
        }
    }
}
