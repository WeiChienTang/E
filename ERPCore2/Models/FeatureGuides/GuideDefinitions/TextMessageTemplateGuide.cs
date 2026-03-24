using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// TextMessageTemplate 功能說明定義
/// </summary>
public static class TextMessageTemplateGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-tmt-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.TextMessageTemplate.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-tmt-steps",
                TitleKey = "Guide.Steps",
                Icon = "bi-list-ol",
                BookmarkLabel = "步驟",
                BookmarkColor = "#10B981",
                Type = GuideSectionType.Steps,
                Items =
                {
                    new("Guide.TextMessageTemplate.Step1"),
                    new("Guide.TextMessageTemplate.Step2"),
                    new("Guide.TextMessageTemplate.Step3"),
                }
            },
        }
    };
}
