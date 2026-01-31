using System.Text;
using ERPCore2.Helpers;

namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 報表統計區建構器
    /// 用於生成標準的統計區域（左側備註、右側金額統計）
    /// </summary>
    public class ReportSummaryBuilder
    {
        private string _remarks = string.Empty;
        private readonly List<SummaryItem> _summaryItems = new();
        private string _cssClass = "print-summary";

        /// <summary>
        /// 統計項目定義
        /// </summary>
        private class SummaryItem
        {
            public string Label { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        /// <summary>
        /// 設定外層 CSS 類別名稱（預設為 'print-summary'）
        /// </summary>
        public ReportSummaryBuilder SetCssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>
        /// 設定備註內容（左側區域）
        /// </summary>
        public ReportSummaryBuilder SetRemarks(string? remarks)
        {
            _remarks = remarks ?? string.Empty;
            return this;
        }

        /// <summary>
        /// 新增統計項目（右側區域）
        /// </summary>
        public ReportSummaryBuilder AddSummaryItem(string label, string value)
        {
            _summaryItems.Add(new SummaryItem
            {
                Label = label,
                Value = value
            });
            return this;
        }

        /// <summary>
        /// 新增金額統計項目（智能格式化：整數不顯示小數點，有小數才顯示）
        /// </summary>
        public ReportSummaryBuilder AddAmountItem(string label, decimal? amount)
        {
            var value = NumberFormatHelper.FormatSmart(amount, decimalPlaces: 2, useThousandsSeparator: true, nullDisplayText: "0");
            return AddSummaryItem(label, value);
        }

        /// <summary>
        /// 新增數量統計項目（智能格式化：整數不顯示小數點，有小數才顯示）
        /// </summary>
        public ReportSummaryBuilder AddQuantityItem(string label, decimal? quantity)
        {
            var value = NumberFormatHelper.FormatSmart(quantity, decimalPlaces: 2, useThousandsSeparator: true, nullDisplayText: "0");
            return AddSummaryItem(label, value);
        }

        /// <summary>
        /// 條件式新增統計項目
        /// </summary>
        public ReportSummaryBuilder AddSummaryItemIf(bool condition, string label, string value)
        {
            if (condition)
            {
                AddSummaryItem(label, value);
            }
            return this;
        }

        /// <summary>
        /// 建構 HTML
        /// </summary>
        public string Build()
        {
            var html = new StringBuilder();

            html.AppendLine($"            <div class='{_cssClass}'>");

            // 左側：備註區
            html.AppendLine("                <div class='print-summary-left'>");
            html.AppendLine("                    <div class='print-remarks'>");
            html.AppendLine("                        <span class='print-remarks-label'>備註：</span>");
            html.AppendLine($"                        <span class='print-remarks-content'>{_remarks}</span>");
            html.AppendLine("                    </div>");
            html.AppendLine("                </div>");

            // 右側：統計項目
            html.AppendLine("                <div class='print-summary-right'>");
            foreach (var item in _summaryItems)
            {
                html.AppendLine("                    <div class='print-summary-row'>");
                html.AppendLine($"                        <span class='print-summary-label'>{item.Label}：</span>");
                html.AppendLine($"                        <span class='print-summary-value'>{item.Value}</span>");
                html.AppendLine("                    </div>");
            }
            html.AppendLine("                </div>");

            html.AppendLine("            </div>");

            return html.ToString();
        }

        /// <summary>
        /// 清空所有內容（允許重複使用同一個 Builder）
        /// </summary>
        public ReportSummaryBuilder Clear()
        {
            _remarks = string.Empty;
            _summaryItems.Clear();
            return this;
        }
    }
}
