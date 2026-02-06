using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// Excel 匯出服務介面
    /// 將 FormattedDocument 轉換為 Excel 檔案
    /// </summary>
    public interface IExcelExportService
    {
        /// <summary>
        /// 將 FormattedDocument 匯出為 Excel 檔案
        /// </summary>
        /// <param name="document">格式化文件</param>
        /// <returns>Excel 檔案的 byte[] (xlsx 格式)</returns>
        byte[] ExportToExcel(FormattedDocument document);

        /// <summary>
        /// 將 FormattedDocument 匯出為 Excel 檔案（非同步版本）
        /// </summary>
        /// <param name="document">格式化文件</param>
        /// <returns>Excel 檔案的 byte[] (xlsx 格式)</returns>
        Task<byte[]> ExportToExcelAsync(FormattedDocument document);

        /// <summary>
        /// 檢查服務是否可用
        /// </summary>
        bool IsSupported();
    }
}
