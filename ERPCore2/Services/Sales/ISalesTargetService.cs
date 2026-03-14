using ERPCore2.Data.Entities;
using ERPCore2.Models;

namespace ERPCore2.Services
{
    /// <summary>
    /// 業績目標服務介面
    /// </summary>
    public interface ISalesTargetService : IGenericManagementService<SalesTarget>
    {
        /// <summary>取得指定年度所有目標</summary>
        Task<List<SalesTarget>> GetByYearAsync(int year);

        /// <summary>取得指定年月的目標清單（含公司整體目標）</summary>
        Task<List<SalesTarget>> GetByYearMonthAsync(int year, int? month);

        /// <summary>取得指定業務員的目標清單</summary>
        Task<List<SalesTarget>> GetBySalespersonAsync(int salespersonId);

        /// <summary>
        /// 取得業績達成資料：目標 + 實際出貨金額
        /// </summary>
        Task<List<SalesAchievementItem>> GetAchievementAsync(int year, int? month = null);

        /// <summary>
        /// 確認指定年月+業務員組合是否已有目標記錄（用於防止重複）
        /// </summary>
        Task<bool> IsTargetExistsAsync(int year, int? month, int? salespersonId, int? excludeId = null);

        Task<(List<SalesTarget> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<SalesTarget>, IQueryable<SalesTarget>>? filterFunc,
            int pageNumber,
            int pageSize);

        /// <summary>
        /// 取得指定年度的矩陣格式目標（每列=業務員，欄=各月）
        /// </summary>
        Task<List<SalesTargetMatrixRow>> GetMatrixByYearAsync(int year, List<Employee> employees);

        /// <summary>
        /// 批次新增/更新指定年度的目標（金額為 0 視為刪除該目標）
        /// </summary>
        Task BatchUpsertAsync(int year, List<SalesTargetMatrixRow> rows);

        /// <summary>
        /// 取得所有有資料的年份清單（降冪排列，最新年份在前）
        /// 若當年度無資料，仍會補入當年
        /// </summary>
        Task<List<int>> GetAvailableYearsAsync();
    }

    /// <summary>業績達成彙總資料（目標 vs 實際）</summary>
    public class SalesAchievementItem
    {
        public int? SalespersonId { get; set; }
        public string SalespersonName { get; set; } = "";
        public decimal TargetAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal AchievementRate => TargetAmount > 0
            ? Math.Round(ActualAmount / TargetAmount * 100, 1)
            : 0;
        public bool IsCompanyTotal => SalespersonId == null;
    }
}
