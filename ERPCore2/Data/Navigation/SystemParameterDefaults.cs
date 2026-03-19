using ERPCore2.Models.Enums;

namespace ERPCore2.Data.Navigation;

/// <summary>
/// 系統參數預設配置 - 定義所有系統參數的出廠預設值
/// 作為 Single Source of Truth，供 Seeder、Service、UI 共用
/// </summary>
public static class SystemParameterDefaults
{
    /// <summary>
    /// 預設稅率 (%)
    /// </summary>
    public const decimal DefaultTaxRate = 5.00m;

    /// <summary>
    /// 預設備註
    /// </summary>
    public const string DefaultRemarks = "系統預設稅率設定";

    // ===== 審核模式預設值（false=系統自動審核，true=人工審核）=====

    /// <summary>
    /// 預設：報價單使用系統自動審核
    /// </summary>
    public const bool DefaultQuotationManualApproval = false;

    /// <summary>
    /// 預設：採購訂單使用系統自動審核
    /// </summary>
    public const bool DefaultPurchaseOrderManualApproval = false;

    /// <summary>
    /// 預設：進貨單使用系統自動審核
    /// </summary>
    public const bool DefaultPurchaseReceivingManualApproval = false;

    /// <summary>
    /// 預設：進貨退回使用系統自動審核
    /// </summary>
    public const bool DefaultPurchaseReturnManualApproval = false;

    /// <summary>
    /// 預設：銷貨訂單使用系統自動審核
    /// </summary>
    public const bool DefaultSalesOrderManualApproval = false;

    /// <summary>
    /// 預設：銷貨退回使用系統自動審核
    /// </summary>
    public const bool DefaultSalesReturnManualApproval = false;

    /// <summary>
    /// 預設：出貨單使用系統自動審核
    /// </summary>
    public const bool DefaultSalesDeliveryManualApproval = false;

    /// <summary>
    /// 預設：庫存調撥使用系統自動審核
    /// </summary>
    public const bool DefaultInventoryTransferManualApproval = false;

    /// <summary>
    /// 預設：顯示審核資訊欄位（不隱藏）
    /// </summary>
    public const bool DefaultHideApprovalInfoSection = false;

    // ===== 子科目自動產生預設值 =====

    /// <summary>預設：不自動建立客戶子科目</summary>
    public const bool DefaultAutoCreateCustomerSubAccount = false;

    /// <summary>預設：不自動建立廠商子科目</summary>
    public const bool DefaultAutoCreateSupplierSubAccount = false;

    /// <summary>預設：不自動建立品項子科目</summary>
    public const bool DefaultAutoCreateItemSubAccount = false;

    /// <summary>預設應收帳款統制科目代碼</summary>
    public const string DefaultCustomerSubAccountParentCode = "1191";

    /// <summary>預設應收票據統制科目代碼</summary>
    public const string DefaultCustomerNoteSubAccountParentCode = "1131";

    /// <summary>預設銷貨退回統制科目代碼</summary>
    public const string DefaultCustomerReturnSubAccountParentCode = "4191";

    /// <summary>預設預收款項統制科目代碼</summary>
    public const string DefaultCustomerAdvanceSubAccountParentCode = "2163";

    /// <summary>預設應付帳款統制科目代碼</summary>
    public const string DefaultSupplierSubAccountParentCode = "2171";

    /// <summary>預設應付票據統制科目代碼</summary>
    public const string DefaultSupplierNoteSubAccountParentCode = "2131";

    /// <summary>預設進貨退出統制科目代碼</summary>
    public const string DefaultSupplierReturnSubAccountParentCode = "5111";

    /// <summary>預設預付款項統制科目代碼</summary>
    public const string DefaultSupplierAdvanceSubAccountParentCode = "1161";

    /// <summary>預設品項存貨統制科目代碼</summary>
    public const string DefaultItemSubAccountParentCode = "1231";

    /// <summary>預設子科目代碼格式：流水號</summary>
    public const SubAccountCodeFormat DefaultSubAccountCodeFormat = SubAccountCodeFormat.EntityCode;

    /// <summary>
    /// 將系統參數實體的所有業務欄位重置為預設值
    /// 保留 Id、CreatedAt、CreatedBy 等基礎欄位不變
    /// </summary>
    /// <param name="parameter">要重置的系統參數實體</param>
    public static void ApplyDefaults(Data.Entities.SystemParameter parameter)
    {
        parameter.TaxRate = DefaultTaxRate;
        parameter.Remarks = DefaultRemarks;
        parameter.QuotationManualApproval = DefaultQuotationManualApproval;
        parameter.PurchaseOrderManualApproval = DefaultPurchaseOrderManualApproval;
        parameter.PurchaseReceivingManualApproval = DefaultPurchaseReceivingManualApproval;
        parameter.PurchaseReturnManualApproval = DefaultPurchaseReturnManualApproval;
        parameter.SalesOrderManualApproval = DefaultSalesOrderManualApproval;
        parameter.SalesReturnManualApproval = DefaultSalesReturnManualApproval;
        parameter.SalesDeliveryManualApproval = DefaultSalesDeliveryManualApproval;
        parameter.InventoryTransferManualApproval = DefaultInventoryTransferManualApproval;
        parameter.HideApprovalInfoSection = DefaultHideApprovalInfoSection;
        parameter.AutoCreateCustomerSubAccount = DefaultAutoCreateCustomerSubAccount;
        parameter.AutoCreateSupplierSubAccount = DefaultAutoCreateSupplierSubAccount;
        parameter.AutoCreateItemSubAccount = DefaultAutoCreateItemSubAccount;
        parameter.CustomerSubAccountParentCode = DefaultCustomerSubAccountParentCode;
        parameter.CustomerNoteSubAccountParentCode = DefaultCustomerNoteSubAccountParentCode;
        parameter.CustomerReturnSubAccountParentCode = DefaultCustomerReturnSubAccountParentCode;
        parameter.CustomerAdvanceSubAccountParentCode = DefaultCustomerAdvanceSubAccountParentCode;
        parameter.SupplierSubAccountParentCode = DefaultSupplierSubAccountParentCode;
        parameter.SupplierNoteSubAccountParentCode = DefaultSupplierNoteSubAccountParentCode;
        parameter.SupplierReturnSubAccountParentCode = DefaultSupplierReturnSubAccountParentCode;
        parameter.SupplierAdvanceSubAccountParentCode = DefaultSupplierAdvanceSubAccountParentCode;
        parameter.ItemSubAccountParentCode = DefaultItemSubAccountParentCode;
        parameter.SubAccountCodeFormat = DefaultSubAccountCodeFormat;
    }
}
