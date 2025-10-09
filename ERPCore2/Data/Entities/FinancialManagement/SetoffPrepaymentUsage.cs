using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 預收付款項使用記錄實體 - 記錄預收付款項被沖款單使用的歷史
    /// 用途：當沖款單使用既有的預收付款項進行轉沖款時，建立此記錄
    /// 關係：多筆 Usage 對應一筆 SetoffPrepayment（一筆預收付可以被多次使用）
    /// </summary>
    [Index(nameof(SetoffPrepaymentId))]
    [Index(nameof(SetoffDocumentId))]
    [Index(nameof(UsageDate))]
    public class SetoffPrepaymentUsage : BaseEntity
    {
        /// <summary>
        /// 預收付款項ID - 指向原始的預收付款項記錄
        /// </summary>
        [Required(ErrorMessage = "預收付款項為必填")]
        [Display(Name = "預收付款項")]
        [ForeignKey(nameof(SetoffPrepayment))]
        public int SetoffPrepaymentId { get; set; }
        
        /// <summary>
        /// 沖款單ID - 使用該預收付款項的沖款單
        /// </summary>
        [Required(ErrorMessage = "沖款單為必填")]
        [Display(Name = "沖款單")]
        [ForeignKey(nameof(SetoffDocument))]
        public int SetoffDocumentId { get; set; }
        
        /// <summary>
        /// 使用金額 - 本次沖款單使用的金額
        /// </summary>
        [Required(ErrorMessage = "使用金額為必填")]
        [Display(Name = "使用金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UsedAmount { get; set; } = 0;
        
        /// <summary>
        /// 使用日期
        /// </summary>
        [Display(Name = "使用日期")]
        public DateTime UsageDate { get; set; } = DateTime.Now;
        
        // Navigation Properties
        
        /// <summary>
        /// 預收付款項導航屬性 - 指向被使用的預收付款項
        /// </summary>
        public SetoffPrepayment SetoffPrepayment { get; set; } = null!;
        
        /// <summary>
        /// 沖款單導航屬性 - 指向使用預收付款項的沖款單
        /// </summary>
        public SetoffDocument SetoffDocument { get; set; } = null!;
        
        // Computed Properties (NotMapped)
        
        /// <summary>
        /// 沖款單號（計算屬性）- 方便顯示用
        /// </summary>
        [NotMapped]
        [Display(Name = "沖款單號")]
        public string SetoffDocumentNumber => SetoffDocument?.SetoffNumber ?? string.Empty;
        
        /// <summary>
        /// 預收付款項來源單號（計算屬性）- 方便顯示用
        /// </summary>
        [NotMapped]
        [Display(Name = "來源單號")]
        public string SourceDocumentCode => SetoffPrepayment?.SourceDocumentCode ?? string.Empty;
        
        /// <summary>
        /// 剩餘可用金額（計算屬性）- 原始預收付款項的剩餘可用金額
        /// </summary>
        [NotMapped]
        [Display(Name = "剩餘可用金額")]
        public decimal RemainingBalance => SetoffPrepayment?.AvailableBalance ?? 0;
    }
}
