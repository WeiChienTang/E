using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 材質服務實作
    /// </summary>
    public class MaterialService : GenericManagementService<Material>, IMaterialService
    {
        public MaterialService(
            AppDbContext context, 
            ILogger<GenericManagementService<Material>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        /// <summary>
        /// 覆寫取得所有資料方法，包含供應商關聯資料並加入排序
        /// </summary>
        public override async Task<List<Material>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(m => m.Supplier)
                    .Where(m => !m.IsDeleted)
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting all materials");
                throw;
            }
        }

        /// <summary>
        /// 覆寫取得單一資料方法，包含供應商關聯資料
        /// </summary>
        public override async Task<Material?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(m => m.Supplier)
                    .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    Id = id
                });
                _logger.LogError(ex, "Error getting material by id {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// 覆寫搜尋方法，實作材質特定的搜尋邏輯
        /// </summary>
        public override async Task<List<Material>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Include(m => m.Supplier)
                    .Where(m => !m.IsDeleted &&
                               (m.Name.Contains(searchTerm) ||
                                m.Code.Contains(searchTerm) ||
                                (m.Description != null && m.Description.Contains(searchTerm)) ||
                                (m.Category != null && m.Category.Contains(searchTerm)) ||
                                (m.Supplier != null && m.Supplier.CompanyName.Contains(searchTerm))))
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                _logger.LogError(ex, "Error searching materials with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        /// <summary>
        /// 覆寫驗證方法，添加材質特定的驗證規則
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Material entity)
        {
            try
            {
                // 基本驗證
                if (string.IsNullOrWhiteSpace(entity.Name))
                    return ServiceResult.Failure("材質名稱為必填");

                if (string.IsNullOrWhiteSpace(entity.Code))
                    return ServiceResult.Failure("材質代碼為必填");

                // 檢查代碼是否重複
                if (await IsCodeExistsAsync(entity.Code, entity.Id))
                    return ServiceResult.Failure("材質代碼已存在");

                // 檢查名稱是否重複
                if (await IsNameExistsAsync(entity.Name, entity.Id))
                    return ServiceResult.Failure("材質名稱已存在");

                // 檢查供應商是否存在（如果有指定）
                if (entity.SupplierId.HasValue)
                {
                    var supplierExists = await _context.Suppliers
                        .AnyAsync(s => s.Id == entity.SupplierId.Value && !s.IsDeleted);
                    
                    if (!supplierExists)
                        return ServiceResult.Failure("指定的供應商不存在");
                }

                // 檢查數值範圍
                if (entity.Density.HasValue && entity.Density.Value < 0)
                    return ServiceResult.Failure("密度不能為負數");

                if (entity.MeltingPoint.HasValue && entity.MeltingPoint.Value < -273.15m)
                    return ServiceResult.Failure("熔點不能低於絕對零度");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    EntityName = entity.Name,
                    EntityCode = entity.Code
                });
                _logger.LogError(ex, "Error validating material {EntityName} with code {EntityCode}", entity.Name, entity.Code);
                throw;
            }
        }

        /// <summary>
        /// 覆寫名稱存在檢查
        /// </summary>
        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(m => m.Name == name && !m.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(m => m.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsNameExistsAsync),
                    ServiceType = GetType().Name,
                    Name = name,
                    ExcludeId = excludeId
                });
                _logger.LogError(ex, "Error checking if material name exists {Name}", name);
                throw;
            }
        }

        /// <summary>
        /// 檢查材質代碼是否已存在
        /// </summary>
        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(m => m.Code == code && !m.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(m => m.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = code,
                    ExcludeId = excludeId
                });
                _logger.LogError(ex, "Error checking if material code exists {Code}", code);
                throw;
            }
        }

        /// <summary>
        /// 根據代碼取得材質資料
        /// </summary>
        public async Task<Material?> GetByCodeAsync(string code)
        {
            try
            {
                return await _dbSet
                    .Include(m => m.Supplier)
                    .Where(m => m.Code == code && !m.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByCodeAsync),
                    ServiceType = GetType().Name,
                    Code = code
                });
                _logger.LogError(ex, "Error getting material by code {Code}", code);
                throw;
            }
        }

        /// <summary>
        /// 根據材質類別取得材質清單
        /// </summary>
        public async Task<List<Material>> GetByCategoryAsync(string category)
        {
            try
            {
                return await _dbSet
                    .Include(m => m.Supplier)
                    .Where(m => !m.IsDeleted && m.Category == category)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByCategoryAsync),
                    ServiceType = GetType().Name,
                    Category = category
                });
                _logger.LogError(ex, "Error getting materials by category {Category}", category);
                throw;
            }
        }

        /// <summary>
        /// 取得所有材質類別
        /// </summary>
        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                return await _dbSet
                    .Where(m => !m.IsDeleted && !string.IsNullOrEmpty(m.Category))
                    .Select(m => m.Category!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetCategoriesAsync),
                    ServiceType = GetType().Name
                });
                _logger.LogError(ex, "Error getting material categories");
                throw;
            }
        }

        /// <summary>
        /// 根據供應商ID取得材質清單
        /// </summary>
        public async Task<List<Material>> GetBySupplierAsync(int supplierId)
        {
            try
            {
                return await _dbSet
                    .Include(m => m.Supplier)
                    .Where(m => !m.IsDeleted && m.SupplierId == supplierId)
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetBySupplierAsync),
                    ServiceType = GetType().Name,
                    SupplierId = supplierId
                });
                _logger.LogError(ex, "Error getting materials by supplier {SupplierId}", supplierId);
                throw;
            }
        }

        /// <summary>
        /// 取得環保材質清單
        /// </summary>
        public async Task<List<Material>> GetEcoFriendlyMaterialsAsync()
        {
            try
            {
                return await _dbSet
                    .Include(m => m.Supplier)
                    .Where(m => !m.IsDeleted && m.IsEcoFriendly)
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetEcoFriendlyMaterialsAsync),
                    ServiceType = GetType().Name
                });
                _logger.LogError(ex, "Error getting eco-friendly materials");
                throw;
            }
        }

        /// <summary>
        /// 根據密度範圍取得材質清單
        /// </summary>
        public async Task<List<Material>> GetByDensityRangeAsync(decimal? minDensity = null, decimal? maxDensity = null)
        {
            try
            {
                var query = _dbSet
                    .Include(m => m.Supplier)
                    .Where(m => !m.IsDeleted);

                if (minDensity.HasValue)
                    query = query.Where(m => m.Density >= minDensity.Value);

                if (maxDensity.HasValue)
                    query = query.Where(m => m.Density <= maxDensity.Value);

                return await query
                    .OrderBy(m => m.Density)
                    .ThenBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByDensityRangeAsync),
                    ServiceType = GetType().Name,
                    MinDensity = minDensity,
                    MaxDensity = maxDensity
                });
                _logger.LogError(ex, "Error getting materials by density range {MinDensity} - {MaxDensity}", minDensity, maxDensity);
                throw;
            }
        }

        /// <summary>
        /// 根據熔點範圍取得材質清單
        /// </summary>
        public async Task<List<Material>> GetByMeltingPointRangeAsync(decimal? minMeltingPoint = null, decimal? maxMeltingPoint = null)
        {
            try
            {
                var query = _dbSet
                    .Include(m => m.Supplier)
                    .Where(m => !m.IsDeleted);

                if (minMeltingPoint.HasValue)
                    query = query.Where(m => m.MeltingPoint >= minMeltingPoint.Value);

                if (maxMeltingPoint.HasValue)
                    query = query.Where(m => m.MeltingPoint <= maxMeltingPoint.Value);

                return await query
                    .OrderBy(m => m.MeltingPoint)
                    .ThenBy(m => m.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByMeltingPointRangeAsync),
                    ServiceType = GetType().Name,
                    MinMeltingPoint = minMeltingPoint,
                    MaxMeltingPoint = maxMeltingPoint
                });
                _logger.LogError(ex, "Error getting materials by melting point range {MinMeltingPoint} - {MaxMeltingPoint}", minMeltingPoint, maxMeltingPoint);
                throw;
            }
        }
    }
}