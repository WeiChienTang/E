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
    /// 設備保養維修記錄報表服務實作
    /// </summary>
    public class EquipmentMaintenanceReportService : IEquipmentMaintenanceReportService
    {
        private readonly IEquipmentMaintenanceService _maintenanceService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<EquipmentMaintenanceReportService>? _logger;

        public EquipmentMaintenanceReportService(
            IEquipmentMaintenanceService maintenanceService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<EquipmentMaintenanceReportService>? logger = null)
        {
            _maintenanceService = maintenanceService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        public async Task<FormattedDocument> GenerateReportAsync(int maintenanceId)
        {
            var maintenance = await _maintenanceService.GetByIdAsync(maintenanceId);
            if (maintenance == null)
                throw new ArgumentException($"找不到保養維修記錄 ID: {maintenanceId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildMaintenanceDocument(new List<EquipmentMaintenance> { maintenance }, company, null);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int maintenanceId)
        {
            var document = await GenerateReportAsync(maintenanceId);
            return _formattedPrintService.RenderToImages(document);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int maintenanceId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(maintenanceId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int maintenanceId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(maintenanceId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印保養維修記錄 {MaintenanceId} 時發生錯誤", maintenanceId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var records = await GetRecordsByCriteriaAsync(criteria);
                if (!records.Any())
                    return ServiceResult.Failure($"無符合條件的保養維修記錄\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildMaintenanceDocument(records, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印保養維修記錄時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            try
            {
                var records = await GetRecordsByCriteriaAsync(criteria);
                records = records.ExcludeDrafts();

                if (!records.Any())
                    return BatchPreviewResult.Failure($"無符合條件的保養維修記錄\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildMaintenanceDocument(records, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, records.Count, new List<FormattedDocument> { document });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染保養維修記錄時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region EquipmentMaintenanceCriteria 批次報表

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(EquipmentMaintenanceCriteria criteria)
        {
            try
            {
                var records = await GetRecordsByTypedCriteriaAsync(criteria);
                records = records.ExcludeDrafts();

                if (!records.Any())
                    return BatchPreviewResult.Failure($"無符合條件的保養維修記錄\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildMaintenanceDocument(records, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, records.Count, new List<FormattedDocument> { document });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染保養維修記錄時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        private async Task<List<EquipmentMaintenance>> GetRecordsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _maintenanceService.GetAllAsync();
            return results.OrderByDescending(r => r.MaintenanceDate).ToList();
        }

        private async Task<List<EquipmentMaintenance>> GetRecordsByTypedCriteriaAsync(EquipmentMaintenanceCriteria criteria)
        {
            var results = await _maintenanceService.GetAllAsync();

            if (criteria.EquipmentId.HasValue)
                results = results.Where(r => r.EquipmentId == criteria.EquipmentId.Value).ToList();

            if (criteria.MaintenanceType.HasValue)
                results = results.Where(r => (int)r.MaintenanceType == criteria.MaintenanceType.Value).ToList();

            if (criteria.MaintenanceDateStart.HasValue)
                results = results.Where(r => r.MaintenanceDate >= criteria.MaintenanceDateStart).ToList();

            if (criteria.MaintenanceDateEnd.HasValue)
                results = results.Where(r => r.MaintenanceDate <= criteria.MaintenanceDateEnd).ToList();

            return results.OrderByDescending(r => r.MaintenanceDate).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        private FormattedDocument BuildMaintenanceDocument(
            List<EquipmentMaintenance> records,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"設備保養維修記錄-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("設 備 保 養 維 修 記 錄", 16f, true)
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

            // === 記錄表格 ===
            doc.AddTable(table =>
            {
                table.AddColumn("項次", 0.30f, TextAlignment.Center)
                     .AddColumn("記錄編號", 0.65f, TextAlignment.Left)
                     .AddColumn("設備名稱", 0.90f, TextAlignment.Left)
                     .AddColumn("維修類型", 0.55f, TextAlignment.Center)
                     .AddColumn("保養日期", 0.65f, TextAlignment.Center)
                     .AddColumn("費用", 0.55f, TextAlignment.Right)
                     .AddColumn("服務廠商", 0.75f, TextAlignment.Left)
                     .AddColumn("執行人員", 0.55f, TextAlignment.Left)
                     .AddColumn("下次保養", 0.65f, TextAlignment.Center)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                int rowNum = 1;
                foreach (var record in records)
                {
                    var maintenanceTypeText = record.MaintenanceType switch
                    {
                        ERPCore2.Models.Enums.EquipmentMaintenanceType.RegularMaintenance => "定期保養",
                        ERPCore2.Models.Enums.EquipmentMaintenanceType.Repair => "維修",
                        ERPCore2.Models.Enums.EquipmentMaintenanceType.Calibration => "校驗",
                        ERPCore2.Models.Enums.EquipmentMaintenanceType.Inspection => "檢查",
                        _ => "其他"
                    };

                    table.AddRow(
                        rowNum.ToString(),
                        record.Code ?? "",
                        record.Equipment?.Name ?? "",
                        maintenanceTypeText,
                        record.MaintenanceDate.ToString("yyyy/MM/dd"),
                        record.Cost?.ToString("N0") ?? "",
                        record.ServiceProvider ?? "",
                        record.Employee?.Name ?? "",
                        record.NextMaintenanceDate?.ToString("yyyy/MM/dd") ?? ""
                    );
                    rowNum++;
                }
            });

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);
                var totalCost = records.Where(r => r.Cost.HasValue).Sum(r => r.Cost!.Value);
                footer.AddKeyValueRow(
                    ("記錄總數", $"{records.Count} 筆"),
                    ("費用合計", $"{totalCost:N0}"));
            });

            return doc;
        }

        #endregion
    }
}
