using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 車型（車輛類型）
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class VehicleType : BaseEntity
    {
        [Required(ErrorMessage = "車型名稱為必填")]
        [MaxLength(50, ErrorMessage = "車型名稱不可超過50個字元")]
        [Display(Name = "車型名稱")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "車型描述不可超過200個字元")]
        [Display(Name = "車型描述")]
        public string? Description { get; set; }

        // Navigation Properties
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
