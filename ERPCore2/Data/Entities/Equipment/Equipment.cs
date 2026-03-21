using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 設備主檔
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Equipment : BaseEntity
    {
        [Required(ErrorMessage = "設備名稱為必填")]
        [MaxLength(100, ErrorMessage = "設備名稱不可超過100個字元")]
        [Display(Name = "設備名稱")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "出廠序號不可超過50個字元")]
        [Display(Name = "出廠序號")]
        public string? SerialNumber { get; set; }

        [MaxLength(50, ErrorMessage = "品牌不可超過50個字元")]
        [Display(Name = "品牌")]
        public string? Brand { get; set; }

        [MaxLength(50, ErrorMessage = "型號不可超過50個字元")]
        [Display(Name = "型號")]
        public string? Model { get; set; }

        [Display(Name = "購入日期")]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = "購入金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchaseCost { get; set; }

        [MaxLength(100, ErrorMessage = "放置地點不可超過100個字元")]
        [Display(Name = "放置地點")]
        public string? Location { get; set; }

        [Display(Name = "保養週期（月）")]
        public int? MaintenanceCycleMonths { get; set; }

        [Display(Name = "上次保養日期")]
        public DateTime? LastMaintenanceDate { get; set; }

        [Display(Name = "下次保養日期")]
        public DateTime? NextMaintenanceDate { get; set; }

        // ===== 外鍵關聯 =====

        [Display(Name = "設備類別")]
        [ForeignKey(nameof(EquipmentCategory))]
        public int? EquipmentCategoryId { get; set; }

        public EquipmentCategory? EquipmentCategory { get; set; }

        [Display(Name = "負責人員")]
        [ForeignKey(nameof(Employee))]
        public int? ResponsibleEmployeeId { get; set; }

        public Employee? Employee { get; set; }

        // ===== Navigation Properties =====

        public ICollection<EquipmentMaintenance> EquipmentMaintenances { get; set; } = new List<EquipmentMaintenance>();
    }
}
