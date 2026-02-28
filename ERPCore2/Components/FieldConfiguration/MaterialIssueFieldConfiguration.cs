using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Models;
using ERPCore2.Services;
using ERPCore2.Components.Shared.UI.Form;
using Microsoft.AspNetCore.Components;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
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
                            DisplayName = Dn("Field.MaterialIssueCode", "領貨單號"),
                            FilterPlaceholder = Fp("Field.MaterialIssueCode", "輸入領貨單號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(MaterialIssue.Code), mi => mi.Code)
                        }
                    },
                    {
                        nameof(MaterialIssue.IssueDate),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = nameof(MaterialIssue.IssueDate),
                            DisplayName = Dn("Field.MaterialIssueDate", "領貨日期"),
                            FilterType = SearchFilterType.DateRange,
                            TableOrder = 2,
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
                            DisplayName = Dn("Field.MaterialIssueStaff", "領料人員"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
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
                            DisplayName = Dn("Field.MaterialIssueDept", "領料部門"),
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
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
                            DisplayName = Dn("Field.TotalQuantity", "總數量"),
                            TableOrder = 5,
                            ShowInFilter = false,
                        }
                    },
                    {
                        nameof(MaterialIssue.DetailCount),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = nameof(MaterialIssue.DetailCount),
                            DisplayName = Dn("Field.DetailCount", "明細筆數"),
                            TableOrder = 6,
                            ShowInFilter = false,
                        }
                    },
                    {
                        nameof(MaterialIssue.Remarks),
                        new FieldDefinition<MaterialIssue>
                        {
                            PropertyName = nameof(MaterialIssue.Remarks),
                            DisplayName = Dn("Field.Remarks", "備註"),
                            FilterPlaceholder = Fp("Field.Remarks", "輸入備註搜尋"),
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


