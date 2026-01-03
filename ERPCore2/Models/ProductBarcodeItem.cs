namespace ERPCore2.Models;

/// <summary>
/// 商品條碼列印項目模型
/// </summary>
public class ProductBarcodeItem
{
    /// <summary>
    /// 商品ID
    /// </summary>
    public int ProductId { get; set; }
    
    /// <summary>
    /// 商品編號
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 商品名稱
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
