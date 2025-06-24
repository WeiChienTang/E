using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    public class WeatherService : GenericManagementService<Weather>, IWeatherService
    {
        public WeatherService(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料方法，加入排序
        /// </summary>
        public override async Task<List<Weather>> GetAllAsync()
        {
            return await _dbSet
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        /// <summary>
        /// 覆寫搜尋方法，實作天氣特定的搜尋邏輯
        /// </summary>
        public override async Task<List<Weather>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Where(w => !w.IsDeleted &&
                           (w.Name.Contains(searchTerm) ||
                            w.Code.Contains(searchTerm) ||
                            (w.Description != null && w.Description.Contains(searchTerm))))
                .OrderBy(w => w.Name)
                .ToListAsync();
        }    /// <summary>
        /// 覆寫驗證方法，添加天氣特定的驗證規則
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Weather entity)
        {
            // 基本驗證
            if (string.IsNullOrWhiteSpace(entity.Name))
                return ServiceResult.Failure("天氣名稱為必填");

            // 檢查代碼是否重複
            if (await IsCodeExistsAsync(entity.Code, entity.Id))
                return ServiceResult.Failure("天氣代碼已存在");

            // 檢查名稱是否重複
            if (await IsNameExistsAsync(entity.Name, entity.Id))
                return ServiceResult.Failure("天氣名稱已存在");

            return ServiceResult.Success();
        }

        /// <summary>
        /// 覆寫名稱存在檢查
        /// </summary>
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(w => w.Name == name && !w.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// 檢查天氣代碼是否已存在
        /// </summary>
        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            var query = _dbSet.Where(w => w.Code == code && !w.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(w => w.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// 根據代碼取得天氣資料
        /// </summary>
        public async Task<Weather?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Where(w => w.Code == code && !w.IsDeleted)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根據溫度範圍取得天氣資料
        /// </summary>
        public async Task<List<Weather>> GetByTemperatureRangeAsync(decimal minTemperature, decimal maxTemperature)
        {
            return await _dbSet
                .Where(w => !w.IsDeleted &&
                           w.ReferenceTemperature.HasValue &&
                           w.ReferenceTemperature >= minTemperature &&
                           w.ReferenceTemperature <= maxTemperature)
                .OrderBy(w => w.ReferenceTemperature)
                .ToListAsync();
        }
    }
}