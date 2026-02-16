using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 車輛保養表報表服務介面
    /// 繼承自 IEntityReportService&lt;VehicleMaintenance&gt;，提供統一的報表服務方法
    /// 額外提供以 VehicleMaintenanceCriteria 為條件的批次報表方法
    /// </summary>
    public interface IVehicleMaintenanceReportService : IEntityReportService<VehicleMaintenance>
    {
        /// <summary>
        /// 以車輛保養表專用篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(VehicleMaintenanceCriteria criteria);
    }
}
