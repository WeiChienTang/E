using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 預收付款項類型種子器
    /// </summary>
    public class PrepaymentTypeSeeder : IDataSeeder
    {
        public int Order => 10;
        public string Name => "預收付款項類型";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.PrepaymentTypes.AnyAsync())
                return;

            var (createdAt, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);

            var prepaymentTypes = new[]
            {
                new Entities.PrepaymentType
                {
                    Name = "預收",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Remarks = "預先收取客戶的款項（預收款）"
                },
                new Entities.PrepaymentType
                {
                    Name = "預收轉沖款",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Remarks = "預收款項轉為沖款使用"
                },
                new Entities.PrepaymentType
                {
                    Name = "預付",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Remarks = "預先支付給供應商的款項"
                },
                new Entities.PrepaymentType
                {
                    Name = "預付轉沖款",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Remarks = "預付款項轉為沖款使用"
                },
                new Entities.PrepaymentType
                {
                    Name = "其他",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy,
                    Remarks = "其他類型的預收付款項"
                }
            };

            await context.PrepaymentTypes.AddRangeAsync(prepaymentTypes);
            await context.SaveChangesAsync();
        }
    }
}
