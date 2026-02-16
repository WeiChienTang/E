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
    /// 庫存盤點差異表報表服務實作
    /// 依盤點單分組顯示各商品的系統庫存、實盤數量及差異
    /// </summary>
    public class StockTakingDifferenceReportService : IStockTakingDifferenceReportService
    {
        private readonly IStockTakingService _stockTakingService;
        private readonly IWarehouseService _warehouseService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<StockTakingDifferenceReportService>? _logger;

        public StockTakingDifferenceReportService(
            IStockTakingService stockTakingService,
            IWarehouseService warehouseService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<StockTakingDifferenceReportService>? logger = null)
        {
            _stockTakingService = stockTakingService;
            _warehouseService = warehouseService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        /// <summary>
        /// 渲染庫存盤點差異表為圖片（使用專用篩選條件）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(StockTakingDifferenceCriteria criteria)
        {
            try
            {
                var stockTakingGroups = await GetStockTakingDataAsync(criteria);

                if (!stockTakingGroups.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的盤點資料\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildDifferenceDocument(stockTakingGroups, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                var totalItems = stockTakingGroups.Sum(g => g.Items.Count);
                return BatchPreviewResult.Success(images, document, totalItems);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生庫存盤點差異表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #region IEntityReportService<StockTaking> 實作

        /// <summary>
        /// 生成單一盤點單的格式化報表文件
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<FormattedDocument> GenerateReportAsync(int entityId)
        {
            var stockTaking = await _stockTakingService.GetWithDetailsAsync(entityId);
            if (stockTaking == null)
            {
                throw new ArgumentException($"找不到盤點單 ID: {entityId}");
            }

            var group = MapToStockTakingGroup(stockTaking, onlyDifference: false);
            var groups = new List<StockTakingGroup> { group };

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildDifferenceDocument(groups, company, new StockTakingDifferenceCriteria());
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int entityId)
        {
            var document = await GenerateReportAsync(entityId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張設定）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int entityId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(entityId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int entityId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(entityId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "直接列印盤點差異表時發生錯誤，盤點ID: {StockTakingId}", entityId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次直接列印（BatchPrintCriteria）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var differenceCriteria = new StockTakingDifferenceCriteria
                {
                    StartDate = criteria.StartDate,
                    EndDate = criteria.EndDate
                };
                var groups = await GetStockTakingDataAsync(differenceCriteria);

                if (!groups.Any())
                {
                    return ServiceResult.Failure($"無符合條件的盤點資料\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildDifferenceDocument(groups, company, differenceCriteria);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印盤點差異表時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染報表為圖片（BatchPrintCriteria）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            try
            {
                var differenceCriteria = new StockTakingDifferenceCriteria
                {
                    StartDate = criteria.StartDate,
                    EndDate = criteria.EndDate,
                    PaperSetting = criteria.PaperSetting
                };

                return await RenderBatchToImagesAsync(differenceCriteria);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染盤點差異表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 資料查詢

        /// <summary>
        /// 查詢盤點資料並按盤點單分組
        /// </summary>
        private async Task<List<StockTakingGroup>> GetStockTakingDataAsync(StockTakingDifferenceCriteria criteria)
        {
            // 取得日期範圍內的盤點單
            var startDate = criteria.StartDate ?? DateTime.MinValue;
            var endDate = criteria.EndDate ?? DateTime.MaxValue;
            var stockTakings = await _stockTakingService.GetByDateRangeAsync(startDate, endDate);

            // 篩選倉庫
            if (criteria.WarehouseIds.Any())
            {
                stockTakings = stockTakings
                    .Where(st => criteria.WarehouseIds.Contains(st.WarehouseId))
                    .ToList();
            }

            // 關鍵字搜尋（盤點單號）
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                stockTakings = stockTakings
                    .Where(st =>
                        (st.TakingNumber != null && st.TakingNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 逐筆載入明細並轉換為分組模型
            var groups = new List<StockTakingGroup>();
            foreach (var st in stockTakings.OrderByDescending(s => s.TakingDate).ThenBy(s => s.TakingNumber))
            {
                var fullStockTaking = await _stockTakingService.GetWithDetailsAsync(st.Id);
                if (fullStockTaking == null) continue;

                var group = MapToStockTakingGroup(fullStockTaking, criteria.OnlyDifferenceItems);

                // 關鍵字搜尋（品號、品名）- 在明細層級過濾
                if (!string.IsNullOrWhiteSpace(criteria.Keyword))
                {
                    var keyword = criteria.Keyword;
                    group.Items = group.Items
                        .Where(i =>
                            i.ProductCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            i.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            (fullStockTaking.TakingNumber != null && fullStockTaking.TakingNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                if (group.Items.Any())
                {
                    groups.Add(group);
                }
            }

            return groups;
        }

        /// <summary>
        /// 將 StockTaking 實體轉換為報表分組模型
        /// </summary>
        private StockTakingGroup MapToStockTakingGroup(StockTaking stockTaking, bool onlyDifference)
        {
            var details = stockTaking.StockTakingDetails?.ToList() ?? new List<StockTakingDetail>();

            if (onlyDifference)
            {
                details = details.Where(d => d.HasDifference).ToList();
            }

            return new StockTakingGroup
            {
                StockTakingId = stockTaking.Id,
                TakingNumber = stockTaking.TakingNumber ?? "",
                TakingDate = stockTaking.TakingDate,
                WarehouseName = stockTaking.Warehouse?.Name ?? "",
                WarehouseLocationName = stockTaking.WarehouseLocation?.Name,
                TakingPersonnel = stockTaking.TakingPersonnel ?? "",
                SupervisingPersonnel = stockTaking.SupervisingPersonnel ?? "",
                Items = details
                    .OrderBy(d => d.Product?.Code)
                    .Select(d => new DifferenceItem
                    {
                        ProductCode = d.Product?.Code ?? "",
                        ProductName = d.Product?.Name ?? "",
                        LocationName = d.WarehouseLocation?.Name ?? "",
                        SystemStock = d.SystemStock,
                        ActualStock = d.ActualStock,
                        DifferenceQuantity = d.DifferenceQuantity ?? 0,
                        UnitCost = d.UnitCost ?? 0,
                        DifferenceAmount = d.DifferenceAmount ?? 0,
                        IsAdjusted = d.IsAdjusted
                    })
                    .ToList()
            };
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構庫存盤點差異表報表
        /// </summary>
        private FormattedDocument BuildDifferenceDocument(
            List<StockTakingGroup> groups,
            Company? company,
            StockTakingDifferenceCriteria criteria)
        {
            var totalItems = groups.Sum(g => g.Items.Count);
            var totalDifferenceAmount = groups.Sum(g => g.Items.Sum(i => i.DifferenceAmount));
            var differenceItemCount = groups.Sum(g => g.Items.Count(i => i.DifferenceQuantity != 0));

            var doc = new FormattedDocument()
                .SetDocumentName($"庫存盤點差異表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                var dateRangeText = "";
                if (criteria.StartDate.HasValue && criteria.EndDate.HasValue)
                    dateRangeText = $"期間：{criteria.StartDate:yyyy/MM/dd} ～ {criteria.EndDate:yyyy/MM/dd}";
                else if (criteria.StartDate.HasValue)
                    dateRangeText = $"起始：{criteria.StartDate:yyyy/MM/dd} 起";
                else if (criteria.EndDate.HasValue)
                    dateRangeText = $"截至：{criteria.EndDate:yyyy/MM/dd}";

                var rightLines = new List<string>
                {
                    $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                    $"盤點單數：{groups.Count} | 品項數：{totalItems}",
                    $"頁次：{{PAGE}}/{{PAGES}}"
                };

                if (!string.IsNullOrEmpty(dateRangeText))
                {
                    rightLines.Insert(1, dateRangeText);
                }

                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("庫 存 盤 點 差 異 表", 16f, true)
                    },
                    rightLines: rightLines,
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依盤點單分組顯示 ===
            foreach (var group in groups)
            {
                // 盤點單標題
                var locationInfo = string.IsNullOrEmpty(group.WarehouseLocationName)
                    ? ""
                    : $" / 庫位：{group.WarehouseLocationName}";
                var personnelInfo = string.IsNullOrEmpty(group.SupervisingPersonnel)
                    ? $"盤點員：{group.TakingPersonnel}"
                    : $"盤點員：{group.TakingPersonnel} / 監盤員：{group.SupervisingPersonnel}";

                doc.AddKeyValueRow(
                    ("盤點單號", $"{group.TakingNumber}  日期：{group.TakingDate:yyyy/MM/dd}  倉庫：{group.WarehouseName}{locationInfo}"));

                doc.AddKeyValueRow(
                    ("人員", personnelInfo));

                doc.AddSpacing(3);

                // 盤點明細表格
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.30f, TextAlignment.Center)
                         .AddColumn("品號", 0.85f, TextAlignment.Left)
                         .AddColumn("品名", 1.20f, TextAlignment.Left)
                         .AddColumn("庫位", 0.50f, TextAlignment.Left)
                         .AddColumn("系統庫存", 0.65f, TextAlignment.Right)
                         .AddColumn("實盤數量", 0.65f, TextAlignment.Right)
                         .AddColumn("差異數量", 0.65f, TextAlignment.Right)
                         .AddColumn("單位成本", 0.65f, TextAlignment.Right)
                         .AddColumn("差異金額", 0.75f, TextAlignment.Right)
                         .AddColumn("已調整", 0.45f, TextAlignment.Center)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var item in group.Items)
                    {
                        var actualStockText = item.ActualStock.HasValue
                            ? item.ActualStock.Value.ToString("N0")
                            : "未盤";

                        var diffQtyText = item.DifferenceQuantity != 0
                            ? (item.DifferenceQuantity > 0 ? $"+{item.DifferenceQuantity:N0}" : item.DifferenceQuantity.ToString("N0"))
                            : "0";

                        var diffAmtText = item.DifferenceAmount != 0
                            ? (item.DifferenceAmount > 0 ? $"+{item.DifferenceAmount:N0}" : item.DifferenceAmount.ToString("N0"))
                            : "0";

                        table.AddRow(
                            rowNum.ToString(),
                            item.ProductCode,
                            item.ProductName,
                            item.LocationName,
                            item.SystemStock.ToString("N0"),
                            actualStockText,
                            diffQtyText,
                            item.UnitCost.ToString("N2"),
                            diffAmtText,
                            item.IsAdjusted ? "V" : ""
                        );
                        rowNum++;
                    }
                });

                // 盤點單小計
                var groupDiffItems = group.Items.Count(i => i.DifferenceQuantity != 0);
                var groupDiffAmount = group.Items.Sum(i => i.DifferenceAmount);
                var groupAdjustedCount = group.Items.Count(i => i.IsAdjusted);
                var diffAmountDisplay = groupDiffAmount >= 0 ? $"+{groupDiffAmount:N0}" : groupDiffAmount.ToString("N0");

                doc.AddKeyValueRow(
                    ("小計", $"品項：{group.Items.Count}  差異項：{groupDiffItems}  差異金額：{diffAmountDisplay}  已調整：{groupAdjustedCount}"));

                doc.AddSpacing(8);
            }

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var totalDiffDisplay = totalDifferenceAmount >= 0
                    ? $"+{totalDifferenceAmount:N0}"
                    : totalDifferenceAmount.ToString("N0");
                var totalAdjusted = groups.Sum(g => g.Items.Count(i => i.IsAdjusted));

                var summaryLines = new List<string>
                {
                    $"盤點單數：{groups.Count}",
                    $"盤點品項總數：{totalItems}",
                    $"差異品項數：{differenceItemCount}",
                    $"差異金額合計：{totalDiffDisplay}",
                    $"已調整項數：{totalAdjusted}"
                };

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.6f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "倉庫主管" });
            });

            return doc;
        }

        #endregion

        #region 內部資料模型

        /// <summary>
        /// 盤點單分組資料
        /// </summary>
        private class StockTakingGroup
        {
            public int StockTakingId { get; set; }
            public string TakingNumber { get; set; } = "";
            public DateTime TakingDate { get; set; }
            public string WarehouseName { get; set; } = "";
            public string? WarehouseLocationName { get; set; }
            public string TakingPersonnel { get; set; } = "";
            public string SupervisingPersonnel { get; set; } = "";
            public List<DifferenceItem> Items { get; set; } = new();
        }

        /// <summary>
        /// 盤點差異項目資料
        /// </summary>
        private class DifferenceItem
        {
            public string ProductCode { get; set; } = "";
            public string ProductName { get; set; } = "";
            public string LocationName { get; set; } = "";
            public decimal SystemStock { get; set; }
            public decimal? ActualStock { get; set; }
            public decimal DifferenceQuantity { get; set; }
            public decimal UnitCost { get; set; }
            public decimal DifferenceAmount { get; set; }
            public bool IsAdjusted { get; set; }
        }

        #endregion
    }
}
