using System.Text;

namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 報表資訊區塊建構器
    /// 用於生成標準的 Grid 佈局資訊區塊（標籤 + 值）
    /// </summary>
    public class ReportInfoSectionBuilder
    {
        private readonly List<InfoField> _fields = new();
        private string _cssClass = "print-info-section";

        /// <summary>
        /// 資訊欄位定義
        /// </summary>
        private class InfoField
        {
            public string Label { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public int ColumnSpan { get; set; } = 1;
            public string? CustomStyle { get; set; }
        }

        /// <summary>
        /// 設定外層 CSS 類別名稱（預設為 'print-info-section'）
        /// </summary>
        public ReportInfoSectionBuilder SetCssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>
        /// 新增一個資訊欄位
        /// </summary>
        /// <param name="label">標籤文字</param>
        /// <param name="value">值文字</param>
        /// <param name="columnSpan">跨欄數（預設為 1）</param>
        /// <param name="customStyle">自訂樣式</param>
        public ReportInfoSectionBuilder AddField(string label, string? value, int columnSpan = 1, string? customStyle = null)
        {
            _fields.Add(new InfoField
            {
                Label = label,
                Value = value ?? string.Empty,
                ColumnSpan = columnSpan,
                CustomStyle = customStyle
            });
            return this;
        }

        /// <summary>
        /// 條件式新增欄位（只有條件為 true 時才新增）
        /// </summary>
        public ReportInfoSectionBuilder AddFieldIf(bool condition, string label, string? value, int columnSpan = 1, string? customStyle = null)
        {
            if (condition)
            {
                AddField(label, value, columnSpan, customStyle);
            }
            return this;
        }

        /// <summary>
        /// 新增日期欄位（自動格式化為 yyyy/MM/dd）
        /// </summary>
        public ReportInfoSectionBuilder AddDateField(string label, DateTime? date, int columnSpan = 1)
        {
            var value = date?.ToString("yyyy/MM/dd") ?? string.Empty;
            return AddField(label, value, columnSpan);
        }

        /// <summary>
        /// 新增日期時間欄位（自動格式化為 yyyy/MM/dd HH:mm）
        /// </summary>
        public ReportInfoSectionBuilder AddDateTimeField(string label, DateTime? dateTime, int columnSpan = 1)
        {
            var value = dateTime?.ToString("yyyy/MM/dd HH:mm") ?? string.Empty;
            return AddField(label, value, columnSpan);
        }

        /// <summary>
        /// 新增金額欄位（自動格式化為千分位小數）
        /// </summary>
        public ReportInfoSectionBuilder AddAmountField(string label, decimal? amount, int columnSpan = 1)
        {
            var value = amount?.ToString("N2") ?? "0.00";
            return AddField(label, value, columnSpan);
        }

        /// <summary>
        /// 新增數量欄位（自動格式化為千分位整數）
        /// </summary>
        public ReportInfoSectionBuilder AddQuantityField(string label, decimal? quantity, int columnSpan = 1)
        {
            var value = quantity?.ToString("N0") ?? "0";
            return AddField(label, value, columnSpan);
        }

        /// <summary>
        /// 建構 HTML
        /// </summary>
        public string Build()
        {
            var html = new StringBuilder();

            html.AppendLine($"            <div class='{_cssClass}'>");
            html.AppendLine("                <div class='print-info-grid'>");

            foreach (var field in _fields)
            {
                var styleAttr = string.Empty;
                var styles = new List<string>();

                // 處理跨欄
                if (field.ColumnSpan > 1)
                {
                    styles.Add($"grid-column: span {field.ColumnSpan}");
                }

                // 處理自訂樣式
                if (!string.IsNullOrEmpty(field.CustomStyle))
                {
                    styles.Add(field.CustomStyle);
                }

                if (styles.Any())
                {
                    styleAttr = $" style='{string.Join("; ", styles)};'";
                }

                html.AppendLine($"                    <div class='print-info-item'{styleAttr}>");
                html.AppendLine($"                        <span class='print-info-label'>{field.Label}：</span>");
                html.AppendLine($"                        <span class='print-info-value'>{field.Value}</span>");
                html.AppendLine("                    </div>");
            }

            html.AppendLine("                </div>");
            html.AppendLine("            </div>");

            return html.ToString();
        }

        /// <summary>
        /// 清空所有欄位（允許重複使用同一個 Builder）
        /// </summary>
        public ReportInfoSectionBuilder Clear()
        {
            _fields.Clear();
            return this;
        }
    }
}
