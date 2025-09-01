using System.ComponentModel.DataAnnotations;
using ERPCore2.Data.Enums;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 印表機設定實體 - 管理系統中的印表機連接設定
    /// </summary>
    public class PrinterConfiguration : BaseEntity
    {
        /// <summary>
        /// 印表機名稱
        /// </summary>
        [Required(ErrorMessage = "印表機名稱為必填")]
        [MaxLength(100, ErrorMessage = "印表機名稱不可超過100個字元")]
        [Display(Name = "印表機名稱")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// IP 位址（適用於網路印表機）
        /// </summary>
        [MaxLength(50, ErrorMessage = "IP位址不可超過50個字元")]
        [Display(Name = "IP位址")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// 連接埠（適用於網路印表機）
        /// </summary>
        [Display(Name = "連接埠")]
        public int? Port { get; set; }

        /// <summary>
        /// 是否為預設印表機
        /// </summary>
        [Display(Name = "預設印表機")]
        public bool IsDefault { get; set; } = false;
    }
}
