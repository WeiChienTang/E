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
    /// 廠商名冊表報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 支援單筆列印和清單式批次列印
    /// </summary>
    public class SupplierRosterReportService : ISupplierRosterReportService
    {
        private readonly ISupplierService _supplierService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<SupplierRosterReportService>? _logger;

        public SupplierRosterReportService(
            ISupplierService supplierService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<SupplierRosterReportService>? logger = null)
        {
            _supplierService = supplierService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一廠商資料報表（與批次列印使用相同格式）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int supplierId)
        {
            var supplier = await _supplierService.GetByIdAsync(supplierId);
            if (supplier == null)
            {
                throw new ArgumentException($"找不到廠商 ID: {supplierId}");
            }

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildSupplierRosterDocument(new List<Supplier> { supplier }, company, null);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int supplierId)
        {
            var document = await GenerateReportAsync(supplierId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int supplierId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(supplierId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印廠商資料
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int supplierId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(supplierId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印廠商資料 {SupplierId} 時發生錯誤", supplierId);
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
                var suppliers = await GetSuppliersByCriteriaAsync(criteria);

                if (!suppliers.Any())
                {
                    return ServiceResult.Failure($"無符合條件的廠商\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildSupplierRosterDocument(suppliers, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印廠商名冊表時發生錯誤");
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
                var suppliers = await GetSuppliersByCriteriaAsync(criteria);

                if (!suppliers.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的廠商\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildSupplierRosterDocument(suppliers, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, suppliers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染廠商名冊表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region SupplierRosterCriteria 批次報表

        /// <summary>
        /// 以廠商名冊專用篩選條件渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(SupplierRosterCriteria criteria)
        {
            try
            {
                var suppliers = await GetSuppliersByTypedCriteriaAsync(criteria);

                if (!suppliers.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的廠商\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildSupplierRosterDocument(suppliers, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, suppliers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染廠商名冊表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>
        /// 根據 BatchPrintCriteria 查詢廠商
        /// </summary>
        private async Task<List<Supplier>> GetSuppliersByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _supplierService.GetAllAsync();

            // 關鍵字搜尋（廠商編號、公司名稱、聯絡人）
            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(s =>
                    (s.Code != null && s.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.CompanyName != null && s.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.ContactPerson != null && s.ContactPerson.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.TaxNumber != null && s.TaxNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 排除已停用
            if (!criteria.IncludeCancelled)
            {
                results = results.Where(s => s.Status == EntityStatus.Active).ToList();
            }

            return results.OrderBy(s => s.Code).ToList();
        }

        /// <summary>
        /// 根據 SupplierRosterCriteria 查詢廠商
        /// </summary>
        private async Task<List<Supplier>> GetSuppliersByTypedCriteriaAsync(SupplierRosterCriteria criteria)
        {
            var results = await _supplierService.GetAllAsync();

            // 指定廠商篩選
            if (criteria.SupplierIds.Any())
            {
                results = results.Where(s => criteria.SupplierIds.Contains(s.Id)).ToList();
            }

            // 僅啟用廠商
            if (criteria.ActiveOnly)
            {
                results = results.Where(s => s.Status == EntityStatus.Active).ToList();
            }

            // 關鍵字搜尋
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(s =>
                    (s.Code != null && s.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.CompanyName != null && s.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.ContactPerson != null && s.ContactPerson.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.TaxNumber != null && s.TaxNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return results.OrderBy(s => s.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構廠商名冊表（清單式）
        /// </summary>
        private FormattedDocument BuildSupplierRosterDocument(
            List<Supplier> suppliers,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"廠商名冊表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("廠 商 名 冊 表", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"廠商數：{suppliers.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 廠商資料表格 ===
            doc.AddTable(table =>
            {
                table.AddColumn("項次", 0.30f, TextAlignment.Center)
                     .AddColumn("廠商編號", 0.70f, TextAlignment.Left)
                     .AddColumn("廠商名稱", 1.20f, TextAlignment.Left)
                     .AddColumn("聯絡人", 0.55f, TextAlignment.Left)
                     .AddColumn("統一編號", 0.65f, TextAlignment.Left)
                     .AddColumn("聯絡電話", 0.75f, TextAlignment.Left)
                     .AddColumn("聯絡地址", 1.30f, TextAlignment.Left)
                     .AddColumn("付款方式", 0.60f, TextAlignment.Left)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                int rowNum = 1;
                foreach (var supplier in suppliers)
                {
                    // 取得聯絡電話（優先公司電話，其次聯絡人電話）
                    var phone = !string.IsNullOrEmpty(supplier.SupplierContactPhone)
                        ? supplier.SupplierContactPhone
                        : supplier.ContactPhone ?? "";

                    table.AddRow(
                        rowNum.ToString(),
                        supplier.Code ?? "",
                        supplier.CompanyName ?? "",
                        supplier.ContactPerson ?? "",
                        supplier.TaxNumber ?? "",
                        phone,
                        supplier.ContactAddress ?? "",
                        supplier.PaymentMethod?.Name ?? ""
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
                    $"廠商總數：{suppliers.Count} 筆"
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
