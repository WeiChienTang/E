namespace ERPCore2.Services.Reports.Common
{
    /// <summary>
    /// 報表明細項目介面（通用）
    /// 所有報表明細項目都應該實作此介面，以支援通用分頁計算
    /// </summary>
    public interface IReportDetailItem
    {
        /// <summary>
        /// 取得備註內容（用於高度計算）
        /// 分頁器會根據備註長度估算此項目需要的高度
        /// </summary>
        /// <returns>備註文字，若無備註則回傳空字串</returns>
        string GetRemarks();

        /// <summary>
        /// 取得額外高度因素（mm）
        /// 某些明細可能有特殊欄位影響高度（例如：多行規格、圖片預覽等）
        /// 預設為 0，有特殊需求時可覆寫此方法
        /// </summary>
        /// <returns>額外需要的高度（mm），預設為 0</returns>
        decimal GetExtraHeightFactor() => 0m;
    }
}
