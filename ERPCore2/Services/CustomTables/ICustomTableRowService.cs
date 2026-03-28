using ERPCore2.Data.Entities.CustomTables;

namespace ERPCore2.Services.CustomTables
{
    /// <summary>
    /// 自訂資料表資料列服務介面 - 管理資料列及其欄位值（EAV 模式）
    /// </summary>
    public interface ICustomTableRowService : IGenericManagementService<CustomTableRow>
    {
        /// <summary>
        /// 取得指定表的所有資料列（含欄位值）
        /// </summary>
        Task<List<CustomTableRow>> GetRowsByTableIdAsync(int tableDefinitionId);

        /// <summary>
        /// 取得單筆資料列（含欄位值和欄位定義）
        /// </summary>
        Task<CustomTableRow?> GetByIdWithValuesAsync(int rowId);

        /// <summary>
        /// 建立資料列及其欄位值（單一交易）
        /// </summary>
        Task<ServiceResult<CustomTableRow>> CreateRowWithValuesAsync(
            CustomTableRow row, List<CustomFieldValue> fieldValues);

        /// <summary>
        /// 更新資料列及其欄位值（單一交易，差異更新）
        /// </summary>
        Task<ServiceResult<CustomTableRow>> UpdateRowWithValuesAsync(
            CustomTableRow row, List<CustomFieldValue> fieldValues);

        /// <summary>
        /// 分頁查詢指定表的資料列
        /// </summary>
        /// <param name="isDraft">null=正式資料, true=草稿, false=全部</param>
        Task<(List<CustomTableRow> Items, int TotalCount)> GetPagedByTableIdAsync(
            int tableDefinitionId, int pageNumber, int pageSize, string? searchTerm = null, bool? isDraft = null);

        /// <summary>
        /// 驗證欄位值（型別檢查、必填檢查）
        /// </summary>
        Task<ServiceResult> ValidateFieldValuesAsync(
            int tableDefinitionId, List<CustomFieldValue> fieldValues);
    }
}
