using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 預收付款項實體 - 記錄預收款、預付款項的主檔資料
    /// 用途：當廠商/客戶預先支付款項時，建立此主記錄
    /// 關係：一筆 SetoffPrepayment 可以被多筆 SetoffPrepaymentUsage 使用（轉沖款）
    /// 注意：UsedAmount 應該等於所有相關 SetoffPrepaymentUsage 的 UsedAmount 總和
    /// </summary>
    [Index(nameof(SourceDocumentCode), IsUnique = true)]
    [Index(nameof(PrepaymentTypeId))]
    [Index(nameof(CustomerId))]
    [Index(nameof(SupplierId))]
    [Index(nameof(SetoffDocumentId))]
    [Index(nameof(SourcePrepaymentId))]
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
        /// 沖款單ID - 建立此預收付款項的沖款單（預收/預付時使用）
        /// 注意：此欄位記錄的是「建立」此預收付款項的沖款單，非必填
        /// 若要查詢「使用」此預收付款項的沖款單，請使用 SetoffPrepaymentUsage 表
        /// </summary>
        [Display(Name = "建立沖款單")]
        [ForeignKey(nameof(SetoffDocument))]
        public int? SetoffDocumentId { get; set; }
        
        /// <summary>
        /// 來源預收付款項ID - 用於「轉沖款」類型，記錄此筆轉沖款來自哪一筆預收付款項
        /// 只有「轉沖款」類型才會填寫此欄位，預收/預付類型此欄位為 null
        /// </summary>
        [Display(Name = "來源預收付款項")]
        [ForeignKey(nameof(SourcePrepayment))]
        public int? SourcePrepaymentId { get; set; }
        
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
        /// 沖款單導航屬性 - 建立此預收付款項的沖款單
        /// </summary>
        public SetoffDocument? SetoffDocument { get; set; }
        
        /// <summary>
        /// 來源預收付款項導航屬性 - 用於「轉沖款」類型
        /// 指向此筆轉沖款的來源預收付款項
        /// </summary>
        public SetoffPrepayment? SourcePrepayment { get; set; }
        
        /// <summary>
        /// 使用記錄集合 - 此預收付款項被哪些沖款單使用的記錄
        /// </summary>
        public ICollection<SetoffPrepaymentUsage> UsageRecords { get; set; } = new List<SetoffPrepaymentUsage>();
        
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
