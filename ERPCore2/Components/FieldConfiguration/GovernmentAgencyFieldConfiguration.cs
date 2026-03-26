using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 公家機關欄位配置
    /// </summary>
    public class GovernmentAgencyFieldConfiguration : BaseFieldConfiguration<GovernmentAgency>
    {
        private readonly INotificationService? _notificationService;

        public GovernmentAgencyFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<GovernmentAgency>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<GovernmentAgency>>
                {
                    {
                        nameof(GovernmentAgency.Code),
                        new FieldDefinition<GovernmentAgency>
                        {
                            PropertyName = nameof(GovernmentAgency.Code),
                            DisplayName = Dn("Field.GovernmentAgencyCode", "機關編號"),
                            FilterPlaceholder = Fp("Field.GovernmentAgencyCode", "輸入機關編號搜尋"),
                            TableOrder = 1,
                            Width = "120px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(GovernmentAgency.Code), g => g.Code)
                        }
                    },
                    {
                        nameof(GovernmentAgency.AgencyType),
                        new FieldDefinition<GovernmentAgency>
                        {
                            PropertyName = nameof(GovernmentAgency.AgencyType),
                            DisplayName = Dn("Field.AgencyType", "機關類型"),
                            FilterType = SearchFilterType.Select,
                            FilterPlaceholder = "選擇類型",
                            TableOrder = 2,
                            Width = "100px",
                            Options = new List<SelectOption>
                            {
                                new() { Value = ((int)GovernmentAgencyType.Central).ToString(),    Text = L?["GovernmentAgencyType.Central"].ToString() ?? "中央機關" },
                                new() { Value = ((int)GovernmentAgencyType.Municipal).ToString(),  Text = L?["GovernmentAgencyType.Municipal"].ToString() ?? "直轄市機關" },
                                new() { Value = ((int)GovernmentAgencyType.County).ToString(),     Text = L?["GovernmentAgencyType.County"].ToString() ?? "縣市機關" },
                                new() { Value = ((int)GovernmentAgencyType.Township).ToString(),   Text = L?["GovernmentAgencyType.Township"].ToString() ?? "鄉鎮市區機關" },
                                new() { Value = ((int)GovernmentAgencyType.Other).ToString(),      Text = L?["GovernmentAgencyType.Other"].ToString() ?? "其他" }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var val = model.GetFilterValue(nameof(GovernmentAgency.AgencyType))?.ToString();
                                if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out var intVal))
                                {
                                    var type = (GovernmentAgencyType)intVal;
                                    query = query.Where(g => g.AgencyType == type);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var agency = (GovernmentAgency)item;
                                var (cssClass, text) = agency.AgencyType switch
                                {
                                    GovernmentAgencyType.Central   => ("bg-primary",   L?["GovernmentAgencyType.Central"].ToString()   ?? "中央機關"),
                                    GovernmentAgencyType.Municipal => ("bg-info",      L?["GovernmentAgencyType.Municipal"].ToString() ?? "直轄市機關"),
                                    GovernmentAgencyType.County    => ("bg-success",   L?["GovernmentAgencyType.County"].ToString()    ?? "縣市機關"),
                                    GovernmentAgencyType.Township  => ("bg-warning",   L?["GovernmentAgencyType.Township"].ToString()  ?? "鄉鎮市區機關"),
                                    GovernmentAgencyType.Other     => ("bg-secondary", L?["GovernmentAgencyType.Other"].ToString()     ?? "其他"),
                                    _                              => ("bg-secondary", L?["Label.Unknown"].ToString()                   ?? "未知")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", $"badge text-white {cssClass}");
                                builder.AddContent(2, text);
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        nameof(GovernmentAgency.AgencyName),
                        new FieldDefinition<GovernmentAgency>
                        {
                            PropertyName = nameof(GovernmentAgency.AgencyName),
                            DisplayName = Dn("Field.AgencyName", "機關名稱"),
                            FilterPlaceholder = Fp("Field.AgencyName", "輸入機關名稱搜尋"),
                            TableOrder = 3,
                            Width = "160px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(GovernmentAgency.AgencyName), g => g.AgencyName)
                        }
                    },
                    {
                        nameof(GovernmentAgency.ContactPerson),
                        new FieldDefinition<GovernmentAgency>
                        {
                            PropertyName = nameof(GovernmentAgency.ContactPerson),
                            DisplayName = Dn("Field.ContactPerson", "聯絡人"),
                            FilterPlaceholder = Fp("Field.ContactPerson", "輸入聯絡人姓名搜尋"),
                            TableOrder = 4,
                            Width = "100px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(GovernmentAgency.ContactPerson), g => g.ContactPerson, allowNull: true)
                        }
                    },
                    {
                        nameof(GovernmentAgency.AgencyCode),
                        new FieldDefinition<GovernmentAgency>
                        {
                            PropertyName = nameof(GovernmentAgency.AgencyCode),
                            DisplayName = Dn("Field.AgencyCode", "機關代碼"),
                            FilterPlaceholder = Fp("Field.AgencyCode", "輸入機關代碼搜尋"),
                            TableOrder = 5,
                            Width = "120px",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(GovernmentAgency.AgencyCode), g => g.AgencyCode, allowNull: true)
                        }
                    },
                    {
                        nameof(GovernmentAgency.AgencyStatus),
                        new FieldDefinition<GovernmentAgency>
                        {
                            PropertyName = nameof(GovernmentAgency.AgencyStatus),
                            DisplayName = Dn("Field.AgencyStatus", "機關狀態"),
                            TableOrder = 6,
                            Width = "100px",
                            FilterType = SearchFilterType.Select,
                            FilterPlaceholder = "選擇狀態",
                            Options = new List<SelectOption>
                            {
                                new() { Value = ((int)GovernmentAgencyStatus.Active).ToString(),   Text = L?["GovernmentAgencyStatus.Active"].ToString() ?? "正常往來" },
                                new() { Value = ((int)GovernmentAgencyStatus.Inactive).ToString(), Text = L?["GovernmentAgencyStatus.Inactive"].ToString() ?? "停用" }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var val = model.GetFilterValue(nameof(GovernmentAgency.AgencyStatus))?.ToString();
                                if (!string.IsNullOrWhiteSpace(val) && int.TryParse(val, out var intVal))
                                {
                                    var status = (GovernmentAgencyStatus)intVal;
                                    query = query.Where(g => g.AgencyStatus == status);
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var agency = (GovernmentAgency)item;
                                var (cssClass, text) = agency.AgencyStatus switch
                                {
                                    GovernmentAgencyStatus.Active   => ("bg-success",   L?["GovernmentAgencyStatus.Active"].ToString()   ?? "正常往來"),
                                    GovernmentAgencyStatus.Inactive => ("bg-secondary", L?["GovernmentAgencyStatus.Inactive"].ToString() ?? "停用"),
                                    _                               => ("bg-secondary", L?["Label.Unknown"].ToString()                    ?? "未知")
                                };
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", $"badge text-white {cssClass}");
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType());
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化公家機關欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<GovernmentAgency>>();
            }
        }
    }
}
