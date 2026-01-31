using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 泛型報表服務介面
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// 生成 HTML 報表
        /// </summary>
        /// <typeparam name="TMainEntity">主要實體類型</typeparam>
        /// <typeparam name="TDetailEntity">明細實體類型</typeparam>
        /// <param name="configuration">報表配置</param>
        /// <param name="reportData">報表資料</param>
        /// <returns>HTML 字串</returns>
        Task<string> GenerateHtmlReportAsync<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class;
        
        /// <summary>
        /// 列印報表（開啟瀏覽器列印對話框）
        /// </summary>
        Task<string> GeneratePrintableReportAsync<TMainEntity, TDetailEntity>(
            ReportConfiguration configuration,
            ReportData<TMainEntity, TDetailEntity> reportData)
            where TMainEntity : class;
    }
}
