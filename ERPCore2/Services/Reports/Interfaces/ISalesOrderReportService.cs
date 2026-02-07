using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 銷貨單報表服務介面
    /// 設計理念：統一使用格式化報表，支援表格框線、圖片嵌入等進階功能
    /// 繼承自 IEntityReportService&lt;SalesOrder&gt;，提供統一的報表服務方法
    /// </summary>
    public interface ISalesOrderReportService : IEntityReportService<SalesOrder>
    {
        // 繼承自 IEntityReportService<SalesOrder> 的所有方法
    }
}
