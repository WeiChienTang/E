namespace ERPCore2.Models
{
    /// <summary>
    /// 導航選單項目模型
    /// </summary>
    public class NavigationMenuItem
    {
        /// <summary>
        /// 選單項目顯示的文字
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// 圖示的 CSS 類別
        /// </summary>
        public string IconClass { get; set; } = "";

        /// <summary>
        /// 連結的 URL（簡單連結項目使用）
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 是否為下拉選單項目
        /// </summary>
        public bool IsDropdown { get; set; } = false;

        /// <summary>
        /// 下拉選單的子項目
        /// </summary>
        public List<NavigationMenuItem> DropdownItems { get; set; } = new();

        /// <summary>
        /// 下拉選單是否開啟
        /// </summary>
        public bool IsOpen { get; set; } = false;

        /// <summary>
        /// 創建簡單連結項目
        /// </summary>
        public static NavigationMenuItem CreateLink(string text, string iconClass, string url)
        {
            return new NavigationMenuItem
            {
                Text = text,
                IconClass = iconClass,
                Url = url,
                IsDropdown = false
            };
        }

        /// <summary>
        /// 創建下拉選單項目
        /// </summary>
        public static NavigationMenuItem CreateDropdown(string text, string iconClass, params NavigationMenuItem[] items)
        {
            return new NavigationMenuItem
            {
                Text = text,
                IconClass = iconClass,
                IsDropdown = true,
                DropdownItems = items.ToList()
            };
        }

        /// <summary>
        /// 切換下拉選單狀態
        /// </summary>
        public void Toggle()
        {
            IsOpen = !IsOpen;
        }
    }
}
