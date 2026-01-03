using ERPCore2.Models;
using ERPCore2.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ERPCore2.Helpers.EditModal;

/// <summary>
/// 編號生成策略
/// </summary>
public enum CodeGenerationStrategy
{
    /// <summary>
    /// 時間戳策略 - 使用前綴 + 完整時間戳 (yyyyMMddHHmmss)
    /// 例如: PO20251110143025
    /// 優點: 簡單快速、包含完整時間資訊
    /// 缺點: 無法知道是當天第幾筆
    /// </summary>
    Timestamp,
    
    /// <summary>
    /// 時間戳 + 序號策略 - 使用前綴 + 日期 + 當日流水號
    /// 例如: PO20251110001, PO20251110002
    /// 優點: 既有日期資訊，又可知道是當天第幾筆，序號連續累加
    /// 缺點: 需要查詢資料庫計算當日最大序號
    /// </summary>
    TimestampWithSequence,
    
    /// <summary>
    /// 每日序號策略 - 使用前綴 + 日期 + 當日流水號
    /// 例如: PO20251110001, PO20251110002
    /// 優點: 可以清楚知道是當天第幾筆訂單
    /// 缺點: 需要查詢資料庫計算當日最大序號
    /// </summary>
    DailySequence,
    
    /// <summary>
    /// 每月序號策略 - 使用前綴 + 年月 + 當月流水號
    /// 例如: PO2025110001, PO2025110002
    /// 優點: 可以清楚知道是當月第幾筆訂單
    /// 缺點: 需要查詢資料庫計算當月最大序號
    /// </summary>
    MonthlySequence,
    
    /// <summary>
    /// 全局序號策略 - 使用前綴 + 全局流水號
    /// 例如: PO000001, PO000002
    /// 優點: 簡單清晰的遞增序號
    /// 缺點: 不包含時間資訊、需要資料庫支援
    /// </summary>
    GlobalSequence
}

/// <summary>
/// 編碼生成策略特性
/// 標記在實體類別上，指定該實體使用的編碼生成策略
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CodeGenerationStrategyAttribute : Attribute
{
    /// <summary>
    /// 編碼策略
    /// </summary>
    public CodeGenerationStrategy Strategy { get; set; }
    
    /// <summary>
    /// 編碼前綴
    /// </summary>
    public string Prefix { get; set; }
    
    /// <summary>
    /// 日期欄位名稱（用於序號策略的日期範圍查詢）
    /// </summary>
    /// <remarks>
    /// 預設使用 "CreatedAt"，可以指定為其他日期欄位如 "OrderDate"
    /// </remarks>
    public string DateFieldName { get; set; } = "CreatedAt";
    
    /// <summary>
    /// 序號位數（用於補零）
    /// </summary>
    public int SequenceDigits { get; set; } = 3;
    
    /// <summary>
    /// 建構函數
    /// </summary>
    public CodeGenerationStrategyAttribute(
        CodeGenerationStrategy strategy = CodeGenerationStrategy.Timestamp,
        string prefix = "")
    {
        Strategy = strategy;
        Prefix = prefix;
    }
}

/// <summary>
/// 實體編號生成輔助類別
/// 統一處理所有實體的編號生成邏輯，減少重複代碼
/// </summary>
public static class EntityCodeGenerationHelper
{
    /// <summary>
    /// 為指定實體生成編號（自動讀取特性配置，優先使用）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TService">服務類型</typeparam>
    /// <param name="service">服務實例</param>
    /// <param name="dbContext">資料庫上下文（用於序號查詢）</param>
    /// <param name="excludeId">排除的實體ID（用於更新時檢查重複）</param>
    /// <returns>生成的編號</returns>
    public static async Task<string> GenerateForEntity<TEntity, TService>(
        TService service,
        DbContext dbContext,
        int? excludeId = null)
        where TEntity : class
        where TService : class
    {
        // 從實體類別讀取特性配置
        var attribute = typeof(TEntity).GetCustomAttribute<CodeGenerationStrategyAttribute>();
        
        if (attribute == null)
        {
            throw new InvalidOperationException(
                $"實體 {typeof(TEntity).Name} 未標記 CodeGenerationStrategy 特性。" +
                $"請在實體類別上加上 [CodeGenerationStrategy(策略, Prefix = \"前綴\")] 特性。");
        }
        
        // 根據策略生成編號
        return await GenerateWithStrategy<TEntity>(
            service,
            dbContext,
            attribute.Strategy,
            attribute.Prefix,
            attribute.DateFieldName,
            attribute.SequenceDigits,
            excludeId
        );
    }
    
    /// <summary>
    /// 根據策略生成編號（內部方法）
    /// </summary>
    private static async Task<string> GenerateWithStrategy<TEntity>(
        object service,
        DbContext dbContext,
        CodeGenerationStrategy strategy,
        string prefix,
        string dateFieldName,
        int sequenceDigits,
        int? excludeId)
        where TEntity : class
    {
        try
        {
            var now = DateTime.Now;
            string baseCode;
            
            switch (strategy)
            {
                case CodeGenerationStrategy.Timestamp:
                    // 時間戳策略 - 使用現有邏輯
                    var timestamp = now.ToString("yyyyMMddHHmmss");
                    baseCode = $"{prefix}{timestamp}";
                    
                    // 檢查重複
                    if (await IsCodeExists<TEntity>(dbContext, baseCode, excludeId))
                    {
                        var random = new Random().Next(100, 999);
                        baseCode = $"{prefix}{timestamp}{random}";
                        
                        if (await IsCodeExists<TEntity>(dbContext, baseCode, excludeId))
                        {
                            var preciseTime = now.ToString("yyyyMMddHHmmssfff");
                            baseCode = $"{prefix}{preciseTime}";
                        }
                    }
                    break;
                    
                case CodeGenerationStrategy.TimestampWithSequence:
                    // 時間戳 + 序號策略 - PO20251110001
                    // 使用日期作為時間戳（不含時分秒），同一天的單據序號累加
                    var dailyTimestamp = now.ToString("yyyyMMdd");
                    var maxSeqForDay = await GetMaxSequenceNumberByTimestamp<TEntity>(
                        dbContext,
                        prefix,
                        dailyTimestamp,
                        excludeId
                    );
                    var seqForDay = (maxSeqForDay + 1).ToString($"D{sequenceDigits}");
                    baseCode = $"{prefix}{dailyTimestamp}{seqForDay}";
                    break;
                    
                case CodeGenerationStrategy.DailySequence:
                    // 每日序號策略
                    var dailyDate = now.Date;
                    var maxDailySeq = await GetMaxSequenceNumber<TEntity>(
                        dbContext, 
                        prefix, 
                        dateFieldName, 
                        dailyDate, 
                        dailyDate.AddDays(1),
                        excludeId
                    );
                    var dailySeq = (maxDailySeq + 1).ToString($"D{sequenceDigits}");
                    baseCode = $"{prefix}{now:yyyyMMdd}{dailySeq}";
                    break;
                    
                case CodeGenerationStrategy.MonthlySequence:
                    // 每月序號策略
                    var monthStart = new DateTime(now.Year, now.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);
                    var maxMonthlySeq = await GetMaxSequenceNumber<TEntity>(
                        dbContext, 
                        prefix, 
                        dateFieldName, 
                        monthStart, 
                        monthEnd,
                        excludeId
                    );
                    var monthlySeq = (maxMonthlySeq + 1).ToString($"D{sequenceDigits}");
                    baseCode = $"{prefix}{now:yyyyMM}{monthlySeq}";
                    break;
                    
                case CodeGenerationStrategy.GlobalSequence:
                    // 全局序號策略
                    var maxGlobalSeq = await GetMaxSequenceNumber<TEntity>(
                        dbContext, 
                        prefix, 
                        dateFieldName, 
                        DateTime.MinValue, 
                        DateTime.MaxValue,
                        excludeId
                    );
                    var globalSeq = (maxGlobalSeq + 1).ToString($"D{sequenceDigits}");
                    baseCode = $"{prefix}{globalSeq}";
                    break;
                    
                default:
                    throw new ArgumentException($"不支援的編號生成策略: {strategy}");
            }
            
            // 最終檢查（保險起見）
            if (await IsCodeExists<TEntity>(dbContext, baseCode, excludeId))
            {
                // 加上時間戳後綴
                baseCode = $"{baseCode}_{now:HHmmssfff}";
            }
            
            return baseCode;
        }
        catch
        {
            // 失敗時返回安全的時間戳格式
            return $"{prefix}{DateTime.Now:yyyyMMddHHmmssfff}";
        }
    }
    
    /// <summary>
    /// 檢查編號是否已存在
    /// </summary>
    private static async Task<bool> IsCodeExists<TEntity>(
        DbContext dbContext,
        string code,
        int? excludeId)
        where TEntity : class
    {
        var dbSet = dbContext.Set<TEntity>();
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        
        // 建立 e.Code == code 的表達式
        var codeProperty = Expression.Property(parameter, "Code");
        var codeValue = Expression.Constant(code);
        var codeEquals = Expression.Equal(codeProperty, codeValue);
        
        Expression finalExpression = codeEquals;
        
        // 如果有 excludeId，加上 e.Id != excludeId 條件
        if (excludeId.HasValue)
        {
            var idProperty = Expression.Property(parameter, "Id");
            var idValue = Expression.Constant(excludeId.Value);
            var idNotEquals = Expression.NotEqual(idProperty, idValue);
            finalExpression = Expression.AndAlso(codeEquals, idNotEquals);
        }
        
        var lambda = Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);
        
        return await dbSet.AnyAsync(lambda);
    }
    
    /// <summary>
    /// 取得指定時間戳前綴的最大序號（用於 TimestampWithSequence 策略）
    /// </summary>
    private static async Task<int> GetMaxSequenceNumberByTimestamp<TEntity>(
        DbContext dbContext,
        string prefix,
        string timestamp,
        int? excludeId)
        where TEntity : class
    {
        try
        {
            var dbSet = dbContext.Set<TEntity>();
            var searchPattern = $"{prefix}{timestamp}";
            
            var query = dbSet.AsQueryable();
            
            // 如果有 excludeId，排除該筆
            if (excludeId.HasValue)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var idProperty = Expression.Property(parameter, "Id");
                var idValue = Expression.Constant(excludeId.Value);
                var idNotEquals = Expression.NotEqual(idProperty, idValue);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(idNotEquals, parameter);
                query = query.Where(lambda);
            }
            
            // 查詢以該時間戳開頭的所有編號
            var codes = await query
                .Select(e => EF.Property<string>(e, "Code"))
                .Where(code => code.StartsWith(searchPattern))
                .ToListAsync();
            
            if (!codes.Any())
                return 0;
            
            // 從編號中提取序號（時間戳後面的數字）
            var maxSeq = codes
                .Select(code =>
                {
                    if (code.Length <= searchPattern.Length)
                        return 0;
                    
                    var seqPart = code.Substring(searchPattern.Length);
                    // 移除可能的下劃線後綴
                    var underscoreIndex = seqPart.IndexOf('_');
                    if (underscoreIndex > 0)
                    {
                        seqPart = seqPart.Substring(0, underscoreIndex);
                    }
                    
                    return int.TryParse(seqPart, out var seq) ? seq : 0;
                })
                .DefaultIfEmpty(0)
                .Max();
            
            return maxSeq;
        }
        catch
        {
            return 0;
        }
    }
    
    /// <summary>
    /// 取得指定日期範圍內的最大序號
    /// </summary>
    private static async Task<int> GetMaxSequenceNumber<TEntity>(
        DbContext dbContext,
        string prefix,
        string dateFieldName,
        DateTime startDate,
        DateTime endDate,
        int? excludeId)
        where TEntity : class
    {
        try
        {
            var dbSet = dbContext.Set<TEntity>();
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            
            // 建立日期範圍過濾條件
            var dateProperty = Expression.Property(parameter, dateFieldName);
            var startValue = Expression.Constant(startDate);
            var endValue = Expression.Constant(endDate);
            
            var greaterThanOrEqual = Expression.GreaterThanOrEqual(dateProperty, startValue);
            var lessThan = Expression.LessThan(dateProperty, endValue);
            var dateRangeCondition = Expression.AndAlso(greaterThanOrEqual, lessThan);
            
            // 如果有 excludeId，加上排除條件
            if (excludeId.HasValue)
            {
                var idProperty = Expression.Property(parameter, "Id");
                var idValue = Expression.Constant(excludeId.Value);
                var idNotEquals = Expression.NotEqual(idProperty, idValue);
                dateRangeCondition = Expression.AndAlso(dateRangeCondition, idNotEquals);
            }
            
            var lambda = Expression.Lambda<Func<TEntity, bool>>(dateRangeCondition, parameter);
            
            // 查詢符合條件的編號
            var codes = await dbSet
                .Where(lambda)
                .Select(e => EF.Property<string>(e, "Code"))
                .ToListAsync();
            
            if (!codes.Any())
                return 0;
            
            // 從編號中提取序號
            var maxSeq = codes
                .Where(code => !string.IsNullOrEmpty(code) && code.StartsWith(prefix))
                .Select(code => ExtractSequenceNumber(code, prefix))
                .Where(seq => seq.HasValue)
                .DefaultIfEmpty(0)
                .Max();
            
            return maxSeq ?? 0;
        }
        catch
        {
            return 0;
        }
    }
    
    /// <summary>
    /// 從編號中提取序號
    /// </summary>
    private static int? ExtractSequenceNumber(string code, string prefix)
    {
        if (string.IsNullOrEmpty(code) || !code.StartsWith(prefix))
            return null;
        
        // 移除前綴
        var withoutPrefix = code.Substring(prefix.Length);
        
        // 使用正則表達式提取最後的連續數字
        var match = Regex.Match(withoutPrefix, @"(\d+)$");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var seq))
        {
            return seq;
        }
        
        return null;
    }

    /// <summary>
    /// 為指定實體生成編號（使用約定優於配置，向下相容舊版本）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TService">服務類型</typeparam>
    /// <param name="service">服務實例</param>
    /// <param name="prefix">編號前綴</param>
    /// <param name="excludeId">排除的實體ID（用於更新時檢查重複）</param>
    /// <returns>生成的編號</returns>
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
    /// 為指定實體生成編號（使用自訂檢查函式）
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TService">服務類型</typeparam>
    /// <param name="service">服務實例</param>
    /// <param name="prefix">編號前綴</param>
    /// <param name="codeExistsChecker">自訂的編號檢查函式</param>
    /// <param name="excludeId">排除的實體ID（用於更新時檢查重複）</param>
    /// <returns>生成的編號</returns>
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
    /// 為指定實體生成簡單編號（使用時間戳記，不檢查重複）
    /// </summary>
    /// <param name="prefix">編號前綴</param>
    /// <param name="usePreciseTimestamp">是否使用精確時間戳記（含毫秒）</param>
    /// <returns>生成的編號</returns>
    /// <remarks>
    /// 適用於不需要檢查重複的場景，例如：訂單號、單據號等
    /// </remarks>
    public static string GenerateSimpleCode(string prefix, bool usePreciseTimestamp = false)
    {
        return CodeGenerationHelper.GenerateSimpleEntityCode(prefix, usePreciseTimestamp);
    }

    /// <summary>
    /// 批次為多個實體生成編號
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TService">服務類型</typeparam>
    /// <param name="service">服務實例</param>
    /// <param name="prefix">編號前綴</param>
    /// <param name="count">需要生成的數量</param>
    /// <returns>生成的編號列表</returns>
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
    /// 驗證編號格式是否符合規範
    /// </summary>
    /// <param name="code">要驗證的編號</param>
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
            return (false, "編號不可為空");
        }

        if (!code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return (false, $"編號必須以 {prefix} 開頭");
        }

        if (minLength.HasValue && code.Length < minLength.Value)
        {
            return (false, $"編號長度不可小於 {minLength.Value} 個字元");
        }

        if (maxLength.HasValue && code.Length > maxLength.Value)
        {
            return (false, $"編號長度不可超過 {maxLength.Value} 個字元");
        }

        return (true, null);
    }
}
