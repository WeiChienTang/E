namespace ERPCore2.Models.Enums
{
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
