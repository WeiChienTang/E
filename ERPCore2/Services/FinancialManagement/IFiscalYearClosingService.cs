namespace ERPCore2.Services
{
    /// <summary>
    /// 年底結帳服務介面
    /// 執行損益科目歸零 → 本期損益(3353) → 累積盈虧(3351) → 鎖定期間 → 初始化下年度
    /// </summary>
    public interface IFiscalYearClosingService
    {
        /// <summary>
        /// 年底結帳前置檢查（不執行實際結帳，僅回傳可行性及預估損益）
        /// </summary>
        Task<YearEndClosingPreCheck> PreCheckAsync(int year, int companyId);

        /// <summary>
        /// 執行年底結帳
        /// Step 1：損益科目餘額歸零，轉入本期損益 (3353)
        /// Step 2：本期損益 (3353) 轉入累積盈虧 (3351)
        /// Step 3：鎖定所有年度期間（Closed → Locked）
        /// Step 4：初始化下一年度 12 個期間
        /// </summary>
        Task<(bool Success, string ErrorMessage)> ExecuteYearEndClosingAsync(int year, int companyId, string executedBy);
    }

    /// <summary>
    /// 年底結帳前置檢查結果
    /// </summary>
    public class YearEndClosingPreCheck
    {
        /// <summary>是否可以執行結帳</summary>
        public bool CanClose { get; set; }

        /// <summary>該年度已建立的期間數</summary>
        public int TotalPeriods { get; set; }

        /// <summary>已關帳或已鎖定的期間數</summary>
        public int ClosedOrLockedPeriods { get; set; }

        /// <summary>仍開放中的期間（阻擋結帳）</summary>
        public List<int> OpenPeriodNumbers { get; set; } = new();

        /// <summary>該年度是否已有 Closing 傳票（已結帳）</summary>
        public bool AlreadyClosed { get; set; }

        /// <summary>預估本期損益（正值=盈利，負值=虧損）</summary>
        public decimal EstimatedNetIncome { get; set; }

        /// <summary>阻擋結帳的錯誤清單</summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>提示但不阻擋的警告清單</summary>
        public List<string> Warnings { get; set; } = new();
    }
}
