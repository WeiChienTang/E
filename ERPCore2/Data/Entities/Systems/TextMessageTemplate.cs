using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 文字訊息範本實體 - 儲存各類單據的訊息複製範本
    /// 可套用於採購單、銷貨單、報價單等單據的訊息複製功能
    /// </summary>
    public class TextMessageTemplate : BaseEntity
    {
        /// <summary>
        /// 範本代碼（如：PurchaseOrder, SalesOrder, Quotation）
        /// 用於識別此範本屬於哪種單據類型
        /// </summary>
        [Display(Name = "範本代碼")]
        [Required(ErrorMessage = "請設定範本代碼")]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        /// <summary>
        /// 範本名稱（如：採購單訊息範本）
        /// </summary>
        [Display(Name = "範本名稱")]
        [Required(ErrorMessage = "請設定範本名稱")]
        [MaxLength(100)]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// 第一段文字（問候語）
        /// 支援變數：{supplierName}, {customerName}, {orderCode}, {orderDate}, {companyName}
        /// </summary>
        [Display(Name = "問候語")]
        [MaxLength(1000)]
        public string HeaderText { get; set; } = string.Empty;

        /// <summary>
        /// 第三段文字（結語）
        /// 支援變數：{supplierName}, {customerName}, {orderCode}, {orderDate}, {companyName}
        /// </summary>
        [Display(Name = "結語")]
        [MaxLength(1000)]
        public string FooterText { get; set; } = string.Empty;

        /// <summary>
        /// 明細格式設定（JSON 格式）
        /// 儲存 DetailFormatConfig 的序列化資料
        /// </summary>
        [Display(Name = "明細格式設定")]
        public string DetailFormatJson { get; set; } = "{}";

        /// <summary>
        /// 是否啟用此範本
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序順序
        /// </summary>
        [Display(Name = "排序")]
        public int SortOrder { get; set; } = 0;
    }
}
