using System.Text;

namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 報表表頭建構器
    /// 用於生成標準的三欄式表頭（左側公司資訊、中間標題、右側頁次）
    /// </summary>
    public class ReportHeaderBuilder
    {
        private readonly List<string> _leftInfoRows = new();
        private string _companyName = "公司名稱";
        private string _reportTitle = "報表";
        private string _pageInfo = "";
        private string _cssClass = "print-header";

        /// <summary>
        /// 設定外層 CSS 類別名稱（預設為 'print-header'）
        /// </summary>
        public ReportHeaderBuilder SetCssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>
        /// 設定公司資訊（左側區域）
        /// </summary>
        public ReportHeaderBuilder SetCompanyInfo(string? taxId, string? phone, string? fax)
        {
            _leftInfoRows.Clear();
            
            if (!string.IsNullOrEmpty(taxId))
            {
                _leftInfoRows.Add($"<strong>統一編號：</strong>{taxId}");
            }
            
            if (!string.IsNullOrEmpty(phone))
            {
                _leftInfoRows.Add($"<strong>聯絡電話：</strong>{phone}");
            }
            
            if (!string.IsNullOrEmpty(fax))
            {
                _leftInfoRows.Add($"<strong>傳　　真：</strong>{fax}");
            }
            
            return this;
        }

        /// <summary>
        /// 新增左側資訊列（可自訂內容）
        /// </summary>
        public ReportHeaderBuilder AddLeftInfoRow(string label, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _leftInfoRows.Add($"<strong>{label}：</strong>{value}");
            }
            return this;
        }

        /// <summary>
        /// 設定報表標題（中間區域）
        /// </summary>
        public ReportHeaderBuilder SetTitle(string? companyName, string reportTitle)
        {
            _companyName = companyName ?? "公司名稱";
            _reportTitle = reportTitle;
            return this;
        }

        /// <summary>
        /// 設定頁次資訊（右側區域）
        /// </summary>
        public ReportHeaderBuilder SetPageInfo(int currentPage, int totalPages)
        {
            _pageInfo = $"第 {currentPage}/{totalPages} 頁";
            return this;
        }

        /// <summary>
        /// 設定自訂頁次資訊
        /// </summary>
        public ReportHeaderBuilder SetCustomPageInfo(string pageInfo)
        {
            _pageInfo = pageInfo;
            return this;
        }

        /// <summary>
        /// 建構 HTML
        /// </summary>
        public string Build()
        {
            var html = new StringBuilder();

            html.AppendLine($"            <div class='{_cssClass}'>");
            html.AppendLine("                <div class='print-company-header'>");

            // 左側：公司資訊
            html.AppendLine("                    <div class='print-company-left'>");
            foreach (var row in _leftInfoRows)
            {
                html.AppendLine($"                        <div class='print-info-row'>{row}</div>");
            }
            html.AppendLine("                    </div>");

            // 中間：公司名稱與報表標題
            html.AppendLine("                    <div class='print-company-center'>");
            html.AppendLine($"                        <div class='print-company-name'>{_companyName}</div>");
            html.AppendLine($"                        <div class='print-report-title'>{_reportTitle}</div>");
            html.AppendLine("                    </div>");

            // 右側：頁次
            html.AppendLine("                    <div class='print-company-right'>");
            if (!string.IsNullOrEmpty(_pageInfo))
            {
                html.AppendLine($"                        <div class='print-info-row'>{_pageInfo}</div>");
            }
            html.AppendLine("                    </div>");

            html.AppendLine("                </div>");
            html.AppendLine("            </div>");

            return html.ToString();
        }

        /// <summary>
        /// 清空所有內容（允許重複使用同一個 Builder）
        /// </summary>
        public ReportHeaderBuilder Clear()
        {
            _leftInfoRows.Clear();
            _companyName = "公司名稱";
            _reportTitle = "報表";
            _pageInfo = "";
            return this;
        }
    }
}
