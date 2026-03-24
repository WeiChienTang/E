using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// Permission 功能說明定義
/// </summary>
public static class PermissionGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-perm-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Permission.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-perm-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.Permission.Field.Code"),
                    new("Field.Name", "Guide.Permission.Field.Name"),
                    new("Field.Level", "Guide.Permission.Field.Level"),
                    new("Field.Remarks", "Guide.Permission.Field.Remarks"),
                }
            },
        }
    };
}
