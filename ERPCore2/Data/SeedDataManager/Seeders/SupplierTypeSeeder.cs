using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 廠商類型種子器
    /// </summary>
    public class SupplierTypeSeeder : IDataSeeder
    {
        public int Order => 13;
        public string Name => "廠商類型";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.SupplierTypes.AnyAsync())
                return;

            var supplierTypes = new[]
            {
                new SupplierType { 
                    Code = "GEN",
                    TypeName = "製造商", 
                    Remarks = "產品製造廠商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    Code = "RAW",
                    TypeName = "原料供應商", 
                    Remarks = "原材料供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
            };

            await context.SupplierTypes.AddRangeAsync(supplierTypes);
            await context.SaveChangesAsync();
        }
    }
}
