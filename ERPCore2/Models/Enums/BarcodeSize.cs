namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 條碼尺寸枚舉
    /// 注意：目前系統固定使用 Large (58mm x 28mm)，一行可放 3 張
    /// </summary>
    public enum BarcodeSize
    {
        /// <summary>
        /// 小尺寸 (僅供參考，目前未使用)
        /// </summary>
        Small,
        
        /// <summary>
        /// 中尺寸 (僅供參考，目前未使用)
        /// </summary>
        Medium,
        
        /// <summary>
        /// 大尺寸 (58mm x 28mm) - 目前固定使用此尺寸
        /// </summary>
        Large
    }
}
