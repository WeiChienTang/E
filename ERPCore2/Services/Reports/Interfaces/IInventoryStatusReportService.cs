using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 庫存現況表報表服務介面
    /// 繼承自 IEntityReportService&lt;InventoryStock&gt;，提供統一的報表服務方法
    /// 額外提供以 InventoryStatusCriteria 為條件的批次報表方法
    /// </summary>
    public interface IInventoryStatusReportService : IEntityReportService<InventoryStock>
    {
        /// <summary>
        /// 以庫存現況專用篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(InventoryStatusCriteria criteria);
    }
}
