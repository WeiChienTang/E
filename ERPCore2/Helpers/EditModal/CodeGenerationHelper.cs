using System;
using System.Threading.Tasks;
using ERPCore2.Services;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 編號生成輔助類別 - 提供通用的實體編號生成功能
    /// </summary>
    public static class CodeGenerationHelper
    {
        /// <summary>
        /// 生成實體編號（含重複檢查）
        /// </summary>
        /// <typeparam name="TService">服務類型</typeparam>
        /// <param name="service">服務實例</param>
        /// <param name="prefix">編號前綴</param>
        /// <param name="codeExistsChecker">編號存在檢查函數</param>
        /// <param name="excludeId">排除的 ID（用於編輯模式）</param>
        /// <returns>生成的唯一編號</returns>
        public static async Task<string> GenerateEntityCodeAsync<TService>(
            TService service,
            string prefix,
            Func<TService, string, int?, Task<bool>> codeExistsChecker,
            int? excludeId = null)
        {
            try
            {
                // 生成基礎編號：前綴 + 時間戳
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var baseCode = $"{prefix}{timestamp}";
                
                // 檢查是否重複
                var isExists = await codeExistsChecker(service, baseCode, excludeId);
                if (isExists)
                {
                    // 如果重複，加上隨機數
                    var random = new Random().Next(100, 999);
                    baseCode = $"{prefix}{timestamp}{random}";
                    
                    // 再次檢查加了隨機數的編號是否重複
                    var isStillExists = await codeExistsChecker(service, baseCode, excludeId);
                    if (isStillExists)
                    {
                        // 如果還是重複，使用毫秒級時間戳
                        var preciseName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        baseCode = $"{prefix}{preciseName}";
                    }
                }
                
                return baseCode;
            }
            catch
            {
                // 如果生成失敗，返回預設格式（使用毫秒級時間戳確保唯一性）
                return $"{prefix}{DateTime.Now:yyyyMMddHHmmssfff}";
            }
        }

        /// <summary>
        /// 生成實體編號（含重複檢查，支援 ServiceResult 返回類型）
        /// </summary>
        /// <typeparam name="TService">服務類型</typeparam>
        /// <param name="service">服務實例</param>
        /// <param name="prefix">編號前綴</param>
        /// <param name="codeExistsChecker">編號存在檢查函數（返回 ServiceResult&lt;bool&gt;）</param>
        /// <param name="excludeId">排除的 ID（用於編輯模式）</param>
        /// <returns>生成的唯一編號</returns>
        public static async Task<string> GenerateEntityCodeWithServiceResultAsync<TService>(
            TService service,
            string prefix,
            Func<TService, string, int?, Task<ServiceResult<bool>>> codeExistsChecker,
            int? excludeId = null)
        {
            try
            {
                // 生成基礎編號：前綴 + 時間戳
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var baseCode = $"{prefix}{timestamp}";
                
                // 檢查是否重複
                var existsResult = await codeExistsChecker(service, baseCode, excludeId);
                var isExists = existsResult.IsSuccess && existsResult.Data;
                
                if (isExists)
                {
                    // 如果重複，加上隨機數
                    var random = new Random().Next(100, 999);
                    baseCode = $"{prefix}{timestamp}{random}";
                    
                    // 再次檢查加了隨機數的編號是否重複
                    var stillExistsResult = await codeExistsChecker(service, baseCode, excludeId);
                    var isStillExists = stillExistsResult.IsSuccess && stillExistsResult.Data;
                    
                    if (isStillExists)
                    {
                        // 如果還是重複，使用毫秒級時間戳
                        var preciseName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        baseCode = $"{prefix}{preciseName}";
                    }
                }
                
                return baseCode;
            }
            catch
            {
                // 如果生成失敗，返回預設格式（使用毫秒級時間戳確保唯一性）
                return $"{prefix}{DateTime.Now:yyyyMMddHHmmssfff}";
            }
        }

        /// <summary>
        /// 生成實體編號（不含重複檢查的簡化版本）
        /// </summary>
        /// <param name="prefix">編號前綴</param>
        /// <param name="usePreciseTimestamp">是否使用精確時間戳（包含毫秒）</param>
        /// <returns>生成的編號</returns>
        public static string GenerateSimpleEntityCode(string prefix, bool usePreciseTimestamp = false)
        {
            try
            {
                var timestampFormat = usePreciseTimestamp ? "yyyyMMddHHmmssfff" : "yyyyMMddHHmmss";
                var timestamp = DateTime.Now.ToString(timestampFormat);
                return $"{prefix}{timestamp}";
            }
            catch
            {
                // 如果生成失敗，使用精確時間戳作為後備
                return $"{prefix}{DateTime.Now:yyyyMMddHHmmssfff}";
            }
        }

        /// <summary>
        /// 驗證編號格式是否正確
        /// </summary>
        /// <param name="code">要驗證的編號</param>
        /// <param name="expectedPrefix">期望的前綴</param>
        /// <returns>是否符合格式</returns>
        public static bool IsValidEntityCode(string code, string expectedPrefix)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(expectedPrefix))
                return false;

            // 檢查是否以期望的前綴開始
            if (!code.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            // 檢查總長度是否合理（前綴 + 14位時間戳 或 前綴 + 14位時間戳 + 3位隨機數）
            var expectedMinLength = expectedPrefix.Length + 14; // YYYYMMDDHHMMSS
            var expectedMaxLength = expectedPrefix.Length + 17; // YYYYMMDDHHMMSS + 3位隨機數

            return code.Length >= expectedMinLength && code.Length <= expectedMaxLength;
        }

        /// <summary>
        /// 從編號中提取時間戳部分
        /// </summary>
        /// <param name="code">實體編號</param>
        /// <param name="prefix">前綴</param>
        /// <returns>時間戳字串，如果無法解析則返回 null</returns>
        public static string? ExtractTimestampFromCode(string code, string prefix)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(prefix))
                return null;

            if (!code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var timestampPart = code.Substring(prefix.Length);
            
            // 提取前14位作為時間戳（YYYYMMDDHHMMSS）
            if (timestampPart.Length >= 14)
            {
                return timestampPart.Substring(0, 14);
            }

            return null;
        }

        /// <summary>
        /// 嘗試從編號中解析生成時間
        /// </summary>
        /// <param name="code">實體編號</param>
        /// <param name="prefix">前綴</param>
        /// <returns>生成時間，如果無法解析則返回 null</returns>
        public static DateTime? ParseGenerationTimeFromCode(string code, string prefix)
        {
            var timestamp = ExtractTimestampFromCode(code, prefix);
            if (string.IsNullOrWhiteSpace(timestamp))
                return null;

            if (DateTime.TryParseExact(timestamp, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dateTime))
            {
                return dateTime;
            }

            return null;
        }
    }
}
