using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SupplierService : GenericManagementService<Supplier>, ISupplierService
    {
        /// <summary>
        /// 完整建構子 - 包含 ILogger
        /// </summary>
        public SupplierService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Supplier>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不包含 ILogger
        /// </summary>
        public SupplierService(
            IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底方法

        public override async Task<List<Supplier>> GetAllAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Suppliers
                    .Include(s => s.SupplierType)
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                throw;
            }
        }

        public override async Task<Supplier?> GetByIdAsync(int id)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Suppliers
                    .Include(s => s.SupplierType)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                throw;
            }
        }

        public override async Task<List<Supplier>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await context.Suppliers
                    .Include(s => s.SupplierType)
                    .Where(s => !s.IsDeleted &&
                               (s.CompanyName.Contains(searchTerm) ||
                                s.SupplierCode.Contains(searchTerm) ||
                                (s.ContactPerson != null && s.ContactPerson.Contains(searchTerm)) ||
                                (s.TaxNumber != null && s.TaxNumber.Contains(searchTerm))))
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                throw;
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Supplier entity)
        {
            try
            {
                var errors = new List<string>();

                // 驗證必填欄位
                if (string.IsNullOrWhiteSpace(entity.SupplierCode))
                {
                    errors.Add("廠商代碼為必填欄位");
                }

                if (string.IsNullOrWhiteSpace(entity.CompanyName))
                {
                    errors.Add("公司名稱為必填欄位");
                }

                // 驗證廠商代碼唯一性
                if (!string.IsNullOrWhiteSpace(entity.SupplierCode))
                {
                    var isDuplicate = await IsSupplierCodeExistsAsync(entity.SupplierCode, entity.Id);
                    if (isDuplicate)
                    {
                        errors.Add("廠商代碼已存在");
                    }
                }

                // 驗證統一編號格式（如果有提供）
                if (!string.IsNullOrWhiteSpace(entity.TaxNumber) && entity.TaxNumber.Length != 8)
                {
                    errors.Add("統一編號必須為8位數字");
                }



                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, 
                    new { EntityId = entity.Id, SupplierCode = entity.SupplierCode });
                throw;
            }
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<Supplier?> GetBySupplierCodeAsync(string supplierCode)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Suppliers
                    .Include(s => s.SupplierType)
                    .FirstOrDefaultAsync(s => s.SupplierCode == supplierCode && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierCodeAsync), GetType(), _logger, 
                    new { SupplierCode = supplierCode });
                throw;
            }
        }

        public async Task<bool> IsSupplierCodeExistsAsync(string supplierCode, int? excludeId = null)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var query = context.Suppliers.Where(s => s.SupplierCode == supplierCode && !s.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(s => s.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsSupplierCodeExistsAsync), GetType(), _logger, 
                    new { SupplierCode = supplierCode, ExcludeId = excludeId });
                throw;
            }
        }

        public async Task<List<Supplier>> GetBySupplierTypeAsync(int supplierTypeId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.Suppliers
                    .Include(s => s.SupplierType)
                    .Where(s => s.SupplierTypeId == supplierTypeId && !s.IsDeleted)
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetBySupplierTypeAsync), GetType(), _logger, 
                    new { SupplierTypeId = supplierTypeId });
                throw;
            }
        }

        #endregion

        #region 輔助資料查詢

        public async Task<List<SupplierType>> GetSupplierTypesAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.SupplierTypes
                    .Where(st => st.Status == EntityStatus.Active && !st.IsDeleted)
                    .OrderBy(st => st.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetSupplierTypesAsync), GetType(), _logger);
                throw;
            }
        }
        #endregion

        // 聯絡資料管理已移至 ContactService
        // 地址資料管理已移至 AddressService
        // #region 地址資料管理
        // ... 相關方法已移除，請使用 IAddressService
        // #endregion

        #region 狀態管理

        public async Task<ServiceResult> UpdateSupplierStatusAsync(int supplierId, EntityStatus status)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var supplier = await GetByIdAsync(supplierId);
                if (supplier == null)
                {
                    return ServiceResult.Failure("找不到指定的廠商");
                }

                supplier.Status = status;
                supplier.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateSupplierStatusAsync), GetType(), _logger, 
                    new { SupplierId = supplierId, Status = status });
                return ServiceResult.Failure($"更新廠商狀態時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 輔助方法

        public void InitializeNewSupplier(Supplier supplier)
        {
            try
            {
                supplier.SupplierCode = string.Empty;
                supplier.CompanyName = string.Empty;
                supplier.ContactPerson = string.Empty;
                supplier.TaxNumber = string.Empty;
                supplier.SupplierTypeId = null;
                supplier.Status = EntityStatus.Active;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(InitializeNewSupplier), GetType(), _logger);
                throw;
            }
        }

        public int GetBasicRequiredFieldsCount()
        {
            try
            {
                return 2; // SupplierCode, CompanyName
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetBasicRequiredFieldsCount), GetType(), _logger);
                throw;
            }
        }

        public int GetBasicCompletedFieldsCount(Supplier supplier)
        {
            try
            {
                int count = 0;

                if (!string.IsNullOrWhiteSpace(supplier.SupplierCode))
                    count++;

                if (!string.IsNullOrWhiteSpace(supplier.CompanyName))
                    count++;

                return count;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetBasicCompletedFieldsCount), GetType(), _logger, 
                    new { SupplierId = supplier.Id });
                throw;
            }
        }

        #endregion
    }
}



