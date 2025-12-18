using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Components.Shared.Forms;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 領貨欄位配置
    /// </summary>
    public class MaterialIssueFieldConfiguration : BaseFieldConfiguration<MaterialIssue>
    {
        private readonly List<Employee> _employees;
        private readonly List<Department> _departments;
        private readonly INotificationService? _notificationService;

        public MaterialIssueFieldConfiguration(
            List<Employee> employees,
            List<Department> departments,
            INotificationService? notificationService = null)
        {
            _employees = employees;
            _departments = departments;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<MaterialIssue>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<MaterialIssue>>
                {
                    {
                        nameof(MaterialIssue.Code),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = nameof(MaterialIssue.Code),
                            DisplayName = "領貨單號",
                            FilterPlaceholder = "輸入領貨單號搜尋",
                            TableOrder = 1,
                            HeaderStyle = "width: 150px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(MaterialIssue.Code), mi => mi.Code)
                        }
                    },
                    {
                        nameof(MaterialIssue.IssueDate),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = nameof(MaterialIssue.IssueDate),
                            DisplayName = "領貨日期",
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 2,
                            HeaderStyle = "width: 120px;",
                            FilterFunction = (model, query) => FilterHelper.ApplyDateRangeFilter(
                                model, query, nameof(MaterialIssue.IssueDate), mi => mi.IssueDate)
                        }
                    },
                    {
                        nameof(MaterialIssue.EmployeeId),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = "Employee.Name",
                            FilterPropertyName = nameof(MaterialIssue.EmployeeId),
                            DisplayName = "領料人員",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            HeaderStyle = "width: 120px;",
                            Options = _employees.Select(e => new SelectOption
                            {
                                Text = e.Name ?? "",
                                Value = e.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(MaterialIssue.EmployeeId), mi => mi.EmployeeId)
                        }
                    },
                    {
                        nameof(MaterialIssue.DepartmentId),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = "Department.Name",
                            FilterPropertyName = nameof(MaterialIssue.DepartmentId),
                            DisplayName = "領料部門",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            HeaderStyle = "width: 120px;",
                            Options = _departments.Select(d => new SelectOption
                            {
                                Text = d.Name ?? "",
                                Value = d.Id.ToString()
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(MaterialIssue.DepartmentId), mi => mi.DepartmentId)
                        }
                    },
                    {
                        nameof(MaterialIssue.TotalQuantity),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = nameof(MaterialIssue.TotalQuantity),
                            DisplayName = "總數量",
                            TableOrder = 5,
                            ShowInFilter = false,
                            HeaderStyle = "width: 100px; text-align: right;"
                        }
                    },
                    {
                        nameof(MaterialIssue.DetailCount),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = nameof(MaterialIssue.DetailCount),
                            DisplayName = "明細筆數",
                            TableOrder = 6,
                            ShowInFilter = false,
                            HeaderStyle = "width: 100px; text-align: right;"
                        }
                    },
                    {
                        nameof(MaterialIssue.Remarks),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = nameof(MaterialIssue.Remarks),
                            DisplayName = "備註",
                            FilterPlaceholder = "輸入備註搜尋",
                            TableOrder = 7,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(MaterialIssue.Remarks), mi => mi.Remarks, allowNull: true)
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[MaterialIssueFieldConfiguration] GetFieldDefinitions 發生錯誤: {ex.Message}");
                
                _ = Task.Run(async () =>
                {
                    if (_notificationService != null)
                    {
                        await _notificationService.ShowErrorAsync("載入領貨欄位配置時發生錯誤");
                    }
                });

                return new Dictionary<string, FieldDefinition<MaterialIssue>>();
            }
        }
    }
}
