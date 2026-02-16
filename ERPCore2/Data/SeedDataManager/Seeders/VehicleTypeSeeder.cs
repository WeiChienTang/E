using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 車型種子資料
    /// </summary>
    public class VehicleTypeSeeder : IDataSeeder
    {
        public int Order => 13;
        public string Name => "車型";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedVehicleTypesAsync(context);
        }

        private static async Task SeedVehicleTypesAsync(AppDbContext context)
        {
            if (await context.VehicleTypes.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(15);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt6, _) = SeedDataHelper.GetSystemCreateInfo(5);

            var vehicleTypes = new[]
            {
                new VehicleType
                {
                    Code = "TRUCK",
                    Name = "貨車",
                    Description = "一般貨運車輛",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                },
                new VehicleType
                {
                    Code = "VAN",
                    Name = "廂型車",
                    Description = "封閉式廂型貨車",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                },
                new VehicleType
                {
                    Code = "SEDAN",
                    Name = "小客車",
                    Description = "一般乘用小客車",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                },
                new VehicleType
                {
                    Code = "MOTORCYCLE",
                    Name = "機車",
                    Description = "機車或摩托車",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                },
                new VehicleType
                {
                    Code = "REFRIGERATED",
                    Name = "冷藏車",
                    Description = "具備冷藏設備的貨車",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                },
                new VehicleType
                {
                    Code = "TRAILER",
                    Name = "拖車",
                    Description = "拖車或半拖車",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6,
                }
            };

            await context.VehicleTypes.AddRangeAsync(vehicleTypes);
            await context.SaveChangesAsync();
        }
    }
}
