using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 磅秤紀錄服務實作
    /// </summary>
    public class ScaleRecordService : GenericManagementService<ScaleRecord>, IScaleRecordService
    {
        private readonly IInventoryStockService? _inventoryStockService;

        public ScaleRecordService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public ScaleRecordService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ScaleRecord>> logger) : base(contextFactory, logger)
        {
        }

        public ScaleRecordService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<ScaleRecord>> logger,
            IInventoryStockService inventoryStockService,
            IFieldDisplaySettingService? fieldDisplaySettingService = null) : base(contextFactory, logger)
        {
            _inventoryStockService = inventoryStockService;
            _fieldDisplaySettingService = fieldDisplaySettingService;
        }

        #region 覆寫基底方法

        protected override IQueryable<ScaleRecord> BuildGetAllQuery(AppDbContext context)
        {
            return context.ScaleRecords
                .Include(sr => sr.Vehicle)
                .Include(sr => sr.Item)
                .Include(sr => sr.Customer)
                .Include(sr => sr.Warehouse)
                .Include(sr => sr.WarehouseLocation)
                .OrderByDescending(sr => sr.RecordDate)
                .ThenByDescending(sr => sr.Id);
        }

        public override async Task<ScaleRecord?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScaleRecords
                    .Include(sr => sr.Vehicle)
                    .Include(sr => sr.Item)
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Warehouse)
                    .Include(sr => sr.WarehouseLocation)
                    .FirstOrDefaultAsync(sr => sr.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<ScaleRecord>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var lowerSearchTerm = searchTerm.ToLower();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScaleRecords
                    .Include(sr => sr.Vehicle)
                    .Include(sr => sr.Item)
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Warehouse)
                    .Include(sr => sr.WarehouseLocation)
                    .Where(sr => (sr.Code != null && sr.Code.ToLower().Contains(lowerSearchTerm)) ||
                                 (sr.Vehicle != null && sr.Vehicle.LicensePlate.ToLower().Contains(lowerSearchTerm)) ||
                                 (sr.Item != null && sr.Item.Name != null && sr.Item.Name.ToLower().Contains(lowerSearchTerm)) ||
                                 (sr.Warehouse != null && sr.Warehouse.Name.ToLower().Contains(lowerSearchTerm)) ||
                                 (sr.Customer != null && sr.Customer.CompanyName != null &&
                                  sr.Customer.CompanyName.ToLower().Contains(lowerSearchTerm)))
                    .OrderByDescending(sr => sr.RecordDate)
                    .ThenByDescending(sr => sr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(ScaleRecord entity)
        {
            try
            {
                var errors = new List<string>();

                if (!entity.VehicleId.HasValue || entity.VehicleId.Value <= 0)
                    errors.Add("車輛為必填欄位");

                if (!entity.WarehouseId.HasValue || entity.WarehouseId.Value <= 0)
                    errors.Add("入庫倉庫為必填欄位");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 若同時選擇了車輛與客戶，驗證車輛的所屬客戶是否相符
                if (entity.VehicleId.HasValue && entity.VehicleId.Value > 0 && entity.CustomerId.HasValue)
                {
                    var vehicle = await context.Vehicles
                        .AsNoTracking()
                        .Select(v => new { v.Id, v.CustomerId })
                        .FirstOrDefaultAsync(v => v.Id == entity.VehicleId);

                    if (vehicle?.CustomerId.HasValue == true && vehicle.CustomerId != entity.CustomerId)
                        errors.Add("所選車輛不屬於此客戶，請確認車輛與客戶是否匹配");
                }

                // 若同時選擇了倉庫與庫位，驗證庫位是否屬於所選倉庫
                if (entity.WarehouseId.HasValue && entity.WarehouseId.Value > 0 && entity.WarehouseLocationId.HasValue)
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

        public async Task<bool> IsScaleRecordCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.ScaleRecords.Where(sr => sr.Code == code);

                if (excludeId.HasValue)
                    query = query.Where(sr => sr.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsScaleRecordCodeExistsAsync), GetType(), _logger, new { Code = code, ExcludeId = excludeId });
                return false;
            }
        }

        public async Task<List<ScaleRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScaleRecords
                    .Include(sr => sr.Vehicle)
                    .Include(sr => sr.Item)
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Warehouse)
                    .Include(sr => sr.WarehouseLocation)
                    .Where(sr => sr.RecordDate >= startDate && sr.RecordDate <= endDate)
                    .OrderByDescending(sr => sr.RecordDate)
                    .ThenByDescending(sr => sr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDateRangeAsync), GetType(), _logger, new { StartDate = startDate, EndDate = endDate });
                throw;
            }
        }

        public async Task<List<ScaleRecord>> GetByVehicleAsync(int vehicleId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScaleRecords
                    .Include(sr => sr.Vehicle)
                    .Include(sr => sr.Item)
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Warehouse)
                    .Include(sr => sr.WarehouseLocation)
                    .Where(sr => sr.VehicleId == vehicleId)
                    .OrderByDescending(sr => sr.RecordDate)
                    .ThenByDescending(sr => sr.Id)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByVehicleAsync), GetType(), _logger, new { VehicleId = vehicleId });
                throw;
            }
        }

        public async Task<List<ScaleRecord>> GetByCustomerAsync(int customerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ScaleRecords
                    .Include(sr => sr.Vehicle)
                    .Include(sr => sr.Item)
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Warehouse)
                    .Include(sr => sr.WarehouseLocation)
                    .Where(sr => sr.CustomerId == customerId)
                    .OrderByDescending(sr => sr.RecordDate)
                    .ThenByDescending(sr => sr.Id)
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
        /// 新增磅秤紀錄後確認入庫
        /// 若磅秤紀錄沒有關聯品項，則跳過入庫（不視為錯誤）
        /// </summary>
        public async Task<ServiceResult> ConfirmScaleReceiptAsync(int id)
        {
            try
            {
                if (_inventoryStockService == null)
                    return ServiceResult.Failure("庫存服務未初始化");

                using var context = await _contextFactory.CreateDbContextAsync();

                var scaleRecord = await context.ScaleRecords
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                if (scaleRecord == null)
                    return ServiceResult.Failure("找不到指定的磅秤紀錄");

                // 若無關聯品項，跳過入庫
                if (!scaleRecord.ItemId.HasValue)
                    return ServiceResult.Success();

                var quantity = scaleRecord.NetWeight ?? 0;
                if (quantity <= 0)
                    return ServiceResult.Success();

                return await _inventoryStockService.AddStockAsync(
                    scaleRecord.ItemId.Value,
                    scaleRecord.WarehouseId!.Value,
                    quantity,
                    InventoryTransactionTypeEnum.WasteReceiving,
                    scaleRecord.Code ?? string.Empty,
                    null,
                    scaleRecord.WarehouseLocationId,
                    $"磅秤紀錄收料 - {scaleRecord.Code ?? string.Empty}",
                    null,
                    scaleRecord.RecordDate,
                    null,
                    sourceDocumentType: InventorySourceDocumentTypes.ScaleRecord,
                    sourceDocumentId: scaleRecord.Id
                );
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConfirmScaleReceiptAsync), GetType(), _logger, new { Id = id });
                return ServiceResult.Failure("磅秤紀錄收料確認過程發生錯誤");
            }
        }

        /// <summary>
        /// 編輯磅秤紀錄後先逆轉舊庫存，再以當前數值重新入庫（Void and Repost）
        /// </summary>
        public async Task<ServiceResult> ReverseAndRepostScaleInventoryAsync(int id)
        {
            try
            {
                var reverseResult = await ReverseScaleInventoryAsync(id);
                if (!reverseResult.IsSuccess)
                    return reverseResult;

                return await ConfirmScaleReceiptAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ReverseAndRepostScaleInventoryAsync), GetType(), _logger, new { Id = id });
                return ServiceResult.Failure("磅秤紀錄庫存更新過程發生錯誤");
            }
        }

        /// <summary>
        /// 逆轉此磅秤紀錄的所有庫存影響
        /// </summary>
        private async Task<ServiceResult> ReverseScaleInventoryAsync(int id)
        {
            if (_inventoryStockService == null)
                return ServiceResult.Failure("庫存服務未初始化");

            using var context = await _contextFactory.CreateDbContextAsync();

            var scaleRecord = await context.ScaleRecords
                .FirstOrDefaultAsync(sr => sr.Id == id);

            if (scaleRecord == null)
                return ServiceResult.Failure("找不到指定的磅秤紀錄");

            if (!scaleRecord.ItemId.HasValue)
                return ServiceResult.Success();

            // 查詢此磅秤紀錄的所有庫存異動明細
            var allDetails = await context.InventoryTransactionDetails
                .Include(d => d.InventoryTransaction)
                .Include(d => d.InventoryStockDetail)
                .Where(d => d.InventoryTransaction.SourceDocumentType == InventorySourceDocumentTypes.ScaleRecord &&
                            d.InventoryTransaction.SourceDocumentId == id)
                .ToListAsync();

            if (!allDetails.Any())
                return ServiceResult.Success();

            var groups = allDetails
                .GroupBy(d => new
                {
                    ItemId = d.ItemId,
                    WarehouseId = d.InventoryStockDetail != null ? d.InventoryStockDetail.WarehouseId : d.InventoryTransaction.WarehouseId,
                    LocationId = d.WarehouseLocationId
                })
                .Select(g => new
                {
                    g.Key.ItemId,
                    g.Key.WarehouseId,
                    g.Key.LocationId,
                    NetQuantity = g.Sum(d => d.Quantity)
                })
                .Where(g => g.NetQuantity > 0.0001m)
                .ToList();

            foreach (var group in groups)
            {
                var result = await _inventoryStockService.ReduceStockAsync(
                    group.ItemId,
                    group.WarehouseId,
                    group.NetQuantity,
                    InventoryTransactionTypeEnum.WasteReceiving,
                    scaleRecord.Code ?? string.Empty,
                    group.LocationId,
                    $"磅秤紀錄逆轉 - {scaleRecord.Code ?? string.Empty}",
                    sourceDocumentType: InventorySourceDocumentTypes.ScaleRecord,
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

        public override async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                if (_inventoryStockService != null)
                {
                    var reverseResult = await ReverseScaleInventoryAsync(id);
                    if (!reverseResult.IsSuccess)
                        return ServiceResult.Failure($"刪除失敗，無法逆轉庫存：{reverseResult.ErrorMessage}");
                }

                return await base.PermanentDeleteAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger, new { Id = id });
                return ServiceResult.Failure("刪除磅秤紀錄時發生錯誤");
            }
        }

        #endregion

        #region 伺服器端分頁

        public async Task<(List<ScaleRecord> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<ScaleRecord>, IQueryable<ScaleRecord>>? filterFunc,
            int pageNumber,
            int pageSize)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                IQueryable<ScaleRecord> query = context.ScaleRecords.Include(sr => sr.Item);
                if (filterFunc != null) query = filterFunc(query);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(sr => sr.RecordDate).ThenByDescending(sr => sr.Id)
                    .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedWithFiltersAsync), GetType(), _logger);
                return (new List<ScaleRecord>(), 0);
            }
        }

        #endregion
    }
}
