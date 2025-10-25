using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 領貨明細實體 - 記錄領貨明細資訊
    /// </summary>
    [Index(nameof(MaterialIssueId), nameof(ProductId))]
    [Index(nameof(ProductId))]
    [Index(nameof(WarehouseId))]
    public class MaterialIssueDetail : BaseEntity
    {
        [Required(ErrorMessage = "領貨主檔為必填")]
        [Display(Name = "領貨主檔")]
        [ForeignKey(nameof(MaterialIssue))]
        public int MaterialIssueId { get; set; }
        
        [Required(ErrorMessage = "商品為必填")]
        [Display(Name = "商品")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "倉庫為必填")]
        [Display(Name = "倉庫")]
        [ForeignKey(nameof(Warehouse))]
        public int WarehouseId { get; set; }
        
        [Display(Name = "庫位")]
        [ForeignKey(nameof(WarehouseLocation))]
        public int? WarehouseLocationId { get; set; }
        
        [Required(ErrorMessage = "領貨數量為必填")]
        [Display(Name = "領貨數量")]
        public int IssueQuantity { get; set; }
        
        [Display(Name = "單位成本")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitCost { get; set; }
        
        [Display(Name = "總成本")]
        [Column(TypeName = "decimal(18,4)")]
        [NotMapped]
        public decimal? TotalCost => UnitCost.HasValue ? UnitCost.Value * IssueQuantity : null;
        
        // Navigation Properties
        public MaterialIssue MaterialIssue { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
        public WarehouseLocation? WarehouseLocation { get; set; }
    }
}
