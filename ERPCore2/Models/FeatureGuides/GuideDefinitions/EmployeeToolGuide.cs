using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class EmployeeToolGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-etool-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.EmployeeTool.Description") }
            },

            new GuideSection
            {
                Id = "guide-etool-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.ToolName", "Guide.EmployeeTool.Field.Name"),
                    new("Field.SerialModel", "Guide.EmployeeTool.Field.Serial"),
                    new("Field.AssignDate", "Guide.EmployeeTool.Field.AssignDate"),
                    new("Field.ReturnDate", "Guide.EmployeeTool.Field.ReturnDate")
                }
            },
        }
    };
}
