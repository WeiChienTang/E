using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// Department 功能說明定義
/// </summary>
public static class DepartmentGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-dept-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Department.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-dept-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.Department.Field.Code"),
                    new("Field.Name", "Guide.Department.Field.Name"),
                    new("Field.ManagerId", "Guide.Department.Field.Manager"),
                    new("Field.DeputyManagerId", "Guide.Department.Field.Deputy"),
                    new("Field.ParentDepartment", "Guide.Department.Field.Parent"),
                    new("Field.Phone", "Guide.Department.Field.Phone"),
                    new("Field.Location", "Guide.Department.Field.Location"),
                    new("Field.Remarks", "Guide.Department.Field.Remarks"),
                }
            },
        }
    };
}
