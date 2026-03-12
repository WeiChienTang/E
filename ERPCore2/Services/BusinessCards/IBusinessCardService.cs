using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 名片照片服務介面
    /// </summary>
    public interface IBusinessCardService : IGenericManagementService<BusinessCard>
    {
        /// <summary>
        /// 取得指定擁有者的所有名片（依 SortOrder 排序）
        /// </summary>
        Task<List<BusinessCard>> GetByOwnerAsync(string ownerType, int ownerId);
    }
}
