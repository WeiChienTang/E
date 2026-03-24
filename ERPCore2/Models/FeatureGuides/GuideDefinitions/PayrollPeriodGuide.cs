using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// PayrollPeriod 功能說明定義
/// </summary>
public static class PayrollPeriodGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-pp-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.PayrollPeriod.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-pp-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Name", "Guide.PayrollPeriod.Field.Name"),
                    new("Field.Year", "Guide.PayrollPeriod.Field.Year"),
                    new("Field.Month", "Guide.PayrollPeriod.Field.Month"),
                    new("Field.Remarks", "Guide.PayrollPeriod.Field.Remarks"),
                }
            },
        }
    };
}
