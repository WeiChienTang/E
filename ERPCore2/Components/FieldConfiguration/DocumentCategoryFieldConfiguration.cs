using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
using ERPCore2.Models.Enums;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 檔案分類欄位配置
    /// </summary>
    public class DocumentCategoryFieldConfiguration : BaseFieldConfiguration<DocumentCategory>
    {
        private readonly INotificationService? _notificationService;

        public DocumentCategoryFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<DocumentCategory>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<DocumentCategory>>
                {
                    {
                        nameof(DocumentCategory.Code),
                        new FieldDefinition<DocumentCategory>
                        {
                            PropertyName = nameof(DocumentCategory.Code),
                            DisplayName = Dn("Field.Code", "編號"),
                            FilterPlaceholder = Fp("Field.Code", "輸入編號搜尋"),
                            TableOrder = 0,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(DocumentCategory.Code), c => c.Code)
                        }
                    },
                    {
                        nameof(DocumentCategory.Name),
                        new FieldDefinition<DocumentCategory>
                        {
                            PropertyName = nameof(DocumentCategory.Name),
                            DisplayName = Dn("Field.CategoryName", "分類名稱"),
                            FilterPlaceholder = Fp("Field.CategoryName", "輸入分類名稱搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(DocumentCategory.Name), c => c.Name)
                        }
                    },
                    {
                        nameof(DocumentCategory.Source),
                        new FieldDefinition<DocumentCategory>
                        {
                            PropertyName = nameof(DocumentCategory.Source),
                            DisplayName = Dn("Field.DocumentSource", "來源類型"),
                            ShowInFilter = false,
                            TableOrder = 2,
                            CustomTemplate = obj =>
                            {
                                var item = obj as DocumentCategory;
                                var text = (item?.Source) switch
                                {
                                    DocumentSource.Government => L?["Label.DocumentSource.Government"].ToString() ?? "政府/法規",
                                    DocumentSource.Vendor => L?["Label.DocumentSource.Vendor"].ToString() ?? "廠商",
                                    DocumentSource.Customer => L?["Label.DocumentSource.Customer"].ToString() ?? "客戶",
                                    DocumentSource.Internal => L?["Label.DocumentSource.Internal"].ToString() ?? "內部文件",
                                    DocumentSource.Other => L?["Label.DocumentSource.Other"].ToString() ?? "其他",
                                    _ => item?.Source.ToString() ?? "-"
                                };
                                return builder => builder.AddContent(0, text);
                            }
                        }
                    },
                    {
                        nameof(DocumentCategory.DefaultAccessLevel),
                        new FieldDefinition<DocumentCategory>
                        {
                            PropertyName = nameof(DocumentCategory.DefaultAccessLevel),
                            DisplayName = Dn("Field.DefaultAccessLevel", "預設存取層級"),
                            ShowInFilter = false,
                            TableOrder = 3,
                            CustomTemplate = obj =>
                            {
                                var item = obj as DocumentCategory;
                                var text = (item?.DefaultAccessLevel) switch
                                {
                                    DocumentAccessLevel.Normal => L?["Label.DocumentAccessLevel.Normal"].ToString() ?? "一般",
                                    DocumentAccessLevel.Sensitive => L?["Label.DocumentAccessLevel.Sensitive"].ToString() ?? "敏感",
                                    _ => item?.DefaultAccessLevel.ToString() ?? "-"
                                };
                                return builder => builder.AddContent(0, text);
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化檔案分類欄位配置失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化檔案分類欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<DocumentCategory>>();
            }
        }
    }
}
