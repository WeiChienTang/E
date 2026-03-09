using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>薪資單明細 — 每筆 PayrollRecord 有多筆，逐項列示收入與扣除</summary>
    public class PayrollRecordDetail
    {
        public int Id { get; set; }

        [Required] public int PayrollRecordId { get; set; }
        public PayrollRecord Record { get; set; } = null!;

        [Required] public int PayrollItemId { get; set; }
        public PayrollItem Item { get; set; } = null!;

        /// <summary>數量（時數 / 天數 / 1）</summary>
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "數量")] public decimal Quantity { get; set; } = 1;

        /// <summary>單位金額</summary>
        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "單位金額")] public decimal UnitAmount { get; set; }

        /// <summary>小計（收入為正數，扣除為負數）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "小計")] public decimal Amount { get; set; }

        /// <summary>計算說明（如「加班 2.5hr × $125」）</summary>
        [MaxLength(200)]
        [Display(Name = "計算說明")] public string? Remark { get; set; }
    }
}
