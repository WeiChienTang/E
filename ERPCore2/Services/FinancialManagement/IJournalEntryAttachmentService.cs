using ERPCore2.Data.Entities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;

namespace ERPCore2.Services
{
    /// <summary>
    /// 傳票附件服務介面
    /// 負責傳票附件的查詢、上傳、刪除（附件不影響帳務，已過帳傳票亦可操作）
    /// </summary>
    public interface IJournalEntryAttachmentService
    {
        /// <summary>取得指定傳票的所有附件（按上傳時間排序）</summary>
        Task<List<JournalEntryAttachment>> GetByJournalEntryAsync(int journalEntryId);

        /// <summary>
        /// 上傳附件並建立資料庫記錄
        /// 支援 PDF、Word、Excel、圖片（最大 20MB）
        /// 儲存路徑：wwwroot/uploads/journal-attachments/{year}/{month}/
        /// </summary>
        Task<(bool Success, string Message, JournalEntryAttachment? Attachment)> UploadAsync(
            int journalEntryId,
            IBrowserFile file,
            int? uploadedByEmployeeId,
            IWebHostEnvironment environment);

        /// <summary>刪除附件（同時刪除實體檔案和資料庫記錄）</summary>
        Task<(bool Success, string Message)> DeleteAsync(
            int attachmentId,
            IWebHostEnvironment environment);
    }
}
