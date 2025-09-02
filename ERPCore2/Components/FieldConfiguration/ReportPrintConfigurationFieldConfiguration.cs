using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ERPCore2.Helpers;

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
                        DisplayName = "代碼",
                        FilterPlaceholder = "輸入代碼搜尋",
                        TableOrder = 1,
                        HeaderStyle = "width: 120px;",
                        FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                            model, query, nameof(ReportPrintConfiguration.Code), r => r.Code)
                    }
                    },
                    {
                        nameof(ReportPrintConfiguration.ReportType),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.ReportType),
                            DisplayName = "報表類型",
                            FilterPlaceholder = "輸入報表類型搜尋",
                            TableOrder = 2,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ReportPrintConfiguration.ReportType), r => r.ReportType)
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
                            HeaderStyle = "width: 200px;",
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
                            HeaderStyle = "width: 150px;",
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
                            HeaderStyle = "width: 150px;",
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
