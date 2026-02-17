# Service 服務層設計

## 更新日期
2026-02-17

---

## 概述

服務層封裝所有業務邏輯與資料存取操作。所有服務繼承 `GenericManagementService<T>`，透過 `ServiceResult` 統一回傳操作結果，並使用 `ErrorHandlingHelper` 統一處理錯誤。

---

## 標準服務建立流程

### 1. 建立介面

```csharp
// Services/IYourEntityService.cs
namespace ERPCore2.Services
{
    public interface IYourEntityService : IGenericManagementService<YourEntity>
    {
        // 自訂業務方法（如有需要）
        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);
    }
}
```

### 2. 建立實作

```csharp
// Services/YourEntityService.cs
namespace ERPCore2.Services
{
    public class YourEntityService : GenericManagementService<YourEntity>, IYourEntityService
    {
        public YourEntityService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<YourEntity>> logger) : base(contextFactory, logger)
        {
        }

        // 覆寫 GetAllAsync 加入 Include 與排序
        public override async Task<List<YourEntity>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.YourEntities
                    .Include(e => e.RelatedEntity)
                    .OrderBy(e => e.Code)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(GetAllAsync), GetType(), _logger, new {
                        Method = nameof(GetAllAsync),
                        ServiceType = GetType().Name
                    });
                return new List<YourEntity>();
            }
        }

        // 實作驗證邏輯
        public override async Task<ServiceResult> ValidateAsync(YourEntity entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.Code))
                    errors.Add("編號不能為空");

                if (!string.IsNullOrWhiteSpace(entity.Code) &&
                    await IsCodeExistsAsync(entity.Code, entity.Id == 0 ? null : entity.Id))
                    errors.Add("編號已存在");

                return errors.Any()
                    ? ServiceResult.Failure(string.Join("; ", errors))
                    : ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(ValidateAsync), GetType(), _logger, new {
                        Method = nameof(ValidateAsync),
                        EntityId = entity.Id
                    });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        // 自訂業務方法
        public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var query = context.YourEntities.Where(e => e.Code == code);
                if (excludeId.HasValue)
                    query = query.Where(e => e.Id != excludeId.Value);
                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(IsCodeExistsAsync), GetType(), _logger, new {
                        Code = code, ExcludeId = excludeId
                    });
                return false;
            }
        }
    }
}
```

### 3. 註冊 DI 服務

在 `Data/ServiceRegistration.cs` 中註冊：

```csharp
services.AddScoped<IYourEntityService, YourEntityService>();
```

---

## GenericManagementService 提供的功能

### Protected 成員

| 成員 | 類型 | 說明 |
|------|------|------|
| `_contextFactory` | `IDbContextFactory<AppDbContext>` | 資料庫上下文工廠 |
| `_logger` | `ILogger<GenericManagementService<T>>` | 日誌記錄器 |

### 標準 CRUD 方法

| 方法 | 說明 |
|------|------|
| `GetAllAsync()` | 取得所有記錄 |
| `GetByIdAsync(int id)` | 依 ID 取得單筆 |
| `CreateAsync(T entity)` | 新增記錄 |
| `UpdateAsync(T entity)` | 更新記錄 |
| `DeleteAsync(int id)` | 刪除記錄 |

### 批次操作

| 方法 | 說明 |
|------|------|
| `CreateBatchAsync(List<T>)` | 批次新增 |
| `UpdateBatchAsync(List<T>)` | 批次更新 |
| `DeleteBatchAsync(List<int>)` | 批次刪除 |

### 查詢與狀態

| 方法 | 說明 |
|------|------|
| `GetPagedAsync(int, int)` | 分頁查詢 |
| `SearchAsync(string)` | 搜尋 |
| `SetStatusAsync(int, EntityStatus)` | 設定狀態 |
| `ToggleStatusAsync(int)` | 切換狀態 |
| `GetPreviousIdAsync(int)` | 取得上一筆 ID |
| `GetNextIdAsync(int)` | 取得下一筆 ID |

### 必須覆寫的方法

| 方法 | 說明 |
|------|------|
| `SearchAsync(string searchTerm)` | 實作特定實體的搜尋邏輯 |
| `ValidateAsync(T entity)` | 實作特定實體的驗證邏輯 |

---

## 錯誤處理規則

### 安全回傳值

| 回傳類型 | 安全預設值 |
|---------|-----------|
| `List<T>` | `new List<T>()` |
| `T?` | `null` |
| `bool` | `false` |
| `ServiceResult` | `ServiceResult.Failure("錯誤訊息")` |

### 統一使用 ErrorHandlingHelper

```csharp
// Service 層使用 HandleServiceErrorAsync
await ErrorHandlingHelper.HandleServiceErrorAsync(
    ex, nameof(MethodName), GetType(), _logger, new { /* 額外資訊 */ });

// 頁面層使用 HandlePageErrorAsync
await ErrorHandlingHelper.HandlePageErrorAsync(
    ex, nameof(MethodName), GetType(), additionalData: "錯誤描述");
```

---

## 開發檢查清單

- [ ] 繼承 `GenericManagementService<T>`
- [ ] **不重複宣告** `_logger`、`_contextFactory` 欄位（基底類別已提供）
- [ ] 建構子正確調用 `base(contextFactory, logger)`
- [ ] 使用 `using var context = await _contextFactory.CreateDbContextAsync()`
- [ ] 覆寫 `SearchAsync()` 和 `ValidateAsync()`
- [ ] 所有公開方法都有 `try-catch`
- [ ] `catch` 中使用 `ErrorHandlingHelper` 並回傳安全預設值
- [ ] 在 `ServiceRegistration.cs` 註冊 `AddScoped<>`

---

## 相關文件

- [README_完整頁面設計總綱.md](README_完整頁面設計總綱.md) - 總綱
