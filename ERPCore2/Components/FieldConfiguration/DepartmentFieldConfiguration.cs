using ERPCore2.Components.Shared.UI.Form;
using ERPCore2.Data.Entities;
using ERPCore2.Services;
using ERPCore2.Helpers;
using ERPCore2.Components.Shared.Modal;
using ERPCore2.Components.Shared.Table;
using ERPCore2.Components.Shared.Page;
using ERPCore2.Components.Shared.Statistics;
namespace ERPCore2.FieldConfiguration
{
    /// <summary>
    /// 部門欄位配置
    /// </summary>
    public class DepartmentFieldConfiguration : BaseFieldConfiguration<Department>
    {
        private readonly INotificationService? _notificationService;
        
        public DepartmentFieldConfiguration(INotificationService? notificationService = null)
        {
            _notificationService = notificationService;
        }
        
        public override Dictionary<string, FieldDefinition<Department>> GetFieldDefinitions()
        {
            try
            {
                return new Dictionary<string, FieldDefinition<Department>>
                {
                    {
                        nameof(Department.Code),
                        new FieldDefinition<Department>
                        {
                            PropertyName = nameof(Department.Code),
                            DisplayName = Dn("Field.DepartmentCode", "部門編號"),
                            FilterPlaceholder = Fp("Field.DepartmentCode", "輸入部門編號搜尋"),
                            TableOrder = 1,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Department.Code), d => d.Code)
                        }
                    },
                    {
                        nameof(Department.Name),
                        new FieldDefinition<Department>
                        {
                            PropertyName = nameof(Department.Name),
                            DisplayName = Dn("Field.DepartmentName", "部門名稱"),
                            FilterPlaceholder = Fp("Field.DepartmentName", "輸入部門名稱搜尋"),
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Department.Name), d => d.Name)
                        }
                    },
                    {
                        "Manager",
                        new FieldDefinition<Department>
                        {
                            PropertyName = "Manager.Name",
                            DisplayName = Dn("Field.DepartmentManager", "部門主管"),
                            ShowInFilter = false,
                            TableOrder = 3,
                            NullDisplayText = Nd("Label.Unassigned", "未指派"),
                            CustomTemplate = item => builder =>
                            {
                                var department = (Department)item;
                                if (department.Manager != null)
                                {
                                    var managerName = department.Manager.Name?.Trim() ?? "";
                                    if (string.IsNullOrWhiteSpace(managerName))
                                        managerName = department.Manager.Code ?? "";
                                    builder.AddContent(0, managerName);
                                }
                                else
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", "text-muted");
                                    builder.AddContent(2, L?["Label.Unassigned"].ToString() ?? "未指派");
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        "ParentDepartment",
                        new FieldDefinition<Department>
                        {
                            PropertyName = "ParentDepartment.Name",
                            DisplayName = Dn("Field.ParentDepartment", "上級部門"),
                            ShowInFilter = false,
                            TableOrder = 4,
                            NullDisplayText = Nd("Label.NotSet", "未設定"),
                            CustomTemplate = item => builder =>
                            {
                                var department = (Department)item;
                                if (department.ParentDepartment != null)
                                {
                                    builder.AddContent(0, department.ParentDepartment.Name);
                                }
                                else
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", "text-muted");
                                    builder.AddContent(2, L?["Label.NotSet"].ToString() ?? "未設定");
                                    builder.CloseElement();
                                }
                            }
                        }
                    },
                    {
                        nameof(Department.Phone),
                        new FieldDefinition<Department>
                        {
                            PropertyName = nameof(Department.Phone),
                            DisplayName = Dn("Field.DepartmentPhone", "部門電話"),
                            ShowInFilter = false,
                            TableOrder = 5
                        }
                    },
                    {
                        nameof(Department.Location),
                        new FieldDefinition<Department>
                        {
                            PropertyName = nameof(Department.Location),
                            DisplayName = Dn("Field.DepartmentLocation", "辦公地點"),
                            ShowInFilter = false,
                            TableOrder = 6
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                // 記錄錯誤
                _ = Task.Run(async () =>
                {
                    await ErrorHandlingHelper.HandlePageErrorAsync(ex, nameof(GetFieldDefinitions), GetType(), additionalData: "初始化部門欄位配置失敗");
                });

                // 通知使用者
                if (_notificationService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _notificationService.ShowErrorAsync("初始化部門欄位配置時發生錯誤，已使用預設配置");
                    });
                }

                // 回傳空的配置，讓頁面使用預設行為
                return new Dictionary<string, FieldDefinition<Department>>();
            }
        }
    }
}


