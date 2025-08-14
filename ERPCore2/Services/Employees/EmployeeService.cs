using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工服務實作
    /// </summary>
    public class EmployeeService : GenericManagementService<Employee>, IEmployeeService
    {
        /// <summary>
        /// 完整建構子 - 包含 Logger
        /// </summary>
        public EmployeeService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<Employee>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 簡易建構子 - 不包含 Logger
        /// </summary>
        public EmployeeService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        // 覆寫 GetAllAsync 以載入相關資料
        public override async Task<List<Employee>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new { 
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name 
                });
                throw;
            }
        }

        // 覆寫 GetByIdAsync 以載入相關資料
        public override async Task<Employee?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger, new { 
                    Method = nameof(GetByIdAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = id
                });
                throw;
            }
        }

        /// <summary>
        /// 根據帳號取得員工
        /// </summary>
        public async Task<ServiceResult<Employee>> GetByAccountAsync(string account)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(account))
                    return ServiceResult<Employee>.Failure("帳號不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .FirstOrDefaultAsync(e => e.Account == account && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult<Employee>.Failure("找不到指定的員工");

                return ServiceResult<Employee>.Success(employee);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByAccountAsync), GetType(), _logger, new { 
                    Method = nameof(GetByAccountAsync),
                    ServiceType = GetType().Name,
                    Account = account
                });
                return ServiceResult<Employee>.Failure($"取得員工資料時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 根據員工編號取得員工
        /// </summary>
        public async Task<ServiceResult<Employee>> GetByEmployeeCodeAsync(string Code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Code))
                    return ServiceResult<Employee>.Failure("員工編號不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .FirstOrDefaultAsync(e => e.Code == Code && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult<Employee>.Failure("找不到指定的員工");

                return ServiceResult<Employee>.Success(employee);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeCodeAsync), GetType(), _logger, new { 
                    Method = nameof(GetByEmployeeCodeAsync),
                    ServiceType = GetType().Name,
                    Code = Code
                });
                return ServiceResult<Employee>.Failure($"取得員工資料時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查帳號是否已存在
        /// </summary>
        public async Task<ServiceResult<bool>> IsAccountExistsAsync(string account, int? excludeEmployeeId = null)
        {
            try
            {
                // 如果帳號為空白，直接回傳不存在（允許多個空白帳號）
                if (string.IsNullOrWhiteSpace(account))
                    return ServiceResult<bool>.Success(false);

                using var context = await _contextFactory.CreateDbContextAsync();
                // 只檢查非空白帳號的重複性
                var query = context.Employees.Where(e => e.Account == account && !e.IsDeleted);

                if (excludeEmployeeId.HasValue)
                    query = query.Where(e => e.Id != excludeEmployeeId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsAccountExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsAccountExistsAsync),
                    ServiceType = GetType().Name,
                    Account = account,
                    ExcludeEmployeeId = excludeEmployeeId
                });
                return ServiceResult<bool>.Failure($"檢查帳號時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查員工編號是否已存在
        /// </summary>
        public async Task<ServiceResult<bool>> IsEmployeeCodeExistsAsync(string Code, int? excludeEmployeeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Code))
                    return ServiceResult<bool>.Failure("員工編號不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.Employees.Where(e => e.Code == Code && !e.IsDeleted);

                if (excludeEmployeeId.HasValue)
                    query = query.Where(e => e.Id != excludeEmployeeId.Value);

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsEmployeeCodeExistsAsync), GetType(), _logger, new { 
                    Method = nameof(IsEmployeeCodeExistsAsync),
                    ServiceType = GetType().Name,
                    Code = Code,
                    ExcludeEmployeeId = excludeEmployeeId
                });
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
                    return ServiceResult<List<Employee>>.Success(await GetAllAsync());

                using var context = await _contextFactory.CreateDbContextAsync();
                var employees = await context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .Where(e => !e.IsDeleted && 
                               ((e.FirstName != null && e.FirstName.Contains(searchTerm)) ||
                                (e.LastName != null && e.LastName.Contains(searchTerm)) ||
                                e.Code.Contains(searchTerm) ||
                                (e.Account != null && e.Account.Contains(searchTerm)) ||
                                (e.Department != null && e.Department.Name.Contains(searchTerm))))
                    .OrderBy(e => e.Code)
                    .ToListAsync();

                return ServiceResult<List<Employee>>.Success(employees);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchEmployeesAsync), GetType(), _logger, new { 
                    Method = nameof(SearchEmployeesAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
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
                using var context = await _contextFactory.CreateDbContextAsync();
                var employees = await context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .Where(e => e.RoleId.HasValue && e.RoleId.Value == roleId && !e.IsDeleted)
                    .OrderBy(e => e.Code)
                    .ToListAsync();

                return ServiceResult<List<Employee>>.Success(employees);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetEmployeesByRoleAsync), GetType(), _logger, new { 
                    Method = nameof(GetEmployeesByRoleAsync),
                    ServiceType = GetType().Name,
                    RoleId = roleId
                });
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

                using var context = await _contextFactory.CreateDbContextAsync();
                var employees = await context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .Where(e => e.Department != null && e.Department.Name == department && !e.IsDeleted)
                    .OrderBy(e => e.Code)
                    .ToListAsync();

                return ServiceResult<List<Employee>>.Success(employees);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetEmployeesByDepartmentAsync), GetType(), _logger, new { 
                    Method = nameof(GetEmployeesByDepartmentAsync),
                    ServiceType = GetType().Name,
                    Department = department
                });
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
                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);
                if (employee == null)
                    return ServiceResult.Failure("員工不存在");

                // 檢查新角色是否存在
                var roleExists = await context.Roles.AnyAsync(r => r.Id == newRoleId && !r.IsDeleted);
                if (!roleExists)
                    return ServiceResult.Failure("指定的角色不存在");

                employee.RoleId = newRoleId;
                employee.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ChangeEmployeeRoleAsync), GetType(), _logger, new { 
                    Method = nameof(ChangeEmployeeRoleAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId,
                    NewRoleId = newRoleId
                });
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetEmployeeActiveStatusAsync), GetType(), _logger, new { 
                    Method = nameof(SetEmployeeActiveStatusAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId,
                    IsActive = isActive
                });
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
                using var context = await _contextFactory.CreateDbContextAsync();
                var employees = await context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .Where(e => !e.IsDeleted && e.Status == EntityStatus.Active)
                    .OrderBy(e => e.Code)
                    .ToListAsync();

                return ServiceResult<List<Employee>>.Success(employees);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveEmployeesAsync), GetType(), _logger, new { 
                    Method = nameof(GetActiveEmployeesAsync),
                    ServiceType = GetType().Name
                });
                return ServiceResult<List<Employee>>.Failure($"取得啟用員工清單時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 驗證員工資料完整性
        /// </summary>
        public ServiceResult<bool> ValidateEmployeeData(Employee employee)
        {
            try
            {
                if (employee == null)
                    return ServiceResult<bool>.Failure("員工資料不能為空");

                // 驗證必要欄位
                if (string.IsNullOrWhiteSpace(employee.FirstName))
                    return ServiceResult<bool>.Failure("名字不能為空");

                if (string.IsNullOrWhiteSpace(employee.LastName))
                    return ServiceResult<bool>.Failure("姓氏不能為空");

                if (string.IsNullOrWhiteSpace(employee.Code))
                    return ServiceResult<bool>.Failure("員工編號不能為空");

                if (string.IsNullOrWhiteSpace(employee.Account))
                    return ServiceResult<bool>.Failure("使用者名稱不能為空");

                // 驗證員工編號格式
                if (!Regex.IsMatch(employee.Code, @"^[A-Z0-9]+$"))
                    return ServiceResult<bool>.Failure("員工編號只能包含大寫字母和數字");

                // 驗證使用者名稱格式
                if (!Regex.IsMatch(employee.Account, @"^[a-zA-Z0-9_]+$"))
                    return ServiceResult<bool>.Failure("使用者名稱只能包含英文字母、數字和底線");

                // 驗證角色ID（只有系統使用者才需要角色）
                if (employee.IsSystemUser && (!employee.RoleId.HasValue || employee.RoleId.Value <= 0))
                    return ServiceResult<bool>.Failure("系統使用者必須指定有效的角色");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidateEmployeeData), GetType(), _logger);
                return ServiceResult<bool>.Failure($"驗證員工資料時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 產生下一個員工編號
        /// </summary>
        public async Task<ServiceResult<string>> GenerateNextEmployeeCodeAsync(string prefix = "EMP")
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var lastEmployee = await context.Employees
                    .Where(e => e.Code.StartsWith(prefix))
                    .OrderByDescending(e => e.Code)
                    .FirstOrDefaultAsync();

                if (lastEmployee == null)
                    return ServiceResult<string>.Success($"{prefix}001");

                var lastCode = lastEmployee.Code;
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
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GenerateNextEmployeeCodeAsync), GetType(), _logger, new { 
                    Method = nameof(GenerateNextEmployeeCodeAsync),
                    ServiceType = GetType().Name,
                    Prefix = prefix
                });
                return ServiceResult<string>.Failure($"產生員工編號時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 驗證員工資料
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(Employee entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var errors = new List<string>();

                // 檢查必要欄位
                // 系統使用者的帳號和密碼驗證
                if (entity.IsSystemUser)
                {
                    if (string.IsNullOrWhiteSpace(entity.Account))
                        errors.Add("系統使用者必須設定帳號");
                    
                    if (string.IsNullOrWhiteSpace(entity.Password))
                        errors.Add("系統使用者必須設定密碼");
                }

                if (string.IsNullOrWhiteSpace(entity.FirstName))
                    errors.Add("名字為必填");

                if (string.IsNullOrWhiteSpace(entity.LastName))
                    errors.Add("姓氏為必填");

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("員工代碼為必填");

                // 檢查長度限制
                if (entity.Account?.Length > 50)
                    errors.Add("使用者名稱不可超過50個字元");

                if (entity.FirstName?.Length > 50)
                    errors.Add("名字不可超過50個字元");

                if (entity.LastName?.Length > 50)
                    errors.Add("姓氏不可超過50個字元");

                if (!string.IsNullOrEmpty(entity.Code) && entity.Code.Length > 20)
                    errors.Add("員工代碼不可超過20個字元");

                // 檢查帳號是否重複（只有系統使用者需要檢查帳號唯一性）
                if (entity.IsSystemUser && !string.IsNullOrWhiteSpace(entity.Account))
                {
                    var isDuplicate = await context.Employees
                        .Where(e => e.Account == entity.Account && !e.IsDeleted)
                        .Where(e => e.Id != entity.Id) // 排除自己
                        .AnyAsync();

                    if (isDuplicate)
                        errors.Add("此帳號已被其他員工使用");
                }

                // 檢查員工代碼是否重複
                if (!string.IsNullOrWhiteSpace(entity.Code))
                {
                    var isCodeDuplicate = await context.Employees
                        .Where(e => e.Code == entity.Code && !e.IsDeleted)
                        .Where(e => e.Id != entity.Id) // 排除自己
                        .AnyAsync();

                    if (isCodeDuplicate)
                        errors.Add("員工代碼已存在");
                }

                // 檢查角色是否存在（只有當指定了角色時才檢查）
                if (entity.RoleId.HasValue && entity.RoleId.Value > 0)
                {
                    var roleExists = await context.Roles
                        .AnyAsync(r => r.Id == entity.RoleId.Value && !r.IsDeleted && r.Status == EntityStatus.Active);

                    if (!roleExists)
                        errors.Add("指定的角色不存在或已停用");
                }
                // 系統使用者必須有角色
                else if (entity.IsSystemUser)
                {
                    errors.Add("系統使用者必須指定有效的角色");
                }

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new { 
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity?.Id
                });
                return ServiceResult.Failure($"驗證員工資料時發生錯誤：{ex.Message}");
            }
        }

        // 覆寫 SearchAsync 實作搜尋邏輯
        public override async Task<List<Employee>> SearchAsync(string searchTerm)
        {
            try
            {
                var result = await SearchEmployeesAsync(searchTerm);
                return result.IsSuccess ? result.Data ?? new List<Employee>() : new List<Employee>();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new { 
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<Employee>();
            }
        }

        #region 聯絡資料管理

        // 聯絡資料管理已移至 ContactService
        // 地址資料管理已移至 AddressService
        // #region 地址資料管理
        // ... UpdateEmployeeAddressesAsync 方法已移除，請使用 IAddressService
        // #endregion

        #endregion

        #region 軟刪除員工管理

        /// <summary>
        /// 檢查軟刪除的員工資料
        /// </summary>
        public async Task<ServiceResult<Employee?>> GetSoftDeletedEmployeeAsync(string username, string Code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(Code))
                    return ServiceResult<Employee?>.Failure("使用者名稱和員工編號不能都為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                
                var query = context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .Where(e => e.IsDeleted);

                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(Code))
                {
                    query = query.Where(e => e.Account == username || e.Code == Code);
                }
                else if (!string.IsNullOrWhiteSpace(username))
                {
                    query = query.Where(e => e.Account == username);
                }
                else
                {
                    query = query.Where(e => e.Code == Code);
                }

                var employee = await query.FirstOrDefaultAsync();
                return ServiceResult<Employee?>.Success(employee);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetSoftDeletedEmployeeAsync), GetType(), _logger, new { 
                    Method = nameof(GetSoftDeletedEmployeeAsync),
                    ServiceType = GetType().Name,
                    Account = username,
                    Code = Code
                });
                return ServiceResult<Employee?>.Failure($"檢查軟刪除員工時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 檢查新增員工時是否與軟刪除員工衝突
        /// </summary>
        public async Task<ServiceResult<IEmployeeService.SoftDeletedEmployeeCheckResult>> CheckSoftDeletedConflictAsync(string username, string Code)
        {
            try
            {
                var result = new IEmployeeService.SoftDeletedEmployeeCheckResult();

                if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(Code))
                    return ServiceResult<IEmployeeService.SoftDeletedEmployeeCheckResult>.Success(result);

                using var context = await _contextFactory.CreateDbContextAsync();
                
                Employee? softDeletedEmployee = null;
                var conflictTypes = new List<string>();

                // 檢查使用者名稱衝突（只有系統使用者的帳號才需要檢查）
                if (!string.IsNullOrWhiteSpace(username))
                {
                    var usernameConflict = await context.Employees
                        .Include(e => e.Role)
                        .Include(e => e.Department)
                        .Include(e => e.EmployeePosition)
                        .FirstOrDefaultAsync(e => e.Account == username && e.IsDeleted && e.IsSystemUser);
                    
                    if (usernameConflict != null)
                    {
                        softDeletedEmployee = usernameConflict;
                        conflictTypes.Add("Account");
                    }
                }

                // 檢查員工編號衝突
                if (!string.IsNullOrWhiteSpace(Code))
                {
                    var codeConflict = await context.Employees
                        .Include(e => e.Role)
                        .Include(e => e.Department)
                        .Include(e => e.EmployeePosition)
                        .FirstOrDefaultAsync(e => e.Code == Code && e.IsDeleted);
                    
                    if (codeConflict != null)
                    {
                        // 如果是同一個員工，優先使用這個
                        if (softDeletedEmployee == null || softDeletedEmployee.Id == codeConflict.Id)
                        {
                            softDeletedEmployee = codeConflict;
                            if (!conflictTypes.Contains("Account"))
                                conflictTypes.Add("Code");
                        }
                        else
                        {
                            // 不同員工有不同的衝突，這種情況比較複雜
                            conflictTypes.Add("Code");
                        }
                    }
                }

                if (softDeletedEmployee != null)
                {
                    result.HasSoftDeletedEmployee = true;
                    result.SoftDeletedEmployee = softDeletedEmployee;
                    result.ConflictType = string.Join(", ", conflictTypes);
                }

                return ServiceResult<IEmployeeService.SoftDeletedEmployeeCheckResult>.Success(result);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CheckSoftDeletedConflictAsync), GetType(), _logger, new { 
                    Method = nameof(CheckSoftDeletedConflictAsync),
                    ServiceType = GetType().Name,
                    Account = username,
                    Code = Code
                });
                return ServiceResult<IEmployeeService.SoftDeletedEmployeeCheckResult>.Failure($"檢查軟刪除衝突時發生錯誤：{ex.Message}");
            }
        }

        /// <summary>
        /// 復原軟刪除的員工
        /// </summary>
        public async Task<ServiceResult> RestoreSoftDeletedEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsDeleted);
                if (employee == null)
                {
                    return ServiceResult.Failure("找不到要復原的員工資料");
                }

                // 復原員工
                employee.IsDeleted = false;
                employee.Status = EntityStatus.Active; // 復原時預設為啟用狀態
                employee.FailedLoginAttempts = 0; // 重設失敗次數
                employee.UpdatedAt = DateTime.Now;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RestoreSoftDeletedEmployeeAsync), GetType(), _logger, new { 
                    Method = nameof(RestoreSoftDeletedEmployeeAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId
                });
                return ServiceResult.Failure($"復原員工失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 復原軟刪除的員工並更新資料
        /// </summary>
        public async Task<ServiceResult<Employee>> RestoreAndUpdateSoftDeletedEmployeeAsync(int employeeId, Employee updateData)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                
                var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsDeleted);
                if (employee == null)
                {
                    return ServiceResult<Employee>.Failure("找不到要復原的員工資料");
                }

                // 驗證角色是否存在且有效
                if (updateData.RoleId.HasValue && updateData.RoleId.Value > 0)
                {
                    var roleExists = await context.Roles
                        .AnyAsync(r => r.Id == updateData.RoleId.Value && !r.IsDeleted && r.Status == EntityStatus.Active);
                    
                    if (!roleExists)
                    {
                        return ServiceResult<Employee>.Failure($"指定的角色(ID: {updateData.RoleId})不存在或已停用");
                    }
                }

                // 驗證部門是否存在且有效（如果有指定）
                if (updateData.DepartmentId.HasValue && updateData.DepartmentId > 0)
                {
                    var departmentExists = await context.Departments
                        .AnyAsync(d => d.Id == updateData.DepartmentId && !d.IsDeleted && d.Status == EntityStatus.Active);
                    
                    if (!departmentExists)
                    {
                        return ServiceResult<Employee>.Failure($"指定的部門(ID: {updateData.DepartmentId})不存在或已停用");
                    }
                }

                // 驗證職位是否存在且有效（如果有指定）
                if (updateData.EmployeePositionId.HasValue && updateData.EmployeePositionId > 0)
                {
                    var positionExists = await context.EmployeePositions
                        .AnyAsync(p => p.Id == updateData.EmployeePositionId && !p.IsDeleted && p.Status == EntityStatus.Active);
                    
                    if (!positionExists)
                    {
                        return ServiceResult<Employee>.Failure($"指定的職位(ID: {updateData.EmployeePositionId})不存在或已停用");
                    }
                }

                // 復原員工並更新資料
                employee.IsDeleted = false;
                employee.Status = EntityStatus.Active;
                employee.FailedLoginAttempts = 0;
                employee.UpdatedAt = DateTime.Now;
                
                // 更新業務資料
                employee.Code = updateData.Code;
                employee.Account = updateData.Account;
                employee.FirstName = updateData.FirstName;
                employee.LastName = updateData.LastName;
                employee.RoleId = updateData.RoleId;
                employee.DepartmentId = updateData.DepartmentId;
                employee.EmployeePositionId = updateData.EmployeePositionId;

                await context.SaveChangesAsync();
                
                // 重新載入員工資料以包含相關資料
                var updatedEmployee = await context.Employees
                    .Include(e => e.Role)
                    .Include(e => e.Department)
                    .Include(e => e.EmployeePosition)
                    .FirstOrDefaultAsync(e => e.Id == employeeId);

                return ServiceResult<Employee>.Success(updatedEmployee!);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RestoreAndUpdateSoftDeletedEmployeeAsync), GetType(), _logger, new { 
                    Method = nameof(RestoreAndUpdateSoftDeletedEmployeeAsync),
                    ServiceType = GetType().Name,
                    EmployeeId = employeeId,
                    UpdateData = new { 
                        updateData.Account, 
                        updateData.Code, 
                        updateData.RoleId, 
                        updateData.DepartmentId, 
                        updateData.EmployeePositionId 
                    }
                });
                return ServiceResult<Employee>.Failure($"復原並更新員工失敗：{ex.Message}");
            }
        }

        #endregion
    }
}

