using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 用料損耗退料記錄報表服務
    /// 查詢有損耗量或退料量的生產排程明細，依生產項目分組呈現
    /// </summary>
    public class MaterialScrapReportService : IMaterialScrapReportService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<MaterialScrapReportService>? _logger;

        public MaterialScrapReportService(
            IDbContextFactory<AppDbContext> contextFactory,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<MaterialScrapReportService>? logger = null)
        {
            _contextFactory = contextFactory;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(MaterialScrapCriteria criteria)
        {
            try
            {
                var groups = await GetScrapDataAsync(criteria);

                if (!groups.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的損耗退料記錄\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildDocument(groups, company, criteria);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                var totalRows = groups.Sum(g => g.Details.Count);
                return BatchPreviewResult.Success(images, document, totalRows, new List<FormattedDocument> { document });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生用料損耗退料記錄報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            var scrapCriteria = new MaterialScrapCriteria
            {
                StartDate = criteria.StartDate,
                EndDate = criteria.EndDate,
                PaperSetting = criteria.PaperSetting
            };
            return await RenderBatchToImagesAsync(scrapCriteria);
        }

        #region 資料查詢

        private async Task<List<ScrapGroup>> GetScrapDataAsync(MaterialScrapCriteria criteria)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var startDate = criteria.StartDate ?? DateTime.Today.AddMonths(-1);
                var endDate = criteria.EndDate?.Date.AddDays(1) ?? DateTime.Today.AddDays(1);

                // 基礎查詢：有損耗或退料的明細
                var query = context.ProductionScheduleDetails
                    .Include(psd => psd.ProductionScheduleItem)
                        .ThenInclude(psi => psi.Item)
                    .Include(psd => psd.ComponentItem)
                    .Include(psd => psd.ReturnWarehouse)
                    .Where(psd => psd.ScrapQty > 0 || psd.ReturnQty > 0)
                    .Where(psd => (psd.UpdatedAt ?? psd.CreatedAt) >= startDate
                               && (psd.UpdatedAt ?? psd.CreatedAt) < endDate)
                    .Where(psd => !psd.IsDraft)
                    .AsQueryable();

                // 成品篩選
                if (criteria.ItemIds.Any())
                {
                    query = query.Where(psd =>
                        criteria.ItemIds.Contains(psd.ProductionScheduleItem.ItemId));
                }

                // 組件篩選
                if (criteria.ComponentItemIds.Any())
                {
                    query = query.Where(psd => criteria.ComponentItemIds.Contains(psd.ComponentItemId));
                }

                // 只顯示有損耗
                if (criteria.OnlyWithScrap)
                {
                    query = query.Where(psd => psd.ScrapQty > 0);
                }

                // 只顯示有退料
                if (criteria.OnlyWithReturn)
                {
                    query = query.Where(psd => psd.ReturnQty > 0);
                }

                var details = await query
                    .OrderBy(psd => psd.ProductionScheduleItemId)
                    .ThenBy(psd => psd.ComponentItemId)
                    .ToListAsync();

                // 依生產項目分組
                return details
                    .GroupBy(psd => psd.ProductionScheduleItemId)
                    .Select(g =>
                    {
                        var first = g.First();
                        var item = first.ProductionScheduleItem;
                        return new ScrapGroup
                        {
                            ItemId = item.Id,
                            ItemCode = item.Code ?? "",
                            FinishedItemName = item.Item?.Name ?? "（未知成品）",
                            FinishedItemCode = item.Item?.Code ?? "",
                            ScheduledQuantity = item.ScheduledQuantity,
                            CompletedQuantity = item.CompletedQuantity,
                            SettlementDate = g.Max(d => d.UpdatedAt ?? d.CreatedAt),
                            Details = g.ToList()
                        };
                    })
                    .OrderByDescending(g => g.SettlementDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "查詢損耗退料資料時發生錯誤");
                return new List<ScrapGroup>();
            }
        }

        #endregion

        #region 建構報表文件

        [SupportedOSPlatform("windows6.1")]
        private FormattedDocument BuildDocument(
            List<ScrapGroup> groups,
            Company? company,
            MaterialScrapCriteria criteria)
        {
            var startDate = criteria.StartDate ?? DateTime.Today.AddMonths(-1);
            var endDate = criteria.EndDate ?? DateTime.Today;
            var dateRangeText = $"{startDate:yyyy/MM/dd} ~ {endDate:yyyy/MM/dd}";

            var totalRows = groups.Sum(g => g.Details.Count);
            var totalScrapQty = groups.Sum(g => g.Details.Sum(d => d.ScrapQty));
            var totalReturnQty = groups.Sum(g => g.Details.Sum(d => d.ReturnQty));

            var doc = new FormattedDocument()
                .SetDocumentName($"用料損耗退料記錄-{startDate:yyyyMMdd}-{endDate:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("用 料 損 耗 退 料 記 錄", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"查詢期間：{dateRangeText}",
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"生產項目：{groups.Count} 筆 | 明細：{totalRows} 筆",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依生產項目分組顯示 ===
            foreach (var group in groups)
            {
                doc.AddKeyValueRow(
                    ("成品", $"{group.FinishedItemCode}  {group.FinishedItemName}"),
                    ("結算日期", group.SettlementDate.ToString("yyyy/MM/dd")));

                doc.AddKeyValueRow(
                    ("排程數量", group.ScheduledQuantity.ToString("N0")),
                    ("完成數量", group.CompletedQuantity.ToString("N0")));

                doc.AddSpacing(3);

                doc.AddTable(table =>
                {
                    table.AddColumn("組件品號", 0.8f, TextAlignment.Left)
                         .AddColumn("組件品名", 1.4f, TextAlignment.Left)
                         .AddColumn("需求量", 0.6f, TextAlignment.Right)
                         .AddColumn("已領量", 0.6f, TextAlignment.Right)
                         .AddColumn("實際消耗", 0.65f, TextAlignment.Right)
                         .AddColumn("損耗量", 0.6f, TextAlignment.Right)
                         .AddColumn("退料量", 0.6f, TextAlignment.Right)
                         .AddColumn("退料倉庫", 0.8f, TextAlignment.Left)
                         .AddColumn("損耗備註", 1.2f, TextAlignment.Left)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    foreach (var detail in group.Details)
                    {
                        table.AddRow(
                            detail.ComponentItem?.Code ?? "",
                            detail.ComponentItem?.Name ?? "",
                            detail.RequiredQuantity > 0 ? detail.RequiredQuantity.ToString("N4").TrimEnd('0').TrimEnd('.') : "",
                            detail.IssuedQuantity > 0 ? detail.IssuedQuantity.ToString("N4").TrimEnd('0').TrimEnd('.') : "",
                            detail.ActualUsedQty > 0 ? detail.ActualUsedQty.ToString("N4").TrimEnd('0').TrimEnd('.') : "",
                            detail.ScrapQty > 0 ? detail.ScrapQty.ToString("N4").TrimEnd('0').TrimEnd('.') : "",
                            detail.ReturnQty > 0 ? detail.ReturnQty.ToString("N4").TrimEnd('0').TrimEnd('.') : "",
                            detail.ReturnWarehouse?.Name ?? "",
                            detail.ScrapReason ?? ""
                        );
                    }

                    // 小計行
                    var groupScrap = group.Details.Sum(d => d.ScrapQty);
                    var groupReturn = group.Details.Sum(d => d.ReturnQty);
                    table.AddRow(
                        "小計", "",
                        "",
                        "",
                        "",
                        groupScrap > 0 ? groupScrap.ToString("N4").TrimEnd('0').TrimEnd('.') : "0",
                        groupReturn > 0 ? groupReturn.ToString("N4").TrimEnd('0').TrimEnd('.') : "0",
                        "", ""
                    );
                });

                doc.AddSpacing(8);
            }

            // === 頁尾 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);
                footer.AddLine();
                footer.AddKeyValueRow(
                    ("生產項目數", groups.Count.ToString()),
                    ("明細總筆數", totalRows.ToString()));
                footer.AddKeyValueRow(
                    ("損耗量合計", totalScrapQty > 0 ? totalScrapQty.ToString("N4").TrimEnd('0').TrimEnd('.') : "0"),
                    ("退料量合計", totalReturnQty > 0 ? totalReturnQty.ToString("N4").TrimEnd('0').TrimEnd('.') : "0"));
            });

            return doc;
        }

        #endregion

        #region 私有資料類別

        private class ScrapGroup
        {
            public int ItemId { get; set; }
            public string ItemCode { get; set; } = "";
            public string FinishedItemName { get; set; } = "";
            public string FinishedItemCode { get; set; } = "";
            public decimal ScheduledQuantity { get; set; }
            public decimal CompletedQuantity { get; set; }
            public DateTime SettlementDate { get; set; }
            public List<ProductionScheduleDetail> Details { get; set; } = new();
        }

        #endregion
    }
}
