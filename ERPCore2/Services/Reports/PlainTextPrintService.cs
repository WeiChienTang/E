using ERPCore2.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 純文字列印服務介面
    /// 提供純文字報表的直接列印功能
    /// </summary>
    public interface IPlainTextPrintService
    {
        /// <summary>
        /// 執行純文字列印
        /// </summary>
        /// <param name="textContent">純文字內容</param>
        /// <param name="printerName">印表機名稱</param>
        /// <param name="documentName">文件名稱</param>
        /// <param name="fontSize">字型大小（預設 10）</param>
        /// <returns>列印結果</returns>
        ServiceResult PrintText(string textContent, string printerName, string documentName, float fontSize = 10);

        /// <summary>
        /// 使用報表配置列印純文字
        /// </summary>
        /// <param name="textContent">純文字內容</param>
        /// <param name="reportId">報表識別碼</param>
        /// <param name="documentName">文件名稱</param>
        /// <returns>列印結果</returns>
        Task<ServiceResult> PrintTextByReportIdAsync(string textContent, string reportId, string documentName);

        /// <summary>
        /// 檢查是否支援直接列印
        /// </summary>
        /// <returns>是否支援</returns>
        bool IsDirectPrintSupported();
    }

    /// <summary>
    /// 純文字列印服務實作
    /// 使用 System.Drawing.Printing 實現純文字直接列印功能
    /// </summary>
    [SupportedOSPlatform("windows6.1")]
    public class PlainTextPrintService : IPlainTextPrintService
    {
        private readonly IReportPrintConfigurationService _reportPrintConfigService;
        private readonly IPrinterConfigurationService _printerConfigService;
        private readonly ILogger<PlainTextPrintService>? _logger;

        /// <summary>
        /// 預設字型名稱（等寬字型）
        /// </summary>
        public const string DefaultFontName = "Courier New";

        /// <summary>
        /// 預設字型大小
        /// </summary>
        public const float DefaultFontSize = 10f;

        /// <summary>
        /// 列印後等待時間（毫秒）
        /// </summary>
        public const int PrintWaitTimeMs = 2000;

        public PlainTextPrintService(
            IReportPrintConfigurationService reportPrintConfigService,
            IPrinterConfigurationService printerConfigService,
            ILogger<PlainTextPrintService>? logger = null)
        {
            _reportPrintConfigService = reportPrintConfigService;
            _printerConfigService = printerConfigService;
            _logger = logger;
        }

        /// <summary>
        /// 檢查是否支援直接列印
        /// </summary>
        public bool IsDirectPrintSupported()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        /// <summary>
        /// 執行純文字列印
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public ServiceResult PrintText(string textContent, string printerName, string documentName, float fontSize = DefaultFontSize)
        {
            try
            {
                if (!IsDirectPrintSupported())
                {
                    return ServiceResult.Failure("直接列印功能僅支援 Windows 平台");
                }

                if (string.IsNullOrWhiteSpace(textContent))
                {
                    return ServiceResult.Failure("列印內容不能為空");
                }

                if (string.IsNullOrWhiteSpace(printerName))
                {
                    return ServiceResult.Failure("印表機名稱不能為空");
                }

                _logger?.LogInformation("開始列印 {DocumentName}，印表機：{PrinterName}", documentName, printerName);

                using var printDocument = new System.Drawing.Printing.PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.DocumentName = documentName;

                // 檢查印表機是否有效
                if (!printDocument.PrinterSettings.IsValid)
                {
                    return ServiceResult.Failure($"印表機 '{printerName}' 無效或不可用");
                }

                // 設定列印內容
                var lines = textContent.Split('\n').ToList();
                var currentLine = 0;
                Exception? printException = null;

                printDocument.PrintPage += (sender, e) =>
                {
                    try
                    {
                        if (e.Graphics == null) return;

                        float yPos = e.MarginBounds.Top;
                        float leftMargin = e.MarginBounds.Left;

                        // 使用等寬字型
                        using var font = new System.Drawing.Font(DefaultFontName, fontSize);
                        var lineHeight = font.GetHeight(e.Graphics);

                        // 列印每一行
                        while (currentLine < lines.Count && yPos + lineHeight < e.MarginBounds.Bottom)
                        {
                            var line = lines[currentLine].TrimEnd('\r');

                            // 處理分頁符號
                            if (line == "\f")
                            {
                                currentLine++;
                                e.HasMorePages = true;
                                return;
                            }

                            e.Graphics.DrawString(line, font, System.Drawing.Brushes.Black, leftMargin, yPos);
                            yPos += lineHeight;
                            currentLine++;
                        }

                        // 如果還有更多行，設定需要更多頁面
                        e.HasMorePages = (currentLine < lines.Count);
                    }
                    catch (Exception ex)
                    {
                        printException = ex;
                        e.HasMorePages = false;
                    }
                };

                // 執行列印
                printDocument.Print();

                // 等待列印處理
                Thread.Sleep(PrintWaitTimeMs);

                if (printException != null)
                {
                    _logger?.LogError(printException, "列印 {DocumentName} 時發生錯誤", documentName);
                    return ServiceResult.Failure($"列印時發生錯誤: {printException.Message}");
                }

                _logger?.LogInformation("{DocumentName} 列印完成", documentName);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印 {DocumentName} 時發生錯誤", documentName);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 使用報表配置列印純文字
        /// </summary>
        public async Task<ServiceResult> PrintTextByReportIdAsync(string textContent, string reportId, string documentName)
        {
            try
            {
                if (!IsDirectPrintSupported())
                {
                    return ServiceResult.Failure("直接列印功能僅支援 Windows 平台");
                }

                // 載入列印配置
                var printConfig = await _reportPrintConfigService.GetByReportIdAsync(reportId);
                if (printConfig == null)
                {
                    return ServiceResult.Failure($"找不到報表 '{reportId}' 的列印配置");
                }

                // 載入印表機配置
                if (!printConfig.PrinterConfigurationId.HasValue)
                {
                    return ServiceResult.Failure("列印配置未設定印表機");
                }

                var printerConfig = await _printerConfigService.GetByIdAsync(printConfig.PrinterConfigurationId.Value);
                if (printerConfig == null)
                {
                    return ServiceResult.Failure("印表機配置不存在");
                }

                // 執行列印
                return PrintText(textContent, printerConfig.Name, documentName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "使用配置列印 {DocumentName} 時發生錯誤，ReportId: {ReportId}", documentName, reportId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }
    }
}
