using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 公司欄位配置類別
    /// </summary>
    public class CompanyFieldConfiguration : BaseFieldConfiguration<Company>
    {
        private readonly INotificationService? _notificationService;

        public CompanyFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<Company>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Company>>
                {
                    {
                        nameof(Company.Code),
                        new FieldDefinition<Company>
                        {
                            PropertyName = nameof(Company.Code),
                            DisplayName = "公司代碼",
                            FilterPlaceholder = "輸入公司代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Company.Code), c => c.Code)
                        }
                    },
                    {
                        nameof(Company.CompanyName),
                        new FieldDefinition<Company>
                        {
                            PropertyName = nameof(Company.CompanyName),
                            DisplayName = "公司名稱",
                            FilterPlaceholder = "輸入公司名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Company.CompanyName), c => c.CompanyName)
                        }
                    },
                    {
                        nameof(Company.TaxId),
                        new FieldDefinition<Company>
                        {
                            PropertyName = nameof(Company.TaxId),
                            DisplayName = "統一編號",
                            FilterPlaceholder = "輸入統一編號搜尋",
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Company.TaxId), c => c.TaxId)
                        }
                    },
                    {
                        nameof(Company.Representative),
                        new FieldDefinition<Company>
                        {
                            PropertyName = nameof(Company.Representative),
                            DisplayName = "負責人",
                            FilterPlaceholder = "輸入負責人姓名搜尋",
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Company.Representative), c => c.Representative)
                        }
                    },
                    {
                        nameof(Company.Phone),
                        new FieldDefinition<Company>
                        {
                            PropertyName = nameof(Company.Phone),
                            DisplayName = "電話",
                            FilterPlaceholder = "輸入電話號碼搜尋",
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 120px;",
                            ShowInFilter = false, // 電話不常用於篩選
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Company.Phone), c => c.Phone)
                        }
                    },
                    {
                        nameof(Company.IsPrimary),
                        new FieldDefinition<Company>
                        {
                            PropertyName = nameof(Company.IsPrimary),
                            DisplayName = "主要公司",
                            FilterType = SearchFilterType.Boolean,
                            ColumnType = Components.Shared.Tables.ColumnDataType.Boolean,
                            TableOrder = 6,
                            FilterOrder = 6,
                            HeaderStyle = "width: 100px;",
                            CustomTemplate = item => builder =>
                            {
                                var company = (Company)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", company.IsPrimary ? "badge bg-primary" : "badge bg-secondary");
                                builder.AddContent(2, company.IsPrimary ? "主要" : "一般");
                                builder.CloseElement();
                            },
                            FilterFunction = (model, query) => 
                            {
                                var filterValue = model.GetFilterValue(nameof(Company.IsPrimary))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && bool.TryParse(filterValue, out var isPrimary))
                                {
                                    return query.Where(c => c.IsPrimary == isPrimary);
                                }
                                return query;
                            }
                        }
                    },
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "建立公司欄位定義時發生錯誤");
                    if (_notificationService != null)
                    {
                        await _notificationService.ShowErrorAsync("欄位配置載入失敗");
                    }
                });

                // 回傳安全的預設值
                return new Dictionary<string, FieldDefinition<Company>>();
            }
        }
    }
}
