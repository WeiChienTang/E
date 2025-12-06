using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 生產排程分配服務介面
    /// 用於追蹤生產數量分配到哪些銷售訂單
    /// </summary>
    public interface IProductionScheduleAllocationService : IGenericManagementService<ProductionScheduleAllocation>
    {
        /// <summary>
        /// 根據生產項目ID取得分配列表
        /// </summary>
        Task<List<ProductionScheduleAllocation>> GetByScheduleItemIdAsync(int scheduleItemId);

        /// <summary>
        /// 根據銷售訂單明細ID取得分配列表
        /// </summary>
        Task<List<ProductionScheduleAllocation>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId);

        /// <summary>
        /// 取得生產項目的已分配總數量
        /// </summary>
        Task<decimal> GetTotalAllocatedQuantityAsync(int scheduleItemId);

        /// <summary>
        /// 批次建立分配紀錄
        /// </summary>
        Task<ServiceResult> CreateAllocationsAsync(int scheduleItemId, List<ProductionScheduleAllocation> allocations);

        /// <summary>
        /// 更新分配紀錄
        /// </summary>
        Task<ServiceResult> UpdateAllocationsAsync(int scheduleItemId, List<ProductionScheduleAllocation> allocations);
    }
}
