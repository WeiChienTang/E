namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 客戶類型
    /// </summary>
    public enum CustomerType
    {
        /// <summary>企業</summary>
        Enterprise = 1,
        /// <summary>個人</summary>
        Individual = 2,
        /// <summary>政府機關</summary>
        Government = 3
    }

    /// <summary>
    /// 客戶來源
    /// </summary>
    public enum CustomerSource
    {
        /// <summary>業務開發</summary>
        BusinessDevelopment = 1,
        /// <summary>客戶推薦</summary>
        Referral = 2,
        /// <summary>展覽/展示會</summary>
        Exhibition = 3,
        /// <summary>網路/社群</summary>
        Internet = 4,
        /// <summary>其他</summary>
        Other = 5
    }

    /// <summary>
    /// 信用評等
    /// </summary>
    public enum CreditRating
    {
        /// <summary>優良 (A)</summary>
        A = 1,
        /// <summary>良好 (B)</summary>
        B = 2,
        /// <summary>普通 (C)</summary>
        C = 3,
        /// <summary>注意 (D)</summary>
        D = 4
    }

    /// <summary>
    /// 廠商類型
    /// </summary>
    public enum SupplierType
    {
        /// <summary>製造商</summary>
        Manufacturer = 1,
        /// <summary>貿易商</summary>
        Trader = 2,
        /// <summary>代理商</summary>
        Agent = 3,
        /// <summary>服務商</summary>
        ServiceProvider = 4
    }

    /// <summary>
    /// 廠商狀態
    /// </summary>
    public enum SupplierStatus
    {
        /// <summary>正常往來</summary>
        Active = 1,
        /// <summary>停用</summary>
        Inactive = 2,
        /// <summary>暫停往來</summary>
        Suspended = 3
    }

    /// <summary>
    /// 公家機關類型
    /// </summary>
    public enum GovernmentAgencyType
    {
        /// <summary>中央機關</summary>
        Central = 1,
        /// <summary>直轄市機關</summary>
        Municipal = 2,
        /// <summary>縣市機關</summary>
        County = 3,
        /// <summary>鄉鎮市區機關</summary>
        Township = 4,
        /// <summary>其他</summary>
        Other = 5
    }

    /// <summary>
    /// 公家機關狀態
    /// </summary>
    public enum GovernmentAgencyStatus
    {
        /// <summary>正常往來</summary>
        Active = 1,
        /// <summary>停用</summary>
        Inactive = 2
    }
}
