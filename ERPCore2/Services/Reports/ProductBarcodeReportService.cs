using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Barcode;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports;

/// <summary>
/// 商品條碼報表服務實作
/// 使用 FormattedDocument 架構，支援預覽、列印、Excel 匯出
/// </summary>
[SupportedOSPlatform("windows")]
public class ProductBarcodeReportService : IProductBarcodeReportService
{
    private readonly IProductService _productService;
    private readonly IFormattedPrintService _formattedPrintService;
    private readonly IBarcodeGeneratorService _barcodeGeneratorService;
    private readonly ILogger<ProductBarcodeReportService> _logger;

    // 預覽 DPI（與 FormattedPrintService 一致）
    private const int PreviewDPI = 96;
    // 1公分 = PreviewDPI / 2.54 像素
    private float CmToPixel => PreviewDPI / 2.54f;

    public ProductBarcodeReportService(
        IProductService productService,
        IFormattedPrintService formattedPrintService,
        IBarcodeGeneratorService barcodeGeneratorService,
        ILogger<ProductBarcodeReportService> logger)
    {
        _productService = productService;
        _formattedPrintService = formattedPrintService;
        _barcodeGeneratorService = barcodeGeneratorService;
        _logger = logger;
    }

    /// <summary>
    /// 批次渲染條碼報表為圖片（統一報表架構）
    /// </summary>
    public async Task<BatchPreviewResult> RenderBatchToImagesAsync(ProductBarcodeBatchPrintCriteria criteria)
    {
        try
        {
            // 驗證條件
            if (!criteria.Validate(out var errorMessage))
            {
                return BatchPreviewResult.Failure(errorMessage ?? "條件驗證失敗");
            }

            // 載入商品資料
            var products = await LoadProductsAsync(criteria);

            if (products == null || !products.Any())
            {
                return BatchPreviewResult.Failure("無符合條件的商品條碼");
            }

            // 準備紙張設定
            var paperSettings = GetPaperSettings(criteria);

            // 建立條碼項目清單（展開列印數量）
            var barcodeItems = CreateBarcodeItems(products, criteria);

            // 計算版面配置
            var layout = CalculateLayout(paperSettings, criteria.BarcodeSize);

            // 生成預覽圖片和 FormattedDocument
            var (previewImages, document) = RenderBarcodePages(barcodeItems, paperSettings, layout, criteria);

            return new BatchPreviewResult
            {
                IsSuccess = true,
                PreviewImages = previewImages,
                MergedDocument = document,
                DocumentCount = products.Count,
                TotalPages = previewImages.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "產生條碼預覽失敗");
            return BatchPreviewResult.Failure($"產生預覽失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 生成條碼批次列印報表（舊版 API，保留向後相容）
    /// </summary>
    public async Task<string> GenerateBarcodeReportAsync(ProductBarcodePrintCriteria criteria)
    {
        try
        {
            // 轉換為新版 Criteria
            var batchCriteria = new ProductBarcodeBatchPrintCriteria
            {
                ProductIds = criteria.ProductIds,
                CategoryIds = criteria.CategoryIds,
                OnlyWithBarcode = criteria.OnlyWithBarcode,
                BarcodeSize = criteria.BarcodeSize,
                BarcodesPerPage = criteria.BarcodesPerPage,
                ShowProductName = criteria.ShowProductName,
                ShowProductCode = criteria.ShowProductCode,
                PrintQuantities = criteria.PrintQuantities
            };

            // 呼叫新版方法
            var result = await RenderBatchToImagesAsync(batchCriteria);

            if (!result.IsSuccess)
            {
                return GenerateErrorPage(result.ErrorMessage ?? "列印失敗");
            }

            // 返回簡單的成功消息 HTML（舊版 API 主要用於直接列印）
            return GenerateSuccessPage(result.DocumentCount, result.TotalPages);
        }
        catch (Exception ex)
        {
            return GenerateErrorPage($"生成報表時發生錯誤：{ex.Message}");
        }
    }

    #region 私有方法

    /// <summary>
    /// 載入商品資料
    /// </summary>
    private async Task<List<Product>> LoadProductsAsync(ProductBarcodeBatchPrintCriteria criteria)
    {
        var allProducts = await _productService.GetAllAsync();
        var query = allProducts.AsQueryable();

        // 只列印有條碼的商品
        if (criteria.OnlyWithBarcode)
        {
            query = query.Where(p => !string.IsNullOrWhiteSpace(p.Barcode));
        }

        // 篩選特定商品
        if (criteria.ProductIds.Any())
        {
            query = query.Where(p => criteria.ProductIds.Contains(p.Id));
        }

        // 篩選特定分類
        if (criteria.CategoryIds.Any())
        {
            query = query.Where(p => p.ProductCategoryId.HasValue &&
                                    criteria.CategoryIds.Contains(p.ProductCategoryId.Value));
        }

        return query.OrderBy(p => p.Code).ToList();
    }

    /// <summary>
    /// 取得紙張設定
    /// </summary>
    private PaperSettings GetPaperSettings(ProductBarcodeBatchPrintCriteria criteria)
    {
        if (criteria.PaperSetting != null)
        {
            return new PaperSettings
            {
                PageWidth = (float)criteria.PaperSetting.Width,
                PageHeight = (float)criteria.PaperSetting.Height,
                LeftMargin = (float)(criteria.PaperSetting.LeftMargin ?? 0.5m),
                TopMargin = (float)(criteria.PaperSetting.TopMargin ?? 0.3m),
                RightMargin = (float)(criteria.PaperSetting.RightMargin ?? 0.5m),
                BottomMargin = (float)(criteria.PaperSetting.BottomMargin ?? 0.3m)
            };
        }

        // 預設使用中一刀尺寸
        return new PaperSettings
        {
            PageWidth = 21.3f,
            PageHeight = 14f,
            LeftMargin = 0.5f,
            TopMargin = 0.3f,
            RightMargin = 0.5f,
            BottomMargin = 0.3f
        };
    }

    /// <summary>
    /// 建立條碼項目清單（展開列印數量）
    /// </summary>
    private List<BarcodeItem> CreateBarcodeItems(List<Product> products, ProductBarcodeBatchPrintCriteria criteria)
    {
        var items = new List<BarcodeItem>();
        foreach (var product in products)
        {
            int quantity = criteria.PrintQuantities.TryGetValue(product.Id, out var qty) ? qty : 1;
            for (int i = 0; i < quantity; i++)
            {
                items.Add(new BarcodeItem
                {
                    Product = product,
                    ShowCode = criteria.ShowProductCode,
                    ShowName = criteria.ShowProductName
                });
            }
        }
        return items;
    }

    /// <summary>
    /// 計算版面配置
    /// </summary>
    private BarcodeLayout CalculateLayout(PaperSettings paper, BarcodeSize barcodeSize)
    {
        // 頁面像素尺寸
        int pageWidthPx = (int)(paper.PageWidth * CmToPixel);
        int pageHeightPx = (int)(paper.PageHeight * CmToPixel);
        int marginLeftPx = (int)(paper.LeftMargin * CmToPixel);
        int marginTopPx = (int)(paper.TopMargin * CmToPixel);
        int marginRightPx = (int)(paper.RightMargin * CmToPixel);
        int marginBottomPx = (int)(paper.BottomMargin * CmToPixel);
        int contentWidthPx = pageWidthPx - marginLeftPx - marginRightPx;
        int contentHeightPx = pageHeightPx - marginTopPx - marginBottomPx;

        // 條碼尺寸（使用預覽 DPI）
        var (barcodeWidthPx, barcodeHeightPx) = GetScaledBarcodeDimensions(barcodeSize);

        // 條碼間距：8mm 間距，確保條碼之間有足夠空間
        int gapPx = (int)(0.8f * CmToPixel); // 8mm 間距

        // 計算每行和每頁可容納的條碼數量
        int barcodesPerRow = Math.Max(1, (contentWidthPx + gapPx) / (barcodeWidthPx + gapPx));
        int rowsPerPage = Math.Max(1, (contentHeightPx + gapPx) / (barcodeHeightPx + gapPx));

        return new BarcodeLayout
        {
            PageWidthPx = pageWidthPx,
            PageHeightPx = pageHeightPx,
            MarginLeftPx = marginLeftPx,
            MarginTopPx = marginTopPx,
            ContentWidthPx = contentWidthPx,
            ContentHeightPx = contentHeightPx,
            BarcodeWidthPx = barcodeWidthPx,
            BarcodeHeightPx = barcodeHeightPx,
            GapPx = gapPx,
            BarcodesPerRow = barcodesPerRow,
            RowsPerPage = rowsPerPage,
            BarcodesPerPage = barcodesPerRow * rowsPerPage
        };
    }

    /// <summary>
    /// 取得使用預覽 DPI 的條碼尺寸（像素）
    /// </summary>
    private (int Width, int Height) GetScaledBarcodeDimensions(BarcodeSize size)
    {
        // 固定條碼尺寸：58×28mm
        // 設計依據：A4 紙張寬度 210mm，減去左右邊距各 10mm 後為 190mm
        // 一行 3 張條碼 + 2 個間距(8mm)，每張條碼寬度 = (190 - 16) / 3 ≈ 58mm
        const float widthMm = 58f;
        const float heightMm = 28f;

        // 轉換為像素（使用預覽 DPI）
        // 1cm = 10mm, 所以 1mm = CmToPixel / 10
        float mmToPixel = CmToPixel / 10f;
        return ((int)(widthMm * mmToPixel), (int)(heightMm * mmToPixel));
    }

    /// <summary>
    /// 渲染所有條碼頁面
    /// </summary>
    private (List<byte[]> PreviewImages, FormattedDocument Document) RenderBarcodePages(
        List<BarcodeItem> items,
        PaperSettings paper,
        BarcodeLayout layout,
        ProductBarcodeBatchPrintCriteria criteria)
    {
        var previewImages = new List<byte[]>();
        var document = new FormattedDocument
        {
            DocumentName = $"商品條碼列印-{DateTime.Now:yyyyMMdd}"
        };

        // 設定文件紙張尺寸
        document.PageSettings.PageWidth = paper.PageWidth;
        document.PageSettings.PageHeight = paper.PageHeight;
        document.PageSettings.LeftMargin = paper.LeftMargin;
        document.PageSettings.TopMargin = paper.TopMargin;
        document.PageSettings.RightMargin = paper.RightMargin;
        document.PageSettings.BottomMargin = paper.BottomMargin;

        // 分頁處理
        int totalPages = Math.Max(1, (int)Math.Ceiling(items.Count / (double)layout.BarcodesPerPage));

        // 計算列印尺寸（FormattedPrintService 使用 1/100 英吋單位）
        // 公分轉 1/100 英吋：cm * 100 / 2.54
        const float CmToHundredthsInch = 39.37f; // 100 / 2.54
        float printWidth = paper.PageWidth * CmToHundredthsInch;
        float printHeight = paper.PageHeight * CmToHundredthsInch;

        for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
        {
            if (pageIndex > 0)
            {
                document.AddPageBreak();
            }

            // 取得本頁的條碼
            var pageItems = items
                .Skip(pageIndex * layout.BarcodesPerPage)
                .Take(layout.BarcodesPerPage)
                .ToList();

            // 渲染頁面圖片
            var pageImage = RenderSinglePage(pageItems, layout, criteria.BarcodeSize);
            previewImages.Add(pageImage);

            // 添加到 FormattedDocument（用於列印）
            // 注意：FormattedPrintService 列印時使用 1/100 英吋單位
            // 所以傳入正確的 1/100 英吋尺寸
            document.AddImage(pageImage, printWidth, printHeight, TextAlignment.Center);
        }

        return (previewImages, document);
    }

    /// <summary>
    /// 渲染單頁條碼圖片
    /// </summary>
    private byte[] RenderSinglePage(List<BarcodeItem> items, BarcodeLayout layout, BarcodeSize size)
    {
        using var bitmap = new Bitmap(layout.PageWidthPx, layout.PageHeightPx);
        using var graphics = Graphics.FromImage(bitmap);

        // 白色背景
        graphics.Clear(System.Drawing.Color.White);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        // 計算水平置中偏移量
        int totalBarcodeWidth = layout.BarcodesPerRow * layout.BarcodeWidthPx + (layout.BarcodesPerRow - 1) * layout.GapPx;
        int horizontalOffset = (layout.ContentWidthPx - totalBarcodeWidth) / 2;
        
        int index = 0;
        foreach (var item in items)
        {
            // 計算位置（水平置中）
            int col = index % layout.BarcodesPerRow;
            int row = index / layout.BarcodesPerRow;

            int x = layout.MarginLeftPx + horizontalOffset + col * (layout.BarcodeWidthPx + layout.GapPx);
            int y = layout.MarginTopPx + row * (layout.BarcodeHeightPx + layout.GapPx);

            // 生成條碼圖片
            var barcodeBytes = _barcodeGeneratorService.GenerateBarcodeWithInfo(
                item.Product.Barcode ?? "",
                item.ShowCode ? item.Product.Code : null,
                item.ShowName ? item.Product.Name : null,
                size);

            if (barcodeBytes.Length > 0)
            {
                using var ms = new MemoryStream(barcodeBytes);
                using var barcodeImage = Image.FromStream(ms);
                // 縮放條碼圖片到目標尺寸
                graphics.DrawImage(barcodeImage, x, y, layout.BarcodeWidthPx, layout.BarcodeHeightPx);
            }

            index++;
        }

        // 輸出為 PNG
        using var outputStream = new MemoryStream();
        bitmap.Save(outputStream, ImageFormat.Png);
        return outputStream.ToArray();
    }

    /// <summary>
    /// 生成錯誤頁面
    /// </summary>
    private string GenerateErrorPage(string errorMessage)
    {
        return $@"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <title>條碼列印 - 錯誤</title>
    <style>
        body {{
            font-family: 'Microsoft JhengHei', Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: #f5f5f5;
        }}
        .error {{
            text-align: center;
            padding: 40px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            max-width: 500px;
        }}
        .error h1 {{ color: #dc3545; font-size: 24px; margin-bottom: 10px; }}
        .error p {{ color: #666; font-size: 14px; margin-top: 10px; }}
    </style>
</head>
<body>
    <div class='error'>
        <h1>❌ 列印失敗</h1>
        <p>{errorMessage}</p>
    </div>
</body>
</html>";
    }

    /// <summary>
    /// 生成成功頁面
    /// </summary>
    private string GenerateSuccessPage(int documentCount, int pageCount)
    {
        return $@"
<!DOCTYPE html>
<html lang='zh-TW'>
<head>
    <meta charset='UTF-8'>
    <title>條碼列印 - 成功</title>
    <style>
        body {{
            font-family: 'Microsoft JhengHei', Arial, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
            background: #f5f5f5;
        }}
        .success {{
            text-align: center;
            padding: 40px;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            max-width: 500px;
        }}
        .success h1 {{ color: #28a745; font-size: 24px; margin-bottom: 10px; }}
        .success p {{ color: #666; font-size: 14px; margin-top: 10px; }}
    </style>
</head>
<body>
    <div class='success'>
        <h1>✅ 條碼已準備就緒</h1>
        <p>共 {documentCount} 個商品，{pageCount} 頁</p>
    </div>
</body>
</html>";
    }

    #endregion

    #region 內部類別

    /// <summary>
    /// 紙張設定（公分）
    /// </summary>
    private class PaperSettings
    {
        public float PageWidth { get; set; }
        public float PageHeight { get; set; }
        public float LeftMargin { get; set; }
        public float TopMargin { get; set; }
        public float RightMargin { get; set; }
        public float BottomMargin { get; set; }
    }

    /// <summary>
    /// 版面配置（像素）
    /// </summary>
    private class BarcodeLayout
    {
        public int PageWidthPx { get; set; }
        public int PageHeightPx { get; set; }
        public int MarginLeftPx { get; set; }
        public int MarginTopPx { get; set; }
        public int ContentWidthPx { get; set; }
        public int ContentHeightPx { get; set; }
        public int BarcodeWidthPx { get; set; }
        public int BarcodeHeightPx { get; set; }
        public int GapPx { get; set; }
        public int BarcodesPerRow { get; set; }
        public int RowsPerPage { get; set; }
        public int BarcodesPerPage { get; set; }
    }

    /// <summary>
    /// 條碼項目（內部使用）
    /// </summary>
    private class BarcodeItem
    {
        public Product Product { get; set; } = null!;
        public bool ShowCode { get; set; }
        public bool ShowName { get; set; }
    }

    #endregion
}
