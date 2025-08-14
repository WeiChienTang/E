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
                    TypeName = "一般客戶", 
                    Description = "一般合作客戶",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new CustomerType { 
                    TypeName = "潛在客戶", 
                    Description = "有合作意向的客戶",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new CustomerType { 
                    TypeName = "合作夥伴", 
                    Description = "策略合作夥伴",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new CustomerType { 
                    TypeName = "企業客戶", 
                    Description = "企業法人客戶",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new CustomerType { 
                    TypeName = "個人客戶", 
                    Description = "個人消費者客戶",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new CustomerType { 
                    TypeName = "政府客戶", 
                    Description = "政府機關客戶",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                }
            };

            await context.CustomerTypes.AddRangeAsync(customerTypes);
            await context.SaveChangesAsync();
        }
    }
}
