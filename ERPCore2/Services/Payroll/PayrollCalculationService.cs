using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.Payroll
{
    /// <summary>
    /// 薪資計算引擎
    /// Phase 1：依 EmployeeSalary 設定 + PayrollRecord 出勤欄位計算
    /// 費率固定寫於常數（Phase 2 改為從 DB 費率表讀取）
    /// </summary>
    public class PayrollCalculationService : IPayrollCalculationService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<PayrollCalculationService>? _logger;

        // ── 費率備援常數（InsuranceRate 資料表無資料時使用）───────────
        private const decimal DefaultLaborInsuranceEmployeeRate  = 0.02m;
        private const decimal DefaultLaborInsuranceEmployerRate  = 0.10m;
        private const decimal DefaultHealthInsuranceEmployeeRate = 0.0235m;
        private const decimal DefaultHealthInsuranceEmployerRate = 0.0611m;
        private const decimal DefaultRetirementEmployerRate      = 0.06m;
        private const decimal DefaultMealTaxFreeLimit            = 3000m;
        private const decimal DefaultTransportTaxFreeLimit       = 3000m;

        // 加班費倍率備援常數（勞基法法定值，DB 欄位為 0 時使用）
        private const decimal DefaultOvertimeRate1      = 4m / 3m;  // 1.3333...（平日前2hr）
        private const decimal DefaultOvertimeRate2      = 5m / 3m;  // 1.6667...（平日後2hr）
        private const decimal DefaultRestDayRate1       = 4m / 3m;  // 1.3333...（休息日前2hr）
        private const decimal DefaultRestDayRate2       = 5m / 3m;  // 1.6667...（休息日後2hr）
        private const decimal DefaultNationalHolidayRate = 1.0m;    // 1.0000（國定假日加給1倍）

        public PayrollCalculationService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<PayrollCalculationService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        // ────────────────────────────────────────────────────────────
        // 公開方法
        // ────────────────────────────────────────────────────────────

        public async Task<ServiceResult<PayrollRecord>> CalculateEmployeeAsync(
            int employeeId, int periodId, string? calculatedBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var period = await context.PayrollPeriods.FindAsync(periodId);
                if (period == null)
                    return ServiceResult<PayrollRecord>.Failure("找不到指定的薪資週期");

                if (period.PeriodStatus == PayrollPeriodStatus.Closed)
                    return ServiceResult<PayrollRecord>.Failure("此薪資週期已關帳，不可重新計算");

                var salary = await GetCurrentSalaryAsync(context, employeeId, period);
                if (salary == null)
                    return ServiceResult<PayrollRecord>.Failure("找不到員工的有效薪資設定，請先設定薪資");

                // 取得或建立薪資單
                var record = await context.PayrollRecords
                    .Include(r => r.Details)
                    .FirstOrDefaultAsync(r => r.EmployeeId == employeeId && r.PayrollPeriodId == periodId);

                bool isNew = record == null;
                if (isNew)
                {
                    int adYear = period.Year + 1911;
                    int workDays = GetWorkDaysInMonth(adYear, period.Month);
                    record = new PayrollRecord
                    {
                        PayrollPeriodId = periodId,
                        EmployeeId = employeeId,
                        ScheduledWorkDays = workDays,
                        ActualWorkDays = workDays,   // 預設全勤
                        RecordStatus = PayrollRecordStatus.Draft,
                        Code = $"{period.Year:D3}{period.Month:D2}-{employeeId}",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = calculatedBy ?? "System"
                    };
                    context.PayrollRecords.Add(record);
                }
                else
                {
                    if (record!.RecordStatus == PayrollRecordStatus.Confirmed)
                        return ServiceResult<PayrollRecord>.Failure("薪資單已確認，請先取消確認再重算");

                    // 清除舊明細
                    context.PayrollRecordDetails.RemoveRange(record.Details);
                    record.Details.Clear();
                }

                // ── 從出勤彙總同步出勤資料（若有設定）──────────────────────
                var attendance = await context.MonthlyAttendanceSummaries
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                          && a.Year == period.Year
                                          && a.Month == period.Month);

                if (attendance != null)
                {
                    record.ScheduledWorkDays = attendance.ScheduledWorkDays;
                    record.ActualWorkDays = attendance.ActualWorkDays;
                    record.AbsentDays = attendance.AbsentDays;
                    record.SickLeaveDays = attendance.SickLeaveDays;
                    record.OvertimeHours1 = attendance.OvertimeHours1;
                    record.OvertimeHours2 = attendance.OvertimeHours2;
                    record.HolidayOvertimeHours = attendance.HolidayOvertimeHours;
                    record.NationalHolidayHours = attendance.NationalHolidayHours;
                    record.TotalWorkHours = attendance.TotalWorkHours;
                }

                var items = await context.PayrollItems
                    .Where(i => i.Status == EntityStatus.Active)
                    .ToListAsync();

                await DoCalculateAsync(context, record, salary, period, items);

                record.UpdatedAt = DateTime.UtcNow;
                record.UpdatedBy = calculatedBy ?? "System";
                record.CalculatedAt = DateTime.UtcNow;
                record.CalculatedBy = calculatedBy ?? "System";

                await context.SaveChangesAsync();

                // 重新載入含明細的完整資料
                var result = await context.PayrollRecords
                    .Include(r => r.Employee)
                    .Include(r => r.Period)
                    .Include(r => r.Details).ThenInclude(d => d.Item)
                    .FirstAsync(r => r.Id == record.Id);

                return ServiceResult<PayrollRecord>.Success(result);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateEmployeeAsync), GetType(), _logger);
                return ServiceResult<PayrollRecord>.Failure("薪資計算時發生錯誤");
            }
        }

        public async Task<ServiceResult> CalculateAllAsync(int periodId, string? calculatedBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var period = await context.PayrollPeriods.FindAsync(periodId);
                if (period == null)
                    return ServiceResult.Failure("找不到指定的薪資週期");

                if (period.PeriodStatus == PayrollPeriodStatus.Closed)
                    return ServiceResult.Failure("此薪資週期已關帳，不可重新計算");

                // 取得所有在職員工
                var employees = await context.Employees
                    .Where(e => e.EmploymentStatus == EmployeeStatus.Active
                             || e.EmploymentStatus == EmployeeStatus.Probation)
                    .Select(e => e.Id)
                    .ToListAsync();

                var errors = new List<string>();
                int successCount = 0;

                foreach (var employeeId in employees)
                {
                    var r = await CalculateEmployeeAsync(employeeId, periodId, calculatedBy);
                    if (r.IsSuccess)
                        successCount++;
                    else
                        errors.Add($"員工 {employeeId}：{r.ErrorMessage}");
                }

                if (errors.Any())
                    return ServiceResult.Failure($"完成 {successCount} 筆，{errors.Count} 筆失敗：{string.Join("；", errors)}");

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CalculateAllAsync), GetType(), _logger);
                return ServiceResult.Failure("批次計算時發生錯誤");
            }
        }

        public async Task<ServiceResult<PayrollRecord>> RecalculateAsync(int recordId, string? calculatedBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var record = await context.PayrollRecords
                    .Include(r => r.Period)
                    .FirstOrDefaultAsync(r => r.Id == recordId);

                if (record == null)
                    return ServiceResult<PayrollRecord>.Failure("找不到指定的薪資單");

                if (record.Period.PeriodStatus == PayrollPeriodStatus.Closed)
                    return ServiceResult<PayrollRecord>.Failure("此薪資週期已關帳，不可重新計算");

                // 強制重置為 Draft，並先儲存至 DB（修正 Bug-P3）
                // CalculateEmployeeAsync 開啟獨立 DbContext，若不先 SaveChanges，
                // 新 context 仍從 DB 讀到 Confirmed 狀態而返回錯誤
                record.RecordStatus = PayrollRecordStatus.Draft;
                await context.SaveChangesAsync();

                return await CalculateEmployeeAsync(record.EmployeeId, record.PayrollPeriodId, calculatedBy);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(RecalculateAsync), GetType(), _logger);
                return ServiceResult<PayrollRecord>.Failure("重新計算時發生錯誤");
            }
        }

        public async Task<List<PayrollRecord>> GetByPeriodAsync(int periodId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PayrollRecords
                    .Include(r => r.Employee)
                    .Include(r => r.Details).ThenInclude(d => d.Item)
                    .Where(r => r.PayrollPeriodId == periodId)
                    .OrderBy(r => r.Employee.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByPeriodAsync), GetType(), _logger);
                return new List<PayrollRecord>();
            }
        }

        public async Task<PayrollRecord?> GetRecordAsync(int employeeId, int periodId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PayrollRecords
                    .Include(r => r.Employee)
                    .Include(r => r.Period)
                    .Include(r => r.Details).ThenInclude(d => d.Item)
                    .FirstOrDefaultAsync(r => r.EmployeeId == employeeId && r.PayrollPeriodId == periodId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetRecordAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<PayrollRecord?> GetByIdAsync(int recordId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PayrollRecords
                    .Include(r => r.Employee).ThenInclude(e => e.Department)
                    .Include(r => r.Period)
                    .Include(r => r.Details).ThenInclude(d => d.Item)
                    .FirstOrDefaultAsync(r => r.Id == recordId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<List<PayrollRecord>> GetByEmployeeAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.PayrollRecords
                    .Include(r => r.Period)
                    .Where(r => r.EmployeeId == employeeId)
                    .OrderByDescending(r => r.Period.Year)
                    .ThenByDescending(r => r.Period.Month)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeAsync), GetType(), _logger);
                return new List<PayrollRecord>();
            }
        }

        public async Task<ServiceResult> ConfirmRecordAsync(int recordId, string? confirmedBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var record = await context.PayrollRecords
                    .Include(r => r.Period)
                    .FirstOrDefaultAsync(r => r.Id == recordId);

                if (record == null)
                    return ServiceResult.Failure("找不到指定的薪資單");

                if (record.Period.PeriodStatus == PayrollPeriodStatus.Closed)
                    return ServiceResult.Failure("此薪資週期已關帳");

                if (record.RecordStatus == PayrollRecordStatus.Confirmed)
                    return ServiceResult.Failure("薪資單已是確認狀態");

                if (!record.CalculatedAt.HasValue)
                    return ServiceResult.Failure("薪資單尚未計算，無法確認");

                record.RecordStatus = PayrollRecordStatus.Confirmed;
                record.UpdatedAt = DateTime.UtcNow;
                record.UpdatedBy = confirmedBy ?? "System";
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ConfirmRecordAsync), GetType(), _logger);
                return ServiceResult.Failure("確認薪資單時發生錯誤");
            }
        }

        public async Task<ServiceResult> UnconfirmRecordAsync(int recordId, string? operatedBy = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var record = await context.PayrollRecords
                    .Include(r => r.Period)
                    .FirstOrDefaultAsync(r => r.Id == recordId);

                if (record == null)
                    return ServiceResult.Failure("找不到指定的薪資單");

                if (record.Period.PeriodStatus == PayrollPeriodStatus.Closed)
                    return ServiceResult.Failure("此薪資週期已關帳，不可取消確認");

                record.RecordStatus = PayrollRecordStatus.Draft;
                record.UpdatedAt = DateTime.UtcNow;
                record.UpdatedBy = operatedBy ?? "System";
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UnconfirmRecordAsync), GetType(), _logger);
                return ServiceResult.Failure("取消確認時發生錯誤");
            }
        }

        // ────────────────────────────────────────────────────────────
        // 核心計算邏輯（私有）
        // ────────────────────────────────────────────────────────────

        private async Task DoCalculateAsync(
            AppDbContext context,
            PayrollRecord record,
            EmployeeSalary salary,
            PayrollPeriod period,
            List<PayrollItem> items)
        {
            var details = new List<PayrollRecordDetail>();
            int adYear = period.Year + 1911;
            var periodDate = new DateOnly(adYear, period.Month, 1);

            // ── 0. 取得費率（優先從 DB InsuranceRate 讀取，無資料則使用備援常數）──
            var rate = await context.InsuranceRates
                .Where(r => r.EffectiveDate <= periodDate)
                .OrderByDescending(r => r.EffectiveDate)
                .FirstOrDefaultAsync();

            decimal laborInsuranceEmployeeRate  = rate?.LaborInsuranceEmployeeRate  ?? DefaultLaborInsuranceEmployeeRate;
            decimal laborInsuranceEmployerRate  = rate?.LaborInsuranceEmployerRate  ?? DefaultLaborInsuranceEmployerRate;
            decimal healthInsuranceEmployeeRate = rate?.HealthInsuranceEmployeeRate ?? DefaultHealthInsuranceEmployeeRate;
            decimal healthInsuranceEmployerRate = rate?.HealthInsuranceEmployerRate ?? DefaultHealthInsuranceEmployerRate;
            decimal retirementEmployerRate      = rate?.RetirementEmployerRate      ?? DefaultRetirementEmployerRate;
            decimal mealTaxFreeLimit            = rate?.MealTaxFreeLimit            ?? DefaultMealTaxFreeLimit;
            decimal transportTaxFreeLimit       = rate?.TransportTaxFreeLimit       ?? DefaultTransportTaxFreeLimit;

            // 加班費倍率：DB 值 > 0 時使用設定值，否則 fallback 至法定備援常數
            decimal ot1Rate      = (rate?.OvertimeRate1      > 0) ? rate!.OvertimeRate1      : DefaultOvertimeRate1;
            decimal ot2Rate      = (rate?.OvertimeRate2      > 0) ? rate!.OvertimeRate2      : DefaultOvertimeRate2;
            decimal restDay1Rate = (rate?.RestDayRate1       > 0) ? rate!.RestDayRate1       : DefaultRestDayRate1;
            decimal restDay2Rate = (rate?.RestDayRate2       > 0) ? rate!.RestDayRate2       : DefaultRestDayRate2;
            decimal natHolRate   = (rate?.NationalHolidayRate > 0) ? rate!.NationalHolidayRate : DefaultNationalHolidayRate;

            // ── 1. 工資基準（月薪 / 時薪分開計算）────────────────────
            decimal basePay;
            decimal hourlyRate;

            if (salary.SalaryType == SalaryType.Hourly)
            {
                // 時薪制：BaseSalary 即時薪，依實際總工時計算
                hourlyRate = salary.BaseSalary;
                basePay = Math.Round(record.TotalWorkHours * hourlyRate, 0);
                AddDetail(details, items, "BASE", record.TotalWorkHours, hourlyRate, basePay,
                    $"時薪 {hourlyRate:N0} × 實際工時 {record.TotalWorkHours:N2}h = {basePay:N0}");
            }
            else
            {
                // 月薪制：依出勤比例調整
                decimal dailyRate = salary.BaseSalary / 30m;
                hourlyRate = dailyRate / 8m;
                decimal attendanceRatio = record.ScheduledWorkDays > 0
                    ? record.ActualWorkDays / record.ScheduledWorkDays
                    : 1m;
                basePay = Math.Round(salary.BaseSalary * attendanceRatio, 0);
                AddDetail(details, items, "BASE", 1, salary.BaseSalary, basePay,
                    $"本薪 {salary.BaseSalary:N0} × 出勤比例 {attendanceRatio:P0}");
            }

            // ── 3. 固定月付津貼項目（動態，從 EmployeeSalaryItem 讀取）──
            foreach (var allowanceItem in salary.AllowanceItems.Where(i => i.Amount > 0 && i.PayrollItem != null))
            {
                AddDetail(details, items, allowanceItem.PayrollItem.Code!, 1,
                    allowanceItem.Amount, allowanceItem.Amount,
                    $"{allowanceItem.PayrollItem.Name}（固定）");
            }

            // ── 6. 加班費（倍率從 InsuranceRate DB 讀取，備援至法定常數）──────
            if (record.OvertimeHours1 > 0)
            {
                decimal appliedRate1 = Math.Round(hourlyRate * ot1Rate, 4);
                decimal ot1Pay = Math.Round(record.OvertimeHours1 * appliedRate1, 0);
                AddDetail(details, items, "OT1", record.OvertimeHours1, appliedRate1, ot1Pay,
                    $"平日加班（前2hr）{record.OvertimeHours1}hr × {appliedRate1:N4}（×{ot1Rate:N4}）= {ot1Pay:N0}");
            }

            if (record.OvertimeHours2 > 0)
            {
                decimal appliedRate2 = Math.Round(hourlyRate * ot2Rate, 4);
                decimal ot2Pay = Math.Round(record.OvertimeHours2 * appliedRate2, 0);
                AddDetail(details, items, "OT2", record.OvertimeHours2, appliedRate2, ot2Pay,
                    $"平日加班（後2hr）{record.OvertimeHours2}hr × {appliedRate2:N4}（×{ot2Rate:N4}）= {ot2Pay:N0}");
            }

            // 休息日加班（勞基法第24條第2項：前2hr 用 restDay1Rate，後續 用 restDay2Rate）
            if (record.HolidayOvertimeHours > 0)
            {
                decimal restFirst = Math.Min(record.HolidayOvertimeHours, 2m);
                decimal restExtra = Math.Max(record.HolidayOvertimeHours - 2m, 0m);
                decimal appliedRestRate1 = Math.Round(hourlyRate * restDay1Rate, 4);
                decimal appliedRestRate2 = Math.Round(hourlyRate * restDay2Rate, 4);
                decimal restPay = Math.Round(restFirst * appliedRestRate1 + restExtra * appliedRestRate2, 0);
                decimal restAvgRate = record.HolidayOvertimeHours > 0
                    ? Math.Round(restPay / record.HolidayOvertimeHours, 4) : 0;
                string restRemark = restExtra > 0
                    ? $"休息日加班 前{restFirst}hr×{appliedRestRate1:N4}（×{restDay1Rate:N4}）+ 後{restExtra}hr×{appliedRestRate2:N4}（×{restDay2Rate:N4}）= {restPay:N0}"
                    : $"休息日加班 {restFirst}hr×{appliedRestRate1:N4}（×{restDay1Rate:N4}）= {restPay:N0}";
                AddDetail(details, items, "OT_HOLIDAY", record.HolidayOvertimeHours, restAvgRate, restPay, restRemark);
            }

            // 國定假日加班（勞基法第39條：月薪已含假日薪，加給 natHolRate 倍時薪）
            if (record.NationalHolidayHours > 0)
            {
                decimal appliedNatRate = Math.Round(hourlyRate * natHolRate, 4);
                decimal nationalPay = Math.Round(record.NationalHolidayHours * appliedNatRate, 0);
                AddDetail(details, items, "OT_NATIONAL", record.NationalHolidayHours, appliedNatRate, nationalPay,
                    $"國定假日加班 {record.NationalHolidayHours}hr × {appliedNatRate:N4}（加給×{natHolRate:N4}，月薪已含假日薪）= {nationalPay:N0}");
            }

            // ── 7. 曠職扣薪（負數）— 月薪制才適用，時薪制以工時計算已無此需求 ──
            if (salary.SalaryType != SalaryType.Hourly && record.AbsentDays > 0)
            {
                decimal dailyRateForDeduct = salary.BaseSalary / 30m;
                decimal absentDeduct = Math.Round(record.AbsentDays * dailyRateForDeduct, 0);
                AddDetail(details, items, "ABSENT", record.AbsentDays, dailyRateForDeduct, -absentDeduct,
                    $"曠職 {record.AbsentDays}天 × 日薪 {dailyRateForDeduct:N2} = -{absentDeduct:N0}");
            }

            // ── 8. 病假半薪扣除（負數）— 月薪制才適用 ─────────────────
            if (salary.SalaryType != SalaryType.Hourly && record.SickLeaveDays > 0)
            {
                decimal dailyRateForDeduct = salary.BaseSalary / 30m;
                decimal sickDeduct = Math.Round(record.SickLeaveDays * dailyRateForDeduct * 0.5m, 0);
                AddDetail(details, items, "SICK", record.SickLeaveDays, dailyRateForDeduct * 0.5m, -sickDeduct,
                    $"病假 {record.SickLeaveDays}天 × 日薪半薪 = -{sickDeduct:N0}");
            }

            // ── 9. 計算應發總額（含投保基礎計算）──────────────────────
            decimal grossIncome = details.Where(d => d.Amount > 0).Sum(d => d.Amount);
            decimal totalDeductSoFar = details.Where(d => d.Amount < 0).Sum(d => d.Amount);

            // ── 10. 勞保費（員工負擔）───────────────────────────────
            decimal laborInsuredSalary = salary.LaborInsuredSalary > 0 ? salary.LaborInsuredSalary : salary.BaseSalary;
            decimal laborInsuranceEE = Math.Round(laborInsuredSalary * laborInsuranceEmployeeRate, 0);
            if (laborInsuranceEE > 0)
                AddDetail(details, items, "LI_EE", 1, laborInsuredSalary, -laborInsuranceEE,
                    $"勞保費 投保薪資 {laborInsuredSalary:N0} × {laborInsuranceEmployeeRate:P0} = -{laborInsuranceEE:N0}");

            // ── 11. 健保費（員工負擔）───────────────────────────────
            decimal healthInsuredAmount = salary.HealthInsuredAmount > 0 ? salary.HealthInsuredAmount : salary.BaseSalary;
            int multiplier = Math.Min(salary.DependentCount, 3) + 1; // 最多 4（本人+3眷屬）
            decimal healthInsuranceEE = Math.Round(healthInsuredAmount * healthInsuranceEmployeeRate * multiplier, 0);
            if (healthInsuranceEE > 0)
            {
                string depNote = salary.DependentCount > 0 ? $"×{multiplier}（含{salary.DependentCount}眷屬）" : "";
                AddDetail(details, items, "HI_EE", 1, healthInsuredAmount, -healthInsuranceEE,
                    $"健保費 投保金額 {healthInsuredAmount:N0} × {healthInsuranceEmployeeRate:P2}{depNote} = -{healthInsuranceEE:N0}");
            }

            // ── 12. 勞退自提（員工）─────────────────────────────────
            decimal voluntaryRetirement = 0;
            if (salary.VoluntaryRetirementRate > 0)
            {
                voluntaryRetirement = Math.Round(salary.BaseSalary * (salary.VoluntaryRetirementRate / 100m), 0);
                AddDetail(details, items, "RETIRE_VOL", 1, salary.BaseSalary, -voluntaryRetirement,
                    $"勞退自提 本薪 {salary.BaseSalary:N0} × {salary.VoluntaryRetirementRate}% = -{voluntaryRetirement:N0}");
            }

            // ── 13. 課稅所得 ─────────────────────────────────────────
            // 依各津貼項目 PayrollItem.IsTaxable 旗標決定免稅額
            // MEAL / TRANSPORT 有法定免稅上限（來自 InsuranceRate 表）；其他 IsTaxable=false 項目全額免稅
            decimal taxFreeFromAllowances = 0;
            foreach (var allowance in salary.AllowanceItems
                .Where(i => i.PayrollItem != null && !i.PayrollItem.IsTaxable && i.Amount > 0))
            {
                decimal taxFreeAmount = allowance.Amount;
                if (allowance.PayrollItem!.Code == "MEAL")
                    taxFreeAmount = Math.Min(allowance.Amount, mealTaxFreeLimit);
                else if (allowance.PayrollItem!.Code == "TRANSPORT")
                    taxFreeAmount = Math.Min(allowance.Amount, transportTaxFreeLimit);
                taxFreeFromAllowances += taxFreeAmount;
            }
            decimal taxableIncome = grossIncome - taxFreeFromAllowances - laborInsuranceEE - healthInsuranceEE - voluntaryRetirement;
            taxableIncome = Math.Max(taxableIncome, 0);

            // ── 14. 查扣繳稅額表 ─────────────────────────────────────
            decimal withholdingTax = 0;
            if (taxableIncome > 0 && salary.TaxType == TaxWithholdingType.Standard)
            {
                withholdingTax = await LookupWithholdingTaxAsync(context, taxableIncome, salary.DependentCount, periodDate);
            }
            else if (taxableIncome > 0 && salary.TaxType == TaxWithholdingType.Resident)
            {
                withholdingTax = Math.Round(taxableIncome * 0.05m, 0);
            }
            else if (taxableIncome > 0 && salary.TaxType == TaxWithholdingType.NonResident)
            {
                withholdingTax = Math.Round(taxableIncome * 0.18m, 0);
            }

            if (withholdingTax > 0)
                AddDetail(details, items, "TAX", 1, taxableIncome, -withholdingTax,
                    $"薪資所得稅 課稅所得 {taxableIncome:N0} = -{withholdingTax:N0}");

            // ── 15. 彙總計算 ─────────────────────────────────────────
            record.GrossIncome = details.Where(d => d.Amount > 0).Sum(d => d.Amount);
            record.TotalDeduction = Math.Abs(details.Where(d => d.Amount < 0).Sum(d => d.Amount));
            record.NetPay = record.GrossIncome - record.TotalDeduction;

            // 快照
            record.LaborInsuranceSalary = laborInsuredSalary;
            record.HealthInsuranceAmount = healthInsuredAmount;
            record.TaxableIncome = taxableIncome;
            record.WithholdingTax = withholdingTax;

            // 雇主成本（不影響員工薪資）
            record.EmployerLaborInsurance = Math.Round(laborInsuredSalary * laborInsuranceEmployerRate, 0);
            record.EmployerHealthInsurance = Math.Round(healthInsuredAmount * healthInsuranceEmployerRate * multiplier, 0);
            record.EmployerRetirement = Math.Round(salary.BaseSalary * retirementEmployerRate, 0);

            // 掛載明細到 record
            foreach (var d in details)
            {
                d.PayrollRecordId = record.Id;
                record.Details.Add(d);
            }

            context.PayrollRecordDetails.AddRange(details);
        }

        // ────────────────────────────────────────────────────────────
        // 輔助方法
        // ────────────────────────────────────────────────────────────

        /// <summary>取得員工在指定薪資月份的有效薪資設定</summary>
        private static async Task<EmployeeSalary?> GetCurrentSalaryAsync(
            AppDbContext context, int employeeId, PayrollPeriod period)
        {
            int adYear = period.Year + 1911;
            var periodStart = new DateOnly(adYear, period.Month, 1);
            var periodEnd = new DateOnly(adYear, period.Month, DateTime.DaysInMonth(adYear, period.Month));

            return await context.EmployeeSalaries
                .Include(s => s.AllowanceItems).ThenInclude(i => i.PayrollItem)
                .Where(s => s.EmployeeId == employeeId
                         && s.EffectiveDate <= periodEnd        // 月內任一天設定都算生效
                         && (s.ExpiryDate == null || s.ExpiryDate >= periodStart)
                         && s.Status == EntityStatus.Active)
                .OrderByDescending(s => s.EffectiveDate)
                .FirstOrDefaultAsync();
        }

        /// <summary>查扣繳稅額表（Standard 制度使用）</summary>
        private static async Task<decimal> LookupWithholdingTaxAsync(
            AppDbContext context, decimal taxableIncome, int dependentCount, DateOnly periodDate)
        {
            // 取得適用於該月份的最新版本扣繳稅額表
            var entry = await context.WithholdingTaxTables
                .Where(t => t.DependentCount == dependentCount
                         && t.SalaryFrom <= taxableIncome
                         && (t.SalaryTo == null || t.SalaryTo >= taxableIncome)
                         && t.EffectiveDate <= periodDate)
                .OrderByDescending(t => t.EffectiveDate)
                .FirstOrDefaultAsync();

            return entry?.TaxAmount ?? 0;
        }

        /// <summary>建立薪資明細並加入清單</summary>
        private static void AddDetail(
            List<PayrollRecordDetail> details,
            List<PayrollItem> items,
            string itemCode,
            decimal quantity,
            decimal unitAmount,
            decimal amount,
            string? remark = null)
        {
            var item = items.FirstOrDefault(i => i.Code == itemCode);
            if (item == null)
            {
                // 找不到項目定義：通常表示種子資料未執行或項目被停用
                // 不應靜默略過，以免造成薪資計算結果不完整而無法察覺
                System.Diagnostics.Debug.WriteLine($"[PayrollCalculation] 找不到薪資項目代碼 '{itemCode}'，此明細將略過。請確認薪資項目種子資料已完整執行且項目狀態為啟用。");
                return;
            }

            details.Add(new PayrollRecordDetail
            {
                PayrollItemId = item.Id,
                Item = item,
                Quantity = quantity,
                UnitAmount = unitAmount,
                Amount = amount,
                Remark = remark
            });
        }

        /// <summary>計算指定年月中的工作日數（扣除週六日，不含國定假日）</summary>
        private static int GetWorkDaysInMonth(int adYear, int month)
        {
            int totalDays = DateTime.DaysInMonth(adYear, month);
            int workDays = 0;
            for (int d = 1; d <= totalDays; d++)
            {
                var dow = new DateTime(adYear, month, d).DayOfWeek;
                if (dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday)
                    workDays++;
            }
            return workDays;
        }
    }
}
