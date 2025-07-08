using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Inventory
{
    /// <summary>
    /// 庫存預留服務實作
    /// </summary>
    public class InventoryReservationService : GenericManagementService<InventoryReservation>, IInventoryReservationService
    {
        public InventoryReservationService(AppDbContext context) : base(context)
        {
        }

        public InventoryReservationService(
            AppDbContext context, 
            ILogger<GenericManagementService<InventoryReservation>> logger) : base(context, logger)
        {
        }

        #region 基本查詢

        public async Task<List<InventoryReservation>> GetByProductIdAsync(int productId)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.ProductId == productId && !r.IsDeleted)
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByProductIdAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        public async Task<List<InventoryReservation>> GetByWarehouseIdAsync(int warehouseId)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.WarehouseId == warehouseId && !r.IsDeleted)
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByWarehouseIdAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        public async Task<List<InventoryReservation>> GetByReferenceNumberAsync(string referenceNumber)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.ReferenceNumber == referenceNumber && !r.IsDeleted)
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByReferenceNumberAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        public async Task<List<InventoryReservation>> GetByTypeAsync(InventoryReservationType reservationType)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.ReservationType == reservationType && !r.IsDeleted)
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByTypeAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        public async Task<List<InventoryReservation>> GetActiveReservationsAsync()
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.ReservationStatus == InventoryReservationStatus.Reserved && 
                               !r.IsDeleted &&
                               (r.ExpiryDate == null || r.ExpiryDate > DateTime.Now))
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveReservationsAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        public async Task<List<InventoryReservation>> GetExpiredReservationsAsync()
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.ReservationStatus == InventoryReservationStatus.Reserved && 
                               !r.IsDeleted &&
                               r.ExpiryDate.HasValue && 
                               r.ExpiryDate < DateTime.Now)
                    .OrderBy(r => r.ExpiryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetExpiredReservationsAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        #endregion

        #region 特定查詢

        public async Task<List<InventoryReservation>> GetActiveByProductAsync(int productId, int? warehouseId = null)
        {
            try
            {
                var query = _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.ProductId == productId && 
                               r.ReservationStatus == InventoryReservationStatus.Reserved && 
                               !r.IsDeleted &&
                               (r.ExpiryDate == null || r.ExpiryDate > DateTime.Now));

                if (warehouseId.HasValue)
                    query = query.Where(r => r.WarehouseId == warehouseId.Value);

                return await query
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveByProductAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        public async Task<List<InventoryReservation>> GetActiveByWarehouseAsync(int warehouseId)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.WarehouseId == warehouseId && 
                               r.ReservationStatus == InventoryReservationStatus.Reserved && 
                               !r.IsDeleted &&
                               (r.ExpiryDate == null || r.ExpiryDate > DateTime.Now))
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveByWarehouseAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        public async Task<InventoryReservation?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdWithDetailsAsync), typeof(InventoryReservationService), _logger);
                return null;
            }
        }

        public async Task<List<InventoryReservation>> GetExpiringReservationsAsync(DateTime beforeDate)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.ReservationStatus == InventoryReservationStatus.Reserved && 
                               !r.IsDeleted &&
                               r.ExpiryDate.HasValue && 
                               r.ExpiryDate <= beforeDate)
                    .OrderBy(r => r.ExpiryDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetExpiringReservationsAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        #endregion

        #region 統計查詢

        public async Task<int> GetTotalReservedQuantityAsync(int productId, int warehouseId, int? locationId = null)
        {
            try
            {
                var query = _dbSet.Where(r => r.ProductId == productId && 
                                             r.WarehouseId == warehouseId && 
                                             r.ReservationStatus == InventoryReservationStatus.Reserved && 
                                             !r.IsDeleted &&
                                             (r.ExpiryDate == null || r.ExpiryDate > DateTime.Now));

                if (locationId.HasValue)
                    query = query.Where(r => r.WarehouseLocationId == locationId.Value);

                return await query.SumAsync(r => r.ReservedQuantity - r.ReleasedQuantity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetTotalReservedQuantityAsync), typeof(InventoryReservationService), _logger);
                return 0;
            }
        }

        public async Task<int> GetAvailableQuantityForReservationAsync(int productId, int warehouseId, int? locationId = null)
        {
            try
            {
                // 獲取庫存數量
                var stockQuery = _context.InventoryStocks
                    .Where(s => s.ProductId == productId && 
                               s.WarehouseId == warehouseId && 
                               !s.IsDeleted);

                if (locationId.HasValue)
                    stockQuery = stockQuery.Where(s => s.WarehouseLocationId == locationId.Value);

                var totalStock = await stockQuery.SumAsync(s => s.CurrentStock);

                // 獲取已預留數量
                var reservedQuantity = await GetTotalReservedQuantityAsync(productId, warehouseId, locationId);

                return Math.Max(0, totalStock - reservedQuantity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAvailableQuantityForReservationAsync), typeof(InventoryReservationService), _logger);
                return 0;
            }
        }

        public async Task<Dictionary<InventoryReservationType, int>> GetReservationSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _dbSet.Where(r => !r.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(r => r.ReservationDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(r => r.ReservationDate <= endDate.Value);

                return await query
                    .GroupBy(r => r.ReservationType)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReservationSummaryAsync), typeof(InventoryReservationService), _logger);
                return new Dictionary<InventoryReservationType, int>();
            }
        }

        #endregion

        #region 預留操作

        public async Task<ServiceResult> CreateReservationAsync(int productId, int warehouseId, int quantity,
            InventoryReservationType reservationType, string referenceNumber,
            DateTime? expiryDate = null, int? locationId = null, string? remarks = null, int? employeeId = null)
        {
            try
            {
                if (quantity <= 0)
                    return ServiceResult.Failure("預留數量必須大於0");

                // 檢查是否有足夠庫存可預留
                var canReserve = await CanReserveQuantityAsync(productId, warehouseId, quantity, locationId);
                if (!canReserve)
                    return ServiceResult.Failure("庫存不足，無法進行預留");

                var reservation = new InventoryReservation
                {
                    ProductId = productId,
                    WarehouseId = warehouseId,
                    WarehouseLocationId = locationId,
                    ReservationType = reservationType,
                    ReferenceNumber = referenceNumber,
                    ReservedQuantity = quantity,
                    ReleasedQuantity = 0,
                    ReservationStatus = InventoryReservationStatus.Reserved,
                    ReservationDate = DateTime.Now,
                    ExpiryDate = expiryDate,
                    ReservationRemarks = remarks,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                };

                var validationResult = await ValidateReservationAsync(reservation);
                if (!validationResult.IsSuccess)
                    return validationResult;

                await _dbSet.AddAsync(reservation);
                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateReservationAsync), typeof(InventoryReservationService), _logger);
                return ServiceResult.Failure("建立庫存預留時發生錯誤");
            }
        }

        public async Task<ServiceResult> ReleaseReservationAsync(int reservationId, int? releaseQuantity = null, 
            string? remarks = null, int? employeeId = null)
        {
            try
            {
                var reservation = await GetByIdAsync(reservationId);
                if (reservation == null)
                    return ServiceResult.Failure("找不到指定的預留記錄");

                if (reservation.ReservationStatus != InventoryReservationStatus.Reserved)
                    return ServiceResult.Failure("只能釋放預留狀態的預留記錄");

                var remainingQuantity = reservation.ReservedQuantity - reservation.ReleasedQuantity;
                if (remainingQuantity <= 0)
                    return ServiceResult.Failure("該預留記錄已完全釋放");

                var actualReleaseQuantity = releaseQuantity ?? remainingQuantity;
                if (actualReleaseQuantity > remainingQuantity)
                    return ServiceResult.Failure($"釋放數量不能超過剩餘預留數量({remainingQuantity})");

                reservation.ReleasedQuantity += actualReleaseQuantity;
                reservation.UpdatedAt = DateTime.Now;

                // 如果完全釋放，更新狀態
                if (reservation.ReleasedQuantity >= reservation.ReservedQuantity)
                {
                    reservation.ReservationStatus = InventoryReservationStatus.Released;
                }

                if (!string.IsNullOrWhiteSpace(remarks))
                {
                    reservation.ReservationRemarks = string.IsNullOrWhiteSpace(reservation.ReservationRemarks) 
                        ? remarks 
                        : $"{reservation.ReservationRemarks}; {remarks}";
                }

                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReleaseReservationAsync), typeof(InventoryReservationService), _logger);
                return ServiceResult.Failure("釋放庫存預留時發生錯誤");
            }
        }

        public async Task<ServiceResult> CancelReservationAsync(int reservationId, string? reason = null, int? employeeId = null)
        {
            try
            {
                var reservation = await GetByIdAsync(reservationId);
                if (reservation == null)
                    return ServiceResult.Failure("找不到指定的預留記錄");

                if (reservation.ReservationStatus == InventoryReservationStatus.Cancelled)
                    return ServiceResult.Failure("該預留記錄已被取消");

                if (reservation.ReservationStatus == InventoryReservationStatus.Released)
                    return ServiceResult.Failure("已釋放的預留記錄無法取消");

                reservation.ReservationStatus = InventoryReservationStatus.Cancelled;
                reservation.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(reason))
                {
                    reservation.ReservationRemarks = string.IsNullOrWhiteSpace(reservation.ReservationRemarks) 
                        ? $"取消原因: {reason}" 
                        : $"{reservation.ReservationRemarks}; 取消原因: {reason}";
                }

                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CancelReservationAsync), typeof(InventoryReservationService), _logger);
                return ServiceResult.Failure("取消庫存預留時發生錯誤");
            }
        }

        public async Task<ServiceResult> ExtendReservationAsync(int reservationId, DateTime newExpiryDate, 
            string? remarks = null, int? employeeId = null)
        {
            try
            {
                var reservation = await GetByIdAsync(reservationId);
                if (reservation == null)
                    return ServiceResult.Failure("找不到指定的預留記錄");

                if (reservation.ReservationStatus != InventoryReservationStatus.Reserved)
                    return ServiceResult.Failure("只能延長預留狀態的預留記錄");

                if (newExpiryDate <= DateTime.Now)
                    return ServiceResult.Failure("新到期日必須大於目前時間");

                if (reservation.ExpiryDate.HasValue && newExpiryDate <= reservation.ExpiryDate.Value)
                    return ServiceResult.Failure("新到期日必須大於原到期日");

                reservation.ExpiryDate = newExpiryDate;
                reservation.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(remarks))
                {
                    reservation.ReservationRemarks = string.IsNullOrWhiteSpace(reservation.ReservationRemarks) 
                        ? $"延長到期日: {remarks}" 
                        : $"{reservation.ReservationRemarks}; 延長到期日: {remarks}";
                }

                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ExtendReservationAsync), typeof(InventoryReservationService), _logger);
                return ServiceResult.Failure("延長預留到期日時發生錯誤");
            }
        }

        public async Task<ServiceResult> PartialReleaseAsync(int reservationId, int releaseQuantity, 
            string? remarks = null, int? employeeId = null)
        {
            try
            {
                if (releaseQuantity <= 0)
                    return ServiceResult.Failure("釋放數量必須大於0");

                return await ReleaseReservationAsync(reservationId, releaseQuantity, remarks, employeeId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PartialReleaseAsync), typeof(InventoryReservationService), _logger);
                return ServiceResult.Failure("部分釋放預留時發生錯誤");
            }
        }

        #endregion

        #region 批次操作

        public async Task<ServiceResult> ReleaseExpiredReservationsAsync(int? employeeId = null)
        {
            try
            {
                var expiredReservations = await GetExpiredReservationsAsync();
                var processedCount = 0;

                foreach (var reservation in expiredReservations)
                {
                    var releaseResult = await ReleaseReservationAsync(reservation.Id, null, "自動釋放過期預留", employeeId);
                    if (releaseResult.IsSuccess)
                        processedCount++;
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReleaseExpiredReservationsAsync), typeof(InventoryReservationService), _logger);
                return ServiceResult.Failure("釋放過期預留時發生錯誤");
            }
        }

        public async Task<ServiceResult> ReleaseReservationsByReferenceAsync(string referenceNumber, 
            string? remarks = null, int? employeeId = null)
        {
            try
            {
                var reservations = await GetByReferenceNumberAsync(referenceNumber);
                var activeReservations = reservations.Where(r => r.ReservationStatus == InventoryReservationStatus.Reserved).ToList();

                if (!activeReservations.Any())
                    return ServiceResult.Failure("找不到預留狀態的預留記錄");

                var processedCount = 0;

                foreach (var reservation in activeReservations)
                {
                    var releaseResult = await ReleaseReservationAsync(reservation.Id, null, remarks, employeeId);
                    if (releaseResult.IsSuccess)
                        processedCount++;
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReleaseReservationsByReferenceAsync), typeof(InventoryReservationService), _logger);
                return ServiceResult.Failure("批次釋放預留時發生錯誤");
            }
        }

        #endregion

        #region 驗證方法

        public override async Task<ServiceResult> ValidateAsync(InventoryReservation reservation)
        {
            return await ValidateReservationAsync(reservation);
        }

        public async Task<ServiceResult> ValidateReservationAsync(InventoryReservation reservation)
        {
            try
            {
                var errors = new List<string>();

                if (reservation.ProductId <= 0)
                    errors.Add("產品ID不能為空");

                if (reservation.WarehouseId <= 0)
                    errors.Add("倉庫ID不能為空");

                if (string.IsNullOrWhiteSpace(reservation.ReferenceNumber))
                    errors.Add("參考單號不能為空");

                if (reservation.ReservedQuantity <= 0)
                    errors.Add("預留數量必須大於0");

                if (reservation.ReleasedQuantity < 0)
                    errors.Add("已釋放數量不能小於0");

                if (reservation.ReleasedQuantity > reservation.ReservedQuantity)
                    errors.Add("已釋放數量不能大於預留數量");

                if (reservation.ExpiryDate.HasValue && reservation.ExpiryDate.Value <= reservation.ReservationDate)
                    errors.Add("到期日必須大於預留日期");

                // 檢查產品是否存在
                var productExists = await _context.Products.AnyAsync(p => p.Id == reservation.ProductId && !p.IsDeleted);
                if (!productExists)
                    errors.Add("指定的產品不存在");

                // 檢查倉庫是否存在
                var warehouseExists = await _context.Warehouses.AnyAsync(w => w.Id == reservation.WarehouseId && !w.IsDeleted);
                if (!warehouseExists)
                    errors.Add("指定的倉庫不存在");

                // 檢查倉庫位置是否存在（如果有指定）
                if (reservation.WarehouseLocationId.HasValue)
                {
                    var locationExists = await _context.WarehouseLocations
                        .AnyAsync(l => l.Id == reservation.WarehouseLocationId.Value && 
                                      l.WarehouseId == reservation.WarehouseId && 
                                      !l.IsDeleted);
                    if (!locationExists)
                        errors.Add("指定的倉庫位置不存在或不屬於該倉庫");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateReservationAsync), typeof(InventoryReservationService), _logger);
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public async Task<bool> CanReserveQuantityAsync(int productId, int warehouseId, int quantity, int? locationId = null)
        {
            try
            {
                var availableQuantity = await GetAvailableQuantityForReservationAsync(productId, warehouseId, locationId);
                return availableQuantity >= quantity;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanReserveQuantityAsync), typeof(InventoryReservationService), _logger);
                return false;
            }
        }

        public async Task<bool> IsReferenceNumberUniqueAsync(string referenceNumber, InventoryReservationType reservationType, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(r => r.ReferenceNumber == referenceNumber && 
                                             r.ReservationType == reservationType && 
                                             !r.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(r => r.Id != excludeId.Value);

                return !await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsReferenceNumberUniqueAsync), typeof(InventoryReservationService), _logger);
                return false;
            }
        }

        #endregion

        #region 業務邏輯方法

        public async Task<ServiceResult> ConvertReservationToSaleAsync(int reservationId, string saleOrderNumber, 
            int? employeeId = null)
        {
            try
            {
                var reservation = await GetByIdAsync(reservationId);
                if (reservation == null)
                    return ServiceResult.Failure("找不到指定的預留記錄");

                if (reservation.ReservationStatus != InventoryReservationStatus.Reserved)
                    return ServiceResult.Failure("只能轉換預留狀態的預留記錄");

                if (reservation.ReservationType != InventoryReservationType.SalesOrder)
                    return ServiceResult.Failure("只能轉換銷售訂單類型的預留記錄");

                var remainingQuantity = reservation.ReservedQuantity - reservation.ReleasedQuantity;
                if (remainingQuantity <= 0)
                    return ServiceResult.Failure("該預留記錄已完全釋放，無法轉換");

                // 完全釋放預留
                var releaseResult = await ReleaseReservationAsync(reservationId, null, 
                    $"轉換為銷售出貨: {saleOrderNumber}", employeeId);

                if (!releaseResult.IsSuccess)
                    return releaseResult;

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConvertReservationToSaleAsync), typeof(InventoryReservationService), _logger);
                return ServiceResult.Failure("轉換預留記錄為銷售時發生錯誤");
            }
        }

        public async Task<List<InventoryReservation>> GetReservationsForStockCheckAsync(int productId, int warehouseId)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => r.ProductId == productId && 
                               r.WarehouseId == warehouseId && 
                               r.ReservationStatus == InventoryReservationStatus.Reserved && 
                               !r.IsDeleted &&
                               (r.ExpiryDate == null || r.ExpiryDate > DateTime.Now))
                    .OrderBy(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetReservationsForStockCheckAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        #endregion

        #region Override Methods

        public override async Task<List<InventoryReservation>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => !r.IsDeleted)
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        public override async Task<InventoryReservation?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), typeof(InventoryReservationService), _logger);
                return null;
            }
        }

        public override async Task<List<InventoryReservation>> SearchAsync(string searchTerm)
        {
            try
            {
                return await _dbSet
                    .Include(r => r.Product)
                    .Include(r => r.Warehouse)
                    .Include(r => r.WarehouseLocation)
                    .Where(r => !r.IsDeleted && 
                               ((r.ReservationNumber != null && r.ReservationNumber.Contains(searchTerm)) ||
                                (r.ReferenceNumber != null && r.ReferenceNumber.Contains(searchTerm)) ||
                                (r.ReservationRemarks != null && r.ReservationRemarks.Contains(searchTerm))))
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), typeof(InventoryReservationService), _logger);
                return new List<InventoryReservation>();
            }
        }

        #endregion
    }
}
