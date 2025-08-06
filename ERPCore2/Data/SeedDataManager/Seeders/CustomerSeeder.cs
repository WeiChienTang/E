using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 客戶資料種子器
    /// </summary>
    public class CustomerSeeder : IDataSeeder
    {
        public int Order => 4; // 在 BasicDataSeeder 之後執行，因為需要 CustomerTypes
        public string Name => "客戶資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedCustomersAsync(context);
        }

        /// <summary>
        /// 初始化示例客戶資料
        /// </summary>
        private static async Task SeedCustomersAsync(AppDbContext context)
        {
            if (await context.Customers.AnyAsync())
                return; // 客戶資料已存在

            // 取得客戶類型
            var vipCustomerType = await context.CustomerTypes.FirstOrDefaultAsync(ct => ct.TypeName == "VIP客戶");
            var generalCustomerType = await context.CustomerTypes.FirstOrDefaultAsync(ct => ct.TypeName == "一般客戶");
            var potentialCustomerType = await context.CustomerTypes.FirstOrDefaultAsync(ct => ct.TypeName == "潛在客戶");

            var customers = new[]
            {
                new Customer
                {
                    CustomerCode = "C001",
                    CompanyName = "台灣科技股份有限公司",
                    ContactPerson = "張經理",
                    TaxNumber = "12345678",
                    CustomerTypeId = vipCustomerType?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                },
                new Customer
                {
                    CustomerCode = "C002", 
                    CompanyName = "精密機械工業有限公司",
                    ContactPerson = "李副理",
                    TaxNumber = "23456789",
                    CustomerTypeId = vipCustomerType?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    CreatedBy = "System"
                },
                new Customer
                {
                    CustomerCode = "C003",
                    CompanyName = "全球貿易商行",
                    ContactPerson = "王主任",
                    TaxNumber = "34567890",
                    CustomerTypeId = generalCustomerType?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-20),
                    CreatedBy = "System"
                }
            };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
    }
}
