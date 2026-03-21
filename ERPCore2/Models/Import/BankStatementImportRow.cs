namespace ERPCore2.Models.Import
{
    /// <summary>
    /// CSV 匯入的單筆銀行交易資料（解析後的預覽行）
    /// 第 6 欄「交易後餘額」僅用於預覽顯示，不寫入資料庫。
    /// </summary>
    public class BankStatementImportRow
    {
        /// <summary>CSV 列號（從 1 開始，不含標題列）</summary>
        public int RowNumber { get; set; }

        /// <summary>交易日期（必填）</summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>交易說明（必填，最多 200 字）</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>支出金額（>=0，0 表示無支出）</summary>
        public decimal DebitAmount { get; set; }

        /// <summary>收入金額（>=0，0 表示無收入）</summary>
        public decimal CreditAmount { get; set; }

        /// <summary>銀行流水號（選填，存入 DB 供未來自動配對）</summary>
        public string? BankReference { get; set; }

        /// <summary>交易後餘額（選填，僅供預覽顯示核對，不存入 DB）</summary>
        public decimal? BalanceAfter { get; set; }

        /// <summary>此列是否通過驗證</summary>
        public bool IsValid { get; set; } = true;

        /// <summary>驗證錯誤訊息（IsValid=false 時填入）</summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// CSV 解析與驗證的整體結果
    /// </summary>
    public class BankStatementImportResult
    {
        /// <summary>解析是否整體成功（無致命錯誤）</summary>
        public bool Success { get; set; }

        /// <summary>整體錯誤訊息（如檔案無法讀取、格式完全錯誤等）</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>解析出的所有行（含驗證失敗的行）</summary>
        public List<BankStatementImportRow> Rows { get; set; } = new();

        /// <summary>通過驗證的有效行數</summary>
        public int ValidRowCount => Rows.Count(r => r.IsValid);

        /// <summary>驗證失敗的行數</summary>
        public int InvalidRowCount => Rows.Count(r => !r.IsValid);

        /// <summary>是否有任何驗證失敗的行</summary>
        public bool HasInvalidRows => Rows.Any(r => !r.IsValid);

        public static BankStatementImportResult Fail(string errorMessage) => new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };

        public static BankStatementImportResult Ok(List<BankStatementImportRow> rows) => new()
        {
            Success = true,
            Rows = rows
        };
    }
}
