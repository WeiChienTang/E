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
    /// 車輛保養表報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 支援單筆列印和清單式批次列印（依車輛分組）
    /// </summary>
    public class VehicleMaintenanceReportService : IVehicleMaintenanceReportService
    {
        private readonly IVehicleMaintenanceService _maintenanceService;
        private readonly IVehicleService _vehicleService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<VehicleMaintenanceReportService>? _logger;

        public VehicleMaintenanceReportService(
            IVehicleMaintenanceService maintenanceService,
            IVehicleService vehicleService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<VehicleMaintenanceReportService>? logger = null)
        {
            _maintenanceService = maintenanceService;
            _vehicleService = vehicleService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一保養紀錄報表（與批次列印使用相同格式）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int maintenanceId)
        {
            var maintenance = await _maintenanceService.GetByIdAsync(maintenanceId);
            if (maintenance == null)
            {
                throw new ArgumentException($"找不到保養紀錄 ID: {maintenanceId}");
            }

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildMaintenanceListDocument(new List<VehicleMaintenance> { maintenance }, company, null);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int maintenanceId)
        {
            var document = await GenerateReportAsync(maintenanceId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int maintenanceId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(maintenanceId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印保養紀錄
        /// </summary>
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
                _logger?.LogError(ex, "列印保養紀錄 {MaintenanceId} 時發生錯誤", maintenanceId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次直接列印
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var maintenances = await GetMaintenancesByCriteriaAsync(criteria);

                if (!maintenances.Any())
                {
                    return ServiceResult.Failure($"無符合條件的保養紀錄\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildMaintenanceListDocument(maintenances, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印車輛保養表時發生錯誤");
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
                var maintenances = await GetMaintenancesByCriteriaAsync(criteria);

                if (!maintenances.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的保養紀錄\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildMaintenanceListDocument(maintenances, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, maintenances.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染車輛保養表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region VehicleMaintenanceCriteria 批次報表

        /// <summary>
        /// 以車輛保養表專用篩選條件渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(VehicleMaintenanceCriteria criteria)
        {
            try
            {
                var maintenances = await GetMaintenancesByTypedCriteriaAsync(criteria);

                if (!maintenances.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的保養紀錄\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildMaintenanceListDocument(maintenances, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, maintenances.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染車輛保養表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>
        /// 根據 BatchPrintCriteria 查詢保養紀錄
        /// </summary>
        private async Task<List<VehicleMaintenance>> GetMaintenancesByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _maintenanceService.GetAllAsync();

            // 篩選車輛 ID
            if (criteria.RelatedEntityIds.Any())
            {
                results = results.Where(m => criteria.RelatedEntityIds.Contains(m.VehicleId)).ToList();
            }

            // 日期範圍
            if (criteria.StartDate.HasValue)
            {
                results = results.Where(m => m.MaintenanceDate >= criteria.StartDate.Value).ToList();
            }
            if (criteria.EndDate.HasValue)
            {
                results = results.Where(m => m.MaintenanceDate <= criteria.EndDate.Value).ToList();
            }

            // 關鍵字搜尋（車牌號碼、維修廠）
            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(m =>
                    (m.Vehicle?.LicensePlate != null && m.Vehicle.LicensePlate.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (m.ServiceProvider != null && m.ServiceProvider.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Description != null && m.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return results.OrderBy(m => m.Vehicle?.LicensePlate).ThenByDescending(m => m.MaintenanceDate).ToList();
        }

        /// <summary>
        /// 根據 VehicleMaintenanceCriteria 查詢保養紀錄
        /// </summary>
        private async Task<List<VehicleMaintenance>> GetMaintenancesByTypedCriteriaAsync(VehicleMaintenanceCriteria criteria)
        {
            var results = await _maintenanceService.GetAllAsync();

            // 篩選車輛
            if (criteria.VehicleIds.Any())
            {
                results = results.Where(m => criteria.VehicleIds.Contains(m.VehicleId)).ToList();
            }

            // 篩選保養類型
            if (criteria.MaintenanceTypes.Any())
            {
                results = results.Where(m => criteria.MaintenanceTypes.Contains(m.MaintenanceType)).ToList();
            }

            // 日期範圍
            if (criteria.StartDate.HasValue)
            {
                results = results.Where(m => m.MaintenanceDate >= criteria.StartDate.Value).ToList();
            }
            if (criteria.EndDate.HasValue)
            {
                results = results.Where(m => m.MaintenanceDate <= criteria.EndDate.Value).ToList();
            }

            // 關鍵字搜尋
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(m =>
                    (m.Vehicle?.LicensePlate != null && m.Vehicle.LicensePlate.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (m.ServiceProvider != null && m.ServiceProvider.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Description != null && m.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return results.OrderBy(m => m.Vehicle?.LicensePlate).ThenByDescending(m => m.MaintenanceDate).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構車輛保養表（清單式：依車輛分組）
        /// </summary>
        private FormattedDocument BuildMaintenanceListDocument(
            List<VehicleMaintenance> maintenances,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"車輛保養表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("車 輛 保 養 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"紀錄數：{maintenances.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依車輛分組顯示 ===
            var vehicleGroups = maintenances
                .GroupBy(m => new { m.VehicleId, LicensePlate = m.Vehicle?.LicensePlate ?? "未知車輛", VehicleName = m.Vehicle?.VehicleName ?? "" })
                .OrderBy(g => g.Key.LicensePlate);

            foreach (var group in vehicleGroups)
            {
                // 車輛標題
                doc.AddKeyValueRow(
                    ("車輛", $"{group.Key.LicensePlate} {group.Key.VehicleName}（{group.Count()} 筆）"));

                doc.AddSpacing(3);

                // 保養紀錄表格
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.30f, TextAlignment.Center)
                         .AddColumn("保養日期", 0.65f, TextAlignment.Center)
                         .AddColumn("保養類型", 0.60f, TextAlignment.Left)
                         .AddColumn("保養內容", 1.40f, TextAlignment.Left)
                         .AddColumn("里程數", 0.55f, TextAlignment.Right)
                         .AddColumn("費用", 0.55f, TextAlignment.Right)
                         .AddColumn("維修廠", 0.70f, TextAlignment.Left)
                         .AddColumn("經手人", 0.50f, TextAlignment.Left)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var maintenance in group.OrderByDescending(m => m.MaintenanceDate))
                    {
                        var maintenanceTypeText = GetMaintenanceTypeText(maintenance.MaintenanceType);

                        table.AddRow(
                            rowNum.ToString(),
                            maintenance.MaintenanceDate.ToString("yyyy/MM/dd"),
                            maintenanceTypeText,
                            maintenance.Description ?? "",
                            maintenance.MileageAtMaintenance?.ToString("N0") ?? "",
                            maintenance.Cost?.ToString("N0") ?? "",
                            maintenance.ServiceProvider ?? "",
                            maintenance.Employee?.Name ?? ""
                        );
                        rowNum++;
                    }
                });

                // 小計
                var groupCost = group.Sum(m => m.Cost ?? 0);
                if (groupCost > 0)
                {
                    doc.AddKeyValueRow(
                        ("小計", $"費用合計：{groupCost:N0} 元"));
                }

                doc.AddSpacing(5);
            }

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var totalCost = maintenances.Sum(m => m.Cost ?? 0);
                var summaryLines = new List<string>
                {
                    $"保養紀錄總數：{maintenances.Count} 筆",
                    $"費用總計：{totalCost:N0} 元"
                };

                // 按保養類型統計
                var typeGroups = maintenances
                    .GroupBy(m => m.MaintenanceType)
                    .OrderBy(g => g.Key);

                summaryLines.Add("");
                foreach (var tg in typeGroups)
                {
                    var typeCost = tg.Sum(m => m.Cost ?? 0);
                    summaryLines.Add($"  {GetMaintenanceTypeText(tg.Key)}：{tg.Count()} 筆 / {typeCost:N0} 元");
                }

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.6f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "車輛管理員" });
            });

            return doc;
        }


        /// <summary>
        /// 取得保養類型顯示文字
        /// </summary>
        private static string GetMaintenanceTypeText(MaintenanceType type)
        {
            return type switch
            {
                MaintenanceType.RegularService => "定期保養",
                MaintenanceType.Repair => "維修",
                MaintenanceType.TireChange => "輪胎更換",
                MaintenanceType.OilChange => "換機油",
                MaintenanceType.Insurance => "保險",
                MaintenanceType.Inspection => "驗車",
                MaintenanceType.Other => "其他",
                _ => ""
            };
        }

        #endregion
    }
}
