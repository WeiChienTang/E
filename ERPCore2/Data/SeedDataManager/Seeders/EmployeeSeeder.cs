using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 員工資料種子器
    /// </summary>
    public class EmployeeSeeder : IDataSeeder
    {
        public int Order => 3; // 在角色權限關聯之後執行
        public string Name => "員工資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedDefaultAdminAsync(context);
            await SeedDefaultEmployeesAsync(context);
        }

        /// <summary>
        /// 建立預設系統管理員帳號
        /// </summary>
        private static async Task SeedDefaultAdminAsync(AppDbContext context)
        {
            // 檢查是否已有 ADMIN001 員工
            if (await context.Employees.AnyAsync(e => e.Code == "ADMIN001"))
                return;

            // 取得系統管理員角色 - 修正角色名稱
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "管理員");

            // 建立系統管理員帳號
            var adminEmployee = new Employee
            {
                Code = "ADMIN001",
                FirstName = "系統",
                LastName = "管理員",
                Account = "admin",
                Password = SeedDataHelper.HashPassword("1234"),
                IsSystemUser = true, // 設置為系統使用者
                DepartmentId = null, // 先設為 null，部門建立後再更新
                RoleId = adminRole?.Id ?? 1,
                Status = EntityStatus.Active,
                CreatedAt = DateTime.Now,
                CreatedBy = "System"
            };

            await context.Employees.AddAsync(adminEmployee);
            await context.SaveChangesAsync();

            // 聯絡資料將由 ContactSeeder 統一處理
        }

        /// <summary>
        /// 建立預設一般員工帳號
        /// </summary>
        private static async Task SeedDefaultEmployeesAsync(AppDbContext context)
        {
            // 定義要建立的員工資料
            var defaultEmployees = new[]
            {
                new { Code = "EMP001", FirstName = "測試01", LastName = "測試用員工", Account = "tt", RoleName = "辦公室員工" },
                new { Code = "EMP002", FirstName = "測試02", LastName = "一般員工", Account = "tt2", RoleName = "員工" },
            };

            foreach (var emp in defaultEmployees)
            {
                // 檢查員工是否已存在
                if (await context.Employees.AnyAsync(e => e.Code == emp.Code))
                    continue;

                // 取得角色
                var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == emp.RoleName);

                // 建立員工
                var employee = new Employee
                {
                    Code = emp.Code,
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    Account = emp.Account,
                    Password = SeedDataHelper.HashPassword("1234"), // 預設密碼
                    IsSystemUser = true,
                    DepartmentId = null, // 可在之後設定部門
                    RoleId = role?.Id ?? 3, // 預設為 員工 角色 (第三個角色)
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                };

                await context.Employees.AddAsync(employee);
                await context.SaveChangesAsync();

                // 為員工添加Email聯絡資料
                // 聯絡資料將由 ContactSeeder 統一處理
            }
        }
    }
}
