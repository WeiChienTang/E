namespace ERPCore2.Models.Import
{
    /// <summary>
    /// 匯入結果 DTO（Step 5 完成後回報）
    /// </summary>
    public class ImportResult
    {
        /// <summary>是否整體成功</summary>
        public bool IsSuccess { get; set; }

        /// <summary>總筆數（Excel 資料行數）</summary>
        public int TotalRows { get; set; }

        /// <summary>成功匯入筆數</summary>
        public int SuccessCount { get; set; }

        /// <summary>失敗筆數</summary>
        public int FailureCount { get; set; }

        /// <summary>整體錯誤訊息（如 Transaction Rollback 原因）</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>各行的錯誤明細（Key = 行號）</summary>
        public List<ImportRowError> RowErrors { get; set; } = new();

        /// <summary>匯入耗時</summary>
        public TimeSpan ElapsedTime { get; set; }

        public static ImportResult Success(int totalRows, TimeSpan elapsed)
        {
            return new ImportResult
            {
                IsSuccess = true,
                TotalRows = totalRows,
                SuccessCount = totalRows,
                FailureCount = 0,
                ElapsedTime = elapsed
            };
        }

        public static ImportResult Failure(string errorMessage, int totalRows, List<ImportRowError>? rowErrors = null)
        {
            return new ImportResult
            {
                IsSuccess = false,
                TotalRows = totalRows,
                SuccessCount = 0,
                FailureCount = totalRows,
                ErrorMessage = errorMessage,
                RowErrors = rowErrors ?? new()
            };
        }
    }

    /// <summary>
    /// 單行匯入錯誤明細
    /// </summary>
    public class ImportRowError
    {
        /// <summary>Excel 行號（1-based）</summary>
        public int RowNumber { get; set; }

        /// <summary>錯誤訊息</summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>有問題的欄位名稱（若可辨識）</summary>
        public string? PropertyName { get; set; }
    }
}
