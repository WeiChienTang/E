using System.ComponentModel.DataAnnotations;

namespace ERPCore2.Data.Entities
{
    /// <summary>
    /// 系統參數實體 - 儲存系統全域設定參數
    /// </summary>
    public class SystemParameter : BaseEntity
    {
        /// <summary>
        /// 稅率 (%)
        /// </summary>
        [Display(Name = "稅率 (%)")]
        [Range(0.00, 100.00, ErrorMessage = "稅率範圍必須在 0% 到 100% 之間")]
        public decimal TaxRate { get; set; } = 5.00m; // 預設 5% 稅率

        // ===== 審核流程開關 =====

        /// <summary>
        /// 是否啟用採購單審核
        /// </summary>
        [Display(Name = "啟用採購單審核")]
        public bool EnablePurchaseOrderApproval { get; set; } = false;

        /// <summary>
        /// 是否啟用進貨單審核
        /// </summary>
        [Display(Name = "啟用進貨單審核")]
        public bool EnablePurchaseReceivingApproval { get; set; } = false;

        /// <summary>
        /// 是否啟用進貨退回審核
        /// </summary>
        [Display(Name = "啟用進貨退回審核")]
        public bool EnablePurchaseReturnApproval { get; set; } = false;

        /// <summary>
        /// 是否啟用銷貨單審核
        /// </summary>
        [Display(Name = "啟用銷貨單審核")]
        public bool EnableSalesOrderApproval { get; set; } = false;

        /// <summary>
        /// 是否啟用銷貨退回審核
        /// </summary>
        [Display(Name = "啟用銷貨退回審核")]
        public bool EnableSalesReturnApproval { get; set; } = false;

        /// <summary>
        /// 是否啟用庫存調撥審核
        /// </summary>
        [Display(Name = "啟用庫存調撥審核")]
        public bool EnableInventoryTransferApproval { get; set; } = false;
    }
}