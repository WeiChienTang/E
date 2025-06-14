using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services.Inventory
{
    public class UnitConversionService : GenericManagementService<UnitConversion>, IUnitConversionService
    {
        public UnitConversionService(AppDbContext context) : base(context)
        {
        }

        public override async Task<List<UnitConversion>> GetAllAsync()
        {
            return await _context.UnitConversions
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .Where(uc => !uc.IsDeleted)
                .OrderBy(uc => uc.FromUnit.UnitName)
                .ThenBy(uc => uc.ToUnit.UnitName)
                .ToListAsync();
        }

        public override async Task<UnitConversion?> GetByIdAsync(int id)
        {
            return await _context.UnitConversions
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .FirstOrDefaultAsync(uc => uc.Id == id && !uc.IsDeleted);
        }

        public async Task<ServiceResult<IEnumerable<UnitConversion>>> GetByUnitAsync(int unitId)
        {
            try
            {
                var entities = await _context.UnitConversions
                    .Include(uc => uc.FromUnit)
                    .Include(uc => uc.ToUnit)
                    .Where(uc => (uc.FromUnitId == unitId || uc.ToUnitId == unitId) && !uc.IsDeleted)
                    .OrderBy(uc => uc.FromUnit.UnitName)
                    .ThenBy(uc => uc.ToUnit.UnitName)
                    .ToListAsync();

                return ServiceResult<IEnumerable<UnitConversion>>.Success(entities);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<UnitConversion>>.Failure($"取得單位轉換列表失敗: {ex.Message}");
            }
        }

        public async Task<ServiceResult<decimal?>> GetConversionRateAsync(int fromUnitId, int toUnitId)
        {
            try
            {
                // 直接轉換
                var directConversion = await _context.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.FromUnitId == fromUnitId && 
                                              uc.ToUnitId == toUnitId && 
                                              uc.IsActive && 
                                              !uc.IsDeleted);

                if (directConversion != null)
                {
                    return ServiceResult<decimal?>.Success(directConversion.ConversionRate);
                }

                // 反向轉換
                var reverseConversion = await _context.UnitConversions
                    .FirstOrDefaultAsync(uc => uc.FromUnitId == toUnitId && 
                                              uc.ToUnitId == fromUnitId && 
                                              uc.IsActive && 
                                              !uc.IsDeleted);

                if (reverseConversion != null)
                {
                    return ServiceResult<decimal?>.Success(1 / reverseConversion.ConversionRate);
                }

                return ServiceResult<decimal?>.Success(null);
            }
            catch (Exception ex)
            {
                return ServiceResult<decimal?>.Failure($"取得轉換率失敗: {ex.Message}");
            }
        }

        public override async Task<ServiceResult> ValidateAsync(UnitConversion entity)
        {
            // 檢查是否已存在相同的轉換關係
            var exists = await _context.UnitConversions
                .AnyAsync(uc => uc.FromUnitId == entity.FromUnitId && 
                               uc.ToUnitId == entity.ToUnitId && 
                               uc.Id != entity.Id && 
                               !uc.IsDeleted);

            if (exists)
            {
                return ServiceResult.Failure("已存在相同的單位轉換關係");
            }

            return ServiceResult.Success();
        }

        public override async Task<List<UnitConversion>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _context.UnitConversions
                .Include(uc => uc.FromUnit)
                .Include(uc => uc.ToUnit)
                .Where(uc => !uc.IsDeleted && 
                            (uc.FromUnit.UnitName.Contains(searchTerm) || 
                             uc.ToUnit.UnitName.Contains(searchTerm) ||
                             uc.FromUnit.UnitCode.Contains(searchTerm) ||
                             uc.ToUnit.UnitCode.Contains(searchTerm)))
                .OrderBy(uc => uc.FromUnit.UnitName)
                .ThenBy(uc => uc.ToUnit.UnitName)
                .ToListAsync();
        }
    }
}
