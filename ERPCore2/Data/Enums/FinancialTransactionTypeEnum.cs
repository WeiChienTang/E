using System.ComponentModel;

namespace ERPCore2.Data.Enums
{
    /// <summary>
    /// 財務交易類型枚舉 - 定義所有財務交易的類型
    /// </summary>
    public enum FinancialTransactionTypeEnum
    {
        // === 應收帳款相關 ===
        
        /// <summary>
        /// 應收沖款 - 收取客戶款項沖銷應收帳款
        /// </summary>
        [Description("應收沖款")]
        AccountsReceivableSetoff = 1,
        
        /// <summary>
        /// 應收退款 - 退還客戶已付款項
        /// </summary>
        [Description("應收退款")]
        AccountsReceivableRefund = 2,
        
        /// <summary>
        /// 應收調整 - 調整應收帳款餘額
        /// </summary>
        [Description("應收調整")]
        AccountsReceivableAdjustment = 3,
        
        /// <summary>
        /// 應收折讓 - 不收取客戶款項但透過折讓抵銷應收帳款
        /// </summary>
        [Description("應收折讓")]
        AccountsReceivableDiscount = 4,
        
        // === 應付帳款相關 ===
        
        /// <summary>
        /// 應付沖款 - 支付供應商款項沖銷應付帳款
        /// </summary>
        [Description("應付沖款")]
        AccountsPayableSetoff = 11,
        
        /// <summary>
        /// 應付預付 - 預先支付供應商款項
        /// </summary>
        [Description("應付預付")]
        AccountsPayableAdvance = 12,
        
        /// <summary>
        /// 應付調整 - 調整應付帳款餘額
        /// </summary>
        [Description("應付調整")]
        AccountsPayableAdjustment = 13,
        
        // === 現金收付款 ===
        
        /// <summary>
        /// 現金收款 - 現金收取款項
        /// </summary>
        [Description("現金收款")]
        CashReceipt = 21,
        
        /// <summary>
        /// 現金付款 - 現金支付款項
        /// </summary>
        [Description("現金付款")]
        CashPayment = 22,
        
        /// <summary>
        /// 銀行收款 - 透過銀行收取款項
        /// </summary>
        [Description("銀行收款")]
        BankReceipt = 23,
        
        /// <summary>
        /// 銀行付款 - 透過銀行支付款項
        /// </summary>
        [Description("銀行付款")]
        BankPayment = 24,
        
        /// <summary>
        /// 支票收款 - 收取支票款項
        /// </summary>
        [Description("支票收款")]
        CheckReceipt = 25,
        
        /// <summary>
        /// 支票付款 - 開立支票付款
        /// </summary>
        [Description("支票付款")]
        CheckPayment = 26,
        
        // === 其他財務交易 ===
        
        /// <summary>
        /// 匯率調整 - 外幣匯率變動調整
        /// </summary>
        [Description("匯率調整")]
        ExchangeRateAdjustment = 31,
        
        /// <summary>
        /// 壞帳提列 - 提列壞帳損失
        /// </summary>
        [Description("壞帳提列")]
        BadDebtProvision = 32,
        
        /// <summary>
        /// 壞帳沖銷 - 沖銷確定無法收回的應收帳款
        /// </summary>
        [Description("壞帳沖銷")]
        BadDebtWriteOff = 33,
        
        /// <summary>
        /// 財務費用 - 利息支出、手續費等
        /// </summary>
        [Description("財務費用")]
        FinancialExpense = 34,
        
        /// <summary>
        /// 財務收入 - 利息收入、匯兌收益等
        /// </summary>
        [Description("財務收入")]
        FinancialIncome = 35,
        
        // === 預收預付款相關 (新增) ===
        
        /// <summary>
        /// 預收款 - 客戶預先支付款項
        /// </summary>
        [Description("預收款")]
        Prepayment = 41,
        
        /// <summary>
        /// 預付款 - 預先支付供應商款項
        /// </summary>
        [Description("預付款")]
        Prepaid = 42,
        
        /// <summary>
        /// 預收款使用 - 使用預收款沖抵應收帳款
        /// </summary>
        [Description("預收款使用")]
        PrepaymentUsage = 43,
        
        /// <summary>
        /// 預付款使用 - 使用預付款沖抵應付帳款
        /// </summary>
        [Description("預付款使用")]
        PrepaidUsage = 44,
        
        // === 沖款單付款記錄 (新增) ===
        
        /// <summary>
        /// 沖款單付款 - 沖款單的實際付款記錄
        /// </summary>
        [Description("沖款單付款")]
        SetoffPayment = 51
    }
}
