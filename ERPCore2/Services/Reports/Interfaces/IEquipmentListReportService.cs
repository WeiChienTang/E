using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 設備清單報表服務介面
    /// </summary>
    public interface IEquipmentListReportService : IEntityReportService<Equipment>
    {
        Task<BatchPreviewResult> RenderBatchToImagesAsync(EquipmentCriteria criteria);
    }
}
