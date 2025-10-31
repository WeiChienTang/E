using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 聯絡類型種子器
    /// </summary>
    public class ContactTypeSeeder : IDataSeeder
    {
        public int Order => 10;
        public string Name => "聯絡類型";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.ContactTypes.AnyAsync())
                return;

            var contactTypes = new[]
            {
                new ContactType {
                    Code = "PHONE",
                    TypeName = "電話",
                    Description = "固定電話號碼",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new ContactType { 

                    TypeName = "手機", 
                    Description = "行動電話號碼",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new ContactType { 
                    Code = "EMAIL",
                    TypeName = "Email", 
                    Description = "電子郵件地址",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new ContactType { 
                    Code = "FAX",
                    TypeName = "傳真", 
                    Description = "傳真號碼",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new ContactType { 
                    Code = "LINE",
                    TypeName = "Line", 
                    Description = "Line 即時通訊",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new ContactType { 
                    Code = "WEBSITE",
                    TypeName = "網站", 
                    Description = "公司或個人網站",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                }
            };

            await context.ContactTypes.AddRangeAsync(contactTypes);
            await context.SaveChangesAsync();
        }
    }
}
