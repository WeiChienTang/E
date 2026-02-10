using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 付款方式 - 管理不同的付款方式類型
    /// </summary>
    public class PaymentMethod : BaseEntity
    {
        /// <summary>
        /// 付款方式名稱
        /// </summary>
        [Required(ErrorMessage = "付款方式名稱為必填")]
        [MaxLength(50, ErrorMessage = "付款方式名稱不可超過50個字元")]
        [Display(Name = "付款方式名稱")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 是否為預設付款方式
        /// </summary>
        [Display(Name = "預設付款方式")]
        public bool IsDefault { get; set; } = false;
    }
}
