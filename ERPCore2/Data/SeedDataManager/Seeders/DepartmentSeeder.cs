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

            var currentTime = DateTime.UtcNow;
            var (_, createdBy) = SeedDataHelper.GetSystemCreateInfo(0);

            var departments = new[]
            {
                new Department
                {
                    DepartmentCode = "ADMIN",
                    Name = "行政部",
                    Description = "行政管理部門",
                    Status = EntityStatus.Active,
                    CreatedAt = currentTime,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "FINANCE",
                    Name = "財務部",
                    Description = "財務管理部門",
                    Status = EntityStatus.Active,
                    CreatedAt = currentTime,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "SALES",
                    Name = "業務部",
                    Description = "業務推廣部門",
                    Status = EntityStatus.Active,
                    CreatedAt = currentTime,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "PROD",
                    Name = "生產部",
                    Description = "生產製造部門",
                    Status = EntityStatus.Active,
                    CreatedAt = currentTime,
                    CreatedBy = createdBy
                },
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }
    }
}
