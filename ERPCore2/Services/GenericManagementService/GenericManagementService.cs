using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERPCore2.Data;
using ERPCore2.Data.Context;
using ERPCore2.Data.Enums;
using ERPCore2.Services;
using ERPCore2.Helpers;

namespace ERPCore2.Services.GenericManagementService
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
        /// 取得所有資料（不含已刪除）
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                return await dbSet
                    .Where(x => !x.IsDeleted)
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
                    .Where(x => !x.IsDeleted && x.Status == EntityStatus.Active)
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
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
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
                entity.IsDeleted = false;
                
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
                    .FirstOrDefaultAsync(x => x.Id == entity.Id && !x.IsDeleted);
                    
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

                // 更新資訊
                entity.UpdatedAt = DateTime.UtcNow;
                entity.CreatedAt = existingEntity.CreatedAt; // 保持原建立時間
                entity.CreatedBy = existingEntity.CreatedBy; // 保持原建立者

                context.Entry(existingEntity).CurrentValues.SetValues(entity);
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
        /// 刪除資料（軟刪除）
        /// </summary>
        public virtual async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                var entity = await dbSet
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                    
                if (entity == null)
                {
                    return ServiceResult.Failure("找不到要刪除的資料");
                }

                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure($"刪除資料時發生錯誤: {ex.Message}");
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
        /// 批次刪除
        /// </summary>
        public virtual async Task<ServiceResult> DeleteBatchAsync(List<int> ids)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var dbSet = context.Set<T>();
                
                var entities = await dbSet
                    .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
                    .ToListAsync();

                if (!entities.Any())
                {
                    return ServiceResult.Failure("找不到要刪除的資料");
                }

                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedAt = DateTime.UtcNow;
                }

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
                
                var query = dbSet.Where(x => !x.IsDeleted);

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
                
                return await dbSet.AnyAsync(x => x.Id == id && !x.IsDeleted);
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
                
                return await dbSet.CountAsync(x => !x.IsDeleted);
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
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                    
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
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
                    
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
                    .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
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
            return await Task.FromResult(ServiceResult.Success());
        }

        #endregion
    }
}