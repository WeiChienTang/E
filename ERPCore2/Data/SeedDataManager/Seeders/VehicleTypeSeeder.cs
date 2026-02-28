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

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(0);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(0);

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
                    Code = "TRAILER",
                    Name = "拖車",
                    Description = "拖車或半拖車",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                }
            };

            await context.VehicleTypes.AddRangeAsync(vehicleTypes);
            await context.SaveChangesAsync();
        }
    }
}
