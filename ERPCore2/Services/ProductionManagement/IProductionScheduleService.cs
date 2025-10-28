using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 生產排程服務介面
    /// </summary>
    public interface IProductionScheduleService : IGenericManagementService<ProductionSchedule>
    {
        /// <summary>
        /// 檢查排程單號是否已存在
        /// </summary>
        Task<bool> IsScheduleNumberExistsAsync(string scheduleNumber, int? excludeId = null);

        /// <summary>
        /// 根據客戶取得排程列表
        /// </summary>
        Task<List<ProductionSchedule>> GetByCustomerIdAsync(int customerId);

        /// <summary>
        /// 根據來源單據取得排程列表
        /// </summary>
        Task<List<ProductionSchedule>> GetBySourceDocumentAsync(string sourceDocumentType, int sourceDocumentId);

        /// <summary>
        /// 根據日期範圍取得排程列表
        /// </summary>
        Task<List<ProductionSchedule>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 取得排程詳細資訊（含明細）
        /// </summary>
        Task<ProductionSchedule?> GetWithDetailsAsync(int id);
    }
}
