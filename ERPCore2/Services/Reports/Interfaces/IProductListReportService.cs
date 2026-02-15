using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 商品資料表報表服務介面
    /// 支援單品列印和批次（清單式）列印
    /// </summary>
    public interface IProductListReportService : IEntityReportService<Product>
    {
        /// <summary>
        /// 批次渲染商品清單報表為圖片（清單式報表，非逐筆列印）
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(ProductListBatchPrintCriteria criteria);
    }
}
