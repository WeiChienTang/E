namespace ERPCore2.Models
{
    /// <summary>
    /// 明細格式設定 - 用於序列化/反序列化 TextMessageTemplate.DetailFormatJson
    /// 控制訊息複製時明細項目要顯示哪些欄位
    /// </summary>
    public class DetailFormatConfig
    {
        /// <summary>
        /// 是否顯示商品編號
        /// </summary>
        public bool ShowProductCode { get; set; } = false;

        /// <summary>
        /// 是否顯示商品名稱
        /// </summary>
        public bool ShowProductName { get; set; } = true;

        /// <summary>
        /// 是否顯示數量
        /// </summary>
        public bool ShowQuantity { get; set; } = true;

        /// <summary>
        /// 是否顯示單位
        /// </summary>
        public bool ShowUnit { get; set; } = true;

        /// <summary>
        /// 是否顯示單價
        /// </summary>
        public bool ShowUnitPrice { get; set; } = false;

        /// <summary>
        /// 是否顯示小計
        /// </summary>
        public bool ShowSubtotal { get; set; } = false;

        /// <summary>
        /// 是否顯示備註
        /// </summary>
        public bool ShowRemark { get; set; } = false;
    }
}
