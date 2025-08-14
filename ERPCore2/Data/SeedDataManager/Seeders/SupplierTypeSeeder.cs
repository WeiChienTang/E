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
                    TypeName = "製造商", 
                    Remarks = "產品製造廠商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "代理商", 
                    Remarks = "產品代理經銷商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "批發商", 
                    Remarks = "批發供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "零售商", 
                    Remarks = "零售供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "服務商", 
                    Remarks = "服務提供商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "原料供應商", 
                    Remarks = "原材料供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "設備供應商", 
                    Remarks = "設備器材供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "軟體供應商", 
                    Remarks = "軟體系統供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "物流商", 
                    Remarks = "物流運輸供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "顧問公司", 
                    Remarks = "專業顧問服務商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                }
            };

            await context.SupplierTypes.AddRangeAsync(supplierTypes);
            await context.SaveChangesAsync();
        }
    }
}
