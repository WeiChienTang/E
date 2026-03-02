using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 廠商拜訪紀錄服務介面
    /// </summary>
    public interface ISupplierVisitService : IGenericManagementService<SupplierVisit>
    {
        /// <summary>
        /// 取得指定廠商的所有拜訪紀錄（依拜訪日期降序）
        /// </summary>
        Task<List<SupplierVisit>> GetBySupplierAsync(int supplierId);
    }
}
