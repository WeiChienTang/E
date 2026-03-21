using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Import;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ERPCore2.Services
{
    /// <summary>
    /// 銀行對帳單 CSV 匯入服務
    /// </summary>
    public class BankStatementImportService : IBankStatementImportService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<BankStatementImportService>? _logger;

        // CSV 最大檔案大小 5MB
        private const long MaxCsvFileSize = 5 * 1024 * 1024;

        // 預期欄位數（最少 4，最多 6）
        private const int MinColumns = 4;
        private const int MaxColumns = 6;

        // 支援的日期格式
        private static readonly string[] DateFormats =
        {
            "yyyy/MM/dd", "yyyy-MM-dd", "yyyy/M/d", "yyyy-M-d",
            "MM/dd/yyyy", "M/d/yyyy"
        };

        public BankStatementImportService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<BankStatementImportService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        // ===== 解析 CSV =====

        public async Task<BankStatementImportResult> ParseCsvAsync(IBrowserFile file)
        {
            try
            {
                if (file.Size > MaxCsvFileSize)
                    return BankStatementImportResult.Fail($"檔案大小不可超過 5MB（目前：{file.Size / 1024 / 1024:F1}MB）");

                var ext = Path.GetExtension(file.Name).ToLowerInvariant();
                if (ext != ".csv")
                    return BankStatementImportResult.Fail("請上傳 .csv 格式的檔案");

                // 讀取原始 bytes（用於編碼偵測）
                using var memStream = new MemoryStream();
                await file.OpenReadStream(MaxCsvFileSize).CopyToAsync(memStream);
                var rawBytes = memStream.ToArray();

                // 偵測編碼：UTF-8 優先，fallback Big5
                var content = DecodeWithFallback(rawBytes);
                if (content == null)
                    return BankStatementImportResult.Fail("無法讀取檔案內容，請確認檔案為 UTF-8 或 Big5 編碼");

                return ParseCsvContent(content);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "解析 CSV 時發生錯誤");
                return BankStatementImportResult.Fail($"解析失敗：{ex.Message}");
            }
        }

        /// <summary>
        /// 編碼偵測：嘗試 UTF-8（含 BOM 偵測），失敗時 fallback Big5
        /// </summary>
        private static string? DecodeWithFallback(byte[] bytes)
        {
            // 1. 有 BOM → 直接用 UTF-8
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);

            // 2. 嘗試 UTF-8（不含 BOM，DecoderFallback=ExceptionFallback 確保真的是 UTF-8）
            try
            {
                var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                var decoded = utf8.GetString(bytes);
                // 確認沒有替換字元（U+FFFD）
                if (!decoded.Contains('\uFFFD'))
                    return decoded;
            }
            catch { /* 不是合法 UTF-8，繼續嘗試 Big5 */ }

            // 3. 嘗試 Big5（Code Page 950）
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var big5 = Encoding.GetEncoding(950);
                return big5.GetString(bytes);
            }
            catch { return null; }
        }

        private static BankStatementImportResult ParseCsvContent(string content)
        {
            var lines = content
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (lines.Count == 0)
                return BankStatementImportResult.Fail("CSV 檔案是空的");

            // 跳過標題列（第一列）
            var dataLines = lines.Skip(1).ToList();
            if (dataLines.Count == 0)
                return BankStatementImportResult.Fail("CSV 只有標題列，無任何資料");

            var rows = new List<BankStatementImportRow>();
            for (int i = 0; i < dataLines.Count; i++)
            {
                var line = dataLines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var row = ParseLine(line, i + 1); // RowNumber 從 1 開始
                rows.Add(row);
            }

            if (rows.Count == 0)
                return BankStatementImportResult.Fail("CSV 沒有有效的資料列");

            return BankStatementImportResult.Ok(rows);
        }

        private static BankStatementImportRow ParseLine(string line, int rowNumber)
        {
            var row = new BankStatementImportRow { RowNumber = rowNumber };

            var cols = SplitCsvLine(line);

            if (cols.Count < MinColumns)
            {
                row.IsValid = false;
                row.ErrorMessage = $"欄位數不足（需至少 {MinColumns} 欄，實際 {cols.Count} 欄）";
                return row;
            }

            var errors = new List<string>();

            // 欄 1：交易日期（必填）
            var dateStr = cols[0].Trim();
            if (!DateTime.TryParseExact(dateStr, DateFormats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var date))
            {
                errors.Add($"交易日期格式錯誤（「{dateStr}」，支援 yyyy/MM/dd 或 yyyy-MM-dd）");
            }
            else
            {
                row.TransactionDate = date;
            }

            // 欄 2：交易說明（必填）
            var desc = cols[1].Trim();
            if (string.IsNullOrWhiteSpace(desc))
                errors.Add("交易說明不可空白");
            else if (desc.Length > 200)
                errors.Add($"交易說明超過 200 字（目前 {desc.Length} 字）");
            else
                row.Description = desc;

            // 欄 3：支出金額（選填，空白=0）
            var debitStr = cols[2].Trim().Replace(",", "");
            if (!string.IsNullOrWhiteSpace(debitStr))
            {
                if (!decimal.TryParse(debitStr, out var debit) || debit < 0)
                    errors.Add($"支出金額格式錯誤（「{cols[2].Trim()}」）");
                else
                    row.DebitAmount = debit;
            }

            // 欄 4：收入金額（選填，空白=0）
            var creditStr = cols[3].Trim().Replace(",", "");
            if (!string.IsNullOrWhiteSpace(creditStr))
            {
                if (!decimal.TryParse(creditStr, out var credit) || credit < 0)
                    errors.Add($"收入金額格式錯誤（「{cols[3].Trim()}」）");
                else
                    row.CreditAmount = credit;
            }

            // 欄 5：銀行流水號（選填）
            if (cols.Count >= 5)
            {
                var ref5 = cols[4].Trim();
                if (ref5.Length > 100)
                    errors.Add($"銀行流水號超過 100 字");
                else if (!string.IsNullOrWhiteSpace(ref5))
                    row.BankReference = ref5;
            }

            // 欄 6：交易後餘額（選填，僅預覽）
            if (cols.Count >= 6)
            {
                var balStr = cols[5].Trim().Replace(",", "");
                if (!string.IsNullOrWhiteSpace(balStr))
                {
                    if (decimal.TryParse(balStr, out var bal))
                        row.BalanceAfter = bal;
                    // 餘額格式錯誤不阻擋匯入，只是不顯示
                }
            }

            // 借貸均為 0 → 警告但不阻擋
            if (errors.Count == 0 && row.DebitAmount == 0 && row.CreditAmount == 0)
                errors.Add("支出金額與收入金額均為 0，請確認是否正確");

            if (errors.Count > 0)
            {
                row.IsValid = false;
                row.ErrorMessage = string.Join("；", errors);
            }

            return row;
        }

        /// <summary>
        /// 分割 CSV 列（支援欄位值含逗號時用雙引號包裹）
        /// </summary>
        private static List<string> SplitCsvLine(string line)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            bool inQuote = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuote && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuote = !inQuote;
                    }
                }
                else if (c == ',' && !inQuote)
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            result.Add(sb.ToString());
            return result;
        }

        // ===== 前提條件驗證 =====

        public async Task<(bool CanImport, string? BlockReason)> ValidatePreConditionAsync(int bankStatementId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var unmatchedCount = await context.BankStatementLines
                .CountAsync(l => l.BankStatementId == bankStatementId
                               && !l.IsMatched
                               && l.Status == Models.Enums.EntityStatus.Active);

            if (unmatchedCount > 0)
                return (false, $"此對帳單已有 {unmatchedCount} 筆未配對的明細行。請先清空所有未配對明細，再執行 CSV 匯入。");

            return (true, null);
        }

        // ===== 寫入資料庫 =====

        public async Task<(bool Success, string Message, int ImportedCount)> ImportAsync(
            int bankStatementId,
            List<BankStatementImportRow> rows,
            string operatorName)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                // 取得對帳單（驗證期間）
                var statement = await context.BankStatements
                    .FirstOrDefaultAsync(s => s.Id == bankStatementId);

                if (statement == null)
                    return (false, "找不到指定的對帳單", 0);

                var validRows = rows.Where(r => r.IsValid).ToList();
                if (validRows.Count == 0)
                    return (false, "沒有通過驗證的資料列可供匯入", 0);

                // 期間驗證（軟性警告：超出期間不阻擋，但記錄）
                var outOfRange = validRows.Count(r =>
                    r.TransactionDate < statement.PeriodStart ||
                    r.TransactionDate > statement.PeriodEnd);

                var lines = validRows.Select((row, idx) => new BankStatementLine
                {
                    BankStatementId  = bankStatementId,
                    TransactionDate  = row.TransactionDate,
                    Description      = row.Description,
                    DebitAmount      = row.DebitAmount,
                    CreditAmount     = row.CreditAmount,
                    BankReference    = row.BankReference,
                    IsMatched        = false,
                    SortOrder        = idx + 1,
                    CreatedAt        = DateTime.Now,
                    UpdatedAt        = DateTime.Now,
                    CreatedBy        = operatorName,
                    UpdatedBy        = operatorName
                }).ToList();

                await context.BankStatementLines.AddRangeAsync(lines);
                await context.SaveChangesAsync();

                var msg = $"成功匯入 {lines.Count} 筆明細";
                if (outOfRange > 0)
                    msg += $"（其中 {outOfRange} 筆日期超出對帳期間 {statement.PeriodStart:yyyy/MM/dd}–{statement.PeriodEnd:yyyy/MM/dd}，請確認是否正確）";

                return (true, msg, lines.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "匯入銀行對帳單明細時發生錯誤（BankStatementId={Id}）", bankStatementId);
                return (false, $"匯入失敗：{ex.Message}", 0);
            }
        }

        // ===== 範本 CSV =====

        public string GenerateTemplateCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("交易日期,交易說明,支出金額,收入金額,銀行流水號,交易後餘額");
            sb.AppendLine("2026/03/01,薪資撥款,,250000,TXN20260301001,250000");
            sb.AppendLine("2026/03/02,水電費繳納,3500,,TXN20260302001,246500");
            sb.AppendLine("2026/03/05,客戶匯款-台灣科技,,50000,TXN20260305001,296500");
            sb.AppendLine("2026/03/10,辦公室租金,80000,,TXN20260310001,216500");
            return sb.ToString();
        }
    }
}
