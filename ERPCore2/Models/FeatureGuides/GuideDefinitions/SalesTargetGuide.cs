using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// SalesTarget 功能說明定義
/// </summary>
public static class SalesTargetGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-st-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.SalesTarget.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-st-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Year", "Guide.SalesTarget.Field.Year"),
                    new("Field.Month", "Guide.SalesTarget.Field.Month"),
                    new("Field.SalesPerson", "Guide.SalesTarget.Field.Salesperson"),
                    new("Field.TargetAmount", "Guide.SalesTarget.Field.Amount"),
                    new("Field.Remarks", "Guide.SalesTarget.Field.Remarks"),
                }
            },
        }
    };
}
