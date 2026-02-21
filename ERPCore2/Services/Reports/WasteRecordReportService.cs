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
    /// 廢料記錄報表服務實作（WL001）
    /// 支援單筆列印（EditModal）和清單式批次列印（篩選條件）
    /// </summary>
    public class WasteRecordReportService : IWasteRecordReportService
    {
        private readonly IWasteRecordService _wasteRecordService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<WasteRecordReportService>? _logger;

        public WasteRecordReportService(
            IWasteRecordService wasteRecordService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<WasteRecordReportService>? logger = null)
        {
            _wasteRecordService = wasteRecordService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一廢料記錄報表
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int wasteRecordId)
        {
            var record = await _wasteRecordService.GetByIdAsync(wasteRecordId);
            if (record == null)
                throw new ArgumentException($"找不到廢料記錄 ID: {wasteRecordId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildWasteRecordDocument(new List<WasteRecord> { record }, company, null);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int wasteRecordId)
        {
            var document = await GenerateReportAsync(wasteRecordId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int wasteRecordId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(wasteRecordId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印單筆廢料記錄
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int wasteRecordId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(wasteRecordId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印廢料記錄 {Id} 時發生錯誤", wasteRecordId);
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
                    return ServiceResult.Failure($"無符合條件的廢料記錄\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildWasteRecordDocument(records, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印廢料記錄時發生錯誤");
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

                if (!records.Any())
                    return BatchPreviewResult.Failure($"無符合條件的廢料記錄\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildWasteRecordDocument(records, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, records.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染廢料記錄報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region WasteRecordCriteria 批次報表

        /// <summary>
        /// 以廢料記錄篩選條件批次渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(WasteRecordCriteria criteria)
        {
            try
            {
                var records = await GetRecordsByTypedCriteriaAsync(criteria);

                if (!records.Any())
                    return BatchPreviewResult.Failure($"無符合條件的廢料記錄\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildWasteRecordDocument(records, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, records.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染廢料記錄報表（WasteRecordCriteria）時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>根據 BatchPrintCriteria 查詢廢料記錄</summary>
        private async Task<List<WasteRecord>> GetRecordsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _wasteRecordService.GetAllAsync();

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
                    r.Vehicle.LicensePlate.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return results.OrderBy(r => r.WasteType?.Name).ThenBy(r => r.RecordDate).ToList();
        }

        /// <summary>根據 WasteRecordCriteria 查詢廢料記錄</summary>
        private async Task<List<WasteRecord>> GetRecordsByTypedCriteriaAsync(WasteRecordCriteria criteria)
        {
            var results = await _wasteRecordService.GetAllAsync();

            if (criteria.ActiveOnly)
                results = results.Where(r => r.Status == EntityStatus.Active).ToList();

            if (criteria.WasteTypeIds.Any())
                results = results.Where(r => criteria.WasteTypeIds.Contains(r.WasteTypeId)).ToList();

            if (criteria.VehicleIds.Any())
                results = results.Where(r => criteria.VehicleIds.Contains(r.VehicleId)).ToList();

            if (criteria.CustomerIds.Any())
                results = results.Where(r => r.CustomerId.HasValue &&
                    criteria.CustomerIds.Contains(r.CustomerId.Value)).ToList();

            if (criteria.WarehouseIds.Any())
                results = results.Where(r => criteria.WarehouseIds.Contains(r.WarehouseId)).ToList();

            if (criteria.StartDate.HasValue)
                results = results.Where(r => r.RecordDate >= criteria.StartDate.Value).ToList();

            if (criteria.EndDate.HasValue)
                results = results.Where(r => r.RecordDate <= criteria.EndDate.Value).ToList();

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var kw = criteria.Keyword;
                results = results.Where(r =>
                    (r.Code != null && r.Code.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    r.Vehicle.LicensePlate.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return results.OrderBy(r => r.WasteType?.Name).ThenBy(r => r.RecordDate).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構廢料記錄報表（清單式：依廢料類型分組）
        /// </summary>
        private FormattedDocument BuildWasteRecordDocument(
            List<WasteRecord> records,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"廢料記錄表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("廢 料 記 錄 表", 16f, true)
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

            // === 依廢料類型分組顯示 ===
            var typeGroups = records
                .GroupBy(r => r.WasteType?.Name ?? "未分類")
                .OrderBy(g => g.Key);

            foreach (var group in typeGroups)
            {
                // 廢料類型標題
                doc.AddKeyValueRow(
                    ("廢料類型", $"{group.Key}（{group.Count()} 筆）"));

                doc.AddSpacing(3);

                // 廢料記錄資料表格
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.30f, TextAlignment.Center)
                         .AddColumn("廢料單號", 0.90f, TextAlignment.Left)
                         .AddColumn("記錄日期", 0.70f, TextAlignment.Center)
                         .AddColumn("車牌號碼", 0.70f, TextAlignment.Left)
                         .AddColumn("客戶", 0.90f, TextAlignment.Left)
                         .AddColumn("入庫倉庫", 0.70f, TextAlignment.Left)
                         .AddColumn("總重量(kg)", 0.70f, TextAlignment.Right)
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
                            record.TotalWeight?.ToString("N2") ?? "",
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

                var totalWeight = records.Sum(r => r.TotalWeight ?? 0);
                var totalDisposalFee = records.Sum(r => r.DisposalFee ?? 0);
                var totalPurchaseFee = records.Sum(r => r.PurchaseFee ?? 0);
                var totalNetAmount = records.Sum(r => r.NetAmount ?? 0);

                var summaryLines = new List<string>
                {
                    $"記錄總數：{records.Count} 筆",
                    $"總重量：{totalWeight:N2} kg",
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
