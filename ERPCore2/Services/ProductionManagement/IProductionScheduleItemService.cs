using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 生產排程項目服務介面
    /// </summary>
    public interface IProductionScheduleItemService : IGenericManagementService<ProductionScheduleItem>
    {
        /// <summary>
        /// 根據排程主檔ID取得項目列表
        /// </summary>
        Task<List<ProductionScheduleItem>> GetByScheduleIdAsync(int scheduleId);

        /// <summary>
        /// 根據商品ID取得項目列表
        /// </summary>
        Task<List<ProductionScheduleItem>> GetByProductIdAsync(int productId);

        /// <summary>
        /// 根據銷售訂單明細ID取得項目列表
        /// </summary>
        Task<List<ProductionScheduleItem>> GetBySalesOrderDetailIdAsync(int salesOrderDetailId);

        /// <summary>
        /// 根據狀態取得項目列表
        /// </summary>
        Task<List<ProductionScheduleItem>> GetByStatusAsync(ProductionItemStatus status);

        /// <summary>
        /// 取得待生產項目
        /// </summary>
        Task<List<ProductionScheduleItem>> GetPendingItemsAsync();

        /// <summary>
        /// 取得生產中項目
        /// </summary>
        Task<List<ProductionScheduleItem>> GetInProgressItemsAsync();

        /// <summary>
        /// 開始生產 - 將狀態改為 InProgress 並扣除組件庫存
        /// </summary>
        Task<ServiceResult> StartProductionAsync(int itemId);

        /// <summary>
        /// 完成生產入庫
        /// </summary>
        Task<ServiceResult> CompleteProductionAsync(int itemId, decimal completedQuantity, int? warehouseId = null, int? warehouseLocationId = null);

        /// <summary>
        /// 批次建立排程項目（從銷售訂單轉排程）
        /// </summary>
        Task<ServiceResult> CreateItemsFromSalesOrderAsync(int scheduleId, List<ProductionScheduleItem> items);

        /// <summary>
        /// 取得項目含完整明細
        /// </summary>
        Task<ProductionScheduleItem?> GetWithDetailsAsync(int id);

        /// <summary>
        /// 更新已完成數量
        /// </summary>
        Task<ServiceResult> UpdateCompletedQuantityAsync(int itemId, decimal completedQuantity);

        /// <summary>
        /// 依 PlannedStartDate 範圍取得排程項目（供看板視圖使用）
        /// </summary>
        Task<List<ProductionScheduleItem>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 取得尚未排入看板（PlannedStartDate == null）且未結案的項目
        /// </summary>
        Task<List<ProductionScheduleItem>> GetUnscheduledAsync();

        /// <summary>
        /// 更新項目的計畫生產日（拖曳到看板某天時呼叫）
        /// </summary>
        Task<ServiceResult> UpdatePlannedDateAsync(int itemId, DateTime? plannedStartDate);

        /// <summary>
        /// 批次取得所有排程項目的已排程數量（key = SalesOrderDetailId），解決 N+1 問題
        /// </summary>
        Task<Dictionary<int, decimal>> GetScheduledQuantityMapAsync();

        /// <summary>
        /// 將排程項目退回待排清單。
        /// 條件：CompletedQuantity == 0。
        /// 同時扣回 SalesOrderDetail.ScheduledQuantity。
        /// 回傳 bool = true 表示退回前有已發出的領料記錄，應提示使用者手動沖銷。
        /// </summary>
        Task<ServiceResult<bool>> ReturnToSidebarAsync(int itemId);
    }
}
