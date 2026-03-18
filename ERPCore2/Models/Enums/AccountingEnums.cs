using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 領料用途分類（決定借方科目）
    /// </summary>
    public enum MaterialIssueType
    {
        [Description("生產用料")]
        [Display(Name = "生產用料")]
        Production = 1,

        [Description("一般消耗")]
        [Display(Name = "一般消耗")]
        GeneralConsumption = 2,

        [Description("樣品")]
        [Display(Name = "樣品")]
        Sample = 3
    }


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

        [Description("實體編碼（客戶/廠商/品項代碼）")]
        [Display(Name = "實體編碼（客戶/廠商/品項代碼）")]
        EntityCode = 1
    }

    /// <summary>
    /// 子科目連結類型（對應客戶/廠商的科目種類，null 視同 ReceivablePayable 以維持舊資料相容）
    /// </summary>
    public enum SubAccountLinkType
    {
        [Description("應收/應付帳款")]
        [Display(Name = "應收/應付帳款")]
        ReceivablePayable = 0,

        [Description("應收/應付票據")]
        [Display(Name = "應收/應付票據")]
        NoteReceivablePayable = 1,

        [Description("銷貨退回/進貨退出")]
        [Display(Name = "銷貨退回/進貨退出")]
        SalesPurchaseReturn = 2,

        [Description("預收/預付款項")]
        [Display(Name = "預收/預付款項")]
        AdvanceReceiptPayment = 3
    }

    /// <summary>
    /// 會計子科目 Tab 的實體類型（決定服務呼叫與欄位標籤）
    /// </summary>
    public enum AccountingEntityType
    {
        Customer = 1,
        Supplier = 2,
        Product = 3
    }

    /// <summary>
    /// 會計期間狀態
    /// </summary>
    public enum FiscalPeriodStatus
    {
        [Description("開放中")]
        [Display(Name = "開放中")]
        Open = 1,

        [Description("已關帳")]
        [Display(Name = "已關帳")]
        Closed = 2,

        [Description("已鎖定")]
        [Display(Name = "已鎖定")]
        Locked = 3
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
