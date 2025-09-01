# DeletedRecordService 軟刪除記錄服務

## 📋 概述

`DeletedRecordService` 是 ERPCore2 系統中負責管理軟刪除記錄的核心服務。它提供了完整的軟刪除審計追蹤功能，記錄所有被軟刪除的資料，並支援永久刪除操作。

## 🎯 主要功能

### 1. 軟刪除記錄管理
- **記錄刪除操作**：當實體被軟刪除時，自動記錄刪除資訊
- **審計追蹤**：保存刪除時間、刪除者、刪除原因等詳細資訊
- **歷史查詢**：提供多種查詢方式來檢視刪除歷史

### 2. 永久刪除功能
- **真實刪除**：同時從 `DeletedRecord` 表和原始實體表中移除記錄
- **交易安全**：使用資料庫交易確保操作的原子性
- **動態支援**：使用反射技術自動支援所有實體類型

## 🔧 核心方法

### 記錄管理方法
```csharp
// 記錄軟刪除操作
Task<ServiceResult> RecordDeletionAsync(string tableName, int recordId, 
    string? recordDisplayName = null, string? deleteReason = null, string? deletedBy = null)

// 根據資料表和記錄ID查詢
Task<DeletedRecord?> GetByTableAndRecordAsync(string tableName, int recordId)

// 根據資料表名稱查詢
Task<List<DeletedRecord>> GetByTableNameAsync(string tableName)

// 根據刪除者查詢
Task<List<DeletedRecord>> GetByDeletedByAsync(string deletedBy)
```

### 永久刪除方法
```csharp
// 永久刪除記錄（真實刪除）
Task<ServiceResult> PermanentlyDeleteAsync(int deletedRecordId, string tableName, int recordId)
```

## 🚀 反射技術實現

### 動態 DbSet 查找

服務使用反射技術動態查找對應的 DbSet，無需為每個實體類型編寫專門的處理代碼：

```csharp
/// <summary>
/// 根據資料表名稱查找對應的 DbSet 屬性
/// </summary>
private PropertyInfo? FindDbSetProperty(AppDbContext context, string tableName)
{
    var contextType = context.GetType();
    var properties = contextType.GetProperties()
        .Where(p => p.PropertyType.IsGenericType && 
                   p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
        .ToList();

    var normalizedTableName = tableName.ToLower();
    
    foreach (var property in properties)
    {
        var propertyName = property.Name.ToLower();
        
        // 1. 直接匹配屬性名稱（複數形式）
        if (propertyName == normalizedTableName || propertyName == normalizedTableName + "s")
        {
            return property;
        }
        
        // 2. 匹配實體類型名稱（單數形式）
        var entityType = property.PropertyType.GetGenericArguments()[0];
        var entityTypeName = entityType.Name.ToLower();
        if (entityTypeName == normalizedTableName)
        {
            return property;
        }
        
        // 3. 移除常見後綴進行匹配
        var withoutSuffix = normalizedTableName.TrimEnd('s');
        if (propertyName == withoutSuffix || entityTypeName == withoutSuffix)
        {
            return property;
        }
    }

    return null;
}
```

### 智能匹配策略

系統支援多種表名匹配方式：

| 輸入表名 | 匹配的 DbSet | 實體類型 |
|---------|-------------|----------|
| "Employee" | `Employees` | `Employee` |
| "Employees" | `Employees` | `Employee` |
| "Product" | `Products` | `Product` |
| "PurchaseOrder" | `PurchaseOrders` | `PurchaseOrder` |

### 動態實體查詢

使用 `DbContext.Set<T>()` 方法動態獲取 DbSet 並執行查詢：

```csharp
/// <summary>
/// 動態查找已軟刪除的實體
/// </summary>
private async Task<object?> FindDeletedEntityAsync(AppDbContext context, Type entityType, int recordId)
{
    // 確認實體類型繼承自 BaseEntity
    if (!typeof(BaseEntity).IsAssignableFrom(entityType))
    {
        return null;
    }

    // 使用 DbContext.Set<T>() 方法獲取 DbSet
    var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes)?.MakeGenericMethod(entityType);
    var set = setMethod?.Invoke(context, null) as IQueryable<BaseEntity>;
    
    if (set == null) return null;

    // 查找符合條件的實體：Id == recordId && IsDeleted == true
    var entity = await set
        .Where(e => e.Id == recordId && e.IsDeleted)
        .FirstOrDefaultAsync();

    return entity;
}
```

### 動態 Remove 操作

使用反射動態調用 DbSet 的 Remove 方法：

```csharp
// 使用反射調用 Remove 方法
var removeMethod = dbSetProperty.PropertyType.GetMethod("Remove");
removeMethod?.Invoke(dbSet, new[] { entity });
```

## 🔒 安全性與驗證

### 多重驗證機制
1. **記錄存在性驗證**：確認 DeletedRecord 存在
2. **資訊匹配驗證**：確認資料表名稱和記錄ID匹配
3. **軟刪除狀態驗證**：只刪除 `IsDeleted = true` 的記錄
4. **實體類型驗證**：確認實體繼承自 `BaseEntity`

### 交易安全
```csharp
using var transaction = await context.Database.BeginTransactionAsync();
try
{
    // 1. 驗證操作
    // 2. 刪除原始記錄
    // 3. 刪除 DeletedRecord
    // 4. 提交交易
    await transaction.CommitAsync();
}
catch (Exception)
{
    await transaction.RollbackAsync();
    throw;
}
```

## 📊 使用範例

### 記錄軟刪除
```csharp
// 當員工被軟刪除時記錄
await deletedRecordService.RecordDeletionAsync(
    tableName: "Employee",
    recordId: 123,
    recordDisplayName: "張三",
    deleteReason: "離職",
    deletedBy: "管理員"
);
```

### 查詢刪除記錄
```csharp
// 查詢特定員工的刪除記錄
var deletedRecord = await deletedRecordService.GetByTableAndRecordAsync("Employee", 123);

// 查詢所有員工的刪除記錄
var employeeDeletes = await deletedRecordService.GetByTableNameAsync("Employee");

// 查詢特定管理員執行的刪除操作
var adminDeletes = await deletedRecordService.GetByDeletedByAsync("管理員");
```

### 永久刪除
```csharp
// 永久刪除記錄（同時從兩個表中移除）
var result = await deletedRecordService.PermanentlyDeleteAsync(
    deletedRecordId: 1,
    tableName: "Employee", 
    recordId: 123
);

if (result.IsSuccess)
{
    Console.WriteLine("永久刪除成功");
}
```

## 🎨 設計優勢

### 1. 完全動態化
- **自動適應**：新增實體時無需修改服務代碼
- **零配置**：只要實體繼承 `BaseEntity` 就自動支援
- **智能匹配**：支援多種命名慣例

### 2. 高度可維護性
- **單一職責**：專注於軟刪除記錄管理
- **清晰邏輯**：方法職責分明，易於理解和維護
- **擴展性強**：可輕鬆添加新功能

### 3. 強大的審計功能
- **完整記錄**：保存所有刪除相關資訊
- **可追溯性**：支援多維度查詢和分析
- **歷史保存**：永久保存刪除歷史（除非永久刪除）

## 🛡️ 錯誤處理

### 全面的異常處理
- **反射錯誤**：處理 DbSet 查找失敗
- **資料庫錯誤**：處理查詢和更新異常
- **驗證錯誤**：處理資料驗證失敗
- **交易錯誤**：自動回滾失敗的交易

### 詳細的日誌記錄
```csharp
// 成功操作記錄
_logger?.LogInformation($"永久刪除記錄成功：資料表={tableName}, 記錄ID={recordId}");

// 警告記錄
_logger?.LogWarning($"未找到對應的 DbSet：{tableName}");

// 錯誤記錄
_logger?.LogError(ex, $"刪除原始記錄失敗：資料表={tableName}, 記錄ID={recordId}");
```

## 🔄 與其他服務的整合

### GenericManagementService 整合
`DeletedRecordService` 繼承自 `GenericManagementService<DeletedRecord>`，獲得完整的 CRUD 功能。

### UI 組件整合
與 `DeletedRecordIndex.razor` 頁面完美整合，提供用戶友善的管理介面。

## 📈 效能考量

### 反射效能最佳化
- **一次性查找**：DbSet 屬性只查找一次
- **快取機制**：可考慮添加 DbSet 查找結果快取
- **延遲執行**：只在需要時執行反射操作

### 查詢效能
- **索引支援**：`DeletedRecord` 表有適當的索引
- **分頁支援**：大量資料時支援分頁查詢
- **條件篩選**：支援多種篩選條件減少資料傳輸

## 🚀 未來擴展方向

1. **批次操作**：支援批次永久刪除
2. **快取機制**：添加 DbSet 查找結果快取
3. **事件驅動**：集成領域事件通知機制
4. **統計分析**：提供刪除操作統計和分析功能
5. **恢復功能**：從刪除記錄恢復軟刪除的實體

---

> 💡 **提示**：這個服務展示了如何巧妙地使用反射技術創建通用且可維護的解決方案，是現代 .NET 應用程式設計的最佳實踐範例。
