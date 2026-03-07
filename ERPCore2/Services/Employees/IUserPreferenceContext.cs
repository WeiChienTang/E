namespace ERPCore2.Services
{
    /// <summary>
    /// 當前使用者偏好設定的 Circuit-scoped 快取。
    /// 由 MainLayout.OnAfterRenderAsync 從 DB 寫入一次，
    /// GenericIndexPageComponent 等共用元件直接讀取，避免重複 DB 查詢。
    /// </summary>
    public interface IUserPreferenceContext
    {
        /// <summary>每頁顯示筆數（10 / 20 / 50 / 100）。預設 20，未登入或載入前維持此值。</summary>
        int DefaultPageSize { get; set; }
    }

    public class UserPreferenceContext : IUserPreferenceContext
    {
        public int DefaultPageSize { get; set; } = 20;
    }
}
