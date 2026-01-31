using System.Text;
using ERPCore2.Helpers;

namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 報表表格建構器
    /// 用於生成標準的明細表格（支援泛型與欄位定義）
    /// </summary>
    public class ReportTableBuilder<T> where T : class
    {
        private readonly List<TableColumn> _columns = new();
        private string _cssClass = "print-table";

        /// <summary>
        /// 表格欄位定義
        /// </summary>
        private class TableColumn
        {
            public string Header { get; set; } = string.Empty;
            public string Width { get; set; } = string.Empty;
            public Func<T, int, string> ValueGetter { get; set; } = (item, index) => string.Empty;
            public string Alignment { get; set; } = "text-left";
        }

        /// <summary>
        /// 設定表格 CSS 類別名稱（預設為 'print-table'）
        /// </summary>
        public ReportTableBuilder<T> SetCssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>
        /// 新增欄位
        /// </summary>
        /// <param name="header">欄位標題</param>
        /// <param name="width">欄位寬度（例如：'10%' 或 '100px'）</param>
        /// <param name="valueGetter">取值函式（參數：item, rowIndex）</param>
        /// <param name="alignment">對齊方式（'text-left', 'text-center', 'text-right'）</param>
        public ReportTableBuilder<T> AddColumn(
            string header, 
            string width, 
            Func<T, int, string> valueGetter, 
            string alignment = "text-left")
        {
            _columns.Add(new TableColumn
            {
                Header = header,
                Width = width,
                ValueGetter = valueGetter,
                Alignment = alignment
            });
            return this;
        }

        /// <summary>
        /// 新增序號欄位（自動產生序號）
        /// </summary>
        /// <param name="header">欄位標題（預設為"序號"）</param>
        /// <param name="width">欄位寬度</param>
        /// <param name="startIndex">起始序號（預設為 0，會自動 +1）</param>
        public ReportTableBuilder<T> AddIndexColumn(string header = "序號", string width = "5%", int startIndex = 0)
        {
            return AddColumn(header, width, (item, index) => (startIndex + index + 1).ToString(), "text-center");
        }

        /// <summary>
        /// 新增文字欄位
        /// </summary>
        public ReportTableBuilder<T> AddTextColumn(string header, string width, Func<T, string> valueGetter, string alignment = "text-left")
        {
            return AddColumn(header, width, (item, index) => valueGetter(item), alignment);
        }

        /// <summary>
        /// 新增數量欄位（智能格式化：整數不顯示小數點，有小數才顯示）
        /// </summary>
        public ReportTableBuilder<T> AddQuantityColumn(string header, string width, Func<T, decimal?> valueGetter)
        {
            return AddColumn(header, width, (item, index) => NumberFormatHelper.FormatSmart(valueGetter(item), decimalPlaces: 2, useThousandsSeparator: true, nullDisplayText: "0"), "text-right");
        }

        /// <summary>
        /// 新增金額欄位（智能格式化：整數不顯示小數點，有小數才顯示）
        /// </summary>
        public ReportTableBuilder<T> AddAmountColumn(string header, string width, Func<T, decimal?> valueGetter)
        {
            return AddColumn(header, width, (item, index) => NumberFormatHelper.FormatSmart(valueGetter(item), decimalPlaces: 2, useThousandsSeparator: true, nullDisplayText: "0"), "text-right");
        }

        /// <summary>
        /// 新增日期欄位（自動格式化為 yyyy/MM/dd）
        /// </summary>
        public ReportTableBuilder<T> AddDateColumn(string header, string width, Func<T, DateTime?> valueGetter)
        {
            return AddColumn(header, width, (item, index) => valueGetter(item)?.ToString("yyyy/MM/dd") ?? "", "text-center");
        }

        /// <summary>
        /// 建構 HTML
        /// </summary>
        /// <param name="items">資料項目清單</param>
        /// <param name="startRowNum">起始列號（用於序號欄位）</param>
        public string Build(List<T> items, int startRowNum = 0)
        {
            var html = new StringBuilder();

            html.AppendLine($"            <table class='{_cssClass}'>");

            // 表頭
            html.AppendLine("                <thead>");
            html.AppendLine("                    <tr>");

            foreach (var column in _columns)
            {
                var widthAttr = !string.IsNullOrEmpty(column.Width) ? $" style='width: {column.Width}'" : "";
                html.AppendLine($"                        <th{widthAttr}>{column.Header}</th>");
            }

            html.AppendLine("                    </tr>");
            html.AppendLine("                </thead>");

            // 表身
            html.AppendLine("                <tbody>");

            if (items != null && items.Any())
            {
                int rowIndex = 0;
                foreach (var item in items)
                {
                    html.AppendLine("                    <tr>");

                    foreach (var column in _columns)
                    {
                        var value = column.ValueGetter(item, rowIndex);
                        html.AppendLine($"                        <td class='{column.Alignment}'>{value}</td>");
                    }

                    html.AppendLine("                    </tr>");
                    rowIndex++;
                }
            }

            html.AppendLine("                </tbody>");
            html.AppendLine("            </table>");

            return html.ToString();
        }

        /// <summary>
        /// 清空所有欄位定義（允許重複使用同一個 Builder）
        /// </summary>
        public ReportTableBuilder<T> Clear()
        {
            _columns.Clear();
            return this;
        }
    }
}
