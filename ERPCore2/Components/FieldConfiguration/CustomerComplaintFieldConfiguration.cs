using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Models.Enums;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 客戶投訴紀錄欄位配置
    /// </summary>
    public class CustomerComplaintFieldConfiguration : BaseFieldConfiguration<CustomerComplaint>
    {
        private readonly INotificationService? _notificationService;

        public CustomerComplaintFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<CustomerComplaint>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<CustomerComplaint>>
                {
                    {
                        nameof(CustomerComplaint.ComplaintDate),
                        new FieldDefinition<CustomerComplaint>
                        {
                            PropertyName = nameof(CustomerComplaint.ComplaintDate),
                            DisplayName = Dn("Field.ComplaintDate", "投訴日期"),
                            FilterPlaceholder = Fp("Field.ComplaintDate", "選擇投訴日期"),
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(CustomerComplaint.ComplaintDate), c => c.ComplaintDate)
                        }
                    },
                    {
                        nameof(CustomerComplaint.CustomerId),
                        new FieldDefinition<CustomerComplaint>
                        {
                            PropertyName = "Customer.CompanyName",
                            FilterPropertyName = nameof(CustomerComplaint.CustomerId),
                            DisplayName = Dn("Field.Customer", "客戶"),
                            FilterPlaceholder = Fp("Field.Customer", "輸入客戶名稱搜尋"),
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(CustomerComplaint.CustomerId), c => c.CustomerId)
                        }
                    },
                    {
                        nameof(CustomerComplaint.Title),
                        new FieldDefinition<CustomerComplaint>
                        {
                            PropertyName = nameof(CustomerComplaint.Title),
                            DisplayName = Dn("Field.ComplaintTitle", "投訴標題"),
                            FilterPlaceholder = Fp("Field.ComplaintTitle", "輸入投訴標題搜尋"),
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CustomerComplaint.Title), c => c.Title)
                        }
                    },
                    {
                        nameof(CustomerComplaint.Category),
                        new FieldDefinition<CustomerComplaint>
                        {
                            PropertyName = nameof(CustomerComplaint.Category),
                            DisplayName = Dn("Field.ComplaintCategory", "投訴類別"),
                            ShowInFilter = false,
                            TableOrder = 4,
                            CustomTemplate = item =>
                            {
                                var complaint = item as CustomerComplaint;
                                var text = complaint?.Category switch
                                {
                                    ComplaintCategory.ItemQuality => L?["ComplaintCategory.ItemQuality"].ToString() ?? "產品品質",
                                    ComplaintCategory.DeliveryDelay  => L?["ComplaintCategory.DeliveryDelay"].ToString() ?? "交期延誤",
                                    ComplaintCategory.ServiceAttitude => L?["ComplaintCategory.ServiceAttitude"].ToString() ?? "服務態度",
                                    ComplaintCategory.PriceDispute   => L?["ComplaintCategory.PriceDispute"].ToString() ?? "價格爭議",
                                    ComplaintCategory.Other          => L?["ComplaintCategory.Other"].ToString() ?? "其他",
                                    _ => complaint?.Category.ToString() ?? "-"
                                };
                                return builder => builder.AddContent(0, text ?? "-");
                            },
                            FilterFunction = (model, query) => query
                        }
                    },
                    {
                        nameof(CustomerComplaint.ComplaintStatus),
                        new FieldDefinition<CustomerComplaint>
                        {
                            PropertyName = nameof(CustomerComplaint.ComplaintStatus),
                            DisplayName = Dn("Field.ComplaintStatus", "處理狀態"),
                            ShowInFilter = false,
                            TableOrder = 5,
                            CustomTemplate = item =>
                            {
                                var complaint = item as CustomerComplaint;
                                var (text, cssClass) = complaint?.ComplaintStatus switch
                                {
                                    ComplaintStatus.Open       => (L?["ComplaintStatus.Open"].ToString() ?? "待處理", "badge bg-danger"),
                                    ComplaintStatus.InProgress => (L?["ComplaintStatus.InProgress"].ToString() ?? "處理中", "badge bg-warning text-dark"),
                                    ComplaintStatus.Resolved   => (L?["ComplaintStatus.Resolved"].ToString() ?? "已解決", "badge bg-success"),
                                    ComplaintStatus.Closed     => (L?["ComplaintStatus.Closed"].ToString() ?? "已關閉", "badge bg-secondary"),
                                    _ => (complaint?.ComplaintStatus.ToString() ?? "-", "badge bg-secondary")
                                };
                                return builder =>
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", cssClass);
                                    builder.AddContent(2, text);
                                    builder.CloseElement();
                                };
                            },
                            FilterFunction = (model, query) => query
                        }
                    },
                    {
                        nameof(CustomerComplaint.EmployeeId),
                        new FieldDefinition<CustomerComplaint>
                        {
                            PropertyName = "Employee.Name",
                            FilterPropertyName = nameof(CustomerComplaint.EmployeeId),
                            DisplayName = Dn("Field.ResponsibleEmployee", "負責人員"),
                            FilterPlaceholder = Fp("Field.ResponsibleEmployee", "選擇負責人員"),
                            TableOrder = 6,
                            FilterOrder = 4,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(CustomerComplaint.EmployeeId), c => c.EmployeeId ?? 0)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "客戶投訴紀錄欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("客戶投訴紀錄欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<CustomerComplaint>>();
            }
        }
    }
}
