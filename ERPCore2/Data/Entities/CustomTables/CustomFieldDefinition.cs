using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities.CustomTables
{
    /// <summary>
    /// 自訂欄位定義 - 定義自訂資料表中的欄位結構
    /// </summary>
    public class CustomFieldDefinition : BaseEntity
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
        /// 欄位名稱（內部識別用，如 "TestTemp"）
        /// </summary>
        [Required(ErrorMessage = "欄位名稱為必填")]
        [MaxLength(100, ErrorMessage = "欄位名稱不可超過100個字元")]
        [Display(Name = "欄位名稱")]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// 欄位顯示名稱（UI 顯示用，如「測試溫度」）
        /// </summary>
        [Required(ErrorMessage = "顯示名稱為必填")]
        [MaxLength(100, ErrorMessage = "顯示名稱不可超過100個字元")]
        [Display(Name = "顯示名稱")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 欄位資料類型
        /// </summary>
        [Required]
        [Display(Name = "資料類型")]
        public CustomFieldType FieldType { get; set; } = CustomFieldType.Text;

        /// <summary>
        /// 是否為必填欄位
        /// </summary>
        [Display(Name = "必填")]
        public bool IsRequired { get; set; } = false;

        /// <summary>
        /// 排序順序（決定欄位在表單和列表中的顯示順序）
        /// </summary>
        [Display(Name = "排序")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否在列表中顯示
        /// </summary>
        [Display(Name = "列表中顯示")]
        public bool ShowInList { get; set; } = true;

        /// <summary>
        /// 是否在表單中顯示
        /// </summary>
        [Display(Name = "表單中顯示")]
        public bool ShowInForm { get; set; } = true;

        /// <summary>
        /// 下拉選單選項（JSON 格式，僅 FieldType = Select 時使用）
        /// 例如：["通過","不通過"] 或 ["A級","B級","C級"]
        /// </summary>
        [MaxLength(1000, ErrorMessage = "選項內容不可超過1000個字元")]
        [Display(Name = "選項")]
        public string? Options { get; set; }

        /// <summary>
        /// 預設值
        /// </summary>
        [MaxLength(500)]
        [Display(Name = "預設值")]
        public string? DefaultValue { get; set; }

        /// <summary>
        /// 欄位提示文字
        /// </summary>
        [MaxLength(200, ErrorMessage = "提示文字不可超過200個字元")]
        [Display(Name = "提示文字")]
        public string? Placeholder { get; set; }
    }
}
