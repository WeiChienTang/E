using ERPCore2.Data.Entities;

namespace ERPCore2.Services.PersonalTools
{
    /// <summary>
    /// 便條貼服務介面
    /// 所有操作皆以 EmployeeId 為範圍，確保使用者只能存取自己的便條
    /// </summary>
    public interface IStickyNoteService
    {
        /// <summary>
        /// 取得員工的所有便條（依更新時間倒序）
        /// </summary>
        Task<List<StickyNote>> GetByEmployeeIdAsync(int employeeId);

        /// <summary>
        /// 依關鍵字搜尋便條內容
        /// </summary>
        Task<List<StickyNote>> SearchAsync(int employeeId, string keyword);

        /// <summary>
        /// 新增便條
        /// </summary>
        Task<ServiceResult<StickyNote>> CreateAsync(int employeeId, string content, StickyNoteColor color);

        /// <summary>
        /// 更新便條內容與顏色
        /// </summary>
        Task<ServiceResult<StickyNote>> UpdateAsync(int noteId, int employeeId, string content, StickyNoteColor color);

        /// <summary>
        /// 刪除便條（確保只能刪除自己的）
        /// </summary>
        Task<ServiceResult> DeleteAsync(int noteId, int employeeId);
    }
}
