using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 身份驗證服務介面
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// 登入驗證
        /// </summary>
        /// <param name="account">帳號</param>
        /// <param name="password">密碼</param>
        /// <returns>驗證結果</returns>
        Task<ServiceResult<Employee>> LoginAsync(string account, string password);

        /// <summary>
        /// 登出
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> LogoutAsync(int employeeId);

        /// <summary>
        /// 變更密碼
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="currentPassword">目前密碼</param>
        /// <param name="newPassword">新密碼</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> ChangePasswordAsync(int employeeId, string currentPassword, string newPassword);

        /// <summary>
        /// 重設密碼
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="newPassword">新密碼</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> ResetPasswordAsync(int employeeId, string newPassword);

        /// <summary>
        /// 檢查密碼強度
        /// </summary>
        /// <param name="password">密碼</param>
        /// <returns>強度檢查結果</returns>
        ServiceResult<bool> ValidatePasswordStrength(string password);

        /// <summary>
        /// 驗證帳號格式
        /// </summary>
        /// <param name="account">帳號</param>
        /// <returns>驗證結果</returns>
        ServiceResult<bool> ValidateAccount(string account);

        /// <summary>
        /// 更新最後登入時間
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UpdateLastLoginAsync(int employeeId);

        /// <summary>
        /// 檢查使用者是否已鎖定
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>檢查結果</returns>
        Task<ServiceResult<bool>> IsUserLockedAsync(int employeeId);

        /// <summary>
        /// 鎖定使用者帳號
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> LockUserAsync(int employeeId);

        /// <summary>
        /// 解鎖使用者帳號
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> UnlockUserAsync(int employeeId);
    }
}

