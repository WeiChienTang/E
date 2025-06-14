using ERPCore2.Data.Entities;
using ERPCore2.Services.Interfaces;

namespace ERPCore2.Services.Inventory
{
    public interface IUnitConversionService : IGenericManagementService<UnitConversion>
    {
        Task<ServiceResult<IEnumerable<UnitConversion>>> GetByUnitAsync(int unitId);
        Task<ServiceResult<decimal?>> GetConversionRateAsync(int fromUnitId, int toUnitId);
    }
}
