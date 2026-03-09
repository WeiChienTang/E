using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>薪資週期流程狀態</summary>
    public enum PayrollPeriodStatus
    {
        [Display(Name = "草稿")] Draft = 1,
        [Display(Name = "計算中")] Processing = 2,
        [Display(Name = "已關帳")] Closed = 3
    }

    /// <summary>薪資項目類型（收入 / 扣除）</summary>
    public enum PayrollItemType
    {
        [Display(Name = "收入")] Income = 1,
        [Display(Name = "扣除")] Deduction = 2
    }

    /// <summary>薪資項目類別</summary>
    public enum PayrollItemCategory
    {
        [Display(Name = "薪資")] Salary = 1,
        [Display(Name = "津貼補助")] Allowance = 2,
        [Display(Name = "加班費")] Overtime = 3,
        [Display(Name = "獎金")] Bonus = 4,
        [Display(Name = "法定扣繳")] Legal = 5,
        [Display(Name = "其他")] Other = 6
    }

    /// <summary>薪資制度</summary>
    public enum SalaryType
    {
        [Display(Name = "月薪制")] Monthly = 1,
        [Display(Name = "時薪制")] Hourly = 2
    }

    /// <summary>扣繳類型（依所得稅法）</summary>
    public enum TaxWithholdingType
    {
        [Display(Name = "一般薪資扣繳")] Standard = 1,
        [Display(Name = "居留者")] Resident = 2,
        [Display(Name = "非居留者")] NonResident = 3
    }

    /// <summary>薪資單流程狀態</summary>
    public enum PayrollRecordStatus
    {
        [Display(Name = "試算中")] Draft = 1,
        [Display(Name = "已確認")] Confirmed = 2
    }
}
