using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ERPCore2.Services
{
    /// <summary>
    /// 錯誤記錄服務實作
    /// </summary>
    public class ErrorLogService : GenericManagementService<ErrorLog>, IErrorLogService
    {
        public ErrorLogService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory) { }

        /// <summary>
        /// 記錄錯誤並返回錯誤ID
        /// </summary>
        public async Task<string> LogErrorAsync(Exception exception, object? additionalData = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var errorId = GenerateErrorId();
                
                var errorLog = new ErrorLog
                {
                    ErrorId = errorId,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    Source = DetermineErrorSource(exception),
                    Level = DetermineErrorLevel(exception),
                    OccurredAt = DateTime.UtcNow,
                    ExceptionType = exception.GetType().FullName,
                    InnerException = exception.InnerException?.ToString(),
                    AdditionalData = additionalData != null ? JsonSerializer.Serialize(additionalData) : null,
                    Category = CategorizeError(exception),
                    Module = ExtractModule(additionalData),
                    UserId = ExtractUserId(additionalData),
                    UserAgent = ExtractUserAgent(additionalData),
                    RequestPath = ExtractRequestPath(additionalData),
                    IsResolved = false,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.ErrorLogs.AddAsync(errorLog);
                await context.SaveChangesAsync();
                
                return errorId;
            }
            catch (Exception logException)
            {
                // 如果記錄錯誤時也發生錯誤，使用檔案記錄作為備援
                var fallbackErrorId = Guid.NewGuid().ToString("N")[..8].ToUpper();
                await LogToFileAsFallback(exception, logException, fallbackErrorId);
                return fallbackErrorId;
            }
        }

        /// <summary>
        /// 根據錯誤ID取得錯誤記錄
        /// </summary>
        public async Task<ErrorLog?> GetByErrorIdAsync(string errorId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(errorId))
                {
                    throw new ArgumentException("錯誤ID不能為空", nameof(errorId));
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.ErrorLogs
                    .Where(x => x.ErrorId == errorId && !x.IsDeleted)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // Service 層的錯誤處理：記錄並重新拋出
                // 注意：這裡不能使用自己的 LogErrorAsync，會造成循環
                await LogToFileAsFallback(ex, null, $"SERVICE_ERROR_{DateTime.UtcNow.Ticks}");
                throw; // 重新拋出讓上層處理
            }
        }

        /// <summary>
        /// 根據錯誤等級取得錯誤記錄
        /// </summary>
        public async Task<List<ErrorLog>> GetByLevelAsync(ErrorLevel level)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ErrorLogs
                .Where(x => x.Level == level && !x.IsDeleted)
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync();
        }

        /// <summary>
        /// 根據錯誤來源取得錯誤記錄
        /// </summary>
        public async Task<List<ErrorLog>> GetBySourceAsync(ErrorSource source)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ErrorLogs
                .Where(x => x.Source == source && !x.IsDeleted)
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync();
        }

        /// <summary>
        /// 根據時間範圍取得錯誤記錄
        /// </summary>
        public async Task<List<ErrorLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ErrorLogs
                .Where(x => x.OccurredAt >= startDate && x.OccurredAt <= endDate && !x.IsDeleted)
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync();
        }

        /// <summary>
        /// 取得未解決的錯誤記錄
        /// </summary>
        public async Task<List<ErrorLog>> GetUnresolvedAsync()
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ErrorLogs
                .Where(x => !x.IsResolved && !x.IsDeleted)
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync();
        }

        /// <summary>
        /// 標記錯誤為已解決
        /// </summary>
        public async Task<ServiceResult> MarkAsResolvedAsync(string errorId, string resolvedBy, string resolution)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var errorLog = await GetByErrorIdAsync(errorId);
            if (errorLog == null)
            {
                return ServiceResult.Failure("找不到指定的錯誤記錄");
            }

            errorLog.IsResolved = true;
            errorLog.ResolvedBy = resolvedBy;
            errorLog.ResolvedAt = DateTime.UtcNow;
            errorLog.Resolution = resolution;
            errorLog.UpdatedAt = DateTime.UtcNow;

            context.ErrorLogs.Update(errorLog);
            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        /// <summary>
        /// 批次標記錯誤為已解決
        /// </summary>
        public async Task<ServiceResult> MarkBatchAsResolvedAsync(List<string> errorIds, string resolvedBy, string resolution)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var errorLogs = await context.ErrorLogs
                .Where(x => errorIds.Contains(x.ErrorId) && !x.IsDeleted)
                .ToListAsync();

            if (!errorLogs.Any())
            {
                return ServiceResult.Failure("找不到任何指定的錯誤記錄");
            }

            foreach (var errorLog in errorLogs)
            {
                errorLog.IsResolved = true;
                errorLog.ResolvedBy = resolvedBy;
                errorLog.ResolvedAt = DateTime.UtcNow;
                errorLog.Resolution = resolution;
                errorLog.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return ServiceResult.Success();
        }

        /// <summary>
        /// 清理舊的錯誤記錄
        /// </summary>
        public async Task<int> CleanupOldErrorsAsync(int daysToKeep = 30)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            
            var oldErrors = await context.ErrorLogs
                .Where(x => x.OccurredAt < cutoffDate && x.IsResolved)
                .ToListAsync();

            if (oldErrors.Any())
            {
                context.ErrorLogs.RemoveRange(oldErrors);
                await context.SaveChangesAsync();
            }

            return oldErrors.Count;
        }

        #region 私有方法

        /// <summary>
        /// 判斷錯誤來源
        /// </summary>
        private ErrorSource DetermineErrorSource(Exception exception)
        {
            var stackTrace = exception.StackTrace ?? "";
            var exceptionType = exception.GetType().Name;

            return exceptionType switch
            {
                "SqlException" => ErrorSource.Database,
                "DbUpdateException" => ErrorSource.Database,
                "UnauthorizedAccessException" => ErrorSource.Security,
                "SecurityException" => ErrorSource.Security,
                _ when stackTrace.Contains("razor") => ErrorSource.UserInterface,
                _ when stackTrace.Contains("Controller") => ErrorSource.API,
                _ when stackTrace.Contains("Service") => ErrorSource.BusinessLogic,
                _ => ErrorSource.System
            };
        }

        /// <summary>
        /// 判斷錯誤等級
        /// </summary>
        private ErrorLevel DetermineErrorLevel(Exception exception)
        {
            return exception switch
            {
                OutOfMemoryException => ErrorLevel.Critical,
                StackOverflowException => ErrorLevel.Critical,
                AccessViolationException => ErrorLevel.Critical,
                UnauthorizedAccessException => ErrorLevel.Error,
                ArgumentNullException => ErrorLevel.Error,
                ArgumentException => ErrorLevel.Warning,
                _ => ErrorLevel.Error
            };
        }

        /// <summary>
        /// 錯誤分類
        /// </summary>
        private string CategorizeError(Exception exception)
        {
            return exception.GetType().Name switch
            {
                "SqlException" => "Database",
                "DbUpdateException" => "Database",
                "ValidationException" => "Validation",
                "UnauthorizedAccessException" => "Security",
                "TimeoutException" => "Performance",
                "OutOfMemoryException" => "Resource",
                _ => "General"
            };
        }

        /// <summary>
        /// 從額外資料中提取模組資訊
        /// </summary>
        private string? ExtractModule(object? additionalData)
        {
            if (additionalData == null) return null;
            
            var json = JsonSerializer.Serialize(additionalData);
            if (json.Contains("Page")) return "UI";
            if (json.Contains("Service")) return "Service";
            if (json.Contains("Controller")) return "API";
            
            return null;
        }

        /// <summary>
        /// 從額外資料中提取使用者ID
        /// </summary>
        private string? ExtractUserId(object? additionalData)
        {
            // 實際實作時可以從 HTTP Context 或額外資料中提取
            return null;
        }

        /// <summary>
        /// 從額外資料中提取使用者代理
        /// </summary>
        private string? ExtractUserAgent(object? additionalData)
        {
            // 實際實作時可以從 HTTP Context 或額外資料中提取
            return null;
        }

        /// <summary>
        /// 從額外資料中提取請求路徑
        /// </summary>
        private string? ExtractRequestPath(object? additionalData)
        {
            // 實際實作時可以從額外資料中提取
            if (additionalData == null) return null;
            
            var json = JsonSerializer.Serialize(additionalData);
            if (json.Contains("PageUrl")) 
            {
                // 從 JSON 中提取 PageUrl
                try
                {
                    using var document = JsonDocument.Parse(json);
                    if (document.RootElement.TryGetProperty("PageUrl", out var pageUrl))
                    {
                        return pageUrl.GetString();
                    }
                }
                catch
                {
                    // 忽略解析錯誤
                }
            }
            
            return null;
        }

        #endregion

        #region 覆寫基底類別抽象方法

        /// <summary>
        /// 搜尋錯誤記錄
        /// </summary>
        public override async Task<List<ErrorLog>> SearchAsync(string searchTerm)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            return await context.ErrorLogs
                .Where(x => !x.IsDeleted && (
                    x.Message.Contains(searchTerm) ||
                    x.ErrorId.Contains(searchTerm) ||
                    x.Category.Contains(searchTerm) ||
                    (x.Module != null && x.Module.Contains(searchTerm)) ||
                    (x.UserId != null && x.UserId.Contains(searchTerm))
                ))
                .OrderByDescending(x => x.OccurredAt)
                .ToListAsync();
        }

        /// <summary>
        /// 驗證錯誤記錄實體
        /// </summary>
        public override async Task<ServiceResult> ValidateAsync(ErrorLog entity)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var errors = new List<string>();

            // 檢查必填欄位
            if (string.IsNullOrWhiteSpace(entity.ErrorId))
                errors.Add("錯誤ID不能為空");

            if (string.IsNullOrWhiteSpace(entity.Message))
                errors.Add("錯誤訊息不能為空");

            if (string.IsNullOrWhiteSpace(entity.Category))
                errors.Add("錯誤分類不能為空");

            // 檢查錯誤ID是否重複（排除自己）
            if (!string.IsNullOrWhiteSpace(entity.ErrorId))
            {
                var exists = await context.ErrorLogs.AnyAsync(x => 
                    x.ErrorId == entity.ErrorId && 
                    x.Id != entity.Id && 
                    !x.IsDeleted);
                
                if (exists)
                    errors.Add("錯誤ID已存在");
            }

            // 檢查日期邏輯
            if (entity.ResolvedAt.HasValue && entity.ResolvedAt < entity.OccurredAt)
                errors.Add("解決時間不能早於發生時間");

            if (errors.Any())
                return ServiceResult.Failure(string.Join("; ", errors));

            return ServiceResult.Success();
        }

        /// <summary>
        /// 生成可讀性較高的錯誤ID
        /// </summary>
        private string GenerateErrorId()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = Guid.NewGuid().ToString("N")[..4].ToUpper();
            return $"ERR{timestamp}{random}";
        }

        /// <summary>
        /// 記錄錯誤到檔案作為備援
        /// </summary>
        private async Task LogToFileAsFallback(Exception originalException, Exception? logException, string errorId)
        {
            try
            {
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "FallbackErrors");
                Directory.CreateDirectory(logPath);
                
                var fileName = $"Error_{DateTime.UtcNow:yyyyMMdd}.log";
                var filePath = Path.Combine(logPath, fileName);
                
                var logEntry = $"""
                    [{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}] ERROR_ID: {errorId}
                    ORIGINAL_EXCEPTION: {originalException}
                    LOG_EXCEPTION: {logException}
                    ==========================================
                    
                    """;
                
                await File.AppendAllTextAsync(filePath, logEntry);
            }
            catch
            {
                // 如果連檔案也無法寫入，只能放棄記錄
            }
        }

        #endregion
    }
}

