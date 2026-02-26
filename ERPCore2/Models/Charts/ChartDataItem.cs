namespace ERPCore2.Models.Charts;

public class ChartDataItem
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class CustomerChartSummary
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int InactiveCustomers { get; set; }
    public int CustomersThisMonth { get; set; }
    public decimal? AverageCreditLimit { get; set; }
}
