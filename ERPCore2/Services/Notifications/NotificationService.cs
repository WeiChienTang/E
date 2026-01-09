using Microsoft.JSInterop;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 通知服務實作 - 使用 Toast 通知替代 alert
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IJSRuntime _jsRuntime;

        public NotificationService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// 顯示成功訊息
        /// </summary>
        public async Task ShowSuccessAsync(string message, string? title = null)
        {
            await _jsRuntime.InvokeVoidAsync("showToast", "success", message, title ?? "成功");
        }

        /// <summary>
        /// 顯示錯誤訊息
        /// </summary>
        public async Task ShowErrorAsync(string message, string? title = null)
        {
            await _jsRuntime.InvokeVoidAsync("showToast", "error", message, title ?? "錯誤");
        }

        /// <summary>
        /// 顯示警告訊息
        /// </summary>
        public async Task ShowWarningAsync(string message, string? title = null)
        {
            await _jsRuntime.InvokeVoidAsync("showToast", "warning", message, title ?? "警告");
        }

        /// <summary>
        /// 顯示資訊訊息
        /// </summary>
        public async Task ShowInfoAsync(string message, string? title = null)
        {
            await _jsRuntime.InvokeVoidAsync("showToast", "info", message, title ?? "資訊");
        }        /// <summary>
        /// 顯示確認對話框
        /// </summary>
        public async Task<bool> ShowConfirmAsync(string message, string? title = null)
        {
            return await _jsRuntime.InvokeAsync<bool>("confirm", message);
        }

        /// <summary>
        /// 驗證必填欄位並顯示警告訊息
        /// </summary>
        public async Task<bool> ValidateRequiredFieldsAsync()
        {
            var result = await _jsRuntime.InvokeAsync<string>("validateRequiredFields");
            if (!string.IsNullOrEmpty(result))
            {
                await ShowErrorAsync(result, "請填寫必要資訊");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 顯示服務結果訊息
        /// </summary>
        public async Task ShowServiceResultAsync(ServiceResult result, string? successMessage = null)
        {
            if (result.IsSuccess)
            {
                await ShowSuccessAsync(successMessage ?? "操作成功");
            }
            else
            {
                var errorMessage = !string.IsNullOrEmpty(result.ErrorMessage) 
                    ? result.ErrorMessage 
                    : "操作失敗";
                    
                if (result.ValidationErrors?.Any() == true)
                {
                    errorMessage += "\n" + string.Join("\n", result.ValidationErrors);
                }
                
                await ShowErrorAsync(errorMessage);
            }
        }

        /// <summary>
        /// 顯示服務結果訊息（泛型版本）
        /// </summary>
        public async Task ShowServiceResultAsync<T>(ServiceResult<T> result, string? successMessage = null)
        {
            if (result.IsSuccess)
            {
                await ShowSuccessAsync(successMessage ?? "操作成功");
            }
            else
            {
                var errorMessage = !string.IsNullOrEmpty(result.ErrorMessage) 
                    ? result.ErrorMessage 
                    : "操作失敗";
                    
                if (result.ValidationErrors?.Any() == true)
                {
                    errorMessage += "\n" + string.Join("\n", result.ValidationErrors);
                }
                
                await ShowErrorAsync(errorMessage);
            }
        }

        /// <summary>
        /// 清除所有通知訊息
        /// </summary>
        public async Task ClearAllNotificationsAsync()
        {
            await _jsRuntime.InvokeVoidAsync("clearAllToasts");
        }

        /// <summary>
        /// 設定最大同時顯示的通知數量
        /// </summary>
        public async Task SetMaxNotificationsAsync(int maxCount)
        {
            await _jsRuntime.InvokeVoidAsync("setMaxToasts", maxCount);
        }

        /// <summary>
        /// 複製文字到剪貼簿
        /// </summary>
        /// <param name="text">要複製的文字</param>
        /// <param name="showSuccessMessage">是否顯示成功訊息</param>
        /// <returns>是否成功</returns>
        public async Task<bool> CopyToClipboardAsync(string text, bool showSuccessMessage = true)
        {
            try
            {
                var success = await _jsRuntime.InvokeAsync<bool>("copyToClipboard", text);
                
                if (success && showSuccessMessage)
                {
                    await ShowSuccessAsync("已複製到剪貼簿");
                }
                else if (!success)
                {
                    await ShowErrorAsync("複製失敗，請手動複製");
                }
                
                return success;
            }
            catch (Exception)
            {
                await ShowErrorAsync("複製失敗，瀏覽器可能不支援此功能");
                return false;
            }
        }
    }
}

