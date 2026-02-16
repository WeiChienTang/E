using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 車輛管理表報表服務介面
    /// 繼承自 IEntityReportService&lt;Vehicle&gt;，提供統一的報表服務方法
    /// 額外提供以 VehicleListCriteria 為條件的批次報表方法
    /// </summary>
    public interface IVehicleListReportService : IEntityReportService<Vehicle>
    {
        /// <summary>
        /// 以車輛管理表專用篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(VehicleListCriteria criteria);
    }
}
