using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services;
using ERPCore2.Services.Customers;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 客戶詳細資料報表服務實作（AR006）
    /// 每位客戶各佔一區塊，以 key-value 方式顯示完整聯絡與付款資訊
    /// 支援區段選擇：使用者可在列印前勾選要包含的內容（銀行帳戶、車輛、拜訪紀錄等）
    /// </summary>
    public class CustomerDetailReportService : ICustomerDetailReportService
    {
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ICustomerBankAccountService _bankAccountService;
        private readonly IItemCustomerService _itemCustomerService;
        private readonly ICustomerVisitService _visitService;
        private readonly ICustomerComplaintService _complaintService;
        private readonly IVehicleService _vehicleService;
        private readonly IBusinessCardService _businessCardService;
        private readonly INavigationPermissionService _permissionService;
        private readonly ICompanyModuleService _companyModuleService;
        private readonly ILogger<CustomerDetailReportService>? _logger;

        public CustomerDetailReportService(
            ICustomerService customerService,
            IEmployeeService employeeService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ICustomerBankAccountService bankAccountService,
            IItemCustomerService itemCustomerService,
            ICustomerVisitService visitService,
            ICustomerComplaintService complaintService,
            IVehicleService vehicleService,
            IBusinessCardService businessCardService,
            INavigationPermissionService permissionService,
            ICompanyModuleService companyModuleService,
            ILogger<CustomerDetailReportService>? logger = null)
        {
            _customerService = customerService;
            _employeeService = employeeService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _bankAccountService = bankAccountService;
            _itemCustomerService = itemCustomerService;
            _visitService = visitService;
            _complaintService = complaintService;
            _vehicleService = vehicleService;
            _businessCardService = businessCardService;
            _permissionService = permissionService;
            _companyModuleService = companyModuleService;
            _logger = logger;
        }

        // ===== 區段常數 =====

        public static class SectionKeys
        {
            public const string BasicInfo = "BasicInfo";
            public const string BankAccounts = "BankAccounts";
            public const string CustomerItems = "CustomerItems";
            public const string VisitRecords = "VisitRecords";
            public const string ComplaintRecords = "ComplaintRecords";
            public const string TransactionRecords = "TransactionRecords";
            public const string Vehicles = "Vehicles";
            public const string BusinessCards = "BusinessCards";
        }

        #region ISectionAwareReportService 實作

        /// <summary>
        /// 取得可用的報表區段清單（已做權限/模組檢查）
        /// </summary>
        public async Task<List<ReportSectionDefinition>> GetAvailableSectionsAsync(int customerId)
        {
            bool? isSuperAdmin = null;

            var sections = new List<ReportSectionDefinition>
            {
                new() { Key = SectionKeys.BasicInfo, Label = "基本資料", Icon = "bi-building", IsChecked = true, IsEnabled = true },
                new() { Key = SectionKeys.BankAccounts, Label = "銀行帳戶", Icon = "bi-bank", IsChecked = true, IsEnabled = true },
                new() { Key = SectionKeys.CustomerItems, Label = "客戶品項", Icon = "bi-box-seam", IsChecked = true, IsEnabled = true },
                new() { Key = SectionKeys.VisitRecords, Label = "拜訪紀錄", Icon = "bi-journal-text", IsChecked = true, IsEnabled = true },
                new() { Key = SectionKeys.ComplaintRecords, Label = "客訴紀錄", Icon = "bi-exclamation-circle", IsChecked = true, IsEnabled = true },
                new() { Key = SectionKeys.BusinessCards, Label = "名片資料", Icon = "bi-person-vcard", IsChecked = false, IsEnabled = true },
            };

            // 車輛模組
            var isVehiclesEnabled = await _companyModuleService.IsModuleEnabledAsync("Vehicles");
            if (!isVehiclesEnabled)
            {
                isSuperAdmin ??= await _permissionService.IsCurrentEmployeeSuperAdminAsync();
                isVehiclesEnabled = isSuperAdmin.Value;
            }
            sections.Add(new()
            {
                Key = SectionKeys.Vehicles, Label = "車輛資訊", Icon = "bi-truck",
                IsChecked = isVehiclesEnabled, IsEnabled = isVehiclesEnabled,
                DisabledReason = isVehiclesEnabled ? null : "模組未啟用"
            });

            return sections;
        }

        /// <summary>
        /// 根據選取的區段產生單一客戶報表文件
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int customerId, List<string> selectedSectionKeys)
        {
            var customer = await _customerService.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException($"找不到客戶 ID: {customerId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            var employeeMap = await GetEmployeeMapAsync();

            return await BuildCustomerDetailDocumentAsync(
                new List<Customer> { customer }, company, null, employeeMap, selectedSectionKeys);
        }

        #endregion

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一客戶詳細資料報表（預設全部區段）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int customerId)
        {
            var sections = await GetAvailableSectionsAsync(customerId);
            var allKeys = sections.Where(s => s.IsEnabled && s.IsChecked).Select(s => s.Key).ToList();
            return await GenerateReportAsync(customerId, allKeys);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int customerId)
        {
            var document = await GenerateReportAsync(customerId);
            return _formattedPrintService.RenderToImages(document);
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int customerId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(customerId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

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
                var document = await BuildCustomerDetailDocumentAsync(customers, company, null, employeeMap, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印客戶詳細資料時發生錯誤");
                return ServiceResult.Failure($"批次列印時發生錯誤: {ex.Message}");
            }
        }

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(BatchPrintCriteria criteria)
        {
            try
            {
                var customers = await GetCustomersByCriteriaAsync(criteria);
                customers = customers.ExcludeDrafts();
                if (!customers.Any())
                    return BatchPreviewResult.Failure($"無符合條件的客戶\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var employeeMap = await GetEmployeeMapAsync();
                var document = await BuildCustomerDetailDocumentAsync(customers, company, criteria.PaperSetting, employeeMap, null);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, customers.Count, new List<FormattedDocument> { document });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染客戶詳細資料時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region CustomerRosterCriteria 批次報表

        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(CustomerRosterCriteria criteria)
        {
            try
            {
                var customers = await GetCustomersByTypedCriteriaAsync(criteria);
                customers = customers.ExcludeDrafts();
                if (!customers.Any())
                    return BatchPreviewResult.Failure($"無符合條件的客戶\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var employeeMap = await GetEmployeeMapAsync();
                var document = await BuildCustomerDetailDocumentAsync(customers, company, criteria.PaperSetting, employeeMap, null);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, customers.Count, new List<FormattedDocument> { document });
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
                results = results.Where(c => c.EmployeeId.HasValue &&
                    criteria.RelatedEntityIds.Contains(c.EmployeeId.Value)).ToList();

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
                results = results.Where(c => c.EmployeeId.HasValue &&
                    criteria.EmployeeIds.Contains(c.EmployeeId.Value)).ToList();

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
        /// selectedSectionKeys 為 null 時包含所有基本區段（批次列印用）
        /// </summary>
        private async Task<FormattedDocument> BuildCustomerDetailDocumentAsync(
            List<Customer> customers,
            Company? company,
            PaperSetting? paperSetting,
            Dictionary<int, string> employeeMap,
            List<string>? selectedSectionKeys)
        {
            // null 表示批次列印，預設只含基本資料
            var sections = selectedSectionKeys ?? new List<string> { SectionKeys.BasicInfo };

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

                // 基本資料（始終包含）
                if (sections.Contains(SectionKeys.BasicInfo))
                    BuildBasicInfoSection(doc, customer, employeeMap);

                // 銀行帳戶
                if (sections.Contains(SectionKeys.BankAccounts))
                    await BuildBankAccountSection(doc, customer.Id);

                // 客戶品項
                if (sections.Contains(SectionKeys.CustomerItems))
                    await BuildCustomerItemSection(doc, customer.Id);

                // 拜訪紀錄
                if (sections.Contains(SectionKeys.VisitRecords))
                    await BuildVisitRecordSection(doc, customer.Id);

                // 客訴紀錄
                if (sections.Contains(SectionKeys.ComplaintRecords))
                    await BuildComplaintRecordSection(doc, customer.Id);

                // 車輛資訊
                if (sections.Contains(SectionKeys.Vehicles))
                    await BuildVehicleSection(doc, customer.Id);

                // 名片資料
                if (sections.Contains(SectionKeys.BusinessCards))
                    await BuildBusinessCardSection(doc, customer.Id);
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

        #region 私有方法 - 各區段建構

        private void BuildBasicInfoSection(FormattedDocument doc, Customer customer, Dictionary<int, string> employeeMap)
        {
            var employeeName = customer.EmployeeId.HasValue && employeeMap.ContainsKey(customer.EmployeeId.Value)
                ? employeeMap[customer.EmployeeId.Value] : "";

            var phone = !string.IsNullOrEmpty(customer.CompanyContactPhone)
                ? customer.CompanyContactPhone
                : customer.ContactPhone ?? "";

            doc.AddKeyValueRow(
                ("客戶編號", customer.Code ?? ""),
                ("公司名稱", customer.CompanyName ?? ""));

            doc.AddKeyValueRow(
                ("統一編號", customer.TaxNumber ?? ""),
                ("負責人", customer.ResponsiblePerson ?? ""));

            doc.AddKeyValueRow(
                ("聯絡人", customer.ContactPerson ?? ""),
                ("職稱", customer.JobTitle ?? ""));

            doc.AddKeyValueRow(
                ("公司電話", phone),
                ("行動電話", customer.MobilePhone ?? ""));

            doc.AddKeyValueRow(
                ("傳真", customer.Fax ?? ""),
                ("信箱", customer.Email ?? ""));

            doc.AddKeyValueRow(
                ("公司網址", customer.Website ?? ""),
                ("業務負責人", employeeName));

            doc.AddKeyValueRow(
                ("聯絡地址", customer.ContactAddress ?? ""),
                ("貨運地址", customer.ShippingAddress ?? ""));

            doc.AddKeyValueRow(
                ("付款方式", customer.PaymentMethod?.Name ?? ""),
                ("付款條件", customer.PaymentTerms ?? ""));

            doc.AddKeyValueRow(
                ("收款日期", customer.PaymentDate.HasValue ? $"{customer.PaymentDate}日" : ""),
                ("信用額度", customer.CreditLimit.HasValue ? customer.CreditLimit.Value.ToString("N0") : ""));

            doc.AddKeyValueRow(
                ("發票抬頭", customer.InvoiceTitle ?? ""),
                ("", ""));

            if (!string.IsNullOrEmpty(customer.Remarks))
            {
                doc.AddKeyValueRow(
                    ("備註", customer.Remarks),
                    ("", ""));
            }
        }

        private async Task BuildBankAccountSection(FormattedDocument doc, int customerId)
        {
            var accounts = await _bankAccountService.GetByCustomerIdAsync(customerId);
            if (!accounts.Any()) return;

            doc.AddSpacing(8);
            doc.AddText("【銀行帳戶】", fontSize: 10, bold: true);
            doc.AddSpacing(3);

            doc.AddTable(table =>
            {
                table.AddColumn("銀行名稱", 1.2f, TextAlignment.Left)
                     .AddColumn("分行", 1.0f, TextAlignment.Left)
                     .AddColumn("帳號", 1.5f, TextAlignment.Left)
                     .AddColumn("戶名", 1.0f, TextAlignment.Left)
                     .AddColumn("主要", 0.5f, TextAlignment.Center)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var acct in accounts)
                {
                    table.AddRow(
                        acct.Bank?.BankName ?? "",
                        acct.BranchName ?? "",
                        acct.AccountNumber ?? "",
                        acct.AccountName ?? "",
                        acct.IsPrimary ? "✓" : "");
                }
            });
        }

        private async Task BuildCustomerItemSection(FormattedDocument doc, int customerId)
        {
            var items = await _itemCustomerService.GetByCustomerIdAsync(customerId);
            if (!items.Any()) return;

            doc.AddSpacing(8);
            doc.AddText("【客戶品項】", fontSize: 10, bold: true);
            doc.AddSpacing(3);

            doc.AddTable(table =>
            {
                table.AddColumn("品項編號", 1.0f, TextAlignment.Left)
                     .AddColumn("品項名稱", 1.8f, TextAlignment.Left)
                     .AddColumn("客戶料號", 1.0f, TextAlignment.Left)
                     .AddColumn("客戶單價", 0.8f, TextAlignment.Right)
                     .AddColumn("最近售價", 0.8f, TextAlignment.Right)
                     .AddColumn("最近銷售日", 0.8f, TextAlignment.Center)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var item in items)
                {
                    table.AddRow(
                        item.Item?.Code ?? "",
                        item.Item?.Name ?? "",
                        item.CustomerItemCode ?? "",
                        item.CustomerPrice.HasValue ? NumberFormatHelper.FormatSmart(item.CustomerPrice.Value) : "",
                        item.LastSalePrice.HasValue ? NumberFormatHelper.FormatSmart(item.LastSalePrice.Value) : "",
                        item.LastSaleDate?.ToString("yyyy/MM/dd") ?? "");
                }
            });
        }

        private async Task BuildVisitRecordSection(FormattedDocument doc, int customerId)
        {
            var visits = await _visitService.GetByCustomerAsync(customerId);
            if (!visits.Any()) return;

            // 只取最近 20 筆
            var recentVisits = visits.OrderByDescending(v => v.VisitDate).Take(20).ToList();

            doc.AddSpacing(8);
            doc.AddText($"【拜訪紀錄】（最近 {recentVisits.Count} 筆）", fontSize: 10, bold: true);
            doc.AddSpacing(3);

            doc.AddTable(table =>
            {
                table.AddColumn("日期", 0.8f, TextAlignment.Center)
                     .AddColumn("拜訪類型", 0.6f, TextAlignment.Center)
                     .AddColumn("目的", 1.2f, TextAlignment.Left)
                     .AddColumn("內容", 2.5f, TextAlignment.Left)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var visit in recentVisits)
                {
                    table.AddRow(
                        visit.VisitDate.ToString("yyyy/MM/dd"),
                        ERPCore2.Helpers.EnumHelper.GetDisplayName(visit.VisitType),
                        TruncateText(visit.Purpose ?? "", 30),
                        TruncateText(visit.Content ?? "", 50));
                }
            });
        }

        private async Task BuildComplaintRecordSection(FormattedDocument doc, int customerId)
        {
            var complaints = await _complaintService.GetByCustomerAsync(customerId);
            if (!complaints.Any()) return;

            var recentComplaints = complaints.OrderByDescending(c => c.ComplaintDate).Take(20).ToList();

            doc.AddSpacing(8);
            doc.AddText($"【客訴紀錄】（最近 {recentComplaints.Count} 筆）", fontSize: 10, bold: true);
            doc.AddSpacing(3);

            doc.AddTable(table =>
            {
                table.AddColumn("日期", 0.8f, TextAlignment.Center)
                     .AddColumn("客訴內容", 2.5f, TextAlignment.Left)
                     .AddColumn("處理方式", 2.0f, TextAlignment.Left)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var c in recentComplaints)
                {
                    table.AddRow(
                        c.ComplaintDate.ToString("yyyy/MM/dd"),
                        TruncateText(c.Description ?? "", 50),
                        TruncateText(c.Resolution ?? "", 40));
                }
            });
        }

        private async Task BuildVehicleSection(FormattedDocument doc, int customerId)
        {
            var vehicles = await _vehicleService.GetByCustomerAsync(customerId);
            if (!vehicles.Any()) return;

            doc.AddSpacing(8);
            doc.AddText("【車輛資訊】", fontSize: 10, bold: true);
            doc.AddSpacing(3);

            doc.AddTable(table =>
            {
                table.AddColumn("車牌號碼", 1.0f, TextAlignment.Left)
                     .AddColumn("車輛類型", 0.8f, TextAlignment.Left)
                     .AddColumn("品牌/型號", 1.2f, TextAlignment.Left)
                     .AddColumn("備註", 1.5f, TextAlignment.Left)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var v in vehicles)
                {
                    table.AddRow(
                        v.LicensePlate ?? "",
                        v.VehicleType?.Name ?? "",
                        $"{v.Brand} {v.Model}".Trim(),
                        TruncateText(v.Remarks ?? "", 30));
                }
            });
        }

        private async Task BuildBusinessCardSection(FormattedDocument doc, int customerId)
        {
            var cards = await _businessCardService.GetByOwnerAsync("Customer", customerId);
            if (!cards.Any()) return;

            doc.AddSpacing(8);
            doc.AddText("【名片資料】", fontSize: 10, bold: true);
            doc.AddSpacing(3);

            doc.AddTable(table =>
            {
                table.AddColumn("姓名", 1.5f, TextAlignment.Left)
                     .AddColumn("職稱", 1.5f, TextAlignment.Left)
                     .ShowBorder(false)
                     .ShowHeaderBackground(false)
                     .ShowHeaderSeparator(false)
                     .SetRowHeight(18);

                foreach (var card in cards)
                {
                    table.AddRow(
                        card.ContactPersonName ?? "",
                        card.JobTitle ?? "");
                }
            });
        }

        #endregion

        #region 私有方法 - 工具

        private static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength) return text;
            return text[..(maxLength - 1)] + "…";
        }

        #endregion
    }
}
