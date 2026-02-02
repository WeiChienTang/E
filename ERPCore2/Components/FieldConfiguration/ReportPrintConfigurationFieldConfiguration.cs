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
    /// 報表列印配置欄位配置類別
    /// </summary>
    public class ReportPrintConfigurationFieldConfiguration : BaseFieldConfiguration<ReportPrintConfiguration>
    {
        private readonly INotificationService? _notificationService;

        public ReportPrintConfigurationFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<ReportPrintConfiguration>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<ReportPrintConfiguration>>
                {
                    {
                        nameof(ReportPrintConfiguration.Code),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.Code),
                            DisplayName = "編號",
                            FilterPlaceholder = "輸入編號搜尋",
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ReportPrintConfiguration.Code), r => r.Code)
                        }
                    },
                    {
                        nameof(ReportPrintConfiguration.ReportId),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.ReportId),
                            DisplayName = "報表識別碼",
                            FilterPlaceholder = "輸入報表識別碼搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ReportPrintConfiguration.ReportId), r => r.ReportId)
                        }
                    },
                    {
                        nameof(ReportPrintConfiguration.ReportName),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.ReportName),
                            DisplayName = "報表名稱",
                            FilterPlaceholder = "輸入報表名稱搜尋",
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ReportPrintConfiguration.ReportName), r => r.ReportName)
                        }
                    },
                    {
                        "PrinterConfigurationName",
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = "PrinterConfigurationName",
                            DisplayName = "印表機設定",
                            FilterPlaceholder = "輸入印表機名稱搜尋",
                            TableOrder = 4,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is ReportPrintConfiguration report)
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddContent(1, report.PrinterConfiguration?.Name ?? "未設定");
                                    builder.CloseElement();
                                }
                            }),
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "PrinterConfigurationName", 
                                r => r.PrinterConfiguration != null ? r.PrinterConfiguration.Name : string.Empty)
                        }
                    },
                    {
                        "PaperSettingName",
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = "PaperSettingName",
                            DisplayName = "紙張設定",
                            FilterPlaceholder = "輸入紙張名稱搜尋",
                            TableOrder = 5,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is ReportPrintConfiguration report)
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddContent(1, report.PaperSetting?.Name ?? "未設定");
                                    builder.CloseElement();
                                }
                            }),
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, "PaperSettingName", 
                                r => r.PaperSetting != null ? r.PaperSetting.Name : string.Empty)
                        }
                    },

                };
            }
            catch (Exception)
            {
                // 顯示錯誤通知
                if (_notificationService != null)
                {
                    Task.Run(async () => await _notificationService.ShowErrorAsync("載入欄位配置失敗"));
                }
                
                // 回傳空字典以避免應用程式崩潰
                return new Dictionary<string, FieldDefinition<ReportPrintConfiguration>>();
            }
        }
    }
}


