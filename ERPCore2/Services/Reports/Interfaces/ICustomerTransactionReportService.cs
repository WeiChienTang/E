using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 客戶交易明細報表服務介面
    /// 查詢客戶所有交易記錄明細（出貨、退貨），依客戶分組顯示
    /// </summary>
    public interface ICustomerTransactionReportService
    {
        /// <summary>
        /// 渲染客戶交易明細報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerTransactionCriteria criteria);
    }
}
