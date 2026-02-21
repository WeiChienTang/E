using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 商品詳細資料報表服務介面（PD005）
    /// 每項商品各佔一區塊，顯示完整規格、分類、採購類型與成本資訊
    /// 繼承自 IEntityReportService&lt;Product&gt;，提供統一的報表服務方法
    /// </summary>
    public interface IProductDetailReportService : IEntityReportService<Product>
    {
        /// <summary>
        /// 以商品清單篩選條件批次渲染詳細格式報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(ProductListBatchPrintCriteria criteria);
    }
}
