using ERPCore2.Data.Entities;
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
    /// 商品詳細資料報表服務實作（PD005）
    /// 每項商品各佔一區塊，以 key-value 方式顯示完整規格、分類、採購類型與成本資訊
    /// 支援單筆（EditModal）和批次（報表集 / Alt+R）兩種進入路徑
    /// </summary>
    public class ProductDetailReportService : IProductDetailReportService
    {
        private readonly IProductService _productService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<ProductDetailReportService>? _logger;

        public ProductDetailReportService(
            IProductService productService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<ProductDetailReportService>? logger = null)
        {
            _productService = productService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一商品詳細資料報表（供 EditModal 使用）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int productId)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"找不到商品 ID: {productId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildProductDetailDocument(new List<Product> { product }, company, null);
        }

        /// <summary>
        /// 將單筆商品報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int productId)
        {
            var document = await GenerateReportAsync(productId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將單筆商品報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int productId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(productId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印單筆商品詳細資料
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
                _logger?.LogError(ex, "列印商品詳細資料 {ProductId} 時發生錯誤", productId);
                return ServiceResult.Failure($"列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次直接列印（使用通用 BatchPrintCriteria）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintBatchAsync(BatchPrintCriteria criteria, string reportId)
        {
            try
            {
                var products = await GetProductsByCriteriaAsync(criteria);
                if (!products.Any())
                    return ServiceResult.Failure($"無符合條件的商品\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildProductDetailDocument(products, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印商品詳細資料時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 批次渲染報表為圖片（使用通用 BatchPrintCriteria）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            try
            {
                var products = await GetProductsByCriteriaAsync(criteria);
                if (!products.Any())
                    return BatchPreviewResult.Failure($"無符合條件的商品\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildProductDetailDocument(products, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, products.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染商品詳細資料時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region ProductListBatchPrintCriteria 批次報表

        /// <summary>
        /// 以商品清單篩選條件批次渲染詳細格式報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(ProductListBatchPrintCriteria criteria)
        {
            try
            {
                var products = await GetProductsByTypedCriteriaAsync(criteria);
                if (!products.Any())
                    return BatchPreviewResult.Failure($"無符合條件的商品\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildProductDetailDocument(products, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, products.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染商品詳細資料時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        private async Task<List<Product>> GetProductsByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _productService.GetAllAsync();

            if (criteria.RelatedEntityIds.Any())
            {
                results = results.Where(p => p.ProductCategoryId.HasValue &&
                    criteria.RelatedEntityIds.Contains(p.ProductCategoryId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(p =>
                    (p.Code != null && p.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Name != null && p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Barcode != null && p.Barcode.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Specification != null && p.Specification.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!criteria.IncludeCancelled)
                results = results.Where(p => p.Status != EntityStatus.Inactive).ToList();

            if (criteria.MaxResults.HasValue)
                results = results.Take(criteria.MaxResults.Value).ToList();

            return results;
        }

        private async Task<List<Product>> GetProductsByTypedCriteriaAsync(ProductListBatchPrintCriteria criteria)
        {
            List<Product> results;

            if (criteria.ActiveOnly)
                results = await _productService.GetActiveProductsAsync();
            else
                results = await _productService.GetAllAsync();

            if (criteria.CategoryIds.Any())
            {
                results = results.Where(p => p.ProductCategoryId.HasValue &&
                    criteria.CategoryIds.Contains(p.ProductCategoryId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(p =>
                    (p.Code != null && p.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Name != null && p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Barcode != null && p.Barcode.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Specification != null && p.Specification.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return results.OrderBy(p => p.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構商品詳細資料報表
        /// 每項商品以 key-value 區塊呈現，區塊間以分隔線隔開
        /// </summary>
        private FormattedDocument BuildProductDetailDocument(
            List<Product> products,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"商品詳細資料-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區（每頁重複） ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("商 品 詳 細 資 料", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"商品數：{products.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 商品資料區塊 ===
            bool isFirst = true;
            foreach (var product in products)
            {
                if (!isFirst)
                {
                    doc.AddSpacing(5);
                    doc.AddLine(Models.Reports.LineStyle.Solid, 0.5f);
                    doc.AddSpacing(5);
                }
                isFirst = false;

                // 基本資訊
                doc.AddKeyValueRow(
                    ("品號", product.Code ?? ""),
                    ("品名", product.Name ?? ""));

                doc.AddKeyValueRow(
                    ("規格", product.Specification ?? ""),
                    ("條碼", product.Barcode ?? ""));

                doc.AddKeyValueRow(
                    ("分類", product.ProductCategory?.Name ?? ""),
                    ("單位", product.Unit?.Name ?? ""));

                doc.AddKeyValueRow(
                    ("尺寸", product.Size?.Name ?? ""),
                    ("", ""));

                // 財務資訊
                doc.AddKeyValueRow(
                    ("稅率", product.TaxRate > 0 ? $"{product.TaxRate:0.##}%" : ""),
                    ("標準成本", product.StandardCost > 0 ? product.StandardCost.Value.ToString("N2") : ""));

                doc.AddKeyValueRow(
                    ("生產單位", product.ProductionUnit?.Name ?? ""),
                    ("", ""));

                // 生產換算比率（有生產單位且比率不為 1 時顯示）
                if (product.ProductionUnit != null && product.ProductionUnitConversionRate != null && product.ProductionUnitConversionRate != 1)
                {
                    doc.AddKeyValueRow(
                        ("換算比率", product.ProductionUnitConversionRate.Value.ToString("G")),
                        ("", ""));
                }

                // 備註
                if (!string.IsNullOrEmpty(product.Remarks))
                {
                    doc.AddKeyValueRow(
                        ("備註", product.Remarks),
                        ("", ""));
                }
            }

            // === 頁尾區（最後一頁） ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "主管" });
            });

            return doc;
        }

        #endregion
    }
}
