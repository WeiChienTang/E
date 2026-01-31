namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 通用報表分頁計算器
    /// 根據頁面配置和明細項目，智能計算最佳分頁方式
    /// 
    /// 分頁策略：
    /// 1. 非最後一頁：使用完整空間填滿明細
    /// 2. 最後一頁：根據剩餘空間決定結尾位置
    ///    - 剩餘空間足夠 → 結尾放在當前頁
    ///    - 剩餘空間不足 → 新增一頁只放結尾（結尾專用頁）
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
        /// 使用「計算行高」而非「顯示行高」，確保保守估算
        /// </summary>
        /// <param name="item">明細項目</param>
        /// <returns>此項目需要的高度（mm）</returns>
        public decimal EstimateItemHeight(T item)
        {
            // 使用計算行高（較大），確保保守估算
            // 這樣即使備註過長導致換行，也不會超出預期
            return _layout.RowCalculationHeight;
        }

        /// <summary>
        /// 將明細清單分割成多頁
        /// 
        /// 新分頁策略：
        /// 1. 所有頁面使用完整空間填滿明細（不預留結尾空間）
        /// 2. 處理完所有明細後，檢查最後一頁剩餘空間
        /// 3. 如果剩餘空間足夠放結尾 → 結尾放在當前頁
        /// 4. 如果剩餘空間不足 → 新增「結尾專用頁」
        /// </summary>
        /// <param name="allDetails">所有明細項目</param>
        /// <returns>分頁後的頁面清單</returns>
        public List<ReportPage<T>> SplitIntoPages(List<T> allDetails)
        {
            var pages = new List<ReportPage<T>>();

            // 若無明細，返回只有結尾的單頁
            if (allDetails == null || !allDetails.Any())
            {
                return new List<ReportPage<T>>
                {
                    new ReportPage<T>(new List<T>(), true)
                };
            }

            // 使用完整空間來分頁（不預留結尾）
            decimal fullPageHeight = _layout.GetAvailableDetailsHeightForNonLastPage();
            decimal footerHeight = _layout.SummaryHeight + _layout.SignatureHeight;

            var currentPageItems = new List<T>();
            decimal currentPageDetailsHeight = 0m;

            // 步驟 1：將所有明細分配到各頁（使用完整空間）
            for (int i = 0; i < allDetails.Count; i++)
            {
                var item = allDetails[i];
                decimal itemHeight = EstimateItemHeight(item);

                // 計算加入此項後的高度
                decimal heightAfterAdd = currentPageDetailsHeight + itemHeight;

                // 檢查是否超過完整頁面可用空間
                if (heightAfterAdd > fullPageHeight && currentPageItems.Count > 0)
                {
                    // 當前頁滿了，換頁（標記為非最後一頁）
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

            // 步驟 2：處理最後一頁和結尾
            decimal remainingSpace = fullPageHeight - currentPageDetailsHeight;

            if (remainingSpace >= footerHeight)
            {
                // 剩餘空間足夠放結尾 → 當前頁就是最後一頁
                pages.Add(new ReportPage<T>(new List<T>(currentPageItems), isLastPage: true));
            }
            else
            {
                // 剩餘空間不夠 → 當前頁填滿明細（非最後一頁），新增結尾專用頁
                pages.Add(new ReportPage<T>(new List<T>(currentPageItems), isLastPage: false));
                // 結尾專用頁：無明細，只有結尾
                pages.Add(new ReportPage<T>(new List<T>(), isLastPage: true));
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
