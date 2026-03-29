using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// Tab 頁籤顯示設定 - 控制 EditModal 中各 Tab 的顯示行為（全公司適用）
    /// EBC 可配置化 Level 4 延伸：讓使用者自訂 Tab 的顯示/隱藏與排序，不需改程式碼
    /// </summary>
    public class TabDisplaySetting : BaseEntity
    {
        /// <summary>
        /// 目標模組名稱（對應 Entity 類別名稱，如 "Customer", "Supplier"）
        /// </summary>
        [Required(ErrorMessage = "目標模組為必填")]
        [MaxLength(50, ErrorMessage = "目標模組不可超過50個字元")]
        [Display(Name = "目標模組")]
        public string TargetModule { get; set; } = string.Empty;

        /// <summary>
        /// Tab 識別鍵（對應 FormTabDefinition.TabKey）
        /// 主表單 Tab 為 "__main__"；自訂 Tab 為 DebugComponentName（如 "CustomerBankAccountTab"）
        /// </summary>
        [Required(ErrorMessage = "Tab 識別鍵為必填")]
        [MaxLength(100, ErrorMessage = "Tab 識別鍵不可超過100個字元")]
        [Display(Name = "Tab 識別鍵")]
        public string TabKey { get; set; } = string.Empty;

        /// <summary>
        /// 是否顯示此 Tab（false=隱藏）
        /// null = 使用程式碼預設值（顯示）
        /// </summary>
        [Display(Name = "顯示 Tab")]
        public bool? IsVisible { get; set; }

        /// <summary>
        /// Tab 排序順序（null = 使用程式碼預設順序）
        /// </summary>
        [Display(Name = "排序順序")]
        public int? SortOrder { get; set; }

        /// <summary>
        /// 判斷是否所有設定皆為預設值（null）— 如是，資料庫列可自動刪除
        /// </summary>
        public bool IsDefaultSetting() => IsVisible == null && SortOrder == null;
    }
}
