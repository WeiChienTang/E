using ERPCore2.Models.Charts;
using ERPCore2.Services.Customers;
using ERPCore2.Services.Employees;
using ERPCore2.Services.Inventory;
using ERPCore2.Services.Products;
using ERPCore2.Services.Purchase;
using ERPCore2.Services.Sales;
using ERPCore2.Services.Suppliers;

namespace ERPCore2.Data.Charts;

/// <summary>全域圖表登記表 — 彙總各模組的圖表定義</summary>
public static class ChartRegistry
{
    private static readonly List<ChartDefinition> _definitions = new();
    private static bool _initialized = false;
    private static readonly object _lock = new();

    public static void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_lock)
        {
            if (_initialized) return;
            Initialize();
            _initialized = true;
        }
    }

    private static void Initialize()
    {
        CustomerChartDefinitions.Register(_definitions);
        SupplierChartDefinitions.Register(_definitions);
        EmployeeChartDefinitions.Register(_definitions);
        PurchaseChartDefinitions.Register(_definitions);
        SalesChartDefinitions.Register(_definitions);
        InventoryChartDefinitions.Register(_definitions);
        ProductChartDefinitions.Register(_definitions);
    }

    /// <summary>取得指定分類的圖表定義（依 SortOrder 排序）</summary>
    public static List<ChartDefinition> GetByCategory(string category)
    {
        EnsureInitialized();
        return _definitions
            .Where(d => d.Category == category)
            .OrderBy(d => d.SortOrder)
            .ToList();
    }
}
