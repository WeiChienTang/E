using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.Payroll;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ERPCore2.Services.Payroll
{
    public class MinimumWageService : IMinimumWageService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MinimumWageService>? _logger;

        private const string GovernmentApiUrl =
            "https://apiservice.mol.gov.tw/OdService/rest/datastore/A17000000J-020050-o8Q";

        public MinimumWageService(
            IDbContextFactory<AppDbContext> contextFactory,
            IHttpClientFactory httpClientFactory,
            ILogger<MinimumWageService>? logger = null)
        {
            _contextFactory = contextFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<MinimumWage>> GetAllAsync()
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MinimumWages
                    .OrderByDescending(x => x.EffectiveDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetAllAsync), GetType(), _logger);
                return new List<MinimumWage>();
            }
        }

        public async Task<MinimumWage?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MinimumWages.FindAsync(id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByIdAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<MinimumWage?> GetEffectiveAsync(DateOnly date)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.MinimumWages
                    .Where(x => x.EffectiveDate <= date)
                    .OrderByDescending(x => x.EffectiveDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetEffectiveAsync), GetType(), _logger);
                return null;
            }
        }

        public async Task<ServiceResult<MinimumWage>> CreateAsync(MinimumWage entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.MinimumWages.Add(entity);
                await context.SaveChangesAsync();
                return ServiceResult<MinimumWage>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<MinimumWage>.Failure("新增基本工資時發生錯誤");
            }
        }

        public async Task<ServiceResult<MinimumWage>> UpdateAsync(MinimumWage entity)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                context.MinimumWages.Update(entity);
                await context.SaveChangesAsync();
                return ServiceResult<MinimumWage>.Success(entity);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<MinimumWage>.Failure("更新基本工資時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var entity = await context.MinimumWages.FindAsync(id);
                if (entity == null)
                    return ServiceResult.Failure("找不到指定的基本工資記錄");
                context.MinimumWages.Remove(entity);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除基本工資時發生錯誤");
            }
        }

        public async Task<ServiceResult<int>> SyncFromGovernmentAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(15);

                var response = await httpClient.GetAsync(GovernmentApiUrl);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MolApiResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiResponse?.Result?.Records == null)
                    return ServiceResult<int>.Failure("API 回應格式異常，請稍後再試");

                using var context = await _contextFactory.CreateDbContextAsync();
                var existingDates = (await context.MinimumWages
                    .Select(w => w.EffectiveDate)
                    .ToListAsync())
                    .ToHashSet();

                var imported = 0;
                foreach (var record in apiResponse.Result.Records)
                {
                    var effectiveDate = ParseDate(record.EffectiveDate);
                    if (effectiveDate == default) continue;
                    if (existingDates.Contains(effectiveDate)) continue;

                    var (monthly, hourly) = ParseWageContent(record.Content);
                    if (monthly == 0 && hourly == 0) continue;

                    context.MinimumWages.Add(new MinimumWage
                    {
                        EffectiveDate = effectiveDate,
                        MonthlyAmount = monthly,
                        HourlyAmount = hourly,
                        Remarks = "勞動部開放資料自動匯入"
                    });
                    existingDates.Add(effectiveDate);
                    imported++;
                }

                if (imported > 0)
                    await context.SaveChangesAsync();

                return ServiceResult<int>.Success(imported);
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<int>.Failure($"無法連線至勞動部 API：{ex.Message}");
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SyncFromGovernmentAsync), GetType(), _logger);
                return ServiceResult<int>.Failure("從政府資料載入時發生錯誤");
            }
        }

        // ── 私有解析工具 ──────────────────────────────────────────

        /// <summary>解析 YYYYMMDD 格式（西元年）為 DateOnly</summary>
        private static DateOnly ParseDate(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr) || dateStr.Length != 8)
                return default;
            if (int.TryParse(dateStr[..4], out int year) &&
                int.TryParse(dateStr[4..6], out int month) &&
                int.TryParse(dateStr[6..8], out int day) &&
                year >= 1900 && year <= 2100 &&
                month >= 1 && month <= 12 &&
                day >= 1 && day <= 31)
            {
                try { return new DateOnly(year, month, day); }
                catch { return default; }
            }
            return default;
        }

        /// <summary>從文字（如「月薪27,470、時薪183」）解析月薪與時薪</summary>
        private static (decimal monthly, decimal hourly) ParseWageContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return (0, 0);

            var monthlyMatch = Regex.Match(content, @"月薪([\d,]+)");
            var hourlyMatch  = Regex.Match(content, @"時薪([\d,]+)");

            decimal monthly = monthlyMatch.Success &&
                decimal.TryParse(monthlyMatch.Groups[1].Value.Replace(",", ""), out var m) ? m : 0;
            decimal hourly = hourlyMatch.Success &&
                decimal.TryParse(hourlyMatch.Groups[1].Value.Replace(",", ""), out var h) ? h : 0;

            return (monthly, hourly);
        }

        // ── 勞動部 API 回應 DTO ───────────────────────────────────

        private class MolApiResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("result")]
            public MolApiResult? Result { get; set; }
        }

        private class MolApiResult
        {
            [JsonPropertyName("records")]
            public List<MolWageRecord>? Records { get; set; }
        }

        private class MolWageRecord
        {
            [JsonPropertyName("年度")]
            public string? Year { get; set; }

            [JsonPropertyName("內容/調整金額（新台幣）")]
            public string? Content { get; set; }

            [JsonPropertyName("實施日期（民國）")]
            public string? EffectiveDate { get; set; }
        }
    }
}
