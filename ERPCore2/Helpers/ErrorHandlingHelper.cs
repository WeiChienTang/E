using Microsoft.Extensions.Logging;
using ERPCore2.Services;
using ERPCore2.Services.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 統一錯誤處理 Helper
    /// 提供三種核心功能：1.防止程式崩潰 2.記錄錯誤到資料庫 3.通知使用者
    /// </summary>
    public static class ErrorHandlingHelper
    {
        private static IServiceProvider? _serviceProvider;

        /// <summary>
        /// 初始化服務提供者（在 Program.cs 中呼叫）
        /// </summary>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Razor 頁面層錯誤處理 - 記錄錯誤 + 通知使用者
        /// 功能：1.防止程式崩潰 2.記錄到ErrorLog資料表 3.顯示使用者友善訊息
        /// </summary>
        /// <param name="exception">捕獲的例外</param>
        /// <param name="methodName">發生錯誤的方法名稱</param>
        /// <param name="componentType">元件類型</param>
        /// <param name="additionalData">額外的錯誤資料</param>
        /// <returns>錯誤 ID</returns>
        public static async Task<string> HandlePageErrorAsync(
            Exception exception,
            string methodName,
            Type componentType,
            object? additionalData = null)
        {
            string errorId = string.Empty;
            
            if (_serviceProvider == null)
            {
                // 如果服務提供者未初始化，回傳降級錯誤 ID
                return $"PAGE_NO_PROVIDER_{DateTime.UtcNow.Ticks}";
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var errorLogService = scope.ServiceProvider.GetRequiredService<IErrorLogService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // 記錄錯誤到資料庫
                errorId = await LogErrorToDatabase(exception, methodName, componentType.Name, componentType.FullName, true, errorLogService, additionalData);

                // 通知使用者
                var userMessage = GetUserFriendlyMessage(exception);
                await notificationService.ShowErrorAsync(userMessage, "操作失敗");
            }
            catch (Exception)
            {
                // 如果記錄錯誤失敗，至少要通知使用者
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.ShowErrorAsync("系統發生錯誤，請稍後再試", "系統錯誤");
                }
                catch
                {
                    // 如果連通知都失敗，無能為力
                }
                
                errorId = $"PAGE_FALLBACK_{DateTime.UtcNow.Ticks}";
            }

            return errorId;
        }

        /// <summary>
        /// Service 層錯誤處理 - 只記錄錯誤，不通知使用者
        /// 功能：1.防止程式崩潰 2.記錄到ErrorLog資料表 3.記錄到日誌
        /// </summary>
        /// <param name="exception">捕獲的例外</param>
        /// <param name="methodName">發生錯誤的方法名稱</param>
        /// <param name="serviceType">服務類型</param>
        /// <param name="logger">記錄器（可為 null）</param>
        /// <param name="additionalData">額外的錯誤資料</param>
        /// <returns>錯誤 ID</returns>
        public static async Task<string> HandleServiceErrorAsync(
            Exception exception,
            string methodName,
            Type serviceType,
            ILogger? logger = null,
            object? additionalData = null)
        {
            string errorId = string.Empty;
            
            if (_serviceProvider == null)
            {
                // 如果服務提供者未初始化，回傳降級錯誤 ID
                return $"SERVICE_NO_PROVIDER_{DateTime.UtcNow.Ticks}";
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var errorLogService = scope.ServiceProvider.GetRequiredService<IErrorLogService>();

                // 記錄錯誤到資料庫
                errorId = await LogErrorToDatabase(exception, methodName, serviceType.Name, serviceType.FullName, false, errorLogService, additionalData);

                // 記錄到應用程式日誌
                logger?.LogError(exception, "Service Error in {ServiceType}.{MethodName}", serviceType.Name, methodName);
            }
            catch (Exception)
            {
                errorId = $"SERVICE_FALLBACK_{DateTime.UtcNow.Ticks}";
            }

            return errorId;
        }

        /// <summary>
        /// Service 層同步方法錯誤處理 - 記錄錯誤但不阻塞執行
        /// 功能：1.防止程式崩潰 2.記錄到日誌 3.嘗試記錄到資料庫（非阻塞）
        /// </summary>
        /// <param name="exception">捕獲的例外</param>
        /// <param name="methodName">發生錯誤的方法名稱</param>
        /// <param name="serviceType">服務類型</param>
        /// <param name="logger">記錄器（可為 null）</param>
        /// <param name="additionalData">額外的錯誤資料</param>
        /// <returns>錯誤 ID（可能為空）</returns>
        public static string HandleServiceErrorSync(
            Exception exception,
            string methodName,
            Type serviceType,
            ILogger? logger = null,
            object? additionalData = null)
        {
            string errorId = string.Empty;
            
            try
            {
                // 先記錄到應用程式日誌（同步操作）
                logger?.LogError(exception, "Sync Service Error in {ServiceType}.{MethodName}", serviceType.Name, methodName);
                
                // 嘗試非阻塞地記錄到資料庫
                if (_serviceProvider != null)
                {
                    // 使用 Task.Run 避免阻塞同步方法
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            using var scope = _serviceProvider.CreateScope();
                            var errorLogService = scope.ServiceProvider.GetRequiredService<IErrorLogService>();
                            
                            var errorData = new
                            {
                                TypeName = serviceType.Name,
                                MethodName = methodName,
                                FullTypeName = serviceType.FullName,
                                Timestamp = DateTime.UtcNow,
                                IsPageLayer = false,
                                IsServiceLayer = true,
                                IsSyncMethod = true,
                                AdditionalData = additionalData
                            };
                            
                            await errorLogService.LogErrorAsync(exception, errorData);
                        }
                        catch
                        {
                            // 如果記錄到資料庫失敗，忽略錯誤（已經記錄到日誌）
                        }
                    });
                    
                    errorId = $"SYNC_SERVICE_{DateTime.UtcNow.Ticks}";
                }
                else
                {
                    errorId = $"SYNC_SERVICE_NO_PROVIDER_{DateTime.UtcNow.Ticks}";
                }
            }
            catch (Exception)
            {
                // 如果連日誌記錄都失敗，至少產生一個錯誤 ID
                errorId = $"SYNC_SERVICE_FALLBACK_{DateTime.UtcNow.Ticks}";
            }
            
            return errorId;
        }


        #region 私有輔助方法

        /// <summary>
        /// 記錄錯誤到資料庫的統一方法
        /// </summary>
        private static async Task<string> LogErrorToDatabase(
            Exception exception,
            string methodName,
            string typeName,
            string? fullTypeName,
            bool isPageLayer,
            IErrorLogService errorLogService,
            object? additionalData = null)
        {
            var errorData = new
            {
                TypeName = typeName,
                MethodName = methodName,
                FullTypeName = fullTypeName,
                Timestamp = DateTime.UtcNow,
                IsPageLayer = isPageLayer,
                IsServiceLayer = !isPageLayer,
                AdditionalData = additionalData
            };

            return await errorLogService.LogErrorAsync(exception, errorData);
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

        #endregion
    }
}
