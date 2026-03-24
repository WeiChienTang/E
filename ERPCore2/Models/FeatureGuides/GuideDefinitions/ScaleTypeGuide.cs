using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// ScaleType 功能說明定義
/// </summary>
public static class ScaleTypeGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-sct-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.ScaleType.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-sct-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.ScaleType.Field.Code"),
                    new("Field.Name", "Guide.ScaleType.Field.Name"),
                    new("Field.ItemId", "Guide.ScaleType.Field.Item"),
                    new("Field.Description", "Guide.ScaleType.Field.Description"),
                    new("Field.Remarks", "Guide.ScaleType.Field.Remarks"),
                }
            },
        }
    };
}
