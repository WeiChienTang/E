using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 員工名冊表報表服務介面
    /// 繼承自 IEntityReportService&lt;Employee&gt;，提供統一的報表服務方法
    /// 額外提供以 EmployeeRosterCriteria 為條件的批次報表方法
    /// </summary>
    public interface IEmployeeRosterReportService : IEntityReportService<Employee>
    {
        /// <summary>
        /// 以員工名冊專用篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(EmployeeRosterCriteria criteria);
    }
}
