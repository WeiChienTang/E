using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities.CustomTables
{
    /// <summary>
    /// 自訂欄位值 - 儲存每筆資料列中各欄位的實際值（EAV 模式）
    /// 所有值以字串儲存，顯示時根據 CustomFieldDefinition.FieldType 轉型
    /// </summary>
    public class CustomFieldValue
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 所屬資料列 ID
        /// </summary>
        [Required]
        public int CustomTableRowId { get; set; }

        /// <summary>
        /// 所屬資料列（導覽屬性）
        /// </summary>
        [ForeignKey(nameof(CustomTableRowId))]
        public CustomTableRow? CustomTableRow { get; set; }

        /// <summary>
        /// 對應的欄位定義 ID
        /// </summary>
        [Required]
        public int CustomFieldDefinitionId { get; set; }

        /// <summary>
        /// 對應的欄位定義（導覽屬性）
        /// </summary>
        [ForeignKey(nameof(CustomFieldDefinitionId))]
        public CustomFieldDefinition? CustomFieldDefinition { get; set; }

        /// <summary>
        /// 欄位值（統一以字串儲存）
        /// Number → "250"、Date → "2026-03-28"、Boolean → "true"/"false"、Select → "通過"
        /// </summary>
        [MaxLength(2000, ErrorMessage = "欄位值不可超過2000個字元")]
        public string? Value { get; set; }
    }
}
