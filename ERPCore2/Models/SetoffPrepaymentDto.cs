using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Models
{
    /// <summary>
    /// 沖款預收/預付款 DTO - 用於管理沖款單使用預收/預付款的明細
    /// </summary>
    public class SetoffPrepaymentDto
    {
        /// <summary>
        /// 明細ID（SetoffPrepaymentDetail.Id）
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 預收/預付款ID（Prepayment.Id）
        /// </summary>
        public int PrepaymentId { get; set; }
        
        /// <summary>
        /// 沖款單ID
        /// </summary>
        public int SetoffId { get; set; }
        
        /// <summary>
        /// 類別
        /// </summary>
        [Display(Name = "類別")]
        public PrepaymentType PrepaymentType { get; set; }
        
        /// <summary>
        /// 類別顯示名稱
        /// </summary>
        public string TypeDisplayName => PrepaymentType switch
        {
            PrepaymentType.Prepayment => "預收款",
            PrepaymentType.Prepaid => "預付款",
            PrepaymentType.Other => "其他",
            _ => "未知"
        };
        
        /// <summary>
        /// 來源單號
        /// </summary>
        [Display(Name = "來源單號")]
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// 款項日期
        /// </summary>
        [Display(Name = "款項日期")]
        public DateTime PaymentDate { get; set; }
        
        /// <summary>
        /// 原始金額
        /// </summary>
        [Display(Name = "原始金額")]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// 已用金額（不含本次，從資料庫計算）
        /// </summary>
        [Display(Name = "已用金額")]
        public decimal UsedAmount { get; set; }
        
        /// <summary>
        /// 可用金額（動態計算）
        /// 計算公式：原始金額 - 已用金額 + 本次使用金額（編輯模式時需加回）
        /// </summary>
        [Display(Name = "可用金額")]
        public decimal AvailableAmount => Amount - UsedAmount + OriginalThisTimeUseAmount;
        
        /// <summary>
        /// 本次使用金額
        /// </summary>
        [Display(Name = "本次使用金額")]
        public decimal ThisTimeUseAmount { get; set; } = 0;
        
        /// <summary>
        /// 原始本次使用金額（編輯模式用，載入時的值）
        /// </summary>
        public decimal OriginalThisTimeUseAmount { get; set; } = 0;
        
        /// <summary>
        /// 備註
        /// </summary>
        [MaxLength(500, ErrorMessage = "備註不可超過500個字元")]
        [Display(Name = "備註")]
        public string? Remarks { get; set; }
        
        /// <summary>
        /// 客戶/供應商名稱（可選，用於顯示）
        /// </summary>
        public string? PartnerName { get; set; }
        
        /// <summary>
        /// 驗證使用金額是否有效
        /// </summary>
        /// <returns>驗證結果</returns>
        public (bool IsValid, string? ErrorMessage) ValidateUseAmount()
        {
            // 驗證使用金額必須大於0
            if (ThisTimeUseAmount <= 0)
            {
                return (false, "使用金額必須大於0");
            }
            
            // 驗證使用金額不能大於可用金額
            if (ThisTimeUseAmount > AvailableAmount)
            {
                return (false, $"使用金額不能大於可用金額 {AvailableAmount:N2}");
            }
            
            return (true, null);
        }
    }
}
