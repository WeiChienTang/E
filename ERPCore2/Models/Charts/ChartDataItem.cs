namespace ERPCore2.Models.Charts;

public class ChartDataItem
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

/// <summary>圖表 Drill-down 明細項目</summary>
public class ChartDetailItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SubLabel { get; set; }
}

public class CustomerChartSummary
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public int BlacklistedCustomers { get; set; }
    public int CustomersThisMonth { get; set; }
    public decimal? AverageCreditLimit { get; set; }
}

public class SupplierChartSummary
{
    public int TotalSuppliers { get; set; }
    public int ActiveSuppliers { get; set; }
    public int InactiveSuppliers { get; set; }
    public int SuspendedSuppliers { get; set; }
    public int SuppliersThisMonth { get; set; }
    public decimal TotalPayable { get; set; }
}

public class EmployeeChartSummary
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int ProbationEmployees { get; set; }
    public int HiredThisMonth { get; set; }
    public int ResignedThisMonth { get; set; }
    public int ExpiringLicenses { get; set; }
}

public class PurchaseChartSummary
{
    public int TotalOrdersThisMonth { get; set; }
    public int PendingApprovalOrders { get; set; }
    public int ApprovedOrders { get; set; }
    public decimal ThisMonthReceivingAmount { get; set; }
    public decimal ThisMonthReturnAmount { get; set; }
    public int TotalReceivingsThisMonth { get; set; }
}

public class SalesChartSummary
{
    public int TotalDeliveriesThisMonth { get; set; }
    public decimal ThisMonthDeliveryAmount { get; set; }
    public decimal ThisMonthReturnAmount { get; set; }
    public int TotalOrdersThisMonth { get; set; }
    public int PendingApprovalDeliveries { get; set; }
    public decimal YearToDateDeliveryAmount { get; set; }
}

public class InventoryChartSummary
{
    public int TotalProductsWithStock { get; set; }
    public decimal TotalStockValue { get; set; }
    public int LowStockCount { get; set; }
    public int ExpiringStockCount { get; set; }
    public int WarehouseCount { get; set; }
    public int TransactionsLast30Days { get; set; }
}
