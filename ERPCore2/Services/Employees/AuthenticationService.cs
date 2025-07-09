using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
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
        /// 完整建構子 (包含 ILogger)
        /// </summary>
        public AuthenticationService(IDbContextFactory<AppDbContext> contextFactory, ILogger<AuthenticationService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        /// <summary>
        /// 簡易建構子
        /// </summary>
        public AuthenticationService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// 登入驗證
        /// </summary>
        public async Task<ServiceResult<Employee>> LoginAsync(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    return ServiceResult<Employee>.Failure("使用者名稱和密碼不能為空");

                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .Include(e => e.Role)
                    .FirstOrDefaultAsync(e => e.Username == username && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult<Employee>.Failure("使用者名稱或密碼錯誤");

                if (employee.Status != EntityStatus.Active)
                    return ServiceResult<Employee>.Failure("帳號已停用");

                if (employee.IsLocked)
                    return ServiceResult<Employee>.Failure("帳號已被鎖定，請聯絡管理員");

                if (!VerifyPassword(password, employee.PasswordHash))
                    return ServiceResult<Employee>.Failure("使用者名稱或密碼錯誤");

                // 更新最後登入時間
                await UpdateLastLoginAsync(employee.Id);

                return ServiceResult<Employee>.Success(employee);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(LoginAsync), typeof(AuthenticationService), _logger);
                return ServiceResult<Employee>.Failure($"登入時發生錯誤，錯誤編號：{errorId}");
            }
        }

        /// <summary>
        /// 登出
        /// </summary>
        public async Task<ServiceResult> LogoutAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult.Failure("員工不存在");

                // 這裡可以清除 session 或其他登出邏輯
                // 目前只是一個佔位符，實際實作會根據認證機制而定

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(LogoutAsync), typeof(AuthenticationService), _logger);
                return ServiceResult.Failure($"登出時發生錯誤，錯誤編號：{errorId}");
            }
        }

        /// <summary>
        /// 變更密碼
        /// </summary>
        public async Task<ServiceResult> ChangePasswordAsync(int employeeId, string currentPassword, string newPassword)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult.Failure("員工不存在");

                if (!VerifyPassword(currentPassword, employee.PasswordHash))
                    return ServiceResult.Failure("當前密碼錯誤");

                var passwordValidation = ValidatePasswordStrength(newPassword);
                if (!passwordValidation.IsSuccess)
                    return ServiceResult.Failure(passwordValidation.ErrorMessage);

                employee.PasswordHash = HashPassword(newPassword);
                employee.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ChangePasswordAsync), typeof(AuthenticationService), _logger);
                return ServiceResult.Failure($"變更密碼時發生錯誤，錯誤編號：{errorId}");
            }
        }

        /// <summary>
        /// 重設密碼
        /// </summary>
        public async Task<ServiceResult> ResetPasswordAsync(int employeeId, string newPassword)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult.Failure("員工不存在");

                var passwordValidation = ValidatePasswordStrength(newPassword);
                if (!passwordValidation.IsSuccess)
                    return ServiceResult.Failure(passwordValidation.ErrorMessage);

                employee.PasswordHash = HashPassword(newPassword);
                employee.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ResetPasswordAsync), typeof(AuthenticationService), _logger);
                return ServiceResult.Failure($"重設密碼時發生錯誤，錯誤編號：{errorId}");
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
                    return ServiceResult<bool>.Failure("密碼不能為空");

                if (password.Length < 8)
                    return ServiceResult<bool>.Failure("密碼長度不能少於8個字元");

                if (password.Length > 128)
                    return ServiceResult<bool>.Failure("密碼長度不能超過128個字元");

                // 檢查是否包含大寫字母
                if (!Regex.IsMatch(password, @"[A-Z]"))
                    return ServiceResult<bool>.Failure("密碼必須包含至少一個大寫字母");

                // 檢查是否包含小寫字母
                if (!Regex.IsMatch(password, @"[a-z]"))
                    return ServiceResult<bool>.Failure("密碼必須包含至少一個小寫字母");

                // 檢查是否包含數字
                if (!Regex.IsMatch(password, @"[0-9]"))
                    return ServiceResult<bool>.Failure("密碼必須包含至少一個數字");

                // 檢查是否包含特殊字元
                if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
                    return ServiceResult<bool>.Failure("密碼必須包含至少一個特殊字元");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                var errorId = ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidatePasswordStrength), typeof(AuthenticationService), _logger);
                return ServiceResult<bool>.Failure($"驗證密碼強度時發生錯誤，錯誤編號：{errorId}");
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
                    return ServiceResult<bool>.Failure("使用者名稱不能為空");

                if (username.Length < 3)
                    return ServiceResult<bool>.Failure("使用者名稱長度不能少於3個字元");

                if (username.Length > 50)
                    return ServiceResult<bool>.Failure("使用者名稱長度不能超過50個字元");

                // 檢查是否只包含英文字母、數字和底線
                if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
                    return ServiceResult<bool>.Failure("使用者名稱只能包含英文字母、數字和底線");

                // 不能以數字開頭
                if (char.IsDigit(username[0]))
                    return ServiceResult<bool>.Failure("使用者名稱不能以數字開頭");

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                var errorId = ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(ValidateUsername), typeof(AuthenticationService), _logger);
                return ServiceResult<bool>.Failure($"驗證使用者名稱時發生錯誤，錯誤編號：{errorId}");
            }
        }

        /// <summary>
        /// 更新最後登入時間
        /// </summary>
        public async Task<ServiceResult> UpdateLastLoginAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult.Failure("員工不存在");

                employee.LastLoginAt = DateTime.UtcNow;
                employee.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateLastLoginAsync), typeof(AuthenticationService), _logger);
                return ServiceResult.Failure($"更新最後登入時間時發生錯誤，錯誤編號：{errorId}");
            }
        }

        /// <summary>
        /// 檢查使用者是否已鎖定
        /// </summary>
        public async Task<ServiceResult<bool>> IsUserLockedAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult<bool>.Failure("員工不存在");

                return ServiceResult<bool>.Success(employee.IsLocked);
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsUserLockedAsync), typeof(AuthenticationService), _logger);
                return ServiceResult<bool>.Failure($"檢查鎖定狀態時發生錯誤，錯誤編號：{errorId}");
            }
        }

        /// <summary>
        /// 鎖定使用者帳號
        /// </summary>
        public async Task<ServiceResult> LockUserAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult.Failure("員工不存在");

                employee.IsLocked = true;
                employee.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(LockUserAsync), typeof(AuthenticationService), _logger);
                return ServiceResult.Failure($"鎖定帳號時發生錯誤，錯誤編號：{errorId}");
            }
        }

        /// <summary>
        /// 解鎖使用者帳號
        /// </summary>
        public async Task<ServiceResult> UnlockUserAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var employee = await context.Employees
                    .FirstOrDefaultAsync(e => e.Id == employeeId && !e.IsDeleted);

                if (employee == null)
                    return ServiceResult.Failure("員工不存在");

                employee.IsLocked = false;
                employee.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                var errorId = await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UnlockUserAsync), typeof(AuthenticationService), _logger);
                return ServiceResult.Failure($"解鎖帳號時發生錯誤，錯誤編號：{errorId}");
            }
        }

        #region Private Methods

        /// <summary>
        /// 密碼雜湊
        /// </summary>
        private string HashPassword(string password)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var saltedPassword = password + "ERPCore2_Salt";
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
            catch (Exception ex)
            {
                var errorId = ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(HashPassword), typeof(AuthenticationService), _logger);
                throw new InvalidOperationException($"密碼雜湊時發生錯誤，錯誤編號：{errorId}", ex);
            }
        }

        /// <summary>
        /// 驗證密碼
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                var hashedPassword = HashPassword(password);
                return hashedPassword == hash;
            }
            catch (Exception ex)
            {
                var errorId = ErrorHandlingHelper.HandleServiceErrorSync(ex, nameof(VerifyPassword), typeof(AuthenticationService), _logger);
                throw new InvalidOperationException($"驗證密碼時發生錯誤，錯誤編號：{errorId}", ex);
            }
        }

        #endregion
    }
}

