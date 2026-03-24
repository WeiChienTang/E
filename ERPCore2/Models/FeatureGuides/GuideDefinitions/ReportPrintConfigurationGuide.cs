using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// ReportPrintConfiguration 功能說明定義
/// </summary>
public static class ReportPrintConfigurationGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-rpc-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.ReportPrintConfiguration.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-rpc-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.ReportPrintConfiguration.Field.Code"),
                    new("Field.Name", "Guide.ReportPrintConfiguration.Field.ReportName"),
                    new("Field.PaperSetting", "Guide.ReportPrintConfiguration.Field.PaperSetting"),
                    new("Field.Remarks", "Guide.ReportPrintConfiguration.Field.Remarks"),
                }
            },
        }
    };
}
