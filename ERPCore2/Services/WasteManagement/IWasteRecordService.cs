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
    }
}
