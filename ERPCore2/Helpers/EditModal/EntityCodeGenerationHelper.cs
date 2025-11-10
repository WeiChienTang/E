using ERPCore2.Models;
using ERPCore2.Services;
using System.Reflection;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// 實體代碼生成輔助類別
/// 統一處理所有實體的代碼生成邏輯，減少重複代碼
/// </summary>
public static class EntityCodeGenerationHelper
{
    /// <summary>
    /// 為指定實體生成代碼（使用約定優於配置）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TService">服務類型</typeparam>
    /// <param name="service">服務實例</param>
    /// <param name="prefix">代碼前綴</param>
    /// <param name="excludeId">排除的實體ID（用於更新時檢查重複）</param>
    /// <returns>生成的代碼</returns>
    /// <remarks>
    /// 此方法使用約定優於配置的原則，自動尋找服務中的 IsXxxCodeExistsAsync 方法
    /// 例如：Customer 實體會尋找 IsCustomerCodeExistsAsync 方法
    /// </remarks>
    public static async Task<string> GenerateForEntity<TEntity, TService>(
        TService service,
        string prefix,
        int? excludeId = null)
        where TService : class
    {
        // 使用約定優於配置：自動找到 IsXxxCodeExistsAsync 方法
        var entityName = typeof(TEntity).Name;
        var methodName = $"Is{entityName}CodeExistsAsync";

        var method = typeof(TService).GetMethod(methodName);
        if (method == null)
        {
            throw new InvalidOperationException(
                $"找不到方法 {methodName} 在服務 {typeof(TService).Name} 中。" +
                $"請確保服務實作了 {methodName}(string code, int? excludeId) 方法。");
        }

        return await CodeGenerationHelper.GenerateEntityCodeAsync(
            service,
            prefix,
            (svc, code, excludeIdParam) =>
            {
                var result = method.Invoke(svc, new object?[] { code, excludeIdParam });
                if (result is Task<bool> task)
                {
                    return task;
                }
                throw new InvalidOperationException(
                    $"方法 {methodName} 的返回類型不正確。" +
                    $"預期: Task<bool>，實際: {result?.GetType().Name ?? "null"}");
            },
            excludeId
        );
    }

    /// <summary>
    /// 為指定實體生成代碼（使用自訂檢查函式）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TService">服務類型</typeparam>
    /// <param name="service">服務實例</param>
    /// <param name="prefix">代碼前綴</param>
    /// <param name="codeExistsChecker">自訂的代碼檢查函式</param>
    /// <param name="excludeId">排除的實體ID（用於更新時檢查重複）</param>
    /// <returns>生成的代碼</returns>
    public static async Task<string> GenerateForEntityWithCustomChecker<TEntity, TService>(
        TService service,
        string prefix,
        Func<TService, string, int?, Task<bool>> codeExistsChecker,
        int? excludeId = null)
        where TService : class
    {
        return await CodeGenerationHelper.GenerateEntityCodeAsync(
            service,
            prefix,
            codeExistsChecker,
            excludeId
        );
    }

    /// <summary>
    /// 為指定實體生成簡單代碼（使用時間戳記，不檢查重複）
    /// </summary>
    /// <param name="prefix">代碼前綴</param>
    /// <param name="usePreciseTimestamp">是否使用精確時間戳記（含毫秒）</param>
    /// <returns>生成的代碼</returns>
    /// <remarks>
    /// 適用於不需要檢查重複的場景，例如：訂單號、單據號等
    /// </remarks>
    public static string GenerateSimpleCode(string prefix, bool usePreciseTimestamp = false)
    {
        return CodeGenerationHelper.GenerateSimpleEntityCode(prefix, usePreciseTimestamp);
    }

    /// <summary>
    /// 批次為多個實體生成代碼
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TService">服務類型</typeparam>
    /// <param name="service">服務實例</param>
    /// <param name="prefix">代碼前綴</param>
    /// <param name="count">需要生成的數量</param>
    /// <returns>生成的代碼列表</returns>
    public static async Task<List<string>> GenerateBatchCodes<TEntity, TService>(
        TService service,
        string prefix,
        int count)
        where TService : class
    {
        var codes = new List<string>();

        for (int i = 0; i < count; i++)
        {
            var code = await GenerateForEntity<TEntity, TService>(service, prefix);
            codes.Add(code);

            // 短暫延遲以確保時間戳記不同
            if (i < count - 1)
            {
                await Task.Delay(10);
            }
        }

        return codes;
    }

    /// <summary>
    /// 驗證代碼格式是否符合規範
    /// </summary>
    /// <param name="code">要驗證的代碼</param>
    /// <param name="prefix">預期的前綴</param>
    /// <param name="minLength">最小長度（可選）</param>
    /// <param name="maxLength">最大長度（可選）</param>
    /// <returns>驗證結果（成功/失敗訊息）</returns>
    public static (bool IsValid, string? ErrorMessage) ValidateCode(
        string code,
        string prefix,
        int? minLength = null,
        int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return (false, "代碼不可為空");
        }

        if (!code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return (false, $"代碼必須以 {prefix} 開頭");
        }

        if (minLength.HasValue && code.Length < minLength.Value)
        {
            return (false, $"代碼長度不可小於 {minLength.Value} 個字元");
        }

        if (maxLength.HasValue && code.Length > maxLength.Value)
        {
            return (false, $"代碼長度不可超過 {maxLength.Value} 個字元");
        }

        return (true, null);
    }
}
