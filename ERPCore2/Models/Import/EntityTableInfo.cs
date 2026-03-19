namespace ERPCore2.Models.Import
{
    /// <summary>
    /// AppDbContext 中 DbSet 資訊 DTO
    /// </summary>
    public class EntityTableInfo
    {
        /// <summary>DbSet 屬性名稱（如 "Customers"、"Items"）</summary>
        public string DbSetName { get; set; } = string.Empty;

        /// <summary>Entity 型別完整名稱（如 "ERPCore2.Data.Entities.Customer"）</summary>
        public string EntityTypeName { get; set; } = string.Empty;

        /// <summary>Entity 型別短名（如 "Customer"）</summary>
        public string EntityShortName { get; set; } = string.Empty;

        /// <summary>顯示名稱（DbSetName + EntityShortName，方便選取）</summary>
        public string DisplayName => $"{DbSetName} ({EntityShortName})";
    }
}
