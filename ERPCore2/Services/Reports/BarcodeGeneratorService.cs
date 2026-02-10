using BarcodeStandard;
using ERPCore2.Models.Barcode;
using ERPCore2.Services.Reports.Interfaces;
using SkiaSharp;

namespace ERPCore2.Services.Reports;

/// <summary>
/// 條碼生成服務實作
/// 使用 BarcodeLib (BarcodeStandard) 在後端直接生成條碼圖片
/// </summary>
public class BarcodeGeneratorService : IBarcodeGeneratorService
{
    // 預覽 DPI（與 FormattedPrintService 預覽一致）
    private const float DefaultDPI = 96f;
    // 列印 DPI（高解析度，確保文字清晰）
    private const float PrintDPI = 300f;
    // 1公釐 = DPI / 25.4 像素
    private static float PixelsPerMm(float dpi) => dpi / 25.4f;

    // 中文字體名稱
    private const string ChineseFontName = "Microsoft JhengHei";
    private const string FallbackFontName = "Arial";
    
    /// <summary>
    /// 生成單個條碼圖片
    /// </summary>
    public byte[] GenerateBarcode(string barcodeValue, BarcodeSize size, bool showText = true)
    {
        if (string.IsNullOrWhiteSpace(barcodeValue))
            return Array.Empty<byte>();
        
        var (width, height) = GetBarcodeDimensions(size);
        
        try
        {
            var barcode = new Barcode
            {
                IncludeLabel = showText
            };
            
            // 生成條碼圖片
            var barcodeType = GetBarcodeType(barcodeValue);
            var image = barcode.Encode(barcodeType, barcodeValue, SKColors.Black, SKColors.White, width, height);
            
            if (image == null)
                return Array.Empty<byte>();
            
            // 轉換為 PNG bytes
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
        catch (Exception)
        {
            // 如果編碼失敗，返回空陣列
            return Array.Empty<byte>();
        }
    }
    
    /// <summary>
    /// 生成帶有商品資訊的條碼圖片
    /// 商品資訊（[編號] [名稱]）顯示在條碼下方一行
    /// </summary>
    public byte[] GenerateBarcodeWithInfo(
        string barcodeValue, 
        string? productCode, 
        string? productName, 
        BarcodeSize size)
    {
        if (string.IsNullOrWhiteSpace(barcodeValue))
            return Array.Empty<byte>();
        
        // 使用高 DPI 生成圖片，確保列印清晰
        var (width, height) = GetBarcodeDimensions(size, PrintDPI);
        
        // 根據條碼尺寸調整文字大小（按 DPI 縮放）
        // 基準字體大小 11pt，按 300/96 DPI 比例放大
        float dpiScale = PrintDPI / 96f;
        float textSize = 11f * dpiScale;  // 約 34 像素
        
        float infoTextSize = textSize;
        float infoLineHeight = infoTextSize + 4f * dpiScale;
        
        // 計算標籤區域高度（只有一行商品資訊）
        bool hasProductInfo = !string.IsNullOrWhiteSpace(productCode) || !string.IsNullOrWhiteSpace(productName);
        int labelHeight = (int)(hasProductInfo ? infoLineHeight : 0) + (int)(4f * dpiScale);
        
        // 條碼本身高度（不含標籤）
        int barcodeOnlyHeight = height - labelHeight;
        int minBarcodeHeight = (int)(30 * dpiScale); // 最小條碼高度（按 DPI 縮放）
        if (barcodeOnlyHeight < minBarcodeHeight) barcodeOnlyHeight = minBarcodeHeight;
        
        int totalHeight = barcodeOnlyHeight + labelHeight;
        
        try
        {
            // 生成條碼（不含標籤，我們自己繪製所有文字）
            var barcode = new Barcode
            {
                IncludeLabel = false // 不顯示條碼值，我們自己畫
            };
            
            var barcodeType = GetBarcodeType(barcodeValue);
            
            // 修正條碼寬度不一致問題：
            // 直接生成目標寬度的條碼，BarcodeLib 會自動計算合適的條紋寬度
            // 這樣所有條碼都會填滿相同的寬度，條紋比例由 BarcodeLib 控制
            var barcodeImage = barcode.Encode(barcodeType, barcodeValue, SKColors.Black, SKColors.White, width, barcodeOnlyHeight);
            
            if (barcodeImage == null)
                return Array.Empty<byte>();
            
            // 創建帶標籤的完整圖片
            using var surface = SKSurface.Create(new SKImageInfo(width, totalHeight));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);
            
            // 繪製條碼（直接使用生成的圖片，無需縮放）
            canvas.DrawImage(barcodeImage, 0, 0);
            
            // 準備文字畫筆（支援中文，高品質渲染）
            var typeface = SKTypeface.FromFamilyName(ChineseFontName, SKFontStyle.Bold) 
                          ?? SKTypeface.FromFamilyName(FallbackFontName, SKFontStyle.Bold)
                          ?? SKTypeface.Default;
            
            using var infoTextPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = infoTextSize,
                IsAntialias = true,
                SubpixelText = true,  // 次像素文字渲染，提高清晰度
                LcdRenderText = true, // LCD 文字渲染
                TextAlign = SKTextAlign.Center,
                Typeface = typeface
            };
            
            // 繪製標籤（商品資訊一行，顯示在條碼下方）
            if (hasProductInfo)
            {
                float yOffset = barcodeOnlyHeight + infoLineHeight;
                
                // 商品資訊（[編號] [名稱]）
                string infoText = BuildProductInfoText(productCode, productName, width, infoTextPaint);
                if (!string.IsNullOrWhiteSpace(infoText))
                {
                    canvas.DrawText(infoText, width / 2f, yOffset, infoTextPaint);
                }
            }
            
            // 輸出為 PNG
            using var finalImage = surface.Snapshot();
            using var pngData = finalImage.Encode(SKEncodedImageFormat.Png, 100);
            return pngData.ToArray();
        }
        catch (Exception)
        {
            return Array.Empty<byte>();
        }
    }
    
    /// <summary>
    /// 取得條碼尺寸（像素）- 使用預設 96 DPI
    /// </summary>
    public (int Width, int Height) GetBarcodeDimensions(BarcodeSize size)
    {
        return GetBarcodeDimensions(size, DefaultDPI);
    }
    
    /// <summary>
    /// 取得條碼尺寸（像素）- 指定 DPI
    /// </summary>
    public (int Width, int Height) GetBarcodeDimensions(BarcodeSize size, float dpi)
    {
        float pxPerMm = PixelsPerMm(dpi);
        
        // 固定條碼尺寸：58×28mm
        // 設計依據：A4 紙張寬度 210mm，減去左右邊距各 10mm 後為 190mm
        // 一行 3 張條碼 + 2 個間距(8mm)，每張條碼寬度 = (190 - 16) / 3 ≈ 58mm
        const float widthMm = 58f;
        const float heightMm = 28f;
        
        return ((int)(widthMm * pxPerMm), (int)(heightMm * pxPerMm));
    }
    
    /// <summary>
    /// 建立商品資訊文字（[編號] [名稱] 格式），確保不超過條碼寬度
    /// </summary>
    private static string BuildProductInfoText(string? productCode, string? productName, int maxWidth, SKPaint paint)
    {
        // 組合文字：[編號] [名稱]
        string fullText;
        if (!string.IsNullOrWhiteSpace(productCode) && !string.IsNullOrWhiteSpace(productName))
        {
            fullText = $"{productCode} {productName}";
        }
        else if (!string.IsNullOrWhiteSpace(productCode))
        {
            fullText = productCode;
        }
        else if (!string.IsNullOrWhiteSpace(productName))
        {
            fullText = productName;
        }
        else
        {
            return string.Empty;
        }
        
        // 檢查是否超過寬度，若超過則截斷
        float textWidth = paint.MeasureText(fullText);
        if (textWidth <= maxWidth - 4) // 留 4 像素邊距
        {
            return fullText;
        }
        
        // 需要截斷
        string ellipsis = "…";
        for (int len = fullText.Length - 1; len > 0; len--)
        {
            string truncated = fullText[..len] + ellipsis;
            if (paint.MeasureText(truncated) <= maxWidth - 4)
            {
                return truncated;
            }
        }
        
        return ellipsis;
    }
    
    /// <summary>
    /// 根據條碼值自動選擇條碼類型
    /// </summary>
    private static BarcodeStandard.Type GetBarcodeType(string barcodeValue)
    {
        // EAN-13（13位數字）
        if (barcodeValue.Length == 13 && barcodeValue.All(char.IsDigit))
            return BarcodeStandard.Type.Ean13;
        
        // EAN-8（8位數字）
        if (barcodeValue.Length == 8 && barcodeValue.All(char.IsDigit))
            return BarcodeStandard.Type.Ean8;
        
        // UPC-A（12位數字）
        if (barcodeValue.Length == 12 && barcodeValue.All(char.IsDigit))
            return BarcodeStandard.Type.UpcA;
        
        // 預設使用 Code128（支援所有 ASCII 字元）
        return BarcodeStandard.Type.Code128;
    }
}
