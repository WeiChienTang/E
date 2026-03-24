using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

/// <summary>
/// PaperSetting 功能說明定義
/// </summary>
public static class PaperSettingGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-ps-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items =
                {
                    new("Guide.PaperSetting.Description"),
                }
            },

            new GuideSection
            {
                Id = "guide-ps-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.PaperSetting.Field.Code"),
                    new("Field.Name", "Guide.PaperSetting.Field.Name"),
                    new("Field.Width", "Guide.PaperSetting.Field.Width"),
                    new("Field.Height", "Guide.PaperSetting.Field.Height"),
                    new("Field.Orientation", "Guide.PaperSetting.Field.Orientation"),
                    new("Field.Remarks", "Guide.PaperSetting.Field.Remarks"),
                }
            },

            new GuideSection
            {
                Id = "guide-ps-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.PaperSetting.Tip1", GuideItemStyle.Tip),
                }
            },
        }
    };
}
