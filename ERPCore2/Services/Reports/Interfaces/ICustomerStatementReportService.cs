using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 客戶對帳單報表服務介面
    /// 產生指定期間的客戶對帳單，包含出貨、退貨、收款明細及期初期末餘額
    /// </summary>
    public interface ICustomerStatementReportService
    {
        /// <summary>
        /// 渲染客戶對帳單報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerStatementCriteria criteria);
    }
}
