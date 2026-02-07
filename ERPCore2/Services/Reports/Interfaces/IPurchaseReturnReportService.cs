using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 進貨退出單報表服務介面
    /// 統一使用格式化報表模式（FormattedDocument），支援表格線、圖片嵌入
    /// 繼承自 IEntityReportService&lt;PurchaseReturn&gt;，提供統一的報表服務方法
    /// </summary>
    public interface IPurchaseReturnReportService : IEntityReportService<PurchaseReturn>
    {
        // 繼承自 IEntityReportService<PurchaseReturn> 的所有方法
    }
}
