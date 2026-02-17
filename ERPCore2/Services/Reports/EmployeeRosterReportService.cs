using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
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
    /// 員工名冊表報表服務實作 - 使用 FormattedDocument 進行圖形化渲染
    /// 支援單筆列印和清單式批次列印
    /// </summary>
    public class EmployeeRosterReportService : IEmployeeRosterReportService
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;
        private readonly IFormattedPrintService _formattedPrintService;
        private readonly ILogger<EmployeeRosterReportService>? _logger;

        public EmployeeRosterReportService(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            ICompanyService companyService,
            IFormattedPrintService formattedPrintService,
            ILogger<EmployeeRosterReportService>? logger = null)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _companyService = companyService;
            _formattedPrintService = formattedPrintService;
            _logger = logger;
        }

        #region IEntityReportService 實作

        /// <summary>
        /// 生成單一員工資料報表
        /// </summary>
        public async Task<FormattedDocument> GenerateReportAsync(int employeeId)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (employee == null)
            {
                throw new ArgumentException($"找不到員工 ID: {employeeId}");
            }

            var company = await _companyService.GetPrimaryCompanyAsync();
            return BuildSingleEmployeeDocument(employee, company);
        }

        /// <summary>
        /// 將報表渲染為圖片（預設紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int employeeId)
        {
            var document = await GenerateReportAsync(employeeId);
            return _formattedPrintService.RenderToImages(document);
        }

        /// <summary>
        /// 將報表渲染為圖片（指定紙張）
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<List<byte[]>> RenderToImagesAsync(int employeeId, PaperSetting paperSetting)
        {
            var document = await GenerateReportAsync(employeeId);
            return _formattedPrintService.RenderToImages(document, paperSetting);
        }

        /// <summary>
        /// 直接列印員工資料
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
                _logger?.LogError(ex, "列印員工資料 {EmployeeId} 時發生錯誤", employeeId);
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
                var employees = await GetEmployeesByCriteriaAsync(criteria);

                if (!employees.Any())
                {
                    return ServiceResult.Failure($"無符合條件的員工\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildEmployeeRosterDocument(employees, company, null);
                return await _formattedPrintService.PrintByReportIdAsync(document, reportId, 1);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次列印員工名冊表時發生錯誤");
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
                var employees = await GetEmployeesByCriteriaAsync(criteria);

                if (!employees.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的員工\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildEmployeeRosterDocument(employees, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, employees.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染員工名冊表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region EmployeeRosterCriteria 批次報表

        /// <summary>
        /// 以員工名冊專用篩選條件渲染報表為圖片
        /// </summary>
        [SupportedOSPlatform("windows6.1")]
        public async Task<BatchPreviewResult> RenderBatchToImagesAsync(EmployeeRosterCriteria criteria)
        {
            try
            {
                var employees = await GetEmployeesByTypedCriteriaAsync(criteria);

                if (!employees.Any())
                {
                    return BatchPreviewResult.Failure($"無符合條件的員工\n篩選條件：{criteria.GetSummary()}");
                }

                var company = await _companyService.GetPrimaryCompanyAsync();
                var document = BuildEmployeeRosterDocument(employees, company, criteria.PaperSetting);
                var images = criteria.PaperSetting != null
                    ? _formattedPrintService.RenderToImages(document, criteria.PaperSetting)
                    : _formattedPrintService.RenderToImages(document);

                return BatchPreviewResult.Success(images, document, employees.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "批次渲染員工名冊表時發生錯誤");
                return BatchPreviewResult.Failure($"產生預覽時發生錯誤: {ex.Message}");
            }
        }

        #endregion

        #region 私有方法 - 查詢資料

        /// <summary>
        /// 根據 BatchPrintCriteria 查詢員工
        /// </summary>
        private async Task<List<Employee>> GetEmployeesByCriteriaAsync(BatchPrintCriteria criteria)
        {
            var results = await _employeeService.GetAllAsync();

            // 排除超級管理員（系統特殊身分，不列入名冊）
            results = results.Where(e => !e.IsSuperAdmin).ToList();

            // 篩選部門 ID
            if (criteria.RelatedEntityIds.Any())
            {
                results = results.Where(e => e.DepartmentId.HasValue &&
                    criteria.RelatedEntityIds.Contains(e.DepartmentId.Value)).ToList();
            }

            // 關鍵字搜尋（員工編號、姓名）
            if (!string.IsNullOrEmpty(criteria.DocumentNumberKeyword))
            {
                var keyword = criteria.DocumentNumberKeyword;
                results = results.Where(e =>
                    (e.Code != null && e.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Name != null && e.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // 排除已停用
            if (!criteria.IncludeCancelled)
            {
                results = results.Where(e => e.EmploymentStatus != EmployeeStatus.Inactive &&
                                              e.EmploymentStatus != EmployeeStatus.Resigned).ToList();
            }

            return results.OrderBy(e => e.Code).ToList();
        }

        /// <summary>
        /// 根據 EmployeeRosterCriteria 查詢員工
        /// </summary>
        private async Task<List<Employee>> GetEmployeesByTypedCriteriaAsync(EmployeeRosterCriteria criteria)
        {
            var results = await _employeeService.GetAllAsync();

            // 排除超級管理員（系統特殊身分，不列入名冊）
            results = results.Where(e => !e.IsSuperAdmin).ToList();

            // 僅在職員工
            if (criteria.ActiveOnly)
            {
                results = results.Where(e => e.EmploymentStatus == EmployeeStatus.Active ||
                                              e.EmploymentStatus == EmployeeStatus.Probation).ToList();
            }

            // 篩選指定員工
            if (criteria.EmployeeIds.Any())
            {
                results = results.Where(e => criteria.EmployeeIds.Contains(e.Id)).ToList();
            }

            // 篩選部門
            if (criteria.DepartmentIds.Any())
            {
                results = results.Where(e => e.DepartmentId.HasValue &&
                    criteria.DepartmentIds.Contains(e.DepartmentId.Value)).ToList();
            }

            // 篩選職位
            if (criteria.PositionIds.Any())
            {
                results = results.Where(e => e.EmployeePositionId.HasValue &&
                    criteria.PositionIds.Contains(e.EmployeePositionId.Value)).ToList();
            }

            // 篩選在職狀態
            if (criteria.EmploymentStatuses.Any())
            {
                results = results.Where(e => criteria.EmploymentStatuses.Contains(e.EmploymentStatus)).ToList();
            }

            // 篩選權限組
            if (criteria.RoleIds.Any())
            {
                results = results.Where(e => e.RoleId.HasValue &&
                    criteria.RoleIds.Contains(e.RoleId.Value)).ToList();
            }

            // 篩選到職日期
            if (criteria.HireDateStart.HasValue)
            {
                results = results.Where(e => e.HireDate.HasValue &&
                    e.HireDate.Value.Date >= criteria.HireDateStart.Value.Date).ToList();
            }
            if (criteria.HireDateEnd.HasValue)
            {
                results = results.Where(e => e.HireDate.HasValue &&
                    e.HireDate.Value.Date <= criteria.HireDateEnd.Value.Date).ToList();
            }

            // 篩選離職日期
            if (criteria.ResignationDateStart.HasValue)
            {
                results = results.Where(e => e.ResignationDate.HasValue &&
                    e.ResignationDate.Value.Date >= criteria.ResignationDateStart.Value.Date).ToList();
            }
            if (criteria.ResignationDateEnd.HasValue)
            {
                results = results.Where(e => e.ResignationDate.HasValue &&
                    e.ResignationDate.Value.Date <= criteria.ResignationDateEnd.Value.Date).ToList();
            }

            // 篩選生日
            if (criteria.BirthDateStart.HasValue)
            {
                results = results.Where(e => e.BirthDate.HasValue &&
                    e.BirthDate.Value.Date >= criteria.BirthDateStart.Value.Date).ToList();
            }
            if (criteria.BirthDateEnd.HasValue)
            {
                results = results.Where(e => e.BirthDate.HasValue &&
                    e.BirthDate.Value.Date <= criteria.BirthDateEnd.Value.Date).ToList();
            }

            // 關鍵字搜尋
            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var keyword = criteria.Keyword;
                results = results.Where(e =>
                    (e.Code != null && e.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Name != null && e.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (e.Account != null && e.Account.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            return results.OrderBy(e => e.Department?.Name).ThenBy(e => e.Code).ToList();
        }

        #endregion

        #region 私有方法 - 建構報表文件

        /// <summary>
        /// 建構員工名冊表（清單式：依部門分組）
        /// </summary>
        private FormattedDocument BuildEmployeeRosterDocument(
            List<Employee> employees,
            Company? company,
            PaperSetting? paperSetting)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"員工名冊表-{DateTime.Now:yyyyMMdd}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("員 工 名 冊 表", 16f, true)
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

            // === 依部門分組顯示 ===
            var departmentGroups = employees
                .GroupBy(e => e.Department?.Name ?? "未分配部門")
                .OrderBy(g => g.Key);

            foreach (var group in departmentGroups)
            {
                // 部門標題
                doc.AddKeyValueRow(
                    ("部門", $"{group.Key}（{group.Count()} 人）"));

                doc.AddSpacing(3);

                // 員工資料表格
                doc.AddTable(table =>
                {
                    table.AddColumn("項次", 0.30f, Models.Reports.TextAlignment.Center)
                         .AddColumn("員工編號", 0.65f, Models.Reports.TextAlignment.Left)
                         .AddColumn("姓名", 0.70f, Models.Reports.TextAlignment.Left)
                         .AddColumn("職位", 0.70f, Models.Reports.TextAlignment.Left)
                         .AddColumn("性別", 0.35f, Models.Reports.TextAlignment.Center)
                         .AddColumn("到職日期", 0.70f, Models.Reports.TextAlignment.Center)
                         .AddColumn("在職狀態", 0.55f, Models.Reports.TextAlignment.Center)
                         .AddColumn("手機", 0.80f, Models.Reports.TextAlignment.Left)
                         .AddColumn("Email", 1.20f, Models.Reports.TextAlignment.Left)
                         .ShowBorder(false)
                         .ShowHeaderBackground(false)
                         .ShowHeaderSeparator(false)
                         .SetRowHeight(20);

                    int rowNum = 1;
                    foreach (var employee in group.OrderBy(e => e.Code))
                    {
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
                            EmployeeStatus.LeaveOfAbsence => "留停",
                            EmployeeStatus.Resigned => "已離職",
                            EmployeeStatus.Inactive => "停用",
                            _ => ""
                        };

                        table.AddRow(
                            rowNum.ToString(),
                            employee.Code ?? "",
                            employee.Name ?? "",
                            employee.EmployeePosition?.Name ?? "",
                            genderText,
                            employee.HireDate?.ToString("yyyy/MM/dd") ?? "",
                            statusText,
                            employee.Mobile ?? "",
                            employee.Email ?? ""
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

                var summaryLines = new List<string>
                {
                    $"員工總數：{employees.Count} 人"
                };

                // 按部門統計
                foreach (var group in departmentGroups)
                {
                    summaryLines.Add($"  {group.Key}：{group.Count()} 人");
                }

                // 按狀態統計
                var statusGroups = employees
                    .GroupBy(e => e.EmploymentStatus)
                    .OrderBy(g => g.Key);

                summaryLines.Add("");
                foreach (var sg in statusGroups)
                {
                    var statusName = sg.Key switch
                    {
                        EmployeeStatus.Probation => "試用期",
                        EmployeeStatus.Active => "在職",
                        EmployeeStatus.LeaveOfAbsence => "留職停薪",
                        EmployeeStatus.Resigned => "已離職",
                        EmployeeStatus.Inactive => "停用",
                        _ => "其他"
                    };
                    summaryLines.Add($"  {statusName}：{sg.Count()} 人");
                }

                footer.AddTwoColumnSection(
                    leftContent: summaryLines,
                    leftTitle: null,
                    leftHasBorder: false,
                    rightContent: new List<string>(),
                    leftWidthRatio: 0.6f);

                footer.AddSpacing(20)
                      .AddSignatureSection(new[] { "製表人員", "人事主管" });
            });

            return doc;
        }

        /// <summary>
        /// 建構單一員工資料報表
        /// </summary>
        private FormattedDocument BuildSingleEmployeeDocument(Employee employee, Company? company)
        {
            var doc = new FormattedDocument()
                .SetDocumentName($"員工資料-{employee.Code}")
                .SetMargins(0.8f, 0.3f, 0.8f, 0.3f);

            // === 頁首區 ===
            doc.BeginHeader(header =>
            {
                header.AddReportHeaderBlock(
                    centerLines: new List<(string, float, bool)>
                    {
                        (company?.CompanyName ?? "公司名稱", 14f, true),
                        ("員 工 資 料", 16f, true)
                    },
                    rightLines: new List<string>
                    {
                        $"列印日期：{DateTime.Now:yyyy/MM/dd}",
                        $"頁次：{{PAGE}}/{{PAGES}}"
                    },
                    rightFontSize: 10f);

                header.AddSpacing(5);

                // 員工基本資訊
                header.AddKeyValueRow(
                    ("員工編號", employee.Code ?? ""),
                    ("姓名", employee.Name ?? ""));

                header.AddKeyValueRow(
                    ("部門", employee.Department?.Name ?? ""),
                    ("職位", employee.EmployeePosition?.Name ?? ""));

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

                header.AddKeyValueRow(
                    ("性別", genderText),
                    ("在職狀態", statusText));

                header.AddKeyValueRow(
                    ("到職日期", employee.HireDate?.ToString("yyyy/MM/dd") ?? ""),
                    ("離職日期", employee.ResignationDate?.ToString("yyyy/MM/dd") ?? ""));

                header.AddKeyValueRow(
                    ("手機", employee.Mobile ?? ""),
                    ("Email", employee.Email ?? ""));

                header.AddKeyValueRow(
                    ("緊急聯絡人", employee.EmergencyContact ?? ""),
                    ("緊急聯絡電話", employee.EmergencyPhone ?? ""));

                header.AddSpacing(5);
            });

            // === 頁尾區 ===
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
