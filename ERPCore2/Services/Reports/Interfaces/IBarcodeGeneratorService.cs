using ERPCore2.Models.Barcode;

namespace ERPCore2.Services.Reports.Interfaces;

/// <summary>
/// 條碼生成服務介面
/// 提供後端條碼圖片生成功能，不依賴瀏覽器
/// </summary>
public interface IBarcodeGeneratorService
{
    /// <summary>
    /// 生成單個條碼圖片
    /// </summary>
    /// <param name="barcodeValue">條碼值</param>
    /// <param name="size">條碼尺寸</param>
    /// <param name="showText">是否顯示條碼文字</param>
    /// <returns>PNG 圖片位元組陣列</returns>
    byte[] GenerateBarcode(string barcodeValue, BarcodeSize size, bool showText = true);
    
    /// <summary>
    /// 生成帶有商品資訊的條碼圖片
    /// </summary>
    /// <param name="barcodeValue">條碼值</param>
    /// <param name="productCode">商品編號（可選）</param>
    /// <param name="productName">商品名稱（可選）</param>
    /// <param name="size">條碼尺寸</param>
    /// <returns>PNG 圖片位元組陣列</returns>
    byte[] GenerateBarcodeWithInfo(
        string barcodeValue, 
        string? productCode, 
        string? productName, 
        BarcodeSize size);
    
    /// <summary>
    /// 取得條碼尺寸（像素）
    /// </summary>
    /// <param name="size">條碼尺寸</param>
    /// <returns>寬度和高度（像素）</returns>
    (int Width, int Height) GetBarcodeDimensions(BarcodeSize size);
}
