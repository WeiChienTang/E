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
    /// 物料清單報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 依配方分組顯示組件明細（品號、品名、數量、單位、成本）
    /// </summary>
    public class BOMReportService : IBOMReportService
    {
        private readonly IProductCompositionService _compositionService;
        private readonly ICompositionCategoryService _compositionCategoryService;
        private readonly IUnitService _unitService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<BOMReportService>? _logger;

        public BOMReportService(
            IProductCompositionService compositionService,
            ICompositionCategoryService compositionCategoryService,
            IUnitService unitService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<BOMReportService>? logger = null)
        {
            _compositionService = compositionService;
            _compositionCategoryService = compositionCategoryService;
            _unitService = unitService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一物料清單報表（與批次列印使用相同格式）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int compositionId)
        {
            var composition = await _compositionService.GetByIdAsync(compositionId);
            if (composition == null)
            {
                throw new ArgumentException($"找不到物料清單 ID: {compositionId}");
            }

            var company = await _companyService.GetPrimaryCompanyAsync();
            var categoryMap = await GetCategoryMapAsync();
            var unitMap = await GetUnitMapAsync();
            return BuildBOMDocument(new List<ProductComposition> { composition }, company, null, categoryMap, unitMap);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int compositionId)
        {
            var document = await GenerateReportAsync(compositionId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int compositionId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(compositionId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印物料清單
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int compositionId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(compositionId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印物料清單 {CompositionId} 時發生錯誤", compositionId);
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
                var compositions = await GetCompositionsByCriteriaAsync(criteria);

                if (!compositions.Any())
                {
                    return ServiceResult.Failure($"無符合條件的物料清單\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var categoryMap = await GetCategoryMapAsync();
                var unitMap = await GetUnitMapAsync();
                var document = BuildBOMDocument(compositions, company, null, categoryMap, unitMap);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印物料清單報表時發生錯誤");
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
                var compositions = await GetCompositionsByCriteriaAsync(criteria);

                if (!compositions.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的物料清單\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var categoryMap = await GetCategoryMapAsync();
                var unitMap = await GetUnitMapAsync();
                var document = BuildBOMDocument(compositions, company, criteria.PaperSetting, categoryMap, unitMap);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, compositions.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染物料清單報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region BOMReportCriteria 批次報表

        /// <summary>
        /// 以物料清單專用篩選條件渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BOMReportCriteria criteria)
        {
            try
            {
                var compositions = await GetCompositionsByTypedCriteriaAsync(criteria);

                if (!compositions.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的物料清單\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var categoryMap = await GetCategoryMapAsync();
                var unitMap = await GetUnitMapAsync();
                var document = BuildBOMDocument(compositions, company, criteria.PaperSetting, categoryMap, unitMap);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, compositions.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染物料清單報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 輔助

        /// <summary>
        /// 取得物料清單類型 ID → 名稱對照表
        /// </summary>
        private async Task<Dictionary<int, string>> GetCategoryMapAsync()
        {
            var categories = await _compositionCategoryService.GetAllAsync();
            return categories.ToDictionary(c => c.Id, c => c.Name ?? "");
        }

        /// <summary>
        /// 取得單位 ID → 名稱對照表
        /// </summary>
        private async Task<Dictionary<int, string>> GetUnitMapAsync()
        {
            var units = await _unitService.GetAllAsync();
            return units.ToDictionary(u => u.Id, u => u.Name ?? "");
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>
        /// 根據 BatchPrintCriteria 查詢物料清單
        /// </summary>
        private async Task<List<ProductComposition>> GetCompositionsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _compositionService.GetAllAsync();

            // 關鍵字搜尋（配方編號、成品品號、成品品名）
            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(c =>
                    (c.Code != null && c.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.ParentProduct?.Code != null && c.ParentProduct.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.ParentProduct?.Name != null && c.ParentProduct.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 排除已停用
            if (!criteria.IncludeCancelled)
            {
                results = results.Where(c => c.Status == EntityStatus.Active).ToList();
            }

            return results.OrderBy(c => c.ParentProduct?.Code).ThenBy(c => c.Code).ToList();
        }

        /// <summary>
        /// 根據 BOMReportCriteria 查詢物料清單
        /// </summary>
        private async Task<List<ProductComposition>> GetCompositionsByTypedCriteriaAsync(BOMReportCriteria criteria)
        {
            var results = await _compositionService.GetAllAsync();

            // 僅啟用
            if (criteria.ActiveOnly)
            {
                results = results.Where(c => c.Status == EntityStatus.Active).ToList();
            }

            // 篩選物料清單類型
            if (criteria.CompositionCategoryIds.Any())
            {
                results = results.Where(c => c.CompositionCategoryId.HasValue &&
                    criteria.CompositionCategoryIds.Contains(c.CompositionCategoryId.Value)).ToList();
            }

            // 關鍵字搜尋
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(c =>
                    (c.Code != null && c.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.ParentProduct?.Code != null && c.ParentProduct.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.ParentProduct?.Name != null && c.ParentProduct.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return results.OrderBy(c => c.ParentProduct?.Code).ThenBy(c => c.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構物料清單報表（依配方分組顯示組件明細）
        /// </summary>
        private FormattedDocument BuildBOMDocument(
            List<ProductComposition> compositions,
            Company? company,
            PaperSetting? paperSetting,
            Dictionary<int, string> categoryMap,
            Dictionary<int, string> unitMap)
        {
            var totalDetails = compositions.Sum(c => c.CompositionDetails?.Count ?? 0);

            var doc = new FormattedDocument()
                .SetDocumentName($"物料清單報表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("物 料 清 單 報 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"配方數：{compositions.Count} | 材料數：{totalDetails}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依配方分組顯示 ===
            foreach (var composition in compositions)
            {
                var productCode = composition.ParentProduct?.Code ?? "";
                var productName = composition.ParentProduct?.Name ?? "";
                var categoryName = composition.CompositionCategoryId.HasValue &&
                    categoryMap.ContainsKey(composition.CompositionCategoryId.Value)
                    ? categoryMap[composition.CompositionCategoryId.Value]
                    : "";
                var customerName = composition.Customer?.CompanyName ?? "";

                // 配方標題
                doc.AddKeyValueRow(
                    ("配方編號", composition.Code ?? ""),
                    ("成品", $"{productCode} - {productName}"));

                doc.AddKeyValueRow(
                    ("清單類型", !string.IsNullOrEmpty(categoryName) ? categoryName : "（無）"),
                    ("客戶", !string.IsNullOrEmpty(customerName) ? customerName : "（無）"));

                if (!string.IsNullOrEmpty(composition.Specification))
                {
                    doc.AddKeyValueRow(
                        ("規格", composition.Specification));
                }

                doc.AddSpacing(3);

                // 組件明細表格
                var details = composition.CompositionDetails?
                    .OrderBy(d => d.ComponentProduct?.Code)
                    .ToList() ?? new List<ProductCompositionDetail>();

                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.35f, TextAlignment.Center)
                         .AddColumn("品號", 0.90f, TextAlignment.Left)
                         .AddColumn("品名", 1.50f, TextAlignment.Left)
                         .AddColumn("所需數量", 0.65f, TextAlignment.Right)
                         .AddColumn("單位", 0.45f, TextAlignment.Center)
                         .AddColumn("組件成本", 0.70f, TextAlignment.Right)
                         .AddColumn("小計金額", 0.70f, TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var detail in details)
                    {
                        var unitName = detail.UnitId.HasValue && unitMap.ContainsKey(detail.UnitId.Value)
                            ? unitMap[detail.UnitId.Value]
                            : "";

                        var subtotal = detail.ComponentCost.HasValue
                            ? detail.Quantity * detail.ComponentCost.Value
                            : (decimal?)null;

                        table.AddRow(
                            rowNum.ToString(),
                            detail.ComponentProduct?.Code ?? "",
                            detail.ComponentProduct?.Name ?? "",
                            detail.Quantity.ToString("N2"),
                            unitName,
                            detail.ComponentCost?.ToString("N2") ?? "",
                            subtotal?.ToString("N2") ?? ""
                        );
                        rowNum++;
                    }
                });

                // 配方小計
                var detailCount = details.Count;
                var totalCost = details
                    .Where(d => d.ComponentCost.HasValue)
                    .Sum(d => d.Quantity * d.ComponentCost!.Value);

                doc.AddKeyValueRow(
                    ("小計", $"材料：{detailCount}  總成本：{totalCost:N2}"));

                doc.AddSpacing(8);
            }

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var grandTotalCost = compositions.Sum(c =>
                    (c.CompositionDetails ?? new List<ProductCompositionDetail>())
                    .Where(d => d.ComponentCost.HasValue)
                    .Sum(d => d.Quantity * d.ComponentCost!.Value));

                var summaryLines = new List<string>
                {
                    $"配方總數：{compositions.Count}",
                    $"材料總數：{totalDetails}",
                    $"總成本合計：{grandTotalCost:N2}"
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
