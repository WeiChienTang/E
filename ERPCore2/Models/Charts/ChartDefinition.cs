using ApexCharts;

namespace ERPCore2.Models.Charts;

/// <summary>圖表定義模型</summary>
public class ChartDefinition
{
    public string ChartId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public SeriesType DefaultSeriesType { get; set; } = SeriesType.Bar;
    public List<SeriesType> AllowedSeriesTypes { get; set; } = new();

    /// <summary>資料取得委派，由 IServiceProvider 解析服務</summary>
    public Func<IServiceProvider, Task<List<ChartDataItem>>> DataFetcher { get; set; } = null!;
}

/// <summary>圖表分類常數</summary>
public static class ChartCategory
{
    public const string Customer = "Customer";
    public const string Supplier = "Supplier";
    public const string Employee = "Employee";
    public const string Product  = "Product";
    public const string Sales    = "Sales";
}

/// <summary>SeriesType 中文名稱與 Bootstrap Icon 對照表</summary>
public static class ChartSeriesTypeInfo
{
    private static readonly Dictionary<SeriesType, (string Label, string Icon)> _map = new()
    {
        [SeriesType.Bar]       = ("長條圖",   "bi bi-bar-chart-fill"),
        [SeriesType.Pie]       = ("圓餅圖",   "bi bi-pie-chart-fill"),
        [SeriesType.Donut]     = ("甜甜圈圖", "bi bi-circle-half"),
        [SeriesType.Line]      = ("折線圖",   "bi bi-graph-up"),
        [SeriesType.Area]      = ("面積圖",   "bi bi-graph-up-arrow"),
        [SeriesType.Treemap]   = ("樹狀圖",   "bi bi-grid-fill"),
        [SeriesType.PolarArea] = ("極區圖",   "bi bi-bullseye"),
        [SeriesType.Radar]     = ("雷達圖",   "bi bi-hexagon-fill"),
        [SeriesType.RadialBar] = ("儀表圖",   "bi bi-speedometer2"),
    };

    public static string GetLabel(SeriesType type) =>
        _map.TryGetValue(type, out var info) ? info.Label : type.ToString();

    public static string GetIcon(SeriesType type) =>
        _map.TryGetValue(type, out var info) ? info.Icon : "bi bi-bar-chart";
}
