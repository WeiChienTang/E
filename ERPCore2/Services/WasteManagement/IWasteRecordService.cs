using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廢料記錄服務介面
    /// </summary>
    public interface IWasteRecordService : IGenericManagementService<WasteRecord>
    {
        Task<bool> IsWasteRecordCodeExistsAsync(string code, int? excludeId = null);
        Task<List<WasteRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<WasteRecord>> GetByVehicleAsync(int vehicleId);
        Task<List<WasteRecord>> GetByCustomerAsync(int customerId);

        /// <summary>新增廢料記錄後確認入庫（首次建立時呼叫）</summary>
        Task<ServiceResult> ConfirmWasteReceiptAsync(int id);

        /// <summary>編輯廢料記錄後先逆轉舊庫存，再以當前數值重新入庫（Void and Repost）</summary>
        Task<ServiceResult> ReverseAndRepostWasteInventoryAsync(int id);
    }
}
