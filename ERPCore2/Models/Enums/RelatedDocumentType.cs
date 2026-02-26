namespace ERPCore2.Models.Enums
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
        /// 商品物料清單（BOM 配方）
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
        SupplierRecommendation,
        
        /// <summary>
        /// 庫存異動記錄（原始交易 + 調整記錄）
        /// </summary>
        InventoryTransaction,

        /// <summary>
        /// 報價單（銷貨報價）
        /// </summary>
        Quotation
    }
}
