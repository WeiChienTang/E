using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 會計科目大類
    /// </summary>
    public enum AccountType
    {
        [Description("資產")]
        [Display(Name = "資產")]
        Asset = 1,

        [Description("負債")]
        [Display(Name = "負債")]
        Liability = 2,

        [Description("權益")]
        [Display(Name = "權益")]
        Equity = 3,

        [Description("營業收入")]
        [Display(Name = "營業收入")]
        Revenue = 4,

        [Description("營業成本")]
        [Display(Name = "營業成本")]
        Cost = 5,

        [Description("營業費用")]
        [Display(Name = "營業費用")]
        Expense = 6,

        [Description("營業外收益及費損")]
        [Display(Name = "營業外收益及費損")]
        NonOperatingIncomeAndExpense = 7,

        [Description("綜合損益總額")]
        [Display(Name = "綜合損益總額")]
        ComprehensiveIncome = 8
    }

    /// <summary>
    /// 借貸方向
    /// </summary>
    public enum AccountDirection
    {
        [Description("借方")]
        [Display(Name = "借方")]
        Debit = 1,

        [Description("貸方")]
        [Display(Name = "貸方")]
        Credit = 2
    }

    /// <summary>
    /// 子科目代碼格式
    /// Sequential：依序流水號（1191.001, 1191.002...）
    /// EntityCode：使用實體自身代碼作為後綴（1191.C0001）
    /// </summary>
    public enum SubAccountCodeFormat
    {
        [Description("流水號（001, 002...）")]
        [Display(Name = "流水號（001, 002...）")]
        Sequential = 0,

        [Description("實體編碼（客戶/廠商/商品代碼）")]
        [Display(Name = "實體編碼（客戶/廠商/商品代碼）")]
        EntityCode = 1
    }

    /// <summary>
    /// 會計科目層級（用於報表篩選）
    /// </summary>
    public enum AccountLevelFilter
    {
        [Display(Name = "第1層")]
        Level1 = 1,

        [Display(Name = "第2層")]
        Level2 = 2,

        [Display(Name = "第3層")]
        Level3 = 3,

        [Display(Name = "第4層")]
        Level4 = 4,

        [Display(Name = "第5層")]
        Level5 = 5
    }
}
