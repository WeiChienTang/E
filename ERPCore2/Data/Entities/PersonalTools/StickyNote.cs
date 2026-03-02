using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 便條貼 - 每位員工的個人快速記事
    /// </summary>
    public class StickyNote : BaseEntity
    {
        /// <summary>
        /// 所屬員工
        /// </summary>
        [Required]
        [Display(Name = "員工")]
        [ForeignKey(nameof(Employee))]
        public int EmployeeId { get; set; }

        /// <summary>
        /// 便條內容
        /// </summary>
        [Required]
        [Display(Name = "內容")]
        [MaxLength(1000, ErrorMessage = "便條內容不可超過 1000 字元")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 便條顏色分類
        /// </summary>
        [Display(Name = "顏色")]
        public StickyNoteColor Color { get; set; } = StickyNoteColor.Yellow;

        /// <summary>
        /// 排序順序（預留拖曳排序用）
        /// </summary>
        [Display(Name = "排序")]
        public int SortOrder { get; set; } = 0;

        // 導航屬性
        public Employee? Employee { get; set; }
    }

    /// <summary>
    /// 便條貼顏色列舉
    /// </summary>
    public enum StickyNoteColor
    {
        [Display(Name = "黃色")]
        Yellow = 1,

        [Display(Name = "綠色")]
        Green = 2,

        [Display(Name = "藍色")]
        Blue = 3,

        [Display(Name = "紅色")]
        Red = 4
    }
}
