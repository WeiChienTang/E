using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 採購單報表服務介面
    /// 統一使用格式化報表模式（FormattedDocument），支援表格線、圖片嵌入
    /// 繼承自 IEntityReportService&lt;PurchaseOrder&gt;，提供統一的報表服務方法
    /// </summary>
    public interface IPurchaseOrderReportService : IEntityReportService<PurchaseOrder>
    {
        // 繼承自 IEntityReportService<PurchaseOrder> 的所有方法：
        // - GenerateReportAsync(int entityId)
        // - RenderToImagesAsync(int entityId)
        // - RenderToImagesAsync(int entityId, PaperSetting paperSetting)
        // - DirectPrintAsync(int entityId, string reportId, int copies = 1)
        // - DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
    }
}
