using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工服務實作
    /// </summary>
    public class EmployeeService : GenericManagementService<Employee>, IEmployeeService
    {
        public EmployeeService(AppDbContext context) : base(context)
        {
        }        // 覆寫 GetAllAsync 以載入相關資料
        public override async Task<List<Employee>> GetAllAsync()
        {
            return await _dbSet
                .Include(e => e.Role)
                .Include(e => e.EmployeeContacts)
                    .ThenInclude(ec => ec.ContactType)
                .Include(e => e.EmployeeAddresses)
                    .ThenInclude(ea => ea.AddressType)
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.EmployeeCode)
                .ToListAsync();
        }        // 覆寫 GetByIdAsync 以載入相關資料
        public override async Task<Employee?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Role)
                .Include(e => e.EmployeeContacts)
                    .ThenInclude(ec => ec.ContactType)
                .Include(e => e.EmployeeAddresses)
                    .ThenInclude(ea => ea.AddressType)
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        /// <summary>
        /// 根據使用者名稱取得員工
        /// </summary>
        public async Task<ServiceResult<Employee>> GetByUsernameAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return ServiceResult<Employee>.Failure("使用者名稱不能為空");

                var employee = await _dbSet
                    .Include(e => e.Role)
                    .FirstOrDefaultAsync(e => e.Username == username && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult<Employee>.Failure("找不到指定的員工");

                return ServiceResult<Employee>.Success(employee);
            }
            catch (Exception ex)
            {
                return ServiceResult<Employee>.Failure($"取得員工資料時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 根據員工編號取得員工
        /// </summary>
        public async Task<ServiceResult<Employee>> GetByEmployeeCodeAsync(string employeeCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeCode))
                    return ServiceResult<Employee>.Failure("員工編號不能為空");

                var employee = await _dbSet
                    .Include(e => e.Role)
                    .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult<Employee>.Failure("找不到指定的員工");

                return ServiceResult<Employee>.Success(employee);
            }
            catch (Exception ex)
            {
                return ServiceResult<Employee>.Failure($"取得員工資料時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查使用者名稱是否已存在
        /// </summary>
        public async Task<ServiceResult<bool>> IsUsernameExistsAsync(string username, int? excludeEmployeeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    return ServiceResult<bool>.Failure("使用者名稱不能為空");

                var query = _dbSet.Where(e => e.Username == username && !e.IsDeleted);

                if (excludeEmployeeId.HasValue)
                    query = query.Where(e => e.Id != excludeEmployeeId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"檢查使用者名稱時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查員工編號是否已存在
        /// </summary>
        public async Task<ServiceResult<bool>> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeCode))
                    return ServiceResult<bool>.Failure("員工編號不能為空");

                var query = _dbSet.Where(e => e.EmployeeCode == employeeCode && !e.IsDeleted);

                if (excludeEmployeeId.HasValue)
                    query = query.Where(e => e.Id != excludeEmployeeId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"檢查員工編號時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 搜尋員工（根據姓名、員工編號或使用者名稱）
        /// </summary>
        public async Task<ServiceResult<List<Employee>>> SearchEmployeesAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return ServiceResult<List<Employee>>.Success(await GetAllAsync());                var employees = await _dbSet
                    .Include(e => e.Role)
                    .Where(e => !e.IsDeleted && 
                               ((e.FirstName != null && e.FirstName.Contains(searchTerm)) ||
                                (e.LastName != null && e.LastName.Contains(searchTerm)) ||
                                e.EmployeeCode.Contains(searchTerm) ||
                                e.Username.Contains(searchTerm) ||
                                (e.Department != null && e.Department.Contains(searchTerm))))
                    .OrderBy(e => e.EmployeeCode)
                    .ToListAsync();

                return ServiceResult<List<Employee>>.Success(employees);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Employee>>.Failure($"搜尋員工時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得指定角色的所有員工
        /// </summary>
        public async Task<ServiceResult<List<Employee>>> GetEmployeesByRoleAsync(int roleId)
        {
            try
            {
                var employees = await _dbSet
                    .Include(e => e.Role)
                    .Where(e => e.RoleId == roleId && !e.IsDeleted)
                    .OrderBy(e => e.EmployeeCode)
                    .ToListAsync();

                return ServiceResult<List<Employee>>.Success(employees);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Employee>>.Failure($"取得角色員工時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得指定部門的所有員工
        /// </summary>
        public async Task<ServiceResult<List<Employee>>> GetEmployeesByDepartmentAsync(string department)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(department))
                    return ServiceResult<List<Employee>>.Failure("部門名稱不能為空");

                var employees = await _dbSet
                    .Include(e => e.Role)
                    .Where(e => e.Department == department && !e.IsDeleted)
                    .OrderBy(e => e.EmployeeCode)
                    .ToListAsync();

                return ServiceResult<List<Employee>>.Success(employees);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Employee>>.Failure($"取得部門員工時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 變更員工角色
        /// </summary>
        public async Task<ServiceResult> ChangeEmployeeRoleAsync(int employeeId, int newRoleId)
        {
            try
            {
                var employee = await GetByIdAsync(employeeId);
                if (employee == null)
                    return ServiceResult.Failure("員工不存在");

                // 檢查新角色是否存在
                var roleExists = await _context.Roles.AnyAsync(r => r.Id == newRoleId && !r.IsDeleted);
                if (!roleExists)
                    return ServiceResult.Failure("指定的角色不存在");

                employee.RoleId = newRoleId;
                employee.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"變更員工角色時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 啟用/停用員工帳號
        /// </summary>
        public async Task<ServiceResult> SetEmployeeActiveStatusAsync(int employeeId, bool isActive)
        {
            try
            {
                var status = isActive ? EntityStatus.Active : EntityStatus.Inactive;
                return await SetStatusAsync(employeeId, status);
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"設定員工狀態時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 取得可登入的員工清單（啟用且未刪除）
        /// </summary>
        public async Task<ServiceResult<List<Employee>>> GetActiveEmployeesAsync()
        {
            try
            {
                var employees = await _dbSet
                    .Include(e => e.Role)
                    .Where(e => !e.IsDeleted && e.Status == EntityStatus.Active)
                    .OrderBy(e => e.EmployeeCode)
                    .ToListAsync();

                return ServiceResult<List<Employee>>.Success(employees);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<Employee>>.Failure($"取得啟用員工清單時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 驗證員工資料完整性
        /// </summary>
        public ServiceResult<bool> ValidateEmployeeData(Employee employee)
        {
            if (employee == null)
                return ServiceResult<bool>.Failure("員工資料不能為空");

            // 驗證必要欄位
            if (string.IsNullOrWhiteSpace(employee.FirstName))
                return ServiceResult<bool>.Failure("名字不能為空");

            if (string.IsNullOrWhiteSpace(employee.LastName))
                return ServiceResult<bool>.Failure("姓氏不能為空");

            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                return ServiceResult<bool>.Failure("員工編號不能為空");

            if (string.IsNullOrWhiteSpace(employee.Username))
                return ServiceResult<bool>.Failure("使用者名稱不能為空");

            // 驗證員工編號格式
            if (!Regex.IsMatch(employee.EmployeeCode, @"^[A-Z0-9]+$"))
                return ServiceResult<bool>.Failure("員工編號只能包含大寫字母和數字");            // 驗證使用者名稱格式
            if (!Regex.IsMatch(employee.Username, @"^[a-zA-Z0-9_]+$"))
                return ServiceResult<bool>.Failure("使用者名稱只能包含英文字母、數字和底線");

            // 驗證角色ID
            if (employee.RoleId <= 0)
                return ServiceResult<bool>.Failure("必須指定有效的角色");

            return ServiceResult<bool>.Success(true);
        }

        /// <summary>
        /// 產生下一個員工編號
        /// </summary>
        public async Task<ServiceResult<string>> GenerateNextEmployeeCodeAsync(string prefix = "EMP")
        {
            try
            {
                var lastEmployee = await _dbSet
                    .Where(e => e.EmployeeCode.StartsWith(prefix))
                    .OrderByDescending(e => e.EmployeeCode)
                    .FirstOrDefaultAsync();

                if (lastEmployee == null)
                    return ServiceResult<string>.Success($"{prefix}001");

                var lastCode = lastEmployee.EmployeeCode;
                var numberPart = lastCode.Substring(prefix.Length);

                if (int.TryParse(numberPart, out int lastNumber))
                {
                    var nextNumber = lastNumber + 1;
                    var nextCode = $"{prefix}{nextNumber:D3}";
                    return ServiceResult<string>.Success(nextCode);
                }

                return ServiceResult<string>.Failure("無法解析最後的員工編號");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure($"產生員工編號時發生錯誤：{ex.Message}");
            }
        }        /// <summary>
        /// 驗證員工資料
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Employee entity)        {
            var errors = new List<string>();

            // 檢查必要欄位
            if (string.IsNullOrWhiteSpace(entity.Username))
                errors.Add("使用者名稱為必填");

            if (string.IsNullOrWhiteSpace(entity.FirstName))
                errors.Add("名字為必填");

            if (string.IsNullOrWhiteSpace(entity.LastName))
                errors.Add("姓氏為必填");

            // 檢查長度限制
            if (entity.Username?.Length > 50)
                errors.Add("使用者名稱不可超過50個字元");            if (entity.FirstName?.Length > 50)
                errors.Add("名字不可超過50個字元");

            if (entity.LastName?.Length > 50)
                errors.Add("姓氏不可超過50個字元");

            if (!string.IsNullOrEmpty(entity.EmployeeCode) && entity.EmployeeCode.Length > 20)
                errors.Add("員工代碼不可超過20個字元");

            // 檢查使用者名稱是否重複
            if (!string.IsNullOrWhiteSpace(entity.Username))
            {
                var isDuplicate = await _dbSet
                    .Where(e => e.Username == entity.Username && !e.IsDeleted)
                    .Where(e => e.Id != entity.Id) // 排除自己
                    .AnyAsync();

                if (isDuplicate)
                    errors.Add("使用者名稱已存在");
            }

            // 檢查員工代碼是否重複
            if (!string.IsNullOrWhiteSpace(entity.EmployeeCode))
            {
                var isCodeDuplicate = await _dbSet
                    .Where(e => e.EmployeeCode == entity.EmployeeCode && !e.IsDeleted)
                    .Where(e => e.Id != entity.Id) // 排除自己
                    .AnyAsync();

                if (isCodeDuplicate)
                    errors.Add("員工代碼已存在");
            }            // 檢查角色是否存在
            if (entity.RoleId > 0)
            {
                var roleExists = await _context.Roles
                    .AnyAsync(r => r.Id == entity.RoleId && !r.IsDeleted && r.Status == EntityStatus.Active);

                if (!roleExists)
                    errors.Add("指定的角色不存在或已停用");
            }
            else
            {
                errors.Add("必須指定有效的角色");
            }

            if (errors.Any())
                return ServiceResult.Failure(string.Join("; ", errors));

            return ServiceResult.Success();
        }        // 覆寫 SearchAsync 實作搜尋邏輯
        public override async Task<List<Employee>> SearchAsync(string searchTerm)
        {
            var result = await SearchEmployeesAsync(searchTerm);
            return result.IsSuccess ? result.Data ?? new List<Employee>() : new List<Employee>();
        }

        #region 聯絡資料與地址管理        /// <summary>
        /// 更新員工聯絡資料
        /// </summary>
        public async Task<ServiceResult> UpdateEmployeeContactsAsync(int employeeId, List<EmployeeContact> contacts)
        {
            try
            {
                // 移除現有的聯絡資料
                var existingContacts = await _context.EmployeeContacts
                    .Where(ec => ec.EmployeeId == employeeId)
                    .ToListAsync();
                _context.EmployeeContacts.RemoveRange(existingContacts);
                  // 新增新的聯絡資料
                foreach (var contact in contacts.Where(c => !string.IsNullOrWhiteSpace(c.ContactValue)))
                {
                    // 建立新的聯絡實體以避免 ID 衝突
                    var newContact = new EmployeeContact
                    {
                        EmployeeId = employeeId,
                        ContactTypeId = contact.ContactTypeId,
                        ContactValue = contact.ContactValue,
                        IsPrimary = contact.IsPrimary,
                        Status = EntityStatus.Active,
                        IsDeleted = false,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = "System", // TODO: 從認證取得使用者
                        Remarks = contact.Remarks
                    };
                    _context.EmployeeContacts.Add(newContact);
                }
                
                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"更新員工聯絡資料失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 更新員工地址資料
        /// </summary>
        public async Task<ServiceResult> UpdateEmployeeAddressesAsync(int employeeId, List<EmployeeAddress> addresses)
        {
            try
            {
                // 移除現有的地址資料
                var existingAddresses = await _context.EmployeeAddresses
                    .Where(ea => ea.EmployeeId == employeeId)
                    .ToListAsync();
                _context.EmployeeAddresses.RemoveRange(existingAddresses);
                  // 新增新的地址資料
                foreach (var address in addresses.Where(a => !string.IsNullOrWhiteSpace(a.Address) || !string.IsNullOrWhiteSpace(a.City)))
                {
                    // 建立新的地址實體以避免 ID 衝突
                    var newAddress = new EmployeeAddress
                    {
                        EmployeeId = employeeId,
                        AddressTypeId = address.AddressTypeId,
                        PostalCode = address.PostalCode,
                        City = address.City,
                        District = address.District,
                        Address = address.Address,
                        IsPrimary = address.IsPrimary,
                        Status = EntityStatus.Active,
                        IsDeleted = false,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = "System", // TODO: 從認證取得使用者
                        Remarks = address.Remarks
                    };
                    _context.EmployeeAddresses.Add(newAddress);
                }
                
                await _context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"更新員工地址資料失敗：{ex.Message}");
            }
        }

        #endregion
    }
}
