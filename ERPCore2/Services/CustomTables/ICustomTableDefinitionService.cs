using ERPCore2.Data.Entities.CustomTables;

namespace ERPCore2.Services.CustomTables
{
    /// <summary>
    /// 自訂資料表定義服務介面 - 管理表定義及其欄位定義
    /// </summary>
    public interface ICustomTableDefinitionService : IGenericManagementService<CustomTableDefinition>
    {
        /// <summary>
        /// 取得表定義（含所有欄位定義）
        /// </summary>
        Task<CustomTableDefinition?> GetByIdWithFieldsAsync(int id);

        /// <summary>
        /// 檢查表名稱是否已存在
        /// </summary>
        Task<bool> IsTableNameExistsAsync(string tableName, int? excludeId = null);

        // ── 欄位定義管理（子項目）──────────────────────────────

        /// <summary>
        /// 取得指定表的所有欄位定義（按 SortOrder 排序）
        /// </summary>
        Task<List<CustomFieldDefinition>> GetFieldDefinitionsAsync(int tableDefinitionId);

        /// <summary>
        /// 新增欄位定義
        /// </summary>
        Task<ServiceResult<CustomFieldDefinition>> AddFieldDefinitionAsync(CustomFieldDefinition fieldDefinition);

        /// <summary>
        /// 更新欄位定義
        /// </summary>
        Task<ServiceResult<CustomFieldDefinition>> UpdateFieldDefinitionAsync(CustomFieldDefinition fieldDefinition);

        /// <summary>
        /// 刪除欄位定義（同時刪除所有關聯的欄位值）
        /// </summary>
        Task<ServiceResult> DeleteFieldDefinitionAsync(int fieldDefinitionId);

        /// <summary>
        /// 重新排序欄位定義
        /// </summary>
        Task<ServiceResult> ReorderFieldsAsync(int tableDefinitionId, List<int> orderedFieldIds);
    }
}
