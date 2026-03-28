using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities.CustomTables
{
    /// <summary>
    /// 自訂資料表的資料列 - 繼承 BaseEntity，擁有 Id、Code、Status、審計欄位、草稿支援
    /// </summary>
    public class CustomTableRow : BaseEntity
    {
        /// <summary>
        /// 所屬自訂資料表 ID
        /// </summary>
        [Required]
        [Display(Name = "所屬資料表")]
        public int CustomTableDefinitionId { get; set; }

        /// <summary>
        /// 所屬自訂資料表（導覽屬性）
        /// </summary>
        [ForeignKey(nameof(CustomTableDefinitionId))]
        public CustomTableDefinition? CustomTableDefinition { get; set; }

        /// <summary>
        /// 欄位值集合（導覽屬性）
        /// </summary>
        public List<CustomFieldValue> FieldValues { get; set; } = new();
    }
}
