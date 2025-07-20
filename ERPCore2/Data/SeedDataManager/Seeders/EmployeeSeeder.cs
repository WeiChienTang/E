using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Helpers;
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
            if (await context.Employees.AnyAsync(e => e.EmployeeCode == "ADMIN001"))
                return;

            // 取得系統管理員角色
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Administrator");

            // 建立系統管理員帳號
            var adminEmployee = new Employee
            {
                EmployeeCode = "ADMIN001",
                FirstName = "系統",
                LastName = "管理員",
                Account = "admin",
                PasswordHash = SeedDataHelper.HashPassword("admin123"),
                IsSystemUser = true, // 設置為系統使用者
                DepartmentId = null, // 先設為 null，部門建立後再更新
                RoleId = adminRole?.Id ?? 1,
                Status = EntityStatus.Active,
                CreatedAt = DateTime.Now,
                CreatedBy = "System"
            };

            await context.Employees.AddAsync(adminEmployee);
            await context.SaveChangesAsync();

            // 為系統管理員添加Email聯絡資料
            await SeedEmployeeContactAsync(context, adminEmployee);
        }

        /// <summary>
        /// 為員工添加聯絡資料
        /// </summary>
        private static async Task SeedEmployeeContactAsync(AppDbContext context, Employee employee)
        {
            var emailContactType = await context.ContactTypes
                .FirstOrDefaultAsync(ct => ct.TypeName == "Email");

            if (emailContactType != null)
            {
                var emailContact = new EmployeeContact
                {
                    EmployeeId = employee.Id,
                    ContactTypeId = emailContactType.Id,
                    ContactValue = $"{employee.Account}@erpcore2.com",
                    IsPrimary = true,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "System"
                };

                await context.EmployeeContacts.AddAsync(emailContact);
                await context.SaveChangesAsync();
            }
        }
    }
}
