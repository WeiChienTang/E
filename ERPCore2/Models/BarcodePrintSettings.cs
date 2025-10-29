namespace ERPCore2.Models;

/// <summary>
/// 條碼列印設定模型
/// </summary>
public class BarcodePrintSettings
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
    /// 是否顯示產品名稱
    /// </summary>
    public bool ShowProductName { get; set; } = true;
    
    /// <summary>
    /// 是否顯示產品代碼
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

/// <summary>
/// 條碼尺寸枚舉
/// </summary>
public enum BarcodeSize
{
    /// <summary>
    /// 小尺寸 (40mm x 20mm)
    /// </summary>
    Small,
    
    /// <summary>
    /// 中尺寸 (50mm x 25mm)
    /// </summary>
    Medium,
    
    /// <summary>
    /// 大尺寸 (70mm x 35mm)
    /// </summary>
    Large
}
