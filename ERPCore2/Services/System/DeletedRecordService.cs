using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services
{
    /// <summary>
    /// 刪除記錄服務實作
    /// </summary>
    public class DeletedRecordService : GenericManagementService<DeletedRecord>, IDeletedRecordService
    {
        public DeletedRecordService(IDbContextFactory<AppDbContext> contextFactory) : base(contextFactory)
        {
        }

        public DeletedRecordService(
            IDbContextFactory<AppDbContext> contextFactory, 
            ILogger<GenericManagementService<DeletedRecord>> logger) : base(contextFactory, logger)
        {
        }

        /// <summary>
        /// 記錄刪除操作
        /// </summary>
        public async Task<DeletedRecord> LogDeleteAsync(string tableName, int recordId, string? recordDisplayName = null, string? deletedBy = null, string? deleteReason = null)
        {
            var deletedRecord = new DeletedRecord
            {
                TableName = tableName,
                RecordId = recordId,
                RecordDisplayName = recordDisplayName,
                DeletedBy = deletedBy,
                DeleteReason = deleteReason,
                DeletedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                CreatedBy = deletedBy
            };

            var result = await AddAsync(deletedRecord);
            if (result.IsSuccess)
            {
                return result.Data!;
            }

            throw new InvalidOperationException($"記錄刪除操作失敗: {result.Message}");
        }

        /// <summary>
        /// 根據資料表名稱和記錄ID取得刪除記錄
        /// </summary>
        public async Task<DeletedRecord?> GetDeletedRecordAsync(string tableName, int recordId)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.DeletedRecords
                .FirstOrDefaultAsync(dr => dr.TableName == tableName && dr.RecordId == recordId);
        }

        /// <summary>
        /// 根據資料表名稱取得所有刪除記錄
        /// </summary>
        public async Task<List<DeletedRecord>> GetDeletedRecordsByTableAsync(string tableName)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.DeletedRecords
                .Where(dr => dr.TableName == tableName)
                .OrderByDescending(dr => dr.DeletedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 根據刪除用戶取得刪除記錄
        /// </summary>
        public async Task<List<DeletedRecord>> GetDeletedRecordsByUserAsync(string deletedBy)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.DeletedRecords
                .Where(dr => dr.DeletedBy == deletedBy)
                .OrderByDescending(dr => dr.DeletedAt)
                .ToListAsync();
        }
    }
}
