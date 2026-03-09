using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>
    /// 勞保投保薪資分級表
    /// 共34個等級，每年可能調整，需保留各年度版本
    /// </summary>
    public class LaborInsuranceGrade
    {
        public int Id { get; set; }

        /// <summary>等級（1-34）</summary>
        [Display(Name = "等級")] public int Grade { get; set; }

        /// <summary>實際薪資下限（此等級適用範圍）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "薪資下限")] public decimal SalaryFrom { get; set; }

        /// <summary>實際薪資上限（null = 最高等級無上限）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "薪資上限")] public decimal? SalaryTo { get; set; }

        /// <summary>投保薪資（此等級的申報金額）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "投保薪資")] public decimal InsuredSalary { get; set; }

        /// <summary>生效日期（對應各年度版本）</summary>
        [Display(Name = "生效日期")] public DateOnly EffectiveDate { get; set; }
    }
}
