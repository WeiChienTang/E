namespace ERPCore2.Models.Import
{
    /// <summary>
    /// 預覽行 DTO（Step 4 顯示用）
    /// </summary>
    public class ImportPreviewRow
    {
        /// <summary>Excel 來源行號（1-based，不含標頭行）</summary>
        public int RowNumber { get; set; }

        /// <summary>各欄位的轉換結果（Key = 目標屬性名稱）</summary>
        public Dictionary<string, ImportCellValue> Cells { get; set; } = new();

        /// <summary>此行是否有任何欄位有錯誤</summary>
        public bool HasError => Cells.Values.Any(c => c.HasError);
    }

    /// <summary>
    /// 單一儲存格的轉換結果
    /// </summary>
    public class ImportCellValue
    {
        /// <summary>原始值（來自 Excel 或預設值）</summary>
        public string? RawValue { get; set; }

        /// <summary>轉換後的顯示文字</summary>
        public string DisplayValue { get; set; } = string.Empty;

        /// <summary>轉換是否成功</summary>
        public bool HasError { get; set; }

        /// <summary>錯誤訊息（型別轉換失敗等）</summary>
        public string? ErrorMessage { get; set; }
    }
}
