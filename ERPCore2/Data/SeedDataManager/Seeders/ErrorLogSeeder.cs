using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 錯誤記錄種子器
    /// </summary>
    public class ErrorLogSeeder : IDataSeeder
    {
        public int Order => 11; // 在基礎資料之後執行
        public string Name => "錯誤記錄資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedErrorLogsAsync(context);
        }

        /// <summary>
        /// 新增錯誤記錄測試資料
        /// </summary>
        private static async Task SeedErrorLogsAsync(AppDbContext context)
        {
            if (await context.ErrorLogs.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);

            var errorLogs = new[]
            {
                new ErrorLog
                {
                    ErrorId = Guid.NewGuid().ToString(),
                    Message = "資料庫連線逾時",
                    StackTrace = "System.Data.SqlClient.SqlException: Timeout expired",
                    Source = ErrorSource.Database,
                    Level = ErrorLevel.Error,
                    OccurredAt = createdAt1,
                    ExceptionType = "System.Data.SqlClient.SqlException",
                    Category = "Database",
                    Module = "CustomerService",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy,
                    IsResolved = true,
                    ResolvedBy = "系統管理員",
                    ResolvedAt = createdAt1.AddHours(2),
                    Resolution = "已重新啟動資料庫服務"
                },
                new ErrorLog
                {
                    ErrorId = Guid.NewGuid().ToString(),
                    Message = "客戶資料驗證失敗",
                    StackTrace = "ERPCore2.Services.ValidationException: 客戶名稱不能為空",
                    Source = ErrorSource.BusinessLogic,
                    Level = ErrorLevel.Warning,
                    OccurredAt = createdAt2,
                    ExceptionType = "ERPCore2.Services.ValidationException",
                    Category = "Validation",
                    Module = "CustomerService",
                    RequestPath = "/customers/create",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy,
                    IsResolved = true,
                    ResolvedBy = "開發人員",
                    ResolvedAt = createdAt2.AddHours(1),
                    Resolution = "已修正前端驗證邏輯"
                },
                new ErrorLog
                {
                    ErrorId = Guid.NewGuid().ToString(),
                    Message = "記憶體不足",
                    StackTrace = "System.OutOfMemoryException: Insufficient memory to continue",
                    Source = ErrorSource.System,
                    Level = ErrorLevel.Critical,
                    OccurredAt = createdAt3,
                    ExceptionType = "System.OutOfMemoryException",
                    Category = "Resource",
                    Module = "ProductService",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy,
                    IsResolved = false
                }
            };

            await context.ErrorLogs.AddRangeAsync(errorLogs);
            await context.SaveChangesAsync();
        }
    }
}
