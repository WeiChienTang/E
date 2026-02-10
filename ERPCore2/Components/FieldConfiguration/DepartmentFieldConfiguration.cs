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
                            DisplayName = "部門編號",
                            FilterPlaceholder = "輸入部門編號搜尋",
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
                            DisplayName = "部門名稱",
                            FilterPlaceholder = "輸入部門名稱搜尋",
                            TableOrder = 2,
                            FilterFunction = (model, query) => FilterHelper.ApplyTextContainsFilter(
                                model, query, nameof(Department.Name), d => d.Name)
                        }
                    },
                    {
                        "Manager",
                        new FieldDefinition<Department>
                        {
                            PropertyName = "Manager.FirstName",
                            DisplayName = "部門主管",
                            ShowInFilter = false, // 不顯示在篩選器中
                            TableOrder = 3,
                            NullDisplayText = "未指派",
                            CustomTemplate = item => builder =>
                            {
                                var department = (Department)item;
                                if (department.Manager != null)
                                {
                                    var managerName = department.Manager.Name?.Trim() ?? "";
                                    if (string.IsNullOrWhiteSpace(managerName))
                                    {
                                        managerName = department.Manager.Code;
                                    }
                                    builder.AddContent(0, managerName);
                                }
                                else
                                {
                                    builder.OpenElement(0, "span");
                                    builder.AddAttribute(1, "class", "text-muted");
                                    builder.AddContent(2, "未指派");
                                    builder.CloseElement();
                                }
                            }
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


