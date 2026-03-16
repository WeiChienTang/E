using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.PersonalTools
{
    /// <summary>
    /// 便條貼服務實作
    /// </summary>
    public class StickyNoteService : IStickyNoteService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<StickyNoteService> _logger;

        public StickyNoteService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<StickyNoteService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<List<StickyNote>> GetByEmployeeIdAsync(int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StickyNotes
                    .Where(n => n.EmployeeId == employeeId)
                    .OrderByDescending(n => n.UpdatedAt)
                    .ThenByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetByEmployeeIdAsync), GetType(), _logger);
                return new List<StickyNote>();
            }
        }

        public async Task<List<StickyNote>> SearchAsync(int employeeId, string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return await GetByEmployeeIdAsync(employeeId);

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.StickyNotes
                    .Where(n => n.EmployeeId == employeeId && n.Content.Contains(keyword))
                    .OrderByDescending(n => n.UpdatedAt)
                    .ThenByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(SearchAsync), GetType(), _logger);
                return new List<StickyNote>();
            }
        }

        public async Task<ServiceResult<StickyNote>> CreateAsync(int employeeId, string content, StickyNoteColor color)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    return ServiceResult<StickyNote>.Failure("便條內容不可為空");

                if (content.Length > 1000)
                    return ServiceResult<StickyNote>.Failure("便條內容不可超過 1000 字元");

                using var context = await _contextFactory.CreateDbContextAsync();
                var note = new StickyNote
                {
                    EmployeeId = employeeId,
                    Content = content.Trim(),
                    Color = color,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.StickyNotes.Add(note);
                await context.SaveChangesAsync();
                return ServiceResult<StickyNote>.Success(note);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CreateAsync), GetType(), _logger);
                return ServiceResult<StickyNote>.Failure("新增便條失敗");
            }
        }

        public async Task<ServiceResult<StickyNote>> UpdateAsync(int noteId, int employeeId, string content, StickyNoteColor color)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    return ServiceResult<StickyNote>.Failure("便條內容不可為空");

                if (content.Length > 1000)
                    return ServiceResult<StickyNote>.Failure("便條內容不可超過 1000 字元");

                using var context = await _contextFactory.CreateDbContextAsync();
                var note = await context.StickyNotes
                    .FirstOrDefaultAsync(n => n.Id == noteId && n.EmployeeId == employeeId);

                if (note == null)
                    return ServiceResult<StickyNote>.Failure("便條不存在或無權限修改");

                note.Content = content.Trim();
                note.Color = color;
                note.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return ServiceResult<StickyNote>.Success(note);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(UpdateAsync), GetType(), _logger);
                return ServiceResult<StickyNote>.Failure("更新便條失敗");
            }
        }

        public async Task<ServiceResult> DeleteAsync(int noteId, int employeeId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var note = await context.StickyNotes
                    .FirstOrDefaultAsync(n => n.Id == noteId && n.EmployeeId == employeeId);

                if (note == null)
                    return ServiceResult.Failure("便條不存在或無權限刪除");

                context.StickyNotes.Remove(note);
                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(DeleteAsync), GetType(), _logger);
                return ServiceResult.Failure("刪除便條失敗");
            }
        }
    }
}
