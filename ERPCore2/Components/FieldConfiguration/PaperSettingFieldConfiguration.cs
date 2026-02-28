using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
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
                            DisplayName = Dn("Field.PaperCode", "紙張編號"),
                            FilterPlaceholder = Fp("Field.PaperCode", "輸入紙張編號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PaperSetting.Code), p => p.Code)
                        }
                    },
                    {
                        nameof(PaperSetting.Name),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.Name),
                            DisplayName = Dn("Field.PaperName", "紙張名稱"),
                            FilterPlaceholder = Fp("Field.PaperName", "輸入紙張名稱搜尋"),
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PaperSetting.Name), p => p.Name)
                        }
                    },
                    {
                        nameof(PaperSetting.Width),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.Width),
                            DisplayName = Dn("Field.PaperWidth", "寬度 (cm)"),
                            FilterPlaceholder = Fp("Field.PaperWidth", "輸入寬度搜尋"),
                            TableOrder = 3,
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
                            DisplayName = Dn("Field.PaperHeight", "高度 (cm)"),
                            FilterPlaceholder = Fp("Field.PaperHeight", "輸入高度搜尋"),
                            TableOrder = 4,
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
                    },
                    {
                        nameof(PaperSetting.TopMargin),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.TopMargin),
                            DisplayName = Dn("Field.PaperTopMargin", "上邊距 (cm)"),
                            TableOrder = 5,
                            FilterPlaceholder = Fp("Field.PaperTopMargin", "輸入上邊距搜尋"),
                            ColumnType = ColumnDataType.Number,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PaperSetting paper)
                                {
                                    builder.OpenElement(0, "div");
                                    builder.AddAttribute(1, "class", "text-end");
                                    builder.AddContent(2, $"{paper.TopMargin:F1}");
                                    builder.CloseElement();
                                }
                            })
                        }
                    }, 
                    {
                        nameof(PaperSetting.BottomMargin),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.BottomMargin),
                            DisplayName = Dn("Field.PaperBottomMargin", "下邊距 (cm)"),
                            TableOrder = 6,
                            FilterPlaceholder = Fp("Field.PaperBottomMargin", "輸入下邊距搜尋"),
                            ColumnType = ColumnDataType.Number,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PaperSetting paper)
                                {
                                    builder.OpenElement(0, "div");
                                    builder.AddAttribute(1, "class", "text-end");
                                    builder.AddContent(2, $"{paper.BottomMargin:F1}");
                                    builder.CloseElement();
                                }
                            })
                        }
                    }, 
                    {
                        nameof(PaperSetting.LeftMargin),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.LeftMargin),
                            DisplayName = Dn("Field.PaperLeftMargin", "左邊距 (cm)"),
                            TableOrder = 7,
                            FilterPlaceholder = Fp("Field.PaperLeftMargin", "輸入左邊距搜尋"),
                            ColumnType = ColumnDataType.Number,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PaperSetting paper)
                                {
                                    builder.OpenElement(0, "div");
                                    builder.AddAttribute(1, "class", "text-end");
                                    builder.AddContent(2, $"{paper.LeftMargin:F1}");
                                    builder.CloseElement();
                                }
                            })
                        }
                    }, 
                    {
                        nameof(PaperSetting.RightMargin),
                        new FieldDefinition<PaperSetting>
                        {
                            PropertyName = nameof(PaperSetting.RightMargin),
                            DisplayName = Dn("Field.PaperRightMargin", "右邊距 (cm)"),
                            TableOrder = 8,
                            FilterPlaceholder = Fp("Field.PaperRightMargin", "輸入右邊距搜尋"),
                            ColumnType = ColumnDataType.Number,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PaperSetting paper)
                                {
                                    builder.OpenElement(0, "div");
                                    builder.AddAttribute(1, "class", "text-end");
                                    builder.AddContent(2, $"{paper.RightMargin:F1}");
                                    builder.CloseElement();
                                }
                            })
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


