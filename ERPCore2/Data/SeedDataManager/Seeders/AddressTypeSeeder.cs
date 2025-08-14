using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 地址類型種子器
    /// </summary>
    public class AddressTypeSeeder : IDataSeeder
    {
        public int Order => 11;
        public string Name => "地址類型";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.AddressTypes.AnyAsync())
                return;

            var addressTypes = new[]
            {
                new AddressType { 
                    TypeName = "公司地址", 
                    Description = "公司營業地址",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new AddressType { 
                    TypeName = "通訊地址", 
                    Description = "通訊聯絡地址",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new AddressType { 
                    TypeName = "帳單地址", 
                    Description = "帳單寄送地址",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new AddressType { 
                    TypeName = "送貨地址", 
                    Description = "商品送貨地址",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new AddressType { 
                    TypeName = "倉庫地址", 
                    Description = "倉庫或儲存地址",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new AddressType { 
                    TypeName = "工廠地址", 
                    Description = "生產工廠地址",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                }
            };

            await context.AddressTypes.AddRangeAsync(addressTypes);
            await context.SaveChangesAsync();
        }
    }
}
