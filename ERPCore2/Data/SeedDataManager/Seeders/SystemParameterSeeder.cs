using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
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

            var systemParameters = new[]
            {
                new SystemParameter
                {
                    TaxRate = 5.00m, // 預設稅率 5%
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Remarks = "系統預設稅率設定"
                }
            };

            await context.SystemParameters.AddRangeAsync(systemParameters);
            await context.SaveChangesAsync();
        }
    }
}
