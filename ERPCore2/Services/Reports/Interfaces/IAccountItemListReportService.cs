using ERPCore2.Data.Entities;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 會計科目表報表服務介面
    /// 繼承自 IEntityReportService&lt;AccountItem&gt;，提供統一的報表服務方法
    /// 額外提供以 AccountItemListCriteria 為條件的批次報表方法
    /// </summary>
    public interface IAccountItemListReportService : IEntityReportService<AccountItem>
    {
        /// <summary>
        /// 以會計科目表專用篩選條件渲染報表為圖片
        /// </summary>
        Task<BatchPreviewResult> RenderBatchToImagesAsync(AccountItemListCriteria criteria);
    }
}
