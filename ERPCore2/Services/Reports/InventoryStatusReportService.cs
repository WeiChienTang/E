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
    /// 庫存現況表報表服務實作
    /// 依倉庫分組顯示各商品庫存數量、預留、可用及金額
    /// </summary>
    public class InventoryStatusReportService : IInventoryStatusReportService
    {
        private readonly IInventoryStockService _inventoryStockService;
        private readonly IInventoryStockDetailService _inventoryStockDetailService;
        private readonly IWarehouseService _warehouseService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<InventoryStatusReportService>? _logger;

        public InventoryStatusReportService(
            IInventoryStockService inventoryStockService,
            IInventoryStockDetailService inventoryStockDetailService,
            IWarehouseService warehouseService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<InventoryStatusReportService>? logger = null)
        {
            _inventoryStockService = inventoryStockService;
            _inventoryStockDetailService = inventoryStockDetailService;
            _warehouseService = warehouseService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        /// <summary>
        /// 渲染庫存現況表報表為圖片（使用專用篩選條件）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(InventoryStatusCriteria criteria)
        {
            try
            {
                var warehouseGroups = await GetInventoryDataAsync(criteria);

                if (!warehouseGroups.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的庫存資料\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildInventoryStatusDocument(warehouseGroups, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                var totalItems = warehouseGroups.Sum(g => g.Items.Count);
                return BatchPreviewResult.Success(images, document, totalItems);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生庫存現況表報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #region IEntityReportService<InventoryStock> 實作

        /// <summary>
        /// 生成單一庫存項目的格式化報表文件
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<FormattedDocument> GenerateReportAsync(int entityId)
        {
            var criteria = new InventoryStatusCriteria
            {
                IncludeZeroStock = true
            };

            // entityId 對應 InventoryStock.Id，取得其所有明細
            var stock = await _inventoryStockService.GetByIdAsync(entityId);
            if (stock == null)
            {
                throw new ArgumentException($"找不到庫存資料 ID: {entityId}");
            }

            var details = await _inventoryStockDetailService.GetByInventoryStockIdAsync(entityId);
            var warehouseGroups = details
                .GroupBy(d => new { d.WarehouseId, WarehouseName = d.Warehouse?.Name ?? "" })
                .Select(g => new WarehouseGroup
                {
                    WarehouseId = g.Key.WarehouseId,
                    WarehouseName = g.Key.WarehouseName,
                    Items = g.Select(d => new StockItem
                    {
                        ProductCode = stock.Product?.Code ?? "",
                        ProductName = stock.Product?.Name ?? "",
                        LocationName = d.WarehouseLocation?.Name ?? "",
                        CurrentStock = d.CurrentStock,
                        ReservedStock = d.ReservedStock,
                        AvailableStock = d.AvailableStock,
                        AverageCost = d.AverageCost ?? 0,
                        StockValue = d.CurrentStock * (d.AverageCost ?? 0)
                    }).ToList()
                })
                .Where(g => g.Items.Any())
                .OrderBy(g => g.WarehouseName)
                .ToList();

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildInventoryStatusDocument(warehouseGroups, company, criteria);
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
                _logger?.LogError(ex, "直接列印庫存現況表時發生錯誤，庫存ID: {StockId}", entityId);
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
                var inventoryCriteria = new InventoryStatusCriteria();
                var warehouseGroups = await GetInventoryDataAsync(inventoryCriteria);

                if (!warehouseGroups.Any())
                {
                    return ServiceResult.Failure($"無符合條件的庫存資料\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildInventoryStatusDocument(warehouseGroups, company, inventoryCriteria);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印庫存現況表時發生錯誤");
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
                var inventoryCriteria = new InventoryStatusCriteria
                {
                    PaperSetting = criteria.PaperSetting
                };

                return await RenderBatchToImagesAsync(inventoryCriteria);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染庫存現況表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 資料查詢

        /// <summary>
        /// 查詢庫存資料並按倉庫分組
        /// </summary>
        private async Task<List<WarehouseGroup>> GetInventoryDataAsync(InventoryStatusCriteria criteria)
        {
            // 取得所有庫存明細（含 Product、Warehouse、WarehouseLocation 導航屬性）
            var allDetails = await _inventoryStockDetailService.GetAllAsync();

            // 篩選倉庫
            if (criteria.WarehouseIds.Any())
            {
                allDetails = allDetails
                    .Where(d => criteria.WarehouseIds.Contains(d.WarehouseId))
                    .ToList();
            }

            // 篩選商品分類
            if (criteria.CategoryIds.Any())
            {
                allDetails = allDetails
                    .Where(d => d.InventoryStock?.Product?.ProductCategoryId != null &&
                                criteria.CategoryIds.Contains(d.InventoryStock.Product.ProductCategoryId.Value))
                    .ToList();
            }

            // 關鍵字搜尋（品號、品名）
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                allDetails = allDetails
                    .Where(d =>
                        (d.InventoryStock?.Product?.Code != null && d.InventoryStock.Product.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                        (d.InventoryStock?.Product?.Name != null && d.InventoryStock.Product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 是否排除零庫存
            if (!criteria.IncludeZeroStock)
            {
                allDetails = allDetails.Where(d => d.CurrentStock != 0).ToList();
            }

            // 按倉庫分組
            var groups = allDetails
                .GroupBy(d => new { d.WarehouseId, WarehouseName = d.Warehouse?.Name ?? "", WarehouseCode = d.Warehouse?.Code ?? "" })
                .Select(g => new WarehouseGroup
                {
                    WarehouseId = g.Key.WarehouseId,
                    WarehouseName = g.Key.WarehouseName,
                    WarehouseCode = g.Key.WarehouseCode,
                    Items = g.Select(d => new StockItem
                    {
                        ProductCode = d.InventoryStock?.Product?.Code ?? "",
                        ProductName = d.InventoryStock?.Product?.Name ?? "",
                        LocationName = d.WarehouseLocation?.Name ?? "",
                        CurrentStock = d.CurrentStock,
                        ReservedStock = d.ReservedStock,
                        AvailableStock = d.AvailableStock,
                        AverageCost = d.AverageCost ?? 0,
                        StockValue = d.CurrentStock * (d.AverageCost ?? 0)
                    })
                    .OrderBy(i => i.ProductCode)
                    .ThenBy(i => i.LocationName)
                    .ToList()
                })
                .Where(g => g.Items.Any())
                .OrderBy(g => g.WarehouseCode)
                .ThenBy(g => g.WarehouseName)
                .ToList();

            return groups;
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構庫存現況表報表
        /// </summary>
        private FormattedDocument BuildInventoryStatusDocument(
            List<WarehouseGroup> groups,
            Company? company,
            InventoryStatusCriteria criteria)
        {
            var totalItems = groups.Sum(g => g.Items.Count);
            var totalStockValue = groups.Sum(g => g.Items.Sum(i => i.StockValue));

            var doc = new FormattedDocument()
                .SetDocumentName($"庫存現況表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("庫 存 現 況 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"倉庫數：{groups.Count} | 品項數：{totalItems}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依倉庫分組顯示 ===
            foreach (var group in groups)
            {
                // 倉庫標題
                doc.AddKeyValueRow(
                    ("倉庫", $"{group.WarehouseCode} {group.WarehouseName}"));

                doc.AddSpacing(3);

                // 庫存項目表格
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.35f, TextAlignment.Center)
                         .AddColumn("品號", 0.9f, TextAlignment.Left)
                         .AddColumn("品名", 1.3f, TextAlignment.Left)
                         .AddColumn("庫位", 0.6f, TextAlignment.Left)
                         .AddColumn("現有庫存", 0.7f, TextAlignment.Right)
                         .AddColumn("預留庫存", 0.7f, TextAlignment.Right)
                         .AddColumn("可用庫存", 0.7f, TextAlignment.Right)
                         .AddColumn("平均成本", 0.7f, TextAlignment.Right)
                         .AddColumn("庫存金額", 0.8f, TextAlignment.Right)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var item in group.Items)
                    {
                        table.AddRow(
                            rowNum.ToString(),
                            item.ProductCode,
                            item.ProductName,
                            item.LocationName,
                            item.CurrentStock.ToString("N0"),
                            item.ReservedStock.ToString("N0"),
                            item.AvailableStock.ToString("N0"),
                            item.AverageCost.ToString("N2"),
                            item.StockValue.ToString("N0")
                        );
                        rowNum++;
                    }
                });

                // 倉庫小計
                var groupCurrentStock = group.Items.Sum(i => i.CurrentStock);
                var groupReservedStock = group.Items.Sum(i => i.ReservedStock);
                var groupAvailableStock = group.Items.Sum(i => i.AvailableStock);
                var groupStockValue = group.Items.Sum(i => i.StockValue);

                doc.AddKeyValueRow(
                    ("小計", $"品項：{group.Items.Count}  現有：{groupCurrentStock:N0}  預留：{groupReservedStock:N0}  可用：{groupAvailableStock:N0}  金額：{groupStockValue:N0}"));

                doc.AddSpacing(8);
            }

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var totalCurrentStock = groups.Sum(g => g.Items.Sum(i => i.CurrentStock));
                var totalReservedStock = groups.Sum(g => g.Items.Sum(i => i.ReservedStock));
                var totalAvailableStock = groups.Sum(g => g.Items.Sum(i => i.AvailableStock));

                var summaryLines = new List<string>
                {
                    $"倉庫數：{groups.Count}",
                    $"品項總數：{totalItems}",
                    $"現有庫存合計：{totalCurrentStock:N0}",
                    $"預留庫存合計：{totalReservedStock:N0}",
                    $"可用庫存合計：{totalAvailableStock:N0}",
                    $"庫存金額合計：{totalStockValue:N0}"
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
        /// 倉庫分組資料
        /// </summary>
        private class WarehouseGroup
        {
            public int WarehouseId { get; set; }
            public string WarehouseName { get; set; } = "";
            public string WarehouseCode { get; set; } = "";
            public List<StockItem> Items { get; set; } = new();
        }

        /// <summary>
        /// 庫存項目資料
        /// </summary>
        private class StockItem
        {
            public string ProductCode { get; set; } = "";
            public string ProductName { get; set; } = "";
            public string LocationName { get; set; } = "";
            public decimal CurrentStock { get; set; }
            public decimal ReservedStock { get; set; }
            public decimal AvailableStock { get; set; }
            public decimal AverageCost { get; set; }
            public decimal StockValue { get; set; }
        }

        #endregion
    }
}
