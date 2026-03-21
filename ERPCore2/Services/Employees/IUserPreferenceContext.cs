namespace ERPCore2.Services
{
    /// <summary>
    /// 當前使用者偏好設定的 Circuit-scoped 快取。
    /// 由 MainLayout.OnAfterRenderAsync 從 localStorage 寫入一次（首次登入自動從 DB 遷移），
    /// 各元件注入後直接讀取，避免重複查詢 DB 或 localStorage。
    /// </summary>
    public interface IUserPreferenceContext
    {
        /// <summary>每頁顯示筆數（10 / 20 / 50 / 100）。預設 20。</summary>
        int DefaultPageSize { get; set; }

        /// <summary>離開未儲存表單時是否顯示確認對話框。預設 true。</summary>
        bool ShowUnsavedChangesWarning { get; set; }
    }

    public class UserPreferenceContext : IUserPreferenceContext
    {
        public int DefaultPageSize { get; set; } = 20;
        public bool ShowUnsavedChangesWarning { get; set; } = true;
    }
}
