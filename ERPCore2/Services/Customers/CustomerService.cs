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
    /// 客戶服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class CustomerService : GenericManagementService<Customer>, ICustomerService
    {
        private readonly ILogger<CustomerService> _logger;
        private readonly IErrorLogService? _errorLogService;

        public CustomerService(AppDbContext context, ILogger<CustomerService> logger, IErrorLogService? errorLogService = null) : base(context)
        {
            _logger = logger;
            _errorLogService = errorLogService;
        }

        #region 覆寫基底方法

        public override async Task<List<Customer>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.CustomerType)
                .Include(c => c.IndustryType)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public override async Task<Customer?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.CustomerType)
                .Include(c => c.IndustryType)
                .Include(c => c.CustomerContacts)
                    .ThenInclude(cc => cc.ContactType)
                .Include(c => c.CustomerAddresses)
                    .ThenInclude(ca => ca.AddressType)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public override async Task<List<Customer>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Include(c => c.CustomerType)
                .Include(c => c.IndustryType)
                .Where(c => !c.IsDeleted && 
                           (c.CustomerCode.Contains(searchTerm) ||
                            c.CompanyName.Contains(searchTerm) ||
                            (c.ContactPerson != null && c.ContactPerson.Contains(searchTerm)) ||
                            (c.TaxNumber != null && c.TaxNumber.Contains(searchTerm))))
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public override async Task<ServiceResult> ValidateAsync(Customer entity)
        {
            var errors = new List<string>();

            // 檢查必要欄位
            if (string.IsNullOrWhiteSpace(entity.CustomerCode))
                errors.Add("客戶代碼為必填");
            
            if (string.IsNullOrWhiteSpace(entity.CompanyName))
                errors.Add("公司名稱為必填");

            // 檢查長度限制
            if (entity.CustomerCode?.Length > 20)
                errors.Add("客戶代碼不可超過20個字元");
            
            if (entity.CompanyName?.Length > 100)
                errors.Add("公司名稱不可超過100個字元");

            if (!string.IsNullOrEmpty(entity.ContactPerson) && entity.ContactPerson.Length > 50)
                errors.Add("聯絡人不可超過50個字元");

            if (!string.IsNullOrEmpty(entity.TaxNumber) && entity.TaxNumber.Length > 20)
                errors.Add("統一編號不可超過20個字元");

            // 檢查客戶代碼是否重複
            if (!string.IsNullOrWhiteSpace(entity.CustomerCode))
            {
                var isDuplicate = await _dbSet
                    .Where(c => c.CustomerCode == entity.CustomerCode && !c.IsDeleted)
                    .Where(c => c.Id != entity.Id) // 排除自己
                    .AnyAsync();

                if (isDuplicate)
                    errors.Add("客戶代碼已存在");
            }

            // 檢查客戶類型是否存在
            if (entity.CustomerTypeId.HasValue)
            {
                var customerTypeExists = await _context.CustomerTypes
                    .AnyAsync(ct => ct.Id == entity.CustomerTypeId.Value && ct.Status == EntityStatus.Active);

                if (!customerTypeExists)
                    errors.Add("指定的客戶類型不存在或已停用");
            }

            // 檢查行業類型是否存在
            if (entity.IndustryTypeId.HasValue)
            {
                var industryTypeExists = await _context.IndustryTypes
                    .AnyAsync(it => it.Id == entity.IndustryTypeId.Value && it.Status == EntityStatus.Active);

                if (!industryTypeExists)
                    errors.Add("指定的行業類型不存在或已停用");
            }

            if (errors.Any())
                return ServiceResult.Failure(string.Join("; ", errors));

            return ServiceResult.Success();
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<Customer?> GetByCustomerCodeAsync(string customerCode)
        {
            if (string.IsNullOrWhiteSpace(customerCode))
                return null;

            try
            {
                return await _dbSet
                    .Include(c => c.CustomerType)
                    .Include(c => c.IndustryType)
                    .FirstOrDefaultAsync(c => c.CustomerCode == customerCode && !c.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer by code {CustomerCode}", customerCode);
                throw;
            }
        }

        public async Task<List<Customer>> GetByCompanyNameAsync(string companyName)
        {
            if (string.IsNullOrWhiteSpace(companyName))
                return new List<Customer>();

            try
            {
                return await _dbSet
                    .Include(c => c.CustomerType)
                    .Include(c => c.IndustryType)
                    .Where(c => c.CompanyName.Contains(companyName) && !c.IsDeleted)
                    .OrderBy(c => c.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers by company name {CompanyName}", companyName);
                throw;
            }
        }

        public async Task<bool> IsCustomerCodeExistsAsync(string customerCode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(customerCode))
                return false;

            try
            {
                var query = _dbSet.Where(c => c.CustomerCode == customerCode && !c.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(c => c.Id != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if customer code exists {CustomerCode}", customerCode);
                throw;
            }
        }

        #endregion

        #region 關聯資料查詢

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

        public async Task<List<IndustryType>> GetIndustryTypesAsync()
        {
            try
            {
                return await _context.IndustryTypes
                    .Where(it => it.Status == EntityStatus.Active)
                    .OrderBy(it => it.IndustryTypeName)
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
                return await _context.ContactTypes
                    .Where(ct => ct.Status == EntityStatus.Active)
                    .OrderBy(ct => ct.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact types");
                throw;
            }
        }        public async Task<List<AddressType>> GetAddressTypesAsync()
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
        
        public async Task<List<CustomerType>> SearchCustomerTypesAsync(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return new List<CustomerType>();
                    
                return await _context.CustomerTypes
                    .Where(ct => ct.Status == EntityStatus.Active && 
                                ct.TypeName.Contains(keyword))
                    .OrderBy(ct => ct.TypeName)
                    .Take(10) // 限制結果數量
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customer types with keyword: {Keyword}", keyword);
                throw;
            }
        }
        
        public async Task<List<IndustryType>> SearchIndustryTypesAsync(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return new List<IndustryType>();
                    
                return await _context.IndustryTypes
                    .Where(it => it.Status == EntityStatus.Active && 
                                it.IndustryTypeName.Contains(keyword))
                    .OrderBy(it => it.IndustryTypeName)
                    .Take(10) // 限制結果數量
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching industry types with keyword: {Keyword}", keyword);
                throw;
            }
        }

        #endregion

        #region 聯絡資料管理

        public async Task<List<CustomerContact>> GetCustomerContactsAsync(int customerId)
        {
            try
            {
                return await _context.CustomerContacts
                    .Include(cc => cc.ContactType)
                    .Where(cc => cc.CustomerId == customerId && !cc.IsDeleted)
                    .OrderBy(cc => cc.ContactType!.TypeName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer contacts for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<ServiceResult> UpdateCustomerContactsAsync(int customerId, List<CustomerContact> contacts)
        {
            try
            {
                // 驗證客戶是否存在
                var customerExists = await _dbSet.AnyAsync(c => c.Id == customerId && !c.IsDeleted);
                if (!customerExists)
                    return ServiceResult.Failure("客戶不存在");

                // 取得現有聯絡資料
                var existingContacts = await _context.CustomerContacts
                    .Where(cc => cc.CustomerId == customerId)
                    .ToListAsync();

                // 刪除現有聯絡資料
                _context.CustomerContacts.RemoveRange(existingContacts);                // 新增更新的聯絡資料
                foreach (var contact in contacts.Where(c => !string.IsNullOrWhiteSpace(c.ContactValue)))
                {
                    // 建立新的聯絡實體以避免 ID 衝突
                    var newContact = new CustomerContact
                    {
                        CustomerId = customerId,
                        ContactTypeId = contact.ContactTypeId,
                        ContactValue = contact.ContactValue,
                        IsPrimary = contact.IsPrimary,
                        Status = EntityStatus.Active,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System", // TODO: 從認證取得使用者
                        Remarks = contact.Remarks
                    };
                    _context.CustomerContacts.Add(newContact);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated contacts for customer {CustomerId}", customerId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer contacts for customer {CustomerId}", customerId);
                return ServiceResult.Failure("更新客戶聯絡資料時發生錯誤");
            }
        }

        #endregion

        #region 輔助方法

        public void InitializeNewCustomer(Customer customer)
        {
            customer.CustomerCode = string.Empty;
            customer.CompanyName = string.Empty;
            customer.ContactPerson = string.Empty;
            customer.TaxNumber = string.Empty;
            customer.CustomerTypeId = null;
            customer.IndustryTypeId = null;
            customer.Status = EntityStatus.Active;
        }

        public int GetBasicRequiredFieldsCount()
        {
            return 2; // CustomerCode, CompanyName
        }

        public int GetBasicCompletedFieldsCount(Customer customer)
        {
            int count = 0;

            if (!string.IsNullOrWhiteSpace(customer.CustomerCode))
                count++;

            if (!string.IsNullOrWhiteSpace(customer.CompanyName))
                count++;

            return count;
        }

        #endregion
    }
}
