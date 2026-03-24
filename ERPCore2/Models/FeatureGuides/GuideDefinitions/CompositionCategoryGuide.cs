using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// CompositionCategory 功能說明定義
/// </summary>
public static class CompositionCategoryGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-cc-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.CompositionCategory.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-cc-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.CompositionCategory.Field.Code"),
                    new("Field.Name", "Guide.CompositionCategory.Field.Name"),
                    new("Field.Remarks", "Guide.CompositionCategory.Field.Remarks"),
                }
            },
        }
    };
}
