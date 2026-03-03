namespace ERPCore2.Models.Export
{
    /// <summary>
    /// 資料庫匯出結果 DTO
    /// </summary>
    public class ExportResult
    {
        /// <summary>是否匯出成功</summary>
        public bool IsSuccess { get; set; }

        /// <summary>錯誤訊息（若匯出失敗）</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>匯出的 Excel 檔案位元組</summary>
        public byte[] FileContent { get; set; } = Array.Empty<byte>();

        /// <summary>建議的檔案名稱</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>匯出的資料表數量</summary>
        public int TableCount { get; set; }

        /// <summary>匯出的總資料行數</summary>
        public int TotalRowCount { get; set; }

        /// <summary>各資料表的匯出摘要</summary>
        public List<ExportTableSummary> TableSummaries { get; set; } = new();

        /// <summary>匯出耗時</summary>
        public TimeSpan ElapsedTime { get; set; }

        public static ExportResult Success(byte[] fileContent, string fileName, int tableCount, int totalRowCount, List<ExportTableSummary> summaries, TimeSpan elapsed)
        {
            return new ExportResult
            {
                IsSuccess = true,
                FileContent = fileContent,
                FileName = fileName,
                TableCount = tableCount,
                TotalRowCount = totalRowCount,
                TableSummaries = summaries,
                ElapsedTime = elapsed
            };
        }

        public static ExportResult Failure(string errorMessage)
        {
            return new ExportResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }

    /// <summary>
    /// 單一資料表的匯出摘要
    /// </summary>
    public class ExportTableSummary
    {
        /// <summary>DbSet 名稱</summary>
        public string DbSetName { get; set; } = string.Empty;

        /// <summary>Entity 短名</summary>
        public string EntityShortName { get; set; } = string.Empty;

        /// <summary>匯出的資料行數</summary>
        public int RowCount { get; set; }

        /// <summary>匯出的欄位數</summary>
        public int ColumnCount { get; set; }
    }
}
