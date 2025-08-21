using ERPCore2.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data
{
    /// <summary>
    /// 基礎實體介面 - 定義所有實體共同的屬性
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// 主鍵 ID
        /// </summary>
        [Display(Name = "ID")]
        [Required(ErrorMessage = "請設定 ID")]
        public int Id { get; set; }
        
        /// <summary>
        /// 代碼
        /// </summary>
        [Display(Name = "代碼")]
        [MaxLength(50)]
        public string? Code { get; set; }
        
        /// <summary>
        /// 實體狀態
        /// </summary>
        [Display(Name = "實體狀態")]
        [Required(ErrorMessage = "請設定實體狀態")]
        public EntityStatus Status { get; set; } = EntityStatus.Active;

        /// <summary>
        /// 是否已刪除（軟刪除標記）
        /// </summary>
        [Display(Name = "已刪除")]
        [Required(ErrorMessage = "請設定是否已刪除")]
        public bool IsDeleted { get; set; } = false;
        
        /// <summary>
        /// 建立時間
        /// </summary>
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 最後更新時間
        /// </summary>
        [Display(Name = "最後更新時間")]        
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// 建立者 ID
        /// </summary>
        [Display(Name = "建立者")]
        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        
        /// <summary>
        /// 最後更新者 ID
        /// </summary>
        [Display(Name = "修改者")]
        [MaxLength(50)]
        public string? UpdatedBy { get; set; }
        
        /// <summary>
        /// 備註
        /// </summary>
        [Display(Name = "備註")]
        [MaxLength(500, ErrorMessage = "備註不可超過120個字元")]
        public string? Remarks { get; set; }
    }
}