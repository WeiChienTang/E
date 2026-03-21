using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>設備保養/維修類型</summary>
    public enum EquipmentMaintenanceType
    {
        [Display(Name = "定期保養")] RegularMaintenance = 1,
        [Display(Name = "維修")] Repair = 2,
        [Display(Name = "校驗")] Calibration = 3,
        [Display(Name = "檢查")] Inspection = 4,
        [Display(Name = "其他")] Other = 99
    }
}
