using ERPCore2.Data.Context;
using ERPCore2.Models.Charts;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Products;

public class ProductChartService : IProductChartService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProductChartService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    /// <summary>依品項分類統計品項數量（未分類者歸入「未分類」）</summary>
    public async Task<List<ChartDataItem>> GetProductsByCategoryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var result = await (
            from p in context.Products
            where p.Status != EntityStatus.Deleted
            join c in context.ProductCategories on p.ProductCategoryId equals c.Id into cg
            from cat in cg.DefaultIfEmpty()
            select new { CategoryName = cat != null ? cat.Name : "未分類" }
        )
        .GroupBy(x => x.CategoryName)
        .Select(g => new { Label = g.Key ?? "未分類", Value = (decimal)g.Count() })
        .OrderByDescending(x => x.Value)
        .ToListAsync();

        return result.Select(x => new ChartDataItem { Label = x.Label, Value = x.Value }).ToList();
    }

    /// <summary>銷售數量排行 Top N（依出貨單明細合計出貨數量）</summary>
    public async Task<List<ChartDataItem>> GetTopProductsBySalesQuantityAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        // 先 ToListAsync，避免 EF Core GroupBy + JOIN 翻譯問題
        var rows = await (
            from sdd in context.SalesDeliveryDetails
            where sdd.Status != EntityStatus.Deleted
            join sd in context.SalesDeliveries on sdd.SalesDeliveryId equals sd.Id
            where sd.Status != EntityStatus.Deleted
            join p in context.Products on sdd.ProductId equals p.Id
            select new { p.Id, ProductName = p.Name ?? p.Code ?? $"ID:{p.Id}", sdd.DeliveryQuantity }
        ).ToListAsync();

        return rows
            .GroupBy(x => new { x.Id, x.ProductName })
            .Select(g => new ChartDataItem
            {
                Label = g.Key.ProductName,
                Value = g.Sum(x => x.DeliveryQuantity)
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>銷售金額排行 Top N（依出貨單明細合計含稅小計）</summary>
    public async Task<List<ChartDataItem>> GetTopProductsBySalesAmountAsync(int top = 10)
    {
        using var context = await _factory.CreateDbContextAsync();

        // 先 ToListAsync，避免 EF Core GroupBy + 計算欄位翻譯問題
        var rows = await (
            from sdd in context.SalesDeliveryDetails
            where sdd.Status != EntityStatus.Deleted
            join sd in context.SalesDeliveries on sdd.SalesDeliveryId equals sd.Id
            where sd.Status != EntityStatus.Deleted
            join p in context.Products on sdd.ProductId equals p.Id
            select new
            {
                p.Id,
                ProductName      = p.Name ?? p.Code ?? $"ID:{p.Id}",
                sdd.DeliveryQuantity,
                sdd.UnitPrice,
                sdd.DiscountPercentage,
                sdd.TaxRate
            }
        ).ToListAsync();

        return rows
            .GroupBy(x => new { x.Id, x.ProductName })
            .Select(g => new ChartDataItem
            {
                Label = g.Key.ProductName,
                Value = g.Sum(x =>
                {
                    var subtotal = x.DeliveryQuantity * x.UnitPrice * (1 - x.DiscountPercentage / 100);
                    return Math.Round(subtotal * (1 + (x.TaxRate ?? 0) / 100), 2);
                })
            })
            .OrderByDescending(x => x.Value)
            .Take(top)
            .ToList();
    }

    /// <summary>每月銷售收入趨勢（含稅，近 N 個月，依出貨日期）</summary>
    public async Task<List<ChartDataItem>> GetMonthlySalesTrendAsync(int months = 12)
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var startDate = new DateTime(now.AddMonths(-(months - 1)).Year, now.AddMonths(-(months - 1)).Month, 1);

        var rows = await (
            from sdd in context.SalesDeliveryDetails
            where sdd.Status != EntityStatus.Deleted
            join sd in context.SalesDeliveries on sdd.SalesDeliveryId equals sd.Id
            where sd.Status != EntityStatus.Deleted && sd.DeliveryDate >= startDate
            select new
            {
                sd.DeliveryDate,
                sdd.DeliveryQuantity,
                sdd.UnitPrice,
                sdd.DiscountPercentage,
                sdd.TaxRate
            }
        ).ToListAsync();

        var grouped = rows
            .GroupBy(x => new { x.DeliveryDate.Year, x.DeliveryDate.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => g.Sum(x =>
                {
                    var subtotal = x.DeliveryQuantity * x.UnitPrice * (1 - x.DiscountPercentage / 100);
                    return Math.Round(subtotal * (1 + (x.TaxRate ?? 0) / 100), 2);
                })
            );

        var result = new List<ChartDataItem>();
        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            grouped.TryGetValue((date.Year, date.Month), out var total);
            result.Add(new ChartDataItem
            {
                Label = date.ToString("yyyy/MM"),
                Value = total
            });
        }

        return result;
    }

    /// <summary>依供應商統計其報價品項數</summary>
    public async Task<List<ChartDataItem>> GetProductsBySupplierCountAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var result = await (
            from sp in context.SupplierPricings
            where sp.Status != EntityStatus.Deleted
            join s in context.Suppliers on sp.SupplierId equals s.Id
            where s.Status != EntityStatus.Deleted
            select new { SupplierName = s.CompanyName ?? s.Code ?? $"ID:{s.Id}", sp.ProductId }
        )
        .GroupBy(x => x.SupplierName)
        .Select(g => new { Label = g.Key, Value = (decimal)g.Select(x => x.ProductId).Distinct().Count() })
        .OrderByDescending(x => x.Value)
        .ToListAsync();

        return result.Select(x => new ChartDataItem { Label = x.Label, Value = x.Value }).ToList();
    }

    /// <summary>依標準成本分段統計品項數量</summary>
    public async Task<List<ChartDataItem>> GetProductsByStandardCostRangeAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var costs = await context.Products
            .Where(p => p.Status != EntityStatus.Deleted)
            .Select(p => p.StandardCost)
            .ToListAsync();

        int noCost = 0, low = 0, mid = 0, high = 0, veryHigh = 0;
        foreach (var cost in costs)
        {
            if (!cost.HasValue || cost == 0)
                noCost++;
            else if (cost <= 100)
                low++;
            else if (cost <= 1_000)
                mid++;
            else if (cost <= 10_000)
                high++;
            else
                veryHigh++;
        }

        return new List<ChartDataItem>
        {
            new() { Label = "未設定",         Value = noCost },
            new() { Label = "1~100",          Value = low },
            new() { Label = "101~1,000",      Value = mid },
            new() { Label = "1,001~10,000",   Value = high },
            new() { Label = "10,000以上",     Value = veryHigh }
        };
    }

    /// <summary>取得品項基本統計摘要</summary>
    public async Task<ProductChartSummary> GetSummaryAsync()
    {
        using var context = await _factory.CreateDbContextAsync();

        var now = DateTime.Now;
        var firstOfMonth = new DateTime(now.Year, now.Month, 1);

        var total = await context.Products.CountAsync(p => p.Status != EntityStatus.Deleted);
        var totalCategories = await context.ProductCategories.CountAsync(c => c.Status != EntityStatus.Deleted);
        var addedThisMonth = await context.Products.CountAsync(p => p.Status != EntityStatus.Deleted && p.CreatedAt >= firstOfMonth);

        var withSuppliers = await context.SupplierPricings
            .Where(sp => sp.Status != EntityStatus.Deleted)
            .Select(sp => sp.ProductId)
            .Distinct()
            .CountAsync();

        var withStock = await context.InventoryStocks
            .Where(s => s.Status != EntityStatus.Deleted && s.TotalCurrentStock > 0)
            .CountAsync();

        return new ProductChartSummary
        {
            TotalProducts        = total,
            TotalCategories      = totalCategories,
            ProductsWithSuppliers = withSuppliers,
            ProductsWithStock    = withStock,
            AddedThisMonth       = addedThisMonth
        };
    }

    // ===== Drill-down 明細查詢 =====

    /// <summary>依品項分類 Drill-down：顯示該分類下所有品項（代碼 + 名稱）</summary>
    public async Task<List<ChartDetailItem>> GetProductDetailsByCategoryAsync(string categoryLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        IQueryable<ERPCore2.Data.Entities.Product> query;
        if (categoryLabel == "未分類")
        {
            query = context.Products
                .Where(p => p.Status != EntityStatus.Deleted && p.ProductCategoryId == null);
        }
        else
        {
            query = from p in context.Products
                    where p.Status != EntityStatus.Deleted
                    join c in context.ProductCategories on p.ProductCategoryId equals c.Id
                    where (c.Name ?? "") == categoryLabel
                    select p;
        }

        return await query
            .OrderBy(p => p.Code)
            .Select(p => new ChartDetailItem
            {
                Id       = p.Id,
                Name     = p.Name ?? p.Code ?? $"ID:{p.Id}",
                SubLabel = p.Code
            })
            .ToListAsync();
    }

    /// <summary>銷售數量排行 Drill-down：顯示該品項最近 20 筆出貨明細（出貨日期 + 數量）</summary>
    public async Task<List<ChartDetailItem>> GetTopProductSalesQuantityDetailsAsync(string productLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from sdd in context.SalesDeliveryDetails
            where sdd.Status != EntityStatus.Deleted
            join sd in context.SalesDeliveries on sdd.SalesDeliveryId equals sd.Id
            where sd.Status != EntityStatus.Deleted
            join p in context.Products on sdd.ProductId equals p.Id
            where (p.Name ?? p.Code ?? $"ID:{p.Id}") == productLabel
            orderby sd.DeliveryDate descending
            select new { sd.Id, sd.DeliveryDate, sdd.DeliveryQuantity }
        ).Take(20).ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.DeliveryDate.ToString("yyyy/MM/dd"),
            SubLabel = $"{x.DeliveryQuantity:N2}"
        }).ToList();
    }

    /// <summary>銷售金額排行 Drill-down：顯示該品項最近 20 筆出貨明細（出貨日期 + 含稅金額）</summary>
    public async Task<List<ChartDetailItem>> GetTopProductSalesAmountDetailsAsync(string productLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from sdd in context.SalesDeliveryDetails
            where sdd.Status != EntityStatus.Deleted
            join sd in context.SalesDeliveries on sdd.SalesDeliveryId equals sd.Id
            where sd.Status != EntityStatus.Deleted
            join p in context.Products on sdd.ProductId equals p.Id
            where (p.Name ?? p.Code ?? $"ID:{p.Id}") == productLabel
            orderby sd.DeliveryDate descending
            select new { sd.Id, sd.DeliveryDate, sdd.DeliveryQuantity, sdd.UnitPrice, sdd.DiscountPercentage, sdd.TaxRate }
        ).Take(20).ToListAsync();

        return raw.Select(x =>
        {
            var subtotal = x.DeliveryQuantity * x.UnitPrice * (1 - x.DiscountPercentage / 100);
            var amount   = Math.Round(subtotal * (1 + (x.TaxRate ?? 0) / 100), 2);
            return new ChartDetailItem
            {
                Id       = x.Id,
                Name     = x.DeliveryDate.ToString("yyyy/MM/dd"),
                SubLabel = $"NT${amount:N0}"
            };
        }).ToList();
    }

    /// <summary>依供應商 Drill-down：顯示該供應商報價的品項清單（最多 50 筆）</summary>
    public async Task<List<ChartDetailItem>> GetProductDetailsBySupplierAsync(string supplierLabel)
    {
        using var context = await _factory.CreateDbContextAsync();

        var raw = await (
            from sp in context.SupplierPricings
            where sp.Status != EntityStatus.Deleted
            join s in context.Suppliers on sp.SupplierId equals s.Id
            where s.Status != EntityStatus.Deleted
                  && (s.CompanyName ?? s.Code ?? $"ID:{s.Id}") == supplierLabel
            join p in context.Products on sp.ProductId equals p.Id
            where p.Status != EntityStatus.Deleted
            orderby p.Code
            select new { p.Id, ProductName = p.Name ?? p.Code ?? $"ID:{p.Id}", ProductCode = p.Code ?? "", sp.PurchasePrice }
        ).Take(50).ToListAsync();

        return raw
            .GroupBy(x => new { x.Id, x.ProductName, x.ProductCode })
            .Select(g => new ChartDetailItem
            {
                Id       = g.Key.Id,
                Name     = g.Key.ProductName,
                SubLabel = $"NT${g.Min(x => x.PurchasePrice):N2}"
            })
            .ToList();
    }

    /// <summary>依標準成本分段 Drill-down：顯示該成本區間的品項清單</summary>
    public async Task<List<ChartDetailItem>> GetProductDetailsByStandardCostRangeAsync(string label)
    {
        using var context = await _factory.CreateDbContextAsync();

        IQueryable<ERPCore2.Data.Entities.Product> query = label switch
        {
            "未設定"       => context.Products.Where(p => p.Status != EntityStatus.Deleted && (!p.StandardCost.HasValue || p.StandardCost == 0)),
            "1~100"        => context.Products.Where(p => p.Status != EntityStatus.Deleted && p.StandardCost > 0 && p.StandardCost <= 100),
            "101~1,000"    => context.Products.Where(p => p.Status != EntityStatus.Deleted && p.StandardCost > 100 && p.StandardCost <= 1_000),
            "1,001~10,000" => context.Products.Where(p => p.Status != EntityStatus.Deleted && p.StandardCost > 1_000 && p.StandardCost <= 10_000),
            "10,000以上"   => context.Products.Where(p => p.Status != EntityStatus.Deleted && p.StandardCost > 10_000),
            _              => context.Products.Where(p => false)
        };

        var raw = await query
            .OrderBy(p => p.StandardCost)
            .Select(p => new { p.Id, p.Name, p.Code, p.StandardCost })
            .ToListAsync();

        return raw.Select(x => new ChartDetailItem
        {
            Id       = x.Id,
            Name     = x.Name ?? x.Code ?? $"ID:{x.Id}",
            SubLabel = x.StandardCost.HasValue ? $"NT${x.StandardCost.Value:N2}" : "未設定"
        }).ToList();
    }
}
