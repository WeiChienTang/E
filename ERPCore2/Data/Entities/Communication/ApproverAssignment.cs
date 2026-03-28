using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 審核人員指派 - 定義各模組的審核通知對象
    /// 若模組未設定審核人員，則不發送審核請求通知（向下相容）
    /// 若已設定，則文件提交審核時通知指定人員
    /// </summary>
    [Index(nameof(ModuleName))]
    public class ApproverAssignment : BaseEntity
    {
        /// <summary>
        /// 模組名稱（如 "SalesOrder"、"PurchaseOrder"）
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "模組")]
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// 審核人員（EmployeeId）
        /// </summary>
        [Required]
        [Display(Name = "審核人員")]
        [ForeignKey(nameof(Approver))]
        public int ApproverEmployeeId { get; set; }

        /// <summary>
        /// 是否為主要審核人（主要審核人優先接收通知）
        /// </summary>
        [Display(Name = "主要審核人")]
        public bool IsPrimary { get; set; } = true;

        // 導航屬性
        public Employee? Approver { get; set; }

        // ===== 計算屬性 =====

        /// <summary>審核人員顯示名稱</summary>
        [NotMapped]
        public string ApproverDisplayName => Approver?.Name ?? "未知";
    }
}
