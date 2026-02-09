using System.Drawing;

namespace ERPCore2.Models.Reports
{
    /// <summary>
    /// 格式化報表文件
    /// 用於定義報表的結構化內容，支援文字、表格、線條、圖片等元素
    /// </summary>
    public class FormattedDocument
    {
        /// <summary>
        /// 文件名稱（列印時顯示）
        /// </summary>
        public string DocumentName { get; set; } = "Report";

        /// <summary>
        /// 頁首元素集合（每頁都會重複顯示）
        /// </summary>
        public List<PageElement> HeaderElements { get; set; } = new();

        /// <summary>
        /// 頁面元素集合（主要內容）
        /// </summary>
        public List<PageElement> Elements { get; set; } = new();

        /// <summary>
        /// 頁尾元素集合（只在最後一頁顯示）
        /// </summary>
        public List<PageElement> FooterElements { get; set; } = new();

        /// <summary>
        /// 頁面設定
        /// </summary>
        public DocumentPageSettings PageSettings { get; set; } = new();

        #region Fluent API

        /// <summary>
        /// 設定文件名稱
        /// </summary>
        public FormattedDocument SetDocumentName(string name)
        {
            DocumentName = name;
            return this;
        }

        /// <summary>
        /// 設定頁面邊距（單位：公分）
        /// </summary>
        public FormattedDocument SetMargins(float left, float top, float right, float bottom)
        {
            PageSettings.LeftMargin = left;
            PageSettings.TopMargin = top;
            PageSettings.RightMargin = right;
            PageSettings.BottomMargin = bottom;
            return this;
        }

        /// <summary>
        /// 開始定義頁首區塊（每頁都會重複顯示）
        /// </summary>
        public FormattedDocument BeginHeader(Action<FormattedDocument> headerBuilder)
        {
            var headerDoc = new FormattedDocument();
            headerBuilder(headerDoc);
            HeaderElements.AddRange(headerDoc.Elements);
            return this;
        }

        /// <summary>
        /// 開始定義頁尾區塊（只在最後一頁顯示）
        /// </summary>
        public FormattedDocument BeginFooter(Action<FormattedDocument> footerBuilder)
        {
            var footerDoc = new FormattedDocument();
            footerBuilder(footerDoc);
            FooterElements.AddRange(footerDoc.Elements);
            return this;
        }

        /// <summary>
        /// 新增標題
        /// </summary>
        public FormattedDocument AddTitle(string text, float fontSize = 16, bool bold = true)
        {
            Elements.Add(new TextElement
            {
                Text = text,
                FontSize = fontSize,
                Bold = bold,
                Alignment = TextAlignment.Center
            });
            return this;
        }

        /// <summary>
        /// 新增文字
        /// </summary>
        public FormattedDocument AddText(string text, float fontSize = 10, TextAlignment alignment = TextAlignment.Left, bool bold = false)
        {
            Elements.Add(new TextElement
            {
                Text = text,
                FontSize = fontSize,
                Alignment = alignment,
                Bold = bold
            });
            return this;
        }

        /// <summary>
        /// 新增分隔線
        /// </summary>
        public FormattedDocument AddLine(LineStyle style = LineStyle.Solid, float thickness = 1)
        {
            Elements.Add(new LineElement
            {
                Style = style,
                Thickness = thickness
            });
            return this;
        }

        /// <summary>
        /// 新增垂直間距
        /// </summary>
        public FormattedDocument AddSpacing(float height = 10)
        {
            Elements.Add(new SpacingElement { Height = height });
            return this;
        }

        /// <summary>
        /// 新增表格
        /// </summary>
        public FormattedDocument AddTable(Action<TableBuilder> configure)
        {
            var builder = new TableBuilder();
            configure(builder);
            Elements.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// 新增圖片
        /// </summary>
        public FormattedDocument AddImage(byte[] imageData, float? width = null, float? height = null, TextAlignment alignment = TextAlignment.Center)
        {
            Elements.Add(new ImageElement
            {
                ImageData = imageData,
                Width = width,
                Height = height,
                Alignment = alignment
            });
            return this;
        }

        /// <summary>
        /// 新增分頁
        /// </summary>
        public FormattedDocument AddPageBreak()
        {
            Elements.Add(new PageBreakElement());
            return this;
        }

        /// <summary>
        /// 合併另一個文件的內容到此文件
        /// 用於批次列印時將多個報表合併為一個文件
        /// </summary>
        /// <param name="other">要合併的文件</param>
        public FormattedDocument MergeFrom(FormattedDocument other)
        {
            if (other == null) return this;
            
            // 合併主要元素
            Elements.AddRange(other.Elements);
            
            // 保留第一個文件的頁首頁尾設定
            // 不合併 HeaderElements 和 FooterElements
            
            return this;
        }

        /// <summary>
        /// 新增簽名區
        /// </summary>
        public FormattedDocument AddSignatureSection(params string[] labels)
        {
            Elements.Add(new SignatureSectionElement { Labels = labels.ToList() });
            return this;
        }

        /// <summary>
        /// 新增鍵值對行（如：採購單號：PO001    日期：2026/01/01）
        /// </summary>
        public FormattedDocument AddKeyValueRow(params (string Key, string Value)[] pairs)
        {
            Elements.Add(new KeyValueRowElement { Pairs = pairs.ToList() });
            return this;
        }

        /// <summary>
        /// 新增三欄標頭行（左側靠左、中間置中放大、右側靠右）
        /// </summary>
        /// <param name="leftText">左側文字</param>
        /// <param name="centerText">中間文字（置中放大）</param>
        /// <param name="rightText">右側文字</param>
        /// <param name="centerFontSize">中間文字字型大小</param>
        /// <param name="centerBold">中間文字是否粗體</param>
        /// <param name="sideFontSize">左右文字字型大小</param>
        public FormattedDocument AddThreeColumnHeader(
            string leftText,
            string centerText,
            string rightText,
            float centerFontSize = 14f,
            bool centerBold = true,
            float sideFontSize = 10f)
        {
            Elements.Add(new ThreeColumnHeaderElement
            {
                LeftText = leftText,
                CenterText = centerText,
                RightText = rightText,
                CenterFontSize = centerFontSize,
                CenterBold = centerBold,
                SideFontSize = sideFontSize
            });
            return this;
        }

        /// <summary>
        /// 新增報表標頭區塊（中間多行標題、右側多行資訊，各自有獨立行高）
        /// </summary>
        /// <param name="centerLines">中間標題行（文字、字型大小、是否粗體）</param>
        /// <param name="rightLines">右側資訊行</param>
        /// <param name="rightFontSize">右側字型大小</param>
        public FormattedDocument AddReportHeaderBlock(
            List<(string Text, float FontSize, bool Bold)> centerLines,
            List<string> rightLines,
            float rightFontSize = 10f)
        {
            Elements.Add(new ReportHeaderBlockElement
            {
                CenterLines = centerLines,
                RightLines = rightLines,
                RightFontSize = rightFontSize
            });
            return this;
        }

        /// <summary>
        /// 新增左右並排區塊（用於備註和金額並排顯示）
        /// </summary>
        /// <param name="leftContent">左側內容（多行文字）</param>
        /// <param name="leftTitle">左側標題</param>
        /// <param name="leftHasBorder">左側是否有邊框</param>
        /// <param name="rightContent">右側內容（多行文字）</param>
        /// <param name="rightTitle">右側標題</param>
        /// <param name="rightHasBorder">右側是否有邊框</param>
        /// <param name="leftWidthRatio">左側寬度比例</param>
        public FormattedDocument AddTwoColumnSection(
            List<string> leftContent,
            string? leftTitle = null,
            bool leftHasBorder = false,
            List<string>? rightContent = null,
            string? rightTitle = null,
            bool rightHasBorder = false,
            float leftWidthRatio = 0.6f)
        {
            Elements.Add(new TwoColumnSectionElement
            {
                LeftContent = leftContent,
                LeftTitle = leftTitle,
                LeftHasBorder = leftHasBorder,
                RightContent = rightContent ?? new List<string>(),
                RightTitle = rightTitle,
                RightHasBorder = rightHasBorder,
                LeftWidthRatio = leftWidthRatio
            });
            return this;
        }

        #endregion
    }

    /// <summary>
    /// 文件頁面設定
    /// </summary>
    public class DocumentPageSettings
    {
        /// <summary>紙張寬度（公分），預設 A4 寬度</summary>
        public float PageWidth { get; set; } = 21.0f;
        /// <summary>紙張高度（公分），預設 A4 高度</summary>
        public float PageHeight { get; set; } = 29.7f;
        /// <summary>左邊距（公分）</summary>
        public float LeftMargin { get; set; } = 1.0f;
        /// <summary>上邊距（公分）</summary>
        public float TopMargin { get; set; } = 1.0f;
        /// <summary>右邊距（公分）</summary>
        public float RightMargin { get; set; } = 1.0f;
        /// <summary>下邊距（公分）</summary>
        public float BottomMargin { get; set; } = 1.0f;
        /// <summary>預設字型名稱</summary>
        public string FontName { get; set; } = "Microsoft JhengHei"; // 微軟正黑體
        /// <summary>預設字型大小</summary>
        public float DefaultFontSize { get; set; } = 10f;
    }

    #region 頁面元素

    /// <summary>
    /// 頁面元素基底類別
    /// </summary>
    public abstract class PageElement { }

    /// <summary>
    /// 文字元素
    /// </summary>
    public class TextElement : PageElement
    {
        public string Text { get; set; } = "";
        public float FontSize { get; set; } = 10;
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
        public string? FontName { get; set; } // null 表示使用預設字型
    }

    /// <summary>
    /// 線條元素
    /// </summary>
    public class LineElement : PageElement
    {
        public LineStyle Style { get; set; } = LineStyle.Solid;
        public float Thickness { get; set; } = 1;
    }

    /// <summary>
    /// 間距元素
    /// </summary>
    public class SpacingElement : PageElement
    {
        public float Height { get; set; } = 10;
    }

    /// <summary>
    /// 分頁元素
    /// </summary>
    public class PageBreakElement : PageElement { }

    /// <summary>
    /// 圖片元素
    /// </summary>
    public class ImageElement : PageElement
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public float? Width { get; set; }  // null 表示自動
        public float? Height { get; set; } // null 表示自動
        public TextAlignment Alignment { get; set; } = TextAlignment.Center;
    }

    /// <summary>
    /// 表格元素
    /// </summary>
    public class TableElement : PageElement
    {
        public List<TableColumn> Columns { get; set; } = new();
        public List<TableRow> Rows { get; set; } = new();
        public bool ShowBorder { get; set; } = true;
        public bool ShowHeaderBackground { get; set; } = true;
        /// <summary>表頭欄位之間是否顯示 | 分隔符</summary>
        public bool ShowHeaderSeparator { get; set; } = false;
        /// <summary>表頭下方是否顯示橫線（分隔表頭與內容）</summary>
        public bool ShowHeaderUnderline { get; set; } = true;
        public float RowHeight { get; set; } = 20f;
        public float HeaderRowHeight { get; set; } = 25f;
    }

    /// <summary>
    /// 簽名區元素
    /// </summary>
    public class SignatureSectionElement : PageElement
    {
        public List<string> Labels { get; set; } = new();
        public float LineWidth { get; set; } = 80f; // 簽名線寬度
    }

    /// <summary>
    /// 鍵值對行元素
    /// </summary>
    public class KeyValueRowElement : PageElement
    {
        public List<(string Key, string Value)> Pairs { get; set; } = new();
    }

    /// <summary>
    /// 三欄標頭行元素（左/中/右 對齊）
    /// </summary>
    public class ThreeColumnHeaderElement : PageElement
    {
        /// <summary>左側文字</summary>
        public string LeftText { get; set; } = "";
        /// <summary>中間文字（置中顯示）</summary>
        public string CenterText { get; set; } = "";
        /// <summary>右側文字</summary>
        public string RightText { get; set; } = "";
        /// <summary>中間文字字型大小</summary>
        public float CenterFontSize { get; set; } = 14f;
        /// <summary>中間文字是否粗體</summary>
        public bool CenterBold { get; set; } = true;
        /// <summary>左右文字字型大小</summary>
        public float SideFontSize { get; set; } = 10f;
    }

    /// <summary>
    /// 報表標頭區塊元素（左側留空、中間多行標題、右側多行資訊）
    /// 中間和右側各自有獨立的行高
    /// </summary>
    public class ReportHeaderBlockElement : PageElement
    {
        /// <summary>中間標題行（每行的文字和字型大小）</summary>
        public List<(string Text, float FontSize, bool Bold)> CenterLines { get; set; } = new();
        /// <summary>右側資訊行</summary>
        public List<string> RightLines { get; set; } = new();
        /// <summary>右側字型大小</summary>
        public float RightFontSize { get; set; } = 10f;
    }

    /// <summary>
    /// 左右並排區塊元素（用於備註和金額並排顯示）
    /// </summary>
    public class TwoColumnSectionElement : PageElement
    {
        /// <summary>左側內容（多行文字）</summary>
        public List<string> LeftContent { get; set; } = new();
        /// <summary>右側內容（多行文字）</summary>
        public List<string> RightContent { get; set; } = new();
        /// <summary>左側寬度比例（0.0-1.0）</summary>
        public float LeftWidthRatio { get; set; } = 0.6f;
        /// <summary>左側是否顯示邊框</summary>
        public bool LeftHasBorder { get; set; } = false;
        /// <summary>左側標題</summary>
        public string? LeftTitle { get; set; }
        /// <summary>右側是否顯示邊框</summary>
        public bool RightHasBorder { get; set; } = true;
        /// <summary>右側標題</summary>
        public string? RightTitle { get; set; }
    }

    #endregion

    #region 表格相關

    /// <summary>
    /// 表格欄位定義
    /// </summary>
    public class TableColumn
    {
        /// <summary>欄位標題</summary>
        public string Header { get; set; } = "";
        /// <summary>寬度比例（相對寬度）</summary>
        public float WidthRatio { get; set; } = 1;
        /// <summary>對齊方式</summary>
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
    }

    /// <summary>
    /// 表格資料列
    /// </summary>
    public class TableRow
    {
        public List<string> Cells { get; set; } = new();
        public bool IsHeader { get; set; } = false;
    }

    /// <summary>
    /// 表格建構器
    /// </summary>
    public class TableBuilder
    {
        private readonly TableElement _table = new();

        /// <summary>
        /// 定義欄位
        /// </summary>
        public TableBuilder AddColumn(string header, float widthRatio = 1, TextAlignment alignment = TextAlignment.Left)
        {
            _table.Columns.Add(new TableColumn
            {
                Header = header,
                WidthRatio = widthRatio,
                Alignment = alignment
            });
            return this;
        }

        /// <summary>
        /// 新增資料列
        /// </summary>
        public TableBuilder AddRow(params string[] cells)
        {
            _table.Rows.Add(new TableRow { Cells = cells.ToList() });
            return this;
        }

        /// <summary>
        /// 設定是否顯示框線
        /// </summary>
        public TableBuilder ShowBorder(bool show = true)
        {
            _table.ShowBorder = show;
            return this;
        }

        /// <summary>
        /// 設定是否顯示表頭背景
        /// </summary>
        public TableBuilder ShowHeaderBackground(bool show = true)
        {
            _table.ShowHeaderBackground = show;
            return this;
        }

        /// <summary>
        /// 設定表頭欄位之間是否顯示 | 分隔符
        /// </summary>
        public TableBuilder ShowHeaderSeparator(bool show = true)
        {
            _table.ShowHeaderSeparator = show;
            return this;
        }

        /// <summary>
        /// 設定表頭下方是否顯示橫線（分隔表頭與內容）
        /// </summary>
        public TableBuilder ShowHeaderUnderline(bool show = true)
        {
            _table.ShowHeaderUnderline = show;
            return this;
        }

        /// <summary>
        /// 設定行高
        /// </summary>
        public TableBuilder SetRowHeight(float height)
        {
            _table.RowHeight = height;
            return this;
        }

        internal TableElement Build() => _table;
    }

    #endregion

    #region 列舉

    /// <summary>
    /// 線條樣式
    /// </summary>
    public enum LineStyle
    {
        /// <summary>實線</summary>
        Solid,
        /// <summary>虛線</summary>
        Dashed,
        /// <summary>點線</summary>
        Dotted,
        /// <summary>雙線</summary>
        Double
    }

    #endregion
}
