using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.PersonalTools
{
    /// <summary>
    /// 個人通知服務實作（Scoped）
    /// 每位用戶一個獨立實例，所有資料存放在記憶體中。
    /// DB 查詢時機：初始化時、CRUD 後標記 dirty 時、跨日時。
    /// PeriodicTimer 每分鐘呼叫 GetDueRemindersAsync()，純記憶體比對，不查 DB。
    /// </summary>
    public class PersonalNotificationService : IPersonalNotificationService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ILogger<PersonalNotificationService> _logger;

        // ===== 用戶狀態 =====
        private int _employeeId = 0;
        private EmployeePreference _preference = new();

        // ===== 便條貼快取 =====
        private int _stickyNoteCount = 0;
        private bool _notesDirty = true;

        // ===== 行事曆快取 =====
        private List<CalendarEvent> _todayEvents = new();
        private bool _eventsDirty = true;
        private DateOnly _cacheDate = DateOnly.MinValue;

        // ===== 防重複提醒 =====
        private readonly HashSet<int> _alreadyNotifiedIds = new();

        // ===== 通知事件 =====
        public event Action? OnChanged;

        public PersonalNotificationService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<PersonalNotificationService> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        // ===== 公開屬性 =====

        public int StickyNoteCount => _stickyNoteCount;

        public int UpcomingEventCount
        {
            get
            {
                var now = DateTime.Now;
                return _todayEvents.Count(ev =>
                    ev.EventTime.HasValue &&
                    ev.EventDate.ToDateTime(ev.EventTime.Value) > now &&
                    !_alreadyNotifiedIds.Contains(ev.Id));
            }
        }

        // ===== 初始化 =====

        public async Task InitializeAsync(int employeeId, EmployeePreference preference)
        {
            _employeeId = employeeId;
            _preference = preference;
            _notesDirty = true;
            _eventsDirty = true;
            _alreadyNotifiedIds.Clear();
            await RefreshIfNeededAsync();
        }

        // ===== 快取失效 =====

        public void InvalidateNotes()
        {
            _notesDirty = true;
            OnChanged?.Invoke();
        }

        public void InvalidateEvents()
        {
            _eventsDirty = true;
            OnChanged?.Invoke();
        }

        // ===== Timer 呼叫 =====

        public async Task<List<CalendarEvent>> GetDueRemindersAsync()
        {
            await RefreshIfNeededAsync();

            var now = DateTime.Now;
            var due = new List<CalendarEvent>();

            foreach (var ev in _todayEvents)
            {
                // 全天事項無時間，不提醒
                if (!ev.EventTime.HasValue) continue;

                // 已通知過，跳過
                if (_alreadyNotifiedIds.Contains(ev.Id)) continue;

                var reminderMinutes = ev.ReminderMinutes ?? _preference.DefaultReminderMinutes;
                if (reminderMinutes <= 0) continue;

                var eventDateTime = ev.EventDate.ToDateTime(ev.EventTime.Value);
                var minutesUntil = (eventDateTime - now).TotalMinutes;

                // 在提醒時間窗口內（事項尚未過期，且距離到期時間 ≤ reminderMinutes）
                if (minutesUntil >= 0 && minutesUntil <= reminderMinutes)
                {
                    due.Add(ev);
                    _alreadyNotifiedIds.Add(ev.Id);
                }
            }

            return due;
        }

        // ===== 內部：快取刷新 =====

        private async Task RefreshIfNeededAsync()
        {
            if (_employeeId == 0) return;

            var today = DateOnly.FromDateTime(DateTime.Today);

            // 跨日偵測：清除快取，重新載入隔天資料
            if (_cacheDate != today)
            {
                _cacheDate = today;
                _eventsDirty = true;
                _alreadyNotifiedIds.Clear();
            }

            bool changed = false;

            if (_notesDirty)
            {
                try
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    _stickyNoteCount = await context.StickyNotes
                        .CountAsync(n => n.EmployeeId == _employeeId);
                    _notesDirty = false;
                    changed = true;
                }
                catch (Exception ex)
                {
                    await ErrorHandlingHelper.HandleServiceErrorAsync(
                        ex, nameof(RefreshIfNeededAsync), GetType(), _logger,
                        new { Area = "StickyNoteCount", EmployeeId = _employeeId });
                }
            }

            if (_eventsDirty)
            {
                try
                {
                    using var context = await _contextFactory.CreateDbContextAsync();
                    _todayEvents = await context.CalendarEvents
                        .Where(e => e.EmployeeId == _employeeId && e.EventDate == today)
                        .OrderBy(e => e.EventTime)
                        .ToListAsync();
                    _eventsDirty = false;
                    changed = true;
                }
                catch (Exception ex)
                {
                    await ErrorHandlingHelper.HandleServiceErrorAsync(
                        ex, nameof(RefreshIfNeededAsync), GetType(), _logger,
                        new { Area = "TodayEvents", EmployeeId = _employeeId });
                }
            }

            if (changed)
            {
                OnChanged?.Invoke();
            }
        }

        // ===== 資源清理 =====

        public void Dispose()
        {
            // Scoped 服務：Circuit 斷線時由 DI 容器呼叫
            // 無需額外清理（無 Timer、無非受控資源）
        }
    }
}
