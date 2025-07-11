using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
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
                    Category = "金屬材料",
                    Density = 7.93m,
                    MeltingPoint = 1400m,
                    IsEcoFriendly = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new Material
                {
                    Code = "MAT002",
                    Name = "鋁合金 6061",
                    Description = "輕量化鋁合金，適用於結構件",
                    Category = "金屬材料",
                    Density = 2.70m,
                    MeltingPoint = 582m,
                    IsEcoFriendly = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new Material
                {
                    Code = "MAT003",
                    Name = "聚乙烯 PE",
                    Description = "高密度聚乙烯，化學穩定性佳",
                    Category = "塑膠材料",
                    Density = 0.95m,
                    MeltingPoint = 130m,
                    IsEcoFriendly = false,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new Material
                {
                    Code = "MAT004",
                    Name = "碳纖維",
                    Description = "高強度碳纖維複合材料",
                    Category = "複合材料",
                    Density = 1.60m,
                    MeltingPoint = null, // 碳纖維不熔化
                    IsEcoFriendly = false,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                },
                new Material
                {
                    Code = "MAT005",
                    Name = "橡膠 EPDM",
                    Description = "三元乙丙橡膠，耐候性優異",
                    Category = "橡膠材料",
                    Density = 1.35m,
                    MeltingPoint = null, // 橡膠為熱固性材料
                    IsEcoFriendly = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                    CreatedBy = createdBy
                },
                new Material
                {
                    Code = "MAT006",
                    Name = "玻璃纖維",
                    Description = "增強用玻璃纖維，提高材料強度",
                    Category = "纖維材料",
                    Density = 2.55m,
                    MeltingPoint = 1700m,
                    IsEcoFriendly = true,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6,
                    CreatedBy = createdBy
                }
            };

            await context.Materials.AddRangeAsync(materials);
            await context.SaveChangesAsync();
        }
    }
}
