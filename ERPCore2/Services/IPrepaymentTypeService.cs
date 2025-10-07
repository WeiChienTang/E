using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 預收付款項類型服務介面
    /// </summary>
    public interface IPrepaymentTypeService : IGenericManagementService<PrepaymentType>
    {
        /// <summary>
        /// 根據名稱模糊搜尋預收付款項類型
        /// </summary>
        Task<List<PrepaymentType>> SearchByNameAsync(string keyword);
    }
}
