using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>
    /// 員工薪資固定津貼項目 — 每筆薪資設定可有多筆固定月付項目
    /// （取代 EmployeeSalary 上的 PositionAllowance / MealAllowance / TransportAllowance 硬寫欄位）
    /// </summary>
    public class EmployeeSalaryItem
    {
        public int Id { get; set; }

        /// <summary>所屬薪資設定</summary>
        [Required]
        public int EmployeeSalaryId { get; set; }
        public EmployeeSalary EmployeeSalary { get; set; } = null!;

        /// <summary>薪資項目（Category = Allowance 的項目）</summary>
        [Required]
        public int PayrollItemId { get; set; }
        public PayrollItem PayrollItem { get; set; } = null!;

        /// <summary>每月固定金額</summary>
        [Required]
        [Column(TypeName = "decimal(18,0)")]
        [Range(0, 9_999_999)]
        public decimal Amount { get; set; }
    }
}
