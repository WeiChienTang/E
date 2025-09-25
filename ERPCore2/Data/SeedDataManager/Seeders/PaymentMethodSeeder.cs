using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 付款方式種子器
    /// </summary>
    public class PaymentMethodSeeder : IDataSeeder
    {
        public int Order => 12; // 設定執行順序
        public string Name => "付款方式";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedPaymentMethodsAsync(context);
        }

        private static async Task SeedPaymentMethodsAsync(AppDbContext context)
        {
            if (await context.PaymentMethods.AnyAsync()) return;

            var (createdAt1, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);
            var (createdAt2, _) = SeedDataHelper.GetSystemCreateInfo(25);
            var (createdAt3, _) = SeedDataHelper.GetSystemCreateInfo(20);
            var (createdAt4, _) = SeedDataHelper.GetSystemCreateInfo(15);
            var (createdAt5, _) = SeedDataHelper.GetSystemCreateInfo(10);

            var paymentMethods = new[]
            {
                new PaymentMethod
                {
                    Name = "現金",
                    IsDefault = true, // 現金設為預設付款方式
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt1,
                    CreatedBy = createdBy
                },
                new PaymentMethod
                {
                    Name = "信用卡",
                    IsDefault = false,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt2,
                    CreatedBy = createdBy
                },
                new PaymentMethod
                {
                    Name = "銀行轉帳",
                    IsDefault = false,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt3,
                    CreatedBy = createdBy
                },
                new PaymentMethod
                {
                    Name = "月結",
                    IsDefault = false,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt4,
                    CreatedBy = createdBy
                },
                new PaymentMethod
                {
                    Name = "支票",
                    IsDefault = false,
                    Status = EntityStatus.Active,
                    CreatedAt = createdAt5,
                    CreatedBy = createdBy
                }
            };

            await context.PaymentMethods.AddRangeAsync(paymentMethods);
            await context.SaveChangesAsync();
        }
    }
}