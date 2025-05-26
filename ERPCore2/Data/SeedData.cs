using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
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
            }

            // 新增聯絡類型資料
            var contactTypes = new[]
            {
                new ContactType { TypeName = "電話" },
                new ContactType { TypeName = "手機" },
                new ContactType { TypeName = "Email" },
                new ContactType { TypeName = "傳真" }
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
            var industries = new[]
            {
                new Industry { IndustryName = "製造業", IndustryCode = "MFG" },
                new Industry { IndustryName = "資訊科技業", IndustryCode = "IT" },
                new Industry { IndustryName = "服務業", IndustryCode = "SVC" },
                new Industry { IndustryName = "貿易業", IndustryCode = "TRD" },
                new Industry { IndustryName = "建築業", IndustryCode = "CON" },
                new Industry { IndustryName = "金融業", IndustryCode = "FIN" },
                new Industry { IndustryName = "零售業", IndustryCode = "RTL" },
                new Industry { IndustryName = "餐飲業", IndustryCode = "F&B" }
            };

            await context.Industries.AddRangeAsync(industries);

            // 儲存變更
            await context.SaveChangesAsync();
        }
    }
}
