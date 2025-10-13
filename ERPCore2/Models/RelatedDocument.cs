namespace ERPCore2.Models
{
    /// <summary>
    /// 相關單據類型
    /// </summary>
    public enum RelatedDocumentType
    {
        /// <summary>
        /// 退貨單（採購退貨或銷貨退回）
        /// </summary>
        ReturnDocument,
        
        /// <summary>
        /// 沖款單（應付帳款或應收帳款）
        /// </summary>
        SetoffDocument
    }

    /// <summary>
    /// 相關單據資訊 - 用於顯示與明細項目相關的單據列表
    /// </summary>
    public class RelatedDocument
    {
        /// <summary>
        /// 單據 ID
        /// </summary>
        public int DocumentId { get; set; }

        /// <summary>
        /// 單據類型
        /// </summary>
        public RelatedDocumentType DocumentType { get; set; }

        /// <summary>
        /// 單據編號
        /// </summary>
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// 單據日期
        /// </summary>
        public DateTime DocumentDate { get; set; }

        /// <summary>
        /// 相關數量（退貨數量）
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// 相關金額（沖款金額）
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 顯示圖示
        /// </summary>
        public string Icon => DocumentType == RelatedDocumentType.ReturnDocument 
            ? "bi-arrow-return-left" 
            : "bi-cash-coin";

        /// <summary>
        /// 顯示顏色
        /// </summary>
        public string BadgeColor => DocumentType == RelatedDocumentType.ReturnDocument 
            ? "warning" 
            : "success";

        /// <summary>
        /// 類型顯示名稱
        /// </summary>
        public string TypeDisplayName => DocumentType == RelatedDocumentType.ReturnDocument 
            ? "退貨單" 
            : "沖款單";
    }

    /// <summary>
    /// 相關單據查詢請求
    /// </summary>
    public class RelatedDocumentsRequest
    {
        /// <summary>
        /// 來源明細類型（PurchaseReceivingDetail, SalesOrderDetail 等）
        /// </summary>
        public string SourceDetailType { get; set; } = string.Empty;

        /// <summary>
        /// 來源明細 ID
        /// </summary>
        public int SourceDetailId { get; set; }

        /// <summary>
        /// 商品名稱（用於顯示標題）
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
    }
}
