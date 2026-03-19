namespace ERPCore2.Models.Charts;

public class ItemChartSummary
{
    public int TotalItems { get; set; }
    public int TotalCategories { get; set; }
    public int ItemsWithSuppliers { get; set; }
    public int ItemsWithStock { get; set; }
    public int AddedThisMonth { get; set; }
}
