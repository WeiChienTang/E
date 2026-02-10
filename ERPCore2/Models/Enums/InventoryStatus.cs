namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 庫存狀態枚舉
    /// </summary>
    public enum InventoryStatus
    {
        /// <summary>
        /// 充足 - 庫存充足
        /// </summary>
        Sufficient = 0,

        /// <summary>
        /// 警戒 - 庫存低於安全庫存但仍足夠
        /// </summary>
        Warning = 1,

        /// <summary>
        /// 不足 - 庫存不足以滿足需求
        /// </summary>
        Insufficient = 2
    }
}
