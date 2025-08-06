using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 材質種子器
    /// </summary>
    public class MaterialSeeder : IDataSeeder
    {
        public int Order => 9; // 在基礎資料之後執行
        public string Name => "材質資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedMaterialsAsync(context);
        }

        /// <summary>
        /// 初始化材質資料
        /// </summary>
        private static async Task SeedMaterialsAsync(AppDbContext context)
        {
            // 檢查資料是否已存在
            if (await context.Materials.AnyAsync())
                return;

            // 取得建立時間和建立者資訊
            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(15);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt6, _) = SeedDataHelper.GetSystemCreateInfo(5);

            var materials = new[]
            {
                new Material
                {
                    Code = "MAT001",
                    Name = "不鏽鋼 304",
                    Description = "食品級不鏽鋼，耐腐蝕性佳",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
            };

            await context.Materials.AddRangeAsync(materials);
            await context.SaveChangesAsync();
        }
    }
}
