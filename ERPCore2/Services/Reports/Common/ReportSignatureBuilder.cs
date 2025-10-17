using System.Text;

namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 報表簽名區建構器
    /// 用於生成標準的簽名區域（多個簽名欄位並排顯示）
    /// </summary>
    public class ReportSignatureBuilder
    {
        private readonly List<string> _signatureLabels = new();
        private string _cssClass = "print-signature-section";

        /// <summary>
        /// 設定外層 CSS 類別名稱（預設為 'print-signature-section'）
        /// </summary>
        public ReportSignatureBuilder SetCssClass(string cssClass)
        {
            _cssClass = cssClass;
            return this;
        }

        /// <summary>
        /// 新增簽名欄位
        /// </summary>
        /// <param name="label">簽名欄位標籤（例如：「採購人員」、「核准人員」）</param>
        public ReportSignatureBuilder AddSignature(string label)
        {
            _signatureLabels.Add(label);
            return this;
        }

        /// <summary>
        /// 批次新增多個簽名欄位
        /// </summary>
        public ReportSignatureBuilder AddSignatures(params string[] labels)
        {
            _signatureLabels.AddRange(labels);
            return this;
        }

        /// <summary>
        /// 條件式新增簽名欄位
        /// </summary>
        public ReportSignatureBuilder AddSignatureIf(bool condition, string label)
        {
            if (condition)
            {
                AddSignature(label);
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

            foreach (var label in _signatureLabels)
            {
                html.AppendLine("                <div class='print-signature-item'>");
                html.AppendLine($"                    <div class='print-signature-label'>{label}</div>");
                html.AppendLine("                    <div class='print-signature-line'></div>");
                html.AppendLine("                </div>");
            }

            html.AppendLine("            </div>");

            return html.ToString();
        }

        /// <summary>
        /// 清空所有簽名欄位（允許重複使用同一個 Builder）
        /// </summary>
        public ReportSignatureBuilder Clear()
        {
            _signatureLabels.Clear();
            return this;
        }
    }
}
