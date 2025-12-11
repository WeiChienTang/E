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
        SetoffDocument,
        
        /// <summary>
        /// 入庫單（採購進貨）
        /// </summary>
        ReceivingDocument,
        
        /// <summary>
        /// 銷貨訂單（從報價單轉入）
        /// </summary>
        SalesOrder,
        
        /// <summary>
        /// 商品合成表（BOM 配方）
        /// </summary>
        ProductComposition,
        
        /// <summary>
        /// 銷貨單/出貨單（從銷貨訂單產生）
        /// </summary>
        DeliveryDocument,
        
        /// <summary>
        /// 生產排程（從銷貨訂單產生）
        /// </summary>
        ProductionSchedule,
        
        /// <summary>
        /// 供應商推薦（低庫存商品的供應商建議）
        /// </summary>
        SupplierRecommendation
    }    /// <summary>
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
        /// 相關數量（退貨數量/入庫數量）
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// 單價（入庫單價）
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// 相關金額（沖款金額）
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// 本次金額（本次沖款/收款/付款）
        /// </summary>
        public decimal? CurrentAmount { get; set; }

        /// <summary>
        /// 累計金額（累計沖款/收款/付款）
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// 顯示圖示
        /// </summary>
        public string Icon => DocumentType switch
        {
            RelatedDocumentType.ReturnDocument => "bi-arrow-return-left",
            RelatedDocumentType.SetoffDocument => "bi-cash-coin",
            RelatedDocumentType.ReceivingDocument => "bi-box-seam",
            RelatedDocumentType.SalesOrder => "bi-cart-check",
            RelatedDocumentType.ProductComposition => "bi-diagram-3",
            RelatedDocumentType.DeliveryDocument => "bi-truck",
            RelatedDocumentType.ProductionSchedule => "bi-calendar-check",
            RelatedDocumentType.SupplierRecommendation => "bi-shop",
            _ => "bi-file-text"
        };

        /// <summary>
        /// 顯示顏色
        /// </summary>
        public string BadgeColor => DocumentType switch
        {
            RelatedDocumentType.ReturnDocument => "warning",
            RelatedDocumentType.SetoffDocument => "success",
            RelatedDocumentType.ReceivingDocument => "info",
            RelatedDocumentType.SalesOrder => "primary",
            RelatedDocumentType.ProductComposition => "purple",
            RelatedDocumentType.DeliveryDocument => "info",
            RelatedDocumentType.ProductionSchedule => "dark",
            RelatedDocumentType.SupplierRecommendation => "success",
            _ => "secondary"
        };

        /// <summary>
        /// 類型顯示名稱
        /// </summary>
        public string TypeDisplayName => DocumentType switch
        {
            RelatedDocumentType.ReturnDocument => "退貨單",
            RelatedDocumentType.SetoffDocument => "沖款單",
            RelatedDocumentType.ReceivingDocument => "入庫單",
            RelatedDocumentType.SalesOrder => "銷貨訂單",
            RelatedDocumentType.ProductComposition => "商品合成表",
            RelatedDocumentType.DeliveryDocument => "銷貨單",
            RelatedDocumentType.ProductionSchedule => "生產排程",
            RelatedDocumentType.SupplierRecommendation => "供應商推薦",
            _ => "未知單據"
        };
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
