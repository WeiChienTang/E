using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 商品類別資料種子類別
    /// </summary>
    public class ProductCategorySeeder : IDataSeeder
    {
        public int Order => 5;
        public string Name => "商品類別";

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
                    Name = "成品",
                    IsSaleable = true,  // 成品可對外銷售
                    Remarks = "已完成製造、可對外銷售的商品",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new ProductCategory
                {
                    Code = "PC002",
                    Name = "原物料",
                    IsSaleable = false,  // 原物料不對外銷售
                    Remarks = "生產用原料材料，不對外銷售",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
                new ProductCategory
                {
                    Code = "PC003",
                    Name = "半成品",
                    IsSaleable = false,  // 半成品不對外銷售
                    Remarks = "生產過程中的半成品，不對外銷售",
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                },
            };

            await context.ProductCategories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
