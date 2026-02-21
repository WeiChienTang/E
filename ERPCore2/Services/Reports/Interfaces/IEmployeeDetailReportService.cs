using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 員工詳細資料報表服務介面（HR002）
    /// 每位員工各佔一區塊，顯示完整聯絡與任職資訊
    /// 繼承自 IEntityReportService&lt;Employee&gt;，提供統一的報表服務方法
    /// </summary>
    public interface IEmployeeDetailReportService : IEntityReportService<Employee>
    {
        /// <summary>
        /// 以員工名冊篩選條件批次渲染詳細格式報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(EmployeeRosterCriteria criteria);
    }
}
