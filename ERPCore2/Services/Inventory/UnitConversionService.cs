using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    public class UnitConversionService : GenericManagementService<UnitConversion>, IUnitConversionService
    {
        // 完整建構子
        public UnitConversionService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<UnitConversion>> logger) : base(contextFactory, logger)
        {
        }

        // 簡易建構子
        public UnitConversionService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public override async Task<List<UnitConversion>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.UnitConversions
                    .Include(uc => uc.FromUnit)
                    .Include(uc => uc.ToUnit)
                    .AsQueryable()
                    .OrderBy(uc => uc.FromUnit.Name)
                    .ThenBy(uc => uc.ToUnit.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    ServiceType = GetType().Name 
                });
                throw;
            }
        }

        public override async Task<UnitConversion?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.UnitConversions
                    .Include(uc => uc.FromUnit)
                    .Include(uc => uc.ToUnit)
                    .FirstOrDefaultAsync(uc => uc.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { 
                    Id = id,
                    ServiceType = GetType().Name 
                });
                throw;
            }
        }

        public async Task<ServiceResult<IEnumerable<UnitConversion>>> GetByUnitAsync(int unitId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entities = await context.UnitConversions
                    .Include(uc => uc.FromUnit)
                    .Include(uc => uc.ToUnit)
                    .Where(uc => (uc.FromUnitId == unitId || uc.ToUnitId == unitId))
                    .OrderBy(uc => uc.FromUnit.Name)
                    .ThenBy(uc => uc.ToUnit.Name)
                    .ToListAsync();

                return ServiceResult<IEnumerable<UnitConversion>>.Success(entities);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByUnitAsync), GetType(), _logger, new { 
                    UnitId = unitId,
                    ServiceType = GetType().Name 
                });
                return ServiceResult<IEnumerable<UnitConversion>>.Failure("取得單位轉換列表時發生錯誤");
            }
        }

        public async Task<ServiceResult<decimal?>> GetConversionRateAsync(int fromUnitId, int toUnitId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // 直接轉換
                var directConversion = await context.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.FromUnitId == fromUnitId && 
                                              uc.ToUnitId == toUnitId && 
                                              uc.IsActive);

                if (directConversion != null)
                {
                    return ServiceResult<decimal?>.Success(directConversion.ConversionRate);
                }

                // 反向轉換
                var reverseConversion = await context.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.FromUnitId == toUnitId && 
                                              uc.ToUnitId == fromUnitId && 
                                              uc.IsActive);

                if (reverseConversion != null)
                {
                    return ServiceResult<decimal?>.Success(1 / reverseConversion.ConversionRate);
                }

                return ServiceResult<decimal?>.Success(null);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetConversionRateAsync), GetType(), _logger, new { 
                    FromUnitId = fromUnitId,
                    ToUnitId = toUnitId,
                    ServiceType = GetType().Name 
                });
                return ServiceResult<decimal?>.Failure("取得轉換率時發生錯誤");
            }
        }

        public override async Task<ServiceResult> ValidateAsync(UnitConversion entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // 檢查是否已存在相同的轉換關係
                var exists = await context.UnitConversions
                    .AnyAsync(uc => uc.FromUnitId == entity.FromUnitId && 
                                   uc.ToUnitId == entity.ToUnitId && 
                                   uc.Id != entity.Id);

                if (exists)
                {
                    return ServiceResult.Failure("已存在相同的單位轉換關係");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    EntityId = entity.Id,
                    FromUnitId = entity.FromUnitId,
                    ToUnitId = entity.ToUnitId,
                    ServiceType = GetType().Name 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public override async Task<List<UnitConversion>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.UnitConversions
                    .Include(uc => uc.FromUnit)
                    .Include(uc => uc.ToUnit)
                    .Where(uc => ((uc.FromUnit.Name != null && uc.FromUnit.Name.Contains(searchTerm)) || 
                                 (uc.ToUnit.Name != null && uc.ToUnit.Name.Contains(searchTerm)) ||
                                 (uc.FromUnit.Code != null && uc.FromUnit.Code.Contains(searchTerm)) ||
                                 (uc.ToUnit.Code != null && uc.ToUnit.Code.Contains(searchTerm))))
                    .OrderBy(uc => uc.FromUnit.Name)
                    .ThenBy(uc => uc.ToUnit.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                throw;
            }
        }
    }
}


