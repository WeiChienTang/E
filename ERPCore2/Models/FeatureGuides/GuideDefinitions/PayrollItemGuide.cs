using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// PayrollItem 功能說明定義
/// </summary>
public static class PayrollItemGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-pi-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.PayrollItem.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-pi-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.PayrollItem.Field.Code"),
                    new("Field.Name", "Guide.PayrollItem.Field.Name"),
                    new("Field.Description", "Guide.PayrollItem.Field.Description"),
                    new("Field.Remarks", "Guide.PayrollItem.Field.Remarks"),
                }
            },
        }
    };
}
