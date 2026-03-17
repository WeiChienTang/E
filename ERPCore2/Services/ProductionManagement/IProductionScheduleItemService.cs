using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Schedule;
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
        /// 根據品項ID取得項目列表
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
        /// <param name="settlements">用料結算（最後一次完工時傳入，null 表示批次完工不做結算）</param>
        Task<ServiceResult> CompleteProductionAsync(int itemId, decimal completedQuantity,
            int? warehouseId = null, int? warehouseLocationId = null,
            List<MaterialSettlementDto>? settlements = null);

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
        /// 批次取得指定銷貨訂單明細的排程狀態聚合（key = SalesOrderDetailId）
        /// InProgress > WaitingMaterial > Pending；全 Completed 回傳 Completed
        /// </summary>
        Task<Dictionary<int, ProductionItemStatus>> GetAggregateStatusMapAsync(IEnumerable<int> salesOrderDetailIds);

        /// <summary>
        /// 查詢同品項最近一次完工入庫的倉庫/庫位（用於預填）
        /// </summary>
        Task<(int? WarehouseId, int? LocationId)> GetLastCompletionWarehouseAsync(int productId);

        /// <summary>
        /// 批次更新同日內卡片的優先順序（日內拖曳排序）
        /// </summary>
        Task<ServiceResult> UpdatePrioritiesAsync(List<(int Id, int Priority)> updates);

        /// <summary>
        /// 將排程項目退回待排清單。
        /// 條件：CompletedQuantity == 0。
        /// 同時扣回 SalesOrderDetail.ScheduledQuantity。
        /// 回傳 bool = true 表示退回前有已發出的領料記錄，應提示使用者手動沖銷。
        /// </summary>
        Task<ServiceResult<bool>> ReturnToSidebarAsync(int itemId);

        /// <summary>
        /// 暫停生產 - 將 InProgress 狀態改為 Paused，不做任何庫存操作。
        /// </summary>
        Task<ServiceResult> PauseProductionAsync(int itemId);

        /// <summary>
        /// 繼續生產 - 將 Paused 狀態改回 InProgress。
        /// </summary>
        Task<ServiceResult> ResumeProductionAsync(int itemId);

        /// <summary>
        /// 終止：強制結案，不再繼續生產。
        /// 允許狀態：InProgress、Paused、WaitingMaterial（已領料）、Pending（已領料）。
        /// 不寫入成品庫存（已有部分完工則已分批入庫）。
        /// 不調整 SalesOrderDetail.ScheduledQuantity（保持原值，使訂單不重出現在待排 Sidebar）。
        /// 更新 SalesOrderDetail.ProducedQuantity += 已完成數量（如有）。
        /// 若傳入 settlements 則執行退料入庫（選填，不強制等式驗證）。
        /// </summary>
        Task<ServiceResult> AbortProductionAsync(int itemId, List<MaterialSettlementDto>? settlements = null);
    }
}
