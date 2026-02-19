using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廢料類型服務介面
    /// </summary>
    public interface IWasteTypeService : IGenericManagementService<WasteType>
    {
        Task<WasteType?> GetByCodeAsync(string code);
        Task<bool> IsWasteTypeCodeExistsAsync(string code, int? excludeId = null);
        Task<List<WasteType>> GetActiveWasteTypesAsync();
    }
}
