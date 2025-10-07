using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 沖銷商品明細實體 - 記錄商品層級的沖銷金額與折讓
    /// 支援部分沖銷，一筆來源明細可分多次完成沖銷
    /// </summary>
    [Index(nameof(SetoffDocumentId))]
    [Index(nameof(ProductId))]
    [Index(nameof(SourceDetailType), nameof(SourceDetailId))]
    public class SetoffProductDetail : BaseEntity
    {
        // ===== 主要關聯 =====
        [Required(ErrorMessage = "沖銷單據為必填")]
        [Display(Name = "沖銷單據")]
        public int SetoffDocumentId { get; set; }

        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        public int ProductId { get; set; }

        // ===== 來源明細關聯（使用 Enum + Id）=====
        [Required(ErrorMessage = "來源明細類型為必填")]
        [Display(Name = "來源明細類型")]
        public SetoffDetailType SourceDetailType { get; set; }

        [Required(ErrorMessage = "來源明細ID為必填")]
        [Display(Name = "來源明細ID")]
        public int SourceDetailId { get; set; }

        // ===== 沖銷金額（沖款）=====
        [Required(ErrorMessage = "本次沖款金額為必填")]
        [Display(Name = "本次沖款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentSetoffAmount { get; set; } = 0;

        [Required(ErrorMessage = "累計沖款金額為必填")]
        [Display(Name = "累計沖款金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSetoffAmount { get; set; } = 0;

        // ===== 折讓金額 =====
        [Display(Name = "本次折讓金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAllowanceAmount { get; set; } = 0;

        [Display(Name = "累計折讓金額")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAllowanceAmount { get; set; } = 0;

        // ===== 導航屬性 =====
        public SetoffDocument SetoffDocument { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
