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
                            DisplayName = Dn("Field.Code", "編號"),
                            FilterPlaceholder = Fp("Field.Code", "輸入編號搜尋"),
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
                            DisplayName = Dn("Field.ReportId", "報表識別碼"),
                            FilterPlaceholder = Fp("Field.ReportId", "輸入報表識別碼搜尋"),
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
                            DisplayName = Dn("Field.ReportName", "報表名稱"),
                            FilterPlaceholder = Fp("Field.ReportName", "輸入報表名稱搜尋"),
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
                            DisplayName = Dn("Field.Printer", "印表機設定"),
                            FilterPlaceholder = Fp("Field.Printer", "輸入印表機名稱搜尋"),
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
                            DisplayName = Dn("Field.Paper", "紙張設定"),
                            FilterPlaceholder = Fp("Field.Paper", "輸入紙張名稱搜尋"),
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
                    {
                        nameof(ReportPrintConfiguration.Status),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.Status),
                            DisplayName = Dn("Field.Status", "狀態"),
                            FilterPlaceholder = "選擇狀態搜尋",
                            TableOrder = 6,
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is ReportPrintConfiguration report)
                                {
                                    var statusText = report.Status == EntityStatus.Active ? "啟用" : "停用";
                                    var badgeClass = report.Status == EntityStatus.Active ? "badge bg-success" : "badge bg-secondary";

                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", badgeClass);
                                    builder.AddContent(2, statusText);
                                    builder.CloseElement();
                                }
                            }),
                            FilterFunction = (model, query) => FilterHelper.ApplyStatusFilter(model, query)
                        }
                    }

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


