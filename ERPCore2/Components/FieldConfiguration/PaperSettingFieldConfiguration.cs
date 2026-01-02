using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.PageTemplate;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 紙張設定欄位配置類別
    /// </summary>
    public class PaperSettingFieldConfiguration : BaseFieldConfiguration<PaperSetting>
    {
        private readonly INotificationService? _notificationService;

        public PaperSettingFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PaperSetting>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<PaperSetting>>
                {
                    {
                        nameof(PaperSetting.Code),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.Code),
                            DisplayName = "紙張代碼",
                            FilterPlaceholder = "輸入紙張代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PaperSetting.Code), p => p.Code)
                        }
                    },
                    {
                        nameof(PaperSetting.Name),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.Name),
                            DisplayName = "紙張名稱",
                            FilterPlaceholder = "輸入紙張名稱搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PaperSetting.Name), p => p.Name)
                        }
                    },
                    {
                        nameof(PaperSetting.Width),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.Width),
                            DisplayName = "寬度 (mm)",
                            FilterPlaceholder = "輸入寬度搜尋",
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 100px; text-align: right;",
                            // 自訂模板顯示數值並右對齊
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PaperSetting paper)
                                {
                                    builder.OpenElement(0, "div");
                                    builder.AddAttribute(1, "class", "text-end");
                                    builder.AddContent(2, $"{paper.Width:F1}");
                                    builder.CloseElement();
                                }
                            }),
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PaperSetting.Width), p => p.Width.ToString())
                        }
                    },
                    {
                        nameof(PaperSetting.Height),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.Height),
                            DisplayName = "高度 (mm)",
                            FilterPlaceholder = "輸入高度搜尋",
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 100px; text-align: right;",
                            // 自訂模板顯示數值並右對齊
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PaperSetting paper)
                                {
                                    builder.OpenElement(0, "div");
                                    builder.AddAttribute(1, "class", "text-end");
                                    builder.AddContent(2, $"{paper.Height:F1}");
                                    builder.CloseElement();
                                }
                            }),
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PaperSetting.Height), p => p.Height.ToString())
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "建立紙張設定欄位定義時發生錯誤");
                    if (_notificationService != null)
                    {
                        await _notificationService.ShowErrorAsync("欄位配置載入失敗");
                    }
                });

                // 回傳安全的預設值
                return new Dictionary<string, FieldDefinition<PaperSetting>>();
            }
        }
    }
}


