using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 進貨退出原因實體
    /// </summary>
    public class PurchaseReturnReason : BaseEntity
    {
        [Required(ErrorMessage = "原因名稱為必填")]
        [MaxLength(50, ErrorMessage = "原因名稱不可超過50個字元")]
        [Display(Name = "原因名稱")]
        public string Name { get; set; } = string.Empty;

        // 導航屬性 - 使用此退出原因的進貨退出單
        public ICollection<PurchaseReturn> PurchaseReturns { get; set; } = new List<PurchaseReturn>();
    }
}
