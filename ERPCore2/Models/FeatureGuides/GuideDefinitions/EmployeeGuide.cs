using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// 員工管理功能說明定義
/// </summary>
public static class EmployeeGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            // ===== 功能概述 =====
            new GuideSection
            {
                Id = "guide-emp-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.Employee.Description") }
            },

            // ===== 分頁說明 =====
            new GuideSection
            {
                Id = "guide-emp-tabs",
                TitleKey = "Guide.TabDescriptions",
                Icon = "bi-folder2-open",
                BookmarkLabel = "分頁",
                BookmarkColor = "#8B5CF6",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Tab.EmployeeData", "Guide.Employee.Tab.EmployeeData"),
                    new("Tab.VehicleInfo", "Guide.Employee.Tab.VehicleInfo"),
                    new("Tab.PersonalTools", "Guide.Employee.Tab.PersonalTools"),
                    new("Tab.Training", "Guide.Employee.Tab.Training"),
                    new("Tab.Permissions", "Guide.Employee.Tab.Permissions"),
                    new("Tab.Payroll", "Guide.Employee.Tab.Payroll"),
                    new("Tab.BankAccounts", "Guide.Employee.Tab.BankAccounts"),
                    new("Tab.EmployeePhoto", "Guide.Employee.Tab.Photo"),
                    new("Tab.Documents", "Guide.Employee.Tab.Documents"),
                }
            },

            // ===== 基本資料欄位 =====
            new GuideSection
            {
                Id = "guide-emp-fields-basic",
                TitleKey = "Guide.Employee.BasicFieldsTitle",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.EmployeeCode", "Guide.Employee.Field.Code"),
                    new("Field.FullName", "Guide.Employee.Field.Name"),
                    new("Field.EnglishName", "Guide.Employee.Field.EnglishName"),
                    new("Field.Gender", "Guide.Employee.Field.Gender"),
                    new("Field.Birthday", "Guide.Employee.Field.Birthday"),
                    new("Field.IdNumber", "Guide.Employee.Field.IdNumber"),
                    new("Field.Nationality", "Guide.Employee.Field.Nationality"),
                    new("Field.MaritalStatus", "Guide.Employee.Field.MaritalStatus"),
                    new("Field.BloodType", "Guide.Employee.Field.BloodType"),
                }
            },

            // ===== 聯絡與組織欄位 =====
            new GuideSection
            {
                Id = "guide-emp-fields-contact",
                TitleKey = "Guide.Employee.ContactFieldsTitle",
                Icon = "bi-telephone",
                BookmarkLabel = "聯絡",
                BookmarkColor = "#059669",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.PhoneNumber", "Guide.Employee.Field.Mobile"),
                    new("Field.Phone", "Guide.Employee.Field.Phone"),
                    new("Field.Email", "Guide.Employee.Field.Email"),
                    new("Field.HomeAddress", "Guide.Employee.Field.HomeAddress"),
                    new("Field.MailingAddress", "Guide.Employee.Field.MailingAddress"),
                    new("Field.EmergencyContact", "Guide.Employee.Field.EmergencyContact"),
                    new("Field.EmergencyPhone", "Guide.Employee.Field.EmergencyPhone"),
                    new("Entity.Department", "Guide.Employee.Field.Department"),
                    new("Entity.EmployeePosition", "Guide.Employee.Field.Position"),
                    new("Field.HireDate", "Guide.Employee.Field.HireDate"),
                    new("Field.ResignDate", "Guide.Employee.Field.ResignDate"),
                    new("Field.EmploymentStatus", "Guide.Employee.Field.EmploymentStatus"),
                }
            },

            // ===== 提示與警告 =====
            new GuideSection
            {
                Id = "guide-emp-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.Employee.Tip1", GuideItemStyle.Tip),
                    new("Guide.Employee.Tip2", GuideItemStyle.Tip),
                    new("Guide.Employee.Warning1", GuideItemStyle.Warning),
                }
            },
        }
    };
}
