using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 設備類別
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class EquipmentCategory : BaseEntity
    {
        [Required(ErrorMessage = "類別名稱為必填")]
        [MaxLength(50, ErrorMessage = "類別名稱不可超過50個字元")]
        [Display(Name = "類別名稱")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200, ErrorMessage = "類別描述不可超過200個字元")]
        [Display(Name = "類別描述")]
        public string? Description { get; set; }

        // Navigation Properties
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
