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
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.Product)
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.SalesDelivery)
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
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.Product)
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.SalesDelivery)
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
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.Product)
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.SalesDelivery)
                    .Where(srd => ((srd.Product != null && srd.Product.Code != null && srd.Product.Code.ToLower().Contains(lowerSearchTerm)) ||
                         (srd.Product != null && srd.Product.Name != null && srd.Product.Name.ToLower().Contains(lowerSearchTerm)) ||
                         (srd.SalesReturn != null && srd.SalesReturn.Code != null && srd.SalesReturn.Code.ToLower().Contains(lowerSearchTerm)) ||
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
                    errors.Add("必須選擇商品");

                if (entity.ReturnQuantity <= 0)
                    errors.Add("退回數量必須大於零");

                if (entity.OriginalUnitPrice < 0)
                    errors.Add("原始單價不能為負數");

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
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.Product)
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.SalesDelivery)
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
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.Product)
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.SalesDelivery)
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

        public async Task<List<SalesReturnDetail>> GetBySalesDeliveryDetailIdAsync(int salesDeliveryDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Include(srd => srd.SalesReturn)
                        .ThenInclude(sr => sr.Customer)
                    .Include(srd => srd.Product)
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.Product)
                    .Include(srd => srd.SalesDeliveryDetail)
                        .ThenInclude(sdd => sdd!.SalesDelivery)
                    .Where(srd => srd.SalesDeliveryDetailId == salesDeliveryDetailId)
                    .OrderByDescending(srd => srd.SalesReturn.ReturnDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesDeliveryDetailIdAsync), GetType(), _logger, new
                {
                    Method = nameof(GetBySalesDeliveryDetailIdAsync),
                    ServiceType = GetType().Name,
                    SalesDeliveryDetailId = salesDeliveryDetailId
                });
                return new List<SalesReturnDetail>();
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
                    // ReturnSubtotalAmount 現在是計算屬性，由 ReturnQuantity * OriginalUnitPrice * (1 - DiscountPercentage / 100) 自動計算
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

                // 允許刪除
                return true;
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
                    TotalReturnAmount = details.Sum(d => d.ReturnSubtotalAmount),
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
        /// 取得指定銷售出貨明細的已退貨數量
        /// </summary>
        public async Task<decimal> GetReturnedQuantityByDeliveryDetailAsync(int salesDeliveryDetailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesReturnDetails
                    .Where(srd => srd.SalesDeliveryDetailId == salesDeliveryDetailId)
                    .SumAsync(srd => srd.ReturnQuantity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReturnedQuantityByDeliveryDetailAsync), GetType(), _logger, new { 
                    Method = nameof(GetReturnedQuantityByDeliveryDetailAsync),
                    ServiceType = GetType().Name,
                    SalesDeliveryDetailId = salesDeliveryDetailId 
                });
                return 0;
            }
        }
    }
}

