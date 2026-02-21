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
    /// 客戶詳細資料報表服務實作（AR006）
    /// 每位客戶各佔一區塊，以 key-value 方式顯示完整聯絡與付款資訊
    /// 支援單筆（EditModal）和批次（報表集 / Alt+R）兩種進入路徑
    /// </summary>
    public class CustomerDetailReportService : ICustomerDetailReportService
    {
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<CustomerDetailReportService>? _logger;

        public CustomerDetailReportService(
            ICustomerService customerService,
            IEmployeeService employeeService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<CustomerDetailReportService>? logger = null)
        {
            _customerService = customerService;
            _employeeService = employeeService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一客戶詳細資料報表（供 EditModal 使用）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int customerId)
        {
            var customer = await _customerService.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException($"找不到客戶 ID: {customerId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            var employeeMap = await GetEmployeeMapAsync();
            return BuildCustomerDetailDocument(new List<Customer> { customer }, company, null, employeeMap);
        }

        /// <summary>
        /// 將單筆客戶報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int customerId)
        {
            var document = await GenerateReportAsync(customerId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將單筆客戶報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int customerId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(customerId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印單筆客戶詳細資料
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int customerId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(customerId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印客戶詳細資料 {CustomerId} 時發生錯誤", customerId);
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
                var customers = await GetCustomersByCriteriaAsync(criteria);
                if (!customers.Any())
                    return ServiceResult.Failure($"無符合條件的客戶\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var employeeMap = await GetEmployeeMapAsync();
                var document = BuildCustomerDetailDocument(customers, company, null, employeeMap);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印客戶詳細資料時發生錯誤");
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
                var customers = await GetCustomersByCriteriaAsync(criteria);
                if (!customers.Any())
                    return BatchPreviewResult.Failure($"無符合條件的客戶\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var employeeMap = await GetEmployeeMapAsync();
                var document = BuildCustomerDetailDocument(customers, company, criteria.PaperSetting, employeeMap);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, customers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染客戶詳細資料時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region CustomerRosterCriteria 批次報表

        /// <summary>
        /// 以客戶名冊篩選條件批次渲染詳細格式報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerRosterCriteria criteria)
        {
            try
            {
                var customers = await GetCustomersByTypedCriteriaAsync(criteria);
                if (!customers.Any())
                    return BatchPreviewResult.Failure($"無符合條件的客戶\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var employeeMap = await GetEmployeeMapAsync();
                var document = BuildCustomerDetailDocument(customers, company, criteria.PaperSetting, employeeMap);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, customers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染客戶詳細資料時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 輔助

        private async Task<Dictionary<int, string>> GetEmployeeMapAsync()
        {
            var employees = await _employeeService.GetAllAsync();
            return employees.Where(e => !e.IsSuperAdmin).ToDictionary(e => e.Id, e => e.Name ?? "");
        }

        #endregion

        #region 私有方法 - 查詢資料

        private async Task<List<Customer>> GetCustomersByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _customerService.GetAllAsync();

            if (criteria.RelatedEntityIds.Any())
            {
                results = results.Where(c => c.EmployeeId.HasValue &&
                    criteria.RelatedEntityIds.Contains(c.EmployeeId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(c =>
                    (c.Code != null && c.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.CompanyName != null && c.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.ContactPerson != null && c.ContactPerson.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.TaxNumber != null && c.TaxNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!criteria.IncludeCancelled)
                results = results.Where(c => c.Status == EntityStatus.Active).ToList();

            return results.OrderBy(c => c.Code).ToList();
        }

        private async Task<List<Customer>> GetCustomersByTypedCriteriaAsync(CustomerRosterCriteria criteria)
        {
            var results = await _customerService.GetAllAsync();

            if (criteria.CustomerIds.Any())
                results = results.Where(c => criteria.CustomerIds.Contains(c.Id)).ToList();

            if (criteria.ActiveOnly)
                results = results.Where(c => c.Status == EntityStatus.Active).ToList();

            if (criteria.EmployeeIds.Any())
            {
                results = results.Where(c => c.EmployeeId.HasValue &&
                    criteria.EmployeeIds.Contains(c.EmployeeId.Value)).ToList();
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(c =>
                    (c.Code != null && c.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.CompanyName != null && c.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.ContactPerson != null && c.ContactPerson.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.TaxNumber != null && c.TaxNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return results.OrderBy(c => c.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構客戶詳細資料報表
        /// 每位客戶以 key-value 區塊呈現，區塊間以分隔線隔開
        /// </summary>
        private FormattedDocument BuildCustomerDetailDocument(
            List<Customer> customers,
            Company? company,
            PaperSetting? paperSetting,
            Dictionary<int, string> employeeMap)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"客戶詳細資料-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區（每頁重複） ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("客 戶 詳 細 資 料", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"客戶數：{customers.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 客戶資料區塊 ===
            bool isFirst = true;
            foreach (var customer in customers)
            {
                if (!isFirst)
                {
                    doc.AddSpacing(5);
                    doc.AddLine(Models.Reports.LineStyle.Solid, 0.5f);
                    doc.AddSpacing(5);
                }
                isFirst = false;

                // 取得業務負責人名稱
                var employeeName = customer.EmployeeId.HasValue && employeeMap.ContainsKey(customer.EmployeeId.Value)
                    ? employeeMap[customer.EmployeeId.Value] : "";

                // 聯絡電話（優先公司電話，其次聯絡人電話）
                var phone = !string.IsNullOrEmpty(customer.CompanyContactPhone)
                    ? customer.CompanyContactPhone
                    : customer.ContactPhone ?? "";

                // 基本資訊
                doc.AddKeyValueRow(
                    ("客戶編號", customer.Code ?? ""),
                    ("公司名稱", customer.CompanyName ?? ""));

                doc.AddKeyValueRow(
                    ("統一編號", customer.TaxNumber ?? ""),
                    ("負責人", customer.ResponsiblePerson ?? ""));

                doc.AddKeyValueRow(
                    ("聯絡人", customer.ContactPerson ?? ""),
                    ("職稱", customer.JobTitle ?? ""));

                // 聯絡方式
                doc.AddKeyValueRow(
                    ("公司電話", phone),
                    ("行動電話", customer.MobilePhone ?? ""));

                doc.AddKeyValueRow(
                    ("傳真", customer.Fax ?? ""),
                    ("信箱", customer.Email ?? ""));

                doc.AddKeyValueRow(
                    ("公司網址", customer.Website ?? ""),
                    ("業務負責人", employeeName));

                // 地址資訊
                doc.AddKeyValueRow(
                    ("聯絡地址", customer.ContactAddress ?? ""),
                    ("貨運地址", customer.ShippingAddress ?? ""));

                // 付款與財務資訊
                doc.AddKeyValueRow(
                    ("付款方式", customer.PaymentMethod?.Name ?? ""),
                    ("付款條件", customer.PaymentTerms ?? ""));

                doc.AddKeyValueRow(
                    ("收款日期", customer.PaymentDate.HasValue ? $"{customer.PaymentDate}日" : ""),
                    ("信用額度", customer.CreditLimit.HasValue ? customer.CreditLimit.Value.ToString("N0") : ""));

                doc.AddKeyValueRow(
                    ("發票抬頭", customer.InvoiceTitle ?? ""),
                    ("", ""));

                // 備註
                if (!string.IsNullOrEmpty(customer.Remarks))
                {
                    doc.AddKeyValueRow(
                        ("備註", customer.Remarks),
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
