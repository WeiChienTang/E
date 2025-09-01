using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 紙張設定實體 - 用於報表和列印的紙張規格設定
    /// </summary>
    [Index(nameof(Name), IsUnique = true)]
    public class PaperSetting : BaseEntity
    {
        /// <summary>
        /// 紙張名稱
        /// </summary>
        [Required(ErrorMessage = "紙張名稱為必填")]
        [MaxLength(50, ErrorMessage = "紙張名稱不可超過50個字元")]
        [Display(Name = "紙張名稱")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 紙張類型（如：A4、A3、Letter等）
        /// </summary>
        [Required(ErrorMessage = "紙張類型為必填")]
        [MaxLength(20, ErrorMessage = "紙張類型不可超過20個字元")]
        [Display(Name = "紙張類型")]
        public string PaperType { get; set; } = string.Empty;

        /// <summary>
        /// 紙張寬度（單位：毫米）
        /// </summary>
        [Required(ErrorMessage = "紙張寬度為必填")]
        [Range(1, 9999, ErrorMessage = "紙張寬度必須在1-9999毫米之間")]
        [Display(Name = "寬度(cm)")]
        public decimal Width { get; set; }

        /// <summary>
        /// 紙張高度（單位：毫米）
        /// </summary>
        [Required(ErrorMessage = "紙張高度為必填")]
        [Range(1, 9999, ErrorMessage = "紙張高度必須在1-9999毫米之間")]
        [Display(Name = "高度(cm)")]
        public decimal Height { get; set; }

        /// <summary>
        /// 上邊距（單位：毫米）
        /// </summary>
        [Range(0, 999, ErrorMessage = "上邊距必須在0-999毫米之間")]
        [Display(Name = "上邊距(cm)")]
        public decimal TopMargin { get; set; } = 10;

        /// <summary>
        /// 下邊距（單位：毫米）
        /// </summary>
        [Range(0, 999, ErrorMessage = "下邊距必須在0-999毫米之間")]
        [Display(Name = "下邊距(cm)")]
        public decimal BottomMargin { get; set; } = 10;

        /// <summary>
        /// 左邊距（單位：毫米）
        /// </summary>
        [Range(0, 999, ErrorMessage = "左邊距必須在0-999毫米之間")]
        [Display(Name = "左邊距(cm)")]
        public decimal LeftMargin { get; set; } = 10;

        /// <summary>
        /// 右邊距（單位：毫米）
        /// </summary>
        [Range(0, 999, ErrorMessage = "右邊距必須在0-999毫米之間")]
        [Display(Name = "右邊距(cm)")]
        public decimal RightMargin { get; set; } = 10;

        /// <summary>
        /// 紙張方向（Portrait: 直向, Landscape: 橫向）
        /// </summary>
        [Required(ErrorMessage = "紙張方向為必填")]
        [MaxLength(10, ErrorMessage = "紙張方向不可超過10個字元")]
        [Display(Name = "紙張方向")]
        public string Orientation { get; set; } = "Portrait";
    }
}
