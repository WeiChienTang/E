using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨出貨主檔實體 - 記錄銷貨出貨基本資訊
    /// </summary>
    [Index(nameof(DeliveryNumber), IsUnique = true)]
    [Index(nameof(SalesOrderId), nameof(DeliveryDate))]
    [Index(nameof(DeliveryStatus), nameof(DeliveryDate))]
    public class SalesDelivery : BaseEntity
    {
        [Required(ErrorMessage = "出貨單號為必填")]
        [MaxLength(30, ErrorMessage = "出貨單號不可超過30個字元")]
        [Display(Name = "出貨單號")]
        public string DeliveryNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "出貨日期為必填")]
        [Display(Name = "出貨日期")]
        public DateTime DeliveryDate { get; set; } = DateTime.Today;

        [Display(Name = "預計送達日期")]
        public DateTime? ExpectedArrivalDate { get; set; }

        [Display(Name = "實際送達日期")]
        public DateTime? ActualArrivalDate { get; set; }

        [Required(ErrorMessage = "出貨狀態為必填")]
        [Display(Name = "出貨狀態")]
        public SalesDeliveryStatus DeliveryStatus { get; set; } = SalesDeliveryStatus.Pending;

        [MaxLength(100, ErrorMessage = "出貨人員不可超過100個字元")]
        [Display(Name = "出貨人員")]
        public string? DeliveryPersonnel { get; set; }

        [MaxLength(100, ErrorMessage = "運送方式不可超過100個字元")]
        [Display(Name = "運送方式")]
        public string? ShippingMethod { get; set; }

        [MaxLength(50, ErrorMessage = "追蹤號碼不可超過50個字元")]
        [Display(Name = "追蹤號碼")]
        public string? TrackingNumber { get; set; }

        [MaxLength(500, ErrorMessage = "出貨備註不可超過500個字元")]
        [Display(Name = "出貨備註")]
        public string? DeliveryRemarks { get; set; }

        [MaxLength(200, ErrorMessage = "收貨地址不可超過200個字元")]
        [Display(Name = "收貨地址")]
        public string? DeliveryAddress { get; set; }

        [MaxLength(50, ErrorMessage = "收貨聯絡人不可超過50個字元")]
        [Display(Name = "收貨聯絡人")]
        public string? DeliveryContact { get; set; }

        [MaxLength(20, ErrorMessage = "收貨聯絡電話不可超過20個字元")]
        [Display(Name = "收貨聯絡電話")]
        public string? DeliveryPhone { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "銷貨訂單為必填")]
        [Display(Name = "銷貨訂單")]
        [ForeignKey(nameof(SalesOrder))]
        public int SalesOrderId { get; set; }

        [Display(Name = "員工")]
        [ForeignKey(nameof(Employee))]
        public int? EmployeeId { get; set; }

        // Navigation Properties
        public SalesOrder SalesOrder { get; set; } = null!;
        public Employee? Employee { get; set; }
        public ICollection<SalesDeliveryDetail> SalesDeliveryDetails { get; set; } = new List<SalesDeliveryDetail>();
    }
}
