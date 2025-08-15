using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 產品類別資料種子類別
    /// </summary>
    public class ProductCategorySeeder : IDataSeeder
    {
        public int Order => 5;
        public string Name => "產品類別";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedProductCategoriesAsync(context);
        }

        /// <summary>
        /// 種子商品類別資料
        /// </summary>
        private static async Task SeedProductCategoriesAsync(AppDbContext context)
        {
            if (await context.ProductCategories.AnyAsync())
                return; // 資料已存在

            var categories = new[]
            {
                new ProductCategory
                {
                    Code = "PC001",
                    CategoryName = "原物料",
                    Remarks = "生產原料材料",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                },
                new ProductCategory
                {
                    Code = "PC002",
                    CategoryName = "半成品",
                    Remarks = "半成品",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                },
                new ProductCategory
                {
                    Code = "PC003",
                    CategoryName = "成品",
                    Remarks = "已完成製造的產品",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "System"
                }
            };

            await context.ProductCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
