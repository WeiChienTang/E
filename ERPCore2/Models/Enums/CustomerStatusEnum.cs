namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 客戶狀態
    /// </summary>
    public enum CustomerStatus
    {
        /// <summary>正常往來</summary>
        Active = 1,
        /// <summary>停用</summary>
        Inactive = 2,
        /// <summary>黑名單</summary>
        Blacklisted = 3
    }
}
