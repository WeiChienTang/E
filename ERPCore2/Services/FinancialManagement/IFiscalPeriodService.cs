using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services
{
    public interface IFiscalPeriodService : IGenericManagementService<FiscalPeriod>
    {
        /// <summary>
        /// 依年度取得所有期間
        /// </summary>
        Task<List<FiscalPeriod>> GetByYearAsync(int year, int companyId);

        /// <summary>
        /// 依年度與期間編號取得期間
        /// </summary>
        Task<FiscalPeriod?> GetByYearAndPeriodAsync(int year, int periodNumber, int companyId);

        /// <summary>
        /// 取得目前開放中的期間（日期在 StartDate~EndDate 之間且狀態為 Open）
        /// </summary>
        Task<FiscalPeriod?> GetCurrentPeriodAsync(int companyId);

        /// <summary>
        /// 取得所有開放中的期間
        /// </summary>
        Task<List<FiscalPeriod>> GetOpenPeriodsAsync(int companyId);

        /// <summary>
        /// 判斷指定日期是否在某個開放中的期間內
        /// </summary>
        Task<bool> IsDateInOpenPeriodAsync(DateTime date, int companyId);

        /// <summary>
        /// 判斷指定年度期間是否已存在
        /// </summary>
        Task<bool> IsYearPeriodExistsAsync(int year, int periodNumber, int companyId, int? excludeId = null);

        /// <summary>
        /// 關帳（Open → Closed），記錄關帳人員
        /// </summary>
        Task<(bool Success, string ErrorMessage)> ClosePeriodAsync(int id, int? closedByEmployeeId = null);

        /// <summary>
        /// 鎖定（Closed → Locked）
        /// </summary>
        Task<(bool Success, string ErrorMessage)> LockPeriodAsync(int id);

        /// <summary>
        /// 重新開放（Closed → Open），記錄重開原因
        /// </summary>
        Task<(bool Success, string ErrorMessage)> ReopenPeriodAsync(int id, string? reason = null);

        /// <summary>
        /// 初始化指定年度的 12 個月份期間（已存在的月份跳過）
        /// 用於公司首次啟用、年底結帳後自動初始化下一年度
        /// </summary>
        Task<(bool Success, string ErrorMessage, int CreatedCount)> InitializeYearAsync(int year, int companyId);

        /// <summary>
        /// 伺服器端分頁查詢
        /// </summary>
        Task<(List<FiscalPeriod> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<FiscalPeriod>, IQueryable<FiscalPeriod>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
