using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 磅秤紀錄服務介面
    /// </summary>
    public interface IScaleRecordService : IGenericManagementService<ScaleRecord>
    {
        Task<bool> IsScaleRecordCodeExistsAsync(string code, int? excludeId = null);
        Task<List<ScaleRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<ScaleRecord>> GetByVehicleAsync(int vehicleId);
        Task<List<ScaleRecord>> GetByCustomerAsync(int customerId);

        /// <summary>新增磅秤紀錄後確認入庫（首次建立時呼叫）</summary>
        Task<ServiceResult> ConfirmScaleReceiptAsync(int id);

        /// <summary>編輯磅秤紀錄後先逆轉舊庫存，再以當前數值重新入庫（Void and Repost）</summary>
        Task<ServiceResult> ReverseAndRepostScaleInventoryAsync(int id);

        /// <summary>
        /// 伺服器端分頁查詢（僅取列表所需欄位）。
        /// </summary>
        Task<(List<ScaleRecord> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<ScaleRecord>, IQueryable<ScaleRecord>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
