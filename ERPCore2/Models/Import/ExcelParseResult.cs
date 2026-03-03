namespace ERPCore2.Models.Import
{
    /// <summary>
    /// Excel 解析結果 DTO
    /// </summary>
    public class ExcelParseResult
    {
        /// <summary>是否解析成功</summary>
        public bool IsSuccess { get; set; }

        /// <summary>錯誤訊息（若解析失敗）</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>欄位標頭清單（Excel 第一行）</summary>
        public List<string> Headers { get; set; } = new();

        /// <summary>資料行（不含標頭行）。每行為 Dictionary，Key = 標頭名稱。</summary>
        public List<Dictionary<string, string?>> Rows { get; set; } = new();

        /// <summary>總資料行數（不含標頭）</summary>
        public int RowCount => Rows.Count;

        /// <summary>Worksheet 名稱</summary>
        public string WorksheetName { get; set; } = string.Empty;

        public static ExcelParseResult Success(string worksheetName, List<string> headers, List<Dictionary<string, string?>> rows)
        {
            return new ExcelParseResult
            {
                IsSuccess = true,
                WorksheetName = worksheetName,
                Headers = headers,
                Rows = rows
            };
        }

        public static ExcelParseResult Failure(string errorMessage)
        {
            return new ExcelParseResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
