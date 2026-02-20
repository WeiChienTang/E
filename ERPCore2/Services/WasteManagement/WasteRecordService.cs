using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廢料記錄服務實作
    /// </summary>
    public class WasteRecordService : GenericManagementService<WasteRecord>, IWasteRecordService
    {
        private readonly IInventoryStockService? _inventoryStockService;

        public WasteRecordService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public WasteRecordService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<WasteRecord>> logger) : base(contextFactory, logger)
        {
        }

        public WasteRecordService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<WasteRecord>> logger,
            IInventoryStockService inventoryStockService) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
        }

        #region 覆寫基底方法

        public override async Task<List<WasteRecord>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .Include(wr => wr.Warehouse)
                    .Include(wr => wr.WarehouseLocation)
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<WasteRecord?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                        .ThenInclude(wt => wt.Product)
                    .Include(wr => wr.Customer)
                    .Include(wr => wr.Warehouse)
                    .Include(wr => wr.WarehouseLocation)
                    .FirstOrDefaultAsync(wr => wr.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<WasteRecord>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .Include(wr => wr.Warehouse)
                    .Include(wr => wr.WarehouseLocation)
                    .Where(wr => (wr.Code != null && wr.Code.ToLower().Contains(lowerSearchTerm)) ||
                                 wr.Vehicle.LicensePlate.ToLower().Contains(lowerSearchTerm) ||
                                 wr.WasteType.Name.ToLower().Contains(lowerSearchTerm) ||
                                 wr.Warehouse.Name.ToLower().Contains(lowerSearchTerm) ||
                                 (wr.Customer != null && wr.Customer.CompanyName != null &&
                                  wr.Customer.CompanyName.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(WasteRecord entity)
        {
            try
            {
                var errors = new List<string>();

                if (entity.VehicleId <= 0)
                    errors.Add("車輛為必填欄位");

                if (entity.WasteTypeId <= 0)
                    errors.Add("廢料類型為必填欄位");

                if (entity.WarehouseId <= 0)
                    errors.Add("入庫倉庫為必填欄位");

                if (!entity.TotalWeight.HasValue || entity.TotalWeight.Value <= 0)
                    errors.Add("總重量為必填欄位且必須大於 0");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 若同時選擇了車輛與客戶，驗證車輛的所屬客戶是否相符
                if (entity.VehicleId > 0 && entity.CustomerId.HasValue)
                {
                    var vehicle = await context.Vehicles
                        .AsNoTracking()
                        .Select(v => new { v.Id, v.CustomerId })
                        .FirstOrDefaultAsync(v => v.Id == entity.VehicleId);

                    if (vehicle?.CustomerId.HasValue == true && vehicle.CustomerId != entity.CustomerId)
                        errors.Add("所選車輛不屬於此客戶，請確認車輛與客戶是否匹配");
                }

                // 若同時選擇了倉庫與庫位，驗證庫位是否屬於所選倉庫
                if (entity.WarehouseId > 0 && entity.WarehouseLocationId.HasValue)
                {
                    var location = await context.WarehouseLocations
                        .AsNoTracking()
                        .Select(wl => new { wl.Id, wl.WarehouseId })
                        .FirstOrDefaultAsync(wl => wl.Id == entity.WarehouseLocationId.Value);

                    if (location != null && location.WarehouseId != entity.WarehouseId)
                        errors.Add("所選庫位不屬於此倉庫，請確認庫位與倉庫是否匹配");
                }

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<bool> IsWasteRecordCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.WasteRecords.Where(wr => wr.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(wr => wr.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsWasteRecordCodeExistsAsync), GetType(), _logger, new { Code = code, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<List<WasteRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .Include(wr => wr.Warehouse)
                    .Include(wr => wr.WarehouseLocation)
                    .Where(wr => wr.RecordDate >= startDate && wr.RecordDate <= endDate)
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new { StartDate = startDate, EndDate = endDate });
                throw;
            }
        }

        public async Task<List<WasteRecord>> GetByVehicleAsync(int vehicleId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .Include(wr => wr.Warehouse)
                    .Include(wr => wr.WarehouseLocation)
                    .Where(wr => wr.VehicleId == vehicleId)
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByVehicleAsync), GetType(), _logger, new { VehicleId = vehicleId });
                throw;
            }
        }

        public async Task<List<WasteRecord>> GetByCustomerAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.WasteRecords
                    .Include(wr => wr.Vehicle)
                    .Include(wr => wr.WasteType)
                    .Include(wr => wr.Customer)
                    .Include(wr => wr.Warehouse)
                    .Include(wr => wr.WarehouseLocation)
                    .Where(wr => wr.CustomerId == customerId)
                    .OrderByDescending(wr => wr.RecordDate)
                    .ThenByDescending(wr => wr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByCustomerAsync), GetType(), _logger, new { CustomerId = customerId });
                throw;
            }
        }

        #endregion

        #region 庫存操作方法

        /// <summary>
        /// 新增廢料記錄後確認入庫
        /// 若廢料類型沒有關聯商品，則跳過入庫（不視為錯誤）
        /// </summary>
        public async Task<ServiceResult> ConfirmWasteReceiptAsync(int id)
        {
            try
            {
                if (_inventoryStockService == null)
                    return ServiceResult.Failure("庫存服務未初始化");

                using var context = await _contextFactory.CreateDbContextAsync();

                var wasteRecord = await context.WasteRecords
                    .Include(wr => wr.WasteType)
                    .FirstOrDefaultAsync(wr => wr.Id == id);

                if (wasteRecord == null)
                    return ServiceResult.Failure("找不到指定的廢料記錄");

                // 若廢料類型無關聯商品，跳過入庫
                if (!wasteRecord.WasteType.ProductId.HasValue)
                    return ServiceResult.Success();

                var quantity = wasteRecord.TotalWeight ?? 0;
                if (quantity <= 0)
                    return ServiceResult.Success();

                return await _inventoryStockService.AddStockAsync(
                    wasteRecord.WasteType.ProductId.Value,
                    wasteRecord.WarehouseId,
                    quantity,
                    InventoryTransactionTypeEnum.WasteReceiving,
                    wasteRecord.Code ?? string.Empty,
                    null,
                    wasteRecord.WarehouseLocationId,
                    $"廢料收料 - {wasteRecord.Code ?? string.Empty}",
                    null,
                    wasteRecord.RecordDate,
                    null,
                    sourceDocumentType: InventorySourceDocumentTypes.WasteRecord,
                    sourceDocumentId: wasteRecord.Id
                );
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConfirmWasteReceiptAsync), GetType(), _logger, new { Id = id });
                return ServiceResult.Failure("廢料收料確認過程發生錯誤");
            }
        }

        /// <summary>
        /// 編輯廢料記錄後先逆轉舊庫存，再以當前數值重新入庫（Void and Repost）
        /// 此方式確保倉庫或庫位變更時，舊倉庫庫存正確移除，新倉庫庫存正確新增
        /// </summary>
        public async Task<ServiceResult> ReverseAndRepostWasteInventoryAsync(int id)
        {
            try
            {
                var reverseResult = await ReverseWasteInventoryAsync(id);
                if (!reverseResult.IsSuccess)
                    return reverseResult;

                return await ConfirmWasteReceiptAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReverseAndRepostWasteInventoryAsync), GetType(), _logger, new { Id = id });
                return ServiceResult.Failure("廢料庫存更新過程發生錯誤");
            }
        }

        /// <summary>
        /// 逆轉此廢料記錄的所有庫存影響
        /// 查詢歷史異動明細的 signed 淨值，對有淨正值的倉庫/庫位執行 ReduceStock
        /// </summary>
        private async Task<ServiceResult> ReverseWasteInventoryAsync(int id)
        {
            if (_inventoryStockService == null)
                return ServiceResult.Failure("庫存服務未初始化");

            using var context = await _contextFactory.CreateDbContextAsync();

            var wasteRecord = await context.WasteRecords
                .Include(wr => wr.WasteType)
                .FirstOrDefaultAsync(wr => wr.Id == id);

            if (wasteRecord == null)
                return ServiceResult.Failure("找不到指定的廢料記錄");

            if (!wasteRecord.WasteType.ProductId.HasValue)
                return ServiceResult.Success();

            // 查詢此廢料記錄的所有庫存異動明細（含 Delete 操作，用於計算正確淨值）
            var allDetails = await context.InventoryTransactionDetails
                .Include(d => d.InventoryTransaction)
                .Include(d => d.InventoryStockDetail)
                .Where(d => d.InventoryTransaction.SourceDocumentType == InventorySourceDocumentTypes.WasteRecord &&
                            d.InventoryTransaction.SourceDocumentId == id)
                .ToListAsync();

            if (!allDetails.Any())
                return ServiceResult.Success();

            // 以 signed quantity 加總計算每個（倉庫, 庫位）的淨庫存貢獻
            // AddStock 存正數、ReduceStock 存負數，加總後 > 0 表示目前有淨入庫需要逆轉
            // 使用 InventoryStockDetail.WarehouseId 而非 InventoryTransaction.WarehouseId，
            // 因為 GetOrCreateTransactionAsync 會重用同一 Transaction（含原始倉庫），
            // 但 InventoryStockDetail 才記錄實際入庫倉庫
            var groups = allDetails
                .GroupBy(d => new
                {
                    ProductId = d.ProductId,
                    WarehouseId = d.InventoryStockDetail != null ? d.InventoryStockDetail.WarehouseId : d.InventoryTransaction.WarehouseId,
                    LocationId = d.WarehouseLocationId
                })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.WarehouseId,
                    g.Key.LocationId,
                    NetQuantity = g.Sum(d => d.Quantity)
                })
                .Where(g => g.NetQuantity > 0.0001m)
                .ToList();

            foreach (var group in groups)
            {
                var result = await _inventoryStockService.ReduceStockAsync(
                    group.ProductId,
                    group.WarehouseId,
                    group.NetQuantity,
                    InventoryTransactionTypeEnum.WasteReceiving,
                    wasteRecord.Code ?? string.Empty,
                    group.LocationId,
                    $"廢料記錄逆轉 - {wasteRecord.Code ?? string.Empty}",
                    sourceDocumentType: InventorySourceDocumentTypes.WasteRecord,
                    sourceDocumentId: id,
                    operationType: InventoryOperationTypeEnum.Delete
                );

                if (!result.IsSuccess)
                    return ServiceResult.Failure($"庫存逆轉失敗（倉庫 {group.WarehouseId}）：{result.ErrorMessage}");
            }

            return ServiceResult.Success();
        }

        #endregion

        #region 覆寫刪除方法

        /// <summary>
        /// 永久刪除廢料記錄前先逆轉庫存，確保庫存資料一致性
        /// </summary>
        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                if (_inventoryStockService != null)
                {
                    var reverseResult = await ReverseWasteInventoryAsync(id);
                    if (!reverseResult.IsSuccess)
                        return ServiceResult.Failure($"刪除失敗，無法逆轉庫存：{reverseResult.ErrorMessage}");
                }

                return await base.PermanentDeleteAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new { Id = id });
                return ServiceResult.Failure("刪除廢料記錄時發生錯誤");
            }
        }

        #endregion
    }
}
