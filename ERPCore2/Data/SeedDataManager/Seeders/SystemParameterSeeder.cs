using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Navigation;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 系統參數資料種子器
    /// </summary>
    public class SystemParameterSeeder : IDataSeeder
    {
        public int Order => 2; // 早期執行，作為系統基礎設定
        public string Name => "系統參數資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedSystemParametersAsync(context);
        }

        private static async Task SeedSystemParametersAsync(AppDbContext context)
        {
            if (await context.SystemParameters.AnyAsync()) return;

            var (createdAt, createdBy) = SeedDataHelper.GetSystemCreateInfo(0);

            var parameter = new SystemParameter
            {
                Status = EntityStatus.Active,
                CreatedAt = createdAt,
                CreatedBy = createdBy
            };

            // 從 SystemParameterDefaults 套用預設值（Single Source of Truth）
            SystemParameterDefaults.ApplyDefaults(parameter);

            var systemParameters = new[] { parameter };

            await context.SystemParameters.AddRangeAsync(systemParameters);
            await context.SaveChangesAsync();
        }
    }
}
