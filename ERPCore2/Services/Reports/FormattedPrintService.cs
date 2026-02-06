using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Services.Reports.Interfaces;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using DrawingColor = System.Drawing.Color;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 格式化列印服務實作
    /// 使用 System.Drawing.Printing 直接繪製文字、表格、線條、圖片到印表機
    /// </summary>
    [SupportedOSPlatform("windows6.1")]
    public class FormattedPrintService : IFormattedPrintService
    {
        private readonly IReportPrintConfigurationService _reportPrintConfigService;
        private readonly IPrinterConfigurationService _printerConfigService;
        private readonly IPaperSettingService _paperSettingService;

        /// <summary>
        /// 列印後等待時間（毫秒）
        /// </summary>
        private const int PrintWaitTimeMs = 2000;

        /// <summary>
        /// 公分轉點數的比例（1公分 = 28.3465點）
        /// </summary>
        private const float CmToPoints = 28.3465f;

        public FormattedPrintService(
            IReportPrintConfigurationService reportPrintConfigService,
            IPrinterConfigurationService printerConfigService,
            IPaperSettingService paperSettingService)
        {
            _reportPrintConfigService = reportPrintConfigService;
            _printerConfigService = printerConfigService;
            _paperSettingService = paperSettingService;
        }

        /// <summary>
        /// 檢查是否支援格式化列印
        /// </summary>
        public bool IsSupported()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        /// <summary>
        /// 列印格式化文件到指定印表機
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public ServiceResult Print(FormattedDocument document, string printerName, int copies = 1)
        {
            try
            {
                if (!IsSupported())
                {
                    return ServiceResult.Failure("格式化列印功能僅支援 Windows 平台");
                }

                if (document == null || !document.Elements.Any())
                {
                    return ServiceResult.Failure("列印文件不能為空");
                }

                if (string.IsNullOrWhiteSpace(printerName))
                {
                    return ServiceResult.Failure("印表機名稱不能為空");
                }

                copies = Math.Max(1, Math.Min(copies, 99));

                using var printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.DocumentName = document.DocumentName;
                printDocument.PrinterSettings.Copies = (short)copies;

                // 設定紙張尺寸（公分轉百分之一英吋：1 cm = 0.3937 inch，乘以 100）
                // PaperSize 使用百分之一英吋為單位
                const float CmToHundredthsInch = 39.37f;  // 1 cm = 100/2.54 百分之一英吋
                var paperWidthHundredthsInch = (int)(document.PageSettings.PageWidth * CmToHundredthsInch);
                var paperHeightHundredthsInch = (int)(document.PageSettings.PageHeight * CmToHundredthsInch);
                var customPaperSize = new PaperSize("Custom", paperWidthHundredthsInch, paperHeightHundredthsInch);
                printDocument.DefaultPageSettings.PaperSize = customPaperSize;

                // 設定邊距（Margins 使用百分之一英吋為單位，與 PaperSize 一致）
                var margins = new Margins(
                    (int)(document.PageSettings.LeftMargin * CmToHundredthsInch),
                    (int)(document.PageSettings.RightMargin * CmToHundredthsInch),
                    (int)(document.PageSettings.TopMargin * CmToHundredthsInch),
                    (int)(document.PageSettings.BottomMargin * CmToHundredthsInch)
                );
                printDocument.DefaultPageSettings.Margins = margins;

                if (!printDocument.PrinterSettings.IsValid)
                {
                    return ServiceResult.Failure($"印表機 '{printerName}' 無效或不可用");
                }

                // === 第一次模擬渲染：計算總頁數（與預覽相同的邏輯）===
                int totalPages = 0;
                {
                    var countState = new PrintState(document);
                    int maxPages = 100;
                    
                    // 使用與預覽相同的計算邏輯（包含硬邊界模擬）
                    const float simDpi = 96f;
                    float simCmToPixels = simDpi / 2.54f;
                    
                    // 預設硬邊界（與預覽一致）
                    const float DefaultHardMarginCm = 0.3f;
                    int simHardMargin = (int)(DefaultHardMarginCm * simCmToPixels);
                    
                    int simPageWidth = (int)(document.PageSettings.PageWidth * simCmToPixels);
                    int simPageHeight = (int)(document.PageSettings.PageHeight * simCmToPixels);
                    
                    // 可列印區域
                    int simPrintableWidth = simPageWidth - simHardMargin * 2;
                    int simPrintableHeight = simPageHeight - simHardMargin * 2;
                    
                    // 使用者邊距
                    int simMarginLeft = (int)(document.PageSettings.LeftMargin * simCmToPixels);
                    int simMarginTop = (int)(document.PageSettings.TopMargin * simCmToPixels);
                    int simMarginRight = (int)(document.PageSettings.RightMargin * simCmToPixels);
                    int simMarginBottom = (int)(document.PageSettings.BottomMargin * simCmToPixels);
                    
                    // 內容區域（基於可列印區域）
                    int simContentWidth = simPrintableWidth - simMarginLeft - simMarginRight;
                    int simContentHeight = simPrintableHeight - simMarginTop - simMarginBottom;
                    
                    var simBounds = new Rectangle(
                        simHardMargin + simMarginLeft, 
                        simHardMargin + simMarginTop, 
                        simContentWidth, 
                        simContentHeight);

                    while (!countState.IsComplete && totalPages < maxPages)
                    {
                        totalPages++;
                        using var bitmap = new Bitmap(simPageWidth, simPageHeight);
                        using var graphics = Graphics.FromImage(bitmap);
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                        graphics.Clear(DrawingColor.White);
                        RenderPage(graphics, simBounds, document, countState);
                    }
                }

                // === 第二次實際列印：帶入正確的頁次資訊 ===
                var printState = new PrintState(document);
                int currentPageNumber = 0;
                Exception? printException = null;

                printDocument.PrintPage += (sender, e) =>
                {
                    try
                    {
                        if (e.Graphics == null) return;
                        
                        currentPageNumber++;
                        
                        // ===== 修正預覽與列印不一致的問題 =====
                        // 
                        // 關鍵理解：
                        // 1. e.PageBounds 使用 1/100 英吋為單位（不是像素）
                        // 2. e.Graphics.DpiX 是印表機實際 DPI（如 300-600），但 Graphics 座標系統與 PageBounds 一致
                        // 3. Graphics 原點在 PrintableArea 左上角，不是紙張左上角
                        //
                        // 問題根因：
                        // - 預覽使用完整紙張尺寸渲染（Bitmap 大小 = 紙張尺寸）
                        // - 列印時 Graphics 原點在 HardMargin 處，內容會往右下偏移
                        // - 如果內容超出 PrintableArea，右側會被裁切
                        //
                        // 解決方案：
                        // - 使用 PrintableArea 作為可用區域，確保內容在可列印範圍內
                        // - 邊距相對於 PrintableArea 而非紙張
                        
                        // 公分轉 1/100 英吋（與 PageBounds 單位一致）
                        const float CmToHundredthsInch = 39.37f;
                        
                        // 獲取可列印區域（已排除印表機硬邊界）
                        var printableArea = e.PageSettings.PrintableArea;
                        
                        // 使用者設定的邊距（轉換為 1/100 英吋）
                        int marginLeft = (int)(document.PageSettings.LeftMargin * CmToHundredthsInch);
                        int marginTop = (int)(document.PageSettings.TopMargin * CmToHundredthsInch);
                        int marginRight = (int)(document.PageSettings.RightMargin * CmToHundredthsInch);
                        int marginBottom = (int)(document.PageSettings.BottomMargin * CmToHundredthsInch);
                        
                        // 內容區域基於可列印區域（而非完整紙張）
                        // 這確保內容不會超出印表機可列印範圍
                        int contentWidth = (int)printableArea.Width - marginLeft - marginRight;
                        int contentHeight = (int)printableArea.Height - marginTop - marginBottom;
                        
                        // 確保內容區域不為負
                        if (contentWidth < 100) 
                        {
                            marginLeft = (int)(printableArea.Width * 0.05f);
                            marginRight = (int)(printableArea.Width * 0.05f);
                            contentWidth = (int)printableArea.Width - marginLeft - marginRight;
                        }
                        if (contentHeight < 100)
                        {
                            marginTop = (int)(printableArea.Height * 0.05f);
                            marginBottom = (int)(printableArea.Height * 0.05f);
                            contentHeight = (int)printableArea.Height - marginTop - marginBottom;
                        }
                        
                        // 渲染起點：由於 Graphics 原點已在 PrintableArea 左上角，
                        // 所以 (0,0) 就是可列印區域的起點
                        var actualBounds = new Rectangle(
                            marginLeft,
                            marginTop,
                            contentWidth,
                            contentHeight
                        );
                        
                        RenderPage(e.Graphics, actualBounds, document, printState, currentPageNumber, totalPages);
                        e.HasMorePages = !printState.IsComplete;
                    }
                    catch (Exception ex)
                    {
                        printException = ex;
                        e.HasMorePages = false;
                    }
                };

                printDocument.Print();
                Thread.Sleep(PrintWaitTimeMs);

                if (printException != null)
                {
                    return ServiceResult.Failure($"列印時發生錯誤: {printException.Message}");
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 使用報表配置列印格式化文件
        /// </summary>
        public async Task<ServiceResult> PrintByReportIdAsync(FormattedDocument document, string reportId, int copies = 1)
        {
            try
            {
                if (!IsSupported())
                {
                    return ServiceResult.Failure("格式化列印功能僅支援 Windows 平台");
                }

                var printConfig = await _reportPrintConfigService.GetByReportIdAsync(reportId);
                if (printConfig == null)
                {
                    return ServiceResult.Failure($"找不到報表 '{reportId}' 的列印配置");
                }

                if (!printConfig.PrinterConfigurationId.HasValue)
                {
                    return ServiceResult.Failure("列印配置未設定印表機");
                }

                var printerConfig = await _printerConfigService.GetByIdAsync(printConfig.PrinterConfigurationId.Value);
                if (printerConfig == null)
                {
                    return ServiceResult.Failure("印表機配置不存在");
                }

                // 載入紙張設定並套用邊距
                if (printConfig.PaperSettingId.HasValue)
                {
                    var paperSetting = await _paperSettingService.GetByIdAsync(printConfig.PaperSettingId.Value);
                    if (paperSetting != null)
                    {
                        document.PageSettings.LeftMargin = (float)(paperSetting.LeftMargin ?? 1.0m);
                        document.PageSettings.TopMargin = (float)(paperSetting.TopMargin ?? 1.0m);
                        document.PageSettings.RightMargin = (float)(paperSetting.RightMargin ?? 1.0m);
                        document.PageSettings.BottomMargin = (float)(paperSetting.BottomMargin ?? 1.0m);
                    }
                }

                return Print(document, printerConfig.Name, copies);
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 將格式化文件渲染為圖片（用於預覽）
        /// 根據紙張設定計算頁面尺寸，並同步更新文件的邊距設定
        /// </summary>
        /// <param name="document">格式化文件</param>
        /// <param name="paperSetting">紙張設定</param>
        /// <param name="dpi">預覽 DPI</param>
        [SupportedOSPlatform("windows6.1")]
        public List<byte[]> RenderToImages(FormattedDocument document, PaperSetting paperSetting, int dpi = 96)
        {
            // 同步更新 FormattedDocument 的紙張和邊距設定
            // 這確保預覽和列印使用相同的設定
            document.PageSettings.PageWidth = (float)paperSetting.Width;
            document.PageSettings.PageHeight = (float)paperSetting.Height;
            document.PageSettings.LeftMargin = (float)(paperSetting.LeftMargin ?? 0.8m);
            document.PageSettings.TopMargin = (float)(paperSetting.TopMargin ?? 0.3m);
            document.PageSettings.RightMargin = (float)(paperSetting.RightMargin ?? 0.8m);
            document.PageSettings.BottomMargin = (float)(paperSetting.BottomMargin ?? 0.3m);

            // 將公分轉換為像素：pixels = cm * dpi / 2.54
            // 2.54 是每英吋的公分數
            int pageWidth = (int)(((float)paperSetting.Width * dpi) / 2.54f);
            int pageHeight = (int)(((float)paperSetting.Height * dpi) / 2.54f);

            // 考慮紙張方向
            if (paperSetting.Orientation == "Landscape")
            {
                (pageWidth, pageHeight) = (pageHeight, pageWidth);
            }

            return RenderToImages(document, pageWidth, pageHeight, dpi);
        }

        /// <summary>
        /// 將格式化文件渲染為圖片（用於預覽）
        /// 使用預設的 A4 紙張尺寸（794x1123 像素 @ 96 DPI）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public List<byte[]> RenderToImages(FormattedDocument document, int pageWidth = 794, int pageHeight = 1123, int dpi = 96)
        {
            var images = new List<byte[]>();
            // 檢查文件是否有任何內容（主內容、頁首或頁尾）
            if (document == null || (!document.Elements.Any() && !document.HeaderElements.Any() && !document.FooterElements.Any()))
                return images;

            var printState = new PrintState(document);
            var pageImages = new List<byte[]>();

            // ===== 預覽與列印一致的關鍵：模擬印表機硬邊界 =====
            // 
            // 問題：預覽使用完整紙張尺寸，但列印時印表機有硬邊界（通常左右各 3-8mm）
            // 解決：在預覽時也預留硬邊界空間，確保預覽所見即列印所得
            //
            // 預設印表機硬邊界（針孔印表機/連續報表紙通常較小，一般印表機約 5mm）
            const float DefaultHardMarginCm = 0.3f;  // 3mm 硬邊界
            
            float cmToPixels = dpi / 2.54f;
            
            // 模擬印表機可列印區域（紙張減去硬邊界）
            int hardMarginLeft = (int)(DefaultHardMarginCm * cmToPixels);
            int hardMarginRight = (int)(DefaultHardMarginCm * cmToPixels);
            int hardMarginTop = (int)(DefaultHardMarginCm * cmToPixels);
            int hardMarginBottom = (int)(DefaultHardMarginCm * cmToPixels);
            
            int printableWidth = pageWidth - hardMarginLeft - hardMarginRight;
            int printableHeight = pageHeight - hardMarginTop - hardMarginBottom;
            
            // 使用者設定的邊距（相對於可列印區域）
            var marginLeft = (int)(document.PageSettings.LeftMargin * cmToPixels);
            var marginTop = (int)(document.PageSettings.TopMargin * cmToPixels);
            var marginRight = (int)(document.PageSettings.RightMargin * cmToPixels);
            var marginBottom = (int)(document.PageSettings.BottomMargin * cmToPixels);

            // 內容區域（基於可列印區域，與列印邏輯一致）
            int contentWidth = printableWidth - marginLeft - marginRight;
            int contentHeight = printableHeight - marginTop - marginBottom;
            
            // 確保內容區域不為負
            if (contentWidth < 50) 
            {
                marginLeft = (int)(printableWidth * 0.05f);
                marginRight = (int)(printableWidth * 0.05f);
                contentWidth = printableWidth - marginLeft - marginRight;
            }
            if (contentHeight < 50)
            {
                marginTop = (int)(printableHeight * 0.05f);
                marginBottom = (int)(printableHeight * 0.05f);
                contentHeight = printableHeight - marginTop - marginBottom;
            }
            
            // 渲染起點需要加上硬邊界偏移
            var bounds = new Rectangle(
                hardMarginLeft + marginLeft,
                hardMarginTop + marginTop,
                contentWidth,
                contentHeight
            );

            // 第一次渲染：計算總頁數
            int pageCount = 0;
            int maxPages = 100; // 防止無限迴圈
            while (!printState.IsComplete && pageCount < maxPages)
            {
                pageCount++;
                
                using var bitmap = new Bitmap(pageWidth, pageHeight);
                using var graphics = Graphics.FromImage(bitmap);

                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                graphics.Clear(DrawingColor.White);

                RenderPage(graphics, bounds, document, printState);

                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                pageImages.Add(ms.ToArray());
            }

            int totalPages = pageImages.Count;

            // 第二次渲染：替換頁次佔位符 {{PAGE}}/{{PAGES}}
            printState = new PrintState(document);
            int currentPageNumber = 0;

            while (!printState.IsComplete && currentPageNumber < maxPages)
            {
                currentPageNumber++;
                using var bitmap = new Bitmap(pageWidth, pageHeight);
                using var graphics = Graphics.FromImage(bitmap);

                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                graphics.Clear(DrawingColor.White);

                RenderPage(graphics, bounds, document, printState, currentPageNumber, totalPages);

                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                images.Add(ms.ToArray());
            }

            return images;
        }

        #region 繪製方法

        /// <summary>
        /// 計算頁首高度
        /// </summary>
        private float MeasureHeaderHeight(Graphics g, FormattedDocument document, float availableWidth)
        {
            float height = 0;
            foreach (var element in document.HeaderElements)
            {
                height += MeasureElementHeight(g, element, availableWidth, document.PageSettings);
            }
            return height;
        }

        /// <summary>
        /// 計算頁尾高度
        /// </summary>
        private float MeasureFooterHeight(Graphics g, FormattedDocument document, float availableWidth)
        {
            float height = 0;
            foreach (var element in document.FooterElements)
            {
                height += MeasureElementHeight(g, element, availableWidth, document.PageSettings);
            }
            return height;
        }

        /// <summary>
        /// 繪製頁首（支援頁次佔位符替換）
        /// </summary>
        private float RenderHeader(Graphics g, FormattedDocument document, float x, float y, float width, int currentPage = 0, int totalPages = 0)
        {
            float currentY = y;
            foreach (var element in document.HeaderElements)
            {
                // 對頁首元素進行頁次佔位符替換
                var processedElement = ReplacePagePlaceholders(element, currentPage, totalPages);
                currentY = RenderElement(g, processedElement, x, currentY, width, document.PageSettings);
            }
            return currentY;
        }

        /// <summary>
        /// 繪製頁尾
        /// </summary>
        private float RenderFooter(Graphics g, FormattedDocument document, float x, float y, float width)
        {
            float currentY = y;
            foreach (var element in document.FooterElements)
            {
                currentY = RenderElement(g, element, x, currentY, width, document.PageSettings);
            }
            return currentY;
        }

        /// <summary>
        /// 替換元素中的頁次佔位符
        /// 注意：C# 字串插值會把 {{PAGE}} 轉成 {PAGE}，所以需要匹配 {PAGE}
        /// </summary>
        private PageElement ReplacePagePlaceholders(PageElement element, int currentPage, int totalPages)
        {
            if (currentPage <= 0 || totalPages <= 0)
                return element;

            if (element is ReportHeaderBlockElement block)
            {
                // 複製並替換 RightLines 中的頁次佔位符
                var newRightLines = block.RightLines
                    .Select(line => line
                        .Replace("{PAGE}", currentPage.ToString())
                        .Replace("{PAGES}", totalPages.ToString()))
                    .ToList();

                return new ReportHeaderBlockElement
                {
                    CenterLines = block.CenterLines,
                    RightLines = newRightLines,
                    RightFontSize = block.RightFontSize
                };
            }
            else if (element is TextElement text)
            {
                return new TextElement
                {
                    Text = text.Text
                        .Replace("{PAGE}", currentPage.ToString())
                        .Replace("{PAGES}", totalPages.ToString()),
                    FontSize = text.FontSize,
                    FontName = text.FontName,
                    Bold = text.Bold,
                    Italic = text.Italic,
                    Alignment = text.Alignment
                };
            }
            else if (element is ThreeColumnHeaderElement header)
            {
                string? ReplaceInText(string? text) => text?
                    .Replace("{PAGE}", currentPage.ToString())
                    .Replace("{PAGES}", totalPages.ToString());

                return new ThreeColumnHeaderElement
                {
                    LeftText = ReplaceInText(header.LeftText) ?? "",
                    CenterText = ReplaceInText(header.CenterText) ?? "",
                    RightText = ReplaceInText(header.RightText) ?? "",
                    CenterFontSize = header.CenterFontSize,
                    SideFontSize = header.SideFontSize,
                    CenterBold = header.CenterBold
                };
            }

            return element;
        }

        /// <summary>
        /// 繪製單頁內容（支援頁首和頁尾，不含頁次資訊）
        /// </summary>
        private void RenderPage(Graphics g, Rectangle bounds, FormattedDocument document, PrintState state)
        {
            RenderPage(g, bounds, document, state, 0, 0);
        }

        /// <summary>
        /// 繪製單頁內容（支援頁首和頁尾，含頁次資訊，支援表格跨頁）
        /// </summary>
        private void RenderPage(Graphics g, Rectangle bounds, FormattedDocument document, PrintState state, int currentPage, int totalPages)
        {
            float currentY = bounds.Top;
            float availableWidth = bounds.Width;

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 計算頁首和頁尾高度
            float headerHeight = MeasureHeaderHeight(g, document, availableWidth);
            float footerHeight = MeasureFooterHeight(g, document, availableWidth);

            // 繪製頁首（傳遞頁次資訊以替換佔位符）
            if (document.HeaderElements.Any())
            {
                currentY = RenderHeader(g, document, bounds.Left, currentY, availableWidth, currentPage, totalPages);
            }

            // === 處理「頁尾專用頁」===
            // 如果上一頁內容已完成但放不下頁尾，這一頁只需要渲染頁首+頁尾
            if (state.NeedFooterPage)
            {
                if (document.FooterElements.Any())
                {
                    RenderFooter(g, document, bounds.Left, currentY, availableWidth);
                }
                state.NeedFooterPage = false;
                state.IsComplete = true;
                return;
            }

            // 計算頁尾高度（用於最後判斷）
            float footerReserve = document.FooterElements.Any() ? footerHeight : 0;

            // 不預先判斷是否為最後一頁，讓內容自然填滿
            // 明細結束後才判斷頁尾是否能放下

            // 繪製主要內容（不預留頁尾空間，讓內容盡量填滿）
            float contentBottom = bounds.Bottom;

            while (state.CurrentElementIndex < document.Elements.Count)
            {
                var element = document.Elements[state.CurrentElementIndex];

                // 檢查是否為分頁元素
                if (element is PageBreakElement)
                {
                    state.CurrentElementIndex++;
                    state.ResetTableState();
                    return; // 結束當前頁面
                }

                // 特別處理表格元素（支援跨頁）
                if (element is TableElement table)
                {
                    var tableResult = RenderTableWithPagination(g, table, bounds.Left, currentY, availableWidth, contentBottom, document.PageSettings, state);
                    currentY = tableResult.NewY;
                    
                    if (!tableResult.IsComplete)
                    {
                        // 表格尚未完成，需要換頁
                        return;
                    }
                    
                    // 表格完成，移至下一個元素
                    state.CurrentElementIndex++;
                    state.ResetTableState();
                    continue;
                }

                // 計算元素高度
                float elementHeight = MeasureElementHeight(g, element, availableWidth, document.PageSettings);

                // 檢查是否超出頁面（只有在不是頁面第一個內容元素時才換頁）
                if (currentY + elementHeight > contentBottom && currentY > bounds.Top + headerHeight + 10)
                {
                    return; // 需要換頁
                }

                // 繪製元素
                currentY = RenderElement(g, element, bounds.Left, currentY, availableWidth, document.PageSettings);

                state.CurrentElementIndex++;
            }

            // === 主要內容已全部完成 ===
            // 現在檢查頁尾是否能放在這一頁
            if (document.FooterElements.Any())
            {
                float remainingSpace = bounds.Bottom - currentY;
                
                if (remainingSpace >= footerReserve)
                {
                    // 空間足夠，在同一頁渲染頁尾
                    RenderFooter(g, document, bounds.Left, currentY, availableWidth);
                    state.IsComplete = true;
                }
                else
                {
                    // 空間不足，需要換頁
                    // 標記需要渲染頁尾頁
                    state.NeedFooterPage = true;
                    return; // 結束當前頁，下一頁會渲染頁尾
                }
            }
            else
            {
                state.IsComplete = true;
            }
        }

        /// <summary>
        /// 計算剩餘內容高度（考慮表格當前行位置）
        /// </summary>
        private float CalculateRemainingContentHeight(Graphics g, FormattedDocument document, PrintState state, float availableWidth)
        {
            float remainingHeight = 0;
            
            for (int i = state.CurrentElementIndex; i < document.Elements.Count; i++)
            {
                var element = document.Elements[i];
                
                if (element is TableElement table && i == state.CurrentElementIndex)
                {
                    // 對於當前正在處理的表格，只計算剩餘行的高度
                    remainingHeight += MeasureTableRemainingHeight(g, table, state.CurrentTableRowIndex, availableWidth, document.PageSettings);
                }
                else
                {
                    remainingHeight += MeasureElementHeight(g, element, availableWidth, document.PageSettings);
                }
            }
            
            return remainingHeight;
        }

        /// <summary>
        /// 測量表格剩餘行的高度
        /// </summary>
        private float MeasureTableRemainingHeight(Graphics g, TableElement table, int startRowIndex, float availableWidth, DocumentPageSettings settings)
        {
            if (!table.Columns.Any()) return 0;

            float height = 0;

            // 表頭高度（如果尚未渲染或需要在每頁重複）
            if (table.Columns.Any(c => !string.IsNullOrEmpty(c.Header)))
            {
                height += table.HeaderRowHeight;
            }

            // 計算欄位寬度
            float totalRatio = table.Columns.Sum(c => c.WidthRatio);
            var columnWidths = table.Columns.Select(c => (c.WidthRatio / totalRatio) * availableWidth).ToArray();

            using var font = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize);

            // 計算剩餘資料列的高度
            for (int rowIdx = startRowIndex; rowIdx < table.Rows.Count; rowIdx++)
            {
                var row = table.Rows[rowIdx];
                float maxCellHeight = table.RowHeight;
                for (int i = 0; i < Math.Min(row.Cells.Count, table.Columns.Count); i++)
                {
                    var cellText = row.Cells[i];
                    if (!string.IsNullOrEmpty(cellText))
                    {
                        SizeF textSize = g.MeasureString(cellText, font, (int)(columnWidths[i] - 4));
                        if (textSize.Height > maxCellHeight)
                        {
                            maxCellHeight = textSize.Height + 4;
                        }
                    }
                }
                height += maxCellHeight;
            }

            return height + 5;
        }

        /// <summary>
        /// 表格渲染結果
        /// </summary>
        private struct TableRenderResult
        {
            public float NewY;
            public bool IsComplete;
        }

        /// <summary>
        /// 支援分頁的表格渲染
        /// </summary>
        private TableRenderResult RenderTableWithPagination(Graphics g, TableElement table, float x, float y, float width, float maxY, DocumentPageSettings settings, PrintState state)
        {
            if (!table.Columns.Any()) 
                return new TableRenderResult { NewY = y, IsComplete = true };

            // 計算欄位寬度
            float totalRatio = table.Columns.Sum(c => c.WidthRatio);
            var columnWidths = table.Columns.Select(c => (c.WidthRatio / totalRatio) * width).ToArray();

            using var font = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize);
            using var boldFont = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize, System.Drawing.FontStyle.Bold);
            using var pen = new System.Drawing.Pen(DrawingColor.Black, 1);
            using var brush = new System.Drawing.SolidBrush(DrawingColor.Black);

            float currentY = y;

            // 渲染表頭（每頁都需要）
            if (table.Columns.Any(c => !string.IsNullOrEmpty(c.Header)))
            {
                float headerHeight = table.HeaderRowHeight;
                
                // 檢查表頭是否能放下
                if (currentY + headerHeight > maxY)
                {
                    return new TableRenderResult { NewY = currentY, IsComplete = false };
                }

                // 繪製表頭
                float cellX = x;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    var col = table.Columns[i];
                    string headerText = col.Header;
                    if (table.ShowHeaderSeparator && i < table.Columns.Count - 1)
                    {
                        headerText = $"{col.Header} |";
                    }

                    var format = new StringFormat
                    {
                        Alignment = col.Alignment switch
                        {
                            TextAlignment.Center => StringAlignment.Center,
                            TextAlignment.Right => StringAlignment.Far,
                            _ => StringAlignment.Near
                        },
                        LineAlignment = StringAlignment.Center
                    };

                    var cellRect = new RectangleF(cellX + 2, currentY, columnWidths[i] - 4, headerHeight);
                    g.DrawString(headerText, boldFont, brush, cellRect, format);
                    cellX += columnWidths[i];
                }

                // 繪製表頭下方橫線（分隔表頭與內容）
                if (table.ShowHeaderUnderline)
                {
                    g.DrawLine(pen, x, currentY + headerHeight, x + width, currentY + headerHeight);
                }

                currentY += headerHeight;
                state.TableHeaderRendered = true;
            }

            // 從當前行開始渲染資料列
            while (state.CurrentTableRowIndex < table.Rows.Count)
            {
                var row = table.Rows[state.CurrentTableRowIndex];
                
                // 計算此行高度
                float maxCellHeight = table.RowHeight;
                for (int i = 0; i < Math.Min(row.Cells.Count, table.Columns.Count); i++)
                {
                    var cellText = row.Cells[i];
                    if (!string.IsNullOrEmpty(cellText))
                    {
                        SizeF textSize = g.MeasureString(cellText, font, (int)(columnWidths[i] - 4));
                        if (textSize.Height > maxCellHeight)
                        {
                            maxCellHeight = textSize.Height + 4;
                        }
                    }
                }

                // 檢查是否能放下這一行
                if (currentY + maxCellHeight > maxY)
                {
                    // 無法放下，需要換頁
                    return new TableRenderResult { NewY = currentY, IsComplete = false };
                }

                // 繪製此行
                float cellX = x;
                for (int i = 0; i < Math.Min(row.Cells.Count, table.Columns.Count); i++)
                {
                    var col = table.Columns[i];
                    var format = new StringFormat
                    {
                        Alignment = col.Alignment switch
                        {
                            TextAlignment.Center => StringAlignment.Center,
                            TextAlignment.Right => StringAlignment.Far,
                            _ => StringAlignment.Near
                        },
                        LineAlignment = StringAlignment.Near
                    };

                    var cellRect = new RectangleF(cellX + 2, currentY + 2, columnWidths[i] - 4, maxCellHeight - 4);
                    g.DrawString(row.Cells[i], font, brush, cellRect, format);
                    cellX += columnWidths[i];
                }

                currentY += maxCellHeight;
                state.CurrentTableRowIndex++;
            }

            // 表格完成
            return new TableRenderResult { NewY = currentY + 5, IsComplete = true };
        }

        /// <summary>
        /// 計算元素高度
        /// </summary>
        private float MeasureElementHeight(Graphics g, PageElement element, float availableWidth, DocumentPageSettings settings)
        {
            return element switch
            {
                TextElement text => MeasureTextHeight(g, text, availableWidth, settings),
                LineElement => 5f,
                SpacingElement spacing => spacing.Height,
                TableElement table => MeasureTableHeight(g, table, availableWidth, settings),
                ImageElement image => MeasureImageHeight(image, availableWidth),
                SignatureSectionElement => 50f,
                KeyValueRowElement => settings.DefaultFontSize + 5,  // 縮小間距
                ThreeColumnHeaderElement header => string.IsNullOrEmpty(header.CenterText) 
                    ? header.SideFontSize + 3  // 無中間文字時用側邊字體高度
                    : header.CenterFontSize + 3,  // 有中間文字時用中間字體高度
                ReportHeaderBlockElement block => MeasureReportHeaderBlockHeight(g, block, settings),
                TwoColumnSectionElement twoCol => MeasureTwoColumnSectionHeight(g, twoCol, availableWidth, settings),
                _ => 0f
            };
        }

        /// <summary>
        /// 繪製元素並返回新的 Y 位置
        /// </summary>
        private float RenderElement(Graphics g, PageElement element, float x, float y, float width, DocumentPageSettings settings)
        {
            return element switch
            {
                TextElement text => RenderText(g, text, x, y, width, settings),
                LineElement line => RenderLine(g, line, x, y, width),
                SpacingElement spacing => y + spacing.Height,
                TableElement table => RenderTable(g, table, x, y, width, settings),
                ImageElement image => RenderImage(g, image, x, y, width),
                SignatureSectionElement signature => RenderSignatureSection(g, signature, x, y, width, settings),
                KeyValueRowElement kvRow => RenderKeyValueRow(g, kvRow, x, y, width, settings),
                ThreeColumnHeaderElement header => RenderThreeColumnHeader(g, header, x, y, width, settings),
                ReportHeaderBlockElement block => RenderReportHeaderBlock(g, block, x, y, width, settings),
                TwoColumnSectionElement twoCol => RenderTwoColumnSection(g, twoCol, x, y, width, settings),
                _ => y
            };
        }

        /// <summary>
        /// 繪製文字
        /// </summary>
        private float RenderText(Graphics g, TextElement text, float x, float y, float width, DocumentPageSettings settings)
        {
            var fontStyle = FontStyle.Regular;
            if (text.Bold) fontStyle |= FontStyle.Bold;
            if (text.Italic) fontStyle |= FontStyle.Italic;

            using var font = new System.Drawing.Font(text.FontName ?? settings.FontName, text.FontSize, fontStyle);
            using var brush = new System.Drawing.SolidBrush(DrawingColor.Black);

            var format = new StringFormat();
            format.Alignment = text.Alignment switch
            {
                TextAlignment.Center => StringAlignment.Center,
                TextAlignment.Right => StringAlignment.Far,
                _ => StringAlignment.Near
            };

            var rect = new RectangleF(x, y, width, 1000);
            var size = g.MeasureString(text.Text, font, (int)width, format);

            g.DrawString(text.Text, font, brush, rect, format);

            return y + size.Height + 2;
        }

        /// <summary>
        /// 繪製線條
        /// </summary>
        private float RenderLine(System.Drawing.Graphics g, LineElement line, float x, float y, float width)
        {
            using var pen = new System.Drawing.Pen(DrawingColor.Black, line.Thickness);

            pen.DashStyle = line.Style switch
            {
                LineStyle.Dashed => DashStyle.Dash,
                LineStyle.Dotted => DashStyle.Dot,
                _ => DashStyle.Solid
            };

            float lineY = y + 2;
            g.DrawLine(pen, x, lineY, x + width, lineY);

            if (line.Style == LineStyle.Double)
            {
                g.DrawLine(pen, x, lineY + 3, x + width, lineY + 3);
                return y + 8;
            }

            return y + 5;
        }

        /// <summary>
        /// 繪製表格
        /// </summary>
        private float RenderTable(Graphics g, TableElement table, float x, float y, float width, DocumentPageSettings settings)
        {
            if (!table.Columns.Any()) return y;

            // 計算欄位寬度
            float totalRatio = table.Columns.Sum(c => c.WidthRatio);
            var columnWidths = table.Columns.Select(c => (c.WidthRatio / totalRatio) * width).ToArray();

            using var font = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize);
            using var boldFont = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize, System.Drawing.FontStyle.Bold);
            using var pen = new System.Drawing.Pen(DrawingColor.Black, 1);
            using var brush = new System.Drawing.SolidBrush(DrawingColor.Black);
            using var headerBrush = new System.Drawing.SolidBrush(DrawingColor.FromArgb(240, 240, 240));

            float currentY = y;
            float tableStartY = y;

            // 繪製表頭
            if (table.Columns.Any(c => !string.IsNullOrEmpty(c.Header)))
            {
                float headerHeight = table.HeaderRowHeight;

                // 表頭背景
                if (table.ShowHeaderBackground)
                {
                    g.FillRectangle(headerBrush, x, currentY, width, headerHeight);
                }

                // 繪製表頭文字（支援 | 分隔符）
                float cellX = x;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    var col = table.Columns[i];
                    
                    // 如果使用分隔符模式，在標題後加上 |（最後一欄除外）
                    string headerText = col.Header;
                    if (table.ShowHeaderSeparator && i < table.Columns.Count - 1)
                    {
                        headerText = $"{col.Header} |";
                    }
                    
                    var format = new StringFormat
                    {
                        Alignment = col.Alignment switch
                        {
                            TextAlignment.Center => StringAlignment.Center,
                            TextAlignment.Right => StringAlignment.Far,
                            _ => StringAlignment.Near
                        },
                        LineAlignment = StringAlignment.Center
                    };

                    var cellRect = new RectangleF(cellX + 2, currentY, columnWidths[i] - 4, headerHeight);
                    g.DrawString(headerText, boldFont, brush, cellRect, format);
                    cellX += columnWidths[i];
                }

                // 如果使用分隔符模式，繪製表頭下方的分隔線
                if (table.ShowHeaderUnderline)
                {
                    g.DrawLine(pen, x, currentY + headerHeight, x + width, currentY + headerHeight);
                }

                currentY += headerHeight;
            }

            // 繪製資料列（支援文字換行）
            foreach (var row in table.Rows)
            {
                // 先計算此行需要的最大高度
                float maxCellHeight = table.RowHeight;
                for (int i = 0; i < Math.Min(row.Cells.Count, table.Columns.Count); i++)
                {
                    var cellText = row.Cells[i];
                    if (!string.IsNullOrEmpty(cellText))
                    {
                        SizeF textSize = g.MeasureString(cellText, font, (int)(columnWidths[i] - 4));
                        if (textSize.Height > maxCellHeight)
                        {
                            maxCellHeight = textSize.Height + 4;
                        }
                    }
                }

                float cellX = x;
                for (int i = 0; i < Math.Min(row.Cells.Count, table.Columns.Count); i++)
                {
                    var col = table.Columns[i];
                    var format = new StringFormat
                    {
                        Alignment = col.Alignment switch
                        {
                            TextAlignment.Center => StringAlignment.Center,
                            TextAlignment.Right => StringAlignment.Far,
                            _ => StringAlignment.Near
                        },
                        LineAlignment = StringAlignment.Near  // 改為頂部對齊，支援多行
                    };

                    var cellRect = new RectangleF(cellX + 2, currentY + 2, columnWidths[i] - 4, maxCellHeight - 4);
                    g.DrawString(row.Cells[i], font, brush, cellRect, format);
                    cellX += columnWidths[i];
                }
                currentY += maxCellHeight;
            }

            // 繪製框線
            if (table.ShowBorder)
            {
                // 外框
                g.DrawRectangle(pen, x, tableStartY, width, currentY - tableStartY);

                // 水平線（表頭下方）
                if (table.Columns.Any(c => !string.IsNullOrEmpty(c.Header)))
                {
                    g.DrawLine(pen, x, tableStartY + table.HeaderRowHeight, x + width, tableStartY + table.HeaderRowHeight);
                }

                // 水平線（每行）
                float rowY = tableStartY + table.HeaderRowHeight;
                foreach (var _ in table.Rows.Take(table.Rows.Count - 1))
                {
                    rowY += table.RowHeight;
                    g.DrawLine(pen, x, rowY, x + width, rowY);
                }

                // 垂直線
                float colX = x;
                for (int i = 0; i < columnWidths.Length - 1; i++)
                {
                    colX += columnWidths[i];
                    g.DrawLine(pen, colX, tableStartY, colX, currentY);
                }
            }

            return currentY + 5;
        }

        /// <summary>
        /// 繪製圖片
        /// </summary>
        private float RenderImage(Graphics g, ImageElement imageElement, float x, float y, float width)
        {
            if (imageElement.ImageData == null || imageElement.ImageData.Length == 0)
                return y;

            try
            {
                using var ms = new MemoryStream(imageElement.ImageData);
                using var image = Image.FromStream(ms);

                float imgWidth = imageElement.Width ?? image.Width;
                float imgHeight = imageElement.Height ?? image.Height;

                // 如果指定了寬度但沒有高度，按比例計算
                if (imageElement.Width.HasValue && !imageElement.Height.HasValue)
                {
                    imgHeight = image.Height * (imgWidth / image.Width);
                }
                // 如果指定了高度但沒有寬度，按比例計算
                else if (imageElement.Height.HasValue && !imageElement.Width.HasValue)
                {
                    imgWidth = image.Width * (imgHeight / image.Height);
                }

                // 確保不超過可用寬度
                if (imgWidth > width)
                {
                    float scale = width / imgWidth;
                    imgWidth = width;
                    imgHeight *= scale;
                }

                // 計算 X 位置
                float imgX = imageElement.Alignment switch
                {
                    TextAlignment.Center => x + (width - imgWidth) / 2,
                    TextAlignment.Right => x + width - imgWidth,
                    _ => x
                };

                g.DrawImage(image, imgX, y, imgWidth, imgHeight);

                return y + imgHeight + 5;
            }
            catch
            {
                return y;
            }
        }

        /// <summary>
        /// 繪製簽名區
        /// </summary>
        private float RenderSignatureSection(System.Drawing.Graphics g, SignatureSectionElement signature, float x, float y, float width, DocumentPageSettings settings)
        {
            using var font = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize);
            using var pen = new System.Drawing.Pen(DrawingColor.Black, 1);
            using var brush = new System.Drawing.SolidBrush(DrawingColor.Black);

            float spacing = width / signature.Labels.Count;
            float currentX = x;

            foreach (var label in signature.Labels)
            {
                // 標籤
                g.DrawString($"{label}：", font, brush, currentX, y);

                // 簽名線
                var labelSize = g.MeasureString($"{label}：", font);
                float lineX = currentX + labelSize.Width;
                float lineY = y + labelSize.Height - 2;
                g.DrawLine(pen, lineX, lineY, lineX + signature.LineWidth, lineY);

                currentX += spacing;
            }

            return y + 50;
        }

        /// <summary>
        /// 繪製鍵值對行
        /// </summary>
        private float RenderKeyValueRow(System.Drawing.Graphics g, KeyValueRowElement kvRow, float x, float y, float width, DocumentPageSettings settings)
        {
            using var font = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize);
            using var brush = new System.Drawing.SolidBrush(DrawingColor.Black);

            float spacing = width / kvRow.Pairs.Count;
            float currentX = x;

            foreach (var (key, value) in kvRow.Pairs)
            {
                // 如果 key 為空，則只顯示 value（不加冒號）
                string displayText = string.IsNullOrEmpty(key) ? value : $"{key}：{value}";
                g.DrawString(displayText, font, brush, currentX, y);
                currentX += spacing;
            }

            var size = g.MeasureString("測試", font);
            return y + size.Height + 2;  // 縮短間距（原本 +5）
        }

        /// <summary>
        /// 繪製三欄標頭行（左側靠左、中間置中放大、右側靠右）
        /// </summary>
        private float RenderThreeColumnHeader(Graphics g, ThreeColumnHeaderElement header, float x, float y, float width, DocumentPageSettings settings)
        {
            using var sideFont = new System.Drawing.Font(settings.FontName, header.SideFontSize);
            var centerFontStyle = header.CenterBold ? FontStyle.Bold : FontStyle.Regular;
            using var centerFont = new System.Drawing.Font(settings.FontName, header.CenterFontSize, centerFontStyle);
            using var brush = new System.Drawing.SolidBrush(DrawingColor.Black);

            float sideLineHeight = sideFont.GetHeight(g);
            float centerLineHeight = centerFont.GetHeight(g);
            // 行高取決於是否有中間文字
            float lineHeight = string.IsNullOrEmpty(header.CenterText) ? sideLineHeight : Math.Max(sideLineHeight, centerLineHeight);

            // 左側文字（靠左）
            if (!string.IsNullOrEmpty(header.LeftText))
            {
                g.DrawString(header.LeftText, sideFont, brush, x, y + (lineHeight - sideLineHeight) / 2);
            }

            // 中間文字（置中）
            if (!string.IsNullOrEmpty(header.CenterText))
            {
                var centerSize = g.MeasureString(header.CenterText, centerFont);
                float centerX = x + (width - centerSize.Width) / 2;
                g.DrawString(header.CenterText, centerFont, brush, centerX, y + (lineHeight - centerLineHeight) / 2);
            }

            // 右側文字（靠右）
            if (!string.IsNullOrEmpty(header.RightText))
            {
                var rightSize = g.MeasureString(header.RightText, sideFont);
                float rightX = x + width - rightSize.Width;
                g.DrawString(header.RightText, sideFont, brush, rightX, y + (lineHeight - sideLineHeight) / 2);
            }

            return y + lineHeight + 1;  // 縮短間距
        }

        /// <summary>
        /// 測量報表標頭區塊高度
        /// </summary>
        private float MeasureReportHeaderBlockHeight(Graphics g, ReportHeaderBlockElement block, DocumentPageSettings settings)
        {
            // 以右側高度為基準
            using var rightFont = new System.Drawing.Font(settings.FontName, block.RightFontSize);
            float rightLineHeight = rightFont.GetHeight(g) + 1;
            float rightTotalHeight = block.RightLines.Count * rightLineHeight;

            return rightTotalHeight + 3;
        }

        /// <summary>
        /// 繪製報表標頭區塊（中間多行標題、右側多行資訊，各自有獨立行高）
        /// </summary>
        private float RenderReportHeaderBlock(Graphics g, ReportHeaderBlockElement block, float x, float y, float width, DocumentPageSettings settings)
        {
            using var brush = new System.Drawing.SolidBrush(DrawingColor.Black);

            // 以右側高度為基準
            using var rightFont = new System.Drawing.Font(settings.FontName, block.RightFontSize);
            float rightLineHeight = rightFont.GetHeight(g) + 1;
            float rightTotalHeight = block.RightLines.Count * rightLineHeight;
            float blockHeight = rightTotalHeight;

            // 計算中間區域每行的實際高度（字體高度）
            float centerContentHeight = 0;
            foreach (var (text, fontSize, bold) in block.CenterLines)
            {
                var fontStyle = bold ? FontStyle.Bold : FontStyle.Regular;
                using var font = new System.Drawing.Font(settings.FontName, fontSize, fontStyle);
                centerContentHeight += font.GetHeight(g);
            }

            // 計算中間行之間的間距，使總高度等於區塊高度
            float centerSpacing = 0;
            if (block.CenterLines.Count > 1)
            {
                centerSpacing = (blockHeight - centerContentHeight) / (block.CenterLines.Count - 1);
            }

            // 繪製中間標題（垂直分佈，使總高度與右側相同）
            float centerCurrentY = y;
            foreach (var (text, fontSize, bold) in block.CenterLines)
            {
                var fontStyle = bold ? FontStyle.Bold : FontStyle.Regular;
                using var font = new System.Drawing.Font(settings.FontName, fontSize, fontStyle);
                var textSize = g.MeasureString(text, font);
                float centerX = x + (width - textSize.Width) / 2;
                g.DrawString(text, font, brush, centerX, centerCurrentY);
                centerCurrentY += font.GetHeight(g) + centerSpacing;
            }

            // 繪製右側資訊（靠右對齊）
            float rightCurrentY = y;
            foreach (var line in block.RightLines)
            {
                var textSize = g.MeasureString(line, rightFont);
                float rightX = x + width - textSize.Width;
                g.DrawString(line, rightFont, brush, rightX, rightCurrentY);
                rightCurrentY += rightLineHeight;
            }

            return y + blockHeight + 3;
        }

        /// <summary>
        /// 測量文字高度
        /// </summary>
        private float MeasureTextHeight(Graphics g, TextElement text, float width, DocumentPageSettings settings)
        {
            var fontStyle = FontStyle.Regular;
            if (text.Bold) fontStyle |= FontStyle.Bold;
            if (text.Italic) fontStyle |= FontStyle.Italic;

            using var font = new Font(text.FontName ?? settings.FontName, text.FontSize, fontStyle);
            var size = g.MeasureString(text.Text, font, (int)width);
            return size.Height + 2;
        }

        /// <summary>
        /// 測量表格高度（支援動態行高）
        /// </summary>
        private float MeasureTableHeight(Graphics g, TableElement table, float availableWidth, DocumentPageSettings settings)
        {
            if (!table.Columns.Any()) return 0;

            float height = 0;

            // 表頭高度
            if (table.Columns.Any(c => !string.IsNullOrEmpty(c.Header)))
            {
                height += table.HeaderRowHeight;
            }

            // 計算欄位寬度
            float totalRatio = table.Columns.Sum(c => c.WidthRatio);
            var columnWidths = table.Columns.Select(c => (c.WidthRatio / totalRatio) * availableWidth).ToArray();

            using var font = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize);

            // 資料列高度（考慮換行）
            foreach (var row in table.Rows)
            {
                float maxCellHeight = table.RowHeight;
                for (int i = 0; i < Math.Min(row.Cells.Count, table.Columns.Count); i++)
                {
                    var cellText = row.Cells[i];
                    if (!string.IsNullOrEmpty(cellText))
                    {
                        SizeF textSize = g.MeasureString(cellText, font, (int)(columnWidths[i] - 4));
                        if (textSize.Height > maxCellHeight)
                        {
                            maxCellHeight = textSize.Height + 4;
                        }
                    }
                }
                height += maxCellHeight;
            }

            return height + 5;
        }

        /// <summary>
        /// 測量圖片高度
        /// </summary>
        private float MeasureImageHeight(ImageElement image, float availableWidth)
        {
            if (image.ImageData == null || image.ImageData.Length == 0)
                return 0;

            if (image.Height.HasValue)
                return image.Height.Value + 5;

            try
            {
                using var ms = new MemoryStream(image.ImageData);
                using var img = Image.FromStream(ms);

                float imgWidth = image.Width ?? img.Width;
                float imgHeight = img.Height;

                if (image.Width.HasValue)
                {
                    imgHeight = img.Height * (imgWidth / img.Width);
                }

                if (imgWidth > availableWidth)
                {
                    imgHeight *= availableWidth / imgWidth;
                }

                return imgHeight + 5;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 測量左右並排區塊高度（支援自動換行）
        /// </summary>
        private float MeasureTwoColumnSectionHeight(Graphics g, TwoColumnSectionElement twoCol, float availableWidth, DocumentPageSettings settings)
        {
            using var font = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize);
            float lineHeight = font.GetHeight(g) + 2;
            
            float leftWidth = availableWidth * twoCol.LeftWidthRatio - 15; // 邊框內距
            
            // 計算左側內容實際需要的行數（含換行）
            float leftTotalHeight = string.IsNullOrEmpty(twoCol.LeftTitle) ? 0 : lineHeight;
            foreach (var line in twoCol.LeftContent)
            {
                if (string.IsNullOrEmpty(line))
                {
                    leftTotalHeight += lineHeight;
                    continue;
                }
                SizeF textSize = g.MeasureString(line, font, (int)leftWidth);
                leftTotalHeight += textSize.Height;
            }
            
            // 計算右側行數
            int rightLines = twoCol.RightContent.Count + (string.IsNullOrEmpty(twoCol.RightTitle) ? 0 : 1);
            float rightTotalHeight = rightLines * lineHeight;
            
            float maxHeight = Math.Max(leftTotalHeight, rightTotalHeight);
            
            return maxHeight + 15; // 加上邊距
        }

        /// <summary>
        /// 繪製左右並排區塊（備註在左、金額在右，支援自動換行）
        /// </summary>
        private float RenderTwoColumnSection(Graphics g, TwoColumnSectionElement twoCol, float x, float y, float width, DocumentPageSettings settings)
        {
            using var font = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize);
            using var boldFont = new System.Drawing.Font(settings.FontName, settings.DefaultFontSize, FontStyle.Bold);
            using var brush = new System.Drawing.SolidBrush(DrawingColor.Black);
            using var pen = new System.Drawing.Pen(DrawingColor.Black, 1);

            float lineHeight = font.GetHeight(g) + 2;
            float leftWidth = width * twoCol.LeftWidthRatio - 10; // 留間距
            float rightWidth = width * (1 - twoCol.LeftWidthRatio);
            float rightX = x + leftWidth + 10;

            float startY = y;

            // 計算左側內容實際需要的高度（含換行）
            float leftContentHeight = string.IsNullOrEmpty(twoCol.LeftTitle) ? 0 : lineHeight;
            float leftInnerWidth = leftWidth - 10; // 邊框內寬度
            foreach (var line in twoCol.LeftContent)
            {
                if (string.IsNullOrEmpty(line))
                {
                    leftContentHeight += lineHeight;
                    continue;
                }
                SizeF textSize = g.MeasureString(line, font, (int)leftInnerWidth);
                leftContentHeight += textSize.Height;
            }
            
            // 計算右側行數
            int rightLines = twoCol.RightContent.Count + (string.IsNullOrEmpty(twoCol.RightTitle) ? 0 : 1);
            float rightContentHeight = rightLines * lineHeight;
            
            // 統一區塊高度
            float sectionHeight = Math.Max(leftContentHeight, rightContentHeight) + 10;

            // === 繪製左側（備註區，有邊框）===
            if (twoCol.LeftHasBorder)
            {
                g.DrawRectangle(pen, x, startY, leftWidth, sectionHeight);
            }

            float leftCurrentY = startY + 5; // 上內距
            if (!string.IsNullOrEmpty(twoCol.LeftTitle))
            {
                g.DrawString(twoCol.LeftTitle, boldFont, brush, x + 5, leftCurrentY);
                leftCurrentY += lineHeight;
            }

            // 繪製左側內容，支援自動換行
            foreach (var line in twoCol.LeftContent)
            {
                if (string.IsNullOrEmpty(line))
                {
                    leftCurrentY += lineHeight;
                    continue;
                }
                RectangleF textRect = new RectangleF(x + 5, leftCurrentY, leftInnerWidth, sectionHeight - (leftCurrentY - startY));
                SizeF textSize = g.MeasureString(line, font, (int)leftInnerWidth);
                g.DrawString(line, font, brush, textRect);
                leftCurrentY += textSize.Height;
            }

            // === 繪製右側（金額區）===
            if (twoCol.RightHasBorder)
            {
                g.DrawRectangle(pen, rightX, startY, rightWidth, sectionHeight);
            }

            float rightCurrentY = startY + 5; // 上內距
            if (!string.IsNullOrEmpty(twoCol.RightTitle))
            {
                g.DrawString(twoCol.RightTitle, boldFont, brush, rightX + 5, rightCurrentY);
                rightCurrentY += lineHeight;
            }

            // 右側內容靠右對齊
            foreach (var line in twoCol.RightContent)
            {
                var format = new StringFormat { Alignment = StringAlignment.Far }; // 靠右對齊
                var rect = new RectangleF(rightX, rightCurrentY, rightWidth - 5, lineHeight);
                g.DrawString(line, font, brush, rect, format);
                rightCurrentY += lineHeight;
            }

            return startY + sectionHeight + 5;
        }

        #endregion

        #region 內部類別

        /// <summary>
        /// 列印狀態追蹤
        /// </summary>
        private class PrintState
        {
            public int CurrentElementIndex { get; set; } = 0;
            public int CurrentTableRowIndex { get; set; } = 0;  // 追蹤當前表格已渲染到第幾行
            public bool TableHeaderRendered { get; set; } = false; // 追蹤當前頁是否已渲染表格表頭
            public bool NeedFooterPage { get; set; } = false;  // 內容已完成，但需要新頁面放頁尾
            public bool IsComplete { get; set; } = false;

            public PrintState(FormattedDocument document)
            {
                // 只有當主內容、頁首、頁尾都為空時才標記為完成
                // 如果有頁首或頁尾但主內容為空，仍需繪製一頁
                IsComplete = !document.Elements.Any() 
                          && !document.HeaderElements.Any() 
                          && !document.FooterElements.Any();
            }
            
            /// <summary>
            /// 重設表格狀態（進入新表格時）
            /// </summary>
            public void ResetTableState()
            {
                CurrentTableRowIndex = 0;
                TableHeaderRendered = false;
            }
        }

        #endregion
    }
}
