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
    /// 客戶名冊表報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 支援單筆列印和清單式批次列印
    /// </summary>
    public class CustomerRosterReportService : ICustomerRosterReportService
    {
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<CustomerRosterReportService>? _logger;

        public CustomerRosterReportService(
            ICustomerService customerService,
            IEmployeeService employeeService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<CustomerRosterReportService>? logger = null)
        {
            _customerService = customerService;
            _employeeService = employeeService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一客戶資料報表（與批次列印使用相同格式）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int customerId)
        {
            var customer = await _customerService.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new ArgumentException($"找不到客戶 ID: {customerId}");
            }

            var company = await _companyService.GetPrimaryCompanyAsync();
            var employeeMap = await GetEmployeeMapAsync();
            return BuildCustomerRosterDocument(new List<Customer> { customer }, company, null, employeeMap);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int customerId)
        {
            var document = await GenerateReportAsync(customerId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int customerId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(customerId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印客戶資料
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
                _logger?.LogError(ex, "列印客戶資料 {CustomerId} 時發生錯誤", customerId);
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
                var customers = await GetCustomersByCriteriaAsync(criteria);

                if (!customers.Any())
                {
                    return ServiceResult.Failure($"無符合條件的客戶\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var employeeMap = await GetEmployeeMapAsync();
                var document = BuildCustomerRosterDocument(customers, company, null, employeeMap);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印客戶名冊表時發生錯誤");
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
                var customers = await GetCustomersByCriteriaAsync(criteria);

                if (!customers.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的客戶\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var employeeMap = await GetEmployeeMapAsync();
                var document = BuildCustomerRosterDocument(customers, company, criteria.PaperSetting, employeeMap);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, customers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染客戶名冊表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region CustomerRosterCriteria 批次報表

        /// <summary>
        /// 以客戶名冊專用篩選條件渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerRosterCriteria criteria)
        {
            try
            {
                var customers = await GetCustomersByTypedCriteriaAsync(criteria);

                if (!customers.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的客戶\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var employeeMap = await GetEmployeeMapAsync();
                var document = BuildCustomerRosterDocument(customers, company, criteria.PaperSetting, employeeMap);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, customers.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染客戶名冊表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 輔助

        /// <summary>
        /// 取得員工 ID → 名稱對照表
        /// </summary>
        private async Task<Dictionary<int, string>> GetEmployeeMapAsync()
        {
            var employees = await _employeeService.GetAllAsync();
            return employees.Where(e => !e.IsSuperAdmin).ToDictionary(e => e.Id, e => e.Name ?? "");
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>
        /// 根據 BatchPrintCriteria 查詢客戶
        /// </summary>
        private async Task<List<Customer>> GetCustomersByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _customerService.GetAllAsync();

            // 篩選業務負責人 ID
            if (criteria.RelatedEntityIds.Any())
            {
                results = results.Where(c => c.EmployeeId.HasValue &&
                    criteria.RelatedEntityIds.Contains(c.EmployeeId.Value)).ToList();
            }

            // 關鍵字搜尋（客戶編號、公司名稱、聯絡人）
            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(c =>
                    (c.Code != null && c.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.CompanyName != null && c.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.ContactPerson != null && c.ContactPerson.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.TaxNumber != null && c.TaxNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 排除已停用
            if (!criteria.IncludeCancelled)
            {
                results = results.Where(c => c.Status == EntityStatus.Active).ToList();
            }

            return results.OrderBy(c => c.Code).ToList();
        }

        /// <summary>
        /// 根據 CustomerRosterCriteria 查詢客戶
        /// </summary>
        private async Task<List<Customer>> GetCustomersByTypedCriteriaAsync(CustomerRosterCriteria criteria)
        {
            var results = await _customerService.GetAllAsync();

            // 僅啟用客戶
            if (criteria.ActiveOnly)
            {
                results = results.Where(c => c.Status == EntityStatus.Active).ToList();
            }

            // 篩選業務負責人
            if (criteria.EmployeeIds.Any())
            {
                results = results.Where(c => c.EmployeeId.HasValue &&
                    criteria.EmployeeIds.Contains(c.EmployeeId.Value)).ToList();
            }

            // 關鍵字搜尋
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(c =>
                    (c.Code != null && c.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.CompanyName != null && c.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.ContactPerson != null && c.ContactPerson.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (c.TaxNumber != null && c.TaxNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return results.OrderBy(c => c.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構客戶名冊表（清單式）
        /// </summary>
        private FormattedDocument BuildCustomerRosterDocument(
            List<Customer> customers,
            Company? company,
            PaperSetting? paperSetting,
            Dictionary<int, string> employeeMap)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"客戶名冊表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("客 戶 名 冊 表", 16f, true)
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

            // === 客戶資料表格 ===
            doc.AddTable(table =>
            {
                table.AddColumn("項次", 0.30f, TextAlignment.Center)
                     .AddColumn("客戶編號", 0.70f, TextAlignment.Left)
                     .AddColumn("公司名稱", 1.20f, TextAlignment.Left)
                     .AddColumn("聯絡人", 0.55f, TextAlignment.Left)
                     .AddColumn("統一編號", 0.65f, TextAlignment.Left)
                     .AddColumn("聯絡電話", 0.75f, TextAlignment.Left)
                     .AddColumn("聯絡地址", 1.30f, TextAlignment.Left)
                     .AddColumn("業務負責人", 0.60f, TextAlignment.Left)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(20);

                int rowNum = 1;
                foreach (var customer in customers)
                {
                    // 取得聯絡電話（優先公司電話，其次聯絡人電話）
                    var phone = !string.IsNullOrEmpty(customer.CompanyContactPhone)
                        ? customer.CompanyContactPhone
                        : customer.ContactPhone ?? "";

                    table.AddRow(
                        rowNum.ToString(),
                        customer.Code ?? "",
                        customer.CompanyName ?? "",
                        customer.ContactPerson ?? "",
                        customer.TaxNumber ?? "",
                        phone,
                        customer.ContactAddress ?? "",
                        customer.EmployeeId.HasValue && employeeMap.ContainsKey(customer.EmployeeId.Value)
                            ? employeeMap[customer.EmployeeId.Value] : ""
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
                    $"客戶總數：{customers.Count} 筆"
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
