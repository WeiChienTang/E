using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 供應商資料種子器
    /// </summary>
    public class SupplierSeeder : IDataSeeder
    {
        public int Order => 5; // 在 BasicDataSeeder 之後執行，因為需要 SupplierTypes
        public string Name => "供應商資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedSuppliersAsync(context);
        }

        /// <summary>
        /// 初始化示例供應商資料
        /// </summary>
        private static async Task SeedSuppliersAsync(AppDbContext context)
        {
            if (await context.Suppliers.AnyAsync())
                return; // 供應商資料已存在

            // 取得供應商類型
            var manufacturerType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "製造商");
            var agentType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "代理商");
            var wholesalerType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "批發商");
            var retailerType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "零售商");
            var serviceType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "服務商");
            var materialType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "原料供應商");
            var equipmentType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "設備供應商");
            var softwareType = await context.SupplierTypes.FirstOrDefaultAsync(st => st.TypeName == "軟體供應商");


            var suppliers = new[]
            {
                new Supplier
                {
                    SupplierCode = "S001",
                    CompanyName = "精密科技製造股份有限公司",
                    ContactPerson = "張總經理",
                    TaxNumber = "20123456",
                    SupplierTypeId = manufacturerType?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-90),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S002",
                    CompanyName = "台灣電子元件有限公司",
                    ContactPerson = "李經理",
                    TaxNumber = "20234567",
                    SupplierTypeId = manufacturerType?.Id ?? 1,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-85),
                    CreatedBy = "System"
                },
                new Supplier
                {
                    SupplierCode = "S003",
                    CompanyName = "全球軟體代理商",
                    ContactPerson = "王協理",
                    TaxNumber = "20345678",
                    SupplierTypeId = agentType?.Id ?? 2,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-80),
                    CreatedBy = "System"
                }
            };

            await context.Suppliers.AddRangeAsync(suppliers);
            await context.SaveChangesAsync();
        }
    }
}
