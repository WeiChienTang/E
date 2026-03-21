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
    /// 潛在客戶欄位配置
    /// </summary>
    public class CrmLeadFieldConfiguration : BaseFieldConfiguration<CrmLead>
    {
        private readonly INotificationService? _notificationService;

        public CrmLeadFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<CrmLead>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<CrmLead>>
                {
                    {
                        nameof(CrmLead.CompanyName),
                        new FieldDefinition<CrmLead>
                        {
                            PropertyName = nameof(CrmLead.CompanyName),
                            DisplayName = Dn("Field.CompanyName", "公司名稱"),
                            FilterPlaceholder = Fp("Field.CompanyName", "輸入公司名稱搜尋"),
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CrmLead.CompanyName), v => v.CompanyName)
                        }
                    },
                    {
                        nameof(CrmLead.LeadStage),
                        new FieldDefinition<CrmLead>
                        {
                            PropertyName = nameof(CrmLead.LeadStage),
                            DisplayName = Dn("Field.LeadStage", "開發階段"),
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterType = SearchFilterType.Select,
                            Options = Enum.GetValues(typeof(LeadStage))
                                .Cast<LeadStage>()
                                .Select(e => new SelectOption
                                {
                                    Text = GetLeadStageText(e),
                                    Value = ((int)e).ToString()
                                })
                                .ToList(),
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(CrmLead.LeadStage))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out var stageValue) &&
                                    Enum.IsDefined(typeof(LeadStage), stageValue))
                                {
                                    var stage = (LeadStage)stageValue;
                                    return query.Where(v => v.LeadStage == stage);
                                }
                                return query;
                            },
                            CustomTemplate = item =>
                            {
                                var lead = item as CrmLead;
                                var (text, cssClass) = lead?.LeadStage switch
                                {
                                    LeadStage.Cold       => (L?["LeadStage.Cold"].ToString() ?? "冷接觸", "badge bg-secondary"),
                                    LeadStage.Interested => (L?["LeadStage.Interested"].ToString() ?? "有意願", "badge bg-info text-dark"),
                                    LeadStage.Quoting    => (L?["LeadStage.Quoting"].ToString() ?? "報價中", "badge bg-warning text-dark"),
                                    LeadStage.Won        => (L?["LeadStage.Won"].ToString() ?? "成交", "badge bg-success"),
                                    LeadStage.Lost       => (L?["LeadStage.Lost"].ToString() ?? "流失", "badge bg-danger"),
                                    _                    => ("-", "")
                                };
                                return builder =>
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", cssClass);
                                    builder.AddContent(2, text);
                                    builder.CloseElement();
                                };
                            }
                        }
                    },
                    {
                        nameof(CrmLead.LeadSource),
                        new FieldDefinition<CrmLead>
                        {
                            PropertyName = nameof(CrmLead.LeadSource),
                            DisplayName = Dn("Field.LeadSource", "來源"),
                            TableOrder = 3,
                            FilterOrder = 3,
                            FilterType = SearchFilterType.Select,
                            Options = Enum.GetValues(typeof(LeadSource))
                                .Cast<LeadSource>()
                                .Select(e => new SelectOption
                                {
                                    Text = GetLeadSourceText(e),
                                    Value = ((int)e).ToString()
                                })
                                .ToList(),
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(CrmLead.LeadSource))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && int.TryParse(filterValue, out var sourceValue) &&
                                    Enum.IsDefined(typeof(LeadSource), sourceValue))
                                {
                                    var source = (LeadSource)sourceValue;
                                    return query.Where(v => v.LeadSource == source);
                                }
                                return query;
                            },
                            CustomTemplate = item =>
                            {
                                var lead = item as CrmLead;
                                var text = lead?.LeadSource switch
                                {
                                    LeadSource.BusinessDevelopment => L?["LeadSource.BusinessDevelopment"].ToString() ?? "業務開發",
                                    LeadSource.Referral            => L?["LeadSource.Referral"].ToString() ?? "客戶推薦",
                                    LeadSource.Exhibition          => L?["LeadSource.Exhibition"].ToString() ?? "展覽",
                                    LeadSource.Internet            => L?["LeadSource.Internet"].ToString() ?? "網路",
                                    LeadSource.Other               => L?["LeadSource.Other"].ToString() ?? "其他",
                                    _                              => "-"
                                };
                                return builder => builder.AddContent(0, text);
                            }
                        }
                    },
                    {
                        nameof(CrmLead.AssignedEmployeeId),
                        new FieldDefinition<CrmLead>
                        {
                            PropertyName = "AssignedEmployee.Name",
                            FilterPropertyName = nameof(CrmLead.AssignedEmployeeId),
                            DisplayName = Dn("Field.AssignedEmployee", "負責業務員"),
                            FilterPlaceholder = Fp("Field.AssignedEmployee", "選擇業務員"),
                            TableOrder = 4,
                            FilterOrder = 4,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyIntIdFilter(
                                model, query, nameof(CrmLead.AssignedEmployeeId), v => v.AssignedEmployeeId ?? 0)
                        }
                    },
                    {
                        nameof(CrmLead.ContactPerson),
                        new FieldDefinition<CrmLead>
                        {
                            PropertyName = nameof(CrmLead.ContactPerson),
                            DisplayName = Dn("Field.ContactPerson", "聯絡人"),
                            FilterPlaceholder = Fp("Field.ContactPerson", "輸入聯絡人搜尋"),
                            TableOrder = 5,
                            FilterOrder = 5,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CrmLead.ContactPerson), v => v.ContactPerson, allowNull: true)
                        }
                    },
                    {
                        nameof(CrmLead.Industry),
                        new FieldDefinition<CrmLead>
                        {
                            PropertyName = nameof(CrmLead.Industry),
                            DisplayName = Dn("Field.Industry", "行業"),
                            FilterPlaceholder = Fp("Field.Industry", "輸入行業搜尋"),
                            TableOrder = 6,
                            FilterOrder = 6,
                            ShowInTable = false,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(CrmLead.Industry), v => v.Industry, allowNull: true)
                        }
                    },
                    {
                        nameof(CrmLead.ConvertedCustomerId),
                        new FieldDefinition<CrmLead>
                        {
                            PropertyName = nameof(CrmLead.ConvertedCustomerId),
                            DisplayName = Dn("Field.ConvertedCustomer", "已轉換"),
                            ShowInFilter = false,
                            TableOrder = 7,
                            CustomTemplate = item =>
                            {
                                var lead = item as CrmLead;
                                var isConverted = lead?.ConvertedCustomerId.HasValue == true;
                                return builder =>
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", isConverted ? "badge bg-success" : "badge bg-light text-dark");
                                    builder.AddContent(2, isConverted ? "已轉換" : "未轉換");
                                    builder.CloseElement();
                                };
                            },
                            FilterFunction = (model, query) => query
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "潛在客戶欄位配置初始化失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("潛在客戶欄位配置初始化失敗，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<CrmLead>>();
            }
        }

        private static string GetLeadStageText(LeadStage stage) => stage switch
        {
            LeadStage.Cold       => "冷接觸",
            LeadStage.Interested => "有意願",
            LeadStage.Quoting    => "報價中",
            LeadStage.Won        => "成交",
            LeadStage.Lost       => "流失",
            _                    => stage.ToString()
        };

        private static string GetLeadSourceText(LeadSource source) => source switch
        {
            LeadSource.BusinessDevelopment => "業務開發",
            LeadSource.Referral            => "客戶推薦",
            LeadSource.Exhibition          => "展覽",
            LeadSource.Internet            => "網路",
            LeadSource.Other               => "其他",
            _                              => source.ToString()
        };
    }
}
