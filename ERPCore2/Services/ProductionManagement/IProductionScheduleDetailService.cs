using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 生產排程明細服務介面
    /// </summary>
    public interface IProductionScheduleDetailService : IGenericManagementService<ProductionScheduleDetail>
    {
        /// <summary>
        /// 根據排程主檔ID取得明細列表
        /// </summary>
        Task<List<ProductionScheduleDetail>> GetByScheduleIdAsync(int scheduleId);

        /// <summary>
        /// 根據組件商品ID取得明細列表
        /// </summary>
        Task<List<ProductionScheduleDetail>> GetByComponentProductIdAsync(int productId);

        /// <summary>
        /// 根據倉庫ID取得明細列表
        /// </summary>
        Task<List<ProductionScheduleDetail>> GetByWarehouseIdAsync(int warehouseId);

        /// <summary>
        /// 批次建立明細
        /// </summary>
        Task<ServiceResult> CreateDetailsAsync(int scheduleId, List<ProductionScheduleDetail> details);

        /// <summary>
        /// 批次更新明細
        /// </summary>
        Task<ServiceResult> UpdateDetailsAsync(int scheduleId, List<ProductionScheduleDetail> details);

        /// <summary>
        /// 刪除排程的所有明細
        /// </summary>
        Task<ServiceResult> DeleteByScheduleIdAsync(int scheduleId);
    }
}
