using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 預收付款項實體 - 記錄預收款、預付款項，供沖款單使用
    /// 當廠商/客戶要進行預付/收的時候，就是透過此表作紀錄
    /// 當其他的沖款表要使用預收/付沖款時會根據來源單號查詢對應的廠商/客戶是否曾經有預收/付然後還沒有用完的，就可以用來沖款
    /// </summary>
    [Index(nameof(SourceDocumentCode))]
    [Index(nameof(PrepaymentTypeId))]
    [Index(nameof(CustomerId))]
    [Index(nameof(SupplierId))]
    [Index(nameof(SetoffDocumentId))]
    public class SetoffPrepayment : BaseEntity
    {
        /// <summary>
        /// 預收付類型ID
        /// </summary>
        [Required(ErrorMessage = "預收付類型為必填")]
        [Display(Name = "預收付類型")]
        [ForeignKey(nameof(PrepaymentType))]
        public int PrepaymentTypeId { get; set; }
        
        /// <summary>
        /// 來源單號 - 此預收付款項的來源憑證單號
        /// </summary>
        [Required(ErrorMessage = "來源單號為必填")]
        [MaxLength(50, ErrorMessage = "來源單號不可超過50個字元")]
        [Display(Name = "來源單號")]
        public string SourceDocumentCode { get; set; } = string.Empty;
        
        /// <summary>
        /// 金額 - 預收付款項總金額
        /// </summary>
        [Required(ErrorMessage = "金額為必填")]
        [Display(Name = "金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } = 0;
        
        /// <summary>
        /// 已用金額 - 已經被沖款使用的金額
        /// </summary>
        [Display(Name = "已用金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UsedAmount { get; set; } = 0;
        
        /// <summary>
        /// 客戶ID（預收款時使用）
        /// </summary>
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }
        
        /// <summary>
        /// 供應商ID（預付款時使用）
        /// </summary>
        [Display(Name = "供應商")]
        [ForeignKey(nameof(Supplier))]
        public int? SupplierId { get; set; }
        
        /// <summary>
        /// 沖款單ID - 此預收付款項所關聯的沖款單（多對一關係）
        /// </summary>
        [Display(Name = "沖款單")]
        [ForeignKey(nameof(SetoffDocument))]
        public int? SetoffDocumentId { get; set; }
        
        // Navigation Properties
        
        /// <summary>
        /// 預收付類型導航屬性
        /// </summary>
        public PrepaymentType PrepaymentType { get; set; } = null!;
        
        /// <summary>
        /// 客戶導航屬性
        /// </summary>
        public Customer? Customer { get; set; }
        
        /// <summary>
        /// 供應商導航屬性
        /// </summary>
        public Supplier? Supplier { get; set; }
        
        /// <summary>
        /// 沖款單導航屬性 - 此預收付款項所屬的沖款單
        /// </summary>
        public SetoffDocument? SetoffDocument { get; set; }
        
        // Computed Properties (NotMapped)
        
        /// <summary>
        /// 可用餘額（計算屬性）- 總金額減去已用金額
        /// </summary>
        [NotMapped]
        public decimal AvailableBalance => Amount - UsedAmount;
        
        /// <summary>
        /// 是否已全部使用完畢
        /// </summary>
        [NotMapped]
        public bool IsFullyUsed => UsedAmount >= Amount;
        
        /// <summary>
        /// 使用率（百分比）
        /// </summary>
        [NotMapped]
        public decimal UsageRate => Amount > 0 ? (UsedAmount / Amount) * 100 : 0;
    }
}