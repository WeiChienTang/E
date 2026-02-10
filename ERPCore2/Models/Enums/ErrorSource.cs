namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 錯誤來源枚舉
    /// </summary>
    public enum ErrorSource
    {
        /// <summary>
        /// 資料庫相關
        /// </summary>
        Database = 1,
        
        /// <summary>
        /// 業務邏輯
        /// </summary>
        BusinessLogic = 2,
        
        /// <summary>
        /// 使用者介面
        /// </summary>
        UserInterface = 3,
        
        /// <summary>
        /// 系統層級
        /// </summary>
        System = 4,
        
        /// <summary>
        /// API 相關
        /// </summary>
        API = 5,
        
        /// <summary>
        /// 安全相關
        /// </summary>
        Security = 6
    }
}
