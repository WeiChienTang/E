using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services
{
    /// <summary>
    /// 年底結帳服務
    ///
    /// 分錄邏輯：
    ///   Step 1 — 損益科目歸零（轉入本期損益 3353）
    ///     收入類（AccountType = Revenue/NonOperating，淨貸方）：
    ///       借：各收入科目  / 貸：本期損益
    ///     成本/費用類（AccountType = Cost/Expense，淨借方）：
    ///       借：本期損益    / 貸：各成本費用科目
    ///
    ///   Step 2 — 本期損益轉累積盈虧（3351）
    ///     盈利：借 本期損益 / 貸 累積盈虧
    ///     虧損：借 累積盈虧 / 貸 本期損益
    /// </summary>
    public class FiscalYearClosingService : IFiscalYearClosingService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IFiscalPeriodService _fiscalPeriodService;
        private readonly IJournalEntryService _journalEntryService;
        private readonly IAccountItemService _accountItemService;
        private readonly ICompanyService _companyService;
        private readonly ILogger<FiscalYearClosingService> _logger;

        // 結帳用科目代碼（對應種子資料 AccountItemSeeder）
        private const string CurrentPeriodIncomeCode = "3353"; // 本期損益（Income Summary — 損益科目的中間彙總帳）
        private const string RetainedEarningsCode    = "3351"; // 累積盈虧（Accumulated P&L — 最終累積保留盈餘帳）

        // 需歸零的科目類型（損益表科目）
        private static readonly AccountType[] IncomeStatementTypes =
        {
            AccountType.Revenue,
            AccountType.Cost,
            AccountType.Expense,
            AccountType.NonOperatingIncomeAndExpense
        };

        public FiscalYearClosingService(
            IDbContextFactory<AppDbContext> contextFactory,
            IFiscalPeriodService fiscalPeriodService,
            IJournalEntryService journalEntryService,
            IAccountItemService accountItemService,
            ICompanyService companyService,
            ILogger<FiscalYearClosingService> logger)
        {
            _contextFactory = contextFactory;
            _fiscalPeriodService = fiscalPeriodService;
            _journalEntryService = journalEntryService;
            _accountItemService = accountItemService;
            _companyService = companyService;
            _logger = logger;
        }

        public async Task<YearEndClosingPreCheck> PreCheckAsync(int year, int companyId)
        {
            var result = new YearEndClosingPreCheck();

            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 1. 確認是否已結帳（冪等性）
                result.AlreadyClosed = await context.JournalEntries.AnyAsync(je =>
                    je.CompanyId == companyId &&
                    je.FiscalYear == year &&
                    je.EntryType == JournalEntryType.Closing &&
                    je.JournalEntryStatus == JournalEntryStatus.Posted);

                if (result.AlreadyClosed)
                {
                    result.Errors.Add($"{year} 年度已執行過年底結帳");
                    return result;
                }

                // 2. 期間狀態檢查
                var periods = await _fiscalPeriodService.GetByYearAsync(year, companyId);
                result.TotalPeriods = periods.Count;
                result.ClosedOrLockedPeriods = periods.Count(p =>
                    p.PeriodStatus == FiscalPeriodStatus.Closed ||
                    p.PeriodStatus == FiscalPeriodStatus.Locked);
                result.OpenPeriodNumbers = periods
                    .Where(p => p.PeriodStatus == FiscalPeriodStatus.Open)
                    .Select(p => p.PeriodNumber)
                    .OrderBy(n => n)
                    .ToList();

                if (result.TotalPeriods < 12)
                    result.Warnings.Add($"年度期間只有 {result.TotalPeriods} 個（預期 12 個），請確認是否已初始化完整年度");

                if (result.OpenPeriodNumbers.Any())
                    result.Errors.Add($"以下月份仍開放中，請先關帳再執行年底結帳：{string.Join("、", result.OpenPeriodNumbers.Select(n => $"{n} 月"))}");

                // 3. 確認無未過帳傳票（Draft 傳票不計入損益，會導致結帳數字不正確）
                var draftCount = await context.JournalEntries
                    .CountAsync(je =>
                        je.CompanyId == companyId &&
                        je.FiscalYear == year &&
                        je.EntryType != JournalEntryType.Closing &&
                        je.EntryType != JournalEntryType.OpeningBalance &&
                        je.JournalEntryStatus == JournalEntryStatus.Draft);

                if (draftCount > 0)
                    result.Errors.Add($"存在 {draftCount} 筆尚未過帳的傳票（草稿狀態），請先全部過帳或作廢後再執行年底結帳");

                // 4. 計算預估本期損益
                result.EstimatedNetIncome = await CalculateNetIncomeAsync(context, year, companyId);

                result.CanClose = !result.Errors.Any();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PreCheckAsync), GetType(), _logger,
                    new { Year = year, CompanyId = companyId });
                result.Errors.Add("前置檢查過程發生錯誤，請稍後再試");
            }

            return result;
        }

        public async Task<(bool Success, string ErrorMessage)> ExecuteYearEndClosingAsync(
            int year, int companyId, string executedBy)
        {
            try
            {
                // 前置檢查
                var preCheck = await PreCheckAsync(year, companyId);
                if (!preCheck.CanClose)
                    return (false, string.Join("；", preCheck.Errors));

                var company = await _companyService.GetPrimaryCompanyAsync();
                if (company == null)
                    return (false, "找不到預設公司，請先在系統設定中建立公司資料");

                using var context = await _contextFactory.CreateDbContextAsync();

                // 取得本期損益科目
                var incomeAccount = await _accountItemService.GetByCodeAsync(CurrentPeriodIncomeCode);
                if (incomeAccount == null)
                    return (false, $"找不到「本期損益」科目（代碼 {CurrentPeriodIncomeCode}），請確認科目種子資料已正確載入");

                // 取得累積盈虧科目
                var retainedAccount = await _accountItemService.GetByCodeAsync(RetainedEarningsCode);
                if (retainedAccount == null)
                    return (false, $"找不到「累積盈虧」科目（代碼 {RetainedEarningsCode}），請確認科目種子資料已正確載入");

                // ===== Step 1：損益科目歸零，轉入本期損益 =====
                var balances = await GetIncomeStatementBalancesAsync(context, year, companyId);

                if (balances.Any())
                {
                    var closingLines = new List<JournalEntryLine>();
                    decimal totalDebitToIncome  = 0m; // 費用/成本 → 借 本期損益
                    decimal totalCreditToIncome = 0m; // 收入     → 貸 本期損益
                    int lineNum = 1;

                    foreach (var (accountId, accountType, netDebit) in balances)
                    {
                        if (netDebit == 0m) continue;

                        if (netDebit > 0)
                        {
                            // 淨借方餘額（費用/成本）→ 貸：此科目，借：本期損益
                            closingLines.Add(new JournalEntryLine
                            {
                                LineNumber    = lineNum++,
                                AccountItemId = accountId,
                                Direction     = AccountDirection.Credit,
                                Amount        = netDebit,
                                LineDescription = "年度結帳－費用歸零"
                            });
                            totalDebitToIncome += netDebit;
                        }
                        else
                        {
                            // 淨貸方餘額（收入）→ 借：此科目，貸：本期損益
                            closingLines.Add(new JournalEntryLine
                            {
                                LineNumber    = lineNum++,
                                AccountItemId = accountId,
                                Direction     = AccountDirection.Debit,
                                Amount        = -netDebit, // netDebit 為負，取絕對值
                                LineDescription = "年度結帳－收入歸零"
                            });
                            totalCreditToIncome += -netDebit;
                        }
                    }

                    // 加入本期損益平衡行
                    if (totalDebitToIncome > 0)
                        closingLines.Add(new JournalEntryLine
                        {
                            LineNumber    = lineNum++,
                            AccountItemId = incomeAccount.Id,
                            Direction     = AccountDirection.Debit,
                            Amount        = totalDebitToIncome,
                            LineDescription = "年度結帳－費用轉入本期損益"
                        });

                    if (totalCreditToIncome > 0)
                        closingLines.Add(new JournalEntryLine
                        {
                            LineNumber    = lineNum++,
                            AccountItemId = incomeAccount.Id,
                            Direction     = AccountDirection.Credit,
                            Amount        = totalCreditToIncome,
                            LineDescription = "年度結帳－收入轉入本期損益"
                        });

                    var step1Date = new DateTime(year, 12, 31);
                    var step1Result = await CreateAndPostClosingEntryAsync(
                        step1Date, $"{year} 年度結帳－損益科目歸零",
                        closingLines, company.Id, year, executedBy);

                    if (!step1Result.Success)
                        return (false, $"Step 1 失敗：{step1Result.ErrorMessage}");
                }

                // ===== Step 2：本期損益轉保留盈餘 =====
                var netIncome = await CalculateNetIncomeAsync(context, year, companyId);

                if (netIncome != 0m)
                {
                    var step2Lines = new List<JournalEntryLine>();

                    if (netIncome > 0)
                    {
                        // 盈利：借 本期損益 / 貸 保留盈餘
                        step2Lines.Add(new JournalEntryLine
                        {
                            LineNumber    = 1,
                            AccountItemId = incomeAccount.Id,
                            Direction     = AccountDirection.Debit,
                            Amount        = netIncome,
                            LineDescription = "年度損益轉結保留盈餘"
                        });
                        step2Lines.Add(new JournalEntryLine
                        {
                            LineNumber    = 2,
                            AccountItemId = retainedAccount.Id,
                            Direction     = AccountDirection.Credit,
                            Amount        = netIncome,
                            LineDescription = "年度損益轉結保留盈餘"
                        });
                    }
                    else
                    {
                        // 虧損：借 保留盈餘 / 貸 本期損益
                        step2Lines.Add(new JournalEntryLine
                        {
                            LineNumber    = 1,
                            AccountItemId = retainedAccount.Id,
                            Direction     = AccountDirection.Debit,
                            Amount        = -netIncome,
                            LineDescription = "年度虧損轉結保留盈餘"
                        });
                        step2Lines.Add(new JournalEntryLine
                        {
                            LineNumber    = 2,
                            AccountItemId = incomeAccount.Id,
                            Direction     = AccountDirection.Credit,
                            Amount        = -netIncome,
                            LineDescription = "年度虧損轉結保留盈餘"
                        });
                    }

                    var step2Date = new DateTime(year, 12, 31);
                    var step2Result = await CreateAndPostClosingEntryAsync(
                        step2Date, $"{year} 年度結帳－本期損益轉保留盈餘",
                        step2Lines, company.Id, year, executedBy);

                    if (!step2Result.Success)
                        return (false, $"Step 2 失敗：{step2Result.ErrorMessage}");
                }

                // ===== Step 3：鎖定所有年度期間 =====
                var periods = await _fiscalPeriodService.GetByYearAsync(year, companyId);
                foreach (var period in periods.Where(p => p.PeriodStatus == FiscalPeriodStatus.Closed))
                {
                    var lockResult = await _fiscalPeriodService.LockPeriodAsync(period.Id);
                    if (!lockResult.Success)
                        return (false, $"鎖定 {year}/{period.PeriodNumber} 期間失敗：{lockResult.ErrorMessage}");
                }

                // ===== Step 4：初始化下一年度 =====
                await _fiscalPeriodService.InitializeYearAsync(year + 1, companyId);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(ExecuteYearEndClosingAsync), GetType(), _logger,
                    new { Year = year, CompanyId = companyId });
                return (false, "年底結帳過程發生錯誤，請稍後再試");
            }
        }

        // ===== 私有 Helpers =====

        /// <summary>
        /// 計算指定年度的本期損益
        /// 正值 = 盈利，負值 = 虧損
        /// 結果不含 Closing 類型傳票（避免結帳後重複計算）
        /// </summary>
        private async Task<decimal> CalculateNetIncomeAsync(AppDbContext context, int year, int companyId)
        {
            var balances = await GetIncomeStatementBalancesAsync(context, year, companyId);

            decimal netIncome = 0m;
            foreach (var (_, accountType, netDebit) in balances)
            {
                // 收入類：淨貸方 = 收入（貢獻正損益）→ netDebit 為負
                // 費用類：淨借方 = 費用（貢獻負損益）→ netDebit 為正
                netIncome -= netDebit;
            }
            return netIncome;
        }

        /// <summary>
        /// 查詢損益類科目在指定年度的淨借方餘額
        /// 回傳：(AccountItemId, AccountType, NetDebit) — NetDebit > 0 表借方餘額，< 0 表貸方餘額
        /// 排除 Closing/OpeningBalance 類型傳票
        /// </summary>
        private async Task<List<(int AccountItemId, AccountType AccountType, decimal NetDebit)>> GetIncomeStatementBalancesAsync(
            AppDbContext context, int year, int companyId)
        {
            var incomeTypes = IncomeStatementTypes.Cast<int>().ToList();

            var rawData = await context.JournalEntryLines
                .Join(context.JournalEntries,
                    l => l.JournalEntryId,
                    e => e.Id,
                    (l, e) => new { Line = l, Entry = e })
                .Join(context.AccountItems,
                    x => x.Line.AccountItemId,
                    a => a.Id,
                    (x, a) => new { x.Line, x.Entry, Account = a })
                .Where(x =>
                    x.Entry.CompanyId == companyId &&
                    x.Entry.FiscalYear == year &&
                    x.Entry.JournalEntryStatus == JournalEntryStatus.Posted &&
                    x.Entry.EntryType != JournalEntryType.Closing &&
                    x.Entry.EntryType != JournalEntryType.OpeningBalance &&
                    incomeTypes.Contains((int)x.Account.AccountType))
                .GroupBy(x => new { x.Line.AccountItemId, x.Account.AccountType })
                .Select(g => new
                {
                    AccountItemId = g.Key.AccountItemId,
                    AccountType   = g.Key.AccountType,
                    TotalDebit    = g.Sum(x => x.Line.Direction == AccountDirection.Debit  ? x.Line.Amount : 0m),
                    TotalCredit   = g.Sum(x => x.Line.Direction == AccountDirection.Credit ? x.Line.Amount : 0m)
                })
                .ToListAsync();

            return rawData
                .Select(x => (x.AccountItemId, x.AccountType, x.TotalDebit - x.TotalCredit))
                .ToList();
        }

        /// <summary>
        /// 建立並過帳結帳傳票（JournalEntryType.Closing）
        /// </summary>
        private async Task<(bool Success, string ErrorMessage)> CreateAndPostClosingEntryAsync(
            DateTime entryDate,
            string description,
            List<JournalEntryLine> lines,
            int companyId,
            int fiscalYear,
            string createdBy)
        {
            var entry = new JournalEntry
            {
                EntryDate          = entryDate,
                EntryType          = JournalEntryType.Closing,
                JournalEntryStatus = JournalEntryStatus.Draft,
                Description        = description,
                CompanyId          = companyId,
                FiscalYear         = fiscalYear,
                FiscalPeriod       = 12,
                Lines              = lines
            };

            var (saved, saveError) = await _journalEntryService.SaveWithLinesAsync(entry, createdBy);
            if (!saved)
                return (false, $"建立結帳傳票失敗：{saveError}");

            var (posted, postError) = await _journalEntryService.PostEntryAsync(entry.Id, createdBy);
            if (!posted)
            {
                await _journalEntryService.CancelDraftEntryAsync(entry.Id, createdBy);
                return (false, $"過帳結帳傳票失敗：{postError}");
            }

            return (true, string.Empty);
        }
    }
}
