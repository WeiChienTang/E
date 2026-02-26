using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 員工受訓紀錄
    /// </summary>
    public class EmployeeTrainingRecord : BaseEntity
    {
        /// <summary>所屬員工 ID</summary>
        [Required(ErrorMessage = "請選擇員工")]
        [Display(Name = "員工")]
        public int EmployeeId { get; set; }

        /// <summary>所屬員工</summary>
        public Employee? Employee { get; set; }

        /// <summary>課程名稱</summary>
        [Required(ErrorMessage = "請輸入課程名稱")]
        [MaxLength(100, ErrorMessage = "課程名稱不可超過100個字元")]
        [Display(Name = "課程名稱")]
        public string CourseName { get; set; } = string.Empty;

        /// <summary>受訓日期</summary>
        [Required(ErrorMessage = "請輸入受訓日期")]
        [Display(Name = "受訓日期")]
        public DateTime TrainingDate { get; set; } = DateTime.Today;

        /// <summary>完成/結業日期</summary>
        [Display(Name = "完成日期")]
        public DateTime? CompletedDate { get; set; }

        /// <summary>訓練時數</summary>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "訓練時數")]
        public decimal? TrainingHours { get; set; }

        /// <summary>結果/成績</summary>
        [MaxLength(50, ErrorMessage = "結果不可超過50個字元")]
        [Display(Name = "結果/成績")]
        public string? Result { get; set; }

        /// <summary>訓練機構</summary>
        [MaxLength(100, ErrorMessage = "訓練機構不可超過100個字元")]
        [Display(Name = "訓練機構")]
        public string? TrainingOrganization { get; set; }
    }
}
