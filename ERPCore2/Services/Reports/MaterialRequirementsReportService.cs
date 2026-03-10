using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 用料需求報表服務
    /// 依排程日期範圍彙總所有生產項目的物料需求，以組件品號為單位呈現
    /// </summary>
    public class MaterialRequirementsReportService : IMaterialRequirementsReportService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<MaterialRequirementsReportService>? _logger;

        public MaterialRequirementsReportService(
            IDbContextFactory<AppDbContext> contextFactory,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<MaterialRequirementsReportService>? logger = null)
        {
            _contextFactory = contextFactory;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(MaterialRequirementsCriteria criteria)
        {
            try
            {
                var rows = await GetRequirementsDataAsync(criteria);

                if (!rows.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的用料需求記錄\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildDocument(rows, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, rows.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生用料需求報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            var reqCriteria = new MaterialRequirementsCriteria
            {
                StartDate = criteria.StartDate,
                EndDate = criteria.EndDate,
                PaperSetting = criteria.PaperSetting
            };
            return await RenderBatchToImagesAsync(reqCriteria);
        }

        #region 資料查詢

        private async Task<List<MaterialRequirementRow>> GetRequirementsDataAsync(MaterialRequirementsCriteria criteria)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var startDate = criteria.StartDate ?? DateTime.Today.AddMonths(-1);
                var endDate = criteria.EndDate?.Date.AddDays(1) ?? DateTime.Today.AddDays(1);

                // 查詢排程明細：依排程項目的 PlannedStartDate 篩選
                var detailsQuery = context.ProductionScheduleDetails
                    .Include(d => d.ProductionScheduleItem)
                        .ThenInclude(i => i.Product)
                    .Include(d => d.ComponentProduct)
                    .Where(d => d.ProductionScheduleItem.PlannedStartDate.HasValue
                             && d.ProductionScheduleItem.PlannedStartDate >= startDate
                             && d.ProductionScheduleItem.PlannedStartDate < endDate)
                    .AsQueryable();

                // 成品篩選
                if (criteria.ProductIds.Any())
                {
                    detailsQuery = detailsQuery.Where(d =>
                        criteria.ProductIds.Contains(d.ProductionScheduleItem.ProductId));
                }

                // 排除已完成
                if (criteria.ExcludeCompleted)
                {
                    detailsQuery = detailsQuery.Where(d =>
                        d.ProductionScheduleItem.ProductionItemStatus != ProductionItemStatus.Completed);
                }

                var details = await detailsQuery
                    .OrderBy(d => d.ComponentProductId)
                    .ToListAsync();

                if (!details.Any())
                    return new List<MaterialRequirementRow>();

                // 取得所有相關組件的庫存資料
                var componentIds = details.Select(d => d.ComponentProductId).Distinct().ToList();
                var stocks = await context.InventoryStocks
                    .Include(s => s.InventoryStockDetails)
                    .Where(s => s.ProductId.HasValue && componentIds.Contains(s.ProductId.Value))
                    .ToListAsync();

                var stockByProductId = stocks
                    .Where(s => s.ProductId.HasValue)
                    .ToDictionary(s => s.ProductId!.Value, s => s.TotalCurrentStock);

                // 依組件品號彙總
                var rows = details
                    .GroupBy(d => d.ComponentProductId)
                    .Select(g =>
                    {
                        var component = g.First().ComponentProduct;
                        var totalRequired = g.Sum(d => d.RequiredQuantity);
                        var totalIssued = g.Sum(d => d.IssuedQuantity);
                        var pending = Math.Max(0, totalRequired - totalIssued);

                        stockByProductId.TryGetValue(g.Key, out var currentStock);

                        return new MaterialRequirementRow
                        {
                            ComponentProductId = g.Key,
                            ComponentCode = component?.Code ?? "",
                            ComponentName = component?.Name ?? "（未知組件）",
                            ScheduleItemCount = g.Select(d => d.ProductionScheduleItemId).Distinct().Count(),
                            TotalRequiredQty = totalRequired,
                            TotalIssuedQty = totalIssued,
                            PendingIssueQty = pending,
                            CurrentStock = currentStock
                        };
                    })
                    .Where(r => !criteria.OnlyPendingIssue || r.PendingIssueQty > 0)
                    .OrderBy(r => r.ComponentCode)
                    .ToList();

                return rows;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "查詢用料需求資料時發生錯誤");
                return new List<MaterialRequirementRow>();
            }
        }

        #endregion

        #region 建構報表文件

        [SupportedOSPlatform("windows6.1")]
        private FormattedDocument BuildDocument(
            List<MaterialRequirementRow> rows,
            Company? company,
            MaterialRequirementsCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddMonths(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;
            var dateRangeText = $"{startDate:yyyy/MM/dd} ~ {endDate:yyyy/MM/dd}";

            var totalRequired = rows.Sum(r => r.TotalRequiredQty);
            var totalIssued = rows.Sum(r => r.TotalIssuedQty);
            var totalPending = rows.Sum(r => r.PendingIssueQty);

            var doc = new FormattedDocument()
                .SetDocumentName($"用料需求報表-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("用 料 需 求 報 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"查詢期間：{dateRangeText}",
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"組件種類：{rows.Count} 項",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 明細表格 ===
            doc.AddTable(table =>
            {
                table.AddColumn("組件品號", 1.0f, TextAlignment.Left)
                     .AddColumn("組件品名", 1.8f, TextAlignment.Left)
                     .AddColumn("相關排程", 0.7f, TextAlignment.Right)
                     .AddColumn("需求量合計", 0.85f, TextAlignment.Right)
                     .AddColumn("已領量合計", 0.85f, TextAlignment.Right)
                     .AddColumn("待領量", 0.75f, TextAlignment.Right)
                     .AddColumn("現有庫存", 0.8f, TextAlignment.Right)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(true)
                     .SetRowHeight(20);

                foreach (var row in rows)
                {
                    var pendingDisplay = row.PendingIssueQty > 0
                        ? row.PendingIssueQty.ToString("N4").TrimEnd('0').TrimEnd('.')
                        : "";

                    // 庫存不足時標記
                    var stockDisplay = row.CurrentStock > 0
                        ? row.CurrentStock.ToString("N4").TrimEnd('0').TrimEnd('.')
                        : "0";

                    table.AddRow(
                        row.ComponentCode,
                        row.ComponentName,
                        row.ScheduleItemCount.ToString(),
                        row.TotalRequiredQty.ToString("N4").TrimEnd('0').TrimEnd('.'),
                        row.TotalIssuedQty > 0 ? row.TotalIssuedQty.ToString("N4").TrimEnd('0').TrimEnd('.') : "",
                        pendingDisplay,
                        stockDisplay
                    );
                }

                // 合計行
                table.AddRow(
                    "合計", "",
                    "",
                    totalRequired.ToString("N4").TrimEnd('0').TrimEnd('.'),
                    totalIssued > 0 ? totalIssued.ToString("N4").TrimEnd('0').TrimEnd('.') : "0",
                    totalPending > 0 ? totalPending.ToString("N4").TrimEnd('0').TrimEnd('.') : "0",
                    ""
                );
            });

            // === 頁尾 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);
                footer.AddLine();
                footer.AddKeyValueRow(
                    ("組件種類數", rows.Count.ToString()),
                    ("需求量合計", totalRequired.ToString("N4").TrimEnd('0').TrimEnd('.')));
                footer.AddKeyValueRow(
                    ("已領量合計", totalIssued > 0 ? totalIssued.ToString("N4").TrimEnd('0').TrimEnd('.') : "0"),
                    ("待領量合計", totalPending > 0 ? totalPending.ToString("N4").TrimEnd('0').TrimEnd('.') : "0"));
            });

            return doc;
        }

        #endregion

        #region 私有資料類別

        private class MaterialRequirementRow
        {
            public int ComponentProductId { get; set; }
            public string ComponentCode { get; set; } = "";
            public string ComponentName { get; set; } = "";
            public int ScheduleItemCount { get; set; }
            public decimal TotalRequiredQty { get; set; }
            public decimal TotalIssuedQty { get; set; }
            public decimal PendingIssueQty { get; set; }
            public decimal CurrentStock { get; set; }
        }

        #endregion
    }
}
