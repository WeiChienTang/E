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
        private readonly IItemCompositionDetailService _productCompositionDetailService;

        public SalesOrderCompositionDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            IItemCompositionDetailService productCompositionDetailService,
            ILogger<GenericManagementService<SalesOrderCompositionDetail>> logger,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _productCompositionDetailService = productCompositionDetailService;
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public SalesOrderCompositionDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            IItemCompositionDetailService productCompositionDetailService) : base(contextFactory)
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
                .Include(x => x.ComponentItem)
                .Include(x => x.Unit)
                .Where(x => x.SalesOrderDetailId == salesOrderDetailId)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        /// <summary>
        /// 從品項物料清單複製 BOM 資料到銷貨訂單（使用最新的配方）
        /// </summary>
        public async Task<List<SalesOrderCompositionDetail>> CopyFromItemCompositionAsync(
            int salesOrderDetailId, int productId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // 取得品項的物料清單資料
            var productCompositions = await context.ItemCompositionDetails
                .Include(p => p.ComponentItem)
                    .ThenInclude(cp => cp.ProductionUnit)
                .Include(p => p.Unit)
                .Where(p => p.ItemCompositionId == context.ItemCompositions
                    .Where(pc => pc.ParentItemId == productId)
                    .Select(pc => pc.Id)
                    .FirstOrDefault())
                .ToListAsync();

            // 轉換為銷貨訂單組成明細（使用組件品項的製程單位）
            return productCompositions.Select(pc => new SalesOrderCompositionDetail
            {
                SalesOrderDetailId = salesOrderDetailId,
                ComponentItemId = pc.ComponentItemId,
                ComponentItem = pc.ComponentItem,
                Quantity = pc.Quantity,
                UnitId = pc.ComponentItem?.ProductionUnitId ?? pc.ComponentItem?.UnitId ?? pc.UnitId,
                Unit = pc.ComponentItem?.ProductionUnit ?? pc.Unit,
                ComponentCost = pc.ComponentCost,
                Status = EntityStatus.Active
            }).ToList();
        }

        /// <summary>
        /// 從指定的品項配方複製 BOM 資料到銷貨訂單（直接複製配方明細，不遞迴展開）
        /// </summary>
        public async Task<List<SalesOrderCompositionDetail>> CopyFromCompositionAsync(
            int salesOrderDetailId, int compositionId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // 查詢指定的品項配方
            var productComposition = await context.ItemCompositions
                .Include(x => x.CompositionDetails)
                    .ThenInclude(d => d.ComponentItem)
                        .ThenInclude(p => p.ProductionUnit)
                .Include(x => x.CompositionDetails)
                    .ThenInclude(d => d.ComponentItem)
                        .ThenInclude(p => p.Unit)
                .Include(x => x.CompositionDetails)
                    .ThenInclude(d => d.Unit)
                .FirstOrDefaultAsync(x => x.Id == compositionId);

            if (productComposition == null || !productComposition.CompositionDetails.Any())
            {
                return new List<SalesOrderCompositionDetail>();
            }

            // 直接複製組合明細（使用組件品項的製程單位）
            var result = new List<SalesOrderCompositionDetail>();

            foreach (var detail in productComposition.CompositionDetails)
            {
                var effectiveUnitId = detail.ComponentItem?.ProductionUnitId ?? detail.ComponentItem?.UnitId ?? detail.UnitId;
                result.Add(new SalesOrderCompositionDetail
                {
                    SalesOrderDetailId = salesOrderDetailId,
                    ComponentItemId = detail.ComponentItemId,
                    ComponentItem = detail.ComponentItem!,
                    Quantity = detail.Quantity,
                    UnitId = effectiveUnitId,
                    Unit = detail.ComponentItem?.ProductionUnit ?? detail.Unit,
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

            foreach (var detail in compositionDetails.Where(x => x.ComponentItemId > 0 && x.Quantity > 0))
            {
                detail.SalesOrderDetailId = salesOrderDetailId;
                
                // 🔑 清除導航屬性，避免 EF Core 嘗試插入已存在的關聯實體
                detail.ComponentItem = null!;
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
                    .Include(x => x.ComponentItem)
                    .Include(x => x.Unit)
                    .Include(x => x.SalesOrderDetail)
                    .Where(x => (x.ComponentItem.Name != null && x.ComponentItem.Name.Contains(keyword)) || 
                               (x.ComponentItem.Code != null && x.ComponentItem.Code.Contains(keyword)))
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

            if (entity.ComponentItemId <= 0)
            {
                return ServiceResult.Failure("組件品項ID無效");
            }

            if (entity.Quantity <= 0)
            {
                return ServiceResult.Failure("數量必須大於0");
            }

            return await Task.FromResult(ServiceResult.Success());
        }
    }
}
