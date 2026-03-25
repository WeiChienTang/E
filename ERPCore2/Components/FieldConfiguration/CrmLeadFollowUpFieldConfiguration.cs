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
    /// 潛在客戶跟進紀錄欄位配置
    /// </summary>
    public class CrmLeadFollowUpFieldConfiguration : BaseFieldConfiguration<CrmLeadFollowUp>
    {
        private readonly INotificationService? _notificationService;

        public CrmLeadFollowUpFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<CrmLeadFollowUp>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<CrmLeadFollowUp>>
                {
                    {
                        nameof(CrmLeadFollowUp.FollowUpDate),
                        new FieldDefinition<CrmLeadFollowUp>
                        {
                            PropertyName = nameof(CrmLeadFollowUp.FollowUpDate),
                            DisplayName = Dn("Field.FollowUpDate", "跟進日期"),
                            FilterType = SearchFilterType.DateRange,
                            ColumnType = ColumnDataType.Date,
                            TableOrder = 1,
                            Width = "110px",
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(CrmLeadFollowUp.FollowUpDate), v => v.FollowUpDate)
                        }
                    },
                    {
                        nameof(CrmLeadFollowUp.CrmLeadId),
                        new FieldDefinition<CrmLeadFollowUp>
                        {
                            PropertyName = "CrmLead.CompanyName",
                            FilterPropertyName = nameof(CrmLeadFollowUp.CrmLeadId),
                            DisplayName = Dn("Entity.CrmLead", "潛在客戶"),
                            FilterPlaceholder = Fp("Entity.CrmLead", "輸入潛在客戶搜尋"),
                            TableOrder = 2,
                            Width = "160px",
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(CrmLeadFollowUp.CrmLeadId), v => v.CrmLeadId)
                        }
                    },
                    {
                        nameof(CrmLeadFollowUp.FollowUpType),
                        new FieldDefinition<CrmLeadFollowUp>
                        {
                            PropertyName = nameof(CrmLeadFollowUp.FollowUpType),
                            DisplayName = Dn("Field.FollowUpType", "跟進方式"),
                            ShowInFilter = false,
                            TableOrder = 3,
                            Width = "100px",
                            CustomTemplate = item =>
                            {
                                var f = item as CrmLeadFollowUp;
                                var text = f?.FollowUpType switch
                                {
                                    VisitType.Phone          => "電話",
                                    VisitType.OnSite         => "現場拜訪",
                                    VisitType.VideoConference => "視訊",
                                    VisitType.Email          => "Email",
                                    VisitType.Other          => "其他",
                                    _                        => "-"
                                };
                                return builder => builder.AddContent(0, text ?? "-");
                            },
                            FilterFunction = (model, query) => query
                        }
                    },
                    {
                        nameof(CrmLeadFollowUp.EmployeeId),
                        new FieldDefinition<CrmLeadFollowUp>
                        {
                            PropertyName = "Employee.Name",
                            FilterPropertyName = nameof(CrmLeadFollowUp.EmployeeId),
                            DisplayName = Dn("Field.SalesPerson", "跟進人員"),
                            FilterPlaceholder = Fp("Field.SalesPerson", "選擇跟進人員"),
                            TableOrder = 4,
                            Width = "110px",
                            FilterOrder = 3,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(CrmLeadFollowUp.EmployeeId), v => v.EmployeeId ?? 0)
                        }
                    },
                    {
                        nameof(CrmLeadFollowUp.Content),
                        new FieldDefinition<CrmLeadFollowUp>
                        {
                            PropertyName = nameof(CrmLeadFollowUp.Content),
                            DisplayName = Dn("Field.FollowUpContent", "跟進內容"),
                            FilterPlaceholder = Fp("Field.FollowUpContent", "輸入跟進內容搜尋"),
                            TableOrder = 5,
                            Width = "180px",
                            FilterOrder = 4,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CrmLeadFollowUp.Content), v => v.Content, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "跟進紀錄欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("跟進紀錄欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<CrmLeadFollowUp>>();
            }
        }
    }
}
