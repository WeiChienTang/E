using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 沖款單欄位配置
    /// </summary>
    public class SetoffDocumentFieldConfiguration : BaseFieldConfiguration<SetoffDocument>
    {
        private readonly List<Company> _companies;
        private readonly INotificationService? _notificationService;
        
        public SetoffDocumentFieldConfiguration(List<Company> companies, INotificationService? notificationService = null)
        {
            _companies = companies;
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<SetoffDocument>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<SetoffDocument>>
                {
                    {
                        nameof(SetoffDocument.SetoffNumber),
                        new FieldDefinition<SetoffDocument>
                        {
                            PropertyName = nameof(SetoffDocument.SetoffNumber),
                            DisplayName = "沖款單號",
                            FilterPlaceholder = "輸入沖款單號搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 180px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(SetoffDocument.SetoffNumber), s => s.SetoffNumber)
                        }
                    },
                    {
                        nameof(SetoffDocument.SetoffDate),
                        new FieldDefinition<SetoffDocument>
                        {
                            PropertyName = nameof(SetoffDocument.SetoffDate),
                            DisplayName = "沖款日期",
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 3,
                            HeaderStyle = "width: 150px;",
                            CustomTemplate = (context) => builder =>
                            {
                                var setoffDoc = context as SetoffDocument;
                                if (setoffDoc != null)
                                {
                                    builder.AddContent(0, setoffDoc.SetoffDate.ToString("yyyy-MM-dd"));
                                }
                            },
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(SetoffDocument.SetoffDate), s => s.SetoffDate)
                        }
                    },
                    {
                        nameof(SetoffDocument.RelatedPartyName),
                        new FieldDefinition<SetoffDocument>
                        {
                            PropertyName = nameof(SetoffDocument.RelatedPartyName),
                            DisplayName = "關聯方名稱",
                            FilterPlaceholder = "輸入客戶或供應商名稱搜尋",
                            TableOrder = 4,
                            ShowInFilter = false, // NotMapped 屬性無法在資料庫查詢中篩選
                            CustomTemplate = (context) => builder =>
                            {
                                var setoffDoc = context as SetoffDocument;
                                if (setoffDoc != null)
                                {
                                    builder.AddContent(0, setoffDoc.RelatedPartyName ?? "");
                                }
                            },
                            FilterFunction = null
                        }
                    },
                    {
                        nameof(SetoffDocument.CompanyId),
                        new FieldDefinition<SetoffDocument>
                        {
                            PropertyName = "Company.CompanyName", // 用於表格顯示
                            FilterPropertyName = nameof(SetoffDocument.CompanyId), // 用於篩選器
                            DisplayName = "公司",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            HeaderStyle = "width: 180px;",
                            Options = _companies.Select(c => new SelectOption 
                            { 
                                Text = c.CompanyName, 
                                Value = c.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(SetoffDocument.CompanyId), s => s.CompanyId)
                        }
                    },
                    {
                        nameof(SetoffDocument.TotalSetoffAmount),
                        new FieldDefinition<SetoffDocument>
                        {
                            PropertyName = nameof(SetoffDocument.TotalSetoffAmount),
                            DisplayName = "總沖款金額",
                            TableOrder = 6,
                            ShowInFilter = false,
                            HeaderStyle = "width: 150px; text-align: right;",
                            CustomTemplate = (context) => builder =>
                            {
                                var setoffDoc = context as SetoffDocument;
                                if (setoffDoc != null)
                                {
                                    builder.OpenElement(0, "div");
                                    builder.AddAttribute(1, "style", "text-align: right;");
                                    builder.AddContent(2, setoffDoc.TotalSetoffAmount.ToString("N2"));
                                    builder.CloseElement();
                                }
                            },
                            FilterFunction = null // 不提供篩選功能
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: new { CompaniesCount = _companies?.Count ?? 0 });
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化沖款單欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<SetoffDocument>>();
            }
        }

        /// <summary>
        /// 自訂預設排序
        /// </summary>
        protected override Func<IQueryable<SetoffDocument>, IOrderedQueryable<SetoffDocument>> GetDefaultSort()
        {
            return query => query
                .OrderByDescending(s => s.SetoffDate)
                .ThenByDescending(s => s.SetoffNumber);
        }
    }
}
