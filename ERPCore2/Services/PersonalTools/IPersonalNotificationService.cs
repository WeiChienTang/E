using ERPCore2.Data.Entities;

namespace ERPCore2.Services.PersonalTools
{
    /// <summary>
    /// 個人通知服務介面（Scoped — 每位用戶一個實例）
    /// 使用記憶體快取，避免每分鐘 Timer 造成 DB 負荷。
    /// DB 僅在初始化或快取失效（CRUD 後）時才查詢。
    /// </summary>
    public interface IPersonalNotificationService : IDisposable
    {
        /// <summary>便條貼數量（記憶體快取）</summary>
        int StickyNoteCount { get; }

        /// <summary>今日尚未到期、有設定提醒的事項數量（記憶體快取）</summary>
        int UpcomingEventCount { get; }

        /// <summary>通知狀態變更時觸發（由 MainLayout 訂閱以更新 UI）</summary>
        event Action? OnChanged;

        /// <summary>
        /// 初始化快取（登入後由 MainLayout 呼叫一次）
        /// </summary>
        Task InitializeAsync(int employeeId, EmployeePreference preference);

        /// <summary>
        /// 標記便條貼快取為 dirty（CRUD 後呼叫，下次 Timer 重查）
        /// </summary>
        void InvalidateNotes();

        /// <summary>
        /// 標記行事曆事項快取為 dirty（CRUD 後呼叫，下次 Timer 重查）
        /// </summary>
        void InvalidateEvents();

        /// <summary>
        /// 檢查今日到期的提醒事項（由 MainLayout PeriodicTimer 每分鐘呼叫）
        /// 純記憶體比對，不查 DB（除非快取 dirty）
        /// </summary>
        Task<List<CalendarEvent>> GetDueRemindersAsync();
    }
}
