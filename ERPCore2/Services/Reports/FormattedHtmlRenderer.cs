using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using System.Text;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 格式化文件 HTML 渲染器
    /// 將 FormattedDocument 轉換為可列印的 HTML，由瀏覽器原生處理分頁與 DPI，
    /// 解決點陣圖渲染造成的紙張尺寸不符問題。
    /// </summary>
    public static class FormattedHtmlRenderer
    {
        /// <summary>單一文件渲染</summary>
        public static string RenderToHtml(FormattedDocument document, PaperSetting? paper = null)
            => RenderBatchToHtml(new[] { document }, paper);

        /// <summary>批次文件渲染（每份文件占一頁）</summary>
        public static string RenderBatchToHtml(IEnumerable<FormattedDocument> documents, PaperSetting? paper = null)
        {
            var docs = documents.ToList();
            if (!docs.Any()) return "";

            GetPaperDims(paper, docs[0].PageSettings,
                out float wCm, out float hCm,
                out float mTop, out float mRight, out float mBottom, out float mLeft);

            var sb = new StringBuilder();

            // @page size 只允許 <length> <length>，不能附加 landscape/portrait（那是給預定義紙張用的）
            // 寬度大於高度時瀏覽器自動以橫向排版
            sb.Append($@"<!DOCTYPE html><html><head><meta charset=""utf-8""><style>
@page {{
    size: {wCm:F2}cm {hCm:F2}cm;
    margin: {mTop:F2}cm {mRight:F2}cm {mBottom:F2}cm {mLeft:F2}cm;
}}
{BaseStyles()}
</style></head><body>");

            for (int i = 0; i < docs.Count; i++)
                sb.Append(RenderDoc(docs[i], i + 1, docs.Count, i == docs.Count - 1));

            sb.Append("</body></html>");
            return sb.ToString();
        }

        // ===== 紙張尺寸 =====

        private static void GetPaperDims(
            PaperSetting? paper, DocumentPageSettings settings,
            out float w, out float h,
            out float mTop, out float mRight, out float mBottom, out float mLeft)
        {
            if (paper != null)
            {
                // Width 直接當作頁面寬度，Height 直接當作頁面高度
                // 使用者在表單輸入的就是實際列印方向的尺寸，不需要 Orientation 換算
                w = (float)paper.Width;
                h = (float)paper.Height;
                mTop    = (float)(paper.TopMargin    ?? 0.3m);
                mRight  = (float)(paper.RightMargin  ?? 0.8m);
                mBottom = (float)(paper.BottomMargin ?? 0.3m);
                mLeft   = (float)(paper.LeftMargin   ?? 0.8m);
            }
            else
            {
                w      = settings.PageWidth;
                h      = settings.PageHeight;
                mTop   = settings.TopMargin;
                mRight = settings.RightMargin;
                mBottom = settings.BottomMargin;
                mLeft  = settings.LeftMargin;
            }
        }

        // ===== 文件渲染 =====

        private static string RenderDoc(FormattedDocument doc, int docIdx, int totalDocs, bool isLast)
        {
            var sb = new StringBuilder();
            // 使用 table 包裝：thead/tfoot 帶有 display:table-header/footer-group，
            // 確保文件抬頭與頁尾在每個列印頁面自動重複，解決內容溢出時抬頭消失的問題。
            var breakCls = isLast ? " last-page" : "";
            sb.Append($"<table class=\"page-wrap{breakCls}\">");

            // 抬頭（每頁自動重複）
            sb.Append("<thead><tr><td>");
            foreach (var el in doc.HeaderElements)
                sb.Append(RenderElement(el, docIdx, totalDocs));
            sb.Append("</td></tr></thead>");

            // 頁尾（每頁自動重複）
            if (doc.FooterElements.Any())
            {
                sb.Append("<tfoot><tr><td>");
                foreach (var el in doc.FooterElements)
                    sb.Append(RenderElement(el, docIdx, totalDocs));
                sb.Append("</td></tr></tfoot>");
            }

            // 主體內容
            sb.Append("<tbody><tr><td>");
            foreach (var el in doc.Elements)
            {
                if (el is PageBreakElement)
                {
                    // 明確換頁（抬頭已由 thead 自動重複，只需分隔主體）
                    sb.Append("</td></tr><tr class=\"page-break-row\"><td>");
                }
                else
                {
                    sb.Append(RenderElement(el, docIdx, totalDocs));
                }
            }
            sb.Append("</td></tr></tbody>");

            sb.Append("</table>");
            return sb.ToString();
        }

        // ===== 元素渲染分派 =====

        private static string RenderElement(PageElement el, int pageNum, int totalPages) => el switch
        {
            ReportHeaderBlockElement rh => RenderReportHeaderBlock(rh, pageNum, totalPages),
            KeyValueRowElement kv       => RenderKeyValueRow(kv),
            SpacingElement sp           => $"<div style=\"height:{sp.Height}px\"></div>",
            LineElement ln              => RenderLine(ln),
            TextElement tx              => RenderText(tx, pageNum, totalPages),
            TableElement tb             => RenderTable(tb),
            TwoColumnSectionElement tc  => RenderTwoColumn(tc),
            SignatureSectionElement sg  => RenderSignature(sg),
            ImageElement img            => RenderImage(img),
            _                           => ""
        };

        // ===== 各元素渲染 =====

        private static string RenderReportHeaderBlock(ReportHeaderBlockElement el, int pageNum, int totalPages)
        {
            var sb = new StringBuilder();
            sb.Append("<div class=\"rpt-header\">");
            sb.Append("<div class=\"rpt-header-left\"></div>");

            sb.Append("<div class=\"rpt-header-center\">");
            foreach (var (text, fontSize, bold) in el.CenterLines)
            {
                var w = bold ? "font-weight:bold;" : "";
                sb.Append($"<div style=\"font-size:{fontSize}pt;{w}\">{Enc(text)}</div>");
            }
            sb.Append("</div>");

            sb.Append("<div class=\"rpt-header-right\">");
            foreach (var line in el.RightLines)
                sb.Append($"<div style=\"font-size:{el.RightFontSize}pt\">{Enc(ReplacePn(line, pageNum, totalPages))}</div>");
            sb.Append("</div>");

            sb.Append("</div>");
            return sb.ToString();
        }

        private static string RenderKeyValueRow(KeyValueRowElement el)
        {
            // 全部 pair 都為空時不渲染
            if (!el.Pairs.Any(p => !string.IsNullOrEmpty(p.Key) || !string.IsNullOrEmpty(p.Value)))
                return "";

            var pairCount = el.Pairs.Count;
            // 每個 pair 均分總寬度，pair 內 key 佔 30%、val 佔 70%
            var pairPct = 100f / pairCount;
            var keyPct = pairPct * 0.3f;
            var valPct = pairPct * 0.7f;

            var sb = new StringBuilder();
            sb.Append("<table class=\"kv-row\">");

            // colgroup 確保跨行對齊
            sb.Append("<colgroup>");
            for (int i = 0; i < pairCount; i++)
            {
                sb.Append($"<col style=\"width:{keyPct:F1}%\">");
                sb.Append($"<col style=\"width:{valPct:F1}%\">");
            }
            sb.Append("</colgroup>");

            sb.Append("<tr>");
            foreach (var (key, value) in el.Pairs)
            {
                if (string.IsNullOrEmpty(key) && string.IsNullOrEmpty(value))
                {
                    // 空 pair：保留欄位結構（維持對齊），不顯示內容
                    sb.Append("<td class=\"kv-key\"></td>");
                    sb.Append("<td class=\"kv-val\"></td>");
                }
                else if (string.IsNullOrEmpty(key))
                {
                    // key 為空時不顯示冒號，直接顯示 value
                    sb.Append("<td class=\"kv-key\"></td>");
                    sb.Append($"<td class=\"kv-val\">{Enc(value)}</td>");
                }
                else
                {
                    sb.Append($"<td class=\"kv-key\">{Enc(key)}：</td>");
                    sb.Append($"<td class=\"kv-val\">{Enc(value)}</td>");
                }
            }
            sb.Append("</tr></table>");
            return sb.ToString();
        }

        private static string RenderLine(LineElement el)
        {
            var style = el.Style switch
            {
                LineStyle.Dashed => "dashed",
                LineStyle.Dotted => "dotted",
                LineStyle.Double => "double",
                _ => "solid"
            };
            return $"<div style=\"border-bottom:{el.Thickness}px {style} #000;margin:1px 0\"></div>";
        }

        private static string RenderText(TextElement el, int pageNum, int totalPages)
        {
            var align = el.Alignment switch
            {
                TextAlignment.Center => "center",
                TextAlignment.Right  => "right",
                _ => "left"
            };
            var bold   = el.Bold   ? "font-weight:bold;"   : "";
            var italic = el.Italic ? "font-style:italic;"  : "";
            var text   = Enc(ReplacePn(el.Text, pageNum, totalPages));
            return $"<div style=\"text-align:{align};font-size:{el.FontSize}pt;{bold}{italic}\">{text}</div>";
        }

        private static string RenderTable(TableElement el)
        {
            if (!el.Columns.Any()) return "";

            var totalRatio   = el.Columns.Sum(c => c.WidthRatio);
            var borderClass  = el.ShowBorder ? "with-border" : "no-border";
            var sb = new StringBuilder();

            sb.Append($"<table class=\"main-table {borderClass}\">");

            // 欄寬
            sb.Append("<colgroup>");
            foreach (var col in el.Columns)
                sb.Append($"<col style=\"width:{col.WidthRatio / totalRatio * 100f:F1}%\">");
            sb.Append("</colgroup>");

            // 表頭
            sb.Append("<thead><tr>");
            foreach (var col in el.Columns)
            {
                var align = AlignClass(col.Alignment);
                var bgStyle = el.ShowHeaderBackground ? "background-color:#e8e8e8;" : "";
                sb.Append($"<th class=\"{align}\" style=\"{bgStyle}\">{Enc(col.Header)}</th>");
            }
            sb.Append("</tr></thead>");

            // 資料列
            sb.Append("<tbody>");
            foreach (var row in el.Rows)
            {
                sb.Append($"<tr style=\"height:{el.RowHeight}px\">");
                for (int i = 0; i < el.Columns.Count; i++)
                {
                    var cell  = i < row.Cells.Count ? row.Cells[i] : "";
                    var align = AlignClass(el.Columns[i].Alignment);
                    sb.Append($"<td class=\"{align}\">{Enc(cell)}</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        private static string RenderTwoColumn(TwoColumnSectionElement el)
        {
            var lPct = el.LeftWidthRatio * 100f;
            var rPct = (1f - el.LeftWidthRatio) * 100f;
            var lBorder = el.LeftHasBorder  ? "border:1px solid #000;padding:2px 4px;" : "";
            var rBorder = el.RightHasBorder ? "border:1px solid #000;padding:2px 4px;" : "padding:2px 4px;";

            var sb = new StringBuilder();
            sb.Append("<div class=\"two-col\">");

            sb.Append($"<div class=\"two-col-left\" style=\"width:{lPct:F0}%;{lBorder}\">");
            if (el.LeftTitle != null) sb.Append($"<div><strong>{Enc(el.LeftTitle)}</strong></div>");
            foreach (var line in el.LeftContent)
                sb.Append($"<div>{Enc(line)}</div>");
            sb.Append("</div>");

            sb.Append($"<div class=\"two-col-right\" style=\"width:{rPct:F0}%;{rBorder}\">");
            if (el.RightTitle != null) sb.Append($"<div><strong>{Enc(el.RightTitle)}</strong></div>");
            foreach (var line in el.RightContent)
                sb.Append($"<div>{Enc(line)}</div>");
            sb.Append("</div>");

            sb.Append("</div>");
            return sb.ToString();
        }

        private static string RenderSignature(SignatureSectionElement el)
        {
            if (!el.Labels.Any()) return "";
            var pct = 100f / el.Labels.Count;
            var sb = new StringBuilder();
            sb.Append("<div class=\"signatures\">");
            foreach (var label in el.Labels)
            {
                sb.Append($"<div class=\"sig-item\" style=\"width:{pct:F0}%\">");
                sb.Append("<div class=\"sig-line\"></div>");
                sb.Append($"<div>{Enc(label)}</div>");
                sb.Append("</div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        private static string RenderImage(ImageElement el)
        {
            if (el.ImageData == null || el.ImageData.Length == 0) return "";
            var b64  = Convert.ToBase64String(el.ImageData);
            var disp = el.Alignment switch
            {
                TextAlignment.Center => "display:block;margin:0 auto;",
                TextAlignment.Right  => "display:block;margin-left:auto;",
                _ => "display:block;"
            };
            var w = el.Width.HasValue  ? $"width:{el.Width.Value}cm;"   : "max-width:100%;";
            var h = el.Height.HasValue ? $"height:{el.Height.Value}cm;" : "";
            return $"<img src=\"data:image/png;base64,{b64}\" style=\"{disp}{w}{h}\">";
        }

        // ===== 輔助 =====

        private static string ReplacePn(string text, int pageNum, int totalPages)
            => text.Replace("{PAGE}", pageNum.ToString()).Replace("{PAGES}", totalPages.ToString());

        private static string AlignClass(TextAlignment a) => a switch
        {
            TextAlignment.Center => "align-center",
            TextAlignment.Right  => "align-right",
            _ => "align-left"
        };

        private static string Enc(string? text)
            => System.Net.WebUtility.HtmlEncode(text ?? "");

        // ===== CSS =====

        private static string BaseStyles() => @"
* { box-sizing: border-box; margin: 0; padding: 0; }
body { font-family: 'Microsoft JhengHei', '微軟正黑體', 'Noto Sans TC', sans-serif; font-size: 10pt; }

/* 頁面容器（使用 table 包裝確保 thead/tfoot 在每個列印頁面自動重複） */
.page-wrap { width: 100%; border-collapse: collapse; page-break-after: always; }
.page-wrap.last-page { page-break-after: auto; }
.page-wrap > thead { display: table-header-group; }
.page-wrap > tfoot { display: table-footer-group; }
.page-wrap > thead > tr > td,
.page-wrap > tfoot > tr > td,
.page-wrap > tbody > tr > td { padding: 0; border: none; }
/* 明確換頁列（PageBreakElement） */
tr.page-break-row { page-break-before: always; }

/* 報表標頭區塊 */
.rpt-header { display: table; width: 100%; table-layout: fixed; }
.rpt-header-left   { display: table-cell; width: 20%; vertical-align: middle; }
.rpt-header-center { display: table-cell; width: 60%; text-align: center; vertical-align: middle; line-height: 1.4; }
.rpt-header-right  { display: table-cell; width: 20%; text-align: right; vertical-align: top; white-space: nowrap; }

/* 鍵值對行 */
.kv-row { width: 100%; border-collapse: collapse; table-layout: fixed; }
.kv-row td { font-size: 9pt; padding: 0 3px; overflow: hidden; text-overflow: ellipsis; }
.kv-key { font-weight: bold; white-space: nowrap; }
.kv-val { }

/* 主表格 */
.main-table { width: 100%; border-collapse: collapse; }
.main-table.with-border { border: 1px solid #000; }
.main-table.with-border th,
.main-table.with-border td { border: 1px solid #000; }
.main-table th { font-size: 9pt; padding: 1px 4px; background-color: #e8e8e8; }
.main-table td { font-size: 9pt; padding: 1px 4px; }
.main-table thead tr { border-bottom: 1.5px solid #000; }
.align-left   { text-align: left; }
.align-center { text-align: center; }
.align-right  { text-align: right; }

/* 左右並排 */
.two-col       { display: table; width: 100%; }
.two-col-left  { display: table-cell; vertical-align: top; font-size: 9pt; }
.two-col-right { display: table-cell; vertical-align: top; text-align: right; font-size: 9pt; }

/* 簽名區 */
.signatures { display: table; width: 100%; margin-top: 4px; }
.sig-item   { display: table-cell; text-align: center; font-size: 9pt; padding: 0 8px; }
.sig-line   { border-bottom: 1px solid #000; height: 20px; margin: 0 10%; }
";
    }
}
