using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 車輛主檔
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(LicensePlate), IsUnique = true)]
    public class Vehicle : BaseEntity
    {
        [Required(ErrorMessage = "車牌號碼為必填")]
        [MaxLength(20, ErrorMessage = "車牌號碼不可超過20個字元")]
        [Display(Name = "車牌號碼")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "車輛名稱為必填")]
        [MaxLength(100, ErrorMessage = "車輛名稱不可超過100個字元")]
        [Display(Name = "車輛名稱")]
        public string VehicleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "歸屬類型為必填")]
        [Display(Name = "歸屬類型")]
        public VehicleOwnershipType OwnershipType { get; set; } = VehicleOwnershipType.Company;

        [MaxLength(50, ErrorMessage = "廠牌不可超過50個字元")]
        [Display(Name = "廠牌")]
        public string? Brand { get; set; }

        [MaxLength(50, ErrorMessage = "車款型號不可超過50個字元")]
        [Display(Name = "車款型號")]
        public string? Model { get; set; }

        [Display(Name = "出廠年份")]
        public int? ManufactureYear { get; set; }

        [MaxLength(20, ErrorMessage = "車身顏色不可超過20個字元")]
        [Display(Name = "車身顏色")]
        public string? Color { get; set; }

        [MaxLength(50, ErrorMessage = "引擎號碼不可超過50個字元")]
        [Display(Name = "引擎號碼")]
        public string? EngineNumber { get; set; }

        [MaxLength(50, ErrorMessage = "車身號碼不可超過50個字元")]
        [Display(Name = "車身號碼(VIN)")]
        public string? ChassisNumber { get; set; }

        [Display(Name = "燃料類型")]
        public FuelType? FuelType { get; set; }

        [Display(Name = "載重量(公噸)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LoadCapacity { get; set; }

        [Display(Name = "目前里程數(公里)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Mileage { get; set; }

        [Display(Name = "購入日期")]
        public DateTime? PurchaseDate { get; set; }

        [Display(Name = "購入金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? PurchasePrice { get; set; }

        [Display(Name = "保險到期日")]
        public DateTime? InsuranceExpiryDate { get; set; }

        [MaxLength(100, ErrorMessage = "保險公司不可超過100個字元")]
        [Display(Name = "保險公司")]
        public string? InsuranceCompany { get; set; }

        [MaxLength(50, ErrorMessage = "保險單號不可超過50個字元")]
        [Display(Name = "保險單號")]
        public string? InsurancePolicyNumber { get; set; }

        [Display(Name = "驗車到期日")]
        public DateTime? InspectionExpiryDate { get; set; }

        [Display(Name = "最近保養日期")]
        public DateTime? LastMaintenanceDate { get; set; }

        [Display(Name = "下次保養里程")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? NextMaintenanceMileage { get; set; }

        // ===== 外鍵關聯 =====

        [Display(Name = "車型")]
        [ForeignKey(nameof(VehicleType))]
        public int? VehicleTypeId { get; set; }

        public VehicleType? VehicleType { get; set; }

        [Display(Name = "負責人/駕駛人")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Display(Name = "所屬客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [Display(Name = "所屬公司")]
        [ForeignKey(nameof(Company))]
        public int? CompanyId { get; set; }

        public Company? Company { get; set; }

        // ===== Navigation Properties =====

        public ICollection<VehicleMaintenance> VehicleMaintenances { get; set; } = new List<VehicleMaintenance>();
    }

    /// <summary>
    /// 車輛歸屬類型
    /// </summary>
    public enum VehicleOwnershipType
    {
        [Display(Name = "公司")]
        Company = 1,
        [Display(Name = "客戶")]
        Customer = 2
    }

    /// <summary>
    /// 燃料類型
    /// </summary>
    public enum FuelType
    {
        [Display(Name = "汽油")]
        Gasoline = 1,
        [Display(Name = "柴油")]
        Diesel = 2,
        [Display(Name = "電動")]
        Electric = 3,
        [Display(Name = "油電混合")]
        Hybrid = 4
    }
}
