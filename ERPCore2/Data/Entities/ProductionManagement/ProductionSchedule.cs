using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 生產排程批次狀態
    /// </summary>
    public enum ScheduleStatus
    {
        /// <summary>草稿 - 尚未確認</summary>
        [Display(Name = "草稿")]
        Draft = 1,

        /// <summary>已確認 - 可開始生產</summary>
        [Display(Name = "已確認")]
        Confirmed = 2,

        /// <summary>生產中 - 至少有一個項目已開始</summary>
        [Display(Name = "生產中")]
        InProgress = 3,

        /// <summary>已完成 - 所有項目皆已完成</summary>
        [Display(Name = "已完成")]
        Completed = 4
    }

    /// <summary>
    /// 生產排程主檔 - 記錄生產計劃與排程資訊
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(ScheduleDate))]
    public class ProductionSchedule : BaseEntity
    {
        [Required(ErrorMessage = "排程日期為必填")]
        [Display(Name = "排程日期")]
        public DateTime ScheduleDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 批次狀態
        /// </summary>
        [Display(Name = "批次狀態")]
        public ScheduleStatus ScheduleStatus { get; set; } = ScheduleStatus.Draft;

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
        /// 排程項目列表（要生產的品項）
        /// </summary>
        public ICollection<ProductionScheduleItem> ScheduleItems { get; set; } = new List<ProductionScheduleItem>();
    }
}
