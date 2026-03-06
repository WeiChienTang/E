using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品-客戶關聯表
    /// 維護商品與客戶之間的銷售關係（客戶專屬售價、料號等）
    /// </summary>
    [Index(nameof(ProductId), nameof(CustomerId), IsUnique = true, Name = "UX_ProductCustomer_ProductId_CustomerId")]
    [Index(nameof(ProductId), nameof(IsPreferred), nameof(Priority), Name = "IX_ProductCustomer_ProductId_IsPreferred_Priority")]
    [Index(nameof(CustomerId), Name = "IX_ProductCustomer_CustomerId")]
    public class ProductCustomer : BaseEntity
    {
        // ===== 關聯欄位 =====

        [Display(Name = "商品")]
        [Required(ErrorMessage = "商品為必填")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Display(Name = "客戶")]
        [Required(ErrorMessage = "客戶為必填")]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }

        // ===== 客戶優先順序 =====

        [Display(Name = "常用客戶")]
        public bool IsPreferred { get; set; } = false;

        [Display(Name = "優先順序")]
        [Range(1, 999, ErrorMessage = "優先順序必須介於1到999之間")]
        public int Priority { get; set; } = 999;

        // ===== 銷售資訊 =====

        /// <summary>
        /// 客戶專屬售價（優先於商品標準售價）
        /// </summary>
        [Display(Name = "客戶售價")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustomerPrice { get; set; }

        /// <summary>
        /// 客戶折扣率（0~1，例如 0.9 表示 9 折）
        /// </summary>
        [Display(Name = "折扣率")]
        [Column(TypeName = "decimal(5,4)")]
        [Range(0, 1, ErrorMessage = "折扣率必須介於0到1之間")]
        public decimal? DiscountRate { get; set; }

        /// <summary>
        /// 客戶自己的料號（方便對帳與報價時對應）
        /// </summary>
        [Display(Name = "客戶料號")]
        [MaxLength(50, ErrorMessage = "客戶料號不可超過50個字元")]
        public string? CustomerProductCode { get; set; }

        /// <summary>
        /// 最近銷售單價（銷售出貨完成時自動更新）
        /// </summary>
        [Display(Name = "最近銷售價格")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LastSalePrice { get; set; }

        /// <summary>
        /// 最近銷售日期（銷售出貨完成時自動更新）
        /// </summary>
        [Display(Name = "最近銷售日期")]
        public DateTime? LastSaleDate { get; set; }

        // ===== 交貨條件 =====

        [Display(Name = "預計交貨天數")]
        [Range(0, 365, ErrorMessage = "交貨天數必須介於0到365之間")]
        public int? LeadTimeDays { get; set; }

        // ===== 備註說明 =====
        // 備註欄位繼承自 BaseEntity

        // ===== 導航屬性 =====

        public virtual Product? Product { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
