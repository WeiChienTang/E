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
        public int Order => 7; // 設定執行順序
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
                    ParentDepartmentId = null,
                    SortOrder = 1,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "ADMIN",
                    Name = "行政部",
                    Description = "行政管理部門",
                    ParentDepartmentId = null,
                    SortOrder = 2,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "FINANCE",
                    Name = "財務部",
                    Description = "財務管理部門",
                    ParentDepartmentId = null,
                    SortOrder = 3,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "SALES",
                    Name = "業務部",
                    Description = "業務推廣部門",
                    ParentDepartmentId = null,
                    SortOrder = 4,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "PROD",
                    Name = "生產部",
                    Description = "生產製造部門",
                    ParentDepartmentId = null,
                    SortOrder = 5,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "IT",
                    Name = "資訊部",
                    Description = "資訊科技部門",
                    ParentDepartmentId = null,
                    SortOrder = 6,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6,
                    CreatedBy = createdBy
                }
            };

            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();

            // 取得剛建立的部門 ID，用於建立下級部門
            var adminDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "ADMIN");
            var salesDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "SALES");
            var prodDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "PROD");

            // 建立子部門
            var (createdAt7, _) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt8, _) = SeedDataHelper.GetSystemCreateInfo(8);
            var (createdAt9, _) = SeedDataHelper.GetSystemCreateInfo(5);

            var subDepartments = new[]
            {
                // 行政部下級部門
                new Department
                {
                    DepartmentCode = "HR",
                    Name = "人事部",
                    Description = "人力資源管理",
                    ParentDepartmentId = adminDept.Id,
                    SortOrder = 1,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt7,
                    CreatedBy = createdBy
                },
                // 業務部下級部門
                new Department
                {
                    DepartmentCode = "SALES-DOMESTIC",
                    Name = "國內業務",
                    Description = "國內市場業務",
                    ParentDepartmentId = salesDept.Id,
                    SortOrder = 1,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt8,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "SALES-EXPORT",
                    Name = "出口業務",
                    Description = "出口貿易業務",
                    ParentDepartmentId = salesDept.Id,
                    SortOrder = 2,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt8,
                    CreatedBy = createdBy
                },
                // 生產部下級部門
                new Department
                {
                    DepartmentCode = "QC",
                    Name = "品管部",
                    Description = "品質管制部門",
                    ParentDepartmentId = prodDept.Id,
                    SortOrder = 1,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt9,
                    CreatedBy = createdBy
                },
                new Department
                {
                    DepartmentCode = "WAREHOUSE",
                    Name = "倉儲部",
                    Description = "倉儲管理部門",
                    ParentDepartmentId = prodDept.Id,
                    SortOrder = 2,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt9,
                    CreatedBy = createdBy
                }
            };

            await context.Departments.AddRangeAsync(subDepartments);
            await context.SaveChangesAsync();
        }
    }
}
