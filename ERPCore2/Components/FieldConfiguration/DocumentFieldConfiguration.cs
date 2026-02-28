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
    /// 檔案存留欄位配置
    /// </summary>
    public class DocumentFieldConfiguration : BaseFieldConfiguration<Document>
    {
        private readonly INotificationService? _notificationService;

        public DocumentFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<Document>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Document>>
                {
                    {
                        nameof(Document.Code),
                        new FieldDefinition<Document>
                        {
                            PropertyName = nameof(Document.Code),
                            DisplayName = Dn("Field.Code", "編號"),
                            FilterPlaceholder = Fp("Field.Code", "輸入編號搜尋"),
                            TableOrder = 0,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Document.Code), d => d.Code)
                        }
                    },
                    {
                        nameof(Document.Title),
                        new FieldDefinition<Document>
                        {
                            PropertyName = nameof(Document.Title),
                            DisplayName = Dn("Field.DocumentTitle", "文件標題"),
                            FilterPlaceholder = Fp("Field.DocumentTitle", "輸入文件標題搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Document.Title), d => d.Title)
                        }
                    },
                    {
                        nameof(Document.DocumentCategory),
                        new FieldDefinition<Document>
                        {
                            PropertyName = nameof(Document.DocumentCategory),
                            DisplayName = Dn("Field.DocumentCategory", "檔案分類"),
                            ShowInFilter = false,
                            TableOrder = 2,
                            NullDisplayText = "-",
                            CustomTemplate = obj =>
                            {
                                var text = (obj as Document)?.DocumentCategory?.Name ?? "-";
                                return builder => builder.AddContent(0, text);
                            }
                        }
                    },
                    {
                        nameof(Document.IssuedBy),
                        new FieldDefinition<Document>
                        {
                            PropertyName = nameof(Document.IssuedBy),
                            DisplayName = Dn("Field.IssuedBy", "發文機關/來源"),
                            FilterPlaceholder = Fp("Field.IssuedBy", "輸入發文機關搜尋"),
                            TableOrder = 3,
                            NullDisplayText = "-",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Document.IssuedBy), d => d.IssuedBy)
                        }
                    },
                    {
                        nameof(Document.IssuedDate),
                        new FieldDefinition<Document>
                        {
                            PropertyName = nameof(Document.IssuedDate),
                            DisplayName = Dn("Field.IssuedDate", "發文日期"),
                            ShowInFilter = false,
                            TableOrder = 4,
                            NullDisplayText = "-",
                            CustomTemplate = obj =>
                            {
                                var item = obj as Document;
                                var text = item?.IssuedDate.HasValue == true
                                    ? item.IssuedDate.Value.ToString("yyyy/MM/dd")
                                    : "-";
                                return builder => builder.AddContent(0, text);
                            }
                        }
                    },
                    {
                        nameof(Document.ExpiryDate),
                        new FieldDefinition<Document>
                        {
                            PropertyName = nameof(Document.ExpiryDate),
                            DisplayName = Dn("Field.ExpiryDate", "有效期限"),
                            ShowInFilter = false,
                            TableOrder = 5,
                            NullDisplayText = "-",
                            CustomTemplate = obj =>
                            {
                                var item = obj as Document;
                                var text = item?.ExpiryDate.HasValue == true
                                    ? item.ExpiryDate.Value.ToString("yyyy/MM/dd")
                                    : "-";
                                return builder => builder.AddContent(0, text);
                            }
                        }
                    },
                    {
                        nameof(Document.FileSize),
                        new FieldDefinition<Document>
                        {
                            PropertyName = nameof(Document.FileSize),
                            DisplayName = Dn("Field.FileSize", "檔案大小"),
                            ShowInFilter = false,
                            TableOrder = 6,
                            CustomTemplate = obj =>
                            {
                                var text = FileUploadHelper.GetFileSizeString((obj as Document)?.FileSize ?? 0);
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
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化檔案存留欄位配置失敗");
                });

                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化檔案存留欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                return new Dictionary<string, FieldDefinition<Document>>();
            }
        }
    }
}
