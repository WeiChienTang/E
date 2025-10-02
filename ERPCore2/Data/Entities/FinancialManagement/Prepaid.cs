using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 預付款實體 - 記錄預先支付給供應商的款項
    /// </summary>
    [Index(nameof(Code))]
    [Index(nameof(SupplierId))]
    [Index(nameof(PrepaidDate))]
    public class Prepaid : BaseEntity
    {
        /// <summary>
        /// 預付款日期
        /// </summary>
        [Required(ErrorMessage = "預付款日期為必填")]
        [Display(Name = "預付款日期")]
        public DateTime PrepaidDate { get; set; } = DateTime.Today;

        /// <summary>
        /// 預付款金額
        /// </summary>
        [Required(ErrorMessage = "預付款金額為必填")]
        [Display(Name = "預付款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrepaidAmount { get; set; } = 0;

        /// <summary>
        /// 供應商ID
        /// </summary>
        [Required(ErrorMessage = "供應商為必填")]
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }

        // Navigation Properties
        /// <summary>
        /// 供應商導航屬性
        /// </summary>
        public Supplier Supplier { get; set; } = null!;
    }
}
