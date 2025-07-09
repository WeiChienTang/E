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
    /// 員工聯絡資訊服務實作
    /// </summary>
    public class EmployeeContactService : GenericManagementService<EmployeeContact>, IEmployeeContactService
    {
        public EmployeeContactService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<EmployeeContact>> logger) : base(contextFactory, logger)
        {
        }

        public EmployeeContactService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        #region 覆寫基底抽象方法

        public override async Task<List<EmployeeContact>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeContacts
                    .Include(ec => ec.Employee)
                    .Include(ec => ec.ContactType)
                    .Where(ec => !ec.IsDeleted &&
                               (ec.ContactValue.Contains(searchTerm) ||
                                (ec.Employee != null && (ec.Employee.FirstName + " " + ec.Employee.LastName).Contains(searchTerm)) ||
                                (ec.Employee != null && ec.Employee.EmployeeCode.Contains(searchTerm)) ||
                                (ec.ContactType != null && ec.ContactType.TypeName.Contains(searchTerm))))
                    .OrderBy(ec => ec.Employee != null ? ec.Employee.EmployeeCode : string.Empty)
                    .ThenBy(ec => ec.ContactType != null ? ec.ContactType.TypeName : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { SearchTerm = searchTerm });
                return new List<EmployeeContact>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(EmployeeContact entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var errors = new List<string>();

                // 基本驗證 - 員工ID
                if (entity.EmployeeId <= 0)
                {
                    errors.Add("員工ID無效");
                }
                else
                {
                    // 檢查員工是否存在
                    var employeeExists = await context.Employees.AnyAsync(e => e.Id == entity.EmployeeId && !e.IsDeleted);
                    if (!employeeExists)
                    {
                        errors.Add("指定的員工不存在");
                    }
                }

                // 聯絡內容驗證
                if (string.IsNullOrWhiteSpace(entity.ContactValue))
                {
                    errors.Add("聯絡內容不能為空");
                }

                // 聯絡類型驗證
                if (entity.ContactTypeId.HasValue)
                {
                    var contactTypeExists = await context.ContactTypes.AnyAsync(ct => ct.Id == entity.ContactTypeId.Value && !ct.IsDeleted);
                    if (!contactTypeExists)
                    {
                        errors.Add("指定的聯絡類型不存在");
                    }
                }

                // 檢查是否重複（同一員工、同一聯絡類型、同一聯絡內容）
                var duplicateExists = await context.EmployeeContacts.AnyAsync(ec =>
                    ec.EmployeeId == entity.EmployeeId &&
                    ec.ContactTypeId == entity.ContactTypeId &&
                    ec.ContactValue == entity.ContactValue &&
                    ec.Id != entity.Id &&
                    !ec.IsDeleted);

                if (duplicateExists)
                {
                    errors.Add("此員工的該聯絡類型已存在相同的聯絡內容");
                }

                return errors.Any() ? ServiceResult.Failure(string.Join("; ", errors)) : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { EmployeeId = entity.EmployeeId });
                return ServiceResult.Failure("驗證員工聯絡資料時發生錯誤");
            }
        }

        public override async Task<List<EmployeeContact>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeContacts
                    .Include(ec => ec.Employee)
                    .Include(ec => ec.ContactType)
                    .Where(ec => !ec.IsDeleted)
                    .OrderBy(ec => ec.Employee != null ? ec.Employee.EmployeeCode : string.Empty)
                    .ThenBy(ec => ec.ContactType != null ? ec.ContactType.TypeName : string.Empty)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<EmployeeContact>();
            }
        }

        public override async Task<EmployeeContact?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.EmployeeContacts
                    .Include(ec => ec.Employee)
                    .Include(ec => ec.ContactType)
                    .FirstOrDefaultAsync(ec => ec.Id == id && !ec.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { Id = id });
                return null;
            }
        }

        #endregion

        #region IEmployeeContactService 實作

        /// <summary>
        /// 根據聯絡類型名稱取得員工的聯絡資料值
        /// </summary>
        public string GetContactValue(int employeeId, string contactTypeName,
            List<ContactType> contactTypes, List<EmployeeContact> employeeContacts)
        {
            try
            {
                var contactType = contactTypes.FirstOrDefault(ct => ct.TypeName == contactTypeName);
                if (contactType == null)
                    return string.Empty;

                var contact = employeeContacts.FirstOrDefault(ec =>
                    ec.EmployeeId == employeeId && ec.ContactTypeId == contactType.Id);

                return contact?.ContactValue ?? string.Empty;
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetContactValue), GetType(), _logger, new { EmployeeId = employeeId, ContactTypeName = contactTypeName });
                return string.Empty;
            }
        }

        /// <summary>
        /// 更新員工聯絡資料值
        /// </summary>
        public ServiceResult UpdateContactValue(int employeeId, string contactTypeName, string value,
            List<ContactType> contactTypes, List<EmployeeContact> employeeContacts)
        {
            try
            {
                var contactType = contactTypes.FirstOrDefault(ct => ct.TypeName == contactTypeName);
                if (contactType == null)
                    return ServiceResult.Failure($"找不到聯絡類型: {contactTypeName}");

                var existingContact = employeeContacts.FirstOrDefault(ec =>
                    ec.EmployeeId == employeeId && ec.ContactTypeId == contactType.Id);

                if (string.IsNullOrWhiteSpace(value))
                {
                    // 如果值為空，移除現有聯絡資料
                    if (existingContact != null)
                    {
                        employeeContacts.Remove(existingContact);
                    }
                }
                else
                {
                    if (existingContact != null)
                    {
                        // 更新現有聯絡資料
                        existingContact.ContactValue = value;
                        existingContact.UpdatedAt = DateTime.Now;
                    }
                    else
                    {
                        // 建立新的聯絡資料
                        var newContact = new EmployeeContact
                        {
                            EmployeeId = employeeId,
                            ContactTypeId = contactType.Id,
                            ContactValue = value,
                            IsPrimary = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            Status = EntityStatus.Active
                        };
                        employeeContacts.Add(newContact);
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(UpdateContactValue), GetType(), _logger, new { EmployeeId = employeeId, ContactTypeName = contactTypeName });
                return ServiceResult.Failure("更新聯絡資料時發生錯誤");
            }
        }

        /// <summary>
        /// 計算已完成的聯絡資料數量
        /// </summary>
        public int GetContactCompletedFieldsCount(List<EmployeeContact> employeeContacts)
        {
            try
            {
                return employeeContacts.Count(ec => !string.IsNullOrWhiteSpace(ec.ContactValue));
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(GetContactCompletedFieldsCount), GetType(), _logger);
                return 0;
            }
        }

        /// <summary>
        /// 驗證員工聯絡資料
        /// </summary>
        public ServiceResult ValidateEmployeeContacts(List<EmployeeContact> employeeContacts)
        {
            try
            {
                var errors = new List<string>();

                // 檢查重複的聯絡類型
                var duplicateTypes = employeeContacts
                    .Where(ec => ec.ContactTypeId.HasValue)
                    .GroupBy(ec => ec.ContactTypeId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateTypes.Any())
                {
                    errors.Add("存在重複的聯絡類型");
                }

                // 檢查每種聯絡類型的主要聯絡方式唯一性
                var primaryContactTypes = employeeContacts
                    .Where(ec => ec.IsPrimary && ec.ContactTypeId.HasValue)
                    .GroupBy(ec => ec.ContactTypeId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (primaryContactTypes.Any())
                {
                    errors.Add("每種聯絡類型只能有一個主要聯絡方式");
                }

                return errors.Any() ? ServiceResult.Failure(string.Join("; ", errors)) : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidateEmployeeContacts), GetType(), _logger);
                return ServiceResult.Failure("驗證員工聯絡資料時發生錯誤");
            }
        }

        /// <summary>
        /// 確保每種聯絡類型只有一個主要聯絡方式
        /// </summary>
        public ServiceResult EnsureUniquePrimaryContacts(List<EmployeeContact> employeeContacts)
        {
            try
            {                var contactTypeGroups = employeeContacts
                    .Where(ec => ec.ContactTypeId.HasValue)
                    .GroupBy(ec => ec.ContactTypeId!.Value)
                    .ToList();

                foreach (var group in contactTypeGroups)
                {
                    var primaryContacts = group.Where(ec => ec.IsPrimary).ToList();
                    if (primaryContacts.Count > 1)
                    {
                        // 保留第一個，其他設為非主要
                        for (int i = 1; i < primaryContacts.Count; i++)
                        {
                            primaryContacts[i].IsPrimary = false;
                        }
                    }
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(EnsureUniquePrimaryContacts), GetType(), _logger);
                return ServiceResult.Failure("處理主要聯絡方式時發生錯誤");
            }
        }

        /// <summary>
        /// 根據員工ID取得所有聯絡資料
        /// </summary>
        public async Task<ServiceResult<List<EmployeeContact>>> GetByEmployeeIdAsync(int employeeId)
        {
            try
            {
                if (employeeId <= 0)
                    return ServiceResult<List<EmployeeContact>>.Failure("員工ID無效");

                using var context = await _contextFactory.CreateDbContextAsync();
                var contacts = await context.EmployeeContacts
                    .Include(ec => ec.ContactType)
                    .Where(ec => ec.EmployeeId == employeeId && !ec.IsDeleted)
                    .OrderBy(ec => ec.ContactType != null ? ec.ContactType.TypeName : string.Empty)
                    .ToListAsync();

                return ServiceResult<List<EmployeeContact>>.Success(contacts);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeIdAsync), GetType(), _logger, new { EmployeeId = employeeId });
                return ServiceResult<List<EmployeeContact>>.Failure("取得員工聯絡資料時發生錯誤");
            }
        }

        /// <summary>
        /// 根據聯絡類型取得所有員工聯絡資料
        /// </summary>
        public async Task<ServiceResult<List<EmployeeContact>>> GetByContactTypeAsync(int contactTypeId)
        {
            try
            {
                if (contactTypeId <= 0)
                    return ServiceResult<List<EmployeeContact>>.Failure("聯絡類型ID無效");

                using var context = await _contextFactory.CreateDbContextAsync();
                var contacts = await context.EmployeeContacts
                    .Include(ec => ec.Employee)
                    .Where(ec => ec.ContactTypeId == contactTypeId && !ec.IsDeleted)
                    .OrderBy(ec => ec.Employee != null ? ec.Employee.EmployeeCode : string.Empty)
                    .ToListAsync();

                return ServiceResult<List<EmployeeContact>>.Success(contacts);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByContactTypeAsync), GetType(), _logger, new { ContactTypeId = contactTypeId });
                return ServiceResult<List<EmployeeContact>>.Failure("取得員工聯絡資料時發生錯誤");
            }
        }

        /// <summary>
        /// 設定主要聯絡方式
        /// </summary>
        public async Task<ServiceResult> SetAsPrimaryAsync(int employeeContactId)
        {
            try
            {
                var contact = await GetByIdAsync(employeeContactId);
                if (contact == null)
                    return ServiceResult.Failure("找不到指定的員工聯絡資料");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                // 將同一員工同一聯絡類型的其他聯絡方式設為非主要
                if (contact.ContactTypeId.HasValue)
                {
                    var otherContacts = await context.EmployeeContacts
                        .Where(ec => ec.EmployeeId == contact.EmployeeId &&
                                   ec.ContactTypeId == contact.ContactTypeId &&
                                   ec.Id != contact.Id &&
                                   !ec.IsDeleted)
                        .ToListAsync();

                    foreach (var otherContact in otherContacts)
                    {
                        otherContact.IsPrimary = false;
                        otherContact.UpdatedAt = DateTime.Now;
                    }
                }

                // 設定為主要聯絡方式
                contact.IsPrimary = true;
                contact.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetAsPrimaryAsync), GetType(), _logger, new { EmployeeContactId = employeeContactId });
                return ServiceResult.Failure("設定主要聯絡方式時發生錯誤");
            }
        }

        #endregion
    }
}


