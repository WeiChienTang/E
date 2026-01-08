using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 角色種子器
    /// </summary>
    public class RoleSeeder : IDataSeeder
    {
        public int Order => 1;
        public string Name => "角色管理";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedRolesAsync(context);
        }

        /// <summary>
        /// 初始化角色資料
        /// </summary>
        private static async Task SeedRolesAsync(AppDbContext context)
        {
            if (await context.Roles.AnyAsync())
                return;

            var roles = new[]
            {
                new Role { Code = "ADMIN", Name = "管理員", Remarks = "系統管理員，擁有所有權限", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System"},
                new Role { Code = "EMPLOYEE", Name = "一般員工", Remarks = "系統一般員工，具備基本業務操作權限", Status = EntityStatus.Active, CreatedAt = DateTime.Now},
            };

            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }
    }
}
