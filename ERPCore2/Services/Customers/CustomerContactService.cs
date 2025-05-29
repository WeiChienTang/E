using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 客戶聯絡資料服務實作 - 使用 DbContextFactory 解決並發問題
    /// </summary>
    public class CustomerContactService : ICustomerContactService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<CustomerContactService> _logger;

        public CustomerContactService(IDbContextFactory<AppDbContext> contextFactory, ILogger<CustomerContactService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public string GetContactValue(int customerId, string contactTypeName, 
            List<ContactType> contactTypes, List<CustomerContact> customerContacts)
        {
            try
            {
                var contactType = contactTypes.FirstOrDefault(ct => ct.TypeName == contactTypeName);
                if (contactType == null) 
                {
                    _logger.LogWarning("Contact type '{ContactTypeName}' not found for customer {CustomerId}", 
                        contactTypeName, customerId);
                    return string.Empty;
                }

                var contact = customerContacts.FirstOrDefault(c => c.ContactTypeId == contactType.ContactTypeId);
                return contact?.ContactValue ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact value for customer {CustomerId}, contact type '{ContactTypeName}'", 
                    customerId, contactTypeName);
                return string.Empty;
            }
        }

        public ServiceResult UpdateContactValue(int customerId, string contactTypeName, string value,
            List<ContactType> contactTypes, List<CustomerContact> customerContacts)
        {
            try
            {
                var contactType = contactTypes.FirstOrDefault(ct => ct.TypeName == contactTypeName);
                if (contactType == null)
                {
                    var errorMessage = $"聯絡類型 '{contactTypeName}' 不存在";
                    _logger.LogWarning("Contact type '{ContactTypeName}' not found for customer {CustomerId}", 
                        contactTypeName, customerId);
                    return ServiceResult.Failure(errorMessage);
                }

                var existingContact = customerContacts.FirstOrDefault(c => c.ContactTypeId == contactType.ContactTypeId);

                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (existingContact != null)
                    {
                        // 更新現有聯絡資料
                        existingContact.ContactValue = value;
                        _logger.LogDebug("Updated contact value for customer {CustomerId}, contact type '{ContactTypeName}' to '{Value}'", 
                            customerId, contactTypeName, value);
                    }
                    else
                    {
                        // 新增聯絡資料
                        var newContact = new CustomerContact
                        {
                            CustomerId = customerId,
                            ContactTypeId = contactType.ContactTypeId,
                            ContactValue = value,
                            IsPrimary = !customerContacts.Any(c => c.IsPrimary && c.ContactTypeId == contactType.ContactTypeId),
                            Status = EntityStatus.Active
                        };
                        
                        customerContacts.Add(newContact);
                        _logger.LogDebug("Added new contact for customer {CustomerId}, contact type '{ContactTypeName}' with value '{Value}'", 
                            customerId, contactTypeName, value);
                    }
                }
                else if (existingContact != null)
                {
                    // 移除空的聯絡資料
                    customerContacts.Remove(existingContact);
                    _logger.LogDebug("Removed contact for customer {CustomerId}, contact type '{ContactTypeName}'", 
                        customerId, contactTypeName);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                var errorMessage = $"更新聯絡資料時發生錯誤: {ex.Message}";
                _logger.LogError(ex, "Error updating contact value for customer {CustomerId}, contact type '{ContactTypeName}'", 
                    customerId, contactTypeName);
                return ServiceResult.Failure(errorMessage);
            }
        }

        public int GetContactCompletedFieldsCount(List<CustomerContact> customerContacts)
        {
            try
            {
                return customerContacts.Count(c => !string.IsNullOrWhiteSpace(c.ContactValue));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting completed contact fields");
                return 0;
            }
        }

        public ServiceResult ValidateCustomerContacts(List<CustomerContact> customerContacts)
        {
            try
            {
                // 檢查聯絡資料值的長度
                var invalidContacts = customerContacts.Where(c => 
                    !string.IsNullOrEmpty(c.ContactValue) && c.ContactValue.Length > 100).ToList();

                if (invalidContacts.Any())
                {
                    return ServiceResult.Failure("部分聯絡資料超過100個字元的限制");
                }

                // 檢查是否有重複的聯絡類型
                var duplicateContactTypes = customerContacts
                    .Where(c => c.ContactTypeId.HasValue)
                    .GroupBy(c => c.ContactTypeId)
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (duplicateContactTypes.Any())
                {
                    return ServiceResult.Failure("存在重複的聯絡類型");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating customer contacts");
                return ServiceResult.Failure("驗證客戶聯絡資料時發生錯誤");
            }
        }

        public ServiceResult EnsureUniquePrimaryContacts(List<CustomerContact> customerContacts)
        {
            try
            {
                // 確保每種聯絡類型只有一個主要聯絡方式
                var contactTypes = customerContacts.Where(c => c.ContactTypeId.HasValue)
                                                 .GroupBy(c => c.ContactTypeId)
                                                 .ToList();

                foreach (var contactTypeGroup in contactTypes)
                {
                    var primaryContacts = contactTypeGroup.Where(c => c.IsPrimary).ToList();
                    if (primaryContacts.Count > 1)
                    {
                        // 如果有多個主要聯絡方式，只保留第一個
                        for (int i = 1; i < primaryContacts.Count; i++)
                        {
                            primaryContacts[i].IsPrimary = false;
                        }
                        
                        _logger.LogDebug("Fixed multiple primary contacts for contact type {ContactTypeId}", 
                            contactTypeGroup.Key);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring unique primary contacts");
                return ServiceResult.Failure("修正主要聯絡方式時發生錯誤");
            }
        }
    }
}
