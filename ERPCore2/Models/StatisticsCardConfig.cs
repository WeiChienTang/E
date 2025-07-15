using Microsoft.AspNetCore.Components;

namespace ERPCore2.Models
{
    /// <summary>
    /// 統計卡片配置類別
    /// </summary>
    public class StatisticsCardConfig
    {
        /// <summary>
        /// 卡片標題
        /// </summary>
        public string Title { get; set; } = "";

        /// <summary>
        /// 資料索引鍵 (用於從 Statistics 字典取值)
        /// </summary>
        public string? DataKey { get; set; }

        /// <summary>
        /// 自訂值計算器 (優先於 DataKey)
        /// </summary>
        public Func<Dictionary<string, object>, decimal>? ValueCalculator { get; set; }

        /// <summary>
        /// 預設值 (當找不到資料時使用)
        /// </summary>
        public decimal DefaultValue { get; set; } = 0;

        /// <summary>
        /// 圖示 CSS 類別
        /// </summary>
        public string IconClass { get; set; } = "fas fa-chart-bar";

        /// <summary>
        /// 邊框顏色
        /// </summary>
        public string BorderColor { get; set; } = "primary";

        /// <summary>
        /// 文字顏色
        /// </summary>
        public string TextColor { get; set; } = "primary";

        /// <summary>
        /// 是否為貨幣格式
        /// </summary>
        public bool IsCurrency { get; set; } = false;

        /// <summary>
        /// 點擊事件
        /// </summary>
        public EventCallback OnClick { get; set; }
    }
}
