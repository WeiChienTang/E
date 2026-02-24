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

    // ===== 審核流程開關預設值 =====

    /// <summary>
    /// 預設：報價單審核關閉
    /// </summary>
    public const bool DefaultEnableQuotationApproval = false;

    /// <summary>
    /// 預設：採購單審核關閉
    /// </summary>
    public const bool DefaultEnablePurchaseOrderApproval = false;

    /// <summary>
    /// 預設：進貨單審核關閉
    /// </summary>
    public const bool DefaultEnablePurchaseReceivingApproval = false;

    /// <summary>
    /// 預設：進貨退回審核關閉
    /// </summary>
    public const bool DefaultEnablePurchaseReturnApproval = false;

    /// <summary>
    /// 預設：銷貨單審核關閉
    /// </summary>
    public const bool DefaultEnableSalesOrderApproval = false;

    /// <summary>
    /// 預設：銷貨退回審核關閉
    /// </summary>
    public const bool DefaultEnableSalesReturnApproval = false;

    /// <summary>
    /// 預設：庫存調撥審核關閉
    /// </summary>
    public const bool DefaultEnableInventoryTransferApproval = false;

    // ===== 子科目自動產生預設值 =====

    /// <summary>預設：不自動建立客戶子科目</summary>
    public const bool DefaultAutoCreateCustomerSubAccount = false;

    /// <summary>預設：不自動建立廠商子科目</summary>
    public const bool DefaultAutoCreateSupplierSubAccount = false;

    /// <summary>預設：不自動建立商品子科目</summary>
    public const bool DefaultAutoCreateProductSubAccount = false;

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

    /// <summary>預設商品存貨統制科目代碼</summary>
    public const string DefaultProductSubAccountParentCode = "1231";

    /// <summary>預設子科目代碼格式：流水號</summary>
    public const SubAccountCodeFormat DefaultSubAccountCodeFormat = SubAccountCodeFormat.Sequential;

    /// <summary>
    /// 將系統參數實體的所有業務欄位重置為預設值
    /// 保留 Id、CreatedAt、CreatedBy 等基礎欄位不變
    /// </summary>
    /// <param name="parameter">要重置的系統參數實體</param>
    public static void ApplyDefaults(Data.Entities.SystemParameter parameter)
    {
        parameter.TaxRate = DefaultTaxRate;
        parameter.Remarks = DefaultRemarks;
        parameter.EnableQuotationApproval = DefaultEnableQuotationApproval;
        parameter.EnablePurchaseOrderApproval = DefaultEnablePurchaseOrderApproval;
        parameter.EnablePurchaseReceivingApproval = DefaultEnablePurchaseReceivingApproval;
        parameter.EnablePurchaseReturnApproval = DefaultEnablePurchaseReturnApproval;
        parameter.EnableSalesOrderApproval = DefaultEnableSalesOrderApproval;
        parameter.EnableSalesReturnApproval = DefaultEnableSalesReturnApproval;
        parameter.EnableInventoryTransferApproval = DefaultEnableInventoryTransferApproval;
        parameter.AutoCreateCustomerSubAccount = DefaultAutoCreateCustomerSubAccount;
        parameter.AutoCreateSupplierSubAccount = DefaultAutoCreateSupplierSubAccount;
        parameter.AutoCreateProductSubAccount = DefaultAutoCreateProductSubAccount;
        parameter.CustomerSubAccountParentCode = DefaultCustomerSubAccountParentCode;
        parameter.CustomerNoteSubAccountParentCode = DefaultCustomerNoteSubAccountParentCode;
        parameter.CustomerReturnSubAccountParentCode = DefaultCustomerReturnSubAccountParentCode;
        parameter.CustomerAdvanceSubAccountParentCode = DefaultCustomerAdvanceSubAccountParentCode;
        parameter.SupplierSubAccountParentCode = DefaultSupplierSubAccountParentCode;
        parameter.SupplierNoteSubAccountParentCode = DefaultSupplierNoteSubAccountParentCode;
        parameter.SupplierReturnSubAccountParentCode = DefaultSupplierReturnSubAccountParentCode;
        parameter.SupplierAdvanceSubAccountParentCode = DefaultSupplierAdvanceSubAccountParentCode;
        parameter.ProductSubAccountParentCode = DefaultProductSubAccountParentCode;
        parameter.SubAccountCodeFormat = DefaultSubAccountCodeFormat;
    }
}
