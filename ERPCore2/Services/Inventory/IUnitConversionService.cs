using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services.Inventory
{
    public interface IUnitConversionService : IGenericManagementService<UnitConversion>
    {
        Task<ServiceResult<IEnumerable<UnitConversion>>> GetByUnitAsync(int unitId);
    }
}

