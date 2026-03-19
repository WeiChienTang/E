using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
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
    /// 製令單報表服務實作
    /// 每張製令單（ProductionScheduleItem）獨立一份文件，含用料明細與完工記錄
    /// </summary>
    public class ManufacturingOrderReportService : IManufacturingOrderReportService
    {
        private readonly IProductionScheduleItemService _itemService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<ManufacturingOrderReportService>? _logger;

        public ManufacturingOrderReportService(
            IProductionScheduleItemService itemService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<ManufacturingOrderReportService>? logger = null)
        {
            _itemService = itemService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService<ProductionScheduleItem> 實作

        [SupportedOSPlatform("windows6.1")]
        public async Task<FormattedDocument> GenerateReportAsync(int entityId)
        {
            var item = await _itemService.GetWithDetailsAsync(entityId);
            if (item == null)
                throw new ArgumentException($"找不到製令單 ID: {entityId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildManufacturingOrderDocument(item, company);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int entityId)
        {
            var document = await GenerateReportAsync(entityId);
            return _formattedPrintService.RenderToImages(document);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int entityId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(entityId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

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
                _logger?.LogError(ex, "直接列印製令單時發生錯誤，製令單ID: {ItemId}", entityId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var items = await GetFilteredItemsAsync(new ManufacturingOrderCriteria
                {
                    StartDate = criteria.StartDate,
                    EndDate = criteria.EndDate
                });

                foreach (var item in items)
                {
                    var result = await DirectPrintAsync(item.Id, reportId, 1);
                    if (!result.IsSuccess)
                        _logger?.LogWarning("批次列印製令單 {ItemId} 失敗：{Error}", item.Id, result.ErrorMessage);
                }

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印製令單時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            var mosCriteria = new ManufacturingOrderCriteria
            {
                StartDate = criteria.StartDate,
                EndDate = criteria.EndDate,
                PaperSetting = criteria.PaperSetting
            };
            return await RenderBatchToImagesAsync(mosCriteria);
        }

        #endregion

        #region 製令單專用批次報表

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(ManufacturingOrderCriteria criteria)
        {
            try
            {
                var items = await GetFilteredItemsAsync(criteria);
                if (!items.Any())
                    return BatchPreviewResult.Failure($"無符合條件的製令單資料\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();

                return await BatchReportHelper.RenderBatchToImagesAsync(
                    items,
                    async (id, _) =>
                    {
                        var detail = await _itemService.GetWithDetailsAsync(id);
                        return detail != null ? BuildManufacturingOrderDocument(detail, company) : new FormattedDocument();
                    },
                    _formattedPrintService,
                    "製令單",
                    criteria.PaperSetting,
                    criteria.GetSummary(),
                    _logger);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "產生製令單批次報表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法

        private async Task<List<ProductionScheduleItem>> GetFilteredItemsAsync(ManufacturingOrderCriteria criteria)
        {
            var allItems = await _itemService.GetAllAsync();
            var query = allItems.AsQueryable();

            if (criteria.ItemIds.Any())
                return query.Where(i => criteria.ItemIds.Contains(i.Id)).ToList();

            if (criteria.StartDate.HasValue)
                query = query.Where(i => i.PlannedStartDate == null || i.PlannedStartDate >= criteria.StartDate);

            if (criteria.EndDate.HasValue)
                query = query.Where(i => i.PlannedStartDate == null || i.PlannedStartDate <= criteria.EndDate);

            if (criteria.ProductIds.Any())
                query = query.Where(i => criteria.ProductIds.Contains(i.ProductId));

            if (criteria.ResponsibleEmployeeIds.Any())
                query = query.Where(i => i.ResponsibleEmployeeId.HasValue && criteria.ResponsibleEmployeeIds.Contains(i.ResponsibleEmployeeId.Value));

            if (criteria.StatusFilters.Any())
                query = query.Where(i => criteria.StatusFilters.Contains(i.ProductionItemStatus));

            if (!criteria.IncludeClosed)
                query = query.Where(i => !i.IsClosed);

            return query.ToList();
        }

        private FormattedDocument BuildManufacturingOrderDocument(ProductionScheduleItem item, Company? company)
        {
            var statusText = item.ProductionItemStatus switch
            {
                ProductionItemStatus.Pending => "待生產",
                ProductionItemStatus.WaitingMaterial => "等待領料",
                ProductionItemStatus.InProgress => "生產中",
                ProductionItemStatus.Paused => "已暫停",
                ProductionItemStatus.Completed => "已完成",
                ProductionItemStatus.Aborted => "已終止",
                _ => item.ProductionItemStatus.ToString()
            };

            var doc = new FormattedDocument()
                .SetDocumentName($"製令單-{item.Code ?? item.Id.ToString()}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首（每頁重複）===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 13f, true),
                        ("製  令  單", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"單號：{item.Code ?? "-"}",
                        $"狀態：{statusText}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(3);

                header.AddKeyValueRow(
                    ("品項名稱", item.Product?.Name ?? "-"),
                    ("品項編號", item.Product?.Code ?? "-"),
                    ("規格", item.Product?.Specification ?? "-"));

                header.AddKeyValueRow(
                    ("計劃數量", item.ScheduledQuantity.ToString("N0")),
                    ("完成數量", item.CompletedQuantity.ToString("N0")),
                    ("未完成數量", item.PendingQuantity.ToString("N0")));

                header.AddKeyValueRow(
                    ("預計開始", item.PlannedStartDate?.ToString("yyyy/MM/dd") ?? "-"),
                    ("預計完成", item.PlannedEndDate?.ToString("yyyy/MM/dd") ?? "-"),
                    ("負責人員", item.ResponsibleEmployee?.Name ?? "-"));

                header.AddKeyValueRow(
                    ("實際開始", item.ActualStartDate?.ToString("yyyy/MM/dd") ?? "-"),
                    ("實際完成", item.ActualEndDate?.ToString("yyyy/MM/dd") ?? "-"),
                    ("所屬排程", item.ProductionSchedule?.Code ?? "-"));

                var salesOrderCode = item.SalesOrderDetail?.SalesOrder?.Code;
                if (!string.IsNullOrEmpty(salesOrderCode))
                    header.AddKeyValueRow(("來源銷售單", salesOrderCode));

                header.AddSpacing(3);
            });

            // === 用料清單 ===
            var details = item.ScheduleDetails?.ToList() ?? new List<ProductionScheduleDetail>();
            if (details.Any())
            {
                doc.AddTitle("用料清單");
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.4f, Models.Reports.TextAlignment.Center)
                         .AddColumn("原料品項名稱", 2.8f, Models.Reports.TextAlignment.Left)
                         .AddColumn("品項編號", 1.2f, Models.Reports.TextAlignment.Left)
                         .AddColumn("需求數量", 0.8f, Models.Reports.TextAlignment.Right)
                         .AddColumn("已領數量", 0.8f, Models.Reports.TextAlignment.Right)
                         .AddColumn("待領數量", 0.8f, Models.Reports.TextAlignment.Right)
                         .AddColumn("領料倉庫", 1.2f, Models.Reports.TextAlignment.Left)
                         .ShowBorder(true)
                         .ShowHeaderBackground(true)
                         .SetRowHeight(18);

                    int rowNum = 1;
                    foreach (var detail in details)
                    {
                        table.AddRow(
                            rowNum.ToString(),
                            detail.ComponentProduct?.Name ?? $"品項#{detail.ComponentProductId}",
                            detail.ComponentProduct?.Code ?? "-",
                            detail.RequiredQuantity.ToString("N2"),
                            detail.IssuedQuantity.ToString("N2"),
                            detail.PendingIssueQuantity.ToString("N2"),
                            detail.Warehouse?.Name ?? "-");
                        rowNum++;
                    }
                });
            }

            // === 完工入庫記錄 ===
            var completions = item.Completions?.ToList() ?? new List<ProductionScheduleCompletion>();
            if (completions.Any())
            {
                doc.AddSpacing(4);
                doc.AddTitle("完工入庫記錄");
                doc.AddTable(table =>
                {
                    table.AddColumn("完工日期", 1.2f, Models.Reports.TextAlignment.Center)
                         .AddColumn("完工數量", 0.8f, Models.Reports.TextAlignment.Right)
                         .AddColumn("批號", 1.2f, Models.Reports.TextAlignment.Left)
                         .AddColumn("有效日期", 1.0f, Models.Reports.TextAlignment.Center)
                         .AddColumn("品質檢驗結果", 2.0f, Models.Reports.TextAlignment.Left)
                         .AddColumn("入庫倉庫", 1.2f, Models.Reports.TextAlignment.Left)
                         .ShowBorder(true)
                         .ShowHeaderBackground(true)
                         .SetRowHeight(18);

                    foreach (var completion in completions)
                    {
                        table.AddRow(
                            completion.CompletionDate.ToString("yyyy/MM/dd"),
                            completion.CompletedQuantity.ToString("N0"),
                            completion.BatchNumber ?? "-",
                            completion.ExpiryDate?.ToString("yyyy/MM/dd") ?? "-",
                            completion.QualityCheckResult ?? "-",
                            completion.Warehouse?.Name ?? "-");
                    }
                });
            }

            // === 頁尾（最後一頁：備註 + 簽核區）===
            doc.BeginFooter(footer =>
            {
                var remarkLines = new List<string>
                {
                    $"備　註：{item.Remarks ?? ""}"
                };

                footer.AddSpacing(5)
                      .AddTwoColumnSection(
                          leftContent: remarkLines,
                          leftTitle: null,
                          leftHasBorder: false,
                          rightContent: new List<string>(),
                          leftWidthRatio: 1.0f);

                footer.AddSpacing(16)
                      .AddSignatureSection("製令人員", "生產負責人", "品管確認", "主管核准");
            });

            return doc;
        }

        #endregion
    }
}
