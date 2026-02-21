using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace ERPCore2.Helpers;

/// <summary>
/// 簡化版 EF Core 查詢攔截器 - 統計資料庫查詢次數
/// </summary>
public class SimpleQueryCounterInterceptor : DbCommandInterceptor
{
    private static int _queryCount = 0;
    private static readonly Dictionary<string, int> _tableAccessCount = new();
    private static readonly object _lock = new();

    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        LogQuery(command, eventData.Duration.TotalMilliseconds);
        return base.ReaderExecuted(command, eventData, result);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        LogQuery(command, eventData.Duration.TotalMilliseconds);
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        LogQuery(command, eventData.Duration.TotalMilliseconds);
        return base.NonQueryExecuted(command, eventData, result);
    }

    public override async ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        LogQuery(command, eventData.Duration.TotalMilliseconds);
        return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    private void LogQuery(DbCommand command, double durationMs)
    {
        lock (_lock)
        {
            _queryCount++;
            var tables = ExtractTableNames(command.CommandText);
            var queryType = GetQueryType(command.CommandText);

            // 統計表格存取次數
            foreach (var table in tables)
            {
                if (_tableAccessCount.ContainsKey(table))
                    _tableAccessCount[table]++;
                else
                    _tableAccessCount[table] = 1;
            }
        }
    }

    /// <summary>
    /// 從 SQL 中提取表格名稱
    /// </summary>
    private static List<string> ExtractTableNames(string sql)
    {
        var tables = new List<string>();

        // 匹配 FROM [TableName] 和 JOIN [TableName]
        var matches = Regex.Matches(sql, @"(?:FROM|JOIN)\s+\[(\w+)\]", RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var tableName = match.Groups[1].Value;
            if (!tables.Contains(tableName))
                tables.Add(tableName);
        }

        return tables;
    }

    /// <summary>
    /// 取得查詢類型
    /// </summary>
    private static string GetQueryType(string sql)
    {
        var trimmed = sql.TrimStart();
        if (trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return "SELECT";
        if (trimmed.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
            return "INSERT";
        if (trimmed.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
            return "UPDATE";
        if (trimmed.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
            return "DELETE";
        return "OTHER";
    }

    /// <summary>
    /// 重置統計資料 (可用於測試或手動重置)
    /// </summary>
    public static void ResetStats()
    {
        lock (_lock)
        {
            _queryCount = 0;
            _tableAccessCount.Clear();
        }
    }

}
