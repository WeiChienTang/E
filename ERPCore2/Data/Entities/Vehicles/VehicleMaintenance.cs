using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 車輛保養紀錄
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class VehicleMaintenance : BaseEntity
    {
        [Required(ErrorMessage = "所屬車輛為必填")]
        [Display(Name = "所屬車輛")]
        [ForeignKey(nameof(Vehicle))]
        public int VehicleId { get; set; }

        public Vehicle Vehicle { get; set; } = null!;

        [Required(ErrorMessage = "保養類型為必填")]
        [Display(Name = "保養類型")]
        public MaintenanceType MaintenanceType { get; set; }

        [Required(ErrorMessage = "保養日期為必填")]
        [Display(Name = "保養日期")]
        public DateTime MaintenanceDate { get; set; }

        [Display(Name = "保養時里程(公里)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MileageAtMaintenance { get; set; }

        [MaxLength(500, ErrorMessage = "保養內容描述不可超過500個字元")]
        [Display(Name = "保養內容描述")]
        public string? Description { get; set; }

        [Display(Name = "費用")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        [MaxLength(100, ErrorMessage = "維修廠/服務商不可超過100個字元")]
        [Display(Name = "維修廠/服務商")]
        public string? ServiceProvider { get; set; }

        [Display(Name = "下次預計保養日")]
        public DateTime? NextMaintenanceDate { get; set; }

        [Display(Name = "經手人")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        public Employee? Employee { get; set; }
    }

    /// <summary>
    /// 保養類型
    /// </summary>
    public enum MaintenanceType
    {
        [Display(Name = "定期保養")]
        RegularService = 1,
        [Display(Name = "維修")]
        Repair = 2,
        [Display(Name = "輪胎更換")]
        TireChange = 3,
        [Display(Name = "換機油")]
        OilChange = 4,
        [Display(Name = "保險")]
        Insurance = 5,
        [Display(Name = "驗車")]
        Inspection = 6,
        [Display(Name = "其他")]
        Other = 99
    }
}
