namespace ERPCore2.Data.Navigation;

/// <summary>
/// 代碼自動產生預設配置 - 定義所有模組的出廠預設值
/// 作為 Single Source of Truth，供 Seeder、Service、UI 共用
/// </summary>
public static class CodeSettingDefaults
{
    /// <summary>
    /// 所有模組的預設代碼設定
    /// (ModuleKey, ModuleDisplayName, Prefix, FormatTemplate, IsAutoCode)
    /// </summary>
    public static readonly (string ModuleKey, string DisplayName, string Prefix, string FormatTemplate, bool IsAutoCode)[] DefaultModules =
    {
        ("Customer",            "客戶",       "CUS",  "{Prefix}-{Year:yy}{Seq:4}",           true),
        ("Supplier",            "供應商",     "SUP",  "{Prefix}-{Year:yy}{Seq:4}",           true),
        ("Employee",            "員工",       "EMP",  "{Prefix}-{Seq:5}",                     true),
        ("Product",             "產品",       "PRD",  "{Prefix}-{Seq:5}",                     true),
        ("Quotation",           "報價單",     "QT",   "{Prefix}-{Year:yy}{Month:MM}{Seq:3}",  true),
        ("PurchaseOrder",       "採購單",     "PO",   "{Prefix}-{Year:yy}{Month:MM}{Seq:3}",  true),
        ("PurchaseReceiving",   "進貨單",     "PR",   "{Prefix}-{Year:yy}{Month:MM}{Seq:3}",  true),
        ("PurchaseReturn",      "採購退貨單", "PRT",  "{Prefix}-{Year:yy}{Month:MM}{Seq:3}",  true),
        ("SalesOrder",          "銷貨單",     "SO",   "{Prefix}-{Year:yy}{Month:MM}{Seq:3}",  true),
        ("SalesReturn",         "銷貨退貨單", "SRT",  "{Prefix}-{Year:yy}{Month:MM}{Seq:3}",  true),
        ("InventoryTransfer",   "庫存調撥單", "IT",   "{Prefix}-{Year:yy}{Month:MM}{Seq:3}",  true),
        ("WasteRecord",         "磅秤記錄",   "WR",   "{Prefix}-{Year:yy}{Month:MM}{Seq:3}",  true),
        ("GovernmentAgency",    "公家機關",   "GA",   "{Prefix}-{Year:yy}{Seq:4}",            true),
    };
}
