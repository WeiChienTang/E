using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>
    /// 薪資所得扣繳稅額表（財政部每年公告）
    /// 依「月薪資」區間 + 「扶養親屬人數」查扣繳稅額
    /// </summary>
    public class WithholdingTaxTable
    {
        public int Id { get; set; }

        /// <summary>月薪資下限</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "薪資下限")] public decimal SalaryFrom { get; set; }

        /// <summary>月薪資上限（null = 無上限）</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "薪資上限")] public decimal? SalaryTo { get; set; }

        /// <summary>扶養親屬人數（0 = 無扶養，含本人 = 1+扶養數）</summary>
        [Display(Name = "扶養人數")] public int DependentCount { get; set; }

        /// <summary>扣繳稅額</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "扣繳稅額")] public decimal TaxAmount { get; set; }

        /// <summary>生效日期（各年度版本）</summary>
        [Display(Name = "生效日期")] public DateOnly EffectiveDate { get; set; }
    }
}
