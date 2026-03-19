using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Products;

public interface IProductChartService
{
    /// <summary>依品項分類統計品項數量</summary>
    Task<List<ChartDataItem>> GetProductsByCategoryAsync();

    /// <summary>銷售數量排行 Top N（依出貨數量）</summary>
    Task<List<ChartDataItem>> GetTopProductsBySalesQuantityAsync(int top = 10);

    /// <summary>銷售金額排行 Top N（依出貨含稅小計）</summary>
    Task<List<ChartDataItem>> GetTopProductsBySalesAmountAsync(int top = 10);

    /// <summary>每月銷售收入趨勢（近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlySalesTrendAsync(int months = 12);

    /// <summary>依供應商報價品項數分布</summary>
    Task<List<ChartDataItem>> GetProductsBySupplierCountAsync();

    /// <summary>依標準成本分段統計品項數量</summary>
    Task<List<ChartDataItem>> GetProductsByStandardCostRangeAsync();

    /// <summary>取得品項基本統計摘要</summary>
    Task<ProductChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====

    /// <summary>依品項分類 Drill-down：顯示該分類下所有品項</summary>
    Task<List<ChartDetailItem>> GetProductDetailsByCategoryAsync(string categoryLabel);

    /// <summary>銷售數量排行 Drill-down：顯示該品項最近出貨明細</summary>
    Task<List<ChartDetailItem>> GetTopProductSalesQuantityDetailsAsync(string productLabel);

    /// <summary>銷售金額排行 Drill-down：顯示該品項最近出貨明細</summary>
    Task<List<ChartDetailItem>> GetTopProductSalesAmountDetailsAsync(string productLabel);

    /// <summary>依供應商 Drill-down：顯示該供應商報價的品項清單</summary>
    Task<List<ChartDetailItem>> GetProductDetailsBySupplierAsync(string supplierLabel);

    /// <summary>依標準成本分段 Drill-down：顯示該區間的品項清單</summary>
    Task<List<ChartDetailItem>> GetProductDetailsByStandardCostRangeAsync(string label);
}
