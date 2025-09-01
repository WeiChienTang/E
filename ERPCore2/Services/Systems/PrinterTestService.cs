using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ERPCore2.Services
{
    /// <summary>
    /// 印表機測試服務實作
    /// </summary>
    public class PrinterTestService : IPrinterTestService
    {
        private readonly ILogger<PrinterTestService>? _logger;

        public PrinterTestService(ILogger<PrinterTestService>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// 測試印表機連接並列印測試頁
        /// </summary>
        public async Task<ServiceResult> TestPrintAsync(PrinterConfiguration printerConfiguration)
        {
            try
            {
                if (printerConfiguration == null)
                    return ServiceResult.Failure("印表機配置不能為空");

                // 首先檢查連接
                var connectionResult = await CheckConnectionAsync(printerConfiguration);
                if (!connectionResult.IsSuccess)
                    return connectionResult;

                // 根據連接類型執行測試列印
                switch (printerConfiguration.ConnectionType)
                {
                    case PrinterConnectionType.Network:
                        return await TestNetworkPrintAsync(printerConfiguration);
                    case PrinterConnectionType.USB:
                        return await TestUsbPrintAsync(printerConfiguration);
                    default:
                        return ServiceResult.Failure($"不支援的連接類型: {printerConfiguration.ConnectionType}");
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(TestPrintAsync), GetType(), _logger, new
                {
                    Method = nameof(TestPrintAsync),
                    ServiceType = GetType().Name,
                    PrinterName = printerConfiguration?.Name,
                    ConnectionType = printerConfiguration?.ConnectionType
                });
                return ServiceResult.Failure("測試列印時發生錯誤");
            }
        }

        /// <summary>
        /// 檢查印表機連接狀態
        /// </summary>
        public async Task<ServiceResult> CheckConnectionAsync(PrinterConfiguration printerConfiguration)
        {
            try
            {
                if (printerConfiguration == null)
                    return ServiceResult.Failure("印表機配置不能為空");

                switch (printerConfiguration.ConnectionType)
                {
                    case PrinterConnectionType.Network:
                        return await CheckNetworkConnectionAsync(printerConfiguration);
                    case PrinterConnectionType.USB:
                        return CheckUsbConnection(printerConfiguration);
                    default:
                        return ServiceResult.Failure($"不支援的連接類型: {printerConfiguration.ConnectionType}");
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CheckConnectionAsync), GetType(), _logger, new
                {
                    Method = nameof(CheckConnectionAsync),
                    ServiceType = GetType().Name,
                    PrinterName = printerConfiguration?.Name,
                    ConnectionType = printerConfiguration?.ConnectionType
                });
                return ServiceResult.Failure("檢查連接時發生錯誤");
            }
        }

        /// <summary>
        /// 產生測試頁內容
        /// </summary>
        public string GenerateTestPageContent(PrinterConfiguration printerConfiguration)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== 印表機測試頁 ===");
            sb.AppendLine($"測試時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"印表機名稱: {printerConfiguration.Name}");
            sb.AppendLine($"連接方式: {GetConnectionTypeDisplayName(printerConfiguration.ConnectionType)}");
            
            if (printerConfiguration.ConnectionType == PrinterConnectionType.Network)
            {
                sb.AppendLine($"IP位址: {printerConfiguration.IpAddress}");
                sb.AppendLine($"連接埠: {printerConfiguration.Port}");
            }
            else if (printerConfiguration.ConnectionType == PrinterConnectionType.USB)
            {
                sb.AppendLine($"USB埠: {printerConfiguration.UsbPort ?? "預設"}");
            }

            sb.AppendLine();
            sb.AppendLine("如果您能看到此測試頁，表示印表機配置正確！");
            sb.AppendLine();
            sb.AppendLine("測試內容:");
            sb.AppendLine("- 中文字體測試: 這是中文測試文字");
            sb.AppendLine("- 英文字體測試: This is English test text");
            sb.AppendLine("- 數字測試: 0123456789");
            sb.AppendLine("- 符號測試: !@#$%^&*()");
            sb.AppendLine();
            sb.AppendLine("=== 測試頁結束 ===");

            return sb.ToString();
        }

        #region 私有方法

        /// <summary>
        /// 測試網路印表機列印
        /// </summary>
        private async Task<ServiceResult> TestNetworkPrintAsync(PrinterConfiguration printerConfiguration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(printerConfiguration.IpAddress))
                    return ServiceResult.Failure("網路印表機必須設定IP位址");

                if (!printerConfiguration.Port.HasValue)
                    return ServiceResult.Failure("網路印表機必須設定連接埠");

                var testContent = GenerateTestPageContent(printerConfiguration);
                
                // 將測試內容轉換為印表機可理解的格式 (簡單的文字格式)
                var printData = Encoding.UTF8.GetBytes(testContent);

                using var client = new TcpClient();
                
                // 設定連接超時時間
                var connectTask = client.ConnectAsync(printerConfiguration.IpAddress, printerConfiguration.Port.Value);
                var timeoutTask = Task.Delay(5000); // 5秒超時

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                    return ServiceResult.Failure("連接印表機超時，請檢查網路連接");

                if (!client.Connected)
                    return ServiceResult.Failure("無法連接到印表機");

                // 發送測試資料
                using var stream = client.GetStream();
                await stream.WriteAsync(printData, 0, printData.Length);
                await stream.FlushAsync();

                // 給印表機一些時間處理資料
                await Task.Delay(1000);

                return ServiceResult.Success();
            }
            catch (SocketException ex)
            {
                return ServiceResult.Failure($"網路連接錯誤: {ex.Message}");
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(TestNetworkPrintAsync), GetType(), _logger, new
                {
                    Method = nameof(TestNetworkPrintAsync),
                    ServiceType = GetType().Name,
                    IpAddress = printerConfiguration.IpAddress,
                    Port = printerConfiguration.Port
                });
                return ServiceResult.Failure("網路列印測試失敗");
            }
        }

        /// <summary>
        /// 測試USB印表機列印
        /// </summary>
        private async Task<ServiceResult> TestUsbPrintAsync(PrinterConfiguration printerConfiguration)
        {
            try
            {
                // USB印表機的實作會依賴於作業系統和印表機驅動程式
                // 在Windows環境下，通常需要使用Windows Print Spooler API
                // 這裡提供一個基本的實作框架

                var testContent = GenerateTestPageContent(printerConfiguration);

                // 在實際環境中，這裡會使用Windows Print API或第三方印表機程式庫
                // 例如: System.Drawing.Printing.PrintDocument
                
                // 模擬USB列印過程
                await Task.Delay(1000); // 模擬列印時間

                // 檢查USB連接埠是否存在（簡化版本）
                var usbPort = printerConfiguration.UsbPort ?? "LPT1";
                
                try
                {
                    // 嘗試寫入到USB埠（這是一個簡化的範例）
                    // 實際實作中需要使用適當的印表機API
                    
                    // 在Windows中，您可能需要使用:
                    // - System.Drawing.Printing.PrintDocument
                    // - Win32 API (CreateFile, WriteFile)
                    // - 第三方程式庫如 RawPrinterHelper

                    _logger?.LogInformation($"模擬向USB埠 {usbPort} 發送測試頁");
                    
                    return ServiceResult.Success();
                }
                catch (UnauthorizedAccessException)
                {
                    return ServiceResult.Failure($"無法存取USB埠 {usbPort}，請檢查權限設定");
                }
                catch (DirectoryNotFoundException)
                {
                    return ServiceResult.Failure($"找不到USB埠 {usbPort}，請檢查印表機連接");
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(TestUsbPrintAsync), GetType(), _logger, new
                {
                    Method = nameof(TestUsbPrintAsync),
                    ServiceType = GetType().Name,
                    UsbPort = printerConfiguration.UsbPort
                });
                return ServiceResult.Failure("USB列印測試失敗");
            }
        }

        /// <summary>
        /// 檢查網路印表機連接
        /// </summary>
        private async Task<ServiceResult> CheckNetworkConnectionAsync(PrinterConfiguration printerConfiguration)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(printerConfiguration.IpAddress))
                    return ServiceResult.Failure("網路印表機必須設定IP位址");

                // 先檢查IP是否可以ping通
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(printerConfiguration.IpAddress, 3000);
                
                if (reply.Status != IPStatus.Success)
                    return ServiceResult.Failure($"無法ping通印表機IP: {printerConfiguration.IpAddress}");

                // 如果有設定連接埠，檢查埠是否開放
                if (printerConfiguration.Port.HasValue)
                {
                    using var client = new TcpClient();
                    var connectTask = client.ConnectAsync(printerConfiguration.IpAddress, printerConfiguration.Port.Value);
                    var timeoutTask = Task.Delay(3000);

                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                        return ServiceResult.Failure($"連接埠 {printerConfiguration.Port} 無回應");

                    if (!client.Connected)
                        return ServiceResult.Failure($"無法連接到埠 {printerConfiguration.Port}");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CheckNetworkConnectionAsync), GetType(), _logger, new
                {
                    Method = nameof(CheckNetworkConnectionAsync),
                    ServiceType = GetType().Name,
                    IpAddress = printerConfiguration.IpAddress,
                    Port = printerConfiguration.Port
                });
                return ServiceResult.Failure("檢查網路連接時發生錯誤");
            }
        }

        /// <summary>
        /// 檢查USB印表機連接
        /// </summary>
        private ServiceResult CheckUsbConnection(PrinterConfiguration printerConfiguration)
        {
            try
            {
                var usbPort = printerConfiguration.UsbPort ?? "LPT1";
                
                // 在實際環境中，這裡會檢查USB設備是否存在
                // 可以使用Windows Management Instrumentation (WMI)
                // 或檢查系統的印表機列表

                // 簡化版本：檢查常見的印表機埠
                var commonPorts = new[] { "LPT1", "LPT2", "USB001", "USB002" };
                
                if (!commonPorts.Contains(usbPort.ToUpper()) && !usbPort.ToUpper().StartsWith("USB"))
                {
                    return ServiceResult.Failure($"不認識的USB埠格式: {usbPort}");
                }

                // 模擬USB連接檢查
                _logger?.LogInformation($"檢查USB埠: {usbPort}");
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(CheckUsbConnection), GetType(), _logger, new
                {
                    Method = nameof(CheckUsbConnection),
                    ServiceType = GetType().Name,
                    UsbPort = printerConfiguration.UsbPort
                });
                return ServiceResult.Failure("檢查USB連接時發生錯誤");
            }
        }

        /// <summary>
        /// 取得連接類型顯示名稱
        /// </summary>
        private static string GetConnectionTypeDisplayName(PrinterConnectionType connectionType)
        {
            return connectionType switch
            {
                PrinterConnectionType.Network => "網路連接",
                PrinterConnectionType.USB => "USB 連接",
                _ => connectionType.ToString()
            };
        }

        #endregion
    }
}
