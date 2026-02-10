using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services.Reports.Configuration;
using ERPCore2.Services.Reports.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 報表列印服務實作 - 支援伺服器端直接列印
    /// 使用 PuppeteerSharp 將 HTML 渲染為高解析度圖片，再透過 System.Drawing.Printing 直接列印
    /// 完全靜默列印，不需要任何外部 PDF 閱讀器
    /// </summary>
    [SupportedOSPlatform("windows6.1")]
    public class ReportPrintService : IReportPrintService
    {
        private readonly IReportPrintConfigurationService _printConfigService;
        private readonly IPrinterConfigurationService _printerConfigService;
        private readonly IPaperSettingService _paperSettingService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ReportPrintService>? _logger;
        
        // 靜態鎖定物件，確保 Chromium 下載只執行一次
        private static readonly SemaphoreSlim _browserDownloadLock = new(1, 1);
        private static bool _browserDownloaded = false;
        
        // 快取 CSS 內容（避免每次都讀取檔案）
        private static string? _cachedPrintStylesCss = null;
        private static readonly object _cssLock = new();

        public ReportPrintService(
            IReportPrintConfigurationService printConfigService,
            IPrinterConfigurationService printerConfigService,
            IPaperSettingService paperSettingService,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ReportPrintService>? logger = null)
        {
            _printConfigService = printConfigService;
            _printerConfigService = printerConfigService;
            _paperSettingService = paperSettingService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        /// <summary>
        /// 直接列印報表（使用指定的列印配置）
        /// </summary>
        public async Task<ServiceResult> PrintReportAsync(
            string htmlContent, 
            ReportPrintConfiguration printConfig, 
            string documentName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    return ServiceResult.Failure("報表內容不能為空");
                }

                if (printConfig == null)
                {
                    return ServiceResult.Failure("列印配置不能為空");
                }

                // 載入印表機配置
                PrinterConfiguration? printerConfig = null;
                if (printConfig.PrinterConfigurationId.HasValue)
                {
                    printerConfig = await _printerConfigService.GetByIdAsync(printConfig.PrinterConfigurationId.Value);
                }

                if (printerConfig == null)
                {
                    return ServiceResult.Failure("未設定印表機或印表機配置無效");
                }

                // 載入紙張設定
                PaperSetting? paperSetting = null;
                if (printConfig.PaperSettingId.HasValue)
                {
                    paperSetting = await _paperSettingService.GetByIdAsync(printConfig.PaperSettingId.Value);
                }

                // 執行列印
                var result = await PrintWithDetailsAsync(
                    htmlContent,
                    printerConfig.Name,
                    paperSetting,
                    documentName);

                if (result.IsSuccess)
                {
                    return ServiceResult.Success();
                }
                else
                {
                    return ServiceResult.Failure(result.ErrorMessage ?? "列印失敗");
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PrintReportAsync), GetType(), _logger, new
                {
                    Method = nameof(PrintReportAsync),
                    DocumentName = documentName
                });
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 直接列印報表（使用報表識別碼自動載入配置）
        /// </summary>
        public async Task<ServiceResult> PrintReportByIdAsync(
            string htmlContent, 
            string reportId, 
            string documentName)
        {
            try
            {
                var printConfig = await GetDefaultPrintConfigAsync(reportId);
                
                if (printConfig == null)
                {
                    return ServiceResult.Failure($"找不到報表 '{reportId}' 的列印配置，請先在報表列印配置中設定印表機和紙張");
                }

                return await PrintReportAsync(htmlContent, printConfig, documentName);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PrintReportByIdAsync), GetType(), _logger, new
                {
                    Method = nameof(PrintReportByIdAsync),
                    ReportId = reportId,
                    DocumentName = documentName
                });
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 檢查印表機是否可用
        /// </summary>
        public async Task<ServiceResult> CheckPrinterAvailableAsync(int printerConfigurationId)
        {
            try
            {
                var printerConfig = await _printerConfigService.GetByIdAsync(printerConfigurationId);
                
                if (printerConfig == null)
                {
                    return ServiceResult.Failure("印表機配置不存在");
                }

                if (printerConfig.Status != EntityStatus.Active)
                {
                    return ServiceResult.Failure("印表機配置已停用");
                }

                // 檢查系統印表機是否存在
                using var printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerConfig.Name;

                if (!printDocument.PrinterSettings.IsValid)
                {
                    return ServiceResult.Failure($"印表機 '{printerConfig.Name}' 在系統中不存在或無法使用");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(CheckPrinterAvailableAsync), GetType(), _logger, new
                {
                    Method = nameof(CheckPrinterAvailableAsync),
                    PrinterConfigurationId = printerConfigurationId
                });
                return ServiceResult.Failure($"檢查印表機時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 取得報表的預設列印配置
        /// </summary>
        public async Task<ReportPrintConfiguration?> GetDefaultPrintConfigAsync(string reportId)
        {
            try
            {
                return await _printConfigService.GetByReportIdAsync(reportId);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(GetDefaultPrintConfigAsync), GetType(), _logger, new
                {
                    Method = nameof(GetDefaultPrintConfigAsync),
                    ReportId = reportId
                });
                return null;
            }
        }

        /// <summary>
        /// 列印報表並返回詳細結果
        /// 使用 PuppeteerSharp 將 HTML 直接渲染為圖片，再透過 System.Drawing.Printing 列印
        /// 此方法參考 PrinterTestService 的成功模式
        /// </summary>
        public async Task<ReportPrintResult> PrintWithDetailsAsync(
            string htmlContent,
            string printerName,
            PaperSetting? paperSetting,
            string documentName)
        {
            try
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return ReportPrintResult.Failure("目前僅支援 Windows 平台的直接列印功能");
                }

                _logger?.LogInformation("開始列印報表：{DocumentName}，印表機：{PrinterName}", documentName, printerName);

                // 步驟 1：確保 Chromium 已下載
                await EnsureBrowserDownloadedAsync();

                // 步驟 2：使用 PuppeteerSharp 將 HTML 直接渲染為圖片（跳過 PDF）
                List<byte[]> pageImages;
                
                try
                {
                    pageImages = await ConvertHtmlToImagesDirectAsync(htmlContent, paperSetting);
                    _logger?.LogInformation("圖片生成完成：{DocumentName}，共 {PageCount} 頁", 
                        documentName, pageImages.Count);
                    
                    if (pageImages.Count == 0)
                    {
                        return ReportPrintResult.Failure("無法生成列印圖片");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "HTML 轉圖片失敗：{DocumentName}", documentName);
                    return ReportPrintResult.Failure($"HTML 轉圖片失敗: {ex.Message}");
                }

                // 步驟 3：使用 PrintDocument 列印圖片（參考 PrinterTestService 的成功模式）
                var printResult = await PrintImagesWithSystemDrawingAsync(pageImages, printerName, paperSetting, documentName);
                
                if (printResult.IsSuccess)
                {
                    _logger?.LogInformation("列印成功：{DocumentName}，共 {PageCount} 頁", documentName, pageImages.Count);
                }
                
                return printResult;
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(ex, nameof(PrintWithDetailsAsync), GetType(), _logger, new
                {
                    Method = nameof(PrintWithDetailsAsync),
                    PrinterName = printerName,
                    DocumentName = documentName
                });
                return ReportPrintResult.Failure($"列印失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 使用 PuppeteerSharp 將 HTML 直接渲染為圖片（不經過 PDF）
        /// 這是最簡單、最可靠的方式
        /// </summary>
        private async Task<List<byte[]>> ConvertHtmlToImagesDirectAsync(string htmlContent, PaperSetting? paperSetting)
        {
            var images = new List<byte[]>();
            
            // 將外部 CSS 連結轉換為內嵌樣式
            var processedHtml = InlineExternalCss(htmlContent);

            // 計算頁面尺寸（公分轉像素，使用 150 DPI）
            // 注意：PaperSetting 的 Width/Height 是以「公分」儲存的
            // 1 cm = 10 mm，1 inch = 25.4 mm
            const int DPI = 150;
            decimal widthCm = paperSetting?.Width ?? 21.33m;   // 預設中一刀寬度 (cm)
            decimal heightCm = paperSetting?.Height ?? 13.97m; // 預設中一刀高度 (cm)
            
            // 公分轉像素：cm * 10 / 25.4 * DPI = cm / 2.54 * DPI
            int viewportWidth = (int)(widthCm / 2.54m * DPI);
            int viewportHeight = (int)(heightCm / 2.54m * DPI);

            _logger?.LogInformation("Viewport 尺寸：{Width}x{Height} px（{DPI} DPI），紙張：{PaperW}x{PaperH} cm", 
                viewportWidth, viewportHeight, DPI, widthCm, heightCm);

            // 啟動 Headless Chrome
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-gpu" }
            });

            await using var page = await browser.NewPageAsync();

            // 設定視窗大小
            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = viewportWidth,
                Height = viewportHeight,
                DeviceScaleFactor = 1
            });

            // 設定媒體類型為 print（避免螢幕預覽的邊框樣式）
            await page.EmulateMediaTypeAsync(MediaType.Print);

            // 設定頁面內容
            await page.SetContentAsync(processedHtml, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Load, WaitUntilNavigation.DOMContentLoaded }
            });

            // 等待渲染完成
            await Task.Delay(500);

            // 嘗試找到 .report-page 元素（每頁一個）
            var pageElements = await page.QuerySelectorAllAsync(".report-page");
            
            if (pageElements.Length > 0)
            {
                _logger?.LogInformation("找到 {Count} 個 .report-page 元素，逐頁截圖", pageElements.Length);
                
                // 逐頁截圖
                foreach (var pageElement in pageElements)
                {
                    var screenshotBytes = await pageElement.ScreenshotDataAsync(new ElementScreenshotOptions
                    {
                        Type = ScreenshotType.Png
                    });
                    
                    if (screenshotBytes.Length > 1000)
                    {
                        images.Add(screenshotBytes);
                        _logger?.LogInformation("頁面截圖成功，大小：{Size} bytes", screenshotBytes.Length);
                    }
                }
            }
            else
            {
                // 沒有找到 .report-page，嘗試找 .print-container
                var printContainers = await page.QuerySelectorAllAsync(".print-container");
                
                if (printContainers.Length > 0)
                {
                    _logger?.LogInformation("找到 {Count} 個 .print-container 元素，逐頁截圖", printContainers.Length);
                    
                    foreach (var container in printContainers)
                    {
                        var screenshotBytes = await container.ScreenshotDataAsync(new ElementScreenshotOptions
                        {
                            Type = ScreenshotType.Png
                        });
                        
                        if (screenshotBytes.Length > 1000)
                        {
                            images.Add(screenshotBytes);
                            _logger?.LogInformation("容器截圖成功，大小：{Size} bytes", screenshotBytes.Length);
                        }
                    }
                }
                else
                {
                    // 都沒找到，截取整個頁面
                    _logger?.LogInformation("未找到 .report-page 或 .print-container 元素，截取整個頁面");
                    
                    var screenshotBytes = await page.ScreenshotDataAsync(new ScreenshotOptions
                    {
                        Type = ScreenshotType.Png,
                        FullPage = true
                    });
                    
                    if (screenshotBytes.Length > 1000)
                    {
                        images.Add(screenshotBytes);
                        _logger?.LogInformation("整頁截圖成功，大小：{Size} bytes", screenshotBytes.Length);
                    }
                }
            }
            
            // 調試：將截圖保存到臨時目錄
            if (images.Count > 0)
            {
                try
                {
                    var debugDir = Path.Combine(Path.GetTempPath(), "ERPCore2_PrintDebug");
                    Directory.CreateDirectory(debugDir);
                    
                    for (int i = 0; i < images.Count; i++)
                    {
                        var debugPath = Path.Combine(debugDir, $"page_{DateTime.Now:yyyyMMdd_HHmmss}_{i + 1}.png");
                        await File.WriteAllBytesAsync(debugPath, images[i]);
                        _logger?.LogInformation("調試截圖已保存：{Path}（{Size} bytes）", debugPath, images[i].Length);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "保存調試截圖失敗");
                }
            }

            return images;
        }

        /// <summary>
        /// 使用 System.Drawing.Printing 列印圖片
        /// 完全參考 PrinterTestService.PrintUsingSystemDrawing 的成功模式
        /// </summary>
        private async Task<ReportPrintResult> PrintImagesWithSystemDrawingAsync(
            List<byte[]> pageImages,
            string printerName,
            PaperSetting? paperSetting,
            string documentName)
        {
            if (pageImages == null || pageImages.Count == 0)
            {
                return ReportPrintResult.Failure("沒有可列印的頁面");
            }

            try
            {
                // 完全參考 PrinterTestService 的方式，在 Task.Run 外部定義變數
                int currentPageIndex = 0;
                Exception? printException = null;
                var imagesToPrint = pageImages;

                var result = await Task.Run(() =>
                {
                    try
                    {
                        using var printDocument = new System.Drawing.Printing.PrintDocument();
                        printDocument.PrinterSettings.PrinterName = printerName;
                        printDocument.DocumentName = documentName;

                        // 檢查印表機是否有效（與 PrinterTestService 相同）
                        if (!printDocument.PrinterSettings.IsValid)
                        {
                            return ReportPrintResult.Failure($"印表機 '{printerName}' 無效或不可用");
                        }

                        _logger?.LogInformation("印表機有效：{PrinterName}", printerName);

                        // 設定紙張（可選）
                        // 注意：PaperSetting 的 Width/Height 是「公分」，PaperSize 需要「百分之一英吋」
                        // 換算：公分 / 2.54 * 100 = 百分之一英吋
                        if (paperSetting != null && paperSetting.Width > 0 && paperSetting.Height > 0)
                        {
                            int widthHundredths = (int)(paperSetting.Width / 2.54m * 100);
                            int heightHundredths = (int)(paperSetting.Height / 2.54m * 100);
                            
                            _logger?.LogInformation("紙張尺寸：{W} x {H} cm → {WH} x {HH} 百分之一英吋", 
                                paperSetting.Width, paperSetting.Height, widthHundredths, heightHundredths);
                            
                            printDocument.DefaultPageSettings.PaperSize = new PaperSize(
                                paperSetting.Name ?? "Custom", widthHundredths, heightHundredths);
                            
                            if (paperSetting.Orientation?.Equals("Landscape", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                printDocument.DefaultPageSettings.Landscape = true;
                            }
                        }

                        // 設定最小邊距（減少印表機預設的大邊距）
                        printDocument.DefaultPageSettings.Margins = new Margins(20, 20, 20, 20); // 約 5mm 邊距

                        // PrintPage 事件處理（參考 PrinterTestService 的結構）
                        printDocument.PrintPage += (sender, e) =>
                        {
                            try
                            {
                                if (e.Graphics == null) return;

                                if (currentPageIndex < imagesToPrint.Count)
                                {
                                    var imageBytes = imagesToPrint[currentPageIndex];
                                    
                                    using var ms = new MemoryStream(imageBytes);
                                    using var image = Image.FromStream(ms);

                                    // 使用較小的固定邊距，而非印表機預設的 MarginBounds
                                    // PageBounds 是整個頁面，我們自己設定小邊距
                                    float marginMm = 5f; // 5mm 邊距
                                    float margin = marginMm / 25.4f * 100f; // 轉換為百分之一英吋
                                    
                                    float x = margin;
                                    float y = margin;
                                    float maxWidth = e.PageBounds.Width - (margin * 2);
                                    float maxHeight = e.PageBounds.Height - (margin * 2);

                                    // 計算縮放比例（保持比例，讓圖片填滿可用區域）
                                    float scaleX = maxWidth / image.Width;
                                    float scaleY = maxHeight / image.Height;
                                    float scale = Math.Min(scaleX, scaleY);

                                    float scaledWidth = image.Width * scale;
                                    float scaledHeight = image.Height * scale;

                                    // 從左上角開始繪製（不置中，讓內容從上往下排列）
                                    // 水平可以置中，但垂直從頂部開始
                                    float drawX = x + (maxWidth - scaledWidth) / 2; // 水平置中
                                    float drawY = y; // 垂直從頂部開始，不置中

                                    _logger?.LogInformation("繪製圖片：位置({DrawX},{DrawY})，大小({W}x{H})，縮放比例:{Scale}",
                                        drawX, drawY, scaledWidth, scaledHeight, scale);

                                    // 繪製圖片
                                    e.Graphics.DrawImage(image, drawX, drawY, scaledWidth, scaledHeight);

                                    currentPageIndex++;
                                    e.HasMorePages = (currentPageIndex < imagesToPrint.Count);
                                }
                                else
                                {
                                    e.HasMorePages = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                printException = ex;
                                e.HasMorePages = false;
                            }
                        };

                        // 執行列印（與 PrinterTestService 相同）
                        printDocument.Print();

                        if (printException != null)
                        {
                            return ReportPrintResult.Failure($"列印時發生錯誤: {printException.Message}");
                        }

                        return ReportPrintResult.Success(printerName, paperSetting?.Name, imagesToPrint.Count);
                    }
                    catch (Exception ex)
                    {
                        return ReportPrintResult.Failure($"列印失敗: {ex.Message}");
                    }
                });

                // 等待列印處理（與 PrinterTestService 相同：2000ms）
                await Task.Delay(2000);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印圖片失敗");
                return ReportPrintResult.Failure($"列印失敗: {ex.Message}");
            }
        }

        #region Chromium 瀏覽器管理

        /// <summary>
        /// 確保 Chromium 瀏覽器已下載（僅第一次執行時下載）
        /// </summary>
        private async Task EnsureBrowserDownloadedAsync()
        {
            if (_browserDownloaded)
                return;

            await _browserDownloadLock.WaitAsync();
            try
            {
                if (_browserDownloaded)
                    return;

                _logger?.LogInformation("正在檢查/下載 Chromium 瀏覽器...");
                
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                
                _browserDownloaded = true;
                _logger?.LogInformation("Chromium 瀏覽器已就緒");
            }
            finally
            {
                _browserDownloadLock.Release();
            }
        }

        #endregion

        #region 頁面尺寸解析與 CSS 內嵌方法

        /// <summary>
        /// 從 HTML/CSS 中提取頁面尺寸設定
        /// 優先尋找 @page size（實際紙張尺寸），其次使用 CSS 變數（可用內容區域）
        /// </summary>
        private (string? Width, string? Height) ExtractPageSizeFromHtml(string html)
        {
            try
            {
                // 優先：尋找 @page size 定義（這是實際紙張尺寸）
                // 例如：size: 213.3mm 139.7mm;
                var pageSizeMatch = Regex.Match(html, @"@page\s*\{[^}]*size:\s*([0-9.]+mm)\s+([0-9.]+mm)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (pageSizeMatch.Success)
                {
                    var width = pageSizeMatch.Groups[1].Value;
                    var height = pageSizeMatch.Groups[2].Value;
                    _logger?.LogDebug("從 @page size 提取尺寸：{Width} x {Height}", width, height);
                    return (width, height);
                }

                // 備用：尋找 CSS 變數定義（這是可用內容區域，需要加上邊距）
                // 例如：--page-width: 209.3mm;
                var widthMatch = Regex.Match(html, @"--page-width:\s*([0-9.]+)mm", RegexOptions.IgnoreCase);
                var heightMatch = Regex.Match(html, @"--page-height:\s*([0-9.]+)mm", RegexOptions.IgnoreCase);

                if (widthMatch.Success && heightMatch.Success)
                {
                    // CSS 變數是可用內容區域，需要加上邊距（約 4mm 左右）
                    var contentWidth = decimal.Parse(widthMatch.Groups[1].Value);
                    var contentHeight = decimal.Parse(heightMatch.Groups[1].Value);
                    var paperWidth = contentWidth + 4m;
                    var paperHeight = contentHeight + 4m;
                    _logger?.LogDebug("從 CSS 變數計算尺寸：{Width}mm x {Height}mm", paperWidth, paperHeight);
                    return ($"{paperWidth}mm", $"{paperHeight}mm");
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "解析頁面尺寸時發生錯誤");
                return (null, null);
            }
        }

        /// <summary>
        /// 解析 mm 值（如 "213.3mm" → 213.3）
        /// </summary>
        private decimal? ParseMmValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            
            var match = Regex.Match(value, @"([0-9.]+)\s*mm", RegexOptions.IgnoreCase);
            if (match.Success && decimal.TryParse(match.Groups[1].Value, out var result))
                return result;
            
            return null;
        }

        /// <summary>
        /// 將 HTML 中的外部 CSS 連結轉換為內嵌樣式
        /// 這是必要的，因為 PuppeteerSharp 在記憶體中渲染時無法載入相對路徑的 CSS 檔案
        /// </summary>
        private string InlineExternalCss(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
                return htmlContent;

            // 搜尋並替換 print-styles.css 連結（支援多種格式）
            // 格式1: <link href='/css/print-styles.css' rel='stylesheet' />
            // 格式2: <link rel='stylesheet' href='/css/print-styles.css'>
            var linkPattern = @"<link\s+(?:href=['""]?/css/print-styles\.css['""]?\s+rel=['""]?stylesheet['""]?|rel=['""]?stylesheet['""]?\s+href=['""]?/css/print-styles\.css['""]?)[^>]*/?>";
            
            if (!Regex.IsMatch(htmlContent, linkPattern, RegexOptions.IgnoreCase))
            {
                // 沒有找到需要替換的連結，記錄 debug 資訊
                _logger?.LogDebug("未找到 print-styles.css 連結，HTML 前 500 字元：{HtmlPreview}", 
                    htmlContent.Substring(0, Math.Min(500, htmlContent.Length)));
                return htmlContent;
            }

            // 讀取 CSS 檔案內容
            var cssContent = GetPrintStylesCss();
            
            if (string.IsNullOrEmpty(cssContent))
            {
                _logger?.LogWarning("無法讀取 print-styles.css，PDF 可能無法正確渲染");
                return htmlContent;
            }

            // 將 <link> 標籤替換為內嵌的 <style> 標籤
            var inlineStyle = $"<style type=\"text/css\">\n{cssContent}\n</style>";
            var result = Regex.Replace(htmlContent, linkPattern, inlineStyle, RegexOptions.IgnoreCase);

            _logger?.LogInformation("已將外部 CSS 轉換為內嵌樣式（CSS 長度：{CssLength} 字元）", cssContent.Length);
            return result;
        }

        /// <summary>
        /// 取得 print-styles.css 的內容（使用快取）
        /// </summary>
        private string? GetPrintStylesCss()
        {
            // 使用快取避免重複讀取
            if (_cachedPrintStylesCss != null)
                return _cachedPrintStylesCss;

            lock (_cssLock)
            {
                if (_cachedPrintStylesCss != null)
                    return _cachedPrintStylesCss;

                try
                {
                    var cssPath = Path.Combine(_webHostEnvironment.WebRootPath, "css", "print-styles.css");
                    
                    if (File.Exists(cssPath))
                    {
                        _cachedPrintStylesCss = File.ReadAllText(cssPath, Encoding.UTF8);
                        _logger?.LogInformation("已載入 print-styles.css（{Length} 字元）", _cachedPrintStylesCss.Length);
                        return _cachedPrintStylesCss;
                    }
                    else
                    {
                        _logger?.LogWarning("找不到 CSS 檔案：{CssPath}", cssPath);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "讀取 print-styles.css 時發生錯誤");
                    return null;
                }
            }
        }

        #endregion
    }
}

