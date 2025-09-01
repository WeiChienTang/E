using ERPCore2.Components.Shared.Forms;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;
using ERPCore2.Helpers;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 印表機設定欄位配置類別
    /// </summary>
    public class PrinterConfigurationFieldConfiguration : BaseFieldConfiguration<PrinterConfiguration>
    {
        private readonly INotificationService? _notificationService;

        public PrinterConfigurationFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<PrinterConfiguration>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<PrinterConfiguration>>
                {
                    {
                        nameof(PrinterConfiguration.Name),
                        new FieldDefinition<PrinterConfiguration>
                        {
                            PropertyName = nameof(PrinterConfiguration.Name),
                            DisplayName = "印表機名稱",
                            FilterPlaceholder = "輸入印表機名稱搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            HeaderStyle = "width: 200px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PrinterConfiguration.Name), p => p.Name)
                        }
                    },
                    {
                        nameof(PrinterConfiguration.IpAddress),
                        new FieldDefinition<PrinterConfiguration>
                        {
                            PropertyName = nameof(PrinterConfiguration.IpAddress),
                            DisplayName = "IP位址",
                            FilterPlaceholder = "輸入IP位址搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PrinterConfiguration.IpAddress), p => p.IpAddress ?? string.Empty),
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PrinterConfiguration printer)
                                {
                                    var value = string.IsNullOrWhiteSpace(printer.IpAddress) ? "-" : printer.IpAddress;
                                    builder.AddContent(0, value);
                                }
                            })
                        }
                    },
                    {
                        nameof(PrinterConfiguration.Port),
                        new FieldDefinition<PrinterConfiguration>
                        {
                            PropertyName = nameof(PrinterConfiguration.Port),
                            DisplayName = "連接埠",
                            FilterType = SearchFilterType.Number,
                            FilterPlaceholder = "輸入連接埠搜尋",
                            TableOrder = 3,
                            FilterOrder = 3,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(PrinterConfiguration.Port), p => p.Port),
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PrinterConfiguration printer)
                                {
                                    var value = printer.Port?.ToString() ?? "-";
                                    builder.AddContent(0, value);
                                }
                            })
                        }
                    },
                    {
                        nameof(PrinterConfiguration.IsDefault),
                        new FieldDefinition<PrinterConfiguration>
                        {
                            PropertyName = nameof(PrinterConfiguration.IsDefault),
                            DisplayName = "預設印表機",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            FilterOrder = 4,
                            HeaderStyle = "width: 120px;",
                            Options = new List<SelectOption>
                            {
                                new() { Text = "是", Value = "true" },
                                new() { Text = "否", Value = "false" }
                            },
                            FilterFunction = (model, query) =>
                            {
                                var filterValue = model.GetFilterValue(nameof(PrinterConfiguration.IsDefault))?.ToString();
                                if (!string.IsNullOrWhiteSpace(filterValue) && bool.TryParse(filterValue, out var isDefault))
                                {
                                    query = query.Where(p => p.IsDefault == isDefault);
                                }
                                return query;
                            },
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PrinterConfiguration printer)
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", printer.IsDefault ? "badge bg-success" : "badge bg-secondary");
                                    builder.AddContent(2, printer.IsDefault ? "是" : "否");
                                    builder.CloseElement();
                                }
                            })
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 處理例外情況
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(),
                            additionalData: "初始化印表機設定欄位定義失敗");

                        if (_notificationService != null)
                        {
                            await _notificationService.ShowErrorAsync("初始化欄位配置時發生錯誤");
                        }
                    }
                    catch
                    {
                        // 忽略通知錯誤
                    }
                });

                return new Dictionary<string, FieldDefinition<PrinterConfiguration>>();
            }
        }

        protected override Func<IQueryable<PrinterConfiguration>, IQueryable<PrinterConfiguration>> GetDefaultSort()
        {
            return query => query.OrderByDescending(p => p.IsDefault).ThenBy(p => p.Name);
        }
    }
}
