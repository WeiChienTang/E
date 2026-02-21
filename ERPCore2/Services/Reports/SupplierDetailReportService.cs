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
    /// 廠商詳細資料報表服務實作（AP005）
    /// 每位廠商各佔一區塊，以 key-value 方式顯示完整聯絡與付款資訊
    /// 支援單筆（EditModal）和批次（報表集 / Alt+R）兩種進入路徑
    /// </summary>
    public class SupplierDetailReportService : ISupplierDetailReportService
    {
        private readonly ISupplierService _supplierService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<SupplierDetailReportService>? _logger;

        public SupplierDetailReportService(
            ISupplierService supplierService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<SupplierDetailReportService>? logger = null)
        {
            _supplierService = supplierService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一廠商詳細資料報表（供 EditModal 使用）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int supplierId)
        {
            var supplier = await _supplierService.GetByIdAsync(supplierId);
            if (supplier == null)
                throw new ArgumentException($"找不到廠商 ID: {supplierId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildSupplierDetailDocument(new List<Supplier> { supplier }, company, null);
        }

        /// <summary>
        /// 將單筆廠商報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int supplierId)
        {
            var document = await GenerateReportAsync(supplierId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將單筆廠商報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int supplierId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(supplierId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印單筆廠商詳細資料
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
                _logger?.LogError(ex, "列印廠商詳細資料 {SupplierId} 時發生錯誤", supplierId);
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
                var suppliers = await GetSuppliersByCriteriaAsync(criteria);
                if (!suppliers.Any())
                    return ServiceResult.Failure($"無符合條件的廠商\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildSupplierDetailDocument(suppliers, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印廠商詳細資料時發生錯誤");
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
                var suppliers = await GetSuppliersByCriteriaAsync(criteria);
                if (!suppliers.Any())
                    return BatchPreviewResult.Failure($"無符合條件的廠商\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildSupplierDetailDocument(suppliers, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, suppliers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染廠商詳細資料時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region SupplierRosterCriteria 批次報表

        /// <summary>
        /// 以廠商名冊篩選條件批次渲染詳細格式報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(SupplierRosterCriteria criteria)
        {
            try
            {
                var suppliers = await GetSuppliersByTypedCriteriaAsync(criteria);
                if (!suppliers.Any())
                    return BatchPreviewResult.Failure($"無符合條件的廠商\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildSupplierDetailDocument(suppliers, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, suppliers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染廠商詳細資料時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        private async Task<List<Supplier>> GetSuppliersByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _supplierService.GetAllAsync();

            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(s =>
                    (s.Code != null && s.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.CompanyName != null && s.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.ContactPerson != null && s.ContactPerson.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.TaxNumber != null && s.TaxNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!criteria.IncludeCancelled)
                results = results.Where(s => s.Status == EntityStatus.Active).ToList();

            return results.OrderBy(s => s.Code).ToList();
        }

        private async Task<List<Supplier>> GetSuppliersByTypedCriteriaAsync(SupplierRosterCriteria criteria)
        {
            var results = await _supplierService.GetAllAsync();

            if (criteria.SupplierIds.Any())
                results = results.Where(s => criteria.SupplierIds.Contains(s.Id)).ToList();

            if (criteria.ActiveOnly)
                results = results.Where(s => s.Status == EntityStatus.Active).ToList();

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(s =>
                    (s.Code != null && s.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.CompanyName != null && s.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.ContactPerson != null && s.ContactPerson.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (s.TaxNumber != null && s.TaxNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return results.OrderBy(s => s.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構廠商詳細資料報表
        /// 每位廠商以 key-value 區塊呈現，區塊間以分隔線隔開
        /// </summary>
        private FormattedDocument BuildSupplierDetailDocument(
            List<Supplier> suppliers,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"廠商詳細資料-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區（每頁重複） ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("廠 商 詳 細 資 料", 16f, true)
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

            // === 廠商資料區塊 ===
            bool isFirst = true;
            foreach (var supplier in suppliers)
            {
                if (!isFirst)
                {
                    doc.AddSpacing(5);
                    doc.AddLine(Models.Reports.LineStyle.Solid, 0.5f);
                    doc.AddSpacing(5);
                }
                isFirst = false;

                // 聯絡電話（優先公司電話，其次聯絡人電話）
                var phone = !string.IsNullOrEmpty(supplier.SupplierContactPhone)
                    ? supplier.SupplierContactPhone
                    : supplier.ContactPhone ?? "";

                // 基本資訊
                doc.AddKeyValueRow(
                    ("廠商編號", supplier.Code ?? ""),
                    ("公司名稱", supplier.CompanyName ?? ""));

                doc.AddKeyValueRow(
                    ("統一編號", supplier.TaxNumber ?? ""),
                    ("負責人", supplier.ResponsiblePerson ?? ""));

                doc.AddKeyValueRow(
                    ("聯絡人", supplier.ContactPerson ?? ""),
                    ("職稱", supplier.JobTitle ?? ""));

                // 聯絡方式
                doc.AddKeyValueRow(
                    ("公司電話", phone),
                    ("行動電話", supplier.MobilePhone ?? ""));

                doc.AddKeyValueRow(
                    ("傳真", supplier.Fax ?? ""),
                    ("信箱", supplier.Email ?? ""));

                doc.AddKeyValueRow(
                    ("公司網址", supplier.Website ?? ""),
                    ("", ""));

                // 地址資訊
                doc.AddKeyValueRow(
                    ("聯絡地址", supplier.ContactAddress ?? ""),
                    ("公司地址", supplier.SupplierAddress ?? ""));

                // 付款資訊
                doc.AddKeyValueRow(
                    ("付款方式", supplier.PaymentMethod?.Name ?? ""),
                    ("付款條件", supplier.PaymentTerms ?? ""));

                // 備註
                if (!string.IsNullOrEmpty(supplier.Remarks))
                {
                    doc.AddKeyValueRow(
                        ("備註", supplier.Remarks),
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
