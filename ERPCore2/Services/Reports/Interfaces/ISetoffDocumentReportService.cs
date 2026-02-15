using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 沖款單報表服務介面
    /// 適用於應收沖款單（FN003）和應付沖款單（FN004）
    /// 繼承自 IEntityReportService&lt;SetoffDocument&gt;，提供統一的報表服務方法
    /// </summary>
    public interface ISetoffDocumentReportService : IEntityReportService<SetoffDocument>
    {
        // 繼承自 IEntityReportService<SetoffDocument> 的所有方法
    }
}