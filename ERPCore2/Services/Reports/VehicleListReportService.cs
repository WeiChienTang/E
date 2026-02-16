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
    /// 車輛管理表報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 支援單筆列印和清單式批次列印
    /// </summary>
    public class VehicleListReportService : IVehicleListReportService
    {
        private readonly IVehicleService _vehicleService;
        private readonly IVehicleTypeService _vehicleTypeService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<VehicleListReportService>? _logger;

        public VehicleListReportService(
            IVehicleService vehicleService,
            IVehicleTypeService vehicleTypeService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<VehicleListReportService>? logger = null)
        {
            _vehicleService = vehicleService;
            _vehicleTypeService = vehicleTypeService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一車輛資料報表（與批次列印使用相同格式）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int vehicleId)
        {
            var vehicle = await _vehicleService.GetByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw new ArgumentException($"找不到車輛 ID: {vehicleId}");
            }

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildVehicleListDocument(new List<Vehicle> { vehicle }, company, null);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int vehicleId)
        {
            var document = await GenerateReportAsync(vehicleId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int vehicleId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(vehicleId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印車輛資料
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int vehicleId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(vehicleId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印車輛資料 {VehicleId} 時發生錯誤", vehicleId);
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
                var vehicles = await GetVehiclesByCriteriaAsync(criteria);

                if (!vehicles.Any())
                {
                    return ServiceResult.Failure($"無符合條件的車輛\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildVehicleListDocument(vehicles, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印車輛管理表時發生錯誤");
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
                var vehicles = await GetVehiclesByCriteriaAsync(criteria);

                if (!vehicles.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的車輛\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildVehicleListDocument(vehicles, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, vehicles.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染車輛管理表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region VehicleListCriteria 批次報表

        /// <summary>
        /// 以車輛管理表專用篩選條件渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(VehicleListCriteria criteria)
        {
            try
            {
                var vehicles = await GetVehiclesByTypedCriteriaAsync(criteria);

                if (!vehicles.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的車輛\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildVehicleListDocument(vehicles, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, vehicles.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染車輛管理表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>
        /// 根據 BatchPrintCriteria 查詢車輛
        /// </summary>
        private async Task<List<Vehicle>> GetVehiclesByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _vehicleService.GetAllAsync();

            // 篩選車型 ID
            if (criteria.RelatedEntityIds.Any())
            {
                results = results.Where(v => v.VehicleTypeId.HasValue &&
                    criteria.RelatedEntityIds.Contains(v.VehicleTypeId.Value)).ToList();
            }

            // 關鍵字搜尋（車牌號碼、車輛名稱）
            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(v =>
                    v.LicensePlate.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    v.VehicleName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (v.Brand != null && v.Brand.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 排除已停用
            if (!criteria.IncludeCancelled)
            {
                results = results.Where(v => v.Status == ERPCore2.Models.Enums.EntityStatus.Active).ToList();
            }

            return results.OrderBy(v => v.VehicleType?.Name).ThenBy(v => v.LicensePlate).ToList();
        }

        /// <summary>
        /// 根據 VehicleListCriteria 查詢車輛
        /// </summary>
        private async Task<List<Vehicle>> GetVehiclesByTypedCriteriaAsync(VehicleListCriteria criteria)
        {
            var results = await _vehicleService.GetAllAsync();

            // 僅啟用車輛
            if (criteria.ActiveOnly)
            {
                results = results.Where(v => v.Status == ERPCore2.Models.Enums.EntityStatus.Active).ToList();
            }

            // 篩選車型
            if (criteria.VehicleTypeIds.Any())
            {
                results = results.Where(v => v.VehicleTypeId.HasValue &&
                    criteria.VehicleTypeIds.Contains(v.VehicleTypeId.Value)).ToList();
            }

            // 篩選歸屬類型
            if (criteria.OwnershipType.HasValue)
            {
                results = results.Where(v => v.OwnershipType == criteria.OwnershipType.Value).ToList();
            }

            // 關鍵字搜尋
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(v =>
                    v.LicensePlate.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    v.VehicleName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (v.Brand != null && v.Brand.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (v.Code != null && v.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return results.OrderBy(v => v.VehicleType?.Name).ThenBy(v => v.LicensePlate).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構車輛管理表（清單式：依車型分組）
        /// </summary>
        private FormattedDocument BuildVehicleListDocument(
            List<Vehicle> vehicles,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"車輛管理表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("車 輛 管 理 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"車輛數：{vehicles.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依車型分組顯示 ===
            var vehicleTypeGroups = vehicles
                .GroupBy(v => v.VehicleType?.Name ?? "未分類")
                .OrderBy(g => g.Key);

            foreach (var group in vehicleTypeGroups)
            {
                // 車型標題
                doc.AddKeyValueRow(
                    ("車型", $"{group.Key}（{group.Count()} 台）"));

                doc.AddSpacing(3);

                // 車輛資料表格
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.30f, TextAlignment.Center)
                         .AddColumn("車牌號碼", 0.75f, TextAlignment.Left)
                         .AddColumn("車輛名稱", 0.90f, TextAlignment.Left)
                         .AddColumn("廠牌", 0.55f, TextAlignment.Left)
                         .AddColumn("歸屬", 0.40f, TextAlignment.Center)
                         .AddColumn("負責人", 0.55f, TextAlignment.Left)
                         .AddColumn("保險到期", 0.65f, TextAlignment.Center)
                         .AddColumn("驗車到期", 0.65f, TextAlignment.Center)
                         .AddColumn("里程數", 0.60f, TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var vehicle in group.OrderBy(v => v.LicensePlate))
                    {
                        var ownershipText = vehicle.OwnershipType switch
                        {
                            VehicleOwnershipType.Company => "公司",
                            VehicleOwnershipType.Customer => "客戶",
                            _ => ""
                        };

                        table.AddRow(
                            rowNum.ToString(),
                            vehicle.LicensePlate,
                            vehicle.VehicleName,
                            vehicle.Brand ?? "",
                            ownershipText,
                            vehicle.Employee?.Name ?? "",
                            vehicle.InsuranceExpiryDate?.ToString("yyyy/MM/dd") ?? "",
                            vehicle.InspectionExpiryDate?.ToString("yyyy/MM/dd") ?? "",
                            vehicle.Mileage?.ToString("N0") ?? ""
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

                var summaryLines = new List<string>
                {
                    $"車輛總數：{vehicles.Count} 台"
                };

                // 按車型統計
                foreach (var group in vehicleTypeGroups)
                {
                    summaryLines.Add($"  {group.Key}：{group.Count()} 台");
                }

                // 按歸屬類型統計
                var ownershipGroups = vehicles
                    .GroupBy(v => v.OwnershipType)
                    .OrderBy(g => g.Key);

                summaryLines.Add("");
                foreach (var og in ownershipGroups)
                {
                    var ownershipName = og.Key switch
                    {
                        VehicleOwnershipType.Company => "公司",
                        VehicleOwnershipType.Customer => "客戶",
                        _ => "其他"
                    };
                    summaryLines.Add($"  {ownershipName}：{og.Count()} 台");
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


        #endregion
    }
}
