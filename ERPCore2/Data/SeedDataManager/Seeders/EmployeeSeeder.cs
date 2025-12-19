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
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "管理員");

            // 建立系統管理員帳號
            var adminEmployee = new Employee
            {
                Code = "ADMIN001",
                Name = "系統管理員",
                Account = "admin",
                Password = SeedDataHelper.HashPassword("hdcomp1106"),
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
    }
}
