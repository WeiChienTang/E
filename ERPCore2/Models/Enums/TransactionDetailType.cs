namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 交易明細類型枚舉 - 定義不同業務類型對庫存的影響
    /// </summary>
    public enum TransactionDetailType
    {
        /// <summary>
        /// 採購 - 增加庫存
        /// </summary>
        Purchase = 1,
        
        /// <summary>
        /// 採購退回 - 減少庫存
        /// </summary>
        PurchaseReturn = 2,
        
        /// <summary>
        /// 銷貨 - 減少庫存
        /// </summary>
        Sale = 3,
        
        /// <summary>
        /// 銷貨退回 - 增加庫存
        /// </summary>
        SaleReturn = 4,
        
        /// <summary>
        /// 庫存調整 - 可增可減
        /// </summary>
        InventoryAdjustment = 5,
        
        /// <summary>
        /// 庫存盤點 - 可增可減
        /// </summary>
        StockTaking = 6,
        
        /// <summary>
        /// 庫存轉移 - 不影響總庫存，但影響分倉庫存
        /// </summary>
        Transfer = 7
    }
}
