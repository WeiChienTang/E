namespace ERPCore2.Models.Barcode
{
    /// <summary>
    /// 條碼列印設定模型
    /// </summary>
    public class BarcodePrintConfig
    {
        /// <summary>
        /// 每頁條碼數量
        /// </summary>
        public int BarcodesPerPage { get; set; } = 15;
        
        /// <summary>
        /// 條碼尺寸
        /// </summary>
        public BarcodeSize BarcodeSize { get; set; } = BarcodeSize.Medium;
        
        /// <summary>
        /// 是否顯示商品名稱
        /// </summary>
        public bool ShowProductName { get; set; } = true;
        
        /// <summary>
        /// 是否顯示商品編號
        /// </summary>
        public bool ShowProductCode { get; set; } = true;
        
        /// <summary>
        /// 條碼寬度（像素）
        /// </summary>
        public int BarcodeWidth { get; set; } = 2;
        
        /// <summary>
        /// 條碼高度（像素）
        /// </summary>
        public int BarcodeHeight { get; set; } = 60;
    }
}
