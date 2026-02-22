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
    /// 會計科目表報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 支援單筆列印和清單式批次列印
    /// </summary>
    public class AccountItemListReportService : IAccountItemListReportService
    {
        private readonly IAccountItemService _accountItemService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<AccountItemListReportService>? _logger;

        public AccountItemListReportService(
            IAccountItemService accountItemService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<AccountItemListReportService>? logger = null)
        {
            _accountItemService = accountItemService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一科目資料報表（與批次列印使用相同格式）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int accountItemId)
        {
            var accountItem = await _accountItemService.GetByIdAsync(accountItemId);
            if (accountItem == null)
            {
                throw new ArgumentException($"找不到會計科目 ID: {accountItemId}");
            }

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildAccountItemListDocument(new List<AccountItem> { accountItem }, company, null);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int accountItemId)
        {
            var document = await GenerateReportAsync(accountItemId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int accountItemId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(accountItemId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印科目資料
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int accountItemId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(accountItemId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印會計科目資料 {AccountItemId} 時發生錯誤", accountItemId);
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
                var accountItems = await GetAccountItemsByCriteriaAsync(criteria);

                if (!accountItems.Any())
                {
                    return ServiceResult.Failure($"無符合條件的會計科目\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildAccountItemListDocument(accountItems, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印會計科目表時發生錯誤");
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
                var accountItems = await GetAccountItemsByCriteriaAsync(criteria);

                if (!accountItems.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的會計科目\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildAccountItemListDocument(accountItems, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, accountItems.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染會計科目表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region AccountItemListCriteria 批次報表

        /// <summary>
        /// 以會計科目表專用篩選條件渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(AccountItemListCriteria criteria)
        {
            try
            {
                var accountItems = await GetAccountItemsByTypedCriteriaAsync(criteria);

                if (!accountItems.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的會計科目\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildAccountItemListDocument(accountItems, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, accountItems.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染會計科目表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>
        /// 根據 BatchPrintCriteria 查詢會計科目
        /// </summary>
        private async Task<List<AccountItem>> GetAccountItemsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _accountItemService.GetAllWithParentAsync();

            // 關鍵字搜尋（科目代碼、科目名稱）
            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(a =>
                    (a.Code != null && a.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    a.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return results.OrderBy(a => a.AccountType).ThenBy(a => a.AccountLevel)
                .ThenBy(a => a.SortOrder).ThenBy(a => a.Code).ToList();
        }

        /// <summary>
        /// 根據 AccountItemListCriteria 查詢會計科目
        /// </summary>
        private async Task<List<AccountItem>> GetAccountItemsByTypedCriteriaAsync(AccountItemListCriteria criteria)
        {
            var results = await _accountItemService.GetAllWithParentAsync();

            // 篩選科目大類
            if (criteria.AccountTypes.Any())
            {
                results = results.Where(a => criteria.AccountTypes.Contains(a.AccountType)).ToList();
            }

            // 篩選借貸方向
            if (criteria.AccountDirections.Any())
            {
                results = results.Where(a => criteria.AccountDirections.Contains(a.Direction)).ToList();
            }

            // 篩選層級
            if (criteria.AccountLevels.Any())
            {
                var levels = criteria.AccountLevels.Select(l => (int)l).ToHashSet();
                results = results.Where(a => levels.Contains(a.AccountLevel)).ToList();
            }

            // 僅明細科目
            if (criteria.DetailAccountOnly)
            {
                results = results.Where(a => a.IsDetailAccount).ToList();
            }

            // 科目代碼搜尋
            if (!string.IsNullOrEmpty(criteria.CodeKeyword))
            {
                var keyword = criteria.CodeKeyword;
                results = results.Where(a =>
                    a.Code != null && a.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // 科目名稱搜尋
            if (!string.IsNullOrEmpty(criteria.NameKeyword))
            {
                var keyword = criteria.NameKeyword;
                results = results.Where(a =>
                    a.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (a.EnglishName != null && a.EnglishName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return results.OrderBy(a => a.AccountType).ThenBy(a => a.AccountLevel)
                .ThenBy(a => a.SortOrder).ThenBy(a => a.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構會計科目表（清單式：依科目大類分組）
        /// </summary>
        private FormattedDocument BuildAccountItemListDocument(
            List<AccountItem> accountItems,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"會計科目表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("會 計 科 目 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"科目數：{accountItems.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 依科目大類分組顯示 ===
            var accountTypeGroups = accountItems
                .GroupBy(a => a.AccountType)
                .OrderBy(g => g.Key);

            foreach (var group in accountTypeGroups)
            {
                var typeName = GetAccountTypeName(group.Key);

                // 大類標題
                doc.AddKeyValueRow(
                    ("科目大類", $"{typeName}（{group.Count()} 科目）"));

                doc.AddSpacing(3);

                // 科目資料表格
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.30f, Models.Reports.TextAlignment.Center)
                         .AddColumn("科目代碼", 0.75f, Models.Reports.TextAlignment.Left)
                         .AddColumn("科目名稱", 1.40f, Models.Reports.TextAlignment.Left)
                         .AddColumn("層級", 0.35f, Models.Reports.TextAlignment.Center)
                         .AddColumn("借貸", 0.40f, Models.Reports.TextAlignment.Center)
                         .AddColumn("明細", 0.35f, Models.Reports.TextAlignment.Center)
                         .AddColumn("上層科目", 1.00f, Models.Reports.TextAlignment.Left)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    var sortedItems = group.OrderBy(a => a.AccountLevel)
                        .ThenBy(a => a.SortOrder).ThenBy(a => a.Code);

                    foreach (var item in sortedItems)
                    {
                        var indent = new string(' ', (item.AccountLevel - 1) * 2);
                        var directionText = item.Direction == AccountDirection.Debit ? "借" : "貸";
                        var isDetailText = item.IsDetailAccount ? "是" : "-";
                        var parentName = item.Parent != null
                            ? $"{item.Parent.Code} {item.Parent.Name}"
                            : "-";

                        table.AddRow(
                            rowNum.ToString(),
                            item.Code ?? "",
                            indent + item.Name,
                            item.AccountLevel.ToString(),
                            directionText,
                            isDetailText,
                            parentName
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

                var detailCount = accountItems.Count(a => a.IsDetailAccount);
                var summaryLines = new List<string>
                {
                    $"科目總數：{accountItems.Count} 科目",
                    $"  明細科目：{detailCount} 個",
                    $"  非明細科目：{accountItems.Count - detailCount} 個",
                    ""
                };

                // 按科目大類統計
                foreach (var group in accountTypeGroups)
                {
                    summaryLines.Add($"  {GetAccountTypeName(group.Key)}：{group.Count()} 科目");
                }

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.6f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "財務主管" });
            });

            return doc;
        }

        private static string GetAccountTypeName(AccountType accountType) => accountType switch
        {
            AccountType.Asset => "資產",
            AccountType.Liability => "負債",
            AccountType.Equity => "權益",
            AccountType.Revenue => "營業收入",
            AccountType.Cost => "營業成本",
            AccountType.Expense => "營業費用",
            AccountType.NonOperatingIncomeAndExpense => "營業外收益及費損",
            AccountType.ComprehensiveIncome => "綜合損益總額",
            _ => accountType.ToString()
        };

        #endregion
    }
}
