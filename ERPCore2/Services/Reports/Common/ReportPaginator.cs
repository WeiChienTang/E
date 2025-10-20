namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 通用報表分頁計算器
    /// 根據頁面配置和明細項目，智能計算最佳分頁方式
    /// </summary>
    /// <typeparam name="T">明細項目類型，必須實作 IReportDetailItem</typeparam>
    public class ReportPaginator<T> where T : IReportDetailItem
    {
        private readonly ReportPageLayout _layout;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="layout">報表頁面配置</param>
        public ReportPaginator(ReportPageLayout layout)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        }

        /// <summary>
        /// 估算單筆明細的高度（mm）
        /// 固定行高模式：所有明細項目都使用相同的固定高度
        /// </summary>
        /// <param name="item">明細項目</param>
        /// <returns>此項目需要的高度（mm）</returns>
        public decimal EstimateItemHeight(T item)
        {
            // 固定行高模式：所有項目統一高度，不再考慮備註長度
            return _layout.RowBaseHeight;
        }

        /// <summary>
        /// 將明細清單分割成多頁
        /// 固定行高模式：
        /// 1. 非最後一頁：使用較大的可用空間（不含統計和簽名）
        /// 2. 最後一頁：預留統計和簽名空間
        /// 3. 明細序號連續不跳號
        /// </summary>
        /// <param name="allDetails">所有明細項目</param>
        /// <returns>分頁後的頁面清單</returns>
        public List<ReportPage<T>> SplitIntoPages(List<T> allDetails)
        {
            // 若無明細，返回空頁面清單
            if (allDetails == null || !allDetails.Any())
            {
                return new List<ReportPage<T>>
                {
                    new ReportPage<T>(new List<T>(), true)
                };
            }

            var pages = new List<ReportPage<T>>();
            var currentPageItems = new List<T>();
            decimal currentPageDetailsHeight = 0m;

            for (int i = 0; i < allDetails.Count; i++)
            {
                var item = allDetails[i];
                decimal itemHeight = EstimateItemHeight(item); // 固定行高：8mm
                bool isLastItem = (i == allDetails.Count - 1);

                // 計算加入此項後的明細總高度
                decimal heightAfterAdd = currentPageDetailsHeight + itemHeight;

                // 檢查是否需要分頁
                bool needsNewPage = false;
                
                if (isLastItem)
                {
                    // 最後一筆：檢查是否能放在當前頁（需包含統計和簽名）
                    decimal availableHeightForLastPage = _layout.GetAvailableDetailsHeightForLastPage();
                    needsNewPage = (heightAfterAdd > availableHeightForLastPage && currentPageItems.Count > 0);
                }
                else
                {
                    // 非最後一筆：使用較大的可用空間（不含統計和簽名）
                    decimal availableHeightForNonLastPage = _layout.GetAvailableDetailsHeightForNonLastPage();
                    needsNewPage = (heightAfterAdd > availableHeightForNonLastPage && currentPageItems.Count > 0);
                }

                if (needsNewPage)
                {
                    // 需要分頁：當前頁面已滿，將目前項目移到下一頁
                    pages.Add(new ReportPage<T>(new List<T>(currentPageItems), false));
                    currentPageItems = new List<T> { item };
                    currentPageDetailsHeight = itemHeight;
                }
                else
                {
                    // 可以放在當前頁
                    currentPageItems.Add(item);
                    currentPageDetailsHeight += itemHeight;
                }
            }

            // 添加最後一頁（標記為最後一頁）
            if (currentPageItems.Count > 0)
            {
                pages.Add(new ReportPage<T>(new List<T>(currentPageItems), true));
            }

            return pages;
        }

        /// <summary>
        /// 取得頁面配置資訊（供外部查詢使用）
        /// </summary>
        public ReportPageLayout GetLayout()
        {
            return _layout;
        }
    }
}
