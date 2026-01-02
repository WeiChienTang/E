using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 庫存異動類型實體 - 定義庫存異動的類型和規則
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class InventoryTransactionType : BaseEntity
    {        
        [Required(ErrorMessage = "類型名稱為必填")]
        [MaxLength(50, ErrorMessage = "類型名稱不可超過50個字元")]
        [Display(Name = "類型名稱")]
        public string TypeName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "異動類型為必填")]
        [Display(Name = "異動類型")]
        public InventoryTransactionTypeEnum TransactionType { get; set; }
        
        [Display(Name = "是否影響成本")]
        public bool AffectsCost { get; set; } = true;
        
        [Display(Name = "是否需要審核")]
        public bool RequiresApproval { get; set; } = false;
        
        [Display(Name = "是否自動產生單號")]
        public bool AutoGenerateNumber { get; set; } = true;
        
        [MaxLength(10, ErrorMessage = "單號前綴不可超過10個字元")]
        [Display(Name = "單號前綴")]
        public string? NumberPrefix { get; set; }
        
        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;
    }
}
