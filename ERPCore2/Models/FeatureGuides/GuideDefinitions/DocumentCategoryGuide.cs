using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// DocumentCategory 功能說明定義
/// </summary>
public static class DocumentCategoryGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-dc-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.DocumentCategory.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-dc-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.DocumentCategory.Field.Code"),
                    new("Field.Name", "Guide.DocumentCategory.Field.Name"),
                    new("Field.DefaultAccessLevel", "Guide.DocumentCategory.Field.AccessLevel"),
                    new("Field.Remarks", "Guide.DocumentCategory.Field.Remarks"),
                }
            },
        }
    };
}
