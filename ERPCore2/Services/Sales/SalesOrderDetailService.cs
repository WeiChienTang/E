using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銷貨訂單明細服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SalesOrderDetailService : GenericManagementService<SalesOrderDetail>, ISalesOrderDetailService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public SalesOrderDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SalesOrderDetail>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public SalesOrderDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底抽象方法

        public override async Task<List<SalesOrderDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrderDetails
                    .Include(sod => sod.SalesOrder)
                    .Include(sod => sod.Product)
                    .Include(sod => sod.Unit)
                    .Where(sod => !sod.IsDeleted &&
                                 (sod.Product.Name.Contains(searchTerm) ||
                                  sod.SalesOrder.SalesOrderNumber.Contains(searchTerm) ||
                                  (!string.IsNullOrEmpty(sod.DetailRemarks) && sod.DetailRemarks.Contains(searchTerm))))
                    .OrderBy(sod => sod.SalesOrder.OrderDate)
                    .ThenBy(sod => sod.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<SalesOrderDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SalesOrderDetail entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證銷貨訂單
                if (entity.SalesOrderId <= 0)
                {
                    errors.Add("銷貨訂單為必填");
                }

                // 驗證商品
                if (entity.ProductId <= 0)
                {
                    errors.Add("商品為必填");
                }

                // 驗證訂單數量
                if (entity.OrderQuantity <= 0)
                {
                    errors.Add("訂單數量必須大於0");
                }

                // 驗證單價
                if (entity.UnitPrice <= 0)
                {
                    errors.Add("單價必須大於0");
                }

                // 驗證折扣比例
                if (entity.DiscountPercentage < 0 || entity.DiscountPercentage > 100)
                {
                    errors.Add("折扣比例必須介於0-100之間");
                }

                // 檢查商品在同一訂單中是否重複
                if (entity.SalesOrderId > 0 && entity.ProductId > 0)
                {
                    var isDuplicate = await IsProductExistsInOrderAsync(
                        entity.SalesOrderId, 
                        entity.ProductId, 
                        entity.Id > 0 ? entity.Id : null);
                    
                    if (isDuplicate)
                    {
                        errors.Add("該商品在此訂單中已存在");
                    }
                }

                // 驗證已出貨數量不能超過訂單數量
                if (entity.DeliveredQuantity > entity.OrderQuantity)
                {
                    errors.Add("已出貨數量不能超過訂單數量");
                }

                // 驗證待出貨數量
                if (entity.PendingQuantity < 0)
                {
                    errors.Add("待出貨數量不能為負數");
                }

                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    SalesOrderId = entity.SalesOrderId,
                    ProductId = entity.ProductId
                });
                return ServiceResult.Failure("驗證銷貨訂單明細時發生錯誤");
            }
        }

        #endregion

        #region 覆寫基底方法

        public override async Task<List<SalesOrderDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrderDetails
                    .Include(sod => sod.SalesOrder)
                    .Include(sod => sod.Product)
                    .Include(sod => sod.Unit)
                    .Where(sod => !sod.IsDeleted)
                    .OrderBy(sod => sod.SalesOrder.OrderDate)
                    .ThenBy(sod => sod.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<SalesOrderDetail>();
            }
        }

        public override async Task<SalesOrderDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrderDetails
                    .Include(sod => sod.SalesOrder)
                    .Include(sod => sod.Product)
                    .Include(sod => sod.Unit)
                    .FirstOrDefaultAsync(sod => sod.Id == id && !sod.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                return null;
            }
        }

        #endregion

        #region ISalesOrderDetailService 特定方法

        /// <summary>
        /// 根據銷貨訂單ID取得明細清單
        /// </summary>
        public async Task<List<SalesOrderDetail>> GetBySalesOrderIdAsync(int salesOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrderDetails
                    .Include(sod => sod.Product)
                    .Include(sod => sod.Unit)
                    .Where(sod => sod.SalesOrderId == salesOrderId && !sod.IsDeleted)
                    .OrderBy(sod => sod.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderIdAsync), GetType(), _logger, new {
                    Method = nameof(GetBySalesOrderIdAsync),
                    ServiceType = GetType().Name,
                    SalesOrderId = salesOrderId
                });
                return new List<SalesOrderDetail>();
            }
        }

        /// <summary>
        /// 根據商品ID取得銷貨訂單明細
        /// </summary>
        public async Task<List<SalesOrderDetail>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrderDetails
                    .Include(sod => sod.SalesOrder)
                    .Include(sod => sod.Unit)
                    .Where(sod => sod.ProductId == productId && !sod.IsDeleted)
                    .OrderByDescending(sod => sod.SalesOrder.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new {
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId
                });
                return new List<SalesOrderDetail>();
            }
        }

        /// <summary>
        /// 計算明細小計
        /// </summary>
        public async Task<ServiceResult> CalculateSubtotalAsync(int detailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var detail = await context.SalesOrderDetails.FindAsync(detailId);
                
                if (detail == null)
                {
                    return ServiceResult.Failure("找不到指定的銷貨訂單明細");
                }

                // 計算小計：數量 × 單價 - 折扣金額
                var subtotal = detail.OrderQuantity * detail.UnitPrice - detail.DiscountAmount;
                detail.Subtotal = subtotal;
                detail.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateSubtotalAsync), GetType(), _logger, new {
                    Method = nameof(CalculateSubtotalAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId
                });
                return ServiceResult.Failure("計算明細小計時發生錯誤");
            }
        }

        /// <summary>
        /// 批次更新明細資料
        /// </summary>
        public async Task<ServiceResult> UpdateDetailsAsync(List<SalesOrderDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                foreach (var detail in details)
                {
                    // 重新計算小計
                    detail.Subtotal = detail.OrderQuantity * detail.UnitPrice - detail.DiscountAmount;
                    detail.UpdatedAt = DateTime.UtcNow;
                    
                    context.SalesOrderDetails.Update(detail);
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsAsync), GetType(), _logger, new {
                    Method = nameof(UpdateDetailsAsync),
                    ServiceType = GetType().Name,
                    DetailCount = details.Count
                });
                return ServiceResult.Failure("批次更新明細資料時發生錯誤");
            }
        }

        /// <summary>
        /// 刪除指定銷貨訂單的所有明細
        /// </summary>
        public async Task<ServiceResult> DeleteBySalesOrderIdAsync(int salesOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var details = await context.SalesOrderDetails
                    .Where(sod => sod.SalesOrderId == salesOrderId && !sod.IsDeleted)
                    .ToListAsync();

                foreach (var detail in details)
                {
                    detail.IsDeleted = true;
                    detail.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteBySalesOrderIdAsync), GetType(), _logger, new {
                    Method = nameof(DeleteBySalesOrderIdAsync),
                    ServiceType = GetType().Name,
                    SalesOrderId = salesOrderId
                });
                return ServiceResult.Failure("刪除銷貨訂單明細時發生錯誤");
            }
        }

        /// <summary>
        /// 檢查商品在訂單中是否已存在
        /// </summary>
        public async Task<bool> IsProductExistsInOrderAsync(int salesOrderId, int productId, int? excludeDetailId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SalesOrderDetails
                    .Where(sod => sod.SalesOrderId == salesOrderId && 
                                  sod.ProductId == productId && 
                                  !sod.IsDeleted);

                if (excludeDetailId.HasValue)
                {
                    query = query.Where(sod => sod.Id != excludeDetailId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsProductExistsInOrderAsync), GetType(), _logger, new {
                    Method = nameof(IsProductExistsInOrderAsync),
                    ServiceType = GetType().Name,
                    SalesOrderId = salesOrderId,
                    ProductId = productId,
                    ExcludeDetailId = excludeDetailId
                });
                return false;
            }
        }

        /// <summary>
        /// 取得銷貨訂單明細包含關聯資料
        /// </summary>
        public async Task<SalesOrderDetail?> GetWithIncludesAsync(int detailId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrderDetails
                    .Include(sod => sod.SalesOrder)
                        .ThenInclude(so => so.Customer)
                    .Include(sod => sod.Product)
                    .Include(sod => sod.Unit)
                    .FirstOrDefaultAsync(sod => sod.Id == detailId && !sod.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetWithIncludesAsync), GetType(), _logger, new {
                    Method = nameof(GetWithIncludesAsync),
                    ServiceType = GetType().Name,
                    DetailId = detailId
                });
                return null;
            }
        }

        /// <summary>
        /// 根據銷貨訂單ID取得明細包含關聯資料
        /// </summary>
        public async Task<List<SalesOrderDetail>> GetBySalesOrderIdWithIncludesAsync(int salesOrderId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SalesOrderDetails
                    .Include(sod => sod.Product)
                    .Include(sod => sod.Unit)
                    .Where(sod => sod.SalesOrderId == salesOrderId && !sod.IsDeleted)
                    .OrderBy(sod => sod.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySalesOrderIdWithIncludesAsync), GetType(), _logger, new {
                    Method = nameof(GetBySalesOrderIdWithIncludesAsync),
                    ServiceType = GetType().Name,
                    SalesOrderId = salesOrderId
                });
                return new List<SalesOrderDetail>();
            }
        }

        #endregion
    }
}
