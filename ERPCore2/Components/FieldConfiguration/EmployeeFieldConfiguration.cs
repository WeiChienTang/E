using ERPCore2.Data.Entities;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared;
using ERPCore2.Components.Shared.Forms;
using Microsoft.AspNetCore.Components;
using ERPCore2.Services;

namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 員工欄位配置
    /// </summary>
    public class EmployeeFieldConfiguration : BaseFieldConfiguration<Employee>
    {
        private readonly List<Department> _departments;
        private readonly List<Role> _roles;
        private readonly List<EmployeePosition> _employeePositions;
        private readonly INotificationService? _notificationService;

        public EmployeeFieldConfiguration(List<Department> departments, List<Role> roles, List<EmployeePosition> employeePositions, INotificationService? notificationService = null)
        {
            _departments = departments;
            _roles = roles;
            _employeePositions = employeePositions;
            _notificationService = notificationService;
        }

        public override Dictionary<string, FieldDefinition<Employee>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Employee>>
                {
                    {
                        nameof(Employee.Code),
                        new FieldDefinition<Employee>
                        {
                            PropertyName = nameof(Employee.Code),
                            DisplayName = "員工代碼",
                            FilterPlaceholder = "輸入員工代碼搜尋",
                            TableOrder = 1,
                            FilterOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Employee.Code), e => e.Code)
                        }
                    },
                    {
                        "FullName",
                        new FieldDefinition<Employee>
                        {
                            PropertyName = "FullName",
                            DisplayName = "姓名",
                            FilterPlaceholder = "輸入姓名搜尋",
                            TableOrder = 2,
                            FilterOrder = 2,                            
                            FilterFunction = (model, query) => {
                                var fullNameFilter = model.GetFilterValue("FullName")?.ToString();
                                if (!string.IsNullOrWhiteSpace(fullNameFilter))
                                {
                                    // 搜尋名字
                                    query = query.Where(e => 
                                        (e.Name != null && e.Name.Contains(fullNameFilter)));
                                }
                                return query;
                            },
                            CustomTemplate = item => builder =>
                            {
                                var employee = (Employee)item;
                                var name = employee.Name ?? "";
                                builder.OpenElement(0, "span");
                                builder.AddContent(1, name.Trim());
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        "Department",
                        new FieldDefinition<Employee>
                        {
                            PropertyName = "Department.Name",
                            FilterPropertyName = nameof(Employee.DepartmentId),
                            DisplayName = "部門",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 3,
                            FilterOrder = 3,
                            NullDisplayText = "未指派",
                            Options = _departments.Select(d => new SelectOption 
                            { 
                                Text = d.Name, 
                                Value = d.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Employee.DepartmentId), e => e.DepartmentId)
                        }
                    },
                    {
                        "EmployeePosition",
                        new FieldDefinition<Employee>
                        {
                            PropertyName = "EmployeePosition.Name",
                            FilterPropertyName = nameof(Employee.EmployeePositionId),
                            DisplayName = "職位",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 4,
                            FilterOrder = 4,
                            NullDisplayText = "未指派",
                            Options = _employeePositions.Select(p => new SelectOption 
                            { 
                                Text = p.Name, 
                                Value = p.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Employee.EmployeePositionId), e => e.EmployeePositionId)
                        }
                    },
                    {
                        "Role",
                        new FieldDefinition<Employee>
                        {
                            PropertyName = "Role.Name",
                            FilterPropertyName = nameof(Employee.RoleId),
                            DisplayName = "角色",
                            FilterType = SearchFilterType.Select,
                            TableOrder = 5,
                            FilterOrder = 5,
                            NullDisplayText = "未指派",
                            Options = _roles.Select(r => new SelectOption 
                            { 
                                Text = r.Name, 
                                Value = r.Id.ToString() 
                            }).ToList(),
                            FilterFunction = (model, query) => FilterHelper.ApplyNullableIntIdFilter(
                                model, query, nameof(Employee.RoleId), e => e.RoleId)
                        }
                    },
                    {
                        "IsSystemUser",
                        new FieldDefinition<Employee>
                        {
                            PropertyName = "IsSystemUser",
                            DisplayName = "系統操控",
                            ShowInFilter = false,
                            TableOrder = 6,
                            HeaderStyle = "width: 90px;",
                            CustomTemplate = item => builder =>
                            {
                                var employee = (Employee)item;
                                builder.OpenElement(0, "span");
                                builder.AddAttribute(1, "class", "badge text-white");
                                builder.AddAttribute(2, "style", employee.IsSystemUser ? "background-color: #28a745;" : "background-color: #dc3545;");
                                builder.AddContent(3, employee.IsSystemUser ? "有權限" : "無權限");
                                builder.CloseElement();
                            }
                        }
                    },
                    {
                        "LastLoginAt",
                        new FieldDefinition<Employee>
                        {
                            PropertyName = "LastLoginAt",
                            DisplayName = "最後登入",
                            ShowInFilter = false,
                            TableOrder = 7,
                            NullDisplayText = "從未登入"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化員工欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化員工欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Employee>>();
            }
        }
    }
}
