namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 報表頁面配置定義（適用於各種紙張格式）
    /// </summary>
    public class ReportPageLayout
    {
        /// <summary>頁面總高度（mm）</summary>
        public decimal PageHeight { get; set; }

        /// <summary>頁面寬度（mm）</summary>
        public decimal PageWidth { get; set; }

        /// <summary>表頭高度（mm）- 包含公司名稱、報表標題</summary>
        public decimal HeaderHeight { get; set; }

        /// <summary>資訊區塊高度（mm）- 包含單號、日期等固定資訊</summary>
        public decimal InfoSectionHeight { get; set; }

        /// <summary>表格標題列高度（mm）</summary>
        public decimal TableHeaderHeight { get; set; }

        /// <summary>明細基本行高（mm）- 單行無備註時的高度</summary>
        public decimal RowBaseHeight { get; set; }

        /// <summary>備註額外行高（mm）- 每增加一行備註的高度</summary>
        public decimal RemarkExtraLineHeight { get; set; }

        /// <summary>備註每行字元數（用於估算行數）</summary>
        public int RemarkCharsPerLine { get; set; }

        /// <summary>統計區塊高度（mm）- 包含金額小計、稅額等</summary>
        public decimal SummaryHeight { get; set; }

        /// <summary>簽名區塊高度（mm）</summary>
        public decimal SignatureHeight { get; set; }

        /// <summary>安全邊距（mm）- 避免內容過於接近邊界</summary>
        public decimal SafetyMargin { get; set; }

        /// <summary>
        /// 計算可用於明細的高度
        /// </summary>
        public decimal GetAvailableDetailsHeight()
        {
            return PageHeight
                - HeaderHeight
                - InfoSectionHeight
                - TableHeaderHeight
                - SummaryHeight
                - SignatureHeight
                - SafetyMargin;
        }

        /// <summary>
        /// 預設中一刀格式配置（連續報表紙 213.3mm × 139.7mm）
        /// </summary>
        public static ReportPageLayout ContinuousForm()
        {
            return new ReportPageLayout
            {
                PageHeight = 135.7m,           // 可用高度（139.7mm - 上下邊距各2mm）
                PageWidth = 209.3m,            // 可用寬度（213.3mm - 左右邊距各2mm）
                HeaderHeight = 20m,            // 表頭（公司資訊 + 報表標題）
                InfoSectionHeight = 18m,       // 資訊區塊（採購單號、日期、廠商等）
                TableHeaderHeight = 6m,        // 表格標題列（序號、品名等欄位名）
                RowBaseHeight = 5.5m,          // 明細基本行高（無備註或單行備註）
                RemarkExtraLineHeight = 6m,    // 備註每增加一行的高度
                RemarkCharsPerLine = 40,       // 備註欄每行可容納字元數（根據欄位寬度30%估算）
                SummaryHeight = 18m,           // 金額統計區（金額小計、稅額、總計）
                SignatureHeight = 15m,         // 簽名區域（採購人員、核准人員、收貨確認）
                SafetyMargin = 10m             // 安全邊距（增加至10mm，避免內容被切掉）
            };
        }

        /// <summary>
        /// A4 直式格式配置（210mm × 297mm）
        /// </summary>
        public static ReportPageLayout A4Portrait()
        {
            return new ReportPageLayout
            {
                PageHeight = 277m,             // A4 高度扣除邊距（297mm - 上下邊距各10mm）
                PageWidth = 190m,              // A4 寬度扣除邊距（210mm - 左右邊距各10mm）
                HeaderHeight = 25m,            // 表頭
                InfoSectionHeight = 20m,       // 資訊區塊
                TableHeaderHeight = 8m,        // 表格標題列
                RowBaseHeight = 6m,            // 明細基本行高
                RemarkExtraLineHeight = 6.5m,  // 備註額外行高
                RemarkCharsPerLine = 50,       // 備註每行字元數（A4寬度較寬）
                SummaryHeight = 25m,           // 金額統計區
                SignatureHeight = 20m,         // 簽名區域
                SafetyMargin = 5m              // 安全邊距
            };
        }

        /// <summary>
        /// A4 橫式格式配置（297mm × 210mm）
        /// </summary>
        public static ReportPageLayout A4Landscape()
        {
            return new ReportPageLayout
            {
                PageHeight = 190m,             // A4 橫式高度扣除邊距
                PageWidth = 277m,              // A4 橫式寬度扣除邊距
                HeaderHeight = 20m,            // 表頭（橫式可稍低）
                InfoSectionHeight = 18m,       // 資訊區塊
                TableHeaderHeight = 7m,        // 表格標題列
                RowBaseHeight = 5.5m,          // 明細基本行高
                RemarkExtraLineHeight = 6m,    // 備註額外行高
                RemarkCharsPerLine = 70,       // 備註每行字元數（橫式更寬）
                SummaryHeight = 20m,           // 金額統計區
                SignatureHeight = 18m,         // 簽名區域
                SafetyMargin = 4m              // 安全邊距
            };
        }
    }
}
