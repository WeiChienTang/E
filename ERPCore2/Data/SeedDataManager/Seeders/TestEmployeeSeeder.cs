using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using static ERPCore2.Data.Entities.Employee;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 測試用員工資料種子器（僅測試環境使用）
    /// </summary>
    public class TestEmployeeSeeder : IDataSeeder
    {
        public int Order => 52;
        public string Name => "測試員工資料";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.Employees.AnyAsync(e => e.Code != null && e.Code.StartsWith("EMP")))
                return;

            // 取得基本角色（一般員工）
            var staffRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "一般使用者")
                         ?? await context.Roles.FirstOrDefaultAsync(r => r.Name != "管理員");

            var employees = new List<Employee>
            {
                new Employee
                {
                    Code = "EMP001",
                    Name = "張志明",
                    Account = "emp001",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1985, 3, 15),
                    Mobile = "0912-001-001",
                    Email = "zhangzhiming@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2020, 1, 15),
                    JobTitle = "業務專員",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP002",
                    Name = "李雅婷",
                    Account = "emp002",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1990, 7, 22),
                    Mobile = "0923-002-002",
                    Email = "liyating@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2021, 3, 1),
                    JobTitle = "會計專員",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP003",
                    Name = "陳建宏",
                    Account = "emp003",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1988, 11, 5),
                    Mobile = "0934-003-003",
                    Email = "chenjianhong@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2019, 6, 15),
                    JobTitle = "倉管人員",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP004",
                    Name = "林淑芬",
                    Account = "emp004",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1992, 4, 18),
                    Mobile = "0945-004-004",
                    Email = "linshufen@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2022, 2, 1),
                    JobTitle = "採購專員",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP005",
                    Name = "黃俊傑",
                    Account = "emp005",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1983, 9, 30),
                    Mobile = "0956-005-005",
                    Email = "huangjunjie@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2018, 8, 1),
                    JobTitle = "業務副理",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP006",
                    Name = "吳曉玲",
                    Account = "emp006",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1995, 1, 12),
                    Mobile = "0967-006-006",
                    Email = "wuxiaoling@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Probation,
                    HireDate = new DateTime(2026, 1, 1),
                    JobTitle = "人資專員",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP007",
                    Name = "鄭文凱",
                    Account = "emp007",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1987, 6, 25),
                    Mobile = "0978-007-007",
                    Email = "zhengwenkai@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2017, 5, 1),
                    JobTitle = "生產主管",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP008",
                    Name = "許美琴",
                    Account = "emp008",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1991, 8, 14),
                    Mobile = "0989-008-008",
                    Email = "xumeiqin@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2020, 9, 1),
                    JobTitle = "財務專員",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP009",
                    Name = "謝宗翰",
                    Account = "emp009",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1986, 12, 3),
                    Mobile = "0900-009-009",
                    Email = "xiezonghan@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2016, 4, 1),
                    JobTitle = "品管主管",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP010",
                    Name = "施怡君",
                    Account = "emp010",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1993, 2, 28),
                    Mobile = "0911-010-010",
                    Email = "shiyijun@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2021, 7, 1),
                    JobTitle = "客服專員",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP011",
                    Name = "廖志豪",
                    Account = "emp011",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1984, 5, 17),
                    Mobile = "0922-011-011",
                    Email = "liaozhihao@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2015, 11, 1),
                    JobTitle = "業務經理",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP012",
                    Name = "鍾雅雯",
                    Account = "emp012",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1994, 10, 8),
                    Mobile = "0933-012-012",
                    Email = "zhongyanwen@company.com.tw",
                    EmployeeType = EmployeeType.PartTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2023, 3, 1),
                    JobTitle = "行政助理",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP013",
                    Name = "楊明哲",
                    Account = "emp013",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1989, 7, 21),
                    Mobile = "0944-013-013",
                    Email = "yangmingzhe@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2019, 10, 1),
                    JobTitle = "工程師",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP014",
                    Name = "洪秀蘭",
                    Account = "emp014",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1996, 3, 5),
                    Mobile = "0955-014-014",
                    Email = "hongxiulan@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2023, 6, 1),
                    JobTitle = "業務助理",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP015",
                    Name = "賴俊雄",
                    Account = "emp015",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1982, 1, 30),
                    Mobile = "0966-015-015",
                    Email = "laijunxiong@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2014, 2, 1),
                    JobTitle = "廠長",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP016",
                    Name = "蘇佳穎",
                    Account = "emp016",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1997, 9, 16),
                    Mobile = "0977-016-016",
                    Email = "sujiaying@company.com.tw",
                    EmployeeType = EmployeeType.Contract,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2025, 9, 1),
                    JobTitle = "設計師",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP017",
                    Name = "蔡文彬",
                    Account = "emp017",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1981, 4, 11),
                    Mobile = "0988-017-017",
                    Email = "caiwnbin@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2013, 8, 1),
                    JobTitle = "技術總監",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP018",
                    Name = "何宜臻",
                    Account = "emp018",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1998, 6, 27),
                    Mobile = "0999-018-018",
                    Email = "heyizhen@company.com.tw",
                    EmployeeType = EmployeeType.Intern,
                    EmploymentStatus = EmployeeStatus.Probation,
                    HireDate = new DateTime(2026, 2, 1),
                    JobTitle = "實習生",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP019",
                    Name = "曾志遠",
                    Account = "emp019",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1979, 12, 19),
                    Mobile = "0910-019-019",
                    Email = "zengzhiyuan@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2010, 5, 1),
                    JobTitle = "副總經理",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new Employee
                {
                    Code = "EMP020",
                    Name = "顏佩琪",
                    Account = "emp020",
                    Password = SeedDataHelper.HashPassword("test1234"),
                    IsSystemUser = true,
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1993, 8, 9),
                    Mobile = "0921-020-020",
                    Email = "yanpeiqi@company.com.tw",
                    EmployeeType = EmployeeType.FullTime,
                    EmploymentStatus = EmployeeStatus.Active,
                    HireDate = new DateTime(2022, 11, 1),
                    JobTitle = "倉管專員",
                    RoleId = staffRole?.Id,
                    Status = EntityStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
            };

            await context.Employees.AddRangeAsync(employees);
            await context.SaveChangesAsync();
        }
    }
}
