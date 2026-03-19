namespace ERPCore2.Models.Charts;

public class ProductChartSummary
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int ProductsWithSuppliers { get; set; }
    public int ProductsWithStock { get; set; }
    public int AddedThisMonth { get; set; }
}
