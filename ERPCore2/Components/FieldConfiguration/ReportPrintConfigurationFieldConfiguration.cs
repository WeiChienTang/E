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
                        nameof(ReportPrintConfiguration.ReportType),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.ReportType),
                            DisplayName = "報表類型",
                            FilterPlaceholder = "輸入報表類型搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
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
                            TableOrder = 2,
                            FilterOrder = 2,
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
                            TableOrder = 3,
                            FilterOrder = 3,
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
                            TableOrder = 4,
                            FilterOrder = 4,
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
                    {
                        nameof(ReportPrintConfiguration.Status),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.Status),
                            DisplayName = "狀態",
                            TableOrder = 5,
                            FilterOrder = 5,
                            HeaderStyle = "width: 100px;",
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is ReportPrintConfiguration report)
                                {
                                    var badgeClass = report.Status == EntityStatus.Active ? "badge bg-success" : "badge bg-secondary";
                                    var statusText = report.Status == EntityStatus.Active ? "啟用" : "停用";
                                    
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", badgeClass);
                                    builder.AddContent(2, statusText);
                                    builder.CloseElement();
                                }
                            }),
                            FilterFunction = (model, query) => FilterHelper.ApplyStatusFilter(
                                model, query, nameof(ReportPrintConfiguration.Status))
                        }
                    },
                    {
                        nameof(ReportPrintConfiguration.Code),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.Code),
                            DisplayName = "代碼",
                            FilterPlaceholder = "輸入代碼搜尋",
                            TableOrder = 6,
                            FilterOrder = 6,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ReportPrintConfiguration.Code), r => r.Code)
                        }
                    },
                    {
                        nameof(ReportPrintConfiguration.Remarks),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.Remarks),
                            DisplayName = "備註",
                            FilterPlaceholder = "輸入備註搜尋",
                            TableOrder = 7,
                            FilterOrder = 7,
                            HeaderStyle = "width: 200px;",
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is ReportPrintConfiguration report)
                                {
                                    var remarks = report.Remarks ?? "";
                                    var displayText = remarks.Length > 50 ? $"{remarks[..50]}..." : remarks;
                                    
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "title", remarks);
                                    builder.AddContent(2, string.IsNullOrEmpty(displayText) ? "-" : displayText);
                                    builder.CloseElement();
                                }
                            }),
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(ReportPrintConfiguration.Remarks), r => r.Remarks)
                        }
                    },
                    {
                        nameof(ReportPrintConfiguration.UpdatedAt),
                        new FieldDefinition<ReportPrintConfiguration>
                        {
                            PropertyName = nameof(ReportPrintConfiguration.UpdatedAt),
                            DisplayName = "更新時間",
                            TableOrder = 8,
                            FilterOrder = 8,
                            HeaderStyle = "width: 150px;",
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is ReportPrintConfiguration report)
                                {
                                    var displayDate = report.UpdatedAt?.ToString("yyyy/MM/dd HH:mm") ?? 
                                                     report.CreatedAt.ToString("yyyy/MM/dd HH:mm");
                                    
                                    builder.OpenElement(0, "span");
                                    builder.AddContent(1, displayDate);
                                    builder.CloseElement();
                                }
                            }),
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(ReportPrintConfiguration.UpdatedAt), r => r.UpdatedAt ?? r.CreatedAt)
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
