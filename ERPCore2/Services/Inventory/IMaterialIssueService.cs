using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 領貨服務介面
    /// </summary>
    public interface IMaterialIssueService : IGenericManagementService<MaterialIssue>
    {
        /// <summary>
        /// 檢查領貨編號是否已存在
        /// </summary>
        /// <param name="code">領貨編號</param>
        /// <param name="excludeId">排除的ID（用於編輯模式）</param>
        /// <returns>是否存在</returns>
        Task<bool> IsMaterialIssueCodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// 生成領貨單號
        /// </summary>
        /// <returns>領貨單號 (格式: MI + yyyyMMdd + 流水號)</returns>
        Task<string> GenerateIssueNumberAsync();

        /// <summary>
        /// 取得領貨單及其明細
        /// </summary>
        /// <param name="materialIssueId">領貨單ID</param>
        /// <returns>包含明細的領貨單</returns>
        Task<MaterialIssue?> GetWithDetailsAsync(int materialIssueId);

        /// <summary>
        /// 取得領貨明細列表
        /// </summary>
        /// <param name="materialIssueId">領貨單ID</param>
        /// <returns>明細列表</returns>
        Task<List<MaterialIssueDetail>> GetDetailsAsync(int materialIssueId);

        /// <summary>
        /// 依日期範圍查詢領貨單
        /// </summary>
        /// <param name="startDate">起始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>領貨單列表</returns>
        Task<List<MaterialIssue>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// 依員工查詢領貨單
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>領貨單列表</returns>
        Task<List<MaterialIssue>> GetByEmployeeAsync(int employeeId);

        /// <summary>
        /// 依部門查詢領貨單
        /// </summary>
        /// <param name="departmentId">部門ID</param>
        /// <returns>領貨單列表</returns>
        Task<List<MaterialIssue>> GetByDepartmentAsync(int departmentId);

        /// <summary>
        /// 更新領料單的庫存（差異更新模式）
        /// </summary>
        /// <param name="id">領料單ID</param>
        /// <param name="updatedBy">更新人員ID</param>
        /// <returns>更新結果</returns>
        Task<ServiceResult> UpdateInventoryByDifferenceAsync(int id, int updatedBy = 0);

        /// <summary>
        /// 取得指定品項最近一次領料所使用的倉庫和庫位
        /// </summary>
        /// <param name="productId">品項ID</param>
        /// <returns>(WarehouseId, WarehouseLocationId) 或 null（無歷史記錄）</returns>
        Task<(int WarehouseId, int? WarehouseLocationId)?> GetLastIssuedLocationForItemAsync(int productId);

        /// <summary>
        /// 伺服器端分頁查詢（僅取列表所需欄位）。
        /// </summary>
        Task<(List<MaterialIssue> Items, int TotalCount)> GetPagedWithFiltersAsync(
            Func<IQueryable<MaterialIssue>, IQueryable<MaterialIssue>>? filterFunc,
            int pageNumber,
            int pageSize);
    }
}
