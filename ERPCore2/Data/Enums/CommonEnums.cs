namespace ERPCore2.Data.Enums
{
    public enum EntityStatus
    {
        Active = 1,
        Inactive = 2,
        Deleted = 3
    }
    
    /// <summary>
    /// 庫存異動類型
    /// </summary>
    public enum InventoryTransactionTypeEnum
    {
        /// <summary>
        /// 入庫
        /// </summary>
        In = 1,
        
        /// <summary>
        /// 出庫
        /// </summary>
        Out = 2,
        
        /// <summary>
        /// 調整
        /// </summary>
        Adjustment = 3,
        
        /// <summary>
        /// 盤點
        /// </summary>
        Stocktaking = 4,
        
        /// <summary>
        /// 轉倉
        /// </summary>
        Transfer = 5
    }
    
    /// <summary>
    /// 倉庫類型
    /// </summary>
    public enum WarehouseTypeEnum
    {
        /// <summary>
        /// 主倉庫
        /// </summary>
        Main = 1,
        
        /// <summary>
        /// 分倉庫
        /// </summary>
        Branch = 2,
        
        /// <summary>
        /// 虛擬倉庫
        /// </summary>
        Virtual = 3,
        
        /// <summary>
        /// 退貨倉庫
        /// </summary>
        Return = 4
    }
}
