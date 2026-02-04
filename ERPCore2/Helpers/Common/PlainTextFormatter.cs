using System.Text;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 純文字格式化輔助類別
    /// 提供等寬字型文字格式化功能，支援中英文混排
    /// 適用於報表列印、純文字輸出等場景
    /// </summary>
    public static class PlainTextFormatter
    {
        /// <summary>
        /// 預設報表寬度（字元數）
        /// </summary>
        public const int DefaultLineWidth = 80;

        /// <summary>
        /// 分頁符號（Form Feed）
        /// </summary>
        public const string PageBreak = "\f";

        #region 文字對齊與填充

        /// <summary>
        /// 置中文字
        /// </summary>
        /// <param name="text">要置中的文字</param>
        /// <param name="width">總寬度（字元數）</param>
        /// <returns>置中後的文字</returns>
        public static string CenterText(string text, int width)
        {
            if (string.IsNullOrEmpty(text)) return new string(' ', width);
            
            var displayWidth = GetDisplayWidth(text);
            if (displayWidth >= width) return TruncateText(text, width);
            
            var padding = (width - displayWidth) / 2;
            return new string(' ', padding) + text;
        }

        /// <summary>
        /// 靠右填充（考慮中文寬度）
        /// 文字靠左對齊，右側填充空格
        /// </summary>
        /// <param name="text">要填充的文字</param>
        /// <param name="totalWidth">總寬度（字元數）</param>
        /// <returns>填充後的文字</returns>
        public static string PadRight(string text, int totalWidth)
        {
            if (string.IsNullOrEmpty(text)) return new string(' ', totalWidth);
            
            var displayWidth = GetDisplayWidth(text);
            if (displayWidth >= totalWidth) return TruncateText(text, totalWidth);
            
            return text + new string(' ', totalWidth - displayWidth);
        }

        /// <summary>
        /// 靠左填充（考慮中文寬度）
        /// 文字靠右對齊，左側填充空格（適用於數字）
        /// </summary>
        /// <param name="text">要填充的文字</param>
        /// <param name="totalWidth">總寬度（字元數）</param>
        /// <returns>填充後的文字</returns>
        public static string PadLeft(string text, int totalWidth)
        {
            if (string.IsNullOrEmpty(text)) return new string(' ', totalWidth);
            
            var displayWidth = GetDisplayWidth(text);
            if (displayWidth >= totalWidth) return text;
            
            return new string(' ', totalWidth - displayWidth) + text;
        }

        #endregion

        #region 文字寬度計算

        /// <summary>
        /// 取得顯示寬度（中文字佔2個寬度，英文和數字佔1個寬度）
        /// </summary>
        /// <param name="text">要計算的文字</param>
        /// <returns>顯示寬度</returns>
        public static int GetDisplayWidth(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return text.Sum(c => IsWideChar(c) ? 2 : 1);
        }

        /// <summary>
        /// 判斷是否為寬字元（中文、日文、韓文等）
        /// </summary>
        /// <param name="c">要判斷的字元</param>
        /// <returns>是否為寬字元</returns>
        public static bool IsWideChar(char c)
        {
            // 基本的 CJK 字元範圍判斷
            // 可以根據需求擴展更精確的 Unicode 範圍
            return c > 127;
        }

        #endregion

        #region 文字截斷

        /// <summary>
        /// 截斷文字（考慮中文寬度）
        /// </summary>
        /// <param name="text">要截斷的文字</param>
        /// <param name="maxWidth">最大寬度（字元數）</param>
        /// <param name="ellipsis">是否加上省略號</param>
        /// <returns>截斷後的文字</returns>
        public static string TruncateText(string text, int maxWidth, bool ellipsis = false)
        {
            if (string.IsNullOrEmpty(text)) return "";
            
            var effectiveMaxWidth = ellipsis && maxWidth > 2 ? maxWidth - 2 : maxWidth;
            
            var result = new StringBuilder();
            var currentWidth = 0;
            
            foreach (var c in text)
            {
                var charWidth = IsWideChar(c) ? 2 : 1;
                if (currentWidth + charWidth > effectiveMaxWidth) break;
                result.Append(c);
                currentWidth += charWidth;
            }
            
            // 如果有截斷且需要省略號
            if (ellipsis && result.Length < text.Length)
            {
                result.Append("..");
            }
            
            return result.ToString();
        }

        #endregion

        #region 分隔線

        /// <summary>
        /// 生成等號分隔線
        /// </summary>
        /// <param name="width">寬度（字元數）</param>
        /// <returns>分隔線字串</returns>
        public static string Separator(int width = DefaultLineWidth)
        {
            return new string('=', width);
        }

        /// <summary>
        /// 生成虛線分隔線
        /// </summary>
        /// <param name="width">寬度（字元數）</param>
        /// <returns>分隔線字串</returns>
        public static string ThinSeparator(int width = DefaultLineWidth)
        {
            return new string('-', width);
        }

        /// <summary>
        /// 生成點線分隔線
        /// </summary>
        /// <param name="width">寬度（字元數）</param>
        /// <returns>分隔線字串</returns>
        public static string DottedSeparator(int width = DefaultLineWidth)
        {
            return new string('.', width);
        }

        #endregion

        #region 表格格式化

        /// <summary>
        /// 格式化表格行
        /// </summary>
        /// <param name="columns">欄位內容與寬度的配對</param>
        /// <param name="separator">欄位分隔字元</param>
        /// <returns>格式化後的表格行</returns>
        public static string FormatTableRow(IEnumerable<(string Text, int Width, PlainTextAlignment Alignment)> columns, string separator = " ")
        {
            var parts = columns.Select(col =>
            {
                return col.Alignment switch
                {
                    PlainTextAlignment.Left => PadRight(col.Text, col.Width),
                    PlainTextAlignment.Right => PadLeft(col.Text, col.Width),
                    PlainTextAlignment.Center => CenterText(col.Text, col.Width),
                    _ => PadRight(col.Text, col.Width)
                };
            });
            
            return string.Join(separator, parts);
        }

        /// <summary>
        /// 格式化表格行（簡化版：所有欄位靠左對齊）
        /// </summary>
        /// <param name="columns">欄位內容與寬度的配對</param>
        /// <param name="separator">欄位分隔字元</param>
        /// <returns>格式化後的表格行</returns>
        public static string FormatTableRow(IEnumerable<(string Text, int Width)> columns, string separator = " ")
        {
            return FormatTableRow(columns.Select(c => (c.Text, c.Width, PlainTextAlignment.Left)), separator);
        }

        #endregion

        #region 金額與數字格式化

        /// <summary>
        /// 格式化金額（千分位，無小數）
        /// </summary>
        /// <param name="amount">金額</param>
        /// <returns>格式化後的金額字串</returns>
        public static string FormatAmount(decimal amount)
        {
            return amount.ToString("N0");
        }

        /// <summary>
        /// 格式化金額（千分位，2位小數）
        /// </summary>
        /// <param name="amount">金額</param>
        /// <returns>格式化後的金額字串</returns>
        public static string FormatAmountWithDecimals(decimal amount)
        {
            return amount.ToString("N2");
        }

        /// <summary>
        /// 格式化數量（千分位，無小數）
        /// </summary>
        /// <param name="quantity">數量</param>
        /// <returns>格式化後的數量字串</returns>
        public static string FormatQuantity(decimal quantity)
        {
            return quantity.ToString("N0");
        }

        /// <summary>
        /// 格式化數量（千分位，2位小數）
        /// </summary>
        /// <param name="quantity">數量</param>
        /// <returns>格式化後的數量字串</returns>
        public static string FormatQuantityWithDecimals(decimal quantity)
        {
            return quantity.ToString("N2");
        }

        #endregion

        #region 日期格式化

        /// <summary>
        /// 格式化日期（yyyy/MM/dd）
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>格式化後的日期字串</returns>
        public static string FormatDate(DateTime date)
        {
            return date.ToString("yyyy/MM/dd");
        }

        /// <summary>
        /// 格式化日期時間（yyyy/MM/dd HH:mm）
        /// </summary>
        /// <param name="dateTime">日期時間</param>
        /// <returns>格式化後的日期時間字串</returns>
        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm");
        }

        /// <summary>
        /// 格式化可為空的日期
        /// </summary>
        /// <param name="date">可為空的日期</param>
        /// <param name="defaultValue">預設值</param>
        /// <returns>格式化後的日期字串</returns>
        public static string FormatDate(DateTime? date, string defaultValue = "")
        {
            return date.HasValue ? FormatDate(date.Value) : defaultValue;
        }

        #endregion

        #region 報表區塊建構

        /// <summary>
        /// 建構報表標題區塊
        /// </summary>
        /// <param name="companyName">公司名稱</param>
        /// <param name="reportTitle">報表標題</param>
        /// <param name="width">寬度</param>
        /// <returns>標題區塊</returns>
        public static string BuildTitleSection(string companyName, string reportTitle, int width = DefaultLineWidth)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Separator(width));
            sb.AppendLine(CenterText(companyName, width));
            sb.AppendLine(CenterText(reportTitle, width));
            sb.AppendLine(Separator(width));
            return sb.ToString();
        }

        /// <summary>
        /// 建構簽名區塊
        /// </summary>
        /// <param name="labels">簽名欄標籤</param>
        /// <param name="width">寬度</param>
        /// <returns>簽名區塊</returns>
        public static string BuildSignatureSection(string[] labels, int width = DefaultLineWidth)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(string.Join("    ", labels.Select(l => $"{l}：________________")));
            sb.AppendLine();
            sb.AppendLine(Separator(width));
            return sb.ToString();
        }

        /// <summary>
        /// 建構合計行
        /// </summary>
        /// <param name="label">標籤</param>
        /// <param name="amount">金額</param>
        /// <param name="suffix">後綴（如稅率說明）</param>
        /// <param name="labelWidth">標籤前的空白寬度</param>
        /// <param name="amountWidth">金額寬度</param>
        /// <returns>合計行字串</returns>
        public static string BuildTotalLine(string label, decimal amount, string suffix = "", int labelWidth = 50, int amountWidth = 12)
        {
            var amountStr = PadLeft(FormatAmount(amount), amountWidth);
            var suffixStr = string.IsNullOrEmpty(suffix) ? "" : $" ({suffix})";
            return $"{new string(' ', labelWidth)}{label}：{amountStr}{suffixStr}";
        }

        #endregion
    }

    /// <summary>
    /// 純文字對齊方式
    /// </summary>
    public enum PlainTextAlignment
    {
        /// <summary>靠左對齊</summary>
        Left,
        /// <summary>靠右對齊</summary>
        Right,
        /// <summary>置中對齊</summary>
        Center
    }
}
