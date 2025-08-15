using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商類型服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SupplierTypeService : GenericManagementService<SupplierType>, ISupplierTypeService
    {
        /// <summary>
        /// 完整建構子 - 包含 ILogger
        /// </summary>
        public SupplierTypeService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<SupplierType>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不包含 ILogger
        /// </summary>
        public SupplierTypeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<SupplierType>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierTypes
                    .Where(st => !st.IsDeleted)
                    .OrderBy(st => st.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    ServiceType = GetType().Name 
                });
                return new List<SupplierType>();
            }
        }

        public override async Task<List<SupplierType>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierTypes
                    .Where(st => !st.IsDeleted &&
                               st.TypeName.Contains(searchTerm))
                    .OrderBy(st => st.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    SearchTerm = searchTerm,
                    ServiceType = GetType().Name 
                });
                return new List<SupplierType>();
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

                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger, new { 
                    Name = name,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { 
                    EntityId = entity.Id,
                    ServiceType = GetType().Name 
                });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }

        #endregion

        #region 業務特定方法

        public async Task<bool> IsSupplierTypeNameExistsAsync(string typeName, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SupplierTypes.Where(st => st.TypeName == typeName && !st.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(st => st.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSupplierTypeNameExistsAsync), GetType(), _logger, new { 
                    TypeName = typeName,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                return false; // 安全預設值
            }
        }

        public async Task<bool> IsSupplierTypeCodeExistsAsync(string typeCode, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(typeCode))
                    return false;

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.SupplierTypes.Where(st => st.Code == typeCode && !st.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(st => st.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSupplierTypeCodeExistsAsync), GetType(), _logger, new { 
                    TypeCode = typeCode,
                    ExcludeId = excludeId,
                    ServiceType = GetType().Name 
                });
                return false; // 安全預設值
            }
        }

        public async Task<SupplierType?> GetByTypeNameAsync(string typeName)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.SupplierTypes
                    .FirstOrDefaultAsync(st => st.TypeName == typeName && !st.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByTypeNameAsync), GetType(), _logger, new { 
                    TypeName = typeName,
                    ServiceType = GetType().Name 
                });
                return null; // 安全預設值
            }
        }

        public async Task<bool> CanDeleteSupplierTypeAsync(int supplierTypeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                // 檢查是否有廠商使用此類型
                var hasSuppliers = await context.Suppliers
                    .AnyAsync(s => s.SupplierTypeId == supplierTypeId && !s.IsDeleted);

                return !hasSuppliers;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteSupplierTypeAsync), GetType(), _logger, new { 
                    SupplierTypeId = supplierTypeId,
                    ServiceType = GetType().Name 
                });
                return false; // 安全預設值
            }
        }

        #endregion
    }
}

