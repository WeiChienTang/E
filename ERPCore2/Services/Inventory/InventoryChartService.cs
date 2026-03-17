using ERPCore2.Data.Context;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Inventory;

public class InventoryChartService : IInventoryChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public InventoryChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依庫存金額排行 Top N（CurrentStock × AverageCost）</summary>
    public async Task<List<ChartDataItem>> GetTopProductsByStockValueAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from s in context.InventoryStocks
            where s.Status != EntityStatus.Deleted
            join p in context.Products on s.ProductId equals p.Id
            select new
            {
                ProductName = p.Name ?? "未知品項",
                s.WeightedAverageCost,
                s.TotalCurrentStock
            }
        ).ToListAsync();

        return raw
            .Where(x => x.TotalCurrentStock > 0)
            .GroupBy(x => x.ProductName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.TotalCurrentStock * (x.WeightedAverageCost ?? 0))
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>依庫存數量排行 Top N</summary>
    public async Task<List<ChartDataItem>> GetTopProductsByStockQuantityAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from s in context.InventoryStocks
            where s.Status != EntityStatus.Deleted
            join p in context.Products on s.ProductId equals p.Id
            select new
            {
                ProductName = p.Name ?? "未知品項",
                s.TotalCurrentStock
            }
        ).ToListAsync();

        return raw
            .Where(x => x.TotalCurrentStock > 0)
            .GroupBy(x => x.ProductName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.TotalCurrentStock)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>依倉庫統計庫存金額分布</summary>
    public async Task<List<ChartDataItem>> GetStockValueByWarehouseAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from d in context.InventoryStockDetails
            where d.Status != EntityStatus.Deleted && d.CurrentStock > 0
            join w in context.Warehouses on d.WarehouseId equals w.Id
            select new
            {
                WarehouseName = w.Name ?? "未知倉庫",
                d.CurrentStock,
                d.AverageCost
            }
        ).ToListAsync();

        return raw
            .GroupBy(x => x.WarehouseName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.CurrentStock * (x.AverageCost ?? 0))
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>低於安全庫存品項（CurrentStock < MinStockLevel，且 MinStockLevel 有設定）</summary>
    public async Task<List<ChartDataItem>> GetLowStockProductsAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from d in context.InventoryStockDetails
            where d.Status != EntityStatus.Deleted
                  && d.MinStockLevel.HasValue
                  && d.CurrentStock < d.MinStockLevel.Value
            join s in context.InventoryStocks on d.InventoryStockId equals s.Id
            join p in context.Products on s.ProductId equals p.Id
            select new
            {
                ProductName  = p.Name ?? "未知品項",
                d.CurrentStock,
                MinStockLevel = d.MinStockLevel!.Value
            }
        ).ToListAsync();

        return raw
            .GroupBy(x => x.ProductName)
            .Select(g => new ChartDataItem
            {
                Label = g.Key,
                Value = g.Sum(x => x.CurrentStock)  // 現有庫存量（低於安全量）
            })
            .OrderBy(x => x.Value)  // 越少越危險，由低到高
            .ToList();
    }

    /// <summary>近 N 天庫存異動類型分布</summary>
    public async Task<List<ChartDataItem>> GetTransactionTypeDistributionAsync(int days = 30)
    {
        using var context = await _factory.CreateDbContextAsync();

        var since = DateTime.Now.AddDays(-days);

        var raw = await context.InventoryTransactions
            .Where(t => t.Status != EntityStatus.Deleted && t.TransactionDate >= since)
            .ToListAsync();

        var typeLabels = new Dictionary<InventoryTransactionTypeEnum, string>
        {
            [InventoryTransactionTypeEnum.OpeningBalance]        = "期初庫存",
            [InventoryTransactionTypeEnum.Purchase]              = "進貨",
            [InventoryTransactionTypeEnum.Sale]                  = "銷貨",
            [InventoryTransactionTypeEnum.Return]                = "進貨退出",
            [InventoryTransactionTypeEnum.SalesReturn]           = "銷貨退回",
            [InventoryTransactionTypeEnum.Adjustment]            = "調整",
            [InventoryTransactionTypeEnum.Transfer]              = "轉倉",
            [InventoryTransactionTypeEnum.StockTaking]           = "盤點",
            [InventoryTransactionTypeEnum.ProductionConsumption] = "生產投料",
            [InventoryTransactionTypeEnum.ProductionCompletion]  = "生產完工",
            [InventoryTransactionTypeEnum.Scrap]                 = "報廢",
            [InventoryTransactionTypeEnum.MaterialIssue]         = "領料",
            [InventoryTransactionTypeEnum.MaterialReturn]        = "領料退回",
            [InventoryTransactionTypeEnum.WasteReceiving]        = "磅秤收料"
        };

        return raw
            .GroupBy(t => t.TransactionType)
            .Select(g => new ChartDataItem
            {
                Label = typeLabels.TryGetValue(g.Key, out var label) ? label : g.Key.ToString(),
                Value = g.Count()
            })
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    /// <summary>取得庫存統計摘要</summary>
    public async Task<InventoryChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var today = DateTime.Today;
        var expiryThreshold = today.AddDays(30);
        var since30 = DateTime.Now.AddDays(-30);

        var stocks = await context.InventoryStocks
            .Where(s => s.Status != EntityStatus.Deleted)
            .ToListAsync();

        var stockDetails = await context.InventoryStockDetails
            .Where(d => d.Status != EntityStatus.Deleted)
            .ToListAsync();

        var lowStockCount = stockDetails.Count(d =>
            d.MinStockLevel.HasValue && d.CurrentStock < d.MinStockLevel.Value);

        var expiringCount = stockDetails.Count(d =>
            d.ExpiryDate.HasValue && d.ExpiryDate.Value >= today && d.ExpiryDate.Value <= expiryThreshold && d.CurrentStock > 0);

        var warehouseCount = await context.Warehouses
            .CountAsync(w => w.Status != EntityStatus.Deleted);

        var transactions30 = await context.InventoryTransactions
            .CountAsync(t => t.Status != EntityStatus.Deleted && t.TransactionDate >= since30);

        var totalValue = stockDetails.Sum(d => d.CurrentStock * (d.AverageCost ?? 0));

        return new InventoryChartSummary
        {
            TotalProductsWithStock = stocks.Count(s => s.TotalCurrentStock > 0),
            TotalStockValue        = totalValue,
            LowStockCount          = lowStockCount,
            ExpiringStockCount     = expiringCount,
            WarehouseCount         = warehouseCount,
            TransactionsLast30Days = transactions30
        };
    }

    // ===== Drill-down 明細查詢 =====

    public async Task<List<ChartDetailItem>> GetStockDetailsByWarehouseAsync(string warehouseLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from d in context.InventoryStockDetails
            where d.Status != EntityStatus.Deleted && d.CurrentStock > 0
            join w in context.Warehouses on d.WarehouseId equals w.Id
            where (w.Name ?? "") == warehouseLabel
            join s in context.InventoryStocks on d.InventoryStockId equals s.Id
            join p in context.Products on s.ProductId equals p.Id
            orderby d.CurrentStock descending
            select new
            {
                s.Id,
                ProductName = p.Name ?? "未知品項",
                ProductCode = p.Code ?? "",
                d.CurrentStock,
                d.AverageCost
            }
        ).Take(50).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.ProductName,
            SubLabel = $"{x.CurrentStock:N2} 件"
        }).ToList();
    }

    public async Task<List<ChartDetailItem>> GetLowStockDetailsAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from d in context.InventoryStockDetails
            where d.Status != EntityStatus.Deleted
                  && d.MinStockLevel.HasValue
                  && d.CurrentStock < d.MinStockLevel.Value
            join s in context.InventoryStocks on d.InventoryStockId equals s.Id
            join p in context.Products on s.ProductId equals p.Id
            orderby d.CurrentStock ascending
            select new
            {
                s.Id,
                ProductName   = p.Name ?? "未知品項",
                d.CurrentStock,
                MinStockLevel = d.MinStockLevel!.Value
            }
        ).Take(50).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.ProductName,
            SubLabel = $"現有 {x.CurrentStock:N2} / 安全 {x.MinStockLevel:N2}"
        }).ToList();
    }

    public async Task<List<ChartDetailItem>> GetTransactionDetailsByTypeAsync(string typeLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var typeMap = new Dictionary<string, InventoryTransactionTypeEnum>
        {
            ["期初庫存"] = InventoryTransactionTypeEnum.OpeningBalance,
            ["進貨"]     = InventoryTransactionTypeEnum.Purchase,
            ["銷貨"]     = InventoryTransactionTypeEnum.Sale,
            ["進貨退出"] = InventoryTransactionTypeEnum.Return,
            ["銷貨退回"] = InventoryTransactionTypeEnum.SalesReturn,
            ["調整"]     = InventoryTransactionTypeEnum.Adjustment,
            ["轉倉"]     = InventoryTransactionTypeEnum.Transfer,
            ["盤點"]     = InventoryTransactionTypeEnum.StockTaking,
            ["生產投料"] = InventoryTransactionTypeEnum.ProductionConsumption,
            ["生產完工"] = InventoryTransactionTypeEnum.ProductionCompletion,
            ["報廢"]     = InventoryTransactionTypeEnum.Scrap,
            ["領料"]     = InventoryTransactionTypeEnum.MaterialIssue,
            ["領料退回"] = InventoryTransactionTypeEnum.MaterialReturn,
            ["磅秤收料"] = InventoryTransactionTypeEnum.WasteReceiving
        };

        if (!typeMap.TryGetValue(typeLabel, out var transType))
            return new List<ChartDetailItem>();

        var since30 = DateTime.Now.AddDays(-30);

        return await context.InventoryTransactions
            .Where(t => t.Status != EntityStatus.Deleted
                        && t.TransactionType == transType
                        && t.TransactionDate >= since30)
            .OrderByDescending(t => t.TransactionDate)
            .Take(50)
            .Select(t => new ChartDetailItem
            {
                Id       = t.Id,
                Name     = t.TransactionDate.ToString("yyyy/MM/dd"),
                SubLabel = t.TransactionNumber
            })
            .ToListAsync();
    }
}
