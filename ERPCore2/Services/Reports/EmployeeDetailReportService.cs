using ERPCore2.Data.Entities;
using ERPCore2.Models;
using ERPCore2.Models.Reports;
using ERPCore2.Models.Reports.FilterCriteria;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Services;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Services.Reports
{
    /// <summary>
    /// 員工詳細資料報表服務實作（HR002）
    /// 每位員工各佔一區塊，以 key-value 方式顯示完整聯絡與任職資訊
    /// 支援單筆（EditModal）和批次（報表集 / Alt+R）兩種進入路徑
    /// </summary>
    public class EmployeeDetailReportService : IEmployeeDetailReportService
    {
        private readonly IEmployeeService _employeeService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<EmployeeDetailReportService>? _logger;

        public EmployeeDetailReportService(
            IEmployeeService employeeService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<EmployeeDetailReportService>? logger = null)
        {
            _employeeService = employeeService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一員工詳細資料報表（供 EditModal 使用）
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int employeeId)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
                throw new ArgumentException($"找不到員工 ID: {employeeId}");

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildEmployeeDetailDocument(new List<Employee> { employee }, company, null);
        }

        /// <summary>
        /// 將單筆員工報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int employeeId)
        {
            var document = await GenerateReportAsync(employeeId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將單筆員工報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int employeeId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(employeeId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印單筆員工詳細資料
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<ServiceResult> DirectPrintAsync(int employeeId, string reportId, int copies = 1)
        {
            try
            {
                var document = await GenerateReportAsync(employeeId);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, copies);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "列印員工詳細資料 {EmployeeId} 時發生錯誤", employeeId);
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
                var employees = await GetEmployeesByCriteriaAsync(criteria);
                if (!employees.Any())
                    return ServiceResult.Failure($"無符合條件的員工\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildEmployeeDetailDocument(employees, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印員工詳細資料時發生錯誤");
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
                var employees = await GetEmployeesByCriteriaAsync(criteria);
                if (!employees.Any())
                    return BatchPreviewResult.Failure($"無符合條件的員工\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildEmployeeDetailDocument(employees, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, employees.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染員工詳細資料時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region EmployeeRosterCriteria 批次報表

        /// <summary>
        /// 以員工名冊篩選條件批次渲染詳細格式報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(EmployeeRosterCriteria criteria)
        {
            try
            {
                var employees = await GetEmployeesByTypedCriteriaAsync(criteria);
                if (!employees.Any())
                    return BatchPreviewResult.Failure($"無符合條件的員工\n篩選條件：{criteria.GetSummary()}");

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildEmployeeDetailDocument(employees, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, employees.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染員工詳細資料時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        private async Task<List<Employee>> GetEmployeesByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _employeeService.GetAllAsync();
            results = results.Where(e => !e.IsSuperAdmin).ToList();

            if (criteria.RelatedEntityIds.Any())
                results = results.Where(e => e.DepartmentId.HasValue &&
                    criteria.RelatedEntityIds.Contains(e.DepartmentId.Value)).ToList();

            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(e =>
                    (e.Code != null && e.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Name != null && e.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (!criteria.IncludeCancelled)
                results = results.Where(e => e.EmploymentStatus != EmployeeStatus.Inactive &&
                                              e.EmploymentStatus != EmployeeStatus.Resigned).ToList();

            return results.OrderBy(e => e.Code).ToList();
        }

        private async Task<List<Employee>> GetEmployeesByTypedCriteriaAsync(EmployeeRosterCriteria criteria)
        {
            var results = await _employeeService.GetAllAsync();
            results = results.Where(e => !e.IsSuperAdmin).ToList();

            if (criteria.ActiveOnly)
                results = results.Where(e => e.EmploymentStatus == EmployeeStatus.Active ||
                                              e.EmploymentStatus == EmployeeStatus.Probation).ToList();

            if (criteria.EmployeeIds.Any())
                results = results.Where(e => criteria.EmployeeIds.Contains(e.Id)).ToList();

            if (criteria.DepartmentIds.Any())
                results = results.Where(e => e.DepartmentId.HasValue &&
                    criteria.DepartmentIds.Contains(e.DepartmentId.Value)).ToList();

            if (criteria.PositionIds.Any())
                results = results.Where(e => e.EmployeePositionId.HasValue &&
                    criteria.PositionIds.Contains(e.EmployeePositionId.Value)).ToList();

            if (criteria.EmploymentStatuses.Any())
                results = results.Where(e => criteria.EmploymentStatuses.Contains(e.EmploymentStatus)).ToList();

            if (criteria.RoleIds.Any())
                results = results.Where(e => e.RoleId.HasValue &&
                    criteria.RoleIds.Contains(e.RoleId.Value)).ToList();

            if (criteria.HireDateStart.HasValue)
                results = results.Where(e => e.HireDate.HasValue &&
                    e.HireDate.Value.Date >= criteria.HireDateStart.Value.Date).ToList();

            if (criteria.HireDateEnd.HasValue)
                results = results.Where(e => e.HireDate.HasValue &&
                    e.HireDate.Value.Date <= criteria.HireDateEnd.Value.Date).ToList();

            if (criteria.ResignationDateStart.HasValue)
                results = results.Where(e => e.ResignationDate.HasValue &&
                    e.ResignationDate.Value.Date >= criteria.ResignationDateStart.Value.Date).ToList();

            if (criteria.ResignationDateEnd.HasValue)
                results = results.Where(e => e.ResignationDate.HasValue &&
                    e.ResignationDate.Value.Date <= criteria.ResignationDateEnd.Value.Date).ToList();

            if (criteria.BirthDateStart.HasValue)
                results = results.Where(e => e.BirthDate.HasValue &&
                    e.BirthDate.Value.Date >= criteria.BirthDateStart.Value.Date).ToList();

            if (criteria.BirthDateEnd.HasValue)
                results = results.Where(e => e.BirthDate.HasValue &&
                    e.BirthDate.Value.Date <= criteria.BirthDateEnd.Value.Date).ToList();

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(e =>
                    (e.Code != null && e.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Name != null && e.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Account != null && e.Account.Contains(keyword, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return results.OrderBy(e => e.Department?.Name).ThenBy(e => e.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構員工詳細資料報表
        /// 每位員工以 key-value 區塊呈現，區塊間以分隔線隔開
        /// </summary>
        private FormattedDocument BuildEmployeeDetailDocument(
            List<Employee> employees,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"員工詳細資料-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區（每頁重複） ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("員 工 詳 細 資 料", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"人數：{employees.Count}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);
            });

            // === 員工資料區塊 ===
            bool isFirst = true;
            foreach (var employee in employees)
            {
                if (!isFirst)
                {
                    doc.AddSpacing(5);
                    doc.AddLine(Models.Reports.LineStyle.Solid, 0.5f);
                    doc.AddSpacing(5);
                }
                isFirst = false;

                var genderText = employee.Gender switch
                {
                    Gender.Male => "男",
                    Gender.Female => "女",
                    Gender.Other => "其他",
                    _ => ""
                };

                var statusText = employee.EmploymentStatus switch
                {
                    EmployeeStatus.Probation => "試用期",
                    EmployeeStatus.Active => "在職",
                    EmployeeStatus.LeaveOfAbsence => "留職停薪",
                    EmployeeStatus.Resigned => "已離職",
                    EmployeeStatus.Inactive => "停用",
                    _ => ""
                };

                // 基本資訊
                doc.AddKeyValueRow(
                    ("員工編號", employee.Code ?? ""),
                    ("姓名", employee.Name ?? ""));

                doc.AddKeyValueRow(
                    ("部門", employee.Department?.Name ?? ""),
                    ("職位", employee.EmployeePosition?.Name ?? ""));

                doc.AddKeyValueRow(
                    ("性別", genderText),
                    ("在職狀態", statusText));

                // 任職資訊
                doc.AddKeyValueRow(
                    ("到職日期", employee.HireDate?.ToString("yyyy/MM/dd") ?? ""),
                    ("離職日期", employee.ResignationDate?.ToString("yyyy/MM/dd") ?? ""));

                // 聯絡資訊
                doc.AddKeyValueRow(
                    ("手機", employee.Mobile ?? ""),
                    ("Email", employee.Email ?? ""));

                doc.AddKeyValueRow(
                    ("緊急聯絡人", employee.EmergencyContact ?? ""),
                    ("緊急電話", employee.EmergencyPhone ?? ""));
            }

            // === 頁尾區（最後一頁） ===
            doc.BeginFooter(footer =>
            {
                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "人事主管" });
            });

            return doc;
        }

        #endregion
    }
}
