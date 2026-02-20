using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 庫存明細管理服務實作
    /// </summary>
    public class InventoryStockDetailService : GenericManagementService<InventoryStockDetail>, IInventoryStockDetailService
    {
        public InventoryStockDetailService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public InventoryStockDetailService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<InventoryStockDetail>> logger) : base(contextFactory, logger)
        {
        }

        #region 覆寫基本方法

        public override async Task<List<InventoryStockDetail>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .OrderBy(d => d.InventoryStock.Product!.Code)
                    .ThenBy(d => d.Warehouse.Code)
                    .ThenBy(d => d.WarehouseLocation!.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<InventoryStockDetail>();
            }
        }

        public override async Task<InventoryStockDetail?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Include(d => d.InventoryTransactions)
                    .Include(d => d.InventoryReservations)
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

        public override async Task<List<InventoryStockDetail>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var term = searchTerm.ToLower();
                
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => 
                        (d.InventoryStock.Product!.Code != null && d.InventoryStock.Product.Code.ToLower().Contains(term)) ||
                        (d.InventoryStock.Product!.Name != null && d.InventoryStock.Product.Name.ToLower().Contains(term)) ||
                        (d.Warehouse.Code != null && d.Warehouse.Code.ToLower().Contains(term)) ||
                        (d.Warehouse.Name != null && d.Warehouse.Name.ToLower().Contains(term)) ||
                        (d.WarehouseLocation != null && d.WarehouseLocation.Code != null && d.WarehouseLocation.Code.ToLower().Contains(term)) ||
                        (d.WarehouseLocation != null && d.WarehouseLocation.Name != null && d.WarehouseLocation.Name.ToLower().Contains(term)) ||
                        (d.BatchNumber != null && d.BatchNumber.ToLower().Contains(term)))
                    .OrderBy(d => d.InventoryStock.Product!.Code)
                    .ThenBy(d => d.Warehouse.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<InventoryStockDetail>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(InventoryStockDetail entity)
        {
            try
            {
                var errors = new List<string>();

                // 檢查庫存主檔
                if (entity.InventoryStockId <= 0)
                    errors.Add("庫存主檔為必填");

                // 檢查倉庫
                if (entity.WarehouseId <= 0)
                    errors.Add("倉庫為必填");

                // 檢查庫存數量不能為負數
                if (entity.CurrentStock < 0)
                    errors.Add("現有庫存不能為負數");

                if (entity.ReservedStock < 0)
                    errors.Add("預留庫存不能為負數");

                if (entity.InTransitStock < 0)
                    errors.Add("在途庫存不能為負數");

                // 檢查預留庫存不能超過現有庫存
                if (entity.ReservedStock > entity.CurrentStock)
                    errors.Add("預留庫存不能超過現有庫存");

                // 檢查唯一性（同一商品在同一倉庫的同一庫位只能有一筆）
                using var context = await _contextFactory.CreateDbContextAsync();
                var existingDetail = await context.InventoryStockDetails
                    .FirstOrDefaultAsync(d => 
                        d.InventoryStockId == entity.InventoryStockId &&
                        d.WarehouseId == entity.WarehouseId &&
                        d.WarehouseLocationId == entity.WarehouseLocationId &&
                        d.Id != entity.Id);

                if (existingDetail != null)
                    errors.Add("該商品在此倉庫位置已有庫存明細記錄");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 覆寫基底類別方法

        /// <summary>
        /// 覆寫刪除前檢查：確認庫存為零，並清除 InventoryTransactionDetails 的外鍵關聯
        /// </summary>
        protected override async Task<ServiceResult> CanDeleteAsync(InventoryStockDetail entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 重新從資料庫取得最新數量（避免傳入的 entity 未反映最新狀態）
                var detail = await context.InventoryStockDetails
                    .FirstOrDefaultAsync(d => d.Id == entity.Id);

                if (detail == null)
                {
                    return ServiceResult.Failure("找不到要刪除的庫存明細記錄");
                }

                // 只要任何庫存數量不為零就不允許刪除
                if (detail.CurrentStock != 0 || detail.ReservedStock != 0 ||
                    detail.InTransitStock != 0 || detail.InProductionStock != 0)
                {
                    return ServiceResult.Failure(
                        $"無法刪除此庫存明細，目前庫存：{detail.CurrentStock}，" +
                        $"預留庫存：{detail.ReservedStock}，" +
                        $"在途庫存：{detail.InTransitStock}，" +
                        $"生產中庫存：{detail.InProductionStock}");
                }

                // 清除 InventoryTransactionDetails 對此明細的外鍵關聯
                var relatedTransactionDetails = await context.InventoryTransactionDetails
                    .Where(d => d.InventoryStockDetailId == entity.Id)
                    .ToListAsync();

                foreach (var txDetail in relatedTransactionDetails)
                {
                    txDetail.InventoryStockDetailId = null;
                }

                // 清除 InventoryReservations 對此明細的外鍵關聯
                var relatedReservations = await context.InventoryReservations
                    .Where(r => r.InventoryStockDetailId == entity.Id)
                    .ToListAsync();

                foreach (var reservation in relatedReservations)
                {
                    reservation.UpdatedAt = DateTime.UtcNow;
                    reservation.InventoryStockDetailId = null;
                }

                if (relatedTransactionDetails.Any() || relatedReservations.Any())
                {
                    await context.SaveChangesAsync();
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new {
                    EntityId = entity.Id,
                    InventoryStockId = entity.InventoryStockId
                });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 查詢方法

        public async Task<List<InventoryStockDetail>> GetByInventoryStockIdAsync(int inventoryStockId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.InventoryStockId == inventoryStockId)
                    .OrderBy(d => d.Warehouse.Code)
                    .ThenBy(d => d.WarehouseLocation!.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByInventoryStockIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByInventoryStockIdAsync),
                    ServiceType = GetType().Name,
                    InventoryStockId = inventoryStockId 
                });
                return new List<InventoryStockDetail>();
            }
        }

        public async Task<List<InventoryStockDetail>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.WarehouseId == warehouseId)
                    .OrderBy(d => d.InventoryStock.Product!.Code)
                    .ThenBy(d => d.WarehouseLocation!.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByWarehouseIdAsync),
                    ServiceType = GetType().Name,
                    WarehouseId = warehouseId 
                });
                return new List<InventoryStockDetail>();
            }
        }

        public async Task<List<InventoryStockDetail>> GetByWarehouseLocationIdAsync(int warehouseLocationId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .Where(d => d.WarehouseLocationId == warehouseLocationId)
                    .OrderBy(d => d.InventoryStock.Product!.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseLocationIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByWarehouseLocationIdAsync),
                    ServiceType = GetType().Name,
                    WarehouseLocationId = warehouseLocationId 
                });
                return new List<InventoryStockDetail>();
            }
        }

        public async Task<InventoryStockDetail?> GetByInventoryWarehouseLocationAsync(int inventoryStockId, int warehouseId, int? warehouseLocationId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStockDetails
                    .Include(d => d.InventoryStock)
                        .ThenInclude(s => s.Product)
                    .Include(d => d.Warehouse)
                    .Include(d => d.WarehouseLocation)
                    .FirstOrDefaultAsync(d => 
                        d.InventoryStockId == inventoryStockId &&
                        d.WarehouseId == warehouseId &&
                        d.WarehouseLocationId == warehouseLocationId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByInventoryWarehouseLocationAsync), GetType(), _logger, new { 
                    Method = nameof(GetByInventoryWarehouseLocationAsync),
                    ServiceType = GetType().Name,
                    InventoryStockId = inventoryStockId,
                    WarehouseId = warehouseId,
                    WarehouseLocationId = warehouseLocationId 
                });
                return null;
            }
        }

        public async Task<bool> ExistsAsync(int inventoryStockId, int warehouseId, int? warehouseLocationId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.InventoryStockDetails
                    .AnyAsync(d => 
                        d.InventoryStockId == inventoryStockId &&
                        d.WarehouseId == warehouseId &&
                        d.WarehouseLocationId == warehouseLocationId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ExistsAsync), GetType(), _logger, new { 
                    Method = nameof(ExistsAsync),
                    ServiceType = GetType().Name,
                    InventoryStockId = inventoryStockId,
                    WarehouseId = warehouseId,
                    WarehouseLocationId = warehouseLocationId 
                });
                return false;
            }
        }

        #endregion
    }
}
