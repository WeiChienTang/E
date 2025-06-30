using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 尺寸種子器
    /// </summary>
    public class SizeSeeder : IDataSeeder
    {
        public int Order => 5; // 在基礎資料之後，產品之前
        public string Name => "尺寸資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedSizesAsync(context);
        }

        /// <summary>
        /// 初始化尺寸資料
        /// </summary>
        private static async Task SeedSizesAsync(AppDbContext context)
        {
            // 檢查資料是否已存在
            if (await context.Sizes.AnyAsync())
                return;

            // 取得建立時間和建立者資訊
            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(15);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt6, _) = SeedDataHelper.GetSystemCreateInfo(5);

            var sizes = new[]
            {
                new Size
                {
                    SizeCode = "XS",
                    SizeName = "特小",
                    Description = "特小尺寸 (Extra Small)",
                    SortOrder = 10,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new Size
                {
                    SizeCode = "S",
                    SizeName = "小",
                    Description = "小尺寸 (Small)",
                    SortOrder = 20,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new Size
                {
                    SizeCode = "M",
                    SizeName = "中",
                    Description = "中等尺寸 (Medium)",
                    SortOrder = 30,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new Size
                {
                    SizeCode = "L",
                    SizeName = "大",
                    Description = "大尺寸 (Large)",
                    SortOrder = 40,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                },
                new Size
                {
                    SizeCode = "XL",
                    SizeName = "特大",
                    Description = "特大尺寸 (Extra Large)",
                    SortOrder = 50,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                    CreatedBy = createdBy
                },
                new Size
                {
                    SizeCode = "XXL",
                    SizeName = "超大",
                    Description = "超大尺寸 (Double Extra Large)",
                    SortOrder = 60,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6,
                    CreatedBy = createdBy
                },
                new Size
                {
                    SizeCode = "FREE",
                    SizeName = "均碼",
                    Description = "均碼 (Free Size)",
                    SortOrder = 100,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new Size
                {
                    SizeCode = "CUSTOM",
                    SizeName = "客製",
                    Description = "客製化尺寸",
                    SortOrder = 200,
                    IsActive = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                }
            };

            await context.Sizes.AddRangeAsync(sizes);
            await context.SaveChangesAsync();
        }
    }
}
