using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models
{
    /// <summary>
    /// 沖款付款明細 DTO - 用於管理沖款單的付款方式明細
    /// </summary>
    public class SetoffPaymentDetailDto
    {
        /// <summary>
        /// 明細ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 沖款單ID
        /// </summary>
        public int SetoffId { get; set; }

        /// <summary>
        /// 付款方式ID
        /// </summary>
        [Display(Name = "付款方式")]
        public int? PaymentMethodId { get; set; }

        /// <summary>
        /// 付款方式名稱
        /// </summary>
        public string PaymentMethodName { get; set; } = string.Empty;

        /// <summary>
        /// 銀行ID
        /// </summary>
        [Display(Name = "銀行")]
        public int? BankId { get; set; }

        /// <summary>
        /// 銀行名稱
        /// </summary>
        public string? BankName { get; set; }

        /// <summary>
        /// 付款金額
        /// </summary>
        [Display(Name = "付款金額")]
        public decimal Amount { get; set; } = 0;

        /// <summary>
        /// 帳號/票號
        /// </summary>
        [MaxLength(100, ErrorMessage = "帳號不可超過100個字元")]
        [Display(Name = "帳號/票號")]
        public string? AccountNumber { get; set; }

        /// <summary>
        /// 交易參考號
        /// </summary>
        [MaxLength(100, ErrorMessage = "交易參考號不可超過100個字元")]
        [Display(Name = "交易參考號")]
        public string? TransactionReference { get; set; }

        /// <summary>
        /// 付款日期
        /// </summary>
        [Display(Name = "付款日期")]
        public DateTime? PaymentDate { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        [MaxLength(500, ErrorMessage = "備註不可超過500個字元")]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }

        #region 驗證方法

        /// <summary>
        /// 驗證付款金額
        /// </summary>
        /// <param name="totalSetoffAmount">沖款總額</param>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateAmount(decimal totalSetoffAmount)
        {
            if (Amount <= 0)
            {
                return (false, "付款金額必須大於 0");
            }

            if (Amount > totalSetoffAmount)
            {
                return (false, $"付款金額 {Amount:N2} 不可超過沖款總額 {totalSetoffAmount:N2}");
            }

            return (true, null);
        }

        /// <summary>
        /// 驗證付款方式
        /// </summary>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidatePaymentMethod()
        {
            if (!PaymentMethodId.HasValue || PaymentMethodId.Value <= 0)
            {
                return (false, "請選擇付款方式");
            }

            return (true, null);
        }

        /// <summary>
        /// 全面驗證
        /// </summary>
        /// <param name="totalSetoffAmount">沖款總額</param>
        /// <returns>驗證結果和錯誤訊息列表</returns>
        public (bool IsValid, List<string> Errors) ValidateAll(decimal totalSetoffAmount)
        {
            var errors = new List<string>();

            // 驗證付款方式
            var paymentMethodValidation = ValidatePaymentMethod();
            if (!paymentMethodValidation.IsValid)
            {
                errors.Add(paymentMethodValidation.ErrorMessage!);
            }

            // 驗證金額
            var amountValidation = ValidateAmount(totalSetoffAmount);
            if (!amountValidation.IsValid)
            {
                errors.Add(amountValidation.ErrorMessage!);
            }

            return (errors.Count == 0, errors);
        }

        #endregion
    }
}
