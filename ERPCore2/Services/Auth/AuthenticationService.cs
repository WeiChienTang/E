using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ERPCore2.Services
{
    /// <summary>
    /// 身份驗證服務實作
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<AuthenticationService>? _logger;

        /// <summary>
        /// 建構子
        /// </summary>
        public AuthenticationService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<AuthenticationService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// 登入驗證
        /// </summary>
        public async Task<ServiceResult<Employee>> LoginAsync(string username, string password)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Username == username && e.Status == EntityStatus.Active);

                if (employee == null)
                {
                    _logger?.LogWarning("Login attempt with invalid username: {Username}", username);
                    return ServiceResult<Employee>.Failure("使用者名稱或密碼錯誤");
                }

                // 檢查帳號是否被鎖定
                var lockResult = await IsUserLockedAsync(employee.Id);
                if (lockResult.IsSuccess && lockResult.Data)
                {
                    _logger?.LogWarning("Login attempt for locked user: {Username}", username);
                    return ServiceResult<Employee>.Failure("帳號已被鎖定，請聯絡管理員");
                }

                // 驗證密碼
                if (!VerifyPassword(password, employee.PasswordHash))
                {
                    _logger?.LogWarning("Login attempt with invalid password for user: {Username}", username);
                    return ServiceResult<Employee>.Failure("使用者名稱或密碼錯誤");
                }

                // 更新最後登入時間
                await UpdateLastLoginAsync(employee.Id);

                _logger?.LogInformation("User {Username} logged in successfully", username);
                return ServiceResult<Employee>.Success(employee);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(LoginAsync), GetType(), _logger);
                return ServiceResult<Employee>.Failure("登入過程發生錯誤");
            }
        }

        /// <summary>
        /// 登出
        /// </summary>
        public async Task<ServiceResult> LogoutAsync(int employeeId)
        {
            try
            {
                _logger?.LogInformation("User {EmployeeId} logged out", employeeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(LogoutAsync), GetType(), _logger);
                return ServiceResult.Failure("登出過程發生錯誤");
            }
        }

        /// <summary>
        /// 變更密碼
        /// </summary>
        public async Task<ServiceResult> ChangePasswordAsync(int employeeId, string currentPassword, string newPassword)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return ServiceResult.Failure("找不到指定的員工");
                }

                // 驗證當前密碼
                if (!VerifyPassword(currentPassword, employee.PasswordHash))
                {
                    return ServiceResult.Failure("當前密碼錯誤");
                }

                // 驗證新密碼強度
                var strengthResult = ValidatePasswordStrength(newPassword);
                if (!strengthResult.IsSuccess)
                {
                    return ServiceResult.Failure(strengthResult.ErrorMessage);
                }

                // 更新密碼
                employee.PasswordHash = HashPassword(newPassword);
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = $"Employee_{employeeId}";

                await context.SaveChangesAsync();

                _logger?.LogInformation("Password changed for employee {EmployeeId}", employeeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ChangePasswordAsync), GetType(), _logger);
                return ServiceResult.Failure("變更密碼過程發生錯誤");
            }
        }

        /// <summary>
        /// 重設密碼
        /// </summary>
        public async Task<ServiceResult> ResetPasswordAsync(int employeeId, string newPassword)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return ServiceResult.Failure("找不到指定的員工");
                }

                // 驗證新密碼強度
                var strengthResult = ValidatePasswordStrength(newPassword);
                if (!strengthResult.IsSuccess)
                {
                    return ServiceResult.Failure(strengthResult.ErrorMessage);
                }

                // 更新密碼
                employee.PasswordHash = HashPassword(newPassword);
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = "System";

                await context.SaveChangesAsync();

                _logger?.LogInformation("Password reset for employee {EmployeeId}", employeeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ResetPasswordAsync), GetType(), _logger);
                return ServiceResult.Failure("重設密碼過程發生錯誤");
            }
        }

        /// <summary>
        /// 檢查密碼強度
        /// </summary>
        public ServiceResult<bool> ValidatePasswordStrength(string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    return ServiceResult<bool>.Failure("密碼不能為空");
                }

                if (password.Length < 4)
                {
                    return ServiceResult<bool>.Failure("密碼長度不能少於4個字元");
                }

                if (password.Length > 128)
                {
                    return ServiceResult<bool>.Failure("密碼長度不能超過128個字元");
                }

                // 檢查是否只包含有效字元（數字、大寫字母、小寫字母、特殊字元）
                if (!Regex.IsMatch(password, @"^[a-zA-Z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+$"))
                {
                    return ServiceResult<bool>.Failure("密碼只能包含英文字母、數字和特殊字元");
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidatePasswordStrength), GetType(), _logger);
                return ServiceResult<bool>.Failure("密碼強度驗證發生錯誤");
            }
        }

        /// <summary>
        /// 驗證使用者名稱格式
        /// </summary>
        public ServiceResult<bool> ValidateUsername(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return ServiceResult<bool>.Failure("使用者名稱不能為空");
                }

                if (username.Length < 3)
                {
                    return ServiceResult<bool>.Failure("使用者名稱長度至少需要3個字元");
                }

                if (username.Length > 50)
                {
                    return ServiceResult<bool>.Failure("使用者名稱長度不能超過50個字元");
                }

                // 檢查是否只包含字母、數字和底線
                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                {
                    return ServiceResult<bool>.Failure("使用者名稱只能包含字母、數字和底線");
                }

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidateUsername), GetType(), _logger);
                return ServiceResult<bool>.Failure("使用者名稱格式驗證發生錯誤");
            }
        }

        /// <summary>
        /// 更新最後登入時間
        /// </summary>
        public async Task<ServiceResult> UpdateLastLoginAsync(int employeeId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return ServiceResult.Failure("找不到指定的員工");
                }

                employee.LastLoginAt = DateTime.UtcNow;
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = "System";

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateLastLoginAsync), GetType(), _logger);
                return ServiceResult.Failure("更新最後登入時間發生錯誤");
            }
        }

        /// <summary>
        /// 檢查使用者是否已鎖定
        /// </summary>
        public async Task<ServiceResult<bool>> IsUserLockedAsync(int employeeId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return ServiceResult<bool>.Failure("找不到指定的員工");
                }

                // 假設有 IsLocked 屬性或者根據狀態判斷
                bool isLocked = employee.Status == EntityStatus.Inactive;
                
                return ServiceResult<bool>.Success(isLocked);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsUserLockedAsync), GetType(), _logger);
                return ServiceResult<bool>.Failure("檢查帳號鎖定狀態發生錯誤");
            }
        }

        /// <summary>
        /// 鎖定使用者帳號
        /// </summary>
        public async Task<ServiceResult> LockUserAsync(int employeeId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return ServiceResult.Failure("找不到指定的員工");
                }

                employee.Status = EntityStatus.Inactive;
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = "System";

                await context.SaveChangesAsync();

                _logger?.LogInformation("User account locked for employee {EmployeeId}", employeeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(LockUserAsync), GetType(), _logger);
                return ServiceResult.Failure("鎖定帳號發生錯誤");
            }
        }

        /// <summary>
        /// 解鎖使用者帳號
        /// </summary>
        public async Task<ServiceResult> UnlockUserAsync(int employeeId)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                
                var employee = await context.Employees.FindAsync(employeeId);
                if (employee == null)
                {
                    return ServiceResult.Failure("找不到指定的員工");
                }

                employee.Status = EntityStatus.Active;
                employee.UpdatedAt = DateTime.UtcNow;
                employee.UpdatedBy = "System";

                await context.SaveChangesAsync();

                _logger?.LogInformation("User account unlocked for employee {EmployeeId}", employeeId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UnlockUserAsync), GetType(), _logger);
                return ServiceResult.Failure("解鎖帳號發生錯誤");
            }
        }

        #region 私有方法

        /// <summary>
        /// 雜湊密碼
        /// </summary>
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + "ERPCore2_Salt";
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// 驗證密碼
        /// </summary>
        private static bool VerifyPassword(string password, string? hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }

        #endregion
    }
}
