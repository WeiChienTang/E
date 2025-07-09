using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 員工職位資料種子器
    /// </summary>
    public class EmployeePositionSeeder : IDataSeeder
    {
        public int Order => 2; // 在基礎資料之後，員工資料之前執行
        public string Name => "員工職位資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedEmployeePositionsAsync(context);
        }

        /// <summary>
        /// 初始化員工職位資料
        /// </summary>
        private static async Task SeedEmployeePositionsAsync(AppDbContext context)
        {
            // 檢查資料是否已存在
            if (await context.EmployeePositions.AnyAsync())
                return;

            // 取得建立時間和建立者資訊
            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(28);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(26);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(24);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(22);
            var (createdAt6, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt7, _) = SeedDataHelper.GetSystemCreateInfo(18);
            var (createdAt8, _) = SeedDataHelper.GetSystemCreateInfo(16);

            var positions = new[]
            {
                new EmployeePosition
                {
                    Name = "總經理",
                    Code = "CEO",
                    Description = "公司最高執行主管",
                    Level = 10,
                    SortOrder = 1,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new EmployeePosition
                {
                    Name = "副總經理",
                    Code = "VP",
                    Description = "協助總經理管理公司業務",
                    Level = 9,
                    SortOrder = 2,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new EmployeePosition
                {
                    Name = "部門經理",
                    Code = "DM",
                    Description = "負責部門整體管理",
                    Level = 8,
                    SortOrder = 3,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new EmployeePosition
                {
                    Name = "專案經理",
                    Code = "PM",
                    Description = "負責專案規劃與執行",
                    Level = 7,
                    SortOrder = 4,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                },
                new EmployeePosition
                {
                    Name = "資深工程師",
                    Code = "SE",
                    Description = "具備豐富經驗的技術人員",
                    Level = 6,
                    SortOrder = 5,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                    CreatedBy = createdBy
                },
                new EmployeePosition
                {
                    Name = "工程師",
                    Code = "ENG",
                    Description = "負責技術開發與維護",
                    Level = 5,
                    SortOrder = 6,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6,
                    CreatedBy = createdBy
                },
                new EmployeePosition
                {
                    Name = "初級工程師",
                    Code = "JE",
                    Description = "入門級技術人員",
                    Level = 4,
                    SortOrder = 7,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt7,
                    CreatedBy = createdBy
                },
                new EmployeePosition
                {
                    Name = "實習生",
                    Code = "INT",
                    Description = "學習階段的實習人員",
                    Level = 1,
                    SortOrder = 8,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt8,
                    CreatedBy = createdBy
                }
            };

            // 批次新增資料
            await context.EmployeePositions.AddRangeAsync(positions);
            await context.SaveChangesAsync();
        }
    }
}
