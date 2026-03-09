using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Entities.Payroll
{
    /// <summary>薪資項目定義 — 系統預設 + 使用者自訂</summary>
    public class PayrollItem : BaseEntity
    {
        // BaseEntity 已提供：Id, Code（項目代碼）, Status（啟用/停用 EntityStatus）,
        //                    CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, Remarks
        // Code 使用繼承的 BaseEntity.Code（string?, MaxLength 50）
        // 啟用/停用使用繼承的 Status（EntityStatus.Active/Inactive）

        /// <summary>項目名稱</summary>
        [Required]
        [MaxLength(50)]
        [Display(Name = "名稱")]
        public string Name { get; set; } = string.Empty;

        /// <summary>項目類型：收入 / 扣除</summary>
        [Display(Name = "類型")]
        public PayrollItemType ItemType { get; set; }

        /// <summary>項目類別</summary>
        [Display(Name = "類別")]
        public PayrollItemCategory Category { get; set; }

        /// <summary>系統內建項目（不可刪除，不可修改 Code）</summary>
        [Display(Name = "系統項目")]
        public bool IsSystemItem { get; set; } = false;

        /// <summary>是否計入課稅所得</summary>
        [Display(Name = "計入課稅")]
        public bool IsTaxable { get; set; } = true;

        /// <summary>是否計入勞健保投保薪資基礎</summary>
        [Display(Name = "計入投保")]
        public bool IsInsuranceBasis { get; set; } = true;

        /// <summary>是否計入勞退提繳基礎</summary>
        [Display(Name = "計入勞退")]
        public bool IsRetirementBasis { get; set; } = true;

        /// <summary>排列順序（薪資單顯示用）</summary>
        [Display(Name = "排序")]
        public int SortOrder { get; set; } = 0;

        // 導航屬性
        public ICollection<PayrollRecordDetail> Details { get; set; } = new List<PayrollRecordDetail>();
    }
}
