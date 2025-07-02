using Microsoft.AspNetCore.Components;
using ERPCore2.Services;
using ERPCore2.Services.Notifications;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 錯誤處理 Helper - 提供 Razor 頁面統一的錯誤處理方法
    /// 與 GlobalExceptionHelper (中間件) 搭配使用，處理已捕獲的例外
    /// </summary>
    public static class ErrorHandlingHelper
    {
        /// <summary>
        /// 安全處理錯誤 - 適用於 Razor 頁面的事件處理器
        /// </summary>
        /// <param name="exception">捕獲的例外</param>
        /// <param name="methodName">發生錯誤的方法名稱</param>
        /// <param name="componentType">元件類型</param>
        /// <param name="errorLogService">錯誤記錄服務</param>
        /// <param name="notificationService">通知服務</param>
        /// <param name="showUserFriendlyMessage">是否顯示使用者友善訊息，預設 true</param>
        /// <param name="additionalData">額外的錯誤資料</param>
        /// <returns>錯誤 ID</returns>
        public static async Task<string> HandleErrorSafelyAsync(
            Exception exception,
            string methodName,
            Type componentType,
            IErrorLogService errorLogService,
            INotificationService notificationService,
            bool showUserFriendlyMessage = true,
            object? additionalData = null)
        {
            string errorId = string.Empty;
            
            try
            {
                // 組合額外資料
                var errorData = new
                {
                    ComponentType = componentType.Name,
                    MethodName = methodName,
                    ComponentFullName = componentType.FullName,
                    Timestamp = DateTime.UtcNow,
                    IsRazorComponent = true,
                    AdditionalData = additionalData
                };

                // 記錄錯誤到資料庫
                errorId = await errorLogService.LogErrorAsync(exception, errorData);

                // 顯示使用者友善的錯誤訊息
                if (showUserFriendlyMessage)
                {
                    var userMessage = GetUserFriendlyMessage(exception);
                    await notificationService.ShowErrorAsync(userMessage, "操作失敗");
                }
            }
            catch (Exception)
            {
                // 如果記錄錯誤時也失敗，至少要顯示基本錯誤訊息
                try
                {
                    await notificationService.ShowErrorAsync("系統發生錯誤，請稍後再試", "系統錯誤");
                }
                catch
                {
                    // 如果連通知都失敗，就無能為力了
                }
                
                // 生成備用錯誤 ID
                errorId = $"FALLBACK_{DateTime.UtcNow.Ticks}";
            }

            return errorId;
        }

        /// <summary>
        /// 處理 Service 結果的錯誤 - 適用於處理 ServiceResult 失敗的情況
        /// </summary>
        /// <param name="result">失敗的 ServiceResult</param>
        /// <param name="methodName">發生錯誤的方法名稱</param>
        /// <param name="notificationService">通知服務</param>
        /// <param name="customErrorMessage">自訂錯誤訊息</param>
        public static async Task HandleServiceErrorAsync(
            ServiceResult result,
            string methodName,
            INotificationService notificationService,
            string? customErrorMessage = null)
        {
            try
            {
                var errorMessage = customErrorMessage ?? result.ErrorMessage ?? "操作失敗";
                
                // 如果有驗證錯誤，組合訊息
                if (result.ValidationErrors?.Any() == true)
                {
                    errorMessage += "\n" + string.Join("\n", result.ValidationErrors);
                }

                await notificationService.ShowErrorAsync(errorMessage, "操作失敗");
            }
            catch
            {
                // 如果通知失敗，忽略錯誤
            }
        }

        /// <summary>
        /// 處理 Service 結果的錯誤 - 泛型版本
        /// </summary>
        public static async Task HandleServiceErrorAsync<T>(
            ServiceResult<T> result,
            string methodName,
            INotificationService notificationService,
            string? customErrorMessage = null)
        {
            try
            {
                var errorMessage = customErrorMessage ?? result.ErrorMessage ?? "操作失敗";
                
                if (result.ValidationErrors?.Any() == true)
                {
                    errorMessage += "\n" + string.Join("\n", result.ValidationErrors);
                }

                await notificationService.ShowErrorAsync(errorMessage, "操作失敗");
            }
            catch
            {
                // 如果通知失敗，忽略錯誤
            }
        }

        /// <summary>
        /// 簡化版錯誤處理 - 只需要基本服務
        /// </summary>
        /// <param name="exception">捕獲的例外</param>
        /// <param name="methodName">發生錯誤的方法名稱</param>
        /// <param name="notificationService">通知服務</param>
        /// <param name="customMessage">自訂錯誤訊息</param>
        public static async Task HandleErrorSimplyAsync(
            Exception exception,
            string methodName,
            INotificationService notificationService,
            string? customMessage = null)
        {
            try
            {
                var message = customMessage ?? GetUserFriendlyMessage(exception);
                await notificationService.ShowErrorAsync(message, "操作失敗");
            }
            catch
            {
                // 如果通知失敗，忽略錯誤
            }
        }

        /// <summary>
        /// 執行異步操作並處理錯誤 - 通用版本
        /// </summary>
        /// <typeparam name="T">返回值類型</typeparam>
        /// <param name="operation">要執行的異步操作</param>
        /// <param name="defaultValue">錯誤時的預設值</param>
        /// <param name="notificationService">通知服務</param>
        /// <param name="customMessage">自訂錯誤訊息</param>
        /// <returns>操作結果或預設值</returns>
        public static async Task<T> ExecuteWithErrorHandlingAsync<T>(
            Func<Task<T>> operation,
            T defaultValue,
            INotificationService notificationService,
            string? customMessage = null)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                await HandleErrorSimplyAsync(ex, "ExecuteOperation", notificationService, customMessage);
                return defaultValue;
            }
        }

        /// <summary>
        /// 將技術性錯誤訊息轉換為使用者友善的訊息
        /// </summary>
        /// <param name="exception">例外物件</param>
        /// <returns>使用者友善的錯誤訊息</returns>
        private static string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                TimeoutException => "系統回應逾時，請稍後再試",
                UnauthorizedAccessException => "權限不足，請聯繫管理員",
                ArgumentNullException => "必要資料不完整",
                ArgumentException => "輸入資料格式不正確",
                InvalidOperationException => "目前無法執行此操作，請檢查資料狀態",
                NotSupportedException => "此功能目前不支援",
                FileNotFoundException => "找不到指定的檔案",
                DirectoryNotFoundException => "找不到指定的目錄",
                OutOfMemoryException => "系統記憶體不足",
                StackOverflowException => "系統處理過載",
                _ when exception.Message.Contains("網路") || exception.Message.Contains("network") => "網路連線異常，請檢查網路狀態",
                _ when exception.Message.Contains("資料庫") || exception.Message.Contains("database") => "資料庫連線異常，請稍後再試",
                _ when exception.Message.Contains("驗證") || exception.Message.Contains("validation") => "資料驗證失敗，請檢查輸入內容",
                _ => "系統發生錯誤，請稍後再試或聯繫技術人員"
            };
        }
    }
}
