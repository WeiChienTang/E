using System.Runtime.CompilerServices;

namespace ERPCore2.Models.FeatureGuides.GuideDefinitions;

public static class AccountItemGuide
{
    private static string GetSourcePath([CallerFilePath] string? path = null) => path!;

    public static FeatureGuideDefinition Create() => new()
    {
        SourceFile = GetSourcePath(),
        Sections = new()
        {
            new GuideSection
            {
                Id = "guide-acct-overview",
                TitleKey = "Guide.Overview",
                Icon = "bi-info-circle",
                BookmarkLabel = "概述",
                BookmarkColor = "#3B82F6",
                Type = GuideSectionType.Description,
                Items = { new("Guide.AccountItem.Description") }
            },

            new GuideSection
            {
                Id = "guide-acct-fields",
                TitleKey = "Guide.FieldDescriptions",
                Icon = "bi-input-cursor-text",
                BookmarkLabel = "欄位",
                BookmarkColor = "#F59E0B",
                Type = GuideSectionType.FieldList,
                Items =
                {
                    new("Field.Code", "Guide.AccountItem.Field.Code"),
                    new("Field.Name", "Guide.AccountItem.Field.Name"),
                    new("Field.Description", "Guide.AccountItem.Field.Type"),
                    new("Field.Remarks", "Guide.AccountItem.Field.Remarks")
                }
            },

            new GuideSection
            {
                Id = "guide-acct-tips",
                TitleKey = "Guide.Tips",
                Icon = "bi-lightbulb",
                BookmarkLabel = "提示",
                BookmarkColor = "#06B6D4",
                Type = GuideSectionType.Tips,
                Items =
                {
                    new("Guide.AccountItem.Tip1", GuideItemStyle.Tip)
                }
            },
        }
    };
}
