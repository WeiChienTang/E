using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ERPCore2.Services
{
    /// <summary>
    /// 印表機測試服務實作
    /// </summary>
    public class PrinterTestService : IPrinterTestService
    {
        private readonly ILogger<PrinterTestService>? _logger;

        // Windows API 宣告
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool OpenPrinter(string printerName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOC_INFO_1 docInfo);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool WritePrinter(IntPtr hPrinter, byte[] pBytes, int dwCount, out int dwWritten);

        [StructLayout(LayoutKind.Sequential)]
        public struct DOC_INFO_1
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            public string DocName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string OutputFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string DataType;
        }

        public PrinterTestService(ILogger<PrinterTestService>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// 測試印表機連接並列印測試頁
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
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

                var testContent = GenerateTestPageContent(printerConfiguration);
                
                // 嘗試不同的印表機格式，針對常見印表機優化順序
                var printDataFormats = new Dictionary<string, byte[]>
                {
                    { "PCL", GeneratePclData(testContent) },
                    { "Plain Text with FF", GeneratePlainTextData(testContent) },
                    { "ESC/POS", GenerateEscPosData(testContent) }
                };

                // 使用預設印表機連接埠 9100
                int port = 9100;
                bool printSuccess = false;
                string lastError = "";

                foreach (var format in printDataFormats)
                {
                    try
                    {
                        using var client = new TcpClient();
                        
                        // 設定連接超時時間
                        var connectTask = client.ConnectAsync(printerConfiguration.IpAddress, port);
                        var timeoutTask = Task.Delay(5000); // 5秒超時

                        var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                        
                        if (completedTask == timeoutTask)
                        {
                            lastError = "連接印表機超時，請檢查網路連接";
                            continue;
                        }

                        if (!client.Connected)
                        {
                            lastError = "無法連接到印表機";
                            continue;
                        }

                        // 發送測試資料
                        using var stream = client.GetStream();
                        await stream.WriteAsync(format.Value, 0, format.Value.Length);
                        await stream.FlushAsync();

                        // 給印表機一些時間處理資料
                        await Task.Delay(2000);
                        
                        printSuccess = true;
                        break; // 成功發送，跳出迴圈
                    }
                    catch (SocketException ex)
                    {
                        lastError = $"網路連接錯誤: {ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        lastError = $"發送 {format.Key} 格式時發生錯誤: {ex.Message}";
                    }
                }

                if (printSuccess)
                {
                    return ServiceResult.Success();
                }
                else
                {
                    return ServiceResult.Failure($"所有格式都嘗試失敗。最後錯誤: {lastError}");
                }
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
                    IpAddress = printerConfiguration.IpAddress
                });
                return ServiceResult.Failure("網路列印測試失敗");
            }
        }

        /// <summary>
        /// 測試USB印表機列印 (簡化版本)
        /// 直接使用 PrinterConfiguration.Name 作為系統印表機名稱進行列印
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        private async Task<ServiceResult> TestUsbPrintAsync(PrinterConfiguration printerConfiguration)
        {
            try
            {
                var printerName = printerConfiguration.Name;
                
                if (string.IsNullOrWhiteSpace(printerName))
                {
                    return ServiceResult.Failure("印表機名稱不能為空");
                }

                var testContent = GenerateTestPageContent(printerConfiguration);

                // 方法 1: 優先使用 System.Drawing.Printing (更可靠，使用印表機驅動程式)
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var systemDrawingResult = await PrintUsingSystemDrawing(printerName, testContent);
                    if (systemDrawingResult.IsSuccess)
                    {
                        return ServiceResult.Success();
                    }
                }

                // 方法 2: 如果方法 1 失敗，使用 Windows API 直接寫入
                var textData = Encoding.UTF8.GetBytes(testContent);
                var apiResult = await PrintUsingWindowsApi(printerName, textData, "ERP 測試頁");

                if (apiResult.IsSuccess)
                {
                    return ServiceResult.Success();
                }

                return ServiceResult.Failure($"列印失敗: {apiResult.ErrorMessage}");
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(TestUsbPrintAsync), GetType(), _logger, new
                {
                    Method = nameof(TestUsbPrintAsync),
                    ServiceType = GetType().Name,
                    PrinterName = printerConfiguration.Name
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

                // 檢查預設印表機連接埠 9100 是否開放
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(printerConfiguration.IpAddress, 9100);
                var timeoutTask = Task.Delay(3000);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                
                if (completedTask == timeoutTask)
                    return ServiceResult.Failure("印表機連接埠 9100 無回應");

                if (!client.Connected)
                    return ServiceResult.Failure("無法連接到印表機連接埠 9100");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CheckNetworkConnectionAsync), GetType(), _logger, new
                {
                    Method = nameof(CheckNetworkConnectionAsync),
                    ServiceType = GetType().Name,
                    IpAddress = printerConfiguration.IpAddress
                });
                return ServiceResult.Failure("檢查網路連接時發生錯誤");
            }
        }

        /// <summary>
        /// 檢查USB印表機連接
        /// 直接驗證 PrinterConfiguration.Name 對應的系統印表機是否存在
        /// </summary>
        private ServiceResult CheckUsbConnection(PrinterConfiguration printerConfiguration)
        {
            try
            {
                var printerName = printerConfiguration.Name;

                if (string.IsNullOrWhiteSpace(printerName))
                {
                    return ServiceResult.Failure("印表機名稱不能為空");
                }

                // 檢查系統中是否存在此印表機
                var availablePrinters = GetAvailablePrinters();
                var matchedPrinter = availablePrinters.FirstOrDefault(p => 
                    string.Equals(p.Name, printerName, StringComparison.OrdinalIgnoreCase));

                if (matchedPrinter != null)
                {
                    return ServiceResult.Success();
                }
                else
                {
                    // 列出可用的印表機供參考
                    if (availablePrinters.Any())
                    {
                        var availableNames = string.Join(", ", availablePrinters.Select(p => $"'{p.Name}'"));
                        return ServiceResult.Failure($"找不到印表機 '{printerName}'。可用印表機: {availableNames}");
                    }
                    else
                    {
                        return ServiceResult.Failure($"找不到印表機 '{printerName}'，且系統中沒有已安裝的印表機");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(CheckUsbConnection), GetType(), _logger, new
                {
                    Method = nameof(CheckUsbConnection),
                    ServiceType = GetType().Name,
                    PrinterName = printerConfiguration.Name
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

        /// <summary>
        /// 產生 PCL (Printer Control Language) 格式的列印資料
        /// 針對 HP LaserJet 系列印表機優化
        /// </summary>
        private static byte[] GeneratePclData(string textContent)
        {
            var data = new List<byte>();
            
            // 1. PCL 重置命令
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "E")); // ESC E - Reset
            
            // 2. 設定單位解析度 (300 DPI)
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "&u300D")); // ESC &u300D
            
            // 3. 設定頁面方向為直向
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "&l0O")); // ESC &l0O
            
            // 4. 設定頁面大小為 A4
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "&l26A")); // ESC &l26A
            
            // 5. 設定上邊界 (0.5 英吋)
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "&l150E")); // ESC &l150E
            
            // 6. 設定左邊界 (0.5 英吋)
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "&a150L")); // ESC &a150L
            
            // 7. 選擇字型 - Courier 10 point
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "(s0P")); // ESC (s0P - spacing (fixed)
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "(s10H")); // ESC (s10H - pitch (10 cpi)
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "(s0S")); // ESC (s0S - style (upright)
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "(s0B")); // ESC (s0B - stroke weight (medium)
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "(s3T")); // ESC (s3T - typeface (Courier)
            
            // 8. 設定游標位置到頁面開始
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "&a0R")); // ESC &a0R - row 0
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "&a0C")); // ESC &a0C - column 0
            
            // 9. 添加文字內容
            var lines = textContent.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd('\r');
                data.AddRange(Encoding.UTF8.GetBytes(line));
                
                // 除了最後一行外，每行後面加換行
                if (i < lines.Length - 1)
                {
                    data.AddRange(Encoding.ASCII.GetBytes("\r\n")); // CR+LF
                }
            }
            
            // 10. 換行並確保內容輸出
            data.AddRange(Encoding.ASCII.GetBytes("\r\n\r\n"));
            
            // 11. 送紙命令 (Form Feed) - 強制列印頁面
            data.Add(0x0C); // Form Feed
            
            // 12. 最終重置命令
            data.AddRange(Encoding.ASCII.GetBytes("\x1B" + "E")); // ESC E - Reset
            
            return data.ToArray();
        }

        /// <summary>
        /// 產生 ESC/POS 格式的列印資料 (適用於熱感應印表機)
        /// </summary>
        private static byte[] GenerateEscPosData(string textContent)
        {
            var data = new List<byte>();
            
            // 初始化印表機
            data.AddRange(new byte[] { 0x1B, 0x40 }); // ESC @
            
            // 設定字元編碼為 UTF-8
            data.AddRange(new byte[] { 0x1B, 0x74, 0x06 }); // ESC t 6
            
            // 添加文字內容
            var lines = textContent.Split('\n');
            foreach (var line in lines)
            {
                var lineBytes = Encoding.UTF8.GetBytes(line.TrimEnd('\r'));
                data.AddRange(lineBytes);
                data.AddRange(new byte[] { 0x0A }); // LF
            }
            
            // 送紙並切紙
            data.AddRange(new byte[] { 0x0A, 0x0A, 0x0A }); // 3 個換行
            data.AddRange(new byte[] { 0x1D, 0x56, 0x42, 0x00 }); // GS V B 0 (部分切紙)
            
            return data.ToArray();
        }

        /// <summary>
        /// 產生純文字格式的列印資料 (針對 HP LaserJet 優化)
        /// </summary>
        private static byte[] GeneratePlainTextData(string textContent)
        {
            var data = new List<byte>();
            
            // 添加一些空行開始
            data.AddRange(Encoding.ASCII.GetBytes("\r\n"));
            
            // 添加文字內容
            var lines = textContent.Split('\n');
            foreach (var line in lines)
            {
                var cleanLine = line.TrimEnd('\r');
                data.AddRange(Encoding.UTF8.GetBytes(cleanLine));
                data.AddRange(Encoding.ASCII.GetBytes("\r\n")); // CR+LF
            }
            
            // 添加一些空行結束
            data.AddRange(Encoding.ASCII.GetBytes("\r\n\r\n"));
            
            // 強制送紙命令 (Form Feed)
            data.Add(0x0C); // Form Feed
            
            // 再加一個換行確保
            data.AddRange(Encoding.ASCII.GetBytes("\r\n"));
            
            return data.ToArray();
        }

        /// <summary>
        /// 取得系統中所有可用的印表機
        /// </summary>
        private List<PrinterInfo> GetAvailablePrinters()
        {
            var printers = new List<PrinterInfo>();
            
            try
            {
                // 檢查是否為 Windows 平台
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return printers;
                }

                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");
                foreach (ManagementObject printer in searcher.Get())
                {
                    var printerInfo = new PrinterInfo
                    {
                        Name = printer["Name"]?.ToString() ?? "",
                        PortName = printer["PortName"]?.ToString() ?? "",
                        DriverName = printer["DriverName"]?.ToString() ?? "",
                        Status = printer["PrinterStatus"]?.ToString() ?? "",
                        IsDefault = Convert.ToBoolean(printer["Default"]),
                        IsLocal = Convert.ToBoolean(printer["Local"]),
                        IsShared = Convert.ToBoolean(printer["Shared"])
                    };
                    
                    printers.Add(printerInfo);
                }
            }
            catch
            {
            }
            
            return printers;
        }

        /// <summary>
        /// 根據連接埠找到對應的印表機
        /// </summary>
        private PrinterInfo? FindPrinterByPort(string portName)
        {
            var printers = GetAvailablePrinters();
            return printers.FirstOrDefault(p => 
                string.Equals(p.PortName, portName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 使用 Windows API 直接列印到印表機 (改進版本)
        /// </summary>
        private async Task<ServiceResult> PrintUsingWindowsApi(string printerName, byte[] data, string documentName = "Test Page")
        {
            IntPtr hPrinter = IntPtr.Zero;
            
            try
            {
                // 開啟印表機
                if (!OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
                {
                    var error = Marshal.GetLastWin32Error();
                    return ServiceResult.Failure($"無法開啟印表機 '{printerName}': 錯誤碼 {error}");
                }

                // 重要改變：根據印表機類型選擇正確的 DataType
                string dataType = DetermineDataType(printerName);

                // 設定文件資訊
                var docInfo = new DOC_INFO_1
                {
                    DocName = documentName,
                    OutputFile = "",  // 空字串表示直接列印到印表機
                    DataType = dataType  // 使用動態決定的 DataType
                };

                // 開始文件
                if (!StartDocPrinter(hPrinter, 1, ref docInfo))
                {
                    var error = Marshal.GetLastWin32Error();
                    return ServiceResult.Failure($"無法開始列印文件: 錯誤碼 {error}");
                }

                // 開始頁面
                if (!StartPagePrinter(hPrinter))
                {
                    var error = Marshal.GetLastWin32Error();
                    EndDocPrinter(hPrinter);
                    return ServiceResult.Failure($"無法開始列印頁面: 錯誤碼 {error}");
                }

                // 寫入資料
                if (!WritePrinter(hPrinter, data, data.Length, out int bytesWritten))
                {
                    var error = Marshal.GetLastWin32Error();
                    EndPagePrinter(hPrinter);
                    EndDocPrinter(hPrinter);
                    return ServiceResult.Failure($"無法寫入列印資料: 錯誤碼 {error}");
                }

                // 結束頁面
                if (!EndPagePrinter(hPrinter))
                {
                    var error = Marshal.GetLastWin32Error();
                    EndDocPrinter(hPrinter);
                    return ServiceResult.Failure($"無法結束列印頁面: 錯誤碼 {error}");
                }

                // 結束文件
                if (!EndDocPrinter(hPrinter))
                {
                    var error = Marshal.GetLastWin32Error();
                    return ServiceResult.Failure($"無法結束列印文件: 錯誤碼 {error}");
                }
                
                // 等待較長時間讓印表機處理
                await Task.Delay(3000);
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
            finally
            {
                // 確保關閉印表機控制代碼
                if (hPrinter != IntPtr.Zero)
                {
                    ClosePrinter(hPrinter);
                }
            }
        }

        /// <summary>
        /// 根據印表機決定適當的 DataType
        /// </summary>
        private string DetermineDataType(string printerName)
        {
            // 如果印表機名稱包含 PCL，嘗試使用 TEXT 讓驅動程式處理
            if (printerName.Contains("PCL", StringComparison.OrdinalIgnoreCase))
            {
                return "TEXT";  // 讓 Windows 驅動程式處理格式轉換
            }
            
            // 如果是 EPSON，同樣使用 TEXT
            if (printerName.Contains("EPSON", StringComparison.OrdinalIgnoreCase))
            {
                return "TEXT";
            }
            
            // HP 印表機通常需要驅動程式處理
            if (printerName.Contains("HP", StringComparison.OrdinalIgnoreCase))
            {
                return "TEXT";
            }
            
            // Canon 印表機
            if (printerName.Contains("Canon", StringComparison.OrdinalIgnoreCase))
            {
                return "TEXT";
            }
            
            // 預設使用 RAW (適合標籤機和熱感應印表機)
            return "RAW";
        }

        /// <summary>
        /// 使用 System.Drawing.Printing 列印 (建議優先使用)
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        private async Task<ServiceResult> PrintUsingSystemDrawing(string printerName, string textContent)
        {
            try
            {
                // 檢查是否為 Windows 平台
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return ServiceResult.Failure("System.Drawing.Printing 只支援 Windows 平台");
                }

                using var printDocument = new System.Drawing.Printing.PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;
                
                // 檢查印表機是否有效
                if (!printDocument.PrinterSettings.IsValid)
                {
                    return ServiceResult.Failure($"印表機 '{printerName}' 無效或不可用");
                }

                // 設定列印內容
                var lines = textContent.Split('\n').ToList();
                var currentLine = 0;
                
                printDocument.PrintPage += (sender, e) =>
                {
                    if (e.Graphics == null) return;
                    
                    float yPos = e.MarginBounds.Top;
                    float leftMargin = e.MarginBounds.Left;
                    
                    // 使用等寬字型
                    using var font = new System.Drawing.Font("Courier New", 10);
                    var lineHeight = font.GetHeight(e.Graphics);
                    
                    // 列印每一行
                    while (currentLine < lines.Count && yPos < e.MarginBounds.Bottom)
                    {
                        var line = lines[currentLine].TrimEnd('\r');
                        e.Graphics.DrawString(line, font, 
                            System.Drawing.Brushes.Black, leftMargin, yPos);
                        yPos += lineHeight;
                        currentLine++;
                    }
                    
                    // 如果還有更多行，設定需要更多頁面
                    e.HasMorePages = (currentLine < lines.Count);
                };

                // 執行列印
                printDocument.Print();
                
                // 等待列印處理
                await Task.Delay(2000);
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"列印失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 印表機資訊類別
        /// </summary>
        private class PrinterInfo
        {
            public string Name { get; set; } = "";
            public string PortName { get; set; } = "";
            public string DriverName { get; set; } = "";
            public string Status { get; set; } = "";
            public bool IsDefault { get; set; }
            public bool IsLocal { get; set; }
            public bool IsShared { get; set; }
        }

        #endregion
    }
}
