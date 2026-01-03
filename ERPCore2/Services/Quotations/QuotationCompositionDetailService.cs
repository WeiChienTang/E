using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 報價單組合明細服務 - 管理報價單專屬的 BOM 組成
    /// </summary>
    public class QuotationCompositionDetailService : GenericManagementService<QuotationCompositionDetail>, IQuotationCompositionDetailService
    {
        private readonly IProductCompositionDetailService _productCompositionDetailService;

        public QuotationCompositionDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            IProductCompositionDetailService productCompositionDetailService,
            ILogger<GenericManagementService<QuotationCompositionDetail>> logger) : base(contextFactory, logger)
        {
            _productCompositionDetailService = productCompositionDetailService;
        }
        
        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public QuotationCompositionDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            IProductCompositionDetailService productCompositionDetailService) : base(contextFactory)
        {
            _productCompositionDetailService = productCompositionDetailService;
        }

        /// <summary>
        /// 取得指定報價單明細的所有組合明細
        /// </summary>
        public async Task<List<QuotationCompositionDetail>> GetByQuotationDetailIdAsync(int quotationDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.QuotationCompositionDetails
                    .Include(x => x.ComponentProduct)
                    .Include(x => x.Unit)
                    .Where(x => x.QuotationDetailId == quotationDetailId)
                    .OrderBy(x => x.ComponentProduct.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByQuotationDetailIdAsync), GetType(), _logger);
                return new List<QuotationCompositionDetail>();
            }
        }

        /// <summary>
        /// 從商品物料清單複製 BOM 到報價單明細（使用最新的配方）
        /// </summary>
        public async Task<List<QuotationCompositionDetail>> CopyFromProductCompositionAsync(int quotationDetailId, int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢該商品的商品物料清單（優先取用最新的一筆）
                var productComposition = await context.ProductCompositions
                    .Include(x => x.CompositionDetails)
                        .ThenInclude(d => d.ComponentProduct)
                    .Include(x => x.CompositionDetails)
                        .ThenInclude(d => d.Unit)
                    .Where(x => x.ParentProductId == productId)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (productComposition == null || !productComposition.CompositionDetails.Any())
                {
                    return new List<QuotationCompositionDetail>();
                }

                // 複製組合明細
                var quotationCompositionDetails = new List<QuotationCompositionDetail>();
                
                foreach (var detail in productComposition.CompositionDetails)
                {
                    quotationCompositionDetails.Add(new QuotationCompositionDetail
                    {
                        QuotationDetailId = quotationDetailId,
                        ComponentProductId = detail.ComponentProductId,
                        Quantity = detail.Quantity,
                        UnitId = detail.UnitId,
                        ComponentCost = detail.ComponentCost
                    });
                }

                return quotationCompositionDetails;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CopyFromProductCompositionAsync), GetType(), _logger);
                return new List<QuotationCompositionDetail>();
            }
        }

        /// <summary>
        /// 從指定的商品配方複製 BOM 到報價單明細
        /// </summary>
        public async Task<List<QuotationCompositionDetail>> CopyFromCompositionAsync(int quotationDetailId, int compositionId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 查詢指定的商品配方
                var productComposition = await context.ProductCompositions
                    .Include(x => x.CompositionDetails)
                        .ThenInclude(d => d.ComponentProduct)
                    .Include(x => x.CompositionDetails)
                        .ThenInclude(d => d.Unit)
                    .FirstOrDefaultAsync(x => x.Id == compositionId);

                if (productComposition == null || !productComposition.CompositionDetails.Any())
                {
                    return new List<QuotationCompositionDetail>();
                }

                // 複製組合明細
                var quotationCompositionDetails = new List<QuotationCompositionDetail>();
                
                foreach (var detail in productComposition.CompositionDetails)
                {
                    quotationCompositionDetails.Add(new QuotationCompositionDetail
                    {
                        QuotationDetailId = quotationDetailId,
                        ComponentProductId = detail.ComponentProductId,
                        Quantity = detail.Quantity,
                        UnitId = detail.UnitId,
                        ComponentCost = detail.ComponentCost
                    });
                }

                return quotationCompositionDetails;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CopyFromCompositionAsync), GetType(), _logger);
                return new List<QuotationCompositionDetail>();
            }
        }

        /// <summary>
        /// 檢查組件是否已存在於報價單明細的組合中
        /// </summary>
        public async Task<bool> IsComponentExistsAsync(int quotationDetailId, int componentProductId, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.QuotationCompositionDetails
                    .Where(x => x.QuotationDetailId == quotationDetailId && x.ComponentProductId == componentProductId);

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsComponentExistsAsync), GetType(), _logger);
                return false;
            }
        }

        /// <summary>
        /// 批次儲存報價單組合明細
        /// </summary>
        public async Task<List<QuotationCompositionDetail>> SaveBatchAsync(int quotationDetailId, List<QuotationCompositionDetail> compositionDetails)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 先刪除舊的組合明細
                await DeleteByQuotationDetailIdAsync(quotationDetailId);

                // 新增新的組合明細
                var result = new List<QuotationCompositionDetail>();

                foreach (var detail in compositionDetails.Where(x => x.ComponentProductId > 0 && x.Quantity > 0))
                {
                    detail.QuotationDetailId = quotationDetailId;
                    detail.Id = 0; // 確保是新增
                    
                    // 🔑 清除導航屬性，避免 EF Core 嘗試插入關聯實體
                    detail.ComponentProduct = null!;
                    detail.Unit = null!;
                    detail.QuotationDetail = null!;
                    
                    var saveResult = await CreateAsync(detail);
                    if (saveResult.IsSuccess && saveResult.Data != null)
                    {
                        result.Add(saveResult.Data);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SaveBatchAsync), GetType(), _logger);
                return new List<QuotationCompositionDetail>();
            }
        }

        /// <summary>
        /// 刪除指定報價單明細的所有組合明細
        /// </summary>
        public async Task DeleteByQuotationDetailIdAsync(int quotationDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var existingDetails = await context.QuotationCompositionDetails
                    .Where(x => x.QuotationDetailId == quotationDetailId)
                    .ToListAsync();

                if (existingDetails.Any())
                {
                    context.QuotationCompositionDetails.RemoveRange(existingDetails);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteByQuotationDetailIdAsync), GetType(), _logger);
            }
        }

        /// <summary>
        /// 搜尋組合明細
        /// </summary>
        public override async Task<List<QuotationCompositionDetail>> SearchAsync(string keyword)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                return await context.QuotationCompositionDetails
                    .Include(x => x.ComponentProduct)
                    .Include(x => x.Unit)
                    .Include(x => x.QuotationDetail)
                    .Where(x => (x.ComponentProduct.Name != null && x.ComponentProduct.Name.Contains(keyword)) || 
                               (x.ComponentProduct.Code != null && x.ComponentProduct.Code.Contains(keyword)))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<QuotationCompositionDetail>();
            }
        }

        /// <summary>
        /// 驗證組合明細
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(QuotationCompositionDetail entity)
        {
            if (entity.QuotationDetailId <= 0)
            {
                return ServiceResult.Failure("報價單明細ID無效");
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
