using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 單位服務實作
    /// </summary>
    public class UnitService : GenericManagementService<Unit>, IUnitService
    {
        public UnitService(
            AppDbContext context, 
            ILogger<GenericManagementService<Unit>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料，包含轉換關係
        /// </summary>
        public override async Task<List<Unit>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(u => u.FromUnitConversions)
                    .Include(u => u.ToUnitConversions)
                    .Where(u => !u.IsDeleted)
                    .OrderBy(u => u.UnitCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting all units");
                throw;
            }
        }

        /// <summary>
        /// 覆寫搜尋功能
        /// </summary>
        public override async Task<List<Unit>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Include(u => u.FromUnitConversions)
                    .Include(u => u.ToUnitConversions)
                    .Where(u => !u.IsDeleted &&
                               (u.UnitName.Contains(searchTerm) ||
                                u.UnitCode.Contains(searchTerm) ||
                                (u.Symbol != null && u.Symbol.Contains(searchTerm))))
                    .OrderBy(u => u.UnitCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error searching units with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// 檢查單位代碼是否存在
        /// </summary>
        public async Task<bool> IsUnitCodeExistsAsync(string unitCode, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(u => u.UnitCode == unitCode && !u.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(u => u.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsUnitCodeExistsAsync),
                    UnitCode = unitCode,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking unit code exists: {UnitCode}", unitCode);
                throw;
            }
        }

        /// <summary>
        /// 取得基本單位
        /// </summary>
        public async Task<List<Unit>> GetBaseUnitsAsync()
        {
            try
            {
                return await _dbSet
                    .Where(u => !u.IsDeleted && u.IsBaseUnit)
                    .OrderBy(u => u.UnitCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetBaseUnitsAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting base units");
                throw;
            }
        }

        /// <summary>
        /// 取得單位及其轉換關係
        /// </summary>
        public async Task<Unit?> GetUnitWithConversionsAsync(int unitId)
        {
            try
            {
                return await _dbSet
                    .Include(u => u.FromUnitConversions.Where(c => !c.IsDeleted))
                        .ThenInclude(c => c.ToUnit)
                    .Include(u => u.ToUnitConversions.Where(c => !c.IsDeleted))
                        .ThenInclude(c => c.FromUnit)
                    .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetUnitWithConversionsAsync),
                    UnitId = unitId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting unit with conversions for unit ID {UnitId}", unitId);
                throw;
            }
        }

        /// <summary>
        /// 計算單位轉換
        /// </summary>
        public async Task<decimal?> ConvertUnitsAsync(int fromUnitId, int toUnitId, decimal quantity)
        {
            try
            {
                if (fromUnitId == toUnitId)
                    return quantity;

                // 直接轉換
                var directConversion = await _context.Set<UnitConversion>()
                    .FirstOrDefaultAsync(c => c.FromUnitId == fromUnitId && 
                                             c.ToUnitId == toUnitId && 
                                             !c.IsDeleted && 
                                             c.IsActive);

                if (directConversion != null)
                    return quantity * directConversion.ConversionRate;

                // 反向轉換
                var reverseConversion = await _context.Set<UnitConversion>()
                    .FirstOrDefaultAsync(c => c.FromUnitId == toUnitId && 
                                             c.ToUnitId == fromUnitId && 
                                             !c.IsDeleted && 
                                             c.IsActive);

                if (reverseConversion != null && reverseConversion.ConversionRate != 0)
                    return quantity / reverseConversion.ConversionRate;

                // 找不到轉換關係
                return null;
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ConvertUnitsAsync),
                    FromUnitId = fromUnitId,
                    ToUnitId = toUnitId,
                    Quantity = quantity,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error converting units from {FromUnitId} to {ToUnitId} with quantity {Quantity}", 
                    fromUnitId, toUnitId, quantity);
                throw;
            }
        }

        /// <summary>
        /// 取得可轉換的單位列表
        /// </summary>
        public async Task<List<Unit>> GetConvertibleUnitsAsync(int unitId)
        {
            try
            {
                var convertibleUnitIds = await _context.Set<UnitConversion>()
                    .Where(c => !c.IsDeleted && c.IsActive && 
                               (c.FromUnitId == unitId || c.ToUnitId == unitId))
                    .Select(c => c.FromUnitId == unitId ? c.ToUnitId : c.FromUnitId)
                    .Distinct()
                    .ToListAsync();

                return await _dbSet
                    .Where(u => !u.IsDeleted && convertibleUnitIds.Contains(u.Id))
                    .OrderBy(u => u.UnitCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetConvertibleUnitsAsync),
                    UnitId = unitId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting convertible units for unit ID {UnitId}", unitId);
                throw;
            }
        }

        /// <summary>
        /// 覆寫驗證方法
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Unit entity)
        {
            try
            {
                // 基本驗證
                if (string.IsNullOrWhiteSpace(entity.UnitCode))
                    return ServiceResult.Failure("單位代碼為必填");
                
                if (string.IsNullOrWhiteSpace(entity.UnitName))
                    return ServiceResult.Failure("單位名稱為必填");

                // 檢查單位代碼是否重複
                if (await IsUnitCodeExistsAsync(entity.UnitCode, entity.Id))
                    return ServiceResult.Failure("單位代碼已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ValidateAsync),
                    UnitId = entity.Id,
                    UnitCode = entity.UnitCode,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error validating unit with code {UnitCode}", entity.UnitCode);
                throw;
            }
        }

        /// <summary>
        /// 覆寫名稱存在檢查（使用單位名稱）
        /// </summary>
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(u => u.UnitName == name && !u.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(u => u.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsNameExistsAsync),
                    Name = name,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking name exists: {Name}", name);
                throw;
            }
        }
    }
}