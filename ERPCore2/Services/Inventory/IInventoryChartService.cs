using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Inventory;

public interface IInventoryChartService
{
    /// <summary>依庫存金額排行 Top N（CurrentStock × AverageCost）</summary>
    Task<List<ChartDataItem>> GetTopProductsByStockValueAsync(int top = 10);

    /// <summary>依庫存數量排行 Top N</summary>
    Task<List<ChartDataItem>> GetTopProductsByStockQuantityAsync(int top = 10);

    /// <summary>依倉庫統計庫存金額分布</summary>
    Task<List<ChartDataItem>> GetStockValueByWarehouseAsync();

    /// <summary>低於安全庫存的商品清單（CurrentStock < MinStockLevel）</summary>
    Task<List<ChartDataItem>> GetLowStockProductsAsync();

    /// <summary>近 30 天庫存異動類型分布</summary>
    Task<List<ChartDataItem>> GetTransactionTypeDistributionAsync(int days = 30);

    /// <summary>取得庫存統計摘要</summary>
    Task<InventoryChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====
    Task<List<ChartDetailItem>> GetStockDetailsByWarehouseAsync(string warehouseLabel);
    Task<List<ChartDetailItem>> GetLowStockDetailsAsync(string label);
    Task<List<ChartDetailItem>> GetTransactionDetailsByTypeAsync(string typeLabel);
}
