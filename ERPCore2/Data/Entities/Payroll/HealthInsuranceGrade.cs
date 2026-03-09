using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>
    /// 健保投保金額分級表
    /// 每年可能調整，需保留各年度版本
    /// </summary>
    public class HealthInsuranceGrade
    {
        public int Id { get; set; }

        /// <summary>等級</summary>
        [Display(Name = "等級")] public int Grade { get; set; }

        /// <summary>實際薪資下限</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "薪資下限")] public decimal SalaryFrom { get; set; }

        /// <summary>實際薪資上限（null = 最高等級無上限）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "薪資上限")] public decimal? SalaryTo { get; set; }

        /// <summary>投保金額（區別於勞保的「投保薪資」）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "投保金額")] public decimal InsuredAmount { get; set; }

        /// <summary>生效日期（對應各年度版本）</summary>
        [Display(Name = "生效日期")] public DateOnly EffectiveDate { get; set; }
    }
}
