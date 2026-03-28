using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities.CustomTables
{
    /// <summary>
    /// 自訂資料表定義 - 使用者可動態建立的資料表
    /// EBC 可配置化 Level 1：讓管理員自訂資料表結構，不需改程式碼
    /// </summary>
    public class CustomTableDefinition : BaseEntity
    {
        /// <summary>
        /// 資料表名稱（顯示用，如「耐熱測試紀錄」）
        /// </summary>
        [Required(ErrorMessage = "資料表名稱為必填")]
        [MaxLength(100, ErrorMessage = "資料表名稱不可超過100個字元")]
        [Display(Name = "資料表名稱")]
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 資料表說明
        /// </summary>
        [MaxLength(500, ErrorMessage = "說明不可超過500個字元")]
        [Display(Name = "說明")]
        public string? Description { get; set; }

        /// <summary>
        /// 資料表圖示（Bootstrap Icon class，如 "bi-table"）
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "圖示")]
        public string? IconClass { get; set; }

        /// <summary>
        /// 資料編號前綴（用於自動產生 Code，如 "HT" → HT-001）
        /// </summary>
        [MaxLength(10, ErrorMessage = "編號前綴不可超過10個字元")]
        [Display(Name = "編號前綴")]
        public string? CodePrefix { get; set; }

        /// <summary>
        /// 導航群組鍵（對應 NavigationConfig 中父級選單的 MenuKey，如 "system_management"）
        /// 設定後，此資料表會作為子項目出現在對應的導航群組下
        /// null 表示不顯示在導航選單
        /// </summary>
        [MaxLength(100)]
        [Display(Name = "導航群組")]
        public string? NavigationGroupKey { get; set; }

        /// <summary>
        /// 欄位定義（導覽屬性）
        /// </summary>
        public List<CustomFieldDefinition> FieldDefinitions { get; set; } = new();

        /// <summary>
        /// 資料列（導覽屬性）
        /// </summary>
        public List<CustomTableRow> Rows { get; set; } = new();
    }
}
