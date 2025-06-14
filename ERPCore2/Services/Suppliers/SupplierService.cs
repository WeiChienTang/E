using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SupplierService : GenericManagementService<Supplier>, ISupplierService
    {
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(AppDbContext context, ILogger<SupplierService> logger) : base(context)
        {
            _logger = logger;
        }

        #region 覆寫基底方法

        public override async Task<List<Supplier>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.SupplierType)
                .Include(s => s.IndustryType)
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.CompanyName)
                .ToListAsync();
        }

        public override async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(s => s.SupplierType)
                .Include(s => s.IndustryType)
                .Include(s => s.SupplierContacts)
                    .ThenInclude(sc => sc.ContactType)
                .Include(s => s.SupplierAddresses)
                    .ThenInclude(sa => sa.AddressType)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public override async Task<List<Supplier>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Include(s => s.SupplierType)
                .Include(s => s.IndustryType)
                .Where(s => !s.IsDeleted &&
                           (s.CompanyName.Contains(searchTerm) ||
                            s.SupplierCode.Contains(searchTerm) ||
                            (s.ContactPerson != null && s.ContactPerson.Contains(searchTerm)) ||
                            (s.TaxNumber != null && s.TaxNumber.Contains(searchTerm))))
                .OrderBy(s => s.CompanyName)
                .ToListAsync();
        }        public override async Task<ServiceResult> ValidateAsync(Supplier entity)
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

            // 驗證信用額度
            if (entity.CreditLimit.HasValue && entity.CreditLimit.Value < 0)
            {
                errors.Add("信用額度不能為負數");
            }

            return errors.Any() 
                ? ServiceResult.Failure(string.Join("; ", errors))
                : ServiceResult.Success();
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<Supplier?> GetBySupplierCodeAsync(string supplierCode)
        {
            try
            {
                return await _dbSet
                    .Include(s => s.SupplierType)
                    .Include(s => s.IndustryType)
                    .FirstOrDefaultAsync(s => s.SupplierCode == supplierCode && !s.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier by code {SupplierCode}", supplierCode);
                throw;
            }
        }

        public async Task<bool> IsSupplierCodeExistsAsync(string supplierCode, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(s => s.SupplierCode == supplierCode && !s.IsDeleted);
                
                if (excludeId.HasValue)
                    query = query.Where(s => s.Id != excludeId.Value);
                    
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking supplier code exists {SupplierCode}", supplierCode);
                throw;
            }
        }

        public async Task<List<Supplier>> GetBySupplierTypeAsync(int supplierTypeId)
        {
            try
            {
                return await _dbSet
                    .Include(s => s.SupplierType)
                    .Include(s => s.IndustryType)
                    .Where(s => s.SupplierTypeId == supplierTypeId && !s.IsDeleted)
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suppliers by type {SupplierTypeId}", supplierTypeId);
                throw;
            }
        }

        public async Task<List<Supplier>> GetByIndustryTypeAsync(int industryTypeId)
        {
            try
            {
                return await _dbSet
                    .Include(s => s.SupplierType)
                    .Include(s => s.IndustryType)
                    .Where(s => s.IndustryTypeId == industryTypeId && !s.IsDeleted)
                    .OrderBy(s => s.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suppliers by industry type {IndustryTypeId}", industryTypeId);
                throw;
            }
        }

        #endregion

        #region 輔助資料查詢

        public async Task<List<SupplierType>> GetSupplierTypesAsync()
        {
            try
            {
                return await _context.SupplierTypes
                    .Where(st => st.Status == EntityStatus.Active && !st.IsDeleted)
                    .OrderBy(st => st.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier types");
                throw;
            }
        }

        public async Task<List<IndustryType>> GetIndustryTypesAsync()
        {
            try
            {
                return await _context.IndustryTypes
                    .Where(it => it.Status == EntityStatus.Active && !it.IsDeleted)
                    .OrderBy(it => it.IndustryTypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting industry types");
                throw;
            }
        }

        #endregion

        #region 聯絡資料管理

        public async Task<List<SupplierContact>> GetSupplierContactsAsync(int supplierId)
        {
            try
            {
                return await _context.SupplierContacts
                    .Include(sc => sc.ContactType)
                    .Where(sc => sc.SupplierId == supplierId && !sc.IsDeleted)
                    .OrderBy(sc => sc.ContactType!.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier contacts for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<ServiceResult> UpdateSupplierContactsAsync(int supplierId, List<SupplierContact> contacts)
        {
            try
            {
                // 取得現有聯絡資料
                var existingContacts = await _context.SupplierContacts
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
                    contact.SupplierId = supplierId;
                    
                    if (contact.Id == 0)
                    {
                        // 新增
                        contact.CreatedAt = DateTime.UtcNow;
                        contact.UpdatedAt = DateTime.UtcNow;
                        _context.SupplierContacts.Add(contact);
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

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier contacts for supplier {SupplierId}", supplierId);
                return ServiceResult.Failure($"更新廠商聯絡資料時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 地址資料管理

        public async Task<List<SupplierAddress>> GetSupplierAddressesAsync(int supplierId)
        {
            try
            {
                return await _context.SupplierAddresses
                    .Include(sa => sa.AddressType)
                    .Where(sa => sa.SupplierId == supplierId && !sa.IsDeleted)
                    .OrderBy(sa => sa.AddressType!.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier addresses for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<ServiceResult> UpdateSupplierAddressesAsync(int supplierId, List<SupplierAddress> addresses)
        {
            try
            {
                // 取得現有地址資料
                var existingAddresses = await _context.SupplierAddresses
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
                        _context.SupplierAddresses.Add(address);
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

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier addresses for supplier {SupplierId}", supplierId);
                return ServiceResult.Failure($"更新廠商地址資料時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 狀態管理

        public async Task<ServiceResult> UpdateSupplierStatusAsync(int supplierId, EntityStatus status)
        {
            try
            {
                var supplier = await GetByIdAsync(supplierId);
                if (supplier == null)
                {
                    return ServiceResult.Failure("找不到指定的廠商");
                }

                supplier.Status = status;
                supplier.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier status for supplier {SupplierId}", supplierId);
                return ServiceResult.Failure($"更新廠商狀態時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 輔助方法

        public void InitializeNewSupplier(Supplier supplier)
        {
            supplier.SupplierCode = string.Empty;
            supplier.CompanyName = string.Empty;
            supplier.ContactPerson = string.Empty;
            supplier.TaxNumber = string.Empty;
            supplier.PaymentTerms = string.Empty;
            supplier.SupplierTypeId = null;
            supplier.IndustryTypeId = null;
            supplier.CreditLimit = null;
            supplier.Status = EntityStatus.Active;
        }

        public int GetBasicRequiredFieldsCount()
        {
            return 2; // SupplierCode, CompanyName
        }

        public int GetBasicCompletedFieldsCount(Supplier supplier)
        {
            int count = 0;

            if (!string.IsNullOrWhiteSpace(supplier.SupplierCode))
                count++;

            if (!string.IsNullOrWhiteSpace(supplier.CompanyName))
                count++;

            return count;
        }

        #endregion
    }
}
