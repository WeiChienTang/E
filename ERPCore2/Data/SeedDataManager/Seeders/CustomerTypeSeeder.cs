using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 客戶類型種子器
    /// </summary>
    public class CustomerTypeSeeder : IDataSeeder
    {
        public int Order => 12;
        public string Name => "客戶類型";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.CustomerTypes.AnyAsync())
                return;

            var customerTypes = new[]
            {
                new CustomerType { 
                    Code = "GEN",
                    TypeName = "一般客戶", 
                    Remarks = "一般合作客戶",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new CustomerType { 
                    Code = "PER",
                    TypeName = "個人客戶", 
                    Remarks = "個人消費者客戶",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
            };

            await context.CustomerTypes.AddRangeAsync(customerTypes);
            await context.SaveChangesAsync();
        }
    }
}
