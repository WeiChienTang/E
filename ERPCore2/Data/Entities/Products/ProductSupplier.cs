using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 商品-供應商關聯表
    /// 維護商品與供應商之間的採購關係
    /// </summary>
    [Index(nameof(ProductId), nameof(SupplierId), IsUnique = true, Name = "UX_ProductSupplier_ProductId_SupplierId")]
    [Index(nameof(ProductId), nameof(IsPreferred), nameof(Priority), Name = "IX_ProductSupplier_ProductId_IsPreferred_Priority")]
    [Index(nameof(SupplierId), Name = "IX_ProductSupplier_SupplierId")]
    public class ProductSupplier : BaseEntity
    {
        // ===== 關聯欄位 =====
        
        /// <summary>
        /// 商品ID
        /// </summary>
        [Display(Name = "商品")]
        [Required(ErrorMessage = "商品為必填")]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        
        /// <summary>
        /// 供應商ID
        /// </summary>
        [Display(Name = "供應商")]
        [Required(ErrorMessage = "供應商為必填")]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }
        
        // ===== 供應商優先順序 =====
        
        /// <summary>
        /// 是否為常用供應商（可以有多個常用供應商）
        /// 用於在推薦清單中優先顯示
        /// </summary>
        [Display(Name = "常用供應商")]
        public bool IsPreferred { get; set; } = false;
        
        /// <summary>
        /// 優先順序（數字越小越優先，用於排序顯示順序）
        /// 當有多個常用供應商時，決定推薦的先後順序
        /// 例如：1=第一順位, 2=第二順位...
        /// </summary>
        [Display(Name = "優先順序")]
        [Range(1, 999, ErrorMessage = "優先順序必須介於1到999之間")]
        public int Priority { get; set; } = 999;
        
        // ===== 採購資訊 =====
        
        /// <summary>
        /// 最近採購單價（參考用，採購單完成時自動更新）
        /// </summary>
        [Display(Name = "最近採購價格")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? LastPurchasePrice { get; set; }
        
        /// <summary>
        /// 最近採購日期（採購單完成時自動更新）
        /// </summary>
        [Display(Name = "最近採購日期")]
        public DateTime? LastPurchaseDate { get; set; }
        
        /// <summary>
        /// 供應商料號（供應商自己的商品編號，方便採購時對應）
        /// </summary>
        [Display(Name = "供應商料號")]
        [MaxLength(50, ErrorMessage = "供應商料號不可超過50個字元")]
        public string? SupplierProductCode { get; set; }
        
        // ===== 交貨條件 =====
        
        /// <summary>
        /// 預計交貨天數（從下單到交貨的天數，可選填）
        /// </summary>
        [Display(Name = "預計交貨天數")]
        [Range(0, 365, ErrorMessage = "交貨天數必須介於0到365之間")]
        public int? LeadTimeDays { get; set; }
        
        // ===== 備註說明 =====
        // 備註欄位繼承自 BaseEntity，不需重複宣告
        // BaseEntity.Remarks 已有 [MaxLength(500)] 限制
        
        // ===== 導航屬性 =====
        
        /// <summary>
        /// 關聯的商品
        /// </summary>
        public virtual Product? Product { get; set; }
        
        /// <summary>
        /// 關聯的供應商
        /// </summary>
        public virtual Supplier? Supplier { get; set; }
        
        // ===== 輔助屬性（不存入資料庫）=====
        
        /// <summary>
        /// 搜尋文字（用於 SearchableSelect 的搜尋功能）
        /// </summary>
        [NotMapped]
        public string? SearchText { get; set; }
        
        /// <summary>
        /// 篩選後的商品清單（用於 SearchableSelect 下拉選單）
        /// </summary>
        [NotMapped]
        public List<Product> FilteredProducts { get; set; } = new();
        
        /// <summary>
        /// 是否顯示商品下拉選單（用於 SearchableSelect）
        /// </summary>
        [NotMapped]
        public bool ShowProductDropdown { get; set; }
        
        /// <summary>
        /// 商品下拉選單的選中索引（用於 SearchableSelect 鍵盤導航）
        /// </summary>
        [NotMapped]
        public int ProductSelectedIndex { get; set; } = -1;
    }
}
