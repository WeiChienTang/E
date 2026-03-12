namespace ERPCore2.Models.Charts;

public static class ChartIds
{
    public const string CustomerByAccountManager   = "CU001";
    public const string CustomerByPaymentMethod    = "CU002";
    public const string CustomerMonthlyTrend       = "CU003";
    public const string CustomerByStatus           = "CU004";
    public const string CustomerByCreditLimit      = "CU005";
    public const string CustomerTopSalesByAmount   = "CU006";
    public const string CustomerMonthlySalesTrend  = "CU007";
    public const string CustomerByCurrentBalance   = "CU008";
    public const string CustomerTopReturnsByAmount = "CU009";

    public const string SupplierByStatus            = "SU001";
    public const string SupplierByType              = "SU002";
    public const string SupplierByPaymentMethod     = "SU003";
    public const string SupplierMonthlyTrend        = "SU004";
    public const string SupplierTopPurchaseByAmount = "SU005";
    public const string SupplierMonthlyPurchaseTrend = "SU006";
    public const string SupplierByCurrentPayable    = "SU007";
    public const string SupplierTopReturnsByAmount  = "SU008";

    public const string EmployeeByDepartment        = "EM001";
    public const string EmployeeByType              = "EM002";
    public const string EmployeeByStatus            = "EM003";
    public const string EmployeeByGender            = "EM004";
    public const string EmployeeMonthlyHireTrend    = "EM005";
    public const string EmployeeBySeniority         = "EM006";
    public const string EmployeeTopTrainingHours    = "EM007";

    public const string PurchaseTopProductByAmount  = "PO001";
    public const string PurchaseMonthlyTrend        = "PO002";
    public const string PurchaseOrderApprovalStatus = "PO003";
    public const string PurchaseReturnByReason      = "PO004";
    public const string PurchaseMonthlyReturnTrend  = "PO005";

    public const string SalesTopProductByAmount     = "SA001";
    public const string SalesTopEmployeeByAmount    = "SA002";
    public const string SalesMonthlyTrend           = "SA003";
    public const string SalesReturnByReason         = "SA004";
    public const string SalesMonthlyReturnTrend     = "SA005";

    public const string InventoryTopByValue         = "IN001";
    public const string InventoryTopByQuantity      = "IN002";
    public const string InventoryByWarehouse        = "IN003";
    public const string InventoryLowStock           = "IN004";
    public const string InventoryTransactionTypes   = "IN005";
}
