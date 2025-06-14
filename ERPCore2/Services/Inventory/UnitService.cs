using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;
using ERPCore2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 單位服務實作
    /// </summary>
    public class UnitService : GenericManagementService<Unit>, IUnitService
    {
        public UnitService(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料，包含轉換關係
        /// </summary>
        public override async Task<List<Unit>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.FromUnitConversions)
                .Include(u => u.ToUnitConversions)
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.UnitCode)
                .ToListAsync();
        }

        /// <summary>
        /// 覆寫搜尋功能
        /// </summary>
        public override async Task<List<Unit>> SearchAsync(string searchTerm)
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

        /// <summary>
        /// 檢查單位代碼是否存在
        /// </summary>
        public async Task<bool> IsUnitCodeExistsAsync(string unitCode, int? excludeId = null)
        {
            var query = _dbSet.Where(u => u.UnitCode == unitCode && !u.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// 取得基本單位
        /// </summary>
        public async Task<List<Unit>> GetBaseUnitsAsync()
        {
            return await _dbSet
                .Where(u => !u.IsDeleted && u.IsBaseUnit)
                .OrderBy(u => u.UnitCode)
                .ToListAsync();
        }

        /// <summary>
        /// 取得單位及其轉換關係
        /// </summary>
        public async Task<Unit?> GetUnitWithConversionsAsync(int unitId)
        {
            return await _dbSet
                .Include(u => u.FromUnitConversions.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.ToUnit)
                .Include(u => u.ToUnitConversions.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.FromUnit)
                .FirstOrDefaultAsync(u => u.Id == unitId && !u.IsDeleted);
        }

        /// <summary>
        /// 計算單位轉換
        /// </summary>
        public async Task<decimal?> ConvertUnitsAsync(int fromUnitId, int toUnitId, decimal quantity)
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

        /// <summary>
        /// 取得可轉換的單位列表
        /// </summary>
        public async Task<List<Unit>> GetConvertibleUnitsAsync(int unitId)
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

        /// <summary>
        /// 覆寫驗證方法
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Unit entity)
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

        /// <summary>
        /// 覆寫名稱存在檢查（使用單位名稱）
        /// </summary>
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(u => u.UnitName == name && !u.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}