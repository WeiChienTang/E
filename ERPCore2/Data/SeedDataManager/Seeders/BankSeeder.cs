using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 銀行別種子器
    /// </summary>
    public class BankSeeder : IDataSeeder
    {
        public int Order => 13; // 設定執行順序
        public string Name => "銀行別";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedBanksAsync(context);
        }

        private static async Task SeedBanksAsync(AppDbContext context)
        {
            if (await context.Banks.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(15);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(10);
            var (createdAt6, _) = SeedDataHelper.GetSystemCreateInfo(5);

            var banks = new[]
            {
                new Bank
                {
                    BankName = "台灣銀行",
                    BankNameEn = "Bank of Taiwan",
                    SwiftCode = "BKTWTWTP",
                    Phone = "02-2349-3456",
                    Address = "台北市中正區重慶南路一段120號",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new Bank
                {
                    BankName = "台灣土地銀行",
                    BankNameEn = "Land Bank of Taiwan",
                    SwiftCode = "LBOTTWTP",
                    Phone = "02-2348-3456",
                    Address = "台北市中正區館前路46號",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new Bank
                {
                    BankName = "合作金庫商業銀行",
                    BankNameEn = "Taiwan Cooperative Bank",
                    SwiftCode = "TACBTWTP",
                    Phone = "02-2311-8811",
                    Address = "台北市松山區長安東路二段225號",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new Bank
                {
                    BankName = "第一商業銀行",
                    BankNameEn = "First Commercial Bank",
                    SwiftCode = "FCBKTWTP",
                    Phone = "02-2348-1111",
                    Address = "台北市中正區重慶南路一段30號",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                },
                new Bank
                {
                    BankName = "華南商業銀行",
                    BankNameEn = "Hua Nan Commercial Bank",
                    SwiftCode = "HNBKTWTP",
                    Phone = "02-2371-3111",
                    Address = "台北市信義區松仁路123號",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                    CreatedBy = createdBy
                },
                new Bank
                {
                    BankName = "彰化商業銀行",
                    BankNameEn = "Chang Hwa Commercial Bank",
                    SwiftCode = "CCBCTWTP",
                    Phone = "04-722-2001",
                    Address = "台北市中山區中山北路二段57號",
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt6,
                    CreatedBy = createdBy
                }
            };

            await context.Banks.AddRangeAsync(banks);
            await context.SaveChangesAsync();
        }
    }
}
