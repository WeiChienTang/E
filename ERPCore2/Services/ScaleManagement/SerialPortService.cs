using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// RS-232C 串列埠通訊服務實作
    /// 用於連接磅秤設備（如 LT-100）讀取重量資料
    /// 
    /// 支援 LT-100 所有傳送格式（SF5 設定）：
    ///   Format 1: ST,GS,+03000.0kg        （預設格式，狀態+模式+正負號+數值+單位）
    ///   Format 2: ST,GS,+03000.0,kg        （單位前有逗號）
    ///   Format 3: +03000.0kg               （僅正負號+數值+單位，無狀態/模式前綴）
    ///   Format 4: ST,GS,+03000.0,kg        （無小數點時補空白）
    /// 
    /// 同時相容 SF6 設定（有無正負號、有無前導零）：
    ///   有 +- 有 0000（LT100 預設）/ 有 +- 無 0000 / 無 +- 有 0000 / 無 +- 無 0000
    /// 
    /// 穩定狀態：ST=穩定, US=不穩定, OL=過載
    /// 重量模式：GS=總重, NT=淨重
    /// 單位：kg/lb/g/t
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

        public void ApplySettings(SerialPortSettings settings)
        {
            CurrentSettings = settings;
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
        /// 解析 LT-100 磅秤資料格式（支援所有 SF5/SF6 組合）
        /// 
        /// 支援範例：
        ///   Format 1: ST,GS,+03000.0kg     （預設）
        ///   Format 2: ST,GS,+03000.0,kg    （單位前有逗號）
        ///   Format 3: +03000.0kg            （無狀態/模式前綴）
        ///   Format 4: ST,GS,+03000.0,kg    （無小數點補空白）
        ///   SF6 無正負號: ST,GS,03000.0kg 或 03000.0kg
        ///   SF6 無前導零: ST,GS,+3000.0kg
        /// </summary>
        internal static ScaleDataReceivedEventArgs ParseScaleData(string rawData)
        {
            var result = new ScaleDataReceivedEventArgs { RawData = rawData };

            try
            {
                // 清除前後空白與不可見字元
                var cleaned = rawData.Trim();

                // 嘗試完整格式（Format 1/2/4：含狀態+模式前綴）
                var match = ScaleDataPatternFull().Match(cleaned);

                if (!match.Success)
                {
                    // 嘗試簡潔格式（Format 3：僅正負號+數值+單位，無狀態/模式前綴）
                    match = ScaleDataPatternShort().Match(cleaned);
                }

                if (!match.Success)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"無法解析資料格式：{cleaned}";
                    return result;
                }

                // 狀態與模式：可能不存在（Format 3 無狀態前綴）
                // Format 3 預設為 "US"（不穩定），因為無法從資料中判斷穩定狀態
                // 若 SF4 設為「穩定後連續傳送」+ SF5 設為 Format 3，則每筆資料理論上皆穩定，
                // 但程式無法確認，寧可保守處理。建議使用者維持 Format 1 預設。
                var statusGroup = match.Groups["status"];
                var modeGroup = match.Groups["mode"];
                result.StabilityStatus = statusGroup.Success ? statusGroup.Value.ToUpperInvariant() : "US";
                result.WeightMode = modeGroup.Success ? modeGroup.Value.ToUpperInvariant() : "GS";

                // 正負號：可能不存在（SF6 無 +- 設定），預設為正值
                var signGroup = match.Groups["sign"];
                result.IsPositive = !signGroup.Success || signGroup.Value != "-";

                // 重量數值：去除可能的空白（Format 4 無小數點時補空白）
                var numStr = match.Groups["value"].Value.Trim();
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

                result.Unit = match.Groups["unit"].Value.ToLowerInvariant();
                result.IsValid = true;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"解析資料時發生例外：{ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 完整格式正則（Format 1/2/4）：含狀態 + 模式前綴
        /// 範例：ST,GS,+03000.0kg 或 ST,GS,+03000.0,kg 或 US,NT,03000.0kg
        /// 正負號為可選（相容 SF6 無 +- 設定）
        /// 單位前逗號為可選（相容 Format 2/4）
        /// </summary>
        [GeneratedRegex(@"(?<status>ST|US|OL)\s*,\s*(?<mode>GS|NT)\s*,\s*(?<sign>[+\-])?(?<value>[\d.\s]+)\s*,?\s*(?<unit>kg|lb|g|t)", RegexOptions.IgnoreCase)]
        private static partial Regex ScaleDataPatternFull();

        /// <summary>
        /// 簡潔格式正則（Format 3）：僅正負號 + 數值 + 單位，無狀態/模式前綴
        /// 範例：+03000.0kg 或 03000.0kg
        /// 正負號為可選（相容 SF6 無 +- 設定）
        /// </summary>
        [GeneratedRegex(@"^\s*(?<sign>[+\-])?(?<value>[\d.\s]+)\s*,?\s*(?<unit>kg|lb|g|t)\s*$", RegexOptions.IgnoreCase)]
        private static partial Regex ScaleDataPatternShort();

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
