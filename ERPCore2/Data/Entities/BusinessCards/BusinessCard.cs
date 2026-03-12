using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 名片照片 - 掛載於客戶或廠商下的聯絡人名片
    /// </summary>
    public class BusinessCard : BaseEntity
    {
        /// <summary>擁有者類型 ("Customer" | "Supplier")</summary>
        [Required]
        [MaxLength(20)]
        public string OwnerType { get; set; } = string.Empty;

        /// <summary>擁有者 ID（客戶或廠商的 Id）</summary>
        [Required]
        public int OwnerId { get; set; }

        /// <summary>聯絡人姓名（名片上的姓名）</summary>
        [MaxLength(50)]
        public string? ContactPersonName { get; set; }

        /// <summary>職稱</summary>
        [MaxLength(50)]
        public string? JobTitle { get; set; }

        /// <summary>照片在系統資料夾中的相對路徑</summary>
        [Required]
        [MaxLength(500)]
        public string PhotoPath { get; set; } = string.Empty;

        /// <summary>原始檔案名稱</summary>
        [MaxLength(255)]
        public string? OriginalFileName { get; set; }

        /// <summary>排序順序</summary>
        public int SortOrder { get; set; } = 0;
    }
}
