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

    public const string PurchaseTopItemByAmount  = "PO001";
    public const string PurchaseMonthlyTrend        = "PO002";
    public const string PurchaseOrderApprovalStatus = "PO003";
    public const string PurchaseReturnByReason      = "PO004";
    public const string PurchaseMonthlyReturnTrend  = "PO005";

    public const string SalesTopItemByAmount       = "SA001";
    public const string SalesTopEmployeeByAmount      = "SA002";
    public const string SalesMonthlyTrend             = "SA003";
    public const string SalesReturnByReason           = "SA004";
    public const string SalesMonthlyReturnTrend       = "SA005";
    public const string SalesMonthlyAchievementRate   = "SA006";
    public const string SalesAnnualTargetByPerson     = "SA007";

    public const string InventoryTopByValue         = "IN001";
    public const string InventoryTopByQuantity      = "IN002";
    public const string InventoryByWarehouse        = "IN003";
    public const string InventoryLowStock           = "IN004";
    public const string InventoryTransactionTypes   = "IN005";

    public const string ItemByCategory              = "PR001";
    public const string ItemTopSalesByQuantity      = "PR002";
    public const string ItemTopSalesByAmount        = "PR003";
    public const string ItemMonthlySalesTrend       = "PR004";
    public const string ItemBySupplierCount         = "PR005";
    public const string ItemByStandardCostRange     = "PR006";

    // Production Management (MO prefix)
    public const string ProductionByStatus              = "MO001";
    public const string ProductionTopItemsByQuantity    = "MO002";
    public const string ProductionMonthlyTrend          = "MO003";
    public const string ProductionByEmployee            = "MO004";
    public const string ProductionCompletionRateByItem  = "MO005";
    public const string ProductionComponentUsage        = "MO006";

    // Financial Management (FM prefix)
    public const string FinancialSetoffByType           = "FM001";
    public const string FinancialMonthlySetoffTrend     = "FM002";
    public const string FinancialTopSetoffByAmount      = "FM003";
    public const string FinancialJournalByType          = "FM004";
    public const string FinancialMonthlyJournalTrend    = "FM005";
    public const string FinancialJournalByStatus        = "FM006";

    // Payroll (PY prefix)
    public const string PayrollMonthlyTrend             = "PY001";
    public const string PayrollGrossByDepartment        = "PY002";
    public const string PayrollTopEarners               = "PY003";
    public const string PayrollRecordStatus             = "PY004";
    public const string PayrollOvertimeByEmployee       = "PY005";

    // Scale Management (SC prefix)
    public const string ScaleWeightByItem               = "SC001";
    public const string ScaleMonthlyWeightTrend         = "SC002";
    public const string ScaleNetAmountByCustomer        = "SC003";
    public const string ScaleMonthlyRevenueTrend        = "SC004";
    public const string ScaleRecordCountByMonth         = "SC005";

    // Vehicle Management (VH prefix)
    public const string VehicleMaintenanceByType        = "VH001";
    public const string VehicleMonthlyCostTrend         = "VH002";
    public const string VehicleCostByVehicle            = "VH003";
    public const string VehicleByOwnershipType          = "VH004";
    public const string VehicleInsuranceExpiry          = "VH005";
}
