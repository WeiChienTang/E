using ERPCore2.Services.Reports.Interfaces;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Text;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// PDF 匯出服務實作
    /// 將已渲染的報表頁面圖片（PNG）透過 PuppeteerSharp 組合為 PDF 檔案
    /// </summary>
    public class PdfExportService : IPdfExportService
    {
        // 靜態鎖定物件，確保 Chromium 下載只執行一次
        private static readonly SemaphoreSlim _browserDownloadLock = new(1, 1);
        private static bool _browserDownloaded = false;

        /// <summary>
        /// 將頁面圖片列表匯出為 PDF 檔案
        /// 使用 PuppeteerSharp 將含有嵌入圖片的 HTML 頁面轉換為 PDF
        /// </summary>
        public async Task<byte[]> ExportToPdfAsync(List<byte[]> pageImages, double pageWidthCm = 21.0, double pageHeightCm = 29.7)
        {
            if (pageImages == null || pageImages.Count == 0)
                throw new ArgumentException("沒有可匯出的頁面內容");

            await EnsureBrowserDownloadedAsync();

            var html = BuildHtml(pageImages, pageWidthCm, pageHeightCm);

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-gpu" }
            });

            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Load }
            });

            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Width = $"{pageWidthCm.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}cm",
                Height = $"{pageHeightCm.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}cm",
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "0",
                    Bottom = "0",
                    Left = "0",
                    Right = "0"
                }
            });

            return pdfBytes;
        }

        /// <summary>
        /// 檢查服務是否可用（PuppeteerSharp 跨平台支援）
        /// </summary>
        public bool IsSupported() => true;

        /// <summary>
        /// 建立含有嵌入圖片的 HTML 字串，每張圖片對應一個 PDF 頁面
        /// </summary>
        private static string BuildHtml(List<byte[]> pageImages, double widthCm, double heightCm)
        {
            var w = widthCm.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
            var h = heightCm.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"utf-8\">");
            sb.AppendLine("<style>");
            sb.AppendLine($"  @page {{ margin: 0; size: {w}cm {h}cm; }}");
            sb.AppendLine("  html, body { margin: 0; padding: 0; background: white; }");
            sb.AppendLine($"  .page {{ width: {w}cm; height: {h}cm; page-break-after: always; overflow: hidden; }}");
            sb.AppendLine("  .page:last-child { page-break-after: auto; }");
            sb.AppendLine("  img { width: 100%; height: 100%; display: block; object-fit: fill; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            foreach (var imageBytes in pageImages)
            {
                var base64 = Convert.ToBase64String(imageBytes);
                sb.AppendLine($"<div class=\"page\"><img src=\"data:image/png;base64,{base64}\"></div>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        /// <summary>
        /// 確保 Chromium 瀏覽器已下載（僅第一次執行時下載）
        /// </summary>
        private static async Task EnsureBrowserDownloadedAsync()
        {
            if (_browserDownloaded)
                return;

            await _browserDownloadLock.WaitAsync();
            try
            {
                if (_browserDownloaded)
                    return;

                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
                _browserDownloaded = true;
            }
            finally
            {
                _browserDownloadLock.Release();
            }
        }
    }
}
