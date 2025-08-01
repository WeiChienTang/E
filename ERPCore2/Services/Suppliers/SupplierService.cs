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
                    .Include(s => s.SupplierContacts)
                        .ThenInclude(sc => sc.ContactType)
                    .Include(s => s.SupplierAddresses)
                        .ThenInclude(sa => sa.AddressType)
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

        #region 聯絡資料管理

        public async Task<List<SupplierContact>> GetSupplierContactsAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.SupplierContacts
                    .Include(sc => sc.ContactType)
                    .Where(sc => sc.SupplierId == supplierId && !sc.IsDeleted)
                    .OrderBy(sc => sc.ContactType!.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetSupplierContactsAsync), GetType(), _logger, 
                    new { SupplierId = supplierId });
                throw;
            }
        }

        public async Task<ServiceResult> UpdateSupplierContactsAsync(int supplierId, List<SupplierContact> contacts)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                // 取得現有聯絡資料
                var existingContacts = await context.SupplierContacts
                    .Where(sc => sc.SupplierId == supplierId && !sc.IsDeleted)
                    .ToListAsync();

                // 刪除不在新列表中的聯絡資料
                var contactsToDelete = existingContacts
                    .Where(ec => !contacts.Any(c => c.Id == ec.Id))
                    .ToList();

                foreach (var contact in contactsToDelete)
                {
                    contact.IsDeleted = true;
                    contact.UpdatedAt = DateTime.UtcNow;
                }

                // 更新或新增聯絡資料
                foreach (var contact in contacts)
                {
                    if (contact.Id <= 0) // 新增（包括負數臨時 ID）
                    {
                        // 建立新的聯絡實體以避免 ID 衝突
                        var newContact = new SupplierContact
                        {
                            SupplierId = supplierId,
                            ContactTypeId = contact.ContactTypeId,
                            ContactValue = contact.ContactValue,
                            IsPrimary = contact.IsPrimary,
                            Status = EntityStatus.Active,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedBy = "System", // TODO: 從認證取得使用者
                            Remarks = contact.Remarks
                        };
                        context.SupplierContacts.Add(newContact);
                    }
                    else
                    {
                        // 更新
                        var existingContact = existingContacts.FirstOrDefault(ec => ec.Id == contact.Id);
                        if (existingContact != null)
                        {
                            existingContact.ContactTypeId = contact.ContactTypeId;
                            existingContact.ContactValue = contact.ContactValue;
                            existingContact.IsPrimary = contact.IsPrimary;
                            existingContact.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateSupplierContactsAsync), GetType(), _logger, 
                    new { SupplierId = supplierId, ContactsCount = contacts.Count });
                return ServiceResult.Failure($"更新廠商聯絡資料時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 地址資料管理

        public async Task<List<SupplierAddress>> GetSupplierAddressesAsync(int supplierId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                return await context.SupplierAddresses
                    .Include(sa => sa.AddressType)
                    .Where(sa => sa.SupplierId == supplierId && !sa.IsDeleted)
                    .OrderBy(sa => sa.AddressType!.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetSupplierAddressesAsync), GetType(), _logger, 
                    new { SupplierId = supplierId });
                throw;
            }
        }

        public async Task<ServiceResult> UpdateSupplierAddressesAsync(int supplierId, List<SupplierAddress> addresses)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                // 取得現有地址資料
                var existingAddresses = await context.SupplierAddresses
                    .Where(sa => sa.SupplierId == supplierId && !sa.IsDeleted)
                    .ToListAsync();

                // 刪除不在新列表中的地址資料
                var addressesToDelete = existingAddresses
                    .Where(ea => !addresses.Any(a => a.Id == ea.Id))
                    .ToList();

                foreach (var address in addressesToDelete)
                {
                    address.IsDeleted = true;
                    address.UpdatedAt = DateTime.UtcNow;
                }

                // 更新或新增地址資料
                foreach (var address in addresses)
                {
                    address.SupplierId = supplierId;
                    
                    if (address.Id == 0)
                    {
                        // 新增
                        address.CreatedAt = DateTime.UtcNow;
                        address.UpdatedAt = DateTime.UtcNow;
                        context.SupplierAddresses.Add(address);
                    }
                    else
                    {
                        // 更新
                        var existingAddress = existingAddresses.FirstOrDefault(ea => ea.Id == address.Id);
                        if (existingAddress != null)
                        {
                            existingAddress.AddressTypeId = address.AddressTypeId;
                            existingAddress.PostalCode = address.PostalCode;
                            existingAddress.City = address.City;
                            existingAddress.District = address.District;
                            existingAddress.Address = address.Address;
                            existingAddress.IsPrimary = address.IsPrimary;
                            existingAddress.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateSupplierAddressesAsync), GetType(), _logger, 
                    new { SupplierId = supplierId, AddressesCount = addresses.Count });
                return ServiceResult.Failure($"更新廠商地址資料時發生錯誤: {ex.Message}");
            }
        }

        #endregion

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



