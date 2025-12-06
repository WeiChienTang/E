using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 生產完成入庫服務介面
    /// </summary>
    public interface IProductionScheduleCompletionService : IGenericManagementService<ProductionScheduleCompletion>
    {
        /// <summary>
        /// 根據生產項目ID取得完成紀錄列表
        /// </summary>
        Task<List<ProductionScheduleCompletion>> GetByScheduleItemIdAsync(int scheduleItemId);

        /// <summary>
        /// 根據日期範圍取得完成紀錄
        /// </summary>
        Task<List<ProductionScheduleCompletion>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 根據倉庫取得完成紀錄
        /// </summary>
        Task<List<ProductionScheduleCompletion>> GetByWarehouseIdAsync(int warehouseId);

        /// <summary>
        /// 取得項目的總完成數量
        /// </summary>
        Task<decimal> GetTotalCompletedQuantityAsync(int scheduleItemId);

        /// <summary>
        /// 建立完成入庫紀錄並更新庫存
        /// </summary>
        Task<ServiceResult> CreateCompletionAsync(ProductionScheduleCompletion completion);
    }
}
