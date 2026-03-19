namespace ERPCore2.Services
{
    /// <summary>
    /// RS-232C 串列埠通訊服務介面
    /// 用於連接磅秤設備（如 LT-100）讀取重量資料
    /// </summary>
    public interface ISerialPortService : IDisposable
    {
        /// <summary>是否已開啟串列埠</summary>
        bool IsOpen { get; }

        /// <summary>目前的連線設定</summary>
        SerialPortSettings CurrentSettings { get; }

        /// <summary>取得系統可用的串列埠名稱清單</summary>
        string[] GetAvailablePortNames();

        /// <summary>儲存連線參數（不實際開啟串列埠，供"讀取"按鈕自動連線時使用）</summary>
        void ApplySettings(SerialPortSettings settings);

        /// <summary>開啟串列埠連線</summary>
        void Open(SerialPortSettings settings);

        /// <summary>關閉串列埠連線</summary>
        void Close();

        /// <summary>
        /// 當收到完整的一筆重量資料時觸發
        /// </summary>
        event EventHandler<ScaleDataReceivedEventArgs>? DataReceived;
    }

    /// <summary>
    /// RS-232C 串列埠連線參數
    /// </summary>
    public class SerialPortSettings
    {
        /// <summary>串列埠名稱，例如 COM1、COM3</summary>
        public string PortName { get; set; } = "COM1";

        /// <summary>鮑率（傳輸速率），預設 2400</summary>
        public int BaudRate { get; set; } = 2400;

        /// <summary>同位檢查：None / Even / Odd，預設 Even</summary>
        public string Parity { get; set; } = "Even";

        /// <summary>資料位元數，預設 7</summary>
        public int DataBits { get; set; } = 7;

        /// <summary>停止位元數，預設 1</summary>
        public int StopBits { get; set; } = 1;
    }

    /// <summary>
    /// 磅秤資料接收事件參數
    /// 解析 LT-100 RS-232C 傳送格式（支援 Format 1～4、SF6 有無正負號/前導零組合）
    /// 範例：ST,GS,+03000.0kg 或 ST,GS,+03000.0,kg 或 +03000.0kg 或 03000.0kg
    /// </summary>
    public class ScaleDataReceivedEventArgs : EventArgs
    {
        /// <summary>原始資料字串</summary>
        public string RawData { get; set; } = string.Empty;

        /// <summary>穩定狀態：ST=穩定, US=不穩定, OL=過載</summary>
        public string StabilityStatus { get; set; } = string.Empty;

        /// <summary>重量模式：GS=總重, NT=淨重</summary>
        public string WeightMode { get; set; } = string.Empty;

        /// <summary>正負號：true=正值, false=負值</summary>
        public bool IsPositive { get; set; } = true;

        /// <summary>重量數值</summary>
        public decimal WeightValue { get; set; }

        /// <summary>單位：kg, lb, g, t</summary>
        public string Unit { get; set; } = "kg";

        /// <summary>是否為穩定狀態</summary>
        public bool IsStable => StabilityStatus == "ST";

        /// <summary>是否過載</summary>
        public bool IsOverload => StabilityStatus == "OL";

        /// <summary>是否解析成功</summary>
        public bool IsValid { get; set; }

        /// <summary>解析失敗時的錯誤訊息</summary>
        public string? ErrorMessage { get; set; }
    }
}
