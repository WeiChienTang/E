using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// RS-232C 串列埠通訊服務實作
    /// 用於連接磅秤設備（如 LT-100）讀取重量資料
    /// 
    /// LT-100 傳送格式（Format 1）：
    ///   ST,GS,+0052050kg CR LF
    ///   ├─ ST/US/OL  穩定/不穩/過載
    ///   ├─ GS/NT     總重/淨重
    ///   ├─ +/-       正負號
    ///   ├─ 7位數     六位重量值+小數點（無小數點時補 "0"）
    ///   └─ kg/lb/g/t 單位
    /// </summary>
    public partial class SerialPortService : ISerialPortService
    {
        private SerialPort? _serialPort;
        private readonly StringBuilder _buffer = new();
        private readonly object _lock = new();
        private readonly ILogger<SerialPortService> _logger;

        public bool IsOpen => _serialPort?.IsOpen == true;
        public SerialPortSettings CurrentSettings { get; private set; } = new();

        public event EventHandler<ScaleDataReceivedEventArgs>? DataReceived;

        public SerialPortService(ILogger<SerialPortService> logger)
        {
            _logger = logger;
        }

        public string[] GetAvailablePortNames()
        {
            try
            {
                return SerialPort.GetPortNames();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得可用串列埠清單失敗");
                return [];
            }
        }

        public void Open(SerialPortSettings settings)
        {
            lock (_lock)
            {
                // 若已開啟則先關閉
                Close();

                try
                {
                    _serialPort = new SerialPort
                    {
                        PortName = settings.PortName,
                        BaudRate = settings.BaudRate,
                        Parity = ParseParity(settings.Parity),
                        DataBits = settings.DataBits,
                        StopBits = ParseStopBits(settings.StopBits),
                        ReadTimeout = 3000,
                        WriteTimeout = 3000,
                        Encoding = Encoding.ASCII,
                        NewLine = "\r\n"
                    };

                    _serialPort.DataReceived += OnSerialDataReceived;
                    _serialPort.Open();

                    CurrentSettings = settings;
                    _buffer.Clear();

                    _logger.LogInformation(
                        "串列埠已開啟：{Port} ({BaudRate}, {Parity}, {DataBits}, {StopBits})",
                        settings.PortName, settings.BaudRate, settings.Parity, settings.DataBits, settings.StopBits);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "開啟串列埠 {Port} 失敗", settings.PortName);
                    _serialPort?.Dispose();
                    _serialPort = null;
                    throw;
                }
            }
        }

        public void Close()
        {
            lock (_lock)
            {
                if (_serialPort == null) return;

                try
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.DataReceived -= OnSerialDataReceived;
                        _serialPort.Close();
                    }

                    _serialPort.Dispose();
                    _logger.LogInformation("串列埠已關閉：{Port}", CurrentSettings.PortName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "關閉串列埠時發生錯誤");
                }
                finally
                {
                    _serialPort = null;
                    _buffer.Clear();
                }
            }
        }

        /// <summary>
        /// 串列埠資料接收處理（由 SerialPort 背景執行緒觸發）
        /// </summary>
        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;

            try
            {
                var data = _serialPort.ReadExisting();
                if (string.IsNullOrEmpty(data)) return;

                lock (_lock)
                {
                    _buffer.Append(data);

                    // 以 CR LF 為斷行，逐筆解析
                    var bufferStr = _buffer.ToString();
                    int newlineIndex;
                    while ((newlineIndex = bufferStr.IndexOf('\n')) >= 0)
                    {
                        var line = bufferStr[..newlineIndex].TrimEnd('\r', '\n');
                        bufferStr = bufferStr[(newlineIndex + 1)..];

                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var parsed = ParseScaleData(line);
                            DataReceived?.Invoke(this, parsed);
                        }
                    }

                    _buffer.Clear();
                    _buffer.Append(bufferStr);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "讀取串列埠資料時發生錯誤");
            }
        }

        /// <summary>
        /// 解析 LT-100 磅秤資料格式
        /// 範例：ST,GS,+0052050kg  或  US,GS,+0052090kg
        /// </summary>
        internal static ScaleDataReceivedEventArgs ParseScaleData(string rawData)
        {
            var result = new ScaleDataReceivedEventArgs { RawData = rawData };

            try
            {
                // 清除前後空白與不可見字元
                var cleaned = rawData.Trim();

                // 使用正則表達式解析
                // 格式：(ST|US|OL),(GS|NT),(+|-)\d+(\.\d+)?(kg|lb|g|t)
                var match = ScaleDataPattern().Match(cleaned);

                if (!match.Success)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"無法解析資料格式：{cleaned}";
                    return result;
                }

                result.StabilityStatus = match.Groups["status"].Value;
                result.WeightMode = match.Groups["mode"].Value;
                result.IsPositive = match.Groups["sign"].Value != "-";

                var numStr = match.Groups["value"].Value;
                if (decimal.TryParse(numStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var weightValue))
                {
                    result.WeightValue = result.IsPositive ? weightValue : -weightValue;
                }
                else
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"無法解析重量數值：{numStr}";
                    return result;
                }

                result.Unit = match.Groups["unit"].Value;
                result.IsValid = true;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"解析資料時發生例外：{ex.Message}";
            }

            return result;
        }

        [GeneratedRegex(@"(?<status>ST|US|OL)\s*,\s*(?<mode>GS|NT)\s*,\s*(?<sign>[+\-])(?<value>[\d.]+)\s*(?<unit>kg|lb|g|t)", RegexOptions.IgnoreCase)]
        private static partial Regex ScaleDataPattern();

        private static Parity ParseParity(string parity) => parity?.ToUpperInvariant() switch
        {
            "EVEN" or "E" => System.IO.Ports.Parity.Even,
            "ODD" or "O" => System.IO.Ports.Parity.Odd,
            "NONE" or "N" => System.IO.Ports.Parity.None,
            _ => System.IO.Ports.Parity.Even
        };

        private static StopBits ParseStopBits(int stopBits) => stopBits switch
        {
            0 => System.IO.Ports.StopBits.None,
            1 => System.IO.Ports.StopBits.One,
            2 => System.IO.Ports.StopBits.Two,
            _ => System.IO.Ports.StopBits.One
        };

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}
