using ERPCore2.Models.Charts;

namespace ERPCore2.Services.Items;

public interface IItemChartService
{
    /// <summary>依品項分類統計品項數量</summary>
    Task<List<ChartDataItem>> GetItemsByCategoryAsync();

    /// <summary>銷售數量排行 Top N（依出貨數量）</summary>
    Task<List<ChartDataItem>> GetTopItemsBySalesQuantityAsync(int top = 10);

    /// <summary>銷售金額排行 Top N（依出貨含稅小計）</summary>
    Task<List<ChartDataItem>> GetTopItemsBySalesAmountAsync(int top = 10);

    /// <summary>每月銷售收入趨勢（近 N 個月）</summary>
    Task<List<ChartDataItem>> GetMonthlySalesTrendAsync(int months = 12);

    /// <summary>依供應商報價品項數分布</summary>
    Task<List<ChartDataItem>> GetItemsBySupplierCountAsync();

    /// <summary>依標準成本分段統計品項數量</summary>
    Task<List<ChartDataItem>> GetItemsByStandardCostRangeAsync();

    /// <summary>取得品項基本統計摘要</summary>
    Task<ItemChartSummary> GetSummaryAsync();

    // ===== Drill-down 明細查詢 =====

    /// <summary>依品項分類 Drill-down：顯示該分類下所有品項</summary>
    Task<List<ChartDetailItem>> GetItemDetailsByCategoryAsync(string categoryLabel);

    /// <summary>銷售數量排行 Drill-down：顯示該品項最近出貨明細</summary>
    Task<List<ChartDetailItem>> GetTopItemSalesQuantityDetailsAsync(string productLabel);

    /// <summary>銷售金額排行 Drill-down：顯示該品項最近出貨明細</summary>
    Task<List<ChartDetailItem>> GetTopItemSalesAmountDetailsAsync(string productLabel);

    /// <summary>依供應商 Drill-down：顯示該供應商報價的品項清單</summary>
    Task<List<ChartDetailItem>> GetItemDetailsBySupplierAsync(string supplierLabel);

    /// <summary>依標準成本分段 Drill-down：顯示該區間的品項清單</summary>
    Task<List<ChartDetailItem>> GetItemDetailsByStandardCostRangeAsync(string label);
}
