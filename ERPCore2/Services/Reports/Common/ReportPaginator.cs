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
        /// </summary>
        /// <param name="item">明細項目</param>
        /// <returns>此項目需要的高度（mm）</returns>
        public decimal EstimateItemHeight(T item)
        {
            if (item == null)
                return _layout.RowBaseHeight;

            var remarks = item.GetRemarks();

            // 如果沒有備註，返回基本高度 + 額外高度因素
            if (string.IsNullOrEmpty(remarks))
                return _layout.RowBaseHeight + item.GetExtraHeightFactor();

            // 計算備註需要的行數
            int totalChars = remarks.Length;
            int lines = (int)Math.Ceiling((double)totalChars / _layout.RemarkCharsPerLine);

            if (lines <= 1)
            {
                // 單行備註：使用基本高度
                return _layout.RowBaseHeight + item.GetExtraHeightFactor();
            }

            // 多行備註：基本高度 + 額外行數 × 行高 + 特殊高度因素
            return _layout.RowBaseHeight
                + (lines - 1) * _layout.RemarkExtraLineHeight
                + item.GetExtraHeightFactor();
        }

        /// <summary>
        /// 將明細清單分割成多頁
        /// 智能計算每頁應包含的明細數量，確保：
        /// 1. 每頁不超過可用高度
        /// 2. 最後一頁預留統計和簽名空間
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
            decimal availableHeight = _layout.GetAvailableDetailsHeight();

            for (int i = 0; i < allDetails.Count; i++)
            {
                var item = allDetails[i];
                decimal itemHeight = EstimateItemHeight(item);
                bool isLastItem = (i == allDetails.Count - 1);

                // 計算加入此項後的明細總高度
                decimal heightAfterAdd = currentPageDetailsHeight + itemHeight;

                // 檢查是否需要分頁（統一邏輯）
                bool needsNewPage = false;
                
                if (isLastItem)
                {
                    // 最後一筆：檢查是否能放在當前頁（需包含統計和簽名）
                    decimal totalRequiredHeight = _layout.HeaderHeight
                        + _layout.InfoSectionHeight
                        + _layout.TableHeaderHeight
                        + heightAfterAdd
                        + _layout.SummaryHeight
                        + _layout.SignatureHeight
                        + _layout.SafetyMargin;

                    needsNewPage = (totalRequiredHeight > _layout.PageHeight && currentPageItems.Count > 0);
                }
                else
                {
                    // 非最後一筆：檢查是否超過可用明細高度
                    needsNewPage = (heightAfterAdd > availableHeight && currentPageItems.Count > 0);
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
