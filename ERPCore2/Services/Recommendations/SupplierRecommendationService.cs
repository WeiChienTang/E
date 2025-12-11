using ERPCore2.Data.Context;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 供應商推薦服務實作
    /// 混合商品-供應商綁定資料與採購歷史資料，提供智能推薦
    /// </summary>
    public class SupplierRecommendationService : ISupplierRecommendationService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<SupplierRecommendationService>? _logger;

        public SupplierRecommendationService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<SupplierRecommendationService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// 取得商品的供應商推薦清單（混合綁定資料與歷史資料）
        /// </summary>
        public async Task<List<SupplierRecommendation>> GetRecommendedSuppliersAsync(int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                // 1. 取得綁定的供應商資料
                var bindingData = await context.ProductSuppliers
                    .Where(ps => ps.ProductId == productId && ps.Status == EntityStatus.Active)
                    .Include(ps => ps.Supplier)
                    .Select(ps => new
                    {
                        ps.SupplierId,
                        SupplierName = ps.Supplier!.CompanyName,
                        SupplierCode = ps.Supplier.Code ?? "",
                        ps.SupplierProductCode,
                        ps.IsPreferred,
                        ps.Priority,
                        ps.LastPurchasePrice,
                        ps.LastPurchaseDate,
                        ps.LeadTimeDays,
                        ps.Remarks
                    })
                    .ToListAsync();

                // 2. 取得採購歷史資料（所有供應商，不限時間）
                var historyData = await context.PurchaseOrders
                    .Where(po => po.Status == EntityStatus.Active)
                    .SelectMany(po => po.PurchaseOrderDetails
                        .Where(pod => pod.ProductId == productId)
                        .Select(pod => new
                        {
                            po.SupplierId,
                            pod.UnitPrice,
                            po.OrderDate,
                            SupplierName = po.Supplier!.CompanyName,
                            SupplierCode = po.Supplier.Code ?? ""
                        }))
                    .GroupBy(x => x.SupplierId)
                    .Select(g => new
                    {
                        SupplierId = g.Key,
                        SupplierName = g.First().SupplierName,
                        SupplierCode = g.First().SupplierCode,
                        PurchaseCount = g.Count(),
                        LastPurchasePrice = g.OrderByDescending(x => x.OrderDate).First().UnitPrice,
                        LastPurchaseDate = g.Max(x => x.OrderDate),
                        AveragePrice = g.Average(x => x.UnitPrice),
                        LowestPrice = g.Min(x => x.UnitPrice),
                        HighestPrice = g.Max(x => x.UnitPrice)
                    })
                    .ToListAsync();

                // 3. 合併資料
                var recommendations = new List<SupplierRecommendation>();

                // 3.1 先加入綁定的供應商（優先顯示）
                foreach (var binding in bindingData)
                {
                    var history = historyData.FirstOrDefault(h => h.SupplierId == binding.SupplierId);

                    recommendations.Add(new SupplierRecommendation
                    {
                        SupplierId = binding.SupplierId,
                        SupplierName = binding.SupplierName,
                        SupplierCode = binding.SupplierCode,
                        SupplierProductCode = binding.SupplierProductCode,
                        IsPreferred = binding.IsPreferred,
                        Priority = binding.Priority,
                        LastPurchasePrice = binding.LastPurchasePrice ?? history?.LastPurchasePrice,
                        LastPurchaseDate = binding.LastPurchaseDate ?? history?.LastPurchaseDate,
                        PurchaseCount = history?.PurchaseCount ?? 0,
                        AveragePrice = history?.AveragePrice,
                        LowestPrice = history?.LowestPrice,
                        HighestPrice = history?.HighestPrice,
                        LeadTimeDays = binding.LeadTimeDays,
                        Remarks = binding.Remarks,
                        RecommendationSource = history != null ? "Both" : "Preferred"
                    });
                }

                // 3.2 加入未綁定但有採購歷史的供應商
                var bindingSupplierIds = bindingData.Select(b => b.SupplierId).ToList();
                var historyOnlySuppliers = historyData.Where(h => !bindingSupplierIds.Contains(h.SupplierId));

                foreach (var history in historyOnlySuppliers)
                {
                    recommendations.Add(new SupplierRecommendation
                    {
                        SupplierId = history.SupplierId,
                        SupplierName = history.SupplierName,
                        SupplierCode = history.SupplierCode,
                        IsPreferred = false,
                        Priority = 999,
                        LastPurchasePrice = history.LastPurchasePrice,
                        LastPurchaseDate = history.LastPurchaseDate,
                        PurchaseCount = history.PurchaseCount,
                        AveragePrice = history.AveragePrice,
                        LowestPrice = history.LowestPrice,
                        HighestPrice = history.HighestPrice,
                        RecommendationSource = "History"
                    });
                }

                // 4. 排序：常用供應商優先（依 Priority），其他依最近採購日期
                return recommendations
                    .OrderByDescending(r => r.IsPreferred)
                    .ThenBy(r => r.Priority)
                    .ThenByDescending(r => r.LastPurchaseDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetRecommendedSuppliersAsync), GetType(), _logger,
                    new { ProductId = productId });
                throw;
            }
        }

        /// <summary>
        /// 取得指定商品和供應商的最近採購資訊
        /// </summary>
        public async Task<(decimal Price, DateTime PurchaseDate)?> GetLastPurchasePriceAsync(int supplierId, int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var lastPurchase = await context.PurchaseOrders
                    .Where(po => po.SupplierId == supplierId && po.Status == EntityStatus.Active)
                    .SelectMany(po => po.PurchaseOrderDetails
                        .Where(pod => pod.ProductId == productId)
                        .Select(pod => new
                        {
                            pod.UnitPrice,
                            po.OrderDate
                        }))
                    .OrderByDescending(x => x.OrderDate)
                    .FirstOrDefaultAsync();

                return lastPurchase != null
                    ? (lastPurchase.UnitPrice, lastPurchase.OrderDate)
                    : null;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLastPurchasePriceAsync), GetType(), _logger,
                    new { SupplierId = supplierId, ProductId = productId });
                return null;  // 不拋出例外，避免影響主流程
            }
        }
    }
}
