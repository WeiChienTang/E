using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 設備保養維修記錄報表服務介面
    /// </summary>
    public interface IEquipmentMaintenanceReportService : IEntityReportService<EquipmentMaintenance>
    {
        Task<BatchPreviewResult> RenderBatchToImagesAsync(EquipmentMaintenanceCriteria criteria);
    }
}
