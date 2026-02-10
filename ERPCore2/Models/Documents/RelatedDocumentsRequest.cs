namespace ERPCore2.Models.Documents
{
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
