using ERPCore2.Components.Shared.Forms;
using ERPCore2.Components.Shared.Tables;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ERPCore2.Components.Shared.Configurations;

/// <summary>
/// 員工管理組件配置
/// </summary>
public static class EmployeeComponentConfiguration
{
    /// <summary>
    /// 取得員工表單配置
    /// </summary>
    public static (List<FormFieldDefinition> Fields, Dictionary<string, string> Sections) GetFormConfiguration()
    {
        var formBuilder = FormConfigurationBuilder<Employee>.Create()
            .AddText("EmployeeCode", "員工代碼", "請輸入員工代碼", true)
            .AddText("Username", "帳號", "請輸入登入帳號", true)
            .AddText("Name", "姓名", "請輸入姓名", true)
            .AddEmail("Email", "電子郵件", "請輸入電子郵件", false)
            .AddText("Department", "部門", "請輸入部門", false)
            .AddText("Position", "職位", "請輸入職位", false)
            .AddPassword("PasswordHash", "密碼", "請輸入密碼", true);

        var sections = new Dictionary<string, string>
        {
            { "EmployeeCode", "基本資料" },
            { "Username", "基本資料" },
            { "Name", "基本資料" },
            { "Email", "聯絡資訊" },
            { "Department", "職務資訊" },
            { "Position", "職務資訊" },
            { "PasswordHash", "安全設定" }
        };

        return (formBuilder.Build(), sections);
    }

    /// <summary>
    /// 取得員工搜尋篩選配置
    /// </summary>
    public static List<SearchFilterDefinition> GetSearchFilterConfiguration(List<Role> roles)
    {
        var roleOptions = roles.Select(r => new SelectOption 
        { 
            Value = r.Id.ToString(), 
            Text = r.RoleName 
        }).ToList();

        var statusOptions = Enum.GetValues<EntityStatus>()
            .Select(s => new SelectOption 
            { 
                Value = s.ToString(), 
                Text = GetStatusText(s) 
            }).ToList();

        return SearchFilterConfigurationBuilder<Employee>.Create()
            .AddText("EmployeeCode", "員工代碼", "搜尋員工代碼")
            .AddText("Name", "姓名", "搜尋姓名")
            .AddText("Email", "電子郵件", "搜尋電子郵件")
            .AddText("Department", "部門", "搜尋部門")
            .AddSelect("RoleId", "角色", roleOptions)
            .AddSelect("Status", "狀態", statusOptions)
            .Build();
    }

    /// <summary>
    /// 取得員工表格配置
    /// </summary>
    public static List<TableColumnDefinition> GetTableConfiguration()
    {
        var statusBadgeMap = new Dictionary<object, string>
        {
            { EntityStatus.Active, "bg-success" },
            { EntityStatus.Inactive, "bg-secondary" },
            { EntityStatus.Deleted, "bg-danger" }
        };

        return TableConfigurationBuilder<Employee>.Create()
            .AddText("EmployeeCode", "員工代碼")
            .AddText("Name", "姓名")
            .AddText("Department", "部門")
            .AddText("Position", "職位")
            .AddStatus("Status", "狀態", statusBadgeMap)
            .AddCustomColumn("Actions", "操作", (RenderTreeBuilder builder) =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "btn-group");
                
                builder.OpenElement(2, "button");
                builder.AddAttribute(3, "class", "btn btn-sm btn-outline-primary");
                builder.AddAttribute(4, "title", "檢視");
                builder.OpenElement(5, "i");
                builder.AddAttribute(6, "class", "fas fa-eye");
                builder.CloseElement();
                builder.CloseElement();
                
                builder.OpenElement(7, "button");
                builder.AddAttribute(8, "class", "btn btn-sm btn-outline-secondary");
                builder.AddAttribute(9, "title", "編輯");
                builder.OpenElement(10, "i");
                builder.AddAttribute(11, "class", "fas fa-edit");
                builder.CloseElement();
                builder.CloseElement();
                
                builder.CloseElement();
            })
            .Build();
    }

    private static string GetStatusText(EntityStatus status)
    {
        return status switch
        {
            EntityStatus.Active => "啟用",
            EntityStatus.Inactive => "停用",
            EntityStatus.Deleted => "已刪除",
            _ => status.ToString()
        };
    }
}

/// <summary>
/// 角色管理組件配置
/// </summary>
public static class RoleComponentConfiguration
{
    /// <summary>
    /// 取得角色表單配置
    /// </summary>
    public static (List<FormFieldDefinition> Fields, Dictionary<string, string> Sections) GetFormConfiguration()
    {
        var formBuilder = FormConfigurationBuilder<Role>.Create()
            .AddText("RoleName", "角色名稱", "請輸入角色名稱", true)
            .AddTextArea("Description", "角色描述", 3, "請輸入角色描述", false)
            .AddCheckbox("IsSystemRole", "系統角色");

        var sections = new Dictionary<string, string>
        {
            { "RoleName", "基本資料" },
            { "Description", "基本資料" },
            { "IsSystemRole", "設定" }
        };

        return (formBuilder.Build(), sections);
    }

    /// <summary>
    /// 取得角色搜尋篩選配置
    /// </summary>
    public static List<SearchFilterDefinition> GetSearchFilterConfiguration()
    {
        var statusOptions = Enum.GetValues<EntityStatus>()
            .Select(s => new SelectOption 
            { 
                Value = s.ToString(), 
                Text = GetStatusText(s) 
            }).ToList();

        var typeOptions = new List<SelectOption>
        {
            new SelectOption { Value = "true", Text = "系統角色" },
            new SelectOption { Value = "false", Text = "自定義角色" }
        };

        return SearchFilterConfigurationBuilder<Role>.Create()
            .AddText("RoleName", "角色名稱", "搜尋角色名稱")
            .AddSelect("IsSystemRole", "角色類型", typeOptions)
            .AddSelect("Status", "狀態", statusOptions)
            .Build();
    }

    /// <summary>
    /// 取得角色表格配置
    /// </summary>
    public static List<TableColumnDefinition> GetTableConfiguration()
    {
        var statusBadgeMap = new Dictionary<object, string>
        {
            { EntityStatus.Active, "bg-success" },
            { EntityStatus.Inactive, "bg-secondary" },
            { EntityStatus.Deleted, "bg-danger" }
        };

        return TableConfigurationBuilder<Role>.Create()
            .AddText("RoleName", "角色名稱")
            .AddText("Description", "描述")
            .AddCustomColumn("IsSystemRole", "類型", (RenderTreeBuilder builder) =>
            {
                // 這裡會渲染角色類型的徽章
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "class", "badge bg-info");
                builder.AddContent(2, "系統角色"); // 這裡應該根據實際值動態設定
                builder.CloseElement();
            })
            .AddStatus("Status", "狀態", statusBadgeMap)
            .AddCustomColumn("Actions", "操作", (RenderTreeBuilder builder) =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "btn-group");
                
                builder.OpenElement(2, "button");
                builder.AddAttribute(3, "class", "btn btn-sm btn-outline-primary");
                builder.AddAttribute(4, "title", "檢視");
                builder.OpenElement(5, "i");
                builder.AddAttribute(6, "class", "fas fa-eye");
                builder.CloseElement();
                builder.CloseElement();
                
                builder.OpenElement(7, "button");
                builder.AddAttribute(8, "class", "btn btn-sm btn-outline-secondary");
                builder.AddAttribute(9, "title", "編輯");
                builder.OpenElement(10, "i");
                builder.AddAttribute(11, "class", "fas fa-edit");
                builder.CloseElement();
                builder.CloseElement();
                
                builder.OpenElement(12, "button");
                builder.AddAttribute(13, "class", "btn btn-sm btn-outline-warning");
                builder.AddAttribute(14, "title", "權限管理");
                builder.OpenElement(15, "i");
                builder.AddAttribute(16, "class", "fas fa-key");
                builder.CloseElement();
                builder.CloseElement();
                
                builder.CloseElement();
            })
            .Build();
    }

    private static string GetStatusText(EntityStatus status)
    {
        return status switch
        {
            EntityStatus.Active => "啟用",
            EntityStatus.Inactive => "停用",
            EntityStatus.Deleted => "已刪除",
            _ => status.ToString()
        };
    }
}
