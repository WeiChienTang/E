using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;

namespace ERPCore2.Services.Reports.Interfaces
{
    /// <summary>
    /// 出貨單報表服務介面
    /// 設計理念：統一使用格式化報表，支援表格框線、圖片嵌入等進階功能
    /// 繼承自 IEntityReportService&lt;SalesDelivery&gt;，提供統一的報表服務方法
    /// </summary>
    public interface ISalesDeliveryReportService : IEntityReportService<SalesDelivery>
    {
        // 繼承自 IEntityReportService<SalesDelivery> 的所有方法
    }
}
