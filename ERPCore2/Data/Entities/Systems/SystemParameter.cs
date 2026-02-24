using System.ComponentModel.DataAnnotations;
using ERPCore2.Models.Enums;

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
        /// 是否啟用報價單審核
        /// </summary>
        [Display(Name = "啟用報價單審核")]
        public bool EnableQuotationApproval { get; set; } = false;

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

        // ===== 子科目自動產生設定 =====

        /// <summary>
        /// 新增客戶時自動建立應收帳款子科目
        /// </summary>
        [Display(Name = "自動建立客戶子科目")]
        public bool AutoCreateCustomerSubAccount { get; set; } = false;

        /// <summary>
        /// 新增廠商時自動建立應付帳款子科目
        /// </summary>
        [Display(Name = "自動建立廠商子科目")]
        public bool AutoCreateSupplierSubAccount { get; set; } = false;

        /// <summary>
        /// 新增商品時自動建立存貨子科目
        /// </summary>
        [Display(Name = "自動建立商品子科目")]
        public bool AutoCreateProductSubAccount { get; set; } = false;

        /// <summary>
        /// 應收帳款統制科目代碼（客戶子科目的父層），預設 1191
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "應收帳款統制科目代碼")]
        public string CustomerSubAccountParentCode { get; set; } = "1191";

        /// <summary>
        /// 應付帳款統制科目代碼（廠商子科目的父層），預設 2171
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "應付帳款統制科目代碼")]
        public string SupplierSubAccountParentCode { get; set; } = "2171";

        /// <summary>
        /// 商品存貨統制科目代碼（商品子科目的父層），預設 1231
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "商品存貨統制科目代碼")]
        public string ProductSubAccountParentCode { get; set; } = "1231";

        /// <summary>
        /// 子科目代碼格式：Sequential（流水號）或 EntityCode（實體代碼）
        /// </summary>
        [Display(Name = "子科目代碼格式")]
        public SubAccountCodeFormat SubAccountCodeFormat { get; set; } = SubAccountCodeFormat.Sequential;
    }
}
