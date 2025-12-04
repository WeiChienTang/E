# 資料表調用次數偵測功能

## 📋 功能概述

為了監控和優化資料庫查詢效能,實作了 `SimpleQueryCounterInterceptor` 來即時追蹤 EF Core 的資料庫查詢次數、執行時間和表格存取情況。

**主要目的:**
- 檢測資料庫調用次數,避免重複查詢
- 識別 N+1 查詢問題
- 監控慢查詢 (>100ms)
- 提供簡潔的查詢統計資訊

---

## 🎯 實作方式

### 1. 建立 SimpleQueryCounterInterceptor

**檔案位置:** `Helpers/SimpleQueryCounterInterceptor.cs`

這是一個無依賴的 EF Core 攔截器,特點:
- ✅ **無 HttpContext 依賴** - 避免啟動時的 DI 問題
- ✅ **直接 new 實例** - 不需要從 DI 容器取得
- ✅ **使用 ConsoleHelper** - 彩色輸出,清晰易讀
- ✅ **自動統計** - 追蹤查詢次數和表格存取

**核心功能:**
```csharp
public class SimpleQueryCounterInterceptor : DbCommandInterceptor
{
    private static int _queryCount = 0;
    private static readonly Dictionary<string, int> _tableAccessCount = new();
    
    // 攔截所有資料庫查詢並記錄
    public override DbDataReader ReaderExecuted(...)
    {
        LogQuery(command, eventData.Duration.TotalMilliseconds);
        return base.ReaderExecuted(...);
    }
}
```

### 2. 註冊 Interceptor

**檔案位置:** `Data/ServiceRegistration.cs`

在 DbContextFactory 配置時直接建立實例:

```csharp
public static void AddDatabaseServices(this IServiceCollection services, string connectionString)
{
    services.AddDbContextFactory<AppDbContext>(options =>
        options.UseSqlServer(connectionString,
            sqlServerOptions => sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
        .AddInterceptors(new SimpleQueryCounterInterceptor())); // 直接 new 實例
}
```

### 3. 設定日誌等級

**檔案位置:** `appsettings.Development.json`, `appsettings.json`

隱藏 EF Core 原始的詳細 SQL 日誌:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  }
}
```

---

## 📊 輸出格式

### 正常查詢輸出

終端機會顯示簡潔的查詢資訊:

```
ℹ Query #1 - SELECT in 19.5ms - 
ℹ Query #5 - SELECT in 5.5ms - Permissions
ℹ Query #11 - SELECT in 9.4ms - Employees
ℹ Query #23 - SELECT in 1.7ms - Materials
```

**格式說明:**
- `ℹ` - 資訊圖示 (藍色/Cyan)
- `Query #N` - 查詢編號 (從啟動開始累計)
- `SELECT/INSERT/UPDATE/DELETE` - 查詢類型
- `Xms` - 執行時間 (毫秒)
- `TableName1, TableName2` - 涉及的表格名稱

### 慢查詢警告

當查詢執行時間超過 100ms:

```
ℹ Query #42 - SELECT in 125.3ms - Orders, OrderDetails
⚠ Slow query detected: 125.3ms
```

### N+1 查詢警告

當同一表格被存取超過 5 次:

```
ℹ Query #15 - SELECT in 3.2ms - Employees
⚠ Table 'Employees' accessed 6 times - possible N+1 query issue
```

---

## 🔧 使用方式

### 基本使用

啟動應用程式後,Interceptor 會自動記錄所有查詢:

```bash
dotnet run
```

終端機會即時顯示查詢統計。

### 查看統計摘要 (程式碼調用)

在程式碼中可以呼叫靜態方法查看摘要:

```csharp
SimpleQueryCounterInterceptor.ShowSummary();
```

輸出範例:
```
════════════════════════════════════════════════════════════
  Database Query Statistics
════════════════════════════════════════════════════════════
ℹ Total Queries Executed: 34
ℹ Table Access Count:
  ⚠ Employees: 12 times
  ✓ Permissions: 3 times
  ✓ Products: 2 times
  ✓ Warehouses: 1 times
════════════════════════════════════════════════════════════
```

### 重置統計資料

```csharp
SimpleQueryCounterInterceptor.ResetStats();
```

---

## ⚠️ 注意事項

### 1. 靜態計數器的限制

- **全域計數** - 所有請求共用同一個計數器
- **無請求隔離** - 無法區分不同 HTTP 請求的查詢
- **執行緒安全** - 已使用 `lock` 確保執行緒安全

**影響:**
- 查詢編號會持續累加,不會在每個請求後重置
- 無法準確統計「單一請求調用幾次資料庫」
- 適合用於開發階段的整體監控

### 2. 效能考量

- **最小開銷** - 只做簡單的計數和字串解析
- **不影響查詢** - 不會修改或延遲資料庫操作
- **建議用途** - 開發和測試環境

### 3. 生產環境使用

如需在生產環境使用,建議:
- 移除 Interceptor 或只在特定條件下啟用
- 考慮使用專業的 APM 工具 (如 Application Insights)
- 或改用 MiniProfiler 進行效能分析

---

## 🔍 技術細節

### 表格名稱提取

使用正則表達式從 SQL 中提取表格名稱:

```csharp
private static List<string> ExtractTableNames(string sql)
{
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
```

### 查詢類型判斷

```csharp
private static string GetQueryType(string sql)
{
    var trimmed = sql.TrimStart();
    if (trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        return "SELECT";
    if (trimmed.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
        return "INSERT";
    // ... 其他類型
}
```

### 執行緒安全

```csharp
private void LogQuery(DbCommand command, double durationMs)
{
    lock (_lock) // 確保執行緒安全
    {
        _queryCount++;
        // ... 記錄邏輯
    }
}
```

---

## 📈 優化建議

當發現以下情況時,建議優化:

### 1. 查詢次數過多
```
ℹ Query #1 - SELECT in 5ms - Products
ℹ Query #2 - SELECT in 3ms - Products
ℹ Query #3 - SELECT in 4ms - Products
⚠ Table 'Products' accessed 10 times
```

**解決方案:**
- 使用 `.Include()` 進行預先載入
- 考慮使用 `.AsSplitQuery()` 或 `.AsSingleQuery()`
- 檢查是否有 N+1 查詢問題

### 2. 慢查詢
```
ℹ Query #5 - SELECT in 250ms - Orders
⚠ Slow query detected: 250ms
```

**解決方案:**
- 檢查是否缺少索引
- 優化查詢邏輯
- 考慮分頁或限制結果數量

### 3. 重複查詢相同資料
```
ℹ Query #10 - SELECT in 3ms - Employees WHERE Id = 1
ℹ Query #15 - SELECT in 2ms - Employees WHERE Id = 1
ℹ Query #20 - SELECT in 3ms - Employees WHERE Id = 1
```

**解決方案:**
- 實作快取機制
- 在服務層面重用查詢結果
- 使用 `IMemoryCache` 快取常用資料

---

## 🎨 ConsoleHelper 整合

Interceptor 使用 `ConsoleHelper` 提供彩色輸出:

```csharp
ConsoleHelper.WriteInfo($"Query #{_queryCount} - {queryType} in {duration}ms");
ConsoleHelper.WriteWarning($"Slow query detected: {duration}ms");
ConsoleHelper.WriteWarning($"Table '{table}' accessed {count} times");
```

**輸出顏色:**
- 🔵 **資訊 (Info)** - 藍色 (Cyan) - 正常查詢
- 🟡 **警告 (Warning)** - 黃色 (Yellow) - 慢查詢或重複存取

---

## 📝 更新歷程

### 2025-12-04 - 初始版本

**新增功能:**
1. ✅ 建立 `SimpleQueryCounterInterceptor` 攔截器
2. ✅ 整合 `ConsoleHelper` 彩色輸出
3. ✅ 實作查詢計數和表格存取統計
4. ✅ 慢查詢偵測 (>100ms)
5. ✅ N+1 查詢警告 (同表 >5 次)
6. ✅ 設定 appsettings 隱藏原始 EF Core 日誌

**技術決策:**
- 採用無依賴設計,避免 DI 生命週期問題
- 使用靜態變數進行全域統計
- 直接在 DbContextFactory 註冊時建立實例

**已知限制:**
- 無法按 HTTP 請求分組統計
- 計數器不會自動重置
- 不適合高並發場景的精確統計

---

## 🚀 未來改進方向

如需更精確的請求級別統計,可考慮:

1. **使用 AsyncLocal + 中間件**
   - 在中間件中初始化 AsyncLocal 統計
   - 請求結束時輸出該請求的總查詢次數
   
2. **整合 MiniProfiler**
   - 提供網頁版查詢分析界面
   - 自動檢測 N+1 問題
   - 視覺化查詢時間線

3. **APM 工具整合**
   - Application Insights
   - New Relic
   - Datadog

---

## 📚 相關文件

- [ConsoleHelper 輔助工具](./ConsoleHelper.cs)
- [EF Core Interceptors 官方文件](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors)
- [DbCommandInterceptor API](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.diagnostics.dbcommandinterceptor)

---

## ✅ 總結

透過 `SimpleQueryCounterInterceptor` 可以:
- ✅ 即時監控資料庫查詢次數
- ✅ 快速發現效能問題
- ✅ 識別 N+1 查詢和重複查詢
- ✅ 保持終端機輸出清晰易讀

這是開發階段的實用工具,幫助開發者及早發現和優化資料庫查詢效能問題。
