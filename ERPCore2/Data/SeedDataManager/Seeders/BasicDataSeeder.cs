using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 基礎資料種子器
    /// </summary>
    public class BasicDataSeeder : IDataSeeder
    {
        public int Order => 1;
        public string Name => "基礎資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedContactTypesAsync(context);
            await SeedAddressTypesAsync(context);
            await SeedCustomerTypesAsync(context);
            await SeedIndustryTypesAsync(context);
            await SeedSupplierTypesAsync(context);
        }

        /// <summary>
        /// 新增聯絡類型資料
        /// </summary>
        private static async Task SeedContactTypesAsync(AppDbContext context)
        {
            if (await context.ContactTypes.AnyAsync())
                return;

            var contactTypes = new[]
            {
                new ContactType {
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
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 新增地址類型資料
        /// </summary>
        private static async Task SeedAddressTypesAsync(AppDbContext context)
        {
            if (await context.AddressTypes.AnyAsync())
                return;

            var addressTypes = new[]
            {
                new AddressType { TypeName = "公司地址", Description = "公司營業地址" },
                new AddressType { TypeName = "通訊地址", Description = "通訊聯絡地址" },
                new AddressType { TypeName = "帳單地址", Description = "帳單寄送地址" },
                new AddressType { TypeName = "送貨地址", Description = "商品送貨地址" }
            };

            await context.AddressTypes.AddRangeAsync(addressTypes);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 新增客戶類型資料
        /// </summary>
        private static async Task SeedCustomerTypesAsync(AppDbContext context)
        {
            if (await context.CustomerTypes.AnyAsync())
                return;

            var customerTypes = new[]
            {
                new CustomerType { TypeName = "VIP客戶", Description = "重要客戶" },
                new CustomerType { TypeName = "一般客戶", Description = "一般合作客戶" },
                new CustomerType { TypeName = "潛在客戶", Description = "有合作意向的客戶" },
                new CustomerType { TypeName = "合作夥伴", Description = "策略合作夥伴" }
            };

            await context.CustomerTypes.AddRangeAsync(customerTypes);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 新增行業別資料
        /// </summary>
        private static async Task SeedIndustryTypesAsync(AppDbContext context)
        {
            if (await context.IndustryTypes.AnyAsync())
                return;

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
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 新增廠商類型資料
        /// </summary>
        private static async Task SeedSupplierTypesAsync(AppDbContext context)
        {
            if (await context.SupplierTypes.AnyAsync())
                return;

            var supplierTypes = new[]
            {
                new SupplierType { 
                    TypeName = "製造商", 
                    Description = "產品製造廠商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "代理商", 
                    Description = "產品代理經銷商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "批發商", 
                    Description = "批發供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "零售商", 
                    Description = "零售供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "服務商", 
                    Description = "服務提供商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "原料供應商", 
                    Description = "原材料供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "設備供應商", 
                    Description = "設備器材供應商",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new SupplierType { 
                    TypeName = "軟體供應商", 
                    Description = "軟體系統供應商",
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
