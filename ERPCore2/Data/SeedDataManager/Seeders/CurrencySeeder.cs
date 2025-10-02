using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    public class CurrencySeeder : IDataSeeder
    {
        public int Order => 50; // 放在 Financial Management 相關 Seeder 之後
        public string Name => "貨幣資料";

        public async Task SeedAsync(AppDbContext context)
        {
            if (await context.Set<Currency>().AnyAsync()) return;

            var (createdAt, createdBy) = SeedDataHelper.GetSystemCreateInfo(30);

            var currencies = new[]
            {
                new Currency { Code = "TWD", Name = "新台幣", Symbol = "NT$", IsBaseCurrency = true, ExchangeRate = 1m, Status = EntityStatus.Active, CreatedAt = createdAt, CreatedBy = createdBy },
                new Currency { Code = "USD", Name = "美元", Symbol = "$", IsBaseCurrency = false, ExchangeRate = 30m, Status = EntityStatus.Active, CreatedAt = createdAt, CreatedBy = createdBy },
                new Currency { Code = "JPY", Name = "日圓", Symbol = "¥", IsBaseCurrency = false, ExchangeRate = 3.5m, Status = EntityStatus.Active, CreatedAt = createdAt, CreatedBy = createdBy },
                new Currency { Code = "CNY", Name = "人民幣", Symbol = "¥", IsBaseCurrency = false, ExchangeRate = 4.5m, Status = EntityStatus.Active, CreatedAt = createdAt, CreatedBy = createdBy }
            };

            await context.Set<Currency>().AddRangeAsync(currencies);
            await context.SaveChangesAsync();
        }
    }
}
