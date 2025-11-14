using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERPCore2.Data;
using ERPCore2.Data.Context;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.Services
{
    /// <summary>
    /// 通用管理服務抽象基底類別
    /// </summary>
    /// <typeparam name="T">實體類型，必須繼承 BaseEntity</typeparam>
    public abstract class GenericManagementService<T> : IGenericManagementService<T> 
        where T : BaseEntity
    {
    protected readonly IDbContextFactory<AppDbContext> _contextFactory;
    protected readonly ILogger<GenericManagementService<T>>? _logger;

        protected GenericManagementService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<T>> logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected GenericManagementService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = null;
        }

        #region 基本 CRUD 操作

        /// <summary>
        /// 取得所有資料
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                return await dbSet
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<T>();
            }
        }

        /// <summary>
        /// 取得所有啟用的資料
        /// </summary>
        public virtual async Task<List<T>> GetActiveAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                return await dbSet
                    .Where(x => x.Status == EntityStatus.Active)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetActiveAsync), GetType(), _logger);
                return new List<T>();
            }
        }

        /// <summary>
        /// 根據 ID 取得單一資料
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                return await dbSet
                    .AsNoTracking() // 避免 Entity Framework 追蹤衝突
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        /// <summary>
        /// 建立新資料
        /// </summary>
        public virtual async Task<ServiceResult<T>> CreateAsync(T entity)
        {
            try
            {
                // 驗證實體
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<T>.Failure(validationResult.ErrorMessage);
                }

                // 設定建立資訊
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
                
                if (entity.Status == default)
                {
                    entity.Status = EntityStatus.Active;
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                dbSet.Add(entity);
                await context.SaveChangesAsync();

                return ServiceResult<T>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<T>.Failure($"建立資料時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新資料
        /// </summary>
        public virtual async Task<ServiceResult<T>> UpdateAsync(T entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                // 檢查實體是否存在
                var existingEntity = await dbSet
                    .FirstOrDefaultAsync(x => x.Id == entity.Id);
                    
                if (existingEntity == null)
                {
                    return ServiceResult<T>.Failure("找不到要更新的資料");
                }

                // 驗證實體
                var validationResult = await ValidateAsync(entity);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<T>.Failure(validationResult.ErrorMessage);
                }

                // 保持原建立資訊
                entity.CreatedAt = existingEntity.CreatedAt;
                entity.CreatedBy = existingEntity.CreatedBy;
                
                // 更新時間
                entity.UpdatedAt = DateTime.UtcNow;

                // 分離舊實體並附加新實體
                context.Entry(existingEntity).State = EntityState.Detached;
                context.Entry(entity).State = EntityState.Modified;
                
                await context.SaveChangesAsync();

                return ServiceResult<T>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<T>.Failure($"更新資料時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 刪除資料（硬刪除）
        /// 不要再調用此方法，已經不再使用軟除 2025/09/24
        /// </summary>
        public virtual async Task<ServiceResult> DeleteAsync(int id)
        {
            // 直接調用永久刪除方法，不再使用軟刪除
            return await PermanentDeleteAsync(id);
        }

        /// <summary>
        /// 永久刪除資料（硬刪除）
        /// </summary>
        public virtual async Task<ServiceResult> PermanentDeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                var entity = await dbSet
                    .FirstOrDefaultAsync(x => x.Id == id);
                    
                if (entity == null)
                {
                    return ServiceResult.Failure("找不到要刪除的資料");
                }

                // 檢查是否可以刪除
                var canDeleteResult = await CanDeleteAsync(entity);
                if (!canDeleteResult.IsSuccess)
                {
                    return canDeleteResult;
                }

                dbSet.Remove(entity);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentDeleteAsync), GetType(), _logger);
                return ServiceResult.Failure($"永久刪除資料時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 批次操作

        /// <summary>
        /// 批次建立
        /// </summary>
        public virtual async Task<ServiceResult<List<T>>> CreateBatchAsync(List<T> entities)
        {
            try
            {
                var results = new List<T>();
                var errors = new List<string>();

                foreach (var entity in entities)
                {
                    var result = await CreateAsync(entity);
                    if (result.IsSuccess)
                    {
                        results.Add(result.Data!);
                    }
                    else
                    {
                        errors.Add($"ID {entity.Id}: {result.ErrorMessage}");
                    }
                }

                if (errors.Any())
                {
                    return ServiceResult<List<T>>.Failure($"部分資料建立失敗: {string.Join(", ", errors)}");
                }

                return ServiceResult<List<T>>.Success(results);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateBatchAsync), GetType(), _logger);
                return ServiceResult<List<T>>.Failure($"批次建立時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次更新
        /// </summary>
        public virtual async Task<ServiceResult<List<T>>> UpdateBatchAsync(List<T> entities)
        {
            try
            {
                var results = new List<T>();
                var errors = new List<string>();

                foreach (var entity in entities)
                {
                    var result = await UpdateAsync(entity);
                    if (result.IsSuccess)
                    {
                        results.Add(result.Data!);
                    }
                    else
                    {
                        errors.Add($"ID {entity.Id}: {result.ErrorMessage}");
                    }
                }

                if (errors.Any())
                {
                    return ServiceResult<List<T>>.Failure($"部分資料更新失敗: {string.Join(", ", errors)}");
                }

                return ServiceResult<List<T>>.Success(results);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateBatchAsync), GetType(), _logger);
                return ServiceResult<List<T>>.Failure($"批次更新時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次刪除（硬刪除）
        /// </summary>
        public virtual async Task<ServiceResult> DeleteBatchAsync(List<int> ids)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                var entities = await dbSet
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync();

                if (!entities.Any())
                {
                    return ServiceResult.Failure("找不到要刪除的資料");
                }

                // 檢查每個實體是否可以刪除
                foreach (var entity in entities)
                {
                    var canDeleteResult = await CanDeleteAsync(entity);
                    if (!canDeleteResult.IsSuccess)
                    {
                        return ServiceResult.Failure($"無法刪除 ID {entity.Id} 的資料: {canDeleteResult.ErrorMessage}");
                    }
                }

                // 執行硬刪除
                dbSet.RemoveRange(entities);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteBatchAsync), GetType(), _logger);
                return ServiceResult.Failure($"批次刪除時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 查詢操作

        /// <summary>
        /// 分頁查詢
        /// </summary>
        public virtual async Task<(List<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                var query = dbSet.AsQueryable();

                // 如果有搜尋條件，使用子類別的搜尋邏輯
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchResults = await SearchAsync(searchTerm);
                    var searchIds = searchResults.Select(x => x.Id).ToList();
                    query = query.Where(x => searchIds.Contains(x.Id));
                }

                var totalCount = await query.CountAsync();
                
                var items = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPagedAsync), GetType(), _logger);
                return (new List<T>(), 0);
            }
        }

        /// <summary>
        /// 根據條件查詢（需要子類別實作具體邏輯）
        /// </summary>
        public abstract Task<List<T>> SearchAsync(string searchTerm);

        /// <summary>
        /// 檢查資料是否存在
        /// </summary>
        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                return await dbSet.AnyAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ExistsAsync), GetType(), _logger);
                return false;
            }
        }

        /// <summary>
        /// 取得資料總數
        /// </summary>
        public virtual async Task<int> GetCountAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                return await dbSet.CountAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetCountAsync), GetType(), _logger);
                return 0;
            }
        }

        #endregion

        #region 狀態管理

        /// <summary>
        /// 設定特定狀態
        /// </summary>
        public virtual async Task<ServiceResult> SetStatusAsync(int id, EntityStatus status)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                var entity = await dbSet
                    .FirstOrDefaultAsync(x => x.Id == id);
                    
                if (entity == null)
                {
                    return ServiceResult.Failure("找不到指定的資料");
                }

                entity.Status = status;
                entity.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetStatusAsync), GetType(), _logger);
                return ServiceResult.Failure($"設定狀態時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 切換狀態（Active <-> Inactive）
        /// </summary>
        public virtual async Task<ServiceResult> ToggleStatusAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                var entity = await dbSet
                    .FirstOrDefaultAsync(x => x.Id == id);
                    
                if (entity == null)
                {
                    return ServiceResult.Failure("找不到指定的資料");
                }

                entity.Status = entity.Status == EntityStatus.Active 
                    ? EntityStatus.Inactive 
                    : EntityStatus.Active;
                    
                entity.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ToggleStatusAsync), GetType(), _logger);
                return ServiceResult.Failure($"切換狀態時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次設定狀態
        /// </summary>
        public virtual async Task<ServiceResult> SetStatusBatchAsync(List<int> ids, EntityStatus status)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                var entities = await dbSet
                    .Where(x => ids.Contains(x.Id))
                    .ToListAsync();

                if (!entities.Any())
                {
                    return ServiceResult.Failure("找不到指定的資料");
                }

                foreach (var entity in entities)
                {
                    entity.Status = status;
                    entity.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SetStatusBatchAsync), GetType(), _logger);
                return ServiceResult.Failure($"批次設定狀態時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 驗證

        /// <summary>
        /// 驗證實體資料（需要子類別實作具體邏輯）
        /// </summary>
        public abstract Task<ServiceResult> ValidateAsync(T entity);

        /// <summary>
        /// 檢查名稱是否存在（適用於有名稱欄位的實體）
        /// 子類別可以覆寫此方法來實作名稱檢查邏輯
        /// </summary>
        public virtual async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            try
            {
                // 預設實作：如果實體沒有 Name 屬性，直接回傳 false
                // 子類別應該覆寫此方法來實作實際的名稱檢查邏輯
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(IsNameExistsAsync), GetType(), _logger);
                return false;
            }
        }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 取得實體的顯示名稱（用於錯誤訊息等）
        /// </summary>
        protected virtual string GetEntityDisplayName()
        {
            return typeof(T).Name;
        }

        /// <summary>
        /// 檢查實體是否可以被刪除
        /// 子類別可以覆寫此方法來實作複雜的刪除邏輯檢查
        /// </summary>
        protected virtual async Task<ServiceResult> CanDeleteAsync(T entity)
        {
            try
            {
                // 預設檢查：使用反射檢查外鍵關聯
                var hasReferences = await CheckForeignKeyReferencesAsync(entity.Id);
                if (hasReferences.HasValue && hasReferences.Value)
                {
                    var entityDisplayName = GetEntityDisplayName();
                    return ServiceResult.Failure($"無法刪除此{entityDisplayName}，因為有其他資料正在使用");
                }
                
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CanDeleteAsync), GetType(), _logger, new { EntityId = entity.Id });
                return ServiceResult.Failure("檢查刪除條件時發生錯誤");
            }
        }
        
        /// <summary>
        /// 檢查是否有外鍵關聯（自動檢測）
        /// 子類別可以覆寫此方法來自訂關聯檢查邏輯
        /// </summary>
        protected virtual async Task<bool?> CheckForeignKeyReferencesAsync(int entityId)
        {
            try
            {
                // 預設實作：讓子類別覆寫具體的關聯檢查邏輯
                // 這裡返回 null 表示使用預設行為（允許刪除）
                await Task.CompletedTask;
                return null;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CheckForeignKeyReferencesAsync), GetType(), _logger, new { EntityId = entityId });
                return null; // 發生錯誤時，預設允許刪除
            }
        }

        #endregion

        #region 記錄導航

        /// <summary>
        /// 取得上一筆記錄的 ID（按 ID 排序）
        /// </summary>
        public virtual async Task<int?> GetPreviousIdAsync(int currentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                // 查找 Id 小於當前 Id 的最大 Id
                var previousId = await dbSet
                    .Where(x => x.Id < currentId)
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();
                
                return previousId > 0 ? previousId : null;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetPreviousIdAsync), GetType(), _logger, new { CurrentId = currentId });
                return null;
            }
        }

        /// <summary>
        /// 取得下一筆記錄的 ID（按 ID 排序）
        /// </summary>
        public virtual async Task<int?> GetNextIdAsync(int currentId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                // 查找 Id 大於當前 Id 的最小 Id
                var nextId = await dbSet
                    .Where(x => x.Id > currentId)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();
                
                return nextId > 0 ? nextId : null;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetNextIdAsync), GetType(), _logger, new { CurrentId = currentId });
                return null;
            }
        }

        /// <summary>
        /// 取得第一筆記錄的 ID（按 ID 排序）
        /// </summary>
        public virtual async Task<int?> GetFirstIdAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                // 查找最小的 Id
                var firstId = await dbSet
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();
                
                return firstId > 0 ? firstId : null;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetFirstIdAsync), GetType(), _logger, null);
                return null;
            }
        }

        /// <summary>
        /// 取得最後一筆記錄的 ID（按 ID 排序）
        /// </summary>
        public virtual async Task<int?> GetLastIdAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                // 查找最大的 Id
                var lastId = await dbSet
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();
                
                return lastId > 0 ? lastId : null;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetLastIdAsync), GetType(), _logger, null);
                return null;
            }
        }

        #endregion
    }
}