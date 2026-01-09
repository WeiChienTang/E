using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 通知服務介面 - 提供統一的訊息通知功能
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// 顯示成功訊息
        /// </summary>
        Task ShowSuccessAsync(string message, string? title = null);
        
        /// <summary>
        /// 顯示錯誤訊息
        /// </summary>
        Task ShowErrorAsync(string message, string? title = null);
        
        /// <summary>
        /// 顯示警告訊息
        /// </summary>
        Task ShowWarningAsync(string message, string? title = null);
        
        /// <summary>
        /// 顯示資訊訊息
        /// </summary>
        Task ShowInfoAsync(string message, string? title = null);
          /// <summary>
        /// 顯示確認對話框
        /// </summary>
        Task<bool> ShowConfirmAsync(string message, string? title = null);
        
        /// <summary>
        /// 驗證必填欄位並顯示警告訊息
        /// </summary>
        Task<bool> ValidateRequiredFieldsAsync();
        
        /// <summary>
        /// 顯示服務結果訊息
        /// </summary>
        Task ShowServiceResultAsync(ServiceResult result, string? successMessage = null);
        
        /// <summary>
        /// 顯示服務結果訊息（泛型版本）
        /// </summary>
        Task ShowServiceResultAsync<T>(ServiceResult<T> result, string? successMessage = null);
        
        /// <summary>
        /// 清除所有通知訊息
        /// </summary>
        Task ClearAllNotificationsAsync();
        
        /// <summary>
        /// 設定最大同時顯示的通知數量
        /// </summary>
        Task SetMaxNotificationsAsync(int maxCount);
        
        /// <summary>
        /// 複製文字到剪貼簿
        /// </summary>
        /// <param name="text">要複製的文字</param>
        /// <param name="showSuccessMessage">是否顯示成功訊息</param>
        /// <returns>是否成功</returns>
        Task<bool> CopyToClipboardAsync(string text, bool showSuccessMessage = true);
    }
}

