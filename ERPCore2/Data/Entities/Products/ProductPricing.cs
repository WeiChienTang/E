using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品定價表 - 管理對客戶的銷售價格
    /// </summary>
    [Index(nameof(ProductId), nameof(CustomerId), nameof(EffectiveDate))]
    [Index(nameof(ProductId), nameof(PricingType), nameof(EffectiveDate))]
    public class ProductPricing : BaseEntity
    {
        // 基本資訊
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "定價類型為必填")]
        [Display(Name = "定價類型")]
        public PricingType PricingType { get; set; } = PricingType.Standard;

        // 價格資訊
        [Required(ErrorMessage = "價格為必填")]
        [Display(Name = "價格")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [MaxLength(3, ErrorMessage = "貨幣代碼不可超過3個字元")]
        [Display(Name = "貨幣")]
        public string Currency { get; set; } = "TWD";

        // 適用條件
        [Display(Name = "客戶")]
        [ForeignKey(nameof(Customer))]
        public int? CustomerId { get; set; }

        // TODO: 未來可新增客戶群組功能
        // [Display(Name = "客戶群組")]
        // public int? CustomerGroupId { get; set; }

        [Display(Name = "最小數量")]
        public int? MinQuantity { get; set; }

        [Display(Name = "最大數量")]
        public int? MaxQuantity { get; set; }

        // 時效性
        [Required(ErrorMessage = "生效日期為必填")]
        [Display(Name = "生效日期")]
        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        [Display(Name = "失效日期")]
        public DateTime? ExpiryDate { get; set; }

        // 優先順序
        [Display(Name = "優先順序")]
        public int Priority { get; set; } = 0;

        [MaxLength(200, ErrorMessage = "定價說明不可超過200個字元")]
        [Display(Name = "定價說明")]
        public string? PricingDescription { get; set; }

        // Navigation Properties
        public Product Product { get; set; } = null!;
        public Customer? Customer { get; set; }
        // public CustomerGroup? CustomerGroup { get; set; }
    }
}
