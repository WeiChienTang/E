namespace ERPCore2.Components.Shared.UI.Button
{
    /// <summary>
    /// 按鈕變體枚舉 - 決定按鈕的顏色和樣式
    /// </summary>
    public enum ButtonVariant
    {
        /// <summary>
        /// 主要按鈕 - 深藍色背景
        /// </summary>
        DarkBlue,
        
        /// <summary>
        /// 次要按鈕 - 灰色背景
        /// </summary>
        Gray,
        
        /// <summary>
        /// 成功按鈕 - 綠色背景
        /// </summary>
        Green,
        
        /// <summary>
        /// 警告按鈕 - 黃色背景
        /// </summary>
        Yellow,
        
        /// <summary>
        /// 危險按鈕 - 紅色背景
        /// </summary>
        Red,
        
        /// <summary>
        /// 資訊按鈕 - 藍色背景
        /// </summary>
        Blue,
        
        /// <summary>
        /// 主要輪廓按鈕 - 透明背景深藍色邊框
        /// </summary>
        OutlineDarkBlue,
        
        /// <summary>
        /// 次要輪廓按鈕 - 透明背景灰色邊框
        /// </summary>
        OutlineGray,
        
        /// <summary>
        /// 成功輪廓按鈕 - 透明背景綠色邊框
        /// </summary>
        OutlineGreen,
        
        /// <summary>
        /// 警告輪廓按鈕 - 透明背景黃色邊框
        /// </summary>
        OutlineYellow,
        
        /// <summary>
        /// 危險輪廓按鈕 - 透明背景紅色邊框
        /// </summary>
        OutlineRed,
        
        /// <summary>
        /// 資訊輪廓按鈕 - 透明背景藍色邊框
        /// </summary>
        OutlineBlue,

        /// <summary>
        /// 青色按鈕（已棄用，自動映射至 Blue/btn-info）
        /// </summary>
        [Obsolete("請改用 Blue（btn-info）")]
        Cyan,

        /// <summary>
        /// 紫色按鈕（已棄用，自動映射至 DarkBlue/btn-primary）
        /// </summary>
        [Obsolete("請改用 DarkBlue（btn-primary）")]
        Purple,

        /// <summary>
        /// 粉紅色按鈕（已棄用，自動映射至 Red/btn-danger）
        /// </summary>
        [Obsolete("請改用 Red（btn-danger）")]
        Pink,

        /// <summary>
        /// 橙色按鈕（已棄用，自動映射至 Yellow/btn-warning）
        /// </summary>
        [Obsolete("請改用 Yellow（btn-warning）")]
        Orange,

        /// <summary>
        /// 黑色按鈕（已棄用，自動映射至 Gray/btn-secondary）
        /// </summary>
        [Obsolete("請改用 Gray（btn-secondary）")]
        Black,

        /// <summary>
        /// 白色按鈕（已棄用，自動映射至 OutlineGray/btn-outline-secondary）
        /// </summary>
        [Obsolete("請改用 OutlineGray（btn-outline-secondary）")]
        White,

        /// <summary>
        /// 青色輪廓按鈕（已棄用，自動映射至 OutlineBlue/btn-outline-info）
        /// </summary>
        [Obsolete("請改用 OutlineBlue（btn-outline-info）")]
        OutlineCyan,

        /// <summary>
        /// 紫色輪廓按鈕（已棄用，自動映射至 OutlineDarkBlue/btn-outline-primary）
        /// </summary>
        [Obsolete("請改用 OutlineDarkBlue（btn-outline-primary）")]
        OutlinePurple,

        /// <summary>
        /// 粉紅色輪廓按鈕（已棄用，自動映射至 OutlineRed/btn-outline-danger）
        /// </summary>
        [Obsolete("請改用 OutlineRed（btn-outline-danger）")]
        OutlinePink,

        /// <summary>
        /// 橙色輪廓按鈕（已棄用，自動映射至 OutlineYellow/btn-outline-warning）
        /// </summary>
        [Obsolete("請改用 OutlineYellow（btn-outline-warning）")]
        OutlineOrange,

        /// <summary>
        /// 黑色輪廓按鈕（已棄用，自動映射至 OutlineGray/btn-outline-secondary）
        /// </summary>
        [Obsolete("請改用 OutlineGray（btn-outline-secondary）")]
        OutlineBlack,

        /// <summary>
        /// 白色輪廓按鈕（已棄用，自動映射至 OutlineGray/btn-outline-secondary）
        /// </summary>
        [Obsolete("請改用 OutlineGray（btn-outline-secondary）")]
        OutlineWhite
    }
}
