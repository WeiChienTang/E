namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 文件來源類型
    /// </summary>
    public enum DocumentSource
    {
        /// <summary>政府/法規</summary>
        Government = 1,
        /// <summary>廠商</summary>
        Vendor = 2,
        /// <summary>客戶</summary>
        Customer = 3,
        /// <summary>內部文件</summary>
        Internal = 4,
        /// <summary>其他</summary>
        Other = 99
    }

    /// <summary>
    /// 文件存取層級
    /// </summary>
    public enum DocumentAccessLevel
    {
        /// <summary>一般（需要 Document.Read 權限）</summary>
        Normal = 1,
        /// <summary>敏感（需要 Document.Sensitive 權限）</summary>
        Sensitive = 2
    }
}
