using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商聯絡方式管理服務實作 - 繼承 GenericManagementService
    /// </summary>
    public class SupplierContactService : GenericManagementService<SupplierContact>, ISupplierContactService
    {
        private readonly ILogger<SupplierContactService> _logger;

        public SupplierContactService(AppDbContext context, ILogger<SupplierContactService> logger) : base(context)
        {
            _logger = logger;
        }

        #region 覆寫基底方法

        public override async Task<List<SupplierContact>> GetAllAsync()
        {
            return await _dbSet
                .Include(sc => sc.Supplier)
                .Include(sc => sc.ContactType)
                .Where(sc => !sc.IsDeleted)
                .OrderBy(sc => sc.Supplier.CompanyName)
                .ThenBy(sc => sc.ContactType!.TypeName)
                .ToListAsync();
        }

        public override async Task<List<SupplierContact>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _dbSet
                .Include(sc => sc.Supplier)
                .Include(sc => sc.ContactType)
                .Where(sc => !sc.IsDeleted &&
                           (sc.ContactValue.Contains(searchTerm) ||
                            sc.Supplier.CompanyName.Contains(searchTerm) ||
                            (sc.ContactType != null && sc.ContactType.TypeName.Contains(searchTerm))))
                .OrderBy(sc => sc.Supplier.CompanyName)
                .ThenBy(sc => sc.ContactType!.TypeName)
                .ToListAsync();
        }

        public override async Task<ServiceResult> ValidateAsync(SupplierContact entity)
        {
            var errors = new List<string>();

            // 驗證必填欄位
            if (entity.SupplierId <= 0)
            {
                errors.Add("廠商為必填欄位");
            }

            if (string.IsNullOrWhiteSpace(entity.ContactValue))
            {
                errors.Add("聯絡內容為必填欄位");
            }

            // 驗證聯絡內容格式（基本驗證）
            if (!string.IsNullOrWhiteSpace(entity.ContactValue))
            {
                if (entity.ContactValue.Length > 100)
                {
                    errors.Add("聯絡內容不可超過100個字元");
                }

                // 如果有聯絡類型，可以根據類型進行格式驗證
                if (entity.ContactTypeId.HasValue)
                {
                    var contactType = await _context.ContactTypes.FindAsync(entity.ContactTypeId.Value);
                    if (contactType != null)
                    {
                        // 可以在這裡添加特定聯絡類型的格式驗證
                        // 例如：Email 格式、電話號碼格式等
                    }
                }
            }

            return errors.Any() 
                ? ServiceResult.Failure(string.Join("; ", errors))
                : ServiceResult.Success();
        }

        #endregion

        #region 業務特定查詢方法

        public async Task<List<SupplierContact>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                return await _dbSet
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

        public async Task<List<SupplierContact>> GetByContactTypeAsync(int contactTypeId)
        {
            try
            {
                return await _dbSet
                    .Include(sc => sc.Supplier)
                    .Include(sc => sc.ContactType)
                    .Where(sc => sc.ContactTypeId == contactTypeId && !sc.IsDeleted)
                    .OrderBy(sc => sc.Supplier.CompanyName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier contacts by type {ContactTypeId}", contactTypeId);
                throw;
            }
        }

        public async Task<SupplierContact?> GetPrimaryContactAsync(int supplierId)
        {
            try
            {
                return await _dbSet
                    .Include(sc => sc.ContactType)
                    .FirstOrDefaultAsync(sc => sc.SupplierId == supplierId && sc.IsPrimary && !sc.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary contact for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<SupplierContact?> GetContactByTypeAsync(int supplierId, int contactTypeId)
        {
            try
            {
                return await _dbSet
                    .Include(sc => sc.ContactType)
                    .FirstOrDefaultAsync(sc => sc.SupplierId == supplierId && 
                                              sc.ContactTypeId == contactTypeId && 
                                              !sc.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact by type for supplier {SupplierId}, type {ContactTypeId}", 
                    supplierId, contactTypeId);
                throw;
            }
        }

        #endregion

        #region 業務邏輯操作

        public async Task<ServiceResult> SetPrimaryContactAsync(int contactId)
        {
            try
            {
                var contact = await GetByIdAsync(contactId);
                if (contact == null)
                {
                    return ServiceResult.Failure("找不到指定的聯絡方式");
                }

                // 將同一廠商的其他聯絡方式設為非主要
                var otherContacts = await _dbSet
                    .Where(sc => sc.SupplierId == contact.SupplierId && 
                                sc.Id != contactId && 
                                sc.IsPrimary && 
                                !sc.IsDeleted)
                    .ToListAsync();

                foreach (var otherContact in otherContacts)
                {
                    otherContact.IsPrimary = false;
                    otherContact.UpdatedAt = DateTime.UtcNow;
                }

                // 設定指定聯絡方式為主要
                contact.IsPrimary = true;
                contact.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary contact {ContactId}", contactId);
                return ServiceResult.Failure($"設定主要聯絡方式時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult<SupplierContact>> CopyContactToSupplierAsync(SupplierContact sourceContact, int targetSupplierId, int? targetContactTypeId = null)
        {
            try
            {
                var newContact = new SupplierContact
                {
                    SupplierId = targetSupplierId,
                    ContactTypeId = targetContactTypeId ?? sourceContact.ContactTypeId,
                    ContactValue = sourceContact.ContactValue,
                    IsPrimary = false, // 複製的聯絡方式預設不是主要聯絡方式
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await CreateAsync(newContact);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error copying contact to supplier {TargetSupplierId}", targetSupplierId);
                return ServiceResult<SupplierContact>.Failure($"複製聯絡方式時發生錯誤: {ex.Message}");
            }
        }

        public async Task<ServiceResult> EnsureSupplierHasPrimaryContactAsync(int supplierId)
        {
            try
            {
                var hasEemail = await _dbSet
                    .AnyAsync(sc => sc.SupplierId == supplierId && sc.IsPrimary && !sc.IsDeleted);

                if (!hasEemail)
                {
                    // 找第一個可用的聯絡方式設為主要
                    var firstContact = await _dbSet
                        .Where(sc => sc.SupplierId == supplierId && !sc.IsDeleted)
                        .OrderBy(sc => sc.Id)
                        .FirstOrDefaultAsync();

                    if (firstContact != null)
                    {
                        firstContact.IsPrimary = true;
                        firstContact.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring supplier has primary contact {SupplierId}", supplierId);
                return ServiceResult.Failure($"確保主要聯絡方式時發生錯誤: {ex.Message}");
            }
        }

        public async Task<List<SupplierContact>> GetContactsWithDefaultAsync(int supplierId, List<ContactType> contactTypes)
        {
            try
            {
                var contacts = await GetBySupplierIdAsync(supplierId);
                
                // 如果沒有聯絡方式，建立預設的聯絡方式
                if (!contacts.Any())
                {
                    InitializeDefaultContacts(contacts, contactTypes);
                }

                return contacts.OrderBy(c => c.ContactType?.TypeName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts with default for supplier {SupplierId}", supplierId);
                throw;
            }
        }

        public async Task<ServiceResult> UpdateSupplierContactsAsync(int supplierId, List<SupplierContact> contacts)
        {
            try
            {
                // 取得現有聯絡方式
                var existingContacts = await _dbSet
                    .Where(sc => sc.SupplierId == supplierId && !sc.IsDeleted)
                    .ToListAsync();

                // 刪除不在新列表中的聯絡方式
                var contactsToDelete = existingContacts
                    .Where(ec => !contacts.Any(c => c.Id == ec.Id))
                    .ToList();

                foreach (var contact in contactsToDelete)
                {
                    contact.IsDeleted = true;
                    contact.UpdatedAt = DateTime.UtcNow;
                }

                // 更新或新增聯絡方式
                foreach (var contact in contacts)
                {
                    contact.SupplierId = supplierId;
                    
                    if (contact.Id == 0)
                    {
                        // 新增
                        contact.CreatedAt = DateTime.UtcNow;
                        contact.UpdatedAt = DateTime.UtcNow;
                        _dbSet.Add(contact);
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
                return ServiceResult.Failure($"更新廠商聯絡方式時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 記憶體操作方法（用於UI編輯）

        public SupplierContact CreateNewContact(int supplierId, int contactCount)
        {
            return new SupplierContact
            {
                SupplierId = supplierId,
                ContactTypeId = null,
                ContactValue = string.Empty,
                IsPrimary = contactCount == 0, // 第一個聯絡方式預設為主要
                Status = EntityStatus.Active
            };
        }

        public void InitializeDefaultContacts(List<SupplierContact> contactList, List<ContactType> contactTypes)
        {
            // 確保有基本的聯絡類型
            var defaultTypes = new[] { "電話", "傳真", "Email" };

            foreach (var typeName in defaultTypes)
            {
                var contactType = contactTypes.FirstOrDefault(ct => ct.TypeName == typeName);
                if (contactType != null && !contactList.Any(c => c.ContactTypeId == contactType.Id))
                {
                    var newContact = new SupplierContact
                    {
                        ContactTypeId = contactType.Id,
                        ContactValue = string.Empty,
                        IsPrimary = contactList.Count == 0,
                        Status = EntityStatus.Active
                    };
                    contactList.Add(newContact);
                }
            }
        }

        public int? GetDefaultContactTypeId(int contactCount, List<ContactType> contactTypes)
        {
            // 根據聯絡方式數量決定預設的聯絡類型
            var defaultTypes = new[] { "電話", "傳真", "Email" };
            
            if (contactCount < defaultTypes.Length)
            {
                var typeName = defaultTypes[contactCount];
                var contactType = contactTypes.FirstOrDefault(ct => ct.TypeName == typeName);
                return contactType?.Id;
            }
            
            // 如果超出預設類型，返回第一個可用的類型
            return contactTypes.FirstOrDefault()?.Id;
        }

        public ServiceResult EnsurePrimaryContactExists(List<SupplierContact> contacts)
        {
            if (!contacts.Any(c => c.IsPrimary))
            {
                var firstContact = contacts.FirstOrDefault();
                if (firstContact != null)
                {
                    firstContact.IsPrimary = true;
                }
            }

            return ServiceResult.Success();
        }

        public int GetCompletedContactCount(List<SupplierContact> contacts)
        {
            if (contacts == null)
                return 0;

            return contacts.Count(c => 
                !string.IsNullOrWhiteSpace(c.ContactValue) &&
                c.ContactTypeId.HasValue);
        }

        public int GetContactCompletedFieldsCount(List<SupplierContact> contacts)
        {
            if (contacts == null)
                return 0;

            int totalFields = 0;
            int completedFields = 0;

            foreach (var contact in contacts)
            {
                totalFields += 2; // ContactTypeId, ContactValue

                if (contact.ContactTypeId.HasValue)
                    completedFields++;

                if (!string.IsNullOrWhiteSpace(contact.ContactValue))
                    completedFields++;
            }

            return completedFields;
        }

        #endregion
    }
}
