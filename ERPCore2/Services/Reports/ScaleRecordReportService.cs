using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 磅秤紀錄報表服務實作（WL001）
    /// 支援單筆列印（EditModal）和清單式批次列印（篩選條件）
    /// </summary>
    public class ScaleRecordReportService : IScaleRecordReportService
    {
        private readonly IScaleRecordService _scaleRecordService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<ScaleRecordReportService>? _logger;

        public ScaleRecordReportService(
            IScaleRecordService scaleRecordService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<ScaleRecordReportService>? logger = null)
        {
            _scaleRecordService = scaleRecordService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一磅秤紀錄報表
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int scaleRecordId)
        {
            var record = await _scaleRecordService.GetByIdAsync(scaleRecordId);
            if (record == null)
                throw new ArgumentException($"找不到磅秤紀錄 ID: {scaleRecordId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildScaleRecordDocument(new List<ScaleRecord> { record }, company, null);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int scaleRecordId)
        {
            var document = await GenerateReportAsync(scaleRecordId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int scaleRecordId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(scaleRecordId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印單筆磅秤紀錄
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int scaleRecordId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(scaleRecordId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印磅秤紀錄 {Id} 時發生錯誤", scaleRecordId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次直接列印（標準 BatchPrintCriteria）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var records = await GetRecordsByCriteriaAsync(criteria);

                if (!records.Any())
                    return ServiceResult.Failure($"無符合條件的磅秤紀錄\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildScaleRecordDocument(records, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印磅秤紀錄時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染報表為圖片（標準 BatchPrintCriteria）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            try
            {
                var records = await GetRecordsByCriteriaAsync(criteria);
                records = records.ExcludeDrafts();

                if (!records.Any())
                    return BatchPreviewResult.Failure($"無符合條件的磅秤紀錄\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildScaleRecordDocument(records, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, records.Count, new List<FormattedDocument> { document });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染磅秤紀錄報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region ScaleRecordCriteria 批次報表

        /// <summary>
        /// 以磅秤紀錄篩選條件批次渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(ScaleRecordCriteria criteria)
        {
            try
            {
                var records = await GetRecordsByTypedCriteriaAsync(criteria);
                records = records.ExcludeDrafts();

                if (!records.Any())
                    return BatchPreviewResult.Failure($"無符合條件的磅秤紀錄\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildScaleRecordDocument(records, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, records.Count, new List<FormattedDocument> { document });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染磅秤紀錄報表（ScaleRecordCriteria）時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>根據 BatchPrintCriteria 查詢磅秤紀錄</summary>
        private async Task<List<ScaleRecord>> GetRecordsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _scaleRecordService.GetAllAsync();

            if (!criteria.IncludeCancelled)
                results = results.Where(r => r.Status == EntityStatus.Active).ToList();

            if (criteria.StartDate.HasValue)
                results = results.Where(r => r.RecordDate >= criteria.StartDate.Value).ToList();

            if (criteria.EndDate.HasValue)
                results = results.Where(r => r.RecordDate <= criteria.EndDate.Value).ToList();

            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var kw = criteria.DocumentNumberKeyword;
                results = results.Where(r =>
                    (r.Code != null && r.Code.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (r.Vehicle != null && r.Vehicle.LicensePlate.Contains(kw, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return results.OrderBy(r => r.Product?.Name).ThenBy(r => r.RecordDate).ToList();
        }

        /// <summary>根據 ScaleRecordCriteria 查詢磅秤紀錄</summary>
        private async Task<List<ScaleRecord>> GetRecordsByTypedCriteriaAsync(ScaleRecordCriteria criteria)
        {
            var results = await _scaleRecordService.GetAllAsync();

            if (criteria.ActiveOnly)
                results = results.Where(r => r.Status == EntityStatus.Active).ToList();

            if (criteria.ScaleTypeIds.Any())
                results = results.Where(r => r.ProductId.HasValue && criteria.ScaleTypeIds.Contains(r.ProductId.Value)).ToList();

            if (criteria.VehicleIds.Any())
                results = results.Where(r => r.VehicleId.HasValue && criteria.VehicleIds.Contains(r.VehicleId.Value)).ToList();

            if (criteria.CustomerIds.Any())
                results = results.Where(r => r.CustomerId.HasValue &&
                    criteria.CustomerIds.Contains(r.CustomerId.Value)).ToList();

            if (criteria.WarehouseIds.Any())
                results = results.Where(r => r.WarehouseId.HasValue && criteria.WarehouseIds.Contains(r.WarehouseId.Value)).ToList();

            if (criteria.StartDate.HasValue)
                results = results.Where(r => r.RecordDate >= criteria.StartDate.Value).ToList();

            if (criteria.EndDate.HasValue)
                results = results.Where(r => r.RecordDate <= criteria.EndDate.Value).ToList();

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var kw = criteria.Keyword;
                results = results.Where(r =>
                    (r.Code != null && r.Code.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (r.Vehicle != null && r.Vehicle.LicensePlate.Contains(kw, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return results.OrderBy(r => r.Product?.Name).ThenBy(r => r.RecordDate).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構磅秤紀錄報表（清單式：依磅秤類型分組）
        /// </summary>
        private FormattedDocument BuildScaleRecordDocument(
            List<ScaleRecord> records,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"磅秤紀錄表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("磅 秤 紀 錄 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"記錄數：{records.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依品項分組顯示 ===
            var typeGroups = records
                .GroupBy(r => r.Product?.Name ?? "未分類")
                .OrderBy(g => g.Key);

            foreach (var group in typeGroups)
            {
                // 品項標題
                doc.AddKeyValueRow(
                    ("品項", $"{group.Key}（{group.Count()} 筆）"));

                doc.AddSpacing(3);

                // 磅秤紀錄資料表格
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.30f, TextAlignment.Center)
                         .AddColumn("磅秤紀錄單號", 0.90f, TextAlignment.Left)
                         .AddColumn("記錄日期", 0.70f, TextAlignment.Center)
                         .AddColumn("車牌號碼", 0.70f, TextAlignment.Left)
                         .AddColumn("客戶", 0.90f, TextAlignment.Left)
                         .AddColumn("入庫倉庫", 0.70f, TextAlignment.Left)
                         .AddColumn("淨重(kg)", 0.70f, TextAlignment.Right)
                         .AddColumn("處理費", 0.65f, TextAlignment.Right)
                         .AddColumn("採購費", 0.65f, TextAlignment.Right)
                         .AddColumn("淨額", 0.65f, TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var record in group.OrderBy(r => r.RecordDate))
                    {
                        table.AddRow(
                            rowNum.ToString(),
                            record.Code ?? "",
                            record.RecordDate.ToString("yyyy/MM/dd"),
                            record.Vehicle?.LicensePlate ?? "",
                            record.Customer?.CompanyName ?? "",
                            record.Warehouse?.Name ?? "",
                            record.NetWeight?.ToString("N2") ?? "",
                            record.DisposalFee?.ToString("N0") ?? "",
                            record.PurchaseFee?.ToString("N0") ?? "",
                            record.NetAmount?.ToString("N0") ?? ""
                        );
                        rowNum++;
                    }
                });

                doc.AddSpacing(5);
            }

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var totalWeight = records.Sum(r => r.NetWeight ?? 0);
                var totalDisposalFee = records.Sum(r => r.DisposalFee ?? 0);
                var totalPurchaseFee = records.Sum(r => r.PurchaseFee ?? 0);
                var totalNetAmount = records.Sum(r => r.NetAmount ?? 0);

                var summaryLines = new List<string>
                {
                    $"記錄總數：{records.Count} 筆",
                    $"總淨重：{totalWeight:N2} kg",
                    $"處理費合計：{totalDisposalFee:N0}",
                    $"採購費合計：{totalPurchaseFee:N0}",
                    $"淨額合計：{totalNetAmount:N0}"
                };

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.6f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "主管" });
            });

            return doc;
        }

        #endregion
    }
}
