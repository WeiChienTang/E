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
    /// 商品資料表報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 支援單品列印和清單式批次列印
    /// </summary>
    public class ProductListReportService : IProductListReportService
    {
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<ProductListReportService>? _logger;

        public ProductListReportService(
            IProductService productService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<ProductListReportService>? logger = null)
        {
            _productService = productService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region 報表生成

        /// <summary>
        /// 生成單一商品資料報表（與批次列印使用相同格式）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int productId)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
            {
                throw new ArgumentException($"找不到商品 ID: {productId}");
            }

            var company = await _companyService.GetPrimaryCompanyAsync();

            return BuildProductListDocument(new List<Product> { product }, company, null);
        }

        /// <summary>
        /// 直接列印商品資料
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int productId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(productId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印商品資料 {ProductId} 時發生錯誤", productId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int productId)
        {
            var document = await GenerateReportAsync(productId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int productId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(productId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 批次直接列印
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var products = await GetProductsByCriteriaAsync(criteria);

                if (!products.Any())
                {
                    return ServiceResult.Failure($"無符合條件的商品\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildProductListDocument(products, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印商品資料表時發生錯誤");
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
                var products = await GetProductsByCriteriaAsync(criteria);

                if (!products.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的商品\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildProductListDocument(products, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, products.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染商品資料表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染商品清單報表為圖片（使用 ProductListBatchPrintCriteria）
        /// 清單式報表：將所有商品以表格形式呈現在同一份報表
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(ProductListBatchPrintCriteria criteria)
        {
            try
            {
                var products = await GetProductsByTypedCriteriaAsync(criteria);

                if (!products.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的商品\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildProductListDocument(products, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, products.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染商品資料表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>
        /// 根據 BatchPrintCriteria 查詢商品
        /// </summary>
        private async Task<List<Product>> GetProductsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _productService.GetAllAsync();

            // 篩選關聯實體（分類 ID）
            if (criteria.RelatedEntityIds.Any())
            {
                results = results.Where(p => p.ProductCategoryId.HasValue &&
                    criteria.RelatedEntityIds.Contains(p.ProductCategoryId.Value)).ToList();
            }

            // 單據編號關鍵字搜尋（搜尋品號）
            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(p =>
                    (p.Code != null && p.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Name != null && p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Barcode != null && p.Barcode.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Specification != null && p.Specification.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 排除已取消
            if (!criteria.IncludeCancelled)
            {
                results = results.Where(p => p.Status != EntityStatus.Inactive).ToList();
            }

            // 最大筆數限制
            if (criteria.MaxResults.HasValue)
            {
                results = results.Take(criteria.MaxResults.Value).ToList();
            }

            return results;
        }

        /// <summary>
        /// 根據 ProductListBatchPrintCriteria 查詢商品
        /// </summary>
        private async Task<List<Product>> GetProductsByTypedCriteriaAsync(ProductListBatchPrintCriteria criteria)
        {
            List<Product> results;

            if (criteria.ActiveOnly)
            {
                results = await _productService.GetActiveProductsAsync();
            }
            else
            {
                results = await _productService.GetAllAsync();
            }

            // 篩選分類
            if (criteria.CategoryIds.Any())
            {
                results = results.Where(p => p.ProductCategoryId.HasValue &&
                    criteria.CategoryIds.Contains(p.ProductCategoryId.Value)).ToList();
            }

            // 篩選採購類型
            if (criteria.ProcurementType.HasValue)
            {
                results = results.Where(p => p.ProcurementType == criteria.ProcurementType.Value).ToList();
            }

            // 關鍵字搜尋
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(p =>
                    (p.Code != null && p.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Name != null && p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Barcode != null && p.Barcode.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Specification != null && p.Specification.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return results.OrderBy(p => p.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構商品清單報表（清單式：多筆商品在同一份報表）
        /// </summary>
        private FormattedDocument BuildProductListDocument(
            List<Product> products,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"商品資料表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("商 品 資 料 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"筆數：{products.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 商品資料表格 ===
            doc.AddTable(table =>
            {
                table.AddColumn("項次", 0.35f, Models.Reports.TextAlignment.Center)
                     .AddColumn("品號", 0.9f, Models.Reports.TextAlignment.Left)
                     .AddColumn("品名", 1.5f, Models.Reports.TextAlignment.Left)
                     .AddColumn("規格", 1.0f, Models.Reports.TextAlignment.Left)
                     .AddColumn("條碼", 1.0f, Models.Reports.TextAlignment.Left)
                     .AddColumn("分類", 0.7f, Models.Reports.TextAlignment.Center)
                     .AddColumn("單位", 0.5f, Models.Reports.TextAlignment.Center)
                     .AddColumn("採購類型", 0.6f, Models.Reports.TextAlignment.Center)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                int rowNum = 1;
                foreach (var product in products)
                {
                    var procurementTypeText = product.ProcurementType switch
                    {
                        ProcurementType.Purchased => "外購",
                        ProcurementType.Manufactured => "自製",
                        ProcurementType.Outsourced => "委外",
                        _ => ""
                    };

                    table.AddRow(
                        rowNum.ToString(),
                        product.Code ?? "",
                        product.Name ?? "",
                        product.Specification ?? "",
                        product.Barcode ?? "",
                        product.ProductCategory?.Name ?? "",
                        product.Unit?.Name ?? "",
                        procurementTypeText
                    );
                    rowNum++;
                }
            });

            // === 頁尾區 ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(10);

                var summaryLines = new List<string>
                {
                    $"商品總數：{products.Count} 筆"
                };

                // 按分類統計
                var categoryGroups = products
                    .GroupBy(p => p.ProductCategory?.Name ?? "未分類")
                    .OrderBy(g => g.Key);

                foreach (var group in categoryGroups)
                {
                    summaryLines.Add($"  {group.Key}：{group.Count()} 筆");
                }

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
