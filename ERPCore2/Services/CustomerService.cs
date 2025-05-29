using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶服務實作 - 使用 DbContextFactory 解決並發問題
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IDbContextFactory<AppDbContext> contextFactory, ILogger<CustomerService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Customers
                    .Where(c => c.Status != EntityStatus.Deleted)
                    .Include(c => c.CustomerType)
                    .Include(c => c.IndustryType)
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all customers");
                throw;
            }
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Customers
                    .Include(c => c.CustomerType)
                    .Include(c => c.IndustryType)
                    .Include(c => c.CustomerContacts)
                    .Include(c => c.CustomerAddresses)
                    .FirstOrDefaultAsync(c => c.CustomerId == id && c.Status != EntityStatus.Deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer with ID {CustomerId}", id);
                throw;
            }
        }

        public async Task<ServiceResult<Customer>> CreateAsync(Customer customer)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                Console.WriteLine("=== CustomerService.CreateAsync 開始 ===");
                Console.WriteLine($"客戶名稱: {customer.CompanyName}");
                Console.WriteLine($"客戶代碼: {customer.CustomerCode}");

                // 1. 先檢查是否已存在相同代碼的客戶
                var existingCustomer = await context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerCode == customer.CustomerCode && c.Status != EntityStatus.Deleted);

                if (existingCustomer != null)
                {
                    Console.WriteLine($"客戶代碼 {customer.CustomerCode} 已存在");
                    return ServiceResult<Customer>.Failure($"客戶代碼 '{customer.CustomerCode}' 已經存在");
                }

                // 2. 檢查公司名稱是否重複
                var existingCompany = await context.Customers
                    .FirstOrDefaultAsync(c => c.CompanyName == customer.CompanyName && c.Status != EntityStatus.Deleted);

                if (existingCompany != null)
                {
                    Console.WriteLine($"公司名稱 {customer.CompanyName} 已存在");
                    return ServiceResult<Customer>.Failure($"公司名稱 '{customer.CompanyName}' 已經存在");
                }

                // 3. 驗證客戶類型和行業類型是否存在
                if (customer.CustomerTypeId.HasValue)
                {
                    var customerTypeExists = await context.CustomerTypes
                        .AnyAsync(ct => ct.CustomerTypeId == customer.CustomerTypeId.Value && ct.Status != EntityStatus.Deleted);
                    if (!customerTypeExists)
                    {
                        return ServiceResult<Customer>.Failure("指定的客戶類型不存在");
                    }
                }

                if (customer.IndustryTypeId.HasValue)
                {
                    var industryExists = await context.IndustryTypes
                        .AnyAsync(it => it.IndustryTypeId == customer.IndustryTypeId.Value && it.Status != EntityStatus.Deleted);
                    if (!industryExists)
                    {
                        return ServiceResult<Customer>.Failure("指定的行業類型不存在");
                    }
                }

                // 4. 開始交易
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    // 設定基本欄位
                    customer.Status = EntityStatus.Active;
                    customer.CreatedAt = DateTime.Now;
                    customer.UpdatedAt = DateTime.Now;

                    context.Customers.Add(customer);
                    await context.SaveChangesAsync();

                    Console.WriteLine($"客戶建立成功，ID: {customer.CustomerId}");

                    // 如果有聯絡人，一併建立
                    if (customer.CustomerContacts?.Any() == true)
                    {
                        foreach (var contact in customer.CustomerContacts)
                        {
                            contact.CustomerId = customer.CustomerId;
                            contact.Status = EntityStatus.Active;
                            contact.CreatedAt = DateTime.Now;
                            contact.UpdatedAt = DateTime.Now;
                        }
                        await context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    Console.WriteLine("=== CustomerService.CreateAsync 完成 ===");
                    return ServiceResult<Customer>.Success(customer);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"建立客戶時發生錯誤: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomerService.CreateAsync 發生例外: {ex.Message}");
                _logger.LogError(ex, "Error creating customer {CustomerCode}", customer.CustomerCode);
                return ServiceResult<Customer>.Failure($"建立客戶時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<Customer>> UpdateAsync(Customer customer)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var existingCustomer = await context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId && c.Status != EntityStatus.Deleted);

                if (existingCustomer == null)
                {
                    return ServiceResult<Customer>.Failure("客戶不存在");
                }

                // 檢查代碼是否與其他客戶重複
                var duplicateCode = await context.Customers
                    .AnyAsync(c => c.CustomerCode == customer.CustomerCode && 
                                  c.CustomerId != customer.CustomerId && 
                                  c.Status != EntityStatus.Deleted);

                if (duplicateCode)
                {
                    return ServiceResult<Customer>.Failure($"客戶代碼 '{customer.CustomerCode}' 已經存在");
                }

                // 檢查公司名稱是否與其他客戶重複
                var duplicateCompany = await context.Customers
                    .AnyAsync(c => c.CompanyName == customer.CompanyName && 
                                  c.CustomerId != customer.CustomerId && 
                                  c.Status != EntityStatus.Deleted);

                if (duplicateCompany)
                {
                    return ServiceResult<Customer>.Failure($"公司名稱 '{customer.CompanyName}' 已經存在");
                }

                // 驗證客戶類型和行業類型是否存在
                if (customer.CustomerTypeId.HasValue)
                {
                    var customerTypeExists = await context.CustomerTypes
                        .AnyAsync(ct => ct.CustomerTypeId == customer.CustomerTypeId.Value && ct.Status != EntityStatus.Deleted);
                    if (!customerTypeExists)
                    {
                        return ServiceResult<Customer>.Failure("指定的客戶類型不存在");
                    }
                }

                if (customer.IndustryTypeId.HasValue)
                {
                    var industryExists = await context.IndustryTypes
                        .AnyAsync(it => it.IndustryTypeId == customer.IndustryTypeId.Value && it.Status != EntityStatus.Deleted);
                    if (!industryExists)
                    {
                        return ServiceResult<Customer>.Failure("指定的行業類型不存在");
                    }
                }

                // 更新欄位
                existingCustomer.CustomerCode = customer.CustomerCode;
                existingCustomer.CompanyName = customer.CompanyName;
                existingCustomer.CompanyNameEn = customer.CompanyNameEn;
                existingCustomer.TaxId = customer.TaxId;
                existingCustomer.CustomerTypeId = customer.CustomerTypeId;
                existingCustomer.IndustryTypeId = customer.IndustryTypeId;
                existingCustomer.ContactPerson = customer.ContactPerson;
                existingCustomer.Phone = customer.Phone;
                existingCustomer.Email = customer.Email;
                existingCustomer.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult<Customer>.Success(existingCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", customer.CustomerId);
                return ServiceResult<Customer>.Failure($"更新客戶時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var customer = await context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == id && c.Status != EntityStatus.Deleted);

                if (customer == null)
                {
                    return ServiceResult<bool>.Failure("客戶不存在");
                }

                // 檢查是否有關聯的聯絡人
                var hasContacts = await context.CustomerContacts
                    .AnyAsync(cc => cc.CustomerId == id && cc.Status != EntityStatus.Deleted);

                if (hasContacts)
                {
                    return ServiceResult<bool>.Failure("無法刪除有聯絡人記錄的客戶");
                }

                // 檢查是否有關聯的地址
                var hasAddresses = await context.CustomerAddresses
                    .AnyAsync(ca => ca.CustomerId == id && ca.Status != EntityStatus.Deleted);

                if (hasAddresses)
                {
                    return ServiceResult<bool>.Failure("無法刪除有地址記錄的客戶");
                }

                // 軟刪除
                customer.Status = EntityStatus.Deleted;
                customer.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
                return ServiceResult<bool>.Failure($"刪除客戶時發生錯誤: {ex.Message}");
            }
        }

        public async Task<List<Customer>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Customer>();
            }

            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Customers
                    .Where(c => c.Status != EntityStatus.Deleted &&
                               (c.CompanyName.Contains(searchTerm) ||
                                c.CustomerCode.Contains(searchTerm) ||
                                (c.ContactPerson != null && c.ContactPerson.Contains(searchTerm))))
                    .Include(c => c.CustomerType)
                    .Include(c => c.IndustryType)
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term {SearchTerm}", searchTerm);
                return new List<Customer>();
            }
        }

        public async Task<List<Customer>> GetActiveCustomersAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Customers
                    .Where(c => c.Status == EntityStatus.Active)
                    .Include(c => c.CustomerType)
                    .Include(c => c.IndustryType)
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active customers");
                return new List<Customer>();
            }
        }

        public async Task<(List<Customer> Data, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var query = context.Customers
                    .Where(c => c.Status != EntityStatus.Deleted)
                    .Include(c => c.CustomerType)
                    .Include(c => c.IndustryType);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(c => c.CompanyName.Contains(searchTerm) ||
                                           c.CustomerCode.Contains(searchTerm) ||
                                           (c.ContactPerson != null && c.ContactPerson.Contains(searchTerm)));
                }

                var totalCount = await query.CountAsync();
                var data = await query
                    .OrderBy(c => c.CompanyName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (data, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged customers");
                return (new List<Customer>(), 0);
            }
        }

        public async Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.Customers
                    .Where(c => c.CustomerCode == customerCode && c.Status != EntityStatus.Deleted)
                    .Where(c => !excludeId.HasValue || c.CustomerId != excludeId.Value)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer code exists {CustomerCode}", customerCode);
                throw;
            }
        }

        #region 關聯資料
        public async Task<List<CustomerType>> GetCustomerTypesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CustomerTypes
                    .Where(ct => ct.Status != EntityStatus.Deleted)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer types");
                throw;
            }
        }

        public async Task<List<IndustryType>> GetIndustryTypesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.IndustryTypes
                    .Where(it => it.Status != EntityStatus.Deleted)
                    .OrderBy(it => it.IndustryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting industry types");
                throw;
            }
        }

        public async Task<List<ContactType>> GetContactTypesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.ContactTypes
                    .Where(ct => ct.Status != EntityStatus.Deleted)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact types");
                throw;
            }
        }

        public async Task<List<AddressType>> GetAddressTypesAsync()
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.AddressTypes
                    .Where(at => at.Status != EntityStatus.Deleted)
                    .OrderBy(at => at.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address types");
                throw;
            }
        }
        #endregion

        #region 客戶聯絡人管理
        public async Task<List<CustomerContact>> GetCustomerContactsAsync(int customerId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                return await context.CustomerContacts
                    .Where(cc => cc.CustomerId == customerId && cc.Status != EntityStatus.Deleted)
                    .Include(cc => cc.ContactType)
                    .OrderBy(cc => cc.ContactName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer contacts for {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<ServiceResult<bool>> UpdateCustomerContactsAsync(int customerId, List<CustomerContact> contacts)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                using var transaction = await context.Database.BeginTransactionAsync();
                
                try
                {
                    // 取得現有聯絡人
                    var existingContacts = await context.CustomerContacts
                        .Where(cc => cc.CustomerId == customerId && cc.Status != EntityStatus.Deleted)
                        .ToListAsync();

                    // 刪除不在新清單中的聯絡人
                    foreach (var existing in existingContacts)
                    {
                        if (!contacts.Any(c => c.CustomerContactId == existing.CustomerContactId))
                        {
                            existing.Status = EntityStatus.Deleted;
                            existing.UpdatedAt = DateTime.Now;
                        }
                    }

                    // 新增或更新聯絡人
                    foreach (var contact in contacts)
                    {
                        if (contact.CustomerContactId == 0)
                        {
                            // 新增
                            contact.CustomerId = customerId;
                            contact.Status = EntityStatus.Active;
                            contact.CreatedAt = DateTime.Now;
                            contact.UpdatedAt = DateTime.Now;
                            context.CustomerContacts.Add(contact);
                        }
                        else
                        {
                            // 更新
                            var existing = existingContacts.FirstOrDefault(ec => ec.CustomerContactId == contact.CustomerContactId);
                            if (existing != null)
                            {
                                existing.ContactName = contact.ContactName;
                                existing.ContactTypeId = contact.ContactTypeId;
                                existing.Phone = contact.Phone;
                                existing.Email = contact.Email;
                                existing.Title = contact.Title;
                                existing.Department = contact.Department;
                                existing.UpdatedAt = DateTime.Now;
                            }
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return ServiceResult<bool>.Success(true);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer contacts for {CustomerId}", customerId);
                return ServiceResult<bool>.Failure($"更新聯絡人時發生錯誤: {ex.Message}");
            }
        }
        #endregion
    }
}
