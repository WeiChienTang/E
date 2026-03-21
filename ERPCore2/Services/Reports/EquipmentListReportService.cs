using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 設備清單報表服務實作
    /// </summary>
    public class EquipmentListReportService : IEquipmentListReportService
    {
        private readonly IEquipmentService _equipmentService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<EquipmentListReportService>? _logger;

        public EquipmentListReportService(
            IEquipmentService equipmentService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<EquipmentListReportService>? logger = null)
        {
            _equipmentService = equipmentService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        public async Task<FormattedDocument> GenerateReportAsync(int equipmentId)
        {
            var equipment = await _equipmentService.GetByIdAsync(equipmentId);
            if (equipment == null)
                throw new ArgumentException($"找不到設備 ID: {equipmentId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildEquipmentListDocument(new List<Equipment> { equipment }, company, null);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int equipmentId)
        {
            var document = await GenerateReportAsync(equipmentId);
            return _formattedPrintService.RenderToImages(document);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int equipmentId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(equipmentId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int equipmentId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(equipmentId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印設備資料 {EquipmentId} 時發生錯誤", equipmentId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var equipments = await GetEquipmentsByCriteriaAsync(criteria);
                if (!equipments.Any())
                    return ServiceResult.Failure($"無符合條件的設備\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildEquipmentListDocument(equipments, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印設備清單時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            try
            {
                var equipments = await GetEquipmentsByCriteriaAsync(criteria);
                equipments = equipments.ExcludeDrafts();

                if (!equipments.Any())
                    return BatchPreviewResult.Failure($"無符合條件的設備\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildEquipmentListDocument(equipments, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, equipments.Count, new List<FormattedDocument> { document });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染設備清單時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region EquipmentCriteria 批次報表

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(EquipmentCriteria criteria)
        {
            try
            {
                var equipments = await GetEquipmentsByTypedCriteriaAsync(criteria);
                equipments = equipments.ExcludeDrafts();

                if (!equipments.Any())
                    return BatchPreviewResult.Failure($"無符合條件的設備\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildEquipmentListDocument(equipments, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, equipments.Count, new List<FormattedDocument> { document });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染設備清單時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        private async Task<List<Equipment>> GetEquipmentsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _equipmentService.GetAllAsync();
            return results.OrderBy(e => e.EquipmentCategory?.Name).ThenBy(e => e.Name).ToList();
        }

        private async Task<List<Equipment>> GetEquipmentsByTypedCriteriaAsync(EquipmentCriteria criteria)
        {
            var results = await _equipmentService.GetAllAsync();

            if (criteria.EquipmentCategoryId.HasValue)
                results = results.Where(e => e.EquipmentCategoryId == criteria.EquipmentCategoryId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var kw = criteria.Keyword;
                results = results.Where(e =>
                    e.Name.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    (e.Code != null && e.Code.Contains(kw, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Brand != null && e.Brand.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (criteria.PurchaseDateStart.HasValue)
                results = results.Where(e => e.PurchaseDate >= criteria.PurchaseDateStart).ToList();
            if (criteria.PurchaseDateEnd.HasValue)
                results = results.Where(e => e.PurchaseDate <= criteria.PurchaseDateEnd).ToList();

            if (criteria.NextMaintenanceDateStart.HasValue)
                results = results.Where(e => e.NextMaintenanceDate >= criteria.NextMaintenanceDateStart).ToList();
            if (criteria.NextMaintenanceDateEnd.HasValue)
                results = results.Where(e => e.NextMaintenanceDate <= criteria.NextMaintenanceDateEnd).ToList();

            return results.OrderBy(e => e.EquipmentCategory?.Name).ThenBy(e => e.Name).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        private FormattedDocument BuildEquipmentListDocument(
            List<Equipment> equipments,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"設備清單-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("設 備 清 單", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"設備數：{equipments.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依類別分組顯示 ===
            var categoryGroups = equipments
                .GroupBy(e => e.EquipmentCategory?.Name ?? "未分類")
                .OrderBy(g => g.Key);

            foreach (var group in categoryGroups)
            {
                doc.AddKeyValueRow(
                    ("類別", $"{group.Key}（{group.Count()} 台）"));

                doc.AddSpacing(3);

                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.30f, TextAlignment.Center)
                         .AddColumn("設備編號", 0.70f, TextAlignment.Left)
                         .AddColumn("設備名稱", 1.00f, TextAlignment.Left)
                         .AddColumn("品牌", 0.55f, TextAlignment.Left)
                         .AddColumn("型號", 0.55f, TextAlignment.Left)
                         .AddColumn("放置地點", 0.80f, TextAlignment.Left)
                         .AddColumn("負責人員", 0.60f, TextAlignment.Left)
                         .AddColumn("下次保養", 0.65f, TextAlignment.Center)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var equipment in group.OrderBy(e => e.Name))
                    {
                        table.AddRow(
                            rowNum.ToString(),
                            equipment.Code ?? "",
                            equipment.Name,
                            equipment.Brand ?? "",
                            equipment.Model ?? "",
                            equipment.Location ?? "",
                            equipment.Employee?.Name ?? "",
                            equipment.NextMaintenanceDate?.ToString("yyyy/MM/dd") ?? ""
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
                footer.AddKeyValueRow(("設備總數", $"{equipments.Count} 台"));
            });

            return doc;
        }

        #endregion
    }
}
