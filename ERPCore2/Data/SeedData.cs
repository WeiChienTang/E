using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

            // 確保資料庫已建立
            await context.Database.EnsureCreatedAsync();

            // 檢查是否已有資料
            if (await context.ContactTypes.AnyAsync())
            {
                return; // 資料已存在
            }            // 新增聯絡類型資料
            var contactTypes = new[]
            {                new ContactType { 
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
                    TypeName = "Email", 
                    Description = "電子郵件地址",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                },
                new ContactType { 
                    TypeName = "傳真", 
                    Description = "傳真號碼",
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System",
                    Status = EntityStatus.Active
                }
            };

            await context.ContactTypes.AddRangeAsync(contactTypes);

            // 新增地址類型資料
            var addressTypes = new[]
            {
                new AddressType { TypeName = "公司地址", Description = "公司營業地址" },
                new AddressType { TypeName = "通訊地址", Description = "通訊聯絡地址" },
                new AddressType { TypeName = "帳單地址", Description = "帳單寄送地址" },
                new AddressType { TypeName = "送貨地址", Description = "商品送貨地址" }
            };

            await context.AddressTypes.AddRangeAsync(addressTypes);

            // 新增客戶類型資料
            var customerTypes = new[]
            {
                new CustomerType { TypeName = "VIP客戶", Description = "重要客戶" },
                new CustomerType { TypeName = "一般客戶", Description = "一般合作客戶" },
                new CustomerType { TypeName = "潛在客戶", Description = "有合作意向的客戶" },
                new CustomerType { TypeName = "合作夥伴", Description = "策略合作夥伴" }
            };

            await context.CustomerTypes.AddRangeAsync(customerTypes);
            // 新增行業別資料
            var industryTypes = new[]
            {
                new IndustryType { IndustryTypeName = "製造業", IndustryTypeCode = "MFG" },
                new IndustryType { IndustryTypeName = "資訊科技業", IndustryTypeCode = "IT" },
                new IndustryType { IndustryTypeName = "服務業", IndustryTypeCode = "SVC" },
                new IndustryType { IndustryTypeName = "貿易業", IndustryTypeCode = "TRD" },
                new IndustryType { IndustryTypeName = "建築業", IndustryTypeCode = "CON" },
                new IndustryType { IndustryTypeName = "金融業", IndustryTypeCode = "FIN" },
                new IndustryType { IndustryTypeName = "零售業", IndustryTypeCode = "RTL" },
                new IndustryType { IndustryTypeName = "餐飲業", IndustryTypeCode = "F&B" }
            };

            await context.IndustryTypes.AddRangeAsync(industryTypes);

            // 儲存變更
            await context.SaveChangesAsync();
        }
    }
}
