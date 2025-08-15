using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 單位服務實作
    /// </summary>
    public class UnitService : GenericManagementService<Unit>, IUnitService
    {
        /// <summary>
        /// 完整建構子 - 使用 ILogger
        /// </summary>
        public UnitService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Unit>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不使用 ILogger
        /// </summary>
        public UnitService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料，包含轉換關係
        /// </summary>
        public override async Task<List<Unit>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Units
                    .Include(u => u.FromUnitConversions)
                    .Include(u => u.ToUnitConversions)
                    .Where(u => !u.IsDeleted)
                    .OrderBy(u => u.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<Unit>();
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

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Units
                    .Include(u => u.FromUnitConversions)
                    .Include(u => u.ToUnitConversions)
                    .Where(u => !u.IsDeleted &&
                               ((u.UnitName != null && u.UnitName.Contains(searchTerm)) ||
                                (u.Code != null && u.Code.Contains(searchTerm))))
                    .OrderBy(u => u.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, 
                    new { SearchTerm = searchTerm });
                return new List<Unit>();
            }
        }

        /// <summary>
        /// 檢查單位代碼是否存在
        /// </summary>
        public async Task<bool> IsUnitCodeExistsAsync(string unitCode, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Units.Where(u => u.Code == unitCode && !u.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(u => u.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsUnitCodeExistsAsync), GetType(), _logger, 
                    new { UnitCode = unitCode, ExcludeId = excludeId });
                return false;
            }
        }

        /// <summary>
        /// 取得基本單位
        /// </summary>
        public async Task<List<Unit>> GetBaseUnitsAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Units
                    .Where(u => !u.IsDeleted)
                    .OrderBy(u => u.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBaseUnitsAsync), GetType(), _logger);
                return new List<Unit>();
            }
        }

        /// <summary>
        /// 取得單位及其轉換關係
        /// </summary>
        public async Task<Unit?> GetUnitWithConversionsAsync(int unitId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Units
                    .Include(u => u.FromUnitConversions.Where(c => !c.IsDeleted))
                        .ThenInclude(c => c.ToUnit)
                    .Include(u => u.ToUnitConversions.Where(c => !c.IsDeleted))
                        .ThenInclude(c => c.FromUnit)
                    .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetUnitWithConversionsAsync), GetType(), _logger, 
                    new { UnitId = unitId });
                return null;
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

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 直接轉換
                var directConversion = await context.Set<UnitConversion>()
                    .FirstOrDefaultAsync(c => c.FromUnitId == fromUnitId && 
                                             c.ToUnitId == toUnitId && 
                                             !c.IsDeleted && 
                                             c.IsActive);

                if (directConversion != null)
                    return quantity * directConversion.ConversionRate;

                // 反向轉換
                var reverseConversion = await context.Set<UnitConversion>()
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConvertUnitsAsync), GetType(), _logger, 
                    new { FromUnitId = fromUnitId, ToUnitId = toUnitId, Quantity = quantity });
                return null;
            }
        }

        /// <summary>
        /// 取得可轉換的單位列表
        /// </summary>
        public async Task<List<Unit>> GetConvertibleUnitsAsync(int unitId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var convertibleUnitIds = await context.Set<UnitConversion>()
                    .Where(c => !c.IsDeleted && c.IsActive && 
                               (c.FromUnitId == unitId || c.ToUnitId == unitId))
                    .Select(c => c.FromUnitId == unitId ? c.ToUnitId : c.FromUnitId)
                    .Distinct()
                    .ToListAsync();

                return await context.Units
                    .Where(u => !u.IsDeleted && convertibleUnitIds.Contains(u.Id))
                    .OrderBy(u => u.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetConvertibleUnitsAsync), GetType(), _logger, 
                    new { UnitId = unitId });
                return new List<Unit>();
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
                if (string.IsNullOrWhiteSpace(entity.Code))
                    return ServiceResult.Failure("單位代碼為必填");
                
                if (string.IsNullOrWhiteSpace(entity.UnitName))
                    return ServiceResult.Failure("單位名稱為必填");

                // 檢查單位代碼是否重複
                if (await IsUnitCodeExistsAsync(entity.Code, entity.Id))
                    return ServiceResult.Failure("單位代碼已存在");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, 
                    new { UnitId = entity.Id, UnitCode = entity.Code });
                return ServiceResult.Failure("驗證過程中發生錯誤");
            }
        }

        /// <summary>
        /// 覆寫名稱存在檢查（使用單位名稱）
        /// </summary>
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Units.Where(u => u.UnitName == name && !u.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(u => u.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, 
                    new { Name = name, ExcludeId = excludeId });
                return false;
            }
        }
    }
}
