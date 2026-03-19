namespace ERPCore2.Models.Barcode
{
    /// <summary>
    /// 品項條碼列印項目模型
    /// </summary>
    public class ProductBarcodeItem
    {
        /// <summary>
        /// 品項ID
        /// </summary>
        public int ProductId { get; set; }
        
        /// <summary>
        /// 品項編號
        /// </summary>
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// 品項名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 條碼號碼
        /// </summary>
        public string Barcode { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否選中（用於多選）
        /// </summary>
        public bool IsSelected { get; set; }
        
        /// <summary>
        /// 列印數量
        /// </summary>
        public int PrintQuantity { get; set; } = 1;
    }
}
