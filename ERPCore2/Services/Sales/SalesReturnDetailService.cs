using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨退回明細服務實作
    /// </summary>
    public class SalesReturnDetailService : GenericManagementService<SalesReturnDetail>, ISalesReturnDetailService
    {
        public SalesReturnDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public SalesReturnDetailService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<SalesReturnDetail>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<SalesReturnDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                        .ThenInclude(sr => sr.Customer)
                    .Include(srd => srd.Product)
                    .Include(srd => srd.SalesOrderDetail)
                    .AsQueryable()
                    .OrderBy(srd => srd.SalesReturnId)
                    .ThenBy(srd => srd.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new
                {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SalesReturnDetail>();
            }
        }

        public override async Task<SalesReturnDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                        .ThenInclude(sr => sr.Customer)
                    .Include(srd => srd.Product)
                    .Include(srd => srd.SalesOrderDetail)
                    .FirstOrDefaultAsync(srd => srd.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        public override async Task<List<SalesReturnDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var lowerSearchTerm = searchTerm.ToLower();

                return await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                        .ThenInclude(sr => sr.Customer)
                    .Include(srd => srd.Product)
                    .Where(srd => ((srd.Product != null && srd.Product.Code != null && srd.Product.Code.ToLower().Contains(lowerSearchTerm)) ||
                         (srd.Product != null && srd.Product.Name != null && srd.Product.Name.ToLower().Contains(lowerSearchTerm)) ||
                         (srd.SalesReturn != null && srd.SalesReturn.SalesReturnNumber != null && srd.SalesReturn.SalesReturnNumber.ToLower().Contains(lowerSearchTerm)) ||
                         (srd.SalesReturn != null && srd.SalesReturn.Customer != null && srd.SalesReturn.Customer.CompanyName != null && srd.SalesReturn.Customer.CompanyName.ToLower().Contains(lowerSearchTerm)) ||
                         (srd.Remarks != null && srd.Remarks.ToLower().Contains(lowerSearchTerm))))
                    .OrderBy(srd => srd.SalesReturnId)
                    .ThenBy(srd => srd.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new
                {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<SalesReturnDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SalesReturnDetail entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.SalesReturnId <= 0)
                    errors.Add("必須指定銷貨退回");

                if (entity.ProductId <= 0)
                    errors.Add("必須選擇產品");

                if (entity.ReturnQuantity <= 0)
                    errors.Add("退回數量必須大於零");

                if (entity.OriginalUnitPrice < 0)
                    errors.Add("原始單價不能為負數");

                if (entity.ReturnUnitPrice < 0)
                    errors.Add("退回單價不能為負數");

                if (entity.DiscountPercentage < 0 || entity.DiscountPercentage > 100)
                    errors.Add("折扣百分比必須在0-100之間");

                if (entity.ProcessedQuantity < 0)
                    errors.Add("已處理數量不能為負數");

                if (entity.ProcessedQuantity > entity.ReturnQuantity)
                    errors.Add("已處理數量不能超過退回數量");

                if (entity.RestockedQuantity < 0)
                    errors.Add("入庫數量不能為負數");

                if (entity.ScrapQuantity < 0)
                    errors.Add("報廢數量不能為負數");

                if (entity.RestockedQuantity + entity.ScrapQuantity > entity.ProcessedQuantity)
                    errors.Add("入庫數量加報廢數量不能超過已處理數量");

                // 驗證退回數量是否合理
                var quantityValidation = await ValidateReturnQuantityAsync(entity);
                if (!quantityValidation.IsSuccess)
                    errors.Add(quantityValidation.ErrorMessage);

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    SalesReturnId = entity.SalesReturnId,
                    ProductId = entity.ProductId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<List<SalesReturnDetail>> GetBySalesReturnIdAsync(int salesReturnId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Include(srd => srd.Product)
                    .Include(srd => srd.SalesOrderDetail)
                    .Where(srd => srd.SalesReturnId == salesReturnId)
                    .OrderBy(srd => srd.Product.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesReturnIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySalesReturnIdAsync),
                    ServiceType = GetType().Name,
                    SalesReturnId = salesReturnId
                });
                return new List<SalesReturnDetail>();
            }
        }

        public async Task<List<SalesReturnDetail>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                        .ThenInclude(sr => sr.Customer)
                    .Include(srd => srd.Product)
                    .Where(srd => srd.ProductId == productId)
                    .OrderByDescending(srd => srd.SalesReturn.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId
                });
                return new List<SalesReturnDetail>();
            }
        }

        public async Task<List<SalesReturnDetail>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                        .ThenInclude(sr => sr.Customer)
                    .Include(srd => srd.Product)
                    .Include(srd => srd.SalesOrderDetail)
                    .Where(srd => srd.SalesOrderDetailId == salesOrderDetailId)
                    .OrderByDescending(srd => srd.SalesReturn.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderDetailIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySalesOrderDetailIdAsync),
                    ServiceType = GetType().Name,
                    SalesOrderDetailId = salesOrderDetailId
                });
                return new List<SalesReturnDetail>();
            }
        }



        public decimal CalculateSubtotal(SalesReturnDetail detail)
        {
            try
            {
                var subtotal = detail.ReturnQuantity * detail.ReturnUnitPrice;
                subtotal -= detail.DiscountAmount;
                return Math.Max(0, subtotal);
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(CalculateSubtotal), GetType(), _logger, new
                {
                    Method = nameof(CalculateSubtotal),
                    ServiceType = GetType().Name,
                    DetailId = detail.Id,
                    ReturnQuantity = detail.ReturnQuantity,
                    ReturnUnitPrice = detail.ReturnUnitPrice,
                    DiscountAmount = detail.DiscountAmount
                });
                return 0;
            }
        }

        public async Task<ServiceResult> UpdateProcessedQuantityAsync(int detailId, decimal processedQuantity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.SalesReturnDetails.FindAsync(detailId);

                if (detail == null)
                    return ServiceResult.Failure("找不到指定的明細記錄");

                if (processedQuantity < 0)
                    return ServiceResult.Failure("已處理數量不能為負數");

                if (processedQuantity > detail.ReturnQuantity)
                    return ServiceResult.Failure("已處理數量不能超過退回數量");

                detail.ProcessedQuantity = processedQuantity;
                detail.PendingQuantity = detail.ReturnQuantity - processedQuantity;
                detail.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateProcessedQuantityAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateProcessedQuantityAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId,
                    ProcessedQuantity = processedQuantity
                });
                return ServiceResult.Failure("更新已處理數量時發生錯誤");
            }
        }

        public async Task<ServiceResult> SetRestockInfoAsync(int detailId, decimal restockedQuantity, string? qualityCondition = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.SalesReturnDetails.FindAsync(detailId);

                if (detail == null)
                    return ServiceResult.Failure("找不到指定的明細記錄");

                if (restockedQuantity < 0)
                    return ServiceResult.Failure("入庫數量不能為負數");

                if (restockedQuantity + detail.ScrapQuantity > detail.ProcessedQuantity)
                    return ServiceResult.Failure("入庫數量加報廢數量不能超過已處理數量");

                detail.IsRestocked = restockedQuantity > 0;
                detail.RestockedQuantity = restockedQuantity;
                if (!string.IsNullOrWhiteSpace(qualityCondition))
                    detail.QualityCondition = qualityCondition;
                detail.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetRestockInfoAsync), GetType(), _logger, new
                {
                    Method = nameof(SetRestockInfoAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId,
                    RestockedQuantity = restockedQuantity,
                    QualityCondition = qualityCondition
                });
                return ServiceResult.Failure("設定入庫資訊時發生錯誤");
            }
        }

        public async Task<ServiceResult> SetScrapQuantityAsync(int detailId, decimal scrapQuantity, string? remarks = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.SalesReturnDetails.FindAsync(detailId);

                if (detail == null)
                    return ServiceResult.Failure("找不到指定的明細記錄");

                if (scrapQuantity < 0)
                    return ServiceResult.Failure("報廢數量不能為負數");

                if (detail.RestockedQuantity + scrapQuantity > detail.ProcessedQuantity)
                    return ServiceResult.Failure("入庫數量加報廢數量不能超過已處理數量");

                detail.ScrapQuantity = scrapQuantity;
                if (!string.IsNullOrWhiteSpace(remarks))
                    detail.Remarks = remarks;
                detail.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetScrapQuantityAsync), GetType(), _logger, new
                {
                    Method = nameof(SetScrapQuantityAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId,
                    ScrapQuantity = scrapQuantity,
                    Remarks = remarks
                });
                return ServiceResult.Failure("設定報廢數量時發生錯誤");
            }
        }

        public async Task<ServiceResult> UpdateDetailsAsync(List<SalesReturnDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                foreach (var detail in details)
                {
                    // 計算小計
                    detail.ReturnSubtotal = CalculateSubtotal(detail);
                    detail.PendingQuantity = detail.ReturnQuantity - detail.ProcessedQuantity;
                    detail.UpdatedAt = DateTime.UtcNow;

                    if (detail.Id == 0)
                        context.SalesReturnDetails.Add(detail);
                    else
                        context.SalesReturnDetails.Update(detail);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(UpdateDetailsAsync),
                    ServiceType = GetType().Name,
                    DetailCount = details.Count
                });
                return ServiceResult.Failure("批量更新明細時發生錯誤");
            }
        }

        public async Task<bool> CanDeleteDetailAsync(int detailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                    .FirstOrDefaultAsync(srd => srd.Id == detailId);

                if (detail == null)
                    return false;

                // 如果已經有處理數量，則不允許刪除
                return detail.ProcessedQuantity == 0;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteDetailAsync), GetType(), _logger, new
                {
                    Method = nameof(CanDeleteDetailAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId
                });
                return false;
            }
        }

        public async Task<SalesReturnDetailStatistics> GetDetailStatisticsAsync(int salesReturnId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.SalesReturnDetails
                    .Where(srd => srd.SalesReturnId == salesReturnId)
                    .ToListAsync();

                var statistics = new SalesReturnDetailStatistics
                {
                    TotalDetails = details.Count,
                    TotalReturnQuantity = details.Sum(d => d.ReturnQuantity),
                    TotalProcessedQuantity = details.Sum(d => d.ProcessedQuantity),
                    TotalPendingQuantity = details.Sum(d => d.PendingQuantity),
                    TotalRestockedQuantity = details.Sum(d => d.RestockedQuantity),
                    TotalScrapQuantity = details.Sum(d => d.ScrapQuantity),
                    TotalReturnAmount = details.Sum(d => d.ReturnSubtotal),
                    ProductCount = details.Select(d => d.ProductId).Distinct().Count(),
                    ProductReturnQuantities = details
                        .GroupBy(d => d.ProductId)
                        .ToDictionary(g => g.Key, g => g.Sum(d => d.ReturnQuantity))
                };

                return statistics;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDetailStatisticsAsync), GetType(), _logger, new
                {
                    Method = nameof(GetDetailStatisticsAsync),
                    ServiceType = GetType().Name,
                    SalesReturnId = salesReturnId
                });
                return new SalesReturnDetailStatistics();
            }
        }

        public async Task<ServiceResult> ValidateReturnQuantityAsync(SalesReturnDetail detail)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();



                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateReturnQuantityAsync), GetType(), _logger, new
                {
                    Method = nameof(ValidateReturnQuantityAsync),
                    ServiceType = GetType().Name,
                    DetailId = detail.Id,
                    ReturnQuantity = detail.ReturnQuantity,

                });
                return ServiceResult.Failure("驗證退回數量時發生錯誤");
            }
        }

        /// <summary>
        /// 取得指定銷售訂單明細的已退貨數量
        /// </summary>
        public async Task<decimal> GetReturnedQuantityByOrderDetailAsync(int salesOrderDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Where(srd => srd.SalesOrderDetailId == salesOrderDetailId)
                    .SumAsync(srd => srd.ReturnQuantity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReturnedQuantityByOrderDetailAsync), GetType(), _logger, new { 
                    Method = nameof(GetReturnedQuantityByOrderDetailAsync),
                    ServiceType = GetType().Name,
                    SalesOrderDetailId = salesOrderDetailId 
                });
                return 0;
            }
        }
    }
}

