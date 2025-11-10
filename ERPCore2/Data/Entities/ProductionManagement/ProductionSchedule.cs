using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 生產排程主檔 - 記錄生產計劃與排程資訊
    /// </summary>
    [Index(nameof(ScheduleNumber), IsUnique = true)]
    [Index(nameof(ScheduleDate))]
    public class ProductionSchedule : BaseEntity
    {
        // 基本識別資訊
        [Required(ErrorMessage = "排程單號為必填")]
        [MaxLength(30, ErrorMessage = "排程單號不可超過30個字元")]
        [Display(Name = "排程單號")]
        public string ScheduleNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "排程日期為必填")]
        [Display(Name = "排程日期")]
        public DateTime ScheduleDate { get; set; } = DateTime.Today;

        // 責任與審核
        [Display(Name = "製單人員")]
        [ForeignKey(nameof(CreatedByEmployee))]
        public int? CreatedByEmployeeId { get; set; }

        // 來源單據
        [MaxLength(50, ErrorMessage = "來源單據類型不可超過50個字元")]
        [Display(Name = "來源單據類型")]
        public string? SourceDocumentType { get; set; }

        [Display(Name = "來源單據ID")]
        public int? SourceDocumentId { get; set; }

        [Display(Name = "來源銷售訂單")]
        [ForeignKey(nameof(SalesOrder))]
        public int? SalesOrderId { get; set; }

        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }

        // Navigation Properties
        /// <summary>
        /// 製單人員
        /// </summary>
        public Employee? CreatedByEmployee { get; set; }

        /// <summary>
        /// 客戶
        /// </summary>
        public Customer? Customer { get; set; }

        /// <summary>
        /// 來源銷售訂單
        /// </summary>
        public SalesOrder? SalesOrder { get; set; }

        /// <summary>
        /// 排程明細列表
        /// </summary>
        public ICollection<ProductionScheduleDetail> ScheduleDetails { get; set; } = new List<ProductionScheduleDetail>();
    }
}
