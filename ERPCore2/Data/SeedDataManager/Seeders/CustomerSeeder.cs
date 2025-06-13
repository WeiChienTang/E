using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 客戶資料種子器
    /// </summary>
    public class CustomerSeeder : IDataSeeder
    {
        public int Order => 2;
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

            // 取得客戶類型和行業類型
            var vipCustomerType = await context.CustomerTypes.FirstOrDefaultAsync(ct => ct.TypeName == "VIP客戶");
            var generalCustomerType = await context.CustomerTypes.FirstOrDefaultAsync(ct => ct.TypeName == "一般客戶");
            var potentialCustomerType = await context.CustomerTypes.FirstOrDefaultAsync(ct => ct.TypeName == "潛在客戶");

            var itIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "IT");
            var mfgIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "MFG");
            var svcIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "SVC");
            var trdIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "TRD");
            var finIndustry = await context.IndustryTypes.FirstOrDefaultAsync(it => it.IndustryTypeCode == "FIN");

            var customers = new[]
            {
                new Customer
                {
                    CustomerCode = "C001",
                    CompanyName = "台灣科技股份有限公司",
                    ContactPerson = "張經理",
                    TaxNumber = "12345678",
                    CustomerTypeId = vipCustomerType?.Id ?? 1,
                    IndustryTypeId = itIndustry?.Id ?? 1,
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
                    IndustryTypeId = mfgIndustry?.Id ?? 2,
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
                    IndustryTypeId = trdIndustry?.Id ?? 3,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-20),
                    CreatedBy = "System"
                },
                new Customer
                {
                    CustomerCode = "C004",
                    CompanyName = "優質服務顧問公司",
                    ContactPerson = "劉總監",
                    TaxNumber = "45678901",
                    CustomerTypeId = generalCustomerType?.Id ?? 2,
                    IndustryTypeId = svcIndustry?.Id ?? 4,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-15),
                    CreatedBy = "System"
                },
                new Customer
                {
                    CustomerCode = "C005",
                    CompanyName = "新興金融集團",
                    ContactPerson = "陳協理",
                    TaxNumber = "56789012",
                    CustomerTypeId = potentialCustomerType?.Id ?? 3,
                    IndustryTypeId = finIndustry?.Id ?? 5,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    CreatedBy = "System"
                },
                new Customer
                {
                    CustomerCode = "C006",
                    CompanyName = "創新軟體開發股份有限公司",
                    ContactPerson = "黃經理",
                    TaxNumber = "67890123",
                    CustomerTypeId = generalCustomerType?.Id ?? 2,
                    IndustryTypeId = itIndustry?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    CreatedBy = "System"
                },
                new Customer
                {
                    CustomerCode = "C007",
                    CompanyName = "暫停合作企業有限公司",
                    ContactPerson = "林主管",
                    TaxNumber = "78901234",
                    CustomerTypeId = generalCustomerType?.Id ?? 2,
                    IndustryTypeId = mfgIndustry?.Id ?? 2,
                    Status = EntityStatus.Inactive,
                    CreatedAt = DateTime.Now.AddDays(-60),
                    CreatedBy = "System"
                }
            };

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
    }
}
