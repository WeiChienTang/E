using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Models
{
    /// <summary>
    /// 模組權限矩陣模型 - 用於權限顯示表格
    /// </summary>
    public class ModulePermissionMatrix
    {
        /// <summary>
        /// 模組名稱
        /// </summary>
        [Display(Name = "模組名稱")]
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// 模組顯示名稱
        /// </summary>
        [Display(Name = "模組顯示名稱")]
        public string ModuleDisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 是否有檢視權限
        /// </summary>
        [Display(Name = "檢視")]
        public bool CanView { get; set; } = false;

        /// <summary>
        /// 是否有建立權限
        /// </summary>
        [Display(Name = "建立")]
        public bool CanCreate { get; set; } = false;

        /// <summary>
        /// 是否有修改權限
        /// </summary>
        [Display(Name = "修改")]
        public bool CanUpdate { get; set; } = false;

        /// <summary>
        /// 是否有刪除權限
        /// </summary>
        [Display(Name = "刪除")]
        public bool CanDelete { get; set; } = false;

        /// <summary>
        /// 權限總數
        /// </summary>
        public int TotalPermissions { get; set; } = 0;

        /// <summary>
        /// 原始權限清單（用於詳細資訊）
        /// </summary>
        public List<string> PermissionCodes { get; set; } = new();

        /// <summary>
        /// 權限名稱清單
        /// </summary>
        public List<string> PermissionNames { get; set; } = new();
    }
}
