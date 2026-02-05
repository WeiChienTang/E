using ERPCore2.Data.Entities;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 紙張版面配置
    /// 根據紙張尺寸和字型大小計算出的列印版面參數
    /// </summary>
    public class PaperLayout
    {
        /// <summary>
        /// 每行可容納的字元數（報表寬度）
        /// </summary>
        public int LineWidth { get; set; } = PlainTextFormatter.DefaultLineWidth;

        /// <summary>
        /// 每頁可容納的行數
        /// </summary>
        public int LinesPerPage { get; set; } = 60;

        /// <summary>
        /// 字型大小（pt）
        /// </summary>
        public float FontSize { get; set; } = 10f;

        /// <summary>
        /// 紙張寬度（cm）
        /// </summary>
        public decimal PaperWidth { get; set; } = 21m;

        /// <summary>
        /// 紙張高度（cm）
        /// </summary>
        public decimal PaperHeight { get; set; } = 29.7m;

        /// <summary>
        /// 是否為橫向
        /// </summary>
        public bool IsLandscape { get; set; } = false;

        /// <summary>
        /// 左邊距（cm）
        /// </summary>
        public decimal LeftMargin { get; set; } = 1m;

        /// <summary>
        /// 右邊距（cm）
        /// </summary>
        public decimal RightMargin { get; set; } = 1m;

        /// <summary>
        /// 上邊距（cm）
        /// </summary>
        public decimal TopMargin { get; set; } = 1m;

        /// <summary>
        /// 下邊距（cm）
        /// </summary>
        public decimal BottomMargin { get; set; } = 1m;

        /// <summary>
        /// 可用寬度（cm）= 紙張寬度 - 左邊距 - 右邊距
        /// </summary>
        public decimal AvailableWidth => PaperWidth - LeftMargin - RightMargin;

        /// <summary>
        /// 可用高度（cm）= 紙張高度 - 上邊距 - 下邊距
        /// </summary>
        public decimal AvailableHeight => PaperHeight - TopMargin - BottomMargin;

        /// <summary>
        /// 預設 A4 直向版面配置
        /// </summary>
        public static PaperLayout Default => new PaperLayout();

        /// <summary>
        /// 取得版面配置描述
        /// </summary>
        public string GetDescription()
        {
            var orientation = IsLandscape ? "橫向" : "直向";
            return $"{PaperWidth}×{PaperHeight} cm ({orientation}), 每行 {LineWidth} 字元, 每頁 {LinesPerPage} 行, 字型 {FontSize}pt";
        }
    }

    /// <summary>
    /// 紙張版面計算器
    /// 根據紙張設定計算報表版面配置
    /// </summary>
    public static class PaperLayoutCalculator
    {
        /// <summary>
        /// 預設字型大小（pt）
        /// </summary>
        public const float DefaultFontSize = 10f;

        /// <summary>
        /// 預設邊距（cm）
        /// </summary>
        public const decimal DefaultMargin = 1m;

        /// <summary>
        /// 等寬字型的字元寬度係數
        /// Courier New 10pt 字元寬度約為 0.212cm
        /// 計算方式：字型大小(pt) × 0.0352778(cm/pt) × 0.6(等寬字型寬高比)
        /// </summary>
        public const decimal CharWidthFactor = 0.0211667m; // 10pt 字型的字元寬度 (cm)

        /// <summary>
        /// 行高係數（相對於字型大小）
        /// </summary>
        public const decimal LineHeightFactor = 1.4m;

        /// <summary>
        /// 從紙張設定計算版面配置
        /// </summary>
        /// <param name="paperSetting">紙張設定</param>
        /// <param name="fontSize">字型大小（pt）</param>
        /// <returns>版面配置</returns>
        public static PaperLayout Calculate(PaperSetting? paperSetting, float fontSize = DefaultFontSize)
        {
            // 使用預設 A4 尺寸如果未提供紙張設定
            if (paperSetting == null)
            {
                return CalculateFromDimensions(21m, 29.7m, false, DefaultMargin, DefaultMargin, DefaultMargin, DefaultMargin, fontSize);
            }

            var isLandscape = paperSetting.Orientation?.Equals("Landscape", StringComparison.OrdinalIgnoreCase) == true;

            return CalculateFromDimensions(
                paperSetting.Width,
                paperSetting.Height,
                isLandscape,
                paperSetting.LeftMargin ?? DefaultMargin,
                paperSetting.RightMargin ?? DefaultMargin,
                paperSetting.TopMargin ?? DefaultMargin,
                paperSetting.BottomMargin ?? DefaultMargin,
                fontSize
            );
        }

        /// <summary>
        /// 從尺寸數值計算版面配置
        /// </summary>
        public static PaperLayout CalculateFromDimensions(
            decimal paperWidth,
            decimal paperHeight,
            bool isLandscape,
            decimal leftMargin,
            decimal rightMargin,
            decimal topMargin,
            decimal bottomMargin,
            float fontSize = DefaultFontSize)
        {
            // 如果是橫向，交換寬高
            var actualWidth = isLandscape ? paperHeight : paperWidth;
            var actualHeight = isLandscape ? paperWidth : paperHeight;

            // 計算可用區域
            var availableWidth = actualWidth - leftMargin - rightMargin;
            var availableHeight = actualHeight - topMargin - bottomMargin;

            // 確保可用區域為正值
            if (availableWidth <= 0) availableWidth = 1;
            if (availableHeight <= 0) availableHeight = 1;

            // 計算字元寬度（根據字型大小調整）
            var charWidth = CharWidthFactor * (decimal)fontSize / 10m;

            // 計算每行字元數
            var lineWidth = (int)Math.Floor(availableWidth / charWidth);
            lineWidth = Math.Max(lineWidth, 40); // 最小 40 字元
            lineWidth = Math.Min(lineWidth, 200); // 最大 200 字元

            // 計算行高（cm）
            var lineHeight = (decimal)fontSize * 0.0352778m * LineHeightFactor;

            // 計算每頁行數
            var linesPerPage = (int)Math.Floor(availableHeight / lineHeight);
            linesPerPage = Math.Max(linesPerPage, 10); // 最少 10 行
            linesPerPage = Math.Min(linesPerPage, 200); // 最多 200 行

            return new PaperLayout
            {
                LineWidth = lineWidth,
                LinesPerPage = linesPerPage,
                FontSize = fontSize,
                PaperWidth = actualWidth,
                PaperHeight = actualHeight,
                IsLandscape = isLandscape,
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                TopMargin = topMargin,
                BottomMargin = bottomMargin
            };
        }

        /// <summary>
        /// 根據版面配置計算表格欄位寬度
        /// 按照基準比例調整各欄位寬度，確保總寬度符合 LineWidth
        /// </summary>
        /// <param name="layout">版面配置</param>
        /// <param name="baseColumnWidths">基準欄位寬度（基於 80 字元寬度）</param>
        /// <returns>調整後的欄位寬度</returns>
        public static int[] CalculateColumnWidths(PaperLayout layout, int[] baseColumnWidths)
        {
            const int baseLineWidth = 80;
            var totalBaseWidth = baseColumnWidths.Sum();
            var scaleFactor = (double)layout.LineWidth / baseLineWidth;

            var adjustedWidths = new int[baseColumnWidths.Length];
            var totalAdjusted = 0;

            // 按比例調整每個欄位
            for (int i = 0; i < baseColumnWidths.Length - 1; i++)
            {
                adjustedWidths[i] = Math.Max(2, (int)Math.Round(baseColumnWidths[i] * scaleFactor));
                totalAdjusted += adjustedWidths[i];
            }

            // 最後一個欄位使用剩餘寬度（包含分隔空格）
            var separatorWidth = baseColumnWidths.Length - 1; // 每欄之間有 1 個空格
            adjustedWidths[^1] = Math.Max(2, layout.LineWidth - totalAdjusted - separatorWidth);

            return adjustedWidths;
        }

        /// <summary>
        /// 判斷紙張是否足夠小，需要使用緊湊版面
        /// </summary>
        public static bool IsCompactLayout(PaperLayout layout)
        {
            return layout.LineWidth < 60;
        }

        /// <summary>
        /// 判斷紙張是否足夠大，可以使用寬版面
        /// </summary>
        public static bool IsWideLayout(PaperLayout layout)
        {
            return layout.LineWidth > 100;
        }
    }
}
