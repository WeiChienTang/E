using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 聯絡方式服務介面
    /// </summary>
    public interface IContactService : IGenericManagementService<Contact>
    {
        /// <summary>
        /// 根據擁有者取得聯絡方式列表
        /// </summary>
        /// <param name="ownerType">擁有者類型</param>
        /// <param name="ownerId">擁有者ID</param>
        /// <returns>聯絡方式列表</returns>
        Task<List<Contact>> GetByOwnerAsync(string ownerType, int ownerId);
        
        /// <summary>
        /// 取得指定擁有者的主要聯絡方式
        /// </summary>
        /// <param name="ownerType">擁有者類型</param>
        /// <param name="ownerId">擁有者ID</param>
        /// <returns>主要聯絡方式</returns>
        Task<Contact?> GetPrimaryContactAsync(string ownerType, int ownerId);
        
        /// <summary>
        /// 設定主要聯絡方式
        /// </summary>
        /// <param name="contactId">聯絡方式ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> SetPrimaryContactAsync(int contactId);
        
        /// <summary>
        /// 批次刪除指定擁有者的所有聯絡方式
        /// </summary>
        /// <param name="ownerType">擁有者類型</param>
        /// <param name="ownerId">擁有者ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> DeleteByOwnerAsync(string ownerType, int ownerId);
    }
}
