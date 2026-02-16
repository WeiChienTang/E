namespace ERPCore2.Models.Reports;

/// <summary>
/// 報表 ID 常數 - 集中管理所有報表識別碼
/// 使用常數而非硬編碼字串，提供編譯時期檢查和 IDE 支援
/// </summary>
public static class ReportIds
{
    // ==================== 客戶報表 (AR) ====================
    
    /// <summary>應收帳款報表</summary>
    public const string AccountsReceivable = "AR001";
    
    /// <summary>客戶對帳單</summary>
    public const string CustomerStatement = "AR002";
    
    /// <summary>客戶銷售分析</summary>
    public const string CustomerSalesAnalysis = "AR003";
    
    /// <summary>客戶交易明細</summary>
    public const string CustomerTransaction = "AR004";
    
    // ==================== 廠商報表 (AP) ====================
    
    /// <summary>應付帳款報表</summary>
    public const string AccountsPayable = "AP001";
    
    /// <summary>廠商對帳單</summary>
    public const string SupplierStatement = "AP002";
    
    /// <summary>廠商進貨分析</summary>
    public const string SupplierPurchaseAnalysis = "AP003";
    
    // ==================== 採購報表 (PO) ====================
    
    /// <summary>採購單</summary>
    public const string PurchaseOrder = "PO001";
    
    /// <summary>進貨單</summary>
    public const string PurchaseReceiving = "PO002";
    
    /// <summary>進貨退出單</summary>
    public const string PurchaseReturn = "PO003";
    
    // ==================== 銷售報表 (SO) ====================
    
    /// <summary>報價單</summary>
    public const string Quotation = "SO001";
    
    /// <summary>銷貨單</summary>
    public const string SalesOrder = "SO002";
    
    /// <summary>銷貨訂單</summary>
    public const string SalesOrderDocument = "SO003";
    
    /// <summary>出貨單</summary>
    public const string SalesDelivery = "SO004";
    
    /// <summary>銷貨退回單</summary>
    public const string SalesReturn = "SO005";
    
    // ==================== 商品報表 (PD) ====================
    
    /// <summary>商品資料表</summary>
    public const string ProductList = "PD001";

    /// <summary>物料清單報表</summary>
    public const string BOMReport = "PD002";

    /// <summary>商品條碼標籤</summary>
    public const string ProductBarcode = "PD003";

    /// <summary>生產排程表</summary>
    public const string ProductionSchedule = "PD004";

    // ==================== 庫存報表 (IV) ====================
    
    /// <summary>庫存異動明細</summary>
    public const string InventoryTransaction = "IV001";
    
    /// <summary>庫存盤點表</summary>
    public const string InventoryCount = "IV002";
    
    /// <summary>庫存現況表</summary>
    public const string InventoryStatus = "IV003";
    
    // ==================== 人力報表 (HR) ====================

    /// <summary>員工名冊表</summary>
    public const string EmployeeRoster = "HR001";

    // ==================== 財務報表 (FN) ====================

    /// <summary>收款單</summary>
    public const string PaymentReceipt = "FN001";

    /// <summary>付款單</summary>
    public const string PaymentVoucher = "FN002";

    /// <summary>應收沖款單</summary>
    public const string AccountsReceivableSetoff = "FN003";

    /// <summary>應付沖款單</summary>
    public const string AccountsPayableSetoff = "FN004";
}
