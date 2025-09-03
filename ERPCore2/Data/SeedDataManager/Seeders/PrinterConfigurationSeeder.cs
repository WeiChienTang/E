using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 印表機配置資料種子器
    /// </summary>
    public class PrinterConfigurationSeeder : IDataSeeder
    {
        public int Order => 8; // 在紙張設定之後執行
        public string Name => "印表機配置資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedPrinterConfigurationsAsync(context);
        }

        private static async Task SeedPrinterConfigurationsAsync(AppDbContext context)
        {
            if (await context.PrinterConfigurations.AnyAsync()) return;

            var (createdAt, createdBy) = SeedDataHelper.GetSystemCreateInfo(32);

            var printerConfigurations = new[]
            {
                new PrinterConfiguration
                {
                    Name = "系統預設印表機",
                    ConnectionType = PrinterConnectionType.USB,
                    Remarks = "系統預設印表機設定，使用作業系統預設印表機",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt,
                    CreatedBy = createdBy
                },
            };

            await context.PrinterConfigurations.AddRangeAsync(printerConfigurations);
            await context.SaveChangesAsync();
        }
    }
}
