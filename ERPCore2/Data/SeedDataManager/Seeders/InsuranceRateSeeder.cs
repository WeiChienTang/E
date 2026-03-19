using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 保費費率種子資料 — 114年（2025年）費率
    /// 勞保: 員工 2%, 雇主 10%
    /// 健保: 員工 2.35%, 雇主 6.11%
    /// 勞退強制: 雇主 6%
    /// 餐費/交通免稅上限: 各 3,000 元
    /// </summary>
    public class InsuranceRateSeeder : IDataSeeder
    {
        public int Order => 51; // 在 PayrollItemSeeder 之後
        public string Name => "保費費率";

        public async Task SeedAsync(AppDbContext context)
        {
            bool hasData = await context.InsuranceRates.AnyAsync();
            if (hasData) return;

            context.InsuranceRates.Add(new InsuranceRate
            {
                EffectiveDate = new DateOnly(2025, 1, 1),
                LaborInsuranceEmployeeRate = 0.02m,
                LaborInsuranceEmployerRate = 0.10m,
                HealthInsuranceEmployeeRate = 0.0235m,
                HealthInsuranceEmployerRate = 0.0611m,
                RetirementEmployerRate = 0.06m,
                MealTaxFreeLimit = 3000m,
                TransportTaxFreeLimit = 3000m,
                // 加班費倍率（勞基法法定值）
                OvertimeRate1 = 1.3333m,         // 平日加班前2hr：勞基法第24條第1項第1款 4/3
                OvertimeRate2 = 1.6667m,         // 平日加班後2hr：勞基法第24條第1項第2款 5/3
                RestDayRate1 = 1.3333m,          // 休息日加班前2hr：勞基法第24條第2項 4/3
                RestDayRate2 = 1.6667m,          // 休息日加班後2hr：勞基法第24條第2項 5/3
                NationalHolidayRate = 1.0000m,   // 國定假日加給：勞基法第39條，月薪已含假日薪，加給1倍
                Remarks = "114年度費率（2025年1月1日生效）"
            });

            await context.SaveChangesAsync();
        }
    }
}
