using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Services;
using Microsoft.AspNetCore.Components;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
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
                            DisplayName = Dn("Field.PrinterName", "印表機名稱"),
                            FilterPlaceholder = Fp("Field.PrinterName", "輸入印表機名稱搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PrinterConfiguration.Name), p => p.Name)
                        }
                    },
                    {
                        nameof(PrinterConfiguration.ConnectionType),
                        new FieldDefinition<PrinterConfiguration>
                        {
                            PropertyName = nameof(PrinterConfiguration.ConnectionType),
                            DisplayName = Dn("Field.ConnectionType", "連接方式"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 2,
                            Options = Enum.GetValues(typeof(PrinterConnectionType))
                                .Cast<PrinterConnectionType>()
                                .Select(e => new SelectOption
                                {
                                    Text = GetConnectionTypeDisplayName(e),
                                    Value = ((int)e).ToString()
                                })
                                .ToList(),
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PrinterConfiguration printer)
                                {
                                    builder.AddContent(0, GetConnectionTypeDisplayName(printer.ConnectionType));
                                }
                            })
                        }
                    },
                    {
                        nameof(PrinterConfiguration.UsbPort),
                        new FieldDefinition<PrinterConfiguration>
                        {
                            PropertyName = nameof(PrinterConfiguration.UsbPort),
                            DisplayName = Dn("Field.UsbPort", "USB連接埠"),
                            FilterPlaceholder = Fp("Field.UsbPort", "輸入USB連接埠搜尋"),
                            TableOrder = 3,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(PrinterConfiguration.UsbPort), p => p.UsbPort ?? string.Empty),
                            CustomTemplate = (data) => (RenderFragment)((builder) =>
                            {
                                if (data is PrinterConfiguration printer)
                                {
                                    var value = string.IsNullOrWhiteSpace(printer.UsbPort) ? "-" : printer.UsbPort;
                                    builder.AddContent(0, value);
                                }
                            })
                        }
                    },
                    {
                        nameof(PrinterConfiguration.IpAddress),
                        new FieldDefinition<PrinterConfiguration>
                        {
                            PropertyName = nameof(PrinterConfiguration.IpAddress),
                            DisplayName = Dn("Field.PrinterIpAddress", "IP位址"),
                            FilterPlaceholder = Fp("Field.PrinterIpAddress", "輸入IP位址搜尋"),
                            TableOrder = 4,
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

        /// <summary>
        /// 取得印表機連接方式的顯示名稱
        /// </summary>
        /// <param name="connectionType">連接方式枚舉</param>
        /// <returns>顯示名稱</returns>
        private static string GetConnectionTypeDisplayName(PrinterConnectionType connectionType)
        {
            return connectionType switch
            {
                PrinterConnectionType.Network => "網路連接",
                PrinterConnectionType.USB => "USB 連接",
                _ => connectionType.ToString()
            };
        }
    }
}


