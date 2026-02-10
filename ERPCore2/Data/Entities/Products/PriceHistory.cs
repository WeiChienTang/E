using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 價格歷史表 - 記錄所有價格變更歷史
    /// </summary>
    [Index(nameof(ProductId), nameof(PriceType), nameof(ChangeDate))]
    [Index(nameof(ChangeDate))]
    public class PriceHistory : BaseEntity
    {
        // 基本資訊
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "價格類型為必填")]
        [Display(Name = "價格類型")]
        public PriceType PriceType { get; set; }

        // 價格變更資訊
        [Required(ErrorMessage = "原價格為必填")]
        [Display(Name = "原價格")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OldPrice { get; set; }

        [Required(ErrorMessage = "新價格為必填")]
        [Display(Name = "新價格")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NewPrice { get; set; }

        [Display(Name = "價格變動")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceChange => NewPrice - OldPrice;

        [Display(Name = "變動百分比")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal ChangePercentage => OldPrice != 0 ? ((NewPrice - OldPrice) / OldPrice) * 100 : 0;

        // 變更資訊
        [Required(ErrorMessage = "變更原因為必填")]
        [MaxLength(200, ErrorMessage = "變更原因不可超過200個字元")]
        [Display(Name = "變更原因")]
        public string ChangeReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "變更日期為必填")]
        [Display(Name = "變更日期")]
        public DateTime ChangeDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "變更人員為必填")]
        [Display(Name = "變更人員")]
        public int ChangedByUserId { get; set; }

        [MaxLength(100, ErrorMessage = "變更人員姓名不可超過100個字元")]
        [Display(Name = "變更人員姓名")]
        public string? ChangedByUserName { get; set; }

        // 關聯資訊
        [Display(Name = "關聯客戶")]
        [ForeignKey(nameof(RelatedCustomer))]
        public int? RelatedCustomerId { get; set; }

        [Display(Name = "關聯供應商")]
        [ForeignKey(nameof(RelatedSupplier))]
        public int? RelatedSupplierId { get; set; }

        [MaxLength(300, ErrorMessage = "變更詳細說明不可超過300個字元")]
        [Display(Name = "變更詳細說明")]
        public string? ChangeDetails { get; set; }

        // Navigation Properties
        public Product Product { get; set; } = null!;
        public Customer? RelatedCustomer { get; set; }
        public Supplier? RelatedSupplier { get; set; }
    }
}
