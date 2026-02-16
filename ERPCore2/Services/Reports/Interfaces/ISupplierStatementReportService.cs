using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 廠商對帳單報表服務介面
    /// 產生指定期間的廠商對帳單，包含進貨、退貨、付款明細及期初期末餘額
    /// </summary>
    public interface ISupplierStatementReportService
    {
        /// <summary>
        /// 渲染廠商對帳單報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(SupplierStatementCriteria criteria);
    }
}
