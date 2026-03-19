using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 品項照片服務介面
    /// </summary>
    public interface IItemPhotoService : IGenericManagementService<ItemPhoto>
    {
        Task<List<ItemPhoto>> GetByItemAsync(int productId);
    }
}
