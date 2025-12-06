using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 生產排程明細服務介面
    /// 明細現在關聯到 ProductionScheduleItem（生產項目）
    /// </summary>
    public interface IProductionScheduleDetailService : IGenericManagementService<ProductionScheduleDetail>
    {
        /// <summary>
        /// 根據生產項目ID取得明細列表
        /// </summary>
        Task<List<ProductionScheduleDetail>> GetByScheduleItemIdAsync(int scheduleItemId);

        /// <summary>
        /// 根據組件商品ID取得明細列表
        /// </summary>
        Task<List<ProductionScheduleDetail>> GetByComponentProductIdAsync(int productId);

        /// <summary>
        /// 根據倉庫ID取得明細列表
        /// </summary>
        Task<List<ProductionScheduleDetail>> GetByWarehouseIdAsync(int warehouseId);

        /// <summary>
        /// 批次建立明細（為生產項目）
        /// </summary>
        Task<ServiceResult> CreateDetailsForItemAsync(int scheduleItemId, List<ProductionScheduleDetail> details);

        /// <summary>
        /// 批次更新明細（為生產項目）
        /// </summary>
        Task<ServiceResult> UpdateDetailsForItemAsync(int scheduleItemId, List<ProductionScheduleDetail> details);

        /// <summary>
        /// 刪除生產項目的所有明細
        /// </summary>
        Task<ServiceResult> DeleteByScheduleItemIdAsync(int scheduleItemId);
    }
}
