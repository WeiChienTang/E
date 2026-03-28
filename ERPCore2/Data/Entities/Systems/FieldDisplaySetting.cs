using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 欄位顯示設定 - 控制既有欄位在表單和列表中的顯示行為（全公司適用）
    /// EBC 可配置化 Level 4：讓使用者自訂欄位的顯示、必填、名稱，不需改程式碼
    /// </summary>
    public class FieldDisplaySetting : BaseEntity
    {
        /// <summary>
        /// 目標模組名稱（對應 Entity 類別名稱，如 "Department", "Customer", "PurchaseOrder"）
        /// </summary>
        [Required(ErrorMessage = "目標模組為必填")]
        [MaxLength(50, ErrorMessage = "目標模組不可超過50個字元")]
        [Display(Name = "目標模組")]
        public string TargetModule { get; set; } = string.Empty;

        /// <summary>
        /// 欄位名稱（對應 Entity 的 Property Name，如 "Phone", "Location", "Email"）
        /// </summary>
        [Required(ErrorMessage = "欄位名稱為必填")]
        [MaxLength(100, ErrorMessage = "欄位名稱不可超過100個字元")]
        [Display(Name = "欄位名稱")]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// 欄位顯示名稱覆蓋（null = 使用程式碼預設名稱）
        /// 例如：將「電話」改為「分機號碼」
        /// </summary>
        [MaxLength(100, ErrorMessage = "顯示名稱不可超過100個字元")]
        [Display(Name = "自訂顯示名稱")]
        public string? DisplayNameOverride { get; set; }

        /// <summary>
        /// 是否在表單中顯示（true=顯示, false=隱藏）
        /// null = 使用程式碼預設值
        /// </summary>
        [Display(Name = "表單中顯示")]
        public bool? ShowInForm { get; set; }

        /// <summary>
        /// 是否在列表中顯示
        /// null = 使用程式碼預設值
        /// </summary>
        [Display(Name = "列表中顯示")]
        public bool? ShowInList { get; set; }

        /// <summary>
        /// 是否為必填（覆蓋程式碼中的 IsRequired 設定）
        /// null = 使用程式碼預設值
        /// </summary>
        [Display(Name = "必填覆蓋")]
        public bool? IsRequiredOverride { get; set; }

        /// <summary>
        /// 表單中的排序順序（null = 使用程式碼預設順序）
        /// </summary>
        [Display(Name = "排序順序")]
        public int? SortOrder { get; set; }

        /// <summary>
        /// 自訂提示文字覆蓋（null = 使用程式碼預設值）
        /// </summary>
        [MaxLength(200, ErrorMessage = "提示文字不可超過200個字元")]
        [Display(Name = "自訂提示文字")]
        public string? HelpTextOverride { get; set; }
    }
}
