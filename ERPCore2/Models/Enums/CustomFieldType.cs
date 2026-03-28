namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 自訂欄位的資料類型
    /// </summary>
    public enum CustomFieldType
    {
        /// <summary>單行文字</summary>
        Text = 0,

        /// <summary>多行文字</summary>
        TextArea = 1,

        /// <summary>數值</summary>
        Number = 2,

        /// <summary>日期</summary>
        Date = 3,

        /// <summary>日期時間</summary>
        DateTime = 4,

        /// <summary>布林（是/否）</summary>
        Boolean = 5,

        /// <summary>下拉選單（單選）</summary>
        Select = 6,
    }
}
