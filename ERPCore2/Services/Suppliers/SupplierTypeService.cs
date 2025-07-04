using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商類型服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SupplierTypeService : GenericManagementService<SupplierType>, ISupplierTypeService
    {
        public SupplierTypeService(
            AppDbContext context, 
            ILogger<GenericManagementService<SupplierType>> logger, 
            IErrorLogService errorLogService) : base(context, logger, errorLogService)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<SupplierType>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Where(st => !st.IsDeleted)
                    .OrderBy(st => st.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting all supplier types");
                throw;
            }
        }

        public override async Task<List<SupplierType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Where(st => !st.IsDeleted &&
                               (st.TypeName.Contains(searchTerm) ||
                                (st.Description != null && st.Description.Contains(searchTerm))))
                    .OrderBy(st => st.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(SearchAsync),
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error searching supplier types with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(SupplierType entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證類型名稱
                if (string.IsNullOrWhiteSpace(entity.TypeName))
                {
                    errors.Add("廠商類型名稱為必填欄位");
                }
                else
                {
                    // 檢查名稱是否重複
                    var isDuplicate = await IsSupplierTypeNameExistsAsync(entity.TypeName, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("廠商類型名稱已存在");
                    }
                }

                // 驗證描述長度
                if (!string.IsNullOrWhiteSpace(entity.Description) && entity.Description.Length > 200)
                {
                    errors.Add("描述長度不能超過200個字元");
                }

                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(ValidateAsync),
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error validating supplier type entity {EntityId}", entity.Id);
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                return await IsSupplierTypeNameExistsAsync(name, excludeId);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsNameExistsAsync),
                    Name = name,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if supplier type name exists {Name}", name);
                return false; // 安全預設值
            }
        }

        protected override async Task<ServiceResult> CanDeleteAsync(SupplierType entity)
        {
            try
            {
                var canDelete = await CanDeleteSupplierTypeAsync(entity.Id);
                return canDelete 
                    ? ServiceResult.Success() 
                    : ServiceResult.Failure("無法刪除此廠商類型，因為有廠商正在使用此類型");
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(CanDeleteAsync),
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if supplier type can be deleted {EntityId}", entity.Id);
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 業務特定方法

        public async Task<bool> IsSupplierTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(st => st.TypeName == typeName && !st.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(st => st.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(IsSupplierTypeNameExistsAsync),
                    TypeName = typeName,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking supplier type name exists {TypeName}", typeName);
                return false; // 安全預設值
            }
        }

        public async Task<SupplierType?> GetByTypeNameAsync(string typeName)
        {
            try
            {
                return await _dbSet
                    .FirstOrDefaultAsync(st => st.TypeName == typeName && !st.IsDeleted);
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(GetByTypeNameAsync),
                    TypeName = typeName,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error getting supplier type by name {TypeName}", typeName);
                throw;
            }
        }

        public async Task<bool> CanDeleteSupplierTypeAsync(int supplierTypeId)
        {
            try
            {
                // 檢查是否有廠商使用此類型
                var hasSuppliers = await _context.Suppliers
                    .AnyAsync(s => s.SupplierTypeId == supplierTypeId && !s.IsDeleted);

                return !hasSuppliers;
            }
            catch (Exception ex)
            {
                await _errorLogService.LogErrorAsync(ex, new { 
                    Method = nameof(CanDeleteSupplierTypeAsync),
                    SupplierTypeId = supplierTypeId,
                    ServiceType = GetType().Name 
                });
                _logger.LogError(ex, "Error checking if supplier type can be deleted {SupplierTypeId}", supplierTypeId);
                return false; // 安全預設值
            }
        }

        #endregion
    }
}
