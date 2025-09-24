using ERPCore2.Data.Entities;
using ERPCore2.Services;

// 使用別名來避免命名衝突
using EntitySalesReturnReason = ERPCore2.Data.Entities.SalesReturnReason;

namespace ERPCore2.Services.Sales
{
    /// <summary>
    /// 銷貨退貨原因服務介面
    /// </summary>
    public interface ISalesReturnReasonService : IGenericManagementService<EntitySalesReturnReason>
    {
        /// <summary>
        /// 取得所有啟用的退貨原因（按排序順序）
        /// </summary>
        Task<List<EntitySalesReturnReason>> GetActiveReasonsAsync();
    }
}