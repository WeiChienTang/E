using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        // ===== 審核模式設定 =====
        // false = 系統自動審核（儲存時自動核准）
        // true  = 人工審核（需人工按核准鍵）
        // DB 欄位名維持原名（EnableXxxApproval）以避免 Migration，語意已變更

        /// <summary>
        /// 報價單是否使用人工審核（false=系統自動審核，true=人工審核）
        /// </summary>
        [Column("EnableQuotationApproval")]
        [Display(Name = "報價單審核方式")]
        public bool QuotationManualApproval { get; set; } = false;

        /// <summary>
        /// 採購訂單是否使用人工審核（false=系統自動審核，true=人工審核）
        /// </summary>
        [Column("EnablePurchaseOrderApproval")]
        [Display(Name = "採購訂單審核方式")]
        public bool PurchaseOrderManualApproval { get; set; } = false;

        /// <summary>
        /// 進貨單是否使用人工審核（false=系統自動審核，true=人工審核）
        /// </summary>
        [Column("EnablePurchaseReceivingApproval")]
        [Display(Name = "進貨單審核方式")]
        public bool PurchaseReceivingManualApproval { get; set; } = false;

        /// <summary>
        /// 進貨退回是否使用人工審核（false=系統自動審核，true=人工審核）
        /// </summary>
        [Column("EnablePurchaseReturnApproval")]
        [Display(Name = "進貨退回審核方式")]
        public bool PurchaseReturnManualApproval { get; set; } = false;

        /// <summary>
        /// 銷貨訂單是否使用人工審核（false=系統自動審核，true=人工審核）
        /// </summary>
        [Column("EnableSalesOrderApproval")]
        [Display(Name = "銷貨訂單審核方式")]
        public bool SalesOrderManualApproval { get; set; } = false;

        /// <summary>
        /// 銷貨退回是否使用人工審核（false=系統自動審核，true=人工審核）
        /// </summary>
        [Column("EnableSalesReturnApproval")]
        [Display(Name = "銷貨退回審核方式")]
        public bool SalesReturnManualApproval { get; set; } = false;

        /// <summary>
        /// 出貨單是否使用人工審核（false=系統自動審核，true=人工審核）
        /// </summary>
        [Column("EnableSalesDeliveryApproval")]
        [Display(Name = "出貨單審核方式")]
        public bool SalesDeliveryManualApproval { get; set; } = false;

        /// <summary>
        /// 庫存調撥是否使用人工審核（false=系統自動審核，true=人工審核）
        /// </summary>
        [Column("EnableInventoryTransferApproval")]
        [Display(Name = "庫存調撥審核方式")]
        public bool InventoryTransferManualApproval { get; set; } = false;

        /// <summary>
        /// 是否隱藏所有模組的審核資訊欄位（核准人員、時間、狀態等）
        /// false=顯示（預設），true=隱藏
        /// </summary>
        [Display(Name = "隱藏審核資訊欄位")]
        public bool HideApprovalInfoSection { get; set; } = false;

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
        /// 新增品項時自動建立存貨子科目
        /// </summary>
        [Display(Name = "自動建立品項子科目")]
        public bool AutoCreateItemSubAccount { get; set; } = false;

        /// <summary>
        /// 應收帳款統制科目代碼（客戶子科目的父層），預設 1191
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "應收帳款統制科目代碼")]
        public string CustomerSubAccountParentCode { get; set; } = "1191";

        /// <summary>
        /// 應收票據統制科目代碼（客戶票據子科目的父層），預設 1131
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "應收票據統制科目代碼")]
        public string CustomerNoteSubAccountParentCode { get; set; } = "1131";

        /// <summary>
        /// 銷貨退回統制科目代碼（客戶退回子科目的父層），預設 4191
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "銷貨退回統制科目代碼")]
        public string CustomerReturnSubAccountParentCode { get; set; } = "4191";

        /// <summary>
        /// 預收款項統制科目代碼（客戶預收子科目的父層），預設 2163
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "預收款項統制科目代碼")]
        public string CustomerAdvanceSubAccountParentCode { get; set; } = "2163";

        /// <summary>
        /// 應付帳款統制科目代碼（廠商子科目的父層），預設 2171
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "應付帳款統制科目代碼")]
        public string SupplierSubAccountParentCode { get; set; } = "2171";

        /// <summary>
        /// 應付票據統制科目代碼（廠商票據子科目的父層），預設 2131
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "應付票據統制科目代碼")]
        public string SupplierNoteSubAccountParentCode { get; set; } = "2131";

        /// <summary>
        /// 進貨退出統制科目代碼（廠商退回子科目的父層），預設 5111
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "進貨退出統制科目代碼")]
        public string SupplierReturnSubAccountParentCode { get; set; } = "5111";

        /// <summary>
        /// 預付款項統制科目代碼（廠商預付子科目的父層），預設 1161
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "預付款項統制科目代碼")]
        public string SupplierAdvanceSubAccountParentCode { get; set; } = "1161";

        /// <summary>
        /// 品項存貨統制科目代碼（品項子科目的父層），預設 1231
        /// </summary>
        [MaxLength(20)]
        [Display(Name = "品項存貨統制科目代碼")]
        public string ItemSubAccountParentCode { get; set; } = "1231";

        /// <summary>
        /// 子科目代碼格式：Sequential（流水號）或 EntityCode（實體代碼）
        /// </summary>
        [Display(Name = "子科目代碼格式")]
        public SubAccountCodeFormat SubAccountCodeFormat { get; set; } = SubAccountCodeFormat.EntityCode;

        // ===== 薪資系統設定 =====

        /// <summary>每月發薪日（1-31），例：5 = 每月5日發薪</summary>
        [Display(Name = "發薪日")]
        [Range(1, 31, ErrorMessage = "發薪日必須在 1 到 31 之間")]
        public int PayrollPayDay { get; set; } = 5;

        /// <summary>每月薪資結算截止日（1-31），例：25 = 每月25日結算當月出勤</summary>
        [Display(Name = "薪資結算截止日")]
        [Range(1, 31, ErrorMessage = "結算截止日必須在 1 到 31 之間")]
        public int PayrollCutoffDay { get; set; } = 25;

        /// <summary>月薪計算除數（預設30）。0 = 依當月實際天數</summary>
        [Display(Name = "月薪計算除數")]
        [Range(0, 31)]
        public int SalaryMonthDivisor { get; set; } = 30;

        /// <summary>加班計時單位（分鐘），例：30 = 每30分鐘計一次加班</summary>
        [Display(Name = "加班計時單位（分鐘）")]
        [Range(1, 60)]
        public int OvertimeRoundingUnit { get; set; } = 30;

        /// <summary>遲到寬限分鐘數，例：5 = 遲到5分鐘以內不罰</summary>
        [Display(Name = "遲到寬限分鐘")]
        [Range(0, 60)]
        public int LateTolerance { get; set; } = 0;

        // ── 薪資會計科目代碼（Phase 4 使用，空白時不過帳）──────────────

        /// <summary>薪資費用科目代碼（如 6111）</summary>
        [MaxLength(20)]
        [Display(Name = "薪資費用科目")]
        public string? PayrollExpenseAccountCode { get; set; }

        /// <summary>加班費科目代碼（如 6112）</summary>
        [MaxLength(20)]
        [Display(Name = "加班費科目")]
        public string? OvertimeExpenseAccountCode { get; set; }

        /// <summary>勞保費用科目代碼—雇主負擔（如 6121）</summary>
        [MaxLength(20)]
        [Display(Name = "勞保費用科目（雇主）")]
        public string? LaborInsuranceExpenseAccountCode { get; set; }

        /// <summary>健保費用科目代碼—雇主負擔（如 6122）</summary>
        [MaxLength(20)]
        [Display(Name = "健保費用科目（雇主）")]
        public string? HealthInsuranceExpenseAccountCode { get; set; }

        /// <summary>勞退提繳費用科目代碼（如 6131）</summary>
        [MaxLength(20)]
        [Display(Name = "勞退提繳費用科目")]
        public string? RetirementExpenseAccountCode { get; set; }

        /// <summary>應付薪資科目代碼—負債（如 2191）</summary>
        [MaxLength(20)]
        [Display(Name = "應付薪資科目")]
        public string? AccruedPayrollAccountCode { get; set; }

        /// <summary>代扣勞保費科目代碼—負債（如 2192）</summary>
        [MaxLength(20)]
        [Display(Name = "代扣勞保費科目")]
        public string? WithholdingLaborInsuranceAccountCode { get; set; }

        /// <summary>代扣健保費科目代碼—負債（如 2193）</summary>
        [MaxLength(20)]
        [Display(Name = "代扣健保費科目")]
        public string? WithholdingHealthInsuranceAccountCode { get; set; }

        /// <summary>代扣所得稅科目代碼—負債（如 2194）</summary>
        [MaxLength(20)]
        [Display(Name = "代扣所得稅科目")]
        public string? WithholdingTaxAccountCode { get; set; }

        /// <summary>代扣勞退自提科目代碼—負債（如 2195）</summary>
        [MaxLength(20)]
        [Display(Name = "代扣勞退自提科目")]
        public string? VoluntaryRetirementAccountCode { get; set; }
    }
}
