using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶服務實作 - 直接使用 EF Core，無需 Repository 和 DTO
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(AppDbContext context, ILogger<CustomerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            try
            {
                return await _context.Customers
                    .Where(c => c.Status != EntityStatus.Deleted)
                    .Include(c => c.CustomerType)
                    .Include(c => c.Industry)
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
                return await _context.Customers
                    .Include(c => c.CustomerType)
                    .Include(c => c.Industry)
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
                // Business Validation
                var validationResult = ValidateCustomer(customer);
                if (!validationResult.IsSuccess)
                    return ServiceResult<Customer>.Failure(validationResult.ErrorMessage);

                // Business Rules - Check for duplicate customer code
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerCode == customer.CustomerCode && c.Status != EntityStatus.Deleted);

                if (existingCustomer != null)
                    return ServiceResult<Customer>.Failure("客戶代碼已存在");

                // Check for duplicate company name
                var existingCompany = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CompanyName == customer.CompanyName && c.Status != EntityStatus.Deleted);

                if (existingCompany != null)
                    return ServiceResult<Customer>.Failure("公司名稱已存在");

                // Validate foreign keys
                if (customer.CustomerTypeId.HasValue)
                {
                    var customerTypeExists = await _context.CustomerTypes
                        .AnyAsync(ct => ct.CustomerTypeId == customer.CustomerTypeId && ct.Status != EntityStatus.Deleted);
                    if (!customerTypeExists)
                        return ServiceResult<Customer>.Failure("客戶類型不存在");
                }

                if (customer.IndustryId.HasValue)
                {
                    var industryExists = await _context.Industries
                        .AnyAsync(i => i.IndustryId == customer.IndustryId && i.Status != EntityStatus.Deleted);
                    if (!industryExists)
                        return ServiceResult<Customer>.Failure("行業別不存在");
                }

                // Set audit fields
                customer.Status = EntityStatus.Active;
                customer.CreatedDate = DateTime.UtcNow;
                customer.CreatedBy = "System"; // TODO: Replace with actual user when authentication is implemented

                // Save to Database
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Customer created with ID {CustomerId}", customer.CustomerId);
                return ServiceResult<Customer>.Success(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return ServiceResult<Customer>.Failure("創建客戶時發生錯誤");
            }
        }

        public async Task<ServiceResult<Customer>> UpdateAsync(Customer customer)
        {
            try
            {
                // Get existing customer
                var existingCustomer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId && c.Status != EntityStatus.Deleted);

                if (existingCustomer == null)
                    return ServiceResult<Customer>.Failure("客戶不存在");

                // Business Validation
                var validationResult = ValidateCustomer(customer);
                if (!validationResult.IsSuccess)
                    return ServiceResult<Customer>.Failure(validationResult.ErrorMessage);

                // Business Rules - Check for duplicate customer code (excluding current customer)
                var duplicateCode = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerCode == customer.CustomerCode &&
                                           c.CustomerId != customer.CustomerId &&
                                           c.Status != EntityStatus.Deleted);

                if (duplicateCode != null)
                    return ServiceResult<Customer>.Failure("客戶代碼已存在");

                // Check for duplicate company name (excluding current customer)
                var duplicateCompany = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CompanyName == customer.CompanyName &&
                                           c.CustomerId != customer.CustomerId &&
                                           c.Status != EntityStatus.Deleted);

                if (duplicateCompany != null)
                    return ServiceResult<Customer>.Failure("公司名稱已存在");

                // Validate foreign keys
                if (customer.CustomerTypeId.HasValue)
                {
                    var customerTypeExists = await _context.CustomerTypes
                        .AnyAsync(ct => ct.CustomerTypeId == customer.CustomerTypeId && ct.Status != EntityStatus.Deleted);
                    if (!customerTypeExists)
                        return ServiceResult<Customer>.Failure("客戶類型不存在");
                }

                if (customer.IndustryId.HasValue)
                {
                    var industryExists = await _context.Industries
                        .AnyAsync(i => i.IndustryId == customer.IndustryId && i.Status != EntityStatus.Deleted);
                    if (!industryExists)
                        return ServiceResult<Customer>.Failure("行業別不存在");
                }

                // Update Customer properties
                existingCustomer.CustomerCode = customer.CustomerCode;
                existingCustomer.CompanyName = customer.CompanyName;
                existingCustomer.ContactPerson = customer.ContactPerson;
                existingCustomer.TaxNumber = customer.TaxNumber;
                existingCustomer.CustomerTypeId = customer.CustomerTypeId;
                existingCustomer.IndustryId = customer.IndustryId;
                existingCustomer.ModifiedDate = DateTime.UtcNow;
                existingCustomer.ModifiedBy = "System"; // TODO: Replace with actual user when authentication is implemented

                // Save to Database
                await _context.SaveChangesAsync();

                _logger.LogInformation("Customer updated with ID {CustomerId}", customer.CustomerId);
                return ServiceResult<Customer>.Success(existingCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID {CustomerId}", customer.CustomerId);
                return ServiceResult<Customer>.Failure("更新客戶時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == id && c.Status != EntityStatus.Deleted);

                if (customer == null)
                    return ServiceResult.Failure("客戶不存在");

                // Business Rules - Check for dependencies
                var hasContacts = await _context.CustomerContacts
                    .AnyAsync(cc => cc.CustomerId == id);

                var hasAddresses = await _context.CustomerAddresses
                    .AnyAsync(ca => ca.CustomerId == id);

                if (hasContacts || hasAddresses)
                    return ServiceResult.Failure("無法刪除：客戶有相關聯絡人或地址資料");

                // Soft Delete
                customer.Status = EntityStatus.Deleted;
                customer.ModifiedDate = DateTime.UtcNow;
                customer.ModifiedBy = "System"; // TODO: Replace with actual user when authentication is implemented

                await _context.SaveChangesAsync();

                _logger.LogInformation("Customer deleted with ID {CustomerId}", id);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with ID {CustomerId}", id);
                return ServiceResult.Failure("刪除客戶時發生錯誤");
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers
                .AnyAsync(c => c.CustomerId == id && c.Status != EntityStatus.Deleted);
        }

        public async Task<Customer?> GetByCustomerCodeAsync(string customerCode)
        {
            if (string.IsNullOrWhiteSpace(customerCode))
                return null;

            return await _context.Customers
                .Include(c => c.CustomerType)
                .Include(c => c.Industry)
                .FirstOrDefaultAsync(c => c.CustomerCode == customerCode && c.Status != EntityStatus.Deleted);
        }

        public async Task<List<Customer>> GetByCompanyNameAsync(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return new List<Customer>();

            return await _context.Customers
                .Where(c => c.CompanyName.Contains(companyName) && c.Status != EntityStatus.Deleted)
                .Include(c => c.CustomerType)
                .Include(c => c.Industry)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public async Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Customers
                .Where(c => c.Status != EntityStatus.Deleted)
                .Include(c => c.CustomerType)
                .Include(c => c.Industry);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CompanyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Customer>> GetActiveCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.Status == EntityStatus.Active)
                .Include(c => c.CustomerType)
                .Include(c => c.Industry)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        // 新增方法用於支持下拉列表
        public async Task<List<CustomerType>> GetCustomerTypesAsync()
        {
            try
            {
                return await _context.CustomerTypes
                    .Where(ct => ct.Status == EntityStatus.Active)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer types");
                throw;
            }
        }

        public async Task<List<Industry>> GetIndustriesAsync()
        {
            try
            {
                return await _context.Industries
                    .Where(i => i.Status == EntityStatus.Active)
                    .OrderBy(i => i.IndustryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting industries");
                throw;
            }
        }

        public async Task<List<ContactType>> GetContactTypesAsync()
        {
            try
            {
                return await _context.ContactTypes
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
                return await _context.AddressTypes
                    .Where(at => at.Status == EntityStatus.Active)
                    .OrderBy(at => at.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address types");
                throw;
            }
        }

        private ServiceResult ValidateCustomer(Customer customer)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(customer.CustomerCode))
                errors.Add("客戶代碼為必填");

            if (string.IsNullOrWhiteSpace(customer.CompanyName))
                errors.Add("公司名稱為必填");

            if (customer.CustomerCode?.Length > 20)
                errors.Add("客戶代碼不可超過20個字元");

            if (customer.CompanyName?.Length > 100)
                errors.Add("公司名稱不可超過100個字元");

            if (!string.IsNullOrEmpty(customer.ContactPerson) && customer.ContactPerson.Length > 50)
                errors.Add("聯絡人不可超過50個字元");

            if (!string.IsNullOrEmpty(customer.TaxNumber) && customer.TaxNumber.Length > 20)
                errors.Add("統一編號不可超過20個字元");

            if (errors.Any())
                return ServiceResult.ValidationFailure(errors);

            return ServiceResult.Success();
        }
    }
}