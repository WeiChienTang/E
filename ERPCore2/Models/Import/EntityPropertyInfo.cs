namespace ERPCore2.Models.Import
{
    /// <summary>
    /// Entity 屬性資訊 DTO（用於 Step 3 欄位對應表）
    /// </summary>
    public class EntityPropertyInfo
    {
        /// <summary>C# 屬性名稱（如 "Name"、"Price"）</summary>
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>C# 型別的顯示名稱（如 "string"、"int"、"decimal?"）</summary>
        public string TypeDisplayName { get; set; } = string.Empty;

        /// <summary>C# 型別的完整 Type 物件</summary>
        public Type PropertyType { get; set; } = typeof(object);

        /// <summary>是否為必填欄位（不可 Null 且非 BaseEntity 自動處理）</summary>
        public bool IsRequired { get; set; }

        /// <summary>是否為可空型別</summary>
        public bool IsNullable { get; set; }

        /// <summary>是否為 Enum 型別</summary>
        public bool IsEnum { get; set; }

        /// <summary>若為 Enum，列出所有可選值的名稱</summary>
        public List<string> EnumValues { get; set; } = new();

        /// <summary>MaxLength 限制（若有 [MaxLength] Attribute）</summary>
        public int? MaxLength { get; set; }

        /// <summary>是否看起來像 FK 欄位（名稱以 Id 結尾且為 int 型別）</summary>
        public bool IsForeignKeyLike { get; set; }
    }
}
