using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using ERPCore2.Data;

namespace ERPCore2.Services
{
    /// <summary>
    /// 刪除記錄服務實作
    /// 注意：此服務已棄用，因為系統不再使用軟刪除功能
    /// </summary>
    [Obsolete("此服務已棄用，因為系統不再使用軟刪除功能", false)]
    public class DeletedRecordService : GenericManagementService<DeletedRecord>, IDeletedRecordService
    {
        public DeletedRecordService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<DeletedRecord>> logger) : base(contextFactory, logger)
        {
        }

        public override async Task<List<DeletedRecord>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.DeletedRecords
                    .Where(dr => (dr.TableName.Contains(searchTerm) ||
                         (dr.RecordDisplayName != null && dr.RecordDisplayName.Contains(searchTerm)) ||
                         (dr.DeletedBy != null && dr.DeletedBy.Contains(searchTerm)) ||
                         (dr.DeleteReason != null && dr.DeleteReason.Contains(searchTerm))))
                    .OrderByDescending(dr => dr.DeletedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger, new {
                    Method = nameof(SearchAsync),
                    ServiceType = GetType().Name,
                    SearchTerm = searchTerm
                });
                return new List<DeletedRecord>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(DeletedRecord entity)
        {
            try
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(entity.TableName))
                    errors.Add("資料表名稱不能為空");

                if (entity.RecordId <= 0)
                    errors.Add("記錄ID必須大於0");

                if (errors.Any())
                    return ServiceResult.Failure(string.Join("; ", errors));

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ValidateAsync), GetType(), _logger, new {
                    Method = nameof(ValidateAsync),
                    ServiceType = GetType().Name,
                    EntityId = entity.Id,
                    TableName = entity.TableName,
                    RecordId = entity.RecordId
                });
                return ServiceResult.Failure("驗證過程發生錯誤");
            }
        }

        public override async Task<List<DeletedRecord>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.DeletedRecords
                    .AsQueryable()
                    .OrderByDescending(dr => dr.DeletedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger, new {
                    Method = nameof(GetAllAsync),
                    ServiceType = GetType().Name
                });
                return new List<DeletedRecord>();
            }
        }

        public async Task<ServiceResult> RecordDeletionAsync(string tableName, int recordId, string? recordDisplayName = null, string? deleteReason = null, string? deletedBy = null)
        {
            try
            {
                var deletedRecord = new DeletedRecord
                {
                    TableName = tableName,
                    RecordId = recordId,
                    RecordDisplayName = recordDisplayName,
                    DeleteReason = deleteReason,
                    DeletedBy = deletedBy,
                    DeletedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    CreatedBy = deletedBy,
                    Status = EntityStatus.Active
                };

                var validationResult = await ValidateAsync(deletedRecord);
                if (!validationResult.IsSuccess)
                    return validationResult;

                using var context = await _contextFactory.CreateDbContextAsync();
                await context.DeletedRecords.AddAsync(deletedRecord);
                await context.SaveChangesAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RecordDeletionAsync), GetType(), _logger, new {
                    Method = nameof(RecordDeletionAsync),
                    ServiceType = GetType().Name,
                    TableName = tableName,
                    RecordId = recordId,
                    RecordDisplayName = recordDisplayName,
                    DeletedBy = deletedBy
                });
                return ServiceResult.Failure("記錄刪除失敗");
            }
        }

        public async Task<DeletedRecord?> GetByTableAndRecordAsync(string tableName, int recordId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.DeletedRecords
                    .Where(dr => dr.TableName == tableName && dr.RecordId == recordId)
                    .OrderByDescending(dr => dr.DeletedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByTableAndRecordAsync), GetType(), _logger, new {
                    Method = nameof(GetByTableAndRecordAsync),
                    ServiceType = GetType().Name,
                    TableName = tableName,
                    RecordId = recordId
                });
                return null;
            }
        }

        public async Task<List<DeletedRecord>> GetByTableNameAsync(string tableName)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.DeletedRecords
                    .Where(dr => dr.TableName == tableName)
                    .OrderByDescending(dr => dr.DeletedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByTableNameAsync), GetType(), _logger, new {
                    Method = nameof(GetByTableNameAsync),
                    ServiceType = GetType().Name,
                    TableName = tableName
                });
                return new List<DeletedRecord>();
            }
        }

        public async Task<List<DeletedRecord>> GetByDeletedByAsync(string deletedBy)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.DeletedRecords
                    .Where(dr => dr.DeletedBy == deletedBy)
                    .OrderByDescending(dr => dr.DeletedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByDeletedByAsync), GetType(), _logger, new {
                    Method = nameof(GetByDeletedByAsync),
                    ServiceType = GetType().Name,
                    DeletedBy = deletedBy
                });
                return new List<DeletedRecord>();
            }
        }

        public async Task<ServiceResult> PermanentlyDeleteAsync(int deletedRecordId, string tableName, int recordId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // 1. 首先確認 DeletedRecord 存在
                    var deletedRecord = await context.DeletedRecords
                        .FirstOrDefaultAsync(dr => dr.Id == deletedRecordId);

                    if (deletedRecord == null)
                    {
                        return ServiceResult.Failure("刪除記錄不存在");
                    }

                    // 2. 確認資料表名稱和記錄ID匹配
                    if (deletedRecord.TableName != tableName || deletedRecord.RecordId != recordId)
                    {
                        return ServiceResult.Failure("刪除記錄資訊不匹配");
                    }

                    // 3. 根據資料表名稱動態刪除原始記錄
                    await DeleteOriginalRecordAsync(context, tableName, recordId);

                    // 4. 刪除 DeletedRecord 記錄本身
                    context.DeletedRecords.Remove(deletedRecord);

                    // 5. 儲存變更
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return ServiceResult.Success();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PermanentlyDeleteAsync), GetType(), _logger, new {
                    Method = nameof(PermanentlyDeleteAsync),
                    ServiceType = GetType().Name,
                    DeletedRecordId = deletedRecordId,
                    TableName = tableName,
                    RecordId = recordId
                });
                return ServiceResult.Failure("永久刪除時發生錯誤");
            }
        }

        /// <summary>
        /// 根據資料表名稱動態刪除原始記錄
        /// </summary>
        private async Task DeleteOriginalRecordAsync(AppDbContext context, string tableName, int recordId)
        {
            try
            {
                // 使用反射動態查找對應的 DbSet
                var dbSetProperty = FindDbSetProperty(context, tableName);
                if (dbSetProperty == null)
                {
                    throw new NotSupportedException($"不支援永久刪除資料表：{tableName}");
                }

                // 獲取 DbSet 實例
                var dbSet = dbSetProperty.GetValue(context);
                if (dbSet == null)
                {
                    throw new InvalidOperationException($"DbSet 為 null：{tableName}");
                }

                // 獲取實體類型
                var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];

                // 動態查找已軟刪除的記錄
                var entity = await FindDeletedEntityAsync(context, entityType, recordId);
                if (entity != null)
                {
                    // 使用反射調用 Remove 方法
                    var removeMethod = dbSetProperty.PropertyType.GetMethod("Remove");
                    removeMethod?.Invoke(dbSet, new[] { entity });
                }
                else
                {
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 根據資料表名稱查找對應的 DbSet 屬性
        /// </summary>
        private System.Reflection.PropertyInfo? FindDbSetProperty(AppDbContext context, string tableName)
        {
            var contextType = context.GetType();
            var properties = contextType.GetProperties()
                .Where(p => p.PropertyType.IsGenericType && 
                           p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToList();

            // 嘗試多種匹配方式
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

        /// <summary>
        /// 動態查找已軟刪除的實體
        /// </summary>
        private async Task<object?> FindDeletedEntityAsync(AppDbContext context, Type entityType, int recordId)
        {
            try
            {
                // 確認實體類型繼承自 BaseEntity
                if (!typeof(BaseEntity).IsAssignableFrom(entityType))
                {
                    return null;
                }

                // 使用 DbContext.Set<T>() 方法獲取 DbSet
                var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes)?.MakeGenericMethod(entityType);
                var set = setMethod?.Invoke(context, null) as IQueryable<BaseEntity>;
                
                if (set == null) 
                {
                    return null;
                }

                // 查找符合條件的實體：Id == recordId（已不再使用軟刪除）
                var entity = await set
                    .Where(e => e.Id == recordId)
                    .FirstOrDefaultAsync();

                return entity;
            }
            catch
            {
                return null;
            }
        }
    }
}

