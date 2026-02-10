using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 領貨明細服務實作
    /// </summary>
    public class MaterialIssueDetailService : GenericManagementService<MaterialIssueDetail>, IMaterialIssueDetailService
    {
        public MaterialIssueDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public MaterialIssueDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<MaterialIssueDetail>> logger) : base(contextFactory, logger)
        {
        }

        #region 覆寫基底類別方法

        /// <summary>
        /// 取得所有領貨明細（包含關聯資料）
        /// </summary>
        public override async Task<List<MaterialIssueDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.MaterialIssue)
                        .ThenInclude(mi => mi.Employee)
                    .Include(d => d.MaterialIssue)
                        .ThenInclude(mi => mi.Department)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .AsQueryable()
                    .OrderByDescending(d => d.CreatedAt)
                    .ThenBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<MaterialIssueDetail>();
            }
        }

        /// <summary>
        /// 根據ID取得領貨明細（包含關聯資料）
        /// </summary>
        public override async Task<MaterialIssueDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.MaterialIssue)
                        .ThenInclude(mi => mi.Employee)
                    .Include(d => d.MaterialIssue)
                        .ThenInclude(mi => mi.Department)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .FirstOrDefaultAsync(d => d.Id == id);
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

        /// <summary>
        /// 實作特定搜尋邏輯
        /// </summary>
        public override async Task<List<MaterialIssueDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.MaterialIssue)
                        .ThenInclude(mi => mi.Employee)
                    .Include(d => d.MaterialIssue)
                        .ThenInclude(mi => mi.Department)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => (
                        (d.MaterialIssue.Code != null && d.MaterialIssue.Code.Contains(searchTerm)) ||
                        d.Product.Name.Contains(searchTerm) ||
                        (d.Product.Code != null && d.Product.Code.Contains(searchTerm)) ||
                        (d.Product.Barcode != null && d.Product.Barcode.Contains(searchTerm)) ||
                        d.Warehouse.Name.Contains(searchTerm) ||
                        (d.WarehouseLocation != null && d.WarehouseLocation.Name.Contains(searchTerm))
                    ))
                    .OrderByDescending(d => d.CreatedAt)
                    .ThenBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<MaterialIssueDetail>();
            }
        }

        /// <summary>
        /// 實作特定驗證邏輯
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(MaterialIssueDetail entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本欄位驗證
                if (entity.MaterialIssueId <= 0)
                    errors.Add("領貨主檔不能為空");

                if (entity.ProductId <= 0)
                    errors.Add("商品不能為空");

                if (entity.WarehouseId <= 0)
                    errors.Add("倉庫不能為空");

                if (entity.IssueQuantity <= 0)
                    errors.Add("領貨數量必須大於 0");

                if (entity.UnitCost.HasValue && entity.UnitCost.Value < 0)
                    errors.Add("單位成本不能小於 0");

                // 驗證庫存是否充足
                var (isValid, availableQuantity, errorMessage) = await ValidateStockAvailabilityAsync(
                    entity.ProductId, 
                    entity.WarehouseId, 
                    entity.WarehouseLocationId, 
                    entity.IssueQuantity);

                if (!isValid)
                    errors.Add(errorMessage);

                // 檢查同一領貨單中同一商品+倉庫+庫位的組合是否重複
                if (await IsProductExistsInIssueAsync(
                    entity.MaterialIssueId, 
                    entity.ProductId, 
                    entity.WarehouseId,
                    entity.WarehouseLocationId,
                    entity.Id == 0 ? null : entity.Id))
                {
                    errors.Add("該商品在此領貨單的相同倉庫/庫位組合中已存在");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    MaterialIssueId = entity.MaterialIssueId,
                    ProductId = entity.ProductId 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 特定查詢方法

        /// <summary>
        /// 根據領貨主檔ID取得所有明細
        /// </summary>
        public async Task<List<MaterialIssueDetail>> GetByMaterialIssueIdAsync(int materialIssueId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.MaterialIssueId == materialIssueId)
                    .OrderBy(d => d.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByMaterialIssueIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByMaterialIssueIdAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = materialIssueId 
                });
                return new List<MaterialIssueDetail>();
            }
        }

        /// <summary>
        /// 根據商品ID取得所有領貨明細
        /// </summary>
        public async Task<List<MaterialIssueDetail>> GetByProductIdAsync(int productId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.MaterialIssue)
                        .ThenInclude(mi => mi.Employee)
                    .Include(d => d.MaterialIssue)
                        .ThenInclude(mi => mi.Department)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.ProductId == productId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByProductIdAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId 
                });
                return new List<MaterialIssueDetail>();
            }
        }

        /// <summary>
        /// 根據倉庫ID取得所有領貨明細
        /// </summary>
        public async Task<List<MaterialIssueDetail>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.MaterialIssue)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.WarehouseId == warehouseId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByWarehouseIdAsync),
                    ServiceType = GetType().Name,
                    WarehouseId = warehouseId 
                });
                return new List<MaterialIssueDetail>();
            }
        }

        /// <summary>
        /// 根據庫位ID取得所有領貨明細
        /// </summary>
        public async Task<List<MaterialIssueDetail>> GetByWarehouseLocationIdAsync(int warehouseLocationId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MaterialIssueDetails
                    .Include(d => d.MaterialIssue)
                    .Include(d => d.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.WarehouseLocationId == warehouseLocationId)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseLocationIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByWarehouseLocationIdAsync),
                    ServiceType = GetType().Name,
                    WarehouseLocationId = warehouseLocationId 
                });
                return new List<MaterialIssueDetail>();
            }
        }

        #endregion

        #region 批次操作方法

        /// <summary>
        /// 批次更新領貨明細
        /// </summary>
        public async Task<ServiceResult> UpdateDetailsAsync(int materialIssueId, List<MaterialIssueDetail> details)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    var result = await UpdateDetailsInContextAsync(context, materialIssueId, details, transaction);
                    
                    if (!result.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return result;
                    }

                    await transaction.CommitAsync();
                    return ServiceResult.Success();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = materialIssueId,
                    DetailsCount = details?.Count ?? 0 
                });
                return ServiceResult.Failure("更新領貨明細時發生錯誤");
            }
        }

        /// <summary>
        /// 批次更新領貨明細（支援外部 context 和 transaction）
        /// </summary>
        public async Task<ServiceResult> UpdateDetailsInContextAsync(
            AppDbContext context,
            int materialIssueId, 
            List<MaterialIssueDetail> details,
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? externalTransaction = null)
        {
            try
            {
                // 取得現有的明細記錄
                var existingDetails = await context.MaterialIssueDetails
                    .Where(d => d.MaterialIssueId == materialIssueId)
                    .ToListAsync();

                // 準備新的明細資料
                var newDetailsToAdd = new List<MaterialIssueDetail>();
                var newDetailIds = details?.Where(d => d.Id > 0).Select(d => d.Id).ToList() ?? new List<int>();
                var detailsToDelete = existingDetails
                    .Where(ed => !newDetailIds.Contains(ed.Id))
                    .ToList();

                // 處理傳入的明細
                if (details != null)
                {
                    foreach (var detail in details.Where(d => d.IssueQuantity > 0))
                    {
                        // 驗證必要欄位
                        if (detail.ProductId <= 0 || detail.WarehouseId <= 0 || detail.IssueQuantity <= 0)
                            continue;

                        detail.MaterialIssueId = materialIssueId;
                        detail.UpdatedAt = DateTime.UtcNow;

                        if (detail.Id == 0)
                        {
                            // 新增的明細
                            detail.CreatedAt = DateTime.UtcNow;
                            detail.Status = EntityStatus.Active;
                            newDetailsToAdd.Add(detail);
                        }
                        else
                        {
                            // 更新的明細
                            var existingDetail = existingDetails.FirstOrDefault(ed => ed.Id == detail.Id);
                            if (existingDetail != null)
                            {
                                // 更新現有明細的屬性
                                existingDetail.ProductId = detail.ProductId;
                                existingDetail.WarehouseId = detail.WarehouseId;
                                existingDetail.WarehouseLocationId = detail.WarehouseLocationId;
                                existingDetail.IssueQuantity = detail.IssueQuantity;
                                existingDetail.UnitCost = detail.UnitCost;
                                existingDetail.Remarks = detail.Remarks;
                                existingDetail.UpdatedAt = DateTime.UtcNow;
                            }
                        }
                    }
                }

                // 執行資料庫操作
                // 新增明細
                if (newDetailsToAdd.Any())
                {
                    await context.MaterialIssueDetails.AddRangeAsync(newDetailsToAdd);
                }

                // 硬刪除不需要的明細（不再使用軟刪除）
                if (detailsToDelete.Any())
                {
                    context.MaterialIssueDetails.RemoveRange(detailsToDelete);
                }

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateDetailsInContextAsync), GetType(), _logger, new { 
                    Method = nameof(UpdateDetailsInContextAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = materialIssueId,
                    DetailsCount = details?.Count ?? 0 
                });
                return ServiceResult.Failure("更新領貨明細時發生錯誤");
            }
        }

        #endregion

        #region 驗證方法

        /// <summary>
        /// 檢查商品在指定領貨單中是否已存在
        /// </summary>
        public async Task<bool> IsProductExistsInIssueAsync(
            int materialIssueId, 
            int productId, 
            int warehouseId, 
            int? warehouseLocationId = null, 
            int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.MaterialIssueDetails.Where(d => 
                    d.MaterialIssueId == materialIssueId && 
                    d.ProductId == productId &&
                    d.WarehouseId == warehouseId &&
                    d.Status != EntityStatus.Deleted);
                
                // 同時比較庫位（包括 null 的情況）
                if (warehouseLocationId.HasValue)
                    query = query.Where(d => d.WarehouseLocationId == warehouseLocationId.Value);
                else
                    query = query.Where(d => d.WarehouseLocationId == null);
                    
                if (excludeId.HasValue)
                    query = query.Where(d => d.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsProductExistsInIssueAsync), GetType(), _logger, new { 
                    Method = nameof(IsProductExistsInIssueAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = materialIssueId,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    WarehouseLocationId = warehouseLocationId,
                    ExcludeId = excludeId 
                });
                return false;
            }
        }

        /// <summary>
        /// 驗證領貨明細的庫存是否充足
        /// </summary>
        public async Task<(bool IsValid, decimal AvailableQuantity, string ErrorMessage)> ValidateStockAvailabilityAsync(
            int productId, 
            int warehouseId, 
            int? warehouseLocationId, 
            decimal issueQuantity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 先找到該商品的 InventoryStock
                var inventoryStock = await context.InventoryStocks
                    .FirstOrDefaultAsync(s => s.ProductId == productId && s.Status == EntityStatus.Active);

                if (inventoryStock == null)
                {
                    // 查詢商品名稱以提供更友善的錯誤訊息
                    var product = await context.Products
                        .FirstOrDefaultAsync(p => p.Id == productId);
                    var productName = product?.Name ?? "未知商品";
                    
                    return (false, 0, $"商品「{productName}」尚未建立庫存記錄");
                }

                // 查詢庫存明細
                var stockQuery = context.InventoryStockDetails
                    .Where(s => s.InventoryStockId == inventoryStock.Id && 
                               s.WarehouseId == warehouseId &&
                               s.Status == EntityStatus.Active);

                if (warehouseLocationId.HasValue)
                    stockQuery = stockQuery.Where(s => s.WarehouseLocationId == warehouseLocationId.Value);
                else
                    stockQuery = stockQuery.Where(s => s.WarehouseLocationId == null);

                var stockDetail = await stockQuery.FirstOrDefaultAsync();

                if (stockDetail == null)
                {
                    // 查詢商品名稱以提供更友善的錯誤訊息
                    var product = await context.Products
                        .FirstOrDefaultAsync(p => p.Id == productId);
                    var productName = product?.Name ?? "未知商品";
                    
                    return (false, 0, $"商品「{productName}」在指定的倉庫/庫位沒有庫存記錄");
                }

                // 計算可用數量（現有庫存 - 預留庫存）
                var availableQuantity = stockDetail.AvailableStock;

                if (availableQuantity < issueQuantity)
                {
                    var product = await context.Products
                        .FirstOrDefaultAsync(p => p.Id == productId);
                    var productName = product?.Name ?? "未知商品";
                    
                    return (false, availableQuantity, 
                        $"商品「{productName}」庫存不足，可用數量：{availableQuantity}，需要數量：{issueQuantity}");
                }

                return (true, availableQuantity, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateStockAvailabilityAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateStockAvailabilityAsync),
                    ServiceType = GetType().Name,
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    WarehouseLocationId = warehouseLocationId,
                    IssueQuantity = issueQuantity 
                });
                return (false, 0, "驗證庫存時發生錯誤");
            }
        }

        #endregion

        #region 統計方法

        /// <summary>
        /// 取得領貨明細的統計資料
        /// </summary>
        public async Task<(int TotalQuantity, decimal TotalCost)> GetStatisticsAsync(int materialIssueId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var details = await context.MaterialIssueDetails
                    .Where(d => d.MaterialIssueId == materialIssueId && d.Status != EntityStatus.Deleted)
                    .ToListAsync();

                var totalQuantity = details.Sum(d => d.IssueQuantity);
                var totalCost = details.Sum(d => d.TotalCost ?? 0);

                return (totalQuantity, totalCost);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetStatisticsAsync), GetType(), _logger, new { 
                    Method = nameof(GetStatisticsAsync),
                    ServiceType = GetType().Name,
                    MaterialIssueId = materialIssueId 
                });
                return (0, 0m);
            }
        }

        /// <summary>
        /// 計算領貨明細的總成本
        /// </summary>
        public decimal CalculateTotalCost(int issueQuantity, decimal? unitCost)
        {
            return unitCost.HasValue ? issueQuantity * unitCost.Value : 0m;
        }

        #endregion
    }
}
