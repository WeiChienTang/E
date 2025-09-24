using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Entities;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 銷貨退回原因實體
    /// </summary>
    public class SalesReturnReason : BaseEntity
    {
        [Required(ErrorMessage = "原因名稱為必填")]
        [MaxLength(50, ErrorMessage = "原因名稱不可超過50個字元")]
        [Display(Name = "原因名稱")]
        public string Name { get; set; } = string.Empty;

        // 導航屬性 - 使用此退貨原因的銷售退貨單
        public ICollection<SalesReturn> SalesReturns { get; set; } = new List<SalesReturn>();
    }
}