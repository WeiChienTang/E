using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 聯絡方式服務實作
    /// </summary>
    public class ContactService : GenericManagementService<Contact>, IContactService
    {
        public ContactService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public ContactService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Contact>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<Contact>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Contacts
                    .Include(c => c.ContactType)
                    .AsQueryable()
                    .OrderBy(c => c.OwnerType)
                    .ThenBy(c => c.OwnerId)
                    .ThenByDescending(c => c.IsPrimary)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                return new List<Contact>();
            }
        }

        public async Task<List<Contact>> GetByOwnerAsync(string ownerType, int ownerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var result = await context.Contacts
                    .Include(c => c.ContactType)
                    .Where(c => c.OwnerType == ownerType && c.OwnerId == ownerId)
                    .OrderByDescending(c => c.IsPrimary)
                    .ThenBy(c => c.ContactType!.TypeName)
                    .ToListAsync();
                
                return result;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByOwnerAsync), GetType(), _logger, new { 
                    Method = nameof(GetByOwnerAsync),
                    ServiceType = GetType().Name,
                    OwnerType = ownerType,
                    OwnerId = ownerId 
                });
                return new List<Contact>();
            }
        }

        public async Task<Contact?> GetPrimaryContactAsync(string ownerType, int ownerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Contacts
                    .Include(c => c.ContactType)
                    .FirstOrDefaultAsync(c => c.OwnerType == ownerType && c.OwnerId == ownerId && c.IsPrimary);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPrimaryContactAsync), GetType(), _logger, new { 
                    Method = nameof(GetPrimaryContactAsync),
                    ServiceType = GetType().Name,
                    OwnerType = ownerType,
                    OwnerId = ownerId 
                });
                return null;
            }
        }

        public async Task<ServiceResult> SetPrimaryContactAsync(int contactId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                var contact = await context.Contacts
                    .FirstOrDefaultAsync(c => c.Id == contactId);

                if (contact == null)
                    return ServiceResult.Failure("找不到指定的聯絡方式");

                // 清除同一擁有者的其他主要聯絡方式
                var otherContacts = await context.Contacts
                    .Where(c => c.OwnerType == contact.OwnerType && 
                               c.OwnerId == contact.OwnerId && 
                               c.Id != contactId && 
                               c.IsPrimary)
                    .ToListAsync();

                foreach (var otherContact in otherContacts)
                {
                    otherContact.IsPrimary = false;
                    otherContact.UpdatedAt = DateTime.Now;
                }

                // 設定新的主要聯絡方式
                contact.IsPrimary = true;
                contact.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetPrimaryContactAsync), GetType(), _logger, new { 
                    Method = nameof(SetPrimaryContactAsync),
                    ServiceType = GetType().Name,
                    ContactId = contactId 
                });
                return ServiceResult.Failure("設定主要聯絡方式時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteByOwnerAsync(string ownerType, int ownerId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var contacts = await context.Contacts
                    .Where(c => c.OwnerType == ownerType && c.OwnerId == ownerId)
                    .ToListAsync();

                foreach (var contact in contacts)
                {
                    contact.UpdatedAt = DateTime.Now;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteByOwnerAsync), GetType(), _logger, new { 
                    Method = nameof(DeleteByOwnerAsync),
                    ServiceType = GetType().Name,
                    OwnerType = ownerType,
                    OwnerId = ownerId 
                });
                return ServiceResult.Failure("刪除聯絡方式時發生錯誤");
            }
        }

        public override async Task<List<Contact>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                var term = searchTerm.Trim().ToLower();

                return await context.Contacts
                    .Include(c => c.ContactType)
                    .Where(c => (c.ContactValue.ToLower().Contains(term) ||
                                c.OwnerType.ToLower().Contains(term) ||
                                (c.ContactType != null && c.ContactType.TypeName.ToLower().Contains(term))))
                    .OrderBy(c => c.OwnerType)
                    .ThenBy(c => c.OwnerId)
                    .ThenByDescending(c => c.IsPrimary)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm 
                });
                return new List<Contact>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(Contact entity)
        {
            try
            {
                var errors = new List<string>();
                
                if (string.IsNullOrWhiteSpace(entity.OwnerType))
                    errors.Add("擁有者類型不能為空");
                
                if (entity.OwnerId <= 0)
                    errors.Add("擁有者ID必須大於0");
                
                if (string.IsNullOrWhiteSpace(entity.ContactValue))
                    errors.Add("聯絡內容不能為空");
                
                // 驗證擁有者類型是否有效
                var validOwnerTypes = new[] { ContactOwnerTypes.Customer, ContactOwnerTypes.Supplier, ContactOwnerTypes.Employee };
                if (!string.IsNullOrWhiteSpace(entity.OwnerType) && !validOwnerTypes.Contains(entity.OwnerType))
                    errors.Add("無效的擁有者類型");
                
                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));
                    
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    OwnerType = entity.OwnerType,
                    OwnerId = entity.OwnerId 
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }
    }
}

