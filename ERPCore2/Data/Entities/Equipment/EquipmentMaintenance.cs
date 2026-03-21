using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 設備保養維修記錄
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class EquipmentMaintenance : BaseEntity
    {
        [Display(Name = "所屬設備")]
        [ForeignKey(nameof(Equipment))]
        public int? EquipmentId { get; set; }

        public Equipment? Equipment { get; set; }

        [Required(ErrorMessage = "維修類型為必填")]
        [Display(Name = "維修類型")]
        public EquipmentMaintenanceType MaintenanceType { get; set; } = EquipmentMaintenanceType.RegularMaintenance;

        [Required(ErrorMessage = "保養日期為必填")]
        [Display(Name = "保養日期")]
        public DateTime MaintenanceDate { get; set; }

        [MaxLength(500, ErrorMessage = "工作內容不可超過500個字元")]
        [Display(Name = "工作內容")]
        public string? Description { get; set; }

        [Display(Name = "費用")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        [MaxLength(100, ErrorMessage = "服務廠商不可超過100個字元")]
        [Display(Name = "服務廠商")]
        public string? ServiceProvider { get; set; }

        [Display(Name = "下次保養日期")]
        public DateTime? NextMaintenanceDate { get; set; }

        [Display(Name = "執行人員")]
        [ForeignKey(nameof(Employee))]
        public int? PerformedByEmployeeId { get; set; }

        public Employee? Employee { get; set; }
    }
}
