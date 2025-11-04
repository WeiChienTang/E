using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 公司資料種子器
    /// </summary>
    public class CompanySeeder : IDataSeeder
    {
        public int Order => 1; // 優先執行，作為基礎資料
        public string Name => "公司資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedCompaniesAsync(context);
        }

        private static async Task SeedCompaniesAsync(AppDbContext context)
        {
            if (await context.Companies.AnyAsync()) return;

            var currentTime = DateTime.UtcNow;
            var (_, createdBy) = SeedDataHelper.GetSystemCreateInfo(0);

            var companies = new[]
            {
                new Company
                {
                    Code = "MJ001",
                    CompanyName = "美莊股份有限公司",
                    CompanyNameEn = "Mei Juang",
                    Address = "634雲林縣褒忠鄉中正路28-10號",
                    Website = "https://www.meijuang.com/",
                    Email = "service@meijuang.com",
                    Phone = "(05)697-1210",
                    Fax = "(05)697-3210",
                    Status = EntityStatus.Active,
                    CreatedAt = currentTime,
                    CreatedBy = createdBy,
                    Remarks = "預設公司資料"
                }
            };

            await context.Companies.AddRangeAsync(companies);
            await context.SaveChangesAsync();
        }
    }
}
