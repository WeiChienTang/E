using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class DepartmentSeeder : IDataSeeder
    {
        public int Order => 9; // 設定執行順序
        public string Name => "部門資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedDepartmentsAsync(context);
        }

        private static async Task SeedDepartmentsAsync(AppDbContext context)
        {
            if (await context.Departments.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(35);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(28);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt6, _) = SeedDataHelper.GetSystemCreateInfo(15);

            var departments = new[]
            {
                // 頂級部門
                new Department
                {
                    DepartmentCode = "HQ",
                    Name = "總公司",
                    Description = "總公司管理部門",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "ADMIN",
                    Name = "行政部",
                    Description = "行政管理部門",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "FINANCE",
                    Name = "財務部",
                    Description = "財務管理部門",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "SALES",
                    Name = "業務部",
                    Description = "業務推廣部門",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "PROD",
                    Name = "生產部",
                    Description = "生產製造部門",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "IT",
                    Name = "資訊部",
                    Description = "資訊科技部門",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6,
                    CreatedBy = createdBy
                }
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }
    }
}
