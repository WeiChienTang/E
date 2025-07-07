using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶聯絡資訊服務實作
    /// </summary>
    public class CustomerContactService : GenericManagementService<CustomerContact>, ICustomerContactService
    {
        public CustomerContactService(
            AppDbContext context, 
            ILogger<GenericManagementService<CustomerContact>> logger) : base(context, logger)
        {
        }

        public CustomerContactService(AppDbContext context) : base(context)
        {
        }

        #region 覆寫基底抽象方法

        public override async Task<List<CustomerContact>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                return await _dbSet
                    .Include(cc => cc.Customer)
                    .Include(cc => cc.ContactType)
                    .Where(cc => !cc.IsDeleted &&
                               (cc.ContactValue.Contains(searchTerm) ||
                                (cc.Customer != null && cc.Customer.CompanyName.Contains(searchTerm)) ||
                                (cc.ContactType != null && cc.ContactType.TypeName.Contains(searchTerm))))
                    .OrderBy(cc => cc.Customer != null ? cc.Customer.CompanyName : string.Empty)
                    .ThenBy(cc => cc.ContactType != null ? cc.ContactType.TypeName : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<CustomerContact>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CustomerContact entity)
        {
            try
            {
                var errors = new List<string>();

                // 基本驗證 - 客戶ID
                if (entity.CustomerId <= 0)
                {
                    errors.Add("客戶ID無效");
                }
                else
                {
                    // 驗證客戶是否存在
                    var customerExists = await _context.Customers
                        .AnyAsync(c => c.Id == entity.CustomerId && !c.IsDeleted);
                    if (!customerExists)
                    {
                        errors.Add("指定的客戶不存在");
                    }
                }

                // 基本驗證 - 聯絡類型ID
                if (entity.ContactTypeId <= 0)
                {
                    errors.Add("聯絡類型ID無效");
                }
                else
                {
                    // 驗證聯絡類型是否存在
                    var contactTypeExists = await _context.ContactTypes
                        .AnyAsync(ct => ct.Id == entity.ContactTypeId && !ct.IsDeleted);
                    if (!contactTypeExists)
                    {
                        errors.Add("指定的聯絡類型不存在");
                    }
                }

                // 驗證聯絡資料值
                if (string.IsNullOrWhiteSpace(entity.ContactValue))
                {
                    errors.Add("聯絡資料值不能為空");
                }
                else if (entity.ContactValue.Length > 500)
                {
                    errors.Add("聯絡資料值長度不能超過500個字元");
                }

                // 檢查是否有重複的聯絡資料（同一客戶、同一聯絡類型）
                var duplicateQuery = _context.CustomerContacts
                    .Where(cc => cc.CustomerId == entity.CustomerId && 
                               cc.ContactTypeId == entity.ContactTypeId && 
                               !cc.IsDeleted);

                if (entity.Id > 0)
                {
                    duplicateQuery = duplicateQuery.Where(cc => cc.Id != entity.Id);
                }

                var hasDuplicate = await duplicateQuery.AnyAsync();
                if (hasDuplicate)
                {
                    errors.Add("該客戶已有相同類型的聯絡資料");
                }

                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger);
                return ServiceResult.Failure("驗證客戶聯絡資料時發生錯誤");
            }
        }

        #endregion

        #region 覆寫基底方法以包含關聯資料

        public override async Task<List<CustomerContact>> GetAllAsync()
        {
            try
            {
                return await _dbSet
                    .Include(cc => cc.Customer)
                    .Include(cc => cc.ContactType)
                    .Where(cc => !cc.IsDeleted)
                    .OrderBy(cc => cc.Customer != null ? cc.Customer.CompanyName : string.Empty)
                    .ThenBy(cc => cc.ContactType != null ? cc.ContactType.TypeName : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<CustomerContact>();
            }
        }

        public override async Task<CustomerContact?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(cc => cc.Customer)
                    .Include(cc => cc.ContactType)
                    .FirstOrDefaultAsync(cc => cc.Id == id && !cc.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        #endregion

        #region ICustomerContactService 特定方法

        /// <summary>
        /// 根據聯絡類型名稱取得客戶的聯絡資料值
        /// </summary>
        public string GetContactValue(int customerId, string contactTypeName, 
            List<ContactType> contactTypes, List<CustomerContact> customerContacts)
        {
            try
            {
                var contactType = contactTypes.FirstOrDefault(ct => ct.TypeName == contactTypeName);
                if (contactType == null)
                {
                    return string.Empty;
                }

                var contact = customerContacts.FirstOrDefault(cc => 
                    cc.CustomerId == customerId && cc.ContactTypeId == contactType.Id);

                return contact?.ContactValue ?? string.Empty;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetContactValue), GetType(), _logger, 
                    new { CustomerId = customerId, ContactTypeName = contactTypeName });
                return string.Empty;
            }
        }

        /// <summary>
        /// 更新客戶聯絡資料值
        /// </summary>
        public ServiceResult UpdateContactValue(int customerId, string contactTypeName, string value,
            List<ContactType> contactTypes, List<CustomerContact> customerContacts)
        {
            try
            {
                var contactType = contactTypes.FirstOrDefault(ct => ct.TypeName == contactTypeName);
                if (contactType == null)
                {
                    return ServiceResult.Failure($"找不到聯絡類型: {contactTypeName}");
                }

                var existingContact = customerContacts.FirstOrDefault(cc => 
                    cc.CustomerId == customerId && cc.ContactTypeId == contactType.Id);

                if (string.IsNullOrWhiteSpace(value))
                {
                    // 如果值為空且存在聯絡資料，則移除
                    if (existingContact != null)
                    {
                        customerContacts.Remove(existingContact);
                    }
                }
                else
                {
                    if (existingContact != null)
                    {
                        // 更新現有聯絡資料
                        existingContact.ContactValue = value.Trim();
                        existingContact.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // 新增聯絡資料
                        var newContact = new CustomerContact
                        {
                            CustomerId = customerId,
                            ContactTypeId = contactType.Id,
                            ContactValue = value.Trim(),
                            IsPrimary = false,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        customerContacts.Add(newContact);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(UpdateContactValue), GetType(), _logger, 
                    new { CustomerId = customerId, ContactTypeName = contactTypeName });
                return ServiceResult.Failure("更新聯絡資料時發生錯誤");
            }
        }

        /// <summary>
        /// 計算已完成的聯絡資料數量
        /// </summary>
        public int GetContactCompletedFieldsCount(List<CustomerContact> customerContacts)
        {
            try
            {
                return customerContacts
                    .Where(cc => cc.Status == EntityStatus.Active && !string.IsNullOrWhiteSpace(cc.ContactValue))
                    .Count();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetContactCompletedFieldsCount), GetType(), _logger);
                return 0;
            }
        }

        /// <summary>
        /// 驗證客戶聯絡資料
        /// </summary>
        public ServiceResult ValidateCustomerContacts(List<CustomerContact> customerContacts)
        {
            var errors = new List<string>();

            try
            {
                if (customerContacts == null)
                {
                    return ServiceResult.Failure("客戶聯絡資料清單不能為空");
                }

                foreach (var contact in customerContacts.Where(cc => cc.Status == EntityStatus.Active))
                {
                    // 驗證必要欄位
                    if (contact.CustomerId <= 0)
                    {
                        errors.Add("客戶ID無效");
                    }

                    if (contact.ContactTypeId <= 0)
                    {
                        errors.Add("聯絡類型ID無效");
                    }

                    if (string.IsNullOrWhiteSpace(contact.ContactValue))
                    {
                        continue; // 空值允許，跳過其他驗證
                    }

                    if (contact.ContactValue.Length > 500)
                    {
                        errors.Add("聯絡資料值長度不能超過500個字元");
                    }
                }

                // 檢查重複的聯絡類型
                var duplicateGroups = customerContacts
                    .Where(cc => cc.Status == EntityStatus.Active)
                    .GroupBy(cc => new { cc.CustomerId, cc.ContactTypeId })
                    .Where(g => g.Count() > 1);

                foreach (var group in duplicateGroups)
                {
                    errors.Add($"客戶 {group.Key.CustomerId} 的聯絡類型 {group.Key.ContactTypeId} 有重複資料");
                }

                return errors.Any() 
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidateCustomerContacts), GetType(), _logger);
                return ServiceResult.Failure("驗證客戶聯絡資料時發生錯誤");
            }
        }

        /// <summary>
        /// 確保每種聯絡類型只有一個主要聯絡方式
        /// </summary>
        public ServiceResult EnsureUniquePrimaryContacts(List<CustomerContact> customerContacts)
        {
            try
            {
                if (customerContacts == null)
                {
                    return ServiceResult.Success();
                }

                // 按客戶和聯絡類型分組
                var groupedContacts = customerContacts
                    .Where(cc => cc.Status == EntityStatus.Active)
                    .GroupBy(cc => new { cc.CustomerId, cc.ContactTypeId });

                foreach (var group in groupedContacts)
                {
                    var primaryContacts = group.Where(cc => cc.IsPrimary).ToList();
                    
                    if (primaryContacts.Count > 1)
                    {
                        // 如果有多個主要聯絡方式，只保留第一個
                        var firstPrimary = primaryContacts.First();
                        foreach (var contact in primaryContacts.Skip(1))
                        {
                            contact.IsPrimary = false;
                            contact.UpdatedAt = DateTime.UtcNow;
                        }
                        
                        _logger?.LogWarning("Customer {CustomerId} ContactType {ContactTypeId} had multiple primary contacts, kept only first one", 
                            group.Key.CustomerId, group.Key.ContactTypeId);
                    }
                    else if (primaryContacts.Count == 0 && group.Any())
                    {
                        // 如果沒有主要聯絡方式，設定第一個為主要
                        var firstContact = group.First();
                        firstContact.IsPrimary = true;
                        firstContact.UpdatedAt = DateTime.UtcNow;
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(EnsureUniquePrimaryContacts), GetType(), _logger);
                return ServiceResult.Failure("確保唯一主要聯絡方式時發生錯誤");
            }
        }

        #endregion
    }
}
