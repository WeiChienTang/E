namespace ERPCore2.Components.Shared.SubCollections
{
    /// <summary>
    /// 互動表格欄位類型枚舉
    /// </summary>
    public enum InteractiveColumnType
    {
        /// <summary>
        /// 純顯示文字 - 不可編輯
        /// </summary>
        Display,
        
        /// <summary>
        /// 文字輸入框
        /// </summary>
        Input,
        
        /// <summary>
        /// 數字輸入框
        /// </summary>
        Number,
        
        /// <summary>
        /// 下拉選單
        /// </summary>
        Select,
        
        /// <summary>
        /// 勾選框
        /// </summary>
        Checkbox,
        
        /// <summary>
        /// 按鈕
        /// </summary>
        Button,
        
        /// <summary>
        /// 自訂模板
        /// </summary>
        Custom
    }
}
