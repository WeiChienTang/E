using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨訂單組成明細服務實作
    /// </summary>
    public class SalesOrderCompositionDetailService : GenericManagementService<SalesOrderCompositionDetail>, ISalesOrderCompositionDetailService
    {
        private readonly IProductCompositionDetailService _productCompositionDetailService;

        public SalesOrderCompositionDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            IProductCompositionDetailService productCompositionDetailService,
            ILogger<GenericManagementService<SalesOrderCompositionDetail>> logger) : base(contextFactory, logger)
        {
            _productCompositionDetailService = productCompositionDetailService;
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public SalesOrderCompositionDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            IProductCompositionDetailService productCompositionDetailService) : base(contextFactory)
        {
            _productCompositionDetailService = productCompositionDetailService;
        }

        /// <summary>
        /// 取得指定銷貨訂單明細的組合明細
        /// </summary>
        public async Task<List<SalesOrderCompositionDetail>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.SalesOrderCompositionDetails
                .Include(x => x.ComponentProduct)
                .Include(x => x.Unit)
                .Where(x => x.SalesOrderDetailId == salesOrderDetailId)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        /// <summary>
        /// 從商品物料清單複製 BOM 資料到銷貨訂單（使用最新的配方）
        /// </summary>
        public async Task<List<SalesOrderCompositionDetail>> CopyFromProductCompositionAsync(
            int salesOrderDetailId, int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // 取得商品的物料清單資料
            var productCompositions = await context.ProductCompositionDetails
                .Include(p => p.ComponentProduct)
                    .ThenInclude(cp => cp.ProductionUnit)
                .Include(p => p.Unit)
                .Where(p => p.ProductCompositionId == context.ProductCompositions
                    .Where(pc => pc.ParentProductId == productId)
                    .Select(pc => pc.Id)
                    .FirstOrDefault())
                .ToListAsync();

            // 轉換為銷貨訂單組成明細（使用組件商品的製程單位）
            return productCompositions.Select(pc => new SalesOrderCompositionDetail
            {
                SalesOrderDetailId = salesOrderDetailId,
                ComponentProductId = pc.ComponentProductId,
                ComponentProduct = pc.ComponentProduct,
                Quantity = pc.Quantity,
                UnitId = pc.ComponentProduct?.ProductionUnitId ?? pc.ComponentProduct?.UnitId ?? pc.UnitId,
                Unit = pc.ComponentProduct?.ProductionUnit ?? pc.Unit,
                ComponentCost = pc.ComponentCost,
                Status = EntityStatus.Active
            }).ToList();
        }

        /// <summary>
        /// 從指定的商品配方複製 BOM 資料到銷貨訂單（直接複製配方明細，不遞迴展開）
        /// </summary>
        public async Task<List<SalesOrderCompositionDetail>> CopyFromCompositionAsync(
            int salesOrderDetailId, int compositionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // 查詢指定的商品配方
            var productComposition = await context.ProductCompositions
                .Include(x => x.CompositionDetails)
                    .ThenInclude(d => d.ComponentProduct)
                        .ThenInclude(p => p.ProductionUnit)
                .Include(x => x.CompositionDetails)
                    .ThenInclude(d => d.ComponentProduct)
                        .ThenInclude(p => p.Unit)
                .Include(x => x.CompositionDetails)
                    .ThenInclude(d => d.Unit)
                .FirstOrDefaultAsync(x => x.Id == compositionId);

            if (productComposition == null || !productComposition.CompositionDetails.Any())
            {
                return new List<SalesOrderCompositionDetail>();
            }

            // 直接複製組合明細（使用組件商品的製程單位）
            var result = new List<SalesOrderCompositionDetail>();

            foreach (var detail in productComposition.CompositionDetails)
            {
                var effectiveUnitId = detail.ComponentProduct?.ProductionUnitId ?? detail.ComponentProduct?.UnitId ?? detail.UnitId;
                result.Add(new SalesOrderCompositionDetail
                {
                    SalesOrderDetailId = salesOrderDetailId,
                    ComponentProductId = detail.ComponentProductId,
                    ComponentProduct = detail.ComponentProduct!,
                    Quantity = detail.Quantity,
                    UnitId = effectiveUnitId,
                    Unit = detail.ComponentProduct?.ProductionUnit ?? detail.Unit,
                    ComponentCost = detail.ComponentCost,
                    Status = EntityStatus.Active
                });
            }

            return result;
        }

        /// <summary>
        /// 批次儲存組合明細（新增、更新、刪除）
        /// </summary>
        public async Task SaveBatchAsync(
            int salesOrderDetailId, 
            List<SalesOrderCompositionDetail> compositionDetails)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // 取得現有資料（使用 AsNoTracking 避免追蹤衝突）
            var existingDetails = await context.SalesOrderCompositionDetails
                .AsNoTracking()
                .Where(x => x.SalesOrderDetailId == salesOrderDetailId)
                .ToListAsync();
            
            // 刪除不在新列表中的項目
            var toDelete = existingDetails
                .Where(e => !compositionDetails.Any(n => n.Id == e.Id && e.Id > 0))
                .ToList();
            
            if (toDelete.Any())
            {
                // 需要重新 Attach 才能刪除（因為用了 AsNoTracking）
                context.SalesOrderCompositionDetails.RemoveRange(toDelete);
            }
            
            // 新增或更新
            int addCount = 0;
            int updateCount = 0;

            foreach (var detail in compositionDetails.Where(x => x.ComponentProductId > 0 && x.Quantity > 0))
            {
                detail.SalesOrderDetailId = salesOrderDetailId;
                
                // 🔑 清除導航屬性，避免 EF Core 嘗試插入已存在的關聯實體
                detail.ComponentProduct = null!;
                detail.Unit = null;
                detail.SalesOrderDetail = null!;
                
                if (detail.Id == 0)
                {
                    detail.CreatedAt = DateTime.UtcNow;
                    context.SalesOrderCompositionDetails.Add(detail);
                    addCount++;
                }
                else
                {
                    // 檢查是否已被追蹤
                    var tracked = context.SalesOrderCompositionDetails.Local
                        .FirstOrDefault(e => e.Id == detail.Id);
                    
                    if (tracked != null)
                    {
                        // 更新已追蹤的實體
                        context.Entry(tracked).CurrentValues.SetValues(detail);
                        tracked.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        detail.UpdatedAt = DateTime.UtcNow;
                        context.SalesOrderCompositionDetails.Update(detail);
                    }
                    updateCount++;
                }
            }
            
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 刪除指定銷貨訂單明細的所有組合明細
        /// </summary>
        public async Task DeleteBySalesOrderDetailIdAsync(int salesOrderDetailId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var details = await context.SalesOrderCompositionDetails
                .Where(x => x.SalesOrderDetailId == salesOrderDetailId)
                .ToListAsync();
            
            context.SalesOrderCompositionDetails.RemoveRange(details);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 搜尋組合明細
        /// </summary>
        public override async Task<List<SalesOrderCompositionDetail>> SearchAsync(string keyword)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                return await context.SalesOrderCompositionDetails
                    .Include(x => x.ComponentProduct)
                    .Include(x => x.Unit)
                    .Include(x => x.SalesOrderDetail)
                    .Where(x => (x.ComponentProduct.Name != null && x.ComponentProduct.Name.Contains(keyword)) || 
                               (x.ComponentProduct.Code != null && x.ComponentProduct.Code.Contains(keyword)))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<SalesOrderCompositionDetail>();
            }
        }

        /// <summary>
        /// 驗證組合明細
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(SalesOrderCompositionDetail entity)
        {
            if (entity.SalesOrderDetailId <= 0)
            {
                return ServiceResult.Failure("銷貨訂單明細ID無效");
            }

            if (entity.ComponentProductId <= 0)
            {
                return ServiceResult.Failure("組件商品ID無效");
            }

            if (entity.Quantity <= 0)
            {
                return ServiceResult.Failure("數量必須大於0");
            }

            return await Task.FromResult(ServiceResult.Success());
        }
    }
}
