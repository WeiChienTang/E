using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// Unit 功能說明定義
/// </summary>
public static class UnitGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-unit-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.Unit.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-unit-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.Unit.Field.Code"),
                    new("Field.Name", "Guide.Unit.Field.Name"),
                    new("Field.EnglishName", "Guide.Unit.Field.EnglishName"),
                    new("Field.Remarks", "Guide.Unit.Field.Remarks"),
                }
            },
        }
    };
}
