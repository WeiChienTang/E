namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 報表頁面資訊
    /// 包含單一頁面的明細項目和頁面屬性
    /// </summary>
    /// <typeparam name="T">明細項目類型，必須實作 IReportDetailItem</typeparam>
    public class ReportPage<T> where T : IReportDetailItem
    {
        /// <summary>
        /// 此頁包含的明細項目清單
        /// </summary>
        public List<T> Items { get; }

        /// <summary>
        /// 是否為最後一頁（最後一頁需要顯示統計和簽名區域）
        /// </summary>
        public bool IsLastPage { get; }

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="items">此頁包含的明細項目</param>
        /// <param name="isLastPage">是否為最後一頁</param>
        public ReportPage(List<T> items, bool isLastPage)
        {
            Items = items ?? new List<T>();
            IsLastPage = isLastPage;
        }
    }
}
