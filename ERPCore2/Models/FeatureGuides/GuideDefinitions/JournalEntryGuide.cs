using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class JournalEntryGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-je-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.JournalEntry.Description") }
            },

            new GuideSection
            {
                Id = "guide-je-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.JournalEntry.Field.Code"),
                    new("Field.Description", "Guide.JournalEntry.Field.Description"),
                    new("Field.Remarks", "Guide.JournalEntry.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-je-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.JournalEntry.Tip1", GuideItemStyle.Tip),
                    new("Guide.JournalEntry.Warning1", GuideItemStyle.Warning)
                }
            },
        }
    };
}
