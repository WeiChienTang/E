namespace ERPCore2.Models.Reports
{
    /// <summary>
    /// 報表區段定義
    /// 用於詳細報表的區段選擇功能，讓使用者選擇要列印的內容
    /// </summary>
    public class ReportSectionDefinition
    {
        /// <summary>區段識別碼（如 "Vehicles", "BankAccounts"）</summary>
        public string Key { get; set; } = "";

        /// <summary>顯示名稱（如 "車輛資訊"）</summary>
        public string Label { get; set; } = "";

        /// <summary>圖示 CSS class（如 "bi-truck"）</summary>
        public string Icon { get; set; } = "";

        /// <summary>是否預設勾選</summary>
        public bool IsChecked { get; set; } = true;

        /// <summary>是否可用（權限或模組未啟用時為 false）</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>不可用原因（如 "模組未啟用"、"無此權限"）</summary>
        public string? DisabledReason { get; set; }
    }
}
