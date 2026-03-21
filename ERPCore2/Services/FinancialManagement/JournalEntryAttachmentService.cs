using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERPCore2.Models.Enums;

namespace ERPCore2.Services
{
    /// <summary>
    /// 傳票附件服務
    /// 使用 FileUploadHelper.UploadDocumentAsync 上傳，並按年月目錄組織儲存。
    /// </summary>
    public class JournalEntryAttachmentService : IJournalEntryAttachmentService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<JournalEntryAttachmentService>? _logger;

        private const string SubFolder = "journal-attachments";

        public JournalEntryAttachmentService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<JournalEntryAttachmentService>? logger = null)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<JournalEntryAttachment>> GetByJournalEntryAsync(int journalEntryId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.JournalEntryAttachments
                .Include(a => a.UploadedByEmployee)
                .Where(a => a.JournalEntryId == journalEntryId && a.Status == Models.Enums.EntityStatus.Active)
                .OrderBy(a => a.UploadedAt)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, JournalEntryAttachment? Attachment)> UploadAsync(
            int journalEntryId,
            IBrowserFile file,
            int? uploadedByEmployeeId,
            IWebHostEnvironment environment)
        {
            try
            {
                // 使用年月子目錄組織附件，方便管理與清理
                var now = DateTime.Now;
                var yearMonthFolder = $"{SubFolder}/{now.Year:D4}/{now.Month:D2}";

                var (success, message, filePath, mimeType) = await FileUploadHelper.UploadDocumentToFolderAsync(
                    file, environment, yearMonthFolder);

                if (!success || filePath == null)
                    return (false, message, null);

                var attachment = new JournalEntryAttachment
                {
                    JournalEntryId        = journalEntryId,
                    FileName              = file.Name,
                    StoredFilePath        = filePath,
                    FileSize              = file.Size,
                    ContentType           = mimeType ?? FileUploadHelper.GetDocumentMimeType(
                                               Path.GetExtension(file.Name).ToLowerInvariant()),
                    UploadedAt            = now,
                    UploadedByEmployeeId  = uploadedByEmployeeId
                };

                using var context = await _contextFactory.CreateDbContextAsync();
                context.JournalEntryAttachments.Add(attachment);
                await context.SaveChangesAsync();

                // 若有 Employee ID，載入導航屬性供 UI 立即顯示
                if (uploadedByEmployeeId.HasValue)
                {
                    attachment.UploadedByEmployee = await context.Employees
                        .FirstOrDefaultAsync(e => e.Id == uploadedByEmployeeId.Value);
                }

                return (true, "附件上傳成功", attachment);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "上傳傳票附件時發生錯誤（JournalEntryId={Id}）", journalEntryId);
                return (false, $"上傳失敗：{ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(
            int attachmentId,
            IWebHostEnvironment environment)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var attachment = await context.JournalEntryAttachments
                    .FirstOrDefaultAsync(a => a.Id == attachmentId);

                if (attachment == null)
                    return (false, "找不到指定附件");

                // 先刪除實體檔案
                FileUploadHelper.DeleteFile(attachment.StoredFilePath, environment);

                // 再刪除資料庫記錄
                context.JournalEntryAttachments.Remove(attachment);
                await context.SaveChangesAsync();

                return (true, "附件已刪除");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "刪除傳票附件時發生錯誤（AttachmentId={Id}）", attachmentId);
                return (false, $"刪除失敗：{ex.Message}");
            }
        }
    }
}
